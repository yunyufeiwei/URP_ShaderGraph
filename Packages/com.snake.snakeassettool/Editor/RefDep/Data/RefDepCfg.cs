using System;
using System.Collections.Generic;
using UnityEngine;

public class RefDepCfg
{
    public static Dictionary<Type, RefDepData.RefDepAssetType> UnityType2MyTypeDic =
        new Dictionary<Type, RefDepData.RefDepAssetType>()
        {
            { typeof(UnityEngine.Texture), RefDepData.RefDepAssetType.Tex },
            { typeof(UnityEngine.Texture2D), RefDepData.RefDepAssetType.Tex },
            { typeof(UnityEngine.Cubemap), RefDepData.RefDepAssetType.CubeMap },
            { typeof(UnityEngine.Material), RefDepData.RefDepAssetType.Material },
            { typeof(UnityEngine.Shader), RefDepData.RefDepAssetType.Shader },
            { typeof(UnityEngine.Mesh), RefDepData.RefDepAssetType.Mesh },
            { typeof(UnityEditor.MonoScript), RefDepData.RefDepAssetType.Script },
            { typeof(UnityEditor.SceneAsset), RefDepData.RefDepAssetType.Scene },
            { typeof(UnityEngine.LightingSettings), RefDepData.RefDepAssetType.LightSetting },
            { typeof(UnityEditor.LightingDataAsset), RefDepData.RefDepAssetType.LightData },
            { typeof(UnityEngine.AvatarMask), RefDepData.RefDepAssetType.AvatarMask },
            { typeof(UnityEngine.AnimationClip), RefDepData.RefDepAssetType.AnimClip },
            { typeof(UnityEditor.Animations.AnimatorController), RefDepData.RefDepAssetType.AnimCtrl },
            { typeof(UnityEngine.AudioClip), RefDepData.RefDepAssetType.AudioClip },
            { typeof(UnityEngine.Font), RefDepData.RefDepAssetType.Font },
            { typeof(UnityEngine.U2D.SpriteAtlas), RefDepData.RefDepAssetType.SpriteAtlas },
            { typeof(UnityEngine.ShaderVariantCollection), RefDepData.RefDepAssetType.ShaderVariant },
        };

    public static Dictionary<RefDepData.RefDepAssetType, string> MyType2DescDic =
        new Dictionary<RefDepData.RefDepAssetType, string>()
        {
            { RefDepData.RefDepAssetType.Unknow, "未知"},
            { RefDepData.RefDepAssetType.Tex,"纹理"},
            { RefDepData.RefDepAssetType.CubeMap,"立方体贴图"},
            { RefDepData.RefDepAssetType.Shader, "Shader"},
            { RefDepData.RefDepAssetType.AnimClip, "动画"},
            { RefDepData.RefDepAssetType.Font,"字体"},
            { RefDepData.RefDepAssetType.AudioClip,"音频"},
            { RefDepData.RefDepAssetType.PhysicsMaterial,"物理材质"},
            { RefDepData.RefDepAssetType.Mesh,"Mesh"},
            { RefDepData.RefDepAssetType.LightSetting,"灯光设置"},
            { RefDepData.RefDepAssetType.LightData,"灯光Data"},
            { RefDepData.RefDepAssetType.AvatarMask,"人形动画遮罩"},
            { RefDepData.RefDepAssetType.AnimCtrl,"动画控制器"},
            { RefDepData.RefDepAssetType.UrpRenderData,"Urp渲染器配置"},
            { RefDepData.RefDepAssetType.Material,"材质"},
            { RefDepData.RefDepAssetType.Prefab,"预制体"},
            { RefDepData.RefDepAssetType.Script,"脚本"},
            { RefDepData.RefDepAssetType.SpriteAtlas,"精灵图集"},
            { RefDepData.RefDepAssetType.ShaderVariant,"Shader变体收集器"},
            { RefDepData.RefDepAssetType.UrpPipeline,"Urp主配置"},
            { RefDepData.RefDepAssetType.UrpGlobalSetting,"Urp通用配置"},
            { RefDepData.RefDepAssetType.Scene,"场景"},
            { RefDepData.RefDepAssetType.Timeline,"Timeline"},
            { RefDepData.RefDepAssetType.FoldAsset,"多节点折叠"},
        };

