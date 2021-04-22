using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketBuilding : BuildingBase
{
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
    public override void InitBuildingFunction()
    {
        base.InitBuildingFunction();
    }

    public override void OnRecieveCar(CarMission carMission)
    {
        switch (carMission.missionType)
        {
            case CarMissionType.requestResources:
                break;
            default:
                break;
        }
    }
}
