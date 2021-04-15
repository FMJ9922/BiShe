using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    private TimeScale timeScale = TimeScale.one;
    protected override void InstanceAwake()
    {
        DontDestroyOnLoad(this.gameObject);
    }


    public void TogglePauseGame(out TimeScale scale)
    {
        if(timeScale != TimeScale.stop)
        {
            SetTimeScale(TimeScale.stop);
            scale = TimeScale.stop;
        }
        else
        {
            SetTimeScale(TimeScale.one);
            scale = TimeScale.one;
        }
    }

    public void PauseGame()
    {
        SetTimeScale(TimeScale.stop);
    }

    public void ContinueGame()
    {
        SetTimeScale(TimeScale.one);
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
        EventManager.TriggerEvent<TimeScale>(ConstEvent.OnTimeScaleChanged, timeScale);
    }

    /// <summary>
    /// 设置游戏的播放速率
    /// </summary>
    /// <param name="timeScale"></param>
    private void SetTimeScale(TimeScale scale)
    {
        Debug.Log("时间倍速:" + scale.ToString());
        timeScale = scale;
        switch (scale)
        {
            case TimeScale.stop:
                Time.timeScale = 0.0f;
                CameraMovement.Instance.StopMovement();
                break;
            case TimeScale.one:
                Time.timeScale = 1.0f;
                CameraMovement.Instance.AllowMovement();
                break;
            case TimeScale.two:
                Time.timeScale = 2.0f;
                CameraMovement.Instance.AllowMovement();
                break;
            case TimeScale.four:
                Time.timeScale = 4.0f;
                CameraMovement.Instance.AllowMovement();
                break;
        }
    }
}
