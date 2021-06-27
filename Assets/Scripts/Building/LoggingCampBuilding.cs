using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggingCampBuilding : BuildingBase
{
    [SerializeField]NearByTree sensor;
    public override void InitBuildingFunction()
    {
        sensor.gameObject.SetActive(true);
        base.InitBuildingFunction();
    }

    public override void RestartBuildingFunction()
    {
        sensor.gameObject.SetActive(true);
        base.RestartBuildingFunction();
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

    public override void UpdateRate(string date)
    {
        TreeSystem sys = sensor.GetNearestTree();
        if (sys != null)
        {
            Vector3 treePos = sys.transform.position;
            sys.TreeCutDown(new Vector3(0, Random.value * 360, 0));
            EventManager.TriggerEvent(ConstEvent.OnPlantSingleTree, treePos);
            base.UpdateRate(date);
        }
    }
    protected override void Input()
    {
        base.Input();
    }

    

    
}
