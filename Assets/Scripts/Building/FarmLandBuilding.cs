using System.Collections.Generic;
using CSTools;
using UnityEngine;

namespace Building
{
    public class FarmLandBuilding : BuildingBase,IBuildingBasic,IProduct,ITransportation
    {
        public Color yellow;
        public Color green;
        [SerializeField] Transform wheatTrans;
        [SerializeField] GameObject previewObj;
        public List<GameObject> lists;
        bool isharvesting = false;
        bool waitNextWeekStart = false;

        private PlantController[] plants;

        public Texture wheat;
        public Texture rice;

        private Material mat;
        
        #region IBuildingBasic
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
                Invoke(nameof(FillUpPopulation), 1f);
                InitBuildingFunction();
                //地基
                MapManager.Instance.BuildFoundation(vector2Ints, 2, ((int)runtimeBuildData.direction + 1) % 4);
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
            return EBuildingType.FarmLandBuilding;
        }

        public void PlayAnim()
        {
            //none
        }

        public void InitBuildingFunction()
        {
            Invoke("InitPlant", 1f);
            previewObj.SetActive(false);
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
            InitPlant();
            previewObj.SetActive(false);
            SetProgress(GetProcess());
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

        #endregion IBuildingBasic
        
        #region IProduct
        public void Input()
        {
            ResourceManager.Instance.TryUseUpResource(new CostResource(99999, runtimeBuildData.CostPerWeek * TechManager.Instance.MaintenanceCostBuff()));
            runtimeBuildData.Pause = false;
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
            }
            else
            {
                ResourceManager.Instance.TryUseResources(costResources);
            }
        }

        public void Output()
        {
            if (runtimeBuildData.formula == null || runtimeBuildData.formula.OutputItemID == null) return;

            if (!isharvesting&&!waitNextWeekStart)
            {
                runtimeBuildData.productTime--;
                if (runtimeBuildData.productTime <= 0)
                {
                    isharvesting = true;
                    CarMission mission = MakeHarvestCarMission();
                    TrafficManager.Instance.UseCar(mission);
                }
            }
            if (waitNextWeekStart == true)
            {
                waitNextWeekStart = false;
                Replant();
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
        
        public virtual void UpdateRate(string date)
        {
            UpdateEffectiveness();
            if (!isharvesting&&!waitNextWeekStart)
            {
                runtimeBuildData.Rate += runtimeBuildData.Effectiveness / 7f / runtimeBuildData.formula.ProductTime;
                SetProgress(GetProcess());
            }
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

        #endregion

        #region ITransportation

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
            if (carMission == null)
            {
                return;
            }
            switch (carMission.missionType)
            {
                case CarMissionType.transportResources:
                    ResourceManager.Instance.AddResources(carMission.transportResources.ToArray());
                    break;
                case CarMissionType.harvest:
                {
                    isharvesting = false;
                    waitNextWeekStart = true;
                    OnFinishHarvest(runtimeBuildData.Rate);
                    break;
                }
            }
        }
        private CarMission MakeHarvestCarMission()
        {
            CarMission mission = new CarMission();
            mission.transportationType = TransportationType.harvester;
            mission.missionType = CarMissionType.harvest;

            Vector3[] vecs = new Vector3[lists.Count];
            for (int i = 0; i < lists.Count; i++)
                vecs[i] = (lists[i].transform.position);mission.EndBuilding = parkingGridIn;
            mission.wayPoints = Vector3Serializer.Box(vecs);
            return mission;
        }
        

        #endregion

        #region Private
        
        private void SetProgress(float progress)
        {
            if (mat != null)
            {
                if (progress <= 0)
                {
                    progress = 0.01f;
                }
                if(progress >= 1)
                {
                    progress = 0.99F;
                }
                mat.SetFloat("_Progress", progress);
            }
        }

        private void InitPlant()
        {
            GameObject pfb = Instantiate(LoadAB.Load("mat.ab", "PlantPfb"), transform);
            pfb.transform.position -= Vector3.up * 2000f;
            plants = new PlantController[Size.x * Size.y];
            mat = pfb.GetComponent<PlantController>().mesh.material;
            Texture tex = runtimeBuildData.formula.ID == 50006 ?
                wheat : rice;
            //Debug.Log(mat == null);
            mat.SetTexture("_MainTex", tex);
            SetProgress(GetProcess());
            for (int i = 0; i < Size.x; i++)
            {
                for (int j = 0; j < Size.y; j++)
                {
                    GameObject newGrid = Instantiate(pfb, wheatTrans);
                    plants[i * Size.y + j] = newGrid.GetComponent<PlantController>();
                    plants[i * Size.y + j].SetMat(mat);
                    Vector3 random = Random.insideUnitSphere / 5;
                    Vector3 pos;
                    if (j % 2 == 1)
                    {
                        pos = new Vector3(j * 2 - 2, 0, i * 2 + 1);
                    }
                    else
                    {
                        pos = new Vector3(j * 2, 0, i * 2);
                    }
                    newGrid.transform.localPosition =pos + new Vector3(random.x + 2, 0, random.z + 0.5f);
                    plants[i * Size.y + j].SetPos(newGrid.transform.localPosition);
                }
            }
        }
        
        private void ShowPlant()
        {
            for (int i = 0; i < plants.Length; i++)
            {
                plants[i].Show();
            }
        }
        
        private void Replant()
        {
            //EventManager.StopListening(ConstEvent.OnDayWentBy, Replant);
            runtimeBuildData.Rate = 0;
            runtimeBuildData.productTime = runtimeBuildData.formula.ProductTime;
            ShowPlant();
        }
        
        public void OnFinishHarvest(float rate)
        {
            //Debug.Log("finish");
            isharvesting = false;
            waitNextWeekStart = true;
            CarMission carMission = MakeCarMission(rate);
            if (carMission != null)
            {
                TrafficManager.Instance.UseCar(carMission,(bool success)=> { runtimeBuildData.AvaliableToMarket = success; });
            }
            else
            {
                runtimeBuildData.AvaliableToMarket = false;
            }
        }
        #endregion
        
        
    }
}
