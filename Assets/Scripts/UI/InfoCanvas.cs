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
    [SerializeField] Button[] _switchFormulaBtns;//切换产品按钮
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private Transform inIcons;
    [SerializeField] private Transform outIcons;
    [SerializeField] private GameObject BuildingHighlight;
    [SerializeField] private Image rateImage;
    UnityAction populationChange;
    UnityAction<string> effectivenessChange;
    private RuntimeBuildData _buildData;
    public override void InitCanvas()
    {
        mainCanvas.SetActive(false);
        EventManager.StartListening<BuildingBase>(ConstEvent.OnTriggerInfoPanel, OnOpen);
    }
    public override void OnOpen()
    {

    }
    public void OnOpen(BuildingBase buildbase)
    {
        _buildData = buildbase.runtimeBuildData;
        populationChange = () => ChangeLabels(_buildData);
        effectivenessChange = (string str) => ChangeRateLabel(buildbase);
        ChangeShowItems(_buildData);
        ChangeLabels(_buildData);
        AddBtnsListener(buildbase);
        ChangeOpenCanvas(buildbase.runtimeBuildData.Id);
        mainCanvas.SetActive(true);
        EventManager.StartListening(ConstEvent.OnPopulaitionChange, populationChange);
        EventManager.StartListening<string>(ConstEvent.OnDayWentBy, effectivenessChange);
        if (!BuildingHighlight)
        {
            BuildingHighlight = GameObject.Find("SelectedLight");
        }
        BuildingHighlight.transform.position = buildbase.transform.position;
        BuildingHighlight.transform.localScale = new Vector3(buildbase.Size.y * 2, 10, buildbase.Size.x * 2);
        BuildingHighlight.transform.rotation = buildbase.transform.rotation;
        BuildingHighlight.SetActive(true);
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
                    btn.onClick.AddListener(MainInteractCanvas.Instance.OpenResourceCanvas);
                    _openText.text = Localization.ToSettingLanguage("Open") + Localization.ToSettingLanguage("Warehouse");
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
                break;
            case 20007:
            case 20010:
            case 20011:
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
        _populationBtns[0].onClick.AddListener(() => 
        { 
            buildingBase.DeleteCurPeople(10); 
        });
        _populationBtns[1].onClick.AddListener(() =>
        {
            buildingBase.DeleteCurPeople(1);
        });
        _populationBtns[2].onClick.AddListener(() => 
        { 
            buildingBase.AddCurPeople(1); 
        });
        _populationBtns[3].onClick.AddListener(() => 
        {
            buildingBase.AddCurPeople(10); 
        });
        _destroyBtn.onClick.AddListener(() => { buildingBase.DestroyBuilding(); OnClose(); });
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
        _openCanvas.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
    }

    public override void OnClose()
    {
        RemoveBtnsListener();
        EventManager.StopListening(ConstEvent.OnPopulaitionChange, populationChange);
        EventManager.StopListening(ConstEvent.OnDayWentBy, effectivenessChange);
        mainCanvas.SetActive(false);
        BuildingHighlight.SetActive(false);
        NoticeManager.Instance.CloseNotice();
    }
    private void OnDestroy()
    {
        EventManager.StopListening(ConstEvent.OnPopulaitionChange, populationChange);
        EventManager.StopListening<BuildingBase>(ConstEvent.OnTriggerInfoPanel, OnOpen);
    }

    public void OnDropDownValueChanged(int n)
    {
        //Debug.Log(n);
        _buildData.CurFormula += n + _buildData.formulaDatas.Length;
        _buildData.CurFormula %= _buildData.formulaDatas.Length;
        CleanUpAllAttachedChildren(inIcons);
        CleanUpAllAttachedChildren(outIcons);
        ChangeOutputIcon(_buildData);
        ChangeInputIcon(_buildData);
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
                    _openCanvas.SetActive(false);
                    _outputsObj.SetActive(true);
                    ChangeOutputIcon(buildData);
                    _rateObj.SetActive(true);
                    _workerObj.SetActive(true);
                    _populationObj.SetActive(false);
                    _destroyBtn.gameObject.SetActive(true);
                    _upgradeBtn.gameObject.SetActive(buildData.RearBuildingId != 0);
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
                    break;
                }

        }
    }


    private void ChangeOutputIcon(RuntimeBuildData data)
    {
        List<int> list = data.formulaDatas[data.CurFormula].OutputItemID;
        if (list.Count <= 0) return;
        for (int i = 0; i < list.Count; i++)
        {
            GameObject resource = CommonIcon.GetIcon(data.formulaDatas[data.CurFormula].OutputItemID[i],
                                                     data.formulaDatas[data.CurFormula].ProductNum[i]);
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
            GameObject resource = CommonIcon.GetIcon(data.formulaDatas[data.CurFormula].InputItemID[i],
                                                     data.formulaDatas[data.CurFormula].InputNum[i]);
            resource.transform.parent = inIcons;
            resource.transform.localScale = Vector3.one * 1.5f;
        }
    }
    /// <summary>
    /// 修改显示的文本
    /// </summary>
    /// <param name="buildData"></param>
    private void ChangeLabels(RuntimeBuildData buildData)
    {
        _nameLabel.text = Localization.ToSettingLanguage(buildData.Name);
        _populationLabel.text = buildData.CurPeople + "/" + Mathf.Abs(buildData.Population);
        _workerLabel.text = buildData.CurPeople + "/" + Mathf.Abs(buildData.Population + TechManager.Instance.PopulationBuff());
        _introduceLabel.text = Localization.ToSettingLanguage(buildData.Introduce);
        _rateLabel.text = "效率:" + buildData.Effectiveness * 100 + "%";
    }

    private void ChangeRateLabel(BuildingBase buildData)
    {
        //Debug.Log(buildData.Effectiveness+" " +buildData.Pause);
        RuntimeBuildData data = buildData.runtimeBuildData;
        _rateLabel.text = "效率:" + CastTool.RoundOrFloat(data.Effectiveness * 100) + "%";
        rateImage.fillAmount = buildData.GetProcess();
        
        _outputLabel.text = "产出率:" + CastTool.RoundOrFloat(data.Rate * 100) + "%";
    }

    private void CleanUpAllAttachedChildren(Transform target)
    {
        for (int i = 0; i < target.childCount; i++)
        {
            Destroy(target.GetChild(i).gameObject);
        }

    }
}
