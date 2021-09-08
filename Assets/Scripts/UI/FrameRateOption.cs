using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FrameRateOption : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown;

    public List<int> frameRate = new List<int>
    {
        24,25,30,50,60,75,90,120,144,240
    };

    private void Awake()
    {
        TMP_Dropdown.OptionData tempData;
        dropdown.ClearOptions();
        for (int i = 0; i < frameRate.Count; i++)
        {
            tempData = new TMP_Dropdown.OptionData();
            tempData.text = frameRate[i].ToString();
            dropdown.options.Add(tempData);
        }
        dropdown.onValueChanged.AddListener(ChangeFrameRate);
        int rate = frameRate[PlayerPrefs.GetInt("FrameRate",4)];
        Application.targetFrameRate = rate;
        dropdown.value = PlayerPrefs.GetInt("FrameRate", 4);
        dropdown.RefreshShownValue();
        Invoke("ResetText", 0.1f);
    }

    public void ResetText()
    {
        dropdown.captionText.text = PlayerPrefs.GetInt("FrameRate", 4).ToString();
    }
    public void ChangeFrameRate(int index)
    {
        int rate = frameRate[index];
        Application.targetFrameRate = rate;
        PlayerPrefs.SetInt("FrameRate", index);
        dropdown.captionText.text = rate.ToString();
    }
}
