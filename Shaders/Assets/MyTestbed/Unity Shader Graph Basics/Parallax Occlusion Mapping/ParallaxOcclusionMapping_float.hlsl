void ParallaxOcclusionMapping_float(
    UnityTexture2D HeightMap,
    UnitySamplerState Sampler,
    float2 UV,
    float3 ViewDir,
    float Amplitude,
    float Steps,
    out float2 ParallaxUV)
{
    float layerDepth = 1.0 / Steps;
    float currentDepth = 0.0;

    float2 deltaUV = (ViewDir.xy / max(ViewDir.z, 0.0001)) * Amplitude / Steps;

    float2 currentUV = UV;
    float heightValue = SAMPLE_TEXTURE2D(HeightMap.tex, Sampler.samplerstate, currentUV).r;

    [loop]
    for (int i = 0; i < Steps; i++)
    {
        if (currentDepth >= heightValue)
            break;

        currentUV -= deltaUV;
        heightValue = SAMPLE_TEXTURE2D(HeightMap.tex, Sampler.samplerstate, currentUV).r;
        currentDepth += layerDepth;
    }

    float2 prevUV = currentUV + deltaUV;
    float afterDepth = heightValue - currentDepth;
    float beforeDepth = SAMPLE_TEXTURE2D(HeightMap.tex, Sampler.samplerstate, prevUV).r - currentDepth + layerDepth;
    float weight = afterDepth / (afterDepth - beforeDepth);

    ParallaxUV = lerp(currentUV, prevUV, weight);
}