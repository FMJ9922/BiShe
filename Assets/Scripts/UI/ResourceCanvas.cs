using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceCanvas : MonoBehaviour
{
    [SerializeField] private Transform itemContent;//挂载物品的地方
    [SerializeField] private GameObject itemPfb;//物体预制体
    [SerializeField] private Color oddColor;
    [SerializeField] private Color evenColor;

    private List<GameObject> itemList = new List<GameObject>();//显示物品列表

    private void OnEnable()
    {
        CreateItemList();
    }

    private void OnDisable()
    {
        DestroyList();
    }
    private void CreateItemList()
    {
        itemList.Clear();
        Dictionary<string, float> dic = ResourceManager.Instance.GetAllResource();
        int count = 0;
        foreach(KeyValuePair<string,float> keyValuePair in dic)
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
    private void InitItemPfb(int place,string itemName,float itemNum)
    {
        GameObject item = Instantiate(itemPfb, itemContent);
        item.name = itemName;
        item.GetComponentInChildren<Image>().color = place % 2 == 1 ? evenColor : oddColor;
        TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();
        texts[0].text = Localization.ToSettingLanguage(itemName);
        texts[1].text = string.Format("{0:F}", itemNum);
        itemList.Add(item);
    }
}
