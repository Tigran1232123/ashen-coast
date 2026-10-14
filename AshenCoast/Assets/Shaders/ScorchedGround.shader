Shader "AshenCoast/ScorchedGround"
{
 Properties {_MainTex("Ground",2D)="white"{} _Fade("Fade",Float)=1 _Seed("Seed",Float)=0}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 #include "CoastLighting.cginc"
 float _Fade,_Seed;struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
 struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;};
 v2f vert(appdata i){v2f o;o.vertex=UnityObjectToClipPos(i.vertex);o.world=mul(unity_ObjectToWorld,i.vertex).xy;o.uv=i.uv;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7))+_Seed)*43758.54);}
 fixed4 frag(v2f i):SV_Target
 {
  float2 cell=floor(i.world*40);float2 p=(i.uv-.5)*2;float r=length(p);
  float fleck=hash(cell),edge=hash(floor(cell*.12)),rough=hash(floor(cell*.043));
  float soot=1-smoothstep(.36,1,r+(edge-.5)*.18+(rough-.5)*.12);soot*=.86+.14*fleck;clip(soot-.015);
  float crack=step(.91,hash(floor(cell*.36)))*.13;
  float3 c=lerp(float3(.035,.034,.030),float3(.135,.111,.082),fleck*.75)+crack*.45;
  if(fleck>.96)c=float3(.32,.29,.24);c=floor(c*31+.5)/31;
  return fixed4(c*CoastLight(i.world),soot*_Fade*.72);
 }
 ENDCG
 }}
}
