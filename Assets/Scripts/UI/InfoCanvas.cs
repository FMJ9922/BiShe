﻿using System.Collections;
using System.Collections.Generic;
using Building;
using CSTools;
using UnityEngine;
using TMPro;
using UI;
using UnityEngine.UI;
using UnityEngine.Events;
public class InfoCanvas : CanvasBase
{
    [Header("建筑功能")]
    [SerializeField] GameObject _inputsObj;//挂载输入资源说明的UI
    [SerializeField] GameObject _outputsObj;//输出
    [SerializeField] GameObject _rateObj;//产量
    [SerializeField] GameObject _workerObj;//工人人口
    [SerializeField] GameObject _populationObj;//居民人口
    [SerializeField] GameObject _openCanvas;//开启功能页签
    [SerializeField] Button _destroyBtn;//拆除建筑
    [SerializeField] Button _upgradeBtn;//升级
    [SerializeField] Button[] _populationBtns;//人口相关按钮
    [SerializeField] TMP_Text _nameLabel, _introduceLabel, _rateLabel, _populationLabel, _workerLabel;
    [SerializeField] TMP_Text _openText;//功能
    [SerializeField] TMP_Text _outputLabel;//功能
    [SerializeField] TMP_Text _daysLabel;//生产周期
    [SerializeField] Button[] _switchFormulaBtns;//切换产品按钮
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private Transform inIcons;
    [SerializeField] private Transform outIcons;
    [SerializeField] private Image rateImage;
    [SerializeField] private Transform upgradeCost;
    [SerializeField] private Image[] warnings;
    UnityAction populationChange;
    UnityAction<string> effectivenessChange;
    private RuntimeBuildData _buildData;
    private BuildingBase _buildingBase;

    public override void InitCanvas()
    {
        mainCanvas.SetActive(false);
    }
    public override void OnOpen()
    {
    }
    public void OnOpen(BuildingBase buildbase)
    {
        _buildingBase = buildbase;
        _buildData = buildbase.runtimeBuildData;
        populationChange = () => ChangeLabels(_buildData);
        effectivenessChange = (string str) => ChangeRateLabel(buildbase);
        ChangeShowItems(_buildData);
        ChangeLabels(_buildData);
        AddBtnsListener(buildbase);
        ChangeOpenCanvas(buildbase.runtimeBuildData.Id);
        mainCanvas.SetActive(true);
        EventManager.StartListening(ConstEvent.OnPopulationHudChange, populationChange);
        EventManager.StartListening<string>(ConstEvent.OnDayWentBy, effectivenessChange);
        EventManager.TriggerEvent(ConstEvent.OnSelectLightOpen, new SelectLightInfo
        {
            pos = buildbase.transform.position,
            quaternion = buildbase.transform.rotation,
            scale = new Vector3(buildbase.Size.y * 2, 10, buildbase.Size.x * 2),
        });
        ChangeRateLabel(buildbase);
    }

