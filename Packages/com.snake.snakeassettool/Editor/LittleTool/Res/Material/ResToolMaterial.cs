#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    public class ResToolMaterial
    {
        private const int ResToolMaterialStartIndex = EditorToolsUtility.ResToolStartIndex + EditorToolsUtility.ResToolMaterialOffset; //控制右键菜单位置
        
        [MenuItem("Assets/Tools_Res/移除文件夹下材质的无用属性", false, ResToolMaterialStartIndex)]
        private static void RemoveMaterialUnusedPropertyAtSelectPath()
        {
            string[] selectGuids = Selection.assetGUIDs;
            if (selectGuids.Length <= 0)
                return;
            string dirPath = AssetDatabase.GUIDToAssetPath(selectGuids[0]);
            if (!Directory.Exists(dirPath))
            {
                Debug.LogError("注意，请选择一个文件夹！");
                return;
            }

            string[] allMatGuid = EditorToolsUtility.GetAssetsAtPathByFilterAndRemoveRepeat(dirPath, "t:material");
            int matCount = allMatGuid.Length;
            for (int i = 0; i < matCount; i++)
            {
                string matPath = AssetDatabase.GUIDToAssetPath(allMatGuid[i]);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null)
                    continue;
                EditorToolsUtility.RemoveUnusedMaterialProperty(mat);
            }
            AssetDatabase.SaveAssets();
        }
        
        [MenuItem("Assets/Tools_Res/移除材质的无用属性", false, ResToolMaterialStartIndex + 2)]
        private static void RemoveSelectMaterialUnusedProperty()
        {
            string[] selectGuids = Selection.assetGUIDs;
            if (selectGuids.Length <= 0)
                return;
            
            string matPath = AssetDatabase.GUIDToAssetPath(selectGuids[0]);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
                return;
            EditorToolsUtility.RemoveUnusedMaterialProperty(mat);
            AssetDatabase.SaveAssets();
        }

        #region ForBuild
        /// <summary>
        /// 打包前，清理所有Material无用属性
        /// </summary>
        private static void RemoveMaterialUnusedPropertyAtResources()
        {
            string dirPath = EditorToolsUtility.ResourcesRootPath;
            if (!Directory.Exists(dirPath))
            {
                Debug.LogError($"注意，打包工具未找到{dirPath}文件夹！");
                return;
            }

            string[] allMatGuid = EditorToolsUtility.GetAssetsAtPathByFilterAndRemoveRepeat(dirPath, "t:material");
            int matCount = allMatGuid.Length;
            for (int i = 0; i < matCount; i++)
            {
                string matPath = AssetDatabase.GUIDToAssetPath(allMatGuid[i]);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null)
                    continue;
                EditorToolsUtility.RemoveUnusedMaterialProperty(mat);
            }
            AssetDatabase.SaveAssets();
        }
        #endregion
    }
}
#endif
