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

            // C#���� ���޵Ǵ� CubeProperties ����ü
            cbuffer CubePropertiesBuffer : register(b0)
            {
                float4 baseColor;          // �⺻ ����
                float brightnessFactor;    // ��� ���
                float positionOffset;      // X�� ��ġ ������
                float rotationAngle;       // ȸ�� ����
                float random;              // ���� ��
            };

            float _ScatterRange; // ��Ѹ��� ����

            struct Attributes
            {
                float4 vertex : POSITION;   // ��ġ
                float3 normal : NORMAL;     // ����
                float2 uv : TEXCOORD0;      // �ؽ��� ��ǥ
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;   // ��ȯ�� ��ġ
                float3 normalWS : TEXCOORD0;   // ���� ���� ����
                float3 positionWS : TEXCOORD1; // ���� ���� ��ġ
                float2 uv : TEXCOORD2;         // �ؽ��� ��ǥ
                float randomValue : TEXCOORD3; // ���� ��
            };

            // �ؽ� �Լ�: ������Ʈ�� ���� ���� ���� ����
            float Hash(float3 seed)
            {
                return frac(sin(dot(seed, float3(12.9898, 78.233, 45.164))) * 43758.5453);
            }

            // X�� ȸ���� ���� �Լ�
            float4 rotateX(float4 vertex, float angle)
            {
                float rad = radians(angle);
                float s = sin(rad);
                float c = cos(rad);
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                float2 rotatedYZ = mul(rotationMatrix, vertex.yz);
                return float4(vertex.x, rotatedYZ.x, rotatedYZ.y, 1.0);
            }

            // ���� ��ġ�� ����ϴ� �Լ�
            float3 calculateRandomScatter(float3 originalPosition, float3 randomSeed, float scatterRange)
            {
                // ���� ���� ������� ��ġ ������ ���
                float3 randomOffset = float3(
                    Hash(randomSeed + float3(1.0, 0.0, 0.0)),
                    Hash(randomSeed + float3(0.0, 1.0, 0.0)),
                    Hash(randomSeed + float3(0.0, 0.0, 1.0))
                );

                // -1.0 ~ 1.0 ������ ��ȯ
                randomOffset = randomOffset * 2.0 - 1.0;

                // ��Ѹ��� ������ ���Ͽ� ��ġ ����
                return originalPosition + randomOffset * scatterRange;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                // ���� �������� ��ȯ
                float4 worldPosition = mul(GetObjectToWorldMatrix(), input.vertex);
                float3 worldNormal = TransformObjectToWorldNormal(input.normal);

                // ���� �� ���� (unity_ObjectToWorld ����� ù ��° �� ���)
                float3 seed = unity_ObjectToWorld._m00_m01_m02; // ����� ù ��° ��
                float randomValue = Hash(seed);

                // ���� ��ġ ���
                float3 scatteredPosition = calculateRandomScatter(worldPosition.xyz, seed, _ScatterRange);
                worldPosition.xyz = scatteredPosition;

                // ���� ȸ�� ����
                worldPosition = rotateX(worldPosition, rotationAngle * randomValue);

                // �� �� Ŭ�� �������� ��ȯ
                output.vertex = TransformWorldToHClip(worldPosition);
                output.normalWS = worldNormal;
                output.positionWS = worldPosition.xyz;
                output.uv = input.uv;
                output.randomValue = randomValue;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // �⺻ ���� ��� ����� ���� ���� ����
                half4 color = baseColor * brightnessFactor * input.randomValue;

                // ������ ���� (��ְ� ���Ɽ �̿�)
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half NdotL = saturate(dot(input.normalWS, -lightDir));
                half shadow = mainLight.shadowAttenuation;
                half3 lighting = NdotL * shadow * mainLight.color;

                // ���� ���� = �⺻ ���� * ����
                half4 finalColor = half4(color.rgb * lighting, color.a);

                return finalColor;
            }

            ENDHLSL
        }
    }
}
