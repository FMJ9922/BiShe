using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoticeManager : Singleton<NoticeManager>
{
    [SerializeField] NoticeCanvas canvas;

    private bool lockShow = false;

    private void ToggleVisable(bool vis,bool tracing = true)
    {
        if (vis)
        {
            canvas.gameObject.SetActive(true);
            if (tracing)
            {
                EventManager.StartListening<Vector3>(ConstEvent.OnGroundRayPosMove, TraceMouse);
            }
            else
            {
                EventManager.StopListening<Vector3>(ConstEvent.OnGroundRayPosMove, TraceMouse);
                Vector2 vec = RectTransformUtility.WorldToScreenPoint(Camera.main, InputManager.Instance.LastGroundRayPos);
                canvas.transform.position = vec + new Vector2(-120, 0) * GameManager.Instance.GetScreenRelativeRate();
            }
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
        if (!lockShow&&!CommonIcon.IsShowingOption)
        {
            TraceMouse(InputManager.Instance.LastGroundRayPos);
            canvas.SetText(content);
            ToggleVisable(true);
        }
    }

    public void ShowItemDetailInfo(int itemId)
    {
        CloseNotice();
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown, CloseNotice);
        canvas.SetDetailInfo(itemId);
        ToggleVisable(true, false);
    }

    //显示图标操作
    public void ShowIconOption(ItemData data)
    {
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown, CloseNotice);
        canvas.SetIconOption(data);
        ToggleVisable(true,false);
    }

    public void CloseNotice()
    {
        lockShow = false;
        ToggleVisable(false);
        EventManager.StopListening(ConstEvent.OnMouseRightButtonDown, CloseNotice);
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
