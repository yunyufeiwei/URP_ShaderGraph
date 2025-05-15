using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class RefReplacerWindow : EditorWindow
{
    private static List<Object> _assetList = new List<Object>();
    private ReorderableList _reorderList;
    private Vector2 _scrollPos;

    [MenuItem("Window/LaoWang/Asset Ref Replacer")]
    static void Init()
    {
        RefReplacerWindow window = (RefReplacerWindow)GetWindow<RefReplacerWindow>("引用替换工具");
        window.minSize = new Vector2(300, 400);
        window.Show();
    }

    private void OnEnable()
    {
        _reorderList = new ReorderableList(_assetList, typeof(Object), true, true, true, true);
        _reorderList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            //_assetList[index] = EditorGUILayout.ObjectField("Target Asset", _assetList[index], typeof(Object), false);
            _assetList[index] = EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Target Asset", _assetList[index], typeof(Object), false);

        };
    }

    void OnGUI()
    {
        GUILayout.Label("1、请在下列列表中添加想要替换引用关系的资产");
        GUILayout.Label("2、选中其中一个资产并点击下列替换引用按钮");
        GUILayout.Label("3、工具会将其它资产的引用关系用选中资产替代");
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        _reorderList.DoLayoutList();
        if (GUILayout.Button("清空列表"))
        {
            _assetList.Clear();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("替换引用"))
        {
            ReplaceReferences(true);
        }
        if (GUILayout.Button("替换引用并删除无用资产，慎用"))
        {
            ReplaceReferencesAndRemove();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    private void ReplaceReferencesAndRemove()
    {
        ReplaceReferences(true);
        //int selIndex = _reorderList.selectedIndices[0];
        int selIndex = _reorderList.index;
        int assetCount = _assetList.Count;
        for (int i = 0; i < assetCount; i++)
        {
            if (i == selIndex)
                continue;
            // RefDepCollector列表移除
            RefDepCollecter.Ins.RemoveAsset(_assetList[i]);
            // Unity资产中移除
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_assetList[i]));
            // _assetList列表移除
            _assetList.RemoveAt(i);
            i -= 1;
            assetCount -= 1;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="canDialog">在打包后处理等逻辑中，不可调用模态窗，以免阻塞进程</param>
    private void ReplaceReferences(bool canDialog = false)
    {
        ReplaceRefCommonDo();
        int assetCount = _assetList.Count;
        if (assetCount <= 1)
        {
            ShowDialog("替换引用关系至少需要两个非空资产", canDialog);
            return;
        }

        //int selectCount = _reorderList.selectedIndices.Count;
        //if (selectCount != 1)
        //{
        //    ShowDialog("应仅选中1个资产，工具会将其它资产的引用替换为选中资产", canDialog);
        //    return;
        //}

        Type assetType = _assetList[0].GetType();
        for (int i = 1; i < assetCount; i++)
        {
            Object asset = _assetList[i];
            if (asset.GetType() != assetType)
            {
                ShowDialog("用于替换引用的所有资产类型必须一致", canDialog);
                return;
            }
        }
        // 重新构建引用依赖关系
        RefDepCollecter.Ins.ReCollectInfo();
        int selIndex = _reorderList.index;
        //int selIndex = _reorderList.selectedIndices[0];
        Object selObj = _assetList[selIndex];
        string selPath = AssetDatabase.GetAssetPath(selObj);
        string selGuid = AssetDatabase.AssetPathToGUID(selPath);
        RefDepAssetInfo selInfo = RefDepCollecter.Ins.GetAssetInfoByGuid(selGuid);
        Dictionary<string, List<string>> refScene2ReplaceObjDic = new Dictionary<string, List<string>>();
        // 定位替换用资产，
        foreach (Object obj in _assetList)
        {
            if (ReferenceEquals(obj, selObj) == true)
                continue;
            string assetPath = AssetDatabase.GetAssetPath(obj);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            RefDepAssetInfo info = RefDepCollecter.Ins.GetAssetInfoByGuid(guid);
            if (info == null)
                continue;
            foreach (string refGuid in info.RefList)
            {
                string refPath = AssetDatabase.GUIDToAssetPath(refGuid);
                Object refObj = AssetDatabase.LoadAssetAtPath(refPath, typeof(Object));
                bool isRefPrefab = refPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase);
                // 预制体资产特殊处理
                bool isPrefabReplace = isRefPrefab && assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) && selPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase);
                if (isPrefabReplace)
                {
                    DoAllPrefabReplace(refObj, obj, selObj);
                    continue;
                }

                if (isRefPrefab)
                {// 对于Prefab本身也需要特殊处理，SerializedObject无法处理
                    DoPrefabReplace(refObj, obj, selObj);
                }

                // 场景资产也要特殊处理，且场景之类的主资产，应该最后处理
                bool isScene = refPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);
                if (isScene)
                {
                    List<string> replaceObj;
                    if (!refScene2ReplaceObjDic.TryGetValue(refPath, out replaceObj))
                    {
                        replaceObj = new List<string>();
                        refScene2ReplaceObjDic.Add(refPath, replaceObj);
                    }
                    replaceObj.Add(assetPath);
                    continue;
                }

                // 替换引用关系
                SerializedObject refSer = new SerializedObject(refObj);
                SerializedProperty property = refSer.GetIterator();
                bool hasChanges = false;
                while (property.NextVisible(true))
                {
                    Debug.Log(property.name + "  " + property.displayName + "  " + property.propertyType);
                    if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == obj)
                    {
                        property.objectReferenceValue = selObj;
                        hasChanges = true;
                    }
                }
                if (hasChanges)
                {
                    refSer.ApplyModifiedProperties();
                    EditorUtility.SetDirty(refObj);
                }
            }// foreach string
            // 统一处理场景资产
            foreach (KeyValuePair<string, List<string>> kvPair in refScene2ReplaceObjDic)
            {
                if (selPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    DoScenePrefabReplace(kvPair.Key, kvPair.Value, selPath);
                else
                    DoSceneReplace(kvPair.Key, kvPair.Value, selPath);
            }
            // 最后一个场景保存会失败，需要确认原因
            // 更新RefDepCollecter引用关系
            RefDepCollecter.Ins.ReplaceRef(info, selInfo);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    #region 针对预制体的特殊处理
    private void DoAllPrefabReplace(Object refObj, Object oldObj, Object newObj)
    {
        GameObject refGameObj = PrefabUtility.InstantiatePrefab(refObj) as GameObject;
        GameObject oldGameObj = oldObj as GameObject;
        GameObject newGameObj = newObj as GameObject;
        if (DoAllPrefabReplace(refGameObj, oldGameObj, newGameObj))
        {// 
            PrefabUtility.SaveAsPrefabAsset(refGameObj, AssetDatabase.GetAssetPath(refObj));
            AssetDatabase.Refresh();
        }
        DestroyImmediate(refGameObj);
    }

    private bool DoAllPrefabReplace(GameObject refObj, GameObject oldObj, GameObject newObj)
    {
        // 自身无需处理，处理子级即可
        Transform refTrans = refObj.transform;
        int childCount = refTrans.childCount;
        bool haveChange = false;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = refTrans.GetChild(i);
            //Debug.Log(AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromOriginalSource(child)));
            if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(child.gameObject) == oldObj)
            {// 找到待替换嵌套预制体
             //重新复制newObj
                GameObject replaceObj = (GameObject)PrefabUtility.InstantiatePrefab(newObj);
                Transform replaceTrans = replaceObj.transform;
                replaceTrans.SetParent(refTrans, true);
                replaceTrans.SetSiblingIndex(child.GetSiblingIndex());
                replaceTrans.localPosition = child.localPosition;
                replaceTrans.localRotation = child.localRotation;
                replaceTrans.localScale = child.localScale;
                DestroyImmediate(child.gameObject);
                haveChange = true;
                continue;
            }
            //}
            if (child.childCount > 0)
            {
                if (DoAllPrefabReplace(child.gameObject, oldObj, newObj) == true)
                    haveChange = true;
            }
        }
        return haveChange;
    }

    private void DoPrefabReplace(Object refObj, Object oldObj, Object newObj)
    {
        GameObject refGameObj = PrefabUtility.InstantiatePrefab(refObj) as GameObject;
        Component[] allCmpt = refGameObj.GetComponentsInChildren<Component>();
        foreach (Component cmpt in allCmpt)
        {
            Type cmptType = cmpt.GetType();
            if (cmptType == typeof(Transform) || cmptType == typeof(RectTransform))
                continue;
            SerializedObject serObj = new SerializedObject(cmpt);
            SerializedProperty prop = serObj.GetIterator();
            bool haveChange = false;
            while (prop.NextVisible(true))
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue == oldObj)
                {
                    haveChange = true;
                    prop.objectReferenceValue = newObj;
                }
            }
            if (haveChange)
                serObj.ApplyModifiedProperties();
        }
        PrefabUtility.SaveAsPrefabAsset(refGameObj, AssetDatabase.GetAssetPath(refObj));
        AssetDatabase.Refresh();
        DestroyImmediate(refGameObj);
    }
    #endregion

    #region 针对场景的特殊处理
    private void DoSceneReplace(string scenePath, List<string> oldAssetPaths, string newAssetPath)
    {
        // 打开场景
        EditorSceneManager.OpenScene(scenePath);
        // 加载相关资产
        List<Object> oldAssets = new List<Object>(oldAssetPaths.Count);
        foreach (string path in oldAssetPaths)
        {
            oldAssets.Add(AssetDatabase.LoadAssetAtPath<Object>(path));
        }
        Object newAsset = AssetDatabase.LoadAssetAtPath(newAssetPath, typeof(Object));
        GameObject[] allObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        List<Component> components = new List<Component>();
        foreach (GameObject go in allObjects)
        {
            go.GetComponentsInChildren(true, components);
            foreach (Component component in components)
            {
                bool haveChange = false;
                SerializedObject serObj = new SerializedObject(component);
                SerializedProperty prop = serObj.GetIterator();
                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference && oldAssets.Contains(prop.objectReferenceValue))
                    {
                        prop.objectReferenceValue = newAsset;
                        haveChange = true;
                    }
                }
                // 应用更改
                if (haveChange)
                    serObj.ApplyModifiedProperties();
            }
        }
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    // 针对场景预制体的特殊处理
    private void DoScenePrefabReplace(string scenePath, List<string> oldAssetPaths, string newAssetPath)
    {
        // 打开场景
        EditorSceneManager.OpenScene(scenePath);
        // 加载相关资产
        List<GameObject> oldAssets = new List<GameObject>(oldAssetPaths.Count);
        foreach (string path in oldAssetPaths)
        {
            oldAssets.Add(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        }
        GameObject newObj = AssetDatabase.LoadAssetAtPath<GameObject>(newAssetPath);
        GameObject[] allObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject obj in allObjects)
        {
            DoScenePrefabReplace(obj, oldAssets, newObj);
        }
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    private void DoScenePrefabReplace(GameObject obj, List<GameObject> oldObjs, GameObject newObj)
    {
        // 处理自身
        Transform refTrans = obj.transform;
        Transform refParentTrans = refTrans.parent;
        GameObject originalGameObj = PrefabUtility.GetCorrespondingObjectFromOriginalSource(refTrans.gameObject);
        if (oldObjs.Contains(originalGameObj))
        {// 找到待替换嵌套预制体
            //重新复制newObj
            GameObject replaceObj = (GameObject)PrefabUtility.InstantiatePrefab(newObj);
            Transform replaceTrans = replaceObj.transform;
            if (refParentTrans != null)
                replaceTrans.SetParent(refParentTrans, true);
            replaceTrans.SetSiblingIndex(refTrans.GetSiblingIndex());
            replaceTrans.localPosition = refTrans.localPosition;
            replaceTrans.localRotation = refTrans.localRotation;
            replaceTrans.localScale = refTrans.localScale;
            DestroyImmediate(refTrans.gameObject);
        }
        else
        {
            int childCount = refTrans.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = refTrans.GetChild(i);
                DoScenePrefabReplace(child.gameObject, oldObjs, newObj);
            }
        }
    }
    #endregion

    private static void ReplaceRefCommonDo()
    {
        int itemCount = _assetList.Count;
        // 清理掉空资产
        for (int i = 0; i < itemCount; i++)
        {
            if (_assetList[i] == null)
            {
                _assetList.RemoveAt(i);
                i -= 1;
                itemCount -= 1;
            }
        }
        // 清理掉重复资产
        for (int i = 0; i < itemCount - 1; i++)
        {
            for (int j = i + 1; j < itemCount; j++)
            {
                if (_assetList[i] == _assetList[j])
                {
                    _assetList.RemoveAt(j);
                    j -= 1;
                    itemCount -= 1;
                }
            }
        }
    }

    private void ShowDialog(string msg, bool canDialog)
    {
        if (canDialog == false)
            Debug.Log(msg);
        else
            EditorUtility.DisplayDialog("提示", msg, "OK");
    }
}
