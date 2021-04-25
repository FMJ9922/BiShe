using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditCanvas : MonoBehaviour
{
    int index = 0;
    bool canPaint = false;
    bool canSmooth = false;
    bool canDir = false;
    int dir = 0;
    int brushSize = 1;
    int range = 0;
    int height = 15;
    int triDir = 0;

    bool isOpen = false;

    private void OnEnable()
    {
        isOpen = true;

    }

    private void OnDisable()
    {
        canPaint = false;
        canSmooth = false;
        canDir = false;
        isOpen = false;
    }
    private void Update()
    {
        if (!isOpen)
        {
            return;
        }
        if (!canPaint && !canSmooth && !canDir)
        {
            return;
        }
        Event currentEvent = Event.current;
        if (Input.GetMouseButtonDown(1))
        {
            canPaint = false;
            canSmooth = false;
            canDir = false;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                TerrainGenerator gen = hit.collider.GetComponent<TerrainGenerator>();
                if (canPaint)
                {
                    gen.OnPaint(index, hit.point, dir, brushSize);
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
        GUILayout.BeginArea(new Rect(1470, 200, 400, 700));//固定布局  //Rect(float x,float y,float width,float height)  
        GUILayout.BeginVertical();//内层嵌套一个纵向布局 
        GUI.skin.label.normal.textColor = Color.black;
        GUILayout.Label("选择贴图");
        index = GUILayout.Toolbar(index, new string[16] { "0", "1", "2", "3",
                                                          "4", "5", "6", "7",
                                                          "8", "9", "10", "11",
                                                          "12", "13", "14", "15"});
        GUILayout.Label("选择方向");
        dir = GUILayout.Toolbar(dir, new string[4] { "0", "1", "2", "3" });
        GUILayout.Label("选择笔刷大小");
        brushSize = GUILayout.Toolbar(brushSize, new string[4] { "1", "2", "3", "5" });
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
        range = GUILayout.Toolbar(range, new string[4] { "1", "2", "3", "5" });
        GUILayout.Label("高度");
        //height = EditorGUILayout.IntSlider(height, 0, 20);
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

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
    
