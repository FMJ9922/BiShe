using System.Collections.Generic;
using CSTools;
using UnityEngine;

namespace Building
{
    public class PierBuilding : BuildingBase,IBuildingBasic,IProduct,ITransportation
    {
        public Transform boatPos;
    
        public void OnConfirmBuild(Vector2Int[] vector2Ints)
        {
            takenGrids = vector2Ints;
            gameObject.tag = "Building";

            transform.GetComponent<BoxCollider>().enabled = false;
            transform.GetComponent<BoxCollider>().enabled = true;

            //地基
            //MapManager.Instance.BuildFoundation(vector2Ints, 2, ((int)direction + 1) % 4);
            //整平地面
            //Vector3 targetPos = MapManager.GetTerrainPosition(parkingGridIn);
            //float targetHeight = targetPos.y;
            //TerrainGenerator.Instance.FlatGround(takenGrids, targetHeight);
            //parkingGridOut = GetOutParkingGrid();
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
            }
            else
            {
                MapManager.SetGridTypeToOccupy(takenGrids);
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
            if (runtimeBuildData.formulaDatas.Length>0)
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
            ResourceManager.Instance.TryUseUpResource(new CostResource(99999, runtimeBuildData.CostPerWeek * TechManager.Instance.MaintenanceCostBuff()));
            runtimeBuildData.Pause = false;
            //ChangeFormula();
            if (runtimeBuildData.formula == null|| runtimeBuildData.formula.InputItemID==null) return;
            List<CostResource> costResources = new List<CostResource>();
            for (int i = 0; i < runtimeBuildData.formula.InputItemID.Count; i++)
            {
                costResources.Add(new CostResource(runtimeBuildData.formula.InputItemID[i], runtimeBuildData.formula.InputNum[i]* WorkEffect()* runtimeBuildData.Times));
            }

            bool res = ResourceManager.Instance.IsResourcesEnough(costResources, TechManager.Instance.ResourcesBuff());
            if (!res)
            {
                runtimeBuildData.Pause = true;
                return;
            }
            else
            {
                ResourceManager.Instance.TryUseResources(costResources);
            }
        }

        public void Output()
        {
            if (runtimeBuildData.formula == null|| runtimeBuildData.formula.OutputItemID ==null) return;
            runtimeBuildData.productTime--;
            if (runtimeBuildData.productTime <= 0)
            {
                runtimeBuildData.productTime = runtimeBuildData.formula.ProductTime;
                float rate = runtimeBuildData.Rate;
                CarMission carMission = MakeCarMission(rate);
                if (carMission != null)
                {
                    TrafficManager.Instance.UseCar(carMission, (bool success) => { runtimeBuildData.AvaliableToMarket = success; });
                }
                else
                {
                    runtimeBuildData.AvaliableToMarket = false;
                }
                runtimeBuildData.Rate = 0;
            }
        }

        public float GetProcess()
        {
            return 1 - (float)runtimeBuildData.productTime / runtimeBuildData.formula.ProductTime + (float)LevelManager.Instance.Day / 7 / runtimeBuildData.formula.ProductTime;
        }

        public float WorkEffect()
        {
            if(runtimeBuildData.tabType == BuildTabType.house)
            {
                return 1;
            }
            return (float)runtimeBuildData.CurPeople/(runtimeBuildData.Population + TechManager.Instance.PopulationBuff());
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
            runtimeBuildData.Effectiveness = runtimeBuildData.Pause ? 0 : ((float)cur) / (float)max * TechManager.Instance.EffectivenessBuff();
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
            //Debug.Log(rate);
            CarMission mission = new CarMission();
            BuildingBase target = MapManager.GetNearestMarket(parkingGridIn)?.GetComponent<BuildingBase>();
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
                    runtimeBuildData.CurPeople += ResourceManager.Instance.GetMaxWorkerRemain(runtimeBuildData.Population + TechManager.Instance.PopulationBuff() - runtimeBuildData.CurPeople);
                    EventManager.TriggerEvent(ConstEvent.OnPopulationHudChange);
                }
                //CheckCurPeopleMoreThanMax();
                UpdateEffectiveness();
            }
        }

        public void Upgrade(out bool issuccess, out BuildingBase buildingData)
        {
            issuccess = false;
            buildingData = null;
        }

        public EBuildingType GetBuildingType()
        {
            return EBuildingType.PierBuilding;
        }
    }
}
