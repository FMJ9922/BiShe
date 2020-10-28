using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BuildManager : Singleton<BuildManager>
{
    #region
    
    #endregion

    #region 公共函数
    /// <summary>
    /// 盖建筑
    /// </summary>
    public void CreateBuildingOnMouse()
    {
        GameObject pfb = LoadAB.Load("buildings.1", "House01");
        GameObject building = Instantiate(pfb, transform);
        building.transform.position = Input.mousePosition;
        UnityAction<Vector3> moveAc = (Vector3 p) => OnMouseMove(building, p);
        UnityAction<float> rotateAc = (float dir) => OnRotateBuilding(building, dir);
        UnityAction stopAc = () => OnConfirmBuild(moveAc, rotateAc, building);
        EventManager.StartListening(ConstEvent.OnGroundRayPosMove, moveAc);
        EventManager.StartListening(ConstEvent.OnMouseLeftButtonDown, stopAc);
        EventManager.StartListening(ConstEvent.OnRotateBuilding, rotateAc);
    }

    /// <summary>
    /// 修路
    /// </summary>
    public void CreateRoads()
    {
        RoadManager.Instance.ResetRoad();
        EventManager.StartListening(ConstEvent.OnMouseLeftButtonDown, RoadManager.Instance.BuildRoads);
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown, OnConfirmBuildRoad);
    }

    #endregion

    #region 私有函数

    private void OnConfirmBuildRoad()
    {
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, RoadManager.Instance.BuildRoads);
        EventManager.StopListening(ConstEvent.OnMouseRightButtonDown, OnConfirmBuildRoad);
    }

    private void OnCancelLastRoad()
    {
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, RoadManager.Instance.BuildRoads);
    }

    private void OnMouseMove(GameObject building, Vector3 p)
    {
        building.transform.position = p;
    }
    private void OnConfirmBuild(UnityAction<Vector3> moveAc, UnityAction<float> rotateAc, GameObject building)
    {
        EventManager.StopListening(ConstEvent.OnGroundRayPosMove, moveAc);
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, () => OnConfirmBuild(moveAc, rotateAc, building));
        EventManager.StopListening(ConstEvent.OnRotateBuilding, rotateAc);
    }

    private void OnRotateBuilding(GameObject building, float dir)
    {
        building.transform.Rotate(Vector3.up, dir, Space.World);
    }
    #endregion
}

