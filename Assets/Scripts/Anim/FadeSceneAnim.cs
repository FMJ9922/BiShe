using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class FadeSceneAnim : MonoBehaviour
{
    Image fadeImg;
    int count = 0;
    void Start()
    {
        fadeImg = transform.GetComponent<Image>();
        //imageAlpha = 1;
        //Fade(0, 0.5f);
    }

    public void Fade(float endValue, float duration)
    {
        Material mat = fadeImg.material;
        mat.DOFade(endValue, duration);
    }

    private IEnumerator FadeIE(float delta)
    {
        while(count > 0)
        {
            count--;
            Debug.Log(count);
            SetDeltaColor(delta);
            yield return 0;
        }
    }

    private void SetColor(float alpha)
    {
        Color color = fadeImg.material.color;
        fadeImg.material.color = new Color(color.r, color.g, color.b, alpha);
    }

    private void SetDeltaColor(float delta)
    {
        Color color = fadeImg.material.color;
        fadeImg.material.color = new Color(color.r, color.g, color.b, color.a + delta);
    }
}
