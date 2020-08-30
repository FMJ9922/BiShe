using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LoadAB : MonoBehaviour {

    public enum BundleType
    {
        Default,
        Wall,
    }
	public static GameObject Load(BundleType bundleType ,string name)
    {
        AssetBundle ab = AssetBundle.LoadFromFile(bundleType.ToString());
        return ab.LoadAsset<GameObject>(name);
    }

    /*public static GameObject LoadAsyc(BundleType bundleType, string name)
    {
        var bundleLoadRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, bundleType.ToString()));
        yield return bundleLoadRequest;
    }*/
}
