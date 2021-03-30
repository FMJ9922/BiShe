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

    private GameObject[] grids;
    public override void InitBuildingFunction()
    {
        base.InitBuildingFunction();
        InitWheatGrids();
    }
     private void InitWheatGrids()
    {
        grids = new GameObject[Size.x * Size.y];
        for (int i = 0; i < Size.x * Size.y; i++)
        {
            GameObject newGrid = Instantiate(wheatGridPfb, wheatTrans);
            grids[i] = newGrid;
            newGrid.transform.position = MapManager.Instance.GetTerrainPosition(takenGrids[i]);
            wheatGridPfb.SetActive(false);
        }
        wheatTrans.localPosition = new Vector3(0, -0.8f, 0);
    }
    protected override void Output()
    {
        if (formula == null) return;
        productTime--;
        float progress = (float)productTime / formula.ProductTime;
        wheatTrans.localPosition = new Vector3(0, -0.7f * progress, 0);
        if (productTime <= 0)
        {
            productTime = formula.ProductTime;
            for (int i = 0; i < formula.OutputItemID.Count; i++)
            {
                ResourceManager.Instance.AddResource(formula.OutputItemID[i], formula.ProductNum[i]);
            }
        }

    }

    protected override void Input()
    {
        base.Input();
    }

}
