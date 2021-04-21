using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadManager : Singleton<RoadManager>
{
    public GameObject pfb;
    public Dictionary<Vector2Int, RoadNode> RoadNodeDic { get; private set; }
    public List<RoadNode> RoadNodes { get; private set; }

    public List<RoadNode> CrossNodes { get; private set; }

    //从grid转化为图，再简化

    public void InitRoadManager()
    {
        InitRoadNodeDic();
    }

    public void InitRoadNodeDic()
    {
        //Debug.Log("开始初始化道路");
        RoadNodeDic = new Dictionary<Vector2Int, RoadNode>();
        RoadNodes = new List<RoadNode>();
        CrossNodes = new List<RoadNode>();
        MapData mapData = TerrainGenerator.Instance.GetMapData();
        if (mapData.roadGrids.Count <= 0)
        {
            Debug.Log("道路节点数量为0");
            return;
        }
        //初始化格子
        for (int i = 0; i < mapData.roadGrids.Count; i++)
        {
            RoadNode node = new RoadNode(mapData.roadGrids[i].Vector2Int);
            node.Clear();
            RoadNodes.Add(node);
            RoadNodeDic.Add(mapData.roadGrids[i].Vector2Int, node);
        }
        //连接所有的节点
        for (int i = 0; i < mapData.roadGrids.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Vector2Int dirVec = CastTool.CastDirectionToVector2Int(j) / 2;
                if (RoadNodeDic.TryGetValue(mapData.roadGrids[i].Vector2Int + dirVec, out RoadNode node))
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
                if (!RoadNodeDic.TryGetValue(trigger, out RoadNode node))
                {
                    RoadNodes[i].RemoveNearbyNode(RoadNodes[i].NearbyNode[j]);
                }
            }
            RoadNodeDic[RoadNodes[i].GridPos] = RoadNodes[i];
        }

        //连接在拐点被多删的节点
        for (int i = 0; i < mapData.roadGrids.Count; i++)
        {
            for (int j = 0; j < RoadNodes[i].NearbyNode.Count; j++)
            {
                if (!RoadNodes[i].NearbyNode[j].IsNearbyRoad(RoadNodes[i]))
                {
                    RoadNodes[i].NearbyNode[j].AddNearbyNode(RoadNodes[i]);
                    if (RoadNodeDic.TryGetValue(2 * RoadNodes[i].GridPos - RoadNodes[i].NearbyNode[j].GridPos, out RoadNode node))
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
        TrafficManager.Instance.reachable = RoadNodes;
        CrossNodes = RoadNodes;
        /*
        for (int i = 0; i < RoadNodes.Count; i++)
        {
            if(!RoadNodeDic.TryGetValue(RoadNodes[i].GridPos+new Vector2Int(1,1), out RoadNode node))
            {
                
            }
        }*/
    }
    public int Compare(RoadNode a, RoadNode b)
    {
        return (a.GridPos.x - b.GridPos.x) * 1000 + (a.GridPos.y - b.GridPos.y);
    }

    public void AddRoadNode(Vector2Int grid)
    {
        if (RoadNodeDic.TryGetValue(grid, out RoadNode node))
        {

        }
        else
        {
            Debug.Log("路径字典里没有该节点");
        }
    }

    public void AddCrossNode(Vector2Int grid, Direction dir)
    {
        //Debug.Log(grid);
        if (RoadNodeDic.TryGetValue(grid, out RoadNode node0))
        {
            RoadNode node1 = GetNearestCross(grid, dir);
            RoadNode node2 = GetNearestCross(grid, dir + 2);
            //Debug.Log(node0.GridPos + " " + node1.GridPos + " " + node2.GridPos);
            node1.RemoveNearbyNode(node2);
            node2.RemoveNearbyNode(node1);
            node1.AddNearbyNode(node0);
            node2.AddNearbyNode(node0);
            node0.AddNearbyNode(node1);
            node0.AddNearbyNode(node2);
            CrossNodes.Add(node0);
        }
        else
        {
            Debug.Log("路径字典里没有该节点");
        }
    }

    public List<Vector3> GetWayPoints(Vector2Int start, Vector2Int end)
    {
        ClearRoadNodeH();
        List<RoadNode> path = new List<RoadNode>();
        List<RoadNode> openList = new List<RoadNode>();
        List<RoadNode> closeList = new List<RoadNode>();
        RoadNode startNode = RoadNodeDic[start];
        RoadNode endNode = RoadNodeDic[end];
        startNode.H = GetDistance(startNode.GridPos, end);
        openList.Add(startNode);
        path.Add(startNode);
        RoadNode temp = startNode;
        int n = 20;
        while (temp != endNode&&n-->0)
        {
            if (openList.Count <= 0)
            {
                Debug.LogError("路径被阻挡！");
                return null;
            }
            //Debug.Log("——");
            for (int i = 0; i < openList.Count; i++)
            {
                //Debug.Log(openList[i].GridPos+" "+temp.H+" "+openList[i].H);
                if (temp.H > openList[i].H)
                {
                    temp = openList[i];
                }
            }
            //Debug.Log("选择:"+temp.GridPos);
            openList.Remove(temp);
            closeList.Add(temp);
            if(temp == endNode)
            {
                List<Vector3> list = new List<Vector3>();
                while (temp != startNode)
                {
                    //Debug.Log(MapManager.Instance.GetTerrainPosition(temp.GridPos));
                    list.Add(MapManager.Instance.GetTerrainPosition(temp.GridPos));
                    temp = temp.Parent;
                    //Instantiate(pfb, MapManager.Instance.GetTerrainPosition(temp.GridPos), Quaternion.identity, transform);
                }
                //Debug.Log(MapManager.Instance.GetTerrainPosition(startNode.GridPos));
                list.Add(MapManager.Instance.GetTerrainPosition(startNode.GridPos)+new Vector3(1,0,1));
                list.Reverse();
                return list;
            }
            for(int i = 0; i < temp.NearbyNode.Count; i++)
            {
                if (closeList.Contains(temp.NearbyNode[i])) continue;
                temp.NearbyNode[i].H = GetDistance(temp.NearbyNode[i].GridPos, end);
                temp.NearbyNode[i].Parent = temp;
                openList.Add(temp.NearbyNode[i]);
            }
        }
        Debug.LogError("寻路失败");
        return null;
    }

    private float GetDistance(Vector2Int cur,Vector2Int target)
    {
        return Mathf.Abs(cur.x - target.x) + Mathf.Abs(cur.y - target.y);
    }
    public void InitCube()
    {
        for (int i = 0; i < CrossNodes.Count; i++)
        {
            Instantiate(pfb, MapManager.Instance.GetTerrainPosition(CrossNodes[i].GridPos), Quaternion.identity, transform);
        }
    }

    public void ClearRoadNodeH()
    {
        for (int i = 0; i < RoadNodes.Count; i++)
        {
            RoadNodes[i].Clear();
        }
    }
    private RoadNode GetNearestCross(Vector2Int grid, Direction dir)
    {
        RoadNode temp = RoadNodeDic[grid];
        //Debug.Log(temp.NearbyNode.Count);
        //foreach(var r in temp.NearbyNode)
        //{
        //    Debug.Log(r.GridPos);
        //}
        while (temp.NearbyNode.Count <= 2)
        {
            Vector2Int next = temp.GridPos + CastTool.CastDirectionToVector2Int((int)dir + 1) / 2;
            //Debug.Log(next);
            temp = RoadNodeDic[next];
        }
        return temp;
    }
}


public class RoadNode
{
    public RoadNode(Vector2Int pos)
    {
        NearbyNode = new List<RoadNode>();
        GridPos = pos;
    }
    /*
    public static bool operator ==(RoadNode A, RoadNode B)
    {
        return A.GridPos == B.GridPos;
    }
    public static bool operator !=(RoadNode A, RoadNode B)
    {
        return A.GridPos != B.GridPos;
    }
    */
    public List<RoadNode> NearbyNode { get; private set; }
    public Vector2Int GridPos { get; set; }

    public RoadNode Parent { get; set; }
    public float H { get; set; }

    public void Clear()
    {
        Parent = null;
        H = 0;
    }
    public void AddNearbyNode(RoadNode roadNode)
    {
        if (!IsNearbyRoad(roadNode))
        {
            NearbyNode.Add(roadNode);
        }
        else
        {
            Debug.Log("没用正确添加" + roadNode.GridPos);
        }
    }

    public void RemoveNearbyNode(RoadNode roadNode)
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

    public bool IsNearbyRoad(RoadNode node)
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


}