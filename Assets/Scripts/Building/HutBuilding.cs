using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HutBuilding : BuildingBase
{
    private bool hasFoodThisWeek = true;//这周是否获得了食物
    private bool hasProvidePopulation = false;//这周是否提供了人口
    public override void InitBuildingFunction()
    {
        storage = transform.GetComponent<Storage>();
        storage.AddResource(11001, 1);
        ProvidePopulation();
        base.InitBuildingFunction();
    }

    public override void RestartBuildingFunction()
    {
        storage = transform.GetComponent<Storage>();
        storage.AddResource(11001, Random.Range(3,13));
        ProvidePopulation();
        base.RestartBuildingFunction();
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
            TrafficManager.Instance.UseCar(mission, DriveType.once);
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
    protected override CarMission MakeCarMission(float ratio = 10)
    {
        CarMission mission = new CarMission();
        mission.StartBuilding = parkingGridIn;
        mission.transportationType = TransportationType.van;
        //Debug.Log(MapManager.GetNearestMarket(parkingGridIn).name);
        mission.EndBuilding = MapManager.GetNearestMarket(parkingGridIn).GetComponent<BuildingBase>().parkingGridIn;
        mission.missionType = CarMissionType.requestResources;
        mission.isAnd = false;
        mission.requestResources = new List<CostResource>();
        mission.requestResources.Add(ResourceManager.Instance.GetFoodByMax(ratio));
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
    /// 移除住房提供的人口
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
    public override void Upgrade(out bool issuccess)
    {
        //todo：检查是否有足够的资源升级
        int nextId = runtimeBuildData.RearBuildingId;
        BuildData data = DataManager.GetBuildData(nextId);
        RuntimeBuildData buildData = BuildingBase.CastBuildDataToRuntime(data);
        buildData.Pause = runtimeBuildData.Pause;
        buildData.CurLevel = runtimeBuildData.CurLevel + 1;
        buildData.CurFormula = runtimeBuildData.CurFormula;
        RemovePopulation();
        buildData.Happiness = (80f + 10 * buildData.CurLevel) / 100;
        BuildManager.Instance.UpgradeBuilding(buildData, takenGrids, transform.position, transform.rotation, out bool success);
        issuccess = success;
        if (success)
        {
            SoundManager.Instance.PlaySoundEffect(SoundResource.sfx_upgrade);
            DestroyBuilding(false, false, false);
        }
        else
        {
            ProvidePopulation();
            NoticeManager.Instance.InvokeShowNotice("升级资源不足");
        }
    }
}
