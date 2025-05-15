#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EditorTools
{
    public class EditorToolsUtility
    {
        //此代码作为提醒，单一模块最大工具数量不得超过500，且模块间工具起始间隔应为200
        //建议每一类工具间隔20
        //如UiTool1: offset = 0; 则建议UiTool2: offset = 20/40/60/………/180
        public const int SingleModuleToolMaxCount = 200;

        public const int UiToolStartIndex = 1100;
        public const int UiToolSpriteAtlasOffset = 0;
        public const int UiToolOptAidOffset = 180;

        public const int SceneToolStartIndex = 1300;
        public const int EffectToolStartIndex = 1500;
        public const int ActorToolStartIndex = 1700;
        public const int AnimToolStartIndex = 1900;

        public const int ResToolStartIndex = 2100;
        public const int ResToolMaterialOffset = 0;
        public const int ResToolAssetBundleOffset = 20;
        public const int ResToolLanHuOffset = 40;
        public const int ResToolAllResRelationOffset = 180;

        public const int OtherToolStartIndex = 2300;
        public const int OtherToolBatterUseUnityOffset = 0;

        public const int DebugToolStartIndex = 2500;
        public const int DebugToolCreateObjIndex = 0;

        public const string ResourcesRootPath = "Assets/Resources";

        #region GetAssetByFilter
        public static string[] GetAssetsAtPathByFilter(string rootPath, string filter)
        {
            return AssetDatabase.FindAssets(filter, new string[] { rootPath });
        }

        public static string[] GetAssetsAtPathByFilterAndRemoveRepeat(string rootPath, string filter)
        {
            string[] assetsPath = GetAssetsAtPathByFilter(rootPath, filter);
            return RemoveRepeatGuid(assetsPath);
        }

        public static string[] RemoveRepeatGuid(string[] strAry)
        {
            int curIndex = 0;
            int strCount = strAry.Length;
            for (int i = 0; i < strCount; i++)
            {
                while (true)
                {
                    if (i + 1 >= strAry.Length)
                        break;
                    if (strAry[i + 1] != strAry[curIndex])
                    {
                        curIndex++;
                        strAry[curIndex] = strAry[i + 1];
                        break;
                    }

                    i++;
                }
            }

            curIndex++;
            if (strCount == curIndex)
                return strAry;
            string[] newStrAry = new string[curIndex];
            for (int i = 0; i < curIndex; i++)
            {
                newStrAry[i] = strAry[i];
            }

            return newStrAry;
        }
        #endregion

        #region MatAbout
        private class MatTexData : IEquatable<MatTexData>
        {
            public string TexName;
            public Object Tex;
            public Vector2 TexOffset;
            public Vector2 TexScale;

            public bool Equals(MatTexData other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return TexName == other.TexName && Tex == other.Tex && TexOffset == other.TexOffset &&
                       TexScale == other.TexScale;
            }
        }
        private class MatFloatData : IEquatable<MatFloatData>
        {
            public string FloatName;
            public float Value;

            public bool Equals(MatFloatData other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return FloatName == other.FloatName && Value == other.Value;
            }
        }
        private class MatColorData : IEquatable<MatColorData>
        {
            public string ColorName;
            public Color Color;

            public bool Equals(MatColorData other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return ColorName == other.ColorName && Color == other.Color;
            }
        }
        public static bool IsSameMaterial(Material mat1, Material mat2, bool ignoreMainTex = true)
        {
            bool isMat1Null = mat1 == null;
            bool isMat2Null = mat2 == null;
            if (isMat1Null && isMat2Null)
                return true;
            if (isMat1Null || isMat2Null)
                return false;
            if (mat1 == mat2)
                return true;
            if (mat1.renderQueue != mat2.renderQueue)
                return false;
            if (mat1.shader != mat2.shader)
                return false;

            SerializedObject so1 = new SerializedObject(mat1);
            SerializedProperty sp1 = so1.GetIterator();
            SerializedObject so2 = new SerializedObject(mat2);
            SerializedProperty sp2 = so2.GetIterator();
            while (sp1.Next(true))
            {
                if (sp1.name == "m_SavedProperties")
                    break;
            }
            while (sp2.Next(true))
            {
                if (sp2.name == "m_SavedProperties")
                    break;
            }

            //判断图片信息是否一致
            SerializedProperty texEnvsProp1 = sp1.FindPropertyRelative("m_TexEnvs");
            SerializedProperty texEnvsProp2 = sp2.FindPropertyRelative("m_TexEnvs");
            int texArrayCount1 = texEnvsProp1.arraySize;
            int texArrayCount2 = texEnvsProp2.arraySize;
            if (texArrayCount1 != texArrayCount2)
                return false;
            if (texArrayCount1 > 0)
            {
                List<MatTexData> mat1TexDatalist = new List<MatTexData>(texArrayCount1);
                List<MatTexData> mat2TexDatalist = new List<MatTexData>(texArrayCount1);
                for (int i = 0; i < texArrayCount1; ++i)
                {
                    SerializedProperty dataProperty1 = texEnvsProp1.GetArrayElementAtIndex(i);
                    SerializedProperty firstProp1 = dataProperty1.FindPropertyRelative("first");
                    SerializedProperty secondProp1 = dataProperty1.FindPropertyRelative("second");
                    SerializedProperty texProp1 = secondProp1.FindPropertyRelative("m_Texture");
                    SerializedProperty scaleProp1 = secondProp1.FindPropertyRelative("m_Scale");
                    SerializedProperty offsetProp1 = secondProp1.FindPropertyRelative("m_Offset");
                    if (!ignoreMainTex || firstProp1.stringValue != "_MainTex")
                    {
                        MatTexData data1 = new MatTexData() { TexName = firstProp1.stringValue, Tex = texProp1.objectReferenceValue, TexOffset = offsetProp1.vector2Value, TexScale = scaleProp1.vector2Value };
                        mat1TexDatalist.Add(data1);
                    }
                    SerializedProperty dataProperty2 = texEnvsProp2.GetArrayElementAtIndex(i);
                    SerializedProperty firstProp2 = dataProperty2.FindPropertyRelative("first");
                    SerializedProperty secondProp2 = dataProperty2.FindPropertyRelative("second");
                    SerializedProperty texProp2 = secondProp2.FindPropertyRelative("m_Texture");
                    SerializedProperty scaleProp2 = secondProp2.FindPropertyRelative("m_Scale");
                    SerializedProperty offsetProp2 = secondProp2.FindPropertyRelative("m_Offset");
                    if (!ignoreMainTex || firstProp2.stringValue != "_MainTex")
                    {
                        MatTexData data2 = new MatTexData() { TexName = firstProp2.stringValue, Tex = texProp2.objectReferenceValue, TexOffset = offsetProp2.vector2Value, TexScale = scaleProp2.vector2Value };
                        mat2TexDatalist.Add(data2);
                    }
                }
                if (AreTwoListsEqual<MatTexData>(mat1TexDatalist, mat2TexDatalist) == false)
                    return false;
            }
            //判断Float信息是否一致
            SerializedProperty floatProp1 = sp1.FindPropertyRelative("m_Floats");
            SerializedProperty floatProp2 = sp2.FindPropertyRelative("m_Floats");
            int floatArrayCount1 = floatProp1.arraySize;
            int floatArrayCount2 = floatProp2.arraySize;
            if (floatArrayCount1 != floatArrayCount2)
                return false;
            if (floatArrayCount1 > 0)
            {
                List<MatFloatData> mat1FloatDatalist = new List<MatFloatData>(floatArrayCount1);
                List<MatFloatData> mat2FloatDatalist = new List<MatFloatData>(floatArrayCount1);
                for (int i = 0; i < floatArrayCount1; ++i)
                {
                    SerializedProperty dataProperty1 = floatProp1.GetArrayElementAtIndex(i);
                    SerializedProperty firstProp1 = dataProperty1.FindPropertyRelative("first");
                    SerializedProperty secondProp1 = dataProperty1.FindPropertyRelative("second");
                    MatFloatData data1 = new MatFloatData { FloatName = firstProp1.name, Value = secondProp1.floatValue };
                    mat1FloatDatalist.Add(data1);

                    SerializedProperty dataProperty2 = floatProp2.GetArrayElementAtIndex(i);
                    SerializedProperty firstProp2 = dataProperty2.FindPropertyRelative("first");
                    SerializedProperty secondProp2 = dataProperty2.FindPropertyRelative("second");
                    MatFloatData data2 = new MatFloatData { FloatName = firstProp2.name, Value = secondProp2.floatValue };
                    mat2FloatDatalist.Add(data2);
                }
                if (AreTwoListsEqual<MatFloatData>(mat1FloatDatalist, mat2FloatDatalist) == false)
                    return false;
            }
            //判断Color信息是否一致
            SerializedProperty colorProp1 = sp1.FindPropertyRelative("m_Colors");
            SerializedProperty colorProp2 = sp2.FindPropertyRelative("m_Colors");
            int colorArrayCount1 = colorProp1.arraySize;
            int colorArrayCount2 = colorProp2.arraySize;
            if (colorArrayCount1 != colorArrayCount2)
                return false;
            if (colorArrayCount1 > 0)
            {
                List<MatColorData> mat1ColorDatalist = new List<MatColorData>(colorArrayCount1);
                List<MatColorData> mat2ColorDatalist = new List<MatColorData>(colorArrayCount1);
                for (int i = 0; i < colorArrayCount1; ++i)
                {
                    SerializedProperty dataProperty1 = colorProp1.GetArrayElementAtIndex(i);
                    SerializedProperty firstProp1 = dataProperty1.FindPropertyRelative("first");
                    SerializedProperty secondProp1 = dataProperty1.FindPropertyRelative("second");
                    MatColorData data1 = new MatColorData { ColorName = firstProp1.name, Color = secondProp1.colorValue };
                    mat1ColorDatalist.Add(data1);

                    SerializedProperty dataProperty2 = colorProp2.GetArrayElementAtIndex(i);
                    SerializedProperty firstProp2 = dataProperty2.FindPropertyRelative("first");
                    SerializedProperty secondProp2 = dataProperty2.FindPropertyRelative("second");
                    MatColorData data2 = new MatColorData { ColorName = firstProp2.name, Color = secondProp2.colorValue };
                    mat2ColorDatalist.Add(data2);
                }
                if (AreTwoListsEqual<MatColorData>(mat1ColorDatalist, mat2ColorDatalist) == false)
                    return false;
            }
            return true;
        }

        public static void RemoveUnusedMaterialProperty(Material mat)
        {
            if (mat == null)
                return;
            Shader shader = mat.shader;
            if (shader == null)
                return;

            ShaderPropertyData shaderData = CollectShaderNormalProperty(shader);
            SerializedObject so = new SerializedObject(mat);
            SerializedProperty sp = so.GetIterator();
            while (sp.Next(true))
            {
                if (sp.name == "m_SavedProperties")
                    break;
            }
            //SerializedProperty texEnvsProp = sp.FindPropertyRelative("m_TexEnvs");
            //int texArrayCount = texEnvsProp.arraySize;
            //SerializedProperty floatProp = sp.FindPropertyRelative("m_Floats");
            //int floatArrayCount = floatProp.arraySize;
            //SerializedProperty colorProp = sp.FindPropertyRelative("m_Colors");
            //int colorArrayCount = colorProp.arraySize;
            //SerializedProperty intProp = sp.FindPropertyRelative("m_Ints");
            //int intArrayCount = intProp.arraySize;
            SerializedProperty texEnvsProp = sp.FindPropertyRelative("m_TexEnvs");
            int texArrayCount = 0;
            if (texEnvsProp != null)
                texArrayCount = texEnvsProp.arraySize;
            SerializedProperty floatProp = sp.FindPropertyRelative("m_Floats");
            int floatArrayCount = 0;
            if (floatProp != null)
                floatArrayCount = floatProp.arraySize;
            SerializedProperty colorProp = sp.FindPropertyRelative("m_Colors");
            int colorArrayCount = 0;
            if (colorProp != null)
                colorArrayCount = colorProp.arraySize;
            SerializedProperty intProp = sp.FindPropertyRelative("m_Ints");
            int intArrayCount = 0;
            if (intProp != null)
                intArrayCount = intProp.arraySize;
            bool haveChanged = false;
            for (int i = texArrayCount - 1; i >= 0; i--)
            {
                SerializedProperty dataProperty = texEnvsProp.GetArrayElementAtIndex(i);
                SerializedProperty firstProp = dataProperty.FindPropertyRelative("first");
                string texName = firstProp.stringValue;
                if (shaderData.TexPropertyList.Contains(texName))
                    continue;
                haveChanged = true;
                texEnvsProp.DeleteArrayElementAtIndex(i);
            }
            for (int i = floatArrayCount - 1; i >= 0; i--)
            {
                SerializedProperty dataProperty = floatProp.GetArrayElementAtIndex(i);
                SerializedProperty firstProp = dataProperty.FindPropertyRelative("first");
                string floatName = firstProp.stringValue;
                if (shaderData.FloatPropertyList.Contains(floatName))
                    continue;
                haveChanged = true;
                floatProp.DeleteArrayElementAtIndex(i);
            }
            for (int i = colorArrayCount - 1; i >= 0; i--)
            {
                SerializedProperty dataProperty = colorProp.GetArrayElementAtIndex(i);
                SerializedProperty firstProp = dataProperty.FindPropertyRelative("first");
                string colorName = firstProp.stringValue;
                if (shaderData.ColorPropertyList.Contains(colorName))
                    continue;
                haveChanged = true;
                colorProp.DeleteArrayElementAtIndex(i);
            }
            for (int i = intArrayCount - 1; i >= 0; i--)
            {
                SerializedProperty dataProperty = intProp.GetArrayElementAtIndex(i);
                SerializedProperty firstProp = dataProperty.FindPropertyRelative("first");
                string intName = firstProp.stringValue;
                if (shaderData.IntPropertyList.Contains(intName))
                    continue;
                haveChanged = true;
                intProp.DeleteArrayElementAtIndex(i);
            }
            if (haveChanged)
                so.ApplyModifiedProperties();
        }

        private class ShaderPropertyData
        {
            public List<string> TexPropertyList;
            public List<string> FloatPropertyList;
            public List<string> ColorPropertyList;
            public List<string> IntPropertyList;

            public ShaderPropertyData(int texCount, int floatCount, int colorCount, int intCount)
            {
                TexPropertyList = new List<string>(texCount);
                FloatPropertyList = new List<string>(floatCount);
                ColorPropertyList = new List<string>(colorCount);
                IntPropertyList = new List<string>(intCount);
            }
        }
        private static ShaderPropertyData CollectShaderNormalProperty(Shader shader)
        {
            Material mat = new Material(shader);
            SerializedObject so = new SerializedObject(mat);
            SerializedProperty sp = so.GetIterator();
            while (sp.Next(true))
            {
                if (sp.name == "m_SavedProperties")
                    break;
            }
            SerializedProperty texEnvsProp = sp.FindPropertyRelative("m_TexEnvs");
            int texArrayCount = 0;
            if (texEnvsProp != null)
                texArrayCount = texEnvsProp.arraySize;
            SerializedProperty floatProp = sp.FindPropertyRelative("m_Floats");
            int floatArrayCount = 0;
            if (floatProp != null)
                floatArrayCount = floatProp.arraySize;
            SerializedProperty colorProp = sp.FindPropertyRelative("m_Colors");
            int colorArrayCount = 0;
            if (colorProp != null)
                colorArrayCount = colorProp.arraySize;
            SerializedProperty intProp = sp.FindPropertyRelative("m_Ints");
            int intArrayCount = 0;
            if (intProp != null)
                intArrayCount = intProp.arraySize;
            ShaderPropertyData shaderData = new ShaderPropertyData(texArrayCount, floatArrayCount, colorArrayCount, intArrayCount);
            for (int i = 0; i < texArrayCount; ++i)
            {
                SerializedProperty dataProperty = texEnvsProp.GetArrayElementAtIndex(i);
                SerializedProperty firstProp = dataProperty.FindPropertyRelative("first");
                shaderData.TexPropertyList.Add(firstProp.stringValue);
            }
            for (int i = 0; i < floatArrayCount; ++i)
            {
                SerializedProperty dataProperty = floatProp.GetArrayElementAtIndex(i);
                SerializedProperty firstProp = dataProperty.FindPropertyRelative("first");
                shaderData.FloatPropertyList.Add(firstProp.stringValue);
            }
            for (int i = 0; i < colorArrayCount; ++i)
            {
                SerializedProperty dataProperty = colorProp.GetArrayElementAtIndex(i);
                SerializedProperty firstProp = dataProperty.FindPropertyRelative("first");
                shaderData.ColorPropertyList.Add(firstProp.stringValue);
            }
            for (int i = 0; i < intArrayCount; ++i)
            {
                SerializedProperty dataProperty = intProp.GetArrayElementAtIndex(i);
                SerializedProperty firstProp = dataProperty.FindPropertyRelative("first");
                shaderData.IntPropertyList.Add(firstProp.stringValue);
            }
            return shaderData;
        }
        #endregion

        #region Common
        private static bool AreTwoListsEqual<T>(List<T> list1, List<T> list2) where T : IEquatable<T>
        {
            bool list1Null = list1 == null;
            bool list2Null = list2 == null;
            if (list1Null && list2Null)
                return true;
            if (list1Null || list2Null)
                return false;

            int list1Count = list1.Count;
            int list2Count = list2.Count;
            if (list1Count != list2Count)
                return false;

            List<int> matchIndex = new List<int>();
            for (int i = 0; i < list1Count; i++)
            {
                for (int j = 0; j < list2Count; j++)
                {
                    if (!matchIndex.Contains(j) && list1[i].Equals(list2[j]))
                    {
                        matchIndex.Add(j);
                        break;
                    }
                }
                if (matchIndex.Count <= i)
                    return false;
            }
            return true;
        }

        public static string GetFileMd5ByPath(string filePath)
        {
            string md5str = "";
            if (File.Exists(filePath))
            {
                FileStream fs = File.OpenRead(filePath);
                MD5 md5Hash = MD5.Create();
                byte[] md5Data = md5Hash.ComputeHash(fs);
                for (int i = 0; i < md5Data.Length; i++)
                {
                    string sub = md5Data[i].ToString("x2");
                    md5str += sub;
                    Debug.Log(sub);
                }
                fs.Close();
            }
            return md5str;
        }
        #endregion
    }
}
#endif