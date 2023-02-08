using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DrawTest : MonoBehaviour
{
    /// <summary>
    /// 材质球
    /// </summary>
    public Material mat;
    public Color OutLine;
    public Color Inside;
    /// <summary>
    /// 鼠标开始的位置
    /// </summary>
    private Vector2 FirstMousePosition;
    /// <summary>
    /// 鼠标结束的位置
    /// </summary>
    private Vector2 SecondMousePosition;
    private bool StartRender = false;
    [SerializeField]
    private GameObject[] gameobjects;
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //获取鼠标按下
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Down");
            StartRender = true;
            FirstMousePosition = Input.mousePosition;
            PickGameObject();
        }
        //获取鼠标抬起
        if (Input.GetMouseButtonUp(0))
        {
            //Debug.Log("Up");
            StartRender = false;
            ChangeTwoPoint();
            PickGameObject();
            FirstMousePosition = SecondMousePosition = Vector2.zero;
        }
        SecondMousePosition = Input.mousePosition;
    }

    private void OnPostRender()
    {
        //Debug.Log("render");
        if (StartRender)
        {
            GL.PushMatrix();
            mat.SetPass(0);
            GL.LoadPixelMatrix();
            GL.Begin(GL.QUADS);
            GL.Color(Inside);
            GL.Vertex3(FirstMousePosition.x, FirstMousePosition.y, 0);
            GL.Vertex3(SecondMousePosition.x, FirstMousePosition.y, 0);
            GL.Vertex3(SecondMousePosition.x, SecondMousePosition.y, 0);
            GL.Vertex3(FirstMousePosition.x, SecondMousePosition.y, 0);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Color(OutLine);//设置方框的边框颜色 边框不透明
            GL.Vertex3(FirstMousePosition.x, FirstMousePosition.y, 0);
            GL.Vertex3(SecondMousePosition.x, FirstMousePosition.y, 0);
            GL.Vertex3(SecondMousePosition.x, SecondMousePosition.y, 0);
            GL.Vertex3(FirstMousePosition.x, SecondMousePosition.y, 0);
            GL.Vertex3(FirstMousePosition.x, FirstMousePosition.y, 0);
            GL.Vertex3(FirstMousePosition.x, SecondMousePosition.y, 0);
            GL.Vertex3(SecondMousePosition.x, FirstMousePosition.y, 0);
            GL.Vertex3(SecondMousePosition.x, SecondMousePosition.y, 0);
            GL.End();
            GL.PopMatrix();//恢复摄像机投影矩阵
        }
    }
   
    /// <summary>
    /// 改变两点
    /// </summary>
    private void ChangeTwoPoint()
    {
        if (FirstMousePosition.x > SecondMousePosition.x)
        {
            float position1 = FirstMousePosition.x;
            FirstMousePosition.x = SecondMousePosition.x;
            SecondMousePosition.x = position1;
        }
        if (FirstMousePosition.y > SecondMousePosition.y)
        {
            float position2 = FirstMousePosition.y;
            FirstMousePosition.y = SecondMousePosition.y;
            SecondMousePosition.y = position2;
        }
    }
    /// <summary>
    /// 改变物体颜色
    /// </summary>
    private void PickGameObject()
    {
        //遍历所有的组件
        foreach (GameObject item in gameobjects)
        {
            if (!item.CompareTag("Unit"))
            {
                continue;
            }
            //判断位置
            Vector3 position = Camera.main.WorldToScreenPoint(item.transform.position);
            if (position.x >= FirstMousePosition.x & position.x <= SecondMousePosition.x & position.y >= FirstMousePosition.y & position.y <= SecondMousePosition.y)
            {
                item.GetComponentInChildren<SpriteRenderer>().material.color = Color.red;
            }
            else
            {
                item.GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
            }


        }
    }
}