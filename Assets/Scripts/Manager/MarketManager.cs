using System;
using System.Collections;
using System.Collections.Generic;
using CSTools;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class MarketManager : Singleton<MarketManager>
{
    private float[] _orderAppearValueArray;// 订单出现概率数组

    private List<int> _currOrderIds = new List<int>();//这次随机出来的订单

    private List<RuntimeOrderData> _havingOrders = new List<RuntimeOrderData>();//正在进行中的订单合同

    public long OrderIndex;
    public void InitMarketManager()
    {
        _orderAppearValueArray = new float[DataManager.GetOrderLength()];
        OrderIndex = 0;
    }
    public void InitSavedMarketManager(SaveData saveData)
    {
        _orderAppearValueArray = new float[DataManager.GetOrderLength()];
        OrderIndex = saveData.OrderIndex;
        if (saveData.OrderAppearValueArray != null)
        {
            for (int i = 0; i < saveData.OrderAppearValueArray.Length; i++)
            {
                _orderAppearValueArray[i] = saveData.OrderAppearValueArray[i];
            }
        }

        if (saveData.runtimeOrderDatas != null)
        {
            _havingOrders.AddRange(saveData.runtimeOrderDatas);
        }
    }

    public float[] GetOrderAppearValueArray()
    {
        return _orderAppearValueArray;
    }

    private void OnEnable()
    {
        EventManager.StartListening(ConstEvent.OnRefreshResources,CheckMarketBuildingsCanDoOrder);
    }

    private void OnDisable()
    {
        EventManager.StopListening(ConstEvent.OnRefreshResources,CheckMarketBuildingsCanDoOrder);
    }


    public List<CostResource> GetDeltaNum()
    {
        return null;
    }

    public List<int> GetOrderList()
    {
        return _currOrderIds;
    }

    public void RemoveFromOrderList(int orderId)
    {
        _currOrderIds.Remove(orderId);
    }

    /// <summary>
    /// 根据订单出现概率，随机抽取相应数量的订单
    /// </summary>
    public void GenerateOrder()
    {
        _currOrderIds.Clear();
        var dataArray = DataManager.Instance.OrderArray;
        float max = 0;
        int curLevel = LevelManager.LevelID;
        for (int i = 0; i < dataArray.Length; i++)
        {
            var itemData = dataArray[i];
            int checkItemId = itemData.CheckItemId;
            bool isResourcesEnough = ResourceManager.Instance.IsResourceEnough(checkItemId, itemData.ItemNum);
            if (isResourcesEnough && (itemData.LevelLimit == 0 || curLevel == itemData.LevelLimit))
            {
                int index = itemData.ID - 60001;
                float appearValue =  Mathf.Clamp(_orderAppearValueArray[index], itemData.StartRate, itemData.MaxRate);
                max += appearValue;
                _orderAppearValueArray[index] = appearValue;
            }
        }

        int orderNum = GetOrderNum();
        float[] targetValues = new float[orderNum];
        for (int i = 0; i < orderNum; i++)
        {
            targetValues[i] = Random.value * max;
        }
        
        for (int i = 0; i < _orderAppearValueArray.Length; i++)
        {
            for (int j = 0; j < targetValues.Length; j++)
            {
                if (targetValues[j] < 0)
                {
                    continue;
                }

                targetValues[j] -= _orderAppearValueArray[i];
                if (targetValues[j] < 0)
                {
                    _currOrderIds.Add(i + 60001);
                }
            }
        }
    }

    public int GetOrderNum()
    {
        //todo
        return 3;
    }

    public void AddOrderAppearRate(int orderId, float addValue = 1)
    {
        int index = orderId - 60001;
        if (index < _orderAppearValueArray.Length)
        {
            var orderData = DataManager.GetOrderData(orderId);
            var ordVal = _orderAppearValueArray[index];
            _orderAppearValueArray[index] = Mathf.Clamp(ordVal + addValue,
                orderData.StartRate, orderData.MaxRate);
            if (orderData.AddRateTarget != null)
            {
                for (int i = 0; i < orderData.AddRateTarget.Count; i++)
                {
                    int subIndex = orderData.AddRateTarget[i] - 60001;
                    var oldValue = _orderAppearValueArray[subIndex];
                    var targetOrderData = DataManager.GetOrderData(orderData.AddRateTarget[i]);
                    _orderAppearValueArray[subIndex] = Mathf.Clamp(oldValue + addValue,
                        targetOrderData.StartRate, targetOrderData.MaxRate);
                    
                }
            }
        }
    }

    /// <summary>
    /// 获取某种订单的声望率
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    public float GetOrderAppearValue(int orderId)
    {
        var data = DataManager.GetOrderData(orderId);
        float min = data.StartRate;
        float max = data.MaxRate;
        float curRate = _orderAppearValueArray[orderId - 60001];
        return Mathf.Clamp01((curRate - min) / (max - min));
    }

    public void AddSellOrder(int orderId)
    {
        OrderIndex++;
        RuntimeOrderData orderData = new RuntimeOrderData();
        orderData.Index = OrderIndex;
        orderData.IsBuy = false;
        orderData.OrderId = orderId;
        orderData.StartWeek = LevelManager.Instance.WeekIndex;
        orderData.IsRepeating = false;
        orderData.HasTransportGoodsNum = 0;
        orderData.RunningThisOrderCar = new List<int>();
        _havingOrders.Add(orderData);
    }

    public void AddBuyOrder(int itemId,float buyNum,bool isRepeating)
    {
        OrderIndex++;
        RuntimeOrderData orderData = new RuntimeOrderData();
        orderData.Index = OrderIndex;
        orderData.IsBuy = true;
        orderData.OrderId = itemId;
        orderData.StartWeek = LevelManager.Instance.WeekIndex;
        orderData.IsRepeating = isRepeating;
        orderData.HasTransportGoodsNum = 0;
        orderData.PromiseTransportGoodsNum = buyNum;
        _havingOrders.Add(orderData);
    }

    public List<RuntimeOrderData> GetRuntimeOrderDatas()
    {
        return _havingOrders;
    }

    public RuntimeOrderData GetRuntimeOrderData(int index)
    {
        Debug.Log("get " + index);
        for (int i = 0; i < _havingOrders.Count; i++)
        {
            if (_havingOrders[i].Index == index)
            {
                return _havingOrders[i];
            }
        }

        return null;
    }

    public void RemoveRuntimeOrderData(long index)
    {
        //Debug.Log("remove " + index);
        for (int i = 0; i < _havingOrders.Count; i++)
        {
            if (_havingOrders[i].Index == index)
            {
                _havingOrders.RemoveAt(i);
                return;
            }
        }
        
    }

    public void CheckRuntimeOrderDatas()
    {
        
        
    }


    private void CheckMarketBuildingsCanDoOrder()
    {
        //如果没有订单，则不用派发
        if (_havingOrders.Count <= 0)
        {
            return;
        }
        //对于所有的订单
        for (int i = _havingOrders.Count - 1; i >=0 ; i--)
        {
            var orderData = _havingOrders[i];
            //如果是购买的话
            if (orderData.IsBuy)
            {
                //如果货物还没有交付完全
                if (orderData.PromiseTransportGoodsNum > orderData.HasTransportGoodsNum)
                {
                    var tradeBuilding = MapManager.Instance.GetTradeBuilding();
                    var carMission = tradeBuilding.BuildBuyOrderCarMission(orderData);
                    if (carMission != null)
                    {
                        TrafficManager.Instance.UseCar(carMission);
                    }
                    return;
                }
                //如果是每月一次的订单
                if (orderData.IsRepeating)
                {
                    if (orderData.StartWeek <= LevelManager.Instance.WeekIndex - 4)
                    {
                        orderData.StartWeek = LevelManager.Instance.WeekIndex;
                        var itemData = DataManager.GetItemDataById(orderData.OrderId);
                        float needMoney = 1.5f * orderData.PromiseTransportGoodsNum * itemData.Price;
                        if (ResourceManager.Instance.IsResourceEnough(99999, needMoney))
                        {
                            orderData.HasTransportGoodsNum = 0;
                            ResourceManager.Instance.UseResources(99999,needMoney);
                        }
                    }
                    return;
                }
                //一次性订单从此处删除
                RemoveRuntimeOrderData(orderData.Index);
            }
            else//如果是出售订单则走出售逻辑
            {
                var configData = DataManager.GetOrderData(orderData.OrderId);
                int limitTime = orderData.IsRepeating ? configData.RepeatTime : configData.TimeLimit;
                //如果货物已经全部运输完成，则直接结算
                if (orderData.HasTransportGoodsNum >= configData.ItemNum && !orderData.HasFinish)
                {
                    var itemData = DataManager.GetItemDataById(configData.ItemId);
                    //增加的钱 = 已完成运输物品数量 * 物品基础价格 * 合同规定价格系数
                    var money = orderData.HasTransportGoodsNum * itemData.Price * configData.GoodsPrice;
                    ResourceManager.Instance.AddMoney(money);
                    Debug.Log("加钱"+ money);
                    //一次性的订单需要移除
                    if (configData.Type == 0)
                    {
                        RemoveRuntimeOrderData(orderData.Index);
                    }
                    //如果按时完成订单，给予正常声望奖励
                    AddOrderAppearRate(configData.ID, 1);
                    orderData.HasFinish = true;
                    continue;
                }
                //如果订单到期
                if (orderData.StartWeek + limitTime - LevelManager.Instance.WeekIndex <= 0)
                {
                    if (!orderData.HasFinish)
                    {
                        var itemData = DataManager.GetItemDataById(configData.ItemId);
                        //增加的钱 = 已完成运输物品数量 * 物品基础价格 * 合同规定价格系数
                        ResourceManager.Instance.AddMoney(orderData.HasTransportGoodsNum * itemData.Price *
                                                          configData.GoodsPrice);
                        //一次性的订单需要移除
                        if (configData.Type == 0)
                        {
                            RemoveRuntimeOrderData(orderData.Index);
                        }

                        //如果拖到最后一周数量不足交货，则扣除声望
                        if (orderData.HasTransportGoodsNum >= configData.ItemNum)
                        {
                            AddOrderAppearRate(configData.ID, 1);
                        }
                        else
                        {
                            AddOrderAppearRate(configData.ID, -5f);
                        }
                    }
                    
                    
                    //如果是已经付过钱且持续性的订单，需要重置
                    if (configData.Type == 1)
                    {
                        ResetRepeatOrder(orderData);
                    }

                    continue;
                    
                }
                //如果订单已经确认准备运输的数量和已交付的数量大于所需数量
                if (orderData.HasTransportGoodsNum + orderData.PromiseTransportGoodsNum >= configData.ItemNum)
                {
                    continue;
                }
                //进入运货环节
                //查找一个有空余运力的市场
                var marketBuildings = MapManager.Instance.GetAllBuildings(EBuildingType.MarketBuilding);
                for (int j = 0; j < marketBuildings.Count; j++)
                {
                    var mb = marketBuildings[j] as MarketBuilding;
                    if (mb !=null && mb.HasTransportCapability())
                    {
                        mb.StartOrder(orderData);
                        break;
                    }
                } 
            }
        }
        EventManager.TriggerEvent(ConstEvent.OnMarketOrderDealing);
        
    }

    private void ResetRepeatOrder(RuntimeOrderData orderData)
    {
        Debug.Log("订单重置！");
        orderData.StartWeek = LevelManager.Instance.WeekIndex;
        orderData.HasTransportGoodsNum = 0;
        orderData.PromiseTransportGoodsNum = 0;
        orderData.RunningThisOrderCar.Clear();
        orderData.IsRepeating = true;
        orderData.HasFinish = false;
    }

}


