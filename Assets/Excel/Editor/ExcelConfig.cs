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
        ItemData[] array = new ItemData[rowNum - 1];
        for (int i = 1; i < rowNum; i++)
        {
            ItemData item = new ItemData();
            item.Id = uint.Parse(collect[i][0].ToString());
            item.Name = collect[i][1].ToString();
            item.Cost = uint.Parse(collect[i][2].ToString());
            item.Rate = uint.Parse(collect[i][3].ToString());
            item.Price = uint.Parse(collect[i][4].ToString());
            array[i - 1] = item;
        }
        return array;
    }

    public static LevelData[] CreateLevelArrayWithExcel(string filePath)
    {
        int columnNum = 0, rowNum = 0;
        DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);
        LevelData[] array = new LevelData[rowNum - 1];
        for (int i = 1; i < rowNum; i++)
        {
            
        }
        return array;
    }

    public static TechData[] CreateTechArrayWithExcel(string filePath)
    {
        int columnNum = 0, rowNum = 0;
        DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);
        TechData[] array = new TechData[rowNum - 1];
        for (int i = 1; i < rowNum; i++)
        {

        }
        return array;
    }

    public static BuildData[] CreateBuildArrayWithExcel(string filePath)
    {
        int columnNum = 0, rowNum = 0;
        DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);
        BuildData[] array = new BuildData[rowNum - 1];
        for (int i = 1; i < rowNum; i++)
        {

        }
        return array;
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
