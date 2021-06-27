using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    public MainInteractCanvas MainInteractCanvas;
    private float dayTime = 5f;
    public static int LevelID;
    private int year, month, week, day;
    private bool hasSuccess = false;
    private bool pause = false;
    public float DayTime
    {
        get => dayTime;
    }
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
                day = value - 7;
                ResourceManager.Instance.RecordLastWeekItem();
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
    private float WeekProgress = 0;
    private string yearstr, monthstr, weekstr, daystr;
    public string LogDate()
    {
        string date = string.Format("{0}/{1}/{2}", Year + 1999, Month, Day + (Week - 1) * 7);
        //Debug.Log(date);
        return date;

    }
    public float GetWeekProgress()
    {
        return WeekProgress;
    }
    private void AddDay(out string date)
    {
        Day++;
        date = LogDate();
    }

    private void Start()
    {
        if (GameManager.saveData == null)
        {
            Scene scene = SceneManager.GetActiveScene();
            GameManager.saveData = GameSaver.ReadSaveData(scene.name, true);
            InitLevelManager(int.Parse(scene.name));
            year = 1;
            month = 1;
            week = 1;
            day = 1;
        }
        else
        {
            InitSavedLevelManager(GameManager.saveData);
        }
        EventManager.StartListening(ConstEvent.OnPauseGame, PauseGame);
        EventManager.StartListening(ConstEvent.OnResumeGame, ResumeGame);
    }

    private void PauseGame()
    {
        pause = true;
    }

    private void ResumeGame()
    {
        pause = false;
    }

    private void FixedUpdate()
    {
        if (pause) return;
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
            if (!hasSuccess) CheckSuccess();
        }
    }
    /// <summary>
    /// 初始化关卡所有Manager
    /// </summary>
    public void InitLevelManager(int levelID)
    {
        Debug.Log("InitLevel:" + levelID);
        LevelManager.LevelID = levelID;
        ResourceManager.Instance.InitResourceManager(LevelID);
        MapManager.Instance.InitMapMnager();
        BuildManager.Instance.InitBuildManager();
        MarketManager.Instance.InitMarketManager();
        MainInteractCanvas.InitCanvas();
    }

    public void SaveLevelManager(ref SaveData saveData)
    {
        saveData.dayTime = dayTime;
        saveData.levelID = LevelID;
        saveData.year = year;
        saveData.month = month;
        saveData.week = week;
        saveData.day = day;
        saveData.hasSuccess = hasSuccess;
        saveData.pause = pause;
        saveData.timer = Timer;
        saveData.weekProgress = WeekProgress;
    }

    public void InitSavedLevelManager(SaveData saveData)
    {
        Debug.Log("InitSave");
        dayTime = saveData.dayTime;
        LevelID = saveData.levelID;
        year = saveData.year;
        month = saveData.month;
        week = saveData.week;
        day = saveData.day;
        hasSuccess = saveData.hasSuccess;
        pause = saveData.pause;
        Timer = saveData.timer;
        WeekProgress = saveData.weekProgress;

        ResourceManager.Instance.InitSavedResourceManager(saveData);
        MapManager.Instance.InitMapMnager();
        TreePlanter.Instance.PlantSaveTrees(GameManager.saveData);
        InitSaveWater(GameManager.saveData);
        TerrainGenerator.Instance.LoadTerrain(saveData.mapName, true);
        BuildManager.Instance.InitBuildManager();
        BuildManager.Instance.InitSaveBuildings(GameManager.saveData.buildingDatas);
        MarketManager.Instance.InitSavedMarketManager();
        TrafficManager.Instance.InitSavedTrafficManager(saveData.driveDatas);
        MainInteractCanvas.InitCanvas();
    }

    public void InitSaveWater(SaveData saveData)
    {
        GameObject ww = LoadAB.Load("building.ab", "ww");
        for (int i = 0; i < saveData.waterPos.Length; i++)
        {
            GameObject newWater = Instantiate(ww, TransformFinder.Instance.riverParent);
            newWater.SetActive(true);
            newWater.transform.position = saveData.waterPos[i].V3;
            newWater.transform.localScale = saveData.waterScale[i].V3;
        }
    }

    

    public void CheckSuccess()
    {
        int aimMoney = DataManager.GetLevelData(LevelID).AimMoney;
        int aimPopulation = DataManager.GetLevelData(LevelID).AimPopulation;
        int aimHappiness = DataManager.GetLevelData(LevelID).AimHappiness;

        if (ResourceManager.Instance.TryGetResourceNum(99999) >= aimMoney
            && ResourceManager.Instance.MaxPopulation >= aimPopulation
            && MapManager.Instance.GetHappiness() >= aimHappiness)
        {
            MainInteractCanvas.OpenSuccessCanvas();
            hasSuccess = true;
        }
    }
    /// <summary>
    /// 退出关卡时
    /// </summary>
    private void OnDestroy()
    {
        EventManager.ClearEvents();
        EventManager.StopListening(ConstEvent.OnPauseGame, PauseGame);
        EventManager.StopListening(ConstEvent.OnResumeGame, ResumeGame);
    }

    
}
