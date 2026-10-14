Shader "AshenCoast/DoorPortal"
{
 SubShader
 {
  Tags {"Queue"="Transparent+10"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
   struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
   v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
   float _CosmicTime;
   float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   fixed4 frag(v2f i):SV_Target
   {
    float2 uv=(floor(i.uv*float2(36,64))+.5)/float2(36,64);float t=_CosmicTime;
    float x=abs(uv.x-.5)*2;float arch=uv.y<.70?1:step(x*x+pow((uv.y-.70)/.30,2),1);float mask=step(x,.90)*arch;
    float2 center=(uv-float2(.5,.52))*float2(1,1.8);float radius=length(center),angle=atan2(center.y,center.x);
    float vortex=floor((.5+.5*sin(angle*3-radius*24+t*3.5))*5)/5;
    float bands=floor((.5+.5*sin(uv.y*34-t*5+sin(uv.x*17+t*2)))*4)/4;
    float3 c=lerp(float3(.018,.065,.22),float3(.07,.44,.95),vortex*.78+bands*.22);
    float core=exp(-radius*7)*.85;c+=core*float3(.16,.52,.76);
    float edge=step(.77,x)*(.65+.35*sin(uv.y*20-t*7));c+=edge*float3(.12,.47,.72);
    float2 cells=floor(uv*float2(18,32));float motes=step(.93,hash(float2(cells.x,floor(cells.y+t*8))));c+=motes*float3(.13,.36,.45);
    return fixed4(floor(saturate(c)*31+.5)/31,mask*.96);
   }
   ENDCG
  }
 }
}
