﻿using System.Collections;
using System.Collections.Generic;
using Manager;
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
    private float _edgeRange = 10;
    private bool canMove = true;
    private bool canScroll = true;
    private bool canEdge = true;
    private bool isFocus = true;


    private Vector3 lastMousePos;

    public void MoveToTarget(Vector2Int targetGrid)
    {
        int x = targetGrid.x * 2 + 110;
        int z = targetGrid.y * 2 - 114;
        transform.position = new Vector3(x, 173.2f, z);
    }
    protected override void InstanceAwake()
    {
        _cameraTrans = transform.GetChild(0);
        EventManager.StartListening<bool>(ConstEvent.OnLockScroll,LockScroll);
        EventManager.StartListening<bool>(ConstEvent.OnLockMove, LockMove);
    }
    private void OnDestroy()
    {
        EventManager.StopListening<bool>(ConstEvent.OnLockScroll, LockScroll);
        EventManager.StopListening<bool>(ConstEvent.OnLockMove, LockMove);
    }
    void OnApplicationFocus(bool focus)
    {
        isFocus = focus;
        if (!isFocus)
        {
            _forwardSpeed = 0;
            _rightSpeed = 0;
        }
    }

    private bool isOutOfScene = false;

    private void LateUpdate()
    {
        if (canMove&& isFocus)
        {
            if (Input.mousePosition.y<0|| Input.mousePosition.x<0||
                Input.mousePosition.y> Screen.height|| Input.mousePosition.x > Screen.width)
            {
                isOutOfScene = true;
            }
            else
            {
                isOutOfScene = false;
            }
            float time = Time.unscaledDeltaTime;
            if (Input.GetKey(KeyCode.W)|| (Input.mousePosition.y >= Screen.height - _edgeRange&& canEdge && !isOutOfScene))
            {
                _forwardSpeed = _forwardSpeed >= 0 ?
                    Mathf.MoveTowards(_forwardSpeed, MaxSpeed, Accelerate * time) :
                    Mathf.MoveTowards(_forwardSpeed, 0, StopAccelerate * time);
            }
            if (Input.GetKey(KeyCode.S)|| (Input.mousePosition.y <= _edgeRange && canEdge && !isOutOfScene))
            {
                _forwardSpeed = _forwardSpeed <= 0 ?
                    Mathf.MoveTowards(_forwardSpeed, -MaxSpeed, Accelerate * time) :
                    Mathf.MoveTowards(_forwardSpeed, 0, StopAccelerate * time);
            }
            if (Input.GetKey(KeyCode.A)||(Input.mousePosition.x <= _edgeRange && canEdge && !isOutOfScene))
            {
                _rightSpeed = _rightSpeed <= 0 ?
                    Mathf.MoveTowards(_rightSpeed, -MaxSpeed, Accelerate * time) :
                    Mathf.MoveTowards(_rightSpeed, 0, StopAccelerate * time);
            }
            if (Input.GetKey(KeyCode.D) || (Input.mousePosition.x >= Screen.width - _edgeRange && canEdge && !isOutOfScene))
            {
                _rightSpeed = _rightSpeed >= 0 ?
                    Mathf.MoveTowards(_rightSpeed, MaxSpeed, Accelerate * time) :
                    Mathf.MoveTowards(_rightSpeed, 0, StopAccelerate * time);
            }
            if (!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || (Input.mousePosition.y >= Screen.height - _edgeRange || Input.mousePosition.y <= _edgeRange) && canEdge)|| isOutOfScene)
            {
                _forwardSpeed = Mathf.MoveTowards(_forwardSpeed, 0, StopAccelerate * time);
            }
            if (!(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || (Input.mousePosition.x <= _edgeRange || Input.mousePosition.x >= Screen.width - _edgeRange) && canEdge) || isOutOfScene)
            {
                _rightSpeed = Mathf.MoveTowards(_rightSpeed, 0, StopAccelerate * time);
            }
        }
        transform.position += MoveDirection(_forwardSpeed, _rightSpeed) / Mathf.Clamp((int)GameManager.Instance.GetRealTimeScale(), 1, 4);
        if (Input.GetMouseButton(2))
        {
            //Debug.Log(Input.mousePosition);
            Vector3 delta = Input.mousePosition - lastMousePos;
            float rate = (_scrollValue - MinScrollValue) / (MaxScrollValue - MinScrollValue);
            //Debug.Log((1 - rate) * (1 - rate) + 0.1f);
            AdjustPosition(new Vector3(delta.x - delta.y,0, delta.x + delta.y)*((1-rate)* (1-rate)+0.05f));
        }
        lastMousePos = Input.mousePosition;
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
        AdjustPosition();
    }

    private void AdjustPosition()
    {
        Vector3 pos = transform.position;
        float x = transform.position.x;
        float z = transform.position.z;
        x = Mathf.Clamp(x, 100, MapManager.MapSize.x * 2 + 100);
        z = Mathf.Clamp(z, -100, MapManager.MapSize.y * 2 - 100);
        transform.position = new Vector3(x, 173.2f, z);
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

    public void LockMove(bool isLock)
    {
        canMove = !isLock;
    }

    public void LockEdge(bool isLock)
    {
        canEdge = !isLock;
    }
    private Vector3 MoveDirection(float forwardSpeed, float rightSpeed)
    {
        return new Vector3(rightSpeed - forwardSpeed, 0, rightSpeed + forwardSpeed);
    }
    public void AdjustPosition(Vector3 delta)
    {
        transform.position += delta;
    }
    private Vector3 PlantVector3(Vector3 vector3)
    {
        return new Vector3(vector3.x, 0, vector3.z);
    }
}
