using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CommonIcon : MonoBehaviour,IPointerClickHandler
{
    #region 组件
    [SerializeField] private TMP_Text _indexName;
    #endregion

    #region 字段&属性
    public int _Index;
    public IconType _IconType;
    private TipManager _tipManager;
    #endregion

    #region 接口

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_tipManager)
        {
            _tipManager = TipManager.Instance;
        }

        if (_tipManager.IsShowing)
        {
            _tipManager.CloseTip();
        }
        else 
        {
            _tipManager.ShowTip(_indexName.text, _Index, transform.position, _IconType);
        }

    }

    #endregion
}
