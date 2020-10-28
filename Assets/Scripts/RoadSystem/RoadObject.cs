using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadObject : MonoBehaviour
{

    private List<Vector3> vertices = new List<Vector3>();

    private List<Vector2> uv = new List<Vector2>();

    private List<int> triangles = new List<int>();

    private Mesh mesh;

    //道路宽度
    private float roadWidth { get; } = 2.0f;

    //道路贴UV的间隔长度
    private float sampleLength { get; } = 2.0f;

    //道路拐角长度
    public float turingLength { get; set; } = 4.0f;

    //转弯处取样的间隔
    private float deltaAngle { get; } = 10f;

    private void Start()
    {
    }
    private void Init()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "testMesh";
    }

    private void BuildRoads(List<Vector3> wayPoints)
    {

        for (int i = 1; i < wayPoints.Count - 1; i++)
        {
            Vector3 beforeV = wayPoints[i] - wayPoints[i - 1];
            Vector3 afterV = wayPoints[i + 1] - wayPoints[i];
            Vector3 inPos = wayPoints[i - 1] + beforeV * ((beforeV.magnitude - turingLength) / beforeV.magnitude);
            Vector3 outPos = wayPoints[i] + afterV * (turingLength / afterV.magnitude);
            if (i == 1)
            {
                BuildStraightRoad(wayPoints[i - 1], inPos);
                Debug.Log("start");
            }
            //BuildTurningRoad(inPos, wayPoints[i], outPos);
            if (i == wayPoints.Count - 2)
            {
                //BuildStraightRoad(outPos,wayPoints[i + 1]);
                Debug.Log("end");
            }
            else
            {
                Vector3 nextInPos = wayPoints[i] + afterV * ((afterV.magnitude - turingLength) / afterV.magnitude);
                //BuildStraightRoad(outPos, nextInPos);
            }
        }
        
    }
    public void BuildStraightRoad(Vector3 firstPoint, Vector3 secondPoint)
    {
        Init();
        Debug.Log("BuildStraight:"+firstPoint + "->" + secondPoint);
        //取路径点最后两个点计算
        Vector3 forwardVector = firstPoint - secondPoint;
        //利用相似三角形求出道路两侧距离中心点的偏移量
        float rate = roadWidth / forwardVector.magnitude;
        Vector3 leftDelta = new Vector3(-forwardVector.z * rate, 0, forwardVector.x * rate);
        Vector3 rightDelta = -leftDelta;
        //获取单边采样点的数量
        int sampleNum = (int)((1f / rate) / 2 + 1);
        for (int i = 0; i < sampleNum; i++)
        {
            Vector3 centerPoint = secondPoint + Mathf.Lerp(0, 1, (float)i / (sampleNum - 1)) * forwardVector;
            vertices.Add(centerPoint + leftDelta);
            vertices.Add(centerPoint + rightDelta);
            if (i % 2 < 1)
            {
                uv.Add(new Vector2(0.0f, 1.0f));
                uv.Add(new Vector2(0.0f, 0.0f));
            }
            else
            {
                uv.Add(new Vector2(1.0f, 1.0f));
                uv.Add(new Vector2(1.0f, 0.0f));
            }
        }

        int[] triangle = new int[2 * 3 * (sampleNum - 1)];
        for (int x = 0, ti = 0; x < sampleNum - 1; x++, ti += 6)
        {
            triangle[ti] = 2 * x;
            triangle[ti + 1] = 2 * x + 3;
            triangle[ti + 2] = 2 * x + 1;
            triangle[ti + 3] = 2 * x;
            triangle[ti + 4] = 2 * x + 2;
            triangle[ti + 5] = 2 * x + 3;
        }
        triangles.AddRange(triangle);
        RecalculateMesh();
    }

    public void BuildTurningRoad(Vector3 beforePoint, Vector3 turningPoint, Vector3 afterPoint)
    {
        Init();
        Debug.Log("Build:"+beforePoint + "->" + turningPoint + "->" + afterPoint);
        Vector3 midCenter = (beforePoint + afterPoint) / 2;
        Vector3 a = turningPoint - midCenter;
        Vector3 b = beforePoint - midCenter;
        Vector3 c = -a * (b.sqrMagnitude / a.sqrMagnitude);
        //Debug.Log(a + " " + b + " " + c);
        Vector3 circleCenter = midCenter + c;
        //Debug.Log("center:" + circleCenter);
        Vector3 d = beforePoint - circleCenter;
        //Debug.Log("d:" + d);
        float turningLength = (turningPoint - beforePoint).magnitude;
        //Debug.Log("turn" + turningLength);
        float rad = turningLength * (b.magnitude / a.magnitude);
        //Debug.Log("rad:" + rad);
        float halfAngle = Mathf.Atan2(turningLength, rad) * Mathf.Rad2Deg;
        //Debug.Log("angle:" + halfAngle);
        int sampleNum = (int)(halfAngle / deltaAngle) + 1;
        //Debug.Log("sampleNum:" + sampleNum);
        float realDelta = halfAngle / (sampleNum - 1);
        //Debug.Log("realDelta:" + realDelta);
        Vector3 normal = Vector3.Cross(Vector3.Cross(d, a), d).normalized;
        //Debug.Log("Normal:" + normal);
        for (int i = 0; i < 2 * sampleNum - 1; i++)
        {
            //Debug.Log("angle:" + realDelta * i);
            Vector3 dPosIn = circleCenter + (rad - roadWidth) / rad * d * Mathf.Cos(realDelta * i * Mathf.Deg2Rad);
            Vector3 dPosOut = circleCenter + (rad + roadWidth) / rad * d * Mathf.Cos(realDelta * i * Mathf.Deg2Rad);
            //Debug.Log(dPosIn + " " + dPosOut);
            //Debug.Log("Sin:" + Mathf.Sin(realDelta * i * Mathf.Deg2Rad));
            Vector3 innerPos = dPosIn + Mathf.Sin(realDelta * i * Mathf.Deg2Rad) * (rad - roadWidth) * normal;
            Vector3 outterPos = dPosOut + Mathf.Sin(realDelta * i * Mathf.Deg2Rad) * (rad + roadWidth) * normal;
            //Debug.Log("in:" + innerPos);      
            //Debug.Log("out:" + outterPos);
            //Debug.DrawLine(innerPos, outterPos, Color.red, 100F);
            if (Vector3.Cross(d, a).y > 0)
            {
                vertices.Add(outterPos);
                vertices.Add(innerPos);
            }
            else
            {
                vertices.Add(innerPos);
                vertices.Add(outterPos);
            }
            float angle = 2f * roadWidth / d.magnitude * Mathf.Rad2Deg;
            //Debug.Log("radangle:" + angle);
            float rate = realDelta * i / angle;
            //Debug.Log("rate:" + Clamp0to2(rate));
            uv.Add(new Vector2(Clamp0to2(rate), 1.0f));
            uv.Add(new Vector2(Clamp0to2(rate), 0.0f));
        }
        int[] triangle = new int[2 * 3 * (2 * sampleNum - 2)];
        for (int x = 0, ti = 0; x < 2 * sampleNum - 2; x++, ti += 6)
        {
            triangle[ti] = 2 * x;
            triangle[ti + 1] = 2 * x + 3;
            triangle[ti + 2] = 2 * x + 1;
            triangle[ti + 3] = 2 * x;
            triangle[ti + 4] = 2 * x + 2;
            triangle[ti + 5] = 2 * x + 3;
        }
        triangles.AddRange(triangle);
        RecalculateMesh();
    }

    public void RecalculateMesh()
    {
        mesh.vertices = vertices.ToArray();
        mesh.uv = uv.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    private float Clamp0to2(float num)
    {
        if (num < 0)
        {
            return 0;
        }
        while (num > 2)
        {
            num -= 2;
        }
        if (num <= 1)
        {
            return num;
        }
        else if (num <= 2)
        {
            return 2 - num;
        }
        return num;
    }
}
