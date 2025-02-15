﻿using System.Collections;
using System.Collections.Generic;
using Manager;
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

    private CarDriver _carDriver;

    private bool isTracing = false;
    public override void InitCanvas()
    {
        EventManager.StartListening<CarDriver>(ConstEvent.OnTriggerCarMissionPanel, OnOpen);
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown,StopTracing);
    }
    public void OnOpen(CarDriver carDriver)
    {
        CarMission mission = carDriver.GetCarMission();

        InitLabels(mission);
        InitIcons(mission);
        _carDriver = carDriver;
        TraceCarPosition();
        StartTracing();
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
            case CarMissionType.collectOrderGoods:
            {
                carryLabel.text = Localization.Get("Request") + Localization.Get("Goods") + ":";
                CleanUpAllAttachedChildren(_iconsParent);
                for (int i = 0; i < mission.requestResources.Count; i++)
                {
                    GameObject resource = CommonIcon.GetIcon(mission.requestResources[i].ItemId,
                        mission.requestResources[i].ItemNum);
                    resource.transform.parent = _iconsParent;
                    resource.transform.localScale = Vector3.one * 1.5f;
                }
            }
                break;
            case CarMissionType.transportResources:
            case CarMissionType.goForOrder:
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
            case CarMissionType.backFromOrder:
                carryLabel.text = Localization.Get("BackFromOrder");
                break;
            default:
                break;
        }
    }
    public override void OnClose()
    {
        //if (targetCar)
        {
            //targetCar.GetComponent<CarDriver>().OnArriveDestination -= StopTracing;
            //targetCar = null;
            mainCanvas.SetActive(false);
        }
    }
    private void OnDestroy()
    {
        mainCanvas.SetActive(false);
        EventManager.StopListening<CarDriver>(ConstEvent.OnTriggerInfoPanel, OnOpen);
    }

    private void TraceCarPosition()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(_carDriver.GetCurrentPos());
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

    private void Update()
    {
        if (isTracing &&_carDriver._curState != CarDriver.CarState.idle)
        {
            TraceCarPosition();
        }
        else
        {
            StopTracing();
        }
    }

}
