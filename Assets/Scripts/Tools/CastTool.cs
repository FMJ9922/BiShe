using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastTool : MonoBehaviour
{
    //public static TechType castbuildingtypetotechtype(BuildingType buildingtype)
    //{
    //    int i = (int)buildingtype;
    //    return i switch
    //    {
    //        int agr when agr < 100 => TechType.Agriculture,
    //        int ind when ind < 200 => TechType.Industry,
    //        int man when man < 300 => TechType.Manufacturing,
    //        _ => TechType.Service
    //    };
    //}
    public static string RoundOrFloat(float num)
    {
        return Mathf.Approximately(num, Mathf.Round(num)) ?
            string.Format("{0}", Mathf.Round(num)) :
            string.Format("{0:F}", num);
    }
}
