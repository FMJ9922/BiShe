using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DriveSystem : MonoBehaviour
{
    public Vector3[] WayPoints;
    public float MaxSpeed = 3f;
    private float speed;
    private float a { get { return (MaxSpeed * MaxSpeed) / (2 * _stopDistance); } }
    public TransportationType carType;
    public bool isbraking;//是否在刹车
    private float RotateSpeed = 10f;
    private int wayCount = 0;
    public CarSensor carSensor;
    private bool isForward = true;
    public delegate void ArriveDestination();
    public ArriveDestination OnArriveDestination;
    private UnityAction _callback;
    [SerializeField] Transform leftWheel, rightWheel;
    private Vector3 leftOrigin, rightOrigin;
    private float missionTimer = 0;
    private float brakeTimer = 0;
    private float BrakeMaxTime = 10f;//单次刹车最长时间
    private float MissionMaxTime = 120f;//单次任务最长时间;
    //private bool pause = false;
    private float speedBuff = 1;
    private int _turnCount = 1;

    private List<Vector3> _curTurningControlPosList = new List<Vector3>();

    #region New
    float _slowDownDistance = 4f;
    float _startTurnDistance = 3f;
    float _stopDistance = 2f;
    Vector3 _force;
    float _maxForce = 10;
    Vector3 _velocity;
    Vector3 _acceleration;
    float _mass = 1;
    float _turnSpeed = 20;
    float _dragRate = 0.1f;

    #endregion
    public CarMission CurMission { get; private set; }

    private State curState;

    private enum State
    {
        driveStart,
        driveForward,
        driveTurn,
        driveEnd,
        idle,
    }
    private void Start()
    {
        if (carType != TransportationType.harvester)
        {
            carSensor.OnBrake += () => isbraking = true;
            carSensor.OnStopBrake += () => isbraking = false;
        }
    }

    private Vector3 Force()
    {
        Vector3 retforce;
        Vector3 toTarget = WayPoints[wayCount] - transform.position;
        Vector3 desiredVelocity;
        float remainDistance = toTarget.magnitude;
        if (wayCount == WayPoints.Length - 1)
        {
            if(remainDistance > _startTurnDistance)
            {
                desiredVelocity = toTarget.normalized * MaxSpeed * speedBuff;
                retforce = desiredVelocity;
            }
            else
            {
                desiredVelocity = (2*toTarget.normalized -_velocity.normalized)* MaxSpeed * speedBuff;
                retforce = desiredVelocity;
                if (remainDistance < _stopDistance)
                {
                    DriveStop();
                }
            }
        }
        else
        {
            if (remainDistance < _startTurnDistance)
            {
                wayCount++;
            }
            desiredVelocity = toTarget.normalized * MaxSpeed * speedBuff;
            retforce = desiredVelocity;
        }
        return retforce - _mass * _velocity * (1-Vector3.Dot(_velocity.normalized, desiredVelocity.normalized));
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + _force);
    }
    private void FixedUpdate()
    {
        switch (curState)
        {
            case State.idle:
                return;
            case State.driveStart:
            case State.driveForward:
            case State.driveTurn:
                ControlSpeed();
                break;
            case State.driveEnd:
                DriveStop();
                break;
        }
    }

    private void ControlSpeed()
    {
        _force = Force();
        _force = Vector3.ClampMagnitude(_force, _maxForce);
        _acceleration = _force / _mass;
        _velocity += _acceleration * Time.fixedDeltaTime;
        //_velocity = Vector3.ClampMagnitude(_velocity, MaxSpeed);
        transform.position += _velocity * Time.fixedDeltaTime;
        if (_velocity.sqrMagnitude > 0.0001f)
        {
            Vector3 newForward = Vector3.Slerp(transform.forward, _velocity, _turnSpeed * Time.fixedDeltaTime);
            transform.forward = newForward;
        }
        CurMission.carPosition.Fill(transform.position);
        if (_velocity.sqrMagnitude>1&&leftWheel != null && rightWheel != null)
        {
            leftWheel.LookAt(leftWheel.position * 2 - leftOrigin, transform.up);
            rightWheel.LookAt(rightWheel.position * 2 - rightOrigin, transform.up);
            leftOrigin = leftWheel.position;
            rightOrigin = rightWheel.position;
        }
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

    /*private void PauseGame()
    {
        pause = true;
    }
    private void ResumeGame()
    {
        pause = false;
    }*/
    public void StartDriving(CarMission carMission, DriveType driveType = DriveType.once, UnityAction callBack = null)
    {
        //Debug.Log("startDrive");
        _callback = callBack;
        wayCount = 0;
        brakeTimer = 0;
        missionTimer = 0;
        //Debug.Log((callBack != null).ToString());
        curState = State.driveStart;
        WayPoints = Vector3Serializer.Unbox(carMission.wayPoints);
        
        CurMission = carMission;
        //Debug.Log(carMission.missionType);
        //EventManager.StartListening(ConstEvent.OnPauseGame, PauseGame);
        //EventManager.StartListening(ConstEvent.OnResumeGame, ResumeGame);
        if (WayPoints.Length == 0)
        {
            DriveStop();
        }
        transform.position = WayPoints[0];
        transform.forward = WayPoints[1] - WayPoints[0];
        _velocity = 3* transform.forward;
        _maxForce = _velocity.sqrMagnitude;
        switch (driveType)
        {
            case DriveType.once:
                {
                    curState = State.driveForward;
                    break;
                }
        }

    }

    private void OvertimeStop()
    {
        DriveStop();
    }
    private void ControlSpeed(Vector3[] targets,bool isAtTuring)
    {
        if (isbraking&& wayCount != targets.Length&&carSensor.otherCar.Count>0)
        {
            brakeTimer += Time.fixedDeltaTime;
            if (brakeTimer > BrakeMaxTime)
            {
                isbraking = false;
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

    private void CheckMissionOvertime()
    {
        missionTimer += Time.fixedDeltaTime;
        if (missionTimer > MissionMaxTime)
        {
            OvertimeStop();
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
        }*/
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
    private void DriveStop()
    {
        //Debug.Log("停车"+(callBack!=null).ToString());
        if (OnArriveDestination != null)
        {
            OnArriveDestination();
        }
        if(_callback != null)
        {
            _callback.Invoke();
        }
        //EventManager.StopListening(ConstEvent.OnPauseGame, PauseGame);
        //EventManager.StopListening(ConstEvent.OnResumeGame, ResumeGame);
        //ClearCarMission();
    }

    public void SetIdle()
    {
        curState = State.idle;
    }
}

