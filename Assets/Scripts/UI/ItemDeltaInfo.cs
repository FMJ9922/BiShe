using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemDeltaInfo : MonoBehaviour
{
    [SerializeField] TMP_Text _buildingName;
    [SerializeField] TMP_Text _buildingDelta;
    private List<BuildingBase> _buildings;
    private int count = 0;
    public void Init(string name,string delta,List<BuildingBase> buildings)
    {
        _buildingName.text = name;
        _buildingDelta.text = delta;
        if (buildings == null)
        {
            transform.GetComponentInChildren<Button>().gameObject.SetActive(false);
        }
        else
        {
            _buildings = buildings;
        }
    }

    public void OnButtonClick()
    {
        MainInteractCanvas.Instance.CloseInfoCanvas();
        MainInteractCanvas.Instance.OpenInfoCanvas(_buildings[count]);
        CameraMovement.Instance.MoveToTarget(_buildings[count].parkingGridIn);
        count++;
        if (count >= _buildings.Count)
        {
            count = 0;
        }
    }
}
