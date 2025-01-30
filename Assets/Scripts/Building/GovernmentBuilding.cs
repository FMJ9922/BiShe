using System.Collections.Generic;
using CSTools;
using Manager;
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
                MapManager.Instance.BuildFoundation(vector2Ints, TexIndex.Cement);
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
            MapManager.Instance.AddBuilding(this);
            MapManager.Instance.AddBuildingEntry(parkingGridIn, this);
            runtimeBuildData.CurPeople = ResourceManager.Instance.GetMaxWorkerRemain(runtimeBuildData.Population);
            EventManager.TriggerEvent(ConstEvent.OnPopulationChange);
        }

        public void RestartBuildingFunction()
        {
            if (runtimeBuildData.formulaDatas.Length>0)
            {
                runtimeBuildData.formula = runtimeBuildData.formulaDatas[runtimeBuildData.CurFormula];
            }
            parkingGridIn = BuildingTools.GetInParkingGrid(this);
            MapManager.Instance.AddBuilding(this);
            MapManager.Instance.AddBuildingEntry(parkingGridIn, this);
            EventManager.TriggerEvent(ConstEvent.OnPopulationChange);
        }

        public void DestroyBuilding(bool returnResources, bool returnPopulation, bool repaint = true)
        {
            if (returnResources)
            {
                ReturnBuildResources();
            }
            MapManager.Instance.RemoveBuilding(this);
            MapManager.Instance.RemoveBuildingEntry(parkingGridIn);
        
            if (repaint)
            {
                MapManager.SetGridTypeToEmpty(takenGrids);
                MapManager.Instance.BuildOriginFoundation(takenGrids);
            }
            if (returnPopulation)
            {
                runtimeBuildData.CurPeople = 0;
                EventManager.TriggerEvent(ConstEvent.OnPopulationChange);
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
            BuildingBase target = MapManager.Instance.GetNearestMarket(parkingGridIn);
            if (target == null)
            {
                return null;
            }
            mission.StartBuilding = parkingGridIn;
            mission.EndBuilding = target.parkingGridIn;
            mission.missionType = CarMissionType.transportResources;
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

        public void CheckSendOutputCar(float rate)
        {
            
        }

        public void FillUpPopulation()
        {
            if (runtimeBuildData.Population > 0 && runtimeBuildData.tabType != BuildTabType.house)
            {
                if (runtimeBuildData.Population + TechManager.Instance.PopulationBuff() - runtimeBuildData.CurPeople > 0)
                {
                    runtimeBuildData.CurPeople += ResourceManager.Instance.GetMaxWorkerRemain(runtimeBuildData.Population + TechManager.Instance.PopulationBuff() - runtimeBuildData.CurPeople);
                    EventManager.TriggerEvent(ConstEvent.OnPopulationHudChange);
                }
                //CheckCurPeopleMoreThanMax();
            }
        }

        public void Upgrade(out bool issuccess, out BuildingBase buildingData)
        {
            int nextId = runtimeBuildData.RearBuildingId;
            BuildData data = DataManager.GetBuildData(nextId);
            RuntimeBuildData buildData = CastTool.CastBuildDataToRuntime(data);
            buildData.Pause = runtimeBuildData.Pause;
            buildData.CurLevel = runtimeBuildData.CurLevel + 1;
            buildData.CurFormula = runtimeBuildData.CurFormula;
            buildData.Happiness = (80f + 10 * buildData.CurLevel) / 100;
            buildingData = BuildManager.Instance.UpgradeBuilding(buildData, takenGrids, transform.position, transform.rotation);
            if (buildingData != null)
            {
                SoundManager.Instance.PlaySoundEffect(SoundResource.sfx_upgrade);
                DestroyBuilding(true, true, false);
                MapManager.Instance.AddBuildingEntry(BuildingTools.GetInParkingGrid(this), buildingData);
                issuccess = true;
            }
            else
            {
                issuccess = false;
                NoticeManager.Instance.InvokeShowNotice("升级资源不足");
            }
        }

        public EBuildingType GetBuildingType()
        {
            return EBuildingType.GovernmentBuilding;
        }
    }
}
