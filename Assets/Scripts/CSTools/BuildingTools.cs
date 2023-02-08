using System.Collections.Generic;
using Building;
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
    }
    
    
}
