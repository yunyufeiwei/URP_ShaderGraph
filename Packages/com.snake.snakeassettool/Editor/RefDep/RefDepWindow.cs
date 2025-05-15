using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AssetGraph;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public class RefDepWindow : EditorWindow
{
    #region Const Field
    private const float BtnNormalWidth = 50;
    private const float FilterDescWidth = 90;
    private const float ToolbarHeight = 22;
    private const float NormalYSpace = 5;
    public const int ZoomLvMax = 10;
    public const int ZoomLvMin = 4;
    // 引用依赖展示深度限制
    public const float NormalNodeSpaceY = RefDepNodeGUI.NodeBaseHeight * 8;
    public const float NormalNodeSpaceX = RefDepNodeGUI.NodeBaseWidth * 2.5f;
    private const int ShowcaseQueueLimit = 20;
    #endregion

    #region GUI Field
    private Rect _toolbarRegion = new Rect();
    private RefDepGraphBackground _background = new RefDepGraphBackground();
    private Rect _graphRegion = new Rect();
    private Rect _filterRegion = new Rect();
    private readonly int kDragNodesControlID = "RefDepWindow.HandleDragNodes".GetHashCode();

    private Vector2 _startMousePos = Vector2.zero;
    private Vector2 _lastMousePos = Vector2.zero;
    private Vector2 _curFrameDragPos = Vector2.zero;
    private long _lastMouseDownTick = 0;
    private List<RefDepNodeGUI> _selectNodes = new List<RefDepNodeGUI>();
    //private List<int> _allNodesDepth = new List<int>();
    private List<RefDepNodeGUI> _allNodes = new List<RefDepNodeGUI>();

    private List<string> _showcaseGuid = new List<string>();
    private List<int> _showcaseIndexList = new List<int>();
    private bool _canDragNode = false;
    private DynamicGuiMode _dynamicGuiMode = DynamicGuiMode.None;
    private bool _canCalcDynamicGui = false;
    // 此数值用于记录引用和依赖更多的那一方的子节点数量，用于做自动布局
    private int _childCountMore = 0;

    public static int ZoomLv = 6;
    private float _curRoomChange = 0;
    private float _roomLvChangeLimit = 4;
    public static float _scale = 1;

    private static string _strRefDepthLimit = "1";
    private static string _strDepDepthLimit = "1";
    private static string _strChildCountLimit = "10";
    private static string _strSearch;
    #endregion

    private static int _curShowcaseIndex = -1;
    private static List<List<string>> _showcaseGuidQueue = new List<List<string>>();

    public enum DynamicGuiMode
    {
        // 当前并未处于动态修改状态
        None,
        // 当前正处于鼠标左键拖动框选节点状态
        LeftDragToSelect,
        // 当前正处于鼠标左键拖动选中节点状态
        LeftDragMove,
        // 当前正处于鼠标右键拖动所有节点状态
        RightDragMove,
        // 当前处于鼠标中键滚动缩放界面状态
        ScrollWheel,
        // 左键双击
        LeftDoubleClick,
        // 右键双击
        RightDoubleClick
    }

    [MenuItem("ArtTools/LaoWang/打开引用依赖工具")]
    private static void OpenRefDepWindow()
    {
        RefDepWindow myWindow = GetWindow<RefDepWindow>("引用依赖查看器");
        myWindow.Show();
        myWindow.CalcGuiParam();
    }

    [MenuItem("Assets/LaoWang/查看选中资产引用依赖")]
    private static void ShowUserSelectsAssetRefDep()
    {
        RefDepWindow myWindow = GetWindow<RefDepWindow>("引用依赖查看器");
        myWindow.Show();
        myWindow.CalcGuiParam();
        myWindow.RefreshShowcaseList(null, true);
    }

    private void GenRefNode(RefDepNodeGUI parentGui, string guid, int depth, int maxDepth, int countLimit)
    {
        RefDepAssetInfo selfInfo = RefDepCollecter.Ins.GetAssetInfoByGuid(guid);
        if (selfInfo == null)
            return;
        int refCount = selfInfo.RefList.Count;
        if (refCount == 0)
            return;
        for (int i = 0; i < refCount; i++)
        {
            if (i >= countLimit)
            {
                // 添加折叠节点到子级，并跳出循环
                string foldGuid = selfInfo.RefList[i];
                RefDepAssetInfo foldInfo = RefDepCollecter.Ins.GetAssetInfoByGuid(foldGuid);
                foldInfo.SetToFoldAsset(refCount - countLimit);
                float foldY = (i - (refCount - 1) * 0.5f) * NormalNodeSpaceY;
                RefDepNodeData foldData = new RefDepNodeData(foldGuid, foldInfo.AssetName, foldInfo.TypeDesc, 0, foldY, foldInfo.Style, foldInfo.StyleOn, foldInfo.Icon);
                RefDepNodeGUI foldGui = RefDepNodeGUI.CreateNodeGui(foldData);
                _allNodes.Add(foldGui);
                parentGui.AddChild(foldGui);
                break;
            }
            string refGuid = selfInfo.RefList[i];
            RefDepAssetInfo refInfo = RefDepCollecter.Ins.GetAssetInfoByGuid(refGuid);
            refInfo.CompleteInfo();
            float refY = (i - (refCount - 1) * 0.5f) * NormalNodeSpaceY;
            RefDepNodeData refData = new RefDepNodeData(refGuid, refInfo.AssetName, refInfo.TypeDesc, 0, refY, refInfo.Style, refInfo.StyleOn, refInfo.Icon);
            RefDepNodeGUI refGui = RefDepNodeGUI.CreateNodeGui(refData);
            _allNodes.Add(refGui);
            //_allNodesDepth.Add(rootIndex - depth);
            parentGui.AddChild(refGui);

            if (depth < maxDepth)
                GenRefNode(refGui, refGuid, depth + 1, maxDepth, countLimit);
        }
    }

    private void GenDepNode(RefDepNodeGUI parentGui, string guid, int depth, int maxDepth, int countLimit)
    {
        RefDepAssetInfo selfInfo = RefDepCollecter.Ins.GetAssetInfoByGuid(guid);
        if (selfInfo == null)
            return;
        int depCount = selfInfo.DepList.Count;
        if (depCount == 0)
            return;
        for (int i = 0; i < depCount; i++)
        {
            if (i >= countLimit)
            {
                // 添加折叠节点到子级，并跳出循环
                string foldGuid = selfInfo.DepList[i];
                RefDepAssetInfo foldInfo = RefDepCollecter.Ins.GetAssetInfoByGuid(foldGuid);
                foldInfo.SetToFoldAsset(depCount - countLimit);
                float foldY = (i - (depCount - 1) * 0.5f) * NormalNodeSpaceY;
                RefDepNodeData foldData = new RefDepNodeData(foldGuid, foldInfo.AssetName, foldInfo.TypeDesc, 0, foldY, foldInfo.Style, foldInfo.StyleOn, foldInfo.Icon);
                RefDepNodeGUI foldGui = RefDepNodeGUI.CreateNodeGui(foldData);
                _allNodes.Add(foldGui);
                parentGui.AddChild(foldGui, false);
                break;
            }
            string depGuid = selfInfo.DepList[i];
            RefDepAssetInfo depInfo = RefDepCollecter.Ins.GetAssetInfoByGuid(depGuid);
            depInfo.CompleteInfo();
            float depY = (i - (depCount - 1) * 0.5f) * NormalNodeSpaceY;
            RefDepNodeData depData = new RefDepNodeData(depGuid, depInfo.AssetName, depInfo.TypeDesc, 0, depY, depInfo.Style, depInfo.StyleOn, depInfo.Icon);
            RefDepNodeGUI depGui = RefDepNodeGUI.CreateNodeGui(depData);
            _allNodes.Add(depGui);
            //_allNodesDepth.Add(rootIndex + depth);
            parentGui.AddChild(depGui, false);
            if (depth < maxDepth)
                GenDepNode(depGui, depGuid, depth + 1, maxDepth, countLimit);
        }
    }

    private void RefreshShowcaseList(List<string> guids = null, bool isNew = false)
    {
        // 收集showcase list;
        if (guids == null)
        {
            _showcaseGuid.Clear();
            UnityEngine.Object[] objs = Selection.objects;
            foreach (UnityEngine.Object obj in objs)
            {
                if (obj.GetType() == typeof(DefaultAsset))
                    continue;
                string path = AssetDatabase.GetAssetPath(obj);
                string guid = AssetDatabase.AssetPathToGUID(path);
                _showcaseGuid.Add(guid);
            }
            isNew = true;
        }
        else
        {
            _showcaseGuid = guids;
        }
        if (_showcaseGuid.Count == 0)
            return;
        if (isNew)
        {
            if (_showcaseGuidQueue.Count == ShowcaseQueueLimit)
            {
                _showcaseGuidQueue.RemoveAt(0);
                _curShowcaseIndex -= 1;
            }
            _showcaseGuidQueue.Add(_showcaseGuid);
            _curShowcaseIndex++;
        }

        // 初始化node相关变量
        _allNodes.Clear();
        //_allNodesDepth.Clear();
        foreach (RefDepNodeGUI selectNode in _selectNodes)
        {
            selectNode.SetIsSelect(false);
        }
        _selectNodes.Clear();
        // 生成引用依赖
        int showcaseCount = _showcaseGuid.Count;
        //int[] showcaseIndexAry = new int[showcaseCount];
        _showcaseIndexList.Clear();
        int refMaxDepth = 1;
        int.TryParse(_strRefDepthLimit, out refMaxDepth);
        refMaxDepth = Mathf.Max(1, refMaxDepth);
        int depMaxDepth = 1;
        int.TryParse(_strDepDepthLimit, out depMaxDepth);
        depMaxDepth = Mathf.Max(1, depMaxDepth);
        int childCountLimit = 10;
        int.TryParse(_strChildCountLimit, out childCountLimit);
        childCountLimit = Mathf.Max(1, childCountLimit);
        // 此数值用于记录引用和依赖更多的那一方的子节点数量，用于做自动布局
        _childCountMore = 0;
        for (int i = 0; i < showcaseCount; i++)
        {
            string guid = _showcaseGuid[i];
            RefDepAssetInfo selfInfo = RefDepCollecter.Ins.GetAssetInfoByGuid(guid);
            if (selfInfo == null)
            {
                // todo:增量更新RefDepCollecter Info
                RefDepCollecter.Ins.AddAsset(guid);
            }
            selfInfo.CompleteInfo();
            // 自身加入allNodes列表
            RefDepNodeData selfData = new RefDepNodeData(guid, selfInfo.AssetName, selfInfo.TypeDesc, 0, 0, selfInfo.Style, selfInfo.StyleOn, selfInfo.Icon);
            RefDepNodeGUI selfGui = RefDepNodeGUI.CreateNodeGui(selfData);
            _allNodes.Add(selfGui);
            int showcaseIndex = _allNodes.Count - 1;
            // 构建引用
            GenRefNode(selfGui, guid, 1, refMaxDepth, childCountLimit);
            // 构建依赖
            GenDepNode(selfGui, guid, 1, depMaxDepth, childCountLimit);
            _showcaseIndexList.Add(showcaseIndex);
        }
        // 计算所有节点的引用依赖数目
        for (int i = 0; i < showcaseCount; i++)
        {
            RefDepNodeGUI node = _allNodes[_showcaseIndexList[i]];
            int depCount = node.CalcDepChildCount();
            int refCount = node.CalcRefChildCount();
            _childCountMore += Mathf.Max(depCount, refCount);
        }
        LayoutNodePos();
    }

    //todo:这里后续应该保留上次偏移位置，而非重新排布
    private void LayoutNodePos()
    {
        _scale = ZoomLv * 1.0f / ZoomLvMax;
        int showcaseCount = _showcaseIndexList.Count;
        // 自动布局，先确定某个资产进行居中
        float[] showcaseCenter = new float[_showcaseGuid.Count];
        float totalBeforeMe = 0;
        float halfCount = _childCountMore * 0.5f;
        int centerIndex = 0;
        float centerSubMin = int.MaxValue;
        for (int i = 0; i < showcaseCount; i++)
        {
            RefDepNodeGUI gui = _allNodes[_showcaseIndexList[i]];
            int childCount = Mathf.Max(gui.DepCount, gui.RefCount);
            float curCenterValue = childCount * 0.5f + totalBeforeMe;
            if (Mathf.Abs(curCenterValue - halfCount) < centerSubMin)
            {
                centerIndex = i;
                centerSubMin = Mathf.Abs(curCenterValue - halfCount);
            }
            showcaseCenter[i] = curCenterValue;
            totalBeforeMe += childCount;
        }
        // centerIndex的坐标为ref dep window窗体正中心。借此修正其它showcase资产的位置，进而更新所有资产位置
        float centerValue = showcaseCenter[centerIndex];
        // 将centerIndex的数值强行对齐到0，其它数值统一更新
        float winCenterX = _graphRegion.width * 0.5f;
        float winCenterY = _graphRegion.height * 0.5f;
        // 逐一更新showcase资产位置
        for (int i = 0; i < showcaseCount; i++)
        {
            showcaseCenter[i] -= centerValue;
            int showcaseIndex = _showcaseIndexList[i];
            Vector2 showcasePos = new Vector2(winCenterX, winCenterY + (showcaseCenter[i] * NormalNodeSpaceY) * _scale);
            _allNodes[showcaseIndex].SetPos(showcasePos);
            UpdateChildRefPos(_allNodes[showcaseIndex]);
            UpdateChildDepPos(_allNodes[showcaseIndex]);
        }
    }

    private void UpdateChildRefPos(RefDepNodeGUI parentGui)
    {
        int refCount = parentGui.ChildRefGui.Count;
        if (refCount == 0)
            return;
        Vector2 parentPos = parentGui.GetPos();
        int nodeBeforeMe = 0;
        for (int i = 0; i < refCount; i++)
        {
            RefDepNodeGUI gui = parentGui.ChildRefGui[i];
            int childRefCount = gui.ChildRefGui.Count;
            float posX = parentPos.x - NormalNodeSpaceX * _scale;
            float posY = parentPos.y + (nodeBeforeMe + (childRefCount == 0 ? 1 : childRefCount) * 0.5f - parentGui.RefCount * 0.5f) * NormalNodeSpaceY * _scale;
            gui.SetPos(new Vector2(posX, posY));
            nodeBeforeMe += (childRefCount == 0 ? 1 : childRefCount);
            if (childRefCount > 0)
            {
                UpdateChildRefPos(gui);
            }
        }
    }

    private void UpdateChildDepPos(RefDepNodeGUI parentGui)
    {
        int depCount = parentGui.ChildDepGui.Count;
        if (depCount == 0)
            return;
        Vector2 parentPos = parentGui.GetPos();
        int nodeBeforeMe = 0;
        for (int i = 0; i < depCount; i++)
        {
            RefDepNodeGUI gui = parentGui.ChildDepGui[i];
            int childDepCount = gui.ChildDepGui.Count;
            float posX = parentPos.x + NormalNodeSpaceX * _scale;
            float posY = parentPos.y + (nodeBeforeMe + (childDepCount == 0 ? 1 : childDepCount) * 0.5f - parentGui.DepCount * 0.5f) * NormalNodeSpaceY * _scale;
            gui.SetPos(new Vector2(posX, posY));
            nodeBeforeMe += (childDepCount == 0 ? 1 : childDepCount);
            if (childDepCount > 0)
            {
                UpdateChildDepPos(gui);
            }
        }
    }

    private void OnEnable()
    {
        RefDepNodeGUI.InitNodeGuiStaticParam();
        RefreshShowcaseList(_showcaseGuid);
    }

    private void OnGUI()
    {
        DrawToolbarGui();

        DrawRefDepGui();

        DrawFilterAndSettingGui();
    }

    private void CalcGuiParam()
    {
        _toolbarRegion = new Rect(0, 0, position.width, ToolbarHeight);
        float graphicYPos = ToolbarHeight + NormalYSpace;
        _filterRegion = new Rect(10, graphicYPos + NormalYSpace, 300, 200);
        _graphRegion = new Rect(0, ToolbarHeight, position.width, position.height - ToolbarHeight);
    }

    private void DrawToolbarGui()
    {
        GUILayout.BeginArea(_toolbarRegion);
        // 绘制工具栏
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("收集引用依赖", GUILayout.Width(BtnNormalWidth * 2)))
        {
            RefDepCollecter.Ins.ReCollectInfo();
        }
        if (GUILayout.Button("刷新", GUILayout.Width(BtnNormalWidth)))
        {
            RefreshShowcaseList(_showcaseGuid);
        }
        if (GUILayout.Button("上一个", GUILayout.Width(BtnNormalWidth)))
        {
            if (_curShowcaseIndex > 0)
            {
                _curShowcaseIndex -= 1;
                RefreshShowcaseList(_showcaseGuidQueue[_curShowcaseIndex]);
            }
        }
        if (GUILayout.Button("下一个", GUILayout.Width(BtnNormalWidth)))
        {
            if (_curShowcaseIndex + 1 < _showcaseGuidQueue.Count)
            {
                _curShowcaseIndex += 1;
                RefreshShowcaseList(_showcaseGuidQueue[_curShowcaseIndex]);
            }
        }
        if (GUILayout.Button("选中选择", GUILayout.Width(BtnNormalWidth)))
        {
            int selectCount = _selectNodes.Count;
            if (selectCount > 0)
            {
                Object[] selObjects = new Object[selectCount];
                for (int i = 0; i < selectCount; i++)
                {
                    selObjects[i] = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(_selectNodes[i].NodeData.Guid));
                }
                Selection.objects = selObjects;
            }
        }
        if (GUILayout.Button("下拉1", GUILayout.Width(BtnNormalWidth)))
        {

        }
        if (GUILayout.Button("重复", GUILayout.Width(BtnNormalWidth)))
        {

        }
        if (GUILayout.Button("过滤1", GUILayout.Width(BtnNormalWidth)))
        {
            RenderSettings.ambientMode = AmbientMode.Skybox;
            //RenderSettings.ambientLight = Color.red;
        }
        if (GUILayout.Button("过滤2", GUILayout.Width(BtnNormalWidth)))
        {
            EditorSceneManager.OpenScene("Assets/AssetBundleRes/Endless/Scenes/GameScene.unity");
        }
        GUILayout.TextField("资产路径");
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void DrawRefDepGui()
    {
        GUILayout.BeginArea(_graphRegion);
        Rect _backgroundRegion = _graphRegion;
        _backgroundRegion.position = Vector2.zero;
        // 绘制背景
        _background.Draw(_backgroundRegion, Vector2.zero);
        // 更新节点静态参数
        int nodeCount = _allNodes.Count;
        if (nodeCount > 0)
            RefDepNodeGUI.UpdateStaticParamBeforeDraw();
        // 绘制所有节点
        BeginWindows();
        foreach (RefDepNodeGUI node in _allNodes)
        {
            node.DrawNode();
        }
        HandleGraphGuiDo();
        DynamicGuiModeDo();
        EndWindows();
        // 必须要放到Area中
        DrawConnectGui();
        GUILayout.EndArea();
    }

    private void HandleGraphGuiDo()
    {
        Event evt = Event.current;
        int id = GUIUtility.GetControlID(kDragNodesControlID, FocusType.Passive);
        EventType evtType = evt.GetTypeForControl(id);
        Vector2 curMousePos = evt.mousePosition;
        // 
        switch (evtType)
        {
            case EventType.MouseDown:
                if (_dynamicGuiMode == DynamicGuiMode.None)
                {
                    // 判断是否在Filter窗体内
                    // 这里的鼠标位置是基于graphic Region中0,0
                    if (_filterRegion.Contains(curMousePos + _graphRegion.position) == false)
                    {
                        switch (evt.button)
                        {
                            case 0:
                                LeftMouseDownDo(evt);
                                break;
                            case 1:
                                RightMouseDownDo(evt);
                                break;
                            default:
                                break;
                        }
                    }
                    _curFrameDragPos = Vector2.zero;
                    _lastMousePos = evt.mousePosition;
                    _lastMouseDownTick = DateTime.Now.Ticks;
                    _startMousePos = evt.mousePosition;
                    _canCalcDynamicGui = true;
                }
                break;
            case EventType.MouseDrag:
                // 记录拖动距离
                _curFrameDragPos = evt.mousePosition - _lastMousePos;
                _lastMousePos = evt.mousePosition;
                _canCalcDynamicGui = true;
                //Debug.Log("拖拽中");
                switch (evt.button)
                {
                    case 0:
                        LeftMouseDragDo(evt);
                        break;
                    case 1:
                        RightMouseDragDo(evt);
                        break;
                    default:
                        break;
                }
                break;
            case EventType.MouseUp:
                // 拖动结束标志
                if (_filterRegion.Contains(curMousePos + _graphRegion.position) == false)
                {
                    switch (evt.button)
                    {
                        case 0:
                            LeftMouseUpDo(evt);
                            break;
                        case 1:
                            RightMouseUpDo(evt);
                            break;
                        default:
                            break;
                    }
                }
                _dynamicGuiMode = DynamicGuiMode.None;
                //HandleUtility.Repaint();
                Debug.Log("拖拽结束");
                break;
            case EventType.ScrollWheel:
                _curRoomChange += evt.delta.y;
                if (_curRoomChange > _roomLvChangeLimit || _curRoomChange < -_roomLvChangeLimit)
                {
                    ZoomLv += (int)(_curRoomChange / _roomLvChangeLimit);
                    _curRoomChange = 0;
                    ZoomLv = Mathf.Max(ZoomLvMin, ZoomLv);
                    ZoomLv = Mathf.Min(ZoomLvMax, ZoomLv);
                    // 更新NodeRegion
                    LayoutNodePos();
                    HandleUtility.Repaint();
                }
                break;

            // case EventType.MouseMove:
            //    break;
            // case EventType.KeyDown:
            //    break;
            // case EventType.KeyUp:
            //    break;
            // case EventType.ScrollWheel:
            //    break;
            // case EventType.Repaint:
            //    break;
            // case EventType.Layout:
            //    break;
            // case EventType.DragUpdated:
            //    break;
            // case EventType.DragPerform:
            //    break;
            // case EventType.DragExited:
            //    break;
            // case EventType.Ignore:
            //    break;
            // case EventType.Used:
            //    break;
            // case EventType.ValidateCommand:
            //    break;
            // case EventType.ExecuteCommand:
            //    break;
            // case EventType.ContextClick:
            //    break;
            // case EventType.MouseEnterWindow:
            //    break;
            // case EventType.MouseLeaveWindow:
            //    break;
            // case EventType.TouchDown:
            //    break;
            // case EventType.TouchUp:
            //    break;
            // case EventType.TouchMove:
            //    break;
            // case EventType.TouchEnter:
            //    break;
            // case EventType.TouchLeave:
            //    break;
            // case EventType.TouchStationary:
            //    break;
            default:
                break;
        }
    }

    private void LeftMouseDownDo(Event evt)
    {
        Vector2 curMousePos = evt.mousePosition;
        // 判断是否为双击
        float xChange = Mathf.Abs(curMousePos.x - _lastMousePos.x);
        float yChange = Mathf.Abs(curMousePos.y - _lastMousePos.y);
        if (xChange <= 5 && yChange <= 5)
        {
            long subTick = DateTime.Now.Ticks - _lastMouseDownTick;
            float millSec = (float)(TimeSpan.FromTicks(subTick).TotalMilliseconds);
            if (millSec < 200)
            { // 判断为双击
                _dynamicGuiMode = DynamicGuiMode.LeftDoubleClick;
                string focusGuid = "";
                foreach (RefDepNodeGUI node in _allNodes)
                {
                    if (node.NodeRegion.Contains(curMousePos))
                    {// 将点击节点居中且刷新界面，并选中点击节点
                        focusGuid = node.Guid;
                        break;
                    }
                }
                if (focusGuid != "")
                {
                    RefreshShowcaseList(new List<string>() { focusGuid }, true);
                    foreach (RefDepNodeGUI node in _allNodes)
                    {
                        if (node.Guid == focusGuid)
                        {
                            _selectNodes.Add(node);
                            node.SetIsSelect(true);
                            Selection.activeObject = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(focusGuid), typeof(Object));
                            break;
                        }
                    }
                    
                    HandleUtility.Repaint();
                    return;
                }
            }
        }
        _canDragNode = false;
        // 当前鼠标位置如果在某个选中的Node上，准备拖拽所有选中Node移动
        foreach (RefDepNodeGUI node in _selectNodes)
        {
            if (node.NodeRegion.Contains(curMousePos))
            {
                _canDragNode = true;
                _dynamicGuiMode = DynamicGuiMode.LeftDragMove;
                Debug.Log("左键拖拽移动");
                break;
            }
        }
        // 当前鼠标如果不在某个选中Node上，则准备进入选择Node模式
        if (_canDragNode == false)
        {
            _dynamicGuiMode = DynamicGuiMode.LeftDragToSelect;
            Debug.Log("左键拖拽选择");
        }
    }

    private void RightMouseDownDo(Event evt)
    {
        // 拖拽所有节点
        _dynamicGuiMode = DynamicGuiMode.RightDragMove;
        Debug.Log("右键拖拽移动");
    }

    private void LeftMouseDragDo(Event evt)
    {
        if (_dynamicGuiMode == DynamicGuiMode.LeftDragToSelect || _dynamicGuiMode == DynamicGuiMode.LeftDragMove)
        {
            HandleUtility.Repaint();
        }
    }

    private void RightMouseDragDo(Event evt)
    {
        // 如果鼠标右键按下，拖动整体移动
        if (_dynamicGuiMode == DynamicGuiMode.RightDragMove)
        {
            HandleUtility.Repaint();
        }
    }

    private void LeftMouseUpDo(Event evt)
    {
        if (_dynamicGuiMode == DynamicGuiMode.LeftDragToSelect || _dynamicGuiMode == DynamicGuiMode.LeftDragMove || _dynamicGuiMode == DynamicGuiMode.None)
        {
            HandleUtility.Repaint();
        }
    }

    private void RightMouseUpDo(Event evt)
    {
        if (_dynamicGuiMode == DynamicGuiMode.RightDragMove || _dynamicGuiMode == DynamicGuiMode.None)
        {
            HandleUtility.Repaint();
        }
    }

    private void DrawConnectGui()
    {
        foreach (RefDepNodeGUI node in _allNodes)
        {
            node.DrawConnectLine();
        }
    }

    private void DrawFilterAndSettingGui()
    {
        // 过滤设置框
        GUILayout.BeginArea(_filterRegion);
        EditorGUILayout.BeginVertical();
        // 搜索框
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("搜索", GUILayout.Width(BtnNormalWidth)))
        {
        }
        EditorGUI.BeginChangeCheck();
        _strSearch = GUILayout.TextField(_strSearch);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (RefDepNodeGUI selectNode in _selectNodes)
            {
                selectNode.SetIsSelect(false);
            }
            _selectNodes.Clear();
            if (string.IsNullOrEmpty(_strSearch) == false)
            {
                foreach (RefDepNodeGUI node in _allNodes)
                {
                    if (node.Name.Contains(_strSearch))
                    {
                        node.SetIsSelect(true);
                        _selectNodes.Add(node);
                    }
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.BeginChangeCheck();
        // 引用展示深度
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("引用展示深度:", GUILayout.Width(FilterDescWidth));
        _strRefDepthLimit = GUILayout.TextField(_strRefDepthLimit);
        EditorGUILayout.EndHorizontal();
        // 依赖展示深度
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("依赖展示深度:", GUILayout.Width(FilterDescWidth));
        _strDepDepthLimit = GUILayout.TextField(_strDepDepthLimit);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        // 展示结果数量限制
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("结果数量限制:", GUILayout.Width(FilterDescWidth));
        _strChildCountLimit = GUILayout.TextField(_strChildCountLimit);
        EditorGUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            RefreshShowcaseList(_showcaseGuid);
        }
        GUILayout.EndArea();
    }

    private void DynamicGuiModeDo()
    {
        if (_canCalcDynamicGui)
        {
            DynamicGuiCalc();
            _canCalcDynamicGui = false;
        }
        DrawDynamicGui();
    }

    private void DynamicGuiCalc()
    {
        switch (_dynamicGuiMode)
        {
            case DynamicGuiMode.None:
                break;
            case DynamicGuiMode.LeftDragToSelect:
                Vector2 curMousePos = Event.current.mousePosition;
                float maxX = Mathf.Max(_startMousePos.x, curMousePos.x);
                float maxY = Mathf.Max(_startMousePos.y, curMousePos.y);
                float minX = Mathf.Min(_startMousePos.x, curMousePos.x);
                float minY = Mathf.Min(_startMousePos.y, curMousePos.y);
                Rect selRegion = new Rect(minX, minY, maxX - minX, maxY - minY);
                foreach (RefDepNodeGUI node in _selectNodes)
                {
                    node.SetIsSelect(false);
                }
                _selectNodes.Clear();
                // 动态更新被选中Node
                foreach (RefDepNodeGUI node in _allNodes)
                {
                    // 判断两个矩形区域是否重合
                    if (selRegion.Overlaps(node.NodeRegion))
                    {
                        // 重合则加入选中列表
                        // 并更新选中Style
                        node.SetIsSelect(true);
                        _selectNodes.Add(node);
                    }
                }
                break;
            case DynamicGuiMode.LeftDragMove:
                foreach (RefDepNodeGUI node in _selectNodes)
                {
                    node.AddPos(_curFrameDragPos);
                }
                break;
            case DynamicGuiMode.RightDragMove:
                foreach (RefDepNodeGUI node in _allNodes)
                {
                    node.AddPos(_curFrameDragPos);
                }
                break;
            case DynamicGuiMode.ScrollWheel:
                break;
            default:
                break;
        }
    }

    private void DrawDynamicGui()
    {
        switch (_dynamicGuiMode)
        {
            case DynamicGuiMode.None:
                break;
            case DynamicGuiMode.LeftDragToSelect:
                // 画框选择Node
                float maxX = Mathf.Max(_startMousePos.x, Event.current.mousePosition.x);
                float maxY = Mathf.Max(_startMousePos.y, Event.current.mousePosition.y);
                float minX = Mathf.Min(_startMousePos.x, Event.current.mousePosition.x);
                float minY = Mathf.Min(_startMousePos.y, Event.current.mousePosition.y);
                Rect selRegion = new Rect(minX, minY, maxX - minX, maxY - minY);
                GUI.Label(selRegion, string.Empty, "SelectionRect");
                break;
            case DynamicGuiMode.LeftDragMove:
                break;
            case DynamicGuiMode.RightDragMove:
                break;
            case DynamicGuiMode.ScrollWheel:
                break;
            default:
                break;
        }
    }

    private void SetStyle()
    {
        GUIStyle _tabStyle;
        _tabStyle = new GUIStyle();
        _tabStyle.alignment = TextAnchor.MiddleCenter;// 字体对齐
        _tabStyle.fontSize = 16;// 字体大小
        Texture2D tabNormal = Resources.Load<Texture2D>("Tab_Normal");
        Texture2D tabSelected = Resources.Load<Texture2D>("Tab_(selected");
        Font tabFont = Resources.Load<Font>("Oswald-Regular");
        _tabStyle.font = tabFont;// 字体
        _tabStyle.fixedHeight = 40;// 设置高度
        _tabStyle.normal.background = tabNormal;// 默认状态下背景
        _tabStyle.normal.textColor = Color.grey;// 默认状态下字体颜色
        _tabStyle.onNormal.background = tabSelected;
        _tabStyle.onNormal.textColor = Color.black;
        _tabStyle.onFocused.background = tabSelected; // 选中状态下背景
        _tabStyle.onFocused.textColor = Color.black;
        // 选中状态下字体颜色
        _tabStyle.border = new RectOffset(18, 18, 20, 4); // 设置)边界防止图片变形(九宫格)
    }

    //private void AscOrderNode(int start, int end)
    //{
    //    int nodeCount = _allNodes.Count;
    //    if (start < 0 || end >= nodeCount || start >= end)
    //        return;
    //    int tempDepth;
    //    int tempChildCount;
    //    RefDepNodeGUI tempGui;
    //    for (int i = start; i <= end; i++)
    //    {
    //        for (int j = start; j < end - i + start; j++)
    //        {
    //            int depthJ = _allNodesDepth[j];
    //            int depthJ1 = _allNodesDepth[j + 1];
    //            if (depthJ > depthJ1)
    //            {
    //                tempDepth = depthJ;
    //                _allNodesDepth[j] = _allNodesDepth[j + 1];
    //                _allNodesDepth[j + 1] = tempDepth;
    //                //tempChildCount = _allNodeChildCount[j];
    //                //_allNodeChildCount[j] = _allNodeChildCount[j + 1];
    //                //_allNodeChildCount[j + 1] = tempChildCount;
    //                tempGui = _allNodes[j];
    //                _allNodes[j] = _allNodes[j + 1];
    //                _allNodes[j + 1] = tempGui;
    //            }
    //        }
    //    }
    //}

    //private void DescOrderNode(int start, int end)
    //{
    //    int nodeCount = _allNodes.Count;
    //    if (start < 0 || end >= nodeCount || start >= end)
    //        return;
    //    int tempDepth;
    //    int tempChildCount;
    //    RefDepNodeGUI tempGui;
    //    for (int i = start; i <= end; i++)
    //    {
    //        for (int j = start; j < end - i + start; j++)
    //        {
    //            int depthJ = _allNodesDepth[j];
    //            int depthJ1 = _allNodesDepth[j + 1];
    //            if (depthJ < depthJ1)
    //            {
    //                tempDepth = depthJ;
    //                _allNodesDepth[j] = _allNodesDepth[j + 1];
    //                _allNodesDepth[j + 1] = tempDepth;
    //                //tempChildCount = _allNodeChildCount[j];
    //                //_allNodeChildCount[j] = _allNodeChildCount[j + 1];
    //                //_allNodeChildCount[j + 1] = tempChildCount;
    //                tempGui = _allNodes[j];
    //                _allNodes[j] = _allNodes[j + 1];
    //                _allNodes[j + 1] = tempGui;
    //            }
    //        }
    //    }
    //}
}
