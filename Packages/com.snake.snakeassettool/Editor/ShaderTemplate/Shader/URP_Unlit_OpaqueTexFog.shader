Shader "LaoWang/Unlit/OpaqueTexFog"
{
    Properties
    {
        _BaseMap("BaseMap" , 2D) = "white"{}
        _Color("Color",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry"}

        LOD 100

        pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 texcoord     : TEXCOORD;
                float3 normalOS     : NORMAL;
            };
            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD;
                float3 normalWS     : TEXCOORD1;
                float3 viewWS       : TEXCOORD2;
                float  fogCoord     : TEXCOORD3;
            };
            
            //属性定义部分
            //定义纹理采样贴图和采样状态
            TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);

            //CBuffer部分，数据参数定义在该结构内，可以使用srp的batch功能
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _Color;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;

                half3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewWS = GetWorldSpaceViewDir(positionWS);

                OUT.uv = TRANSFORM_TEX(IN.texcoord , _BaseMap);

                //通过ComputeFogFactor方法，使用裁剪空间的Z方向深度得到雾的坐标
                OUT.fogCoord = ComputeFogFactor(OUT.positionHCS.z);

                return OUT;
            }

            half4 frag(Varyings IN):SV_TARGET
            {
                half4 FinalColor;
                Light light = GetMainLight();
                half3 lightColor = light.color * light.distanceAttenuation;
                half3 lightDir = light.direction;

                half3 ambientColor = unity_AmbientSky.rgb;

                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap , IN.uv);

                FinalColor = baseMap * _Color;
                //混合雾效
                FinalColor.rgb = MixFog(FinalColor.rgb , IN.fogCoord);

                return FinalColor;
            }
            ENDHLSL  
        }
    }
}
