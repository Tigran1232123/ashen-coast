fixed4 OrbSurface(sampler2D orbTex,float2 uv,float4 rect,float clock)
{
    fixed4 base=tex2D(orbTex,uv);float2 p=(uv-rect.xy)/rect.zw-.5;
    float radius=length(p);float angle=atan2(p.y,p.x);
    float turn=clock*.72+sin(radius*16-clock*1.7)*.17;
    float2 rotated=float2(cos(angle+turn),sin(angle+turn))*radius;
    rotated=(floor(rotated*48)+.5)/48;
    fixed3 detail=tex2D(orbTex,rect.xy+(rotated+.5)*rect.zw).rgb;
    float wisp=pow(.5+.5*sin(angle*3+radius*25-clock*3.2),4);
    fixed3 glow=lerp(float3(.04,.17,.46),float3(.36,.83,1),wisp);
    float mask=smoothstep(.43,.34,radius)*step(.5,base.a);
    base.rgb=lerp(base.rgb,detail*.55+glow*.64,mask);
    base.rgb=floor(base.rgb*31+.5)/31;return base;
}
