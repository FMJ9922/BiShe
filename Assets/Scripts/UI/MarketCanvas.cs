using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketCanvas : CanvasBase
{
    [SerializeField] private GameObject mainCanvas;
    List<MarketItem> marketItems = new List<MarketItem>();
    [SerializeField] GameObject marketContent;
    [SerializeField] GameObject marketItemPfb;
    [SerializeField] Button buyBtn, sellBtn;
    [SerializeField] TMP_Text buyText, sellText;
    [SerializeField] GameObject buyContent, sellContent;
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
        InitMarketItems(true);
    }

    public void OnClickSellBtn()
    {
        //Debug.Log("sell");
        buyText.color = Color.gray;
        sellText.color = Color.white;
        InitMarketItems(false);
    }

    private void InitMarketItems(bool isSell)
    {


    }
    public List<MarketItem> GetMarketItems()
    {
        return marketItems;
    }

    public void InitBuyItem()
    {
        GameObject obj = Instantiate(marketItemPfb, marketContent.transform);
        obj.SetActive(true);
        obj.transform.SetParent(marketContent.transform);
        obj.transform.SetSiblingIndex(marketContent.transform.childCount - 2);
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
