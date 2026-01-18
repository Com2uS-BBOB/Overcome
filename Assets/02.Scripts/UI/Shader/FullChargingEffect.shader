Shader "Custom/FullChargingEffect"
{
    Properties
    {
        _BaseMap("Base Map (Glow Circle)", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)

        [Header(Glow Settings)]
        _GlowColor("Glow Color", Color) = (0.3, 0.6, 1, 1)
        _GlowIntensity("Glow Intensity", Range(0, 5)) = 2

        [Header(Distortion Settings)]
        _DistortionStrength("Distortion Strength", Range(0, 0.2)) = 0.05
        _DistortionSpeed("Distortion Speed", Range(0, 5)) = 0.5
        _DistortionScale("Distortion Scale", Range(1, 50)) = 5
        _DistortionDirection("Distortion Direction", Vector) = (0, 1, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4  color : COLOR;
            };

            struct TexPos
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4  color : COLOR;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4  _BaseColor;
                half4  _GlowColor;
                float4 _BaseMap_ST;
                float  _GlowIntensity;
                float  _DistortionStrength;
                float  _DistortionSpeed;
                float  _DistortionScale;
                float4 _DistortionDirection;
            CBUFFER_END

            // Simple noise function for UV distortion
            float noise(float2 uv)
            {
                return frac(sin(dot(uv, float2(1.0, 100.0))) * 43758.5453);
            }

            // Smooth noise
            float smoothNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);

                float a = noise(i);
                float b = noise(i + float2(1.0, 0.0));
                float c = noise(i + float2(0.0, 1.0));
                float d = noise(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            // Fractal noise for more organic distortion
            float fractalNoise(float2 uv)
            {
                float value = 0.0;
                float amplitude = 0.5;

                for(int i = 0; i < 3; i++)
                {
                    value += amplitude * smoothNoise(uv);
                    uv *= 2.0;
                    amplitude *= 0.5;
                }

                return value;
            }

            TexPos vert(Attributes IN)
            {
                TexPos OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(TexPos IN) : SV_Target
            {
                // 시간 값 가져오기 (애니메이션용)
                float time = _Time.y;

                // 왜곡 방향 벡터 정규화
                float2 flowDirection = normalize(_DistortionDirection.xy);

                // 첫 번째 왜곡 UV 계산 (시간에 따라 이동)
                float2 distortionUV = IN.uv * _DistortionScale;
                distortionUV += flowDirection * time * _DistortionSpeed;

                // 두 번째 왜곡 UV 계산 (첫 번째와 다른 스케일로 레이어링)
                float2 distortionUV2 = IN.uv * _DistortionScale * 1.5;
                distortionUV2 += flowDirection * time * _DistortionSpeed;

                // 프랙탈 노이즈로 왜곡 패턴 생성
                float noise1 = fractalNoise(distortionUV);
                float noise2 = fractalNoise(distortionUV2 + float2(0.0, 1.3));

                // 노이즈를 -1 ~ 1 범위로 변환하여 왜곡 벡터 생성
                float2 distortion = float2(noise1, noise2) * 2.0 - 1.0;

                // 흐름 방향에 수직인 벡터 계산 (좌우 흔들림용)
                float2 perpendicular = float2(-flowDirection.y, flowDirection.x);

                // 위쪽으로 갈수록 distortion이 강해짐 (번지는 효과)
                float heightFactor = pow(IN.uv.y, 5) * 10; // UV의 Y값이 클수록 (위로 갈수록) 증가
                float distortionMultiplier = lerp(3.0, 10.0, heightFactor); // 아래쪽 3배 → 위쪽 10배 왜곡

                // UV에 왜곡 적용
                float2 distortedUV = IN.uv;
                distortedUV += perpendicular * distortion.x * _DistortionStrength * distortionMultiplier; // 좌우 흔들림
                distortedUV += flowDirection * distortion.y * _DistortionStrength * 0.5 * distortionMultiplier; // 흐름 방향 늘어남

                // 왜곡된 UV로 베이스 텍스처 샘플링
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, distortedUV);

                // 글로우 색상 계산 - 베이스 텍스처의 알파값에 따라 적용
                half4 glowColor = _GlowColor * _GlowIntensity * baseColor.a;

                // 베이스 색상과 글로우 효과 합성
                half4 finalColor = baseColor * _BaseColor + glowColor;

                // 최종 알파값 계산 (텍스처, 베이스 색상, 버텍스 컬러의 알파를 모두 곱함)
                finalColor.a = baseColor.a * _BaseColor.a * IN.color.a;

                // 버텍스 컬러의 RGB 적용
                finalColor.rgb *= IN.color.rgb;

                return finalColor;
            }
            ENDHLSL
        }
    }
}