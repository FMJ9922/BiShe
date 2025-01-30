using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;

public class TreePlanter : Singleton<TreePlanter>
{
    public GameObject[] TreePfbs;
    private float minDis = 2f;

    Transform Treeparent;

    private void Start()
    {
        Treeparent = TransformFinder.Instance.treeParent;
        EventManager.StartListening<Vector3>(ConstEvent.OnPlantSingleTree, PlantSingleUnInitTree);
        
    }
    private void OnDestroy()
    {
        EventManager.StopListening<Vector3>(ConstEvent.OnPlantSingleTree, PlantSingleUnInitTree);
    }
    public void PlantTree(Vector3 centerPos, float range, float density)
    {
        List<Vector3> treePosList = new List<Vector3>();
        int plantNum = (int)(range * range * density/10);
        for (int i = 0; i < plantNum; i++)
        {
            Vector3 pos;
            int counter = 0;
            do
            {
                counter++;
                pos = GetRandomPos(centerPos, range);
            }
            while (!IsDistanceOk(treePosList, pos) && counter < 10);
            if (counter < 10)
            {
                treePosList.Add(pos);
            }
        }
        //Debug.Log("种了" + treePosList.Count + "棵树");
        for (int i = 0; i < treePosList.Count; i++)
        {
            PlantSingleTree(treePosList[i]);
        }
    }
    public void PlantSingleUnInitTree(Vector3 centerPos)
    {
        List<Vector3> treePosList = new List<Vector3>();
        int plantNum = 1;
        for (int i = 0; i < plantNum; i++)
        {
            Vector3 pos;
            int counter = 0;
            do
            {
                counter++;
                pos = GetRandomPos(centerPos, 2f);
            }
            while (!IsDistanceOk(treePosList, pos) && counter < 10);
            if (counter < 10)
            {
                treePosList.Add(pos);
            }
        }
        //Debug.Log("种了" + treePosList.Count + "棵树");
        for (int i = 0; i < treePosList.Count; i++)
        {
            PlantSingleTree(treePosList[i],TreeState.unInit);
        }
    }
    private Vector3 GetRandomPos(Vector3 centerPos, float range)
    {
        float deltaX = Random.Range(-range, range);
        float deltaZ = Random.Range(-range, range);
        return centerPos + new Vector3(deltaX, 0, deltaZ);
    }
    private bool IsDistanceOk(List<Vector3> checkList, Vector3 checkPoint)
    {
        for (int i = 0; i < checkList.Count; i++)
        {
            if (Vector3.Distance(checkList[i], checkPoint) < minDis)
            {
                return false;
            }
        }
        return true;
    }
    public void PlantSingleTree(Vector3 centerPos,TreeState state = TreeState.mature)
    {
        Vector3 adjustedPos = MapManager.GetStaticTerrainPosition(centerPos);
        if (adjustedPos == Vector3.zero) return;
        if(Treeparent == null)
        {
            Treeparent = GameObject.Find("TreeGenerator").transform;
        }
        int max = TreePfbs.Length;
        int index = Random.Range(0, max);
        Vector3 forward = Random.onUnitSphere;
        forward = new Vector3((forward.x != 0 ? forward.x : 1), 0, forward.z);
        GameObject newTree = Instantiate(TreePfbs[index], adjustedPos, Quaternion.LookRotation(forward, Vector3.up), Treeparent);
        var sys = newTree.GetComponent<TreeSystem>();
        sys.treeData.state = state;
        sys.Init();
    }

    public void PlantSingleTree(TreeData treeSystem,Vector3 pos,Vector3 rotation)
    {
        GameObject newTree = Instantiate(TreePfbs[treeSystem.indexType], pos, Quaternion.Euler(rotation), transform);
        TreeSystem sys = newTree.GetComponent<TreeSystem>();
        newTree.transform.SetParent(transform);
        sys.SetData(treeSystem);
    }

    public void PlantSaveTrees(SaveData saveData)
    {
        for (int i = 0; i < saveData.treeData.Length; i++)
        {
            PlantSingleTree(saveData.treeData[i], saveData.treePosition[i].V3, saveData.treeRotation[i].V3);
        }
    }
}
