using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PierBuilding : BuildingBase
{
    public Transform boatPos;

    public override void OnConfirmBuild(Vector2Int[] vector2Ints)
    {
        takenGrids = vector2Ints;
        gameObject.tag = "Building";
        parkingGridIn = GetInParkingGrid();

        transform.GetComponent<BoxCollider>().enabled = false;
        transform.GetComponent<BoxCollider>().enabled = true;

        //地基
        //MapManager.Instance.BuildFoundation(vector2Ints, 2, ((int)direction + 1) % 4);
        //整平地面
        //Vector3 targetPos = MapManager.GetTerrainPosition(parkingGridIn);
        //float targetHeight = targetPos.y;
        //TerrainGenerator.Instance.FlatGround(takenGrids, targetHeight);
        //parkingGridOut = GetOutParkingGrid();
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
        }
        else
        {
            RestartBuildingFunction();
        }
    }

    protected override void Output()
    {
        if (formula == null || formula.OutputItemID == null) return;
        runtimeBuildData.productTime--;
        if (runtimeBuildData.productTime <= 0)
        {
            runtimeBuildData.productTime = formula.ProductTime;
            float rate = runtimeBuildData.Rate;
            CarMission carMission = MakeCarMission(rate);
            TrafficManager.Instance.UseCar(carMission);
            runtimeBuildData.Rate = 0;
        }
    }

    protected override void Input()
    {
        base.Input();
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
        mission.transportationType = TransportationType.mini;
        for (int i = 0; i < formula.OutputItemID.Count; i++)
        {
            //Debug.Log(formula.OutputItemID[i]);
            mission.transportResources.Add(new CostResource(formula.OutputItemID[i], formula.ProductNum[i] * runtimeBuildData.Times));

        }
        return mission;
    }
}
