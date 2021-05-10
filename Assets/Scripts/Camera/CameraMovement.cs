using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : Singleton<CameraMovement>
{
    [Header("摄像机运动参数")]
    public float MaxSpeed = 0.5f;
    public float Accelerate = 0.5f;
    public float StopAccelerate = 1f;
    private float ScrollWheelSpeed = 10f;
    public float MaxScrollValue = 200f;
    public float MinScrollValue = -100f;

    public delegate void CameraMove();
    public event CameraMove OnCameraMove;

    private Transform _cameraTrans;
    private float _forwardSpeed = 0;
    private float _rightSpeed = 0;
    private float _scrollValue = 40;
    private bool canMove = true;
    private bool canScroll = true;

    protected override void InstanceAwake()
    {
        _cameraTrans = transform.GetChild(0);
        EventManager.StartListening<bool>(ConstEvent.OnLockScroll,LockScroll);
    }
    private void OnDestroy()
    {
        EventManager.StopListening<bool>(ConstEvent.OnLockScroll, LockScroll);
    }
    void Update()
    {
        if (!canMove)
        {
            return;
        }
        if (Input.GetKey(KeyCode.W))
        {
            _forwardSpeed = _forwardSpeed >= 0 ?
                Mathf.MoveTowards(_forwardSpeed, MaxSpeed, Accelerate * Time.deltaTime) :
                Mathf.MoveTowards(_forwardSpeed, 0, StopAccelerate * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            _forwardSpeed = _forwardSpeed <= 0 ?
                Mathf.MoveTowards(_forwardSpeed, -MaxSpeed, Accelerate * Time.deltaTime) :
                Mathf.MoveTowards(_forwardSpeed, 0, StopAccelerate * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            _rightSpeed = _rightSpeed <= 0 ?
                Mathf.MoveTowards(_rightSpeed, -MaxSpeed, Accelerate * Time.deltaTime) :
                Mathf.MoveTowards(_rightSpeed, 0, StopAccelerate * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            _rightSpeed = _rightSpeed >= 0 ?
                Mathf.MoveTowards(_rightSpeed, MaxSpeed, Accelerate * Time.deltaTime) :
                Mathf.MoveTowards(_rightSpeed, 0, StopAccelerate * Time.deltaTime);
        }
        if (!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)))
        {
            _forwardSpeed = Mathf.MoveTowards(_forwardSpeed, 0, StopAccelerate * Time.deltaTime);
        }
        if (!(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            _rightSpeed = Mathf.MoveTowards(_rightSpeed, 0, StopAccelerate * Time.deltaTime);
        }
        transform.position += MoveDirection(_forwardSpeed, _rightSpeed);

        if (!canScroll)
        {
            return;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            _scrollValue = Mathf.MoveTowards(_scrollValue, MaxScrollValue, ScrollWheelSpeed);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            _scrollValue = Mathf.MoveTowards(_scrollValue, MinScrollValue, ScrollWheelSpeed);
        }
        _cameraTrans.localPosition = new Vector3(-1f, -1.42f, 1f) * _scrollValue;
    }

    public void StopMovement()
    {
        //canMove = false;
        _forwardSpeed = 0;
        _rightSpeed = 0;
    }
    public void AllowMovement()
    {
        //canMove = true;
    }

    public void LockScroll(bool isLock)
    {
        canScroll = !isLock;
    }
    private Vector3 MoveDirection(float forwardSpeed, float rightSpeed)
    {
        return new Vector3(rightSpeed - forwardSpeed, 0, rightSpeed + forwardSpeed);
    }
    private Vector3 PlantVector3(Vector3 vector3)
    {
        return new Vector3(vector3.x, 0, vector3.z);
    }
}
