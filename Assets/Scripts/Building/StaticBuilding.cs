using System;
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
    public int InitIndex;
    public static StaticBuilding[] lists;
    private void Start()
    {
        if (lists == null)
        {
            lists = new StaticBuilding[10];
        }
        lists[InitIndex] = this;
    }
    public void SetGrids()
    {
        //Debug.Log(gameObject.name);
        currentBuilding = transform.GetComponent<BuildingBase>();
        Vector2Int[] targetGrids = BuildManager.Instance.GetAllGrids(currentBuilding.Size.x, currentBuilding.Size.y, 
            currentBuilding.transform.position, isFacingX);
        MapManager.SetGridTypeToOccupy(targetGrids);
        currentBuilding.runtimeBuildData = CastTool.CastBuildDataToRuntime(DataManager.GetBuildData(BuildID));
        transform.tag = "Building";
        (currentBuilding as IBuildingBasic)?.OnConfirmBuild(targetGrids);
    }

    private void OnDestroy()
    {
        //Debug.Log("destroy"+gameObject.name);
    }
}
