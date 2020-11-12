using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InputManager : Singleton<InputManager>
{
    #region 字段
    public Vector3 LastGroundRayPos = new Vector3(0, 0, 0);
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

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Ground") && LastGroundRayPos != hit.point && Cursor.lockState != CursorLockMode.Locked)
            {
                LastGroundRayPos = hit.point;
                EventManager.TriggerEvent(ConstEvent.OnGroundRayPosMove, hit.point);
            }
        }
    }

    private void OnKeyDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            EventManager.TriggerEvent(ConstEvent.OnMouseLeftButtonDown);
        }
        if (Input.GetMouseButtonDown(1))
        {
            EventManager.TriggerEvent(ConstEvent.OnMouseRightButtonDown);
        }
        //if (Input.GetKey(KeyCode.W))
        //{
        //    Camera.main.transform.parent.position += Camera.main.transform.parent.forward * Time.deltaTime * _CameraMoveSpeed;
        //}
        //if (Input.GetKey(KeyCode.A))
        //{
        //    Camera.main.transform.parent.position += Camera.main.transform.parent.right * Time.deltaTime * -_CameraMoveSpeed;
        //}
        //if (Input.GetKey(KeyCode.D))
        //{
        //    Camera.main.transform.parent.position += Camera.main.transform.parent.right * Time.deltaTime * _CameraMoveSpeed;
        //}
        //if (Input.GetKey(KeyCode.S))
        //{
        //    Camera.main.transform.parent.position += Camera.main.transform.parent.forward * Time.deltaTime * -_CameraMoveSpeed;
        //}
        if (Input.GetKeyDown(KeyCode.E))
        {
            EventManager.TriggerEvent(ConstEvent.OnRotateBuilding, 90f);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            EventManager.TriggerEvent(ConstEvent.OnRotateBuilding, -90f);
        }
    }

}
