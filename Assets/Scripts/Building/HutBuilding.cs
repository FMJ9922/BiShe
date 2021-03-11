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
        formula = runtimeBuildData.formulaDatas[runtimeBuildData.CurFormula];
        productTime = formula.ProductTime;
    }

    /// <summary>
    /// 住房提供人口
    /// </summary>
    public void ProvidePopulation()
    {
        int num = runtimeBuildData.People;
        bool success;
        ResourceManager.Instance.AddMaxPopulation(num,out success);
    }

    
    //public override string GetIntroduce()
    //{
    //    return string.Empty;
    //}
}