    public static Dictionary<RefDepData.RefDepAssetType, string> MyType2StyleDic =
        new Dictionary<RefDepData.RefDepAssetType, string>()
        {
            { RefDepData.RefDepAssetType.Unknow, "node 0"},
            { RefDepData.RefDepAssetType.Tex,"node 1"},
            { RefDepData.RefDepAssetType.CubeMap,"node 2"},
            { RefDepData.RefDepAssetType.Shader, "node 3"},
            { RefDepData.RefDepAssetType.AnimClip, "node 4"},
            { RefDepData.RefDepAssetType.Font,"node 5"},
            { RefDepData.RefDepAssetType.AudioClip,"node 6"},
            { RefDepData.RefDepAssetType.PhysicsMaterial,"node 7"},
            { RefDepData.RefDepAssetType.Mesh,"node 8"},
            { RefDepData.RefDepAssetType.LightSetting,"node 1"},
            { RefDepData.RefDepAssetType.LightData,"node 2"},
            { RefDepData.RefDepAssetType.AvatarMask,"node 3"},
            { RefDepData.RefDepAssetType.AnimCtrl,"node 4"},
            { RefDepData.RefDepAssetType.UrpRenderData,"node 5"},
            { RefDepData.RefDepAssetType.Material,"node 6"},
            { RefDepData.RefDepAssetType.Prefab,"node 7"},
            { RefDepData.RefDepAssetType.Script,"node 8"},
            { RefDepData.RefDepAssetType.SpriteAtlas,"node 1"},
            { RefDepData.RefDepAssetType.ShaderVariant,"node 2"},
            { RefDepData.RefDepAssetType.UrpPipeline,"node 3"},
            { RefDepData.RefDepAssetType.UrpGlobalSetting,"node 4"},
            { RefDepData.RefDepAssetType.Scene,"node 5"},
            { RefDepData.RefDepAssetType.Timeline,"node 6"},
            { RefDepData.RefDepAssetType.FoldAsset,"node 7"},
        };

    public static Dictionary<RefDepData.RefDepAssetType, string> MyType2StyleOnDic =
        new Dictionary<RefDepData.RefDepAssetType, string>()
        {
            { RefDepData.RefDepAssetType.Unknow, "node 0 on"},
            { RefDepData.RefDepAssetType.Tex,"node 1 on"},
            { RefDepData.RefDepAssetType.CubeMap,"node 2 on"},
            { RefDepData.RefDepAssetType.Shader, "node 3 on"},
            { RefDepData.RefDepAssetType.AnimClip, "node 4 on"},
            { RefDepData.RefDepAssetType.Font,"node 5 on"},
            { RefDepData.RefDepAssetType.AudioClip,"node 6 on"},
            { RefDepData.RefDepAssetType.PhysicsMaterial,"node 7 on"},
            { RefDepData.RefDepAssetType.Mesh,"node 8 on"},
            { RefDepData.RefDepAssetType.LightSetting,"node 1 on"},
            { RefDepData.RefDepAssetType.LightData,"node 2 on"},
            { RefDepData.RefDepAssetType.AvatarMask,"node 3 on"},
            { RefDepData.RefDepAssetType.AnimCtrl,"node 4 on"},
            { RefDepData.RefDepAssetType.UrpRenderData,"node 5 on"},
            { RefDepData.RefDepAssetType.Material,"node 6 on"},
            { RefDepData.RefDepAssetType.Prefab,"node 7 on"},
            { RefDepData.RefDepAssetType.Script,"node 8 on"},
            { RefDepData.RefDepAssetType.SpriteAtlas,"node 1 on"},
            { RefDepData.RefDepAssetType.ShaderVariant,"node 2 on"},
            { RefDepData.RefDepAssetType.UrpPipeline,"node 3 on"},
            { RefDepData.RefDepAssetType.UrpGlobalSetting,"node 4 on"},
            { RefDepData.RefDepAssetType.Scene,"node 5 on"},
            { RefDepData.RefDepAssetType.Timeline,"node 6 on"},
            { RefDepData.RefDepAssetType.FoldAsset,"node 0 on"},
        };

    public static Dictionary<RefDepData.RefDepAssetType, Texture> MyType2IconDic =
        new Dictionary<RefDepData.RefDepAssetType, Texture>()
        {
            { RefDepData.RefDepAssetType.Unknow, null},
            { RefDepData.RefDepAssetType.CubeMap,null},
            { RefDepData.RefDepAssetType.Shader, null},
            { RefDepData.RefDepAssetType.AnimClip, null},
            { RefDepData.RefDepAssetType.Font, null},
            { RefDepData.RefDepAssetType.AudioClip, null},
            { RefDepData.RefDepAssetType.PhysicsMaterial, null},
            { RefDepData.RefDepAssetType.Mesh,null},
            { RefDepData.RefDepAssetType.LightSetting,null},
            { RefDepData.RefDepAssetType.LightData,null},
            { RefDepData.RefDepAssetType.AvatarMask,null},
            { RefDepData.RefDepAssetType.AnimCtrl,null},
            { RefDepData.RefDepAssetType.UrpRenderData,null},
            { RefDepData.RefDepAssetType.Material,null},
            { RefDepData.RefDepAssetType.Prefab,null},
            { RefDepData.RefDepAssetType.Script,null},
            { RefDepData.RefDepAssetType.SpriteAtlas,null},
            { RefDepData.RefDepAssetType.ShaderVariant,null},
            { RefDepData.RefDepAssetType.UrpPipeline,null},
            { RefDepData.RefDepAssetType.UrpGlobalSetting,null},
            { RefDepData.RefDepAssetType.Scene,null},
            { RefDepData.RefDepAssetType.Timeline,null},
            { RefDepData.RefDepAssetType.FoldAsset,null},
        };
}
