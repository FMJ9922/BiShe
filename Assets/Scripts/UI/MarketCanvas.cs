using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketCanvas : CanvasBase
{
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private Button _accept;
    [SerializeField] private Button _generate;
    [SerializeField] private Button _buy;
    [SerializeField] private GameObject _acceptViewRoot;
    [SerializeField] private GameObject _generateViewRoot;
    [SerializeField] private GameObject _buyViewRoot;
    [SerializeField] private GameObject _offerPfb;
    [SerializeField] private GameObject _orderPfb;
    [SerializeField] private GameObject _noOrderAvailableTip;
    [SerializeField] private GameObject _noHavingOrderAvailableTip;
    [SerializeField] private MarketBuyItem _marketBuyItem;
    private List<MarketItem> _orderItems = new List<MarketItem>();
    private List<MarketItem> _havingOrderItems = new List<MarketItem>();
    #region 实现基类
    public override void InitCanvas()
    {
        mainCanvas.SetActive(false);
    }
    public override void OnOpen()
    {
        mainCanvas.SetActive(true);
        GameManager.Instance.PauseGame();
        EventManager.TriggerEvent<bool>(ConstEvent.OnLockScroll,true);
        EventManager.StartListening(ConstEvent.OnMarketOrderDealing,RefreshPanel);
        if (MarketManager.Instance.GetRuntimeOrderDatas().Count <= 0)
        {
            OnClickGenerate();
        }
        else
        {
            OnClickAccepted();
        }
    }

    public override void OnClose()
    {
        if (mainCanvas.activeInHierarchy)
        {
            mainCanvas.SetActive(false);
            GameManager.Instance.ContinueGame();
            EventManager.TriggerEvent<bool>(ConstEvent.OnLockScroll, false);
            EventManager.StopListening(ConstEvent.OnMarketOrderDealing,RefreshPanel);
        }
    }
    #endregion



    #region NewMarket

    public void OnClickAccepted()
    {
        _accept.transform.localScale = Vector3.one * 1.3f;
        _generate.transform.localScale = Vector3.one;
        _buy.transform.localScale = Vector3.one;
        _acceptViewRoot.SetActive(true);
        _generateViewRoot.SetActive(false);
        _buyViewRoot.SetActive(false);
        InitAcceptOrders();
    }

    public void OnClickGenerate()
    {
        _accept.transform.localScale = Vector3.one;
        _generate.transform.localScale = Vector3.one * 1.3f;
        _buy.transform.localScale = Vector3.one;
        _acceptViewRoot.SetActive(false);
        _generateViewRoot.SetActive(true);
        _buyViewRoot.SetActive(false);
        InitMarketOrders();
    }

    /// <summary>
    /// 点击生成买入订单
    /// </summary>
    public void OnClickBuy()
    {
        _accept.transform.localScale = Vector3.one;
        _generate.transform.localScale = Vector3.one;
        _buy.transform.localScale = Vector3.one * 1.3f;
        _acceptViewRoot.SetActive(false);
        _generateViewRoot.SetActive(false);
        _buyViewRoot.SetActive(true);
        ResetBuyView();
    }

    /// <summary>
    /// 点击生成订单
    /// </summary>
    public void OnClickGenerateOrder()
    {
        MarketManager.Instance.GenerateOrder();
        InitMarketOrders();
    }

    private void InitMarketOrders()
    {
        var orders = MarketManager.Instance.GetOrderList();
        _noOrderAvailableTip.SetActive(orders.Count ==0);
        for (int i = 0; i < orders.Count; i++)
        {
            if (i < _orderItems.Count)
            {
                var data = DataManager.GetOrderData(orders[i]);
                _orderItems[i].InitMarketItem(data,OnAcceptOrder);
                _orderItems[i].gameObject.SetActive(true);
            }
            else
            {
                GameObject obj = Instantiate(_offerPfb, _offerPfb.transform.parent);
                obj.SetActive(true);
                var data = DataManager.GetOrderData(orders[i]);
                var comp = obj.GetComponent<MarketItem>();
                comp.InitMarketItem(data,OnAcceptOrder);
                _orderItems.Add(comp);
            }
        }

        for (int i = orders.Count; i < _orderItems.Count; i++)
        {
            _orderItems[i].gameObject.SetActive(false);
        }
    }

    private void OnAcceptOrder(int orderId)
    {
        MarketManager.Instance.RemoveFromOrderList(orderId);
        MarketManager.Instance.AddSellOrder(orderId);
    }

    private void InitAcceptOrders()
    {
        var orders = MarketManager.Instance.GetRuntimeOrderDatas();
        _noHavingOrderAvailableTip.SetActive(orders.Count ==0);
        for (int i = 0; i < orders.Count; i++)
        {
            if (i < _havingOrderItems.Count)
            {
                _havingOrderItems[i].InitAcceptItem(orders[i],OnCancelOrder);
                _havingOrderItems[i].gameObject.SetActive(true);
            }
            else
            {
                GameObject obj = Instantiate(_orderPfb, _orderPfb.transform.parent);
                obj.SetActive(true);
                var comp = obj.GetComponent<MarketItem>();
                comp.InitAcceptItem(orders[i],OnCancelOrder);
                _havingOrderItems.Add(comp);
            }
        }

        for (int i = orders.Count; i < _havingOrderItems.Count; i++)
        {
            _havingOrderItems[i].gameObject.SetActive(false);
        }
    }

    private void OnCancelOrder(int index)
    {
        MarketManager.Instance.RemoveRuntimeOrderData(index);
    }

    private void RefreshPanel()
    {
        if (_acceptViewRoot.activeInHierarchy)
        {
            InitAcceptOrders();
        }
    }

    private void ResetBuyView()
    {
        _marketBuyItem.ResetView();
        _marketBuyItem._onClickOkJumpAction = OnClickAccepted;
    }

    #endregion
}
