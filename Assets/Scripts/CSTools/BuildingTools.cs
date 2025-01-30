using System.Collections.Generic;
using Building;
using Manager;
using UnityEngine;

namespace CSTools
{
    public class BuildingTools 
    {
        public static List<CostResource> GetBuildingWeekDeltaResources(RuntimeBuildData runtimeBuildData)
        {
            List<CostResource> statistics = new List<CostResource>();
            statistics.Add(new CostResource(99999, -runtimeBuildData.CostPerWeek * TechManager.Instance.MaintenanceCostBuff()));
            var formula = runtimeBuildData.formula;
            if (formula != null&& !runtimeBuildData.Pause)
            {
                for (int i = 0; formula.InputItemID!=null&&i < formula.InputItemID.Count; i++)
                {
                    float num = -formula.InputNum[i] * GetWorkEffect(runtimeBuildData) * runtimeBuildData.Times;
                    statistics.Add(new CostResource(formula.InputItemID[i], num*TechManager.Instance.ResourcesBuff()));
                    
                }

                for (int i = 0; formula.OutputItemID != null && i < formula.OutputItemID.Count; i++)
                {
                    float num = formula.ProductNum[i] * GetWorkEffect(runtimeBuildData) * runtimeBuildData.Times / formula.ProductTime * TechManager.Instance.ResourcesBuff();
                    statistics.Add(new CostResource(formula.OutputItemID[i], num));
                }
            }
            if (runtimeBuildData.tabType == BuildTabType.house)
            {
                statistics.Add(ResourceManager.Instance.GetFoodByMax(-1, true));
            }
            return statistics;
        }

        public static float GetWorkEffect(RuntimeBuildData runtimeBuildData)
        {
            if (runtimeBuildData.tabType == BuildTabType.house)
            {
                return 1;
            }
            return (float) runtimeBuildData.CurPeople / (runtimeBuildData.Population + TechManager.Instance.PopulationBuff());
        }
        
        public static List<WarningType> GetWarnings(RuntimeBuildData runtimeBuildData)
        {
            List<WarningType> res = new List<WarningType>();
            if (runtimeBuildData.CurPeople == 0)
            {
                res.Add(WarningType.noPeople);
            }
            if (runtimeBuildData.Pause)
            {
                res.Add(WarningType.noResources);
            }
            if (!runtimeBuildData.AvaliableToMarket)
            {
                res.Add(WarningType.noRoad);
            }
            return res;
        }
        
        public static Vector2Int GetInParkingGrid(BuildingBase buildingBase)
        {
            return MapManager.GetCenterGrid(buildingBase.transform.position+CastTool.CastDirectionToVector(buildingBase.runtimeBuildData.direction)*((float)buildingBase.Size.y/2+0.5f));

        }

        public static void ChangeBuildingWorker(RuntimeBuildData runtimeBuildData,int deltaNum)
        {
            int curWorkerNum = runtimeBuildData.CurPeople;
            int remainWorkPlace = runtimeBuildData.Population + TechManager.Instance.PopulationBuff() - curWorkerNum;
            if (deltaNum > 0)
            {
                runtimeBuildData.CurPeople += ResourceManager.Instance.GetMaxWorkerRemain(Mathf.Min(remainWorkPlace,deltaNum));
            }
            else
            {
                if (runtimeBuildData.CurPeople > 0)
                {
                    runtimeBuildData.CurPeople -= Mathf.Min(runtimeBuildData.CurPeople,-deltaNum);
                }
            }
            EventManager.TriggerEvent(ConstEvent.OnPopulationChange);
        }

        public static void DoInput(RuntimeBuildData runtimeBuildData)
        {
            ResourceManager.Instance.TryUseUpResource(new CostResource(99999, runtimeBuildData.CostPerWeek * TechManager.Instance.MaintenanceCostBuff()));
            runtimeBuildData.Pause = false;
            if (runtimeBuildData.formula == null|| runtimeBuildData.formula.InputItemID==null) return;
            List<CostResource> costResources = new List<CostResource>();
            for (int i = 0; i < runtimeBuildData.formula.InputItemID.Count; i++)
            {
                costResources.Add(new CostResource(runtimeBuildData.formula.InputItemID[i], 
                    runtimeBuildData.formula.InputNum[i]* GetWorkEffect(runtimeBuildData)* runtimeBuildData.Times));
            }

            bool res = ResourceManager.Instance.IsResourcesEnough(costResources, TechManager.Instance.ResourcesBuff());
            if (!res)
            {
                runtimeBuildData.Pause = true;
            }
            else
            {
                ResourceManager.Instance.TryUseResources(costResources);
            }
        }

        public static void DoOutput(RuntimeBuildData runtimeBuildData)
        {
            var formula = runtimeBuildData.formula;
            if (formula?.OutputItemID == null) return;
            runtimeBuildData.productTime--;
            if (runtimeBuildData.productTime <= 0)
            {
                runtimeBuildData.productTime = formula.ProductTime;
                float rate = runtimeBuildData.Rate;
                runtimeBuildData.Rate = 0;
                for (int i = 0; i < formula.OutputItemID.Count; i++)
                {
                    AddResource(runtimeBuildData, new CostResource(formula.OutputItemID[i],
                        formula.ProductNum[i] * rate * runtimeBuildData.Times));
                }
            }
        }

        public static void AddResource(RuntimeBuildData runtimeBuildData, CostResource costResource)
        {
            var itemDic = runtimeBuildData.StoredItemDic ?? new Dictionary<int, float>();

            if (itemDic.ContainsKey(costResource.ItemId))
            {
                itemDic[costResource.ItemId] += costResource.ItemNum;
            }
            else
            {
                itemDic.Add(costResource.ItemId, costResource.ItemNum);
            }

            if (runtimeBuildData.StoredItemDic == null)
            {
                runtimeBuildData.StoredItemDic = itemDic;
            }
        }
        
        public static bool TryUseResource(RuntimeBuildData runtimeBuildData, CostResource costResource)
        {
            var itemDic = runtimeBuildData.StoredItemDic;
            float num = costResource.ItemNum;
            int id = costResource.ItemId;
            if (itemDic.TryGetValue(id, out float storedNum))//字典里已存该物品
            {
                if (num <= storedNum)//物品数量足够消耗
                {
                    itemDic[id] -= num;//消耗物品
                    return true;//返回成功
                }
            }
            return false;
        }

        private static List<CostResource> _tempCostResources = new List<CostResource>();

        public static List<CostResource> CheckCanSendCar(RuntimeBuildData runtimeBuildData)
        {
            if (runtimeBuildData.StoredItemDic == null)
            {
                return null;
            }
            CarData carData = DataManager.GetCarData(TransportationType.mini);
            float storage = carData.Storage;
            float totalNum = 0;
            _tempCostResources.Clear();
            foreach (var kp in runtimeBuildData.StoredItemDic)
            {
                if (kp.Value + totalNum >= storage)
                {
                    float canAddNum = storage - totalNum;
                    var costResource = new CostResource(kp.Key, canAddNum);
                    _tempCostResources.Add(costResource);
                    for (int i = 0; i < _tempCostResources.Count; i++)
                    {
                        TryUseResource(runtimeBuildData, _tempCostResources[i]);
                    }
                    return _tempCostResources;
                }
                else
                {
                    var costResource = new CostResource(kp.Key, kp.Value);
                    _tempCostResources.Add(costResource);
                }
            }
            return null;
        }
    }
    
    
}
