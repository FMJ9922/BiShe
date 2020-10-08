using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechViewCanvas : MonoBehaviour
{
    #region 公有方法
    public void OpenToUpgrade()
    {
        OpenTechView(TechTreeAction.Upgrade);
    }

    /// <summary>
    /// 打开科技树面板
    /// </summary>
    /// <param name="techType">打开哪个科技树面板</param>
    /// <param name="techTreeAction">打开科技树的目的</param>
    public void OpenTechView(TechTreeAction techTreeAction, TechType techType = TechType.Agriculture)
    {
        SetGameObject(true);
    }

    public void CloseTechView()
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
