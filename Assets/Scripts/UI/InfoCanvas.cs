using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoCanvas : CanvasBase
{
    [SerializeField] GameObject _inputsObj;//挂载输入资源说明的UI
    [SerializeField] GameObject _outputsObj;//输出
    [SerializeField] GameObject _effectiveObj;//效率
    [SerializeField] GameObject _introduceObj;//简介说明
    [SerializeField] TMP_Text _nameLabel,_inputsLabel, _outputsLabel, _effectiveLabel, _introduceLabel;
    [SerializeField] private GameObject mainCanvas;
    public override void InitCanvas()
    {
        mainCanvas.SetActive(false);
        EventManager.StartListening<BuildData>(ConstEvent.OnTriggerInfoPanel, OnOpen);
    }
    public override void OnOpen()
    {
    }
    public void OnOpen(BuildData buildData)
    {
        ChangeShowItems(buildData.tabType);
        ChangeLabels(buildData);
        mainCanvas.SetActive(true);
    }

    public override void OnClose()
    {
        mainCanvas.SetActive(false);
    }
    private void OnDestroy()
    {
        EventManager.StopListening<BuildData>(ConstEvent.OnTriggerInfoPanel, OnOpen);
    }

    private void ChangeShowItems(BuildTabType buildTabType)
    {
        _inputsObj.SetActive(false);
        _outputsObj.SetActive(false);
        _effectiveObj.SetActive(false);
        _introduceObj.SetActive(false);
        switch (buildTabType)
        {
            case BuildTabType.agriculture:
            case BuildTabType.forest:
            case BuildTabType.manufacturing:
                {
                    _inputsObj.SetActive(true);
                    _outputsObj.SetActive(true);
                    _effectiveObj.SetActive(true);
                    _introduceObj.SetActive(true);
                    break;
                    
                }
            case BuildTabType.utility:
            case BuildTabType.house:
                {
                    _inputsObj.SetActive(true);
                    _outputsObj.SetActive(true);
                    _effectiveObj.SetActive(false);
                    _introduceObj.SetActive(true);
                    break;
                }

        }
    }

    private void ChangeLabels(BuildData buildData)
    {
        _nameLabel.text = Localization.ToSettingLanguage(buildData.Name);
        string input = string.Empty;
        for (int i = 0; i < buildData.inputResources.Count; i++)
        {
            input += buildData.inputResources[i].ItemNum + " " +
                Localization.ToSettingLanguage(
                    DataManager.GetItemNameById(buildData.inputResources[i].ItemId));
        }
        _inputsLabel.text = input;
        string output = string.Empty;
        for (int i = 0; i < buildData.outputResources.Count; i++)
        {
            output += buildData.outputResources[i].ItemNum + " " +
                Localization.ToSettingLanguage(
                    DataManager.GetItemNameById(buildData.outputResources[i].ItemId));
        }
        _outputsLabel.text = output;
        _introduceLabel.text = Localization.ToSettingLanguage(buildData.Introduce);
    }
}
