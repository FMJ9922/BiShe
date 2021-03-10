using System.Collections.Generic;

[System.Serializable]
public class BuildData
{
    public int Id;//序号
    public string Name;//名称
    public int Length;//占地长
    public int Width;//占地宽
    public int Price;//购买价格
    public List<CostResource> costResources = new List<CostResource>();//购买花费原料
    public int Return;//拆除返还
    public int ProductTime;//生产时长
    public int MaxStorage;//存储上限
    public int InfluenceRange;//影响范围
    public int FrontBuildingId;//前置建筑Id
    public int RearBuildingId;//后置建筑Id
    public string Introduce;//简介
    public List<CostResource> inputResources;//生产需要的物品
    public List<CostResource> outputResources;//产出的物品
    public int people;//建筑物所需的人口
    public string BundleName;
    public string PfbName;
    public BuildTabType tabType;
    public int MaxLevel;//最大等级
}

[System.Serializable]
public struct CostResource
{
    public int ItemId;
    public int ItemNum;
    public CostResource(int id, int num)
    {
        ItemId = id;
        ItemNum = num;
    }
}