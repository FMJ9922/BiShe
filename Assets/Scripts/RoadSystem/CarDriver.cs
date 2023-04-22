using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CarDriver
{
    #region Property
    
    //在任务开始时可以确认的参数
    #region ConstSetting 
    
    private Vector3[] _wayPoints;//路径点
    private float[] _wayPointSpacing;//路径点的间距
    private TransportationType _carType;//车辆类型
    private float _missionMaxTime = 120f;//单次任务最长时间
    private float _speedBuff = 1;//速度加成
    private CarMission _curMission;//车辆的任务
    private float _maxDistance;//从起点，经过每个路径点，到终点所经历的路程
    private float _controlDistance = 3f;//两个控制点距离
    private CarModel _carModel;
    private CarData _carData;
    #endregion
    
    //运行时参数
    #region RunTimeData
    private Action<CarDriver> _callback;//完成任务回调
    private float _timer = 0;//这个任务开始多久了
    private float _speed;//当前速度
    public CarState _curState;//当前车辆的状态
    private float _curProgress;//从出发点到目前行进的距离
    private int _curWayCount;//当前已路过的最新的路径点计数，从0开始
    public Vector3 _controlPointA;//控制点
    public Vector3 _controlPointB;
    private Vector3 _lastPos;
    private Vector3 _brakeDir;//刹车方向，是离自己最近的车辆的向量
    private TrafficManager _trafficManager;
    
    #endregion
    #endregion

    //车辆状态枚举
    public enum CarState
    {
        driveStart,//开始
        driveForward,//极速
        driveBrake,//刹车
        driveEnd,//结束
        idle,//处于对象池中
    }
    
    #region Public

    public CarMission GetCarMission()
    {
        return _curMission;
    }

    public Transform GetTransform()
    {
        return _carModel.GetTransform();
    }

    public void SetCarModel(CarModel model)
    {
        _carModel = model;
        _carData = DataManager.GetCarData(model.CarType);
    }

    public CarModel GetCarModel()
    {
        return _carModel;
    }
    
    #endregion

    #region  StateControl

    
    private void OvertimeStop()
    {
        DriveStop();
    }
    
    private void DriveStop()
    {
        _callback?.Invoke(this);
    }
    
    public void StartDriving(CarMission carMission, Action<CarDriver> callBack = null)
    {
        _curMission = carMission;
        _wayPoints = new Vector3[_curMission.wayPoints.Length];
        _callback = callBack;
        _curState = CarState.driveForward;
        _timer = 0f;
        _speed = 0f;
        _maxDistance = CaluculateMaxDistance(_curMission.wayPoints,ref _wayPointSpacing);
        _controlPointA = _wayPoints[0];
        _controlPointB = _controlPointA;
        _curProgress = 0;
        if (!_trafficManager)
        {
            _trafficManager = TrafficManager.Instance;
        }
    } 
    
    public void SetIdle()
    {
        _carModel = null;
        _curState = CarState.idle;
        _wayPoints = null;
        _wayPointSpacing = null;
    }

    public Vector3 GetCurrentPos()
    {
        return _lastPos;
    }

    public float GetDriveCost()
    {
        return _timer * _carData.Cost;
    }

    #endregion
    
    #region Calculate

    //数据计算，每帧调用
    public void DriveCar(float deltaTime)
    {
        if (_curState != CarState.idle)
        {
            ControlSpeed();
            ControlPosition();
            SetPosition();
            CheckTimer(deltaTime);
            _carModel?.ChangeWheelDirection();
        }
    }

    private void SetPosition()
    {
        if (_carModel)
        {
            var trans = _carModel.GetTransform();
            _lastPos = trans.position;
            var newPos = (_controlPointA + _controlPointB) / 2;
            trans.position= newPos;
            trans.LookAt(_controlPointA,Vector3.up);
        }
    }

    private void ControlPosition()
    {
        //如果马上到终点
        if (_curProgress > _maxDistance)
        {
            DriveStop();
            return;
        }
        CalculateCurWayCount();
        //否则就是在路上
        _curProgress += _speed * Time.fixedDeltaTime;
        _controlPointA = CalculatePosByDis(_curProgress + _controlDistance * _speed / 2/_carData.MaxSpeed);
        _controlPointB = CalculatePosByDis(_curProgress - _controlDistance * _speed/ 2/_carData.MaxSpeed);
    }

    private void CheckTimer(float deltaTime)
    {
        _timer += deltaTime;
        if (_timer > _missionMaxTime)
        {
            OvertimeStop();
        }
    }
    private Vector3 CalculatePosByDis(float distance)
    {
        float tProgress;
        if (distance < 0)
        {
            tProgress = 0;
        }
        else if (distance > _maxDistance)
        {
            tProgress = _maxDistance;
        }
        else
        {
            tProgress = distance;
        }
        int counter = 0;
        while (tProgress > 0&&counter<_wayPointSpacing.Length)
        {
            tProgress -= _wayPointSpacing[counter];
            counter++;
        }
        int wayCount = counter - 1;
        if (wayCount < 0)
        {
            var ratio = tProgress/ _wayPointSpacing[0];
            return Vector3.Lerp(_wayPoints[0], _wayPoints[1],ratio);
        }
        else
        {
            var ratio = (tProgress+ _wayPointSpacing[wayCount])/ _wayPointSpacing[wayCount];
            return Vector3.Lerp(_wayPoints[wayCount], _wayPoints[wayCount+1],ratio);
        }
    }

    private void ControlSpeed()
    {
        var transform = _carModel.transform;
        _curState = _trafficManager.IsNearOtherCar(transform.position,transform.forward,40f) ? CarState.driveBrake : CarState.driveForward;
        switch (_curState)
        {
            case CarState.driveForward:
                if (_speed < _carData.MaxSpeed * _speedBuff)
                {
                    _speed += _carData.Acceleration * _speedBuff * Time.fixedDeltaTime;
                }
                else
                {
                    _speed = _carData.MaxSpeed * _speedBuff;
                }

                break;
            case CarState.driveBrake:

                _speed -= 10 * _carData.Acceleration * _speedBuff * Time.fixedDeltaTime;
                if (_speed < 0)
                {
                    _speed = 0;
                }

                break;
        }
    }

    
    //计算当前所在的路径点
    private void CalculateCurWayCount()
    {
        float tProgress = _curProgress;
        int counter = 0;
        while (tProgress > 0)
        {
            tProgress -= _wayPointSpacing[counter];
            counter++;
        }
        _curWayCount = counter;
    }

    private float GetWayPointDistanceFromBegin(int warPoint)
    {
        float ret = 0;
        for (int i = 0; i < warPoint; i++)
        {
            ret += _wayPointSpacing[i];
        }
        return ret;
    }

    private float CaluculateMaxDistance(Vector3Serializer[] vecs,ref float[] spacings)
    {
        float ret = 0;
        if (vecs.Length <= 1)
        {
            return ret;
        }

        Vector3 previousVector3 = vecs[0].V3;
        spacings = new float[vecs.Length - 1];
        _wayPoints[0] = previousVector3;
        for (int i = 1; i < vecs.Length; i++)
        {
            Vector3 tempVector3 = vecs[i].V3;
            spacings[i - 1] = GetDistance(previousVector3, tempVector3);
            ret += spacings[i - 1];
            previousVector3 = tempVector3;
            _wayPoints[i] = previousVector3;
        }
        return ret;
    }

    private float GetDistance(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b);
    }
    
    #endregion

}

