using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeBuilding : BuildingBase
{
    public override void OnConfirmBuild(Vector2Int[] vector2Ints)
    {
        runtimeBuildData = new RuntimeBuildData();
        runtimeBuildData = CastBuildDataToRuntime(DataManager.GetBuildData(20036));
        runtimeBuildData.tabType = BuildTabType.bridge;
        transform.GetComponent<BoxCollider>().enabled = false;
        transform.GetComponent<BoxCollider>().enabled = true;
        runtimeBuildData.Id = 20036;
        gameObject.tag = "Building";
        takenGrids = vector2Ints;
        Invoke("SetBuildFlag", 1f);
    }

    private void SetBuildFlag()
    {
        buildFlag = true;
    }
    public override void DestroyBuilding(bool returnResources,bool returnPopulation,bool repaint = true)
    {
        MapManager.SetGridTypeToEmpty(takenGrids);
        MapManager.Instance.BuildFoundation(takenGrids, 1, 4);
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
}
