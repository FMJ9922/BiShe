using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantController : MonoBehaviour
{
    public MeshRenderer mesh;

    public void SetMat(Material mat)
    {
        mesh.sharedMaterial = mat;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("car"))
        {
            Hide();
        }
    }

    private void Hide()
    {
        transform.position -= Vector3.up*1000;
    }
    
    public void Show()
    {
        transform.position += Vector3.up * 1000;
    }
}
