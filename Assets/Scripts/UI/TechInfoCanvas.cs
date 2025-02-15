﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TechInfoCanvas : MonoBehaviour
{
    [SerializeField] TMP_Text introduce;
    [SerializeField] TMP_Text title;
    [SerializeField] Button unlock;
    [SerializeField] Button use;
    [SerializeField] TMP_Text useText;

    public void ShowTechInfo(TechItem tech)
    {
        unlock.onClick.RemoveAllListeners();
        use.onClick.RemoveAllListeners();
        gameObject.SetActive(true);
        int index = tech.TechId - 40001;
        TechData data = DataManager.GetTechData(tech.TechId);
        introduce.text = Localization.Get(data.Id+"Intro");
        title.text = Localization.Get(data.Name);
        transform.position = tech.transform.position + new Vector3(170,125,0);
        //如果没有启动过
        if (!TechManager.Instance.GetTechAvalible(tech.TechId))
        {
            unlock.onClick.AddListener(() => OnClickUnlock(tech));
            unlock.gameObject.SetActive(true);
            use.gameObject.SetActive(false);
        }
        else
        {
            WhenTechAvalible(tech);
        }
    }

    public void OnClickUnlock(TechItem tech)
    {
        TechManager.Instance.OpenTech(tech,()=> {
            tech.SetUnlockSprite(); 
            WhenTechAvalible(tech);
        });
    }
    public void WhenTechAvalible(TechItem tech)
    {
        unlock.gameObject.SetActive(false);
        use.gameObject.SetActive(true);
        if (TechManager.Instance.GetTech(tech.TechId))
        {
            useText.text = Localization.Get("Using");
            use.interactable = false;
        }
        else
        {
            use.onClick.AddListener(() => { TechManager.Instance.SetTech(tech); useText.text = Localization.Get("Using"); use.interactable = false; });
            use.interactable = true;
            useText.text = Localization.Get("Use");
        }
    }

    public void CloseTechInfo()
    {
        gameObject.SetActive(false);
    }
}
