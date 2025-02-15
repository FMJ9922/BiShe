﻿
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
public enum EBuildingType
{
    Null,
    BridgeBuilding,
    FactoryBuilding,
    FarmLandBuilding,
    GovernmentBuilding,
    HutBuilding,
    LoggingCampBuilding,
    MarketBuilding,
    MineBuilding,
    OrchardBuilding,
    PierBuilding,
    StorageBuilding,
    TradeBuilding,
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
    straight = 0,
    inner = 1,
    outter = 2,
}

public enum BundlePrimaryType
{
    building,
    road,
}

[System.Serializable]
public enum Direction
{
    right = 1,
    up = 2,
    left = 3,
    down = 0
}

public enum RoadType
{

}
/// <summary>
/// 建造页签的枚举
/// </summary>
public enum BuildTabType
{
    [Description("Road")]
    road = 0,
    [Description("Housing")]
    house = 1,
    [Description("Produce")]
    produce = 2,
    [Description("Manufacturing")]
    manufacturing = 3,
    [Description("Utilitiy")]
    utility = 4,
    hide = 5,
    bridge = 6,
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
[System.Serializable]
public enum GridType
{
    empty,//空
    occupy,//占有,一般指建筑或者地图外
    road,//路
    water,
    bridge,
}

public enum ItemType
{
    human = 0,//人力资源
    food = 1,//食物
    industry = 2,//工业品

}
public enum LanguageType
{
    Chinese = 0,
    English = 1,
    German = 2,
}

public enum TimeScale
{
    stop = 0,
    one = 1,
    two = 2,
    four = 3,
}

[System.Serializable]
public enum TransportationType
{
    [Description("Van")]
    van = 0,
    [Description("MiniTruck")]
    mini = 1,
    [Description("MediumTruck")]
    medium = 2,
    [Description("Harvester")]
    harvester = 3,
    [Description("sandTruck")]
    sandTruck = 4,
}

[System.Serializable]
public enum CarMissionType
{
    requestResources = 0,//请求物资
    transportResources = 1,//运输物资
    harvest = 2,//收割
    collectOrderGoods = 3,//取订单
    goForOrder = 4,//去运输订单
    backFromOrder = 5,//从运输订单返回
}

public enum SoundResource
{
    sfx_click_wareHouse = 0,
    sfx_click_destroy = 1,
    sfx_click_btn1 = 2,
    sfx_click_btn2 = 3,
    sfx_click_logCamp = 4,
    sfx_click_factory = 5,
    sfx_constuction = 6,
    sfx_click_mine = 7,
    sfx_click_pier = 8,
    sfx_click_hut = 9,
    sfx_click_farmland = 10,//农田
    sfx_upgrade = 11,//升级
    sfx_click_market = 12,//市场
    sfx_dropdown = 13,//下拉菜单
    sfx_click_farm = 14,//农场
}

public enum IconDescription
{
    [Description("Build")]
    Build = 9,
    [Description("Destroy")]
    Destroy = 1,
    [Description("Upgrade")]
    Upgrade = 2,
    [Description("Money")]
    Money = 3,
    [Description("Log")]
    Log = 4,
    [Description("Stone")]
    Stone = 5,
    [Description("Food")]
    Food = 6,
    [Description("Population")]
    Population = 7,
    [Description("TechUsing")]
    TechUsing = 8,
    Custom = 0,
}

[System.Serializable]
public enum TradeMode
{
    [Description("None")]
    none = 0,
    [Description("Once")]
    once = 1,
    [Description("EveryWeek")]
    everyWeek = 2,
    [Description("EveryMonth")]
    everyMonth = 3,
}


public enum WarningType
{
    noPeople = 0,
    noResources = 1,
    noRoad = 2,
}

public enum TexIndex
{
    NormalGrass = 0,
    NormalSand = 1,
    FarmLand = 2,
    GrassSand = 3,
    RoadStraight3 = 4,
    RoadInCorner3 = 5,
    RoadOutCorner3 = 6,
    MixMudGrassDark = 7,
    RoadStraight2 = 8,
    RoadInCorner2 = 9,
    RoadOutCorner2 = 10,
    FarmLandSandBorder = 11,
    RoadStraight1 = 12,
    RoadInCorner1 = 13,
    RoadOutCorner1 = 14,
    Cement = 15,
    DarkMud = 16,
    MudGrass = 17,
    DarkGrass = 18,
    LightGrass = 19,
    MixSandRock = 20,
    MixMudGrassLight = 21,
}

