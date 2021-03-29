using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HutBuilding : BuildingBase
{
    private bool hasFoodThisWeek = true;//这周是否获得了食物
    private bool hasProvidePopulation = false;//这周是否提供了人口
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
    public override void InitBuildingFunction()
    {
        base.InitBuildingFunction();
    }

    protected override void Output()
    {
        if (hasFoodThisWeek && !hasProvidePopulation)
        {
            ProvidePopulation();
        }
        else if(!hasFoodThisWeek && hasProvidePopulation)
        {
            RemovePopulation();
        }
    }

    protected override void Input()
    {
        TrafficManager.Instance.UseCar(TransportationType.mini, this, GovernmentBuilding.Instance, DriveType.once);
        //遍历所有可吃的食物，数量够吃就返回
        for (int i = 0; i < formula.InputItemID.Count; i++)
        {
            //ToDo:消耗食物数量会变化？
            if(ResourceManager.Instance.TryUseResource(formula.InputItemID[i], formula.InputNum[0]))
            {
                hasFoodThisWeek = true;
                return;
            }
        }
        hasFoodThisWeek = false;
    }

    /// <summary>
    /// 食物充足时，住房提供人口
    /// </summary>
    public void ProvidePopulation()
    {
        int num = -runtimeBuildData.Population;//负人口表示增加
        hasProvidePopulation = ResourceManager.Instance.AddMaxPopulation(num);
        runtimeBuildData.CurPeople = num;
        EventManager.TriggerEvent<RuntimeBuildData>(ConstEvent.OnPopulaitionChange,runtimeBuildData);
    }

    /// <summary>
    /// 食物不充足，移除住房提供的人口
    /// </summary>
    public void RemovePopulation()
    {
        int num = -runtimeBuildData.Population;//负人口表示增加
        hasProvidePopulation = !ResourceManager.Instance.AddMaxPopulation(-num);
        //todo:表现出饥饿
        runtimeBuildData.CurPeople = num;
        EventManager.TriggerEvent<RuntimeBuildData>(ConstEvent.OnPopulaitionChange, runtimeBuildData);
    }
    
    //public override string GetIntroduce()
    //{
    //    return string.Empty;
    //}
}
