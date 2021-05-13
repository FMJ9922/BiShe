using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelSelectItem : MonoBehaviour,IPointerClickHandler
{
    public static List<LevelSelectItem> items;
    public GameObject select;
    public GameObject locker;
    public Image preview;
    public LevelData data;
    public void Init(LevelData _data)
    {
        //Debug.Log("init" + _data.Id);
        data = _data;
        locker.SetActive(false);
        preview.sprite = LoadAB.LoadSprite("mat.ab", _data.Id + "preview");
        items.Add(this);
        select.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("Click");
        for (int i = 0; i < items.Count; i++)
        {
            items[i].HideOutline();
        }
        ShowOutline();
        GameSelectCanvas.Instance.ChangeShowLevel(data);
    }

    public void ShowOutline()
    {
        select.SetActive(true);
    }

    public void HideOutline()
    {
        select.SetActive(false);
    }
}
