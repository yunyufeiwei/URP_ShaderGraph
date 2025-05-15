using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class RefDepAssetInfo
{
    public string Guid = string.Empty;
    public List<string> RefList = new List<string>();
    public List<string> DepList = new List<string>();
    public RefDepData.RefDepAssetType AssetType = RefDepData.RefDepAssetType.Unknow;
    private string _assetPath = string.Empty;
    private string _assetName = string.Empty;
    private string _typeDesc = string.Empty;
    private string _style = string.Empty;
    private string _styleOn = string.Empty;
    private Texture _icon = null;
    private bool _haveComplete = false;
    public string AssetPath { get { return _assetPath; } set { _assetPath = value; } }
    public string AssetName { get { return _assetName; } }
    public string TypeDesc { get { return _typeDesc; } }
    public string Style { get { return _style; } }
    public string StyleOn { get { return _styleOn; } }
    public Texture Icon { get { return _icon; } }

    public void AddRefGuid(string guid)
    {
        if (RefList.Contains(guid))
            return;
        RefList.Add(guid);
    }

    public void AddDepGuid(string guid)
    {
        if (DepList.Contains(guid))
            return;
        DepList.Add(guid);
    }

    public void CompleteInfo()
    {
        if (_haveComplete)
            return;
        // 更新AssetName
        _assetName = Path.GetFileNameWithoutExtension(_assetPath);
        // 更新AssetType
        Type type = RefDepUtility.GetMainTypeByPath(_assetPath);
        if (!RefDepCfg.UnityType2MyTypeDic.TryGetValue(type, out AssetType))
        {// 处理无法通过类型直接判断资产
            // 判断是否为Prefab资产
            if (_assetPath.EndsWith(".prefab"))
            {
                AssetType = RefDepData.RefDepAssetType.Prefab;
            }
            // 判断是否为Urp资产
            // 判断是否为Timeline资产
        }
        // 更新AssetTypeDesc
        _typeDesc = RefDepCfg.MyType2DescDic[AssetType];
        // 更新显示样式
        _style = RefDepCfg.MyType2StyleDic[AssetType];
        _styleOn = RefDepCfg.MyType2StyleOnDic[AssetType];
        // 处理Icon
        if (AssetType == RefDepData.RefDepAssetType.Tex)
        {
            _icon = RefDepUtility.GetIconByPath(_assetPath);
        }
        else
        {
            _icon = RefDepCfg.MyType2IconDic[AssetType];
            if (ReferenceEquals(_icon, null) == true)
            {
                // 处理unknow
                if (AssetType == RefDepData.RefDepAssetType.Unknow)
                {
                    _icon = RefDepUtility.GetIconByPath("Packages/com.gebilaowxj_outlook.snakeassettool/Res/Texture/Connected.png");
                }
                else
                {
                    _icon = RefDepUtility.GetIconByPath(_assetPath);
                }
                RefDepCfg.MyType2IconDic[AssetType] = _icon;
            }
        }
        _haveComplete = true;
    }

    // 当子级资产太多时，会将超过阈值的节点折叠以节省性能
    public void SetToFoldAsset(int foldCount)
    {
        // 更新AssetName
        _assetName = "其它" + foldCount.ToString() + "个资产";
        // 更新AssetType
        AssetType = RefDepData.RefDepAssetType.FoldAsset;
        // 更新AssetTypeDesc
        _typeDesc = RefDepCfg.MyType2DescDic[AssetType];
        // 更新显示样式
        _style = RefDepCfg.MyType2StyleDic[AssetType];
        _styleOn = RefDepCfg.MyType2StyleOnDic[AssetType];
        _icon = RefDepCfg.MyType2IconDic[RefDepData.RefDepAssetType.Unknow];
        _haveComplete = false;
    }
}