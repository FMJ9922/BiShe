using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBase : MonoBehaviour
{
    //占地长宽
    [SerializeField]
    private int Width;
    [SerializeField]
    private int Height;

    [SerializeField]
    private BundlePrimaryType BundlePrimaryType;

    
    public string BundleName => string.Format("{0}.ab", BundlePrimaryType.ToString());

    public string PfbName => this.gameObject.name;

    public Vector2Int Size => new Vector2Int(Height, Width);

    public bool hasAnima = false;

    public Animation animation;

    public GameObject body;

    public RuntimeBuildData runtimeBuildData;

    public bool buildFlag = false;
     
    public virtual void OnConfirmBuild()
    {
        buildFlag = true;
        gameObject.tag = "Building";
        if (hasAnima)
        {
            body.SetActive(false);
            animation.gameObject.SetActive(true);
            float time = animation.clip.length;
            Invoke("ShowBody", time);
            animation.Play();
        }
        InitBuildingFunction();
    }
    /// <summary>
    /// 建造完后初始化建筑功能
    /// </summary>
    public virtual void InitBuildingFunction()
    {
        
    }
    public void ShowBody()
    {
        body.SetActive(true);
        animation.gameObject.SetActive(false);
    }

    //public virtual string GetIntroduce()
    //{
    //    return string.Empty;
    //}

    public static RuntimeBuildData CastBuildDataToRuntime(BuildData buildData)
    {
        RuntimeBuildData runtimeBuildData = new RuntimeBuildData();
        runtimeBuildData.Id = buildData.Id;
        runtimeBuildData.Name = buildData.Name;
        runtimeBuildData.Length = buildData.Length;
        runtimeBuildData.Width = buildData.Width;
        runtimeBuildData.Price = buildData.Price;
        runtimeBuildData.costResources = buildData.costResources;
        runtimeBuildData.Return = buildData.Return;
        runtimeBuildData.ProductTime = buildData.ProductTime;
        runtimeBuildData.MaxStorage = buildData.MaxStorage;
        runtimeBuildData.InfluenceRange = buildData.InfluenceRange;
        runtimeBuildData.FrontBuildingId = buildData.FrontBuildingId;
        runtimeBuildData.RearBuildingId = buildData.RearBuildingId;
        runtimeBuildData.Introduce = buildData.Introduce;
        runtimeBuildData.inputResources = buildData.inputResources;
        runtimeBuildData.outputResources = buildData.outputResources;
        runtimeBuildData.BundleName = buildData.BundleName;
        runtimeBuildData.PfbName = buildData.PfbName;
        runtimeBuildData.tabType = buildData.tabType;
        runtimeBuildData.MaxLevel = buildData.MaxLevel;
        return runtimeBuildData;
    }
}

public class RuntimeBuildData : BuildData
{
    public bool Pause = true;//是否暂停生产
    public int CurLevel = 0;//当前等级
}