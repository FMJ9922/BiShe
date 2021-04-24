using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HutBuilding : BuildingBase
{
    private bool hasFoodThisWeek = true;//这周是否获得了食物
    private bool hasProvidePopulation = false;//这周是否提供了人口
    private Storage storage;
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
    public override void InitBuildingFunction()
    {
        storage = transform.GetComponent<Storage>();
        storage.AddResource(11001, 2);
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
        //食物少于指定数量就去取货
        //if (storage.GetAllFoodNum() < 10)
        {
            CarMission mission = MakeCarMission();
            TrafficManager.Instance.UseCar(mission.transportationType, mission, () => mission.EndBuilding.OnRecieveCar(mission), DriveType.once);
        }
        //遍历所有可吃的食物，数量够吃就返回
        for (int i = 0; i < formula.InputItemID.Count; i++)
        {
            //ToDo:消耗食物数量会变化？
            if(storage.TryUseResource(formula.InputItemID[i], formula.InputNum[0]))
            {
                hasFoodThisWeek = true;
                return;
            }
        }
        hasFoodThisWeek = false;
    }

    /// <summary>
    /// 配置运货清单
    /// </summary>
    /// <param name="ratio">运送多少个周期的货</param>
    /// <returns></returns>
    private CarMission MakeCarMission(float ratio = 10)
    {
        CarMission mission = new CarMission();
        mission.StartBuilding = this;
        mission.transportationType = TransportationType.van;
        mission.EndBuilding = MapManager.GetNearestMarket(parkingGridIn).GetComponent<BuildingBase>();
        mission.missionType = CarMissionType.requestResources;
        mission.isAnd = false;
        mission.requestResources = new List<CostResource>();
        for (int i = 0; i < formula.InputItemID.Count; i++)
        {
            mission.requestResources.Add(new CostResource(formula.InputItemID[i], formula.InputNum[0]*ratio));
            
        }
        return mission;
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

    public override void OnRecieveCar(CarMission carMission)
    {
        //Debug.Log(this.PfbName + " recieve");
        //Debug.Log("recieveGoods");
        switch (carMission.missionType)
        {
            case CarMissionType.requestResources:
                break;
            case CarMissionType.transportResources:
                {
                    foreach (var goods in carMission.transportResources)
                    {
                        //Debug.Log("recieve:" + goods.ToString());
                        storage.AddResource(goods);
                    }
                }
                break;
            default:
                break;
        }
    }
    //public override string GetIntroduce()
    //{
    //    return string.Empty;
    //}
}
