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
            Destroy(_instance);
            _instance = this;
            return;
        }
    }

    public override void OnRecieveCar(CarMission carMission)
    {
        base.OnRecieveCar(carMission);
    }
}
