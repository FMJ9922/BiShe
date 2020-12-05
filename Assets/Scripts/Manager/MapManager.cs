using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : Singleton<MapManager>
{
    public Vector2Int MapSize { get; private set; }
    private Dictionary<Vector2Int, SingleGrid> _gridDic = new Dictionary<Vector2Int, SingleGrid>();
    private LevelData _leveldata;
    [SerializeField] GameObject gridPfb;
    private void Start()
    {
        InitLevelData();
        InitGrid();
    }

    /// <summary>
    /// 加载关卡的时候调用
    /// </summary>
    private void InitLevelData(int levelId = 30001)
    {
        _leveldata = DataManager.GetLevelData(levelId);
        MapSize = new Vector2Int(_leveldata.Length, _leveldata.Width);
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
        Debug.Log("地图已初始化！");
    }

    public GridType GetGridType(Vector2Int grid)
    {
        SingleGrid result;
        if(_gridDic.TryGetValue(grid,out result))
        {
            return result.GridType;
        }
        else
        {
            return GridType.empty;
        }
    }

    public static bool CheckGridOverlap(Vector2Int[] grids)
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

    private void SetGridType(Vector2Int grid,GridType gridType)
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
            Instantiate(gridPfb, new Vector3(grids[i].x*3, 0.1f, grids[i].y*3), Quaternion.identity, transform);
        }
    }
    public static void SetGridTypeToEmpty(Vector2Int grid)
    {
        Instance.SetGridType(grid, GridType.empty);
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
    public Vector2Int Position { get { return 3 * GridPos; } }

    public GridType GridType { get; set; }

    public SingleGrid(int x, int z, GridType gridType)
    {
        this.GridPos = new Vector2Int(x, z);
        GridType = gridType;
    }
}