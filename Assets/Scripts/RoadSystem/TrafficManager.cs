using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Building;
using Manager;
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
    
    private List<Vector2Int> RoadNodes;
    private readonly Vector3 hidePos = new Vector3(-1000, -1000, -1000);

    private Queue<RuntimeCarMission> queueCarMission = new Queue<RuntimeCarMission>();
    private RuntimeCarMission tempCarMission;

    private List<Vector3> _takenPosList = new List<Vector3>();
    private int _carIndexCounter = 0;


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
    public void UseCar(CarMission mission, Action<bool> callback = null)
    {
        RuntimeCarMission runtimeCarMission = new RuntimeCarMission
        {
            _carMission = mission,
            _callback = callback,
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
        //WeeklyCost -= money;
        //ResourceManager.Instance.AddMoney(money);
        //EventManager.TriggerEvent<float>(ConstEvent.OnOilCost,WeeklyCost);
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
    public List<CostResource> requestResources;//请求的资源
    public List<CostResource> transportResources;//运输的资源
    public CarMissionType missionType;//任务种类
    public TransportationType transportationType;
    public Vector2IntSerializer startBuilding;
    public Vector2IntSerializer endBuilding;
    public Vector2IntSerializer belongToBuilding;
    public int orderIndex;//正在进行的订单index
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
    
    public Vector2Int BelongToBuilding 
    {
        get => belongToBuilding.Vector2Int;
        set => belongToBuilding = new Vector2IntSerializer(value);
    }
}
public class RuntimeCarMission
{
    public CarMission _carMission;
    public Action<bool> _callback;
    
}