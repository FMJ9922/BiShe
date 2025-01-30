using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;

public class LoadAB : MonoBehaviour
{
    // 缓存已加载的资源句柄（替代原有的AssetBundle缓存）
    private static Dictionary<string, AsyncOperationHandle> _assetHandles = new Dictionary<string, AsyncOperationHandle>();
    // 缓存已实例化的GameObject（可选，根据需求保留）
    private static Dictionary<string, GameObject> _objDic = new Dictionary<string, GameObject>();

    // 初始化Addressables（可选，根据需求）
    public static void Init()
    {
        // Addressables初始化通常不需要手动调用
        // 保留原有调用逻辑（如果需要）
        Load("building.ab", "WheatPfb");
    }

    // 同步加载GameObject（外部接口保持不变）
    public static GameObject Load(string bundleName, string name)
    {
        // 合并bundleName和资源名作为唯一标识（根据实际Addressables配置调整）
        string address = $"{bundleName}/{name}";
        
        // 如果已缓存，直接返回
        if (_objDic.ContainsKey(address))
        {
            return _objDic[address];
        }

        // 同步加载（注意：Addressables默认异步，此处强制同步）
        var handle = Addressables.LoadAssetAsync<GameObject>(address);
        handle.WaitForCompletion(); // 阻塞主线程，慎用！

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject obj = handle.Result;
            _objDic.Add(address, obj);
            return obj;
        }
        else
        {
            Debug.LogError($"Failed to load {address}: {handle.OperationException}");
            return null;
        }
    }

    // 泛型方法加载资源（支持Sprite、Prefab等）
    public static T Load<T>(string bundleName, string name) where T : UnityEngine.Object
    {
        string address = $"{bundleName}/{name}";
        
        // 如果已加载，直接返回
        if (_assetHandles.ContainsKey(address))
        {
            return (T)_assetHandles[address].Result;
        }

        // 同步加载
        var handle = Addressables.LoadAssetAsync<T>(address);
        handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _assetHandles.Add(address, handle);
            return handle.Result;
        }
        else
        {
            Debug.LogError($"Failed to load {address}: {handle.OperationException}");
            return null;
        }
    }

    // 加载Sprite（复用泛型方法）
    public static Sprite LoadSprite(string bundleName, string name)
    {
        return Load<Sprite>(bundleName, name);
    }

    // 加载任意资源（兼容旧接口）
    public static T LoadAsset<T>(string bundleName, string name) where T : UnityEngine.Object
    {
        return Load<T>(bundleName, name);
    }

    // 释放资源（可选）
    public static void Release(string bundleName, string name)
    {
        string address = $"{bundleName}/{name}";
        if (_assetHandles.ContainsKey(address))
        {
            Addressables.Release(_assetHandles[address]);
            _assetHandles.Remove(address);
        }
        if (_objDic.ContainsKey(address))
        {
            _objDic.Remove(address);
        }
    }
}
/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.AddressableAssets;
public class LoadAB : MonoBehaviour
{

    static Dictionary<string, AssetBundle> _abDic = new Dictionary<string, AssetBundle>();
    static AssetBundleManifest manifest;
    static Dictionary<string, GameObject> _objDic = new Dictionary<string, GameObject>();

    private static string path;
    public static void Init()
    {
        if(Application.platform == RuntimePlatform.WindowsEditor){
            path = "Assets/StreamingAssets/";
        }
        else{
            path = "Countryside_Data/StreamingAssets/";
        }
        AssetBundle assetBundle = AssetBundle.LoadFromFile(path + "AssetBundles");
        manifest = assetBundle.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
        Load("building.ab", "WheatPfb");

    }
    public static GameObject Load(string bundleName, string name)
    {
        //Addressables.LoadAssetAsync<GameObject>(name);
        AssetBundle ab;
        if (_abDic.ContainsKey(bundleName))
        {
            _abDic.TryGetValue(bundleName, out ab);
        }
        else
        {
            ab = AssetBundle.LoadFromFile(path + bundleName);
            _abDic.Add(bundleName, ab);
            string[] dependencies = manifest.GetAllDependencies(bundleName);
            foreach (string dependency in dependencies)
            {
                //Debug.Log("依赖项：" + dependency);
                if (!_abDic.ContainsKey(dependency))
                {
                    AssetBundle dep = AssetBundle.LoadFromFile(path + dependency);
                    _abDic.Add(dependency, dep);
                }
            }
        }
        if (_objDic.ContainsKey(name))
        {
            return _objDic[name];
        }
        else
        {
            GameObject obj = ab.LoadAsset<GameObject>(name);
            _objDic.Add(name, obj);
            return obj;
        }
    }

    public static T Load<T>(string bundleName, string name) where T : UnityEngine.Object
    {
        AssetBundle ab;
        if (_abDic.ContainsKey(bundleName))
        {
            _abDic.TryGetValue(bundleName, out ab);
        }
        else
        {
            ab = AssetBundle.LoadFromFile(path + bundleName);
            _abDic.Add(bundleName, ab);
            string[] dependencies = manifest.GetAllDependencies(bundleName);
            foreach (string dependency in dependencies)
            {
                Debug.Log("依赖项：" + dependency);
                if (!_abDic.ContainsKey(dependency))
                {
                    AssetBundle dep = AssetBundle.LoadFromFile(path + dependency);
                    _abDic.Add(dependency, dep);
                }
            }
        }

        return ab.LoadAsset<T>(name);
    }
    public static Sprite LoadSprite(string bundleName, string name)
    {
        AssetBundle ab;
        if (_abDic.ContainsKey(bundleName))
        {
            _abDic.TryGetValue(bundleName, out ab);
        }
        else
        {
            ab = AssetBundle.LoadFromFile(path + bundleName);
            _abDic.Add(bundleName, ab);
            string[] dependencies = manifest.GetAllDependencies(bundleName);
            foreach (string dependency in dependencies)
            {
                Debug.Log("依赖项：" + dependency);
                if (!_abDic.ContainsKey(dependency))
                {
                    AssetBundle dep = AssetBundle.LoadFromFile(path + dependency);
                    _abDic.Add(dependency, dep);
                }
            }
        }
        Sprite sp = ab.LoadAsset<Sprite>(name);
        return sp;
    }

    public static T LoadAsset<T>(string abName, string prefabName) where T : UnityEngine.Object
    {
        AssetBundle bundle = AssetBundle.LoadFromFile(path + abName);
        return bundle.LoadAsset(prefabName) as T;
    }

}*/


