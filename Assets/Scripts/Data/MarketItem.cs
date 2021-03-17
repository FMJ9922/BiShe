using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 市场物品信息
/// </summary>
public class MarketItem : MonoBehaviour
{
    public int itemID;
    private string strName;
    private float storageNum;//存储数量
    private float delta = 0;
    public float buy;//买入价格
    public float sell;//卖出价格
    public Button btn_banTrade;//禁止贸易按钮
    public Button btn_maintain;//维持库存
    public InputField input_num;//维持数量（多卖少买）
    private float profit;//利润
    [SerializeField] TMP_Text tmpName, tmpStorage, tmpBuy, tmpSell, tmpProfit;

    private void OnDestroy()
    {
        btn_banTrade.onClick.RemoveAllListeners();
        btn_maintain.onClick.RemoveAllListeners();
    }
    public void InitItem(int id)
    {
        itemID = id;
        ItemData data = DataManager.GetItemDataById(id);
        strName = Localization.ToSettingLanguage(data.Name);
        storageNum = ResourceManager.Instance.TryGetResourceNum(id);
        buy = 1.1f * data.Price;
        sell = data.Price;
        btn_banTrade.onClick.AddListener(OnBanTrade);
        btn_maintain.onClick.AddListener(OnMaintain);
        input_num.onValueChanged.AddListener(RefreshItem);
        profit = 0;
        tmpName.text = strName;
        tmpStorage.text = CastTool.RoundOrFloat(storageNum);
        tmpBuy.text = CastTool.RoundOrFloat(buy);
        tmpSell.text = CastTool.RoundOrFloat(sell);
        tmpProfit.text = CastTool.RoundOrFloat(profit);
    }

    public void RefreshItem(string text)
    {
        storageNum = ResourceManager.Instance.TryGetResourceNum(itemID);
        if(float.TryParse(text,out float target))
        {
            delta = storageNum - target;
            profit = btn_maintain.interactable ? 0 : (delta > 0 ? delta * sell : delta * buy);
        }
        else
        {
            input_num.text = string.Empty;
            profit = 0;
        }
        tmpProfit.text = CastTool.RoundOrFloat(profit);
        tmpStorage.text = CastTool.RoundOrFloat(storageNum);
    }
    public void RefreshItem()
    {
        storageNum = ResourceManager.Instance.TryGetResourceNum(itemID);
        if (float.TryParse(input_num.text, out float target))
        {
            delta = storageNum - target;
            profit = btn_maintain.interactable ? 0 : (delta > 0 ? delta * sell : delta * buy);
        }
        else
        {
            input_num.text = string.Empty;
            profit = 0;
        }
        tmpProfit.text = CastTool.RoundOrFloat(profit);
        tmpStorage.text = CastTool.RoundOrFloat(storageNum);
    }

    public void OnBanTrade()
    {
        btn_banTrade.interactable = false;
        btn_maintain.interactable = true;
        input_num.text = string.Empty;
        input_num.interactable = false;
    }

    public void OnMaintain()
    {
        btn_banTrade.interactable = true;
        btn_maintain.interactable = false;
        input_num.interactable = true;
    }

    public float GetProfit()
    {
        storageNum = ResourceManager.Instance.TryGetResourceNum(itemID);
        if (float.TryParse(input_num.text, out float target))
        {
            delta = storageNum - target;
            profit = btn_maintain.interactable ? 0 : (delta > 0 ? delta * sell : delta * buy);
        }
        else
        {
            input_num.text = string.Empty;
            profit = 0;
        }
        return profit;
    }

    public CostResource GetCostResource()
    {
        CostResource costResource = new CostResource(itemID, delta);
        return costResource;
    }
}
