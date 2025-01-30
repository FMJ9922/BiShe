using System.Collections.Generic;
using CSTools;
using Manager;
using UnityEngine;

namespace Building
{
    public class FactoryBuilding : BuildingBase,IBuildingBasic,IProduct,ITransportation
    {
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
                FillUpPopulation();
                InitBuildingFunction();
                //地基
                MapManager.Instance.BuildFoundation(vector2Ints, TexIndex.Cement, ((int) runtimeBuildData.direction + 1) % 4);
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
            body.SetActive(false);
            animation.gameObject.SetActive(true);
            float time = animation.clip.length;
            Invoke("ShowBody", time);
            animation["Take 001"].speed = 1f;
            animation.Play();
        }

        private void ShowBody()
        {
            body.SetActive(true);
            animation.gameObject.SetActive(false);
        }

        public void InitBuildingFunction()
        {
            parkingGridIn = BuildingTools.GetInParkingGrid(this);
            MapManager.Instance.AddBuilding(this);
            MapManager.Instance.AddBuildingEntry(parkingGridIn, this);
            EventManager.StartListening(ConstEvent.OnOutputResources, Output);
            EventManager.StartListening(ConstEvent.OnInputResources, Input);
            EventManager.StartListening<string>(ConstEvent.OnDayWentBy, UpdateRate);
            ChangeFormula();
            runtimeBuildData.CurPeople = ResourceManager.Instance.GetMaxWorkerRemain(runtimeBuildData.Population);
            EventManager.TriggerEvent(ConstEvent.OnPopulationChange);
        }

        public void RestartBuildingFunction()
        {
            if (runtimeBuildData.formulaDatas.Length > 0)
            {
                runtimeBuildData.formula = runtimeBuildData.formulaDatas[runtimeBuildData.CurFormula];
            }

            parkingGridIn = BuildingTools.GetInParkingGrid(this);
            MapManager.Instance.AddBuilding(this);
            MapManager.Instance.AddBuildingEntry(parkingGridIn, this);
            EventManager.StartListening(ConstEvent.OnOutputResources, Output);
            EventManager.StartListening(ConstEvent.OnInputResources, Input);
            EventManager.StartListening<string>(ConstEvent.OnDayWentBy, UpdateRate);
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

        public void Input()
        {
            BuildingTools.DoInput(runtimeBuildData);
        }

        public void Output()
        {
            BuildingTools.DoOutput(runtimeBuildData);
            var list = BuildingTools.CheckCanSendCar(runtimeBuildData);
            if (list != null)
            {
                CheckSendOutputCar(list);
            }
        }

        public void CheckSendOutputCar(List<CostResource> costResources)
        {
            CarMission carMission = MakeCarMission(costResources);
            if (carMission != null)
            {
                TrafficManager.Instance.UseCar(carMission,
                    (bool success) => { runtimeBuildData.AvaliableToMarket = success; });
            }
            else
            {
                runtimeBuildData.AvaliableToMarket = false;
            }
        }

        public float GetProcess()
        {
            return 1 - (float) runtimeBuildData.productTime / runtimeBuildData.formula.ProductTime +
                   (float) LevelManager.Instance.Day / 7 / runtimeBuildData.formula.ProductTime;
        }

        public void UpdateRate(string date)
        {
            UpdateEffectiveness();
            runtimeBuildData.Rate += runtimeBuildData.Effectiveness / 7f / runtimeBuildData.formula.ProductTime;
        }

        public void UpdateEffectiveness()
        {
            int cur = runtimeBuildData.CurPeople;
            int max = runtimeBuildData.Population + TechManager.Instance.PopulationBuff();
            runtimeBuildData.Effectiveness = runtimeBuildData.Pause
                ? 0
                : ((float) cur) / (float) max * TechManager.Instance.EffectivenessBuff();
        }

        public void ChangeFormula()
        {
            if (runtimeBuildData.formulaDatas.Length > 0)
            {
                runtimeBuildData.formula = runtimeBuildData.formulaDatas[runtimeBuildData.CurFormula];
                runtimeBuildData.productTime = runtimeBuildData.formula.ProductTime;
                runtimeBuildData.Rate = 0;
            }
        }

        public CarMission MakeCarMission(List<CostResource> costResources)
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
                mission.transportResources.Add(new CostResource(costResources[i].ItemId,costResources[i].ItemNum));
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
                    runtimeBuildData.CurPeople += ResourceManager.Instance.GetMaxWorkerRemain(runtimeBuildData.Population + TechManager.Instance.PopulationBuff() - runtimeBuildData.CurPeople);
                    EventManager.TriggerEvent(ConstEvent.OnPopulationHudChange);
                }
                //CheckCurPeopleMoreThanMax();
                UpdateEffectiveness();
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
            return EBuildingType.FactoryBuilding;
        }
    }
}
