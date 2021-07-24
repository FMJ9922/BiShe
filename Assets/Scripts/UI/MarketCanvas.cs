using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketCanvas : CanvasBase
{
    [SerializeField] private GameObject mainCanvas;
    public List<MarketItem> buysItems = new List<MarketItem>();
    public List<MarketItem> sellsItems = new List<MarketItem>();
    [SerializeField] GameObject buyContent,sellContent;
    [SerializeField] GameObject marketItemPfb;
    [SerializeField] Button buyBtn, sellBtn;
    [SerializeField] TMP_Text buyText, sellText,profitText;
    [SerializeField] GameObject buyItems, sellItems;
    [SerializeField] TMP_Text profitInfoPfb;
    [SerializeField] GameObject profitContent,profitItems;
    enum MarketOption
    {
        buy = 0,
        sell = 1,
    }
    #region 实现基类
    public override void InitCanvas()
    {
        mainCanvas.SetActive(false);
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
        if (mainCanvas.activeInHierarchy)
        {
            mainCanvas.SetActive(false);
            GameManager.Instance.ContinueGame();
            EventManager.TriggerEvent<bool>(ConstEvent.OnLockScroll, false);
        }
    }
    #endregion

    public void OnClickBuyBtn()
    {
        //Debug.Log("buy");
        buyText.color = Color.white;
        sellText.color = Color.gray;
        profitText.color = Color.gray;
        buyItems.SetActive(true);
        sellItems.SetActive(false);
        profitItems.SetActive(false);
    }

    public void OnClickSellBtn()
    {
        //Debug.Log("sell");
        buyText.color = Color.gray;
        profitText.color = Color.gray;
        sellText.color = Color.white;
        buyItems.SetActive(false);
        sellItems.SetActive(true);
        profitItems.SetActive(false);

    }

    public void AddProfitInfo(string content)
    {
        GameObject newInfo = Instantiate(profitInfoPfb.gameObject, profitContent.transform);
        newInfo.SetActive(true);
        newInfo.transform.SetAsFirstSibling();
        newInfo.GetComponent<TMP_Text>().text = content;
    }
    public void OnClickProfitInfoBtn()
    {
        buyText.color = Color.gray;
        profitText.color = Color.white; 
        sellText.color = Color.gray;
        buyItems.SetActive(false);
        sellItems.SetActive(false);
        profitItems.SetActive(true);
    }
    public void InitSellItems()
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
            marketItem.marketData.maxNum = (int)(nums[i] * TechManager.Instance.SellNumBuff());
            marketItem.InitSellItem(ids[i]);
            sellsItems.Add(marketItem);
        }
    }

    public void InitSavedSellItems(MarketData[] selldatas)
    {
        for (int i = 0; i < selldatas.Length; i++)
        {
            GameObject obj = Instantiate(marketItemPfb, sellContent.transform);
            obj.SetActive(true);
            obj.transform.SetParent(sellContent.transform);
            MarketItem marketItem = obj.GetComponent<MarketItem>();
            marketItem.InitSavedSellItem(selldatas[i]);
            sellsItems.Add(marketItem);
        }
    }
    public List<MarketItem> GetBuyItems()
    {
        return buysItems;
    }

    public List<MarketItem> GetSellItems()
    {
        return sellsItems;
    }

    public void InitSavedBuyItems(MarketData[] buyDatas)
    {
        for (int i = 0; i < buyDatas.Length; i++)
        {
            GameObject obj = Instantiate(marketItemPfb, buyContent.transform);
            obj.SetActive(true);
            obj.transform.SetParent(buyContent.transform);
            obj.transform.SetSiblingIndex(buyContent.transform.childCount - 2);
            MarketItem marketItem = obj.GetComponent<MarketItem>();
            marketItem.InitSavedBuyItem(buyDatas[i]);
            buysItems.Add(marketItem);
        }

    }
    public void InitBuyItem()
    {
        GameObject obj = Instantiate(marketItemPfb, buyContent.transform);
        obj.SetActive(true);
        obj.transform.SetParent(buyContent.transform);
        obj.transform.SetSiblingIndex(buyContent.transform.childCount - 2);
        MarketItem marketItem = obj.GetComponent<MarketItem>();
        marketItem.InitBuyItem();
        buysItems.Add(marketItem);
    }

    public void RemoveBuyItem(GameObject obj)
    {
        MarketItem item = obj.GetComponent<MarketItem>();
        buysItems.Remove(item);
        Destroy(obj);
    }
    public void RefreshBuyItems()
    {
        foreach (var item in buysItems)
        {
            Destroy(item.gameObject);
        }
        buysItems.Clear();
    }

    public void RefreshSellItems()
    {
        foreach (var item in sellsItems)
        {
            Destroy(item.gameObject);
        }
        sellsItems.Clear();
    }
}
