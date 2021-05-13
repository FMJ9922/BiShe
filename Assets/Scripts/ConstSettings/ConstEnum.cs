
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
    straight = 0,
    inner = 1,
    outter = 2,
}

public enum BundlePrimaryType
{
    building,
    road,
}

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
public enum GridType
{
    empty,//空
    occupy,//占有
    inherent,//固有
    road,//路
}

public enum ItemType
{
    human = 0,//人力资源
    food = 1,//食物
    industry = 2,//工业品

}
public enum LanguageType
{
    chinese = 0,
    english = 1,
}

public enum TimeScale
{
    stop = 0,
    one = 1,
    two = 2,
    four = 3,
}

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

public enum CarMissionType
{
    requestResources = 0,//请求物资
    transportResources = 1,//运输物资

}

public enum SoundResource
{
    sfx_click_wareHouse,
    sfx_click_destroy,
    sfx_click_btn1,
    sfx_click_btn2,
    sfx_click_logCamp,
    sfx_click_factory,
    sfx_bgm_level1,
    sfx_bgm_level2,
    sfx_constuction,
    sfx_bgm_start,
    sfx_click_mine,
    sfx_click_pier,
    sfx_click_hut,
    sfx_click_farmland,//农田
    sfx_upgrade,//升级
    sfx_click_market,//市场
    sfx_dropdown,//下拉菜单
    sfx_click_farm = 17,//农场
}