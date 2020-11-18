
using System.ComponentModel;
/// <summary>
/// 科技类型，可以是建筑，建筑的升级版，加成等
/// </summary>
public enum TechType
{
    Agriculture = 0,//农业
    Industry = 1,//工业
    Manufacturing = 2,//制造业
    Service = 3,//服务业
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

public enum RoadOption
{
    start = 0,
    normal = 1,
    end = 2,
}

public enum BundlePrimaryType
{
    building,
    road,
}



/// <summary>
/// 建造页签的枚举
/// </summary>
public enum BuildTabType
{
    [Description("住房")]
    house = 0,
    [Description("农业")]
    agriculture = 1,
    [Description("林业")]
    forest = 2,
    [Description("制造业")]
    manufacturing = 3,
    [Description("路")]
    road = 4,//路
    [Description("公共设施")]
    utility = 5,
}

public enum DriveType
{
    [Description("单次")]
    once,
    [Description("循环")]
    loop,
    [Description("往返")]
    yoyo,
}


