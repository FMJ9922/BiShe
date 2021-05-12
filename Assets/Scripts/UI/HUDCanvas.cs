using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDCanvas : CanvasBase
{
    [SerializeField] TMP_Text _date, _money, _log, _stone, _food, _population;
    [SerializeField] TMP_Text _deltaMoney, _deltaLog, _deltaStone, _deltaFood;
    [SerializeField] GameObject _mainCanvas;
    [SerializeField] private Slider progress;
    [SerializeField] Image _pause;
    [SerializeField] Image[] _scale;
    [SerializeField] Button graphBtn;
    private string strMoney, strLog, strStone, strFood, strPopulation,strWeek;
    private void OnDestroy()
    {
        EventManager.StopListening<string>(ConstEvent.OnDayWentBy, RefreshDate);
        EventManager.StopListening(ConstEvent.OnRefreshResources, RefreshResources);
        EventManager.StopListening(ConstEvent.OnPopulaitionChange, RefreshPopulation);
        graphBtn.onClick.RemoveAllListeners();
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
        strWeek = Localization.ToSettingLanguage("Week");
        RefreshResources();
        RefreshPopulation();
        ChangeTimeScaleImage(GameManager.Instance.GetTimeScale());
        ChangePauseBtnImage(GameManager.Instance.GetTimeScale());
        graphBtn.onClick.AddListener(GraphManager.Instance.Toggle);
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
        progress.value =LevelManager.Instance.GetWeekProgress();
    }
    public void RefreshResources()
    {
        
        float money = ResourceManager.Instance.TryGetResourceNum(DataManager.GetItemIdByName("Money"));
        float log = ResourceManager.Instance.TryGetResourceNum(DataManager.GetItemIdByName("Log"));
        float stone = ResourceManager.Instance.TryGetResourceNum(DataManager.GetItemIdByName("Stone"));
        float food = ResourceManager.Instance.GetAllFoodNum();
        float deltaMoney = ResourceManager.Instance.GetWeekDeltaNum(DataManager.GetItemIdByName("Money"));
        float deltaLog = ResourceManager.Instance.GetWeekDeltaNum(DataManager.GetItemIdByName("Log"));
        float deltaStone = ResourceManager.Instance.GetWeekDeltaNum(DataManager.GetItemIdByName("Stone"));
        float deltaFood = ResourceManager.Instance.GetWeekDeltaNum(DataManager.GetItemIdByName("Food"));
        _money.text = string.Format("{1}", strMoney, (int)money);
        _log.text = string.Format("{1}", strLog, (int)log);
        _stone.text = string.Format("{1}", strStone, (int)stone);
        _food.text = string.Format("{1}", strFood, (int)food);
        _deltaMoney.text = string.Format("{2}{0}/{1}", (int)Mathf.Abs(deltaMoney), strWeek,deltaMoney>=0?"+":"-");
        _deltaLog.text = string.Format("{2}{0}/{1}", (int)Mathf.Abs(deltaLog), strWeek, deltaLog >= 0 ? "+" : "-");
        _deltaStone.text = string.Format("{2}{0}/{1}", (int)Mathf.Abs(deltaStone), strWeek, deltaStone >= 0 ? "+" : "-");
        _deltaFood.text = string.Format("{2}{0}/{1}", (int)Mathf.Abs(deltaFood), strWeek, deltaFood >= 0 ? "+" : "-");
    }

    public void RefreshPopulation()
    {
        //Debug.Log(ResourceManager.Instance.CurPopulation + " " + ResourceManager.Instance.MaxPopulation);
        _population.text = string.Format("{1}/{2}", strPopulation,
            ResourceManager.Instance.CurPopulation,ResourceManager.Instance.MaxPopulation);
    }
}