    /*private void SetBtnsActive(bool isActive)
    {
        for (int i = 0; i < _populationBtns.Length; i++)
        {
            _populationBtns[i].gameObject.SetActive(isActive);
        }
    }*/
    private void ChangeOpenCanvas(int id)
    {
        Button btn = _openCanvas.GetComponentInChildren<Button>();
        switch (id)
        {
            //仓库
            case 20003:
            case 20016:
            case 20017:
                {
                    //btn.onClick.AddListener(MainInteractCanvas.Instance.OpenResourceCanvas);
                    _openText.text = Localization.Get("Open") + Localization.Get("Warehouse");
                    SoundManager.Instance.PlaySoundEffect(SoundResource.sfx_click_wareHouse);
                }
                break;
            //集市
            case 20004:
            case 20012:
            case 20013:
                _openCanvas.GetComponentInChildren<Button>().image.sprite = LoadAB.LoadSprite("mat.ab", "MarketButton");
                btn.onClick.AddListener(MainInteractCanvas.Instance.OpenMarketCanvas);
                _openCanvas.GetComponentInChildren<Button>().image.SetNativeSize();
                //_openText.text = Localization.ToSettingLanguage("Open") + Localization.ToSettingLanguage("Market");
                SoundManager.Instance.PlaySoundEffect(SoundResource.sfx_click_market);
                EventManager.TriggerEvent(ConstEvent.OnRangeLightOpen, new SelectLightInfo
                {
                    pos = _buildingBase.transform.position,
                    quaternion = _buildingBase.transform.rotation,
                    scale = new Vector3(_buildData.InfluenceRange, 10, _buildData.InfluenceRange),
                });
                break;
            //市政
            case 20020:
            case 20021:
            case 20022:
                _openCanvas.GetComponentInChildren<Button>().image.sprite = LoadAB.LoadSprite("mat.ab", "ScienceButton");
                _openCanvas.GetComponentInChildren<Button>().image.SetNativeSize();
                btn.onClick.AddListener(MainInteractCanvas.Instance.OpenTechCanvas);
                //_openText.text = Localization.ToSettingLanguage("Open") + Localization.ToSettingLanguage("TechCanvas");
                break;
            case 20001:
            case 20009:
            case 20033:

            case 20023:
            case 20024:
            case 20025:

            case 20026:
            case 20027:
            case 20028:
                SoundManager.Instance.PlaySoundEffect(SoundResource.sfx_click_hut);
                break;
            case 20008:
            case 20014:
            case 20015:
                SoundManager.Instance.PlaySoundEffect(SoundResource.sfx_click_logCamp);
                EventManager.TriggerEvent(ConstEvent.OnRangeLightOpen, new SelectLightInfo
                {
                    pos = _buildingBase.transform.position,
                    quaternion = _buildingBase.transform.rotation,
                    scale = new Vector3(_buildData.InfluenceRange, 10, _buildData.InfluenceRange),
                });
                break;
            case 20007:
            case 20010:
            case 20011:
                ChangeWarning();
                SoundManager.Instance.PlaySoundEffect(SoundResource.sfx_click_factory);
                break;
            case 20029:
            case 20030:
            case 20031:
                SoundManager.Instance.PlaySoundEffect(SoundResource.sfx_click_mine);
                break;
            case 20032:
                SoundManager.Instance.PlaySoundEffect(SoundResource.sfx_click_pier);
                break;
            case 20005:
                SoundManager.Instance.PlaySoundEffect(SoundResource.sfx_click_farmland);
                break;
            case 20006:
                SoundManager.Instance.PlaySoundEffect(SoundResource.sfx_click_farm);
                break;

        }
    }

    private void AddBtnsListener(BuildingBase buildingBase)
    {
        RemoveBtnsListener();
        if (buildingBase is IProduct productBuilding)
        {
            _populationBtns[0].onClick.AddListener(() =>
            {
                BuildingTools.ChangeBuildingWorker(buildingBase.runtimeBuildData, -100);
            });
            _populationBtns[1].onClick.AddListener(() =>
            {
                BuildingTools.ChangeBuildingWorker(buildingBase.runtimeBuildData, -1);
            });
            _populationBtns[2].onClick.AddListener(() =>
            {
                BuildingTools.ChangeBuildingWorker(buildingBase.runtimeBuildData, 1);
            });
            _populationBtns[3].onClick.AddListener(() =>
            {
                BuildingTools.ChangeBuildingWorker(buildingBase.runtimeBuildData, 100);
            }); 
        }

        if (buildingBase is IBuildingBasic building)
        {
            _destroyBtn.onClick.AddListener(() =>
            {
                building.DestroyBuilding(true, true, true);
                OnClose();
            });
            _upgradeBtn.onClick.AddListener(() =>
            {
                building.Upgrade(out bool success, out BuildingBase NewBase);
                if (success)
                {
                    OnClose();
                    buildingBase = NewBase;
                    OnOpen(buildingBase);
                }
            });
        }
    }

    IEnumerator DelayOpen(BuildingBase buildingBase)
    {
        yield return new WaitForFixedUpdate();
        OnOpen(buildingBase);
    }

