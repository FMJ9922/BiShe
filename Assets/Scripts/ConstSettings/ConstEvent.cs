
public class ConstEvent
{
    #region 摄像机
    public const string OnCameraMove = "OnCameraMove";//摄像机移动
    public const string OnLockScroll = "OnLockScroll";//锁定滚轮缩放地图
    public const string OnLockMove = "OnLockMove";//锁定摄像机移动
    #endregion

    #region 点击事件
    public const string OnMaskClicked = "OnMaskClicked";//点击背景遮罩
    public const string OnMouseLeftButtonDown = "OnMouseLeftButtonDown";//鼠标左键点击事件
    public const string OnMouseLeftButtonHeld = "OnMouseLeftButtonHeld";//鼠标左键按住事件
    public const string OnMouseRightButtonDown = "OnMouseRightButtonDown";//鼠标右键点击事件
    #endregion

    #region 建造事件
    public const string OnGroundRayPosMove = "OnGroundRayPosMove";//鼠标投影地面位置移动事件
    public const string OnRotateBuilding = "OnRotateBuilding";//旋转建筑
    public const string OnFinishBuilding = "OnFinishBuilding";//确认建造或者取消事件
    public const string OnBuildToBeConfirmed = "OnBuildToBeConfirmed";//等待建筑确认建造
    public const string OnPlantSingleTree = "OnPlantSingleTree";//种树
    public const string OnContinueBuild = "OnContinueBuild";//继续建造
    #endregion

    #region 交互事件
    public const string OnTriggerInfoPanel = "OnTriggerInfoPanel";//点击建筑显示详细面板
    public const string OnTriggerCarMissionPanel = "OnTriggerCarMissionPanel";//点击车辆详情面板
    public const string OnSelectIcon = "OnSelectIcon";//点击通用icon
    #endregion

    #region 时间倍速事件
    public const string OnTimeScaleChanged = "OnTimeScaleChanged";//时间倍速变化事件
    public const string OnPauseGame = "OnPauseGame";//暂停游戏
    public const string OnResumeGame = "OnResumeGame";//恢复游戏
    #endregion

    #region 资源事件
    public const string OnOutputResources = "OnOutputResources";//每周输出资源结算
    public const string OnInputResources = "OnInputResources";//每周输入资源结算
    public const string OnDayWentBy = "OnDayWentBy";//一天过去事件
    public const string OnRefreshResources = "OnRefreshResources";//资源更新显示事件
    public const string OnPopulationHudChange = "OnPopulationHudChange";//人口增减事件
    public const string OnEffectivenessChange = "OnEffectivenessChange";//工作效率变化
    public const string OnSettleAccount = "OnSettleAccount";//市场结算事件
    public const string OnWeekProgress = "OnWeekProgree";//周进度
    public const string OnOilCost = "OnOilCost";//油费事件
    public const string OnPopulationChange = "OnPopulationChange";//人口发生变化
    public const string OnMarketOrderDealing = "OnMarketOrderDealing";//市场订单结算
    #endregion

    #region 加载事件

    public const string OnLoadingTips = "OnLoadingTips";//加载提示
    public const string OnLoadingOver = "OnLoadingOver";//加载完毕
    #endregion

    #region 显示
    public const string OnSelectLightOpen = "OnSelectLightOpen";
    public const string OnSelectLightClose = "OnSelectLightClose";
    public const string OnRangeLightOpen = "OnRangeLightOpen";
    public const string OnRangeLightClose = "OnRangeLightClose";
    public const string OnHudItemChange = "OnHudItemChange";
    #endregion
    
    #region 交通事件

    public const string OnTransportingNumChange = "OnTransportingNumChange";//当车辆运输的数量发生变化的时候

    #endregion
}
