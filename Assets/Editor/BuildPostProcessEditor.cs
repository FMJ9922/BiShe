using System.IO;
using UnityEditor;
using UnityEngine;
namespace Editor
{
    public class BuildPostProcessEditor : UnityEditor.Editor
    {
        private static string _srcPathAssetBundles = "C:/Users/FMJ/Documents/GitHub/BiShe/AssetBundles";
        private static string _desPathAssetBundles = "C:/Users/FMJ/Documents/GitHub/CountrySide/AssetBundles";
        private static string _srcPathData = "C:/Users/FMJ/Documents/GitHub/BiShe/Assets/Resources/Data/ScriptData";
        private static string _desPathData = "C:/Users/FMJ/Documents/GitHub/CountrySide/Assets/Resources/Data/ScriptData";
        private static string _desSavePath = "C:/Users/FMJ/Documents/GitHub/CountrySide/Countryside_Data/Resources/Saves";


        [MenuItem("Tools/Build/DoBuildPostProcess")]
        public static void DoBuildPostProcess()
        {
            string path = Application.streamingAssetsPath;
            string path2 = Application.persistentDataPath;
            string dataPath = Application.dataPath;
            Debug.Log("streaming " + path);
            Debug.Log("persistent " + path2);
            Debug.Log("data " + dataPath);
            DoCopy(_srcPathAssetBundles, _desPathAssetBundles);
            DoCopy(_srcPathData,_desPathData);
            if (!Directory.Exists(_desSavePath))
            {
                Directory.CreateDirectory(_desSavePath);
            }
        }

        private static void DoCopy(string src, string des)
        {
            if (!File.Exists(src) && !Directory.Exists(src))
            {
                Debug.LogError($"拷贝文件失败不存在 源目录：{src}");
                return;
            }
            var allFile = Directory.GetFiles(src, "*", SearchOption.AllDirectories);
            for (int i = 0; i < allFile.Length; i++)
            {
                string desFile;
                desFile = allFile[i].Replace(src, des);
                if (File.Exists(desFile))
                {
                    FileUtil.ReplaceFile(allFile[i], desFile);
                }
                else
                {
                    var folder = Path.GetDirectoryName(desFile);
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    FileUtil.CopyFileOrDirectory(allFile[i], desFile);
                }  
            }
        }
    }
}
