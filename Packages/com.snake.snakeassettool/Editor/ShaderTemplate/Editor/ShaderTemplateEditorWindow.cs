using System.IO;
using UnityEditor;
using UnityEngine;

public class ShaderTemplateEditorWindow : MonoBehaviour
{
    private const string RootDirPath = "Packages/com.gebilaowxj_outlook.snakeassettool/Editor/ShaderTemplate/Shader/";

    [MenuItem("Assets/Create/URP_Shader/Unlit_OpaqueColor", false, 81)]
    static void CreateUnlitOpaqueColor()
    {
        string fileName = "URP_Unlit_OpaqueColor.shader";
        string shaderPath = RootDirPath + "URP_Unlit_OpaqueColor.shader";
        CreateShaderByPath(shaderPath, fileName);
    }

    [MenuItem("Assets/Create/URP_Shader/Unlit_OpaqueTex", false, 82)]
    static void CreateUnlitOpaqueTex()
    {
        string fileName = "URP_Unlit_OpaqueTex.shader";
        string shaderPath = RootDirPath + "URP_Unlit_OpaqueTex.shader";
        CreateShaderByPath(shaderPath, fileName);
    }

    [MenuItem("Assets/Create/URP_Shader/Unlit_TransparentColor", false, 81)]
    static void CreateUnlitTransparentColor()
    {
        string fileName = "URP_Unlit_TransparentColor.shader";
        string shaderPath = RootDirPath + "URP_Unlit_TransparentColor.shader";
        CreateShaderByPath(shaderPath, fileName);
    }

    [MenuItem("Assets/Create/URP_Shader/Unlit_TransparentTex", false, 82)]
    static void CreateUnlitTransparentTex()
    {
        string fileName = "URP_Unlit_TransparentTex.shader";
        string shaderPath = RootDirPath + "URP_Unlit_TransparentTex.shader";
        CreateShaderByPath(shaderPath, fileName);
    }

    private static void CreateShaderByPath(string shaderPath, string fileName)
    {
        string selectPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        //判断所选路径是否为空，在unity中，如果选中了Favorites，此时是没有路径的
        if (selectPath == "")
        {
            selectPath = "Assets/";
        }
        else if (Path.GetExtension(selectPath) != "")
        {
            selectPath = selectPath.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(selectPath + "/" + fileName);
        AssetDatabase.CopyAsset(shaderPath, assetPathAndName);
        AssetDatabase.Refresh(); 
    }
}
