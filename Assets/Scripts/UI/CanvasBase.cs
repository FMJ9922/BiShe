using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasBase : MonoBehaviour
{
    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void InitCanvas() { }

    /// <summary>
    /// 打开时干什么
    /// </summary>
    public virtual void OnOpen() { }

    /// <summary>
    /// 关闭时干什么
    /// </summary>
    public virtual void OnClose() { }
}
