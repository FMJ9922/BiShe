using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageOption : MonoBehaviour
{
    [SerializeField]Dropdown dropdown;


    private void Start()
    {
        dropdown.onValueChanged.AddListener(ChangeLangauge);
    }

    public void ChangeLangauge(int value)
    {
        Localization.ChangeSettingLanguage((LanguageType)value);
    }
}
