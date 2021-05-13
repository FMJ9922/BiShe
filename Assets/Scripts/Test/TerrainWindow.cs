using UnityEngine;
using UnityEditor;

// 自定义编辑器窗口中实现Scene射线
/*
public class TerrainWindow : EditorWindow
{
    int index = 0;
    bool canPaint = false;
    bool canSmooth = false;
    bool canDir = false;
    bool canBuildRoad = false;
    int dir = 0;
    int brushSize = 1;
    int range = 0;
    int height = 0;
    int triDir = 0;
    int temp = 0;
    bool isShow = false;
    private int buildRoadState;//cancel = 0,waitEnterStartPos = 1,waitEnterEndPos =2,waitEnterConfirm = 3.
    public delegate void Paint();
    public static event Paint OnPaint;
    int[] sizelist = new int[4] { 1,2,4,8};
    [MenuItem("Window/TerrainWindow")]//在unity菜单Window下有MyWindow选项

    static void Init()
    {

        TerrainWindow myWindow = (TerrainWindow)EditorWindow.GetWindow(typeof(TerrainWindow), false, "TerrainWindow", true);//创建窗口
        myWindow.Show();//展示

    }
    private void OnEnable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
        //SceneView.onSceneGUIDelegate -= OnSceneGUI;
        //SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    private void OnDestroy()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        //SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        //Debug.Log(canPaint);

        if (!canPaint && !canSmooth && !canDir)
        {
            return;
        }
        Event currentEvent = Event.current;

        Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Ground")))
        {
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
            {
                isShow = !isShow;
            }
            if(currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.V)
            {
                dir++;
                dir %= 4;
                //Debug.Log(dir);
            }
            TerrainGenerator gen = GameObject.Find("TerrainGenerator").GetComponent<TerrainGenerator>();
            gen.ChangeShower(canPaint&&isShow, hit.point, dir,index);
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
            {
                if (canPaint)
                {
                    if(temp == 0)
                    {
                        gen.OnPaint(index, hit.point, 4, brushSize);
                    }
                    else
                    {
                        gen.OnPaint(index, hit.point, dir, brushSize);
                    }
                }
                else if (canDir)
                {
                    gen.OnReTriangle(hit.point, triDir);
                }
                else if (canSmooth)
                {
                    gen.OnFlatGround(hit.point, range, height);
                }
            }

        }

    }

    private void OnGUI()
    {
        GUILayout.Label("选择贴图");
        index = EditorGUILayout.IntSlider(index, 0, 63);
        GUILayout.Label("是否随机方向");
        
        temp = GUILayout.Toolbar(temp, new string[2] { "是", "否" });
        GUILayout.Label("选择笔刷大小");
        brushSize = EditorGUILayout.IntSlider(brushSize, 0, 100);

        if (GUILayout.Button("涂色"))
        {
            canPaint = true;
        }
        if (GUILayout.Button("停止涂色"))
        {
            canPaint = false;
        }
        GUILayout.Label("平整地形");
        GUILayout.Label("范围");
        range = EditorGUILayout.IntField(range);
        GUILayout.Label("高度");
        height = EditorGUILayout.IntField(height);
        if (GUILayout.Button("平整地形"))
        {
            canSmooth = true;
        }
        if (GUILayout.Button("取消平整"))
        {
            canSmooth = false;
        }
        if (GUILayout.Button("改地形0"))
        {
            canDir = true;
            triDir = 0;
        }
        if (GUILayout.Button("改地形1"))
        {
            canDir = true;
            triDir = 1;
        }
        if (GUILayout.Button("取消改地形"))
        {
            canDir = false;
        }
        if (GUILayout.Button("保存地图"))
        {

        }
        //if (GUILayout.Button("开始修路"))
        //{
        //    buildRoadState = 0;
        //    canBuildRoad = true;
        //}
        //if (GUILayout.Button("取消修路"))
        //{
        //    buildRoadState = 0;
        //    canBuildRoad = false;
        //}
    }

}*/