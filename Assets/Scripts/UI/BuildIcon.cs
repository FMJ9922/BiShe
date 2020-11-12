using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BuildIcon : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler,IPointerExitHandler
{
    [SerializeField]
    private Image _image;
    [SerializeField]
    private TMP_Text _name;
    [SerializeField]
    private BuildingCanvas _buildingCanvas;

    public BuildData BuildData;



    public void Init(BuildData buildData,BuildingCanvas buildingCanvas)
    {
        BuildData = buildData;
        _buildingCanvas = buildingCanvas;
        //ToDo初始化图片
        _name.text = BuildData.Name;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _buildingCanvas.OnClickIconToBuild(BuildData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _buildingCanvas.OnEnterHoverIcon(BuildData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _buildingCanvas.OnExitHoverIcon(BuildData);
    }
}
