using System.IO;
using UnityEditor;
using UnityEngine;

namespace Excel.Editor
{
    public class ExcelBuild : UnityEditor.Editor
    {

        [MenuItem("Tools/Excel/CreateItemAsset")]
        public static void CreateItemAsset()
        {
            DataManager manager = ScriptableObject.CreateInstance<DataManager>();
            //赋值
            manager.ItemArray = ExcelTool.CreateItemArrayWithExcel(ExcelConfig.excelsFolderPath + "1Item.xlsx");
            manager.BuildArray = ExcelTool.CreateBuildArrayWithExcel(ExcelConfig.excelsFolderPath + "2Buildings.xlsx");
            manager.LevelArray = ExcelTool.CreateLevelArrayWithExcel(ExcelConfig.excelsFolderPath + "3Levels.xlsx");
            manager.TechArray = ExcelTool.CreateTechArrayWithExcel(ExcelConfig.excelsFolderPath + "4Tech.xlsx");
            manager.LocalizationData = ExcelTool.CreateLocalizationDataWithExcel(ExcelConfig.excelsFolderPath + "Localization.xlsx");
            manager.FormulaArray = ExcelTool.CreateFormulaArrayWithExcel(ExcelConfig.excelsFolderPath + "5Formula.xlsx");
            manager.CarArray = ExcelTool.CreateCarArrayWithExcel(ExcelConfig.excelsFolderPath + "Car.xlsx");
            manager.OrderArray = ExcelTool.CreateOrderDataArrayWithExcel(ExcelConfig.excelsFolderPath + "6MarketOrder.xlsx");
            manager.ChooseSkillArray = ExcelTool.CreateChooseSkillDataWithExcel(ExcelConfig.excelsFolderPath + "7ChooseSkill.xlsx");
            manager.SkillBuffArray = ExcelTool.CreateSkillBuffDataWithExcel(ExcelConfig.excelsFolderPath + "8SkillBuff.xlsx");
            //确保文件夹存在
            if (!Directory.Exists(ExcelConfig.assetPath))
            {
                Directory.CreateDirectory(ExcelConfig.assetPath);
            }

            //asset文件的路径 要以"Assets/..."开始，否则CreateAsset会报错
            string assetPath = string.Format("{0}{1}.asset", ExcelConfig.assetPath, "Data");
            //生成一个Asset文件
            AssetDatabase.CreateAsset(manager, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("生成成功！");
        }
    }
}
