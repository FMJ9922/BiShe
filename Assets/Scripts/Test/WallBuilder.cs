using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WallBuilder : MonoBehaviour
{

    public GameObject straightWall;
    public GameObject gateWall;
    public GameObject cornerWall;

    public Vector3 from;
    public Vector3 to;
    public Vector3[] pos;
    public Terrain terrain;
    

    void Start()
    {
        //BuildStraightWall(from, to);
        //BuildCornerWalls(pos);
        ///BuildSingleWall(gateWall, Vector3.one, Vector3.zero, Vector3.zero);
        //BuildCornerWall(pos[0],pos[1]);
    }
    [ContextMenu("Build")]
    public void BuildInEditor()
    {
        BuildCornerWalls(pos);
    }
    public void BuildCornerWalls(Vector3[] pos)
    {
        float width = 0.5f*cornerWall.GetComponent<WallItem>().width;
        for(int i = 0; i < pos.Length; i++)
        {
            if (i != pos.Length - 1)
            {
                BuildCornerWall(pos[i], pos[i + 1]);
                float distance = GetDistance(pos[i], pos[i + 1]);
                Vector3 start = Vector3.Lerp(pos[i], pos[i + 1], width / distance);
                Vector3 end = Vector3.Lerp(pos[i], pos[i + 1], 1-width / distance);
                BuildStraightWall(start, end);
            }
            else
            {
                BuildCornerWall(pos[i], pos[0]);
                float distance = GetDistance(pos[i], pos[0]);
                Vector3 start = Vector3.Lerp(pos[i], pos[0], width / distance);
                Vector3 end = Vector3.Lerp(pos[i], pos[0], 1 - width / distance);
                BuildStraightWallWithGate(start, end);
            }
        }
    }
    public void BuildCornerWall(Vector3 thisCorner,Vector3 leftCorner)
    {
        Vector3 left = leftCorner - thisCorner;
        Vector3 towards = Vector3.Cross(left, Vector3.up);
        GameObject cornerwall = BuildSingleWall(cornerWall, Vector3.one,GetTerrainHeight(thisCorner), towards);
    }
    
    public void BuildStraightWallWithGate(Vector3 from, Vector3 to)
    {
        GameObject wallContainer = new GameObject("wallContainer");
        float width = gateWall.GetComponent<WallItem>().width;
        float length = GetDistance(from, to);
        Debug.Log(length);
        if (length < gateWall.GetComponent<WallItem>().width)
        {
            return;
        }
        Vector3 center = Vector3.Lerp(from, to, 0.5f);
        Vector3 towards = Vector3.Cross(to - from, Vector3.up);
        GameObject gate = BuildSingleWall(gateWall, Vector3.one, center, towards);
        Debug.Log(Vector3.MoveTowards(center, center + towards, 10));
        gate.transform.position = GetTerrainHeight(Vector3.MoveTowards(center, center+towards, 5));
        Debug.Log(width+" "+length);
        Vector3 newTo = Vector3.Lerp(from, to, 0.5f-width*0.5f/length);
        Vector3 newFrom = Vector3.Lerp(from, to, 0.5f+width*0.5f/length);
        BuildStraightWall(from, newTo);
        BuildStraightWall(newFrom, to);
    }
    public GameObject[] BuildStraightWall(Vector3 from, Vector3 to)
    {
        float width = straightWall.GetComponent<WallItem>().width;
        float length = GetDistance(from, to);
        if (length < 0.8f*width)
        {
            return null;
        }

        int times = Mathf.RoundToInt(length / width);
        GameObject[] straightWalls = new GameObject[times];
        Vector3 scale = new Vector3(length / (width * times), 1, 1);
        Vector3 towards = Vector3.Cross(to - from, Vector3.up);
        for (int i = 0; i < times; i++)
        {
            straightWalls[i] = BuildSingleWall(straightWall, scale, Vector3.Lerp(from, to, (float)i / times + 5f / length), towards);
        }
        return straightWalls;

    }

    public GameObject BuildSingleWall(GameObject pfbs, Vector3 scale, Vector3 pos, Vector3 towards)
    {
        Debug.Log(towards);
        //Vector3 adjPos = Vector3.MoveTowards(pos,pos+towards, pfbs.GetComponent<WallItem>().CenterPos.magnitude) ;
        GameObject newWall = Instantiate(pfbs, GetTerrainHeight(pos), Quaternion.LookRotation(towards), transform);
        Mesh mesh = CopyMesh(newWall.GetComponent<MeshFilter>().mesh);
        List<Vector3> positionList = new List<Vector3>(mesh.vertices);
        for (int i = 0; i < positionList.Count; i++)
        {
            Debug.Log(positionList[i] + " " + pos + " " + newWall.transform.TransformPoint(positionList[i]));
            positionList[i] = AddTerrainHeight(newWall.transform.TransformPoint(positionList[i]), GetTerrainHeight(pos));
            positionList[i] = newWall.transform.InverseTransformPoint(positionList[i]);
            Debug.Log(positionList[i]);
        }
        mesh.vertices = positionList.ToArray();
        mesh.RecalculateNormals();
        mesh.name = "123";
        newWall.GetComponent<MeshFilter>().mesh = mesh;
        if (newWall.transform.childCount!=0&& newWall.transform.GetChild(0).GetComponent<MeshFilter>()!=null)
        {
            Mesh subMesh = CopyMesh(newWall.transform.GetChild(0).GetComponent<MeshFilter>().mesh);
            List<Vector3> subPositionList = new List<Vector3>(subMesh.vertices);
            for (int i = 0; i < subPositionList.Count; i++)
            {
                subPositionList[i] = AddTerrainHeight(newWall.transform.TransformPoint(subPositionList[i]), GetTerrainHeight(pos));
                subPositionList[i] = newWall.transform.InverseTransformPoint(subPositionList[i]);
            }
            subMesh.vertices = subPositionList.ToArray();
            subMesh.RecalculateNormals();
            newWall.transform.GetChild(0).GetComponent<MeshFilter>().mesh = subMesh;
        }
        newWall.transform.localScale = scale;
        return newWall;
    }
    public Mesh CopyMesh(Mesh originMesh)
    {
        Mesh m = new Mesh();
        m.vertices = originMesh.vertices;
        m.normals = originMesh.normals;
        m.triangles = originMesh.triangles;
        m.uv = originMesh.uv;
        return m;
    }
    public float GetDistance(Vector3 from,Vector3 to)
    {
        Vector3 newFrom = new Vector3(from.x, 0, from.z);
        Vector3 newTo = new Vector3(to.x, 0, to.z);
        return Vector3.Distance(from, to);
    }

    public Vector3 GetTerrainHeight(Vector3 originPos)
    {
        Vector3 terrainPos = new Vector3(originPos.x, terrain.SampleHeight(originPos), originPos.z);
        return terrainPos;
    }
    public Vector3 AddTerrainHeight(Vector3 originPos,Vector3 wholePos)
    {
        Vector3 terrainPos = new Vector3(originPos.x, originPos.y+terrain.SampleHeight(originPos)- wholePos.y, originPos.z);
        return terrainPos;
    }
}
