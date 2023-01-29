using System.Collections;
using System.Collections.Generic;
using Building;
using Tools;
using UnityEngine;

public class PierBuilding : BuildingBase
{
    public Transform boatPos;

    public override void OnConfirmBuild(Vector2Int[] vector2Ints)
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
                Invoke("PlayAnim", 0.2f);
            }
            direction = CastTool.CastVector3ToDirection(transform.right);
            runtimeBuildData.Happiness = (80f + 10 * runtimeBuildData.CurLevel) / 100;
            Invoke("FillUpPopulation", 1f);
            InitBuildingFunction();
        }
        else
        {
            MapManager.SetGridTypeToOccupy(takenGrids);
            RestartBuildingFunction();
        }
    }


    protected override void Input()
    {
        base.Input();
    }

}
