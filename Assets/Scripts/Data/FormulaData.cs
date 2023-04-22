using System.Collections.Generic;
using UnityEngine.Serialization;

[System.Serializable]
public class FormulaData 
{
    public int ID;//配方ID
    public string Describe;//配方描述
    public List<int> InputItemID;//生产消耗物品
    public List<int> InputNum;//消耗量/周
    public List<int> OutputItemID;//生产产出物品
    public List<int> ProductNum;//产品数量
    public int ProductTime;//生产时长
}

[System.Serializable]
public class OrderData
{
    public int ID;//订单id
    public int Type;//订单类型 0：一次性 1:持续性
    public int ItemId;//商品id
    public int ItemNum;//商品数量
    public float GoodsPrice;//商品价格系数
    public string Destination;//目的地0.城市 1.其它乡村 2.码头 3.火车站
    public float Distance;//距离
    public float StartRate;//初始权重
    public float MaxRate;//最高权重
    public int TimeLimit;//首次交付时限
    public int RepeatTime;//循环交付周期
    public string DescriptionIds;//描述
    public int LevelLimit;//特定关卡id
    public int CheckItemId;//检查物品存在id
    public List<int> AddRateTarget;//累加权重目标
}

[System.Serializable]
public class RuntimeOrderData
{
    public long Index;//自增id
    public bool IsBuy;//是否是购买
    public int OrderId;
    public int StartWeek;
    public bool IsRepeating;
    public float HasTransportGoodsNum;//已交付货物数量
    public float PromiseTransportGoodsNum;//已确认准备运输的数量
    public List<int> RunningThisOrderCar;//正在运输这单的车辆索引
    public bool HasFinish;//订单是否完成，对于持续性订单来说，代表是否已经领过钱了
}