#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    public class OtherToolBatterUseUnity
    {
        private const int OtherToolBatterUseUnityStartIndex = EditorToolsUtility.OtherToolStartIndex + EditorToolsUtility.OtherToolBatterUseUnityOffset; //控制右键菜单位置
        
        [MenuItem("Assets/Tools_Other/拷贝选中对象路径到剪切板", false, OtherToolBatterUseUnityStartIndex)]
        private static void GetSelectionAssetPath()
        {
            string assetPath = string.Empty;
            if (Selection.activeObject != null)
            {
                assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                GUIUtility.systemCopyBuffer = assetPath;
            }
        }
    }
}
#endif