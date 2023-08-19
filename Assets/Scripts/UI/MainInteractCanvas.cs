using Building;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public enum CanvasType
    {
        BuildCanvas = 0,
        ResourceCanvas = 1,
        InfoCanvas = 2,
        HUDCanvas =3,
        MarketCanvas = 4,
        CarMissionCanvas = 5,
        TechCanvas = 6,
        SuccessCanvas = 7,
        ReturnCanvas = 8,
        SaveCanvas = 9,
        IntroduceCanvas = 10,
        ChooseSkillCanvas = 11,
        Max = 12,
    }
    public class MainInteractCanvas : CanvasBase
    {
        [SerializeField] private Button[] _buttons;
        [SerializeField] public CanvasBase[] _canvas;
        [SerializeField] private GameObject _childParentObj;

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
            foreach (var item in _canvas)
            {
                item.InitCanvas();
            }
            returnBtn.onClick.AddListener(OpenReturnCanvas);
            EventManager.StartListening<BuildingBase>(ConstEvent.OnTriggerInfoPanel, OpenInfoCanvas);
            
            //ToggleCanvas(CanvasType.ChooseSkillCanvas,true);
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

        public void ToggleCanvas(CanvasType type,bool isActive)
        {
            var canvas = _canvas[(int) type];
            if (isActive)
            {
                canvas.OnOpen();
            }
            else
            {
                canvas.OnClose();
            }
        }

        public void ToggleBuildingCanvas()
        {
            BuildingCanvas build = (BuildingCanvas)_canvas[0];
            bool open = build.ToggleBuildingCanvas();
            _buttons[0].image.sprite = LoadAB.LoadSprite("icon.ab", open ? "BuildSelect" : "BuildUnselect");
            CloseInfoCanvas();
            CloseMarketCanvas();
            CloseResourcesCanvas();
            CloseCarMissionCanvas();
        }

        public void OpenBuildingCanvas()
        {
            _canvas[0].OnOpen();
            _buttons[0].image.sprite = LoadAB.LoadSprite("icon.ab", "BuildUnselect");
        }

        public void CloseBuildingCanvas()
        {
            _canvas[0].OnClose();
            _buttons[0].image.sprite = LoadAB.LoadSprite("icon.ab", "BuildSelect");
        }

        public void OpenResourceCanvas(BuildingBase data)
        {
            CloseAllOpenedUI();
            ((ResourceCanvas)_canvas[1]).OnOpen((StorageBuilding)data);
        }


        public SaveCanvas GetSaveCanvas()
        {
            return (SaveCanvas)_canvas[9];
        }

        public ChooseSkillCanvas GetChooseSkillCanvas()
        {
            return (ChooseSkillCanvas)_canvas[11];
        }

        public void OpenChooseSkillCanvas()
        {
            CloseAllOpenedUI();
            GetChooseSkillCanvas().OnOpen();
        }
    

        public void OpenMarketCanvas()
        {
            CloseAllOpenedUI();
            ToggleCanvas(CanvasType.MarketCanvas,true);
        }

        public void OpenInfoCanvas(BuildingBase data)
        {
            CloseAllOpenedUI();
            (_canvas[(int)CanvasType.InfoCanvas] as InfoCanvas)?.OnOpen(data);
        }
        public void OpenCarMissionCanvas(CarDriver car)
        {
            CloseAllOpenedUI();
            (_canvas[(int)CanvasType.CarMissionCanvas] as CarMissionCanvas)?.OnOpen(car);
        }

        public void OpenTechCanvas()
        {
            CloseAllOpenedUI();
            ToggleCanvas(CanvasType.TechCanvas,true);
        }

        public void OpenSuccessCanvas()
        {
            CloseAllOpenedUI();
            ToggleCanvas(CanvasType.SuccessCanvas,true);
        }
        public void OpenReturnCanvas()
        {
            CloseAllOpenedUI();
            ToggleCanvas(CanvasType.ReturnCanvas,true);
        }

        public void OpenSaveCanvas()
        {
            CloseAllOpenedUI();
            ToggleCanvas(CanvasType.SaveCanvas,true);
        }
        public void CloseCarMissionCanvas()
        {
            ToggleCanvas(CanvasType.CarMissionCanvas,false);
        }
        public void OpenIntroduceCanvas()
        {
            ToggleCanvas(CanvasType.IntroduceCanvas,true);
        }

        public void CloseIntroduceCanvas()
        {
            ToggleCanvas(CanvasType.IntroduceCanvas,false);
        }

        public void CloseResourcesCanvas()
        {
            ToggleCanvas(CanvasType.ResourceCanvas,false);
        }

        public void CloseMarketCanvas()
        {
            ToggleCanvas(CanvasType.MarketCanvas,false);
        }

        public void CloseInfoCanvas()
        {
            ToggleCanvas(CanvasType.InfoCanvas,false);
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
            _buttons[0].gameObject.SetActive(false);
        }

        public void ShowBuildingButton()
        {
            _buttons[0].gameObject.SetActive(true);
        }

        //开关所有非canvasUI元素
        public void ToggleInteractUI(bool isActive)
        {
            _childParentObj.SetActive(isActive);
            if (isActive)
            {
                _instance._canvas[3].OnOpen();
            }
            else
            {
                _instance._canvas[3].OnClose();
            }
        
        }
    
    }
}
