#ifndef SPECTRUM_SURFACE_INCLUDED
#define SPECTRUM_SURFACE_INCLUDED
float4 SpectrumSurface(float4 source,float2 p,float clock,float planted)
{
    float blade=planted>.5?1-smoothstep(.56,.64,p.y):smoothstep(.32,.43,p.x);
    float metal=blade*smoothstep(.12,.25,max(source.r,max(source.g,source.b)));
    float along=planted>.5?p.y:p.x;
    float phase=along*9.0+clock*2.8+p.x*2.1;
    float3 rainbow=.24+.76*(.5+.5*cos(phase+float3(0,2.0944,4.1888)));
    rainbow=lerp(rainbow,float3(.20,.62,1),.12);
    float glint=pow(saturate(sin(along*11-clock*4.1+p.x*3)),14);
    float detail=.68+dot(source.rgb,float3(.22,.67,.11))*.58;
    source.rgb=lerp(source.rgb,source.rgb*.38+rainbow*detail*.78,metal*.76);
    source.rgb+=glint*metal*float3(.24,.34,.42);
    return source;
}
#endif
