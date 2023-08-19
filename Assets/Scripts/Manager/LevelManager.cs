using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using CSTools;
using UI;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    public MainInteractCanvas MainInteractCanvas;
    private float dayTime = 5f;
    public static int LevelID;
    private int year = 1, month = 1, week = 1, day = 1;
    private bool hasSuccess = false;
    private bool pause = false;
    [SerializeField]private Material waterMat;
    public float DayTime
    {
        get => dayTime;
    }

    //当前是游戏内的第几周
    public int WeekIndex
    {
        get
        {
            return (year - 1) * 48 + (month - 1) * 4 + (week - 1);
        }
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
                ResourceManager.Instance.UpdateItemHistroyNumDic();
                EventManager.TriggerEvent(ConstEvent.OnOutputResources);
                EventManager.TriggerEvent(ConstEvent.OnInputResources);
                TrafficManager.Instance.WeeklyCost = 0;
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

    public static bool GetPause()
    {
        return Instance.pause;
    }

    private void Start()
    {
        if (GameManager.saveData == null)
        {
            Scene scene = SceneManager.GetActiveScene();
            GameManager.saveData = GameManager.Instance.MakeSaveData(true, int.Parse(scene.name));
            //Debug.Log(GameManager.saveData.levelID);
            InitLevelManager(int.Parse(scene.name));
            EventManager.TriggerEvent(ConstEvent.OnLoadingOver);
        }
        else
        {
            StartCoroutine(InitSavedLevelManager(GameManager.saveData));
        }
        //Debug.Log("START");
        EventManager.StartListening(ConstEvent.OnPauseGame, PauseGame);
        EventManager.StartListening(ConstEvent.OnResumeGame, ResumeGame);
    }

    private void PauseGame()
    {
        StopWater();
        pause = true;
    }

    private void ResumeGame()
    {
        PlayWater();
        pause = false;
    }

    private void FixedUpdate()
    {
        //Debug.Log(pause);
        if (pause) return;
        if (Timer >= dayTime)
        {
            Timer = 0;
            string date;
            AddDay(out date);
            ResourceManager.Instance.UpdateItemDeltaNumDic();
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
        TechManager.Instance.Init();
        ResourceManager.Instance.InitResourceManager(LevelID);
        MapManager.Instance.InitMapManager();
        BuildManager.Instance.InitBuildManager();
        MarketManager.Instance.InitMarketManager();
        MainInteractCanvas.InitCanvas();
        InitWaterMat();
    }
    public void InitWaterMat()
    {
        for (int i = 0; i < TransformFinder.Instance.riverParent.childCount; i++)
        {
            TransformFinder.Instance.riverParent.GetChild(i).GetComponent<MeshRenderer>().material = waterMat;
        }
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

    public IEnumerator InitSavedLevelManager(SaveData saveData)
    {
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
        EventManager.TriggerEvent(ConstEvent.OnLoadingTips, Localization.Get("LoadTech"));
        yield return 0;
        TechManager.Instance.InitTechBySave(saveData);
        EventManager.TriggerEvent(ConstEvent.OnLoadingTips, Localization.Get("LoadResources"));
        yield return 0;
        ResourceManager.Instance.InitSavedResourceManager(saveData);
        EventManager.TriggerEvent(ConstEvent.OnLoadingTips, Localization.Get("LoadTerrain"));
        yield return 0;
        TerrainGenerator.Instance.LoadTerrainFromSaveData(saveData);
        EventManager.TriggerEvent(ConstEvent.OnLoadingTips, Localization.Get("LoadMap"));
        yield return 0;
        MapManager.Instance.InitMapManager();
        EventManager.TriggerEvent(ConstEvent.OnLoadingTips, Localization.Get("正在种树"));
        yield return 0;
        TreePlanter.Instance.PlantSaveTrees(saveData);
        EventManager.TriggerEvent(ConstEvent.OnLoadingTips, Localization.Get("正在加载水面"));
        yield return 0;
        InitSaveWater(saveData);
        InitWaterMat();
        EventManager.TriggerEvent(ConstEvent.OnLoadingTips, Localization.Get("正在加载已有建筑"));
        yield return 0;
        BuildManager.Instance.InitBuildManager();
        BuildManager.Instance.InitSaveBuildings(saveData.buildingDatas);
        EventManager.TriggerEvent(ConstEvent.OnLoadingTips, Localization.Get("正在加载桥梁"));
        yield return 0;
        BuildManager.Instance.InitSaveBridges(saveData.bridgeDatas);
        EventManager.TriggerEvent(ConstEvent.OnLoadingTips, Localization.Get("正在加载市场"));
        yield return 0;
        MarketManager.Instance.InitSavedMarketManager(saveData);
        EventManager.TriggerEvent(ConstEvent.OnLoadingTips, Localization.Get("正在加载小车车"));
        yield return 0;
        TrafficManager.Instance.InitSavedTrafficManager(saveData.driveDatas);
        EventManager.TriggerEvent(ConstEvent.OnLoadingTips, Localization.Get("正在加载UI"));
        yield return 0;
        //sw.Restart();
        MainInteractCanvas.InitCanvas();
        GameManager.Instance.sw.Stop();
        System.TimeSpan dt = GameManager.Instance.sw.Elapsed;
        EventManager.TriggerEvent(ConstEvent.OnLoadingTips, Localization.Get("加载完毕！用时") + CastTool.RoundOrFloat((float)dt.TotalSeconds) + "s");
        yield return 0;
        GameManager.Instance.sw.Reset();
        EventManager.TriggerEvent(ConstEvent.OnLoadingOver);
        //Debug.Log("AllInit:" + dt.TotalSeconds + "秒");
        //Debug.Log("Continue");
        GameManager.Instance.ContinueGame();
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

    public void StopWater()
    {
        //Debug.Log(waterMat.GetVector("_FoamSpeed"));
        waterMat.SetVector("_FoamSpeed", new Vector4(0,0,0,0));
        //Debug.Log(waterMat.GetVector("_FoamSpeed"));
    }

    public void PlayWater()
    {
        //Debug.Log(waterMat.GetVector("_FoamSpeed"));
        waterMat.SetVector("_FoamSpeed", new Vector4(1, 1, -1, -1));
        //Debug.Log(waterMat.GetVector("_FoamSpeed"));
    }
    

    public void CheckSuccess()
    {
        int aimMoney = DataManager.GetLevelData(LevelID).AimMoney;
        int aimPopulation = DataManager.GetLevelData(LevelID).AimPopulation;
        int aimHappiness = DataManager.GetLevelData(LevelID).AimHappiness;

        if (ResourceManager.Instance.TryGetResourceNum(99999) >= aimMoney
            && ResourceManager.Instance.AllPopulation >= aimPopulation
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
