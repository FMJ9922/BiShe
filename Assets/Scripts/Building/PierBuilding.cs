using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PierBuilding : BuildingBase
{
    public Transform boatPos;
    public override void InitBuildingFunction()
    {
        base.InitBuildingFunction();
    }

    public override void OnConfirmBuild(Vector2Int[] vector2Ints)
    {
        gameObject.tag = "Building";
        takenGrids = vector2Ints;
        parkingGridIn = GetInParkingGrid();
        //parkingGridOut = GetOutParkingGrid();
        if (!buildFlag)
        {
            buildFlag = true;
            if (hasAnima)
            {
                Invoke("PlayAnim", 0.2f);
            }
            transform.GetComponent<BoxCollider>().enabled = false;
            direction = CastTool.CastVector3ToDirection(transform.right);
            runtimeBuildData.Happiness = (80f + 10 * runtimeBuildData.CurLevel) / 100;
            Invoke("FillUpPopulation", 1f);
        }
        transform.GetComponent<BoxCollider>().enabled = true;
        InitBuildingFunction();
    }

    protected override void Output()
    {
        if (formula == null || formula.OutputItemID == null) return;
        productTime--;
        if (productTime <= 0)
        {
            productTime = formula.ProductTime;
            float rate = runtimeBuildData.Rate;
            CarMission carMission = MakeCarMission(rate);
            TrafficManager.Instance.UseCar(carMission, null);
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
