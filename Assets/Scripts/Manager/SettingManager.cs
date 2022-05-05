using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingManager : Singleton<SettingManager>
{
    // Start is called before the first frame update
    void Start()
    {
        InitFrameRate();
    }

    public void InitFrameRate()
    {
        int index = PlayerPrefs.GetInt("FrameRate", 4);
        ChangeFrameRate(index);
    }

    public void ChangeFrameRate(int index)
    {
        if(index >=9||index<0)
        {
            index = 4;
        }
        var rate = frameRate[index];
        Application.targetFrameRate = rate;
        PlayerPrefs.SetInt("FrameRate", index);
    }

    public List<int> frameRate = new List<int>
    {
        24,25,30,50,60,75,90,120,144,240
    };

    

}
