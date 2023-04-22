using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 市场物品信息
/// </summary>
public class MarketItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _type;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private TMP_Text _target;
    [SerializeField] private TMP_Text _timeLimit;
    [SerializeField] private TMP_Text _timeRepeat;
    [SerializeField] private TMP_Text _progress;
    [SerializeField] private Slider _slider;
    [SerializeField] private GameObject _acceptBtn;
    [SerializeField] private TMP_Text _acceptText;
    [SerializeField] private CommonIcon _sellIcon;
    [SerializeField] private CommonIcon _earnIcon;
    [SerializeField] private TMP_Text _carInfo;

    private Action<int> _clickCallBack;
    private int orderIndex;
    private int orderId;
    public void InitMarketItem(OrderData data,Action<int> onClickCallback)
    {
        if (data.Type == 0)
        {
            _type.text = Localization.Get("Temporary");
            _timeRepeat.gameObject.SetActive(false);
        }
        else
        {
            _type.text = Localization.Get("Continuous");
            _timeRepeat.gameObject.SetActive(true);
        }

        _description.text = Localization.Get(data.DescriptionIds);
        _target.text = string.Format(Localization.Get("MarketOrderDestination"), data.Distance, Localization.Get(data.Destination));
        _timeLimit.text = string.Format(Localization.Get("MarketOrderTimeLimit"), data.TimeLimit);
        _timeRepeat.text = string.Format(Localization.Get("MarketOrderTimeRepeat"), data.RepeatTime);
        _slider.value = MarketManager.Instance.GetOrderAppearValue(data.ID);
        
         var itemData = DataManager.GetItemDataById(data.ItemId);

        _sellIcon.SetIcon(data.ItemId,data.ItemNum);
        _earnIcon.SetIcon(99999,data.GoodsPrice * data.ItemNum * itemData.Price);

        orderId = data.ID;
        _clickCallBack = onClickCallback;
        _acceptBtn.SetActive(true);
        _acceptText.text = String.Empty;
        _slider.interactable = false;
        RebuildLayout();
    }

    public void InitAcceptItem(RuntimeOrderData data,Action<int> onClickCallback)
    {
        orderIndex = (int)data.Index;
        EventManager.StopListening(ConstEvent.OnTransportingNumChange,RefreshTransportingCarNum);
        EventManager.StartListening(ConstEvent.OnTransportingNumChange,RefreshTransportingCarNum);
        _clickCallBack = onClickCallback;
        if (data.IsBuy)
        {
            if (data.IsRepeating)
            {
                _type.text = Localization.Get("Continuous");
            }
            else
            {
                _type.text = Localization.Get("Temporary");
            }
            _description.text = Localization.Get("MarketBuyNeedTip");
            _target.text = Localization.Get("MarketBuyTransportTip");
            if (data.IsRepeating)
            {
                _timeLimit.gameObject.SetActive(false);
                int remainWeek = data.StartWeek + 4 - LevelManager.Instance.WeekIndex;
                _timeRepeat.text = String.Format(Localization.Get("MarketBuyAutoTip"),remainWeek);
            }
            else
            {
                _timeLimit.gameObject.SetActive(false);
                _timeRepeat.gameObject.SetActive(false);
            }
        
        
            var itemData = DataManager.GetItemDataById(data.OrderId);

            _earnIcon.SetIcon(data.OrderId,data.PromiseTransportGoodsNum);
            _sellIcon.SetIcon(99999,1.5f * data.PromiseTransportGoodsNum * itemData.Price);
            _progress.text = string.Format(Localization.Get("MarketOrderProgress"), data.HasTransportGoodsNum,
                data.PromiseTransportGoodsNum);
        
            _slider.value = data.HasTransportGoodsNum / data.PromiseTransportGoodsNum;
            _slider.interactable = false;

            //_carInfo.text = string.Format(Localization.Get("MarketOrderTransportingNum"), data.RunningThisOrderCar.Count);
        }
        else
        {
            var configData = DataManager.GetOrderData(data.OrderId);
            if (configData.Type == 0)
            {
                _type.text = Localization.Get("Temporary");
            }
            else
            {
                _type.text = Localization.Get("Continuous");
            }
            _description.text = Localization.Get(configData.DescriptionIds);
            _target.text = string.Format(Localization.Get("MarketOrderDestination"), configData.Distance, Localization.Get(configData.Destination));
            _timeLimit.gameObject.SetActive(true);
            _timeRepeat.gameObject.SetActive(true);
            if (data.IsRepeating)
            {
                _timeLimit.text = string.Format(Localization.Get("MarketOrderRepeatTime"), configData.RepeatTime);
                int remainWeek = data.StartWeek + configData.RepeatTime - LevelManager.Instance.WeekIndex;
                _timeRepeat.text = String.Format(Localization.Get("MarketOrderRemainTime"),remainWeek);
            }
            else
            {
                _timeLimit.text = string.Format(Localization.Get("MarketOrderTimeLimit"), configData.TimeLimit); 
                int remainWeek = data.StartWeek + configData.TimeLimit - LevelManager.Instance.WeekIndex;
                _timeRepeat.text = String.Format(Localization.Get("MarketOrderRemainTime"),remainWeek);
            }
        
        
            var itemData = DataManager.GetItemDataById(configData.ItemId);

            _sellIcon.SetIcon(configData.ItemId,configData.ItemNum);
            _earnIcon.SetIcon(99999,configData.GoodsPrice * configData.ItemNum * itemData.Price);
            _progress.text = string.Format(Localization.Get("MarketOrderProgress"), data.HasTransportGoodsNum,
                configData.ItemNum);
        
            _slider.value = data.HasTransportGoodsNum / configData.ItemNum;
            _slider.interactable = false;

            _carInfo.text = string.Format(Localization.Get("MarketOrderTransportingNum"), data.RunningThisOrderCar.Count);
        }
        
        RebuildLayout();
    }

    public void OnClickAcceptButton()
    {
        _clickCallBack?.Invoke(orderId);
        _acceptBtn.SetActive(false);
        _acceptText.text = Localization.Get("Accepted");
        _acceptText.gameObject.SetActive(true);
        RebuildLayout();
    }

    public void OnClickCancelButton()
    {
        _clickCallBack?.Invoke(orderIndex);
        gameObject.SetActive(false);
    }

    private void RebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    private void RefreshTransportingCarNum()
    {
        if (gameObject.activeInHierarchy &&_carInfo)
        {
            var data = MarketManager.Instance.GetRuntimeOrderData(orderIndex);
            _carInfo.text = string.Format(Localization.Get("MarketOrderTransportingNum"), data?.RunningThisOrderCar.Count ?? 0);
        }
    } 
    
}