using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BuildManager : Singleton<BuildManager>
{
    #region

    private BuildingBase currentBuilding;

    private UnityAction<Vector3> moveAc = (Vector3 p) => Instance.OnMouseMove(p);
    private UnityAction<float> rotateAc = (float dir) => Instance.OnRotateBuilding(dir);
    private UnityAction confirmAc = () => Instance.OnConfirmBuild();
    private UnityAction cancelAc = () => Instance.OnCancelBuild();
    #endregion

    #region 公共函数
    public void BuildTest()
    {
        CreateBuildingOnMouse("building.l1_northhouse", "L1_Northhouse001");
    }

    /// <summary>
    /// 盖建筑
    /// </summary>
    public void CreateBuildingOnMouse(string bundleName, string pfbName)
    {
        GameObject pfb = LoadAB.Load(bundleName, pfbName);
        GameObject building = Instantiate(pfb, transform);
        currentBuilding = building.GetComponent<BuildingBase>();
        building.transform.position = Input.mousePosition;
        EventManager.StartListening(ConstEvent.OnGroundRayPosMove, moveAc);
        EventManager.StartListening(ConstEvent.OnMouseLeftButtonDown, confirmAc);
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown, cancelAc);
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

    private void OnMouseMove(Vector3 p)
    {
        currentBuilding.transform.position = CalculateCenterPos(p, currentBuilding.Size);
    }

    private void OnRotateBuilding(float dir)
    {
        currentBuilding.transform.Rotate(Vector3.up, dir, Space.World);
    }
    private void OnConfirmBuild()
    {
        currentBuilding.OnConfirmBuild();
        EventManager.StopListening(ConstEvent.OnGroundRayPosMove, moveAc);
        EventManager.StopListening(ConstEvent.OnRotateBuilding, rotateAc);
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, confirmAc);
        EventManager.StopListening(ConstEvent.OnMouseRightButtonDown, cancelAc);
        EventManager.TriggerEvent(ConstEvent.OnFinishBuilding);
        currentBuilding = null;
    }

    private void OnCancelBuild()
    {
        
        EventManager.StopListening(ConstEvent.OnGroundRayPosMove, moveAc);
        EventManager.StopListening(ConstEvent.OnRotateBuilding, rotateAc);
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, confirmAc);
        EventManager.StopListening(ConstEvent.OnMouseRightButtonDown, cancelAc);
        EventManager.TriggerEvent(ConstEvent.OnFinishBuilding);
        Destroy(currentBuilding.gameObject);
        currentBuilding = null;
    }



    /// <summary>
    /// 对齐网格
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    private Vector3 CalculateCenterPos(Vector3 pos, Vector2Int size)
    {
        Vector3 newPos = pos;
        if (size.x % 2 == 0)
        {
            newPos.x = Mathf.Round(pos.x);
        }
        else
        {
            newPos.x = Mathf.Round(pos.x + 0.5f) - 0.5f;
        }
        if (size.y % 2 == 0)
        {
            newPos.z = Mathf.Round(pos.z);
        }
        else
        {
            newPos.z = Mathf.Round(pos.z + 0.5f) - 0.5f;
        }
        return newPos;
    }
    #endregion
}

