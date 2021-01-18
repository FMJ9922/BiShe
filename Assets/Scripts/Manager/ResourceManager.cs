using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理关卡运行时玩家所拥有的资源
/// </summary>
public class ResourceManager : Singleton<ResourceManager>
{
    /// <summary>
    /// 仓库里物品名称和对应数量的字典
    /// </summary>
    private Dictionary<string, float> _storedItemDic = new Dictionary<string, float>();

    /// <summary>
    /// 初始化关卡资源
    /// </summary>
    public void InitResourceManager(int levelID)
    {
        LevelData data = DataManager.GetLevelData(levelID);
        AddResource("Wood", data.wood);
        AddResource("Stone", data.stone);
        AddResource("Money", data.money);
    }

    /// <summary>
    /// 往仓库里存物品
    /// </summary>
    /// <param name="name">物品名称</param>
    /// <param name="num">物品数量</param>
    public void AddResource(string name, float num)
    {
        if (_storedItemDic.ContainsKey(name))
        {
            _storedItemDic[name] += num;
        }
        else
        {
            _storedItemDic.Add(name, num);
        }
    }

    /// <summary>
    /// 从全局仓库取物品
    /// </summary>
    /// <param name="name">物品名称</param>
    /// <param name="num">物品数量</param>
    /// <returns>是否成功</returns>
    public bool TryUseResource(string name, float num)
    {
        float storedNum;
        if (_storedItemDic.TryGetValue(name, out storedNum))//字典里已存该物品
        {
            if (num <= storedNum)//物品数量足够消耗
            {
                _storedItemDic[name] -= num;//消耗物品
                return true;//返回成功
            }
            else
            {
                return false;//不够就返回失败且不消耗物品
            }
        }
        else
        {
            return false;//不存在该物品就返回失败
        }
    }

    public Dictionary<string, float> GetAllResource()
    {
        return _storedItemDic;
    }
}
