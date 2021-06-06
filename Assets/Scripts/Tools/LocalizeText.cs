using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 挂载在Text或TMP_text下来实现语言本地化
/// </summary>
public class LocalizeText : MonoBehaviour
{
    public string content; 
    void Start()
    {
        ChangeLabelLanguage();
        Localization.OnChangeLanguage += ChangeLabelLanguage;
    }

    private void OnDestroy()
    {
        Localization.OnChangeLanguage -= ChangeLabelLanguage;
    }
    public void ChangeLabelLanguage()
    {
        if (string.IsNullOrEmpty(content))
        {
            return;
        }
        Text text;
        if (transform.TryGetComponent<Text>(out text))
        {
            text.text = Localization.ToSettingLanguage(content);
        }
        TMP_Text tMP_Text;
        if (transform.TryGetComponent<TMP_Text>(out tMP_Text))
        {
            tMP_Text.text = Localization.ToSettingLanguage(content);
        }
    }

    
}
