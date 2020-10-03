using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectLevelIcon : MonoBehaviour, IPointerClickHandler
{
    #region 组件
    [SerializeField] private TMP_Text _townName;
    #endregion

    #region 字段&属性
    public int _levelIndex;
    private LevelTipManager _levelTipManager;
    #endregion

    #region 接口

    public void OnPointerClick(PointerEventData eventData)
    {
        if(_levelTipManager == null)
        {
            _levelTipManager = LevelTipManager.Instance;
        }

        if (_levelTipManager.IsShowing && _levelTipManager.CurrentIndex == _levelIndex)
        {
            _levelTipManager.CloseLevelTip();
        }
        else
        {
            _levelTipManager.ShowLevelTip(_townName.text, _levelIndex, transform.position);
        }

    }

    #endregion
}
