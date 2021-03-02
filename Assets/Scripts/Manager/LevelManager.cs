using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class LevelManager : Singleton<LevelManager>
{
    public int LevelID { get; set; }
    private int year, month, week;
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

    private float Timer = 0;
    private string LogDate()
    {
        string yearstr = Localization.ToSettingLanguage("Year");
        string monthstr = Localization.ToSettingLanguage("Month");
        string weekstr = Localization.ToSettingLanguage("Week");

        string date = string.Format("{0}{3} {1}{4} {2}{5}", Year, Month, Week, yearstr, monthstr, weekstr);
        Debug.Log(date);
        return date;
        
    }

    private void AddWeek()
    {
        Week++;
        LogDate();
    }

    private void Start()
    {
        InitLevelManager(30001);
        year = 1;
        month = 1;
        week = 1;
        LogDate();
    }

    private void Update()
    {
        if (Timer >= 3f)
        {
            Timer = 0;

            EventManager.TriggerEvent(ConstEvent.OnOutputResources);
            EventManager.TriggerEvent(ConstEvent.OnInputResources);
            AddWeek();
        }
        else
        {
            Timer += Time.deltaTime;
        }
    }
    /// <summary>
    /// 初始化关卡所有Manager
    /// </summary>
    public void InitLevelManager(int levelID)
    {
        this.LevelID = levelID;
        ResourceManager.Instance.InitResourceManager(LevelID);
        MapManager.Instance.InitMapMnager(LevelID);
        BuildManager.Instance.InitBuildManager();
    }

    /// <summary>
    /// 退出关卡时
    /// </summary>
    private void OnDestroy()
    {
        EventManager.ClearEvents();
    }
}
