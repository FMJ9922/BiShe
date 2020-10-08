using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TechTipManager : Singleton<TechTipManager>
{
    #region 组件
    [SerializeField] private TMP_Text _techName;
    [SerializeField] private TMP_Text _techExplain;
    #endregion

    #region 属性&字段
    /// <summary>
    /// 是否显示
    /// </summary>
    public bool IsShowing { get { return Instance.gameObject.activeInHierarchy; } }

    /// <summary>
    /// 当前显示的科技索引序号
    /// </summary>
    public int CurrentIndex { get; private set; } = -1;

    public TechTreeAction techTreeAction;
    #endregion

    #region 初始化

    private void Start()
    {
        CloseTechTip();
    }

    private void OnDestroy()
    {

    }
    #endregion

    #region 公有方法

    
    public void ShowTechTip(string techName, int techIndex)
    {
        //排除异常
        if (techIndex >= 0)
        {
            CurrentIndex = techIndex;
        }
        else
        {
            Debug.LogError(string.Format("错误的科技序号:{0}", techIndex));
            return;
        }
        if (string.IsNullOrEmpty(techName))
        {
            Debug.LogError("科技名称为空");
            return;
        }
        //更新标题
        _techName.text = techName;

        SetGameObject(true);
    }

    public void CloseTechTip()
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
