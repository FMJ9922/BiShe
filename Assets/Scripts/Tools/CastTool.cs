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
                return Vector3.one;
        }
    }

    public static Direction CastVector2ToDirection(Vector2 vector2)
    {
        if (vector2.x > 0)
        {
            return Direction.right;
        }
        else if (vector2.x < 0)
        {
            return Direction.left;
        }
        else if (vector2.y > 0)
        {
            return Direction.up;
        }
        else
        {
            return Direction.down;
        }
    }

    public static Direction CastVector3ToDirection(Vector3 vector3)
    {
        if (vector3.x > 0 && Mathf.Abs(vector3.x)> Mathf.Abs(vector3.z))
        {
            return Direction.right;
        }
        else if (vector3.x < 0 && Mathf.Abs(vector3.x) > Mathf.Abs(vector3.z))
        {
            return Direction.left;
        }
        else if (vector3.z > 0)
        {
            return Direction.up;
        }
        else
        {
            return Direction.down;
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

    public static Vector2Int CastDirectionToVector2Int(int direction)
    {
        direction += 4;
        direction %= 4;
        int unit = MapManager.unit;
        switch (direction)
        {
            case (int)Direction.right:
                return new Vector2Int(1, 0) * unit;
            case (int)Direction.left:
                return new Vector2Int(-1, 0) * (unit);
            case (int)Direction.up:
                return new Vector2Int(0, 1) * unit;
            case (int)Direction.down:
                return new Vector2Int(0, -1) * (unit);
            default:
                Debug.Log(direction);
                return Vector2Int.zero;
        }
    }
}
