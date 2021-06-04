using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PierBuilding : BuildingBase
{
    public Transform boatPos;
    public override void InitBuildingFunction()
    {
        EventManager.StartListening<string>(ConstEvent.OnDayWentBy, UpdateRate);
        base.InitBuildingFunction();
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
        //MapManager.Instance.BuildFoundation(vector2Ints, 7, (int)direction);
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
            TrafficManager.Instance.UseCar(carMission, () => carMission.EndBuilding.OnRecieveCar(carMission));
            runtimeBuildData.Rate = 0;
        }
    }

    protected override void Input()
    {
        base.Input();
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
            mission.transportResources.Add(new CostResource(formula.OutputItemID[i], formula.ProductNum[i]));

        }
        return mission;
    }
}
