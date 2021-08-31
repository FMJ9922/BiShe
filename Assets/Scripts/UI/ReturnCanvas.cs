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
        GameManager.Instance.PauseGame();
        gameObject.SetActive(true);
    }

    public override void OnClose()
    {
        GameManager.Instance.ContinueGame();
        gameObject.SetActive(false);
    }

    public void ReturnToMenu()
    {
        GameManager.Instance.LoadMenuScene();
    }

    public void ExitApp()
    {
        GameManager.Instance.QuitApplication();
    }

    public void OpenSaveCanvas()
    {
        OnClose();
        MainInteractCanvas.Instance.OpenSaveCanvas();
    }
}
