using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeBuilding : BuildingBase
{
    BridgeData _bridgeData;
    public override void OnConfirmBuild(Vector2Int[] vector2Ints)
    {
        runtimeBuildData = new RuntimeBuildData();
        runtimeBuildData = CastBuildDataToRuntime(DataManager.GetBuildData(20036));
        runtimeBuildData.tabType = BuildTabType.bridge;
        runtimeBuildData.Id = 20036;
        gameObject.tag = "Building";
        takenGrids = vector2Ints;
        Initbridge();
        transform.GetComponent<BoxCollider>().enabled = false;
        transform.GetComponent<BoxCollider>().enabled = true;
        Invoke("SetBuildFlag", 1f);
    }

    public void Initbridge()
    {
        transform.position = _bridgeData.bridgePos.V3;
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.size = _bridgeData.size.V3;
        for (int i = 0; i < _bridgeData.gridsPos.Length; i++)
        {
            GameObject bridge = Instantiate(GetBridgePfb(_bridgeData.roadLevel), transform);
            bridge.transform.position = _bridgeData.gridsPos[i].V3;
            bridge.transform.rotation = Quaternion.LookRotation(_bridgeData.lookRotation.V3);
        }
    }
    public BridgeData GetBridgeData()
    {
        return _bridgeData;
    }
    private void SetBuildFlag()
    {
        buildFlag = true;
    }
    public override void DestroyBuilding(bool returnResources,bool returnPopulation,bool repaint = true)
    {
        MapManager.SetGridTypeToEmpty(takenGrids);
        MapManager.Instance.BuildOriginFoundation(takenGrids);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!buildFlag)
        {
            if (!other.CompareTag("Untagged"))
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
