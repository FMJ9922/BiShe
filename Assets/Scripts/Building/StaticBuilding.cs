using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BuildingBase))]
public class StaticBuilding : MonoBehaviour
{
    public bool isFacingX = true;
    public int BuildID;
    BuildingBase currentBuilding;
    public static List<StaticBuilding> lists = new List<StaticBuilding>();
    private void Awake()
    {
        lists.Add(this);
    }
    public void SetGrids()
    {
        currentBuilding = transform.GetComponent<BuildingBase>();
        Vector2Int[] targetGrids = BuildManager.Instance.GetAllGrids(currentBuilding.Size.x, currentBuilding.Size.y, 
            currentBuilding.transform.position, isFacingX);
        MapManager.SetGridTypeToOccupy(targetGrids);
        currentBuilding.runtimeBuildData = BuildingBase.CastBuildDataToRuntime(DataManager.GetBuildData(BuildID));
        //Debug.Log(currentBuilding.transform.name);
        Vector2Int[] grids = BuildManager.Instance.GetAllGrids(currentBuilding.Size.x, currentBuilding.Size.y, transform.position, !isFacingX);
        currentBuilding.OnConfirmBuild(grids);
        //Debug.Log("Add:" + currentBuilding.parkingGrid);
        RoadManager.Instance.AddCrossNode(currentBuilding.parkingGrid, currentBuilding.direction);
    }
}
