using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IconNotice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public IconDescription description;
    public string content;
    #region 接口
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(description != IconDescription.Custom)
        {
            NoticeManager.Instance.ShowIconNotice(Localization.ToSettingLanguage(description.GetDescription()));
        }
        else
        {
            NoticeManager.Instance.ShowIconNotice(Localization.ToSettingLanguage(content));
        }
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        NoticeManager.Instance.CloseNotice();
    }

    public void OnDisable()
    {
        if (NoticeManager.Instance)
        {
            NoticeManager.Instance.CloseNotice();
        }
    }
    #endregion
}
