using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookRotation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.LookAt(transform.position-new Vector3(1,0,1),Vector3.up);
    }

}
