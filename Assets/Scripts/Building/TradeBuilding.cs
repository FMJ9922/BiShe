using System.Collections.Generic;
using CSTools;
using UnityEngine;
namespace Building
{
    public class TradeBuilding : BuildingBase,IBuildingBasic,ITransportation
    {
        public void OnConfirmBuild(Vector2Int[] vector2Ints)
        {
            takenGrids = vector2Ints;
            gameObject.tag = "Building";
            transform.GetComponent<BoxCollider>().enabled = false;
            transform.GetComponent<BoxCollider>().enabled = true;
            if (!buildFlag)
            {
                buildFlag = true;
                runtimeBuildData.direction = CastTool.CastVector3ToDirection(transform.right);
                InitBuildingFunction();
            }
            else
            {
                RestartBuildingFunction();
            }
        }

        public void PlayAnim()
        {
            
        }

        public void InitBuildingFunction()
        {
            parkingGridIn = BuildingTools.GetInParkingGrid(this);
            MapManager.Instance.AddBuilding(this);
            MapManager.Instance.AddBuildingEntry(parkingGridIn, this);
        }

        public void RestartBuildingFunction()
        {
            parkingGridIn = BuildingTools.GetInParkingGrid(this);
            MapManager.Instance.AddBuilding(this);
            MapManager.Instance.AddBuildingEntry(parkingGridIn, this);
        }

        public void DestroyBuilding(bool returnResources, bool returnPopulation, bool repaint = true)
        {
            MapManager.Instance.RemoveBuilding(this);
            MapManager.Instance.RemoveBuildingEntry(parkingGridIn);
            Destroy(this.gameObject);
        }

        public bool ReturnBuildResources()
        {
            return false;
        }

        public void FillUpPopulation()
        {
            
        }

        public void Upgrade(out bool issuccess, out BuildingBase buildingData)
        {
            issuccess = false;
            buildingData = this;
        }

        public EBuildingType GetBuildingType()
        {
            return EBuildingType.TradeBuilding;
        }

        public CarMission MakeCarMission(float rate)
        {
            return null;
        }

        public void OnRecieveCar(CarMission carMission)
        {
            if (carMission == null)
            {
                return;
            }

            switch (carMission.missionType)
            {
                case CarMissionType.goForOrder:
                    var orderData = MarketManager.Instance.GetRuntimeOrderData(carMission.orderIndex);
                    float transportedNum = carMission.transportResources[0].ItemNum;
                    orderData.PromiseTransportGoodsNum -= transportedNum;
                    orderData.HasTransportGoodsNum += transportedNum;
                    var newMission = BuildBackFromOrderCarMission(carMission);
                    if (newMission != null)
                    {
                        TrafficManager.Instance.UseCar(newMission);
                    }
                    break;
            }
        }

        private CarMission BuildBackFromOrderCarMission(CarMission carMission)
        {
            var ret = new CarMission();
            ret.StartBuilding = parkingGridIn;
            ret.missionType = CarMissionType.backFromOrder;
            ret.EndBuilding = carMission.BelongToBuilding;
            ret.orderIndex = carMission.orderIndex;
            ret.transportationType = carMission.transportationType;
            return ret;
        }

        public CarMission BuildBuyOrderCarMission(RuntimeOrderData orderData)
        {
            var ret = new CarMission();
            ret.StartBuilding = parkingGridIn;
            ret.missionType = CarMissionType.transportResources;
            ret.EndBuilding = MapManager.Instance.GetEnoughStorageBuilding(orderData.PromiseTransportGoodsNum,parkingGridIn).parkingGridIn;
            ret.orderIndex = (int)orderData.Index;
            ret.transportationType = TransportationType.van;
            float maxTransportNum = DataManager.GetCarData(ret.transportationType).Storage;
            float remainNum = orderData.PromiseTransportGoodsNum - orderData.HasTransportGoodsNum;
            float realTransportNum = maxTransportNum > remainNum ? remainNum : maxTransportNum;
            orderData.HasTransportGoodsNum += realTransportNum;
            ret.transportResources = new List<CostResource>()
                {new CostResource(orderData.OrderId, realTransportNum)};
            return ret;
        }
    }
}
