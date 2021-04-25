using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmLandBuilding : BuildingBase
{
    public Color yellow;
    public Color green;
    [SerializeField] GameObject wheatGridPfb;
    [SerializeField] Transform wheatTrans;
    [SerializeField] GameObject previewObj;
    public List<GameObject> lists;
    bool isharvesting = false;

    private GameObject[] grids;
    public override void InitBuildingFunction()
    {
        InitWheatGrids();
        previewObj.SetActive(false);
        base.InitBuildingFunction();
    }
     private void InitWheatGrids()
    {
        grids = new GameObject[Size.x * Size.y];
        for (int i = 0; i < Size.x * Size.y; i++)
        {
            GameObject newGrid = Instantiate(wheatGridPfb, wheatTrans);
            grids[i] = newGrid;
            newGrid.transform.position = MapManager.GetTerrainPosition(takenGrids[i]);
            newGrid.transform.Rotate(Vector3.up, 90 * (int)(direction-1), Space.Self);
        }
        wheatTrans.localPosition = new Vector3(0, -0.8f, 0);
    }
    protected override void Output()
    {
        if (formula == null) return;
        if (!isharvesting)
        {
            productTime--;
            float progress = (float)productTime / formula.ProductTime;
            wheatTrans.localPosition = new Vector3(0, -0.7f * progress, 0);
            if (productTime <= 0)
            {
                for (int i = 0; i < formula.OutputItemID.Count; i++)
                {
                    ResourceManager.Instance.AddResource(formula.OutputItemID[i], formula.ProductNum[i]);
                }
                isharvesting = true;
                List<Vector3> vecs = new List<Vector3>();
                for (int i = 0; i < lists.Count; i++)
                {
                    vecs.Add(lists[i].transform.position);
                }
                TrafficManager.Instance.UseCar(TransportationType.harvester, vecs, DriveType.once, OnFinishHarvest);
            }
        }
    }
    public void OnFinishHarvest()
    {
        isharvesting = false;
        productTime = formula.ProductTime;
        InitWheatGrids();
    }

    protected override void Input()
    {
        base.Input();
    }

}
