using System.Collections.Generic;
using CSTools;
using Manager;
using UnityEngine;

namespace Building
{
    public class StorageBuilding : BuildingBase, IBuildingBasic, IProduct, ITransportation
    {
        public float Max;

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
                FillMaxPeople();
                InitBuildingFunction();
                //地基
                MapManager.Instance.BuildFoundation(vector2Ints, TexIndex.Cement,
                    ((int) runtimeBuildData.direction + 1) % 4);
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
            EventManager.StartListening(ConstEvent.OnOutputResources, Output);
            EventManager.StartListening(ConstEvent.OnInputResources, Input);
            EventManager.StartListening<string>(ConstEvent.OnDayWentBy, UpdateRate);
            ChangeFormula();
            runtimeBuildData.CurPeople = ResourceManager.Instance.GetMaxWorkerRemain(runtimeBuildData.Population);
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
            EventManager.StopListening(ConstEvent.OnOutputResources, Output);
            EventManager.StopListening(ConstEvent.OnInputResources, Input);
            EventManager.StopListening<string>(ConstEvent.OnDayWentBy, UpdateRate);
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
            ResourceManager.Instance.AddResource(99999,
                -runtimeBuildData.CostPerWeek * TechManager.Instance.MaintenanceCostBuff());
            runtimeBuildData.Pause = false;
            if (runtimeBuildData.CurPeople < runtimeBuildData.Population)
            {
                FillUpPopulation();
            }
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
            buildingData =
                BuildManager.Instance.UpgradeBuilding(buildData, takenGrids, transform.position, transform.rotation);
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
            return EBuildingType.StorageBuilding;
        }

        public void UpdateEffectiveness()
        {
            int cur = runtimeBuildData.CurPeople;
            int max = runtimeBuildData.Population + TechManager.Instance.PopulationBuff();
            runtimeBuildData.Effectiveness = runtimeBuildData.Pause
                ? 0
                : ((float) cur) / (float) max * TechManager.Instance.EffectivenessBuff();
        }

        public void Output()
        {
        }

        public float GetProcess()
        {
            return 1 - (float) runtimeBuildData.productTime / runtimeBuildData.formula.ProductTime +
                   (float) LevelManager.Instance.Day / 7 / runtimeBuildData.formula.ProductTime;
        }

        public float WorkEffect()
        {
            if (runtimeBuildData.tabType == BuildTabType.house)
            {
                return 1;
            }

            return (float) runtimeBuildData.CurPeople /
                   (runtimeBuildData.Population + TechManager.Instance.PopulationBuff());
        }

        public void UpdateRate(string date)
        {
            UpdateEffectiveness();
            //runtimeBuildData.Rate += runtimeBuildData.Effectiveness / 7f / runtimeBuildData.formula.ProductTime;
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
                //Debug.Log(formula.OutputItemID[i]);
                mission.transportResources.Add(new CostResource(runtimeBuildData.formula.OutputItemID[i],
                    runtimeBuildData.formula.ProductNum[i] * rate * runtimeBuildData.Times));
            }

            return mission;
        }

        public void OnRecieveCar(CarMission carMission)
        {
            if (carMission == null)
            {
                return;
            }

            Debug.Log(carMission);
            switch (carMission.missionType)
            {
                case CarMissionType.collectOrderGoods:
                    var newMission = BuildGoForOrderCarMission(carMission);
                    if (newMission != null)
                    {
                        TrafficManager.Instance.UseCar(newMission);
                    }

                    break;
                case CarMissionType.transportResources:
                    ResourceManager.Instance.AddResources(carMission.transportResources.ToArray());
                    var returnCarMission = BuildReturnCarMission(carMission);
                    if (returnCarMission != null)
                    {
                        TrafficManager.Instance.UseCar(returnCarMission);
                    }

                    break;
            }
        }

        public void CheckSendOutputCar(float rate)
        {
            
        }

        private CarMission BuildGoForOrderCarMission(CarMission carMission)
        {
            var ret = new CarMission();
            ret.StartBuilding = parkingGridIn;
            ret.missionType = CarMissionType.goForOrder;
            ret.EndBuilding = MapManager.Instance.GetTradeBuilding().parkingGridIn;
            ret.transportResources = ResourceManager.Instance.UseResources(carMission.requestResources);
            ret.orderIndex = carMission.orderIndex;
            ret.transportationType = carMission.transportationType;
            ret.BelongToBuilding = carMission.BelongToBuilding;
            return ret;
        }

        private CarMission BuildReturnCarMission(CarMission carMission)
        {
            var ret = new CarMission();
            Debug.Log(carMission.StartBuilding);
            ret.StartBuilding = parkingGridIn;
            ret.missionType = CarMissionType.backFromOrder;
            ret.EndBuilding = carMission.StartBuilding;
            ret.orderIndex = carMission.orderIndex;
            ret.transportationType = carMission.transportationType;
            ret.transportResources = carMission.transportResources;
            ret.BelongToBuilding = carMission.BelongToBuilding;
            return ret;
        }
    }
}