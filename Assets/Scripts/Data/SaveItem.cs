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
        SaveData data = GameManager.Instance.MakeSaveData(false);
        GameSaver.WriteSaveData(saveName, data, data.isOffcial);
        fileName = saveName;
    }

    public void Start()
    {
        menuPath = Application.dataPath + "/Assets/Resources/Saves";
        saveCanvas = MainInteractCanvas.Instance.GetSaveCanvas();
    }

    public void Delete() 
    {
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
        GameSaver.DeleteAllFile(GetSavePath());
        GenerateSave(fileName);
    }
    
}
