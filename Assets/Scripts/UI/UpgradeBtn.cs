using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeBtn : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    public InfoCanvas InfoCanvas;
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        InfoCanvas.ShowUpgradeInfo();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        InfoCanvas.CloseUpgradeInfo();
    }

    
}
