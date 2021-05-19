using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorViewer : MonoBehaviour
{
    #region 组件
    [SerializeField] private SpriteRenderer _spriteRenderer;
    #endregion

    #region 字段&属性
    [SerializeField] private Sprite[] _sprites;
    private Vector3 _fromVector;
    private Vector3 _toVector;
    private int _lastIndex;

    #endregion

    private void Start()
    {
        AddListener();
    }
    private void OnDestroy()
    {
        RemoveListener();
    }
    private void AddListener()
    {
        //EventManager.StartListening(ConstEvent.OnCameraMove, ChangeSprite);
    }
    private void RemoveListener()
    {
        //EventManager.StartListening(ConstEvent.OnCameraMove, ChangeSprite);
    }

    /// <summary>
    /// 当摄像机位置和角度变化时，改变渲染的图片
    /// </summary>
    private void ChangeSprite()
    {
        _fromVector = PlantVector3(-transform.parent.forward);
        _toVector = PlantVector3(transform.position - Camera.main.transform.position);
        float angle = Vector3.Angle(_fromVector, _toVector); //求出两向量之间的夹角
        Vector3 normal = Vector3.Cross(_fromVector, _toVector);//叉乘求出法线向量
        angle *= Mathf.Sign(Vector3.Dot(normal, Vector3.up));  //求法线向量与物体上方向向量点乘，结果为1或-1，修正旋转方向
        Debug.Log(angle);
        transform.LookAt(Camera.main.transform.position);
        int index = (Mathf.Clamp(Mathf.FloorToInt(8 * (angle + 180) / 360), 0, 7) + 9) % 8;
        if (_lastIndex != index)
        {
            CrossChangeSpriteByIndex(index);
            _lastIndex = index;
        }
        
    }

    public void CrossChangeSpriteByIndex(int index)
    {        
        _spriteRenderer.sprite = _sprites[index];
    }
    private Vector3 PlantVector3(Vector3 vector3)
    {
        return new Vector3(vector3.x, 0, vector3.z);
    }
}
