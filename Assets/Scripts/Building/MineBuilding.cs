using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineBuilding : BuildingBase
{
    public Transform digPos;
    public float richness = 1;//资源丰度


    public override void OnConfirmBuild(Vector2Int[] vector2Ints)
    {
        takenGrids = vector2Ints;
        gameObject.tag = "Building";
        transform.GetComponent<BoxCollider>().enabled = false;
        transform.GetComponent<BoxCollider>().enabled = true;
        //地基
        //MapManager.Instance.BuildFoundation(vector2Ints, 15);
        //整平地面
        
        richness = SetRichness(takenGrids);
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
            TerrainGenerator.Instance.FlatGround
            (takenGrids, MapManager.GetTerrainPosition(parkingGridIn).y);
        }
        else
        {
            RestartBuildingFunction();
            TerrainGenerator.Instance.FlatGround
             (takenGrids, MapManager.GetTerrainPosition(parkingGridIn).y, false);
        }
    }

    protected override void Output()
    {
        if (formula == null || formula.OutputItemID == null)
        {
            Debug.LogError("矿井配方为空");
        }
        runtimeBuildData.productTime--;
        if (runtimeBuildData.productTime <= 0)
        {
            runtimeBuildData.productTime = formula.ProductTime;
            float rate = runtimeBuildData.Rate;
            CarMission carMission = MakeCarMission(rate);
            TrafficManager.Instance.UseCar(carMission, out runtimeBuildData.AvaliableToMarket);
            runtimeBuildData.Rate = 0;
        }
    }

    public float SetRichness(Vector2Int[] takenGrids)
    {
        float sum = 0;
        float total = takenGrids.Length;
        for (int i = 0; i < takenGrids.Length; i++)
        {
            sum += MapManager.GetMineRichness(takenGrids[i]);
        }
        return Mathf.Clamp01(sum / total*3);
    }
    protected override void Input()
    {
        base.Input();
        DigGround();
    }

    protected override CarMission MakeCarMission(float rate)
    {
        //Debug.Log(rate);
        CarMission mission = new CarMission();
        mission.StartBuilding = parkingGridIn;
        mission.EndBuilding = MapManager.GetNearestMarket(parkingGridIn).GetComponent<BuildingBase>().parkingGridIn;
        mission.missionType = CarMissionType.transportResources;
        mission.isAnd = true;
        mission.transportResources = new List<CostResource>();
        mission.transportationType = TransportationType.sandTruck;
        for (int i = 0; i < formula.OutputItemID.Count; i++)
        {
            //Debug.Log(formula.OutputItemID[i]);
            mission.transportResources.Add(new CostResource(formula.OutputItemID[i], rate * formula.ProductNum[i] * richness * runtimeBuildData.Times));

        }
        return mission;
    }

    private void DigGround()
    {
        float height = MapManager.GetTerrainPosition(digPos.position).y;
        Vector2Int[] grids = BuildManager.Instance.GetAllGrids(5, 5, digPos.position, false);
        TerrainGenerator.Instance.FlatGround(grids,height-0.1f);
    }

    public override void DestroyBuilding(bool returnResources, bool returnPopulation, bool repaint = true)
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
            //MapManager.Instance.BuildOriginFoundation(takenGrids);
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
}
