using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
public class InfoCanvas : CanvasBase
{
    [SerializeField] GameObject _inputsObj;//挂载输入资源说明的UI
    [SerializeField] GameObject _outputsObj;//输出
    [SerializeField] GameObject _effectiveObj;//效率
    [SerializeField] GameObject _introduceObj;//简介说明
    [SerializeField] GameObject _populationObj;//人口
    [SerializeField] GameObject _formulaObj;//配方
    [SerializeField] Button _destroyBtn;//拆除建筑
    [SerializeField] Button _upgradeBtn;//升级
    [SerializeField] Button[] _populationBtns;//人口相关按钮
    [SerializeField] TMP_Text _nameLabel, _inputsLabel, _outputsLabel, _effectiveLabel, _introduceLabel, _populationLabel;
    [SerializeField] TMP_Dropdown _dropDown;
    [SerializeField] private GameObject mainCanvas;
    UnityAction populationChange;
    private RuntimeBuildData _buildData;
    public override void InitCanvas()
    {
        mainCanvas.SetActive(false);
        populationChange = () => ChangeLabels(_buildData);
        EventManager.StartListening<BuildingBase>(ConstEvent.OnTriggerInfoPanel, OnOpen);
    }
    public override void OnOpen()
    {
        
    }
    public void OnOpen(BuildingBase buildbase)
    {
        _buildData = buildbase.runtimeBuildData;
        ChangeShowItems(_buildData);
        ChangeLabels(_buildData);
        mainCanvas.SetActive(true);
        ChangeFormula(_buildData.formulaDatas);
        AddBtnsListener(buildbase);
        _dropDown.onValueChanged.AddListener(OnDropDownValueChanged);
        EventManager.StartListening(ConstEvent.OnPopulaitionChange, populationChange);
    }

    private void SetBtnsActive(bool isActive)
    {
        for (int i = 0; i < _populationBtns.Length; i++)
        {
            _populationBtns[i].gameObject.SetActive(isActive);
        }
    }
    private void AddBtnsListener(BuildingBase buildingBase)
    {
        _populationBtns[0].onClick.AddListener(()=>buildingBase.DeleteCurPeople(10));
        _populationBtns[1].onClick.AddListener(()=>buildingBase.DeleteCurPeople(1));
        _populationBtns[2].onClick.AddListener(()=>buildingBase.AddCurPeople(1));
        _populationBtns[3].onClick.AddListener(()=>buildingBase.AddCurPeople(10));
        _destroyBtn.onClick.AddListener(() => { buildingBase.DestroyBuilding();OnClose(); });
        _upgradeBtn.onClick.AddListener(() => { buildingBase.Upgrade(); OnClose(); });
    }

    private void RemoveBtnsListener()
    {
        _populationBtns[0].onClick.RemoveAllListeners();
        _populationBtns[1].onClick.RemoveAllListeners();
        _populationBtns[2].onClick.RemoveAllListeners();
        _populationBtns[3].onClick.RemoveAllListeners();
        _destroyBtn.onClick.RemoveAllListeners();
        _upgradeBtn.onClick.RemoveAllListeners();
    }

    public override void OnClose()
    {
        RemoveBtnsListener();
        EventManager.StopListening<RuntimeBuildData>(ConstEvent.OnPopulaitionChange, ChangeLabels);
        mainCanvas.SetActive(false);
    }
    private void OnDestroy()
    {
        EventManager.StopListening(ConstEvent.OnPopulaitionChange, populationChange);
        EventManager.StopListening<BuildingBase>(ConstEvent.OnTriggerInfoPanel, OnOpen);
    }

    void OnDropDownValueChanged(int n)
    {
        _buildData.CurFormula = n;
        ChangeLabels(_buildData);
    }
    /// <summary>
    /// 修改显示的条目
    /// </summary>
    /// <param name="buildData"></param>
    private void ChangeShowItems(RuntimeBuildData buildData)
    {
        switch (buildData.tabType)
        {
            case BuildTabType.produce:
            case BuildTabType.road:
            case BuildTabType.manufacturing:
                {
                    _inputsObj.SetActive(true);
                    _outputsObj.SetActive(true);
                    _effectiveObj.SetActive(true);
                    _introduceObj.SetActive(true);
                    _populationObj.SetActive(true);
                    _destroyBtn.gameObject.SetActive(true);
                    _formulaObj.SetActive(true);
                    SetBtnsActive(true);
                    _upgradeBtn.gameObject.SetActive(buildData.CurLevel < 3);
                    break;

                }
            case BuildTabType.utility:
            case BuildTabType.house:
                {
                    _inputsObj.SetActive(true);
                    _outputsObj.SetActive(false);
                    _effectiveObj.SetActive(false);
                    _introduceObj.SetActive(true);
                    _populationObj.SetActive(true);
                    SetBtnsActive(false);
                    _destroyBtn.gameObject.SetActive(true);
                    _formulaObj.SetActive(false);
                    _upgradeBtn.gameObject.SetActive(buildData.CurLevel < 3);
                    break;
                }
            case BuildTabType.hide:
                {
                    _inputsObj.SetActive(false);
                    _outputsObj.SetActive(false);
                    _effectiveObj.SetActive(false);
                    _introduceObj.SetActive(true);
                    _populationObj.SetActive(false);
                    SetBtnsActive(false);
                    _destroyBtn.gameObject.SetActive(false);
                    _formulaObj.SetActive(false);
                    _upgradeBtn.gameObject.SetActive(buildData.CurLevel < 3);
                    break;
                }

        }
    }


    private void ChangeFormula(FormulaData[] formulaDatas)
    {
        if (formulaDatas.Length <= 0) return;
        _dropDown.options.Clear();
        _dropDown.captionText.text = formulaDatas[_buildData.CurFormula].Describe;
        if (formulaDatas == null || formulaDatas.Length == 0)
        {
            _formulaObj.SetActive(false);
            return;
        }
        TMP_Dropdown.OptionData optionData;
        for (int i = 0; i < formulaDatas.Length; i++)
        {
            optionData = new TMP_Dropdown.OptionData();
            optionData.text = formulaDatas[i].Describe;
            _dropDown.options.Add(optionData);
        }
    }
    /// <summary>
    /// 修改显示的文本
    /// </summary>
    /// <param name="buildData"></param>
    private void ChangeLabels(RuntimeBuildData buildData)
    {
        _nameLabel.text = Localization.ToSettingLanguage(buildData.Name);
        FormulaData formula;
        if (buildData.formulaDatas.Length>0)
        {
            formula = buildData.formulaDatas[buildData.CurFormula];
            string input = string.Empty;
            int count = 0;
            for (int i = 0; i < formula.InputItemID.Count; i++)
            {
                //如果是民房
                if (formula.InputNum.Count - 1 < i)
                {
                    input += " 或 " +
                    Localization.ToSettingLanguage(
                        DataManager.GetItemNameById(formula.InputItemID[i]));
                    count++;
                }
                else
                if (formula.InputNum[i] != 0)
                {
                    input += formula.InputNum[i] + " " +
                    Localization.ToSettingLanguage(
                        DataManager.GetItemNameById(formula.InputItemID[i]));
                    count++;
                }
            }
            if (count == 0)
            {
                _inputsObj.SetActive(false);
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
        }
        _populationLabel.text = Localization.ToSettingLanguage(buildData.Population > 0 ? "Worker" : "Resident") + "：" +
            buildData.CurPeople + "/" + Mathf.Abs(buildData.Population);
        _introduceLabel.text = Localization.ToSettingLanguage(buildData.Introduce);
    }
}
