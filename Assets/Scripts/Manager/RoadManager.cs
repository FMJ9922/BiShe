using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 已弃用
/// </summary>
public class RoadManager : Singleton<RoadManager>
{
    //储存修路信息路径点
    private List<Vector3> _wayPoints = new List<Vector3>();

    private Dictionary<int, GameObject> _wayPointDic = new Dictionary<int, GameObject>();

    private GameObject RoadPfb;

    private int _partCounter = 0;

    public float turingLength { get; set; } = 4.0f;

    
    #region 公有方法
    public void ResetRoad()
    {
        _wayPoints = new List<Vector3>();
        _wayPointDic = new Dictionary<int, GameObject>();
        _partCounter = 0;
    }
    /*
    public void BuildRoads()
    {
        RoadPfb = LoadAB.Load("roads.road", "RoadPfb");
        _wayPoints.Add(InputManager.Instance.LastGroundRayPos);
        int count = _wayPoints.Count;
        //如果输入的不是第一个路径点
        if (count == 1)
        {
            return;
        }
        //删除不合法的输入：距离过短
        if (Vector3.Distance(_wayPoints[count - 2], _wayPoints[count - 1]) <= 2 * turingLength)
        {
            _wayPoints.RemoveAt(count - 1);
            return;
        }
        Vector3 firstP;
        Vector3 secondP;
        Vector3 thirdP;
        if (count == 2)
        {
            InstantiateStraightRoad(_wayPoints[count - 2], _wayPoints[count - 1], RoadOption.start);
        }
        else if (count > 2)
        {
            DeleteLastRoad();
            InstantiateStraightRoad(_wayPoints[count - 3], _wayPoints[count - 2], RoadOption.normal);
            InstantiateTurnRoad(_wayPoints[count - 3], _wayPoints[count - 2], _wayPoints[count - 1]);
            InstantiateStraightRoad(_wayPoints[count - 2], _wayPoints[count - 1], RoadOption.end);
        }

    }*/
   

    #endregion
    private void DeleteLastRoad()
    {
        GameObject lastRoad = _wayPointDic[_partCounter];
        _wayPointDic.Remove(_partCounter);
        _partCounter--;
        Destroy(lastRoad);
    }

    private void InstantiateStraightRoad(Vector3 startP, Vector3 endP, RoadOption roadOption)
    {
        Vector3 firstP;
        Vector3 secondP;
        GameObject newRoad = Instantiate(RoadPfb, transform);
        newRoad.name = string.Format("{0}->{1}", startP.ToString(), endP.ToString());
        //CalculateWayPoints(startP, endP, out firstP, out secondP, roadOption);
        RoadObject roadObject = newRoad.GetComponent<RoadObject>();
        //roadObject.BuildStraightRoad(firstP, secondP);
        _partCounter++;
        _wayPointDic.Add(_partCounter, newRoad);
    }

    private void InstantiateTurnRoad(Vector3 startP, Vector3 turnP, Vector3 endP)
    {
        Vector3 firstP;
        Vector3 secondP;
        Vector3 thirdP;
        GameObject newTurn = Instantiate(RoadPfb, transform);
        newTurn.name = string.Format("->{0}->", turnP);
        CalculateWayPoints(startP, turnP, endP, out firstP, out secondP, out thirdP);
        RoadObject roadObject = newTurn.GetComponent<RoadObject>();
        roadObject.BuildTurningRoad(firstP, secondP, thirdP);
        _partCounter++;
        _wayPointDic.Add(_partCounter, newTurn);
    }
    /// <summary>
    /// 计算直行道路的入点和出点
    /// </summary>
    /// <param name="startP"></param>
    /// <param name="endP"></param>
    /// <param name="firstP"></param>
    /// <param name="secondP"></param>
    /// <param name="roadOption"></param>
   /* private void CalculateWayPoints(Vector3 startP, Vector3 endP,
        out Vector3 firstP, out Vector3 secondP, RoadOption roadOption)
    {
        Vector3 vector = endP - startP;
        float length = vector.magnitude;
        if (roadOption == RoadOption.start)
        {
            firstP = startP;
            secondP = endP - vector * (turingLength / length);
        }
        else if (roadOption == RoadOption.normal)
        {
            firstP = startP + vector * (turingLength / length);
            secondP = endP - vector * (turingLength / length);
        }
        else
        {
            firstP = startP + vector * (turingLength / length);
            secondP = endP;
        }

    }*/

    /// <summary>
    /// 计算拐点的入点和出点
    /// </summary>
    /// <param name="startP"></param>
    /// <param name="turnP"></param>
    /// <param name="endP"></param>
    /// <param name="firstP"></param>
    /// <param name="secondP"></param>
    /// <param name="thirdP"></param>
    private void CalculateWayPoints(Vector3 startP, Vector3 turnP, Vector3 endP,
        out Vector3 firstP, out Vector3 secondP, out Vector3 thirdP)
    {
        Vector3 beforeV = turnP - startP;
        Vector3 afterV = endP - turnP;
        firstP = turnP - beforeV * (turingLength / beforeV.magnitude);
        secondP = turnP;
        thirdP = turnP + afterV * (turingLength / afterV.magnitude);
    }
}
