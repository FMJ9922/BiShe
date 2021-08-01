using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeBtn : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    public InfoCanvas InfoCanvas;
    public ResourceCanvas resourceCanvas;
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (InfoCanvas)
        {
            InfoCanvas.ShowUpgradeInfo();
        }
        else if(resourceCanvas)
        {
            resourceCanvas.ShowUpgradeInfo();
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (InfoCanvas)
        {
            InfoCanvas.CloseUpgradeInfo();
        }
        else if (resourceCanvas)
        {
            resourceCanvas.CloseUpgradeInfo();
        }
    }

    
}
