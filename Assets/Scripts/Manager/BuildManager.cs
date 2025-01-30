using System;
using System.Collections;
using System.Collections.Generic;
using Building;
using CSTools;
using Manager;
using UI;
using UnityEngine;
using UnityEngine.Events;

public class SelectLightInfo
{
    public Vector3 pos;
    public Quaternion quaternion;
    public Vector3 scale;
    public Color color = Color.white;
}
public class BuildManager : Singleton<BuildManager>
{
    #region
    //[SerializeField]
    //private GameObject gridHightLight;
    [SerializeField]
    private Material mat_road_green;
    [SerializeField]
    private Material mat_road_red;
    [SerializeField]
    private GameObject preRoadPfb;
    [SerializeField]
    private ParticleSystem dustParticle;
    [SerializeField]
    private GameObject arrowPfb;
    [SerializeField]
    private Transform roadParent;
    [SerializeField]
    private GameObject selectLight;
    [SerializeField]
    private GameObject rangeLight;

    private Transform arrows;

    //修建筑相关
    private BuildingBase currentBuilding;
    private bool isCurCanBuild = false;//当前建筑是否重叠
    private bool isTurn = false;//当前建筑是否旋转
    private Vector2Int[] targetGrids;
    private Vector2Int lastGrid;
    private Material[] mats;
    private Vector3 hidePos = new Vector3(0, -100, 0);
    private Direction lastDir = Direction.right;
    private SelectLightInfo lightInfo;

    //修路相关
    private Vector3 roadStartPos;//道路建造起始点
    private Vector3 roadEndPos;//道路建造终点
    private List<GameObject> preRoads = new List<GameObject>();
    private Direction roadDirection = Direction.right;
    private int roadCount = 0;
    private int roadLevel = 1;//道路等级
    private GameObject desRoadShow;

    //事件相关
    private UnityAction<Vector3> moveAc = (Vector3 p) => Instance.OnMouseMoveSetBuildingPos(p);
    private UnityAction confirmAc = () => Instance.OnConfirmBuildingBuild();
    private UnityAction cancelAc = () => Instance.OnCancelBuild();
    private UnityAction rotateAc = () => Instance.OnRotateBuilding();

    public bool IsInBuildMode { get; set; }

    #endregion


    #region 公共函数

    public void InitBuildManager()
    {
        //ShowGrid(false);
        IsInBuildMode = false;
        EventManager.StartListening<SelectLightInfo>(ConstEvent.OnSelectLightOpen, OpenSelectLight);
        EventManager.StartListening(ConstEvent.OnSelectLightClose, CloseSelectLight);
        EventManager.StartListening<SelectLightInfo>(ConstEvent.OnRangeLightOpen, OpenRangeLight);
        EventManager.StartListening(ConstEvent.OnRangeLightClose, CloseRangeLight);
    }

    public void Start()
    {
        

    }

    private void OnDestroy()
    {
        EventManager.StopListening<SelectLightInfo>(ConstEvent.OnSelectLightOpen, OpenSelectLight);
        EventManager.StopListening(ConstEvent.OnSelectLightClose, CloseSelectLight);
        EventManager.StopListening<SelectLightInfo>(ConstEvent.OnRangeLightOpen, OpenRangeLight);
        EventManager.StopListening(ConstEvent.OnRangeLightClose, CloseRangeLight);
    }
    public void OpenSelectLight(SelectLightInfo info)
    {
        selectLight.transform.position = info.pos;
        selectLight.transform.localScale = info.scale;
        selectLight.transform.rotation = info.quaternion;
        selectLight.SetActive(true);
    }

    public void CloseSelectLight()
    {
        selectLight.SetActive(false);
    }

    public void OpenRangeLight(SelectLightInfo info)
    {
        rangeLight.transform.position = info.pos+Vector3.down;
        rangeLight.transform.localScale = info.scale;
        rangeLight.transform.rotation = info.quaternion;
        rangeLight.SetActive(true);
    }

    public void CloseRangeLight()
    {
        rangeLight.SetActive(false);
    }

