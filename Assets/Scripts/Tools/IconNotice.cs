using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IconNotice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public IconDescription description;
    #region 接口
    public void OnPointerEnter(PointerEventData eventData)
    {
        NoticeManager.Instance.ShowIconNotice(Localization.ToSettingLanguage(description.GetDescription()));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        NoticeManager.Instance.CloseNotice();
    }

    #endregion
}
