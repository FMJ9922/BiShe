using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using Building;
using Manager;

public class GameSaver
{

    public static string GetCustomPath(string fileName)
    {
        string path = string.Format("{0}/{1}", "Resources/Saves", fileName);
        return path;
    }

    private static string GetOffcialPath(string fileName)
    {
        string path = string.Format("{0}/{1}", "Assets/Resources/MapData", fileName);
        return path;
    }

    public static SaveData ReadSaveData(string fileName, bool isOffcial)
    {
        string path = isOffcial ? GetOffcialPath(fileName) : Application.dataPath + "/" + GetCustomPath(fileName);
        SaveData data;
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();
        if (isOffcial)
        {
            file = File.Open(path + ".save", FileMode.Open);
        }
        else
        {
            file = File.Open(path + "/" + fileName + ".save", FileMode.Open);
        }
        data = (SaveData)bf.Deserialize(file);
        file.Close();
        return data;

    }

    public static void WriteSaveData(string fileName, SaveData data, bool isOffcial)
    {
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();
        string path;
        if (isOffcial)
        {
            path = GetOffcialPath(fileName);
            file = File.Open(path + ".save", FileMode.Create);
        }
        else
        {
            path = Application.dataPath + "/" + GetCustomPath(fileName);
            if (!File.Exists(path))
            {
                Debug.Log(path);
                Directory.CreateDirectory(path);
            }
            else
            {
                DeleteAllFile(path);
                Directory.CreateDirectory(path);
            }
            file = File.Open(path + "/" + fileName + ".save", FileMode.Create);
            //TerrainGenerator.Instance.SaveTerrain(fileName, isOffcial);
        }
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("存储成功！\n" + path);
    }

    public static void DeleteSaveData(string path, string filesName)
    {
        if (Directory.Exists(path))
        {
            if (File.Exists(path + "/" + filesName))
            {
                File.Delete(path + "/" + filesName);
            }
        }
    }


    public static void DeleteAllFile(string fullPath)
    {
        if (System.IO.Directory.Exists(fullPath))
        {
            var dir = new System.IO.DirectoryInfo(fullPath);
            dir.Attributes = dir.Attributes & ~FileAttributes.ReadOnly;
            dir.Delete(true);
        }
    }
}


[System.Serializable]
public class SaveData
{
    #region
    public string saveName;
    public Vector2IntSerializer mapSize;
    public bool isOffcial = true;
    #endregion
    #region Mesh
    public string meshName;
    public Vector3Serializer[] meshVerticles;
    public Vector2Serializer[] meshUV;
    public int[] meshTriangles;
    public int[] meshDir;
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
    public Dictionary<int, int[]> allTimeResources;
    public int curPopulation;
    public int[] hudList;
    public int[] forbiddenFoodList;
    #endregion
    #region MapManager
    public GridNode[][] gridNodes;
    #endregion
    #region MarketManager
    public MarketData[] buyDatas;//no use anymore
    public MarketData[] sellDatas;//no use anymore
    public RuntimeOrderData[] runtimeOrderDatas;
    public long OrderIndex;
    public float[] OrderAppearValueArray;
    #endregion
    #region TrafficManager
    public CarMission[] driveDatas;
    #endregion
    #region Bridges
    public BridgeData[] bridgeDatas;
    #endregion
    #region Tech
    public int[] techs;
    public int[] techAvalible;
    public int[] techUsing;
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
        if(vector3s == null)
        {
            return null;
        }
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

[System.Serializable]
public class MarketData
{
    public ItemData curItem;
    public List<int> idList = new List<int>();
    public bool isTrading = false;
    public int needNum = 0;
    public TradeMode curMode;
    public float profit;
    public bool isBuy = true;
    public int maxNum;
}

[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
{
    public SerializableDictionary() { }
    public void WriteXml(XmlWriter write)       // Serializer
    {
        XmlSerializer KeySerializer = new XmlSerializer(typeof(TKey));
        XmlSerializer ValueSerializer = new XmlSerializer(typeof(TValue));

        foreach (KeyValuePair<TKey, TValue> kv in this)
        {
            write.WriteStartElement("SerializableDictionary");
            write.WriteStartElement("key");
            KeySerializer.Serialize(write, kv.Key);
            write.WriteEndElement();
            write.WriteStartElement("value");
            ValueSerializer.Serialize(write, kv.Value);
            write.WriteEndElement();
            write.WriteEndElement();
        }
    }
    public void ReadXml(XmlReader reader)       // Deserializer
    {
        reader.Read();
        XmlSerializer KeySerializer = new XmlSerializer(typeof(TKey));
        XmlSerializer ValueSerializer = new XmlSerializer(typeof(TValue));

        while (reader.NodeType != XmlNodeType.EndElement)
        {
            reader.ReadStartElement("SerializableDictionary");
            reader.ReadStartElement("key");
            TKey tk = (TKey)KeySerializer.Deserialize(reader);
            reader.ReadEndElement();
            reader.ReadStartElement("value");
            TValue vl = (TValue)ValueSerializer.Deserialize(reader);
            reader.ReadEndElement();
            reader.ReadEndElement();
            this.Add(tk, vl);
            reader.MoveToContent();
        }
        reader.ReadEndElement();

    }
    public XmlSchema GetSchema()
    {
        return null;
    }
}