using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MarketBuyItem : MonoBehaviour
{
    [SerializeField] private Transform _iconRoot;
    [SerializeField] private Button[] _tradeOptionBtns;
    [SerializeField] private Button[] _numOptionBtns;
    [SerializeField] private CommonIcon _sellIcon;
    [SerializeField] private CommonIcon _earnIcon;

    [SerializeField] private GameObject _tradeOptionObj;
    [SerializeField] private GameObject _numOptionObj;
    [SerializeField] private GameObject _previewObj;
    [SerializeField] private GameObject _acceptBtn;
    [SerializeField] private GameObject _cancelBtn;


    private ItemData _itemData;
    private TradeMode _tradeMode;
    private int _tradeNum;
    public Action _onClickOkJumpAction;
    public void ResetView()
    {
        _itemData = null;
        _tradeMode = TradeMode.none;
        _tradeNum = 0;
        _tradeOptionObj.SetActive(false);
        _numOptionObj.SetActive(false);
        _previewObj.SetActive(false);
        _acceptBtn.SetActive(false);
        _cancelBtn.SetActive(false);
        if (_iconRoot.childCount == 0)
        {
            ItemData[] itemArray = DataManager.GetItemDatas();
            for (int i = 0; i < itemArray.Length; i++)
            {
                //排除金钱和人力资源
                if (itemArray[i].Id == 10000 || itemArray[i].Id == 99999 || itemArray[i].Id == 11000)
                {
                    continue;
                }
                GameObject item = CommonIcon.GetIcon(itemArray[i].Id, 0);
                item.transform.SetParent(_iconRoot);
                item.transform.localScale = Vector3.one *1.3f;
                item.GetComponent<CommonIcon>().SetIconWithCallback(itemArray[i].Id,OnItemSelect);
            }
        }
        
        for (int i = 0; i < _tradeOptionBtns.Length; i++)
        {
            _tradeOptionBtns[i].interactable = true;
        }
        EventManager.TriggerEvent(ConstEvent.OnSelectIcon,-1);
    }

    private void OnItemSelect(ItemData itemData)
    {
        _itemData = itemData;
        _tradeOptionObj.SetActive(true);
        _cancelBtn.SetActive(true);
        
        _earnIcon.SetIcon(_itemData.Id,_tradeNum);
        _sellIcon.SetIcon(99999,1.5f * _tradeNum * _itemData.Price);
    }

    public void OnClickTradeOptionButton(GameObject sender)
    {
        switch (sender.name)
        {
            case "once":
                _tradeMode = TradeMode.once;
                break;
            case "week":
                _tradeMode = TradeMode.everyWeek;
                break;
            case "month":
                _tradeMode = TradeMode.everyMonth;
                break;
        }
        Button btn = sender.GetComponent<Button>();
        for (int i = 0; i < _tradeOptionBtns.Length; i++)
        {
            _tradeOptionBtns[i].interactable = _tradeOptionBtns[i] != btn;
        }
        _numOptionObj.SetActive(true);
    }
    
    public void OnClickNumOptionButton(GameObject sender)
    {
        switch (sender.name)
        {
            case "m100":
                _tradeNum -= 100;
                break;
            case "m10":
                _tradeNum -= 10;
                break;
            case "m1":
                _tradeNum -= 1;
                break;
            case "gui0":
                _tradeNum = 0;
                break;
            case "a1":
                _tradeNum += 1;
                break;
            case "a10":
                _tradeNum += 10;
                break;
            case "a100":
                _tradeNum += 100;
                break;
        }

        _tradeNum = _tradeNum > 0 ? _tradeNum : 0;
        _previewObj.SetActive(true);
        _acceptBtn.SetActive(true);
        
        _earnIcon.SetIcon(_itemData.Id,_tradeNum);
        _sellIcon.SetIcon(99999,1.5f * _tradeNum * _itemData.Price);
    }

    public void OnClickOk()
    {
        float needMoney = 1.5f * _tradeNum * _itemData.Price;
        if (ResourceManager.Instance.IsResourceEnough(99999, needMoney))
        {
            MarketManager.Instance.AddBuyOrder(_itemData.Id, _tradeNum, _tradeMode == TradeMode.everyMonth);
            ResourceManager.Instance.UseResources(99999, needMoney);
            _onClickOkJumpAction?.Invoke();
        }
        else
        {
            //todo 提示金钱不足
        }
        
    }

}