using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DriveSystem : MonoBehaviour
{
    public List<Vector3> WayPoints;
    public List<GameObject> wayObjects;
    public float MaxSpeed = 3f;
    private float speed;
    public float StopDistance = 1f;
    private float a { get { return (MaxSpeed * MaxSpeed) / (2 * StopDistance); } }
    public DriveType driveType;
    public TransportationType carType;
    private float RotateSpeed = 10f;
    private int wayCount = 0;
    public UnityAction action = null;
    private bool isForward = true;
    private void FixedUpdate()
    {
        if (null != action)
        {
            action.Invoke();
        }
    }

    public void StartDriving(List<Vector3> wayPoints, DriveType driveType = DriveType.once, UnityAction _callBack = null)
    {
        //Debug.Log("startDrive");
        action = null;
        wayCount = 0;
        speed = 0;
        //Debug.Log((callBack != null).ToString());
        WayPoints = wayPoints;
        transform.position = WayPoints[0];
        if (WayPoints[0] == WayPoints[1])
        {
            WayPoints.RemoveAt(0);
        }
        transform.forward = WayPoints[1] - WayPoints[0];
        switch (driveType)
        {
            case DriveType.once:
                {
                    action = () => DriveOnce(WayPoints, _callBack);
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

    private void DriveOnce(List<Vector3> targets, UnityAction callBack)
    {
        if (speed < MaxSpeed && wayCount < targets.Count)
        {
            speed += a * Time.fixedDeltaTime;
        }
        if (speed > 0 && wayCount == targets.Count)
        {
            speed -=  a * Time.fixedDeltaTime;
        }
        if (wayCount < targets.Count && Vector3.Distance(targets[wayCount], transform.position) < StopDistance )
        {
            wayCount++;
            if(wayCount < targets.Count)
            {
                float angle = Vector3.Angle(transform.position - targets[wayCount], targets[wayCount - 1] - transform.position);
                if (angle > 5 && angle < 175)
                {
                    UnityAction temp = action;
                    RotateSpeed = 90 / (Mathf.PI / 2 * StopDistance / speed);
                    //Debug.Log("转弯");
                    action = () => DriveTurn(targets[wayCount] - targets[wayCount - 1], temp);
                }
            }
        }
        if (wayCount < targets.Count)
        {
            transform.position = Vector3.MoveTowards(transform.position, targets[wayCount], speed * Time.fixedDeltaTime);
        }
        else if(wayCount>0&&Vector3.Distance(targets[wayCount-1], transform.position) >0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targets[wayCount-1], speed * Time.fixedDeltaTime);
            
        }
        else
        {
            DriveStop(callBack);
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
                RotateSpeed = Vector3.Angle(transform.position - targets[wayCount], targets[wayCount - 1] - transform.position) / (Mathf.PI / 2 * StopDistance / speed);
                //Debug.Log("转弯");
                action = () => DriveTurn(targets[wayCount] - targets[wayCount - 1], temp);
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
                RotateSpeed = Vector3.Angle(transform.position - targets[wayCount], targets[old] - transform.position) / (Mathf.PI / 2 * StopDistance / speed);
                //Debug.Log("转弯");
                action = () => DriveTurn(targets[wayCount] - targets[old], temp);
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
        transform.position += transform.forward.normalized * speed * Time.fixedDeltaTime;
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
    private void DriveStop(UnityAction callBack)
    {
        //Debug.Log("停车"+(callBack!=null).ToString());
        action = null;
        callBack.Invoke();
        callBack = null;
    }
}
