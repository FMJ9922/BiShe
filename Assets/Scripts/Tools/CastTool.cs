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

    /// <summary>
    /// Direction->Vector3
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static Vector3 CastDirectionToVector(Direction direction)
    {
        int unit = MapManager.unit;
        switch (direction)
        {
            case Direction.right:
                return new Vector3(1, 0, 0) * unit;
            case Direction.left:
                return new Vector3(-1, 0, 0) * unit;
            case Direction.up:
                return new Vector3(0, 0, 1) * unit;
            case Direction.down:
                return new Vector3(0, 0, -1) * unit;
            default:
                return Vector3.zero;
        }
    }
    public static Vector3 CastDirectionToVector(int direction)
    {
        direction %= 4;
        int unit = MapManager.unit;
        switch (direction)
        {
            case (int)Direction.right:
                return new Vector3(1, 0, 0) * unit;
            case (int)Direction.left:
                return new Vector3(-1, 0, 0) * unit;
            case (int)Direction.up:
                return new Vector3(0, 0, 1) * unit;
            case (int)Direction.down:
                return new Vector3(0, 0, -1) * unit;
            default:
                return Vector3.zero;
        }
    }
}
