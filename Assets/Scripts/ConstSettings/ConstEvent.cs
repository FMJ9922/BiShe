
public class ConstEvent
{
    #region 摄像机
    public const string OnCameraMove = "OnCameraMove";//摄像机移动
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
    #endregion

    #region 交互事件
    public const string OnTriggerInfoPanel = "OnTriggerInfoPanel";//点击建筑显示详细面板
    #endregion

    #region 时间倍速事件
    public const string OnTimeScaleChanged = "OnTimeScaleChanged";//时间倍速变化事件
    #endregion

    #region 资源事件
    public const string OnOutputResources = "OnOutputResources";//每周输出资源结算
    public const string OnInputResources = "OnInputResources";//每周输入资源结算
    public const string OnDayWentBy = "OnDayWentBy";//一天过去事件
    public const string OnRefreshResources = "OnRefreshResources";//资源更新显示事件
    public const string OnPopulaitionChange = "OnPopulaitioChange";//人口增减事件
    public const string OnSettleAccount = "OnSettleAccount";//市场结算事件
    #endregion
}
