Shader "UI/InverseMask"
{
    Properties
    {
        _Color ("Tint", Color) = (0,0,0,0.7)
        _HoleCenter ("Hole Center", Vector) = (0.5, 0.5, 0, 0)
        _HoleSize ("Hole Size", Vector) = (0.2, 0.1, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Name "Overlay"
            Tags
            {
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vs
            #pragma fragment fs
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct TexPos
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4  _Color;
                float4 _HoleCenter;
                float4 _HoleSize;
            CBUFFER_END

            TexPos vs(Attributes IN)
            {
                TexPos               OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = posInputs.positionCS;
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 fs(TexPos IN) : SV_Target
            {
                float2 d = abs(IN.uv - _HoleCenter.xy);
                float  dist = max(d.x - _HoleSize.x, d.y - _HoleSize.y);
                float  alphaFactor = step(0, dist);

                return _Color * half4(1, 1, 1, alphaFactor);
            }
            ENDHLSL
        }
    }
}