using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScreenRatioOption : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] Toggle toggle;

    public List<ScreenRatio> sreeenRatios = new List<ScreenRatio>()
    {
        new ScreenRatio(1280,720),
        new ScreenRatio(1366,768),
        new ScreenRatio(1360,768),
        new ScreenRatio(1600,900),
        new ScreenRatio(1920,1080),
        new ScreenRatio(1280,800),
        new ScreenRatio(1440,900),
        new ScreenRatio(1680,1050),
        new ScreenRatio(1920,1200),
        new ScreenRatio(800,600),
        new ScreenRatio(1024,768),
        new ScreenRatio(1280,960),
        new ScreenRatio(1400,1050),
        new ScreenRatio(1600,1200),
        new ScreenRatio(1920,1440),
        new ScreenRatio(2048,1536),
        new ScreenRatio(2048,1080),
        new ScreenRatio(4096,2160),
        new ScreenRatio(3440,1440),
        new ScreenRatio(3840,2160),
        new ScreenRatio(3656,2664),
        new ScreenRatio(4096,3112),
    };

    private void Awake()
    {
        sreeenRatios.Sort(Sort);
        TMP_Dropdown.OptionData tempData;
        dropdown.ClearOptions();
        for (int i = 0; i < sreeenRatios.Count; i++)
        {
            tempData = new TMP_Dropdown.OptionData();
            tempData.text = sreeenRatios[i].ToString();
            dropdown.options.Add(tempData);
        }
        if (PlayerPrefs.HasKey("FullScreen"))
        {
            toggle.isOn = PlayerPrefs.GetInt("FullScreen") == 1;
            ChangeFullScreen(toggle.isOn);
        }
        else
        {
            toggle.isOn = true;
        }
        toggle.onValueChanged.AddListener(ChangeFullScreen);
        dropdown.onValueChanged.AddListener(ChangeScreenRatio);
        if (PlayerPrefs.HasKey("ScreenRatio"))
        {
            ScreenRatio ratio = sreeenRatios[PlayerPrefs.GetInt("ScreenRatio")];
            Screen.SetResolution(ratio.width, ratio.height, toggle.isOn);
            dropdown.value = PlayerPrefs.GetInt("ScreenRatio");
        }
        dropdown.RefreshShownValue();
        Invoke("ResetText",0.1f) ;
    }

    public void ResetText()
    {
        dropdown.captionText.text = Screen.width + " * " + Screen.height;
    }

    public void ChangeFullScreen(bool full)
    {
        int width = Screen.width;
        int height = Screen.height;
        PlayerPrefs.SetInt("FullScreen", full ? 1 : 0);
        Screen.SetResolution(width, height, full);
    }

    public void ChangeScreenRatio(int index)
    {
        ScreenRatio ratio = sreeenRatios[index];
        Screen.SetResolution(ratio.width, ratio.height, toggle.isOn);
        dropdown.captionText.text = ratio.width + " * " + ratio.height;
        PlayerPrefs.SetInt("ScreenRatio", index);
    }

    public int Sort(ScreenRatio a, ScreenRatio b)
    {
        return (a.width - b.width)*4000+ (a.height - b.height);
    }
}

public struct ScreenRatio
{
    public int width;
    public int height;
    public ScreenRatio(int w,int h)
    {
        width = w;
        height = h;
    }

    public override string ToString()
    {
        return width + " * " + height;
    }

    
}