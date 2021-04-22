using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GovernmentBuilding : BuildingBase
{
    public static GovernmentBuilding _instance;
    public static GovernmentBuilding Instance
    {
        get
        {
            return _instance;
        }
    }
    private void Start()
    {
        if (null == _instance)
        {
            _instance = this;
        }
        else
        {
            Debug.LogError("manager has already been created previously. " + gameObject.name + " is goning to be destroyed.");
            Destroy(this);
            return;
        }
    }
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
        base.OnRecieveCar(carMission);
    }
}
