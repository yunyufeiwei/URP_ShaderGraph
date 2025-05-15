using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RefDepNodeGUI : ScriptableObject
{
    #region Const Data
    public const float NodeBaseWidth = 150;
    public const float NodeBaseHeight = 10;
    public const float NodeTitleHeightMargin = 8;
    public const float NodeWidthMargin = 30;
    #endregion

    private static int _genCount = 0;
    private static GUIStyle _assetNameStyleNor;
    private static GUIStyle _assetTypeStyleNor;
    private static GUIStyle _assetNameStyle;
    private static GUIStyle _assetTypeStyle;
    private static float _scale = 1;

    private int _nodeWindowId;
    private Rect _nodeRegion;
    private RefDepNodeData _nodeData;
    private string _styleName;
    private string _selStyleName;
    private string _assetType;

    private bool _haveSelect = false;
    private GUIStyle _style;
    private GUIStyle _selStyle;
    private GUIContent _nameContent;
    private GUIContent _typeContent;
    private Vector2 _nameSize;
    private Vector2 _typeSize;

    private List<RefDepNodeGUI> _childRefGui = new List<RefDepNodeGUI>();
    public List<RefDepNodeGUI> ChildRefGui { get { return _childRefGui; } }
    private List<RefDepNodeGUI> _childDepGui = new List<RefDepNodeGUI>();
    public List<RefDepNodeGUI> ChildDepGui { get { return _childDepGui; } }
    private int _refCount = 0;
    // 需要注意的是RefCount和_childRefGui.Count是不同的，RefCount是叶子节点数量。_childRefGui.Count是一级子节点数量
    public int RefCount { get { return _refCount; } }
    private int _depCount = 0;
    public int DepCount { get { return _depCount; } }


    public string Name
    {
        get { return _nodeData.Name; }
        set
        {
            _nodeData.Name = value;
            name = value;
        }
    }

    public string Guid { get { return _nodeData.Guid; } }
    public RefDepNodeData NodeData { get { return _nodeData; } }
    public Rect NodeRegion { get { return _nodeRegion; } }
    public int WindowId { get { return _nodeWindowId; } set { _nodeWindowId = value; } }

    public static void InitNodeGuiStaticParam()
    {
        _assetNameStyleNor = RefDepUtility.NodeSkin.FindStyle("node asset name");
        _assetTypeStyleNor = RefDepUtility.NodeSkin.FindStyle("node asset type");
        _assetNameStyle = new GUIStyle(_assetNameStyleNor);
        _assetTypeStyle = new GUIStyle(_assetTypeStyleNor);
    }

    public static RefDepNodeGUI CreateNodeGui(RefDepNodeData data)
    {
        RefDepNodeGUI node = CreateInstance<RefDepNodeGUI>();
        node.Init(data);
        node._nodeWindowId = _genCount;
        _genCount++;
        return node;
    }

    public static void UpdateStaticParamBeforeDraw()
    {
        _scale = RefDepWindow._scale;
        _assetNameStyle.fontSize = (int)(_assetNameStyleNor.fontSize * _scale);
        _assetTypeStyle.fontSize = (int)(_assetTypeStyleNor.fontSize * _scale);
    }

    private void Init(RefDepNodeData data)
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.DontSave;
        _nodeWindowId = 0;
        _nodeData = data;
        name = data.Name;
        _assetType = data.Type;
        _nodeRegion = new Rect(data.PosX, data.PosY, NodeBaseWidth, NodeBaseHeight);
        _styleName = data.StyleName;
        _selStyleName = data.SelStyleName;
        _style = RefDepUtility.NodeSkin.FindStyle(_styleName);
        _selStyle = RefDepUtility.NodeSkin.FindStyle(_selStyleName);
        _nameContent = new GUIContent(name);
        _typeContent = new GUIContent(_assetType);
        _nameSize = _assetNameStyle.CalcSize(_nameContent);
        _typeSize = _assetTypeStyle.CalcSize(_typeContent);
        _nameSize.y *= 0.5f;
        _nameSize.y += NodeTitleHeightMargin;
        _typeSize.y *= 0.5f;
        _typeSize.y += NodeTitleHeightMargin;
    }

    public void DrawNode()
    {
        GUI.Window(_nodeWindowId, _nodeRegion, DrawRefDepNode, "", (_haveSelect ? _selStyle : _style));
    }

    private void DrawRefDepNode(int id)
    {
        UpdateNodeRect();
        //HandleNodeMouseEvent();
        DrawNodeContent();
        //DrawConnectLine();
    }

    private void UpdateNodeRect()
    {
        float newHeight = (_nameSize.y + _typeSize.y + NodeBaseHeight) * _scale;
        float newWidth = (Mathf.Max(NodeBaseWidth, Mathf.Max(_nameSize.x, _typeSize.x) + NodeWidthMargin) + 30) * _scale;
        _nodeRegion.width = newWidth;
        _nodeRegion.height = newHeight;
    }

    private void DrawNodeContent()
    {
        GUI.Label(new Rect(0, NodeTitleHeightMargin * _scale, 30 * _scale, 30 * _scale), _nodeData.Icon);
        // 这里暂不知为何CalcSize计算所得size的y值那么大，所以Y值方面做一定调节
        Rect nameRect = new Rect(0, NodeTitleHeightMargin * 2f * _scale, _nodeRegion.width, _nameSize.y * _scale);
        GUI.Label(nameRect, Name, _assetNameStyle);
        Rect typeRect = new Rect(0, nameRect.height + NodeTitleHeightMargin * _scale, _nodeRegion.width, _typeSize.y * _scale);
        GUI.Label(typeRect, _assetType, _assetTypeStyle);
    }

    public void DrawConnectLine()
    {
        int childRef = _childRefGui.Count;
        if (childRef > 0)
        {
            Vector3 curLcPos = GetLeftCenterPos();
            foreach (RefDepNodeGUI childGui in _childRefGui)
            {
                Vector3 refRcPos = childGui.GetRightCenterPos();
                Handles.DrawBezier(refRcPos, curLcPos, refRcPos + Vector3.right * 50, curLcPos + Vector3.left * 50, Color.white, null, 2f);
            }
        }
        int childDep = _childDepGui.Count;
        if (childDep > 0)
        {
            Vector3 curRcPos = GetRightCenterPos();
            foreach (RefDepNodeGUI childGui in _childDepGui)
            {
                Vector3 depLcPos = childGui.GetLeftCenterPos();
                Handles.DrawBezier(curRcPos, depLcPos, curRcPos + Vector3.right * 50, depLcPos + Vector3.left * 50, Color.white, null, 2f);
            }
        }
    }

    public void SetIsSelect(bool isSelect)
    {
        _haveSelect = isSelect;
    }

    public Rect GetRect()
    {
        return _nodeRegion;
    }

    public Vector2 GetLeftCenterPos()
    {
        return new Vector2(_nodeRegion.position.x, _nodeRegion.position.y + _nodeRegion.height * 0.5f);
    }

    public Vector2 GetRightCenterPos()
    {
        return new Vector2(_nodeRegion.position.x + _nodeRegion.width, _nodeRegion.position.y + _nodeRegion.height * 0.5f);
    }

    public void AddPos(Vector2 offset)
    {
        _nodeRegion.position += offset;
        _nodeData.PosX += offset.x;
        _nodeData.PosY += offset.y;
    }

    public void SetPos(Vector2 pos)
    {
        _nodeRegion.position = pos;
        _nodeData.PosX = pos.x;
        _nodeData.PosY = pos.y;
    }

    public Vector2 GetPos()
    {
        return _nodeRegion.position;
    }

    public void AddChild(RefDepNodeGUI gui, bool isRef = true)
    {
        if (isRef)
        {
            if (!_childRefGui.Contains(gui))
                _childRefGui.Add(gui);
        }
        else
        {
            if (!_childDepGui.Contains(gui))
                _childDepGui.Add(gui);
        }
    }

    public int CalcDepChildCount()
    {
        int depCount = 0;
        foreach (RefDepNodeGUI depGui in _childDepGui)
        {
            depCount += depGui.CalcDepChildCount();
        }
        if (depCount == 0)
            depCount = 1;
        _depCount = depCount;
        return depCount;
    }

    public int CalcRefChildCount()
    {
        int refCount = 0;
        foreach (RefDepNodeGUI refGui in _childRefGui)
        {
            refCount += refGui.CalcRefChildCount();
        }
        if (refCount == 0)
            refCount = 1;
        _refCount = refCount;
        return refCount;
    }

    public void CalcRegion()
    {
        float newHeight = (_nameSize.y + _typeSize.y + NodeBaseHeight) * _scale;
        float newWidth = Mathf.Max(NodeBaseWidth, Mathf.Max(_nameSize.x, _typeSize.x) + NodeWidthMargin) * _scale;
        _nodeRegion.width = newWidth;
        _nodeRegion.height = newHeight;
    }
}
