using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TechTreeCanvas : CanvasBase
{
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private List<TechItem> techItems;
    [SerializeField] private Image[] images;
    [SerializeField] private TMP_Text pointText;
    private bool hasInit = false;
    public override void InitCanvas()
    {
        if (hasInit)
        {
            return;
        }
        hasInit = true;
        mainCanvas.SetActive(false);
        for (int i = 0; i < techItems.Count; i++)
        {
            techItems[i].Init();
        }
        techItems.Sort(delegate (TechItem a, TechItem b)
        {
            return a.TechId - b.TechId;
        });
        for (int i = 0; i < images.Length; i++)
        {
            images[i].gameObject.SetActive(false);
        }
    }

    public void InitBySave(bool[] techsAvaliable)
    {
        //Debug.Log("SET UNLOCK");
        for (int i = 0; i < techItems.Count; i++)
        {
            if (techsAvaliable[i])
            {
                techItems[i].SetUnlockSprite();
            }
        }
    }

    public TechItem GetTechItem(int id)
    {
        for (int i = 0; i < techItems.Count; i++)
        {
            if(id == techItems[i].TechId)
            {
                return techItems[i];
            }
        }
        return null;
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

    public void SetImage(int place, string iconName)
    {
        images[place].sprite = LoadAB.LoadSprite("icon.ab", iconName);
        images[place].gameObject.SetActive(true);
    }

    public void ChangePoint(int point)
    {
        pointText.text = Localization.ToSettingLanguage("RemainPoints") + point;
    }
}
