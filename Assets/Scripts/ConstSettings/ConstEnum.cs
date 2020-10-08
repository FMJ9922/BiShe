
/// <summary>
/// 科技类型，可以是建筑，建筑的升级版，加成等
/// </summary>
public enum TechType
{
    Agriculture = 0,
    Industry = 1,
    Manufacturing = 2,
    Service = 3,
}

/// <summary>
/// 建筑类型枚举，每种建筑必有相应的科技枚举，而多种建筑可以对应同一个科技枚举
/// </summary>
public enum BuildingType
{
    Farmland = 0,               //农田
    Sawmill = 1,                //伐木场
    Orchard = 2,                //果园
    Mine = 3,                   //矿井

    ProcessingFactory = 100,    //加工厂
    Mill = 101,                 //磨坊

    FurnitureFactory = 200,     //家具制造厂
}

/// <summary>
/// 调用科技树面板的行为类型
/// </summary>
public enum TechTreeAction
{
    Upgrade,//升级
    Select,//选择带入关卡的相关科技建筑
}

public enum IconType
{
    Tech = 0,
    Level = 1,
}
