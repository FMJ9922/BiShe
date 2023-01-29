using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Building
{
    public class BridgeBuilding : BuildingBase,IBuildingBasic,IBridge
    {
        BridgeData _bridgeData;
    
        #region IBuildingBasic
        public void OnConfirmBuild(Vector2Int[] vector2Ints)
        {
            runtimeBuildData = new RuntimeBuildData();
            runtimeBuildData = CastTool.CastBuildDataToRuntime(DataManager.GetBuildData(20036));
            runtimeBuildData.tabType = BuildTabType.bridge;
            runtimeBuildData.Id = 20036;
            gameObject.tag = "Bridge";
            takenGrids = vector2Ints;
            transform.GetComponent<BoxCollider>().enabled = false;
            transform.GetComponent<BoxCollider>().isTrigger = true;
            transform.GetComponent<BoxCollider>().enabled = true;
            buildFlag = false;
            Invoke("SetBuildFlag", 1f);
        }

        //桥梁目前还没有动画
        public void PlayAnim()
        {
        
        }

        public void InitBuildingFunction()
        {
            transform.position = _bridgeData.bridgePos.V3;
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            Rigidbody rg = gameObject.AddComponent<Rigidbody>();
            rg.isKinematic = true;
            col.size = _bridgeData.size.V3;
            for (int i = 0; i < _bridgeData.gridsPos.Length; i++)
            {
                GameObject bridge = Instantiate(GetBridgePfb(_bridgeData.roadLevel), transform);
                bridge.transform.position = _bridgeData.gridsPos[i].V3;
                bridge.transform.rotation = Quaternion.LookRotation(_bridgeData.lookRotation.V3);
            }
        }

        public void RestartBuildingFunction()
        {
        
        }
    
        #endregion IBuildingBasic
    
        #region IBridge
        public BridgeData GetBridgeData()
        {
            return _bridgeData;
        }
        #endregion
    
        private void SetBuildFlag()
        {
            buildFlag = true;
        }
        public void DestroyBuilding(bool returnResources,bool returnPopulation,bool repaint = true)
        {
            MapManager.SetGridTypeToEmpty(takenGrids);
            MapManager.Instance.BuildOriginFoundation(takenGrids);
            Destroy(this.gameObject);
        }

        public bool ReturnBuildResources()
        {
            List<CostResource> rescources = runtimeBuildData.costResources;
            for (int i = 0; i < rescources.Count; i++)
            {
                rescources[i].ItemNum *= 0.75f;
                ResourceManager.Instance.AddResource(rescources[i]);
            }
            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (buildFlag)
            {
                if (other.CompareTag("Bridge"))
                {
                    Destroy(this.gameObject);
                }
            }
        }

        public void SetBridgeData(BridgeData bridgeData)
        {
            _bridgeData = bridgeData;
        }

        private GameObject GetBridgePfb(int level)
        {
            return LoadAB.Load("building.ab", string.Format("Universal_Building_Bridge_L{0}_01_Preb", level));
        }

    }

    [System.Serializable]
    public class BridgeData
    {
        public Vector3Serializer bridgePos;
        public Vector3Serializer[] gridsPos;
        public Vector2IntSerializer[] takenGrids; 
        public Vector3Serializer lookRotation;
        public Vector3Serializer size;
        public int roadLevel;
    }
}