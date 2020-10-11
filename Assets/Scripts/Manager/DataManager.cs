using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : ScriptableObject
{
    static DataManager mInstance;
    public static DataManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = Resources.Load<DataManager>("Data/ScriptData/Item");
            }
            return mInstance;
        }
    }

    public ItemData[] ItemArray;
    public LevelData[] LevelArray;
    public TechData[] TechArray;
    public BuildData[] BuildArray;
}
