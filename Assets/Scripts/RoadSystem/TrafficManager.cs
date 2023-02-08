using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Building;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TrafficManager : Singleton<TrafficManager>
{

    [SerializeField] GameObject[] carPfbs;
        
        
    private List<CarModel> _carUsingPool = new List<CarModel>();
    private List<CarModel> _carUnusedPool = new List<CarModel>();

    private List<CarDriver> _carDriverUsingPool = new List<CarDriver>();
    private List<CarDriver> _carDriverUnusedPool = new List<CarDriver>();

    //寻路节点
    public List<GridNode> reachable = new List<GridNode>();
    public List<GridNode> explored = new List<GridNode>();

    private List<Vector2Int> RoadNodes;
    private readonly Vector3 hidePos = new Vector3(-1000, -1000, -1000);

    private Queue<RuntimeCarMission> queueCarMission = new Queue<RuntimeCarMission>();
    private RuntimeCarMission tempCarMission;

    private List<Vector3> _takenPosList = new List<Vector3>();


    public float WeeklyCost { get; set; }

    
    


    public void SetMission()
    {
        if (queueCarMission.Count > 0)
        {
            tempCarMission = queueCarMission.Dequeue();
            RealUse(tempCarMission);
        }
    }

    public bool IsNearOtherCar(Vector3 pos, Vector3 dir,float brakeDistanceSqr) 
    {
        for (int i = 0; i < _takenPosList.Count; i++)
        {
            var vector = _takenPosList[0] - pos;
            if (vector.sqrMagnitude < brakeDistanceSqr)
            {
                if (Vector3.Dot(vector, dir) > 0)
                {
                    return true;
                }
            }
        }

        return false;
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

    public void RealUse(RuntimeCarMission runtimeCarMission)
    {
        CarDriver carDriver = GetCarDriverFromPool();
        CarModel model = GetCarModelFromPool(runtimeCarMission._carMission.transportationType);
        carDriver.SetCarModel(model);
        model.SetDriver(carDriver);
        Vector2Int start = runtimeCarMission._carMission.StartBuilding;
        Vector2Int end = runtimeCarMission._carMission.EndBuilding;
        Vector3[] wayPoints = null;
        UnityAction action = null;
        if (runtimeCarMission._carMission.missionType == CarMissionType.harvest)
        {
            wayPoints = Vector3Serializer.Unbox(runtimeCarMission._carMission.wayPoints);
        }
        else
        {
            List<Vector3> list = MapManager.Instance.GetWayPoints(start, end);
            if (list != null)
            {
                wayPoints = list.ToArray();
            }
        }
        if (wayPoints != null && wayPoints.Length > 0)
        {
            runtimeCarMission._carMission.wayPoints = Vector3Serializer.Box(wayPoints);
            carDriver.StartDriving(runtimeCarMission._carMission, RecycleCarDriver);
            runtimeCarMission._callback?.Invoke(true);
        }
        else
        {
            runtimeCarMission._callback?.Invoke(false);
            action?.Invoke();
        }
    }
    public void UseCar(CarMission mission, Action<bool> callback = null, DriveType driveType = DriveType.once)
    {
        //Debug.Log(mission.StartBuilding);
        RuntimeCarMission runtimeCarMission = new RuntimeCarMission
        {
            _carMission = mission,
            _callback = callback,
            _driveType = driveType
        };
        queueCarMission.Enqueue(runtimeCarMission);
    } 

    public void UseCarBySave(CarMission mission, DriveType driveType = DriveType.once)
    {
        CarModel model = GetCarModelFromPool(mission.transportationType);
        CarDriver carDriver = GetCarDriverFromPool();
        carDriver.SetCarModel(model);
        model.SetDriver(carDriver);
        Vector2Int start = MapManager.GetCenterGrid(mission.carPosition.V3);
        Vector2Int end = mission.EndBuilding;
        Vector3[] wayPoints;
        if (mission.missionType == CarMissionType.harvest)
        {
            wayPoints = Vector3Serializer.Unbox(mission.wayPoints);
        }
        else if (start == end)
        {
            RecycleCarDriver(carDriver);
            return;
        }
        else
        {
            wayPoints = MapManager.Instance.GetWayPoints(start, end)?.ToArray();
        }
        mission.wayPoints = Vector3Serializer.Box(wayPoints);
        if (wayPoints != null && wayPoints.Length > 0)
        {
            carDriver.StartDriving(mission, RecycleCarDriver);
        }
        else
        {
            RecycleCarDriver(carDriver);
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.IsGamePause())
        {
            return;
        }
        float deltaTime = Time.fixedDeltaTime;
        SetMission();
        _takenPosList.Clear();
        for (int i = 0; i < _carDriverUsingPool.Count; i++)
        {
            _takenPosList.Add(_carDriverUsingPool[i].GetCurrentPos());
        }
        if (_carDriverUsingPool.Count>0)
        {
            for (int i = 0; i < _carDriverUsingPool.Count; i++)
            {
                var car = _carDriverUsingPool[i];
                car.DriveCar(deltaTime);
            }
        }
    }

    #region 弃用
    /*
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
    */
    #endregion

    private void OnDrawGizmos()
    {
        for (int i = 0; i < _carDriverUsingPool.Count; i++)
        {
            if (_carDriverUsingPool[i]._curState == CarDriver.CarState.driveBrake)
            {
                
            }
            else
            {
                Gizmos.DrawLine(_carDriverUsingPool[i]._controlPointA+Vector3.up*3,_carDriverUsingPool[i]._controlPointB+Vector3.up*3);  
            }
        }
    }
    #region 对象池
    private CarDriver GetCarDriverFromPool()
    {
        for (int i = 0; i < _carDriverUnusedPool.Count; i++)
        {
            CarDriver carDriver = _carDriverUnusedPool[i];
            _carDriverUnusedPool.RemoveAt(i);
            _carDriverUsingPool.Add(carDriver);
            return carDriver;
        }

        CarDriver driver = new CarDriver();
        _carDriverUsingPool.Add(driver);
        return driver;
    }

    private void RecycleCarDriver(CarDriver carDriver)
    {
        DriveCost(-carDriver.GetDriveCost());
        _carDriverUsingPool.Remove(carDriver);
        _carDriverUnusedPool.Add(carDriver);
        RecycleCarModel(carDriver.GetCarModel());
        carDriver.GetTransform().position = hidePos;
        carDriver.SetIdle();
        var destination =MapManager.Instance.GetBuilidngByEntry(carDriver.GetCarMission().endBuilding.Vector2Int) as ITransportation;
        destination?.OnRecieveCar(carDriver.GetCarMission());
    }

    private void DriveCost(float money)
    {
        WeeklyCost -= money;
        ResourceManager.Instance.AddMoney(money);
        EventManager.TriggerEvent<float>(ConstEvent.OnOilCost,WeeklyCost);
    }

    private CarModel GetCarModelFromPool(TransportationType type)
    {
        for (int i = 0; i < _carUnusedPool.Count; i++)
        {
            CarModel carModel = _carUnusedPool[i];
            if (type == carModel.CarType)
            {
                _carUnusedPool.RemoveAt(i);
                _carUsingPool.Add(carModel);
                carModel.enabled = true;
                return carModel;
            }
        }
        GameObject newCar = Instantiate(carPfbs[(int)type], transform);
        CarModel model = newCar.GetComponent<CarModel>();
        _carUsingPool.Add(model);
        return model;
    }
    
    private void RecycleCarModel(CarModel carModel)
    {
        carModel.enabled = false;
        _carUsingPool.Remove(carModel);
        _carUnusedPool.Add(carModel);
    }
    #endregion

    #region 保存
    public CarMission[] GetDriveDatas()
    {
        List<CarMission> driveDatas = new List<CarMission>();
        for (int i = 0; i < _carDriverUsingPool.Count; i++)
        {
            var pos = _carDriverUsingPool[i].GetTransform().position;
            if (!MapManager.CheckOutOfMap(pos))
            {
                var carMission = _carDriverUsingPool[i].GetCarMission();
                carMission.carPosition.Fill(pos);
                driveDatas.Add(carMission);
            }
        }
        return driveDatas.ToArray();
    }
    #endregion
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
public class RuntimeCarMission
{
    public CarMission _carMission;
    public Action<bool> _callback;
    public DriveType _driveType;
}