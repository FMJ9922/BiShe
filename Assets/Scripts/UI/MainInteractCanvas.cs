using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainInteractCanvas : CanvasBase
{
    [SerializeField] private Button[] buttons;
    [SerializeField] public CanvasBase[] canvas;

    public Button returnBtn;
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
        returnBtn.onClick.AddListener(GameManager.Instance.LoadMenuScene);
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
        buttons[0].image.sprite = LoadAB.LoadSprite("icon.ab", open ? "BuildSelect" : "BuildUnselect");
    }

    public void OpenResourceCanvas()
    {
        canvas[1].OnOpen();
    }

    public MarketCanvas GetMarketCanvas()
    {
        return (MarketCanvas)canvas[4];
    }

    public void OpenMarketCanvas()
    {
        GetMarketCanvas().OnOpen();
    }

    public void OpenCarMissionCanvas(GameObject car)
    {
        CarMissionCanvas carCanvas = (CarMissionCanvas)canvas[5];
        carCanvas.OnOpen(car);
    }

    public void OpenTechCanvas()
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
