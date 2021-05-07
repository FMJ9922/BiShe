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
        parkingGridIn = InitParkingGrid();
        MapManager.Instance.BuildFoundation(vector2Ints, 15);
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
    protected Vector2Int InitParkingGrid()
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
        MapManager.Instance._buildings.Add(this.gameObject);
        EventManager.StartListening(ConstEvent.OnOutputResources, Output);
        EventManager.StartListening(ConstEvent.OnInputResources, Input);
        if (runtimeBuildData.formulaDatas.Length>0)
        {
            //Debug.Log(runtimeBuildData.CurFormula);
            formula = runtimeBuildData.formulaDatas[runtimeBuildData.CurFormula];
        }
        productTime = formula.ProductTime;
        if (runtimeBuildData.Population > 0)
        {
            runtimeBuildData.CurPeople = ResourceManager.Instance.TryAddCurPopulation(runtimeBuildData.Population);
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

    public virtual void DestroyBuilding()
    {
        ReturnBuildResources();
        MapManager.SetGridTypeToEmpty(takenGrids);
        MapManager.Instance._buildings.Remove(gameObject);
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
            for (int i = 0; i < formula.OutputItemID.Count; i++)
            {
                ResourceManager.Instance.AddResource(formula.OutputItemID[i],formula.ProductNum[i]);
            }
            runtimeBuildData.Rate = 0;
        }
    }

    protected virtual void Input()
    {
        if (formula == null|| formula.InputItemID==null) return;
        for (int i = 0; i < formula.InputItemID.Count; i++)
        {
            ResourceManager.Instance.TryUseResource(formula.InputItemID[i],formula.InputNum[i]);
        }
    }

    public virtual void OnRecieveCar(CarMission carMission)
    {

    }

    public virtual void AddCurPeople(int num)
    {
        //Debug.Log("Add");
        int cur = runtimeBuildData.CurPeople;
        int max = runtimeBuildData.Population;
        if (cur + num <= max)
        {
            runtimeBuildData.CurPeople += ResourceManager.Instance.TryAddCurPopulation(num);
        }
        else
        {
            runtimeBuildData.CurPeople += ResourceManager.Instance.TryAddCurPopulation(num+cur-max);
        }
        EventManager.TriggerEvent(ConstEvent.OnPopulaitionChange);
    }
    public virtual void DeleteCurPeople(int num)
    {
        //Debug.Log("Delete");
        int cur = runtimeBuildData.CurPeople;
        int max = runtimeBuildData.Population;
        if (cur - num >= 0)
        {
            runtimeBuildData.CurPeople += ResourceManager.Instance.TryAddCurPopulation(-num);
        }
        else
        {
            runtimeBuildData.CurPeople += ResourceManager.Instance.TryAddCurPopulation(-cur);
        }
        EventManager.TriggerEvent(ConstEvent.OnPopulaitionChange);
    }

    public virtual void Upgrade()
    {
        //todo：检查是否有足够的资源升级
        int nextId = runtimeBuildData.RearBuildingId;
        BuildData data = DataManager.GetBuildData(nextId);
        RuntimeBuildData buildData = BuildingBase.CastBuildDataToRuntime(data);
        buildData.Pause = runtimeBuildData.Pause;
        buildData.CurLevel = runtimeBuildData.CurLevel;
        buildData.CurFormula = runtimeBuildData.CurFormula;
        buildData.CurPeople = runtimeBuildData.CurPeople;
        BuildManager.Instance.UpgradeBuilding(data, transform.position,transform.rotation,out bool success);
        if (success)
        {
            DestroyBuilding();
        }
    }

    public virtual void UpdateRate(string date)
    {
        runtimeBuildData.Rate += (float)runtimeBuildData.CurPeople / (float)runtimeBuildData.Population / 7f / formula.ProductTime;
        //Debug.Log(runtimeBuildData.Rate);
    }
}

public class RuntimeBuildData : BuildData
{
    public bool Pause = true;//是否暂停生产
    public int CurLevel = 0;//当前等级
    public int CurFormula;//当前配方
    public FormulaData[] formulaDatas;//当前建筑可用的配方们
    public int CurPeople = 0;//当前建筑的工人或居民
    public float Rate = 0;//当前生产进度0-1
    
}