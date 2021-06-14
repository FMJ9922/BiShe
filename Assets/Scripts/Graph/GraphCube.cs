using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GraphCube : MonoBehaviour
{
    [SerializeField] Transform cube;
    [SerializeField] Transform textTrans;
    [SerializeField] private TMP_Text label;
    [SerializeField] MeshRenderer _renderer;
    float staticHeight = 0.5f;

    public void SetHeight(float height,Vector3 position,Color color)
    {
        cube.transform.localScale = new Vector3(5, height + staticHeight, 5);
        cube.transform.position = position + new Vector3(0, height/2+staticHeight, 0);
        textTrans.position = position + new Vector3(0, height + staticHeight+3, -2.5f);
        _renderer.material.color = color;
    }

    public void SetLabel(string num)
    {
        label.text = num;
    }

}
