using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MarketManager : Singleton<MarketManager>
{
    [SerializeField]MarketCanvas marketCanvas;
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
        List<MarketItem> marketItems = marketCanvas.GetMarketItems();
        
        for (int i = 0; i < marketItems.Count; i++)
        {
            float profit = marketItems[i].GetProfit();
            if (profit == 0) continue;
            Debug.Log("profit" + profit);
            if (ResourceManager.Instance.TryUseResource(99999, -profit)) 
            {
                CostResource costResource = marketItems[i].GetCostResource();
                if (costResource.ItemNum > 0)
                {
                    SellItem(costResource);
                }
                else
                {
                    BuyItem(costResource);
                }
                marketItems[i].RefreshItem();
            }
        }
    }

    public void BuyItem(CostResource costResource)
    {
        Debug.Log("buy"+costResource.ItemNum);
        ResourceManager.Instance.AddResource(costResource.ItemId,-costResource.ItemNum);
    }

    public void SellItem(CostResource costResource)
    {
        Debug.Log("sell");
        ResourceManager.Instance.TryUseResource(costResource);
    }
}


