using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmLandBuilding : BuildingBase
{
    public Color yellow;
    public Color green;
    [SerializeField] Transform wheatTrans;
    [SerializeField] GameObject previewObj;
    public List<GameObject> lists;
    bool isharvesting = false;

    private PlantController[] plants;

    public Texture wheat;
    public Texture rice;

    private Material mat;
    public override void InitBuildingFunction()
    {
        Invoke("InitPlant", 1f);
        previewObj.SetActive(false);
        base.InitBuildingFunction();
    }

    public override void RestartBuildingFunction()
    {
        base.RestartBuildingFunction();
        InitPlant();
        previewObj.SetActive(false);
        //Debug.Log(GetProcess());
        SetProgress(GetProcess());
    }

    public override void OnConfirmBuild(Vector2Int[] vector2Ints)
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
            direction = CastTool.CastVector3ToDirection(transform.right);
            runtimeBuildData.Happiness = (80f + 10 * runtimeBuildData.CurLevel) / 100;
            Invoke("FillUpPopulation", 1f);
            InitBuildingFunction();
            //地基
            MapManager.Instance.BuildFoundation(vector2Ints, 2, ((int)direction + 1) % 4);
            TerrainGenerator.Instance.FlatGround
            (takenGrids, MapManager.GetTerrainPosition(parkingGridIn).y);
        }
        else
        {
            RestartBuildingFunction();
            MapManager.Instance.BuildFoundation(vector2Ints, 2, ((int)direction + 1) % 4, false);
            //TerrainGenerator.Instance.FlatGround
            // (takenGrids, MapManager.GetTerrainPosition(parkingGridIn).y, false);
        }
    }

    private void ShowPlant()
    {
        for (int i = 0; i < plants.Length; i++)
        {
            plants[i].Show();
        }
    }

    private void InitPlant()
    {
        GameObject pfb = Instantiate(LoadAB.Load("mat.ab", "PlantPfb"), transform);
        pfb.transform.position -= Vector3.up * 2000f;
        plants = new PlantController[Size.x * Size.y];
        mat = pfb.GetComponent<PlantController>().mesh.material;
        Texture tex = runtimeBuildData.formulaDatas[runtimeBuildData.CurFormula].ID == 50006 ?
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

    public void SetProgress(float progress)
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

    protected override void Output()
    {
        if (formula == null)
        {
            Debug.Log("no");
            return;
        }

        if (!isharvesting)
        {
            runtimeBuildData.productTime--;
            if (runtimeBuildData.productTime <= 0)
            {
                isharvesting = true;

                CarMission mission = MakeHarvestCarMission();
                TrafficManager.Instance.UseCar(mission, out runtimeBuildData.AvaliableToMarket);
            }
        }
    }

    public override void OnRecieveCar(CarMission carMission)
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
                    //Debug.Log("harvest");
                    OnFinishHarvest(runtimeBuildData.Rate);
                    break;
                }
            default:
                break;
        }
    }
    public override void UpdateRate(string date)
    {
        //CheckCurPeopleMoreThanMax();
        UpdateEffectiveness();
        if (!isharvesting)
        {
            runtimeBuildData.Rate += runtimeBuildData.Effectiveness / 7f / formula.ProductTime;
            SetProgress(GetProcess());
        }
        //Debug.Log(runtimeBuildData.Rate);
    }
    public void OnFinishHarvest(float rate)
    {
        //Debug.Log("finish");
        isharvesting = false;
        runtimeBuildData.productTime = formula.ProductTime;
        CarMission carMission = MakeCarMission(rate);
        TrafficManager.Instance.UseCar(carMission, out runtimeBuildData.AvaliableToMarket);
        //EventManager.StartListening(ConstEvent.OnDayWentBy, Replant);
        Invoke("Replant", 5f);
    }

    private void Replant()
    {
        //EventManager.StopListening(ConstEvent.OnDayWentBy, Replant);
        runtimeBuildData.Rate = 0;
        ShowPlant();
    }
    /// <summary>
    /// 配置货物清单
    /// </summary>
    /// <param name="rate">生长比例</param>
    /// <returns></returns>
    protected override CarMission MakeCarMission(float rate)
    {
        //Debug.Log(rate);
        CarMission mission = new CarMission();
        mission.StartBuilding = parkingGridIn;
        mission.EndBuilding = MapManager.GetNearestMarket(parkingGridIn).GetComponent<BuildingBase>().parkingGridIn;
        mission.missionType = CarMissionType.transportResources;
        mission.isAnd = true;
        mission.transportResources = new List<CostResource>();
        mission.transportationType = TransportationType.mini;
        for (int i = 0; i < formula.OutputItemID.Count; i++)
        {
            //Debug.Log(formula.OutputItemID[i]);
            mission.transportResources.Add(new CostResource(formula.OutputItemID[i], rate * formula.ProductNum[i] * runtimeBuildData.Times));

        }
        return mission;
    }

    private CarMission MakeHarvestCarMission()
    {
        CarMission mission = new CarMission();
        mission.transportationType = TransportationType.harvester;
        mission.missionType = CarMissionType.harvest;

        Vector3[] vecs = new Vector3[lists.Count];
        for (int i = 0; i < lists.Count; i++)
        {
            vecs[i] = (lists[i].transform.position);
        }
        mission.EndBuilding = parkingGridIn;
        mission.wayPoints = Vector3Serializer.Box(vecs);
        return mission;
    }
    protected override void Input()
    {
        base.Input();
    }

}
