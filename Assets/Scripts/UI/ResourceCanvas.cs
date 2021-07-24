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

    private List<GameObject> itemList = new List<GameObject>();//显示物品列表

    #region 实现基类
    public override void InitCanvas()
    {

    }
    public override void OnOpen()
    {
        CreateItemList();
        EventManager.StartListening(ConstEvent.OnInputResources, CreateItemList);
        EventManager.StartListening(ConstEvent.OnRefreshResources, CreateItemList);
        mainCanvas.SetActive(true);
    }

    public override void OnClose()
    {
        DestroyList();
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
