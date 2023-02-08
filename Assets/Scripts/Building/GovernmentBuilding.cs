using System.Collections.Generic;
using CSTools;
using UnityEngine;

namespace Building
{
    public class GovernmentBuilding : BuildingBase,IBuildingBasic,ITransportation
    {
        private static GovernmentBuilding _instance;
        public static GovernmentBuilding Instance
        {
            get
            {
                return _instance;
            }
        }
        private void Start()
        {
            if (null == _instance)
            {
                _instance = this;
            }
            else
            {
                Destroy(_instance);
                _instance = this;
            }
        }


        public void OnConfirmBuild(Vector2Int[] vector2Ints)
        {
            takenGrids = vector2Ints;
            gameObject.tag = "Building";
            transform.GetComponent<BoxCollider>().enabled = false;
            transform.GetComponent<BoxCollider>().enabled = true;

            if (!buildFlag)
            {
                buildFlag = true;
                if (hasAnima)
                {
                    Invoke(nameof(PlayAnim), 0.2f);
                }
                runtimeBuildData.direction = CastTool.CastVector3ToDirection(transform.right);
                runtimeBuildData.Happiness = (80f + 10 * runtimeBuildData.CurLevel) / 100;
                InitBuildingFunction();
                //地基
                MapManager.Instance.BuildFoundation(vector2Ints, 15);
                TerrainGenerator.Instance.FlatGround
                    (takenGrids, MapManager.GetTerrainPosition(parkingGridIn).y);
            }
            else
            {
                RestartBuildingFunction();
            }
        }

        public void PlayAnim()
        {
        
        }

        public void InitBuildingFunction()
        {
            parkingGridIn = BuildingTools.GetInParkingGrid(this);
            MapManager.Instance._buildings.Add(this);
            MapManager.Instance.AddBuildingEntry(parkingGridIn, this);
        }

        public void RestartBuildingFunction()
        {
            if (runtimeBuildData.formulaDatas.Length>0)
            {
                runtimeBuildData.formula = runtimeBuildData.formulaDatas[runtimeBuildData.CurFormula];
            }
            parkingGridIn = BuildingTools.GetInParkingGrid(this);
            MapManager.Instance._buildings.Add(this);
            MapManager.Instance.AddBuildingEntry(parkingGridIn, this);
            ResourceManager.Instance.TryAddCurPopulation(runtimeBuildData.CurPeople,true);
        }

        public void DestroyBuilding(bool returnResources, bool returnPopulation, bool repaint = true)
        {
            if (returnResources)
            {
                ReturnBuildResources();
            }
            MapManager.Instance._buildings.Remove(this);
            MapManager.Instance.RemoveBuildingEntry(parkingGridIn);
        
            if (repaint)
            {
                MapManager.SetGridTypeToEmpty(takenGrids);
                MapManager.Instance.BuildOriginFoundation(takenGrids);
            }
            if (returnPopulation)
            {
                if (runtimeBuildData.Population < 0)
                {
                    ResourceManager.Instance.AddMaxPopulation(runtimeBuildData.Population);
                }
                else
                {
                    ResourceManager.Instance.TryAddCurPopulation(-runtimeBuildData.CurPeople);
                }
                EventManager.TriggerEvent(ConstEvent.OnPopulaitionChange);
            }
        
            Destroy(this.gameObject);
        }

        public bool ReturnBuildResources()
        {
            List<CostResource> rescources = runtimeBuildData.costResources;
            for (int i = 0; i < rescources.Count; i++)
            {
                rescources[i].ItemNum *= 0.75f;
                ResourceManager.Instance.AddResource(rescources[i]);
            }
            return true;
        }

        public CarMission MakeCarMission(float rate)
        {
            CarMission mission = new CarMission();
            BuildingBase target = MapManager.GetNearestMarket(parkingGridIn)?.GetComponent<BuildingBase>();
            if (target == null)
            {
                return null;
            }
            mission.StartBuilding = parkingGridIn;
            mission.EndBuilding = target.parkingGridIn;
            mission.missionType = CarMissionType.transportResources;
            mission.isAnd = true;
            mission.transportResources = new List<CostResource>();
            mission.transportationType = TransportationType.mini;
            for (int i = 0; i < runtimeBuildData.formula.OutputItemID.Count; i++)
            {
                mission.transportResources.Add(new CostResource(runtimeBuildData.formula.OutputItemID[i], runtimeBuildData.formula.ProductNum[i]*rate*runtimeBuildData.Times));
            }
            return mission;
        }

        public void OnRecieveCar(CarMission carMission)
        {
        
        }
        
        public void FillUpPopulation()
        {
            if (runtimeBuildData.Population > 0 && runtimeBuildData.tabType != BuildTabType.house)
            {
                if (runtimeBuildData.Population + TechManager.Instance.PopulationBuff() - runtimeBuildData.CurPeople > 0)
                {
                    runtimeBuildData.CurPeople += ResourceManager.Instance.TryAddCurPopulation(runtimeBuildData.Population + TechManager.Instance.PopulationBuff() - runtimeBuildData.CurPeople);
                    EventManager.TriggerEvent(ConstEvent.OnPopulaitionChange);
                }
                //CheckCurPeopleMoreThanMax();
                runtimeBuildData.Pause = true;
            }
        }

        public void Upgrade(out bool issuccess, out BuildingBase buildingData)
        {
            issuccess = false;
            buildingData = null;
        }
    }
}
