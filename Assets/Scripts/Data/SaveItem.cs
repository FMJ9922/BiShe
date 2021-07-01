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

    public void InitSave(string saveName)
    {
        fileName = saveName;
        nameLabel.text = saveName;
    }

    public void Start()
    {
        menuPath = Application.dataPath + "/Resources/Saves";
        saveCanvas = MainInteractCanvas.Instance.GetSaveCanvas();
    }

    public void Delete() 
    {
        Debug.Log(GetSavePath());
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
        StartCoroutine("Generate1", fileName);
    }

    IEnumerator Generate1(string saveName)
    {
        Debug.Log("1");
        saveCanvas.ShowSavingLabel();
        Debug.Log("2");
        GenerateSave(saveName);
        saveCanvas.ShowSaveList();
        Debug.Log("Finish???????");
        yield return 0;
        Debug.Log("Finish?");

    }

}
