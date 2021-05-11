using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketCanvas : CanvasBase
{
    [SerializeField] private GameObject mainCanvas;
    List<MarketItem> marketItems = new List<MarketItem>();
    List<MarketItem> orderItems = new List<MarketItem>();
    [SerializeField] GameObject buyContent,sellContent;
    [SerializeField] GameObject marketItemPfb;
    [SerializeField] Button buyBtn, sellBtn;
    [SerializeField] TMP_Text buyText, sellText;
    [SerializeField] GameObject buyItems, sellItems;
    enum MarketOption
    {
        buy = 0,
        sell = 1,
    }
    #region 实现基类
    public override void InitCanvas()
    {
        mainCanvas.SetActive(false);
        InitMarketItems();
    }
    public override void OnOpen()
    {
        OnClickBuyBtn();
        mainCanvas.SetActive(true);
        GameManager.Instance.PauseGame();
        EventManager.TriggerEvent<bool>(ConstEvent.OnLockScroll,true);
    }

    public override void OnClose()
    {
        mainCanvas.SetActive(false);
        GameManager.Instance.ContinueGame();
        EventManager.TriggerEvent<bool>(ConstEvent.OnLockScroll, false);
    }
    #endregion

    public void OnClickBuyBtn()
    {
        //Debug.Log("buy");
        buyText.color = Color.white;
        sellText.color = Color.gray;
        buyItems.SetActive(true);
        sellItems.SetActive(false);
    }

    public void OnClickSellBtn()
    {
        //Debug.Log("sell");
        buyText.color = Color.gray;
        sellText.color = Color.white;
        buyItems.SetActive(false);
        sellItems.SetActive(true);
        
    }

    private void InitMarketItems()
    {
        int[] ids = DataManager.GetLevelData(LevelManager.LevelID).orderIds;
        int[] nums = DataManager.GetLevelData(LevelManager.LevelID).orderNums;
        //Debug.Log(ids[0]);
        //Debug.Log(nums[0]);
        for (int i = 0; i < ids.Length; i++)
        {
            GameObject obj = Instantiate(marketItemPfb, sellContent.transform);
            obj.SetActive(true);
            obj.transform.SetParent(sellContent.transform);
            MarketItem marketItem = obj.GetComponent<MarketItem>();
            marketItem.needNum = nums[i];
            marketItem.InitItem(ids[i]);
            orderItems.Add(marketItem);
        }
    }
    public List<MarketItem> GetMarketItems()
    {
        return marketItems;
    }

    public List<MarketItem> GetOrderItems()
    {
        return orderItems;
    }

    public void InitBuyItem()
    {
        GameObject obj = Instantiate(marketItemPfb, buyContent.transform);
        obj.SetActive(true);
        obj.transform.SetParent(buyContent.transform);
        obj.transform.SetSiblingIndex(buyContent.transform.childCount - 2);
        MarketItem marketItem = obj.GetComponent<MarketItem>();
        marketItem.InitItemDropDown();
        marketItems.Add(marketItem);
    }

    public void RemoveBuyItem(GameObject obj)
    {
        MarketItem item = obj.GetComponent<MarketItem>();
        marketItems.Remove(item);
        Destroy(obj);
    }
    public void RefreshMarketItem()
    {
        foreach (var item in marketItems)
        {
            Destroy(item.gameObject);
        }
        marketItems.Clear();
    }
}
