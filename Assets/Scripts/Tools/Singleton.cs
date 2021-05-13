using UnityEngine;
using System.Collections;

/// <summary>
/// Instance Template Base Class
/// </summary>
/// <typeparam name = "T"> </typeparma>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            return Singleton<T>._instance;
        }
    }

    // Use this for initialization
    void Awake()
    {
        if (null == _instance)
        {
            _instance = GetComponent<T>();
        }
        else
        {
            Debug.Log("manager has already been created previously. " + gameObject.name + " is goning to be destroyed.");
            Destroy(this);
            return;
        }


        InstanceAwake();
    }

    protected virtual void InstanceAwake()
    {
    }

    public void OnApplicationQuit()
    {
        _instance = null;
    }
}