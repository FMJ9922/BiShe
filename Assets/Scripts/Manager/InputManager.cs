using System.Collections;
using System.Collections.Generic;
using Building;
using UI;
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
        if (Physics.Raycast(ray,out hit,10000,1<<LayerMask.NameToLayer("Ground")))
        {
            if (hit.collider.CompareTag("Ground") && LastGroundRayPos != hit.point && Cursor.lockState != CursorLockMode.Locked)
            {
                LastGroundRayPos = hit.point;
                //Debug.Log(hit.point);
                EventManager.TriggerEvent(ConstEvent.OnGroundRayPosMove, hit.point);
            }
        }
    }

    private void OnKeyDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //GridNode node = MapManager.GetGridNode(MapManager.GetCenterGrid(LastGroundRayPos));
            //Debug.Log(node.GridPos+" "+node.passSpeed+" "+node.enterCost+" "+node.direction+" "+node.gridType);
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.Instance.TogglePauseGame(out TimeScale scale);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameManager.Instance.SetOneTimeScale();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GameManager.Instance.SetTwoTimeScale();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GameManager.Instance.SetFourTimeScale();
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            EventManager.TriggerEvent(ConstEvent.OnRotateBuilding);
        }
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
        if (Physics.Raycast(ray, out hit,10000, ~(1 << LayerMask.NameToLayer("Enviornment"))) && !EventSystem.current.IsPointerOverGameObject() && !BuildManager.Instance.IsInBuildMode)
        {
            if (hit.collider.CompareTag("Building"))
            {
                BuildingBase buildingBase;
                buildingBase = hit.collider.gameObject.GetComponent<BuildingBase>();
                //Debug.Log(buildingBase.transform.name);
                int id = buildingBase.runtimeBuildData.Id;
                if (id == 20003||id == 20016||id == 20017)
                {
                    SoundManager.Instance.PlaySoundEffect(SoundResource.sfx_click_wareHouse);
                    MainInteractCanvas.Instance.OpenResourceCanvas(buildingBase);
                }
                else
                {
                    EventManager.TriggerEvent<BuildingBase>(ConstEvent.OnTriggerInfoPanel, buildingBase);
                }
            }
            else if (hit.collider.CompareTag("car"))
            {
                var carDriver = hit.collider.gameObject.GetComponent<CarModel>().GetDriver();
                MainInteractCanvas.Instance.OpenCarMissionCanvas(carDriver);
            }
            else if (hit.collider.CompareTag("Bridge"))
            {
                BuildingBase buildingBase;
                buildingBase = hit.collider.gameObject.GetComponent<BuildingBase>();
                EventManager.TriggerEvent<BuildingBase>(ConstEvent.OnTriggerInfoPanel, buildingBase);
            }
        }
    }
}
