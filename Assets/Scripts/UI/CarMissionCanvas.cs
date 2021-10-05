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
        //Debug.Log("open");
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
        nameLabel.text = Localization.Get(mission.transportationType.GetDescription());
        startLabel.text = Localization.Get(MapManager.Instance.GetBuilidngByEntry(mission.StartBuilding).runtimeBuildData.Name);
        endLabel.text = Localization.Get(MapManager.Instance.GetBuilidngByEntry(mission.EndBuilding).runtimeBuildData.Name);
    }
    private void InitIcons(CarMission mission)
    {
        switch (mission.missionType)
        {
            case CarMissionType.requestResources:
                {
                    carryLabel.text = Localization.Get("Request") + Localization.Get("Goods") + ":";
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
                    carryLabel.text = Localization.Get("Transport") + Localization.Get("Goods") + ":";
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
        mainCanvas.transform.position = screenPos+new Vector3(0,440,0) * GameManager.Instance.GetScreenRelativeRate(); ;
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

}
