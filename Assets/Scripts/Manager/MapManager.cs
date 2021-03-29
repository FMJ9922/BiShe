using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : Singleton<MapManager>
{
    public Vector2Int MapSize { get; private set; }
    private Dictionary<Vector2Int, SingleGrid> _gridDic = new Dictionary<Vector2Int, SingleGrid>();
    private LevelData _leveldata;
    private Vector3[] _vertices;//存储地形顶点数据
    public const int unit = 2;//地形一格的长度（单位米）
    [SerializeField] private TerrainGenerator generator;
    //[SerializeField] GameObject gridPfb;


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
        _vertices = generator.GetTerrainMeshVertices();
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
        List<StaticBuilding> lists = StaticBuilding.lists;
        for (int i = 0; i < lists.Count; i++)
        {
            lists[i].SetGrids();
        }
        Debug.Log("地图已初始化！");
    }

    /// <summary>
    /// 刷地基
    /// </summary>
    public void BuildFoundation(Vector2Int[] takenGirds, int tex)
    {
        for (int i = 0; i < takenGirds.Length; i++)
        {
            generator.RefreshUV(tex, 4, takenGirds[i].x + takenGirds[i].y * MapSize.x);
        }
    }
    public void BuildOutCornerRoad(int level, int index, Direction direction)
    {
        generator.RefreshUV(12 - level * 4 + 2, 4, index, (int)direction);
    }
    public void BuildInCornerRoad(int level, int index, Direction direction)
    {
        generator.RefreshUV(12 - level * 4 + 1, 4, index, (int)direction);
    }
    public void BuildStraightRoad(int level, int index, Direction direction)
    {
        generator.RefreshUV(12 - level * 4, 4, index, (int)direction);
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
                    BuildStraightRoad(level-1, roadGrid[i].x + roadGrid[i].y * MapSize.x, direction);
                    break;
                case RoadOption.inner:
                    BuildInCornerRoad(level-1, roadGrid[i].x + roadGrid[i].y * MapSize.x, direction);
                    break;
                case RoadOption.outter:
                    BuildOutCornerRoad(level-1, roadGrid[i].x + roadGrid[i].y * MapSize.x, direction);
                    break;
            }
        }

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
                if (!around[(i+4)] && count == 6)
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
    public Vector3 GetTerrainWorldPosition()
    {
        return generator.transform.position;
    }
    /// <summary>
    /// 获取地面某处的坐标
    /// </summary>
    /// <returns></returns>
    public Vector3 GetTerrainPosition(Vector2Int gridPos)
    {
        int p = gridPos.x * 4 + gridPos.y * _leveldata.Width * 4;
        return _vertices[p];
    }
    public Vector3 GetTerrainPosition(Vector3 mistakeHeightWorldPos)
    {
        Vector3 localPos = mistakeHeightWorldPos - GetTerrainWorldPosition();
        Vector2Int gridPos = GetCenterGrid(localPos);
        return GetTerrainPosition(gridPos);
    }
    private Vector2Int GetCenterGrid(Vector3 centerPos)
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

    public static bool CheckCanBuild(Vector2Int[] grids,int width,int height,out Direction direction)
    {
        //检测安放地点占用
        bool hasOverlap = CheckOverlap(grids);
        //检测道路是否贴近
        bool hasNearRoad = CheckNearRoad(grids, width, height,out direction);
        return !hasOverlap && hasNearRoad; 
    }

    public static bool CheckOverlap(Vector2Int[] grids)
    {
        for (int i = 0; i < grids.Length; i++)
        {
            if (Instance.GetGridType(grids[i]) != GridType.empty)
            {
                return true;
            }
        }
        return false;
    }

    public static bool CheckNearRoad(Vector2Int[] grids, int width, int height,out Direction direction)
    {
        Vector2Int start = grids[0];
        for (int i = 0; i < width; i++)
        {
            if (Instance.GetGridType(start + new Vector2Int(i, -1)) == GridType.road)
            {
                direction = Direction.down;
                return true;
            }
            if(Instance.GetGridType(start + new Vector2Int(i, height)) == GridType.road)
            {
                direction = Direction.up;
                return true;
            }
        }
        for (int i = 0; i < height; i++)
        {
            if (Instance.GetGridType(start + new Vector2Int(-1, i)) == GridType.road)
            {
                direction = Direction.left;
                return true;
            }
            if(Instance.GetGridType(start + new Vector2Int(width, i)) == GridType.road)
            {
                direction = Direction.right;
                return true;
            }
        }
        direction = Direction.right;
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
}

public class SingleGrid
{
    public Vector2Int GridPos { get; private set; }
    public GridType GridType { get; set; }

    public SingleGrid(int x, int z, GridType gridType)
    {
        this.GridPos = new Vector2Int(x, z);
        GridType = gridType;
    }
}
