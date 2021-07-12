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
    public List<GridNode> reachable = new List<GridNode>();
    public List<GridNode> explored = new List<GridNode>();

    private List<Vector2Int> RoadNodes;
    private readonly Vector3 hidePos = new Vector3(-1000, -1000, -1000);

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

    public void InitSavedTrafficManager(CarMission[] carMissions)
    {
        for (int i = 0; i < carMissions.Length; i++)
        {
            if(carMissions[i] == null)
            {
                continue;
            }

            if (!MapManager.CheckOutOfMap(carMissions[i].carPosition.V3))
            {
                UseCarBySave(carMissions[i]);
            }
        }
    }
    public void UseCar(CarMission mission,DriveType driveType = DriveType.once)
    {
        DriveSystem driveSystem = GetCarFromPool(mission.transportationType);
        Vector2Int start = mission.StartBuilding;
        Vector2Int end = mission.EndBuilding;
        Vector3[] wayPoints = null;
        UnityAction action = null;
        if (mission.missionType == CarMissionType.harvest)
        {
            wayPoints = Vector3Serializer.Unbox(mission.wayPoints);
        }
        else
        {
            List<Vector3> list = MapManager.Instance.GetWayPoints(start, end);
            if (list != null)
            {
                wayPoints = list.ToArray();
            }
        }
        action = () => RecycleCar(driveSystem, MapManager.Instance.GetBuilidngByEntry(end));
        if (wayPoints!=null && wayPoints.Length > 0)
        {
            mission.wayPoints = Vector3Serializer.Box(wayPoints);
            driveSystem.StartDriving(mission, driveType, action);
        }
        else
        {
            //NoticeManager.Instance.InvokeShowNotice("寻路失败！坐标："+start+"=>"+end+"请检查是否有建筑没有连接到道路！");
            if (action != null)
            {
                action.Invoke();
            }
        }
    }

    public void UseCarBySave(CarMission mission, DriveType driveType = DriveType.once)
    {
        DriveSystem driveSystem = GetCarFromPool(mission.transportationType);
        Vector2Int start = MapManager.GetCenterGrid(mission.carPosition.V3);
        Vector2Int end = mission.EndBuilding;
        Vector3[] wayPoints;
        UnityAction action = null;
        if (mission.missionType == CarMissionType.harvest)
        {
            wayPoints = Vector3Serializer.Unbox(mission.wayPoints);
        }
        else if (start == end)
        {
            RecycleCar(driveSystem, MapManager.Instance.GetBuilidngByEntry(end));
            return;
        }
        else
        {
            wayPoints = MapManager.Instance.GetWayPoints(start, end).ToArray();
        }

        action = () => RecycleCar(driveSystem, MapManager.Instance.GetBuilidngByEntry(end));
        mission.wayPoints = Vector3Serializer.Box(wayPoints);
        if (wayPoints != null && wayPoints.Length > 0)
        {
            driveSystem.StartDriving(mission, driveType, action);
        }
        else
        {
            if (action != null)
            {
                action.Invoke();
            }
        }
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
        DriveSystem system = newCar.GetComponent<DriveSystem>();
        carUsingPool.Add(system);
        //Debug.Log("new");
        return system;
    }

    private void RecycleCar(DriveSystem driveSystem, BuildingBase destination ,UnityAction unityAction = null)
    {
        //Debug.Log("recycle");
        carUsingPool.Remove(driveSystem);
        carUnusedPool.Add(driveSystem);
        driveSystem.transform.position = hidePos;
        driveSystem.action = null;
        if (destination != null)
        {
            //Debug.Log("des");
            destination.OnRecieveCar(driveSystem.CurMission);
        }
        if (unityAction != null)
        {
            unityAction.Invoke();
        }
        //Debug.Log("hide");
    }

    public CarMission[] GetDriveDatas()
    {
        List<CarMission> driveDatas = new List<CarMission>();
        for (int i = 0; i < carUsingPool.Count; i++)
        {
            if (!MapManager.CheckOutOfMap(carUsingPool[i].transform.position))
            {
                driveDatas.Add(carUsingPool[i].CurMission);
            }
        }
        return driveDatas.ToArray();
    }
}

[System.Serializable]
public class CarMission
{
    public Vector3Serializer[] wayPoints;
    public int wayCount;
    public Vector3Serializer carPosition =  new Vector3Serializer();
    public bool isAnd;//资源是否是并，而不是或
    public List<CostResource> requestResources;//请求的资源
    public List<CostResource> transportResources;//运输的资源
    public CarMissionType missionType;//任务种类
    public TransportationType transportationType;
    public Vector2IntSerializer startBuilding;
    public Vector2IntSerializer endBuilding;
    public Vector2Int StartBuilding 
    { 
        get => startBuilding.Vector2Int;
        set => startBuilding = new Vector2IntSerializer(value);
    }
    public Vector2Int EndBuilding 
    {
        get => endBuilding.Vector2Int;
        set => endBuilding = new Vector2IntSerializer(value);
    }
}