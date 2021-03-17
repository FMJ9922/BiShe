using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDCanvas : CanvasBase
{
    [SerializeField] TMP_Text _date, _money, _log, _stone, _food, _population;
    [SerializeField] Button time_stop,time_one,time_two,time_four;
    private string strMoney, strLog, strStone, strFood, strPopulation;
    private void OnDestroy()
    {
        EventManager.StopListening<string>(ConstEvent.OnDayWentBy, RefreshDate);
        EventManager.StopListening(ConstEvent.OnRefreshResources, RefreshResources);
        EventManager.StopListening(ConstEvent.OnPopulaitionChange, RefreshPopulation);
        time_stop.onClick.RemoveAllListeners();
        time_one.onClick.RemoveAllListeners();
        time_two.onClick.RemoveAllListeners();
        time_four.onClick.RemoveAllListeners();
    }
    public override void InitCanvas()
    {
        EventManager.StartListening<string>(ConstEvent.OnDayWentBy, RefreshDate);
        EventManager.StartListening(ConstEvent.OnRefreshResources, RefreshResources);
        EventManager.StartListening(ConstEvent.OnPopulaitionChange, RefreshPopulation);
        strMoney = Localization.ToSettingLanguage("Money");
        strLog = Localization.ToSettingLanguage("Log");
        strStone = Localization.ToSettingLanguage("Stone");
        strFood = Localization.ToSettingLanguage("Food");
        strPopulation = Localization.ToSettingLanguage("Population");
        time_stop.onClick.AddListener(()=> { GameManager.Instance.SetTimeScale(TimeScale.stop); });
        time_one.onClick.AddListener(()=> { GameManager.Instance.SetTimeScale(TimeScale.one); });
        time_two.onClick.AddListener(()=> { GameManager.Instance.SetTimeScale(TimeScale.two); });
        time_four.onClick.AddListener(()=> { GameManager.Instance.SetTimeScale(TimeScale.four); });
        RefreshResources();
        RefreshPopulation();
    }

    public override void OnOpen()
    {

    }

    public override void OnClose()
    {

    }

    public void RefreshDate(string date)
    {
        _date.text = date;
    }

    public void RefreshResources()
    {
        float money = ResourceManager.Instance.TryGetResourceNum(DataManager.GetItemIdByName("Money"));
        float log = ResourceManager.Instance.TryGetResourceNum(DataManager.GetItemIdByName("Log"));
        float stone = ResourceManager.Instance.TryGetResourceNum(DataManager.GetItemIdByName("Stone"));
        float food = ResourceManager.Instance.TryGetResourceTotalNum(DataManager.GetFoodIDList());
        _money.text = string.Format("{0}：{1}", strMoney, money);
        _log.text = string.Format("{0}：{1}", strLog, log);
        _stone.text = string.Format("{0}：{1}", strStone, stone);
        _food.text = string.Format("{0}：{1}", strFood, food);
    }

    public void RefreshPopulation()
    {
        //Debug.Log(ResourceManager.Instance.CurPopulation + " " + ResourceManager.Instance.MaxPopulation);
        _population.text = string.Format("{0}：{1}/{2}", strPopulation,
            ResourceManager.Instance.CurPopulation,ResourceManager.Instance.MaxPopulation);
    }
}
