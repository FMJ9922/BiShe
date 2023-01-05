using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

public class NoticeCanvas : CanvasBase
{
    [SerializeField] GameObject onlyText;
    [SerializeField] TMP_Text text;

    [SerializeField] GameObject onlyButtons;
    [SerializeField] TMP_Text _titleText;
    [SerializeField] Button _monitorBtn;
    [SerializeField] TMP_Text _monitorText;
    [SerializeField] Button _foodBtn;
    [SerializeField] TMP_Text _foodText;

    [SerializeField] GameObject _onlyItemInfo;
    [SerializeField] GameObject _itemInfoPfb;
    [SerializeField] GameObject _fenGeLinePfb;
    [SerializeField] GameObject _titlePfb;

    public Color test;
    private const char enter = '\n';
    private Dictionary<string, List<BuildingBase>> _nameDic = new Dictionary<string, List<BuildingBase>>();
    private Action<float> _onOilCostChange;
    
    public override void OnOpen()
    {
        EventManager.StartListening<float>(ConstEvent.OnOilCost,OnOilCostChange);
        base.OnOpen();
    }
    public override void OnClose()
    {
        if (onlyText.activeInHierarchy)
        {
            onlyText.SetActive(false);
        }
        if (onlyButtons.activeInHierarchy)
        {
            onlyButtons.SetActive(false);
        }
        EventManager.StopListening<float>(ConstEvent.OnOilCost,OnOilCostChange);
        base.OnClose();
    }

    private void OnOilCostChange(float f)
    {
        _onOilCostChange?.Invoke(f);
    }