    public void ShowUpgradeInfo()
    {
        upgradeCost.gameObject.SetActive(true);
        CleanUpAllAttachedChildren(upgradeCost);
        List<CostResource> costResources = new List<CostResource>();
        BuildData next = DataManager.GetBuildData(_buildData.RearBuildingId);
        costResources.Add(new CostResource(99999, (next.Price) * TechManager.Instance.BuildPriceBuff()));
        for (int i = 0; i < next.costResources.Count; i++)
        {
            CostResource res = next.costResources[i] * TechManager.Instance.BuildResourcesBuff();
            costResources.Add(res);
        }
        for (int i = 0; i < costResources.Count; i++)
        {
            GameObject icon = CommonIcon.GetIcon(costResources[i]);
            icon.transform.SetParent(upgradeCost);
            icon.transform.localScale = ((GameManager.Instance.GetScreenRelativeRate() - 1F) * 0F + 1F) * Vector3.one;
        }
    }
    public void CloseUpgradeInfo()
    {
        upgradeCost.gameObject.SetActive(false);
    }
    private void RemoveBtnsListener()
    {
        _populationBtns[0].onClick.RemoveAllListeners();
        _populationBtns[1].onClick.RemoveAllListeners();
        _populationBtns[2].onClick.RemoveAllListeners();
        _populationBtns[3].onClick.RemoveAllListeners();
        _destroyBtn.onClick.RemoveAllListeners();
        _upgradeBtn.onClick.RemoveAllListeners();
        _openCanvas.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
    }

    public override void OnClose()
    {
        RemoveBtnsListener();
        mainCanvas.SetActive(false);
        CloseUpgradeInfo();
        EventManager.TriggerEvent(ConstEvent.OnSelectLightClose);
        EventManager.TriggerEvent(ConstEvent.OnRangeLightClose);
        NoticeManager.Instance.CloseNotice();
    }
    private void OnDestroy()
    {
        EventManager.StopListening(ConstEvent.OnDayWentBy, effectivenessChange);
        EventManager.StopListening(ConstEvent.OnPopulationHudChange, populationChange);
    }

    public void OnDropDownValueChanged(int n)
    {
        if (_buildingBase is IProduct p)
        {
            //Debug.Log(n);
            _buildData.CurFormula += n + _buildData.formulaDatas.Length;
            _buildData.CurFormula %= _buildData.formulaDatas.Length;
            //Debug.Log(_buildData.CurFormula);
            CleanUpAllAttachedChildren(inIcons);
            CleanUpAllAttachedChildren(outIcons);
            ChangeOutputIcon(_buildData);
            ChangeInputIcon(_buildData);
            p.ChangeFormula();
            _daysLabel.text = Localization.Get("生产周期:") + _buildData.formulaDatas[_buildData.CurFormula].ProductTime * 7 + Localization.Get("Day1");

        }
    }
    /// <summary>
    /// 修改显示的条目
    /// </summary>
    /// <param name="buildData"></param>
    private void ChangeShowItems(RuntimeBuildData buildData)
    {
        CleanUpAllAttachedChildren(inIcons);
        CleanUpAllAttachedChildren(outIcons);
        switch (buildData.tabType)
        {
            case BuildTabType.produce:
            case BuildTabType.manufacturing:
                {
                    if (buildData.formulaDatas[buildData.CurFormula].InputItemID.Count > 0)
                    {
                        _inputsObj.SetActive(true);
                        ChangeInputIcon(buildData);
                    }
                    _daysLabel.text = Localization.Get("生产周期:") + buildData.formulaDatas[buildData.CurFormula].ProductTime * 7 + Localization.Get("Day1"); ;
                    _openCanvas.SetActive(false);
                    _outputsObj.SetActive(true);
                    ChangeOutputIcon(buildData);
                    _rateObj.SetActive(true);
                    _workerObj.SetActive(true);
                    _populationObj.SetActive(false);
                    _destroyBtn.gameObject.SetActive(true);
                    _upgradeBtn.gameObject.SetActive(buildData.RearBuildingId != 0);
                    ChangeWarning();
                    break;

                }
            case BuildTabType.utility:
                {
                    _openCanvas.SetActive(true);
                    _inputsObj.SetActive(false);
                    _outputsObj.SetActive(false);
                    _rateObj.SetActive(false);
                    _workerObj.SetActive(false);
                    _populationObj.SetActive(true);
                    _destroyBtn.gameObject.SetActive(true);
                    _upgradeBtn.gameObject.SetActive(buildData.RearBuildingId != 0);
                    CloseAllWarning();
                    break;
                }
            case BuildTabType.house:
                {
                    _openCanvas.SetActive(false);
                    _inputsObj.SetActive(true);
                    ChangeInputIcon(buildData);
                    _outputsObj.SetActive(false);
                    _rateObj.SetActive(false);
                    _workerObj.SetActive(false);
                    _populationObj.SetActive(true);
                    _destroyBtn.gameObject.SetActive(true);
                    _upgradeBtn.gameObject.SetActive(buildData.RearBuildingId != 0);
                    ChangeWarning();
                    break;
                }
            case BuildTabType.hide:
                {
                    _openCanvas.SetActive(true);
                    _inputsObj.SetActive(false);
                    _outputsObj.SetActive(false);
                    _rateObj.SetActive(false);
                    _workerObj.SetActive(false);
                    _populationObj.SetActive(false);
                    _destroyBtn.gameObject.SetActive(false);
                    _upgradeBtn.gameObject.SetActive(buildData.RearBuildingId != 0);
                    CloseAllWarning();
                    break;
                }
            case BuildTabType.bridge:
                {
                    _openCanvas.SetActive(false);
                    _inputsObj.SetActive(false);
                    _outputsObj.SetActive(false);
                    _rateObj.SetActive(false);
                    _workerObj.SetActive(false);
                    _populationObj.SetActive(false);
                    _destroyBtn.gameObject.SetActive(true);
                    _upgradeBtn.gameObject.SetActive(false);
                    CloseAllWarning();
                    break;
                }

        }
    }

