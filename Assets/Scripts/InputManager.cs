using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InputManager : Singleton<InputManager>
{
    #region 字段
    public float _CameraMoveSpeed = 10f;
    public float _CameraRotateSpeed = 100f;
    #endregion
    /// <summary>
    /// 处理玩家输入事件
    /// </summary>
    public void Update()
    {
        if (Input.anyKey)
        {
            OnKeyDown();
            //当有按键输入的时候响应
            EventManager.TriggerEvent(ConstEvent.OnCameraMove);
        }
    }

    private void OnKeyDown()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Camera.main.transform.parent.position += Camera.main.transform.parent.forward * Time.deltaTime * _CameraMoveSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Camera.main.transform.parent.position += Camera.main.transform.parent.right * Time.deltaTime * -_CameraMoveSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Camera.main.transform.parent.position += Camera.main.transform.parent.right * Time.deltaTime * _CameraMoveSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Camera.main.transform.parent.position += Camera.main.transform.parent.forward * Time.deltaTime * -_CameraMoveSpeed;
        }
        if (Input.GetKey(KeyCode.E))
        {
            Camera.main.transform.parent.Rotate(Vector3.up,Time.deltaTime * -_CameraRotateSpeed,Space.World);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            Camera.main.transform.parent.Rotate(Vector3.up,Time.deltaTime * _CameraRotateSpeed, Space.World);
        }
    }

}
