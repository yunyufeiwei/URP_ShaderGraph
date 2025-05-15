using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class RefDepUtility
{
    private const string NodeSkinPath = "Packages/com.gebilaowxj_outlook.snakeassettool/Res/GuiStyle/NodeStyle.guiskin";
    public static GUISkin NodeSkin = AssetDatabase.LoadAssetAtPath<GUISkin>(NodeSkinPath);

    private static Dictionary<RefDepData.RefDepAssetType, Texture> _type2IconDic =
        new Dictionary<RefDepData.RefDepAssetType, Texture>();

    public void AddAssetIconByType(RefDepData.RefDepAssetType type, Object obj)
    {
        if (HaveAssetIcon(type))
            return;
        if (type == RefDepData.RefDepAssetType.Tex)
            return;
        Texture icon = EditorGUIUtility.ObjectContent(obj, typeof(Object)).image;
        _type2IconDic.Add(type, icon);
    }

    private bool HaveAssetIcon(RefDepData.RefDepAssetType type)
    {
        return _type2IconDic.ContainsKey(type);
    }

    public bool GetAssetIconByType(RefDepData.RefDepAssetType type, out Texture tex)
    {
        if (!_type2IconDic.TryGetValue(type, out tex))
        {
            return false;
        }
        return true;
    }

    public static Texture GetIconByPath(string assetPath)
    {
        return EditorGUIUtility.ObjectContent(AssetDatabase.LoadAssetAtPath<Object>(assetPath), typeof(Texture)).image;
    }

    public static Type GetMainTypeByPath(string assetPath)
    {
        return AssetDatabase.GetMainAssetTypeAtPath(assetPath);
    }
}