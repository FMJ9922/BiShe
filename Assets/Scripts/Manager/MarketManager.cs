using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MarketManager : Singleton<MarketManager>
{
    [SerializeField] MarketCanvas marketCanvas;
    public void InitMarketManager()
    {
        EventManager.StartListening(ConstEvent.OnSettleAccount, SettleAccount);

        marketCanvas.RefreshSellItems();
        marketCanvas.InitSellItems();
    }
    public void InitSavedMarketManager()
    {
        EventManager.StartListening(ConstEvent.OnSettleAccount, SettleAccount);
        marketCanvas.InitSavedBuyItems(GameManager.saveData.buyDatas);
        marketCanvas.InitSavedSellItems(GameManager.saveData.sellDatas);
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
        List<MarketItem> marketItems = marketCanvas.GetBuyItems();
        List<MarketItem> orderItems = marketCanvas.GetSellItems();
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
                case TradeMode.once:
                    if (ResourceManager.Instance.TryUseResource(99999, -profit))
                    {
                        BuyItem(marketItems[i].GetCostResource());
                        marketCanvas.AddProfitInfo(GetBuyProfitDescribe(marketItems[i].GetCostResource(), profit));
                        marketItems[i].OnResetTrading();
                    }
                    break;
                case TradeMode.everyWeek:
                    if (ResourceManager.Instance.TryUseResource(99999, -profit))
                    {
                        BuyItem(marketItems[i].GetCostResource());
                        marketCanvas.AddProfitInfo(GetBuyProfitDescribe(marketItems[i].GetCostResource(), profit));
                    }
                    break;
                case TradeMode.maintain:
                    float num = marketItems[i].needNum - ResourceManager.Instance.TryGetResourceNum(marketItems[i].curItem.Id);
                    if (num > 0 && ResourceManager.Instance.TryUseResource(99999, -profit))
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
            switch (orderItems[i].curMode)
            {
                case TradeMode.once:
                    if (ResourceManager.Instance.IsResourceEnough(costResource))
                    {
                        SellItem(costResource, orderItems[i].curItem.Price);
                        orderItems[i].OnResetTrading();
                    }
                    break;
                case TradeMode.everyWeek:
                    if (ResourceManager.Instance.IsResourceEnough(costResource))
                    {
                        SellItem(costResource, orderItems[i].curItem.Price);
                    }
                    break;
                case TradeMode.maintain:
                    float num = -costResource.ItemNum + ResourceManager.Instance.TryGetResourceNum(orderItems[i].curItem.Id);
                    if (num > 0)
                    {
                        num = Mathf.Clamp(num, 0, 200 * TechManager.Instance.SellNumBuff());
                        SellItem(new CostResource(costResource.ItemId, num), orderItems[i].curItem.Price);
                    }
                    break;
            }
            orderItems[i].RefreshBuyProfitLabel();
        }
    }

    public string GetSellProfitDescribe(CostResource costResource, float profit)
    {
        return string.Format("{0} 出售{1},数量{2},获利{3}", LevelManager.Instance.LogDate(),
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

    public void SellItem(CostResource costResource, float price)
    {
        //Debug.Log("sell" + costResource.ItemNum);
        float profit = ResourceManager.Instance.TryUseUpResource(new CostResource(costResource.ItemId, costResource.ItemNum)).ItemNum
            * price * TechManager.Instance.PriceBuff();
        marketCanvas.AddProfitInfo(GetSellProfitDescribe(costResource, profit));
        ResourceManager.Instance.AddMoney(profit);
    }

    public bool IsItemEnough(CostResource costResource)
    {
        return ResourceManager.Instance.IsResourceEnough(costResource);
    }

    public MarketData[] GetBuyDatas()
    {
        MarketItem[] items = marketCanvas.buysItems.ToArray();
        MarketData[] buyDatas = new MarketData[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            buyDatas[i] = items[i].marketData;
        }
        return buyDatas;
    }

    public MarketData[] GetSellDatas()
    {
        MarketItem[] items = marketCanvas.sellsItems.ToArray();
        MarketData[] sellDatas = new MarketData[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            sellDatas[i] = items[i].marketData;
        }
        return sellDatas;
    }
}


