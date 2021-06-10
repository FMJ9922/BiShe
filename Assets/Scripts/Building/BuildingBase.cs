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

    [SerializeField]
    private BundlePrimaryType BundlePrimaryType;

    
    public string BundleName => string.Format("{0}.ab", BundlePrimaryType.ToString());

    public string PfbName => this.gameObject.name;

    public Vector2Int Size => new Vector2Int(Height, Width);

    public bool hasAnima = false;

    public Animation animation;

    public GameObject body;

    public RuntimeBuildData runtimeBuildData;

    public bool buildFlag = false;

    public Vector2Int[] takenGrids;

    protected FormulaData formula = new FormulaData();


    public Vector2Int parkingGridIn;
    public Vector2Int parkingGridOut;


    public Direction direction;
     
    public virtual void OnConfirmBuild(Vector2Int[] vector2Ints)
    {
        buildFlag = true;
        gameObject.tag = "Building";
        takenGrids = vector2Ints;
        if (hasAnima)
        {
            Invoke("PlayAnim",0.2f);
        }
        //Debug.Log(direction);
        //Debug.Log(transform.name);
        //Debug.Log(transform.right);
        direction = CastTool.CastVector3ToDirection(transform.right);
        //Debug.Log(direction);
        parkingGridIn = GetParkingGrid();
        //刷地基
        MapManager.Instance.BuildFoundation(vector2Ints, 15);
        //Debug.Log("paint");
        //整平地面
        Vector3 targetPos = MapManager.GetTerrainPosition(parkingGridIn);
        float targetHeight = targetPos.y;
        TerrainGenerator.Instance.FlatGround(takenGrids, targetHeight);
        runtimeBuildData.Happiness = (80f + 10 * runtimeBuildData.CurLevel) / 100;
        InitBuildingFunction();
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
    public Vector2Int GetParkingGrid()
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
    /// <summary>
    /// 建造完后初始化建筑功能
    /// </summary>
    public virtual void InitBuildingFunction()
    {
        //Debug.Log("add");
        MapManager.Instance._buildings.Add(this);
        EventManager.StartListening(ConstEvent.OnOutputResources, Output);
        EventManager.StartListening(ConstEvent.OnInputResources, Input);
        EventManager.StartListening<string>(ConstEvent.OnDayWentBy, UpdateRate);
        ChangeFormula();
        productTime = formula.ProductTime;
        if (runtimeBuildData.Population > 0)
        {
            if(runtimeBuildData.Population + TechManager.Instance.PopulationBuff() - runtimeBuildData.CurPeople > 0)
            {
                runtimeBuildData.CurPeople = ResourceManager.Instance.TryAddCurPopulation(runtimeBuildData.Population + TechManager.Instance.PopulationBuff() - runtimeBuildData.CurPeople);
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
            //Debug.Log(runtimeBuildData.CurFormula);
            formula = runtimeBuildData.formulaDatas[runtimeBuildData.CurFormula];
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
        return runtimeBuildData;
    }

    protected int productTime;//生产周期（周）

    protected virtual void OnDestroy()
    {
        EventManager.StopListening(ConstEvent.OnOutputResources, Output);
        EventManager.StopListening(ConstEvent.OnInputResources, Input);
        EventManager.StopListening<string>(ConstEvent.OnDayWentBy, UpdateRate);
    }

    public virtual void DestroyBuilding(bool repaint = true)
    {
        ReturnBuildResources();
        MapManager.Instance._buildings.Remove(this);
        if (repaint)
        {
            MapManager.SetGridTypeToEmpty(takenGrids);
            MapManager.Instance.BuildFoundation(takenGrids, 0, 4);
        }
        if (runtimeBuildData.Population < 0)
        {
            ResourceManager.Instance.AddMaxPopulation(runtimeBuildData.Population);
        }
        else
        {
            ResourceManager.Instance.TryAddCurPopulation(-runtimeBuildData.Population);
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
        productTime--;
        if (productTime <= 0)
        {
            productTime = formula.ProductTime;
            float rate = runtimeBuildData.Rate;
            CarMission carMission = MakeCarMission(rate);
            TrafficManager.Instance.UseCar(carMission, () => carMission.EndBuilding.OnRecieveCar(carMission));
            runtimeBuildData.Rate = 0;
        }
    }

    public float GetProcess()
    {
        return 1 - (float)productTime / formula.ProductTime + (float)LevelManager.Instance.Day / 7 / formula.ProductTime;
    }
    protected virtual void Input()
    {
        ResourceManager.Instance.TryUseUpResource(new CostResource(99999, runtimeBuildData.CostPerWeek));
        runtimeBuildData.Pause = false;
        ChangeFormula();
        if (formula == null|| formula.InputItemID==null) return;
        for (int i = 0; i < formula.InputItemID.Count; i++)
        {
            bool res = ResourceManager.Instance.TryUseResource(formula.InputItemID[i],formula.InputNum[i]*TechManager.Instance.ResourcesBuff());
            //Debug.Log("res" + res+" id"+ formula.InputItemID[i]);
            if (!res)
            {
                runtimeBuildData.Pause = true;
                return;
            }
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
        //Debug.Log("Delete");
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

    public virtual void Upgrade()
    {
        //todo：检查是否有足够的资源升级
        int nextId = runtimeBuildData.RearBuildingId;
        BuildData data = DataManager.GetBuildData(nextId);
        RuntimeBuildData buildData = BuildingBase.CastBuildDataToRuntime(data);
        buildData.Pause = runtimeBuildData.Pause;
        buildData.CurLevel = runtimeBuildData.CurLevel+1;
        buildData.CurFormula = runtimeBuildData.CurFormula;
        buildData.CurPeople = runtimeBuildData.CurPeople;
        buildData.Happiness = (80f+ 10 * buildData.CurLevel) /100;
        BuildManager.Instance.UpgradeBuilding(buildData, takenGrids,transform.position,transform.rotation,out bool success);
        if (success)
        {
            DestroyBuilding(false);
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

    private CarMission MakeCarMission(float rate)
    {
        //Debug.Log(rate);
        CarMission mission = new CarMission();
        mission.StartBuilding = this;
        mission.EndBuilding = MapManager.GetNearestMarket(parkingGridIn).GetComponent<BuildingBase>();
        mission.missionType = CarMissionType.transportResources;
        mission.isAnd = true;
        mission.transportResources = new List<CostResource>();
        mission.transportationType = TransportationType.mini;
        for (int i = 0; i < formula.OutputItemID.Count; i++)
        {
            //Debug.Log(formula.OutputItemID[i]);
            mission.transportResources.Add(new CostResource(formula.OutputItemID[i], formula.ProductNum[i]*rate));
        }
        return mission;
    }
}

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
    
}