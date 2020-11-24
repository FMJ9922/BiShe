using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : Singleton<MapManager>
{
    public Vector2Int MapSize { get; private set; }
    private Dictionary<Vector2Int, SingleGrid> _gridDic = new Dictionary<Vector2Int, SingleGrid>();
    private LevelData _leveldata;
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

}

public struct SingleGrid
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