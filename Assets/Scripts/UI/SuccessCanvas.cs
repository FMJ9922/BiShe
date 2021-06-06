using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuccessCanvas : CanvasBase
{
    public override void InitCanvas()
    {
        gameObject.SetActive(false);
    }

    public override void OnOpen()
    {
        GameManager.Instance.SetOneTimeScale();
        gameObject.SetActive(true);
    }

    public override void OnClose()
    {
        gameObject.SetActive(false);
    }
}
