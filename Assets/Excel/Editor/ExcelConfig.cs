using Excel;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;

public class ExcelConfig
{
    /// <summary>
    /// 存放excel表文件夹的的路径，本例excel表放在了"Assets/Excel/"当中
    /// </summary>
    public static readonly string excelsFolderPath = Application.dataPath + "/Resources/Data/RawData/";

    /// <summary>
    /// 存放Excel转化CS文件的文件夹路径
    /// </summary>
    public static readonly string assetPath = "Assets/Resources/Data/ScriptData/";
}

public class ExcelTool
{
    public static ItemData[] CreateItemArrayWithExcel(string filePath)
    {
        int columnNum = 0, rowNum = 0;
        DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);
        ItemData[] array = new ItemData[rowNum - 2];
        for (int i = 2; i < rowNum; i++)
        {
            ItemData item = new ItemData();
            item.Id = int.Parse(collect[i][0].ToString());
            item.Name = collect[i][1].ToString();
            item.ItemType = int.Parse(collect[i][2].ToString());
            item.Price = int.Parse(collect[i][3].ToString());
            item.Happiness = int.Parse(collect[i][4].ToString());
            array[i - 2] = item;
        }
        return array;
    }

    public static LevelData[] CreateLevelArrayWithExcel(string filePath)
    {
        int columnNum = 0, rowNum = 0;
        DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);
        LevelData[] array = new LevelData[rowNum - 2];
        for (int i = 2; i < rowNum; i++)
        {
            LevelData levelData = new LevelData();
            levelData.Id = int.Parse(collect[i][0].ToString());
            levelData.Name = collect[i][1].ToString();
            levelData.FrontLevelId = int.Parse(collect[i][2].ToString());
            levelData.RearLevelId = int.Parse(collect[i][3].ToString());
            levelData.Aim1 = collect[i][4].ToString();
            levelData.Aim2 = collect[i][5].ToString();
            levelData.Aim3 = collect[i][6].ToString();
            levelData.RewardBuildingId = int.Parse(collect[i][7].ToString());
            levelData.RewardTechId = int.Parse(collect[i][8].ToString());
            levelData.Length = int.Parse(collect[i][9].ToString());
            levelData.Width = int.Parse(collect[i][10].ToString());
            levelData.Introduce = collect[i][11].ToString();
            levelData.log = int.Parse(collect[i][12].ToString());
            levelData.rice = int.Parse(collect[i][13].ToString());
            levelData.money = int.Parse(collect[i][14].ToString());
            levelData.stone = int.Parse(collect[i][15].ToString());
            levelData.orderIds = StringToIntList(collect[i][16].ToString()).ToArray();
            levelData.orderNums = StringToIntList(collect[i][17].ToString()).ToArray();
            levelData.specialIntroduce = collect[i][18].ToString();
            levelData.AimMoney = int.Parse(collect[i][19].ToString());
            levelData.AimHappiness = int.Parse(collect[i][20].ToString());
            levelData.AimPopulation = int.Parse(collect[i][21].ToString());
            array[i - 2] = levelData;
        }
        return array;
    }

    public static FormulaData[] CreateFormulaArrayWithExcel(string filePath)
    {
        int columnNum = 0, rowNum = 0;
        DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);
        FormulaData[] array = new FormulaData[rowNum - 2];
        for (int i = 2; i < rowNum; i++)
        {
            FormulaData formulaData = new FormulaData();
            formulaData.ID = int.Parse(collect[i][0].ToString());
            formulaData.Describe = collect[i][1].ToString();
            formulaData.InputItemID = StringToIntList(collect[i][2].ToString());
            formulaData.InputNum = StringToIntList(collect[i][3].ToString());
            formulaData.OutputItemID = StringToIntList(collect[i][4].ToString());
            formulaData.ProductNum = StringToIntList(collect[i][5].ToString());
            formulaData.ProductTime = int.Parse(collect[i][6].ToString());
            array[i - 2] = formulaData;
        }
        return array;
    }

    public static TechData[] CreateTechArrayWithExcel(string filePath)
    {
        int columnNum = 0, rowNum = 0;
        DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);
        TechData[] array = new TechData[rowNum - 2];
        for (int i = 2; i < rowNum; i++)
        {
            TechData techData = new TechData();
            techData.Id = int.Parse(collect[i][0].ToString());
            techData.Name = collect[i][1].ToString();
            techData.Introduce = collect[i][2].ToString();
            techData.Group = int.Parse(collect[i][3].ToString());
            array[i - 2] = techData;
        }
        return array;
    }
    
    public static CarData[] CreateCarArrayWithExcel(string filePath)
    {
        int columnNum = 0, rowNum = 0;
        DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);
        CarData[] array = new CarData[rowNum - 2];
        for (int i = 2; i < rowNum; i++)
        {
            CarData carData = new CarData();
            carData.CarType = (TransportationType)int.Parse(collect[i][2].ToString());
            carData.Acceleration = float.Parse(collect[i][3].ToString());
            carData.MaxSpeed = float.Parse(collect[i][4].ToString());
            carData.Cost = float.Parse(collect[i][5].ToString());
            carData.Storage = float.Parse(collect[i][6].ToString());
            array[i - 2] = carData;
        }
        return array;
    }

    public static BuildData[] CreateBuildArrayWithExcel(string filePath)
    {
        int columnNum = 0, rowNum = 0;
        DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);
        BuildData[] array = new BuildData[rowNum - 2];
        for (int i = 2; i < rowNum; i++)
        {
            BuildData buildData = new BuildData();
            buildData.Id = int.Parse(collect[i][0].ToString());
            buildData.Name = collect[i][1].ToString();
            buildData.CostPerWeek = int.Parse(collect[i][3].ToString());
            buildData.Formulas = StringToIntList(collect[i][4].ToString());
            buildData.Population = int.Parse(collect[i][5].ToString());
            buildData.MaxStorage = int.Parse(collect[i][6].ToString());
            buildData.InfluenceRange = int.Parse(collect[i][7].ToString());
            if (collect[i][8].ToString() != "Null")
            {
                buildData.FrontBuildingId = int.Parse(collect[i][8].ToString());
            }
            if (collect[i][9].ToString() != "Null")
            {
                buildData.RearBuildingId = int.Parse(collect[i][9].ToString());
            }
            buildData.Length = int.Parse(collect[i][10].ToString());
            buildData.Width = int.Parse(collect[i][11].ToString());
            buildData.Price = int.Parse(collect[i][12].ToString());
            buildData.costResources = new List<CostResource>();
            List<int> ItemIDs = StringToIntList(collect[i][13].ToString());
            List<int> Nums = StringToIntList(collect[i][14].ToString());
            for (int j = 0; j < ItemIDs.Count; j++)
            {
                buildData.costResources.Add(new CostResource(ItemIDs[j],Nums[j]));
            }
            buildData.BundleName = collect[i][15].ToString();
            buildData.PfbName = collect[i][16].ToString();
            buildData.tabType = (BuildTabType)int.Parse(collect[i][17].ToString());
            buildData.Introduce = collect[i][18].ToString();
            buildData.Level = int.Parse(collect[i][19].ToString());
            if(collect[i][20].ToString() != "Null")
            {
                buildData.iconName = collect[i][20].ToString();
            }
            buildData.Times = float.Parse(collect[i][21].ToString());
            buildData.SortRank= int.Parse(collect[i][22].ToString());
            array[i - 2] = buildData;
        }
        return array;
    }
    
    public static OrderData[] CreateOrderDataArrayWithExcel(string filePath)
    {
        int columnNum = 0, rowNum = 0;
        DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);
        OrderData[] array = new OrderData[rowNum - 2];
        for (int i = 2; i < rowNum; i++)
        {
            OrderData orderData = new OrderData();
            orderData.ID = int.Parse(collect[i][0].ToString());
            orderData.Type = int.Parse(collect[i][1].ToString());
            orderData.ItemId = int.Parse(collect[i][2].ToString());
            orderData.ItemNum = int.Parse(collect[i][3].ToString());
            orderData.GoodsPrice = float.Parse(collect[i][4].ToString());
            orderData.Destination = collect[i][5].ToString();
            orderData.Distance = float.Parse(collect[i][6].ToString());
            orderData.StartRate = float.Parse(collect[i][7].ToString());
            orderData.MaxRate = float.Parse(collect[i][8].ToString());
            orderData.TimeLimit = int.Parse(collect[i][9].ToString());
            orderData.RepeatTime = int.Parse(collect[i][10].ToString());
            orderData.DescriptionIds = collect[i][11].ToString();
            orderData.LevelLimit = int.Parse(collect[i][12].ToString());
            orderData.CheckItemId = int.Parse(collect[i][13].ToString());
            orderData.AddRateTarget = StringToIntList(collect[i][14].ToString());
            array[i - 2] = orderData;
        }
        return array;
    }

    public static LocalizationData CreateLocalizationDataWithExcel(string filePath)
    {
        int columnNum = 0, rowNum = 0;
        DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);
        LocalizationData data = new LocalizationData();
        LocalizationCombine[] combines = new LocalizationCombine[rowNum - 1];
        for (int i = 1; i < rowNum; i++)
        {
            combines[i - 1] = new LocalizationCombine(collect[i][0].ToString(),
                collect[i][1].ToString(), collect[i][2].ToString(),collect[i][3].ToString());
        }
        data.combines = combines;
        return data;

    }
    static DataRowCollection ReadExcel(string filePath, ref int columnNum, ref int rowNum)
    {
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        DataSet result = excelReader.AsDataSet();
        //Tables[0] 下标0表示excel文件中第一张表的数据
        columnNum = result.Tables[0].Columns.Count;
        rowNum = result.Tables[0].Rows.Count;
        return result.Tables[0].Rows;
    }

    public static List<int> StringToIntList(string str)
    {
        List<int> list = new List<int>();
        if (str == "Null")
        {
            return list;
        }
        string[] array = str.Split(',');
        for (int i = 0; i < array.Length; i++)
        {
            list.Add(int.Parse(array[i]));
        }
        return list;
    }
}
