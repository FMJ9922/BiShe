using System.Collections.Generic;
using CSTools;
using UnityEngine;

namespace Building
{
    public class HutBuilding : BuildingBase,IBuildingBasic,IHabitable,ITransportation
    {
        private bool hasFoodThisWeek = true;//这周是否获得了食物
        private bool hasProvidePopulation = false;//这周是否提供了人口
       
        public void OnConfirmBuild(Vector2Int[] vector2Ints)
        {
            storage = transform.GetComponent<Storage>();
            storage.AddResource(11001, 1);
            ProvidePopulation();
            takenGrids = vector2Ints;
            gameObject.tag = "Building";
            transform.GetComponent<BoxCollider>().enabled = false;
            transform.GetComponent<BoxCollider>().enabled = true;

            if (!buildFlag)
            {
                buildFlag = true;
                if (hasAnima)
                {
                    Invoke("PlayAnim", 0.2f);
                }
                runtimeBuildData.direction = CastTool.CastVector3ToDirection(transform.right);
                runtimeBuildData.Happiness = (80f + 10 * runtimeBuildData.CurLevel) / 100;
                Invoke(nameof(FillUpPopulation), 1f);
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
                UpdateEffectiveness();
            }
        }
        
        private void UpdateEffectiveness()
        {
            int cur = runtimeBuildData.CurPeople;
            int max = runtimeBuildData.Population + TechManager.Instance.PopulationBuff();
            runtimeBuildData.Effectiveness = runtimeBuildData.Pause ? 0 : ((float)cur) / (float)max * TechManager.Instance.EffectivenessBuff();
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
            MapManager.Instance._buildings.Add(this);
            MapManager.Instance.AddBuildingEntry(parkingGridIn, this);
        }

        public void RestartBuildingFunction()
        {
            storage = transform.GetComponent<Storage>();
            storage.AddResource(11001, Random.Range(3,13));
            ProvidePopulation();
            if (runtimeBuildData.formulaDatas.Length>0)
            {
                runtimeBuildData.formula = runtimeBuildData.formulaDatas[runtimeBuildData.CurFormula];
            }
            parkingGridIn = BuildingTools.GetInParkingGrid(this);
            MapManager.Instance._buildings.Add(this);
            MapManager.Instance.AddBuildingEntry(parkingGridIn, this);
            if (runtimeBuildData.tabType!=BuildTabType.house)
            {
                ResourceManager.Instance.TryAddCurPopulation(runtimeBuildData.CurPeople,true);
            }
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

        public void ProvidePopulation()
        {
            int num = -runtimeBuildData.Population;//负人口表示增加
            hasProvidePopulation = ResourceManager.Instance.AddMaxPopulation(num);
            runtimeBuildData.CurPeople = num;
            EventManager.TriggerEvent<RuntimeBuildData>(ConstEvent.OnPopulaitionChange,runtimeBuildData);
        }

        public void RemovePopulation()
        {
            int num = -runtimeBuildData.Population;//负人口表示增加
            hasProvidePopulation = !ResourceManager.Instance.AddMaxPopulation(-num);
            //todo:表现出饥饿
            runtimeBuildData.CurPeople = num;
            EventManager.TriggerEvent<RuntimeBuildData>(ConstEvent.OnPopulaitionChange, runtimeBuildData);
        }

        public void InputFood()
        {
            ResourceManager.Instance.AddResource(99999, -runtimeBuildData.CostPerWeek * TechManager.Instance.MaintenanceCostBuff());
            //食物少于指定数量就去取货
            if (storage.GetAllFoodNum() < 3)
            {
                CarMission carMission = MakeCarMission(10);
                if (carMission != null)
                {
                    TrafficManager.Instance.UseCar(carMission, (bool success) => { runtimeBuildData.AvaliableToMarket = success; });
                }
                else
                {   
                    runtimeBuildData.AvaliableToMarket = false;
                }
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

        public void Upgrade(out bool issuccess, out BuildingBase buildingData)
        {
            int nextId = runtimeBuildData.RearBuildingId;
            BuildData data = DataManager.GetBuildData(nextId);
            RuntimeBuildData buildData = CastTool.CastBuildDataToRuntime(data);
            buildData.Pause = runtimeBuildData.Pause;
            buildData.CurLevel = runtimeBuildData.CurLevel + 1;
            buildData.CurFormula = runtimeBuildData.CurFormula;
            RemovePopulation();
            buildData.Happiness = (80f + 10 * buildData.CurLevel) / 100;
            buildingData = BuildManager.Instance.UpgradeBuilding(buildData, takenGrids, transform.position, transform.rotation);
            if (buildingData != null)
            {
                SoundManager.Instance.PlaySoundEffect(SoundResource.sfx_upgrade);
                DestroyBuilding(false, false, false);
                MapManager.Instance.AddBuildingEntry(BuildingTools.GetInParkingGrid(this), buildingData);
                issuccess = true;
            }
            else
            {
                issuccess = false;
                ProvidePopulation();
                NoticeManager.Instance.InvokeShowNotice("升级资源不足");
            }
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
            mission.transportationType = TransportationType.van;
            //Debug.Log(MapManager.GetNearestMarket(parkingGridIn).name);
            mission.EndBuilding = target.parkingGridIn;
            mission.missionType = CarMissionType.requestResources;
            mission.isAnd = false;
            mission.requestResources = new List<CostResource>();
            mission.requestResources.Add(ResourceManager.Instance.GetFoodByMax(rate));
            return mission;
        }

        public void OnRecieveCar(CarMission carMission)
        {
            //Debug.Log(this.PfbName + " recieve");
            //Debug.Log("recieveGoods");
            if (carMission == null)
            {
                return;
            }
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
    }
}
