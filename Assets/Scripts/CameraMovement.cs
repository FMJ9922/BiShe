using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed = 0.5f;
    public Transform target;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += PlantVector3(transform.forward) * speed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= PlantVector3(transform.forward) * speed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= PlantVector3(transform.right) * speed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += PlantVector3(transform.right * speed);
        }
        /*if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(new Vector3(0, -0.3f, 0), Space.World);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(new Vector3(0, 0.3f, 0), Space.World);
        }*/
        transform.LookAt(target);
    }

    private Vector3 PlantVector3(Vector3 vector3)
    {
        return new Vector3(vector3.x, 0, vector3.z);
    }
}
