using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByCar : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("car"))
        {
            //Debug.Log("delete");
            Destroy(this.gameObject);
        }
    }
}