    public void ChangeWarning()
    {
        List<WarningType> warns = BuildingTools.GetWarnings(_buildingBase.runtimeBuildData);
        for (int i = 0; i < warnings.Length; i++)
        {
            bool open = false;
            for (int j = 0; j < warns.Count; j++)
            {
                if ((int)warns[j] == i)
                {
                    open = true;
                }
            }
            warnings[i].gameObject.SetActive(open);
        }

    }

    public void CloseAllWarning()
    {
        for (int i = 0; i < warnings.Length; i++)
        {
            bool open = false;
            warnings[i].gameObject.SetActive(open);
        }
    }
    private void ChangeOutputIcon(RuntimeBuildData data)
    {
        List<int> list = data.formulaDatas[data.CurFormula].OutputItemID;
        if (list.Count <= 0) return;
        for (int i = 0; i < list.Count; i++)
        {
            GameObject resource = CommonIcon.GetIcon(data.formulaDatas[data.CurFormula].OutputItemID[i],
                                                     data.formulaDatas[data.CurFormula].ProductNum[i] * data.Times);
            resource.transform.parent = outIcons;
            resource.transform.localScale = Vector3.one * 1.5f;
            if (list[i] == 11000)
            {
                return;
            }
        }
    }

    private void ChangeInputIcon(RuntimeBuildData data)
    {
        List<int> list = data.formulaDatas[data.CurFormula].InputItemID;
        if (list.Count <= 0) return;
        for (int i = 0; i < list.Count; i++)
        {
            FormulaData cur = data.formulaDatas[data.CurFormula];
            GameObject resource = CommonIcon.GetIcon(cur.InputItemID[i], cur.InputNum[i] * data.Times * cur.ProductTime*TechManager.Instance.ResourcesBuff());
            resource.transform.parent = inIcons;
            resource.transform.localScale = Vector3.one * 1.5f;
        }
        _daysLabel.text = Localization.Get("生产周期:") + data.formulaDatas[data.CurFormula].ProductTime * 7 + "天";
    }
    /// <summary>
    /// 修改显示的文本
    /// </summary>
    /// <param name="buildData"></param>
    private void ChangeLabels(RuntimeBuildData buildData)
    {
        _nameLabel.text = Localization.Get(buildData.Name);
        _populationLabel.text = Mathf.Abs(buildData.CurPeople) + "/" + Mathf.Abs(buildData.Population);
        _workerLabel.text = buildData.CurPeople + "/" + Mathf.Abs(buildData.Population + TechManager.Instance.PopulationBuff());
        _introduceLabel.text = Localization.Get(buildData.Introduce);

    }

    private void ChangeRateLabel(BuildingBase buildData)
    {
        if (_rateLabel.gameObject.activeInHierarchy)
        {
            ChangeWarning();
        }

        if (buildData is IProduct p)
        {
            RuntimeBuildData data = buildData.runtimeBuildData;
            _rateLabel.text = Localization.Get("效率:") + CastTool.RoundOrFloat(data.Effectiveness * 100) + "%";
            rateImage.fillAmount = p.GetProcess();

            _outputLabel.text = Localization.Get("产出率:") + CastTool.RoundOrFloat(data.Rate * 100) + "%";
        }
    }
}
