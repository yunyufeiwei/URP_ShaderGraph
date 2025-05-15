Shader "LaoWang/Unlit/OpaqueTexToggle"
{
    Properties
    {
        _Tex1("Tex1" , 2D) = "white"{}        
        _Color("Color",Color) = (1,1,1,1)
        [Toggle]_ToggleTest("Toggle Test", float) = 0
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
            #pragma shader_feature _TOGGLETEST_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS   :   POSITION;
                float2 uv           :   TEXCOORD;
            };
            struct Varyings
            {
                float4 positionHCS  :   SV_POSITION;
                float2 uv           :   TEXCOORD;
            };
            
            //属性定义部分
            //定义纹理采样贴图和采样状态
            TEXTURE2D(_Tex1);SAMPLER(sampler_Tex1);

            //CBuffer部分，数据参数定义在该结构内，可以使用srp的batch功能
            CBUFFER_START(UnityPerMaterial)
                float4 _Tex1_ST;
                float4 _Color;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv , _Tex1);
                return OUT;
            }

            half4 frag(Varyings IN):SV_TARGET
            {
                half4 FinalColor;
                half4 baseMap = SAMPLE_TEXTURE2D(_Tex1,sampler_Tex1 , IN.uv);
#if _TOGGLETEST_ON
                FinalColor = baseMap * _Color;
#else
                FinalColor = baseMap;
#endif
                return FinalColor;
            }
            ENDHLSL  
        }
    }
}
