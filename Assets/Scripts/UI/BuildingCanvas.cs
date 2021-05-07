using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.ComponentModel;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

public class BuildingCanvas : CanvasBase
{
    #region 组件&成员
    [SerializeField]
    private GameObject _mainCanvas;
    [SerializeField]
    private GameObject _activeBtns;
    [SerializeField]
    private Transform _buildingTabs;
    [SerializeField]
    private Transform _buildingIcons;
    [SerializeField]
    private GameObject _InfoCanvas;
    [SerializeField]
    private TMP_Text _nameLabel;
    [SerializeField]
    private TMP_Text _introduceLabel;
    [SerializeField]
    private GameObject _confirmBtns;
    [SerializeField]
    private Button _confirm;
    [SerializeField]
    private Button _cancel;
    [SerializeField] 
    private Transform _iconsParent;
    [SerializeField]
    private TMP_Text _tabLabel;
    private BuildTabType tabType;

    
    #endregion

    #region 预制体
    [SerializeField]
    private GameObject pfbTab;
    [SerializeField]
    private GameObject pfbIcon;
    [SerializeField]
    private GameObject pfbDividingLine;
    #endregion

    #region 数据
    //当前页签的数据
    private BuildData[] currentTabDatas;
    private BuildTabAnim curTab =null;
    #endregion

    #region 实现基类

    public override void InitCanvas()
    {
        InitTabs();
        ChangeTab(0);
        _InfoCanvas.SetActive(false);
    }

    public override void OnOpen()
    {
        _mainCanvas.SetActive(true);
    }

    public override void OnClose()
    {
        _mainCanvas.SetActive(false);
    }
    #endregion

    #region 私有函数

    /// <summary>
    /// 控制建造面板显隐
    /// </summary>
    private void HideOrShowCanvasToggle(bool isShow)
    {
        _mainCanvas.SetActive(isShow);
        //_activeBtns.SetActive(isShow);
    }

    /// <summary>
    /// 创建页签
    /// </summary>
    private void InitTabs()
    {
        DataManager.Instance.InitTabDic();
        CleanUpAllAttachedChildren(_buildingTabs);
        for (int i = 0; i < 5; i++)
        {
            GameObject newTab = Instantiate(pfbTab, _buildingTabs);
            newTab.name = i.ToString();
            Button btn = newTab.GetComponent<Button>();
            btn.interactable = true;
            newTab.GetComponent<BuildTabAnim>().iconName = ((BuildTabType)i).GetDescription();
            newTab.GetComponent<BuildTabAnim>().InitSprite();
            btn.onClick.AddListener(() =>
            {
                ChangeTab(int.Parse(newTab.name));
                newTab.GetComponent<BuildTabAnim>().Rise();
                curTab.Hide();
                curTab = newTab.GetComponent<BuildTabAnim>();
            });
            if (i == 0)
            {
                newTab.GetComponent<BuildTabAnim>().Rise();
                curTab = newTab.GetComponent<BuildTabAnim>();
                _tabLabel.text = Localization.ToSettingLanguage(((BuildTabType)i).GetDescription());
            }
        }
    }


    /// <summary>
    /// 清除一个物体下的所有子物体
    /// </summary>
    /// <param name="target"></param>
    private void CleanUpAllAttachedChildren(Transform target)
    {
        for (int i = 0; i < target.childCount; i++)
        {
            Destroy(target.GetChild(i).gameObject);
        }

    }

    #endregion

    #region 公共函数

    public bool ToggleBuildingCanvas()
    {
        if (_mainCanvas.activeInHierarchy)
        {
            OnClose();
            return false;
        }
        else
        {
            OnOpen();
            return true;
        }
    }
   
    public void ChangeTab(int tabType)
    {
        this.tabType = (BuildTabType)tabType;
        currentTabDatas = DataManager.Instance.TabDic[this.tabType].ToArray();
        CleanUpAllAttachedChildren(_buildingIcons);
        for (int i = 0; i < currentTabDatas.Length; i++)
        {
            int level= currentTabDatas[i].Level;
            if (level <= 1||tabType==0)
            {
                GameObject newDivide = Instantiate(pfbDividingLine, _buildingIcons);
                GameObject newIcon = Instantiate(pfbIcon, _buildingIcons);
                newIcon.GetComponent<BuildIcon>().Init(currentTabDatas[i], this);
            }
        }
        GameObject newDivide1 = Instantiate(pfbDividingLine, _buildingIcons);
        _tabLabel.text = Localization.ToSettingLanguage(((BuildTabType)tabType).GetDescription());
    }
    public void ShowConfirmButtons(Vector2 vector2)
    {
        _confirmBtns.SetActive(true);
        _confirmBtns.transform.position = vector2;
    }


    public void OnClickIconToBuild(BuildData buildData)
    {
        HideOrShowCanvasToggle(false);
        _InfoCanvas.SetActive(false);
        EventManager.StopListening(ConstEvent.OnMouseRightButtonDown, OnClose);
        EventManager.StartListening(ConstEvent.OnFinishBuilding, this.OnFinishBuilding);
        if (buildData.tabType != BuildTabType.road)
        {
            BuildManager.Instance.CreateBuildingOnMouse(buildData);
        }
        else
        {
            EventManager.StartListening<Vector2>(ConstEvent.OnBuildToBeConfirmed, OnBuildToBeConfirmed);
            BuildManager.Instance.StartCreateRoads(buildData.Level);
        }
        
    }
    public void OnBuildToBeConfirmed(Vector2 vector2)
    {
        ShowConfirmButtons(vector2);
        EventManager.StopListening<Vector2>(ConstEvent.OnBuildToBeConfirmed, OnBuildToBeConfirmed);
    }

    public void OnCancel()
    {
        BuildManager.Instance.OnCancelBuildRoad();
        _confirmBtns.SetActive(false);
    }

    public void OnConfirm()
    {
        BuildManager.Instance.OnConfirmBuildRoad();
        _confirmBtns.SetActive(false);
    }
    public void OnFinishBuilding()
    {
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown, OnClose);
        EventManager.StopListening(ConstEvent.OnFinishBuilding, this.OnFinishBuilding);
        HideOrShowCanvasToggle(true);
    }
    public void OnEnterHoverIcon(BuildData buildData,Vector3 adjustPosition)
    {
        _nameLabel.text = Localization.ToSettingLanguage(buildData.Name);
        CleanUpAllAttachedChildren(_iconsParent);
        GameObject money = CommonIcon.GetIcon(99999, buildData.Price);
        money.transform.parent = _iconsParent;
        for (int i = 0; i < buildData.costResources.Count; i++)
        {
            if (buildData.costResources[i].ItemId == 99999) continue;
            GameObject resource = CommonIcon.GetIcon(buildData.costResources[i].ItemId, buildData.costResources[i].ItemNum);
            resource.transform.parent = _iconsParent;
        }
        _InfoCanvas.transform.position = adjustPosition + new Vector3(230,240,0);
        _introduceLabel.text = Localization.ToSettingLanguage(buildData.Introduce);
        _InfoCanvas.SetActive(true);
    }


    public void OnExitHoverIcon(BuildData buildData)
    {
        _InfoCanvas.SetActive(false);
    }
    #endregion
}
public static class EnumExtensionMethods
{
    /// <summary>
    /// 获取枚举类型的描述信息
    /// </summary>
    public static string GetDescription(this Enum value)
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);

        if (name == null) return null;

        var field = type.GetField(name);

        if (!(Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute))
        {
            return name;
        }

        return attribute.Description;
    }
}