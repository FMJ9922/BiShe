using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理关卡运行时玩家所拥有的资源
/// </summary>
public class ResourceManager : Singleton<ResourceManager>
{
    /// <summary>
    /// 仓库里物品ID和对应数量的字典
    /// </summary>
    private Dictionary<int, float> _storedItemDic = new Dictionary<int, float>();

    private int curPopulation = 0;
    private int maxPopulation = 0;

    /// <summary>
    /// 初始化关卡资源
    /// </summary>
    public void InitResourceManager(int levelID)
    {
        LevelData data = DataManager.GetLevelData(levelID);
        AddResource(DataManager.GetItemIdByName("Log"), data.log);
        AddResource(DataManager.GetItemIdByName("Rice"), data.rice);
        AddResource(DataManager.GetItemIdByName("Money"), data.money);
        AddResource(DataManager.GetItemIdByName("Stone"), data.stone);
    }

    /// <summary>
    /// 往仓库里存物品
    /// </summary>
    /// <param name="Id">物品名称</param>
    /// <param name="num">物品数量</param>
    public void AddResource(int Id, float num)
    {
        if (_storedItemDic.ContainsKey(Id))
        {
            _storedItemDic[Id] += num;
        }
        else
        {
            _storedItemDic.Add(Id, num);
        }
        EventManager.TriggerEvent(ConstEvent.OnRefreshResources);
    }

    /// <summary>
    /// 从全局仓库取物品
    /// </summary>
    /// <param name="Id">物品名称</param>
    /// <param name="num">物品数量</param>
    /// <returns>是否成功</returns>
    public bool TryUseResource(int Id, float num)
    {
        float storedNum;
        if (_storedItemDic.TryGetValue(Id, out storedNum))//字典里已存该物品
        {
            if (num <= storedNum)//物品数量足够消耗
            {
                _storedItemDic[Id] -= num;//消耗物品
                EventManager.TriggerEvent(ConstEvent.OnRefreshResources);
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

    public float TryGetResourceNum(int Id)
    {
        float storedNum;
        if (_storedItemDic.TryGetValue(Id, out storedNum))//字典里已存该物品
        {
            return storedNum;
        }
        else
        {
            return 0;
        }
    }

    public float TryGetResourceTotalNum(int[] Ids)
    {
        float storedNum =0;
        for (int i = 0; i < Ids.Length; i++)
        {
            float num;
            if (_storedItemDic.TryGetValue(Ids[i], out num))//字典里已存该物品
            {
                storedNum += num;
            }
        }
        return storedNum;
    }
    public bool TryUseResource(CostResource costResource)
    {
        float storedNum;
        float num = costResource.ItemNum;
        int Id = costResource.ItemId;
        //Debug.Log(costResource.ItemId + " " + costResource.ItemNum);
        if (_storedItemDic.TryGetValue(Id, out storedNum))//字典里已存该物品
        {
            if (num <= storedNum)//物品数量足够消耗
            {
                _storedItemDic[Id] -= num;//消耗物品
                EventManager.TriggerEvent(ConstEvent.OnRefreshResources);
                return true;//返回成功
            }
            else
            {
                Debug.Log("缺少数量：" + Id + " " + storedNum);
                return false;//不够就返回失败且不消耗物品
            }
        }
        else
        {
            Debug.Log("不存在物品：" + Id);
            return false;//不存在该物品就返回失败
        }
    }
    public Dictionary<int, float> GetAllResource()
    {
        return _storedItemDic;
    }

    public void AddMaxPopulation(int num,out bool success)
    {
        maxPopulation += num;
        if (maxPopulation >= 0 && maxPopulation <= 200)
        {
            success = true;
            return;
        }
        maxPopulation = Mathf.Clamp(maxPopulation, 0, 200);
        success = false;
    }

    public void AddCurPopulation(int num, out bool success)
    {
        curPopulation += num;
        if (curPopulation >= 0 && curPopulation <= maxPopulation)
        {
            success = true;
            return;
        }
        curPopulation = Mathf.Clamp(curPopulation, 0, maxPopulation);
        success = false;
    }
}

