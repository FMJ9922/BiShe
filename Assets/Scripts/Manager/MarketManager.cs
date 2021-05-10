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

        for (int i = 0; i < marketItems.Count; i++)
        {
            marketItems[i].RefreshProfitLabel();
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
            marketItems[i].RefreshProfitLabel();
        }
    }

    public void BuyItem(CostResource costResource)
    {
        Debug.Log("buy" + costResource.ItemNum);
        ResourceManager.Instance.AddResource(costResource.ItemId, costResource.ItemNum);
    }

    public void SellItem(CostResource costResource)
    {
        Debug.Log("sell");
        ResourceManager.Instance.TryUseResource(costResource);
    }
}


