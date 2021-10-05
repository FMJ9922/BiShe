using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
public class IntroduceCanvas : CanvasBase
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private TMP_Text text;
    [SerializeField] private GameObject main;
    [SerializeField] private ContentSizeFitter fitter;

    public VideoClip[] clips;
    public string[] ids;
    public Button[] btns;

    public override void InitCanvas()
    {
        for (int i = 0; i < btns.Length; i++)
        {
            Button p = btns[i];
            btns[i].onClick.AddListener(() =>
            {
                OnClick(p);
            });
        }
        main.SetActive(false);
    }
    public override void OnOpen()
    {
        EventManager.TriggerEvent<bool>(ConstEvent.OnLockScroll, true);
        CameraMovement.Instance.LockMove(true);
        main.SetActive(true);
        text.text = Localization.Get(ids[0]);
        videoPlayer.clip = clips[0];
        videoPlayer.Play();
        StartCoroutine(DelayRefesh());
    }

    public IEnumerator DelayRefesh()
    {
        fitter.enabled = false;
        yield return 0;
        fitter.enabled = true;
    }

    public override void OnClose()
    {
        EventManager.TriggerEvent<bool>(ConstEvent.OnLockScroll, false);
        CameraMovement.Instance.LockMove(false);
        main.SetActive(false);
        videoPlayer.Stop();
    }

    public void OnClick(Button btn)
    {
        int index = btn.transform.GetSiblingIndex();
        videoPlayer.clip = clips[index];
        videoPlayer.Play();
        text.text = Localization.Get(ids[index]);
        StartCoroutine(DelayRefesh());
    }
}
