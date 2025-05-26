Shader "Custom/ConstantBufferCube"
{
    Properties
    {
        _ScatterRange("Scatter Range", Float) = 5.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // C#에서 전달되는 CubeProperties 구조체
            cbuffer CubePropertiesBuffer : register(b0)
            {
                float4 baseColor;          // 기본 색상
                float brightnessFactor;    // 밝기 계수
                float positionOffset;      // X축 위치 오프셋
                float rotationAngle;       // 회전 각도
                float random;              // 랜덤 값
            };

            float _ScatterRange; // 흩뿌리기 범위

            struct Attributes
            {
                float4 vertex : POSITION;   // 위치
                float3 normal : NORMAL;     // 법선
                float2 uv : TEXCOORD0;      // 텍스쳐 좌표
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;   // 변환된 위치
                float3 normalWS : TEXCOORD0;   // 월드 공간 법선
                float3 positionWS : TEXCOORD1; // 월드 공간 위치
                float2 uv : TEXCOORD2;         // 텍스쳐 좌표
                float randomValue : TEXCOORD3; // 랜덤 값
            };

            // 해싱 함수: 오브젝트별 고유 랜덤 값을 생성
            float Hash(float3 seed)
            {
                return frac(sin(dot(seed, float3(12.9898, 78.233, 45.164))) * 43758.5453);
            }

            // X축 회전을 위한 함수
            float4 rotateX(float4 vertex, float angle)
            {
                float rad = radians(angle);
                float s = sin(rad);
                float c = cos(rad);
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                float2 rotatedYZ = mul(rotationMatrix, vertex.yz);
                return float4(vertex.x, rotatedYZ.x, rotatedYZ.y, 1.0);
            }

            // 랜덤 위치를 계산하는 함수
            float3 calculateRandomScatter(float3 originalPosition, float3 randomSeed, float scatterRange)
            {
                // 랜덤 값을 기반으로 위치 오프셋 계산
                float3 randomOffset = float3(
                    Hash(randomSeed + float3(1.0, 0.0, 0.0)),
                    Hash(randomSeed + float3(0.0, 1.0, 0.0)),
                    Hash(randomSeed + float3(0.0, 0.0, 1.0))
                );

                // -1.0 ~ 1.0 범위로 변환
                randomOffset = randomOffset * 2.0 - 1.0;

                // 흩뿌리기 범위를 곱하여 위치 조정
                return originalPosition + randomOffset * scatterRange;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                // 월드 공간으로 변환
                float4 worldPosition = mul(GetObjectToWorldMatrix(), input.vertex);
                float3 worldNormal = TransformObjectToWorldNormal(input.normal);

                // 랜덤 값 생성 (unity_ObjectToWorld 행렬의 첫 번째 행 사용)
                float3 seed = unity_ObjectToWorld._m00_m01_m02; // 행렬의 첫 번째 행
                float randomValue = Hash(seed);

                // 랜덤 위치 계산
                float3 scatteredPosition = calculateRandomScatter(worldPosition.xyz, seed, _ScatterRange);
                worldPosition.xyz = scatteredPosition;

                // 랜덤 회전 적용
                worldPosition = rotateX(worldPosition, rotationAngle * randomValue);

                // 뷰 및 클립 공간으로 변환
                output.vertex = TransformWorldToHClip(worldPosition);
                output.normalWS = worldNormal;
                output.positionWS = worldPosition.xyz;
                output.uv = input.uv;
                output.randomValue = randomValue;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 기본 색상에 밝기 계수와 랜덤 값을 적용
                half4 color = baseColor * brightnessFactor * input.randomValue;

                // 간단한 조명 (노멀과 방향광 이용)
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half NdotL = saturate(dot(input.normalWS, -lightDir));
                half shadow = mainLight.shadowAttenuation;
                half3 lighting = NdotL * shadow * mainLight.color;

                // 최종 색상 = 기본 색상 * 조명
                half4 finalColor = half4(color.rgb * lighting, color.a);

                return finalColor;
            }

            ENDHLSL
        }
    }
}
