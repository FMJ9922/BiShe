using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBase : MonoBehaviour
{
    //占地长宽
    [SerializeField]
    private int Width;
    [SerializeField]
    private int Height;


    public Vector2Int Size => new Vector2Int(Height, Width);

    public bool hasAnima = false;

    public Animation animation;

    public GameObject body;

    public RuntimeBuildData runtimeBuildData;

    public bool buildFlag = false;//用于判断是否是来自存档的建造

    public Vector2Int[] takenGrids;

    protected FormulaData formula = new FormulaData();


    public Vector2Int parkingGridIn;
    //public Vector2Int parkingGridOut;


    public Direction direction;

    protected Storage storage;

    public virtual void OnConfirmBuild(Vector2Int[] vector2Ints)
    {
        takenGrids = vector2Ints;
        gameObject.tag = "Building";
        transform.GetComponent<BoxCollider>().enabled = false;
        transform.GetComponent<BoxCollider>().enabled = true;
        
        if (!buildFlag)
        {
            buildFlag = true;
            if (hasAnima)
            {
                Invoke("PlayAnim", 0.2f);
            }
            direction = CastTool.CastVector3ToDirection(transform.right);
            runtimeBuildData.Happiness = (80f + 10 * runtimeBuildData.CurLevel) / 100;
            Invoke("FillUpPopulation", 1f);
            InitBuildingFunction();
            //地基
            MapManager.Instance.BuildFoundation(vector2Ints, 15);
            TerrainGenerator.Instance.FlatGround
            (takenGrids, MapManager.GetTerrainPosition(parkingGridIn).y);
        }
        else
        {
            RestartBuildingFunction();
            MapManager.Instance.BuildFoundation(vector2Ints, 15, 0, false); 
            TerrainGenerator.Instance.FlatGround
             (takenGrids, MapManager.GetTerrainPosition(parkingGridIn).y, false);
        }

        
    }

    protected void PlayAnim()
    {
        body.SetActive(false);
        animation.gameObject.SetActive(true);
        float time = animation.clip.length;
        Invoke("ShowBody", time);
        animation["Take 001"].speed = 1f;
        animation.Play();
    }
    public Vector2Int GetInParkingGrid()
    {
        /*switch (direction)
        {
            case Direction.down:
                return MapManager.Instance.GetCenterGrid(parking.transform.position + Vector3.down);
            case Direction.right:
                return MapManager.Instance.GetCenterGrid(parking.transform.position + Vector3.left);
            case Direction.up:
                return MapManager.Instance.GetCenterGrid(parking.transform.position + Vector3.down);
            case Direction.left:
                return MapManager.Instance.GetCenterGrid(parking.transform.position + Vector3.left);

        }*/
        return MapManager.GetCenterGrid(transform.position+CastTool.CastDirectionToVector(direction)*(Size.y/2+0.5f));
    }
    /*
    public Vector2Int GetOutParkingGrid()
    {
        return MapManager.GetCenterGrid(transform.position + CastTool.CastDirectionToVector(direction) * (Size.y / 2 + 1.5f));
    }*/
    /// <summary>
    /// 建造完后初始化建筑功能
    /// </summary>
    public virtual void InitBuildingFunction()
    {
        //Debug.Log("add");
        parkingGridIn = GetInParkingGrid();
        MapManager.Instance._buildings.Add(this);
        MapManager.Instance.AddBuildingEntry(parkingGridIn, this);
        EventManager.StartListening(ConstEvent.OnOutputResources, Output);
        EventManager.StartListening(ConstEvent.OnInputResources, Input);
        EventManager.StartListening<string>(ConstEvent.OnDayWentBy, UpdateRate);
        ChangeFormula();
    }

    /// <summary>
    /// 从存档中恢复已有建筑
    /// </summary>
    public virtual void RestartBuildingFunction()
    {
        if (runtimeBuildData.formulaDatas.Length>0)
        {
            //Debug.Log(runtimeBuildData.CurFormula);
            //Debug.Log(runtimeBuildData.formulaDatas.Length);
            formula = runtimeBuildData.formulaDatas[runtimeBuildData.CurFormula];
        }
        parkingGridIn = GetInParkingGrid();
        MapManager.Instance._buildings.Add(this);
        RegisterBuildingEntry();
        EventManager.StartListening(ConstEvent.OnOutputResources, Output);
        EventManager.StartListening(ConstEvent.OnInputResources, Input);
        EventManager.StartListening<string>(ConstEvent.OnDayWentBy, UpdateRate);
        if (runtimeBuildData.tabType!=BuildTabType.house)
        {
            ResourceManager.Instance.TryAddCurPopulation(runtimeBuildData.CurPeople);
        }
    }

    public void RegisterBuildingEntry()
    {
        MapManager.Instance.AddBuildingEntry(parkingGridIn, this);
    }

    public void FillUpPopulation()
    {
        if (runtimeBuildData.Population > 0 && runtimeBuildData.tabType != BuildTabType.house)
        {
            if (runtimeBuildData.Population + TechManager.Instance.PopulationBuff() - runtimeBuildData.CurPeople > 0)
            {
                runtimeBuildData.CurPeople += ResourceManager.Instance.TryAddCurPopulation(runtimeBuildData.Population + TechManager.Instance.PopulationBuff() - runtimeBuildData.CurPeople);
                EventManager.TriggerEvent(ConstEvent.OnPopulaitionChange);
            }
            //CheckCurPeopleMoreThanMax();
            runtimeBuildData.Pause = true;
            UpdateEffectiveness();
        }

    }
    public void ChangeFormula()
    {
        if (runtimeBuildData.formulaDatas.Length > 0)
        {
            formula = runtimeBuildData.formulaDatas[runtimeBuildData.CurFormula];
            //Debug.Log("change "+ formula.Describe);
            runtimeBuildData.productTime = formula.ProductTime;
            runtimeBuildData.Rate = 0;
        }
    }
    public void ShowBody()
    {
        body.SetActive(true);
        animation.gameObject.SetActive(false);
    }

    //public virtual string GetIntroduce()
    //{
    //    return string.Empty;
    //}

    public static RuntimeBuildData CastBuildDataToRuntime(BuildData buildData)
    {
        RuntimeBuildData runtimeBuildData = new RuntimeBuildData();
        runtimeBuildData.Id = buildData.Id;
        runtimeBuildData.Name = buildData.Name;
        runtimeBuildData.Length = buildData.Length;
        runtimeBuildData.Width = buildData.Width;
        runtimeBuildData.Price = buildData.Price;
        runtimeBuildData.costResources = buildData.costResources;
        runtimeBuildData.MaxStorage = buildData.MaxStorage;
        runtimeBuildData.InfluenceRange = buildData.InfluenceRange;
        runtimeBuildData.FrontBuildingId = buildData.FrontBuildingId;
        runtimeBuildData.RearBuildingId = buildData.RearBuildingId;
        runtimeBuildData.Introduce = buildData.Introduce;
        runtimeBuildData.BundleName = buildData.BundleName;
        runtimeBuildData.PfbName = buildData.PfbName;
        runtimeBuildData.tabType = buildData.tabType;
        runtimeBuildData.Population = buildData.Population;
        runtimeBuildData.CostPerWeek = buildData.CostPerWeek;
        runtimeBuildData.formulaDatas = new FormulaData[buildData.Formulas.Count];
        for (int i = 0; i < buildData.Formulas.Count; i++)
        {
            runtimeBuildData.formulaDatas[i] = DataManager.GetFormulaById(buildData.Formulas[i]);
        }
        runtimeBuildData.CurFormula = 0;
        runtimeBuildData.Times = buildData.Times;
        return runtimeBuildData;
    }


    protected virtual void OnDestroy()
    {
        EventManager.StopListening(ConstEvent.OnOutputResources, Output);
        EventManager.StopListening(ConstEvent.OnInputResources, Input);
        EventManager.StopListening<string>(ConstEvent.OnDayWentBy, UpdateRate);
    }

    public virtual void DestroyBuilding(bool returnResources ,bool returnPopulation,bool repaint = true)
    {
        if (returnResources)
        {
            ReturnBuildResources();
        }
        MapManager.Instance._buildings.Remove(this);
        MapManager.Instance.RemoveBuildingEntry(parkingGridIn);
        
        if (repaint)
        {
            MapManager.SetGridTypeToEmpty(takenGrids);
            MapManager.Instance.BuildOriginFoundation(takenGrids);
        }
        if (returnPopulation)
        {
            if (runtimeBuildData.Population < 0)
            {
                ResourceManager.Instance.AddMaxPopulation(runtimeBuildData.Population);
            }
            else
            {
                ResourceManager.Instance.TryAddCurPopulation(-runtimeBuildData.CurPeople);
            }
            EventManager.TriggerEvent(ConstEvent.OnPopulaitionChange);
        }
        
        Destroy(this.gameObject);
    }
    public virtual bool ReturnBuildResources()
    {
        List<CostResource> rescources = runtimeBuildData.costResources;
        for (int i = 0; i < rescources.Count; i++)
        {
            rescources[i].ItemNum *= 0.75f;
            ResourceManager.Instance.AddResource(rescources[i]);
        }
        return true;

    }
    protected virtual void Output()
    {
        if (formula == null|| formula.OutputItemID ==null) return;
        runtimeBuildData.productTime--;
        if (runtimeBuildData.productTime <= 0)
        {
            runtimeBuildData.productTime = formula.ProductTime;
            float rate = runtimeBuildData.Rate;
            CarMission carMission = MakeCarMission(rate);
            TrafficManager.Instance.UseCar(carMission,out runtimeBuildData.AvaliableToMarket);
            runtimeBuildData.Rate = 0;
        }
    }

    public float GetProcess()
    {
        return 1 - (float)runtimeBuildData.productTime / formula.ProductTime + (float)LevelManager.Instance.Day / 7 / formula.ProductTime;
    }

    public float WorkEffect()
    {
        return runtimeBuildData.CurPeople/(runtimeBuildData.Population + TechManager.Instance.PopulationBuff());
    }
    protected virtual void Input()
    {
        ResourceManager.Instance.TryUseUpResource(new CostResource(99999, runtimeBuildData.CostPerWeek));
        runtimeBuildData.Pause = false;
        //ChangeFormula();
        if (formula == null|| formula.InputItemID==null) return;
        List<CostResource> costResources = new List<CostResource>();
        for (int i = 0; i < formula.InputItemID.Count; i++)
        {
            costResources.Add(new CostResource(formula.InputItemID[i], formula.InputNum[i]* WorkEffect()));
        }

        bool res = ResourceManager.Instance.IsResourcesEnough(costResources, TechManager.Instance.ResourcesBuff());
        if (!res)
        {
            runtimeBuildData.Pause = true;
            return;
        }
        else
        {
            ResourceManager.Instance.TryUseResources(costResources);
        }
    }

    public virtual void OnRecieveCar(CarMission carMission)
    {

    }

    public virtual void AddCurPeople(int num)
    {
        //Debug.Log("Add");
        //Debug.Log(num);
        int cur = runtimeBuildData.CurPeople;
        //Debug.Log(cur);
        int max = runtimeBuildData.Population + TechManager.Instance.PopulationBuff();
        //Debug.Log(max);
        if (cur + num <= max)
        {
            runtimeBuildData.CurPeople += ResourceManager.Instance.TryAddCurPopulation(num);
        }
        else
        {
            runtimeBuildData.CurPeople += ResourceManager.Instance.TryAddCurPopulation(max-cur);
        }
        UpdateEffectiveness();
        EventManager.TriggerEvent(ConstEvent.OnPopulaitionChange);
    }
    public virtual void DeleteCurPeople(int num)
    {
        //Debug.Log("Delete"+num);
        int cur = runtimeBuildData.CurPeople;
        int max = runtimeBuildData.Population + TechManager.Instance.PopulationBuff();
        if (cur - num >= 0)
        {
            runtimeBuildData.CurPeople += ResourceManager.Instance.TryAddCurPopulation(-num);
        }
        else
        {
            runtimeBuildData.CurPeople += ResourceManager.Instance.TryAddCurPopulation(-cur);
        }
        UpdateEffectiveness();
        EventManager.TriggerEvent(ConstEvent.OnPopulaitionChange);
    }

    public virtual void Upgrade(out bool issuccess)
    {
        //todo：检查是否有足够的资源升级
        int nextId = runtimeBuildData.RearBuildingId;
        BuildData data = DataManager.GetBuildData(nextId);
        RuntimeBuildData buildData = BuildingBase.CastBuildDataToRuntime(data);
        buildData.Pause = runtimeBuildData.Pause;
        buildData.CurLevel = runtimeBuildData.CurLevel+1;
        buildData.CurFormula = runtimeBuildData.CurFormula;
        buildData.CurPeople = runtimeBuildData.CurPeople;
        //DeleteCurPeople(runtimeBuildData.CurPeople);
        buildData.Happiness = (80f+ 10 * buildData.CurLevel) /100;
        BuildingBase rear = BuildManager.Instance.UpgradeBuilding(buildData, takenGrids,transform.position,transform.rotation);
        if (rear!=null)
        {
            SoundManager.Instance.PlaySoundEffect(SoundResource.sfx_upgrade);
            DestroyBuilding(false,false,false);
            rear.RegisterBuildingEntry();
            issuccess = true;
        }
        else
        {
            issuccess = false;
            //AddCurPeople(people);
            NoticeManager.Instance.InvokeShowNotice("升级资源不足");
        }
    }

    public virtual void UpdateRate(string date)
    {
        //CheckCurPeopleMoreThanMax();
        UpdateEffectiveness();
        runtimeBuildData.Rate += runtimeBuildData.Effectiveness / 7f / formula.ProductTime;
        //Debug.Log(runtimeBuildData.Rate);
    }

    public void UpdateEffectiveness()
    {
        //Debug.Log(runtimeBuildData.Pause);
        int cur = runtimeBuildData.CurPeople;
        int max = runtimeBuildData.Population + TechManager.Instance.PopulationBuff();
        runtimeBuildData.Effectiveness = runtimeBuildData.Pause ? 0 : ((float)cur) / (float)max * TechManager.Instance.EffectivenessBuff();
    }

    protected void CheckCurPeopleMoreThanMax()
    {
        int cur = runtimeBuildData.CurPeople;
        int max = runtimeBuildData.Population + TechManager.Instance.PopulationBuff();
        if (cur > max)
        {
            DeleteCurPeople(cur - max);
        }
    }
    public List<WarningType> GetWarnings()
    {
        List<WarningType> res = new List<WarningType>();
        if (runtimeBuildData.CurPeople == 0)
        {
            res.Add(WarningType.noPeople);
        }
        if (runtimeBuildData.Pause)
        {
            res.Add(WarningType.noResources);
        }
        if (!runtimeBuildData.AvaliableToMarket)
        {
            res.Add(WarningType.noRoad);
        }
        return res;
    }
    public virtual float GetHappiness()
    {
        return runtimeBuildData.Happiness;
    }
    protected virtual CarMission MakeCarMission(float rate)
    {
        //Debug.Log(rate);
        CarMission mission = new CarMission();
        mission.StartBuilding = parkingGridIn;
        mission.EndBuilding = MapManager.GetNearestMarket(parkingGridIn).GetComponent<BuildingBase>().parkingGridIn;
        mission.missionType = CarMissionType.transportResources;
        mission.isAnd = true;
        mission.transportResources = new List<CostResource>();
        mission.transportationType = TransportationType.mini;
        for (int i = 0; i < formula.OutputItemID.Count; i++)
        {
            //Debug.Log(formula.OutputItemID[i]);
            mission.transportResources.Add(new CostResource(formula.OutputItemID[i], formula.ProductNum[i]*rate*runtimeBuildData.Times));
        }
        return mission;
    }
}

[System.Serializable] 
public class RuntimeBuildData : BuildData
{
    public bool Pause = false;//是否暂停生产(因为缺少原料)
    public int CurLevel = 0;//当前等级
    public int CurFormula;//当前配方
    public FormulaData[] formulaDatas;//当前建筑可用的配方们
    public int CurPeople = 0;//当前建筑的工人或居民
    public float Rate = 0;//当前生产进度0-1
    public float Effectiveness;//生产效率
    public float Happiness = 0.6f;//当前幸福程度
    public int productTime;//生产周期（周）
    public bool AvaliableToMarket = true;//可以到达市场

    public Vector3Serializer SavePosition;
    public Vector2IntSerializer[] SaveTakenGrids;
    public Direction SaveDir;
    public int SaveOutLookType;
}