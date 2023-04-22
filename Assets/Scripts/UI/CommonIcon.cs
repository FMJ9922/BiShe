using System;
using System.Collections;
using System.Collections.Generic;
using CSTools;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CommonIcon : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{
    #region 组件
    [SerializeField] private TMP_Text _numLabel;
    [SerializeField] private Image _image;
    [SerializeField] private GameObject _selectObj;
    #endregion

    #region 字段&属性
    public ItemData Data { get; private set; }
    private const string _iconBundle = "icon.ab";

    public static bool IsShowingOption = false;
    private Action<ItemData> _onClickAction;

    #endregion

    public static GameObject GetIcon(int itemID,float itemNum)
    {
        GameObject newIcon = Instantiate(LoadAB.Load(_iconBundle, "iconPfb"));
        CommonIcon icon = newIcon.GetComponent<CommonIcon>();
        icon._numLabel.text = CastTool.RoundOrFloat(itemNum);
        icon.Data = DataManager.GetItemDataById(itemID);
        string iconName = icon.Data.Name;
        icon._image.sprite = LoadAB.LoadSprite(_iconBundle, iconName+"Icon");
        icon._image.SetNativeSize();
        icon._numLabel.outlineWidth = 0.3f;
        icon._numLabel.outlineColor = Color.black;
        icon._onClickAction = (ItemData itemData) =>
        {
            IsShowingOption = true;
            NoticeManager.Instance.ShowIconOption(itemData);
        };
        return newIcon;
    }

    public static GameObject GetIcon(CostResource costResource)
    {
        return GetIcon(costResource.ItemId, costResource.ItemNum);
    }

    public void SetIcon(int itemId, float itemNum)
    {
        if (itemNum != 0)
        {
            _numLabel.text = CastTool.RoundOrFloat(itemNum);
            _numLabel.gameObject.SetActive(true);
        }
        else
        {
            _numLabel.gameObject.SetActive(false);
        }
        Data = DataManager.GetItemDataById(itemId);
        string iconName = Data.Name;
        _image.sprite = LoadAB.LoadSprite(_iconBundle, iconName + "Icon");
        _onClickAction = (ItemData itemData) =>
        {
            IsShowingOption = true;
            NoticeManager.Instance.ShowIconOption(itemData);
        };
    }

    public void SetIconWithCallback(int itemId, Action<ItemData> callback)
    {
        _numLabel.gameObject.SetActive(false);
        Data = DataManager.GetItemDataById(itemId);
        string iconName = Data.Name;
        _image.sprite = LoadAB.LoadSprite(_iconBundle, iconName + "Icon");
        _onClickAction = callback;
        _selectObj.SetActive(false);
        EventManager.StartListening<int>(ConstEvent.OnSelectIcon,SetSelect);
    }
    #region 接口
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsShowingOption)
        {
            NoticeManager.Instance.ShowIconNotice(Localization.Get(Data.Name));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsShowingOption)
        {
            NoticeManager.Instance.CloseNotice();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _onClickAction?.Invoke(Data);
        EventManager.TriggerEvent(ConstEvent.OnSelectIcon,Data.Id);
    }

    public void SetSelect(int itemId)
    {
        _selectObj.SetActive(Data.Id == itemId);
    }

    private void OnDisable()
    {
        EventManager.StopListening<int>(ConstEvent.OnSelectIcon,SetSelect);
    }

    #endregion
}
