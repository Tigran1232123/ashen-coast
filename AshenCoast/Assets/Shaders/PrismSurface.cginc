float3 PrismTint(float phase)
{
 float p=frac(phase)*5;
 float3 a=p<1?float3(.77,.83,1):p<2?float3(.93,.80,1):p<3?float3(.77,1,.88):p<4?float3(.87,1,.72):float3(.72,.96,1);
 float3 b=p<1?float3(.93,.80,1):p<2?float3(.77,1,.88):p<3?float3(.87,1,.72):p<4?float3(.72,.96,1):float3(.77,.83,1);
 return lerp(a,b,smoothstep(0,1,frac(p)));
}
fixed4 PrismSurface(sampler2D source,float2 uv,float4 rect,float clock)
{
 fixed4 c=tex2D(source,uv);float2 p=(uv-rect.xy)/max(rect.zw,float2(.0001,.0001));
 float metal=smoothstep(.17,.48,max(c.r,max(c.g,c.b)));
 float3 sheen=PrismTint(p.y*.64+p.x*.30-clock*.14);
 float flash=pow(max(0,sin((p.x*.61+p.y*.92-clock*.17)*18)),18)*.16;
 float luminance=dot(c.rgb,float3(.25,.48,.27));
 float3 opal=lerp(c.rgb,luminance*sheen*1.13,.62);
 c.rgb=lerp(c.rgb,saturate(opal+flash),metal*.85);
 c.rgb=floor(c.rgb*31+.5)/31;return c;
}
