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

    public bool isInMenu;
    public GameObject addBtn;

    #region 实现基类
    public override void InitCanvas()
    {
        mainCanvas.SetActive(false);
        addBtn.SetActive(!isInMenu);
        InitSaveItems();
    }

    public void InitSaveItems()
    {
        //string[] dirs = Directory.GetDirectories(GetSaveParentPath(),".",SearchOption.TopDirectoryOnly);
        DirectoryInfo root = new DirectoryInfo(GetSaveParentPath());
        DirectoryInfo[] dics = root.GetDirectories();
        for (int i = 0; i < dics.Length; i++)
        {
            GameObject newSave = Instantiate(saveItemPfb, saveHolder);
            newSave.SetActive(true);
            newSave.transform.SetSiblingIndex(saveHolder.transform.childCount - 2);
            newSave.GetComponent<SaveItem>().InitSave(dics[i].Name,isInMenu);
        }
    }
    public override void OnOpen()
    {
        mainCanvas.SetActive(true);
        saveContent.SetActive(true);
        nameCanvas.SetActive(false);
        savingContent.SetActive(false);
        GameManager.Instance.PauseGame();
        EventManager.TriggerEvent<bool>(ConstEvent.OnLockScroll, true);
        EventManager.TriggerEvent<bool>(ConstEvent.OnLockMove, true);
    }

    public override void OnClose()
    {
        mainCanvas.SetActive(false);
        GameManager.Instance.ContinueGame();
        EventManager.TriggerEvent<bool>(ConstEvent.OnLockScroll, false);
        EventManager.TriggerEvent<bool>(ConstEvent.OnLockMove, false);
        if (!isInMenu)
        {
            MainInteractCanvas.Instance.OpenReturnCanvas();
        }
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
        bool hasSameFile = Directory.Exists(GetSavePath(saveName));
        //Debug.Log(GetSavePath(saveName));
        //Debug.Log(hasSameFile);
        if (hasSameFile)
        {
            NoticeManager.Instance.InvokeShowNotice("已经存在同名的存档！");
            return;
        }
        CreateNewSave(saveName);
    }

    public void ShowSaveList()
    {
        nameCanvas.SetActive(false);
        saveContent.SetActive(true);
        savingContent.SetActive(false);
    }

    public void CreateNewSave(string saveName)
    {
        ShowSavingLabel();
        StartCoroutine("Generate",saveName);
    }

    public void ShowSavingLabel()
    {
        nameCanvas.SetActive(false);
        saveContent.SetActive(false);
        savingContent.SetActive(true);
    }
    IEnumerator Generate(string saveName)
    {

        GameObject newSave = Instantiate(saveItemPfb, saveHolder);
        newSave.SetActive(true);
        newSave.transform.SetSiblingIndex(saveHolder.transform.childCount - 2);
        yield return 0;
        newSave.GetComponent<SaveItem>().GenerateSave(saveName);
        yield return 0;
        ShowSaveList();


    }

    public string GetSavePath(string saveName)
    {
        return Application.dataPath + "/" + GameSaver.GetCustomPath(saveName);
    }

    public string GetSaveParentPath()
    {
        return Application.dataPath + "/Resources/Saves";
    }

}
