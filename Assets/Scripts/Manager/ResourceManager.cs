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

    /// <summary>
    /// 上周结束时候的物品
    /// </summary>
    private Dictionary<int, float> _lastItemDic = new Dictionary<int, float>();

    private int curPopulation = 0;
    private int maxPopulation = 0;
    public int MaxPopulation { get { return maxPopulation; } }
    public int CurPopulation { get { return curPopulation; } }

    public float maxStorage = 1000;

    /// <summary>
    /// 初始化关卡资源
    /// </summary>
    public void InitResourceManager(int levelID)
    {
        LevelData data = DataManager.GetLevelData(levelID);
        AddResource(DataManager.GetItemIdByName("Log"), 200,false);
        AddResource(DataManager.GetItemIdByName("Rice"), data.rice, false);
        AddResource(DataManager.GetItemIdByName("Money"), data.money, false);
        AddResource(DataManager.GetItemIdByName("Stone"), 100, false);
        //AddResource(12015, 100);
        //AddResource(12020, 100);
        //AddResource(12009, 100);
        RecordLastWeekItem();
    }

    public void InitSavedResourceManager(SaveData saveData)
    {
        AddResources(saveData.saveResources, false);
        curPopulation = 0;
        //Debug.Log(saveData.curPopulation);
        RecordLastWeekItem();
    }

    /// <summary>
    /// 往仓库里存物品
    /// </summary>
    /// <param name="Id">物品名称</param>
    /// <param name="num">物品数量</param>
    public void AddResource(int Id, float num,bool isLimited = true)
    {
        if (Id == DataManager.GetItemIdByName("Money"))
        {
            AddMoney(num);
            return;
        }
        if (isLimited)
        {
            float cur = GetRemainStorage();
            float canAddNum = cur > num ? num : cur;
            if (canAddNum <= 0)
            {
                return;
            }
            //Debug.Log(canAddNum);
            if (_storedItemDic.ContainsKey(Id))
            {
                
                _storedItemDic[Id] += canAddNum;
            }
            else
            {
                _storedItemDic.Add(Id, canAddNum);
            }
        }
        else
        {
            if (_storedItemDic.ContainsKey(Id))
            {
                _storedItemDic[Id] += num;
            }
            else
            {
                _storedItemDic.Add(Id, num);
            }
        }
    }
    public void AddResource(CostResource costResource, bool isLimited = true)
    {
        AddResource(costResource.ItemId, costResource.ItemNum, isLimited);
    }

    public CostResource[] GetAllResources()
    {
        List<CostResource> res = new List<CostResource>();
        foreach(var keypairs in _storedItemDic)
        {
            res.Add(new CostResource(keypairs.Key, keypairs.Value));
        }
        return res.ToArray();
    }

    public void AddMoney(float num)
    {
        if (_storedItemDic.ContainsKey(DataManager.GetItemIdByName("Money")))
        {
            _storedItemDic[DataManager.GetItemIdByName("Money")] += num;
        }
        else
        {
            _storedItemDic.Add(DataManager.GetItemIdByName("Money"), num);
        }
    }
    public void AddResources(CostResource[] costResources,bool isLimited = true)
    {
        for (int i = 0; i < costResources.Length; i++)
        {
            AddResource(costResources[i],isLimited);
        }
        
    }
    public void RecordLastWeekItem()
    {
        _lastItemDic.Clear();
        foreach (var pair in _storedItemDic)
        {
            _lastItemDic.Add(pair.Key, pair.Value);
        }
    }

    public float GetWeekDeltaNum(int id)
    {
        if(id == 11000)
        {
            return GetAllFoodNum() - GetAllLastFoodNum();
        }
        else return TryGetResourceNum(id) - TryGetLastResourceNum(id);
    }

    /// <summary>
    /// 从全局仓库取物品
    /// </summary>
    /// <param name="Id">物品名称</param>
    /// <param name="num">物品数量</param>
    /// <returns>是否成功</returns>
    public bool TryUseResource(int Id, float num, float times = 1)
    {
        float storedNum;
        num *= times;
        if (_storedItemDic.TryGetValue(Id, out storedNum))//字典里已存该物品
        {
            if (num <= storedNum)//物品数量足够消耗
            {
                _storedItemDic[Id] -= num;//消耗物品
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

    public float TryGetLastResourceNum(int Id)
    {
        float storedNum;
        if (_lastItemDic.TryGetValue(Id, out storedNum))
        {
            //Debug.Log(Id + " " + storedNum);
            return storedNum;
        }
        else
        {
            return 0;
        }
    }

    public float TryGetResourceTotalNum(int[] Ids)
    {
        float storedNum = 0;
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
    public void TryUseResource(CostResource costResource, float times = 1)
    {
        float storedNum;
        float num = costResource.ItemNum*times;
        int Id = costResource.ItemId;
        if (_storedItemDic.TryGetValue(Id, out storedNum))//字典里已存该物品
        {
            if (num <= storedNum)//物品数量足够消耗
            {
                _storedItemDic[Id] -= num;//消耗物品
            }
            else
            {
                Debug.Log("缺少数量：" + Id + " " + storedNum);
            }
        }
        else
        {
            Debug.Log("不存在物品：" + Id);
        }
    }
    public void TryUseResources(List<CostResource> costResources, float times = 1)
    {
        for (int i = 0; i < costResources.Count; i++)
        {
            TryUseResource(costResources[i],1);
        }
    }
    public bool IsResourcesEnough(List<CostResource> costResources,float times = 1)
    {
        bool res = true;
        for (int i = 0; i < costResources.Count; i++)
        {
            res &= IsResourceEnough(costResources[i],times);
        }
        return res;
    }
    public bool IsResourceEnough(CostResource costResource, float times = 1)
    {
        float storedNum;
        float num = costResource.ItemNum * times;
        int Id = costResource.ItemId;
        if (_storedItemDic.TryGetValue(Id, out storedNum))//字典里已存该物品
        {
            if (num <= storedNum)//物品数量足够消耗
            {
                return true;//返回成功
            }
            else
            {
                //Debug.Log("缺少数量：" + Id + " " + storedNum);
                return false;//不够就返回失败且不消耗物品
            }
        }
        else
        {
            //Debug.Log("不存在物品：" + Id);
            return false;//不存在该物品就返回失败
        }
    }
    public float GetAllFoodNum()
    {
        int[] foodIds = DataManager.GetFoodIDList();
        float sum = 0;
        for (int i = 0; i < foodIds.Length; i++)
        {
            sum += TryGetResourceNum(foodIds[i]);
        }
        return sum;
    }

    public CostResource GetFoodByHappiness(float requestNum)
    {
        ItemData[] foods = DataManager.GetFoodItemList();
        for (int i = 0; i < foods.Length; i++)
        {
            if(IsResourceEnough(new CostResource(foods[i].Id, requestNum)))
            {
                return new CostResource(foods[i].Id, requestNum);
            }
        }
        return null;
    }

    public CostResource GetFoodByMax(float requestNum)
    {
        ItemData[] foods = DataManager.GetFoodItemList();
        float maxNum = 0;
        int p = 0;
        for (int i = 0; i < foods.Length; i++)
        {
            float num = TryGetResourceNum(foods[i].Id);
            if (num > maxNum)
            {
                p = i;
                maxNum = num;
            }
        }
        if (maxNum < requestNum)
        {
            requestNum = maxNum;
        }
        return new CostResource(foods[p].Id, requestNum);
    }
    public float GetAllLastFoodNum()
    {
        int[] foodIds = DataManager.GetFoodIDList();
        float sum = 0;
        for (int i = 0; i < foodIds.Length; i++)
        {
            sum += TryGetLastResourceNum(foodIds[i]);
        }
        return sum;
    }

    public CostResource TryUseUpResource(CostResource costResource)
    {
        float storedNum;
        float num = costResource.ItemNum;
        int Id = costResource.ItemId;
        if (_storedItemDic.TryGetValue(Id, out storedNum))//字典里已存该物品
        {
            if (num <= storedNum)//物品数量足够消耗
            {
                _storedItemDic[Id] -= num;//消耗物品
                return costResource;//返回成功
            }
            else if (storedNum <= 0)
            {
                _storedItemDic.Remove(Id);
                return null;
            }
            else
            {
                _storedItemDic[Id] -= storedNum;
                return new CostResource(Id,storedNum);
            }
        }
        else
        {
            return null;
        }
    }
    public Dictionary<int, float> GetAllResource()
    {
        return _storedItemDic;
    }

    /// <summary>
    /// 增大人口上限
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public bool AddMaxPopulation(int num)
    {
        maxPopulation += num;
        if (maxPopulation >= 0 && maxPopulation <= 10000)
        {
            EventManager.TriggerEvent(ConstEvent.OnPopulaitionChange);
            return true;
        }
        maxPopulation = Mathf.Clamp(maxPopulation, 0, 10000);
        Debug.Log("人口操作可能有问题，请检查");
        return false;
    }

    /// <summary>
    /// 占用人口
    /// </summary>
    /// <param name="num">申请的人口数量</param>
    /// <returns>实际提供下来的人口数量</returns>
    public int TryAddCurPopulation(int num)
    {
        //Debug.Log(num);
        curPopulation += num;
        if (curPopulation <= 0)
        {
            curPopulation -= num;
            int maxProvide = 0 - curPopulation;
            curPopulation += maxProvide;
            return maxProvide;
        }
        else
        if (curPopulation <= maxPopulation)
        {
            return num;
        }
        else
        {
            curPopulation -= num;
            int maxProvide = maxPopulation - curPopulation;
            curPopulation += maxProvide;
            //Debug.Log(maxProvide);
            return maxProvide;
        }
    }

    public void AddMaxStorage(float max)
    {
        maxStorage += max;
    }

    public float GetCurStorage()
    {
        float num = 0;
        foreach (var item in _storedItemDic)
        {
            if(item.Key!= DataManager.GetItemIdByName("Money"))
            {
                num += item.Value;
            }
        }
        return num;
    }
    /// <summary>
    /// 获取剩余的容量
    /// </summary>
    /// <returns></returns>
    public float GetRemainStorage()
    {
        return maxStorage - GetCurStorage();
    }
}

