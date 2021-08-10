using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageBuilding : BuildingBase
{
    public float Max;

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
        }
        else
        {
            RestartBuildingFunction();
            MapManager.Instance.BuildFoundation(vector2Ints, 15, 0, false);
        }

        //地基
        MapManager.Instance.BuildFoundation(vector2Ints, 15);
        //整平地面
        Vector3 targetPos = MapManager.GetTerrainPosition(parkingGridIn);
        float targetHeight = targetPos.y;
        TerrainGenerator.Instance.FlatGround(takenGrids, targetHeight);
        ResourceManager.Instance.AddMaxStorage(Max);
    }

    protected override void OnDestroy()
    {
        EventManager.StopListening(ConstEvent.OnOutputResources, Output);
        EventManager.StopListening(ConstEvent.OnInputResources, Input);
        EventManager.StopListening<string>(ConstEvent.OnDayWentBy, UpdateRate);
        if (ResourceManager.Instance)
        {
            ResourceManager.Instance.AddMaxStorage(-Max);
        }
    }

    protected override void Input()
    {
        ResourceManager.Instance.AddResource(99999, -runtimeBuildData.CostPerWeek);
        runtimeBuildData.Pause = false;
        if (runtimeBuildData.CurPeople < runtimeBuildData.Population)
        {
            FillUpPopulation();
        }
    }

    protected override void Output()
    {

    }
}
