Shader "Custom/NeonGridFlat"
{
    // ============================================
    // 평면용 네온 그리드 셰이더
    // 용도: NoGoZone 등 플레이어 접근 불가 구역 표시
    // 특징: UV 기반 그리드, Fresnel 없음, 투명 배경
    // ============================================
    
    Properties
    {
        [Header(Grid Appearance)]
        [HDR] _GridColor ("Grid Color", Color) = (0, 1, 1, 1)
        _GridDensity ("Grid Density", Range(1, 50)) = 10
        _LineThickness ("Line Thickness", Range(0.01, 0.2)) = 0.05
        _Intensity ("Intensity", Range(0.1, 5)) = 1
        
        [Header(Animation)]
        _ScrollSpeedX ("Scroll Speed X", Range(-10, 10)) = 0
        _ScrollSpeedY ("Scroll Speed Y", Range(-10, 10)) = 1
        
        [Header(Rendering)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Name "NeonGrid"
            
            // 투명 + Additive 블렌딩 (네온 발광 효과)
            Blend One One
            ZWrite Off
            Cull [_Cull]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            // ============================================
            // Shader Properties (CBUFFER로 SRP Batcher 호환)
            // ============================================
            CBUFFER_START(UnityPerMaterial)
                float4 _GridColor;
                float _GridDensity;
                float _LineThickness;
                float _Intensity;
                float _ScrollSpeedX;
                float _ScrollSpeedY;
            CBUFFER_END
            
            // ============================================
            // 구조체 정의
            // ============================================
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            // ============================================
            // Vertex Shader
            // ============================================
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            
            // ============================================
            // Fragment Shader (그리드 패턴 생성)
            // ============================================
            half4 frag(Varyings input) : SV_Target
            {
                // UV에 시간 기반 스크롤 적용
                float2 uv = input.uv;
                uv.x += _Time.y * _ScrollSpeedX * 0.1;
                uv.y += _Time.y * _ScrollSpeedY * 0.1;
                
                // UV를 그리드 밀도만큼 타일링
                float2 gridUV = uv * _GridDensity;
                
                // frac: 0~1 반복 패턴 생성
                float2 fractUV = frac(gridUV);
                
                // 그리드 선 계산 (가로선 + 세로선)
                float lineX = step(fractUV.x, _LineThickness) + step(1.0 - _LineThickness, fractUV.x);
                float lineY = step(fractUV.y, _LineThickness) + step(1.0 - _LineThickness, fractUV.y);
                
                // 가로선 OR 세로선 = 그리드
                float grid = saturate(lineX + lineY);
                
                // 최종 색상: 그리드 패턴 * 색상 * 강도
                half4 finalColor = half4(_GridColor.rgb * grid * _Intensity, grid);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Unlit"
}
