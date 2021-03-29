using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BuildManager : Singleton<BuildManager>
{
    #region
    //[SerializeField]
    //private GameObject gridHightLight;
    [SerializeField]
    private Material mat_road_green;
    [SerializeField]
    private Material mat_road_red;
    [SerializeField]
    private TerrainGenerator terrainGenerator;
    [SerializeField]
    private GameObject preRoadPfb;

    //修建筑相关
    private BuildingBase currentBuilding;
    private bool isCurCanBuild = false;//当前建筑是否重叠
    private bool isTurn = false;//当前建筑是否旋转
    private Vector2Int[] targetGrids;
    private Vector2Int lastGrid;
    private Material[] mats;

    //修路相关
    private Vector3 roadStartPos;//道路建造起始点
    private Vector3 roadEndPos;//道路建造终点
    private List<GameObject> preRoads = new List<GameObject>();
    private Direction roadDirection = Direction.right;
    private int roadCount = 0;
    private int roadLevel = 1;//道路等级

    //事件相关
    private UnityAction<Vector3> moveAc = (Vector3 p) => Instance.OnMouseMoveSetBuildingPos(p);
    private UnityAction confirmAc = () => Instance.OnConfirmBuild();
    private UnityAction cancelAc = () => Instance.OnCancelBuild();

    public static bool IsInBuildMode { get; set; }
    #endregion


    #region 公共函数

    public void InitBuildManager()
    {
        LoadAB.Init();
        //ShowGrid(false);
        IsInBuildMode = false;
    }
    public void BuildTest()
    {

    }

    /// <summary>
    /// 盖建筑
    /// </summary>
    public void CreateBuildingOnMouse(BuildData buildData)
    {
        GameObject building = InitBuilding(buildData);
        building.transform.position = Input.mousePosition;
        var meshRenderers = building.transform.GetComponentsInChildren<MeshRenderer>();
        mats = new Material[meshRenderers.Length];
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            mats[i] = meshRenderers[i].material;
        }
        WhenStartBuild();
    }
    public GameObject InitBuilding(BuildData buildData)
    {
        string bundleName = buildData.BundleName;
        string pfbName = buildData.PfbName;
        Debug.Log("load:" + bundleName + " " + pfbName);
        GameObject pfb = LoadAB.Load(bundleName, pfbName);
        GameObject building = Instantiate(pfb, transform);
        currentBuilding = building.GetComponent<BuildingBase>();
        currentBuilding.runtimeBuildData = BuildingBase.CastBuildDataToRuntime(buildData);
        return building;
    }

    /// <summary>
    /// 开始修路
    /// </summary>
    public void StartCreateRoads(int _roadLevel = 1)
    {
        EventManager.StartListening(ConstEvent.OnMouseLeftButtonDown, OnConfirmRoadStartPos);
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown, OnCancelBuildRoad);
        roadLevel = _roadLevel;
    }

    #endregion

    #region 私有函数
    private bool CheckRoadStartPosAvalible()
    {
        Vector2Int startGrid = GetCenterGrid(roadStartPos);
        if (MapManager.Instance.GetGridType(startGrid)==GridType.road)
        {
            return true;
        }
        Debug.Log("应与已有道路相连");
        return false;
    }
    /// <summary>
    /// 点下起点
    /// </summary>
    private void OnConfirmRoadStartPos()
    {
        roadStartPos = CalculateCenterPos(InputManager.Instance.LastGroundRayPos, Vector2Int.one);
        if (CheckRoadStartPosAvalible())
        {
            EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, OnConfirmRoadStartPos);
            EventManager.StartListening(ConstEvent.OnMouseLeftButtonDown, OnConfirmRoadEndPos);
            EventManager.StartListening<Vector3>(ConstEvent.OnGroundRayPosMove, OnPreShowRoad);

        }
    }

    /// <summary>
    /// 点下终点
    /// </summary>
    private void OnConfirmRoadEndPos()
    {
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, OnConfirmRoadEndPos);
        EventManager.StopListening<Vector3>(ConstEvent.OnGroundRayPosMove, OnPreShowRoad);
        Vector2 vec = RectTransformUtility.WorldToScreenPoint(Camera.main, InputManager.Instance.LastGroundRayPos);
        EventManager.TriggerEvent<Vector2>(ConstEvent.OnBuildToBeConfirmed,vec);
        //ChangeRoadCount(0);

    }

    /// <summary>
    /// 确认修建
    /// </summary>
    public void OnConfirmBuildRoad()
    {
        List<Vector2Int> grids = new List<Vector2Int>();
        int dir = 0;
        Vector3 adjust = Vector3.zero;
        switch (roadDirection)
        {
            case Direction.down:
                dir = 1;
                adjust += new Vector3(0, 0, -2);
                break;
            case Direction.up:
                dir = 1;
                break;
            case Direction.right:
                dir = 2;
                break;
            case Direction.left:
                adjust += new Vector3(-2, 0, 0);
                dir = 2;
                break;
        }
        Vector3 delta = CastTool.CastDirectionToVector(dir);
        //Debug.Log(delta.ToString());
        for (int i = 0; i < preRoads.Count; i++)
        {
            grids.Add(GetCenterGrid(preRoads[i].transform.position + adjust));
            grids.Add(GetCenterGrid(preRoads[i].transform.position - delta + adjust));
        }
        MapManager.Instance.GenerateRoad(grids.ToArray(),roadLevel);
        ChangeRoadCount(0);
        EventManager.TriggerEvent(ConstEvent.OnFinishBuilding);
    }

    /// <summary>
    /// 取消修建
    /// </summary>
    public void OnCancelBuildRoad()
    {
        EventManager.TriggerEvent(ConstEvent.OnFinishBuilding);
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, OnConfirmRoadEndPos);
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, OnConfirmRoadStartPos);
        EventManager.StopListening(ConstEvent.OnMouseRightButtonDown, OnCancelBuildRoad);
        EventManager.StopListening<Vector3>(ConstEvent.OnGroundRayPosMove, OnPreShowRoad);
        ChangeRoadCount(0);
    }
    /// <summary>
    /// 修路时显示预计建造的路
    /// </summary>
    /// <param name="pos"></param>
    private void OnPreShowRoad(Vector3 pos)
    {
        Vector3 vector = pos - roadStartPos;
        Direction dir;
        int count;
        CalculateLongestDir(vector, out dir, out count);
        if (dir != roadDirection)
        {
            ChangeRoadDirection(dir);
        }
        if (count != roadCount)
        {
            try
            {
                ChangeRoadCount(count);
            }
            catch
            {
                Debug.Log(count);
            }
        }
    }


    /// <summary>
    /// 修改路的长度
    /// </summary>
    private void ChangeRoadCount(int newCount)
    {
        Vector3 extensionDir = CastTool.CastDirectionToVector(roadDirection);
        if (newCount > roadCount)
        {
            for (int i = roadCount; i < newCount; i++)
            {
                GameObject newRoad = Instantiate(preRoadPfb, transform);
                newRoad.name = i.ToString();
                newRoad.transform.position = MapManager.Instance.GetTerrainPosition(roadStartPos + extensionDir * i) + new Vector3(0, 0.01f, 0);
                newRoad.transform.LookAt(MapManager.Instance.GetTerrainPosition(roadStartPos + extensionDir * (i + 1)) + new Vector3(0, 0.01f, 0));
                newRoad.GetComponent<RoadPreview>().SetRoadPreviewMat(roadLevel);
                preRoads.Add(newRoad);
            }
        }
        else if (newCount == roadCount)
        {
            return;
        }
        else
        {
            for (int i = newCount; i < roadCount; i++)
            {
                GameObject deleteRoad = preRoads[preRoads.Count - 1];
                preRoads.RemoveAt(preRoads.Count - 1);
                Destroy(deleteRoad);
            }
        }
        roadCount = newCount;
        roadEndPos = MapManager.Instance.GetTerrainPosition(roadStartPos + extensionDir * roadCount);
    }

    /// <summary>
    /// 修改路的延伸方向
    /// </summary>
    private void ChangeRoadDirection(Direction direction)
    {
        roadDirection = direction;
        Vector3 extensionDir = CastTool.CastDirectionToVector(roadDirection);
        for (int i = 0; i < preRoads.Count; i++)
        {
            preRoads[i].transform.position = MapManager.Instance.GetTerrainPosition(roadStartPos + extensionDir * i) + new Vector3(0, 0.01f, 0);
            preRoads[i].transform.LookAt(MapManager.Instance.GetTerrainPosition(roadStartPos + extensionDir * (i + 1)) + new Vector3(0, 0.01f, 0));
        }
    }

    /// <summary>
    /// 获得用户鼠标输入的最长的道路延伸方向
    /// </summary>
    /// <param name="input"></param>
    /// <param name="dir"></param>
    /// <param name="count"></param>
    private void CalculateLongestDir(Vector3 input, out Direction dir, out int count)
    {
        input += new Vector3(0.5f, 0f, 0.5f);
        int n;
        if (Mathf.Abs(input.x) >= Mathf.Abs(input.z))
        {
            dir = input.x >= 0 ? Direction.right : Direction.left;
            n = Mathf.CeilToInt(Mathf.Abs(input.x) / MapManager.unit);
        }
        else
        {
            dir = input.z >= 0 ? Direction.up : Direction.down;
            n = Mathf.CeilToInt(Mathf.Abs(input.z) / MapManager.unit);
        }
        count = n > 1 ? n : 1;
    }


    private void OnMouseMoveSetBuildingPos(Vector3 p)
    {
        currentBuilding.transform.position = CalculateCenterPos(p, currentBuilding.Size, isTurn);
        //gridHightLight.transform.position = CalculateCenterPos(p, Vector2Int.zero) + new Vector3(0, 0.02f, 0);
        CheckOverlap();
    }

    private void OnRotateBuilding(Direction direction)
    {
        currentBuilding.transform.rotation = Quaternion.LookRotation(CastTool.CastDirectionToVector((int)direction+1), Vector3.up);
        if(direction == Direction.down || direction == Direction.up)
        {
            isTurn = false;
        }
        else
        {
            isTurn = true;
        }
        currentBuilding.transform.position = CalculateCenterPos(InputManager.Instance.LastGroundRayPos, currentBuilding.Size, isTurn);
        //gridHightLight.transform.position = CalculateCenterPos(InputManager.Instance.LastGroundRayPos, Vector2Int.zero) + new Vector3(0, 0.02f, 0);
    }

    private void CheckOverlap()
    {
        Vector3 curPos = currentBuilding.transform.position;
        int width, height;
        targetGrids = GetAllGrids(currentBuilding.Size.x, currentBuilding.Size.y, curPos,out width,out height);
        isCurCanBuild = MapManager.CheckCanBuild(targetGrids, width, height, out Direction direction);
        OnRotateBuilding(direction);
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i].color = isCurCanBuild ? Color.green : Color.red;
        }
        //gridHightLight.GetComponent<MeshRenderer>().material = isCurOverlap ? mat_grid_red : mat_grid_green;
    }
    private void OnConfirmBuild()
    {
        CheckOverlap();
        if (!isCurCanBuild)
        {
            Debug.Log("当前建筑重叠，无法建造！");
            return;
        }
        if (CheckBuildResourcesEnoughAndUse())
        {
            currentBuilding.OnConfirmBuild(targetGrids);
            MapManager.SetGridTypeToOccupy(targetGrids);
            terrainGenerator.OnFlatGround(currentBuilding.transform.position, 3, currentBuilding.transform.position.y);
            //for (int i = 0; i < targetGrids.Length; i++)
            //{
            //    Debug.Log(MapManager.Instance.GetTerrainPosition(targetGrids[i]));
            //}
            //MapManager.Instance.ShowGrid(targetGrids);
            WhenFinishBuild();
        }
    }

    public void UpgradeBuilding(BuildData buildData,Vector3 pos,Quaternion quaternion)
    {
        GameObject building = InitBuilding(buildData);
        building.transform.position = pos;
        building.transform.rotation = quaternion;
        if (CheckBuildResourcesEnoughAndUse())
        {
            currentBuilding.OnConfirmBuild(targetGrids);
            //MapManager.SetGridTypeToOccupy(targetGrids);
            //terrainGenerator.OnFlatGround(currentBuilding.transform.position, 3, currentBuilding.transform.position.y);

        }
        else
        {
            Destroy(building);
        }
    }

    /// <summary>
    /// 检查建造所需的资源是否足够，如果足够就使用掉，不足够就返回false
    /// </summary>
    private bool CheckBuildResourcesEnoughAndUse()
    {
        List<CostResource> rescources = currentBuilding.runtimeBuildData.costResources;
        for (int i = 0; i < rescources.Count; i++)
        {
            if (!ResourceManager.Instance.TryUseResource(rescources[i]))
            {
                return false;
            }
        }
        return true;

    }
    private void OnCancelBuild()
    {
        Destroy(currentBuilding.gameObject);
        WhenFinishBuild();
    }

    private void WhenStartBuild()
    {
        //ShowGrid(true);
        isTurn = false;
        isCurCanBuild = false;
        IsInBuildMode = true;
        EventManager.StartListening(ConstEvent.OnGroundRayPosMove, moveAc);
        EventManager.StartListening(ConstEvent.OnMouseLeftButtonDown, confirmAc);
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown, cancelAc);
        //EventManager.StartListening(ConstEvent.OnRotateBuilding, rotateAc);
        GameManager.Instance.TogglePauseGame();
    }
    private void WhenFinishBuild()
    {
        //ShowGrid(false);
        GameManager.Instance.TogglePauseGame();
        EventManager.StopListening(ConstEvent.OnGroundRayPosMove, moveAc);
        //EventManager.StopListening(ConstEvent.OnRotateBuilding, rotateAc);
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, confirmAc);
        EventManager.StopListening(ConstEvent.OnMouseRightButtonDown, cancelAc);
        EventManager.TriggerEvent(ConstEvent.OnFinishBuilding);
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i].color = Color.white;
        }
        currentBuilding = null;
        targetGrids = null;
        IsInBuildMode = false;
    }
    //private void ShowGrid(bool isShow)
    //{
    //    gridHightLight.SetActive(isShow);
    //}

    /// <summary>
    /// 对齐网格
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    private Vector3 CalculateCenterPos(Vector3 pos, Vector2Int size, bool isExchange = false)
    {
        Vector2Int vector2Int = size;
        if (isExchange)
        {
            vector2Int = new Vector2Int(size.y, size.x);
        }

        Vector3 newPos = pos;
        if (vector2Int.x % 2 != 0)
        {
            newPos.x = Mathf.Floor(pos.x / 2) * 2 + 1f;
        }
        else
        {
            newPos.x = Mathf.Floor(pos.x / 2) * 2;
        }
        if (vector2Int.y % 2 != 0)
        {
            newPos.z = Mathf.Floor(pos.z / 2) * 2 + 1f;
        }
        else
        {
            newPos.z = Mathf.Floor(pos.z / 2) * 2;
        }
        return newPos;
    }
    private Vector2Int GetCenterGrid(Vector3 centerPos)
    {
        Vector3 centerGrid = centerPos / 2;
        int x = Mathf.FloorToInt(centerGrid.x);
        int z = Mathf.FloorToInt(centerGrid.z);
        return new Vector2Int(x, z);
    }
    /// <summary>
    /// 获取当前待造建筑所占用的所有格子
    /// </summary>
    /// <returns></returns>
    private Vector2Int[] GetAllGrids(int sizeX, int sizeY, Vector3 centerPos,out int width,out int height)
    {
        int startX, endX, startZ, endZ;
        width = isTurn ? sizeY : sizeX;
        height = isTurn ? sizeX : sizeY;
        Vector3 centerGrid = centerPos / 2;
        if (width % 2 == 0)
        {
            startX = Mathf.FloorToInt(centerGrid.x) - width / 2 + 1;
            endX = Mathf.FloorToInt(centerGrid.x) + width / 2;
        }
        else
        {
            startX = Mathf.RoundToInt(centerGrid.x) - (width - 1) / 2;
            endX = Mathf.RoundToInt(centerGrid.x) + (width - 1) / 2;
        }
        if (height % 2 == 0)
        {
            startZ = Mathf.FloorToInt(centerGrid.z) - height / 2 + 1;
            endZ = Mathf.FloorToInt(centerGrid.z) + height / 2;
        }
        else
        {
            startZ = Mathf.RoundToInt(centerGrid.z) - (height - 1) / 2;
            endZ = Mathf.RoundToInt(centerGrid.z) + (height - 1) / 2;
        }


        Vector2Int[] grids = new Vector2Int[width * height];
        int index = 0;
        for (int i = startX; i <= endX; i++)
        {
            for (int j = startZ; j <= endZ; j++)
            {
                grids[index] = new Vector2Int(i-1, j-1);
                index++;
            }
        }
        return grids;
    }

    #endregion
}

