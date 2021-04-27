using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDCanvas : CanvasBase
{
    [SerializeField] TMP_Text _date, _money, _log, _stone, _food, _population;
    [SerializeField] GameObject _mainCanvas;
    [SerializeField] private Slider progress;
    [SerializeField] Image _pause;
    [SerializeField] Image[] _scale;
    private string strMoney, strLog, strStone, strFood, strPopulation;
    private void OnDestroy()
    {
        EventManager.StopListening<string>(ConstEvent.OnDayWentBy, RefreshDate);
        EventManager.StopListening(ConstEvent.OnRefreshResources, RefreshResources);
        EventManager.StopListening(ConstEvent.OnPopulaitionChange, RefreshPopulation);
    }
    public override void InitCanvas()
    {
        EventManager.StartListening<string>(ConstEvent.OnDayWentBy, RefreshDate);
        EventManager.StartListening(ConstEvent.OnRefreshResources, RefreshResources);
        EventManager.StartListening(ConstEvent.OnPopulaitionChange, RefreshPopulation);
        EventManager.StartListening<TimeScale>(ConstEvent.OnTimeScaleChanged, OnTimeScaleChangeImage);
        strMoney = Localization.ToSettingLanguage("Money");
        strLog = Localization.ToSettingLanguage("Log");
        strStone = Localization.ToSettingLanguage("Stone");
        strFood = Localization.ToSettingLanguage("Food");
        strPopulation = Localization.ToSettingLanguage("Population");
        RefreshResources();
        RefreshPopulation();
        ChangeTimeScaleImage(GameManager.Instance.GetTimeScale());
        ChangePauseBtnImage(GameManager.Instance.GetTimeScale());
    }

    public void TogglePauseGame()
    {
        Debug.Log("pause");
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
        progress.value =LevelManager.Instance.WeekProgress;
    }
    public void RefreshResources()
    {
        float money = ResourceManager.Instance.TryGetResourceNum(DataManager.GetItemIdByName("Money"));
        float log = ResourceManager.Instance.TryGetResourceNum(DataManager.GetItemIdByName("Log"));
        float stone = ResourceManager.Instance.TryGetResourceNum(DataManager.GetItemIdByName("Stone"));
        float food = ResourceManager.Instance.TryGetResourceTotalNum(DataManager.GetFoodIDList());
        _money.text = string.Format("{1}", strMoney, money);
        _log.text = string.Format("{1}", strLog, log);
        _stone.text = string.Format("{1}", strStone, stone);
        _food.text = string.Format("{1}", strFood, food);
    }

    public void RefreshPopulation()
    {
        //Debug.Log(ResourceManager.Instance.CurPopulation + " " + ResourceManager.Instance.MaxPopulation);
        _population.text = string.Format("{1}/{2}", strPopulation,
            ResourceManager.Instance.CurPopulation,ResourceManager.Instance.MaxPopulation);
    }
}
