﻿using System.Collections;
using System.Collections.Generic;
using CSTools;
using UnityEngine;

/// <summary>
/// not use any more
/// </summary>
public class RoadManager : Singleton<RoadManager>
{/*
    public GameObject pfb;
    public Dictionary<Vector2Int, GridNode> RoadNodeDic { get; private set; }
    public List<GridNode> RoadNodes { get; private set; }

    public List<GridNode> CrossNodes { get; private set; }

    //缓存已有的寻路信息
    public Dictionary<string, List<Vector3>> WayPointDic { get; set; }

    //从grid转化为图，再简化
    public void InitRoadManager()
    {
        //InitRoadNodeDic();
    }

    public void InitRoadNodeDic()
    {
        CleanUpAllAttachedChildren(transform);
        //Debug.Log("开始初始化道路");
        List<Vector2Int> roadGrids = MapManager.Instance.GetAllRoadGrid();
        RoadNodeDic = new Dictionary<Vector2Int, GridNode>();
        WayPointDic = new Dictionary<string, List<Vector3>>();
        RoadNodes = new List<GridNode>();
        CrossNodes = new List<GridNode>();
        if (roadGrids.Count <= 0)
        {
            Debug.Log("道路节点数量为0");
            return;
        }
        //初始化格子
        for (int i = 0; i < roadGrids.Count; i++)
        {
            //Debug.Log(mapData.roadGrids[i].Vector2Int);
            GridNode node = new GridNode();
            //node.GridPos.Fill(roadGrids[i]);
            node.Clear();
            if (!RoadNodeDic.TryGetValue(roadGrids[i],out var value))
            {
                RoadNodes.Add(node);
                RoadNodeDic.Add(roadGrids[i], node);
            }
        }
        //连接所有的节点
        for (int i = 0; i < RoadNodes.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Vector2Int dirVec = CastTool.CastDirectionToVector2Int(j) / 2;
                if (RoadNodeDic.TryGetValue(roadGrids[i]+ dirVec, out GridNode node))
                {
                    RoadNodes[i].AddNearbyNode(node);
                }
            }
            RoadNodeDic[RoadNodes[i].GridPos] = RoadNodes[i];
        }
        //删除并排的节点
        for (int i = 0; i < RoadNodes.Count; i++)
        {
            for (int j = 0; j < RoadNodes[i].NearbyNode.Count; j++)
            {
                Vector2Int dirVec = RoadNodes[i].NearbyNode[j].GridPos - RoadNodes[i].GridPos;
                //若相邻为路，往外延伸两格也为路，则说明这确实是条路，而不是路的横向两个
                Vector2Int trigger = 2 * dirVec + RoadNodes[i].GridPos;
                if (!RoadNodeDic.TryGetValue(trigger, out GridNode node))
                {
                    if (RoadNodes[i].NearbyNode.Count > 2)
                    {
                        RoadNodes[i].RemoveNearbyNode(RoadNodes[i].NearbyNode[j]);
                    }
                }
            }
            RoadNodeDic[RoadNodes[i].GridPos] = RoadNodes[i];
        }
        
        //连接在拐点被多删的节点
        for (int i = 0; i < RoadNodes.Count; i++)
        {
            for (int j = 0; j < RoadNodes[i].NearbyNode.Count; j++)
            {
                if (!RoadNodes[i].NearbyNode[j].IsNearbyRoad(RoadNodes[i]))
                {
                    RoadNodes[i].NearbyNode[j].AddNearbyNode(RoadNodes[i]);
                    if (RoadNodeDic.TryGetValue(2 * RoadNodes[i].GridPos - RoadNodes[i].NearbyNode[j].GridPos, out GridNode node))
                    {
                        RoadNodes[i].AddNearbyNode(node);
                    }
                }
            }
            RoadNodeDic[RoadNodes[i].GridPos] = RoadNodes[i];
        }
        RoadNodes.Sort(Compare);
        
        //删除中间节点
        for (int i = RoadNodes.Count - 1; i >= 0; i--)
        {
            RoadNodes[i].NearNodeCount = RoadNodes[i].NearbyNode.Count;
            //Debug.Log(RoadNodes[i].GridPos);
            //Instantiate(pfb, MapManager.Instance.GetTerrainPosition(RoadNodes[i].GridPos), Quaternion.identity, transform);
            if (RoadNodes[i].NearbyNode.Count == 2)
            {
                Vector2Int v1 = RoadNodes[i].NearbyNode[0].GridPos - RoadNodes[i].GridPos;
                Vector2Int v2 = RoadNodes[i].GridPos - RoadNodes[i].NearbyNode[1].GridPos;
                if (v1.x == 0 && v2.x == 0 || v1.y == 0 && v2.y == 0)
                {
                    //Instantiate(pfb, MapManager.Instance.GetTerrainPosition(RoadNodes[i].GridPos) + new Vector3(0, 2, 0), Quaternion.identity, transform);
                    //Debug.Log("开始-----" + RoadNodes[i].GridPos);
                    RoadNodes[i].NearbyNode[0].AddNearbyNode(RoadNodes[i].NearbyNode[1]);
                    //Debug.Log(RoadNodes[i].NearbyNode[0].GridPos + "=>" + RoadNodes[i].NearbyNode[1].GridPos);
                    RoadNodes[i].NearbyNode[0].RemoveNearbyNode(RoadNodes[i]);
                    RoadNodes[i].NearbyNode[1].AddNearbyNode(RoadNodes[i].NearbyNode[0]);
                    //Debug.Log(RoadNodes[i].NearbyNode[1].GridPos + "=>" + RoadNodes[i].NearbyNode[0].GridPos);
                    RoadNodes[i].NearbyNode[1].RemoveNearbyNode(RoadNodes[i]);
                    RoadNodes[i].RemoveNearbyNode(RoadNodes[i].NearbyNode[1]);
                    RoadNodes[i].RemoveNearbyNode(RoadNodes[i].NearbyNode[0]);
                    RoadNodes.Remove(RoadNodes[i]);
                    //RoadNodeDic.Remove(RoadNodes[i].GridPos);
                    //StraightNodes.Add(RoadNodes[i]);
                    //Instantiate(pfb, MapManager.Instance.GetTerrainPosition(RoadNodes[i].GridPos), Quaternion.identity, transform);
                }
            }
        }
        for (int i = 0; i < RoadNodes.Count; i++)
        {
            for (int j = 0; j < RoadNodes[i].NearbyNode.Count; j++)
            {
                Instantiate(pfb, MapManager.GetTerrainStaticPosition(RoadNodes[i].GridPos)+new Vector3(0,j,0), Quaternion.identity, transform);
            }
        }
        TrafficManager.Instance.reachable = RoadNodes;
        CrossNodes = RoadNodes;
        //删除反向的结点
        for (int i = 0; i < RoadNodes.Count; i++)
        {
            //Debug.Log("_____" + RoadNodes[i].GridPos + " " + RoadNodes[i].NearbyNode.Count);
            for (int j = RoadNodes[i].NearbyNode.Count-1; j >=0; j--)
            {
                //Debug.Log(j);
                Vector2Int dirVec = RoadNodes[i].NearbyNode[j].GridPos - RoadNodes[i].GridPos;
                Direction dir = CastTool.CastVector2ToDirection(dirVec);
                dir++;
                //若某点->邻点的逆时针的相邻不为路，则说明这是逆向的路
                Vector2Int triggerGrid = RoadNodes[i].GridPos + CastTool.CastDirectionToVector2Int((int)dir) / 2;
                //Debug.Log(RoadNodes[i].GridPos + "=>" + RoadNodes[i].NearbyNode[j].GridPos + " " + triggerGrid);
                if(RoadNodeDic.TryGetValue(triggerGrid,out GridNode triggerNode))
                {
                    //Debug.Log(triggerNode.NearNodeCount);
                    if (RoadNodes[i].NearNodeCount == 4 && triggerNode.NearNodeCount<=2)
                    {
                        //Debug.Log("remove1"+ RoadNodes[i].NearbyNode[j].GridPos);
                        RoadNodes[i].RemoveNearbyNode(RoadNodes[i].NearbyNode[j]);
                    }
                    if(RoadNodes[i].NearNodeCount == 3 && !IsCrossNode(triggerNode))
                    {
                        //Debug.Log("remove2" + RoadNodes[i].NearbyNode[j].GridPos);
                        RoadNodes[i].RemoveNearbyNode(RoadNodes[i].NearbyNode[j]);
                    }
                }
                else
                {
                    if(RoadNodes[i].NearNodeCount == 4)
                    {
                        //Debug.Log("remove3" + RoadNodes[i].NearbyNode[j].GridPos);
                        RoadNodes[i].RemoveNearbyNode(RoadNodes[i].NearbyNode[j]);
                    }
                    if(RoadNodes[i].NearNodeCount == 3)
                    {
                        //Debug.Log("remove4" + RoadNodes[i].NearbyNode[j].GridPos);
                        RoadNodes[i].RemoveNearbyNode(RoadNodes[i].NearbyNode[j]);
                    }
                }

            }
            RoadNodeDic[RoadNodes[i].GridPos] = RoadNodes[i];
        }

        
        //Invoke("Show",1f);
    }

    private bool IsCrossNode(GridNode roadNode)
    {
        for (int i = 0; i < CrossNodes.Count; i++)
        {
            if(CrossNodes[i].GridPos == roadNode.GridPos)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsCrossNode(Vector2Int roadNode)
    {
        for (int i = 0; i < CrossNodes.Count; i++)
        {
            if (CrossNodes[i].GridPos == roadNode)
            {
                return true;
            }
        }
        return false;
    }
    public void Show()
    {
        for (int i = 0; i < CrossNodes.Count; i++)
        {
            for (int j = 0; j < CrossNodes[i].NearbyNode.Count; j++)
            {
                Debug.DrawLine(MapManager.GetTerrainPosition(CrossNodes[i].GridPos),
                    MapManager.GetTerrainPosition(CrossNodes[i].NearbyNode[j].GridPos), Color.red, 100f);
                Vector3 delta = MapManager.GetTerrainPosition(CrossNodes[i].NearbyNode[j].GridPos) -
                    MapManager.GetTerrainPosition(CrossNodes[i].GridPos);
                Instantiate(pfb, MapManager.GetTerrainPosition(CrossNodes[i].GridPos) + delta / 2+new Vector3(1,0,1), Quaternion.LookRotation(delta), transform);
            }
        }
    }
    public int Compare(GridNode a, GridNode b)
    {
        return (a.GridPos.x - b.GridPos.x) * 1000 + (a.GridPos.y - b.GridPos.y);
    }
    
    public void AddRoadNode(Vector2Int grid)
    {
        if (!RoadNodeDic.TryGetValue(grid, out GridNode node))
        {
            RoadNodeDic.Add(grid,new GridNode(grid));
        }
    }

    public void CheckAndAddCrossNode(GridNode node)
    {
        bool p = true;
        for (int i = 0; i < CrossNodes.Count; i++)
        {
            if (node.GridPos == CrossNodes[i].GridPos)
            {
                p = false;
            }
        }
        if (p)
        {
            CrossNodes.Add(node);
        }
    }
    public void AddTurnNode(Vector2Int gridIn, Vector2Int gridOut)
    {
        GridNode inNode = RoadNodeDic[gridIn];
        GridNode outNode = RoadNodeDic[gridOut];
        outNode.AddNearbyNode(inNode);
    }
    public void AddCrossNode(Vector2Int grid, Direction dir)
    {
        if (IsCrossNode(grid))
            return;
        if (RoadNodeDic.TryGetValue(grid, out GridNode node0))
        {
            GridNode node1 = GetNearestCross(grid, dir);
            GridNode node2 = GetNearestCross(grid, dir + 2);
            if (node1.IsNearbyRoad(node2))
            {
                node1.RemoveNearbyNode(node2);
                node1.AddNearbyNode(node0);
                node0.AddNearbyNode(node2);
                //Debug.Log(node1.GridPos + " => " + node0.GridPos + " =>  " + node2.GridPos);
            }
            else
            {
                node2.RemoveNearbyNode(node1);
                node2.AddNearbyNode(node0);
                node0.AddNearbyNode(node1);
                //Debug.Log(node2.GridPos + " => " + node0.GridPos + " =>  " + node1.GridPos);
            }
            CrossNodes.Add(node0);
        }
        else
        {
            Debug.Log("路径字典里没有该节点"+ grid);
        }
    }
    private void CleanUpAllAttachedChildren(Transform target)
    {
        for (int i = 0; i < target.childCount; i++)
        {
            Destroy(target.GetChild(i).gameObject);
        }

    }
    public List<Vector3> GetWayPoints(Vector2Int start, Vector2Int end)
    {
        //Debug.Log(start + " " + end);
        if(WayPointDic.TryGetValue(start.ToString()+end.ToString(),out List<Vector3> res))
        {
            return res;
        }
        ClearRoadNodeH();
        List<GridNode> path = new List<GridNode>();
        List<GridNode> openList = new List<GridNode>();
        List<GridNode> closeList = new List<GridNode>();
        GridNode startNode = RoadNodeDic[start];
        GridNode endNode = RoadNodeDic[end];
        startNode.G = GetDistance(startNode.GridPos, end);
        openList.Add(startNode);
        path.Add(startNode);
        GridNode temp = startNode;
        int n = 100;
        while (temp != endNode && n-- > 0)
        {
            if (openList.Count <= 0)
            {
                //Debug.LogError("路径被阻挡！"+ start+"=>"+end);
                return null;
            }
            //Debug.Log("——");
            temp = openList[0];
            for (int i = 0; i < openList.Count; i++)
            {
                //Debug.Log(openList[i].GridPos+" "+temp.H+" "+openList[i].H);
                if (temp.G > openList[i].G)
                {
                    temp = openList[i];
                }
            }
            openList.Remove(temp);
            closeList.Add(temp);
            //Debug.Log("选择:"+temp.GridPos);
            if (temp == endNode)
            {
                List<Vector3> list = new List<Vector3>();
                while (temp != startNode)
                {
                    //Debug.Log(MapManager.Instance.GetTerrainPosition(temp.GridPos));
                    list.Add(MapManager.GetTerrainPosition(temp.GridPos) + new Vector3(1, 0, 1));
                    temp = temp.Parent;
                    //Instantiate(pfb, MapManager.Instance.GetTerrainPosition(temp.GridPos), Quaternion.identity, transform);
                }
                //Debug.Log(MapManager.Instance.GetTerrainPosition(startNode.GridPos));
                list.Add(MapManager.GetTerrainPosition(startNode.GridPos) + new Vector3(1, 0, 1));
                list.Reverse();
                WayPointDic.Add(start.ToString() + end.ToString(), list);
                return list;
            }
            for (int i = 0; i < temp.NearbyNode.Count; i++)
            {
                if (closeList.Contains(temp.NearbyNode[i])) continue;
                temp.NearbyNode[i].G = GetDistance(temp.NearbyNode[i].GridPos, end);
                temp.NearbyNode[i].Parent = temp;
                openList.Add(temp.NearbyNode[i]);
            }
        }
        Debug.LogError("寻路失败");
        return null;
    }

    private int GetDistance(Vector2Int cur, Vector2Int target)
    {
        return Mathf.Abs(cur.x - target.x) + Mathf.Abs(cur.y - target.y);
    }
    public void InitCube()
    {
        for (int i = 0; i < CrossNodes.Count; i++)
        {
            Instantiate(pfb, MapManager.GetTerrainPosition(CrossNodes[i].GridPos), Quaternion.identity, transform);
        }
    }

    public void ClearRoadNodeH()
    {
        for (int i = 0; i < RoadNodes.Count; i++)
        {
            RoadNodes[i].Clear();
        }
    }
    private GridNode GetNearestCross(Vector2Int grid, Direction dir)
    {
        //Debug.Log(grid);
        GridNode temp = RoadNodeDic[grid];
        //Debug.Log(temp.NearbyNode.Count);
        //foreach(var r in temp.NearbyNode)
        //{
        //    Debug.Log(r.GridPos);
        //}
        while (!IsCrossNode(temp))
        {
            Vector2Int next = temp.GridPos + CastTool.CastDirectionToVector2Int((int)dir + 1) / 2;
            //Debug.Log(next+" "+ temp.NearNodeCount);
            temp = RoadNodeDic[next];
        }
        return temp;
    }
}

[System.Serializable]
public class GridNode
{
    public GridNode()
    {
        NearbyNode = new List<GridNode>();
        Parent = null;
        G = 0;
        NearNodeCount = 0;
    }
    public List<GridNode> NearbyNode { get; private set; }

    public int NearNodeCount = 0;//上面只存可通向的的节点，这个存储所有邻点的个数
    public Vector2Int GridPos { get { return new Vector2Int(x, y); } }

    public int x, y;

    public float passSpeed;

    public float height;

    public int enterCost;

    public float mineValue;

    public Direction direction;

    public GridType gridType;
    public GridNode Parent { get; set; }
    public int G { get; set; }

    public int F { get; set; }

    public void Clear()
    {
        Parent = null;
        G = 0;
        NearNodeCount = 0;
    }
    public void AddNearbyNode(GridNode roadNode)
    {
        //如果不是就加入
        if (!IsNearbyRoad(roadNode))
        {
            NearbyNode.Add(roadNode);
        }
        else
        {
            Debug.Log(GridPos + "已经添加过" + roadNode.GridPos);
        }
    }

    public void RemoveNearbyNode(GridNode roadNode)
    {
        if (IsNearbyRoad(roadNode))
        {
            NearbyNode.Remove(roadNode);
        }
        else
        {
            Debug.Log("没用正确删除" + roadNode.GridPos);
        }
    }

    public bool IsNearbyRoad(GridNode node)
    {
        if (NearbyNode.Count <= 0)
        {
            return false;
        }
        for (int i = 0; i < NearbyNode.Count; i++)
        {
            if (NearbyNode[i] == node)
            {
                return true;
            }
        }
        return false;
    }
*/

}