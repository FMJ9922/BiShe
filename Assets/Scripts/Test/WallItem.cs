using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallItem : MonoBehaviour,Wall
{
	//前后左右四个连接点
	[SerializeField]
	protected  GameObject[] linkPos;
	[SerializeField]
	public LinkPosInfo[] linkInfo;
	//连接点对应的枚举类
	protected Dictionary<GameObject, LinkPosInfo> posDic;

	public Vector3 CenterPos;

	public float width;
	/// <summary>
	/// 初始化连接点信息
	/// </summary>
	protected virtual void InitPosInfo()
    {
		posDic = new Dictionary<GameObject, LinkPosInfo>();
		for (int i = 0; i < linkPos.Length; i++)
		{
			posDic.Add(linkPos[i], linkInfo[i]);
		}
		/*CenterPos = Vector3.Lerp(linkPos[0].transform.localPosition,
							     linkPos[1].transform.localPosition, 0.5f);
		width = Vector3.Distance(linkPos[0].transform.localPosition,
								 linkPos[1].transform.localPosition);*/
	}
	
	
	
}

[Serializable]
public class LinkPosInfo
{
	public LinkPosType LinkPosType;
	public bool isAvalible;
}
public interface Wall
{

}
public enum WallType
{
	Straight,
	corner,
	Gate
}
/// <summary>
/// A只能和B,C相连，B只能和A,C相连
/// </summary>
public enum LinkPosType
{
	A,
	B,
	C
}

