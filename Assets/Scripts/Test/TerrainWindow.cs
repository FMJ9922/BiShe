using UnityEngine;
using UnityEditor;

// 自定义编辑器窗口中实现Scene射线

public class TerrainWindow : EditorWindow
{
    int index = 0;
    bool canPaint = false;
    bool canSmooth = false;
    bool canDir = false;
    bool canBuildRoad = false;
    int dir = 0;
    int range = 0;
    int height = 0;
    int triDir = 0;
    private int buildRoadState;//cancel = 0,waitEnterStartPos = 1,waitEnterEndPos =2,waitEnterConfirm = 3.
    public delegate void Paint();
    public static event Paint OnPaint;
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
        if (!canPaint&&!canSmooth&&!canDir)
        {
            return;
        }
        Event currentEvent = Event.current;
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                TerrainGenerator gen = hit.collider.GetComponent<TerrainGenerator>();
                if (canPaint)
                {
                    gen.OnPaint(index, hit.point, dir);
                }
                else if (canDir)
                {
                    gen.OnReTriangle(hit.point, triDir);
                }
                else if(canSmooth)
                {
                    gen.OnFlatGround(hit.point, range, height);
                }
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("选择贴图");
        index = GUILayout.Toolbar(index, new string[16] { "0", "1", "2", "3", 
                                                          "4", "5", "6", "7",
                                                          "8", "9", "10", "11",
                                                          "12", "13", "14", "15"});
        GUILayout.Label("选择方向");
        dir = GUILayout.Toolbar(dir, new string[4] { "0", "1", "2", "3" });
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
            canDir= false;
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
    
}