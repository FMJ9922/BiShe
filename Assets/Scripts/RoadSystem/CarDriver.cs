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
        _carModel.CarSensor.CleanUpSensor();
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

/*
    #region Calculate
    private Vector3 Force()
    {
        //如果走到了最后一个路径点
        if (wayCount == WayPoints.Length - 1)
        {
            return ForceByApproachingDestination();
        }
        else//如果在不是最后一个路径点
        {
            //return ForceByPredictFuturePos();
            return ForceByBesselNextTwoWayPoints();
        }
    }

    private Vector3 Brake()
    {
        //如果不许刹车或者速度小于1，让刹车力变为0
        if (_dontBrake || _velocity.sqrMagnitude < 1)
        {
            return Vector3.zero;
        }
        //获取需要躲避的方向
        Vector3 dir = carSensor.GetAvoidDir();
        if (dir != Vector3.zero)
        {
            //Vector3 a = Vector3.up;
            //float angle = Vector3.Angle(a,dir);
            //Vector3 dirNew = dir * a.magnitude * Mathf.Cos(Mathf.Deg2Rad * angle);
            float dis = dir.sqrMagnitude;
            Vector3 dirNew = new Vector3(dir.x,0,dir.z);
            return dirNew * (1+_avoidForce / (dis + 1));
        }
        else
        {
            return dir;
        }
    }
    
    private void ControlSpeed()
    {
        //计算小车缩受的合力 = 牵引力+刹车力
        //_force = Force();//+Brake();
        //牵引力不能超过最大值
        //_force = Vector3.ClampMagnitude(_force, _maxForce);
        //加速度
        //_acceleration = _force / _mass;
        //速度+=加速度*单位时间
        _velocity = DirectionByBesselNextTwoWayPoints()*MaxSpeed;
        //_velocity = Vector3.ClampMagnitude(_velocity, MaxSpeed);
        
        //新的位置= 原位置+位移
        var transform1 = transform;
        Vector3 adjustPos = transform1.position + _velocity * Time.fixedDeltaTime;
        transform1.position = adjustPos;
        //设置小车头的朝向
        if (_velocity.sqrMagnitude > 0.0001f)
        {
            Vector3 newForward = Vector3.Slerp(transform.forward, _velocity, _turnSpeed * Time.fixedDeltaTime);
            transform.forward = newForward;
        }
        //记录当前小车的位置
        CurMission.carPosition.Fill(transform.position);
        //设置轮子的朝向
        if (_velocity.sqrMagnitude>1&&leftWheel != null && rightWheel != null)
        {
            var position = leftWheel.position;
            leftWheel.LookAt(position * 2 - leftOrigin, transform.up);
            var position1 = rightWheel.position;
            rightWheel.LookAt(position1 * 2 - rightOrigin, transform.up);
            leftOrigin = position;
            rightOrigin = position1;
        }
    }
    
    private Vector3 DirectionByBesselNextTwoWayPoints()
    {
        if (wayCount > WayPoints.Length - 1)
        {
            wayCount = WayPoints.Length - 1;
        }
        //下一个路径点的坐标
        var pos1 = WayPoints[wayCount];
        //下下个路径点的坐标
        var pos2 = wayCount + 1 > WayPoints.Length - 1 ? pos1 : WayPoints[wayCount + 1];
        //当前位置
        var nowPos = transform.position;
        //距离下个路径点的距离
        float x = (pos1 - nowPos).magnitude;
        //二次贝塞尔曲线的控制值，取值应该是0-1
        float y = Mathf.Pow(0.99f, x);
        //根据贝塞尔曲线，计算出牵引力指向
        var toTarget = Vector3.Lerp(pos1,pos2,y ) - nowPos;
        Debug.Log(y);
        if (x < 1.5f)
        {
            //那么路径点计数增加
            wayCount++;
        }
        if (wayCount > WayPoints.Length-1 && x < _stopDistance)
        {
            DriveStop();
        }

        return toTarget.normalized;
    }

    //通过预测未来位置来确定牵引力的方向
    private Vector3 ForceByPredictFuturePos()
    {
        //10帧之后，车会出现的位置
        var forecastFuturePos = transform.position + _velocity * Time.fixedDeltaTime * 10;
        //目标方向的向量：下一个路径点减去10帧后预测的位置
        var toTarget = WayPoints[wayCount] - forecastFuturePos;
        //剩余的距离
        var remainDistance = toTarget.magnitude;
        //不刹车
        _dontBrake = false;
        //如果十帧后到下一个路径点的距离 小于设置的开始转向（切换成下一个路径点）的距离
        if (remainDistance < _startTurnDistance)
        {
            //那么路径点计数增加
            wayCount++;
        }
        //期望的速度方向 = 目标方向的单位向量 * 最大速度 * 地面提供的速度buff - 当前速度
        var desiredVelocity = toTarget.normalized * MaxSpeed * speedBuff-_velocity;
        //牵引力方向设置为期望的速度方向
        return desiredVelocity;
    }

    //通过接下来两个路径点结合贝塞尔曲线确定力的方向
    private Vector3 ForceByBesselNextTwoWayPoints()
    {
        //下一个路径点的坐标
        var pos1 = WayPoints[wayCount];
        //下下个路径点的坐标
        var pos2 = wayCount + 1 > WayPoints.Length - 1 ? pos1 : WayPoints[wayCount + 1];
        //当前位置
        var nowPos = transform.position;
        //距离下个路径点的距离
        float x = (pos1 - nowPos).magnitude - TrafficManager.Instance.Slider2Value;
        //二次贝塞尔曲线的控制值，取值应该是0-1
        float y = Mathf.Pow(TrafficManager.Instance.Slider1Value, x);
        //根据贝塞尔曲线，计算出牵引力指向
        var toTarget = Vector3.Lerp(pos1,pos2,y ) - nowPos;
        
        if (x < _startTurnDistance)
        {
            //那么路径点计数增加
            wayCount++;
        }

        return toTarget.normalized * MaxSpeed * speedBuff-_velocity;
    }

    private Vector3 ForceByApproachingDestination()
    {
        var toTarget = WayPoints[wayCount] - (transform.position+_velocity);
        var remainDistance = toTarget.magnitude;
        if(remainDistance > _startTurnDistance)
        {
            return  toTarget * MaxSpeed * speedBuff-_velocity;
        }
        else
        {
            _dontBrake = true;
            if (remainDistance < _stopDistance)
            {
                DriveStop();
            }
            return toTarget-_velocity;
        } 
    }

    #endregion
    
    #region DebugView
    void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + _force);
    }

    #endregion
    
    
    #region OldLogic
    private void DriveOnce()
    {
        if (LevelManager.GetPause()) return;
        CheckMissionOvertime();
        bool isTurn = false;
        if(wayCount < WayPoints.Length - 1)
        {
            float dis = Vector3.Distance(WayPoints[wayCount], transform.position);
            if (dis < 3 * _stopDistance)
            {
                float angle = Vector3.Angle(transform.position - WayPoints[wayCount + 1], WayPoints[wayCount] - transform.position);
                if (angle > 5 && angle < 175)
                {
                    isTurn = true;
                }
            }
        }
        if (wayCount < WayPoints.Length)
        {
            float dis = Vector3.Distance(WayPoints[wayCount], transform.position);
            if (dis < _stopDistance)
            {
                wayCount++;
                CurMission.wayCount = wayCount;
                if (wayCount < WayPoints.Length)
                {
                    speedBuff = MapManager.GetGridNode(MapManager.GetCenterGrid(WayPoints[wayCount])).passSpeed;
                    float angle = Vector3.Angle(transform.position - WayPoints[wayCount], WayPoints[wayCount - 1] - transform.position);
                    if (angle > 5 && angle < 175)
                    {
                        _curTurningControlPosList.Clear();
                        float turnDis = dis * Mathf.PI / 2;
                        float turnTime = turnDis / speed;
                        int turnCount = (int)(turnTime / Time.fixedDeltaTime);
                        float onceAngle = 90 / turnCount;
                        Vector3 to = WayPoints[wayCount] - WayPoints[wayCount - 1];
                        Vector3 turn = wayCount>1? WayPoints[wayCount - 1]- WayPoints[wayCount - 2]:WayPoints[wayCount - 1] - transform.position;
                        for (int i = 0; i <= turnCount; i++)
                        {
                            float x = -Mathf.Cos(i * onceAngle*Mathf.Deg2Rad);
                            float y = Mathf.Sin(i * onceAngle*Mathf.Deg2Rad);
                            Vector3 point = WayPoints[wayCount - 2] + to * (1 + x) + turn * y;
                            _curTurningControlPosList.Add(point);
                        }
                        _turnCount = 1;
                        curState = State.driveTurn;
                    }
                    else
                    {
                        transform.LookAt(WayPoints[wayCount]);
                    }
                }
            }
        }

        ControlSpeed(WayPoints, isTurn);
        if (wayCount < WayPoints.Length)
        {
            transform.position = Vector3.MoveTowards(transform.position, WayPoints[wayCount], speed * Time.fixedDeltaTime);
        }
        else if (wayCount > 0 && Vector3.Distance(WayPoints[wayCount - 1], transform.position) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, WayPoints[wayCount - 1], speed * Time.fixedDeltaTime);
        }
        else
        {
            DriveStop();
        }
        if (leftWheel != null && rightWheel != null)
        {
            leftWheel.LookAt(leftWheel.position*2 - leftOrigin, transform.up);
            rightWheel.LookAt(rightWheel.position*2 - rightOrigin, transform.up);
            leftOrigin = leftWheel.position;
            rightOrigin = rightWheel.position;
        }
        CurMission.carPosition.Fill(transform.position);
    }



    public void SetCarMission(CarMission carMission)
    {
        CurMission = carMission;
        CurMission.carPosition = new Vector3Serializer();
    }
    public void ClearCarMission()
    {
        CurMission = null;
    }

    private void ControlSpeed(Vector3[] targets,bool isAtTuring)
    {
        if (wayCount != targets.Length&&carSensor._otherCar.Count>0)
        {
            brakeTimer += Time.fixedDeltaTime;
            if (brakeTimer > BrakeMaxTime)
            {
                brakeTimer = 0;
            }
            if (speed > 2)
            {
                speed -= a * Time.fixedDeltaTime;
            }
        }
        else if (isAtTuring)
        {
            if (speed > 5 )
            {
                speed -= a * Time.fixedDeltaTime;
            }
        }
        else
        {
            if (speed < MaxSpeed * speedBuff && wayCount < targets.Length)
            {
                speed += a * Time.fixedDeltaTime;
            }
            if (speed > MaxSpeed * speedBuff || speed > 2 && wayCount == targets.Length)
            {
                speed -= a * Time.fixedDeltaTime;
            }
        }
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
    private void DriveLoop(List<Vector3> targets)
    {
        if (Vector3.Distance(targets[wayCount], transform.position) < _stopDistance)
        {
            wayCount++;
            if (wayCount < targets.Count)
            {
                RotateSpeed = Vector3.Angle(transform.position - targets[wayCount], targets[wayCount - 1] - transform.position) / (Mathf.PI / 2 * _stopDistance / speed);
                //Debug.Log("转弯");
                //action = () => DriveTurn(targets[wayCount] - targets[wayCount - 1], temp);
            }

        }
        if (wayCount < targets.Count)
        {
            transform.position = Vector3.MoveTowards(transform.position, targets[wayCount], speed * Time.fixedDeltaTime);
        }
        else
        {
            RotateSpeed = Vector3.Angle(transform.position - targets[0], targets[targets.Count - 1] - transform.position) / (Mathf.PI / 2 * _stopDistance / speed);
            //Debug.Log("转弯");
            //action = () => DriveTurn(targets[0] - targets[targets.Count - 1], temp);
            wayCount = 0;
        }
    }

    private void DriveYoyo(List<Vector3> targets)
    {
        if (Vector3.Distance(targets[wayCount], transform.position) < _stopDistance)
        {
            int old = wayCount;
            if (isForward)
            {
                wayCount++;
            }
            else
            {
                wayCount--;
            }
            if (wayCount < targets.Count && wayCount >= 0)
            {
                RotateSpeed = Vector3.Angle(transform.position - targets[wayCount], targets[old] - transform.position) / (Mathf.PI / 2 * _stopDistance / speed);
                //Debug.Log("转弯");
                //action = () => DriveTurn(targets[wayCount] - targets[old], temp);
            }

        }
        if (wayCount < targets.Count && wayCount >= 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, targets[wayCount], speed * Time.fixedDeltaTime);
        }
        else
        {
            if (isForward)
            {
                RotateSpeed = 90;
                //Debug.Log("转弯");
                wayCount = targets.Count - 1;
                isForward = false;
            }
            else
            {
                RotateSpeed = 90;
                //Debug.Log("转弯");
                wayCount = 0;
                isForward = true;
            }

        }
    }

    private void DriveTurn()
    {
        if (LevelManager.GetPause()) return;
        CheckMissionOvertime();
        /*Vector3 to = WayPoints[wayCount] - WayPoints[wayCount - 1];
        Vector3 turn = WayPoints[wayCount - 1];
        float dis = Vector3.Distance(WayPoints[wayCount], transform.position);
        var temp = Vector3.Cross(transform.forward, to).y;
        if (temp > 0)
        {
            transform.Rotate(new Vector3(0, RotateSpeed * Time.fixedDeltaTime, 0), Space.Self);
        }
        else
        {
            transform.Rotate(new Vector3(0, -RotateSpeed * Time.fixedDeltaTime, 0), Space.Self);
        }
        if(_turnCount< _curTurningControlPosList.Count-1)
        {
            transform.position = _curTurningControlPosList[_turnCount];
            transform.LookAt(_curTurningControlPosList[_turnCount +1], transform.up);
            _turnCount++;
        }
        else
        {
            transform.position = WayPoints[wayCount];
            curState = State.driveForward;
        }
        if (leftWheel != null && rightWheel != null)
        {
            leftWheel.LookAt(leftWheel.position * 2 - leftOrigin, transform.up);
            rightWheel.LookAt(rightWheel.position * 2 - rightOrigin, transform.up);
            leftOrigin = leftWheel.position;
            rightOrigin = rightWheel.position;
        }
        CurMission.carPosition.Fill(transform.position);
    }

    private void StopTurn(Vector3 to)
    {
        var temp = Vector3.Cross(transform.forward, to).y;
        if (temp > 0)
        {
            transform.Rotate(new Vector3(0, RotateSpeed * Time.fixedDeltaTime, 0), Space.Self);
        }
        else
        {
            transform.Rotate(new Vector3(0, -RotateSpeed * Time.fixedDeltaTime, 0), Space.Self);
        }
        if (Vector3.Angle(transform.forward, to) < 1f)
        {
            curState = State.driveForward;
        }
    }
    
    
    #endregion*/
}

