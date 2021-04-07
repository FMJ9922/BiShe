using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class TerrainGenerator : Singleton<TerrainGenerator>
{
    public Terrain terrain;
    public static float[][] height;
    private Vector3[] verticles;
    private Mesh mesh;
    public Texture2D tx;
    long width, lenth;
    private string pathName = "/MapData";//路径名称
    public Vector3 start, end;
    #region 顶点处理
    [ContextMenu("CalculateHeights")]
    public void CalculateHeights()
    {
        width = (int)(terrain.terrainData.size.x / 2) + 1;
        lenth = (int)(terrain.terrainData.size.z / 2) + 1;
        height = new float[width][];
        for (int i = 0; i < height.Length; i++)
        {
            height[i] = new float[lenth];
            for (int j = 0; j < height.Length; j++)
            {
                height[i][j] = terrain.terrainData.GetInterpolatedHeight((float)i / width, (float)j / lenth);
            }
        }
        //Debug.Log("处理完毕");
    }

    //[ContextMenu("ReplaceTerrain")]
    /*public void ReplaceTerrain()
    {
        for (int j = 0; j < height.Length - 1; j++)
        {
            for (int i = 0; i < height.Length - 1; i++)
            {
                GameObject new1;
                GameObject new2;
                if (j == 25)
                {
                    new1 = GameObject.Instantiate(tri3, transform);
                    new2 = GameObject.Instantiate(tri4, transform);
                }
                else
                {
                    new1 = GameObject.Instantiate(tri1, transform);
                    new2 = GameObject.Instantiate(tri2, transform);
                }
                Vector3 pos = terrain.transform.position;
                float h = pos.y;
                h = (((int)(h / 3)) * 3);
                pos = new Vector3(pos.x, h, pos.z);
                new1.transform.position = pos + new Vector3(i * 2, 0, (j + 1) * 2);
                new2.transform.position = pos + new Vector3(i * 2, 0, (j + 1) * 2);
                Mesh mesh1 = CopyMesh(new1.GetComponent<MeshFilter>().sharedMesh);
                Mesh mesh2 = CopyMesh(new2.GetComponent<MeshFilter>().sharedMesh);
                Vector3[] positionList1 = mesh1.vertices;
                Vector3[] positionList2 = mesh2.vertices;
                positionList1[2] += new Vector3(0, height[i][j + 1], 0);
                positionList1[1] += new Vector3(0, height[i][j], 0);
                positionList1[0] += new Vector3(0, height[i + 1][j], 0);
                positionList2[2] += new Vector3(0, height[i][j + 1], 0);
                positionList2[0] += new Vector3(0, height[i + 1][j + 1], 0);
                positionList2[1] += new Vector3(0, height[i + 1][j], 0);
                mesh1.vertices = positionList1;
                mesh2.vertices = positionList2;
                mesh1.RecalculateNormals();
                mesh2.RecalculateNormals();
                mesh1.name = string.Format("{0}{1}:{2}", i, j, 0);
                mesh2.name = string.Format("{0}{1}:{2}", i, j, 1);
                new1.GetComponent<MeshFilter>().mesh = mesh1;
                new2.GetComponent<MeshFilter>().mesh = mesh2;
            }
        }
        Debug.Log("生成完毕");
    }*/

    public Mesh CopyMesh(Mesh originMesh)
    {
        Mesh m = new Mesh();
        m.vertices = originMesh.vertices;
        m.normals = originMesh.normals;
        m.triangles = originMesh.triangles;
        m.uv = originMesh.uv;
        return m;
    }

    [ContextMenu("RecalculateUV")]
    /// <summary>
    /// 4->8
    /// </summary>
    public void RecalculateUV()
    {
        CalculateHeights();
        Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;
        long xSize = width - 1;
        long ySize = width - 1;
        long size = xSize * ySize * 4;
        Vector2[] uv = mesh.uv;
        for (int i = 0, y = 0; y < height.Length - 1; y++)
        {
            for (int x = 0; x < height.Length - 1; x++, i++)
            {
                int dir = Random.Range((int)0, (int)4);
                uv[4 * i + dir % 4] = new Vector2(0.001F, 1 - 0.001F);
                uv[4 * i + (1 + dir) % 4] = new Vector2(0.125F-0.001F, 1 - 0.001F);
                uv[4 * i + (2 + dir) % 4] = new Vector2(0.125F - 0.001F, 1 - 0.125f+0.001F);
                uv[4 * i + (3 + dir) % 4] = new Vector2(0.001F, 1 - 0.125f + 0.001F);
            }
        }
        mesh.uv = uv;
    }
    public void OnPaint(int tex, Vector3 pos, int dir,int size)
    {
        CalculateHeights();
        Vector3 delta = pos - terrain.transform.position;
        int x = Mathf.FloorToInt(delta.x / 2);
        int z = Mathf.FloorToInt(delta.z / 2);
        int[] indexes = new int[(2*size-1)*(2*size -1)];
        MapData mapData = GetMapData();
        for (int j = -size+1; j < size; j++)
        {
            for (int i = -size+1; i < size; i++)
            {
                indexes[(j+size -1) * (2 * size - 1) + i + size - 1] = (z + j) * (height.Length - 1) + x + i;

                RefreshUV(tex, 8, indexes, dir);
                if (tex == 4 || tex == 5 || tex == 6 || tex == 8 || tex == 9 || tex == 10 || tex == 12 || tex == 13 || tex == 14)
                {
                    mapData.roadGrids.Add(new Vector2IntSerializer(x+i, z+j));
                }
                else
                {
                    for (int k = 0; k < mapData.roadGrids.Count; k++)
                    {
                        if (mapData.roadGrids[k].Vector2Int == new Vector2Int(x+i, z+j))
                        {
                            mapData.roadGrids.RemoveAt(k);
                        }
                    }
                }
            }
        }
        //SaveMapData(mapData);

    }

    public int GetBeforeTex(Vector2 uv,float adjust = 0.01f,int length = 4)
    {
        int x = Mathf.RoundToInt(uv.x - adjust) * length;
        int h = -(Mathf.RoundToInt(uv.y - adjust) * length + 1 - length);
        int tex = x + h * length;
        return tex;
    }
    public void RefreshUV(int tex, int length, int index, int dir = 0, float adjust = 0.01f)
    {
        mesh = transform.GetComponent<MeshFilter>().sharedMesh;
        Vector2[] uv = mesh.uv;
        float h = Mathf.FloorToInt(tex / length);
        float x = tex - length * h;
        //Debug.Log(h + " " + x);
        uv[4 * index + dir % 4] = new Vector2((x / length) + adjust, ((length - h - 1) / length) + adjust);
        uv[4 * index + (1 + dir) % 4] = new Vector2(((x + 1) / length) - adjust, ((length - h - 1) / length) + adjust);
        uv[4 * index + (2 + dir) % 4] = new Vector2(((x + 1) / length) - adjust, ((length - h) / length) - adjust);
        uv[4 * index + (3 + dir) % 4] = new Vector2((x / length) + adjust, ((length - h) / length) - adjust);
        mesh.uv = uv;
        mesh.RecalculateNormals();
    }
    /// <summary>
    /// 刷UV
    /// </summary>
    /// <param name="tex">贴图</param>
    /// <param name="length">贴图大小</param>
    /// <param name="indexs">要修改的顶点索引</param>
    /// <param name="dir">方向</param>
    /// <param name="adjust">调整边缘</param>
    public void RefreshUV(int tex,int length,int[] indexs ,int dir =0,float adjust = 0.01f) 
    {
        mesh = transform.GetComponent<MeshFilter>().sharedMesh;
        Vector2[] uv = mesh.uv;
        float h = Mathf.FloorToInt(tex / length);
        float x = tex - length * h;
        //Debug.Log(h + " " + x);
        Vector2 one = new Vector2((x / length) + adjust, ((length - h - 1) / length) + adjust);
        Vector2 two = new Vector2(((x + 1) / length) - adjust, ((length - h - 1) / length) + adjust);
        Vector2 three = new Vector2(((x + 1) / length) - adjust, ((length - h) / length) - adjust);
        Vector2 four = new Vector2((x / length) + adjust, ((length - h) / length) - adjust);
        for (int i = 0; i < indexs.Length; i++)
        {
            uv[4 * indexs[i] + dir % 4] = one;
            uv[4 * indexs[i] + (1 + dir) % 4] = two;
            uv[4 * indexs[i] + (2 + dir) % 4] = three;
            uv[4 * indexs[i] + (3 + dir) % 4] = four;
        }
        mesh.uv = uv;
        mesh.RecalculateNormals();
        transform.GetComponent<MeshFilter>().mesh = mesh;
    }
    
    
    public void OnReTriangle(Vector3 pos, int dir)
    {
        CalculateHeights();
        Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;
        Vector3 delta = pos - terrain.transform.position;
        int x = Mathf.FloorToInt(delta.x / 2);
        int z = Mathf.FloorToInt(delta.z / 2);
        int index = z * (height.Length - 1) + x;
        int[] triangles = mesh.triangles;
        int vi = index;
        int ti = index * 6;
        if (dir == 0)
        {
            triangles[ti] = vi * 4;
            triangles[ti + 1] = vi * 4 + 3;
            triangles[ti + 2] = vi * 4 + 1;
            triangles[ti + 3] = vi * 4 + 1;
            triangles[ti + 4] = vi * 4 + 3;
            triangles[ti + 5] = vi * 4 + 2;
        }
        else
        {
            triangles[ti] = vi * 4;
            triangles[ti + 1] = vi * 4 + 2;
            triangles[ti + 2] = vi * 4 + 1;
            triangles[ti + 3] = vi * 4;
            triangles[ti + 4] = vi * 4 + 3;
            triangles[ti + 5] = vi * 4 + 2;
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        transform.GetComponent<MeshFilter>().mesh = mesh;
    }

    public void OnFlatGround(Vector3 pos, int range, int targetHeight)
    {
        CalculateHeights();
        Vector3 delta = pos - terrain.transform.position;
        int x = Mathf.FloorToInt(delta.x / 2);
        int z = Mathf.FloorToInt(delta.z / 2);
        int index = z * (height.Length - 1) + x;
        FlatGround(index, range, targetHeight/1.5f);
    }
    public void OnFlatGround(Vector3 pos, int range, float targetHeight)
    {
        CalculateHeights();
        Vector3 delta = pos - terrain.transform.position;
        int x = Mathf.FloorToInt(delta.x / 2);
        int z = Mathf.FloorToInt(delta.z / 2);
        int index = z * (height.Length - 1) + x;
        FlatGround(index, range, targetHeight);
    }

    void FlatGround(int index, int range, float targetHeight)
    {
        Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;
        verticles = mesh.vertices;
        int length = height.Length - 1;
        int z = index / (height.Length - 1);
        int x = index % (height.Length - 1);
        //Debug.Log(x + " " + z);
        for (int i = -range + 1; i < range; i++)
        {
            for (int j = -range + 1; j < range; j++)
            {
                if ((i + x) < 0 || (i + x) > length - 1 || (j + z) < 0 || (j + z) > length - 1)
                {
                    continue;
                }
                int p = (z + j) * length + (i + x);
                verticles[4 * p] = ChangeHeight(verticles[4 * p], targetHeight);
                verticles[4 * p + 1] = ChangeHeight(verticles[4 * p + 1], targetHeight);
                verticles[4 * p + 2] = ChangeHeight(verticles[4 * p + 2], targetHeight);
                verticles[4 * p + 3] = ChangeHeight(verticles[4 * p + 3], targetHeight);
            }
        }
        for (int i = -range + 1; i < range; i++)
        {
            int p;
            if (z - range >= 0)
            {
                p = (z - range) * length + (i + x);
                verticles[4 * p + 2] = ChangeHeight(verticles[4 * p + 2], targetHeight);
                verticles[4 * p + 3] = ChangeHeight(verticles[4 * p + 3], targetHeight);
            }
            if (z + range < length)
            {
                p = (z + range) * length + (i + x);
                verticles[4 * p + 0] = ChangeHeight(verticles[4 * p + 0], targetHeight);
                verticles[4 * p + 1] = ChangeHeight(verticles[4 * p + 1], targetHeight);
            }
            if (x - range >= 0)
            {
                p = (z + i) * length + (-range + x);
                verticles[4 * p + 1] = ChangeHeight(verticles[4 * p + 1], targetHeight);
                verticles[4 * p + 2] = ChangeHeight(verticles[4 * p + 2], targetHeight);
            }
            if (x + range < length)
            {
                p = (z + i) * length + (range + x);
                verticles[4 * p + 0] = ChangeHeight(verticles[4 * p + 0], targetHeight);
                verticles[4 * p + 3] = ChangeHeight(verticles[4 * p + 3], targetHeight);
            }
        }
        int pl;
        if (z - range >= 0 && -range + x >= 0)
        {
            pl = (z - range) * length + (-range + x);
            verticles[4 * pl + 2] = ChangeHeight(verticles[4 * pl + 2], targetHeight);
        }
        if (z - range >= 0 && range + x < length)
        {
            pl = (z - range) * length + (range + x);
            verticles[4 * pl + 3] = ChangeHeight(verticles[4 * pl + 3], targetHeight);
        }
        if (z + range < length && range + x < length)
        {
            pl = (z + range) * length + (range + x);
            verticles[4 * pl + 0] = ChangeHeight(verticles[4 * pl + 0], targetHeight);
        }
        if (z + range < length && -range + x >= 0)
        {
            pl = (z + range) * length + (-range + x);
            verticles[4 * pl + 1] = ChangeHeight(verticles[4 * pl + 1], targetHeight);
        }


        mesh.vertices = verticles;
        mesh.RecalculateNormals();
        transform.GetComponent<MeshFilter>().mesh = mesh;
    }
    void FlatGround(int index, int range, int targetHeight)
    {
        Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;
        verticles = mesh.vertices;
        int length = height.Length - 1;
        int z = index / (height.Length - 1);
        int x = index % (height.Length - 1);
        Debug.Log(x + " " + z);
        for (int i = -range + 1; i < range; i++)
        {
            for (int j = -range + 1; j < range; j++)
            {
                if ((i + x) < 0 || (i + x) > length - 1 || (j + z) < 0 || (j + z) > length - 1)
                {
                    continue;
                }
                int p = (z + j) * length + (i + x);
                verticles[4 * p] = ChangeHeight(verticles[4 * p], targetHeight);
                verticles[4 * p + 1] = ChangeHeight(verticles[4 * p + 1], targetHeight);
                verticles[4 * p + 2] = ChangeHeight(verticles[4 * p + 2], targetHeight);
                verticles[4 * p + 3] = ChangeHeight(verticles[4 * p + 3], targetHeight);
            }
        }
        for (int i = -range + 1; i < range; i++)
        {
            int p;
            if (z - range >= 0)
            {
                p = (z - range) * length + (i + x);
                verticles[4 * p + 2] = ChangeHeight(verticles[4 * p + 2], targetHeight);
                verticles[4 * p + 3] = ChangeHeight(verticles[4 * p + 3], targetHeight);
            }
            if (z + range < length)
            {
                p = (z + range) * length + (i + x);
                verticles[4 * p + 0] = ChangeHeight(verticles[4 * p + 0], targetHeight);
                verticles[4 * p + 1] = ChangeHeight(verticles[4 * p + 1], targetHeight);
            }
            if (x - range >= 0)
            {
                p = (z + i) * length + (-range + x);
                verticles[4 * p + 1] = ChangeHeight(verticles[4 * p + 1], targetHeight);
                verticles[4 * p + 2] = ChangeHeight(verticles[4 * p + 2], targetHeight);
            }
            if (x + range < length)
            {
                p = (z + i) * length + (range + x);
                verticles[4 * p + 0] = ChangeHeight(verticles[4 * p + 0], targetHeight);
                verticles[4 * p + 3] = ChangeHeight(verticles[4 * p + 3], targetHeight);
            }
        }
        int pl;
        if (z - range >= 0 && -range + x >= 0)
        {
            pl = (z - range) * length + (-range + x);
            verticles[4 * pl + 2] = ChangeHeight(verticles[4 * pl + 2], targetHeight);
        }
        if (z - range >= 0 && range + x < length)
        {
            pl = (z - range) * length + (range + x);
            verticles[4 * pl + 3] = ChangeHeight(verticles[4 * pl + 3], targetHeight);
        }
        if (z + range < length && range + x < length)
        {
            pl = (z + range) * length + (range + x);
            verticles[4 * pl + 0] = ChangeHeight(verticles[4 * pl + 0], targetHeight);
        }
        if (z + range < length && -range + x >= 0)
        {
            pl = (z + range) * length + (-range + x);
            verticles[4 * pl + 1] = ChangeHeight(verticles[4 * pl + 1], targetHeight);
        }


        mesh.vertices = verticles;
        mesh.RecalculateNormals();
        transform.GetComponent<MeshFilter>().mesh = mesh;
    }

    Vector3 ChangeHeight(Vector3 vector3, float height)
    {
        return new Vector3(vector3.x, height, vector3.z);
    }
    
    [ContextMenu("存储地形")]
    void SaveAsset()
    {
        Mesh mesh = transform.GetComponent<MeshFilter>().mesh;
        AssetDatabase.CreateAsset(mesh, GetPath("meshData.asset"));
    }
    
    [ContextMenu("加载地形")]
    void LoadAsset()
    {
        transform.GetComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(GetPath("meshData.asset"));
    }
    /// <summary>
    /// 获得地形的顶点数据
    /// </summary>
    /// <returns></returns>
    public Vector3[] GetTerrainMeshVertices()
    {
        Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;
        verticles = mesh.vertices;
        return verticles;
    }
    [ContextMenu("GenerateMeshes")]
    void GenerateMeshes()
    {
        long xSize = width - 1;
        long ySize = width - 1;
        Vector3 origin = terrain.transform.position;
        long size = xSize * ySize * 4;
        Debug.Log(xSize+" "+ySize+" "+size);
        verticles = new Vector3[size];
        Vector2[] uv = new Vector2[verticles.Length];
        for (int i = 0, y = 0; y < height.Length - 1; y++)
        {
            for (int x = 0; x < height.Length - 1; x++, i++)
            {
                verticles[4 * i] = origin + new Vector3(x * 2, height[x][y], y * 2);
                verticles[4 * i + 1] = origin + new Vector3((x + 1) * 2, height[x + 1][y], y * 2);
                verticles[4 * i + 2] = origin + new Vector3((x + 1) * 2, height[x + 1][y + 1], (y + 1) * 2);
                verticles[4 * i + 3] = origin + new Vector3(x * 2, height[x][y + 1], (y + 1) * 2);
                uv[4 * i] = new Vector2(0.01F, 1-0.01F);
                uv[4 * i + 1] = new Vector2(0.24f, 1-0.01F);
                uv[4 * i + 2] = new Vector2(0.24f, 1-0.24f);
                uv[4 * i + 3] = new Vector2(0.01F, 1-0.24f);
                //uv[4 * i] = new Vector2((float)x/ height.Length, (float)y / height.Length);
                //uv[4 * i + 1] = new Vector2((float)x / height.Length, (float)y / height.Length);
                //uv[4 * i + 2] = new Vector2((float)x / height.Length, (float)y / height.Length);
                //uv[4 * i + 3] = new Vector2((float)x / height.Length, (float)y / height.Length);
            }
        }
        transform.GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "TerrainGrid";
        mesh.vertices = verticles;
        mesh.uv = uv;
        //mesh.tangents = tangents;
        int[] triangles = new int[xSize * ySize * 6];
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi * 4;
                triangles[ti + 1] = vi * 4 + 3;
                triangles[ti + 2] = vi * 4 + 1;
                triangles[ti + 3] = vi * 4 + 1;
                triangles[ti + 4] = vi * 4 + 3;
                triangles[ti + 5] = vi * 4 + 2;
            }
        }
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        //DestroyChildren();
    }

    //[ContextMenu("CombineMeshes")]
    void CombineMeshes()
    {

        MeshFilter[] mfChildren = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[mfChildren.Length];


        MeshRenderer[] mrChildren = GetComponentsInChildren<MeshRenderer>();
        Material[] materials = new Material[mrChildren.Length];


        MeshRenderer mrSelf = gameObject.GetComponent<MeshRenderer>();
        MeshFilter mfSelf = gameObject.GetComponent<MeshFilter>();


        Texture2D[] textures = new Texture2D[mrChildren.Length];
        for (int i = 0; i < mrChildren.Length; i++)
        {
            if (mrChildren[i].transform == transform)
            {
                continue;
            }
            materials[i] = mrChildren[i].sharedMaterial;


            Texture2D tx2D = new Texture2D(tx.width, tx.height, TextureFormat.ARGB32, false);
            tx2D.SetPixels(tx.GetPixels(0, 0, tx.width, tx.height));
            tx2D.Apply();
            textures[i] = tx2D;
        }


        Material materialNew = new Material(materials[0].shader);
        materialNew.CopyPropertiesFromMaterial(materials[0]);
        mrSelf.sharedMaterial = materialNew;


        Texture2D texture = new Texture2D(1024, 1024);
        materialNew.SetTexture("_MainTex", texture);
        Rect[] rects = texture.PackTextures(textures, 10, 1024);


        for (int i = 0; i < mfChildren.Length; i++)
        {
            if (mfChildren[i].transform == transform)
            {
                continue;
            }
            Rect rect = rects[i];


            Mesh meshCombine = mfChildren[i].mesh;
            Vector2[] uvs = new Vector2[meshCombine.uv.Length];
            //把网格的uv根据贴图的rect刷一遍
            for (int j = 0; j < uvs.Length; j++)
            {
                uvs[j].x = rect.x + meshCombine.uv[j].x * rect.width;
                uvs[j].y = rect.y + meshCombine.uv[j].y * rect.height;
            }
            meshCombine.uv = uvs;
            combine[i].mesh = meshCombine;
            combine[i].transform = mfChildren[i].transform.localToWorldMatrix;
            mfChildren[i].gameObject.SetActive(false);
        }


        Mesh newMesh = new Mesh();
        newMesh.CombineMeshes(combine, true, true);//合并网格
        mfSelf.mesh = newMesh;

    }

    void DestroyChildren()
    {
        while (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
    }
    bool CheckHasSameMat(List<Material> mats, Material mat)
    {
        for (int i = 0; i < mats.Count; i++)
        {
            if (mats[i] == mat)
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    public void OnBuildRoad(Vector3 pos,int buildRoadState)
    {
        buildRoadState++;
        switch (buildRoadState)
        {
            case 0:
                return;
            case 1:
                start = pos;
                Debug.Log("起点：" + pos);
                Debug.Log("请点击终点");
                break;
            case 2:
                end = pos;
                Debug.Log("终点：" + pos);
                Debug.Log("正在修改并保存数据");

                break;
            default:
                
                break;
        }
    }


    #region 存储与读取数据
    [ContextMenu("生成地图配置文件")]
    public void InitMapData()
    {
        MapData mapData = new MapData();
        mapData.roadGrids = new List<Vector2IntSerializer>();
        mapData.buildingGrids = new List<Vector2IntSerializer>();
        SaveMapData(mapData);
    }

    private void SaveMapData(MapData mapData)
    {
        string path = GetPath("staticData.bin");
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();
        file = File.Open(path, FileMode.Create);
        bf.Serialize(file, mapData);
        file.Close();
        Debug.Log("存储成功！");

    }

    public MapData GetMapData()
    {
        string path = GetPath("staticData.bin");
        MapData mapData;
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();
        file = File.Open(path, FileMode.Open);
        mapData = (MapData)bf.Deserialize(file);
        file.Close();
        return mapData;
    }
    private string GetPath(string fileName)
    {
        string path = string.Format("{0}/{1}{2}", Application.streamingAssetsPath + pathName,LevelManager.LevelID,fileName);
        return path;
    }
    #endregion
}

[System.Serializable]
public class MapData
{
    public List<Vector2IntSerializer> roadGrids;//已有道路的格子
    public List<Vector2IntSerializer> buildingGrids;//已有建筑的格子

}

[System.Serializable]
public struct Vector2IntSerializer
{
    public int x;
    public int y;

    public Vector2IntSerializer(int x,int z)
    {
        this.x = x;
        this.y = z;
    }

    public Vector2Int Vector2Int
    {
        get { return new Vector2Int(x, y); }
    }
}