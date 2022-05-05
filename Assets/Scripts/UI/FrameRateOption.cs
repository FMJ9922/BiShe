using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FrameRateOption : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown;

    private void Awake()
    {
        TMP_Dropdown.OptionData tempData;
        dropdown.ClearOptions();
        var frameRate = SettingManager.Instance.frameRate;
        for (int i = 0; i < frameRate.Count; i++)
        {
            tempData = new TMP_Dropdown.OptionData();
            tempData.text = frameRate[i].ToString();
            dropdown.options.Add(tempData);
        }
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener(ChangeFrameRate);
        dropdown.value = PlayerPrefs.GetInt("FrameRate", 4);
        dropdown.RefreshShownValue();
    }
    public void ChangeFrameRate(int index)
    {
        dropdown.value = index;
        int rate = SettingManager.Instance.frameRate[index];
        SettingManager.Instance.ChangeFrameRate(index);
        dropdown.captionText.text = rate.ToString();
    }
}
