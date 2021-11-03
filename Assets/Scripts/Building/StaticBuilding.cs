using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BuildingBase))]
public class StaticBuilding : MonoBehaviour
{
    public bool isFacingX;
    public int BuildID;
    BuildingBase currentBuilding;
    public static List<StaticBuilding> lists = new List<StaticBuilding>();
    private void Start()
    {
        //Debug.Log("add");
        lists.Add(this);
    }
    public void SetGrids()
    {
        //Debug.Log("set");
        
        currentBuilding = transform.GetComponent<BuildingBase>();
        Vector2Int[] targetGrids = BuildManager.Instance.GetAllGrids(currentBuilding.Size.x, currentBuilding.Size.y, 
            currentBuilding.transform.position, isFacingX);
        MapManager.SetGridTypeToOccupy(targetGrids);
        currentBuilding.runtimeBuildData = BuildingBase.CastBuildDataToRuntime(DataManager.GetBuildData(BuildID));
        transform.tag = "Building";
        currentBuilding.OnConfirmBuild(targetGrids);
        //Vector3 targetPos = MapManager.GetTerrainPosition(currentBuilding.parkingGridIn);
        //float targetHeight = targetPos.y;
        //TerrainGenerator.Instance.FlatGround(currentBuilding.takenGrids, targetHeight);
        //Debug.Log("Add:" + currentBuilding.parkingGrid);
        //RoadManager.Instance.AddCrossNode(currentBuilding.parkingGridIn, currentBuilding.direction); 
        //RoadManager.Instance.AddCrossNode(currentBuilding.parkingGridOut, currentBuilding.direction);
        //RoadManager.Instance.AddTurnNode(currentBuilding.parkingGridIn, currentBuilding.parkingGridOut);
    }
}
