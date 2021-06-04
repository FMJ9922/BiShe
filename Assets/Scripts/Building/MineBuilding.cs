using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineBuilding : BuildingBase
{
    public Transform digPos;

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
        MapManager.Instance.BuildFoundation(vector2Ints, 7, (int)direction);
        Vector3 targetPos = MapManager.GetTerrainPosition(parkingGridIn);
        float targetHeight = targetPos.y;
        TerrainGenerator.Instance.FlatGround(takenGrids, targetHeight);
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
        DigGround();
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
        mission.transportationType = TransportationType.sandTruck;
        for (int i = 0; i < formula.OutputItemID.Count; i++)
        {
            //Debug.Log(formula.OutputItemID[i]);
            mission.transportResources.Add(new CostResource(formula.OutputItemID[i], rate * formula.ProductNum[i]));

        }
        return mission;
    }

    private void DigGround()
    {
        float height = MapManager.GetTerrainPosition(digPos.position).y;
        Vector2Int[] grids = BuildManager.Instance.GetAllGrids(5, 5, digPos.position, false);
        TerrainGenerator.Instance.FlatGround(grids,height-0.1f);
    }
}
