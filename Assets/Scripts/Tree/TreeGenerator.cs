using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
/*
public class TreeGenerator : EditorWindow
{
    private bool canPlantTree = false;
    public float range = 0;
    public float density = 0;
    private float minDis = 2f;
    [MenuItem("Window/PlantTreeWindow")]
    static void Init()
    {
        TreeGenerator myWindow = (TreeGenerator)EditorWindow.GetWindow(typeof(TreeGenerator), false, "TreeGenerator", true);
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
        
        Event currentEvent = Event.current;

        Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Ground")))
        {
            //TerrainGenerator gen = GameObject.Find("TerrainGenerator").GetComponent<TerrainGenerator>();
            TreePlanter planter = GameObject.Find("TerrainGenerator").transform.GetChild(0).GetComponent<TreePlanter>();
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
            {
                if (canPlantTree)
                {
                    planter.PlantTree(hit.point, range, density);
                }
            }

        }

    }
    private void OnGUI()
    {
        GUILayout.Label("选择密度");
        density = EditorGUILayout.Slider(density, 0.0f, 1.0f);
        GUILayout.Label("选择半径");
        range = EditorGUILayout.Slider(range, 0.0f, 300.0f);
        if (GUILayout.Button("开始种树"))
        {
            canPlantTree = true;
        }
        if (GUILayout.Button("停止种树"))
        {
            canPlantTree = false;
        }
    }
        
}*/
