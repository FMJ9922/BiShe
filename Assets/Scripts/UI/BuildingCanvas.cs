using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.ComponentModel;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

public class BuildingCanvas : MonoBehaviour
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

    #region 初始化
    private void Start()
    {
        InitTabs();
        ChangeTab(0);
    }

    #endregion

    #region 私有函数

    /// <summary>
    /// 控制建造面板显隐
    /// </summary>
    private void HideOrShowCanvasToggle(bool isShow)
    {
        _mainCanvas.SetActive(isShow);
        _activeBtns.SetActive(isShow);
    }

    /// <summary>
    /// 创建页签
    /// </summary>
    private void InitTabs()
    {
        DataManager.Instance.InitTabDic();
        CleanUpAllAttachedChildren(_buildingTabs);
        for (int i = 0; i < 6; i++)
        {
            GameObject newTab = Instantiate(pfbTab, _buildingTabs);
            newTab.name = i.ToString();
            newTab.GetComponentInChildren<TMP_Text>().text = ((BuildTabType)i).GetDescription();
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

    /// <summary>
    /// 建造建筑按钮激活时，打开建造面板
    /// </summary>
    public void OnEnterBuildMode()
    {
        _mainCanvas.SetActive(true);
        _activeBtns.SetActive(false);
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown, OnExitBuildMode);
    }

    public void OnExitBuildMode()
    {
        _mainCanvas.SetActive(false);
        _activeBtns.SetActive(true);
        EventManager.StopListening(ConstEvent.OnMouseRightButtonDown, OnExitBuildMode);
    }
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
        EventManager.StopListening(ConstEvent.OnMouseRightButtonDown, OnExitBuildMode);
        EventManager.StartListening(ConstEvent.OnFinishBuilding, this.OnFinishBuilding);
        BuildManager.Instance.CreateBuildingOnMouse(buildData.BundleName, buildData.PfbName);
    }

    public void OnFinishBuilding()
    {
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown, OnExitBuildMode);
        EventManager.StopListening(ConstEvent.OnFinishBuilding, this.OnFinishBuilding);
        HideOrShowCanvasToggle(true);
    }
    public void OnEnterHoverIcon(BuildData buildData)
    {

    }


    public void OnExitHoverIcon(BuildData buildData)
    {

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