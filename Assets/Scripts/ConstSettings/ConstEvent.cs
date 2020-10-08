
public class ConstEvent
{
    #region 摄像机
    public const string OnCameraMove = "OnCameraMove";//摄像机移动
    #endregion

    #region 点击事件
    public const string OnMaskClicked = "OnMaskClicked";//点击背景遮罩
    public const string OnMouseLeftButtonDown = "OnMouseLeftButtonDown";//鼠标左键点击事件
    #endregion

    #region 建造事件
    public const string OnGroundRayPosMove = "OnGroundRayPosMove";//鼠标投影地面位置移动事件
    public const string OnRotateBuilding = "OnRotateBuilding";//按住鼠标中间并左右移动
    #endregion
}
