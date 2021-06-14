using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
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
    public ItemData curItem;
    private List<int> idList = new List<int>();

    public bool isTrading = false;
    public int needNum = 0;
    public TradeMode curMode;
    public float profit;
    public bool isBuy = true;
    public int maxNum;
    public enum TradeMode
    {
        [Description("Once")]
        once = 0,
        [Description("EveryWeek")]
        everyWeek = 1,
        [Description("Maintain")]
        maintain = 2,
    }
    private void OnDestroy()
    {
        itemDrop.onValueChanged.RemoveAllListeners();
        modeDrop.onValueChanged.RemoveAllListeners();
    }
    public void InitItem(int id)
    {
        isBuy = false;
        curItem = DataManager.GetItemDataById(id);
        ClearItemDrop(false);
        icon.SetIcon(id, 0);
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

        OnResetTrading();
    }

    public void InitItemDropDown()
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
        curItem = DataManager.GetItemDataById(idList[id]);
        icon.SetIcon(curItem.Id, 0);
        RefreshBuyProfitLabel();
        OnResetTrading();
    }

    private void ChangeMode(int modeType)
    {
        curMode = (TradeMode)modeType;
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
                profit = isBuy?-curItem.Price * needNum * 1.5f : curItem.Price * needNum*TechManager.Instance.PriceBuff();
                break;
            case TradeMode.maintain:
                float num = (isBuy?1f:-1f)*( needNum - ResourceManager.Instance.TryGetResourceNum(curItem.Id));
                profit = num > 0 ? (isBuy ? -curItem.Price * num * 1.5f : curItem.Price * num * TechManager.Instance.PriceBuff()) : 0;
                break;
        }
        profitText.text = profit.ToString();
    }

    public void RefreshItem()
    {
        
    }


    public void OnAddNum()
    {
        needNum += 10;
        if (needNum > maxNum) needNum = maxNum;
        numText.text = needNum.ToString();
        RefreshBuyProfitLabel();
        OnResetTrading();
    }

    public void OnSubNum()
    {
        needNum -= 10;
        if (needNum < 0) needNum = 0;
        numText.text = needNum.ToString();
        RefreshBuyProfitLabel();
        OnResetTrading();
    }

    public void OnConfirmTrading()
    {
        //Debug.Log("true");
        isTrading = true;
        confirmBtn.gameObject.SetActive(false);
        cancelBtn.gameObject.SetActive(true);
    }

    public void OnResetTrading()
    {
        //Debug.Log("false");
        isTrading = false;
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
    