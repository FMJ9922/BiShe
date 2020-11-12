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
                mInstance = Resources.Load<DataManager>("Data/ScriptData/Data");
            }
            return mInstance;
        }
    }

    public ItemData[] ItemArray;
    public LevelData[] LevelArray;
    public TechData[] TechArray;
    public BuildData[] BuildArray;

    public Dictionary<BuildTabType, List<BuildData>> TabDic = new Dictionary<BuildTabType, List<BuildData>>();

    

    public void InitTabDic()
    {
        for (int i = 0; i < BuildArray.Length; i++)
        {
            BuildTabType tabType = BuildArray[i].tabType;
            if (TabDic.ContainsKey(tabType))
            {
                List<BuildData> buildDatas = TabDic[tabType];
                buildDatas.Add(BuildArray[i]);
                TabDic[tabType] = buildDatas;
            }
            else
            {
                List<BuildData> buildDatas = new List<BuildData>();
                buildDatas.Add(BuildArray[i]);
                TabDic.Add(tabType, buildDatas);
            }
        }
        Debug.Log("创建TabDic成功！");
    }
}
