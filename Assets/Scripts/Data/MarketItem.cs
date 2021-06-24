using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 市场物品信息
/// </summary>
public class MarketItem : MonoBehaviour
{
    [SerializeField] TMP_Dropdown itemDrop;
    [SerializeField] Button subBtn,addBtn;
    [SerializeField] TMP_Text numText;
    [SerializeField] TMP_Text profitText;
    [SerializeField] TMP_Dropdown modeDrop;
    [SerializeField] Button confirmBtn;
    [SerializeField] Button cancelBtn;
    [SerializeField] CommonIcon icon;

    public MarketData marketData;

    public ItemData curItem => marketData.curItem;
    private List<int> idList => marketData.idList;
    public bool isTrading => marketData.isTrading;
    public int needNum =>marketData.needNum;
    public TradeMode curMode => marketData.curMode;
    public float profit => marketData.profit;
    public bool isBuy =>marketData.isBuy;
    public int maxNum =>marketData.maxNum;


    private void OnDestroy()
    {
        itemDrop.onValueChanged.RemoveAllListeners();
        modeDrop.onValueChanged.RemoveAllListeners();
    }
    public void InitSellItem(int id)
    {
        marketData.isBuy = false;
        marketData.curItem = DataManager.GetItemDataById(id);
        ClearItemDrop(false);
        marketData.needNum = 0;
        icon.SetIcon(curItem.Id, ResourceManager.Instance.TryGetResourceNum(curItem.Id));
        modeDrop.ClearOptions();
        TMP_Dropdown.OptionData tempData;
        for (int i = 0; i < 3; i++)
        {
            tempData = new TMP_Dropdown.OptionData();
            tempData.text = Localization.ToSettingLanguage(((TradeMode)i).GetDescription());
            modeDrop.options.Add(tempData);
        }
        modeDrop.onValueChanged.AddListener(ChangeMode);
        numText.text = needNum.ToString();
        RefreshBuyProfitLabel();
        cancelBtn.onClick.RemoveAllListeners();
        cancelBtn.onClick.AddListener(OnResetTrading);
        OnResetTrading();
    }

    public void InitSavedSellItem(MarketData data)
    {
        marketData = data;
        ClearItemDrop(false);
        icon.SetIcon(data.curItem.Id, ResourceManager.Instance.TryGetResourceNum(curItem.Id));
        modeDrop.ClearOptions();
        TMP_Dropdown.OptionData tempData;
        for (int i = 0; i < 3; i++)
        {
            tempData = new TMP_Dropdown.OptionData();
            tempData.text = Localization.ToSettingLanguage(((TradeMode)i).GetDescription());
            modeDrop.options.Add(tempData);
        }
        modeDrop.onValueChanged.AddListener(ChangeMode);
        modeDrop.captionText.text = data.curMode.GetDescription();
        modeDrop.value = (int)data.curMode;

        numText.text = needNum.ToString();
        RefreshBuyProfitLabel();
        cancelBtn.onClick.RemoveAllListeners();
        if (data.isTrading)
        {
            OnConfirmTrading();
        }
        else
        {
            OnResetTrading();
        }
    }

    public void InitSavedBuyItem(MarketData data)
    {
        ItemData[] itemArray = DataManager.GetItemDatas();
        idList.Clear();
        marketData = data;
        TMP_Dropdown.OptionData tempData;
        ClearItemDrop(true);
        for (int i = 0; i < itemArray.Length; i++)
        {
            //排除金钱和人力资源
            if (itemArray[i].Id == 10000 || itemArray[i].Id == 99999 || itemArray[i].Id == 11000)
            {
                continue;
            }
            tempData = new TMP_Dropdown.OptionData();
            tempData.text = Localization.ToSettingLanguage(itemArray[i].Name);
            itemDrop.options.Add(tempData);
            idList.Add(itemArray[i].Id);
        }
        itemDrop.captionText.text = Localization.ToSettingLanguage(data.curItem.Name);
        icon.SetIcon(curItem.Id, ResourceManager.Instance.TryGetResourceNum(curItem.Id));
        cancelBtn.onClick.AddListener(() => MainInteractCanvas.Instance.GetMarketCanvas().RemoveBuyItem(gameObject));
        itemDrop.onValueChanged.AddListener(ChangeItem);
        numText.text = needNum.ToString();

        modeDrop.ClearOptions();
        for (int i = 0; i < 3; i++)
        {
            tempData = new TMP_Dropdown.OptionData();
            tempData.text = Localization.ToSettingLanguage(((TradeMode)i).GetDescription());
            modeDrop.options.Add(tempData);
        }
        modeDrop.onValueChanged.AddListener(ChangeMode);
        modeDrop.captionText.text = data.curMode.GetDescription();
        modeDrop.value = (int)data.curMode;

        RefreshBuyProfitLabel();

        if (data.isTrading)
        {
            OnConfirmTrading();
        }
        else
        {
            OnResetTrading();
        }
    }

