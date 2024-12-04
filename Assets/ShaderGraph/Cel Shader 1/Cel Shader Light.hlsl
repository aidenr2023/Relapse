void ToonShading_float(
    in float3 normal,
    in float toon_ramp_smoothness,
    in float3 clip_space_pos,
    in float3 world_pos,
    in float4 toon_ramp_tinting,
    in float toon_ramp_offset,

    out float3 toon_ramp_output,
    out float3 direction
)
{
    // set the shader graph node previews
    #ifdef SHADERGRAPH_PREVIEW
        toon_ramp_output = float3(0.5,0.5,0);
        direction = float3(0.5,0.5,0);

    #else
        // grab the shadow coordinates
        #if SHADOWS_SCREEN
            half4 shadowCoord = ComputeScreenPos(ClipSpacePos);

        #else
            half4 shadowCoord = TransformWorldToShadowCoord(world_pos);

        #endif

        // grab the main light
        #if _MAIN_LIGHT_SHADOWS_CASCADE || _MAIN_LIGHT_SHADOWS
            Light light = GetMainLight(shadowCoord);

        #else
            Light light = GetMainLight();

        #endif

        // dot product for toonramp
        half d = dot(normal, light.direction) * 0.5 + 0.5;

        // toonramp in a smoothstep
        half toonRamp = smoothstep(toon_ramp_offset, toon_ramp_offset + toon_ramp_smoothness, d);

        // multiply with shadows;
        toonRamp *= light.shadowAttenuation;

        // add in lights and extra tinting
        toon_ramp_output = light.color * (toonRamp + toon_ramp_tinting);

        // output direction for rimlight
        direction = light.direction;

    #endif
}
