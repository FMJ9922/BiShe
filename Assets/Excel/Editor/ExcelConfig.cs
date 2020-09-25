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
    public static readonly string excelsFolderPath = Application.dataPath + "/Resources/Data/RawData";

    /// <summary>
    /// 存放Excel转化CS文件的文件夹路径
    /// </summary>
    public static readonly string assetPath = "Assets/Resources/Data/ScriptData/";
}

public class ExcelTool
{

    /// <summary>
    /// 读取表数据，生成对应的数组
    /// </summary>
    /// <param name="filePath">excel文件全路径</param>
    /// <returns>Item数组</returns>
    public static BuildingItem[] CreateItemArrayWithExcel(string filePath)
    {
        //获得表数据
        int columnNum = 0, rowNum = 0;
        DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);

        Debug.Log(columnNum+" "+rowNum);
        //根据excel的定义，第二行开始才是数据
        BuildingItem[] array = new BuildingItem[rowNum - 1];
        for (int i = 1; i < rowNum; i++)
        {
            BuildingItem item = new BuildingItem();
            //解析每列的数据
            item.Id = uint.Parse(collect[i][0].ToString());
            item.Name = collect[i][1].ToString();
            item.Cost = uint.Parse(collect[i][2].ToString());
            item.Rate = uint.Parse(collect[i][3].ToString());
            item.Price = uint.Parse(collect[i][4].ToString());
            array[i - 1] = item;
        }
        return array;
    }

    /// <summary>
    /// 读取excel文件内容
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="columnNum">行数</param>
    /// <param name="rowNum">列数</param>
    /// <returns></returns>
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
