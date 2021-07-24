using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TechItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int TechId;
    public TechData data;
    private Button button;
    public TechItem[] FrontItems;
    public bool hasInit = false;

    public void Init()
    {
        hasInit = true;
        button = transform.GetComponent<Button>();
        data = DataManager.GetTechData(TechId);
        button.onClick.AddListener(() => TechManager.Instance.OpenInfoCanvas(this));
        transform.GetComponent<Image>().sprite = LoadAB.LoadSprite("icon.ab", "un" + transform.name.TrimEnd(' '));
    }

    public void SetUnlockSprite()
    {
        transform.GetComponent<Image>().sprite = LoadAB.LoadSprite("icon.ab", transform.name.TrimEnd(' '));
    }

    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        NoticeManager.Instance.ShowIconNotice(data.Name);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        NoticeManager.Instance.CloseNotice();
    }
}
