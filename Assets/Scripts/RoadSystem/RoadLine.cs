using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;

public class RoadLine
{
    #region Property
    public GridNode StartGrid { get => _roadGrids[0]; } 
    public GridNode gridNode { get => _roadGrids[_roadGrids.Count - 1]; }

    private List<GridNode> _roadGrids;
    #endregion
    #region Public

    public RoadLine(GridNode start,GridNode end)
    {
        var posList = GetListByStartAndEnd(start.GridPos, end.GridPos);
        _roadGrids = new List<GridNode>();
        for (int i = 0; i < posList.Count; i++)
        {
            _roadGrids.Add(MapManager.GetGridNode(posList[i]));
        }

    }

    #endregion

    #region private

    private List<Vector2Int> GetListByStartAndEnd(Vector2Int start,Vector2Int end)
    {
        List<Vector2Int> ret = new List<Vector2Int>();
        if (start.x < end.x)
        {
            for (int i = start.x; i <= end.x; i++)
            {
                ret.Add(new Vector2Int(i, start.y));
            }
        }
        else if (start.x > end.x) 
        {
            for (int i = end.x; i <= start.x; i++)
            {
                ret.Add(new Vector2Int(i, start.y));
            }
        }
        else if(start.y < end.y)
        {
            for (int i = start.y; i <= end.y; i++)
            {
                ret.Add(new Vector2Int(start.x, i));
            }
        }
        else if(start.y > end.y)
        {
            for (int i = end.y; i <= start.y; i++)
            {
                ret.Add(new Vector2Int(start.x,i)); 
            }
        }
        return ret;
    }
    #endregion
}
