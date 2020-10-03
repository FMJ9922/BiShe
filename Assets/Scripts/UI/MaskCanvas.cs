using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 用于点击背景时关闭所有的悬浮窗口
/// </summary>
public class MaskCanvas : MonoBehaviour,IPointerClickHandler
{
    public void OnMaskClicked()
    {
        EventManager.TriggerEvent(ConstEvent.OnMaskClicked);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnMaskClicked();
    }
}
