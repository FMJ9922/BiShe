using System.Collections;
using System.Collections.Generic;
using Building;
using Tools;
using UnityEngine;

public class OrchardBuilding : BuildingBase
{
    [SerializeField] GameObject previewObj;
    public override void InitBuildingFunction()
    {
        previewObj.SetActive(false);
        base.InitBuildingFunction();
    }

    public override void RestartBuildingFunction()
    {
        previewObj.SetActive(false);
        base.RestartBuildingFunction();
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
            //MapManager.Instance.BuildFoundation(vector2Ints, 15, 0, false);
            //TerrainGenerator.Instance.FlatGround
            // (takenGrids, MapManager.GetTerrainPosition(parkingGridIn).y, false);
        }
    }

    protected override void Output()
    {
        base.Output();
    }

    protected override void Input()
    {
        base.Input();
    }

    public override void OnRecieveCar(CarMission carMission)
    {
        base.OnRecieveCar(carMission);
    }
}
