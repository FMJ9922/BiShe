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
        graphIcon.sprite = LoadAB.LoadSprite("icon.ab", handle.activeInHierarchy ? "enNumberButton" : "NumberButton");
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
        List<BuildingBase> buildings = MapManager.Instance._buildings;
        for (int i = 0; i < buildings.Count; i++)
        {
            GameObject item = Instantiate(cubePfb, transform);
            BuildingBase building = buildings[i];
            item.GetComponent<GraphCube>().SetHeight(building.runtimeBuildData.CurPeople, buildings[i].transform.position);
            string showLabel = building.runtimeBuildData.tabType == BuildTabType.house ?
                Localization.ToSettingLanguage("Resident") : Localization.ToSettingLanguage("Worker");
            item.GetComponent<GraphCube>().SetLabel(building.runtimeBuildData.CurPeople.ToString() + " " + showLabel);
        }
    }

    private void ShowHappiness()
    {
        List<BuildingBase> buildings = MapManager.Instance._buildings;
        for (int i = 0; i < buildings.Count; i++)
        {
            GameObject item = Instantiate(cubePfb, transform);
            BuildingBase building = buildings[i];
            float rand = building.runtimeBuildData.Happiness;
            item.GetComponent<GraphCube>().SetHeight((int)(rand * 20), buildings[i].transform.position);
            string showLabel = Localization.ToSettingLanguage("%");
            item.GetComponent<GraphCube>().SetLabel((int)(rand * 100) + showLabel);
        }
    }
    private void ShowMaintenanceCosts()
    {
        List<BuildingBase> buildings = MapManager.Instance._buildings;
        for (int i = 0; i < buildings.Count; i++)
        {
            GameObject item = Instantiate(cubePfb, transform);
            BuildingBase building = buildings[i];
            int rand = building.runtimeBuildData.CostPerWeek;
            item.GetComponent<GraphCube>().SetHeight(Mathf.Abs(rand), buildings[i].transform.position);
            string showLabel = "/" + Localization.ToSettingLanguage("Week");
            item.GetComponent<GraphCube>().SetLabel((rand<0?"+":"-")+ Mathf.Abs(rand) + showLabel);
        }
    }

    /// <summary>
    /// 高斯分布概率模型
    /// </summary>
    /// <param name="_x">随机变量</param>
    /// <param name="_μ">位置参数</param>
    /// <param name="_σ">尺度参数</param>
    /// <returns></returns>
    private static float NormalDistribution(float _x, float _μ, float _σ)
    {
        float _inverseSqrt2PI = 1 / Mathf.Sqrt(2 * Mathf.PI);
        float _powOfE = -(Mathf.Pow((_x - _μ), 2) / (2 * _σ * _σ));
        float _result = (_inverseSqrt2PI / _σ) * Mathf.Exp(_powOfE);
        return _result;
    }

    public void ShowEffectiveness()
    {
        List<BuildingBase> buildings = MapManager.Instance._buildings;
        for (int i = 0; i < buildings.Count; i++)
        {
            GameObject item = Instantiate(cubePfb, transform);
            BuildingBase building = buildings[i];
            float rand = building.runtimeBuildData.Effectiveness;
            item.GetComponent<GraphCube>().SetHeight((int)(rand * 20), buildings[i].transform.position);
            string showLabel = Localization.ToSettingLanguage("%");
            item.GetComponent<GraphCube>().SetLabel((int)(rand * 100) + showLabel);
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
