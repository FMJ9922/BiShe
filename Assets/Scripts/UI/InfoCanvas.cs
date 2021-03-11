using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InfoCanvas : CanvasBase
{
    [SerializeField] GameObject _inputsObj;//挂载输入资源说明的UI
    [SerializeField] GameObject _outputsObj;//输出
    [SerializeField] GameObject _effectiveObj;//效率
    [SerializeField] GameObject _introduceObj;//简介说明
    [SerializeField] Button _pauseBtn;//暂停生产
    [SerializeField] Button _upgradeBtn;//升级
    [SerializeField] TMP_Text _nameLabel, _inputsLabel, _outputsLabel, _effectiveLabel, _introduceLabel;
    [SerializeField] private GameObject mainCanvas;
    public override void InitCanvas()
    {
        mainCanvas.SetActive(false);
        EventManager.StartListening<RuntimeBuildData>(ConstEvent.OnTriggerInfoPanel, OnOpen);
    }
    public override void OnOpen()
    {
    }
    public void OnOpen(RuntimeBuildData buildData)
    {
        ChangeShowItems(buildData);
        ChangeLabels(buildData);
        mainCanvas.SetActive(true);
    }

    public override void OnClose()
    {
        mainCanvas.SetActive(false);
    }
    private void OnDestroy()
    {
        EventManager.StopListening<RuntimeBuildData>(ConstEvent.OnTriggerInfoPanel, OnOpen);
    }

    /// <summary>
    /// 修改显示的条目
    /// </summary>
    /// <param name="buildData"></param>
    private void ChangeShowItems(RuntimeBuildData buildData)
    {
        switch (buildData.tabType)
        {
            case BuildTabType.agriculture:
            case BuildTabType.forest:
            case BuildTabType.manufacturing:
                {
                    _inputsObj.SetActive(true);
                    _outputsObj.SetActive(true);
                    _effectiveObj.SetActive(true);
                    _introduceObj.SetActive(true);
                    _pauseBtn.gameObject.SetActive(true);
                    _upgradeBtn.gameObject.SetActive(buildData.CurLevel < 3);
                    break;

                }
            case BuildTabType.utility:
            case BuildTabType.house:
                {
                    _inputsObj.SetActive(true);
                    _outputsObj.SetActive(true);
                    _effectiveObj.SetActive(false);
                    _introduceObj.SetActive(true);
                    _pauseBtn.gameObject.SetActive(false);
                    _upgradeBtn.gameObject.SetActive(buildData.CurLevel < 3);
                    break;
                }

        }
    }

    private void UpdateButtonsListener()
    {
        _pauseBtn.onClick.RemoveAllListeners();
        _upgradeBtn.onClick.RemoveAllListeners();
        _pauseBtn.onClick.AddListener(TogglePause);
        //todo:升到满级锁定，暂停生产切换
    }

    private void TogglePause()
    {

    }
    /// <summary>
    /// 修改显示的文本
    /// </summary>
    /// <param name="buildData"></param>
    private void ChangeLabels(RuntimeBuildData buildData)
    {
        _nameLabel.text = Localization.ToSettingLanguage(buildData.Name);
        FormulaData formula = buildData.formulaDatas[buildData.CurFormula];
        string input = string.Empty;
        for (int i = 0; i < formula.InputItemID.Count; i++)
        {
            if (formula.InputNum[i] == 0)
            {
                continue;
            }
            input += formula.InputNum[i] + " " +
                Localization.ToSettingLanguage(
                    DataManager.GetItemNameById(formula.InputItemID[i]));
        }
        _inputsLabel.text = input;
        string output = string.Empty;
        for (int i = 0; i < formula.OutputItemID.Count; i++)
        {
            if (formula.ProductNum[i] == 0)
            {
                continue;
            }
            output += formula.ProductNum[i] + " " +
                Localization.ToSettingLanguage(
                    DataManager.GetItemNameById(formula.OutputItemID[i]));
        }
        _outputsLabel.text = output;
        _introduceLabel.text = Localization.ToSettingLanguage(buildData.Introduce);
    }
}
