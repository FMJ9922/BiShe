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
            marketCanvas = MainInteractCanvas.Instance.GetMarketCanvas();
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
            //Debug.Log("profit" + profit);
            switch (marketItems[i].curMode)
            {
                case MarketItem.TradeMode.once:
                    if (ResourceManager.Instance.TryUseResource(99999, -profit))
                    {
                        BuyItem(marketItems[i].GetCostResource());
                        marketCanvas.AddProfitInfo(GetBuyProfitDescribe(marketItems[i].GetCostResource(), profit));
                        marketItems[i].OnResetTrading();
                    }
                    break;
                case MarketItem.TradeMode.everyWeek:
                    if (ResourceManager.Instance.TryUseResource(99999, -profit))
                    {
                        BuyItem(marketItems[i].GetCostResource());
                        marketCanvas.AddProfitInfo(GetBuyProfitDescribe(marketItems[i].GetCostResource(), profit));
                    }
                    break;
                case MarketItem.TradeMode.maintain:
                    float num = marketItems[i].needNum - ResourceManager.Instance.TryGetResourceNum(marketItems[i].curItem.Id);
                    if (num > 0&& ResourceManager.Instance.TryUseResource(99999, -profit))
                    {
                        BuyItem(new CostResource(marketItems[i].curItem.Id, num));
                        marketCanvas.AddProfitInfo(GetBuyProfitDescribe(marketItems[i].GetCostResource(), profit));
                    }
                    break;
            }
            marketItems[i].RefreshBuyProfitLabel();
        }

        for (int i = 0; i < orderItems.Count; i++)
        {
            orderItems[i].RefreshBuyProfitLabel();
            if (!orderItems[i].isTrading) continue;
            CostResource costResource = orderItems[i].GetCostResource();
            if (ResourceManager.Instance.IsResourceEnough(costResource))
            {
                switch (orderItems[i].curMode)
                {
                    case MarketItem.TradeMode.once:
                        if (IsItemEnough(costResource))
                        {
                            SellItem(costResource);
                            orderItems[i].OnResetTrading();
                        }
                        break;
                    case MarketItem.TradeMode.everyWeek:
                        SellItem(costResource);
                        break;
                    case MarketItem.TradeMode.maintain:
                        float num = -costResource.ItemNum + ResourceManager.Instance.TryGetResourceNum(orderItems[i].curItem.Id);
                        if (num > 0)
                        {
                            SellItem(new CostResource(costResource.ItemId, num));
                        }
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

    public string GetSellProfitDescribe(CostResource costResource,float profit)
    {
        return string.Format("{0} 出售{1},数量{2},获利{3}",LevelManager.Instance.LogDate(),
            Localization.ToSettingLanguage(DataManager.GetItemNameById(costResource.ItemId)),
            CastTool.RoundOrFloat(costResource.ItemNum), CastTool.RoundOrFloat(profit));
    }

    public string GetBuyProfitDescribe(CostResource costResource, float profit)
    {
        return string.Format("{0} 购买{1},数量{2},花费{3}", LevelManager.Instance.LogDate(),
             Localization.ToSettingLanguage(DataManager.GetItemNameById(costResource.ItemId)), 
            CastTool.RoundOrFloat(costResource.ItemNum), CastTool.RoundOrFloat(-profit));
    }
    public void BuyItem(CostResource costResource)
    {
        //Debug.Log("buy" + costResource.ItemNum);
        ResourceManager.Instance.AddResource(costResource.ItemId, costResource.ItemNum);
    }

    public void SellItem(CostResource costResource)
    {
        //Debug.Log("sell" + costResource.ItemNum);
        float profit = ResourceManager.Instance.TryUseUpResource(new CostResource(costResource.ItemId, costResource.ItemNum)).ItemNum;
        marketCanvas.AddProfitInfo(GetSellProfitDescribe(costResource, profit));
        ResourceManager.Instance.AddMoney(profit);
    }

    public bool IsItemEnough(CostResource costResource)
    {
        return ResourceManager.Instance.IsResourceEnough(costResource);
    }

}


