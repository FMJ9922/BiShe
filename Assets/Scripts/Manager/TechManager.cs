using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechManager : Singleton<TechManager>
{
    [SerializeField] TechInfoCanvas infoCanvas;
    [SerializeField] TechTreeCanvas treeCanvas;
    bool[] techs = new bool[33];
    bool[] techAvalible = new bool[33];
    int[] techUsing = new int[3];
    private int point = 1;
    private int techLighted = 0;

    public void OpenInfoCanvas(TechItem tech)
    {
        infoCanvas.ShowTechInfo(tech);
    }

    public void RefreshTechPoint()
    {
        point =  (int)(ResourceManager.Instance.MaxPopulation / 20) - techLighted;
        treeCanvas.ChangePoint(point);
    }

    public void OpenTech(TechItem tech,Action callBack)
    {
        if (point > 0 && CanUpgrade(tech))
        {
            techAvalible[tech.TechId - 40001] = true;
            techLighted++;
            RefreshTechPoint();
            callBack.Invoke();
        }
        else
        {
            if(point <= 0)
            {
                NoticeManager.Instance.InvokeShowNotice("科技点数不足");
            }
            else if (!CanUpgrade(tech))
            {
                NoticeManager.Instance.InvokeShowNotice("前置科技未解锁");
            }
        }
    }
    
    public bool CanUpgrade(TechItem item)
    {
        if (item.FrontItems.Length <= 0) return true;
        for (int i = 0; i < item.FrontItems.Length; i++)
        {
            if (!GetTechAvalible(item.FrontItems[i].TechId))
            {
                return false;
            }
        }
        return true;
    }
    public void SetTech(TechItem techItem)
    {
        techs[techItem.data.Id - 40001] = true;
        treeCanvas.SetImage(techItem.data.Group, techItem.transform.name.TrimEnd(' '));
        //Debug.Log("set:" + techItem.data.Id);
        int oldTech = techUsing[techItem.data.Group];
        if (oldTech != 0)
        {
            techs[oldTech-40001] = false;
        }
        techUsing[techItem.data.Group] = techItem.data.Id;
    }

    public bool GetTech(int techID)
    {
        //Debug.Log("get:" + techID);
        return techs[techID - 40001];
    }
    public bool GetTechAvalible(int techID)
    {
        return techAvalible[techID - 40001];
    }
    private int Int(bool value)
    {
        return value ? 1 : 0;
    }

    private int Float(bool value)
    {
        return value ? 1 : 0;
    }
    /// <summary>
    /// 工人数量
    /// </summary>
    /// <returns></returns>
    public int PopulationBuff()
    {
        return -1 * Int(GetTech(40023)) + -2 * Int(GetTech(40028)) + -3 * Int(GetTech(40031));
    }

    /// <summary>
    /// 生产效率
    /// </summary>
    /// <returns></returns>
    public float EffectivenessBuff()
    {
        return 1 + Float(GetTech(40025)) * 0.1f +
                Float(GetTech(40029)) * 0.2f +
                Float(GetTech(40033)) * 0.3f +
                Float(GetTech(40024)) * 0.05f +
                Float(GetTech(40027)) * 0.1f +
                Float(GetTech(40032)) * 0.15f;
    }

    /// <summary>
    /// 产品价格
    /// </summary>
    /// <returns></returns>
    public float PriceBuff()
    {
        return 1 + Float(GetTech(40012)) * 0.03f +
                Float(GetTech(40016)) * 0.06f +
                Float(GetTech(40019)) * 0.09f +
                Float(GetTech(40015)) * -0.05f +
                Float(GetTech(40020)) * -0.10f +
                Float(GetTech(40012)) * 0.10f +
                Float(GetTech(40016)) * 0.20f +
                Float(GetTech(40019)) * 0.30f;
    }

    /// <summary>
    /// 出售数量上限
    /// </summary>
    /// <returns></returns>
    public float SellNumBuff()
    {
        return 1+ Float(GetTech(40015)) * 0.3f +
                Float(GetTech(40020)) * 0.5f +
                Float(GetTech(40014)) * -0.1f +
                Float(GetTech(40017)) * -0.15f +
                Float(GetTech(40021)) * -0.2f +
                Float(GetTech(40013)) * 0.05f +
                Float(GetTech(40018)) * 0.10f +
                Float(GetTech(40022)) * 0.15f;
    }
    
    /// <summary>
    /// 生产所需原料
    /// </summary>
    /// <returns></returns>
    public float ResourcesBuff()
    {
        return 1 + Float(GetTech(40026)) * -0.1f +
                Float(GetTech(40030)) * -0.2f +
                Float(GetTech(40024)) * -0.05f +
                Float(GetTech(40027)) * -0.10f +
                Float(GetTech(40032)) * -0.15f;
    }

    /// <summary>
    /// 交通费用
    /// </summary>
    /// <returns></returns>
    public float TransportBuff()
    {
        return 1 + Float(GetTech(40002)) * -0.05f +
                Float(GetTech(40006)) * -0.10f +
                Float(GetTech(40011)) * -0.15f;
    }

    /// <summary>
    /// 维护费用
    /// </summary>
    /// <returns></returns>
    public float MaintenanceCostBuff()
    {
        return 1 + Float(GetTech(40003)) * -0.05f +
                Float(GetTech(40009)) * -0.10f;
    }

    /// <summary>
    /// 建造原料花费
    /// </summary>
    /// <returns></returns>
    public float BuildResourcesBuff()
    {
        return 1 + Float(GetTech(40004)) * -0.1f +
                Float(GetTech(40007)) * -0.15f +
                Float(GetTech(40010)) * -0.2f;
    }

    public float BuildPriceBuff()
    {
        return 1 + Float(GetTech(40001)) * -0.1f +
                Float(GetTech(40005)) * -0.15f +
                Float(GetTech(40008)) * -0.2f;
    }
}
