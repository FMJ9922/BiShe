/*using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.IO;

public class AddressableAddressSetter : MonoBehaviour
{
    static AssetBundleManifest manifest;
    private static string path;
    [MenuItem("Tools/Addressables/Set Addresses by BundleName")]
    public static void SetAddressesByBundleName()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable settings not found.");
            return;
        }

        // 遍历所有资源
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string assetPath in allAssetPaths)
        {
            // 忽略非资源路径（如 .meta 文件）
            if (!assetPath.StartsWith("Assets/") || assetPath.EndsWith(".meta"))
                continue;

            // 获取资源的 GUID
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
                continue;

            // 获取 Addressables 中的资源入口（Entry）
            AddressableAssetEntry entry = settings.FindAssetEntry(guid);
            if (entry == null)
                continue; // 跳过未添加到 Addressables 的资源

            // 获取资源所属的 Addressables 组名（作为 bundleName）
            string bundleName = entry.parentGroup.Name;

            // 生成 Address：bundleName/文件名（不带后缀）
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string address = $"{bundleName}/{fileName}";

            // 设置 Address
            entry.address = address;
            Debug.Log(assetPath +"=>\n"+address);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Addresses set successfully!");
    }
}*/