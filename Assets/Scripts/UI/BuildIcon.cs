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

    public Vector3 originPos;



    public void Init(BuildData buildData,BuildingCanvas buildingCanvas)
    {
        BuildData = buildData;
        _buildingCanvas = buildingCanvas;
        //ToDo初始化图片
        if (!string.IsNullOrEmpty(buildData.iconName))
        {
            _image.sprite = LoadAB.LoadSprite("icon.ab", buildData.iconName);
        }
        _image.SetNativeSize();
        _name.text = Localization.ToSettingLanguage(BuildData.Name);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _buildingCanvas.OnClickIconToBuild(BuildData);
        _image.transform.localPosition = Vector3.zero;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _image.transform.position += Vector3.up * 10;
        _buildingCanvas.OnEnterHoverIcon(BuildData,transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _image.transform.position -= Vector3.up * 10;
        _buildingCanvas.OnExitHoverIcon(BuildData);
    }
}
