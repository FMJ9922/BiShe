using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HutBuilding : BuildingBase
{
    private void OnDestroy()
    {
        EventManager.StopListening(ConstEvent.OnInputResources, Input);
    }
    public override void InitBuildingFunction()
    {
        Debug.Log("2");
        ProvidePopulation();
        EventManager.StartListening(ConstEvent.OnInputResources, Input);
    }

    /// <summary>
    /// 住房提供人口
    /// </summary>
    public void ProvidePopulation()
    {
        int num = runtimeBuildData.outputResources[0].ItemNum;
        bool success;
        ResourceManager.Instance.AddMaxPopulation(num,out success);
    }

    /// <summary>
    /// 每周输入资源
    /// </summary>
    public void Input()
    {
        //Debug.Log(runtimeBuildData.inputResources[0].ItemId);
        for (int i = 0; i < runtimeBuildData.inputResources.Count; i++)
        {
            ResourceManager.Instance.TryUseResource(runtimeBuildData.inputResources[i]);
        }
    }
    //public override string GetIntroduce()
    //{
    //    return string.Empty;
    //}
}
