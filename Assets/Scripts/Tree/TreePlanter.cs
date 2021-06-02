using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePlanter : Singleton<MonoBehaviour>
{
    public GameObject[] TreePfbs;
    private float minDis = 2f;

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
        Debug.Log("种了" + treePosList.Count + "棵树");
        for (int i = 0; i < treePosList.Count; i++)
        {
            PlantSingleTree(treePosList[i]);
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
    public void PlantSingleTree(Vector3 centerPos, TreeSystem.TreeState state = TreeSystem.TreeState.mature)
    {
        Vector3 adjustedPos = MapManager.GetTerrainPosition(centerPos);
        if (adjustedPos == Vector3.zero) return;
        int max = TreePfbs.Length;
        int index = Random.Range(0, max);
        Vector3 forward = Random.onUnitSphere;
        forward = new Vector3((forward.x != 0 ? forward.x : 1), 0, forward.z);
        Transform parent = GameObject.Find("TerrainGenerator").transform.GetChild(0);
        GameObject newTree = Instantiate(TreePfbs[index], adjustedPos, Quaternion.LookRotation(forward, Vector3.up), parent);
        newTree.GetComponent<TreeSystem>().state = state;
    }
}
