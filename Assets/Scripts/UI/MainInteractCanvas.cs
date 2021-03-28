using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainInteractCanvas : CanvasBase
{
    [SerializeField] private Button[] buttons;
    [SerializeField] public CanvasBase[] canvas; 


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

    public void ToggleBuildingCanvas()
    {
        BuildingCanvas build = (BuildingCanvas)canvas[0];
        bool open = build.ToggleBuildingCanvas();
        if (open)
        {
            canvas[3].OnClose();
        }
        else
        {
            canvas[3].OnOpen();
        }
    }

    private void CloseAllOpenedUI()
    {
        foreach (var item in canvas)
        {
            item.OnClose();
        }
    }

    
}
