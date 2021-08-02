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
    private int year = 1, month = 1, week = 1, day = 1;
    private bool hasSuccess = false;
    private bool pause = false;
    [SerializeField]private Material waterMat;
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
        }
        else
        {
            InitSavedLevelManager(GameManager.saveData);
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

    public void InitSavedLevelManager(SaveData saveData)
    {
        Debug.Log("InitSave");
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
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
        sw.Stop();
        System.TimeSpan dt = sw.Elapsed;
        Debug.Log("设置耗时:" + dt.TotalSeconds + "秒");
        sw.Restart();
        TechManager.Instance.InitTechBySave(saveData);
        sw.Stop();
        dt = sw.Elapsed;
        Debug.Log("TechManager耗时:" + dt.TotalSeconds + "秒");
        sw.Restart();
        ResourceManager.Instance.InitSavedResourceManager(saveData);
        sw.Stop();
        dt = sw.Elapsed;
        Debug.Log("ResourceManager耗时:" + dt.TotalSeconds + "秒");
        sw.Restart();
        TerrainGenerator.Instance.LoadTerrainFromSaveData(saveData);
        sw.Stop();
        dt = sw.Elapsed;
        Debug.Log("TerrainGenerator耗时:" + dt.TotalSeconds + "秒");
        sw.Restart();
        MapManager.Instance.InitMapManager();
        sw.Stop();
        dt = sw.Elapsed;
        Debug.Log("MapManager耗时:" + dt.TotalSeconds + "秒");
        sw.Restart();
        TreePlanter.Instance.PlantSaveTrees(saveData);
        sw.Stop();
        dt = sw.Elapsed;
        Debug.Log("TreePlanter耗时:" + dt.TotalSeconds + "秒");
        sw.Restart();
        InitSaveWater(saveData);
        InitWaterMat();
        sw.Stop();
        dt = sw.Elapsed;
        Debug.Log("Water耗时:" + dt.TotalSeconds + "秒");
        sw.Restart();
        BuildManager.Instance.InitBuildManager();
        BuildManager.Instance.InitSaveBuildings(saveData.buildingDatas);
        BuildManager.Instance.InitSaveBridges(saveData.bridgeDatas);
        sw.Stop();
        dt = sw.Elapsed;
        Debug.Log("BuildManager:" + dt.TotalSeconds + "秒");
        sw.Restart();
        MarketManager.Instance.InitSavedMarketManager();
        sw.Stop();
        dt = sw.Elapsed;
        Debug.Log("MarketManager:" + dt.TotalSeconds + "秒");
        sw.Restart();
        TrafficManager.Instance.InitSavedTrafficManager(saveData.driveDatas);
        sw.Stop();
        dt = sw.Elapsed;
        Debug.Log("TrafficManager:" + dt.TotalSeconds + "秒");
        //sw.Restart();
        MainInteractCanvas.InitCanvas();

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
