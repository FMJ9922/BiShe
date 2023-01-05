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
        returnBtn.onClick.AddListener(OpenReturnCanvas);
        EventManager.StartListening<BuildingBase>(ConstEvent.OnTriggerInfoPanel, OpenInfoCanvas);
    }

    private void OnDestroy()
    {
        EventManager.StopListening<BuildingBase>(ConstEvent.OnTriggerInfoPanel, OpenInfoCanvas);
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
        CloseInfoCanvas();
        CloseMarketCanvas();
        CloseResourcesCanvas();
        CloseCarMissionCanvas();
    }

    public void OpenBuildingCanvas()
    {
        canvas[0].OnOpen();
        buttons[0].image.sprite = LoadAB.LoadSprite("icon.ab", "BuildUnselect");
    }

    public void CloseBuildingCanvas()
    {
        canvas[0].OnClose();
        buttons[0].image.sprite = LoadAB.LoadSprite("icon.ab", "BuildSelect");
    }

    public void OpenResourceCanvas(BuildingBase data)
    {
        CloseAllOpenedUI();
        ((ResourceCanvas)canvas[1]).OnOpen((StorageBuilding)data);
    }

    public MarketCanvas GetMarketCanvas()
    {
        return (MarketCanvas)canvas[4];
    }

    public SaveCanvas GetSaveCanvas()
    {
        return (SaveCanvas)canvas[9];
    }

    public void OpenMarketCanvas()
    {
        CloseAllOpenedUI();
        GetMarketCanvas().OnOpen();
    }

    public void OpenInfoCanvas(BuildingBase data)
    {
        CloseAllOpenedUI();
        InfoCanvas infocanvas = (InfoCanvas)canvas[2];
        infocanvas.OnOpen(data);
    }
    public void OpenCarMissionCanvas(CarDriver car)
    {
        CloseAllOpenedUI();
        CarMissionCanvas carCanvas = (CarMissionCanvas)canvas[5];
        carCanvas.OnOpen(car);
    }

    public void OpenTechCanvas()
    {
        CloseAllOpenedUI();
        TechTreeCanvas techCanvas = (TechTreeCanvas)canvas[6];
        techCanvas.OnOpen();
    }

    public void OpenSuccessCanvas()
    {
        CloseAllOpenedUI();
        SuccessCanvas successCanvas = (SuccessCanvas)canvas[7];
        successCanvas.OnOpen();
    }
    public void OpenReturnCanvas()
    {
        CloseAllOpenedUI();
        ReturnCanvas returnCanvas = (ReturnCanvas)canvas[8];
        returnCanvas.OnOpen();
    }

    public void OpenSaveCanvas()
    {
        CloseAllOpenedUI();
        SaveCanvas saveCanvas = (SaveCanvas)canvas[9];
        saveCanvas.OnOpen();
    }
    public void CloseCarMissionCanvas()
    {
        canvas[5].OnClose();
    }
    public void OpenIntroduceCanvas()
    {
        IntroduceCanvas intro = canvas[10] as IntroduceCanvas;
        intro.OnOpen();
    }

    public void CloseIntroduceCanvas()
    {
        IntroduceCanvas intro = canvas[10] as IntroduceCanvas;
        intro.OnClose();
    }

    public void CloseResourcesCanvas()
    {
        canvas[1].OnClose();
    }

    public void CloseMarketCanvas()
    {
        canvas[4].OnClose();
    }

    public void CloseInfoCanvas()
    {
        canvas[2].OnClose();
    }
    public void CloseAllOpenedUI()
    {
        CloseBuildingCanvas();
        CloseInfoCanvas();
        CloseMarketCanvas();
        CloseResourcesCanvas();
        CloseCarMissionCanvas();
    }

    public void HideBuildingButton()
    {
        buttons[0].gameObject.SetActive(false);
    }

    public void ShowBuildingButton()
    {
        buttons[0].gameObject.SetActive(true);
    }
    
}
