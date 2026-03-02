#ifndef ADDITIONAL_LIGHT_INCLUDED
#define ADDITIONAL_LIGHT_INCLUDED

void AdditionalLights_float(float3 WorldPos, int lightID,
    out float3 Direction, out float3 Color, out float Attenuation)
{
    Direction = normalize(float3(1.0f, 1.0f, 0.0f));
    Color = 1.0f;
    Attenuation = 1.0f;
    
#ifndef SHADERGRAPH_PREVIEW
    int lightCount = GetAdditionalLightsCount();
    if(lightID < lightCount)
    {
        Light additionalLight = GetAdditionalLight(lightID, WorlPos);
        Direction = additionalLight.direction;
        Color = additionalLight.color;
        Attenuation = additionalLight.distanceAttenuation;
    }
#endif
}
#endif