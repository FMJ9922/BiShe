using UnityEngine;

namespace Building
{
    /// <summary>
    /// 建筑最基础功能
    /// </summary>
    public interface IBuildingBasic
    {
        /// <summary>
        /// 确认建造
        /// </summary>
        void OnConfirmBuild(Vector2Int[] vector2Ints);

        /// <summary>
        /// 播放动画
        /// </summary>
        void PlayAnim();

        /// <summary>
        /// 建造完后初始化建筑功能
        /// </summary>
        void InitBuildingFunction();
        
        /// <summary>
        /// 从存档中恢复已有建筑
        /// </summary>
        void RestartBuildingFunction();

        /// <summary>
        /// 摧毁建筑
        /// </summary>
        void DestroyBuilding(bool returnResources, bool returnPopulation, bool repaint = true);

        bool ReturnBuildResources();

        void FillUpPopulation();
        
        /// <summary>
        /// 升级
        /// </summary>
        void Upgrade(out bool issuccess, out BuildingBase buildingData);

        /// <summary>
        /// 获取建筑类型
        /// </summary>
        EBuildingType GetBuildingType();

    }

    /// <summary>
    /// 可居住的
    /// </summary>
    public interface IHabitable
    {
        /// <summary>
        /// 住房提供人口
        /// </summary>
        void ProvidePopulation();

        /// <summary>
        /// 移除住房提供的人口
        /// </summary>
        void RemovePopulation();

        /// <summary>
        /// 住房消耗食物
        /// </summary>
        void InputFood();
    }

    
    
    /// <summary>
    /// 生产建筑
    /// </summary>
    public interface IProduct
    {

        void Input();
        
        void Output();

        float GetProcess();

        void UpdateRate(string date);

        void UpdateEffectiveness();

        void ChangeFormula();
    }

    public interface ITransportation
    {
        /// <summary>
        /// 配置运货清单
        /// </summary>
        /// <param name="rate">运送多少个周期的货</param>
        /// <returns></returns>
        CarMission MakeCarMission(float rate);

        /// <summary>
        /// 收到车辆后的处理
        /// </summary>
        /// <param name="carMission"></param>
        void OnRecieveCar(CarMission carMission);
    }

    public interface IBridge
    {
        BridgeData GetBridgeData();
        
        void SetBridgeData(BridgeData bridgeData);
    }

    public interface IStorage
    {
        void AddResource(int id, float num);
        void AddResource(CostResource costResource);
        bool TryUseResource(int id, float num);
        bool TryUseResource(CostResource costResource);
        float TryGetResourceNum(int id);
        float GetAllFoodNum();
    }
}