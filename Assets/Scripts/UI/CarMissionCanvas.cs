using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CarMissionCanvas : CanvasBase
{
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private Transform _iconsParent;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text startLabel;
    [SerializeField] private TMP_Text endLabel;
    [SerializeField] private TMP_Text carryLabel;


    private GameObject targetCar;
    private bool isTracing = false;
    public override void InitCanvas()
    {
        EventManager.StartListening<GameObject>(ConstEvent.OnTriggerCarMissionPanel, OnOpen);
    }
    public void OnOpen(GameObject car)
    {
        Debug.Log("open");
        targetCar = car;
        CarMission mission = car.GetComponent<DriveSystem>().CurMission;

        InitLabels(mission);
        InitIcons(mission);

        TraceCarPosition();
        StartTracing();
        car.GetComponent<DriveSystem>().OnArriveDestination += StopTracing;
        mainCanvas.SetActive(true);
    }
    private void InitLabels(CarMission mission)
    {
        nameLabel.text = Localization.ToSettingLanguage(mission.transportationType.GetDescription());
        startLabel.text = Localization.ToSettingLanguage(mission.StartBuilding.runtimeBuildData.Name);
        endLabel.text = Localization.ToSettingLanguage(mission.EndBuilding.runtimeBuildData.Name);
    }
    private void InitIcons(CarMission mission)
    {
        switch (mission.missionType)
        {
            case CarMissionType.requestResources:
                {
                    carryLabel.text = Localization.ToSettingLanguage("Request") + Localization.ToSettingLanguage("Goods") + ":";
                    CleanUpAllAttachedChildren(_iconsParent);
                    for (int i = 0; i < mission.requestResources.Count; i++)
                    {
                        GameObject resource = CommonIcon.GetIcon(mission.requestResources[i].ItemId, mission.requestResources[i].ItemNum);
                        resource.transform.parent = _iconsParent;
                        resource.transform.localScale = Vector3.one * 1.5f;
                    }
                }
                break;
            case CarMissionType.transportResources:
                {
                    carryLabel.text = Localization.ToSettingLanguage("Transport") + Localization.ToSettingLanguage("Goods") + ":";
                    CleanUpAllAttachedChildren(_iconsParent);
                    for (int i = 0; i < mission.transportResources.Count; i++)
                    {
                        GameObject resource = CommonIcon.GetIcon(mission.transportResources[i].ItemId, mission.transportResources[i].ItemNum);
                        resource.transform.parent = _iconsParent;
                        resource.transform.localScale = Vector3.one * 1.5f;
                    }
                }
                break;
            default:
                break;
        }
    }
    public override void OnClose()
    {
        if (targetCar)
        {
            targetCar.GetComponent<DriveSystem>().OnArriveDestination -= StopTracing;
            targetCar = null;
            mainCanvas.SetActive(false);
        }
    }
    private void OnDestroy()
    {
        mainCanvas.SetActive(false);
        EventManager.StopListening<GameObject>(ConstEvent.OnTriggerInfoPanel, OnOpen);
    }

    private void TraceCarPosition()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(targetCar.transform.position);
        mainCanvas.transform.position = screenPos+new Vector3(0,440,0);
    }

    private void StartTracing()
    {
        isTracing = true;
    }


    private void StopTracing()
    {
        isTracing = false;
        OnClose();
    }

    private void LateUpdate()
    {
        if (isTracing && targetCar)
        {
            TraceCarPosition();
        }
    }

    /// <summary>
    /// 清除一个物体下的所有子物体
    /// </summary>
    /// <param name="target"></param>
    private void CleanUpAllAttachedChildren(Transform target)
    {
        for (int i = 0; i < target.childCount; i++)
        {
            Destroy(target.GetChild(i).gameObject);
        }

    }
}
