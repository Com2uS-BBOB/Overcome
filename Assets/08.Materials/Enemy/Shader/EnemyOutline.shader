Shader "CustomRenderTexture/EnemyOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineWidth ("Outline Width", Float) = 0.02
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }

        Pass
        {
            Name "Outline"
            Cull Front          // 인버티드 헐 핵심
            ZWrite Off
            ZTest LEqual        // 가려짐은 기본적으로 반영 (필요시 Always로 바꿔 '벽 넘어 표시'도 가능)
            Offset 50, 50
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 스킨드 메쉬용 키워드 (URP에서 스키닝 데이터가 들어오게)
            #pragma multi_compile _ _SKINNED_MESH

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            float4 _OutlineColor;
            float _OutlineWidth;

            Varyings vert (Attributes v)
            {
                Varyings o;

                float3 pos = v.positionOS.xyz + v.normalOS * _OutlineWidth;
                o.positionHCS = TransformObjectToHClip(pos);

                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                return half4(_OutlineColor.rgb, _OutlineColor.a);
            }
            ENDHLSL
        }
    }
}