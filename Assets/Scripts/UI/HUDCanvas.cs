using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDCanvas : CanvasBase
{
    [SerializeField] TMP_Text _date, _population;
    [SerializeField] GameObject _mainCanvas;
    [SerializeField] private Slider progress;
    [SerializeField] Image _pause;
    [SerializeField] Image[] _scale;
    [SerializeField] Button graphBtn;
    [SerializeField] Transform hudParent;
    [SerializeField] private GameObject _pfbHUDItem;

    List<HUDItem> hudItems;
    private string strPopulation,strWeek;

    private void OnDestroy()
    {
        EventManager.StopListening<string>(ConstEvent.OnDayWentBy, RefreshDate);
        EventManager.StopListening(ConstEvent.OnRefreshResources, RefreshResources);
        EventManager.StopListening(ConstEvent.OnPopulationHudChange, RefreshPopulation);
        EventManager.StopListening<TimeScale>(ConstEvent.OnTimeScaleChanged, OnTimeScaleChangeImage);
        EventManager.StopListening(ConstEvent.OnHudItemChange, ResetHudItems);
        graphBtn.onClick.RemoveAllListeners();
    }
    public override void InitCanvas()
    {
        EventManager.StartListening<string>(ConstEvent.OnDayWentBy, RefreshDate);
        EventManager.StartListening(ConstEvent.OnRefreshResources, RefreshResources);
        EventManager.StartListening(ConstEvent.OnPopulationHudChange, RefreshPopulation);
        EventManager.StartListening<TimeScale>(ConstEvent.OnTimeScaleChanged, OnTimeScaleChangeImage);
        EventManager.StartListening(ConstEvent.OnHudItemChange, ResetHudItems);
        strPopulation = Localization.Get("Population");
        strWeek = Localization.Get("Week");
        InitHUDItems();
        RefreshResources();
        RefreshPopulation();
        ChangeTimeScaleImage(GameManager.Instance.GetTimeScale());
        ChangePauseBtnImage(GameManager.Instance.GetTimeScale());
        graphBtn.onClick.AddListener(GraphManager.Instance.Toggle);
        RefreshDate(LevelManager.Instance.LogDate());
    }

    /// <summary>
    /// 在监视器窗口监视物品有变化时调用
    /// </summary>
    public void ResetHudItems()
    {
        CleanUpAllAttachedChildren(hudParent,1);
        InitHUDItems();
        RefreshResources();
    }

    private void InitHUDItems()
    {
        
        List<int> list = ResourceManager.Instance._hudList;
        hudItems = new List<HUDItem>();
        for (int i = 0; i < list.Count; i++)
        {
            GameObject newHudItem = Instantiate(_pfbHUDItem, hudParent);
            HUDItem item = newHudItem.GetComponent<HUDItem>();
            item.Init(DataManager.GetItemDataById(list[list.Count - 1-i]));
            newHudItem.transform.SetAsFirstSibling();
            hudItems.Add(item);
        }
    }

    public void TogglePauseGame()
    {
        GameManager.Instance.TogglePauseGame(out TimeScale scale);
    }

    public void OnTimeScaleChangeImage(TimeScale scale)
    {
        ChangePauseBtnImage(scale);
        ChangeTimeScaleImage(scale);
    }

    private void ChangePauseBtnImage(TimeScale scale)
    {
        switch (scale)
        {
            case TimeScale.stop:
                _pause.sprite = LoadAB.LoadSprite(ConstString.IconBundle, ConstString.PlayButton);
                break;
            default:
                _pause.sprite = LoadAB.LoadSprite(ConstString.IconBundle, ConstString.PauseButton);
                break;
        }
        _pause.SetNativeSize();
    }

    public void AddTimeScale()
    {
        //Debug.Log("add");
        GameManager.Instance.AddTimeScale(out TimeScale scale);
    }

    private void ChangeTimeScaleImage(TimeScale scale)
    {
        int speed = (int)scale;
        for (int i = 0; i < _scale.Length; i++)
        {
            if (i < speed)
            {
                _scale[i].sprite = LoadAB.LoadSprite(ConstString.IconBundle, ConstString.SolidPlay);
            }
            else
            {
                _scale[i].sprite = LoadAB.LoadSprite(ConstString.IconBundle, ConstString.HollowPlay);
            }
            _scale[i].SetNativeSize();
        }
    }

    private void FixedUpdate()
    {
        RefreshSliderValue();
    }
    public override void OnOpen()
    {
        _mainCanvas.SetActive(true);
    }

    public override void OnClose()
    {
        _mainCanvas.SetActive(false);
    }

    public void RefreshDate(string date)
    {
        _date.text = date;
    }

    public void RefreshSliderValue()
    {
        progress.value =LevelManager.Instance.GetWeekProgress();
    }
    public void RefreshResources()
    {
        for (int i = 0; i < hudItems.Count; i++)
        {
            hudItems[i].Refresh();
        }
    }

    public void RefreshPopulation()
    {
        //Debug.Log(ResourceManager.Instance.CurPopulation + " " + ResourceManager.Instance.MaxPopulation);
        _population.text = string.Format("{1}/{2}", strPopulation,
            ResourceManager.Instance.WorkerPopulation,ResourceManager.Instance.AllPopulation);
    }
}
