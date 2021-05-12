using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : Singleton<GraphManager>
{
    GameObject cubePfb;
    [SerializeField] GameObject subCamera;
    [SerializeField] GameObject handle;
    [SerializeField] Image graphIcon;
    private bool[] openState = new bool[4] { false, false, false, false };
    protected override void InstanceAwake()
    {
    }


    public void Toggle()
    {
        handle.SetActive(!handle.activeInHierarchy);
        graphIcon.sprite = LoadAB.LoadSprite("icon.ab", handle.activeInHierarchy ?"enNumberButton":"NumberButton");
        if (!handle.activeInHierarchy)
        {
            CloseAllGraph();
        }
    }

    public void ShowGraph(int type)
    {
        CleanUpAllAttachedChildren(transform);
        if (openState[type])
        {
            CloseGraph();
            openState[type] = false;
        }
        else
        {
            ResetState();
            openState[type] = true;
            OpenGraph();
            switch (type)
            {
                case 0:
                    ShowPopulation();
                    break;
                case 1:
                    ShowHappiness();
                    break;
                case 2:
                    ShowMaintenanceCosts();
                    break;
                case 3:
                    ShowEffectiveness();
                    break;
            }
        }
    }
    private void ShowPopulation()
    {
        List<GameObject> buildings = MapManager.Instance._buildings;
        for (int i = 0; i < buildings.Count; i++)
        {
            GameObject item = Instantiate(cubePfb, transform);
            BuildingBase building = buildings[i].GetComponent<BuildingBase>();
            item.GetComponent<GraphCube>().SetHeight(building.runtimeBuildData.CurPeople, buildings[i].transform.position);
            string showLabel = building.runtimeBuildData.tabType == BuildTabType.house ?
                Localization.ToSettingLanguage("Resident") : Localization.ToSettingLanguage("Worker");
            item.GetComponent<GraphCube>().SetLabel(building.runtimeBuildData.CurPeople.ToString() + " " + showLabel);
        }
    }

    private void ShowHappiness()
    {
        List<GameObject> buildings = MapManager.Instance._buildings;
        for (int i = 0; i < buildings.Count; i++)
        {
            GameObject item = Instantiate(cubePfb, transform);
            BuildingBase building = buildings[i].GetComponent<BuildingBase>();
            item.GetComponent<GraphCube>().SetHeight((int)(building.runtimeBuildData.Happiness * 20), buildings[i].transform.position);
            string showLabel = Localization.ToSettingLanguage("%");
            item.GetComponent<GraphCube>().SetLabel((int)(building.runtimeBuildData.Happiness * 100) + showLabel);
        }
    }
    private void ShowMaintenanceCosts()
    {
        List<GameObject> buildings = MapManager.Instance._buildings;
        for (int i = 0; i < buildings.Count; i++)
        {
            GameObject item = Instantiate(cubePfb, transform);
            BuildingBase building = buildings[i].GetComponent<BuildingBase>();
            item.GetComponent<GraphCube>().SetHeight(building.runtimeBuildData.CostPerWeek, buildings[i].transform.position);
            string showLabel = "/" + Localization.ToSettingLanguage("Week");
            item.GetComponent<GraphCube>().SetLabel(building.runtimeBuildData.CostPerWeek.ToString() + showLabel);
        }
    }

    public void ShowEffectiveness()
    {
        List<GameObject> buildings = MapManager.Instance._buildings;
        for (int i = 0; i < buildings.Count; i++)
        {
            GameObject item = Instantiate(cubePfb, transform);
            BuildingBase building = buildings[i].GetComponent<BuildingBase>();
            item.GetComponent<GraphCube>().SetHeight((int)(building.runtimeBuildData.Effectiveness*20), buildings[i].transform.position);
            string showLabel = Localization.ToSettingLanguage("%");
            item.GetComponent<GraphCube>().SetLabel((int)(building.runtimeBuildData.Effectiveness * 100) + showLabel);
        }
    }


    private void OpenGraph()
    {
        cubePfb = LoadAB.Load("building.ab", "GraphPfb");
        Camera.main.GetComponent<PostEffect>().isStart = true;
        subCamera.SetActive(true);
    }

    private void CloseGraph()
    {
        Camera.main.GetComponent<PostEffect>().isStart = false;
        subCamera.SetActive(false);
    }

    public void CloseAllGraph()
    {
        ResetState();
        CloseGraph();
    }

    private void ResetState()
    {
        for (int i = 0; i < openState.Length; i++)
        {
            openState[i] = false;
        }
    }
    private void CleanUpAllAttachedChildren(Transform target)
    {
        for (int i = 0; i < target.childCount; i++)
        {
            Destroy(target.GetChild(i).gameObject);
        }

    }
}
