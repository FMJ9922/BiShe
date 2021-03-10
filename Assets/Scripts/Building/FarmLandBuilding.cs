using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmLandBuilding : BuildingBase
{
    private int productTime;//生产周期（周）

    private void OnDestroy()
    {
        EventManager.StopListening(ConstEvent.OnOutputResources, Output);
        EventManager.StopListening(ConstEvent.OnInputResources, Input);
    }
    public override void InitBuildingFunction()
    {
        EventManager.StartListening(ConstEvent.OnOutputResources, Output);
        EventManager.StartListening(ConstEvent.OnInputResources, Input);
        productTime = runtimeBuildData.ProductTime;
    }

    public void Output()
    {
        productTime--;
        if(productTime <= 0)
        {
            productTime = runtimeBuildData.ProductTime;
            for (int i = 0; i < runtimeBuildData.outputResources.Count; i++)
            {
                ResourceManager.Instance.AddResource(runtimeBuildData.outputResources[i]);
            }
        }
    }

    public void Input()
    {
        for (int i = 0; i < runtimeBuildData.inputResources.Count; i++)
        {
            ResourceManager.Instance.TryUseResource(runtimeBuildData.inputResources[i]);
        }
    }
}
