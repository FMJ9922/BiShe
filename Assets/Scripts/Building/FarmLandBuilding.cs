using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmLandBuilding : BuildingBase
{
    public Color yellow;
    public Color green;
    [SerializeField] Transform wheatTrans;
    [SerializeField] GameObject previewObj;
    public List<GameObject> lists;
    bool isharvesting = false;

    private GameObject[] grids;
    public override void InitBuildingFunction()
    {
        Invoke("InitWheatGrids",1f);
        previewObj.SetActive(false);
        base.InitBuildingFunction();
    }
     private void InitWheatGrids()
    {
        StartCoroutine(Plant());
        //wheatTrans.localPosition = Vector3.zero;
    }

    public override void OnConfirmBuild(Vector2Int[] vector2Ints)
    {
        takenGrids = vector2Ints;
        gameObject.tag = "Building";
        parkingGridIn = GetInParkingGrid();
        parkingGridOut = GetOutParkingGrid();
        if (!buildFlag)
        {
            buildFlag = true;
            if (hasAnima)
            {
                Invoke("PlayAnim", 0.2f);
            }
            transform.GetComponent<BoxCollider>().enabled = false;
            direction = CastTool.CastVector3ToDirection(transform.right);
            //地基
            MapManager.Instance.BuildFoundation(vector2Ints, 2, (int)direction+1);
            //整平地面
            Vector3 targetPos = MapManager.GetTerrainPosition(parkingGridIn);
            float targetHeight = targetPos.y;
            TerrainGenerator.Instance.FlatGround(takenGrids, targetHeight);
            runtimeBuildData.Happiness = (80f + 10 * runtimeBuildData.CurLevel) / 100;
            Invoke("FillUpPopulation", 1f);
        }
        transform.GetComponent<BoxCollider>().enabled = true;
        InitBuildingFunction();
    }
    private IEnumerator Plant()
    {
        grids = new GameObject[Size.x * Size.y];
        GameObject pfb = runtimeBuildData.formulaDatas[runtimeBuildData.CurFormula].ID == 50006?
            LoadAB.Load(runtimeBuildData.BundleName, "WheatPfb"):
             LoadAB.Load(runtimeBuildData.BundleName, "RicePfb");
        for (int i = 0; i < Size.x * Size.y*2; i++)
        {
            GameObject newGrid = Instantiate(pfb, wheatTrans);
            grids[i / 2] = newGrid;
            Vector3 random = Random.insideUnitSphere/5;
            Vector3 pos;
            if (i % 2 == 0)
            {
                pos = MapManager.GetNotInWaterPosition(takenGrids[i/2]);
            }
            else
            {
                pos = MapManager.GetNotInWaterPosition(takenGrids[i/2]) + transform.forward;
            }
            newGrid.transform.position = pos+new Vector3(random.x,0,random.z) - transform.forward/2;
            newGrid.transform.Rotate(Vector3.up, 90 * (int)(direction - 1), Space.Self);
            yield return 0;
            //Animator animator = newGrid.GetComponentInChildren<Animator>();
            //animator.Play("WheatGrow");
        }
    }
    protected override void Output()
    {
        if (formula == null)
        {
            Debug.Log("no");
            return;
        }

        if (!isharvesting)
        {
            productTime--;
            Debug.Log(productTime);
            float progress = (float)productTime / formula.ProductTime;
            if (productTime <= 0)
            {
                isharvesting = true;
                List<Vector3> vecs = new List<Vector3>();
                for (int i = 0; i < lists.Count; i++)
                {
                    vecs.Add(lists[i].transform.position);
                }
                float rate = runtimeBuildData.Rate;
                TrafficManager.Instance.UseCar(TransportationType.harvester, vecs, DriveType.once, ()=>OnFinishHarvest(rate));
                runtimeBuildData.Rate = 0;
            }
        }
    }
    public override void UpdateRate(string date)
    {
        //CheckCurPeopleMoreThanMax();
        UpdateEffectiveness();
        if (!isharvesting)
        {
            runtimeBuildData.Rate += runtimeBuildData.Effectiveness / 7f / formula.ProductTime;
        }
        //Debug.Log(runtimeBuildData.Rate);
    }
    public void OnFinishHarvest(float rate)
    {
        isharvesting = false;
        productTime = formula.ProductTime;
        InitWheatGrids();
        CarMission carMission = MakeCarMission(rate);
        TrafficManager.Instance.UseCar(carMission,
            () => carMission.EndBuilding.OnRecieveCar(carMission));
    }
    /// <summary>
    /// 配置货物清单
    /// </summary>
    /// <param name="rate">生长比例</param>
    /// <returns></returns>
    protected override CarMission MakeCarMission(float rate)
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
            mission.transportResources.Add(new CostResource(formula.OutputItemID[i], rate * formula.ProductNum[i] * runtimeBuildData.Times));

        }
        return mission;
    }
    protected override void Input()
    {
        base.Input();
    }

}
