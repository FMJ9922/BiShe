using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MenuCanvas : MonoBehaviour
{

    [SerializeField]private GameSelectCanvas _selectCanvas;
    [SerializeField]private  GameObject _settingCanvas;
    [SerializeField]private  GameObject _infoCanvas;
    [SerializeField]private  SaveCanvas _saveCanvas;

    private void Start()
    {
        _saveCanvas.InitCanvas();
        Localization.OnChangeLanguage += OpenSelectCanvas;
    }

    private void OnDestroy()
    {
        Localization.OnChangeLanguage -= OpenSelectCanvas;
    }
    public void OpenSelectCanvas()
    {
        _selectCanvas.gameObject.SetActive(true);
        _settingCanvas.gameObject.SetActive(false);
        _infoCanvas.gameObject.SetActive(false);
        _selectCanvas.GetComponent<GameSelectCanvas>().ShowDefault();
        _saveCanvas.OnClose();
    }

    public void OpenInfoCanvas()
    {
        _selectCanvas.gameObject.SetActive(false);
        _settingCanvas.gameObject.SetActive(false);
        _saveCanvas.OnClose();
        _infoCanvas.gameObject.SetActive(true);
    }

    public void OpenSettingCanvas()
    {
        _selectCanvas.gameObject.SetActive(false);
        _infoCanvas.gameObject.SetActive(false);
        _saveCanvas.OnClose();
        _settingCanvas.gameObject.SetActive(true);
    }


    public void OpenGameSaveCanvas()
    {
        _selectCanvas.gameObject.SetActive(false);
        _settingCanvas.gameObject.SetActive(false);
        _infoCanvas.gameObject.SetActive(false);
        _saveCanvas.OnOpen();
    }

    public void Exit()
    {
        GameManager.Instance.QuitApplication();
    }
}
