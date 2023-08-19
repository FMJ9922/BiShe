using System;
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

    protected void CleanUpAllAttachedChildren(Transform target,int reserveNum = 0)
    {
        for (int i = 0; i < target.childCount-reserveNum; i++)
        {
            Destroy(target.GetChild(i).gameObject);
        }

    }

    protected void InitGridItem(Action<int,GameObject> onItemInit,int count, Transform parent)
    {
        int childCount = parent.childCount;
        for (int i = 0; i < count; i++)
        {
            GameObject obj;
            if (i >= childCount)
            {
                obj = Instantiate(parent.GetChild(0).gameObject, parent);
                obj.transform.localPosition = Vector3.zero;
                onItemInit(i, obj);
            }
            else
            {
                obj = parent.GetChild(i).gameObject;
            }
            obj.SetActive(true);
            onItemInit(i, obj);
        }

        childCount = parent.childCount;
        if (childCount > count)
        {
            for (int i = count; i < childCount; i++)
            {
                parent.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
