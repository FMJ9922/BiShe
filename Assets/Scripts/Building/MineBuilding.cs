using System.Collections;
using System.Collections.Generic;
using Building;
using Tools;
using UnityEngine;

public class MineBuilding : BuildingBase
{
    public Transform digPos;
    public float richness = 1;//资源丰度


    public override void OnConfirmBuild(Vector2Int[] vector2Ints)
    {
        takenGrids = vector2Ints;
        gameObject.tag = "Building";
        transform.GetComponent<BoxCollider>().enabled = false;
        transform.GetComponent<BoxCollider>().enabled = true;
        //地基
        //MapManager.Instance.BuildFoundation(vector2Ints, 15);
        //整平地面
        
        richness = SetRichness(takenGrids);
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
            TerrainGenerator.Instance.FlatGround
            (takenGrids, MapManager.GetTerrainPosition(parkingGridIn).y);
        }
        else
        {
            MapManager.SetGridTypeToOccupy(takenGrids);
            RestartBuildingFunction();
            //TerrainGenerator.Instance.FlatGround
            // (takenGrids, MapManager.GetTerrainPosition(parkingGridIn).y, false);
        }
    }


    public float SetRichness(Vector2Int[] takenGrids)
    {
        float sum = 0;
        float total = takenGrids.Length;
        for (int i = 0; i < takenGrids.Length; i++)
        {
            sum += MapManager.GetMineRichness(takenGrids[i]);
        }
        return Mathf.Clamp01(sum / total*3);
    }
    protected override void Input()
    {
        base.Input();
        DigGround();
    }

    private void DigGround()
    {
        float height = MapManager.GetTerrainPosition(digPos.position).y;
        Vector2Int[] grids = BuildManager.Instance.GetAllGrids(5, 5, digPos.position, false);
        TerrainGenerator.Instance.FlatGround(grids,height-0.1f);
    }

    public override void DestroyBuilding(bool returnResources, bool returnPopulation, bool repaint = true)
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
            //MapManager.Instance.BuildOriginFoundation(takenGrids);
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
}
