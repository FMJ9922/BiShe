using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class ClickPlaySound : MonoBehaviour,IPointerClickHandler
{

    public SoundResource soundResource;

    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySoundEffect(soundResource);
    }
}
