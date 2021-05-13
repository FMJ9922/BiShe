using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class InputManager : Singleton<InputManager>
{
    #region 字段
    public Vector3 LastGroundRayPos = new Vector3(0, 0, 0);
    #endregion

    private void Start()
    {
        EventManager.StartListening(ConstEvent.OnMouseLeftButtonDown, TriggerInfoPanel);
    }
    private void OnDestroy()
    {
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, TriggerInfoPanel);
    }

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
        if (Physics.Raycast(ray,out hit,1000,1<<LayerMask.NameToLayer("Ground")))
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
        if (Input.GetMouseButton(0))
        {
            EventManager.TriggerEvent(ConstEvent.OnMouseLeftButtonHeld);
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
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    EventManager.TriggerEvent(ConstEvent.OnRotateBuilding, 90f);
        //}
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    EventManager.TriggerEvent(ConstEvent.OnRotateBuilding, -90f);
        //}
    }

    /// <summary>
    /// 鼠标左键点击建筑显示详细面板
    /// </summary>
    private void TriggerInfoPanel()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && !EventSystem.current.IsPointerOverGameObject() && !BuildManager.IsInBuildMode)
        {
            if (hit.collider.CompareTag("Building"))
            {
                BuildingBase buildingBase;
                buildingBase = hit.collider.gameObject.GetComponent<BuildingBase>();
                //Debug.Log(buildingBase.transform.name);
                int id = buildingBase.runtimeBuildData.Id;
                if (id == 20003||id == 20016||id == 20017)
                {
                    MainInteractCanvas.Instance.OpenResourceCanvas();
                }
                else
                {
                    EventManager.TriggerEvent<BuildingBase>(ConstEvent.OnTriggerInfoPanel, buildingBase);
                }
            }
            else if (hit.collider.CompareTag("car"))
            {
                CarMission carMission = hit.collider.gameObject.GetComponent<DriveSystem>().CurMission;
                if (carMission != null)
                {
                    MainInteractCanvas.Instance.OpenCarMissionCanvas(hit.collider.gameObject);
                }
            }
        }
    }
}
