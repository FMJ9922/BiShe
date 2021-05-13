using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StartMenuItem : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public static List<StartMenuItem> items;
    private Vector3 originLocalPos;

    private void Awake()
    {
        items = new List<StartMenuItem>();
    }

    void Start()
    {
        items.Add(this);
        originLocalPos = transform.position;
    }


    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
    }
}
