using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BuildManager : Singleton<BuildManager>
{
    #region
    [SerializeField]
    private GameObject gridHightLight;
    [SerializeField]
    private Material mat_grid_green;
    [SerializeField]
    private Material mat_grid_red;

    private BuildingBase currentBuilding;
    private AreaInfo currentAreaInfo;
    private bool isCurOverLap = false;//当前建筑是否重叠
    private List<AreaInfo> areaInfos = new List<AreaInfo>();

    private UnityAction<Vector3> moveAc = (Vector3 p) => Instance.OnMouseMove(p);
    private UnityAction<float> rotateAc = (float dir) => Instance.OnRotateBuilding(dir);
    private UnityAction confirmAc = () => Instance.OnConfirmBuild();
    private UnityAction cancelAc = () => Instance.OnCancelBuild();
    #endregion

    private void Start()
    {
        LoadAB.Init();
        ShowGrid(false);
    }
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
        Debug.Log("load:" + bundleName + " " + pfbName);
        GameObject pfb = LoadAB.Load(bundleName, pfbName);
        GameObject building = Instantiate(pfb, transform);
        currentBuilding = building.GetComponent<BuildingBase>();
        building.transform.position = Input.mousePosition;
        currentAreaInfo = new AreaInfo(building.transform.position.x, 
            building.transform.position.z, currentBuilding.Size,false);
        WhenStartBuild();
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
        gridHightLight.transform.position = CalculateCenterPos(p, currentBuilding.Size) + new Vector3(0, 0.02f, 0);
        Vector3 curPos = currentBuilding.transform.position;
        currentAreaInfo = new AreaInfo(curPos.x, curPos.z, currentBuilding.Size,currentAreaInfo.isExchangeFlag);
        isCurOverLap = CheckNewBuildingIsOverLap(currentAreaInfo) ? true : false;
        gridHightLight.GetComponent<MeshRenderer>().material = isCurOverLap ? mat_grid_red : mat_grid_green;
    }

    private void OnRotateBuilding(float dir)
    {
        currentBuilding.transform.Rotate(Vector3.up, dir, Space.World);
        currentAreaInfo.ExchangeWidthHeight();
        isCurOverLap = CheckNewBuildingIsOverLap(currentAreaInfo) ? true : false;
        gridHightLight.GetComponent<MeshRenderer>().material = isCurOverLap ? mat_grid_red : mat_grid_green;
    }
    private void OnConfirmBuild()
    {
        if (!isCurOverLap)
        {
            areaInfos.Add(currentAreaInfo);
            Debug.Log(currentAreaInfo.ToString());
            currentBuilding.OnConfirmBuild();
            WhenFinishBuild();
        }
        else
        {
            Debug.Log("当前建筑重叠，无法建造！");
        }
    }

    private void OnCancelBuild()
    {
        Destroy(currentBuilding.gameObject);
        WhenFinishBuild();
    }

    private void WhenStartBuild()
    {
        ShowGrid(true);
        isCurOverLap = false;
        EventManager.StartListening(ConstEvent.OnGroundRayPosMove, moveAc);
        EventManager.StartListening(ConstEvent.OnMouseLeftButtonDown, confirmAc);
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown, cancelAc);
        EventManager.StartListening(ConstEvent.OnRotateBuilding, rotateAc);
    }
    private void WhenFinishBuild()
    {
        ShowGrid(false);
        EventManager.StopListening(ConstEvent.OnGroundRayPosMove, moveAc);
        EventManager.StopListening(ConstEvent.OnRotateBuilding, rotateAc);
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, confirmAc);
        EventManager.StopListening(ConstEvent.OnMouseRightButtonDown, cancelAc);
        EventManager.TriggerEvent(ConstEvent.OnFinishBuilding);
        currentBuilding = null;
    }
    private void ShowGrid(bool isShow)
    {
        gridHightLight.SetActive(isShow);
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


    private static bool IsOverLap(AreaInfo rc1, AreaInfo rc2)
    {
        if (Mathf.Abs(rc1.x - rc2.x) > (rc1.Width + rc2.Width) / 2)
        {
            return false;
        }
        if(Mathf.Abs(rc1.y - rc2.y) > (rc1.Height + rc2.Height) / 2)
        {
            return false;
        }
        return true;
    }

    private bool CheckNewBuildingIsOverLap(AreaInfo areaInfo)
    {
        //Debug.Log("origin:"+areaInfo.ToString());
        foreach (AreaInfo info in areaInfos)
        {
            //Debug.Log("compare:" + info.ToString());
            if (IsOverLap(areaInfo, info))
            {
                return true;
            }
        }
        return false;
    }
    #endregion
}

public struct AreaInfo
{
    public float x;
    public float y;
    public int Height { get { return isExchangeFlag ? width : height; } }
    public int Width { get { return isExchangeFlag ? height :width ; } }
    private int height;
    private int width;
    public bool isExchangeFlag;//长宽是否交换

    public AreaInfo(float _x, float _y, Vector2Int _size,bool _isExchange)
    {
        x = _x;
        y = _y;
        width = _size.y;
        height = _size.x;
        isExchangeFlag = _isExchange;
    }

    public AreaInfo(float _x, float _y, Vector2Int _size)
    {
        x = _x;
        y = _y;
        width = _size.x;
        height = _size.y;
        isExchangeFlag = false;
    }
    public override string ToString()
    {
        return string.Format("Pos:({0},{1})Size:({2},{3})Exchange:{4}", 
                                x, y, width, height,isExchangeFlag.ToString());
    }

    public void ExchangeWidthHeight()
    {
        isExchangeFlag = !isExchangeFlag;
    }
}


