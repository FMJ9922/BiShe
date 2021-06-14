using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 建筑附属仓库
/// </summary>
public class Storage:MonoBehaviour
{
    private Dictionary<int, float> _storedItemDic = new Dictionary<int, float>();


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
    }
    public void AddResource(CostResource costResource)
    {
        if (_storedItemDic.ContainsKey(costResource.ItemId))
        {
            _storedItemDic[costResource.ItemId] += costResource.ItemNum;
        }
        else
        {
            _storedItemDic.Add(costResource.ItemId, costResource.ItemNum);
        }
    }

    /// <summary>
    /// 从仓库取物品
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
                //Debug.Log("消耗了" + Id + num + "个");
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
            //Debug.Log("不存在物品：" + Id);
            return false;//不存在该物品就返回失败
        }
    }
    public Dictionary<int, float> GetAllResource()
    {
        return _storedItemDic;
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
}
