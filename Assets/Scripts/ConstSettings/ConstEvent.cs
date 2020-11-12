
public class ConstEvent
{
    #region 摄像机
    public const string OnCameraMove = "OnCameraMove";//摄像机移动
    #endregion

    #region 点击事件
    public const string OnMaskClicked = "OnMaskClicked";//点击背景遮罩
    public const string OnMouseLeftButtonDown = "OnMouseLeftButtonDown";//鼠标左键点击事件
    public const string OnMouseRightButtonDown = "OnMouseRightButtonDown";//鼠标右键点击事件
    #endregion

    #region 建造事件
    public const string OnGroundRayPosMove = "OnGroundRayPosMove";//鼠标投影地面位置移动事件
    public const string OnRotateBuilding = "OnRotateBuilding";//旋转建筑
    public const string OnFinishBuilding = "OnFinishBuilding";//确认建造或者取消事件
    #endregion
}
