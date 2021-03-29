using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPreview : MonoBehaviour
{
    [SerializeField] Material[] level;
    [SerializeField] MeshRenderer meshRenderer;
    public void SetRoadPreviewMat(int i)
    {
        meshRenderer.material = level[i-1];
    }
}
