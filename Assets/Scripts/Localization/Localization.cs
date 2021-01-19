using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Localization
{
    public static LanguageType language = LanguageType.chinese;

    public static string ToSettingLanguage(string itemName)
    {
        switch (language)
        {
            case LanguageType.chinese:
                {
                    return ItemNameToChinese(itemName);
                }
            case LanguageType.english:
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

}
