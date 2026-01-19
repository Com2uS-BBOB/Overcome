// 하단에 안개색 그라디언트를 블렌딩하는 Cubemap Skybox 쉐이더
// 용도: FogFake 끝부분과 하늘이 자연스럽게 섞이도록 함
Shader "Custom/SkyboxWithFog"
{
    Properties
    {
        [Header(Skybox Settings)]
        [NoScaleOffset] _Tex ("Cubemap (HDR)", Cube) = "grey" {}
        _Tint ("Tint Color", Color) = (0.5, 0.5, 0.5, 1)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        
        [Header(Fog Gradient Settings)]
        _FogColor ("Fog Color", Color) = (0.23, 0.23, 0.23, 1)
        _FogHeight ("Fog Height", Range(0, 1)) = 0.5
        _FogSmoothness ("Fog Smoothness", Range(0.01, 1)) = 0.3
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue" = "Background" 
            "RenderType" = "Background" 
            "PreviewType" = "Skybox" 
        }
        
        Cull Off 
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            // Cubemap 텍스처 선언
            TEXTURECUBE(_Tex);
            SAMPLER(sampler_Tex);
            
            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                half _Exposure;
                float _Rotation;
                half4 _FogColor;
                half _FogHeight;
                half _FogSmoothness;
            CBUFFER_END
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Y축 기준 회전 행렬
            float3 RotateAroundYInDegrees(float3 vertex, float degrees)
            {
                float rad = degrees * PI / 180.0;
                float sina, cosa;
                sincos(rad, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                // 회전 적용
                float3 rotated = RotateAroundYInDegrees(input.positionOS.xyz, _Rotation);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.texcoord = rotated;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 방향 벡터 정규화
                float3 dir = normalize(input.texcoord);
                
                // Cubemap 샘플링 (방향 벡터를 직접 사용)
                half4 skyColor = SAMPLE_TEXTURECUBE(_Tex, sampler_Tex, dir);
                skyColor.rgb *= _Tint.rgb * _Exposure * 2.0;
                
                // 안개 그라디언트 계산
                // dir.y: -1(아래) ~ +1(위), 0이 지평선
                // normalizedHeight: 0(아래) ~ 1(위)
                float normalizedHeight = (dir.y + 1.0) * 0.5;
                
                // _FogHeight 이하에서 안개 적용
                // smoothstep으로 부드러운 전환
                float fogFactor = 1.0 - smoothstep(
                    _FogHeight - _FogSmoothness, 
                    _FogHeight + _FogSmoothness, 
                    normalizedHeight
                );
                
                // 안개색과 스카이박스 블렌딩
                half3 finalColor = lerp(skyColor.rgb, _FogColor.rgb, fogFactor);
                
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    
    Fallback Off
}
