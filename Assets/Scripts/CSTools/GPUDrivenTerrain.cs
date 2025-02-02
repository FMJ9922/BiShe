using Manager;
using NUnit.Framework.Constraints;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class GPUDrivenTerrain : MonoBehaviour
{
    [Header("Settings")]
    public int width = 300;
    public int height = 300;
    public Texture2D heightMap;
    public float heightIntensity = 5f;

    [Header("Wave Settings")] 
    public float waveSpeed = 1f;

    // Compute资源
    public ComputeShader computeShader;
    private ComputeBuffer _vertexBuffer;
    
    // 网格数据
    private Mesh _mesh;
    private Vector3[] _initialVertices;
    private int _kernel;
    private Material _material;

    [Header("Texture Settings")]
    public Texture2D mainTexture;  // 大贴图
    public Texture2D indexTexture; // 存储每个格子的索引
    private int[,] textureIndices; // 二维数组存储索引
    public Terrain terrain;

    void Start()
    {
        InititalizeHeightMap();
        InitializeMesh();
        SetupComputeShader();
        InitializeTextureIndices(); // 初始化索引
        SetTileIndex(0,0,10);
    }

    void InitializeTextureIndices()
    {
        InitIndexTexture();
        // 默认全为0，可根据需要初始化
        indexTexture = new Texture2D(width, height, TextureFormat.R8, false);
        indexTexture.filterMode = FilterMode.Point;
        indexTexture.wrapMode = TextureWrapMode.Clamp;
        UpdateIndexTexture();

        _material.SetTexture("_MainTex", mainTexture);
        _material.SetTexture("_IndexMap", indexTexture);
        _material.SetFloat("_TerrainWidth", width);
        _material.SetFloat("_TerrainHeight", height);
    }

    void InititalizeHeightMap(){
        TerrainData terrainData = terrain.terrainData;
        heightMap = new Texture2D(width, height, TextureFormat.RGBA32, false); // 使用RGBA32确保兼容性
        heightMap.filterMode = FilterMode.Point;
        heightMap.wrapMode = TextureWrapMode.Clamp;
        Color[] colors = new Color[width * height];
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                // 计算正确的UV坐标，覆盖地形边缘
                float u = (float)x / (width - 1);
                float v = (float)z / (height - 1);

                //float h = terrainData.GetInterpolatedHeight(u, v);
                float h = MapManager.GetTerrainPosition(new Vector2Int(x,z)).y;
                float normalizedH = h / 256; // 归一化到[0,1]

                // 存入颜色数组，RGBA32格式下R通道存储高度
                colors[z * width + x] = new Color(normalizedH, normalizedH, normalizedH, 1);
            }
        }
        heightMap.SetPixels(colors);
        heightMap.Apply();
        // 可选：保存为PNG文件
        byte[] pngData = heightMap.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/Heightmap.png", pngData);
    }

    public void InitIndexTexture(){
        textureIndices = new int[width, height];
        Mesh mesh = TerrainGenerator.Instance.GetComponent<MeshFilter>().mesh;
        int[] meshTex = new int[width * height];
        int[] meshDir = new int[width * height];
        int sizeX = width;
        int sizeY = height;
        int texLength = MapManager.TexLength;
        Vector2[] uv = mesh.uv;
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                int index = (i + j * sizeY) * 4;
                int x = Mathf.FloorToInt(uv[index].x * texLength);
                int y = Mathf.FloorToInt(uv[index].y * texLength);
                int tex = 8 * (7 - y) + x;
                float deltaX = x * 0.125f + 0.125f - uv[index].x;
                float deltaY = (7 - y) * 0.125f + 0.125f - uv[index].y;
                int dir = GetDir(deltaX, deltaY);
                meshDir[i + j * sizeY] = dir;
                meshTex[i + j * sizeY] = tex;
                textureIndices[i, j]  = tex;
            }
        }
    }

    // 更新索引到GPU
    public void UpdateIndexTexture()
    {
        Color[] colors = new Color[width * height];
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float normalizedIndex = textureIndices[x, z] / 63.0f;
                colors[z * width + x] = new Color(normalizedIndex, 0, 0, 1);
            }
        }
        indexTexture.SetPixels(colors);
        indexTexture.Apply();
    }

    // 动态修改某个格子的索引
    public void SetTileIndex(int x, int z, int index)
    {
        if (x < 0 || x >= width || z < 0 || z >= height) return;
        textureIndices[x, z] = index;
        UpdateIndexTexture();
    }

    void InitializeMesh()
    {
        _mesh = new Mesh {name = "GPU Terrain"};
        _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = _mesh;

        // 生成初始网格
        Vector3[] vertices = new Vector3[(width+1)*(height+1)];
        Vector2[] uv = new Vector2[vertices.Length];
        
        for (int z = 0, i = 0; z <= height; z++) {
            for (int x = 0; x <= width; x++) {
                vertices[i] = new Vector3(x * 2, 0, z * 2);
                uv[i] = new Vector2(x/(float)width, z/(float)height);
                i++;
            }
        }

        int[] triangles = new int[width*height*6];
        for (int ti = 0, vi = 0, y = 0; y < height; y++, vi++) {
            for (int x = 0; x < width; x++, ti += 6, vi++) {
                triangles[ti] = vi;
                triangles[ti+3] = triangles[ti+2] = vi + 1;
                triangles[ti+4] = triangles[ti+1] = vi + width + 1;
                triangles[ti+5] = vi + width + 2;
            }
        }

        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.uv = uv;
        _initialVertices = vertices;
    }

    void SetupComputeShader()
    {
        // 创建Compute Buffer
        _vertexBuffer = new ComputeBuffer(_initialVertices.Length, sizeof(float)*3);
        _vertexBuffer.SetData(_initialVertices);

        // 初始化Shader
        _kernel = computeShader.FindKernel("TerrainUpdate");
        computeShader.SetBuffer(_kernel, "_Vertices", _vertexBuffer);
        
        _material = GetComponent<MeshRenderer>().material;
        _material.SetBuffer("_VertexBuffer", _vertexBuffer);
    }

    void Update()
    {
        if (_initialVertices != null)
        {
            // 每帧更新参数
            computeShader.SetTexture(_kernel, "_HeightMap", heightMap);
            computeShader.SetFloat("_Intensity", heightIntensity);
            computeShader.SetFloat("_TimeParam", Time.time * waveSpeed);
            computeShader.SetVector("_HeightMapScale",
                new Vector2(0.5f / width, 0.5f / height));

            // 调度Compute Shader
            int threadGroups = Mathf.CeilToInt(_initialVertices.Length / 64f);
            computeShader.Dispatch(_kernel, threadGroups, 1, 1);
        }
    }

    void OnDestroy()
    {
        _vertexBuffer?.Release();
    }

    public int GetDir(float deltaX, float deltaY)
    {
        if (deltaX > 0)
        {
            if (deltaY > 0)
            {
                return 0;
            }
            else
            {
                return 3;
            }
        }
        else
        {
            if (deltaY > 0)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
    }
}