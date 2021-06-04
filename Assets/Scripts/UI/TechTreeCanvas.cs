using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TechTreeCanvas : CanvasBase
{
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private TechItem[] techItems;
    [SerializeField] private Image[] images;
    [SerializeField] private TMP_Text pointText;
    public override void InitCanvas()
    {
        mainCanvas.SetActive(false);
        for (int i = 0; i < techItems.Length; i++)
        {
            techItems[i].Init();
        }
        for (int i = 0; i < images.Length; i++)
        {
            images[i].gameObject.SetActive(false);
        }
    }

    public override void OnOpen()
    {
        mainCanvas.SetActive(true);
        TechManager.Instance.RefreshTechPoint();
    }

    public override void OnClose()
    {
        mainCanvas.SetActive(false);
    }

    public void SetImage(int place,string iconName)
    {
        images[place].sprite = LoadAB.LoadSprite("icon.ab", iconName);
        images[place].gameObject.SetActive(true);
    }

    public void ChangePoint(int point)
    {
        pointText.text = "剩余科技点数：" + point;
    }
}
