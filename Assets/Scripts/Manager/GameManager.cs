using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{

    private FadeScene fadeScene;
    private TimeScale timeScale = TimeScale.one;
    private TimeScale lastTimeScale = TimeScale.one;
    protected override void InstanceAwake()
    {
        DontDestroyOnLoad(this.gameObject);
        LoadAB.Init();
    }


    public void TogglePauseGame(out TimeScale scale)
    {
        if(timeScale != TimeScale.stop)
        {
            PauseGame();
            scale = TimeScale.stop;
        }
        else
        {
            ContinueGame();
            scale = lastTimeScale;
        }
    }

    public void PauseGame()
    {
        SetTimeScale(TimeScale.stop);
    }

    public void ContinueGame()
    {
        SetTimeScale(lastTimeScale);
        EventManager.TriggerEvent(ConstEvent.OnResumeGame);
    }
    public TimeScale GetTimeScale()
    {
        return timeScale;
    }
    public void AddTimeScale(out TimeScale addedScale)
    {
        int scale = ((int)timeScale+1)%4;
        timeScale = (TimeScale)System.Enum.ToObject(typeof(TimeScale), scale);
        addedScale = timeScale;
        SetTimeScale(timeScale);
    }
    public void SetOneTimeScale()
    {
        SetTimeScale(TimeScale.one);
    }

    public void SetTwoTimeScale()
    {
        SetTimeScale(TimeScale.two);
    }

    public void SetFourTimeScale()
    {
        SetTimeScale(TimeScale.four);
    }

    /// <summary>
    /// 设置游戏的播放速率
    /// </summary>
    /// <param name="timeScale"></param>
    private void SetTimeScale(TimeScale scale)
    {
        //Debug.Log("时间倍速:" + scale.ToString());
        timeScale = scale;
        switch (scale)
        {
            case TimeScale.stop:
                EventManager.TriggerEvent(ConstEvent.OnPauseGame);
                break;
            case TimeScale.one:
                Time.timeScale = 1.0f;
                lastTimeScale = timeScale;
                EventManager.TriggerEvent(ConstEvent.OnResumeGame);
                break;
            case TimeScale.two:
                Time.timeScale = 2.0f;
                lastTimeScale = timeScale;
                break;
            case TimeScale.four:
                Time.timeScale = 4.0f;
                lastTimeScale = timeScale;
                break;
        }

        EventManager.TriggerEvent<TimeScale>(ConstEvent.OnTimeScaleChanged, timeScale);
    }
    public void QuitApplication()
    {
         Application.Quit();
    }

    public void LoadLevelScene(int levelNum)
    {
        SetTimeScale(TimeScale.one);
        LoadScene(levelNum);
    }
    public void LoadExampleScene()
    {
        FindFadeImage();
        fadeScene.Fade(1f, 0.5f);
        StartCoroutine(DelayToInvokeDo(() => { SceneManager.LoadScene("MainScene", LoadSceneMode.Single); }, 1f));
    }
    public void LoadMenuScene()
    {
        StaticBuilding.lists = new List<StaticBuilding>();
        FindFadeImage();
        fadeScene.Fade(1f, 0.5f);
        StartCoroutine(DelayToInvokeDo(() => { SceneManager.LoadScene("MenuScene", LoadSceneMode.Single); }, 1f));
    }

    //重载，用于加载游戏关卡场景
    private void LoadScene(int levelNum)
    {
        FindFadeImage();
        fadeScene.Fade(1f, 0.5f);
        //JsonIO.InitLevelData(iLevel);
        StartCoroutine(DelayToInvokeDo(() => { SceneManager.LoadScene(levelNum.ToString(), LoadSceneMode.Single); }, 1f));
        //JsonIO.InitLevelData(ilevel);//更新关卡数据
    }

    private void FindFadeImage()
    {
        if (GameObject.Find("FadeImage"))
        {
            fadeScene = GameObject.Find("FadeImage").GetComponent<FadeScene>();
            //Debug.Log("FindFadeImage");
        }
        else
        {
            fadeScene = null;
            Debug.Log("There is no GameObject names 'FadeImage' in this scene");
        }
    }
    public IEnumerator DelayToInvokeDo(UnityAction action, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        action();
    }

}
