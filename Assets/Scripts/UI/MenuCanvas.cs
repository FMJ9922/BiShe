using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCanvas : MonoBehaviour
{

    public GameSelectCanvas selectCanvas;
    public GameObject settingCanvas;
    public GameObject infoCanvas;
    public SaveCanvas saveCanvas;

    private void Start()
    {
        saveCanvas.InitCanvas();
    }
    public void OpenSelectCanvas()
    {
        selectCanvas.gameObject.SetActive(true);
        settingCanvas.gameObject.SetActive(false);
        infoCanvas.gameObject.SetActive(false);
        selectCanvas.GetComponent<GameSelectCanvas>().ShowDefault();
        saveCanvas.OnClose();
    }

    public void OpenInfoCanvas()
    {
        selectCanvas.gameObject.SetActive(false);
        settingCanvas.gameObject.SetActive(false);
        infoCanvas.gameObject.SetActive(true);
        saveCanvas.OnClose();
    }

    public void OpenSettingCanvas()
    {
        selectCanvas.gameObject.SetActive(false);
        settingCanvas.gameObject.SetActive(true);
        infoCanvas.gameObject.SetActive(false);
        saveCanvas.OnClose();
    }


    public void OpenGameSaveCanvas()
    {
        selectCanvas.gameObject.SetActive(false);
        settingCanvas.gameObject.SetActive(false);
        infoCanvas.gameObject.SetActive(false);
        saveCanvas.OnOpen();
    }

    public void Exit()
    {
        GameManager.Instance.QuitApplication();
    }
}
