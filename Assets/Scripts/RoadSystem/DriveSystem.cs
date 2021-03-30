using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DriveSystem : MonoBehaviour
{
    public List<Vector3> WayPoints;
    public List<GameObject> wayObjects;
    public float Speed = 3f;
    public float StopDistance = 1f;
    public DriveType driveType;
    public TransportationType carType;
    private float RotateSpeed = 10f;
    private int wayCount = 0;
    public UnityAction action = null;
    private UnityAction<DriveSystem> callBack = null;
    private bool isForward = true;
    private void Start()
    {
        
        //StartDriving(ObjectsToVector3s(wayObjects), driveType);

    }
    private void FixedUpdate()
    {
        if (null != action)
        {
            action.Invoke();
        }
    }

    public void StartDriving(List<Vector3> wayPoints, DriveType driveType = DriveType.once,UnityAction<DriveSystem> _callBack = null)
    {
        action = null;
        wayCount = 0;
        callBack = _callBack;
        WayPoints = wayPoints;
        transform.position = WayPoints[0];
        if(WayPoints[0] == WayPoints[1])
        {
            WayPoints.RemoveAt(0);
        }
        transform.forward = WayPoints[1] - WayPoints[0];
        switch (driveType)
        {
            case DriveType.once:
                {
                    action = () => DriveOnce(WayPoints);
                    break;
                }
            case DriveType.loop:
                {
                    action = () => DriveLoop(WayPoints);
                    break;
                }
            case DriveType.yoyo:
                {
                    action = () => DriveYoyo(WayPoints);
                    break;
                }
        }
    }

    private void DriveOnce(List<Vector3> targets)
    {
        if (Vector3.Distance(targets[wayCount], transform.position) < StopDistance)
        {
            wayCount++;
            if (wayCount < targets.Count)
            {
                UnityAction temp = action;
                RotateSpeed = Vector3.Angle(transform.position - targets[wayCount], targets[wayCount - 1] - transform.position) / (StopDistance*Mathf.PI / (Speed*2));
                Debug.Log("转弯");
                action = () => DriveTurn(targets[wayCount] - targets[wayCount - 1], temp);
            }

        }
        if (wayCount < targets.Count)
        {
            transform.position = Vector3.MoveTowards(transform.position, targets[wayCount], Speed * Time.fixedDeltaTime);
        }
        else
        {
            DriveStop();
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
        if (Vector3.Distance(targets[wayCount], transform.position) < StopDistance)
        {
            wayCount++;
            if (wayCount < targets.Count)
            {
                UnityAction temp = action;
                RotateSpeed = Vector3.Angle(transform.position - targets[wayCount], targets[wayCount - 1] - transform.position) / (Mathf.PI / 2 * StopDistance / Speed);
                //Debug.Log("转弯");
                action = () => DriveTurn(targets[wayCount] - targets[wayCount - 1], temp);
            }

        }
        if (wayCount < targets.Count)
        {
            transform.position = Vector3.MoveTowards(transform.position, targets[wayCount], Speed * Time.fixedDeltaTime);
        }
        else
        {
            UnityAction temp = action;
            RotateSpeed = Vector3.Angle(transform.position - targets[0], targets[targets.Count - 1] - transform.position) / (Mathf.PI / 2 * StopDistance / Speed);
            //Debug.Log("转弯");
            action = () => DriveTurn(targets[0] - targets[targets.Count - 1], temp);
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
                RotateSpeed = Vector3.Angle(transform.position - targets[wayCount], targets[old] - transform.position) / (Mathf.PI / 2 * StopDistance / Speed);
                //Debug.Log("转弯");
                action = () => DriveTurn(targets[wayCount] - targets[old], temp);
            }

        }
        if (wayCount < targets.Count && wayCount >= 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, targets[wayCount], Speed * Time.fixedDeltaTime);
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
    private void DriveTurn(Vector3 to, UnityAction callback)
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
        transform.position += transform.forward.normalized * Speed * Time.fixedDeltaTime;
        if (Vector3.Angle(transform.forward, to) < 2f)
        {
            //Debug.Log("前进");
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
    private void DriveStop()
    {
        //Debug.Log("停车");
        action = null;
        if (callBack != null)
        {
            callBack.Invoke(this);
            callBack = null;
        }
    }
}
