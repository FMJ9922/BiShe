using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{

    private FadeScene fadeScene;
    private TimeScale timeScale = TimeScale.one;
    private TimeScale lastTimeScale = TimeScale.one;
    public static SaveData saveData;

    protected override void InstanceAwake()
    {
        DontDestroyOnLoad(this.gameObject);
        LoadAB.Init();
    }


    public void TogglePauseGame(out TimeScale scale)
    {
        if (timeScale != TimeScale.stop)
        {
            PauseGame();
            scale = TimeScale.stop;
        }
        else
        {
            ContinueGame();
            scale = lastTimeScale;
        }
    }

    public void PauseGame()
    {
        TreeSystem.pause = true;
        SetTimeScale(TimeScale.stop);
    }

    public void ContinueGame()
    {
        TreeSystem.pause = false;
        SetTimeScale(lastTimeScale);
        EventManager.TriggerEvent(ConstEvent.OnResumeGame);
    }
    public TimeScale GetTimeScale()
    {
        return timeScale;
    }
    public void AddTimeScale(out TimeScale addedScale)
    {
        int scale = ((int)timeScale + 1) % 4;
        timeScale = (TimeScale)System.Enum.ToObject(typeof(TimeScale), scale);
        addedScale = timeScale;
        SetTimeScale(timeScale);
    }
    public void SetOneTimeScale()
    {
        SetTimeScale(TimeScale.one);
    }

    public void SetTwoTimeScale()
    {
        SetTimeScale(TimeScale.two);
    }

    public void SetFourTimeScale()
    {
        SetTimeScale(TimeScale.four);
    }

    /// <summary>
    /// 设置游戏的播放速率
    /// </summary>
    /// <param name="timeScale"></param>
    private void SetTimeScale(TimeScale scale)
    {
        //Debug.Log("时间倍速:" + scale.ToString());
        timeScale = scale;
        switch (scale)
        {
            case TimeScale.stop:
                EventManager.TriggerEvent(ConstEvent.OnPauseGame);
                break;
            case TimeScale.one:
                Time.timeScale = 1.0f;
                lastTimeScale = timeScale;
                EventManager.TriggerEvent(ConstEvent.OnResumeGame);
                break;
            case TimeScale.two:
                Time.timeScale = 2.0f;
                lastTimeScale = timeScale;
                break;
            case TimeScale.four:
                Time.timeScale = 4.0f;
                lastTimeScale = timeScale;
                break;
        }

        EventManager.TriggerEvent<TimeScale>(ConstEvent.OnTimeScaleChanged, timeScale);
    }
    public void QuitApplication()
    {
        Application.Quit();
    }

    public void LoadLevelScene(int levelNum)
    {
        SetTimeScale(TimeScale.one);
        LoadScene(levelNum);
    }
    public void LoadExampleScene()
    {
        FindFadeImage();
        fadeScene.Fade(1f, 0.5f);
        StartCoroutine(DelayToInvokeDo(() => { SceneManager.LoadScene("MainScene", LoadSceneMode.Single); }, 1f));
    }
    public void LoadMenuScene()
    {
        StaticBuilding.lists = new List<StaticBuilding>();
        FindFadeImage();
        fadeScene.Fade(1f, 0.5f);
        StartCoroutine(DelayToInvokeDo(() => { SceneManager.LoadScene("MenuScene", LoadSceneMode.Single); }, 1f));
    }

    //重载，用于加载游戏关卡场景
    private void LoadScene(int levelNum)
    {
        FindFadeImage();
        fadeScene.Fade(1f, 0.5f);
        //JsonIO.InitLevelData(iLevel);
        StartCoroutine(DelayToInvokeDo(() => { SceneManager.LoadScene(levelNum.ToString(), LoadSceneMode.Single); }, 1f));
        //JsonIO.InitLevelData(ilevel);//更新关卡数据
    }

    [ContextMenu("Load")]
    public void LoadTest()
    {
        FindFadeImage();
        fadeScene.Fade(1f, 0.5f);


        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        sw.Stop();
        System.TimeSpan dt = sw.Elapsed;
        StartCoroutine(DelayToInvokeDo(() => { SceneManager.LoadScene("level",LoadSceneMode.Single); }, 1f));
        SceneManager.sceneLoaded += CallBack;
    }
    public void CallBack(Scene scene, LoadSceneMode sceneType)
    {
        LoadSaveData("30001");
        Debug.Log(scene.name + "is load complete!");
    }

    private void LoadSaveData(string levelNum)
    {
        saveData = GameSaver.ReadSaveData(levelNum, true);
        StaticBuilding.lists = new List<StaticBuilding>();

    }
    private void FindFadeImage()
    {
        if (GameObject.Find("FadeImage"))
        {
            fadeScene = GameObject.Find("FadeImage").GetComponent<FadeScene>();
            //Debug.Log("FindFadeImage");
        }
        else
        {
            fadeScene = null;
            Debug.Log("There is no GameObject names 'FadeImage' in this scene");
        }
    }
    public IEnumerator DelayToInvokeDo(UnityAction action, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        action();
    }

    [ContextMenu("保存")]
    public void SaveLevel()
    {
        Debug.Log("开始保存");
        SaveData data = MakeSaveData(true);

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        GameSaver.WriteSaveData("30001", data,data.isOffcial);

        sw.Stop();
        System.TimeSpan dt = sw.Elapsed;
        Debug.Log("写入文件耗时:" + dt.TotalSeconds + "秒");
    }

    public SaveData MakeSaveData(bool isOffcial)
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        SaveData data = new SaveData();

        data.levelID = 30001;
        data.mapSize = new Vector2IntSerializer(DataManager.GetLevelData(data.levelID).Width,
            DataManager.GetLevelData(data.levelID).Length);
        data.mapName = data.levelID.ToString();
        //mesh
        Mesh mesh = TerrainGenerator.Instance.GetComponent<MeshFilter>().mesh;
        //data.meshName = mesh.name;
        //data.meshVerticles = Vector3Serializer.Box(mesh.vertices);
        //data.meshUV = Vector2Serializer.Box(mesh.uv);
        //data.meshTriangles = mesh.triangles;
        int[] meshTex = new int[data.mapSize.x * data.mapSize.y];
        //int[] meshDir = new int[data.mapSize.x * data.mapSize.y];
        int sizeX = data.mapSize.x;
        int sizeY = data.mapSize.y;
        int texLength = MapManager.TexLength;
        Vector2[] uv = mesh.uv;
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                int index = (i + j * sizeY) * 4;
                int x = Mathf.FloorToInt(uv[index].x * texLength);
                int y = Mathf.FloorToInt(uv[index].y * texLength);
                int tex = 8 * (7 - y) + x;
                float deltaX = x * 0.125f + 0.125f - uv[index].x;
                float deltaY = (7 - y) * 0.125f + 0.125f - uv[index].y;
                int dir = GetDir(deltaX, deltaY);
                //meshDir[i + j * sizeY] = dir;
                meshTex[i + j * sizeY] = tex;
            }
        }
        //data.meshDir = meshDir;
        data.meshTex = meshTex;
        
        //tree
        Transform treeParent = TransformFinder.Instance.treeParent;
        int treeChildCount = treeParent.childCount;
        TreeData[] treeDatas = new TreeData[treeChildCount];
        Vector3Serializer[] treePos= new Vector3Serializer[treeChildCount];
        Vector3Serializer[] treeRotation = new Vector3Serializer[treeChildCount];
        for (int i = 0; i < treeChildCount; i++)
        {
            Transform temp = treeParent.GetChild(i);
            treeDatas[i] = temp.GetComponent<TreeSystem>().treeData;
            treePos[i].Fill(temp.position);
            treeRotation[i].Fill(temp.localEulerAngles);
        }
        data.treeData = treeDatas;
        data.treePosition = treePos;
        data.treeRotation = treeRotation;

        //water
        Transform waterParent = TransformFinder.Instance.riverParent;
        int waterChildCount = waterParent.childCount;
        data.waterPos = new Vector3Serializer[waterChildCount];
        data.waterScale = new Vector3Serializer[waterChildCount];
        Transform p;
        for (int i = 0; i < waterChildCount; i++)
        {
            p = waterParent.GetChild(i);
            data.waterPos[i].Fill(p.position);
            data.waterScale[i].Fill(p.localScale);
        }
        //Debug.Log(waterChildCount);

        //buildings
        Transform buildingParent = TransformFinder.Instance.buildingParent;
        int buildingCount = buildingParent.childCount;
        data.buildingDatas = new RuntimeBuildData[buildingCount];
        BuildingBase building;
        for (int i = 0; i < buildingCount; i++)
        {
            p = buildingParent.GetChild(i);
            building = p.GetComponent<BuildingBase>();
            data.buildingDatas[i] = building.runtimeBuildData;
            data.buildingDatas[i].SavePosition.Fill(p.position);
            data.buildingDatas[i].SaveTakenGrids = 
                Vector2IntSerializer.Box(building.takenGrids);
            data.buildingDatas[i].SaveDir = building.direction;
            data.buildingDatas[i].SaveOutLookType = CheckOutLook(data.buildingDatas[i].Id, p.name);
            Debug.Log(building.runtimeBuildData.PfbName);
        }

        //LevelManager
        data.saveResources = ResourceManager.Instance.GetAllResources();
        data.curPopulation = ResourceManager.Instance.CurPopulation;
        LevelManager.Instance.SaveLevelManager(ref data);

        //MapManager
        data.gridNodes = MapManager.Instance.GetGrids();

        //MarketManager
        data.buyDatas = MarketManager.Instance.GetBuyDatas();
        data.sellDatas = MarketManager.Instance.GetSellDatas();

        //TrafficManager
        data.driveDatas = TrafficManager.Instance.GetDriveDatas();

        sw.Stop();
        System.TimeSpan dt = sw.Elapsed;
        Debug.Log("打包数据耗时:" + dt.TotalSeconds + "秒");
        return data;

    }
    public int CheckOutLook(int id,string name)
    {
        if (id == 20001 || id == 20009 || id == 20033
            || id == 20023 || id == 20024 || id == 20025
            || id == 20028|| id == 20026 || id == 20027)
        {
            name = name.Substring(name.Length - 1,1);
            return int.Parse(name);
        }
        return -1;
    }

    public int GetDir(float deltaX, float deltaY)
    {
        if (deltaX > 0)
        {
            if (deltaY > 0)
            {
                return 0;
            }
            else
            {
                return 3;
            }
        }
        else
        {
            if (deltaY > 0)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
    }

}
