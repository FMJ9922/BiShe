using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDCanvas : CanvasBase
{
    [SerializeField] TMP_Text _date, _money, _log, _stone;
    private string strMoney, strLog, strStone;
    public override void InitCanvas()
    {
        EventManager.StartListening<string>(ConstEvent.OnDayWentBy, RefreshDate);
        EventManager.StartListening(ConstEvent.OnRefreshResources, RefreshResources);
        strMoney = Localization.ToSettingLanguage("Money");
        strLog = Localization.ToSettingLanguage("Log");
        strStone = Localization.ToSettingLanguage("Stone");
        RefreshResources();
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
        Debug.Log("Refresh");
        float money = ResourceManager.Instance.TryGetResourceNum(DataManager.GetItemIdByName("Money"));
        float log = ResourceManager.Instance.TryGetResourceNum(DataManager.GetItemIdByName("Log"));
        float stone = ResourceManager.Instance.TryGetResourceNum(DataManager.GetItemIdByName("Stone"));
        _money.text = string.Format("{0}：{1}", strMoney, money);
        _log.text = string.Format("{0}：{1}", strLog, log);
        _stone.text = string.Format("{0}：{1}", strStone, stone);
    }
}
