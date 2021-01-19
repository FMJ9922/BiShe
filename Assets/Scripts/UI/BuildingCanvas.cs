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
    private TMP_Text _costLabel;

    private BuildTabType tabType;
    #endregion

    #region 预制体
    [SerializeField]
    private GameObject pfbTab;
    [SerializeField]
    private GameObject pfbIcon;
    #endregion

    #region 数据
    //当前页签的数据
    private BuildData[] currentTabDatas;
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
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown, OnClose);
    }

    public override void OnClose()
    {
        _mainCanvas.SetActive(false);
        EventManager.StopListening(ConstEvent.OnMouseRightButtonDown, OnClose);
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
            newTab.GetComponentInChildren<TMP_Text>().text = Localization.ToSettingLanguage(((BuildTabType)i).GetDescription());
            newTab.GetComponent<Button>().interactable = true;
            newTab.GetComponent<Button>().onClick.AddListener(() =>
            {
                ChangeTab(int.Parse(newTab.name));
            });
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

   
    public void ChangeTab(int tabType)
    {
        this.tabType = (BuildTabType)tabType;
        //TODO:替换背景图片
        currentTabDatas = DataManager.Instance.TabDic[this.tabType].ToArray();
        CleanUpAllAttachedChildren(_buildingIcons);
        for (int i = 0; i < currentTabDatas.Length; i++)
        {
            GameObject newIcon = Instantiate(pfbIcon, _buildingIcons);
            newIcon.GetComponent<BuildIcon>().Init(currentTabDatas[i], this);
        }

    }

    public void OnClickIconToBuild(BuildData buildData)
    {
        HideOrShowCanvasToggle(false);
        _InfoCanvas.SetActive(false);
        EventManager.StopListening(ConstEvent.OnMouseRightButtonDown, OnClose);
        EventManager.StartListening(ConstEvent.OnFinishBuilding, this.OnFinishBuilding);
        BuildManager.Instance.CreateBuildingOnMouse(buildData.BundleName, buildData.PfbName);
    }

    public void OnFinishBuilding()
    {
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown, OnClose);
        EventManager.StopListening(ConstEvent.OnFinishBuilding, this.OnFinishBuilding);
        HideOrShowCanvasToggle(true);
    }
    public void OnEnterHoverIcon(BuildData buildData)
    {
        _nameLabel.text = Localization.ToSettingLanguage(buildData.Name);
        string cost = buildData.Price + Localization.ToSettingLanguage("Gold");
        for (int i = 0; i < buildData.costResources.Count; i++)
        {
            cost += "，"+buildData.costResources[i].ItemNum +" "+ 
                Localization.ToSettingLanguage(
                    DataManager.GetItemNameById(buildData.costResources[i].ItemId));
        }
        _costLabel.text = string.Format(Localization.ToSettingLanguage("Cost")+":\n{0}", cost);
        _introduceLabel.text = Localization.ToSettingLanguage("TempIntroduction");
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