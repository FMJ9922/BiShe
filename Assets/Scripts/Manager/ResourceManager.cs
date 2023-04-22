using System;
using System.Collections;
using System.Collections.Generic;
using Building;
using CSTools;
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

    private Dictionary<int, float> _deltaItemBuildingDic = new Dictionary<int, float>();
    private Dictionary<int, float> _deltaItemTradingDic = new Dictionary<int, float>();

    public Dictionary<int, int[]> _itemHistoryNumDic = new Dictionary<int, int[]>();

    private int _workerPopulation = 0;
    private int _allPopulation = 0;
    public int AllPopulation { get { return _allPopulation; } }
    public int WorkerPopulation { get { return _workerPopulation; } }

    public List<int> _hudList;//玩家设置资源监视窗口名单

    public List<int> _forbiddenFoodList;//玩家设置的不可食用名单

    /// <summary>
    /// 初始化关卡资源
    /// </summary>
    public void InitResourceManager(int levelID)
    {
        LevelData data = DataManager.GetLevelData(levelID);
        AddResource(DataManager.GetItemIdByName("Log"), data.log,false);
        AddResource(DataManager.GetItemIdByName("Rice"), data.rice, false);
        AddResource(DataManager.GetItemIdByName("Money"), data.money, false);
        AddResource(DataManager.GetItemIdByName("Stone"), data.stone, false);
/*#if UNITY_EDITOR
        foreach (var item in DataManager.Instance.ItemArray)
        {
            AddResource(item.Id, 999,false);
        }
        AddResource(99999, 999,false);
#endif*/
        InitHUDList(null);
        InitForbiddenFoodList(null);
        InitAllTimeResourcesList(null);
        RecordLastWeekItem();
        StartListening();
    }

    public void InitSavedResourceManager(SaveData saveData)
    {
        AddResources(saveData.saveResources, false);
        _workerPopulation = 0;
        //Debug.Log(saveData.curPopulation);
        InitHUDList(saveData.hudList);
        InitForbiddenFoodList(saveData.forbiddenFoodList);
        InitAllTimeResourcesList(saveData.allTimeResources);
        RecordLastWeekItem();
        StartListening();
    }

    private void StartListening()
    {
        EventManager.StartListening(ConstEvent.OnPopulationChange,RecalculatePopulation);
    }

    private void InitForbiddenFoodList(int[] list)
    {
        if (list != null)
        {
            _forbiddenFoodList = new List<int>(list);
        }
        else
        {
            _forbiddenFoodList = new List<int>();
        }
    }
    private void InitHUDList(int[] list)
    {
        if (list != null)
        {
            _hudList = new List<int>(list);
        }
        else
        {
            _hudList = new List<int> { 99999, 12003, 12009, DataManager.GetItemIdByName("Rice")};
        }
    }

    private void InitAllTimeResourcesList(Dictionary<int,int[]> dic)
    {
        if (dic != null)
        {
            _itemHistoryNumDic = dic;
        }
        else
        {
            _itemHistoryNumDic = new Dictionary<int, int[]>();
        }

    }

    private int[] GetItemHistoryNumArray(int id)
    {
        if (_itemHistoryNumDic.ContainsKey(id))
        {
            return _itemHistoryNumDic[id];
        }
        else
        {
            int[] array = new int[48000];
            array[LevelManager.Instance.WeekIndex] = (int)TryGetResourceNum(id);
            _itemHistoryNumDic.Add(id, array);
            return array;
        }
    }

    public void UpdateItemHistroyNumDic()
    {
        int index = LevelManager.Instance.WeekIndex;
        foreach(var curItem in _storedItemDic)
        {
            if (_itemHistoryNumDic.ContainsKey(curItem.Key))
            {
                _itemHistoryNumDic[curItem.Key][index] = (int)curItem.Value;
            }
            else
            {
                int[] array = new int[48000];
                array[index] = (int)curItem.Value;
                _itemHistoryNumDic.Add(curItem.Key, array);
            }
        }
    }

    public void UpdateItemDeltaNumDic()
    {
        _deltaItemBuildingDic.Clear();
        _deltaItemTradingDic.Clear();
        List<BuildingBase> buildings = MapManager.Instance.GetAllBuildings();
        List<CostResource> p;
        for (int i = 0; i < buildings.Count; i++)
        {
            if ((buildings[i] is IProduct) || (buildings[i] is IHabitable))
            {
                p = BuildingTools.GetBuildingWeekDeltaResources(buildings[i].runtimeBuildData);
                for (int j = 0; j < p.Count; j++)
                {
                    if (_deltaItemBuildingDic.ContainsKey(p[j].ItemId))
                    {
                        _deltaItemBuildingDic[p[j].ItemId] += p[j].ItemNum;
                    }
                    else
                    {
                        _deltaItemBuildingDic.Add(p[j].ItemId, p[j].ItemNum);
                    }
                }
            }
        }
        p = MarketManager.Instance.GetDeltaNum();
        if (p != null)
        {
            for (int j = 0; j < p.Count; j++)
            {
                if (_deltaItemTradingDic.ContainsKey(p[j].ItemId))
                {
                    _deltaItemTradingDic[p[j].ItemId] += p[j].ItemNum;
                }
                else
                {
                    _deltaItemTradingDic.Add(p[j].ItemId, p[j].ItemNum);
                }
            } 
        }
    }

    public float GetMaxStorage()
    {
        float max = 0;
        var storageBuildings = MapManager.Instance.GetAllBuildings(EBuildingType.StorageBuilding);
        for (int i = 0; i < storageBuildings.Count; i++)
        {
            max += storageBuildings[i].runtimeBuildData.MaxStorage;
        }

        return max;
    }

    public float GetWeekDeltaNum(int id)
    {
        float ret = 0;
        if (_deltaItemBuildingDic.ContainsKey(id))
        {
            ret += _deltaItemBuildingDic[id];
        }
        if (_deltaItemTradingDic.ContainsKey(id))
        {
            ret += _deltaItemTradingDic[id];
        }

        if (id == 99999)
        {
            //todo
            //ret -= TrafficManager.Instance.WeeklyCost;
        }
        return ret;
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
    /*
    public float GetWeekDeltaNum(int id)
    {
        if(id == 11000)
        {
            return GetAllFoodNum() - GetAllLastFoodNum();
        }
        else return TryGetResourceNum(id) - TryGetLastResourceNum(id);
    }*/

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
        if(Id == 11000)
        {
            return GetAllFoodNum();
        }
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

    /// <summary>
    /// 尝试消耗物品
    /// </summary>
    /// <param name="id">物品id</param>
    /// <param name="num">物品数量</param>
    /// <returns>实际消耗的数量</returns>
    public float UseResources(int id,float num)
    {
        if (_storedItemDic.TryGetValue(id, out float hasNum))//字典里已存该物品
        {
            if (hasNum >= num)
            {
                _storedItemDic[id] -= num;
                return num;
            }
            else
            {
                _storedItemDic[id] = 0;
                return hasNum;
            }
        }
        return 0;
    }

    public List<CostResource> UseResources(List<CostResource> costResources)
    {
        var ret = new List<CostResource>();
        for (int i = 0; i < costResources.Count; i++)
        {
            float realUseNum = UseResources(costResources[i].ItemId, costResources[i].ItemNum);
            ret.Add(new CostResource(costResources[i].ItemId,realUseNum));
        }
        return ret;
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
                //Debug.Log("缺少数量：" + Id + " " + storedNum);
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
    
    public bool IsResourceEnough(int itemId, float itemNum)
    {
        if (_storedItemDic.TryGetValue(itemId, out float storedNum))
        {
            if (itemNum <= storedNum)//物品数量足够消耗
            {
                return true;//返回成功
            }
        }
        return false;
    }
    public float GetAllFoodNum()
    {
        int[] foodIds = DataManager.GetFoodIDList();
        float sum = 0;
        for (int i = 0; i < foodIds.Length; i++)
        {
            if (foodIds[i] != 11000)
            {
                sum += TryGetResourceNum(foodIds[i]);
            }
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

    public CostResource GetFoodByMax(float requestNum,bool ignoreStorage = false)
    {
        ItemData[] foods = DataManager.GetFoodItemList();
        float maxNum = 0;
        int p = 0;
        for (int i = 0; i < foods.Length; i++)
        {
            //在玩家设置的禁吃名单里则跳过
            if (IsInForbiddenFoodList(foods[i].Id))
            {
                continue;
            }
            float num = TryGetResourceNum(foods[i].Id);
            if (num > maxNum)
            {
                p = i;
                maxNum = num;
            }
        }
        if (!ignoreStorage && maxNum < requestNum)
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

    public static bool IsFood(int id)
    {
        if (id == 11000)
        {
            return true;
        }
        int[] foodIds = DataManager.GetFoodIDList();
        for (int i = 0; i < foodIds.Length; i++)
        {
            if(id == foodIds[i])
            {
                return true;
            }
        }
        return false;
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

    /// <summary>
    /// 重新统计人口信息
    /// </summary>
    public void RecalculatePopulation()
    {
        _workerPopulation = 0;
        _allPopulation = 0;
        var buildings = MapManager.Instance.GetAllBuildings();
        for (int i = 0; i < buildings.Count; i++)
        {
            var data = buildings[i].runtimeBuildData;
            if (data.CurPeople > 0)
            {
                _workerPopulation += data.CurPeople;
            }
            else
            {
                _allPopulation -= data.CurPeople;
            }
        }
        EventManager.TriggerEvent(ConstEvent.OnPopulationHudChange);
    }

    /// <summary>
    /// 占用人口
    /// </summary>
    /// <param name="num">申请的人口数量</param>
    /// <returns>实际提供下来的人口数量</returns>
    public int GetMaxWorkerRemain(int num)
    {
        int remain = _allPopulation - _workerPopulation;
        if (num > remain)
        {
            return remain;
        }
        else
        {
            return num;
        }
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
        return GetMaxStorage() - GetCurStorage();
    }

    public bool IsInHudList(int Id)
    {
        return _hudList.Contains(Id);
    }

    public void ToggleHudItem(ItemData data)
    {
        //如果在监视器窗口里则移出
        if (_hudList.Contains(data.Id))
        {
            _hudList.Remove(data.Id);
        }
        else
        {
            //不在则添加
            _hudList.Add(data.Id);
        }
        EventManager.TriggerEvent(ConstEvent.OnHudItemChange);
    }

    public bool IsInForbiddenFoodList(int Id)
    {
        return _forbiddenFoodList.Contains(Id);
    }

    public void ToggleForbiddenFood(int Id)
    {
        if (_forbiddenFoodList.Contains(Id))
        {
            _forbiddenFoodList.Remove(Id);
        }
        else
        {
            _forbiddenFoodList.Add(Id);
        }
    }
    
    
}

