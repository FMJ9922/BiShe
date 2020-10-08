using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class LoadAB : MonoBehaviour
{
    static Dictionary<string, AssetBundle> _abDic = new Dictionary<string, AssetBundle>();
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
        }
        return ab.LoadAsset<GameObject>(name);
    }

    /*public static GameObject LoadAsyc(BundleType bundleType, string name)
    {
        var bundleLoadRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, bundleType.ToString()));
        yield return bundleLoadRequest;
    }*/
}


