using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CommonIcon : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    #region 组件
    [SerializeField] private TMP_Text _numLabel;
    [SerializeField] private Image _image;
    #endregion

    #region 字段&属性
    public ItemData data { get; private set; }
    private const string _iconBundle = "icon.ab";


    #endregion

    public static GameObject GetIcon(int itemID,float itemNum)
    {
        GameObject newIcon = Instantiate(LoadAB.Load(_iconBundle, "iconPfb"));
        CommonIcon icon = newIcon.GetComponent<CommonIcon>();
        icon._numLabel.text = CastTool.RoundOrFloat(itemNum);
        icon.data = DataManager.GetItemDataById(itemID);
        string iconName = icon.data.Name;
        icon._image.sprite = LoadAB.LoadSprite(_iconBundle, iconName+"Icon");
        icon._image.SetNativeSize();
        icon._numLabel.outlineWidth = 0.3f;
        icon._numLabel.outlineColor = Color.black;
        return newIcon;
    }

    public static GameObject GetIcon(CostResource costResource)
    {
        return GetIcon(costResource.ItemId, costResource.ItemNum);
    }

    public void SetIcon(int itemID, float itemNum)
    {
        if (itemNum != 0)
        {
            _numLabel.text = CastTool.RoundOrFloat(itemNum);
            _numLabel.outlineWidth = 0.3f;
            _numLabel.outlineColor = Color.black;
        }
        else
        {
            _numLabel.gameObject.SetActive(false);
        }
        data = DataManager.GetItemDataById(itemID);
        string iconName = data.Name;
        _image.sprite = LoadAB.LoadSprite(_iconBundle, iconName + "Icon");
        _image.SetNativeSize();
    }
    #region 接口
    public void OnPointerEnter(PointerEventData eventData)
    {
        NoticeManager.Instance.ShowIconNotice(Localization.ToSettingLanguage(data.Name));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        NoticeManager.Instance.CloseNotice();
    }

    #endregion
}
