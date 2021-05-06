using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[ExecuteInEditMode]
public class PostEffect : MonoBehaviour
{

    #region Variables
    public float grayScaleAmout = 1.0f;

    //主相机
    public Camera sourceCamera;


    //渲染纹理
    RenderTexture renderTexture;

    //开始灰色渲染效果标志
    public bool isStart = false;

    //材质
    public Material material = null;

    #endregion



    void OnRenderImage(RenderTexture source, RenderTexture target)
    {
        if (isStart)
        {
            if (material != null)
            {
                material.SetFloat("_LuminosityAmount", grayScaleAmout);
                Graphics.Blit(source, target, material);
            }
        }
        else
        {
            Graphics.Blit(source, target);
        }
    }

}