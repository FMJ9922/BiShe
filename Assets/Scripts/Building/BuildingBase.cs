using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Building
{
    public class BuildingBase : MonoBehaviour
    {
        //占地长宽
        [SerializeField]
        private int Width;
        [SerializeField]
        private int Height;

        public Vector2Int Size => new Vector2Int(Height, Width);

        public bool hasAnima = false;

        public Animation animation;

        public GameObject body;

        public RuntimeBuildData runtimeBuildData;

        public bool buildFlag = false;//用于判断是否是来自存档的建造

        public Vector2Int[] takenGrids;

        public Vector2Int parkingGridIn;
        
        protected Storage storage;

    }

    [System.Serializable] 
    public class RuntimeBuildData : BuildData
    {
        public bool Pause = false;//是否暂停生产(因为缺少原料)
        public int CurLevel = 0;//当前等级
        public int CurFormula;//当前配方
        public FormulaData[] formulaDatas;//当前建筑可用的配方们
        public int CurPeople = 0;//当前建筑的工人或居民
        public float Rate = 0;//当前生产进度0-1
        public float Effectiveness;//生产效率
        public float Happiness = 0.6f;//当前幸福程度
        public int productTime;//生产周期（周）
        public bool AvaliableToMarket = true;//可以到达市场

        public Vector3Serializer SavePosition;
        public Vector2IntSerializer[] SaveTakenGrids;
        public Direction SaveDir;
        public int SaveOutLookType;
        
        public FormulaData formula;//正在使用的配方
        public Direction direction;
    }
}