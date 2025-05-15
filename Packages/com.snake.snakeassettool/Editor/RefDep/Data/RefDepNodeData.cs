using UnityEngine;


public class RefDepNodeData
{
    private string _name;
    private string _type;
    private string _guid;
    private float _posX;
    private float _posY;
    private string _styleName;
    private string _selStyleName;
    private Texture _icon;

    public string Name { get { return _name; } set { _name = value; } }
    public string Type { get { return _type; } set { _type = value; } }
    public string Guid { get { return _guid; } set { _guid = value; } }
    public float PosX { get { return _posX; } set { _posX = value; } }
    public float PosY { get { return _posY; } set { _posY = value; } }
    public string StyleName { get { return _styleName; } set { _styleName = value; } }
    public string SelStyleName { get { return _selStyleName; } set { _selStyleName = value; } }
    public Texture Icon { get { return _icon; } }

    public RefDepNodeData(string guid, string name, string type, float x, float y, string style, string selStyle, Texture icon)
    {
        _guid = guid;
        _name = name;
        _type = type;
        _posX = x;
        _posY = y;
        _styleName = style;
        _selStyleName = selStyle;
        _icon = icon;
    }
}
