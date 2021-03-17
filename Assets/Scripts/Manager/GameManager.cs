using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    protected override void InstanceAwake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// 设置游戏的播放速率
    /// </summary>
    /// <param name="timeScale"></param>
    public void SetTimeScale(TimeScale timeScale)
    {
        Debug.Log("时间倍速:" + timeScale.ToString());
        switch (timeScale)
        {
            case TimeScale.stop:
                Time.timeScale = 0.0f;
                break;
            case TimeScale.one:
                Time.timeScale = 1.0f;
                break;
            case TimeScale.two:
                Time.timeScale = 2.0f;
                break;
            case TimeScale.four:
                Time.timeScale = 4.0f;
                break;
        }
    }
}
