using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveItem : MonoBehaviour
{
    [SerializeField] TMP_Text nameLabel;
    [SerializeField] Button overwrite;
    [SerializeField] Button load;
    [SerializeField] Button delete;
    private SaveCanvas saveCanvas;
    private string menuPath;
    private string fileName;
    private bool lockbtn =false;
    [SerializeField] private Sprite unClick;
    [SerializeField] private Sprite clicked;
    [SerializeField] private Image image;

    public string GetSavePath()
    {
        return menuPath + "/" + fileName;
    }
    public void GenerateSave(string saveName)
    {
        SaveData data = GameManager.Instance.MakeCustomSaveData(saveName);
        GameSaver.WriteSaveData(saveName, data, data.isOffcial);
        nameLabel.text = saveName;
        fileName = saveName;
    }

    public void InitSave(string saveName,bool isInMenu = false)
    {
        menuPath = Application.dataPath + "/Resources/Saves";
        if (!isInMenu)
        {
            saveCanvas = MainInteractCanvas.Instance.GetSaveCanvas();
        }
        else
        {
            overwrite.gameObject.SetActive(false);
        }
        fileName = saveName;
        nameLabel.text = saveName;
    }


    public void Delete() 
    {
        //Debug.Log(GetSavePath());
        GameSaver.DeleteAllFile(GetSavePath());
        GameSaver.DeleteSaveData(menuPath, fileName);
        Destroy(gameObject);
    }

    public void Load()
    {
        GameManager.Instance.LoadLevelFromSaveData(fileName);
    }

    public void Overwrite()
    {
        if (!lockbtn)
        {
            StartCoroutine("Generate1", fileName);
        }
    }

    public void SetSprite()
    {
        image.sprite = clicked;
    }

    public void SetUnclick()
    {
        image.sprite = unClick;
    }

    IEnumerator Generate1(string saveName)
    {
        lockbtn = true;
        //Debug.Log("1");
        if (null == saveCanvas)
        {
            saveCanvas = MainInteractCanvas.Instance.GetSaveCanvas();
        }
        //saveCanvas.ShowSavingLabel();
        yield return 0;
        //Debug.Log("2");
        lockbtn = false;
        GenerateSave(saveName);
        yield return 0;
        saveCanvas.ShowSaveList();
        //Debug.Log("Finish???????");
        //Debug.Log("Finish?");
        yield return 0;

    }

}
