using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelTipManager : Singleton<LevelTipManager>
{
    #region 组件
    [SerializeField] private TMP_Text _levelName;
    [SerializeField] private TMP_Text _levelExplain;
    #endregion

    #region 属性&字段
    /// <summary>
    /// 提示是否显示
    /// </summary>
    public bool IsShowing{ get { return Instance.gameObject.activeInHierarchy; }}

    /// <summary>
    /// 当前显示的关卡索引序号
    /// </summary>
    public int CurrentIndex { get; private set; } = -1;
    #endregion

    #region 初始化

    private void Start()
    {
        EventManager.StartListening(ConstEvent.OnMaskClicked, CloseLevelTip);
        CloseLevelTip();
    }

    private void OnDestroy()
    {
        EventManager.StopListening(ConstEvent.OnMaskClicked, CloseLevelTip);
    }
    #endregion

    #region 公有方法

    /// <summary>
    /// 显示关卡预览提示
    /// </summary>
    /// <param name="levelName">关卡名</param>
    /// <param name="levelIndex">关卡索引序号，用于在数据库里查找该关数据</param>
    /// <param name="position">关卡预览提示的显示位置</param>
    public void ShowLevelTip(string levelName,int levelIndex,Vector3 position)
    {
        //排除异常
        if (levelIndex > 0)
        {
            CurrentIndex = levelIndex;
        }
        else
        {
            Debug.LogError(string.Format("错误的关卡序号:{0}", levelIndex));
            return;
        }
        if (string.IsNullOrEmpty(levelName))
        {
            Debug.LogError("关卡名称为空");
            return;
        }
        //更新标题
        _levelName.text = levelName;
        //同步位置
        transform.position = position;

        SetGameObject(true);
    }
    
    public void CloseLevelTip()
    {
        SetGameObject(false);
    }
    #endregion

    #region 私有方法

    private void SetGameObject(bool active)
    {
        gameObject.SetActive(active);
    }
    #endregion
}
