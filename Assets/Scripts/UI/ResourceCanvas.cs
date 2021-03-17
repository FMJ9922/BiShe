using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceCanvas : CanvasBase
{
    [SerializeField] private Transform itemContent;//挂载物品的地方
    [SerializeField] private GameObject itemPfb;//物体预制体
    [SerializeField] private Color oddColor;
    [SerializeField] private Color evenColor;
    [SerializeField] private GameObject mainCanvas;

    private List<GameObject> itemList = new List<GameObject>();//显示物品列表

    #region 实现基类
    public override void InitCanvas()
    {

    }
    public override void OnOpen()
    {
        CreateItemList();
        EventManager.StartListening(ConstEvent.OnInputResources, CreateItemList);
        mainCanvas.SetActive(true);
    }

    public override void OnClose()
    {
        DestroyList();
        EventManager.StopListening(ConstEvent.OnInputResources, CreateItemList);
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
        EventManager.TriggerEvent(ConstEvent.OnRefreshResources);
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
        GameObject item = Instantiate(itemPfb, itemContent);
        item.name = DataManager.GetItemNameById(itemId);
        item.GetComponentInChildren<Image>().color = place % 2 == 1 ? evenColor : oddColor;
        TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();
        texts[0].text = Localization.ToSettingLanguage(item.name);
        texts[1].text = CastTool.RoundOrFloat(itemNum);
        itemList.Add(item);
    }
}
