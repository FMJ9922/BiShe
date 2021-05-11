using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MarketManager : Singleton<MarketManager>
{
    [SerializeField] MarketCanvas marketCanvas;
    public void InitMarketManager()
    {
        EventManager.StartListening(ConstEvent.OnSettleAccount, SettleAccount);
    }

    private void OnDestroy()
    {
        EventManager.StopListening(ConstEvent.OnSettleAccount, SettleAccount);
    }
    public void SettleAccount()
    {
        //Debug.Log("?");
        if (!marketCanvas)
        {
            marketCanvas = (MarketCanvas)MainInteractCanvas.Instance.canvas[4];
        }
        List<MarketItem> marketItems = marketCanvas.GetMarketItems();
        List<MarketItem> orderItems = marketCanvas.GetOrderItems();
        for (int i = 0; i < marketItems.Count; i++)
        {
            marketItems[i].RefreshBuyProfitLabel();
            //Debug.Log("?");
            if (!marketItems[i].isTrading) continue;
            float profit = marketItems[i].GetProfit();
            if (profit == 0) continue;
            Debug.Log("profit" + profit);
            switch (marketItems[i].curMode)
            {
                case MarketItem.TradeMode.once:
                    if (ResourceManager.Instance.TryUseResource(99999, -profit))
                    {
                        BuyItem(marketItems[i].GetCostResource());
                        marketItems[i].OnResetTrading();
                    }
                    break;
                case MarketItem.TradeMode.everyWeek:
                    if (ResourceManager.Instance.TryUseResource(99999, -profit))
                    {
                        BuyItem(marketItems[i].GetCostResource());
                    }
                    break;
                case MarketItem.TradeMode.maintain:
                    float num = marketItems[i].needNum - ResourceManager.Instance.TryGetResourceNum(marketItems[i].curItem.Id);
                    if (num > 0&& ResourceManager.Instance.TryUseResource(99999, -profit))
                    {
                        BuyItem(new CostResource(marketItems[i].curItem.Id, num));
                    }
                    break;
            }
            marketItems[i].RefreshBuyProfitLabel();
        }

        for (int i = 0; i < orderItems.Count; i++)
        {
            orderItems[i].RefreshBuyProfitLabel();
            if (!orderItems[i].isTrading) continue;
            if (ResourceManager.Instance.TryUseResource(orderItems[i].curItem.Id, orderItems[i].needNum))
            {
                switch (orderItems[i].curMode)
                {
                    case MarketItem.TradeMode.once:
                        orderItems[i].OnResetTrading();
                        break;
                    case MarketItem.TradeMode.everyWeek:
                        BuyItem(orderItems[i].GetCostResource());
                        break;
                }
                orderItems[i].RefreshBuyProfitLabel();
            }
            else
            {
                orderItems[i].OnResetTrading();
                Debug.Log("交易失败：" + orderItems[i].curItem.Id + " " + orderItems[i].needNum);
            }
            
        }
    }

    public void BuyItem(CostResource costResource)
    {
        Debug.Log("buy" + costResource.ItemNum);
        ResourceManager.Instance.AddResource(costResource.ItemId, costResource.ItemNum);
    }

}


