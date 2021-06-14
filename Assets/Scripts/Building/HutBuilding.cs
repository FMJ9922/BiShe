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
        storage.AddResource(11001, 5);
        ProvidePopulation();
        base.InitBuildingFunction();
    }
    public override float GetHappiness()
    {
        //Debug.Log("hap" + runtimeBuildData.Happiness);
        return this.runtimeBuildData.Happiness;
    }
    protected override void Output()
    {
        /*if (hasFoodThisWeek && !hasProvidePopulation)
        {
            ProvidePopulation();
        }
        else if(!hasFoodThisWeek && hasProvidePopulation)
        {
            RemovePopulation();
        }*/
    }

    protected override void Input()
    {
        ResourceManager.Instance.AddResource(99999, -runtimeBuildData.CostPerWeek);
        //食物少于指定数量就去取货
        if (storage.GetAllFoodNum() < 3)
        {
            CarMission mission = MakeCarMission();
            TrafficManager.Instance.UseCar(mission, () => mission.EndBuilding.OnRecieveCar(mission), DriveType.once);
        }

        ItemData[] foodIDs = DataManager.GetFoodItemList();
        for (int i = 0; i < foodIDs.Length; i++)
        {
            if (storage.TryUseResource(new CostResource(foodIDs[i].Id, 0.1f*runtimeBuildData.CurPeople)))
            {
                hasFoodThisWeek = true;
                this.runtimeBuildData.Happiness = (80f + 10 * runtimeBuildData.CurLevel+foodIDs[i].Happiness) / 100f;
                //Debug.Log(runtimeBuildData.Happiness);
                return;
            }
        }
        this.runtimeBuildData.Happiness = (80f + 10 * runtimeBuildData.CurLevel) / 100f;
        hasFoodThisWeek = false;
    }

    /// <summary>
    /// 配置运货清单
    /// </summary>
    /// <param name="ratio">运送多少个周期的货</param>
    /// <returns></returns>
    private CarMission MakeCarMission(float ratio = 5)
    {
        CarMission mission = new CarMission();
        mission.StartBuilding = this;
        mission.transportationType = TransportationType.van;
        //Debug.Log(MapManager.GetNearestMarket(parkingGridIn).name);
        mission.EndBuilding = MapManager.GetNearestMarket(parkingGridIn).GetComponent<BuildingBase>();
        mission.missionType = CarMissionType.requestResources;
        mission.isAnd = false;
        mission.requestResources = new List<CostResource>();
       mission.requestResources.Add(ResourceManager.Instance.GetFoodByHappiness(ratio));
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
