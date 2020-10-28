using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalCanvas : MonoBehaviour
{
    [SerializeField] private Button _confirmBtn;

    private void Awake()
    {
        SetGameObject(true);
        _confirmBtn.onClick.AddListener(OnConfirmBtnClick);
        EventManager.StartListening(ConstEvent.OnMaskClicked, OnConfirmBtnClick);
    }

    #region 私有方法

    private void OnConfirmBtnClick()
    {
        SetGameObject(false);
        EventManager.StopListening(ConstEvent.OnMaskClicked, OnConfirmBtnClick);
    }

    private void SetGameObject(bool active)
    {
        gameObject.SetActive(active);
    }
    #endregion
}
