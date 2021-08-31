using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public class FadeScene : MonoBehaviour
{
    Image fadeImg;
    [SerializeField] TMP_Text tips;
    private void Awake()
    {
        EventManager.StartListening<string>(ConstEvent.OnLoadingTips,SetTips);
        EventManager.StartListening(ConstEvent.OnLoadingOver, StartFade);
    }
    private void OnDestroy()
    {
        EventManager.StopListening<string>(ConstEvent.OnLoadingTips, SetTips);
        EventManager.StopListening(ConstEvent.OnLoadingOver, StartFade);
    }
    void Start()
    {
        fadeImg = transform.GetComponent<Image>();
        //imageAlpha = 1;
        fadeImg.color = Color.black;
        Scene scene = SceneManager.GetActiveScene();
        
        if (scene.name != "level")
        {
            StartFade();
        }
    }

    public void Fade(float endValue, float duration)
    {
        CancelInvoke();
        fadeImg.DOFade(endValue, duration);
    }

    public void SetTips(string content)
    {
        tips.text = content;
    }
    public void SetTipsEmpty()
    {
        if (tips != null)
        {
            tips.text = "";
        }
    }
    public void StartFade()
    {
        Fade(0, 0.5f);
        Invoke("SetTipsEmpty", 3f);
    }
}
