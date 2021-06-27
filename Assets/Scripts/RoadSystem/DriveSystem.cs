using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DriveSystem : MonoBehaviour
{
    public Vector3[] WayPoints;
    public float MaxSpeed = 3f;
    private float speed;
    public float StopDistance = 1f;
    private float a { get { return (MaxSpeed * MaxSpeed) / (2 * StopDistance); } }
    public TransportationType carType;
    public bool isbraking;//是否在刹车
    private float RotateSpeed = 10f;
    private int wayCount = 0;
    public CarSensor carSensor;
    public UnityAction action = null;
    private bool isForward = true;
    private float curTime;
    private float turnTime;
    public delegate void ArriveDestination();
    public ArriveDestination OnArriveDestination;
    private UnityAction _callback;
    [SerializeField] Transform leftWheel, rightWheel;
    private float missionTimer = 0;
    private float brakeTimer = 0;
    private float BrakeMaxTime = 10f;//单次刹车最长时间
    private float MissionMaxTime = 120f;//单次任务最长时间;
    private bool pause = false;
    private float speedBuff = 1;
    public CarMission CurMission { get; private set; }
    private void Start()
    {
        if (carType != TransportationType.harvester)
        {
            carSensor.OnBrake += () => isbraking = true;
            carSensor.OnStopBrake += () => isbraking = false;
        }
    }
    private void FixedUpdate()
    {
        if (null != action)
        {
            action.Invoke();
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

    private void PauseGame()
    {
        pause = true;
    }
    private void ResumeGame()
    {
        pause = false;
    }
    public void StartDriving(CarMission carMission, DriveType driveType = DriveType.once, UnityAction callBack = null)
    {
        //Debug.Log("startDrive");
        _callback = callBack;
        action = null;
        wayCount = 0;
        speed = 3;
        brakeTimer = 0;
        missionTimer = 0;
        //Debug.Log((callBack != null).ToString());
        
        WayPoints = Vector3Serializer.Unbox(carMission.wayPoints);
        
        CurMission = carMission;
        //Debug.Log(carMission.missionType);
        EventManager.StartListening(ConstEvent.OnPauseGame, PauseGame);
        EventManager.StartListening(ConstEvent.OnResumeGame, ResumeGame);
        if (WayPoints.Length == 0)
        {
            DriveStop(_callback);
        }
        transform.position = WayPoints[0];
        transform.forward = WayPoints[1] - WayPoints[0];
        switch (driveType)
        {
            case DriveType.once:
                {
                    action = () => DriveOnce(WayPoints, _callback);
                    break;
                }
        }

    }

    private void OvertimeStop()
    {
        DriveStop(_callback);
    }
    private void ControlSpeed(Vector3[] targets)
    {
        if (isbraking)
        {
            brakeTimer += Time.fixedDeltaTime;
            if (brakeTimer > BrakeMaxTime)
            {
                isbraking = false;
                brakeTimer = 0;
            }
            if (speed > 0)
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
    private void DriveOnce(Vector3[] targets, UnityAction callBack)
    {
        if (pause) return;
        ControlSpeed(targets);
        CheckMissionOvertime();
        if (wayCount < targets.Length && Vector3.Distance(targets[wayCount], transform.position) < StopDistance)
        {
            wayCount++;
            CurMission.wayCount = wayCount;
            if (wayCount < targets.Length)
            {
                speedBuff = MapManager.GetGridNode(MapManager.GetCenterGrid(targets[wayCount])).passSpeed;
                float angle = Vector3.Angle(transform.position - targets[wayCount], targets[wayCount - 1] - transform.position);
                if (angle > 5 && angle < 175)
                {
                    UnityAction temp = action;
                    float dis = (targets[wayCount - 1] - transform.position).magnitude;
                    RotateSpeed = 90 / (dis * Mathf.PI / 2 / speed);
                    turnTime = dis * Mathf.PI / 2 / speed;
                    curTime = 0;
                    action = () => DriveTurn(targets[wayCount] - targets[wayCount - 1], temp, targets[wayCount - 1], dis);
                }
                else
                {
                    transform.LookAt(targets[wayCount]);
                }
            }
        }
        if (wayCount < targets.Length)
        {
            transform.position = Vector3.MoveTowards(transform.position, targets[wayCount], speed * Time.fixedDeltaTime);
        }
        else if (wayCount > 0 && Vector3.Distance(targets[wayCount - 1], transform.position) > 0.5f)
        {
            //Debug.Log("end");
            transform.position = Vector3.MoveTowards(transform.position, targets[wayCount - 1], speed * Time.fixedDeltaTime);
        }
        else
        {
            //Debug.Log("Stop");
            DriveStop(callBack);
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
        if (Vector3.Distance(targets[wayCount], transform.position) < StopDistance)
        {
            wayCount++;
            if (wayCount < targets.Count)
            {
                UnityAction temp = action;
                RotateSpeed = Vector3.Angle(transform.position - targets[wayCount], targets[wayCount - 1] - transform.position) / (Mathf.PI / 2 * StopDistance / speed);
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
            UnityAction temp = action;
            RotateSpeed = Vector3.Angle(transform.position - targets[0], targets[targets.Count - 1] - transform.position) / (Mathf.PI / 2 * StopDistance / speed);
            //Debug.Log("转弯");
            //action = () => DriveTurn(targets[0] - targets[targets.Count - 1], temp);
            wayCount = 0;
        }
    }

    private void DriveYoyo(List<Vector3> targets)
    {
        if (Vector3.Distance(targets[wayCount], transform.position) < StopDistance)
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
                UnityAction temp = action;
                RotateSpeed = Vector3.Angle(transform.position - targets[wayCount], targets[old] - transform.position) / (Mathf.PI / 2 * StopDistance / speed);
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
            UnityAction temp = action;
            if (isForward)
            {
                RotateSpeed = 90;
                //Debug.Log("转弯");
                action = () => StopTurn(targets[targets.Count - 2] - targets[targets.Count - 1], temp);
                wayCount = targets.Count - 1;
                isForward = false;
            }
            else
            {
                RotateSpeed = 90;
                //Debug.Log("转弯");
                action = () => StopTurn(targets[1] - targets[0], temp);
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
    private void DriveTurn(Vector3 to, UnityAction callback, Vector3 turn, float dis)
    {
        if (pause) return;
        CheckMissionOvertime();
        curTime += Time.fixedDeltaTime;
        var temp = Vector3.Cross(transform.forward, to).y;
        if (temp > 0)
        {
            transform.Rotate(new Vector3(0, RotateSpeed * Time.fixedDeltaTime, 0), Space.Self);
        }
        else
        {
            transform.Rotate(new Vector3(0, -RotateSpeed * Time.fixedDeltaTime, 0), Space.Self);
        }
        transform.position += transform.forward.normalized * speed * Time.fixedDeltaTime;
        if (curTime >= turnTime)
        {
            //Debug.Log(Vector3.Angle(transform.forward, to));
            //Debug.Log("前进");
            transform.position = turn + to.normalized * dis;
            transform.LookAt(turn + to);
            action = callback;
        }
    }

    private void StopTurn(Vector3 to, UnityAction callback)
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
            //Debug.Log("前进");
            action = callback;
        }
    }
    private void DriveStop(UnityAction callBack)
    {
        //Debug.Log("停车"+(callBack!=null).ToString());
        if (OnArriveDestination != null)
        {
            OnArriveDestination();
        }
        action = null;
        if(callBack!=null)
        {
            callBack.Invoke();
        }
        EventManager.StopListening(ConstEvent.OnPauseGame, PauseGame);
        EventManager.StopListening(ConstEvent.OnResumeGame, ResumeGame);
        //ClearCarMission();
    }
}

