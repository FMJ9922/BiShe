using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdjustComponent : MonoBehaviour
{
    
    [ContextMenu("ChangeButtonNavigation")]
    void ChangeButtonsNavigation()
    {
        Button[] btns = transform.GetComponentsInChildren<Button>();
        for (int i = 0; i < btns.Length; i++)
        {
            Navigation navigation = new Navigation();
            navigation.mode = Navigation.Mode.None;
            btns[i].navigation = navigation;
        }
    }
}