    public void SetText(string context)
    {
        onlyText.SetActive(true);
        _onlyItemInfo.SetActive(false);
        text.text = context.Replace('|', '\n');
    }
    int SortBuildingBase(BuildingBase a,BuildingBase b)
    {
        return a.runtimeBuildData.SortRank - b.runtimeBuildData.SortRank;
    }
    public void SetDetailInfo(int itemId)
    {
        _nameDic.Clear();
        CleanUpAllAttachedChildren(_onlyItemInfo.transform.GetChild(0));
        _onlyItemInfo.SetActive(true);
        ItemData itemData = DataManager.GetItemDataById(itemId);
        List<BuildingBase> buildings = MapManager.Instance._buildings;
        buildings.Sort(SortBuildingBase);
        for (int i = 0; i < buildings.Count; i++)
        {
            if (_nameDic.ContainsKey(buildings[i].runtimeBuildData.Name))
            {
                _nameDic[buildings[i].runtimeBuildData.Name].Add(buildings[i]);
            }
            else
            {
                _nameDic.Add(buildings[i].runtimeBuildData.Name, new List<BuildingBase> { buildings[i] });
            }
        }
        GameObject titleObj = Instantiate(_titlePfb, _onlyItemInfo.transform.GetChild(0));
        titleObj.GetComponent<TMP_Text>().text = Localization.Get("来自建筑");
        GameObject fenge1 = Instantiate(_fenGeLinePfb, _onlyItemInfo.transform.GetChild(0));
        List<CostResource> temp = new List<CostResource>();
        CostResource p;
        int buildingCounter = 0;
        foreach (var item in _nameDic)
        {
            temp.Clear();
            for (int i = 0; i < item.Value.Count; i++)
            {
                temp.AddRange(item.Value[i].GetPerWeekDeltaResources());
            }
            p = GetListSum(temp, itemId);
            float num = p.ItemNum;
            if (num != 0)
            {
                GameObject itemObj = Instantiate(_itemInfoPfb, _onlyItemInfo.transform.GetChild(0));

                string title = $"{Localization.Get(item.Key)} × {item.Value.Count}";
                int countHan = GetHanNumFromString(title);
                int countSum = title.Length;
                string delta;
                if (num > 0)
                {
                    delta = $"<#9FFF8D>+{CastTool.RoundOrFloat(p.ItemNum)}</color>";
                }
                else
                {
                    delta = $"<#FF7B72>{CastTool.RoundOrFloat(p.ItemNum)}</color>";
                }
                buildingCounter++;
                itemObj.GetComponent<ItemDeltaInfo>().Init(title, delta, item.Value);
            }
        }
        if (buildingCounter == 0)
        {
            GameObject titleObj3 = Instantiate(_titlePfb, _onlyItemInfo.transform.GetChild(0));
            titleObj3.GetComponent<TMP_Text>().text = Localization.Get("无");
        }
        GameObject titleObj2 = Instantiate(_titlePfb, _onlyItemInfo.transform.GetChild(0));
        titleObj2.GetComponent<TMP_Text>().text = Localization.Get("来自贸易");
        GameObject fenge2 = Instantiate(_fenGeLinePfb, _onlyItemInfo.transform.GetChild(0));

        int marketCount = 0;
        if (itemId != 99999)
        {
            if (itemId != 11000)
            {
                List<MarketItem> marketItem = MarketManager.Instance.GetTargetData(itemId);
                if (marketItem == null || marketItem.Count < 1)
                {
                    GameObject titleObj4 = Instantiate(_titlePfb, _onlyItemInfo.transform.GetChild(0));
                    titleObj4.GetComponent<TMP_Text>().text = Localization.Get("无");
                }
                else
                {
                    for (int i = 0; i < marketItem.Count; i++)
                    {
                        string title = Localization.Get(marketItem[i].curMode.GetDescription());
                        string deltaNum;
                        if (marketItem[i].isBuy)
                        {
                            deltaNum = $"<#9FFF8D>+{CastTool.RoundOrFloat(marketItem[i].GetCostResource().ItemNum)}</color>";
                        }
                        else
                        {
                            deltaNum = $"<#FF7B72>-{CastTool.RoundOrFloat(marketItem[i].GetCostResource().ItemNum)}</color>";
                        }
                        GameObject itemObj = Instantiate(_itemInfoPfb, _onlyItemInfo.transform.GetChild(0));
                        itemObj.GetComponent<ItemDeltaInfo>().Init(title, deltaNum, null);
                    }
                }
            }
            else
            {
                List<MarketItem> marketItems = MarketManager.Instance.GetAllFoodData();
                if (marketItems == null ||marketItems.Count<1)
                {
                    GameObject titleObj4 = Instantiate(_titlePfb, _onlyItemInfo.transform.GetChild(0));
                    titleObj4.GetComponent<TMP_Text>().text = Localization.Get("无");
                }
                else
                {
                    for (int i = 0; i < marketItems.Count; i++)
                    {
                        string title = Localization.Get(marketItems[i].curItem.Name);
                        string deltaNum;
                        if (marketItems[i].marketData.isBuy)
                        {
                            deltaNum = $"<#9FFF8D>+{CastTool.RoundOrFloat(marketItems[i].GetCostResource().ItemNum)}</color>";
                        }
                        else
                        {
                            deltaNum = $"<#FF7B72>-{CastTool.RoundOrFloat(marketItems[i].GetCostResource().ItemNum)}</color>";
                        }
                        GameObject itemObj = Instantiate(_itemInfoPfb, _onlyItemInfo.transform.GetChild(0));
                        itemObj.GetComponent<ItemDeltaInfo>().Init(title, deltaNum, null);
                    }
                }
                    
            }
            
        }
        else 
        {
            MarketItem[] buyItems = MarketManager.Instance.GetBuyItems();
            MarketItem[] sellItems = MarketManager.Instance.GetSellItems();
            for (int i = 0; i < buyItems.Length; i++)
            {
                if(buyItems[i].isTrading && ResourceManager.Instance.IsResourceEnough(new CostResource(99999, -buyItems[i].GetProfit())))
                {
                    GameObject itemObj = Instantiate(_itemInfoPfb, _onlyItemInfo.transform.GetChild(0));
                    itemObj.GetComponent<ItemDeltaInfo>().Init(
                        Localization.Get(buyItems[i].curItem.Name),
                        $"<#FF7B72>{CastTool.RoundOrFloat(buyItems[i].GetProfit())}</color>", 
                        null);
                    marketCount++;
                }
            }
            for (int i = 0; i < sellItems.Length; i++)
            {
                if (sellItems[i].isTrading && ResourceManager.Instance.IsResourceEnough(sellItems[i].GetCostResource()))
                {
                    GameObject itemObj = Instantiate(_itemInfoPfb, _onlyItemInfo.transform.GetChild(0));
                    itemObj.GetComponent<ItemDeltaInfo>().Init(
                        Localization.Get(sellItems[i].curItem.Name),
                        $"<#9FFF8D>+{CastTool.RoundOrFloat(sellItems[i].GetProfit())}</color>",
                        null);
                    marketCount++;
                }
            }
            if(marketCount == 0)
            {
                GameObject titleObj3 = Instantiate(_titlePfb, _onlyItemInfo.transform.GetChild(0));
                titleObj3.GetComponent<TMP_Text>().text = Localization.Get("无");
            }
        
        
            GameObject titleObj5 = Instantiate(_titlePfb, _onlyItemInfo.transform.GetChild(0));
            titleObj5.GetComponent<TMP_Text>().text = Localization.Get("来自运输");
            GameObject fenge5 = Instantiate(_fenGeLinePfb, _onlyItemInfo.transform.GetChild(0));
            GameObject itemObj1 = Instantiate(_itemInfoPfb, _onlyItemInfo.transform.GetChild(0));
            itemObj1.GetComponent<ItemDeltaInfo>().Init(
                Localization.Get("油费"),
                $"<#FF7B72>{CastTool.RoundOrFloat(-TrafficManager.Instance.WeeklyCost)}</color>",
                null);
            var info = itemObj1.GetComponent<ItemDeltaInfo>();
            _onOilCostChange = delegate(float f)
            {
                info.SetText($"<#FF7B72>{CastTool.RoundOrFloat(-f)}</color>");
            };
        }
        int moveCount = (marketCount == 0 ? 2 : marketCount+ 1) + (buildingCounter == 0 ? 2 : buildingCounter +1)-4;
        //Debug.Log(moveCount);
        _onlyItemInfo.transform.GetChild(0).localPosition = new Vector3(0, -20 * moveCount, 0);
    }

