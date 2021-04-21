using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class LevelManager : Singleton<LevelManager>
{
    [SerializeField] MainInteractCanvas MainInteractCanvas;

    public float dayTime = 3f;
    public static int LevelID = 30001;
    private int year, month, week,day;
    public int Year
    {
        get
        {
            return year;
        }
        set
        {
            year = value;
        }
    }
    public int Month
    {
        get
        {
            return month;
        }
        set
        {
            if (value > 12)
            {
                year++;
                month = value - 12;
            }
            else
            {
                month = value;
            }
        }
    }
    public int Week
    {
        get
        {
            return week;
        }
        set
        {
            if (value > 4)
            {
                Month++;
                week = value - 4;
            }
            else
            {
                week = value;
            }
        }
    }
    public int Day
    {
        get
        {
            return day;
        }
        set
        {
            if (value > 7)
            {
                Week++;
                day = value -7;
                EventManager.TriggerEvent(ConstEvent.OnOutputResources);
                EventManager.TriggerEvent(ConstEvent.OnInputResources);
                EventManager.TriggerEvent(ConstEvent.OnSettleAccount);
            }
            else
            {
                day = value;
            }
        }
    }

    private float Timer = 0;
    public float WeekProgress = 0;
    private string yearstr, monthstr, weekstr, daystr;
    private string LogDate()
    {
        string date = string.Format("{0}/{1}/{2}", Year+1999, Month, Day+(Week-1)*7);
        //Debug.Log(date);
        return date;
        
    }

    private void AddDay(out string date)
    {
        Day++;
        date = LogDate();
    }

    private void Start()
    {
        InitLevelManager(30001);
        year = 1;
        month = 1;
        week = 1;
        day = 1;
        yearstr = Localization.ToSettingLanguage("Year");
        monthstr = Localization.ToSettingLanguage("Month");
        weekstr = Localization.ToSettingLanguage("Week");
        daystr = Localization.ToSettingLanguage("Day");
    }

    private void FixedUpdate()
    {
        if (Timer >= dayTime)
        {
            Timer = 0;
            string date;
            AddDay(out date);
            EventManager.TriggerEvent<string>(ConstEvent.OnDayWentBy, date);
            EventManager.TriggerEvent(ConstEvent.OnRefreshResources);
        }
        else
        {
            Timer += Time.deltaTime;
            WeekProgress = (dayTime * (day - 1) + Timer) / (dayTime * 7);
        }
    }
    /// <summary>
    /// 初始化关卡所有Manager
    /// </summary>
    public void InitLevelManager(int levelID)
    {
        LevelManager.LevelID = levelID;
        ResourceManager.Instance.InitResourceManager(LevelID);
        RoadManager.Instance.InitRoadManager();
        MapManager.Instance.InitMapMnager(LevelID);
        BuildManager.Instance.InitBuildManager();
        MarketManager.Instance.InitMarketManager();
        MainInteractCanvas.InitCanvas();
    }

    /// <summary>
    /// 退出关卡时
    /// </summary>
    private void OnDestroy()
    {
        EventManager.ClearEvents();
    }
}
