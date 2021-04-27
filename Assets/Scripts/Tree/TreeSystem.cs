using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TreeSystem : MonoBehaviour
{
    public void TreeGrow()
    {

    }

    public void TreeCutDown(Vector3 angle)
    {
        transform.DOLocalRotate(angle + new Vector3(90,0,0), 2f).OnComplete(()=> { Destroy(gameObject); });
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Tree") && !other.CompareTag("Ground"))
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
