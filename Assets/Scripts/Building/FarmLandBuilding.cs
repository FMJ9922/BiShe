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
        InitWheatGrids();
        previewObj.SetActive(false);
        EventManager.StartListening<string>(ConstEvent.OnDayWentBy, UpdateRate);
        base.InitBuildingFunction();
    }
     private void InitWheatGrids()
    {
        StartCoroutine("Plant");
        //wheatTrans.localPosition = Vector3.zero;
    }

    public override void OnConfirmBuild(Vector2Int[] vector2Ints)
    {
        buildFlag = true;
        gameObject.tag = "Building";
        takenGrids = vector2Ints;
        if (hasAnima)
        {
            Invoke("PlayAnim", 0.2f);
        }
        parkingGridIn = InitParkingGrid();
        MapManager.Instance.BuildFoundation(vector2Ints, 2, (int)direction);
        InitBuildingFunction();
    }
    private IEnumerator Plant()
    {
        grids = new GameObject[Size.x * Size.y];
        GameObject pfb = runtimeBuildData.formulaDatas[runtimeBuildData.CurFormula].ID == 50006?
            LoadAB.Load(runtimeBuildData.BundleName, "WheatPfb"):
             LoadAB.Load(runtimeBuildData.BundleName, "RicePfb");
        for (int i = 0; i < Size.x * Size.y; i++)
        {
            GameObject newGrid = Instantiate(pfb, wheatTrans);
            grids[i] = newGrid;
            Vector3 random = Random.insideUnitSphere/10;
            newGrid.transform.position = MapManager.GetTerrainPosition(takenGrids[i])+new Vector3(random.x,0,random.z);
            newGrid.transform.Rotate(Vector3.up, 90 * (int)(direction - 1), Space.Self);
            yield return new WaitForSeconds(0.1f);
            //Animator animator = newGrid.GetComponentInChildren<Animator>();
            //animator.Play("WheatGrow");
        }
    }
    protected override void Output()
    {
        if (formula == null) return;
        if (!isharvesting)
        {
            productTime--;
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
    public void OnFinishHarvest(float rate)
    {
        isharvesting = false;
        productTime = formula.ProductTime;
        InitWheatGrids();
        CarMission carMission = MakeCarMission(rate);
        TrafficManager.Instance.UseCar(TransportationType.mini, carMission,
            () => carMission.EndBuilding.OnRecieveCar(carMission));
    }
    /// <summary>
    /// 配置货物清单
    /// </summary>
    /// <param name="rate">生长比例</param>
    /// <returns></returns>
    private CarMission MakeCarMission(float rate)
    {
        //Debug.Log(rate);
        CarMission mission = new CarMission();
        mission.StartBuilding = this;
        mission.transportationType = TransportationType.van;
        mission.EndBuilding = MapManager.GetNearestMarket(parkingGridIn).GetComponent<BuildingBase>();
        mission.missionType = CarMissionType.transportResources;
        mission.isAnd = true;
        mission.transportResources = new List<CostResource>();
        for (int i = 0; i < formula.OutputItemID.Count; i++)
        {
            //Debug.Log(formula.OutputItemID[i]);
            mission.transportResources.Add(new CostResource(formula.OutputItemID[i], rate * formula.ProductNum[i]));

        }
        return mission;
    }
    protected override void Input()
    {
        base.Input();
    }

}
