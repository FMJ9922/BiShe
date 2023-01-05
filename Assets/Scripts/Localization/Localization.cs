using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Localization
{
    public static LanguageType Language = LanguageType.Chinese;

    public delegate void ChangeLanguage();
    public static event ChangeLanguage OnChangeLanguage;

    public static readonly List<string> SupportLanguageList = new List<string>
    {
        "简体中文",
        "English",
        "German"
    };

    public static string Get(string itemName)
    {
        switch (Language)
        {
            case LanguageType.Chinese:
                {
                    return ItemNameToChinese(itemName);
                }
            case LanguageType.English:
                {
                    return ItemNameToEnglish(itemName);
                }
            case LanguageType.German:
            {
                return ItemNameToGerman(itemName);
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

    public static string ItemNameToGerman(string itemName)
    {
        LocalizationCombine[] combines = DataManager.Instance.LocalizationData.combines;
        for (int i = 0; i < combines.Length; i++)
        {
            if (combines[i].code == itemName)
            {
                return combines[i].german;
            }
        }
        return itemName;
    }

    public static void ChangeSettingLanguage(LanguageType languageType)
    {
        //Debug.Log("change" + languageType);
        if (languageType == Language) return;
        Language = languageType;
        PlayerPrefs.SetInt("Language", (int)languageType);
        OnChangeLanguage?.Invoke();
    }

    public static void ChangeSettingLanguage(int languageType)
    {
        //Debug.Log("change" + languageType);
        Language = (LanguageType)languageType;
        PlayerPrefs.SetInt("Language", (int)languageType);
        OnChangeLanguage?.Invoke();
    }

}
