#ifndef GETLIGHT_INCLUDED
#define GETLIGHT_INCLUDED
 
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightDefinition.cs.hlsl"
 
void GetLightDirection_float(out float3 lightDir, out float3 lightColor)
{
#if SHADERGRAPH_PREVIEW
    lightDir = float3(0.707, 0.707, 0);
    lightColor = 1;
#else
    if (_DirectionalLightCount > 0)
    {
        DirectionalLightData light = _DirectionalLightDatas[0];
        lightDir = light.forward.xyz;
        lightColor = light.color;
    }
    else
    {
        lightDir = float3(1, 0, 0);
        lightColor = 0;
    }
#endif
}
 
#endif