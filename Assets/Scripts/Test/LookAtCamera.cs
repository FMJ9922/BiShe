using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    
    void FixedUpdate()
    {
        transform.LookAt(Camera.main.transform);
    }
}
