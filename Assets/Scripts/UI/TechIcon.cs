using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TechIcon : MonoBehaviour,IPointerClickHandler
{
    #region 组件
    [SerializeField] private TMP_Text _techName;
    #endregion

    #region 字段&属性
    public int _techIndex;
    private TechTipManager _techTipManager;
    #endregion

    #region 接口

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_techTipManager)
        {
            _techTipManager = TechTipManager.Instance;
        }

        if (_techTipManager.IsShowing && _techTipManager.CurrentIndex == _techIndex)
        {
            _techTipManager.CloseTechTip();
        }
        else
        {
            _techTipManager.ShowTechTip(_techName.text, _techIndex);
        }

    }

    #endregion
}
