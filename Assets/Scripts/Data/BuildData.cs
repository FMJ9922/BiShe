using System.Collections.Generic;

[System.Serializable]
public class BuildData
{
    public int Id;//序号
    public string Name;//名称
    public int CostPerWeek;//维护费用
    public List<int> Formulas;//生产可用配方
    public int Population;//建筑物所需的人口(负表示产出)
    public int MaxStorage;//存储上限
    public int InfluenceRange;//影响范围
    public int FrontBuildingId;//前置建筑Id
    public int RearBuildingId;//后置建筑Id
    public int Length;//占地长
    public int Width;//占地宽
    public int Price;//购买价格
    public List<CostResource> costResources = new List<CostResource>();//购买花费原料
    public string BundleName;
    public string PfbName;
    public BuildTabType tabType;
    public string Introduce;//简介
    
}

[System.Serializable]
public struct CostResource
{
    public int ItemId;
    public float ItemNum;
    public CostResource(int id, float num)
    {
        ItemId = id;
        ItemNum = num;
    }
}

