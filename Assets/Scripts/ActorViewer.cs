using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorViewer : MonoBehaviour
{
    public Sprite[] sprites;
    Vector3 fromVector;
    Vector3 toVector;
    public SpriteRenderer spriteRenderer;

    private void Update()
    {
        fromVector = PlantVector3(-transform.parent.forward);
        toVector = PlantVector3(transform.position - Camera.main.transform.position);
        float angle = Vector3.Angle(fromVector, toVector); //求出两向量之间的夹角
        Vector3 normal = Vector3.Cross(fromVector, toVector);//叉乘求出法线向量
        angle *= Mathf.Sign(Vector3.Dot(normal, Vector3.up));  //求法线向量与物体上方向向量点乘，结果为1或-1，修正旋转方向
        transform.LookAt(Camera.main.transform.position);
        Debug.Log(Mathf.FloorToInt(8 * (angle + 180) / 360));
        int index = (Mathf.Clamp(Mathf.FloorToInt(8 * (angle + 180) / 360), 0, 7)+9)%8;
        spriteRenderer.sprite = sprites[index];
        Debug.Log(angle);
    }

    private Vector3 PlantVector3(Vector3 vector3)
    {
        return new Vector3(vector3.x, 0, vector3.z);
    }
}
