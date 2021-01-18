using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public int LevelID { get; set; }


    private void Start()
    {
        InitLevelManager(30001);
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
