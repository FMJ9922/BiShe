using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoticeManager : Singleton<NoticeManager>
{
    [SerializeField] NoticeCanvas canvas;
    private bool lockShow = false;

    private void ToggleVisable(bool vis)
    {
        if (vis)
        {
            canvas.gameObject.SetActive(true);
            EventManager.StartListening<Vector3>(ConstEvent.OnGroundRayPosMove, TraceMouse);
        }
        else
        {
            canvas.gameObject.SetActive(false);
            EventManager.StopListening<Vector3>(ConstEvent.OnGroundRayPosMove, TraceMouse);
        }
    }

    private void TraceMouse(Vector3 pos)
    {
        Vector2 vec = RectTransformUtility.WorldToScreenPoint(Camera.main, pos);
        canvas.transform.position = vec + new Vector2(20, 0) * GameManager.Instance.GetScreenRelativeRate();

    }

    public void ShowIconNotice(string content)
    {
        if (!lockShow)
        {
            TraceMouse(InputManager.Instance.LastGroundRayPos);
            canvas.SetText(content);
            ToggleVisable(true);
        }
    }

    public void CloseNotice()
    {
        lockShow = false;
        ToggleVisable(false);
    }


    public void InvokeShowNotice(string content,float time = 3f)
    {
        ShowIconNotice(content);
        lockShow = true;
        StartCoroutine(Close(time));
        
    }

    IEnumerator Close(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        CloseNotice();
    }

}
