using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : Singleton<MapManager>
{
    public Vector2Int MapSize { get; private set; }
    private Dictionary<Vector2Int, SingleGrid> _gridDic = new Dictionary<Vector2Int, SingleGrid>();
    private LevelData _leveldata;
    private static Vector3[] _vertices;//存储地形顶点数据
    public const int unit = 2;//地形一格的长度（单位米）
    public List<BuildingBase> _buildings = new List<BuildingBase>();
    [SerializeField] private static TerrainGenerator generator;
    //[SerializeField] GameObject gridPfb;
    public static string noticeContent;


    public void InitMapMnager(int levelId)
    {
        InitLevelData(levelId);
        InitGrid();
    }
    /// <summary>
    /// 加载关卡的时候调用
    /// </summary>
    private void InitLevelData(int levelId)
    {
        _leveldata = DataManager.GetLevelData(levelId);
        MapSize = new Vector2Int(_leveldata.Length, _leveldata.Width);
        _vertices = TerrainGenerator.GetTerrainMeshVertices();
        generator = GameObject.Find("TerrainGenerator").GetComponent<TerrainGenerator>();
    }
    private void InitGrid()
    {
        if (MapSize == null || MapSize.x <= 0 || MapSize.y <= 0)
        {
            Debug.LogError("地图尺寸没有初始化！");
            return;
        }
        for (int i = 0; i < MapSize.x; i++)
        {
            for (int j = 0; j < MapSize.y; j++)
            {
                _gridDic.Add(new Vector2Int(i, j), new SingleGrid(i, j, GridType.empty));
            }
        }
        MapData mapData = TerrainGenerator.Instance.GetMapData();
        for (int i = 0; i < mapData.roadGrids.Count; i++)
        {
            SetGridTypeToRoad(mapData.roadGrids[i].Vector2Int);
            //Debug.Log(mapData.roadGrids[i].Vector2Int);
        }
        Debug.Log("初始化中");
        Invoke("InitRoad",0.017f);
    }

    public void InitRoad()
    {
        List<StaticBuilding> lists = StaticBuilding.lists;
        RoadManager.Instance.InitRoadManager();
        SetGrid(lists);
    }

    void SetGrid(List<StaticBuilding> lists)
    {
        for (int i = 0; i < lists.Count; i++)
        {
            lists[i].SetGrids();
        }
        Debug.Log("地图已初始化！");
    }

    public List<Vector2Int> GetAllRoadGrid()
    {
        List<Vector2Int> res = new List<Vector2Int>();
        foreach(var item in _gridDic)
        {
            if(item.Value.GridType == GridType.road)
            {
                res.Add(item.Key);
            }
        }
        return res;
    }

    /// <summary>
    /// 为建筑设置起点
    /// </summary>
    public void SetBuildingsGrid()
    {
        for (int i = 0; i < _buildings.Count; i++)
        {
            BuildingBase currentBuilding = _buildings[i];
            RoadManager.Instance.AddCrossNode(currentBuilding.parkingGridIn, currentBuilding.direction);
        }
    }
    /// <summary>
    /// 刷地基
    /// </summary>
    public void BuildFoundation(Vector2Int[] takenGirds, int tex, int dir = 0)
    {
        for (int i = 0; i < takenGirds.Length; i++)
        {
            generator.RefreshUV(tex, 8, takenGirds[i].x + takenGirds[i].y * MapSize.x, dir);
        }
        generator.ReCalculateNormal();
    }
    public void BuildOutCornerRoad(int level, int index, Direction direction)
    {
        generator.RefreshUV(12 - level * 4 + 2, 8, index, (int)direction);
    }
    public void BuildInCornerRoad(int level, int index, Direction direction)
    {
        generator.RefreshUV(12 - level * 4 + 1, 8, index, (int)direction);
    }
    public void BuildStraightRoad(int level, int index, Direction direction)
    {
        generator.RefreshUV(12 - level * 4, 8, index, (int)direction);
    }
    public void GenerateRoad(Vector2Int[] roadGrid, int level = 1)
    {
        for (int i = 0; i < roadGrid.Length; i++)
        {
            SetGridTypeToRoad(roadGrid[i]);
        }
        for (int i = 0; i < roadGrid.Length; i++)
        {
            RoadOption roadOption;
            Direction direction;
            GetRoadTypeAndDir(roadGrid[i], out roadOption, out direction);
            SetGridType(roadGrid[i], GridType.road);
            switch (roadOption)
            {
                case RoadOption.straight:
                    BuildStraightRoad(level - 1, roadGrid[i].x + roadGrid[i].y * MapSize.x, direction);
                    break;
                case RoadOption.inner:
                    BuildInCornerRoad(level - 1, roadGrid[i].x + roadGrid[i].y * MapSize.x, direction);
                    break;
                case RoadOption.outter:
                    BuildOutCornerRoad(level - 1, roadGrid[i].x + roadGrid[i].y * MapSize.x, direction);
                    break;
            }
        }
        generator.ReCalculateNormal();
        RoadManager.Instance.InitRoadNodeDic();
    }

    public void GetRoadTypeAndDir(Vector2Int roadGrid, out RoadOption roadOption, out Direction direction)
    {
        roadOption = RoadOption.straight;
        direction = Direction.right;
        bool[] around = new bool[8];
        around[0] = GridType.road == GetGridType(roadGrid + new Vector2Int(-1, -1));
        around[1] = GridType.road == GetGridType(roadGrid + new Vector2Int(1, -1));
        around[2] = GridType.road == GetGridType(roadGrid + new Vector2Int(1, 1));
        around[3] = GridType.road == GetGridType(roadGrid + new Vector2Int(-1, 1));
        around[4] = GridType.road == GetGridType(roadGrid + new Vector2Int(0, -1));
        around[5] = GridType.road == GetGridType(roadGrid + new Vector2Int(1, 0));
        around[6] = GridType.road == GetGridType(roadGrid + new Vector2Int(0, 1));
        around[7] = GridType.road == GetGridType(roadGrid + new Vector2Int(-1, 0));
        int count = 0;
        for (int i = 0; i < around.Length; i++)
        {
            if (around[i]) count++;
        }
        //Debug.Log(count);
        if (count == 7)
        {
            roadOption = RoadOption.inner;
            for (int i = 0; i < 4; i++)
            {
                if (around[i] && !around[(i + 1) % 4])
                {
                    direction = (Direction)System.Enum.ToObject(typeof(Direction), (i) % 4);
                    return;
                }
            }
        }
        else
        if (count > 3)
        {
            roadOption = RoadOption.straight;
            for (int i = 0; i < 4; i++)
            {
                if (around[i] && around[(i + 1) % 4] && count != 6)
                {
                    direction = (Direction)System.Enum.ToObject(typeof(Direction), (i + 1) % 4);
                    return;
                }
                if (!around[(i + 4)] && count == 6)
                {
                    direction = (Direction)System.Enum.ToObject(typeof(Direction), (i + 3) % 4);
                    //Debug.Log((int)direction);
                    return;
                }
            }
        }
        else
        {
            roadOption = RoadOption.outter;
            for (int i = 0; i < 4; i++)
            {
                if (around[i] && !around[(i + 1) % 4])
                {
                    direction = (Direction)System.Enum.ToObject(typeof(Direction), (i + 1) % 4);
                    return;
                }
            }
        }
    }


    /// <summary>
    /// 获取地形在世界空间的位置
    /// </summary>
    /// <returns></returns>
    public static Vector3 GetTerrainWorldPosition()
    {
        return GameObject.Find("TerrainGenerator").transform.position;
    }
    /// <summary>
    /// 获取地面某处的坐标
    /// </summary>
    /// <returns></returns>
    /// 
    public static Vector3 GetTerrainPosition(Vector2Int gridPos)
    {
        if (gridPos.x > 0 && gridPos.y > 0 && gridPos.x < 300 && gridPos.y < 300)
        {
            int p = gridPos.x * 4 + gridPos.y * 300 * 4;
            return TerrainGenerator.GetTerrainMeshVertices()[p];
        }
        else return Vector3.zero;
    }

    public static Vector3 GetTerrainStaticPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * 2, 10, gridPos.y * 2);
    }
    public List<Vector3> GetTerrainPosition(List<Vector2Int> gridPos)
    {
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < gridPos.Count; i++)
        {
            result.Add(GetTerrainPosition(gridPos[i]));
        }
        return result;
    }
    public static Vector3 GetTerrainPosition(Vector3 mistakeHeightWorldPos)
    {
        Vector3 localPos = mistakeHeightWorldPos - GetTerrainWorldPosition();
        Vector2Int gridPos = GetCenterGrid(localPos);
        return GetTerrainPosition(gridPos);
    }
    public static Vector2Int GetCenterGrid(Vector3 centerPos)
    {
        Vector3 centerGrid = centerPos / 2;
        int x = Mathf.FloorToInt(centerGrid.x);
        int z = Mathf.FloorToInt(centerGrid.z);
        return new Vector2Int(x, z);
    }
    public GridType GetGridType(Vector2Int grid)
    {
        SingleGrid result;
        if (_gridDic.TryGetValue(grid, out result))
        {
            return result.GridType;
        }
        else
        {
            return GridType.empty;
        }
    }

    /// <summary>
    /// 道路不能与已建造的建筑重合
    /// </summary>
    /// <param name="vector2Int"></param>
    /// <returns></returns>
    public static bool CheckRoadOverlap(Vector2Int vector2Int)
    {
        GridType type = Instance.GetGridType(vector2Int);
        if (type == GridType.inherent||type == GridType.occupy)
        {
            return true;
        }
        return false;
    }

    public SingleGrid GetSingleGrid(Vector2Int grid)
    {
        SingleGrid result;
        if (_gridDic.TryGetValue(grid, out result))
        {
            return result;
        }
        else
        {
            Debug.LogError("不合法输入" + grid.ToString());
            return null;
        }
    }
    public static bool CheckCanBuild(Vector2Int[] grids, Vector2Int parkingPos,bool checkInSea)
    {
        //检测安放地点占用
        bool hasOverlap = CheckOverlap(grids);
        //检测道路是否贴近
        bool hasNearRoad = CheckNearRoad(parkingPos);
        //检测是否靠近海岸线
        bool isInSea = (!checkInSea || CheckIsInWater(grids));
        if (hasNearRoad)
        {
            noticeContent = Localization.ToSettingLanguage(ConstString.NoticeBuildFailNoNearRoad);
        }
        if (hasOverlap)
        {
            noticeContent = Localization.ToSettingLanguage(ConstString.NoticeBuildFailNoPlace);
        }
        if (!isInSea)
        {
            noticeContent = Localization.ToSettingLanguage(ConstString.NoticeBuildFailNoNearSea);
        }
        //if (!isInSea) Debug.Log("不在海里");
        return !hasOverlap && hasNearRoad && isInSea;
    }

    /// <summary>
    /// 检测目标格子对于的地面高度是否有在海平面下的部分
    /// </summary>
    /// <param name="vector2Ints"></param>
    /// <returns></returns>
    public static bool CheckIsInWater(Vector2Int[] vector2Ints)
    {
        
        return CheckIsInWater(vector2Ints[0])|| CheckIsInWater(vector2Ints[vector2Ints.Length-1]);
    }

    public static bool CheckIsInWater(Vector2Int vector2Int)
    {
        Vector3 groundPos = GetTerrainPosition(vector2Int);
        return groundPos.y - 10 < -0.9f;
    }

    /// <summary>
    /// 获得平地位置
    /// </summary>
    /// <returns></returns>
    public static Vector3 GetOnGroundPosition(Vector2Int gridPos)
    {
        if (gridPos.x > 0 && gridPos.y > 0 && gridPos.x < 300 && gridPos.y < 300)
        {
            int p = gridPos.x * 4 + gridPos.y * 300 * 4;
            Vector3 res = TerrainGenerator.GetTerrainMeshVertices()[p];

            return new Vector3(res.x,10f,res.z);
        }
        else return Vector3.zero;
    }
    public static bool CheckOverlap(Vector2Int[] grids)
    {
        for (int i = 0; i < grids.Length; i++)
        {
            if (Instance.GetGridType(grids[i]) != GridType.empty)
            {
                //Debug.Log("建筑重叠");
                return true;
            }
        }
        return false;
    }
    public static bool CheckNearRoad(Vector2Int parkingPos)
    {
        return Instance.GetGridType(parkingPos) == GridType.road;
    }
    public static bool CheckNearRoad(Vector2Int[] grids, int width, int height)
    {
        Vector2Int start = grids[0];
        for (int i = 0; i < width; i++)
        {
            if (Instance.GetGridType(start + new Vector2Int(i, -1)) == GridType.road)
            {
                //direction = Direction.down;
                return true;
            }
            if (Instance.GetGridType(start + new Vector2Int(i, height)) == GridType.road)
            {
                //direction = Direction.up;
                return true;
            }
        }
        for (int i = 0; i < height; i++)
        {
            if (Instance.GetGridType(start + new Vector2Int(-1, i)) == GridType.road)
            {
                //direction = Direction.left;
                return true;
            }
            if (Instance.GetGridType(start + new Vector2Int(width, i)) == GridType.road)
            {
                //direction = Direction.right;
                return true;
            }
        }
        //direction = Direction.right;
        //Debug.Log("不贴合道路");
        return false;
    }

    private void SetGridType(Vector2Int grid, GridType gridType)
    {
        SingleGrid target;
        if (_gridDic.TryGetValue(grid, out target))
        {
            target.GridType = gridType;
        }
        else
        {
            _gridDic.Add(grid, new SingleGrid(grid.x, grid.y, gridType));
        }
    }

    public void ShowGrid(Vector2Int[] grids)
    {
        for (int i = 0; i < grids.Length; i++)
        {
            //Instantiate(gridPfb, new Vector3(grids[i].x*3, 0.1f, grids[i].y*3), Quaternion.identity, transform);
        }
    }
    public static void SetGridTypeToEmpty(Vector2Int grid)
    {
        Instance.SetGridType(grid, GridType.empty);
    }

    public static void SetGridTypeToEmpty(Vector2Int[] grids)
    {
        for (int i = 0; i < grids.Length; i++)
        {
            Instance.SetGridType(grids[i], GridType.empty);
        }
    }

    public static void SetGridTypeToOccupy(Vector2Int grid)
    {
        Instance.SetGridType(grid, GridType.occupy);
    }

    public static void SetGridTypeToOccupy(Vector2Int[] grids)
    {
        for (int i = 0; i < grids.Length; i++)
        {
            Instance.SetGridType(grids[i], GridType.occupy);
        }
    }
    public static void SetGridTypeToInherent(Vector2Int grid)
    {
        Instance.SetGridType(grid, GridType.inherent);
    }

    public static void SetGridTypeToRoad(Vector2Int grid)
    {
        Instance.SetGridType(grid, GridType.road);
    }

    public static GameObject GetNearestMarket(Vector2Int grid)
    {
        float dis = Mathf.Infinity;
        GameObject p = null;
        for (int i = 0; i < Instance._buildings.Count; i++)
        {
            //Debug.Log(Instance._buildings[i].GetComponent<BuildingBase>().runtimeBuildData.Id);
            int id = Instance._buildings[i].GetComponent<BuildingBase>().runtimeBuildData.Id;
            if (id == 20004 || id == 20012 || id == 20013)
            {
                float cur = GetDistance(Instance._buildings[i].parkingGridIn, grid);
                if (cur < dis)
                {
                    dis = cur;
                    p = Instance._buildings[i].gameObject;
                }
            }
        }
        return p;
    }

    public static GameObject GetNearestStorage(Vector2Int grid)
    {
        float dis = Mathf.Infinity;
        GameObject p = null;
        for (int i = 0; i < Instance._buildings.Count; i++)
        {
            if (Instance._buildings[i].GetComponent<BuildingBase>().runtimeBuildData.Id == 20003)
            {
                float cur = GetDistance(Instance._buildings[i].GetComponent<BuildingBase>().parkingGridIn, grid);
                if (cur < dis)
                {
                    dis = cur;
                    p = Instance._buildings[i].gameObject;
                }
            }
        }
        return p;
    }

    public float GetHappiness()
    {
        float sum = 0; ;
        for (int i = 0; i < _buildings.Count; i++)
        {
            sum += _buildings[i].runtimeBuildData.CurLevel;
        }
        return (sum / _buildings.Count) * 10 + 80;
    }

    private static float GetDistance(Vector2Int cur, Vector2Int target)
    {
        return Mathf.Abs(cur.x - target.x) + Mathf.Abs(cur.y - target.y);
    }
}

public class SingleGrid
{
    private List<Direction> originDir = new List<Direction> { Direction.down, Direction.left, Direction.right, Direction.up };
    public Vector2Int GridPos { get; private set; }
    public GridType GridType { get; set; }

    public List<Direction> AvailableDir = new List<Direction> { Direction.down, Direction.left, Direction.right, Direction.up };
    public SingleGrid(int x, int z, GridType gridType)
    {
        this.GridPos = new Vector2Int(x, z);
        GridType = gridType;
    }

    public void RefreshGridDirInfo()
    {
        AvailableDir = originDir;
    }
}
