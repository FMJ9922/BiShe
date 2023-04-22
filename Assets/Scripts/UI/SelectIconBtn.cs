using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class SelectIconBtn : MonoBehaviour, IPointerClickHandler
{
    #region 组件
    #endregion

    #region 字段&属性

    //该槽位可选科技的类型,读配置
    public TechType techType;
    //该槽位已选的建筑类型，玩家选择
    [FormerlySerializedAs("buildingType")] public EBuildingType eBuildingType;
    #endregion

    #region 接口

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }

    #endregion
}
