using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnCanvas : CanvasBase
{
    public override void InitCanvas()
    {
        gameObject.SetActive(false);
    }

    public override void OnOpen()
    {
        gameObject.SetActive(true);
    }

    public override void OnClose()
    {
        gameObject.SetActive(false);
    }

    public void ReturnToMenu()
    {
        GameManager.Instance.LoadMenuScene();
    }
}