    public void InitBuyItem()
    {
        ItemData[] itemArray = DataManager.GetItemDatas();
        idList.Clear();
        TMP_Dropdown.OptionData tempData;
        ClearItemDrop(true);
        for (int i = 0; i < itemArray.Length; i++)
        {
            //排除金钱和人力资源
            if (itemArray[i].Id == 10000 || itemArray[i].Id == 99999 || itemArray[i].Id == 11000)
            {
                continue;
            }
            tempData = new TMP_Dropdown.OptionData();
            tempData.text = Localization.ToSettingLanguage(itemArray[i].Name);
            itemDrop.options.Add(tempData);
            idList.Add(itemArray[i].Id);
        }
        ChangeItem(0);
        cancelBtn.onClick.AddListener(()=>MainInteractCanvas.Instance.GetMarketCanvas().RemoveBuyItem(gameObject));
        itemDrop.onValueChanged.AddListener(ChangeItem);
        numText.text = needNum.ToString();

        modeDrop.ClearOptions();
        for (int i = 0; i < 3; i++)
        {
            tempData = new TMP_Dropdown.OptionData();
            tempData.text = Localization.ToSettingLanguage(((TradeMode)i).GetDescription());
            modeDrop.options.Add(tempData);
        }
        modeDrop.onValueChanged.AddListener(ChangeMode);

        RefreshBuyProfitLabel();

        OnResetTrading();
    }

    private void ClearItemDrop(bool interactable)
    {
        itemDrop.ClearOptions();
        itemDrop.interactable = interactable;
    }

    private void ChangeItem(int id)
    {
        marketData.curItem = DataManager.GetItemDataById(idList[id]);
        icon.SetIcon(curItem.Id, ResourceManager.Instance.TryGetResourceNum(curItem.Id));
        RefreshBuyProfitLabel();
        OnResetTrading();
    }


    private void ChangeMode(int modeType)
    {
        marketData.curMode = (TradeMode)modeType;
        modeDrop.captionText.text = Localization.ToSettingLanguage(((TradeMode)modeType).GetDescription());
        RefreshBuyProfitLabel();
        OnResetTrading();
    }
    
    public void RefreshBuyProfitLabel()
    {
        switch (curMode)
        {
            case TradeMode.once:
            case TradeMode.everyWeek:
                marketData.profit = isBuy?-curItem.Price * needNum * 1.5f : curItem.Price * needNum*TechManager.Instance.PriceBuff();
                break;
            case TradeMode.maintain:
                float num = (isBuy?1f:-1f)*( needNum - ResourceManager.Instance.TryGetResourceNum(curItem.Id));
                marketData.profit = num > 0 ? (isBuy ? -curItem.Price * num * 1.5f : curItem.Price * num * TechManager.Instance.PriceBuff()) : 0;
                break;
        }
        profitText.text = profit.ToString();
    }

    public void RefreshItem()
    {
        
    }


    public void OnAddNum()
    {
        marketData.needNum += 10;
        if (needNum > 200) marketData.needNum = 200;
        numText.text = needNum.ToString();
        RefreshBuyProfitLabel();
        OnResetTrading();
    }

    public void OnSubNum()
    {
        marketData.needNum -= 10;
        if (needNum < 0) marketData.needNum = 0;
        numText.text = needNum.ToString();
        RefreshBuyProfitLabel();
        OnResetTrading();
    }

    public void OnConfirmTrading()
    {
        //Debug.Log("true");
        marketData.isTrading = true;
        confirmBtn.gameObject.SetActive(false);
        cancelBtn.gameObject.SetActive(true);
    }

    public void OnResetTrading()
    {
        //Debug.Log("false");
        marketData.isTrading = false;
        confirmBtn.gameObject.SetActive(true);
        cancelBtn.gameObject.SetActive(false);
    }

    public float GetProfit()
    {
        return profit;
    }

    public CostResource GetCostResource()
    {
        return new CostResource(curItem.Id,needNum);
    }
}
    
[System.Serializable]
public class MarketData
{
    public ItemData curItem;
    public List<int> idList = new List<int>();
    public bool isTrading = false;
    public int needNum = 0;
    public TradeMode curMode;
    public float profit;
    public bool isBuy = true;
    public int maxNum;
}