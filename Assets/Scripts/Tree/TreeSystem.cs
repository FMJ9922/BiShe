using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class TreeData
{
    public TreeState state = TreeState.unInit;
    public float curHeight = -101;
    public float targetHeight;
    public int indexType;
    public int counter = 28;
}

[System.Serializable]
public enum TreeState
{
    unInit = -1,
    growing = 0,
    mature = 1,
    dead = 2,
}
public class TreeSystem : MonoBehaviour
{
    public static bool pause;
    public TreeData treeData;

    private void Start()
    {
        Init();
    }

    public void SetData(TreeData sys)
    {
        treeData = sys;
        Init();
    }

    public void Init()
    {
        if (IsInvoking()|| treeData.state == TreeState.mature) return;
        if (treeData.state == TreeState.unInit)
        {
            treeData.state = TreeState.growing;
            treeData.curHeight = 0f;
            treeData.targetHeight = Random.Range(0.8f, 1.2f);
            TreeGrow();
            InvokeRepeating("TreeGrow", 0, LevelManager.Instance.DayTime*28);
        }
        else
        if (treeData.curHeight < treeData.targetHeight)
        {

            transform.localScale = Vector3.one * (treeData.curHeight / treeData.targetHeight);
            InvokeRepeating("TreeGrow", 0, LevelManager.Instance.DayTime*28);
        }
        else if (treeData.state == TreeState.dead)
        {
            Destroy(gameObject);
        }
    }

    public float GetGrowProgress()
    {
        return treeData.curHeight / treeData.targetHeight;
    }
    public void TreeGrow()
    {
        if (pause) return;
        if (treeData.curHeight < treeData.targetHeight)
        {
            treeData.curHeight += 0.1f;
            transform.localScale = Vector3.one * treeData.curHeight;
        }
        else
        {
            treeData.state = TreeState.mature;
            CancelInvoke();
        }
    }

    public void TreeCutDown(Vector3 angle)
    {
        treeData.state = TreeState.dead;
        transform.DOLocalRotate(angle + new Vector3(90, 0, 0), 2f).OnComplete(() => { Destroy(gameObject); });

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Building") || other.CompareTag("car"))
        {
            Vector3 d = other.transform.position - transform.position;
            d = new Vector3(d.x, 0, d.z);
            d = d.normalized;
            transform.forward = -d;
            Vector3 angle = transform.rotation.eulerAngles;
            TreeCutDown(angle);
        }
    }

    [ContextMenu("Cast")]
    public void CastNameToIntType()
    {
        string name = transform.name;
        if (name.Substring(20, 5) == "Light")
        {
            treeData.indexType = int.Parse(name.Substring(27, 1)) + 8;
        }
        else
        {
            treeData.indexType = int.Parse(name.Substring(26, 1))-1;
        }
    }

}
