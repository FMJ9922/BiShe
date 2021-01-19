using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainInteractCanvas : CanvasBase
{
    [SerializeField] private Button[] buttons;
    [SerializeField] public CanvasBase[] canvas; 

    private void Start()
    {
        InitCanvas();
    }

    #region 实现接口
    public override void InitCanvas()
    {
        foreach (var item in canvas)
        {
            item.InitCanvas();
        }
    }

    public override void OnOpen()
    {

    }

    public override void OnClose()
    {

    }
    #endregion

    private void InitInteractCanvas()
    {

    }

    private void CloseAllOpenedUI()
    {
        foreach (var item in canvas)
        {
            item.OnClose();
        }
    }

    
}