    public void SetIconOption(ItemData data)
    {
        onlyText.SetActive(false);
        onlyButtons.SetActive(true);
        _titleText.text = Localization.Get(data.Name);
        //监视器窗口上下
        _monitorBtn.gameObject.SetActive(true);
        _monitorBtn.onClick.RemoveAllListeners();
        _monitorBtn.onClick.AddListener(() =>
        {
            ResourceManager.Instance.ToggleHudItem(data);
            RefreshLabel(data);
        });
        bool isItemInHud = ResourceManager.Instance.IsInHudList(data.Id);
        _monitorText.text = isItemInHud ?
            Localization.Get("UnMonitorResource") : Localization.Get("MonitorResource");

        //食物禁止或允许食用
        _foodBtn.gameObject.SetActive(data.ItemType == (int)ItemType.food);
        if (data.ItemType == (int)ItemType.food)
        {
            _foodBtn.interactable = data.Id != 11000;
            _foodBtn.onClick.AddListener(() =>
            {
                ResourceManager.Instance.ToggleForbiddenFood(data.Id);
                RefreshLabel(data);
            });
            bool isForbiddenFood = ResourceManager.Instance.IsInForbiddenFoodList(data.Id);
            _foodText.text = isForbiddenFood ?
                Localization.Get("ForbiddenEat") : Localization.Get("AllowEat");
        }
    }

    private void RefreshLabel(ItemData data)
    {
        bool isItemInHud = ResourceManager.Instance.IsInHudList(data.Id);
        _monitorText.text = isItemInHud ?
            Localization.Get("UnMonitorResource") : Localization.Get("MonitorResource");
        if (data.ItemType == (int)ItemType.food)
        {
            bool isForbiddenFood = ResourceManager.Instance.IsInForbiddenFoodList(data.Id);
            _foodText.text = isForbiddenFood ?
                Localization.Get("ForbiddenEat") : Localization.Get("AllowEat");
        }
    }

    private void OnDisable()
    {
        onlyButtons.SetActive(false);
        onlyText.SetActive(false);
        _monitorBtn.onClick.RemoveAllListeners();
        _foodBtn.onClick.RemoveAllListeners();
        CommonIcon.IsShowingOption = false;
    }

    public static int GetHanNumFromString(string str)
    {
        int count = 0;
        Regex regex = new Regex(@"^[u4E00-u9FA5]{0,}$");

        for (int i = 0; i < str.Length; i++)
        {
            if (regex.IsMatch(str[i].ToString()))
            {
                count++;
            }
        }

        return count;
    }

    public CostResource GetListSum(List<CostResource> list, int itemId)
    {
        CostResource ret = new CostResource(itemId, 0);
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].ItemId == itemId)
            {
                ret += list[i];
            }
        }
        return ret;
    }

    public void CloseDetailInfo()
    {
        _onlyItemInfo.SetActive(false);
    }
}
