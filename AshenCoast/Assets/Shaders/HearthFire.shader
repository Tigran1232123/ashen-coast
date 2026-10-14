Shader "AshenCoast/HearthFire"
{
 Properties { [PerRendererData] _MainTex("Hearth atlas",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) _SkyColumn("Fire from sky",Float)=0 _SkyHeight("Sky height",Float)=14 _SkyWidth("Sky width",Float)=4.7 }
 SubShader
 {
  Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   #include "CoastLighting.cginc"
   sampler2D _MainTex;float4 _SpriteRect,_FrameA,_FrameB;float2 _ShiftA,_ShiftB,_Pixels;float _Blend,_HearthClock,_SkyColumn,_SkyHeight,_SkyWidth,_UnderVessel,_VesselCeiling;fixed4 _Color;
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;fixed4 color:COLOR;};
   v2f vert(appdata v)
   {
    v2f o;float height=saturate(v.vertex.y);v.vertex.x+=sin(_HearthClock+height*4)*.011*height*height;
    o.vertex=UnityObjectToClipPos(v.vertex);o.uv=(v.uv-_SpriteRect.xy)/_SpriteRect.zw;o.world=mul(unity_ObjectToWorld,v.vertex).xy;o.color=v.color*_Color;return o;
   }
   fixed4 sampleFrame(float2 p,float4 rect,float2 shift)
   {p+=shift;float inside=step(0,p.x)*step(p.x,1)*step(0,p.y)*step(p.y,1);return tex2D(_MainTex,rect.xy+clamp(p,.001,.999)*rect.zw)*inside;}
   fixed4 frag(v2f i):SV_Target
   {
    if(_UnderVessel>.5)clip(_VesselCeiling-i.world.y);
    float2 p=(floor(i.uv*_Pixels)+.5)/_Pixels;
    float2 flowing=p;
    if(_SkyColumn>.5)
    {
     float height=p.y*_SkyHeight;
     flowing.y=.27+frac(height*.24+_HearthClock*.30)*.40;
     flowing.x+=sin(height*2.1+_HearthClock*1.3)*.025;
     flowing.x+=sin(height*4.7+_HearthClock*2.1)*.012;
     float2 grid=float2(_Pixels.x,296);flowing=(floor(flowing*grid)+.5)/grid;
    }
    fixed4 a=sampleFrame(flowing,_FrameA,_ShiftA),b=sampleFrame(flowing,_FrameB,_ShiftB);
    if(_SkyColumn>.5)
    {
     float cycle=frac(p.y*_SkyHeight*.24+_HearthClock*.30);
     float seam=smoothstep(.24,.48,abs(cycle-.5));
     float2 alternate=float2(flowing.x,.27+frac(cycle+.5)*.40);
     // Offset strips cover each other's wrap seam, retaining a continuous torrent.
     a=lerp(a,sampleFrame(alternate,_FrameA,_ShiftA),seam);
     b=lerp(b,sampleFrame(alternate,_FrameB,_ShiftB),seam);
     float impact=1-smoothstep(.7,2.1,p.y*_SkyHeight);
     float2 ground=float2(p.x,p.y*_SkyHeight/7.4);
     fixed4 ga=sampleFrame(ground,_FrameA,_ShiftA),gb=sampleFrame(ground,_FrameB,_ShiftB);
     a=lerp(a,ga,impact);b=lerp(b,gb,impact);
    }
    float alpha=lerp(a.a,b.a,_Blend);clip(alpha-.018);
    float3 c=lerp(a.rgb*a.a,b.rgb*b.a,_Blend)/max(alpha,.001);
    if(_SkyColumn>.5)
    {
     float pattern=frac(sin(dot(floor(i.world*40),float2(127.1,311.7)))*43758.54);
     fixed4 frame=pattern<_Blend?b:a;alpha=frame.a;clip(alpha-.09);c=frame.rgb;
     float vein=pow(max(0,sin(p.x*29+p.y*_SkyHeight*1.2+_HearthClock*3.8)),12);
     c=lerp(c,float3(1,.90,.48),vein*.22);
    }
    c=min(c,float3(.98,.87,.48));float hot=smoothstep(.28,.63,c.r)*saturate((c.r-c.b)*2);
    c*=lerp(CoastLight(i.world),float3(1,.94,.85),hot*.83);c=floor(c*31+.5)/31;
    return fixed4(c*i.color.rgb,alpha*i.color.a);
   }
   ENDCG
  }
 }
}
