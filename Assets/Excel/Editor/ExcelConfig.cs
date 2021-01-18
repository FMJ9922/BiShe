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
            item.ProductCycle = int.Parse(collect[i][2].ToString());
            item.Price = int.Parse(collect[i][3].ToString());
            item.BuildingID = int.Parse(collect[i][4].ToString());
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
            levelData.wood = int.Parse(collect[i][12].ToString());
            levelData.stone = int.Parse(collect[i][13].ToString());
            levelData.money = int.Parse(collect[i][14].ToString());
            array[i - 2] = levelData;
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
            techData.Cost = int.Parse(collect[i][2].ToString());
            techData.Introduce = collect[i][3].ToString();
            array[i - 2] = techData;
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
            buildData.Length = int.Parse(collect[i][2].ToString());
            buildData.Width = int.Parse(collect[i][3].ToString());
            buildData.Price = int.Parse(collect[i][4].ToString());
            buildData.costResources = new List<CostResource>();
            for (int j = 5; j <= 9; j += 2)
            {
                if (collect[i][j].ToString() != "Null")
                {
                    buildData.costResources.Add(new CostResource(
                        int.Parse(collect[i][j].ToString()),
                        int.Parse(collect[i][j + 1].ToString()))
                        );
                }
            }
            buildData.Return = int.Parse(collect[i][11].ToString());
            if (collect[i][12].ToString() != "Null")
            {
                buildData.ProductId = int.Parse(collect[i][12].ToString());
            }
            buildData.ProductTime = int.Parse(collect[i][13].ToString());
            buildData.ProductNum = float.Parse(collect[i][14].ToString());
            buildData.WorkerNum = int.Parse(collect[i][15].ToString());
            buildData.MaxStorage = int.Parse(collect[i][16].ToString());
            buildData.InfluenceRange = int.Parse(collect[i][17].ToString());
            if (collect[i][18].ToString() != "Null")
            {
                buildData.FrontBuildingId = int.Parse(collect[i][18].ToString());
            }
            if (collect[i][19].ToString() != "Null")
            {
                buildData.FrontBuildingId = int.Parse(collect[i][19].ToString());
            }
            buildData.BundleName = collect[i][20].ToString();
            buildData.PfbName = collect[i][21].ToString();
            buildData.tabType = (BuildTabType)int.Parse(collect[i][22].ToString());
            array[i - 2] = buildData;
        }
        return array;
    }

    public static LocalizationData CreateLocalizationDataWithExcel(string filePath)
    {
        int columnNum = 0, rowNum = 0;
        DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);
        LocalizationData data= new LocalizationData();
        LocalizationCombine[] combines = new LocalizationCombine[rowNum-1];
        for (int i = 1; i < rowNum; i++)
        {
            combines[i - 1] = new LocalizationCombine(collect[i][0].ToString(),
                collect[i][1].ToString(), collect[i][2].ToString());
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
}
