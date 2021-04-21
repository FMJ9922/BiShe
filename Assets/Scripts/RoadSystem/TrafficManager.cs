using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TrafficManager : Singleton<TrafficManager>
{

    [SerializeField] GameObject[] carPfbs;
    private List<DriveSystem> carUsingPool = new List<DriveSystem>();
    private List<DriveSystem> carUnusedPool = new List<DriveSystem>();

    //寻路节点
    public List<RoadNode> reachable = new List<RoadNode>();
    public List<RoadNode> explored = new List<RoadNode>();

    private List<Vector2Int> RoadNodes;
    private readonly Vector3 hidePos = new Vector3(0, -100, 0);

    public List<GameObject> FakeRoute1;
    public List<GameObject> FakeRoute2;
    public List<GameObject> FakeRoute3;
    public List<GameObject> FakeRoute4;
    enum FindRoadState
    {
        straight = 0,//在笔直的路上走
        backward = 1,//走到死胡同了
        turing = 2,//在拐弯路口
    }

    private void Start()
    {
        //UseCar(TransportationType.medium, ObjectsToVector3s(FakeRoute1), DriveType.yoyo);
        //UseCar(TransportationType.mini, ObjectsToVector3s(FakeRoute2), DriveType.yoyo);
        //UseCar(TransportationType.van, ObjectsToVector3s(FakeRoute3), DriveType.loop);
        //UseCar(TransportationType.medium, ObjectsToVector3s(FakeRoute4), DriveType.yoyo);
    }

    private List<Vector3> ObjectsToVector3s(List<GameObject> objs)
    {
        List<Vector3> lists = new List<Vector3>();
        for (int i = 0; i < objs.Count; i++)
        {
            lists.Add(objs[i].transform.position);
        }
        return lists;
    }
    public void UseCar(TransportationType type, BuildingBase startBuilding, BuildingBase endBuilding, DriveType driveType = DriveType.once, UnityAction unityAction = null)
    {
        DriveSystem driveSystem = GetCarFromPool(type);
        Vector2Int start = startBuilding.parkingGrid;
        Vector2Int end = endBuilding.parkingGrid;
        //Debug.Log(start+" "+end);
        List<Vector3> wayPoints = RoadManager.Instance.GetWayPoints(start, end);
        Vector3 delta = new Vector3(0, 0, 0);
        for (int i = 0; i < wayPoints.Count; i++)
        {
            wayPoints[i] += delta;
        }
        //Debug.Log(wayPoints.Count);
        driveSystem.StartDriving(wayPoints, driveType, (DriveSystem sys) => { RecycleCar(sys, unityAction); });
    }


    public void UseCar(TransportationType type, List<Vector3> wayPoints, DriveType driveType = DriveType.once, UnityAction unityAction = null)
    {
        DriveSystem driveSystem = GetCarFromPool(type);
        driveSystem.StartDriving(wayPoints, driveType, (DriveSystem sys) => { RecycleCar(sys, unityAction); });
    }

    public bool CloseToTarget(Vector2 cur, Vector2 target)
    {
        return Vector2.Distance(cur, target) >= 2;
    }

    private Direction ChooseStartDirection(Vector2Int start, Vector2Int end, Direction roadDir)
    {
        Vector2Int delta = end - start;
        switch (roadDir)
        {
            case Direction.left:
            case Direction.right:
                return delta.x > 0 ? Direction.right : Direction.left;
            case Direction.down:
            case Direction.up:
            default:
                return delta.y > 0 ? Direction.up : Direction.down;
        }
    }

    /// <summary>
    /// 获得车辆的行驶路径
    /// </summary>
    /// <returns></returns>
    private List<Vector3> GetWayPoints(Vector2Int start, Vector2Int end, Direction direction)
    {
        RoadNodes = new List<Vector2Int>();
        Vector2Int tempGrid = start;
        Direction curDir = direction;
        Vector2Int forwardVec = CastTool.CastDirectionToVector2Int((int)curDir);
        FindRoadState state = FindRoadState.straight;
        RoadNodes.Add(start);
        int count = 500;
        while (CloseToTarget(tempGrid, end) && count > 0)
        {
            //Debug.Log(tempGrid+" "+forwardVec + " "+ state);
            count--;
            switch (state)
            {
                case FindRoadState.straight:
                    {
                        //int delta = forwardVec.x * (end - tempGrid).x+ forwardVec.y * (end - tempGrid).y;
                        //if(delta<=0)
                        if (IsGridRoad(tempGrid + forwardVec))
                        {
                            tempGrid += forwardVec;
                        }
                        else
                        {
                            state = FindRoadState.turing;
                        }
                        break;
                    }
                case FindRoadState.backward:
                    {
                        Vector3 forward = new Vector3(forwardVec.x, 0, forwardVec.y);
                        Vector2 target2 = (Vector2)end - tempGrid;
                        Vector3 target = new Vector3(target2.x, 0, target2.y);
                        curDir = CastTool.CastVector2ToDirection(forwardVec);
                        bool canLeft = IsGridRoad(tempGrid + CastTool.CastDirectionToVector2Int((int)curDir + 1));
                        bool canRight = IsGridRoad(tempGrid + CastTool.CastDirectionToVector2Int((int)curDir - 1));
                        if (canLeft && canRight)
                        {
                            forwardVec = Vector3.Cross(forward, target).y <= 0 ?
                                CastTool.CastDirectionToVector2Int((int)curDir + 1) :
                                CastTool.CastDirectionToVector2Int((int)curDir - 1);
                            state = FindRoadState.straight;
                            //Debug.Log("1");
                            RoadNodes.Add(tempGrid);
                            break;
                        }
                        else if (canLeft)
                        {
                            forwardVec = CastTool.CastDirectionToVector2Int((int)curDir + 1);
                            state = FindRoadState.straight;
                            //Debug.Log("2" + tempGrid);
                            RoadNodes.Add(tempGrid);
                            break;
                        }
                        else if (canRight)
                        {
                            forwardVec = CastTool.CastDirectionToVector2Int((int)curDir - 1);
                            state = FindRoadState.straight;
                            //Debug.Log("3" + tempGrid);
                            RoadNodes.Add(tempGrid);
                            break;

                        }
                        else
                        {
                            tempGrid += forwardVec;
                            //直行直到找到第一个能转弯的路口
                            break;
                        }
                    }
                case FindRoadState.turing:
                    {
                        Vector3 forward = new Vector3(forwardVec.x, 0, forwardVec.y);
                        Vector2 target2 = (Vector2)end - tempGrid;
                        Vector3 target = new Vector3(target2.x, 0, target2.y);
                        //选择一个靠近目标的方向
                        curDir = CastTool.CastVector2ToDirection(forwardVec);
                        switch (curDir)
                        {
                            case Direction.down:
                                if (!IsGridRoad(tempGrid + Vector2Int.down))
                                {
                                    tempGrid += Vector2Int.up;
                                }
                                break;
                            case Direction.up:
                                if (IsGridRoad(tempGrid + Vector2Int.up))
                                {
                                    tempGrid += Vector2Int.up;
                                }
                                break;
                            case Direction.right:
                                if (IsGridRoad(tempGrid + Vector2Int.right))
                                {
                                    tempGrid += Vector2Int.right;
                                }
                                break;
                            case Direction.left:
                                if (!IsGridRoad(tempGrid + Vector2Int.left))
                                {
                                    tempGrid -= Vector2Int.left;
                                }
                                break;
                        }
                        bool canLeft = IsGridRoad(tempGrid + CastTool.CastDirectionToVector2Int((int)curDir + 1));
                        bool canRight = IsGridRoad(tempGrid + CastTool.CastDirectionToVector2Int((int)curDir - 1));
                        if (canLeft && canRight)
                        {
                            forwardVec = Vector3.Cross(forward, target).y <= 0 ?
                                CastTool.CastDirectionToVector2Int((int)curDir + 1) :
                                CastTool.CastDirectionToVector2Int((int)curDir - 1);
                            state = FindRoadState.straight;
                            //Debug.Log("4"+ tempGrid);
                            RoadNodes.Add(tempGrid);
                            break;
                        }
                        else if (canLeft)
                        {
                            forwardVec = CastTool.CastDirectionToVector2Int((int)curDir + 1);
                            state = FindRoadState.straight;
                            //Debug.Log("5" + tempGrid);
                            RoadNodes.Add(tempGrid);
                            break;
                        }
                        else if (canRight)
                        {
                            forwardVec = CastTool.CastDirectionToVector2Int((int)curDir - 1);
                            state = FindRoadState.straight;
                            RoadNodes.Add(tempGrid);
                            //Debug.Log("6");
                            break;

                        }
                        else
                        {
                            forwardVec = -forwardVec;
                            state = FindRoadState.backward;
                            break;
                        }
                    }
            }
        }
        RoadNodes.Add(end);
        return MapManager.Instance.GetTerrainPosition(RoadNodes);
    }

    private bool IsGridRoad(Vector2Int nextGrid)
    {
        return MapManager.Instance.GetGridType(nextGrid) == GridType.road;
    }

    private DriveSystem GetCarFromPool(TransportationType type)
    {
        for (int i = 0; i < carUnusedPool.Count; i++)
        {
            if (carUnusedPool[i].carType == type)
            {
                DriveSystem driveSystem = carUnusedPool[i];
                carUnusedPool.RemoveAt(i);
                carUsingPool.Add(driveSystem);
                //Debug.Log("get");
                return driveSystem;
            }
        }
        GameObject newCar = Instantiate(carPfbs[(int)type], transform);
        newCar.transform.position = hidePos;
        DriveSystem system = newCar.GetComponent<DriveSystem>();
        carUsingPool.Add(system);
        //Debug.Log("new");
        return system;
    }

    private void RecycleCar(DriveSystem driveSystem, UnityAction unityAction)
    {
        carUsingPool.Remove(driveSystem);
        carUnusedPool.Add(driveSystem);
        driveSystem.transform.position = hidePos;
        driveSystem.action = null;
        if (unityAction != null)
        {
            unityAction.Invoke();
        }
        //Debug.Log("hide");
    }
}

public class CarMission
{
    public List<Vector3> route;
    public List<CostResource> costResources;
    public CarMissionType missionType;
}