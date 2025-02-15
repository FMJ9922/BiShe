﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LanguageOption : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] TMP_Text text;

    private void Start()
    {
        int id = (int)Localization.Language > Localization.SupportLanguageList.Count - 1 ?
                Localization.SupportLanguageList.Count - 1 :
                (int)Localization.Language;
        if (dropdown!=null)
        {
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener(ChangeLangauge);
            dropdown.value = PlayerPrefs.GetInt("Language");
            dropdown.captionText.text = Localization.SupportLanguageList[id];
            if (PlayerPrefs.HasKey("Language"))
            {
                Localization.ChangeSettingLanguage((LanguageType)dropdown.value);
            }
        }
        if (text != null)
        {
            
            text.text = Localization.SupportLanguageList[id];
        }
    }

    public void ChangeLangauge(int value)
    {
        Localization.ChangeSettingLanguage((LanguageType)value);
        PlayerPrefs.SetInt("Language", value);
        PlayerPrefs.Save();
        text.text = Localization.SupportLanguageList[value];
    }

    private void OnEnable()
    {
        if (dropdown != null)
        {
            int id = (int)Localization.Language > Localization.SupportLanguageList.Count - 1 ?
                Localization.SupportLanguageList.Count - 1 :
                (int)Localization.Language;
            dropdown.captionText.text = Localization.SupportLanguageList[id];
        }
    }

    public void ChangeRelativeLanguage(int delta)
    {
        int cur = (int)Localization.Language;
        cur += delta;
        if (cur >= Localization.SupportLanguageList.Count)
        {
            cur -= Localization.SupportLanguageList.Count;
        }
        if (cur < 0)
        {
            cur = Localization.SupportLanguageList.Count - 1;
        }
        text.text = Localization.SupportLanguageList[cur];
        ChangeLangauge(cur);
    }
}
