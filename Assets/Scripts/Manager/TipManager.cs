using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;

public class TipManager : Singleton<TipManager>
{
    #region 组件
    [SerializeField] private GameObject _tipPfb;
    #endregion

    #region 属性&字段

    public bool IsShowing { get { return _tipCanvas.gameObject.activeInHierarchy; } }

    public int CurrentIndex { get; private set; } = -1;

    public TechTreeAction techTreeAction;

    private TipCanvas _tipCanvas;

    #endregion

    #region 初始化


    private void Start()
    {
        InitTip();
        EventManager.StartListening(ConstEvent.OnMaskClicked, CloseTip);
    }

    private void OnDestroy()
    {
        EventManager.StopListening(ConstEvent.OnMaskClicked, CloseTip);
    }

    private void InitTip()
    {
        Transform canvasRoot = GameObject.Find(ConstString.CanvasRoot).transform;
        GameObject tipCanvas = Instantiate(_tipPfb, canvasRoot);
        _tipCanvas = tipCanvas.GetComponent<TipCanvas>();
        SetGameObject(false);
    }
    #endregion

    #region 公有方法


    public void ShowTip(string indexName, int index, Vector3 position,IconType iconType)
    {
        SetGameObject(true);
        if(iconType == IconType.Tech)
        {
            _tipCanvas.ShowTechTip(indexName, position);
        }
        else
        {
            _tipCanvas.ShowLevelTip(indexName, position);
        }
        
    }


    public void CloseTip()
    {
        SetGameObject(false);
    }
    #endregion

    #region 私有方法

    private void SetGameObject(bool active)
    {
        if (_tipCanvas)
        {
            _tipCanvas.gameObject.SetActive(active);
        }
    }
    #endregion
}
