using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine;

public class TipCanvas : MonoBehaviour
{
    #region 组件
    [SerializeField] private TMP_Text _indexName;
    [SerializeField] private TMP_Text _indexExplain;
    [SerializeField] private GameObject _scoreGroup;
    [SerializeField] private GameObject _techChooseGroup;
    [SerializeField] private GameObject _resourcesGroup;
    #endregion

    /// <summary>
    /// 提示是否显示
    /// </summary>
    public bool IsShowing { get { return gameObject.activeInHierarchy; } }

    #region 公有方法

    
    public void ShowLevelTip(string levelName, Vector3 position)
    {
        //更新标题
        _indexName.text = levelName;
        //同步位置
        transform.position = position;
        _scoreGroup.SetActive(true);
        _techChooseGroup.SetActive(true);
        _resourcesGroup.SetActive(true);

    }

    public void ShowTechTip(string techName, Vector3 position)
    {
        _indexName.text = techName;
        transform.position = position;
        _scoreGroup.SetActive(false);
        _techChooseGroup.SetActive(false);
        _resourcesGroup.SetActive(false);
    }
    #endregion
}
