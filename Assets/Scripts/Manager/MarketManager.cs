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

    public List<MarketItem> GetTargetData(int itemId)
    {
        List<MarketItem> ret = new List<MarketItem>();
        List<MarketItem> buyItems = marketCanvas.GetBuyItems();
        for (int i = 0; i < buyItems.Count; i++)
        {
            if (buyItems[i].curItem.Id == itemId && buyItems[i].isTrading)
            {
                ret.Add(buyItems[i]);
            }
        }
        List<MarketItem> sellItems = marketCanvas.GetSellItems();
        for (int i = 0; i < sellItems.Count; i++)
        {
            if (sellItems[i].curItem.Id == itemId && sellItems[i].isTrading)
            {
                ret.Add(sellItems[i]);
            }
        }
        return ret;
    }

    public List<MarketItem> GetAllFoodData()
    {
        List<MarketItem> ret = new List<MarketItem>();
        List<MarketItem> buyItems = marketCanvas.GetBuyItems();
        for (int i = 0; i < buyItems.Count; i++)
        {
            if (ResourceManager.IsFood(buyItems[i].curItem.Id) && buyItems[i].isTrading)
            {
                ret.Add(buyItems[i]);
            }
        }
        List<MarketItem> sellItems = marketCanvas.GetSellItems();
        for (int i = 0; i < sellItems.Count; i++)
        {
            if (ResourceManager.IsFood(sellItems[i].curItem.Id) && sellItems[i].isTrading)
            {
                ret.Add(sellItems[i]);
            }
        }
        return ret;
    }

    public List<CostResource> GetDeltaNum()
    {
        if (!marketCanvas)
        {
            marketCanvas = MainInteractCanvas.Instance.GetMarketCanvas();
        }
        List<MarketItem> buyItems = marketCanvas.GetBuyItems();
        List<MarketItem> sellItems = marketCanvas.GetSellItems();
        List<CostResource> ret = new List<CostResource>();
        for (int i = 0; i < buyItems.Count; i++)
        {
            if (buyItems[i].isTrading
                && ResourceManager.Instance.IsResourceEnough(new CostResource(99999, -buyItems[i].GetProfit())))
            {
                ret.Add(new CostResource(99999, buyItems[i].GetProfit()));
                ret.Add(buyItems[i].GetCostResource());
            }
        }
        for (int i = 0; i < sellItems.Count; i++)
        {
            if (sellItems[i].isTrading
                && ResourceManager.Instance.IsResourceEnough(sellItems[i].GetCostResource()))
            {
                ret.Add(new CostResource(99999, sellItems[i].GetProfit()));
                ret.Add(-sellItems[i].GetCostResource());
            }
        }
        /*int num = MapManager.GetHutBuildingNum();
        CostResource costFood = ResourceManager.Instance.GetFoodByMax(num,true);
        ret.Add(-costFood);*/
        return ret;
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
            CostResource costResource = marketItems[i].GetCostResource();
            //Debug.Log("profit" + profit);
            switch (marketItems[i].curMode)
            {
                case TradeMode.once:
                    if (ResourceManager.Instance.TryUseResource(99999, -profit))
                    {
                        BuyItem(costResource);
                        marketCanvas.AddProfitInfo(GetBuyProfitDescribe(costResource, profit));
                        marketItems[i].OnResetTrading();
                    }
                    break;
                case TradeMode.everyWeek:
                    if (ResourceManager.Instance.TryUseResource(99999, -profit))
                    {
                        BuyItem(costResource);
                        marketCanvas.AddProfitInfo(GetBuyProfitDescribe(costResource, profit));
                    }
                    break;
                case TradeMode.maintain:
                    if (costResource.ItemNum > 0 && ResourceManager.Instance.TryUseResource(99999, -profit))
                    {
                        BuyItem(costResource);
                        marketCanvas.AddProfitInfo(GetBuyProfitDescribe(costResource, profit));
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
                    if (ResourceManager.Instance.IsResourceEnough(costResource))
                    {
                        SellItem(costResource, orderItems[i].curItem.Price);
                    }
                    break;
            }
            orderItems[i].RefreshBuyProfitLabel();
        }
    }

    public string GetSellProfitDescribe(CostResource costResource, float profit)
    {
        return string.Format("{0} {4} {1},{5} {2},{6} {3}", LevelManager.Instance.LogDate(),
            Localization.Get(DataManager.GetItemNameById(costResource.ItemId)),
            CastTool.RoundOrFloat(costResource.ItemNum), CastTool.RoundOrFloat(profit),
            Localization.Get("Sell1"),
            Localization.Get("Amount"),
            Localization.Get("GetProfit")
            );
    }

    public string GetBuyProfitDescribe(CostResource costResource, float profit)
    {
        return string.Format("{0} {4} {1},{5} {2},{6} {3}", LevelManager.Instance.LogDate(),
             Localization.Get(DataManager.GetItemNameById(costResource.ItemId)),
            CastTool.RoundOrFloat(costResource.ItemNum), CastTool.RoundOrFloat(-profit),
            Localization.Get("Buy1"),
            Localization.Get("Amount"),
            Localization.Get("Cost")
            );
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

    public MarketItem[] GetBuyItems()
    {
        return marketCanvas.buysItems.ToArray();
    }

    public MarketItem[] GetSellItems()
    {
        return marketCanvas.sellsItems.ToArray();
    }
}


