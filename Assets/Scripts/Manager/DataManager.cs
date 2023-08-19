using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : ScriptableObject
{
    public static DataManager mInstance;
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
    public FormulaData[] FormulaArray;
    public LocalizationData LocalizationData;
    public CarData[] CarArray;
    public OrderData[] OrderArray;
    public Dictionary<BuildTabType, List<BuildData>> TabDic;
    public ChooseSkillData[] ChooseSkillArray;
    public SkillBuffData[] SkillBuffArray;
    public static string[] foodNames;
    public static int[] foodIds;
    public static int[] itemIds;
    public static ItemData[] foodItems;

    public void InitTabDic()
    {
        TabDic = new Dictionary<BuildTabType, List<BuildData>>();
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
        TabDic[BuildTabType.produce].Sort((BuildData a, BuildData b) => { return a.Name.Length - b.Name.Length; });
        Debug.Log("创建TabDic成功！");
    }

    public static LevelData[] GetAllLevelDatas()
    {
        return Instance.LevelArray;
    }

    public static LevelData GetLevelData(int levelId)
    {
        for (int i = 0; i < Instance.LevelArray.Length; i++)
        {
            if (Instance.LevelArray[i].Id == levelId)
            {
                return Instance.LevelArray[i];
            }
        }
        Debug.Log("无效的关卡ID");
        return null;
    }

    public static TechData GetTechData(int techId)
    {
        for (int i = 0; i < Instance.TechArray.Length; i++)
        {
            if (Instance.TechArray[i].Id == techId)
            {
                return Instance.TechArray[i];
            }
        }
        Debug.Log("无效的科技ID");
        return null;
    }
    public static BuildData GetBuildData(int buildId)
    {
        for (int i = 0; i < Instance.BuildArray.Length; i++)
        {
            if (Instance.BuildArray[i].Id == buildId)
            {
                return Instance.BuildArray[i];
            }
        }
        Debug.Log("无效的建筑id" + buildId);
        return null;
    }

    public static CarData GetCarData(TransportationType carId)
    {
        for (int i = 0; i < Instance.CarArray.Length; i++)
        {
            if (Instance.CarArray[i].CarType == carId)
            {
                return Instance.CarArray[i];
            }
        }
        Debug.Log("无效的车辆id" + carId);
        return null;
    }

    
    public static OrderData GetOrderData(int orderId)
    {
        for (int i = 0; i < Instance.OrderArray.Length; i++)
        {
            if (Instance.OrderArray[i].ID == orderId)
            {
                return Instance.OrderArray[i];
            }
        }
        Debug.Log("无效的订单id" + orderId);
        return null;
    }

    public static int GetOrderLength()
    {
        return Instance.OrderArray.Length;
    }
    public static string GetItemNameById(int ID)
    {
        for (int i = 0; i < Instance.ItemArray.Length; i++)
        {
            if (Instance.ItemArray[i].Id == ID)
            {
                return Instance.ItemArray[i].Name;
            }
        }
        Debug.Log("无效的物品ID" + ID);
        return string.Empty;
    }
    public static ItemData GetItemDataById(int ID)
    {
        for (int i = 0; i < Instance.ItemArray.Length; i++)
        {
            if (Instance.ItemArray[i].Id == ID)
            {
                return Instance.ItemArray[i];
            }
        }
        Debug.Log("无效的物品ID" + ID);
        return null;
    }

    public static ItemData[] GetItemDatas()
    {
        return Instance.ItemArray;
    }

    public static FormulaData GetFormulaById(int ID)
    {
        for (int i = 0; i < Instance.FormulaArray.Length; i++)
        {
            if (Instance.FormulaArray[i].ID == ID)
            {
                return Instance.FormulaArray[i];
            }
        }
        Debug.Log("无效的物品ID" + ID);
        return null;
    }
    public static ItemType GetItemType(int id)
    {
        for (int i = 0; i < Instance.ItemArray.Length; i++)
        {
            if (Instance.ItemArray[i].Id == id)
            {
                return (ItemType)Instance.ItemArray[i].ItemType;
            }
        }
        Debug.Log("无效的物品ID" + id);
        return ItemType.industry;
    }
    public static string[] GetFoodNameList()
    {
        if (foodNames == null)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < Instance.ItemArray.Length; i++)
            {
                if (Instance.ItemArray[i].ItemType == (int)ItemType.food)
                {
                    list.Add(Instance.ItemArray[i].Name);
                }
            }
            foodNames = list.ToArray();
        }
        return foodNames;
    }

    public static int[] GetFoodIDList()
    {
        if (foodIds == null)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < Instance.ItemArray.Length; i++)
            {
                if (Instance.ItemArray[i].ItemType == (int)ItemType.food)
                {
                    list.Add(Instance.ItemArray[i].Id);
                }
            }
            foodIds = list.ToArray();
        }
        return foodIds;
    }

    public static ItemData[] GetFoodItemList()
    {
        if (foodItems == null)
        {
            List<ItemData> list = new List<ItemData>();
            for (int i = 0; i < Instance.ItemArray.Length; i++)
            {
                if (Instance.ItemArray[i].Id == 11000) continue;
                if (Instance.ItemArray[i].ItemType == (int)ItemType.food)
                {
                    list.Add(Instance.ItemArray[i]);
                }
            }
            list.Sort((ItemData a, ItemData b) => { return b.Happiness - a.Happiness; });
            foodItems = list.ToArray();
        }

        return foodItems;
    }

    public static int[] GetAllItemList()
    {
        if (itemIds == null)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < Instance.ItemArray.Length; i++)
            {
                if (Instance.ItemArray[i].Id != 11000 && Instance.ItemArray[i].Id != 99999)
                {
                    list.Add(Instance.ItemArray[i].Id);
                }
            }
            itemIds = list.ToArray();
        }
        return itemIds;
    }

    public static SkillBuffData GetSkillBuffData(int id)
    {
        for (int i = 0; i < Instance.SkillBuffArray.Length; i++)
        {
            if (Instance.SkillBuffArray[i].Id == id)
            {
                return Instance.SkillBuffArray[i];
            }
        }
        Debug.Log("无效的物品ID" + id);
        return null;
    }
    public static int GetItemIdByName(string name)
    {
        for (int i = 0; i < Instance.ItemArray.Length; i++)
        {
            if (Instance.ItemArray[i].Name == name)
            {
                return Instance.ItemArray[i].Id;
            }
        }
        Debug.Log("无效的物品名称" + name);
        return 0;
    }
}
