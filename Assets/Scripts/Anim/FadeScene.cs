using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class FadeScene : MonoBehaviour
{
    Image fadeImg;
    void Start()
    {
        fadeImg = transform.GetComponent<Image>();
        //imageAlpha = 1;
        fadeImg.color = Color.black;
        Fade(0, 0.5f);
    }

    public void Fade(float endValue, float duration)
    {
        fadeImg.DOFade(endValue, duration);
    }
}
