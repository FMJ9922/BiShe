using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TreeSystem : MonoBehaviour
{
    public enum TreeState
    {
        unInit = -1,
        growing = 0,
        mature = 1,
        dead = 2,
    }
    public TreeState state = TreeState.unInit;
    private float curHeight = 0;
    private float targetHeight;

    private void Start()
    {
        if(state == TreeState.unInit)
        {
            state = TreeState.growing;
            curHeight = 0;
            targetHeight = Random.Range(0.8f, 1.2f);
            InvokeRepeating("TreeGrow", 0, 4f*LevelManager.Instance.DayTime*7);
        }
    }

    public float GetGrowProgress()
    {
        return curHeight / targetHeight;
    }
    public void TreeGrow()
    {
        if(curHeight < targetHeight)
        {
            curHeight += 0.1f;
            transform.localScale = Vector3.one * curHeight;
        }
        else
        {
            state = TreeState.mature;
            CancelInvoke();
        }
    }

    public void TreeCutDown(Vector3 angle)
    {
        state = TreeState.dead;
        transform.DOLocalRotate(angle + new Vector3(90,0,0), 2f).OnComplete(()=> { Destroy(gameObject); });

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

}
