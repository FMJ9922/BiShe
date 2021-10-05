using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Localization
{
    public static LanguageType language = LanguageType.Chinese;

    public delegate void ChangeLanguage();
    public static event ChangeLanguage OnChangeLanguage;

    public static List<string> SupportLanguageList = new List<string>
    {
        "简体中文",
        "English"
    };

    public static string Get(string itemName)
    {
        switch (language)
        {
            case LanguageType.Chinese:
                {
                    return ItemNameToChinese(itemName);
                }
            case LanguageType.English:
                {
                    return ItemNameToEnglish(itemName);
                }
        }
        return itemName;
    }
    public static string ItemNameToChinese(string itemName)
    {
        LocalizationCombine[] combines = DataManager.Instance.LocalizationData.combines;
        for (int i = 0; i < combines.Length; i++)
        {
            if (combines[i].code == itemName)
            {
                return combines[i].chinese;
            }
        }
        return itemName;
    }

    public static string ItemNameToEnglish(string itemName)
    {
        LocalizationCombine[] combines = DataManager.Instance.LocalizationData.combines;
        for (int i = 0; i < combines.Length; i++)
        {
            if (combines[i].code == itemName)
            {
                return combines[i].english;
            }
        }
        return itemName;
    }

    public static void ChangeSettingLanguage(LanguageType languageType)
    {
        //Debug.Log("change" + languageType);
        language = languageType;
        PlayerPrefs.SetInt("Language", (int)languageType);
        if (OnChangeLanguage != null)
        {
            OnChangeLanguage();
        }
    }

    public static void ChangeSettingLanguage(int languageType)
    {
        //Debug.Log("change" + languageType);
        language = (LanguageType)languageType;
        PlayerPrefs.SetInt("Language", (int)languageType);
        if (OnChangeLanguage != null)
        {
            OnChangeLanguage();
        }
    }

}
