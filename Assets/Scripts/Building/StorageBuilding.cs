using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageBuilding : BuildingBase
{
    public float Max;

    public override void InitBuildingFunction()
    {
        ResourceManager.Instance.AddMaxStorage(Max);
        base.InitBuildingFunction();
    }

    public override void RestartBuildingFunction()
    {
        ResourceManager.Instance.AddMaxStorage(Max);
        base.RestartBuildingFunction();
    }

    protected override void OnDestroy()
    {
        EventManager.StopListening(ConstEvent.OnOutputResources, Output);
        EventManager.StopListening(ConstEvent.OnInputResources, Input);
        EventManager.StopListening<string>(ConstEvent.OnDayWentBy, UpdateRate);
        if (ResourceManager.Instance&&buildFlag)
        {
            ResourceManager.Instance.AddMaxStorage(-Max);
        }
    }

    protected override void Input()
    {
        ResourceManager.Instance.AddResource(99999, -runtimeBuildData.CostPerWeek * TechManager.Instance.MaintenanceCostBuff());
        runtimeBuildData.Pause = false;
        if (runtimeBuildData.CurPeople < runtimeBuildData.Population)
        {
            FillUpPopulation();
        }
    }

    protected override void Output()
    {

    }
}
