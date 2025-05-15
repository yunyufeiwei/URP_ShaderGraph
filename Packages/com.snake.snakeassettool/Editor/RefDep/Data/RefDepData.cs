public class RefDepData
{
    // 因Urp资产是扩展，暂不好在其它扩展中处理
    // 因Timeline资产是扩展，暂不好在其它扩展中处理
    public enum RefDepAssetType
    {
        Unknow = 0,
        // 以下资产大多数情况下被别人引用
        Tex = 10,
        CubeMap = 12,
        Shader = 20,
        AnimClip = 30,
        Font = 40,
        AudioClip = 50,
        PhysicsMaterial = 60,
        // 暂不区分StaticMesh和SkeletalMesh
        Mesh = 70,
        LightSetting = 80,
        LightData = 90,
        AvatarMask = 100,
        AnimCtrl = 110,
        UrpRenderData = 120,

        // 以下资产通常即会引用他人也会被他人引用
        Material = 1010,
        Prefab = 1020,
        Script = 1030,
        FoldAsset = 1040,

        // 以下资产通常引用他人，不会被其它资产引用
        SpriteAtlas = 2010,
        ShaderVariant = 2020,
        UrpPipeline = 2030,
        UrpGlobalSetting = 2040,

        // 以下资产被认为是主资产，被其引用的资产才会被视为有效资产
        PrimaryAsset = 3000,
        Scene = 3010,
        Timeline = 3020,
    }
}
