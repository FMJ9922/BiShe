using System.Collections;
using System.Collections.Generic;
using Building;
using CSTools;
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
        lists.Add(this);
    }
    public void SetGrids()
    {
        currentBuilding = transform.GetComponent<BuildingBase>();
        Vector2Int[] targetGrids = BuildManager.Instance.GetAllGrids(currentBuilding.Size.x, currentBuilding.Size.y, 
            currentBuilding.transform.position, isFacingX);
        MapManager.SetGridTypeToOccupy(targetGrids);
        currentBuilding.runtimeBuildData = CastTool.CastBuildDataToRuntime(DataManager.GetBuildData(BuildID));
        transform.tag = "Building";
        (currentBuilding as IBuildingBasic)?.OnConfirmBuild(targetGrids);
    }
}
