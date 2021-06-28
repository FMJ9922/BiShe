using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class SaveCanvas : CanvasBase
{
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject nameCanvas;
    public List<SaveItem> saveItems = new List<SaveItem>();
    [SerializeField] GameObject saveContent;
    [SerializeField] GameObject savingContent;
    [SerializeField] Transform saveHolder;
    [SerializeField] GameObject saveItemPfb;
    [SerializeField] TMP_InputField inputField;


    #region 实现基类
    public override void InitCanvas()
    {
        mainCanvas.SetActive(false);
    }
    public override void OnOpen()
    {
        mainCanvas.SetActive(true);
        saveContent.SetActive(true);
        nameCanvas.SetActive(false);
        savingContent.SetActive(false);
        GameManager.Instance.PauseGame();
        EventManager.TriggerEvent<bool>(ConstEvent.OnLockScroll, true);
    }

    public override void OnClose()
    {
        mainCanvas.SetActive(false);
        GameManager.Instance.ContinueGame();
        EventManager.TriggerEvent<bool>(ConstEvent.OnLockScroll, false);
        MainInteractCanvas.Instance.OpenReturnCanvas();
    }
    #endregion
    public void InitNewSave()
    {
        inputField.text = string.Empty;
        nameCanvas.SetActive(true);
        saveContent.SetActive(false);
        savingContent.SetActive(false);
    }

    public void CloseCanvas()
    {
        OnClose();
    }

    public void ConfirmInputName()
    {
        string saveName = inputField.text;
        if (string.IsNullOrEmpty(saveName))
        {
            NoticeManager.Instance.InvokeShowNotice("存档名不能为空");
            return;
        }
        bool hasSameFile = File.Exists(GetSavePath(saveName));
        if (hasSameFile)
        {
            NoticeManager.Instance.InvokeShowNotice("已经存在同名的存档！");
            return;
        }
        CreateNewSave(saveName);
    }

    public void CancelNewSave()
    {
        nameCanvas.SetActive(false);
        saveContent.SetActive(true);
        savingContent.SetActive(false);
    }

    public void CreateNewSave(string saveName)
    {
        nameCanvas.SetActive(false);
        saveContent.SetActive(false);
        savingContent.SetActive(true);
        StartCoroutine("Generate",saveName);
        
    }
    IEnumerator Generate(string saveName)
    {

        GameObject newSave = Instantiate(saveItemPfb, saveHolder);
        yield return 0;
        newSave.GetComponent<SaveItem>().GenerateSave(saveName);
        yield return 0;
        CancelNewSave();


    }

    public string GetSavePath(string saveName)
    {
        return GameSaver.GetCustomPath(saveName);
    }
}
