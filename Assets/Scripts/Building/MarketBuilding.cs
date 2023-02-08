using System.Collections;
using System.Collections.Generic;
using Building;
using CSTools;
using UnityEngine;
using UnityEngine.Events;

public class MarketBuilding : BuildingBase, IBuildingBasic, IProduct, ITransportation
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
                Invoke("PlayAnim", 0.2f);
            }

            runtimeBuildData.direction = CastTool.CastVector3ToDirection(transform.right);
            runtimeBuildData.Happiness = (80f + 10 * runtimeBuildData.CurLevel) / 100;
            Invoke("FillUpPopulation", 1f);
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
        EventManager.StartListening(ConstEvent.OnOutputResources, Output);
        EventManager.StartListening(ConstEvent.OnInputResources, Input);
        EventManager.StartListening<string>(ConstEvent.OnDayWentBy, UpdateRate);
        ChangeFormula();
    }

    public void RestartBuildingFunction()
    {
        if (runtimeBuildData.formulaDatas.Length > 0)
        {
            runtimeBuildData.formula = runtimeBuildData.formulaDatas[runtimeBuildData.CurFormula];
        }

        parkingGridIn = BuildingTools.GetInParkingGrid(this);
        MapManager.Instance._buildings.Add(this);
        MapManager.Instance.AddBuildingEntry(parkingGridIn, this);
        EventManager.StartListening(ConstEvent.OnOutputResources, Output);
        EventManager.StartListening(ConstEvent.OnInputResources, Input);
        EventManager.StartListening<string>(ConstEvent.OnDayWentBy, UpdateRate);
        if (runtimeBuildData.tabType != BuildTabType.house)
        {
            ResourceManager.Instance.TryAddCurPopulation(runtimeBuildData.CurPeople, true);
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

    public void Input()
    {
        ResourceManager.Instance.AddResource(99999,
            -runtimeBuildData.CostPerWeek * TechManager.Instance.MaintenanceCostBuff());
        runtimeBuildData.Pause = false;
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

    public void AddCurPeople(int num)
    {
        int cur = runtimeBuildData.CurPeople;
        int max = runtimeBuildData.Population + TechManager.Instance.PopulationBuff();
        if (cur + num <= max)
        {
            runtimeBuildData.CurPeople += ResourceManager.Instance.TryAddCurPopulation(num);
        }
        else
        {
            runtimeBuildData.CurPeople += ResourceManager.Instance.TryAddCurPopulation(max - cur);
        }

        UpdateEffectiveness();
        EventManager.TriggerEvent(ConstEvent.OnPopulaitionChange);
    }

    public void DeleteCurPeople(int num)
    {
        int cur = runtimeBuildData.CurPeople;
        int max = runtimeBuildData.Population + TechManager.Instance.PopulationBuff();
        if (cur - num >= 0)
        {
            runtimeBuildData.CurPeople += ResourceManager.Instance.TryAddCurPopulation(-num);
        }
        else
        {
            runtimeBuildData.CurPeople += ResourceManager.Instance.TryAddCurPopulation(-cur);
        }

        UpdateEffectiveness();
        EventManager.TriggerEvent(ConstEvent.OnPopulaitionChange);
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
        mission.isAnd = true;
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

        switch (carMission.missionType)
        {
            case CarMissionType.requestResources:
                BuildRecievedCarMission(ref carMission);
                CarMission car = carMission;
                if (carMission != null)
                {
                    TrafficManager.Instance.UseCar(carMission);
                }

                break;
            case CarMissionType.transportResources:
                ResourceManager.Instance.AddResources(carMission.transportResources.ToArray());
                break;
            default:
                break;
        }
    }

    public void UpdateRate(string date)
    {
        UpdateEffectiveness();
        runtimeBuildData.Rate += runtimeBuildData.Effectiveness / 7f / runtimeBuildData.formula.ProductTime;
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
                runtimeBuildData.CurPeople += ResourceManager.Instance.TryAddCurPopulation(runtimeBuildData.Population + TechManager.Instance.PopulationBuff() - runtimeBuildData.CurPeople);
                EventManager.TriggerEvent(ConstEvent.OnPopulaitionChange);
            }
            //CheckCurPeopleMoreThanMax();
            runtimeBuildData.Pause = true;
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
            DestroyBuilding(false, false, false);
            MapManager.Instance.AddBuildingEntry(BuildingTools.GetInParkingGrid(this), buildingData);
            issuccess = true;
        }
        else
        {
            issuccess = false;
            NoticeManager.Instance.InvokeShowNotice("升级资源不足");
        }
    }

    private void BuildRecievedCarMission(ref CarMission carMission)
    {
        BuildingBase temp = MapManager.Instance.GetBuilidngByEntry(carMission.StartBuilding);
        if (temp == null)
        {
            carMission = null;
            return;
        }

        carMission.StartBuilding = parkingGridIn;
        carMission.EndBuilding = temp.parkingGridIn;
        //Debug.Log(carMission.StartBuilding);
        //Debug.Log(carMission.EndBuilding);W
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
                        carMission.requestResources.Add(new CostResource(foodlist[i],
                            carMission.requestResources[i].ItemNum));
                    }

                    carMission.requestResources.RemoveAt(0);
                }

                foreach (var request in carMission.requestResources)
                {
                    CostResource transport = ResourceManager.Instance.TryUseUpResource(request);
                    if (transport != null)
                    {
                        carMission.transportResources.Add(transport);
                        if (carMission.isAnd) continue;
                        else return;
                    }
                }

                EventManager.TriggerEvent(ConstEvent.OnRefreshResources);
                break;
            default:
                break;
        }
    }
}