    private void CleanUpAllAttachedChildren(Transform target)
    {
        for (int i = 0; i < target.childCount; i++)
        {
            Destroy(target.GetChild(i).gameObject);
        }

    }
    /// <summary>
    /// 盖建筑
    /// </summary>
    public void CreateBuildingOnMouse(BuildData buildData)
    {
        CleanUpAllAttachedChildren(roadParent);
        GameObject building = InitBuilding(buildData);
        building.transform.position = Input.mousePosition;
        currentBuilding.runtimeBuildData.direction = lastDir;
        var meshRenderers = building.transform.GetComponentsInChildren<MeshRenderer>();
        mats = new Material[meshRenderers.Length];
        arrows = new GameObject().transform;
        arrows.transform.SetParent(building.transform);
        arrows.localPosition = Vector3.zero;
        arrows.localRotation = Quaternion.identity;
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (!meshRenderers[i].gameObject.name.Contains("Color"))
            {
                mats[i] = meshRenderers[i].material;
            }
        }
        GameObject newArrow = Instantiate(arrowPfb, arrows);
        newArrow.transform.localPosition = (currentBuilding.Size.y+1F) * building.transform.right;
        WhenStartBuild();
    }
    public GameObject InitBuilding(BuildData buildData)
    {
        string bundleName = buildData.BundleName;
        string pfbName = buildData.PfbName;
        if (buildData.Id == 20001 || buildData.Id == 20009 || buildData.Id == 20033
            || buildData.Id == 20023 || buildData.Id == 20024 || buildData.Id == 20025
            || buildData.Id == 20028)
        {
            pfbName = pfbName.Substring(0, pfbName.Length - 1);
            int index = UnityEngine.Random.Range(1, 4);
            pfbName += index.ToString();
        }
        if (buildData.Id == 20026 || buildData.Id == 20027)
        {
            pfbName = pfbName.Substring(0, pfbName.Length - 1);
            int index = UnityEngine.Random.Range(1, 5);
            pfbName += index.ToString();
        }
        //Debug.Log("load:" + bundleName + " " + pfbName);
        GameObject pfb = LoadAB.Load(bundleName, pfbName);
        GameObject building = Instantiate(pfb, TransformFinder.Instance.buildingParent);
        building.name = pfbName;
        currentBuilding = building.GetComponent<BuildingBase>();
        currentBuilding.runtimeBuildData = CastTool.CastBuildDataToRuntime(buildData);
        return building;
    }

    public GameObject InitBuilding(RuntimeBuildData buildData)
    {
        string bundleName = buildData.BundleName;
        string pfbName = buildData.PfbName;
        if (buildData.Id == 20001 || buildData.Id == 20009 || buildData.Id == 20033
            || buildData.Id == 20023 || buildData.Id == 20024 || buildData.Id == 20025
            || buildData.Id == 20028)
        {
            pfbName = pfbName.Substring(0, pfbName.Length - 1);
            int index = UnityEngine.Random.Range(1, 4);
            pfbName += index.ToString();
        }
        if (buildData.Id == 20026 || buildData.Id == 20027)
        {
            pfbName = pfbName.Substring(0, pfbName.Length - 1);
            int index = UnityEngine.Random.Range(1, 5);
            pfbName += index.ToString();
        }
        Debug.Log("load:" + bundleName + " " + pfbName);
        GameObject pfb = LoadAB.Load(bundleName, pfbName);
        GameObject building = Instantiate(pfb, TransformFinder.Instance.buildingParent);
        currentBuilding = building.GetComponent<BuildingBase>();
        building.name = pfbName;
        currentBuilding.runtimeBuildData = buildData;
        return building;
    }

    public void InitSaveBuildings(RuntimeBuildData[] buildDatas)
    {
        //Debug.Log(buildDatas.Length);
        
        for (int i = 0; i < buildDatas.Length; i++)
        {
            InitSaveBuilding(buildDatas[i]);
        }
        TerrainGenerator.Instance.ReCalculateMesh();
    }

    public void InitSaveBridges(BridgeData[] bridgeDatas)
    {
        for (int i = 0; i < bridgeDatas.Length; i++)
        {
            GameObject obj = new GameObject();
            obj.transform.SetParent(TransformFinder.Instance.bridgeParent);
            BridgeBuilding bridge = obj.AddComponent<BridgeBuilding>();
            bridge.SetBridgeData(bridgeDatas[i]);
            bridge.InitBuildingFunction();
            bridge.OnConfirmBuild(Vector2IntSerializer.Unbox(bridgeDatas[i].takenGrids));
        }
    }

    private void InitSaveBuilding(RuntimeBuildData buildData)
    {
        string bundleName = buildData.BundleName;
        string pfbName = buildData.PfbName;
        if (buildData.Id == 20001 || buildData.Id == 20009 || buildData.Id == 20033
            || buildData.Id == 20023 || buildData.Id == 20024 || buildData.Id == 20025
            || buildData.Id == 20028 || buildData.Id == 20026 || buildData.Id == 20027)
        {
            pfbName = pfbName.Substring(0, pfbName.Length - 1);
            pfbName += buildData.SaveOutLookType.ToString();
        }
        Debug.Log(pfbName);
        GameObject pfb = LoadAB.Load(bundleName, pfbName);
        GameObject building = Instantiate(pfb, TransformFinder.Instance.buildingParent);
        building.name = pfbName;
        building.transform.position = buildData.SavePosition.V3;
        BuildingBase buildingBase = building.GetComponent<BuildingBase>();
        buildingBase.runtimeBuildData = buildData;
        if(buildingBase.runtimeBuildData.SortRank == 0)
        {
            buildingBase.runtimeBuildData.SortRank =DataManager.GetBuildData(buildData.Id).SortRank;
        }
        buildingBase.runtimeBuildData.direction = buildData.SaveDir;
        //Debug.Log(buildData.SaveDir);
        buildingBase.buildFlag = true;
        
        Vector2Int[] vector2Ints = Vector2IntSerializer.Unbox(buildData.SaveTakenGrids);
        
        (buildingBase as IBuildingBasic)?.OnConfirmBuild(vector2Ints);
        buildingBase.transform.rotation = Quaternion.LookRotation(CastTool.CastDirectionToVector((int)buildingBase.runtimeBuildData.direction+1));
        
    }

    /// <summary>
    /// 开始修路
    /// </summary>
    public void StartCreateRoads(int _roadLevel = 1)
    {
        IsInBuildMode = true;
        CleanUpAllAttachedChildren(roadParent);
        EventManager.StartListening(ConstEvent.OnMouseLeftButtonDown, OnConfirmRoadStartPos);
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown, OnCancelBuildRoad);
        MainInteractCanvas.Instance.HideBuildingButton();
        MainInteractCanvas.Instance.CloseAllOpenedUI();
        roadLevel = _roadLevel;
    }

    public void StartDestroyRoad()
    {
        IsInBuildMode = true;
        MainInteractCanvas.Instance.CloseAllOpenedUI();
        MainInteractCanvas.Instance.HideBuildingButton();
        CleanUpAllAttachedChildren(roadParent);
        desRoadShow = Instantiate(preRoadPfb, transform);
        GameObject reverse = Instantiate(preRoadPfb, desRoadShow.transform);
        reverse.transform.localPosition = Vector3.zero;
        reverse.transform.localScale = Vector3.one;
        reverse.transform.Rotate(new Vector3(0, 180, 0), Space.Self);
        EventManager.StartListening(ConstEvent.OnMouseLeftButtonDown, OnDestroyRoad);
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown, OnCancelDestroyRoad);
        EventManager.StartListening<Vector3>(ConstEvent.OnGroundRayPosMove, OnPreShowDestroyRoad);
    }

    #endregion

    #region 私有函数

    #region 拆除道路
    private void OnDestroyRoad()
    {
        Vector3 pos = CalculateRoadCenterPos(InputManager.Instance.LastGroundRayPos);
        Vector2Int[] grids = new Vector2Int[4];
        grids[0] = GetCenterGrid(pos - Vector3.forward * 2f);
        grids[1] = GetCenterGrid(pos - Vector3.right * 2f);
        grids[2] = GetCenterGrid(pos - Vector3.right * 2f - Vector3.forward * 2f);
        grids[3] = GetCenterGrid(pos);
        if (MapManager.CheckIsRoad(grids))
        {
            MapManager.SetGridTypeToEmpty(grids);
            MapManager.Instance.BuildOriginFoundation(grids);
            //RoadManager.Instance.InitRoadNodeDic();
            //MapManager.Instance.SetBuildingsGrid();
        }

    }

    private void OnCancelDestroyRoad()
    {
        IsInBuildMode = false;
        MainInteractCanvas.Instance.OpenBuildingCanvas();
        Destroy(desRoadShow);
        preRoads.Remove(desRoadShow);
        MainInteractCanvas.Instance.ShowBuildingButton();
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, OnDestroyRoad);
        EventManager.StopListening(ConstEvent.OnMouseRightButtonDown, OnCancelBuildRoad);
        EventManager.StopListening<Vector3>(ConstEvent.OnGroundRayPosMove, OnPreShowDestroyRoad);
    }

    private void OnPreShowDestroyRoad(Vector3 pos)
    {
        desRoadShow.transform.position = CalculateRoadCenterPos(pos + new Vector3(0, 0.01f, 0));
        preRoads.Add(desRoadShow);
        ChangeRoadPfbColor(false, desRoadShow);
    }
    #endregion
    private bool CheckRoadStartPosAvalible()
    {
        Vector2Int startGrid = GetCenterGrid(roadStartPos);
        //Debug.Log(MapManager.Instance.GetGridType(startGrid));
        //Debug.Log(MapManager.GetMineRichness(startGrid));
        //Debug.Log(MapManager.GetGridNode(startGrid).enterCost);
        //Debug.Log(MapManager.GetGridNode(startGrid).mineValue);
        //Debug.Log(startGrid);
        //float sum = 0;
        /*Debug.Log(MapManager.Instance.GetGrids() != null);
        for (int i = 0; i < MapManager.Instance.GetGrids().Length; i++)
        {
            for (int j = 0; j < MapManager.Instance.GetGrids().LongLength; j++)
            {
                GridNode node = MapManager.Instance.GetGrids()[i][j];
                sum += node.mineValue;
                if (node.mineValue > 0)
                {
                    Debug.Log(i + " " + j);
                    Debug.Log(node.enterCost);
                    Debug.Log(node.gridType);
                }
            }
        }
        Debug.Log(sum);*/
        if (MapManager.Instance.GetGridType(startGrid) == GridType.occupy)
        {
            NoticeManager.Instance.InvokeShowNotice(Localization.Get("道路起点不能设在已被占用的位置"));
            return false;
        }
        if (MapManager.CheckIsInWater(startGrid))
        {
            NoticeManager.Instance.InvokeShowNotice(Localization.Get("道路终点不能在水里"));
            return false;
        }
        return true;
    }
    /// <summary>
    /// 点下起点
    /// </summary>
    private void OnConfirmRoadStartPos()
    {
        roadStartPos = CalculateRoadCenterPos(InputManager.Instance.LastGroundRayPos);
        if (CheckRoadStartPosAvalible())
        {
            preRoads = new List<GameObject>();
            EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, OnConfirmRoadStartPos);
            EventManager.StartListening(ConstEvent.OnMouseLeftButtonDown, OnConfirmRoadEndPos);
            EventManager.StartListening<Vector3>(ConstEvent.OnGroundRayPosMove, OnPreShowRoad);

        }
    }

    /// <summary>
    /// 点下终点
    /// </summary>
    private void OnConfirmRoadEndPos()
    {
        if (!CheckRoadPreShowAvalible())
        {
            NoticeManager.Instance.InvokeShowNotice(Localization.Get("道路不能与建筑重叠"));
            return;
        }
        if (CheckRoadEndPosInWater())
        {
            NoticeManager.Instance.InvokeShowNotice(Localization.Get("道路终点不能在水里"));
            return;
        }
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, OnConfirmRoadEndPos);
        EventManager.StopListening<Vector3>(ConstEvent.OnGroundRayPosMove, OnPreShowRoad);
        Vector2 vec = RectTransformUtility.WorldToScreenPoint(Camera.main, InputManager.Instance.LastGroundRayPos);
        int id;
        switch (roadLevel)
        {
            case 1: id = 20002; break;
            case 2: id = 20018; break;
            case 3: id = 20019; break;
            default: id = 20002; break;
        }
        BuildData data = DataManager.GetBuildData(id);
        RoadInfo info = new RoadInfo();
        info.vec = vec;
        info.costResources = new List<CostResource>();
        info.costResources.Add(new CostResource(99999, data.Price * preRoads.Count));
        for (int i = 0; i < data.costResources.Count; i++)
        {
            info.costResources.Add(new CostResource(data.costResources[i].ItemId, data.costResources[i].ItemNum * preRoads.Count));
        }
        EventManager.TriggerEvent<RoadInfo>(ConstEvent.OnBuildToBeConfirmed, info);
        //ChangeRoadCount(0);

    }

    public class RoadInfo
    {
        public Vector2 vec;
        public List<CostResource> costResources;
    }

    

    /// <summary>
    /// 确认修建
    /// </summary>
    public void OnConfirmBuildRoad(RoadInfo info, out bool success)
    {
        if (!ResourceManager.Instance.IsResourcesEnough(info.costResources))
        {
            success = false;
            return;
        }
        ResourceManager.Instance.TryUseResources(info.costResources);
        EventManager.TriggerEvent(ConstEvent.OnRefreshResources);
        success = true;
        List<Vector2Int> grids = new List<Vector2Int>();
        List<Vector2Int> bridgeGrids = new List<Vector2Int>();
        int dir = 0;
        Vector3 adjust = Vector3.zero;
        Vector3 size = Vector3.zero;
        switch (roadDirection)
        {
            case Direction.down:
                dir = 1;
                adjust += new Vector3(0, 0, -2);
                break;
            case Direction.up:
                dir = 1;
                break;
            case Direction.right:
                dir = 2;
                break;
            case Direction.left:
                adjust += new Vector3(-2, 0, 0);
                dir = 2;
                break;
        }
        Vector3 delta = CastTool.CastDirectionToVector(dir);
        //Debug.Log(delta.ToString());
        List<Vector3> bridges = new List<Vector3>();
        BridgeData bridgeData = new BridgeData();
        bridgeData.lookRotation = new Vector3Serializer();
        bridgeData.lookRotation.Fill(delta);
        bridgeData.roadLevel = roadLevel;
        bool bridgeStart = false;
        for (int i = 0; i < preRoads.Count; i++)
        {
            grids.Add(GetCenterGrid(preRoads[i].transform.position + adjust));
            grids.Add(GetCenterGrid(preRoads[i].transform.position - delta + adjust));
            if (preRoads[i].transform.position.y < 9.1f)
            {
                bridgeGrids.Add(grids[i * 2]);
                bridgeGrids.Add(grids[i * 2 + 1]);
                //桥开始时应该往岸上延伸一格
                if (!bridgeStart && i > 1)
                {
                    bridgeGrids.Add(grids[(i - 1) * 2]);
                    bridgeGrids.Add(grids[(i - 1) * 2 + 1]);
                    bridgeStart = true;
                    bridges.Add(MapManager.GetOnGroundPosition(GetCenterGrid(preRoads[i - 1].transform.position + adjust)));
                }                
                bridges.Add(MapManager.GetOnGroundPosition(GetCenterGrid(preRoads[i].transform.position + adjust)));
            }
        }
        bridgeData.takenGrids = Vector2IntSerializer.Box(bridgeGrids.ToArray());
        if (bridges.Count > 0)
        {
            GameObject bridgeBuilding = new GameObject();
            BridgeBuilding building = bridgeBuilding.AddComponent<BridgeBuilding>();
            bridgeBuilding.transform.parent = TransformFinder.Instance.bridgeParent;
            bridgeBuilding.name = "bridge"+ TransformFinder.Instance.bridgeParent.childCount;
            switch (roadDirection)
            {
                case Direction.down:
                case Direction.up:
                    size += new Vector3(4, 1, bridges.Count * 2);
                    break;
                case Direction.right:
                case Direction.left:
                    size += new Vector3(bridges.Count * 2, 1, 4);
                    break;
            }
            bridgeData.bridgePos = new Vector3Serializer();
            bridgeData.bridgePos.Fill(bridges[bridges.Count / 2]);
            bridgeData.size = new Vector3Serializer();
            bridgeData.size.Fill(size);
            for (int i = bridges.Count - 2; i > 0; i--)
            {
                if (i % 2 == 1)
                {
                    bridges.RemoveAt(i);
                }
            }
            bridgeData.gridsPos = Vector3Serializer.Box(bridges.ToArray());
            building.SetBridgeData(bridgeData);
            building.InitBuildingFunction();
            building.OnConfirmBuild(bridgeGrids.ToArray());
        }
        //Debug.Log("generate");
        MapManager.Instance.GenerateRoad(grids.ToArray(), roadLevel);
        //MapManager.Instance.SetBuildingsGrid();
        MainInteractCanvas.Instance.ShowBuildingButton();
        ChangeRoadCount(0);
        EventManager.TriggerEvent(ConstEvent.OnFinishBuilding);
        //StartCreateRoads(roadLevel);
        IsInBuildMode = false;
    }

    public BridgeData[] GetBridgeDatas()
    {
        Transform bridgeParent = TransformFinder.Instance.bridgeParent;
        int count = bridgeParent.childCount;
        BridgeData[] res = new BridgeData[count];
        for (int i = 0; i < count; i++)
        {
            res[i] = bridgeParent.GetChild(i).GetComponent<BridgeBuilding>().GetBridgeData();
        }
        return res;
    } 

    /// <summary>
    /// 取消修建
    /// </summary>
    public void OnCancelBuildRoad()
    {
        EventManager.TriggerEvent(ConstEvent.OnFinishBuilding);
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, OnConfirmRoadEndPos);
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, OnConfirmRoadStartPos);
        EventManager.StopListening(ConstEvent.OnMouseRightButtonDown, OnCancelBuildRoad);
        EventManager.StopListening<Vector3>(ConstEvent.OnGroundRayPosMove, OnPreShowRoad);
        MainInteractCanvas.Instance.ShowBuildingButton();
        ChangeRoadCount(0);
        IsInBuildMode = false;
    }
    /// <summary>
    /// 修路时显示预计建造的路
    /// </summary>
    /// <param name="pos"></param>
    private void OnPreShowRoad(Vector3 pos)
    {
        Vector3 vector = pos - roadStartPos;
        Direction dir;
        int count;
        CalculateLongestDir(vector, out dir, out count);
        if (dir != roadDirection)
        {
            ChangeRoadDirection(dir);
        }
        if (count != roadCount)
        {
            ChangeRoadCount(count);
        }
    }


    /// <summary>
    /// 修改路的长度
    /// </summary>
    private void ChangeRoadCount(int newCount)
    {
        Vector3 extensionDir = CastTool.CastDirectionToVector(roadDirection);
        if (newCount > roadCount)
        {
            for (int i = roadCount; i < newCount; i++)
            {
                GameObject newRoad = Instantiate(preRoadPfb, roadParent);
                newRoad.name = i.ToString();
                newRoad.transform.position = MapManager.GetTerrainPosition(roadStartPos + extensionDir * i) + new Vector3(0, 0.01f, 0);
                newRoad.transform.LookAt(MapManager.GetTerrainPosition(roadStartPos + extensionDir * (i + 1)) + new Vector3(0, 0.01f, 0));
                newRoad.GetComponent<RoadPreview>().SetRoadPreviewMat(roadLevel);
                preRoads.Add(newRoad);
            }
            CheckRoadPreShowAvalible();
        }
        else if (newCount == roadCount)
        {
            return;
        }
        else
        {
            for (int i = newCount; i < roadCount; i++)
            {
                GameObject deleteRoad = preRoads[preRoads.Count - 1];
                preRoads.RemoveAt(preRoads.Count - 1);
                Destroy(deleteRoad);
            }
            CheckRoadPreShowAvalible();
        }
        roadCount = newCount;
        roadEndPos = MapManager.GetTerrainPosition(roadStartPos + extensionDir * roadCount);
    }

    /// <summary>
    /// 修改路的延伸方向
    /// </summary>
    private void ChangeRoadDirection(Direction direction)
    {
        roadDirection = direction;
        Vector3 extensionDir = CastTool.CastDirectionToVector(roadDirection);
        for (int i = 0; i < preRoads.Count; i++)
        {
            preRoads[i].transform.position = MapManager.GetTerrainPosition(roadStartPos + extensionDir * i) + new Vector3(0, 0.01f, 0);
            preRoads[i].transform.LookAt(MapManager.GetTerrainPosition(roadStartPos + extensionDir * (i + 1)) + new Vector3(0, 0.01f, 0));
        }
        CheckRoadPreShowAvalible();
    }

    /// <summary>
    /// 获得用户鼠标输入的最长的道路延伸方向
    /// </summary>
    /// <param name="input"></param>
    /// <param name="dir"></param>
    /// <param name="count"></param>
    private void CalculateLongestDir(Vector3 input, out Direction dir, out int count)
    {
        input += new Vector3(0.5f, 0f, 0.5f);
        int n;
        if (Mathf.Abs(input.x) >= Mathf.Abs(input.z))
        {
            dir = input.x >= 0 ? Direction.right : Direction.left;
            n = Mathf.CeilToInt(Mathf.Abs(input.x) / MapManager.unit);
        }
        else
        {
            dir = input.z >= 0 ? Direction.up : Direction.down;
            n = Mathf.CeilToInt(Mathf.Abs(input.z) / MapManager.unit);
        }
        n = n / 2 * 2 + 1;
        count = n;
    }

    private bool CheckRoadEndPosInWater()
    {
        return MapManager.CheckIsInWater(MapManager.GetCenterGrid(preRoads[preRoads.Count - 1].transform.position));
    }

    private bool CheckRoadPreShowAvalible()
    {
        bool res = true;
        int dir = 0;
        Vector3 adjust = Vector3.zero;
        Vector3 size = Vector3.zero;
        switch (roadDirection)
        {
            case Direction.down:
                dir = 1;
                adjust += new Vector3(0, 0, -2);
                break;
            case Direction.up:
                dir = 1;
                break;
            case Direction.right:
                dir = 2;
                break;
            case Direction.left:
                adjust += new Vector3(-2, 0, 0);
                dir = 2;
                break;
        }
        Vector3 delta = CastTool.CastDirectionToVector(dir);
        for (int i = 0; i < preRoads.Count; i++)
        {
            bool canBuild = !MapManager.CheckRoadOverlap(MapManager.GetCenterGrid(preRoads[i].transform.position + adjust))
                && !MapManager.CheckRoadOverlap(MapManager.GetCenterGrid(preRoads[i].transform.position - delta + adjust));
            res &= canBuild;
            ChangeRoadPfbColor(canBuild, preRoads[i]);
        }
        return res;
    }
    private Vector3 va = new Vector3(-1, 0, -1);

    private void ChangeRoadPfbColor(bool green, GameObject road)
    {
        road.GetComponentInChildren<MeshRenderer>().material.color = (green ? Color.green : Color.red);
    }

    private void OnMouseMoveSetBuildingPos(Vector3 p)
    {
        currentBuilding.transform.position = CalculateCenterPos(p, currentBuilding.Size, isTurn);
        lightInfo.pos = currentBuilding.transform.position+Vector3.down;
        EventManager.TriggerEvent(ConstEvent.OnSelectLightOpen, lightInfo);
        //gridHightLight.transform.position = CalculateCenterPos(p, Vector2Int.zero) + new Vector3(0, 0.02f, 0);
        CheckBuildingOverlap();
    }

    private void OnRotateBuilding()
    {
        Direction dir = currentBuilding.runtimeBuildData.direction;
        currentBuilding.transform.rotation = Quaternion.LookRotation(CastTool.CastDirectionToVector((int)dir + 2), Vector3.up);
        currentBuilding.runtimeBuildData.direction = (Direction)(((int)dir + 1) % 4);
        //Debug.Log(currentBuilding.direction);
        if (currentBuilding.runtimeBuildData.direction == Direction.down || currentBuilding.runtimeBuildData.direction == Direction.up)
        {
            isTurn = false;
        }
        else
        {
            isTurn = true;
        }
        currentBuilding.transform.position = CalculateCenterPos(InputManager.Instance.LastGroundRayPos, currentBuilding.Size, isTurn);
        lightInfo.pos = currentBuilding.transform.position;
        lightInfo.quaternion = currentBuilding.transform.rotation;
        EventManager.TriggerEvent(ConstEvent.OnSelectLightOpen, lightInfo);
        //Debug.Log(InputManager.Instance.LastGroundRayPos);
        CheckBuildingOverlap();
        //gridHightLight.transform.position = CalculateCenterPos(InputManager.Instance.LastGroundRayPos, Vector2Int.zero) + new Vector3(0, 0.02f, 0);
    }

    private void CheckBuildingOverlap()
    {
        Vector3 curPos = currentBuilding.transform.position;
        int width, height;
        //Debug.Log(curPos);
        targetGrids = GetAllGrids(currentBuilding.Size.x, currentBuilding.Size.y, curPos, out width, out height);
        bool isCheckSea = currentBuilding.runtimeBuildData.Id == 20032;
        isCurCanBuild = MapManager.CheckCanBuild(targetGrids, currentBuilding, isCheckSea);
        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i] != null)
            {
                mats[i].color = isCurCanBuild ? Color.green : Color.red;
            }
        }

        if (currentBuilding.runtimeBuildData.Id == 20029 ||
            currentBuilding.runtimeBuildData.Id == 20030 ||
            currentBuilding.runtimeBuildData.Id == 20030)
        {
            MineBuilding mine = (MineBuilding)currentBuilding;
            float rich = mine.SetRichness(targetGrids);
            NoticeManager.Instance.ShowIconNotice(Localization.Get("矿脉丰度:") + CastTool.RoundOrFloat(rich * 100) + "%");
        }
        //gridHightLight.GetComponent<MeshRenderer>().material = isCurOverlap ? mat_grid_red : mat_grid_green;
    }
    private void OnConfirmBuildingBuild()
    {
        CheckBuildingOverlap();
        NoticeManager.Instance.CloseNotice();
        if (!isCurCanBuild)
        {
            NoticeManager.Instance.InvokeShowNotice(MapManager.noticeContent);
            //Debug.Log("当前建筑重叠，无法建造！");
            return;
        }
        if (CheckBuildResourcesEnoughAndUse(currentBuilding.runtimeBuildData))
        {
            (currentBuilding as IBuildingBasic)?.OnConfirmBuild(targetGrids);
            MapManager.SetGridTypeToOccupy(targetGrids);

            WhenFinishBuild();
            if (currentBuilding.runtimeBuildData.Id != 20005 &&
                currentBuilding.runtimeBuildData.Id != 20037 &&
                currentBuilding.runtimeBuildData.Id != 20038)
            {
                currentBuilding.transform.position += Vector3.up * 5;
                StartCoroutine(nameof(PushDownBuilding), currentBuilding.gameObject);
                SoundManager.Instance.PlaySoundEffect(SoundResource.sfx_constuction);
            }
            lastDir = currentBuilding.runtimeBuildData.direction-1;
            EventManager.TriggerEvent(ConstEvent.OnContinueBuild);
        }
        else
        {
            NoticeManager.Instance.InvokeShowNotice(Localization.Get(ConstString.NoticeBuildFailNoRes));
        }
    }
    private IEnumerator PushDownBuilding(GameObject obj)
    {
        Vector3 start = obj.transform.position;
        Vector3 down = Vector3.down;
        Vector3 ground = MapManager.GetTerrainPosition(obj.transform.position);
        if (ground.y < 10)
        {
            ground = new Vector3(ground.x, 10, ground.z);
        }
        while (start.y - ground.y > 0)
        {
            down += -Vector3.down * 30F * Time.deltaTime;
            start -= down * Time.deltaTime;
            obj.transform.position = start;
            yield return 0;
        }
        obj.transform.position = ground;
        PlayAnim(obj);
    }
    private void PlayAnim(GameObject obj)
    {
        dustParticle.gameObject.SetActive(true);
        dustParticle.transform.position = obj.transform.position;
        dustParticle.transform.rotation = obj.transform.rotation;
        BuildingBase b = obj.GetComponent<BuildingBase>();
        dustParticle.transform.localScale = new Vector3(b.Size.y / 2, b.Size.x / 4 + b.Size.y / 4, b.Size.x / 2);
        dustParticle.Play();
    }

    public BuildingBase UpgradeBuilding(RuntimeBuildData buildData, Vector2Int[] grids, Vector3 pos, Quaternion quaternion)
    {
        if (CheckBuildResourcesEnoughAndUse(buildData))
        {
            GameObject building = InitBuilding(buildData);
            building.transform.position = pos;
            building.transform.rotation = quaternion;
            currentBuilding = building.GetComponent<BuildingBase>();
            (currentBuilding as IBuildingBasic)?.OnConfirmBuild(grids);

            //MapManager.SetGridTypeToOccupy(targetGrids);
            //terrainGenerator.OnFlatGround(currentBuilding.transform.position, 3, currentBuilding.transform.position.y);
            return currentBuilding;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 检查建造所需的资源是否足够，如果足够就使用掉，不足够就返回false
    /// </summary>
    private bool CheckBuildResourcesEnoughAndUse(RuntimeBuildData runtimeBuildData)
    {
        List<CostResource> rescources = runtimeBuildData.costResources;

        if (rescources == null || rescources.Count <=0)
        {
            return false;
        }

        if (!ResourceManager.Instance.IsResourcesEnough(rescources, TechManager.Instance.BuildResourcesBuff()))
        {
            return false;
        }
        if (!ResourceManager.Instance.IsResourceEnough(new CostResource(99999, runtimeBuildData.Price), TechManager.Instance.BuildPriceBuff()))
        {
            return false;
        }
        ResourceManager.Instance.TryUseResources(rescources, TechManager.Instance.BuildResourcesBuff());
        ResourceManager.Instance.TryUseResource(new CostResource(99999, runtimeBuildData.Price), TechManager.Instance.BuildPriceBuff());
        EventManager.TriggerEvent(ConstEvent.OnRefreshResources);
        return true;

    }
    private void OnCancelBuild()
    {
        NoticeManager.Instance.CloseNotice();
        Destroy(currentBuilding.gameObject);
        WhenFinishBuild();
    }

    private void WhenStartBuild()
    {
        //ShowGrid(true);
        isTurn = false;
        isCurCanBuild = false;
        IsInBuildMode = true;
        lightInfo = new SelectLightInfo
        {
            pos = currentBuilding.transform.position,
            quaternion = currentBuilding.transform.rotation,
            scale = new Vector3(currentBuilding.Size.y * 2, 10, currentBuilding.Size.x * 2),
        };
        EventManager.TriggerEvent(ConstEvent.OnSelectLightOpen, lightInfo);
        EventManager.StartListening(ConstEvent.OnGroundRayPosMove, moveAc);
        EventManager.StartListening(ConstEvent.OnMouseLeftButtonDown, confirmAc);
        EventManager.StartListening(ConstEvent.OnMouseRightButtonDown, cancelAc);
        EventManager.StartListening(ConstEvent.OnRotateBuilding, rotateAc);
        MainInteractCanvas.Instance.HideBuildingButton();
        OnRotateBuilding();
        GameManager.Instance.PauseGame();
    }
    private void WhenFinishBuild()
    {
        //ShowGrid(false);
        GameManager.Instance.ContinueGame();
        EventManager.StopListening(ConstEvent.OnGroundRayPosMove, moveAc);
        EventManager.StopListening(ConstEvent.OnRotateBuilding, rotateAc);
        EventManager.StopListening(ConstEvent.OnMouseLeftButtonDown, confirmAc);
        EventManager.StopListening(ConstEvent.OnMouseRightButtonDown, cancelAc);
        EventManager.TriggerEvent(ConstEvent.OnFinishBuilding);
        MainInteractCanvas.Instance.ShowBuildingButton();
        if (arrows != null)
        {
            Destroy(arrows.gameObject);
        }
        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i] != null)
            {
                mats[i].color = Color.white;
            }
        }
        //currentBuilding = null;
        targetGrids = null;
        IsInBuildMode = false;
        EventManager.TriggerEvent(ConstEvent.OnSelectLightClose);
    }
    //private void ShowGrid(bool isShow)
    //{
    //    gridHightLight.SetActive(isShow);
    //}

    private Vector3 CalculateRoadCenterPos(Vector3 pos)
    {
        Vector3 newPos = pos;
        newPos.x = Mathf.Floor(pos.x / 4) * 4 + 2;
        newPos.z = Mathf.Floor(pos.z / 4) * 4 + 2;
        return newPos;
    }

    /// <summary>
    /// 对齐网格
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    private Vector3 CalculateCenterPos(Vector3 pos, Vector2Int size, bool isExchange = false)
    {
        Vector2Int vector2Int = size;
        if (isExchange)
        {
            vector2Int = new Vector2Int(size.y, size.x);
        }

        Vector3 newPos = pos;
        if (vector2Int.x % 2 != 0)
        {
            newPos.x = Mathf.Floor(pos.x / 2) * 2 + 1f;
        }
        else
        {
            newPos.x = Mathf.Floor(pos.x / 2) * 2;
        }
        if (vector2Int.y % 2 != 0)
        {
            newPos.z = Mathf.Floor(pos.z / 2) * 2 + 1f;
        }
        else
        {
            newPos.z = Mathf.Floor(pos.z / 2) * 2;
        }
        if (pos.y < 10f)
        {
            newPos.y = 10;
        }
        return newPos;
    }
    private Vector2Int GetCenterGrid(Vector3 centerPos)
    {
        Vector3 centerGrid = centerPos / 2;
        int x = Mathf.FloorToInt(centerGrid.x);
        int z = Mathf.FloorToInt(centerGrid.z);
        return new Vector2Int(x, z);
    }
    /// <summary>
    /// 获取当前待造建筑所占用的所有格子
    /// </summary>
    /// <returns></returns>
    public Vector2Int[] GetAllGrids(int sizeX, int sizeY, Vector3 centerPos, out int width, out int height)
    {
        int startX, endX, startZ, endZ;
        width = isTurn ? sizeY : sizeX;
        height = isTurn ? sizeX : sizeY;
        Vector3 centerGrid = centerPos / 2;
        if (width % 2 == 0)
        {
            startX = Mathf.FloorToInt(centerGrid.x) - width / 2 + 1;
            endX = Mathf.FloorToInt(centerGrid.x) + width / 2;
        }
        else
        {
            startX = Mathf.RoundToInt(centerGrid.x) - (width - 1) / 2;
            endX = Mathf.RoundToInt(centerGrid.x) + (width - 1) / 2;
        }
        if (height % 2 == 0)
        {
            startZ = Mathf.FloorToInt(centerGrid.z) - height / 2 + 1;
            endZ = Mathf.FloorToInt(centerGrid.z) + height / 2;
        }
        else
        {
            startZ = Mathf.RoundToInt(centerGrid.z) - (height - 1) / 2;
            endZ = Mathf.RoundToInt(centerGrid.z) + (height - 1) / 2;
        }


        Vector2Int[] grids = new Vector2Int[width * height];
        int index = 0;
        for (int i = startX; i <= endX; i++)
        {
            for (int j = startZ; j <= endZ; j++)
            {
                grids[index] = new Vector2Int(i - 1, j - 1);
                index++;
            }
        }
        return grids;
    }

    public Vector2Int[] GetAllGrids(int sizeX, int sizeY, Vector3 centerPos, bool _isTurn)
    {
        int startX, endX, startZ, endZ;
        int width = _isTurn ? sizeY : sizeX;
        int height = _isTurn ? sizeX : sizeY;
        Vector3 centerGrid = centerPos / 2;
        if (width % 2 == 0)
        {
            startX = Mathf.FloorToInt(centerGrid.x) - width / 2 + 1;
            endX = Mathf.FloorToInt(centerGrid.x) + width / 2;
        }
        else
        {
            startX = Mathf.RoundToInt(centerGrid.x) - (width - 1) / 2;
            endX = Mathf.RoundToInt(centerGrid.x) + (width - 1) / 2;
        }
        if (height % 2 == 0)
        {
            startZ = Mathf.FloorToInt(centerGrid.z) - height / 2 + 1;
            endZ = Mathf.FloorToInt(centerGrid.z) + height / 2;
        }
        else
        {
            startZ = Mathf.RoundToInt(centerGrid.z) - (height - 1) / 2;
            endZ = Mathf.RoundToInt(centerGrid.z) + (height - 1) / 2;
        }


        Vector2Int[] grids = new Vector2Int[width * height];
        int index = 0;
        for (int i = startX; i <= endX; i++)
        {
            for (int j = startZ; j <= endZ; j++)
            {
                grids[index] = new Vector2Int(i - 1, j - 1);
                index++;
            }
        }
        return grids;
    }
    #endregion
}

