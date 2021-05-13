using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCanvas : MonoBehaviour
{

    public GameSelectCanvas selectCanvas;
    public GameObject settingCanvas;
    public GameObject infoCanvas;

    public void OpenSelectCanvas()
    {
        selectCanvas.gameObject.SetActive(true);
        settingCanvas.gameObject.SetActive(false);
        infoCanvas.gameObject.SetActive(false);
    }

    public void OpenInfoCanvas()
    {
        selectCanvas.gameObject.SetActive(false);
        settingCanvas.gameObject.SetActive(false);
        infoCanvas.gameObject.SetActive(true);
    }

    public void OpenSettingCanvas()
    {
        selectCanvas.gameObject.SetActive(false);
        settingCanvas.gameObject.SetActive(true);
        infoCanvas.gameObject.SetActive(false);
    }

    public void Exit()
    {
        GameManager.Instance.QuitApplication();
    }
}
