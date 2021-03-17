using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketCanvas : CanvasBase
{
    [SerializeField] private GameObject mainCanvas;
    List<MarketItem> marketItems = new List<MarketItem>();
    [SerializeField] GameObject marketContent;
    [SerializeField] GameObject marketItemPfb;

    #region 实现基类
    public override void InitCanvas()
    {
        InitMarketItems();
        mainCanvas.SetActive(false);
    }
    public override void OnOpen()
    {
        mainCanvas.SetActive(true);
        GameManager.Instance.SetTimeScale(TimeScale.stop);
    }

    public override void OnClose()
    {
        mainCanvas.SetActive(false);
        GameManager.Instance.SetTimeScale(TimeScale.one);
    }
    #endregion

    private void InitMarketItems()
    {
        ItemData[] itemArray = DataManager.GetItemDatas();
        for (int i = 0; i < itemArray.Length; i++)
        {
            //排除金钱和人力资源
            if (itemArray[i].Id == 10000 || itemArray[i].Id == 99999 || itemArray[i].Id == 11000)
            {
                continue;
            }
            GameObject obj = GameObject.Instantiate(marketItemPfb, marketContent.transform);
            obj.SetActive(true);
            MarketItem marketItem = obj.GetComponent<MarketItem>();
            marketItem.InitItem(itemArray[i].Id);
            marketItems.Add(marketItem);
        }
    }
    public List<MarketItem> GetMarketItems()
    {
        return marketItems;
    }
}
