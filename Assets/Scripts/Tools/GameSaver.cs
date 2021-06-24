using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class GameSaver 
{
    private static string GetCustomPath(string fileName)
    {
        string path = string.Format("{0}/{1}", "Assets/Resources/Saves",fileName);
        return path;
    }

    private static string GetOffcialPath(string fileName)
    {
        string path = string.Format("{0}/{1}", "Assets/Resources/MapData", fileName);
        return path;
    }

    public static SaveData ReadSaveData(string fileName,bool isOffcial)
    {
        string path = isOffcial? GetOffcialPath(fileName):GetCustomPath(fileName);
        SaveData data;
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();
        file = File.Open(path+".save", FileMode.Open);
        data = (SaveData)bf.Deserialize(file);
        file.Close();
        return data;
        
    }

    public static void WriteSaveData(string fileName,SaveData data, bool isOffcial)
    {
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();
        string path = isOffcial ? GetOffcialPath(fileName) : GetCustomPath(fileName);
        file = File.Open(path + ".save", FileMode.Create);
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("存储成功！\n"+ path);
    }

}


[System.Serializable]
public class SaveData
{
    #region
    public string mapName;
    public Vector2IntSerializer mapSize;
    public bool isOffcial = true;
    #endregion
    #region Mesh
    //public string meshName;
    //public Vector3Serializer[] meshVerticles;
    //public Vector2Serializer[] meshUV;
    //public int[] meshTriangles;
    //public int[] meshDir;
    public int[] meshTex;
    #endregion
    #region Tree
    public TreeData[] treeData;
    public Vector3Serializer[] treePosition;
    public Vector3Serializer[] treeRotation;
    #endregion
    #region Water
    public Vector3Serializer[] waterPos;
    public Vector3Serializer[] waterScale;
    #endregion
    #region Buildings
    public RuntimeBuildData[] buildingDatas;
    #endregion
    #region LevelManager
    public float dayTime;
    public int levelID;
    public int year, month, week, day;
    public bool hasSuccess;
    public bool pause;
    public float timer;
    public float weekProgress;
    #endregion
    #region ResourceManager
    public CostResource[] saveResources;
    public int curPopulation;
    #endregion
    #region MapManager
    public GridNode[][] gridNodes;
    #endregion
    #region MarketManager
    public MarketData[] buyDatas;
    public MarketData[] sellDatas;
    #endregion

}


[System.Serializable]
public struct Vector3Serializer
{
    public float x;
    public float y;
    public float z;

    public void Fill(Vector3 v3)
    {
        x = v3.x;
        y = v3.y;
        z = v3.z;
    }

    public Vector3 V3
    { get { return new Vector3(x, y, z); } }

    public static Vector3[] Unbox(Vector3Serializer[] vector3Serializers)
    {
        Vector3[] vec3 = new Vector3[vector3Serializers.Length];
        for (int i = 0; i < vector3Serializers.Length; i++)
        {
            vec3[i] = vector3Serializers[i].V3;
        }
        return vec3;
    }

    public static Vector3Serializer[] Box(Vector3[] vector3s)
    {
        Vector3Serializer[] vector3Serializers = new Vector3Serializer[vector3s.Length];
        for (int i = 0; i < vector3s.Length; i++)
        {
            vector3Serializers[i].Fill(vector3s[i]);
        }
        return vector3Serializers;
    }
}
[System.Serializable]
public struct Vector2Serializer
{
    public float x;
    public float y;

    public void Fill(Vector2 v2)
    {
        x = v2.x;
        y = v2.y;
    }

    public Vector2 V2
    { get { return new Vector2(x, y); } }

    public static Vector2[] Unbox(Vector2Serializer[] vector2Serializers)
    {
        Vector2[] vec2 = new Vector2[vector2Serializers.Length];
        for (int i = 0; i < vector2Serializers.Length; i++)
        {
            vec2[i] = vector2Serializers[i].V2;
        }
        return vec2;
    }

    public static Vector2Serializer[] Box(Vector2[] vector2s)
    {
        Vector2Serializer[] vector2Serializers = new Vector2Serializer[vector2s.Length];
        for (int i = 0; i < vector2s.Length; i++)
        {
            vector2Serializers[i].Fill(vector2s[i]);
        }
        return vector2Serializers;
    }
}