using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainInteractCanvas : CanvasBase
{
    [SerializeField] private Button[] buttons;
    [SerializeField] public CanvasBase[] canvas;

    private static MainInteractCanvas _instance;

    public static MainInteractCanvas Instance { get { return _instance; } }


    #region 实现接口
    public override void InitCanvas()
    {
        if (null == Instance)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
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
    }

    public void OpenResourceCanvas()
    {
        canvas[1].OnOpen();
    }

    public void OpenMarketCanvas()
    {
        canvas[4].OnOpen();
    }

    public void OpenCarMissionCanvas(GameObject car)
    {
        CarMissionCanvas carCanvas = (CarMissionCanvas)canvas[5];
        carCanvas.OnOpen(car);
    }

    private void CloseAllOpenedUI()
    {
        foreach (var item in canvas)
        {
            item.OnClose();
        }
    }

    
}
