using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDItem : MonoBehaviour
{
    [SerializeField] private TMP_Text curText;
    [SerializeField] private TMP_Text deltaText;
    private ItemData data;
    private string strWeek;
    [SerializeField] private Image iconImage;
    [SerializeField] private IconNotice notice;
    private static readonly string iconBundle = "icon.ab";

    public void Init(ItemData itemData)
    {
        data = itemData;
        strWeek = Localization.Get("Week");
        Refresh();
    }

    public void Refresh()
    {
        notice.description = IconDescription.Custom;
        notice.content = Localization.Get(data.Name);
        float num = ResourceManager.Instance.TryGetResourceNum(data.Id);
        float deltaNum = ResourceManager.Instance.GetWeekDeltaNum(data.Id);
        curText.text = ((int)num).ToString();
        deltaText.text = string.Format("{2}{0}/{1}", (int)Mathf.Abs(deltaNum), strWeek, deltaNum >= 0 ? "+" : "-");
        iconImage.sprite = LoadAB.LoadSprite(iconBundle, data.Name+ "Icon");
    }

    public void OnClick()
    {
        NoticeManager.Instance.ShowItemDetailInfo(data.Id);
    }


}
