Shader "LaoWang/Unlit/TransparentTexture"
{
    Properties
    {
        _BaseTex("BaseTex" , 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags{"RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent"}

        LOD 100

        pass
        {
            ZWrite off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
            TEXTURE2D(_BaseTex);SAMPLER(sampler_BaseTex);

            //CBuffer部分，数据参数定义在该结构内，可以使用srp的batch功能
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseTex_ST;
                float4 _Color;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv , _BaseTex);
                return OUT;
            }

            half4 frag(Varyings IN):SV_TARGET
            {
                half4 FinalColor;
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseTex,sampler_BaseTex , IN.uv);
                FinalColor = baseMap * _Color;
                return FinalColor;
            }
            ENDHLSL  
        }
    }
}
