using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class LoadAB : MonoBehaviour
{

    static Dictionary<string, AssetBundle> _abDic = new Dictionary<string, AssetBundle>();
    static AssetBundleManifest manifest;
    static Dictionary<string, GameObject> _objDic = new Dictionary<string, GameObject>();
    public static void Init()
    {
        AssetBundle assetBundle = AssetBundle.LoadFromFile("AssetBundles/AssetBundles");
        manifest = assetBundle.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
        Load("building.ab", "WheatPfb");

    }
    public static GameObject Load(string bundleName, string name)
    {
        AssetBundle ab;
        if (_abDic.ContainsKey(bundleName))
        {
            _abDic.TryGetValue(bundleName, out ab);
        }
        else
        {
            ab = AssetBundle.LoadFromFile("AssetBundles/" + bundleName);
            _abDic.Add(bundleName, ab);
            string[] dependencies = manifest.GetAllDependencies(bundleName);
            foreach (string dependency in dependencies)
            {
                //Debug.Log("依赖项：" + dependency);
                if (!_abDic.ContainsKey(dependency))
                {
                    AssetBundle dep = AssetBundle.LoadFromFile("AssetBundles/" + dependency);
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

    public static T Load<T>(string bundleName, string name) where T : Object
    {
        AssetBundle ab;
        if (_abDic.ContainsKey(bundleName))
        {
            _abDic.TryGetValue(bundleName, out ab);
        }
        else
        {
            ab = AssetBundle.LoadFromFile("AssetBundles/" + bundleName);
            _abDic.Add(bundleName, ab);
            string[] dependencies = manifest.GetAllDependencies(bundleName);
            foreach (string dependency in dependencies)
            {
                Debug.Log("依赖项：" + dependency);
                if (!_abDic.ContainsKey(dependency))
                {
                    AssetBundle dep = AssetBundle.LoadFromFile("AssetBundles/" + dependency);
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
            ab = AssetBundle.LoadFromFile("AssetBundles/" + bundleName);
            _abDic.Add(bundleName, ab);
            string[] dependencies = manifest.GetAllDependencies(bundleName);
            foreach (string dependency in dependencies)
            {
                Debug.Log("依赖项：" + dependency);
                if (!_abDic.ContainsKey(dependency))
                {
                    AssetBundle dep = AssetBundle.LoadFromFile("AssetBundles/" + dependency);
                    _abDic.Add(dependency, dep);
                }
            }
        }
        Sprite sp = ab.LoadAsset<Sprite>(name);
        return sp;
    }

    public static T LoadAsset<T>(string abName, string prefabName) where T : UnityEngine.Object
    {
        AssetBundle bundle = AssetBundle.LoadFromFile("AssetBundles/" + abName);
        return bundle.LoadAsset(prefabName) as T;
    }

}


