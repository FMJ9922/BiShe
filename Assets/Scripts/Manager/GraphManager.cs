using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphManager : Singleton<GraphManager>
{
    GameObject cubePfb;
    [SerializeField] GameObject subCamera;
    [SerializeField] GameObject handle;

    protected override void InstanceAwake()
    {
    }


    public void Toggle()
    {
        handle.SetActive(!handle.activeInHierarchy);
        if (!handle.activeInHierarchy)
        {
            CloseGraph();
        }
    }
    public void ShowPopulation()
    {
        CleanUpAllAttachedChildren(transform);
        OpenGraph();
        List<GameObject> buildings = MapManager.Instance._buildings;
        for (int i = 0; i < buildings.Count; i++)
        {
            GameObject item = Instantiate(cubePfb, transform);
            BuildingBase building = buildings[i].GetComponent<BuildingBase>();
            item.GetComponent<GraphCube>().SetHeight(building.runtimeBuildData.CurPeople, buildings[i].transform.position);
            item.GetComponent<GraphCube>().SetLabel(building.runtimeBuildData.CurPeople);
        }
    }

    public void OpenGraph()
    {
        cubePfb = LoadAB.Load("building.ab", "GraphPfb");
        Camera.main.GetComponent<PostEffect>().isStart = true;
        subCamera.SetActive(true);
    }

    public void CloseGraph()
    {
        Camera.main.GetComponent<PostEffect>().isStart = false;
        subCamera.SetActive(false);
    }
    private void CleanUpAllAttachedChildren(Transform target)
    {
        for (int i = 0; i < target.childCount; i++)
        {
            Destroy(target.GetChild(i).gameObject);
        }

    }
}
