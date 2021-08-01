using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceCanvas : CanvasBase
{
    [SerializeField] private Transform itemContent;//挂载物品的地方
    [SerializeField] private Color oddColor;
    [SerializeField] private Color evenColor;
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private Slider progress;
    [SerializeField] private Button destroy;
    [SerializeField] private Button upgrade;
    [SerializeField] private Transform upgradeCost;

    private List<GameObject> itemList = new List<GameObject>();//显示物品列表
    private StorageBuilding building;
    #region 实现基类
    public override void InitCanvas()
    {

    }

    public override void OnOpen()
    {

    }
    public void OnOpen(StorageBuilding _building)
    {
        building = _building;
        CreateItemList();
        SetProgress();
        destroy.onClick.AddListener(() => { building.DestroyBuilding(true, true, true); OnClose(); });
        upgrade.onClick.AddListener(() => { building.Upgrade(out bool success); if (success) OnClose(); });
        EventManager.StartListening(ConstEvent.OnInputResources, CreateItemList);
        EventManager.StartListening(ConstEvent.OnRefreshResources, CreateItemList);
        mainCanvas.SetActive(true);
    }

    public void ShowUpgradeInfo()
    {
        RuntimeBuildData _buildData = building.runtimeBuildData;
        upgradeCost.gameObject.SetActive(true);
        CleanUpAllAttachedChildren(upgradeCost);
        List<CostResource> costResources = new List<CostResource>();
        BuildData next = DataManager.GetBuildData(_buildData.RearBuildingId);
        costResources.Add(new CostResource(99999, (next.Price - _buildData.Price) * TechManager.Instance.BuildPriceBuff()));
        for (int i = 0; i < next.costResources.Count; i++)
        {
            CostResource res = next.costResources[i];
            for (int j = 0; j < _buildData.costResources.Count; j++)
            {
                if (next.costResources[i].ItemId == _buildData.costResources[j].ItemId)
                {
                    res = (next.costResources[i] - _buildData.costResources[j]) * TechManager.Instance.BuildResourcesBuff();
                }
            }
            costResources.Add(res);
        }
        for (int i = 0; i < costResources.Count; i++)
        {
            GameObject icon = CommonIcon.GetIcon(costResources[i]);
            icon.transform.SetParent(upgradeCost);
            icon.transform.localScale = ((GameManager.Instance.GetScreenRelativeRate() - 1F) * 0F + 1F) * Vector3.one;
        }
    }
    private void CleanUpAllAttachedChildren(Transform target)
    {
        for (int i = 0; i < target.childCount; i++)
        {
            Destroy(target.GetChild(i).gameObject);
        }

    }
    public void CloseUpgradeInfo()
    {
        upgradeCost.gameObject.SetActive(false);
    }

    public override void OnClose()
    {
        DestroyList();
        destroy.onClick.RemoveAllListeners();
        upgrade.onClick.RemoveAllListeners();
        EventManager.StopListening(ConstEvent.OnInputResources, CreateItemList);
        EventManager.StopListening(ConstEvent.OnRefreshResources, CreateItemList);
        mainCanvas.SetActive(false);
    }
    #endregion

    private void CreateItemList()
    {
        DestroyList();
        itemList.Clear();
        Dictionary<int, float> dic = ResourceManager.Instance.GetAllResource();
        int count = 0;
        foreach (KeyValuePair<int, float> keyValuePair in dic)
        {
            InitItemPfb(count, keyValuePair.Key, keyValuePair.Value);
            count++;
        }
    }

    public void SetProgress()
    {
        progress.value = ResourceManager.Instance.GetCurStorage() / ResourceManager.Instance.maxStorage;
    }
    private void DestroyList()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            Destroy(itemList[i]);
        }
    }
    private void InitItemPfb(int place, int itemId, float itemNum)
    {
        GameObject item = CommonIcon.GetIcon(itemId, itemNum);
        item.transform.SetParent(itemContent);
        item.transform.localScale = ((GameManager.Instance.GetScreenRelativeRate()-1F)*0F+1F) * Vector3.one;
        itemList.Add(item);
    }
}
