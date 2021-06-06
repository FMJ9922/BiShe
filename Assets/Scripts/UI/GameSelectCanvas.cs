using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameSelectCanvas : Singleton<GameSelectCanvas>
{
    public TMP_Text levelName;
    public TMP_Text aim;
    public TMP_Text special;
    public Image preview;
    public Button enterBtn;
    public Transform content;
    public GameObject itemPfb;
    public Button enterExample;

    void Start()
    {
        enterExample.onClick.AddListener(GameManager.Instance.LoadExampleScene);
        LevelData[] data = DataManager.GetAllLevelDatas();
        LevelSelectItem.items = new List<LevelSelectItem>();
        for (int i = 0; i < data.Length; i++)
        {
            GameObject levelItems = Instantiate(itemPfb);
            levelItems.transform.SetParent(content);
            levelItems.transform.SetSiblingIndex(content.childCount - 2);
            levelItems.GetComponent<LevelSelectItem>().Init(data[i]);
        }
        LevelSelectItem.items[0].ShowOutline();
        ChangeShowLevel(data[0]);
    }

    public void ChangeShowLevel(LevelData data)
    {
        levelName.text = data.Name;
        aim.text = data.Aim1 + '\n' + data.Aim2 + '\n' + data.Aim3;
        special.text = data.specialIntroduce;
        preview.sprite = LoadAB.LoadSprite("mat.ab", data.Id + "preview");
        enterBtn.onClick.RemoveAllListeners();
        int id = data.Id;
        enterBtn.onClick.AddListener(() => GameManager.Instance.LoadLevelScene(id));
    }
}
