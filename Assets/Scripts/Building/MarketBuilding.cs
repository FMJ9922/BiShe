using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MarketBuilding : BuildingBase
{
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
        switch (carMission.missionType)
        {
            case CarMissionType.requestResources:
                BuildRecievedCarMission(carMission);
                CarMission car = carMission;
                void ac() { car.EndBuilding.OnRecieveCar(car); }
                TrafficManager.Instance.UseCar(car, ac, DriveType.once);
                break;
            case CarMissionType.transportResources:
                ResourceManager.Instance.AddResources(carMission.transportResources.ToArray());
                break;
            default:
                break;
        }
    }

    public override void UpdateRate(string date)
    {
        //CheckCurPeopleMoreThanMax();
        UpdateEffectiveness();
        runtimeBuildData.Rate += runtimeBuildData.Effectiveness / 7f / formula.ProductTime;
        if(runtimeBuildData.CurPeople < runtimeBuildData.Population)
        {
            FillUpPopulation();
        }
    }
    private void BuildRecievedCarMission(CarMission carMission)
    {
        BuildingBase temp = carMission.StartBuilding;
        carMission.StartBuilding = carMission.EndBuilding;
        carMission.EndBuilding = temp;
        switch (carMission.missionType)
        {
            case CarMissionType.requestResources:
                carMission.missionType = CarMissionType.transportResources;
                carMission.transportResources = new List<CostResource>();

                if (carMission.requestResources[0].ItemId == 11000)
                {
                    int[] foodlist = DataManager.GetFoodIDList();
                    for (int i = 0; i < foodlist.Length; i++)
                    {
                        carMission.requestResources.Add(new CostResource(foodlist[i], carMission.requestResources[i].ItemNum));
                    }
                    carMission.requestResources.RemoveAt(0);
                }
                foreach (var request in carMission.requestResources)
                {
                    CostResource transport = ResourceManager.Instance.TryUseUpResource(request);
                    if (transport != null)
                    {
                        carMission.transportResources.Add(transport);
                        if (carMission.isAnd)continue;
                        else return;
                    }
                    EventManager.TriggerEvent(ConstEvent.OnRefreshResources);
                }
                break;
            default:
                break;
        }
    }
}
