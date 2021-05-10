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

        RefreshProfitLabel();

        OnResetTrading();
    }

    public void InitItemDropDown(int Id)
    {
        curItem = DataManager.GetItemDataById(Id);
        ClearItemDrop(false);
        itemDrop.captionText.text = Localization.ToSettingLanguage(curItem.Name);
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
        RefreshProfitLabel();
        OnResetTrading();
    }

    private void ChangeMode(int modeType)
    {
        curMode = (TradeMode)modeType;
        modeDrop.captionText.text = Localization.ToSettingLanguage(((TradeMode)modeType).GetDescription());
        RefreshProfitLabel();
        OnResetTrading();
    }
    
    public void RefreshProfitLabel()
    {
        switch (curMode)
        {
            case TradeMode.once:
            case TradeMode.everyWeek:
                profit = -curItem.Price * needNum;
                break;
            case TradeMode.maintain:
                float num = needNum - ResourceManager.Instance.TryGetResourceNum(curItem.Id);
                profit = num > 0 ? -curItem.Price * num : 0;
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
        if (needNum > 50) needNum = 50;
        numText.text = needNum.ToString();
        RefreshProfitLabel();
        OnResetTrading();
    }

    public void OnSubNum()
    {
        needNum -= 10;
        if (needNum < 0) needNum = 0;
        numText.text = needNum.ToString();
        RefreshProfitLabel();
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
    