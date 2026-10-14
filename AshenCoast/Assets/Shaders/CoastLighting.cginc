#ifndef COAST_LIGHTING_INCLUDED
#define COAST_LIGHTING_INCLUDED
float4 _CoastAmbient;
float4 _CoastLights[32];
float4 _CoastLightColors[32];
int _CoastLightCount;
float _CoastClock;
float4 _PrismSegments[7];
float4 _PrismBeamStyle[7];
int _PrismSegmentCount;
float3 CoastLight(float2 world)
{
    float shimmer = 1 + .014 * sin(_CoastClock * 5.7) + .010 * sin(_CoastClock * 12.3);
    float3 value = _CoastAmbient.rgb * shimmer;
    for (int k = 0; k < _CoastLightCount; k++)
    {
        float falloff = saturate(1 - length(world - _CoastLights[k].xy) / _CoastLights[k].z);
        value += _CoastLightColors[k].rgb * falloff * falloff * _CoastLights[k].w;
    }
    float beamGlow=0;
    for(int ray=0;ray<_PrismSegmentCount;ray++)
    {
        float4 segment=_PrismSegments[ray];float2 delta=segment.zw-segment.xy;
        float along=saturate(dot(world-segment.xy,delta)/max(.0001,dot(delta,delta)));
        float distance=length(world-(segment.xy+delta*along));
        float falloff=saturate(1-distance/_PrismBeamStyle[ray].x);
        // Overlapping focused rays share one glow, so crossings do not wash out the floor.
        beamGlow=max(beamGlow,falloff*falloff*_PrismBeamStyle[ray].y);
    }
    return value+float3(.62,.80,1)*beamGlow;
}
#endif
