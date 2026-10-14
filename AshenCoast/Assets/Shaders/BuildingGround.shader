Shader "AshenCoast/BuildingGround"
{
 Properties{_GroundTex("Ground",2D)="white"{} _ClearingRect("Cleared footprint",Vector)=(0,0,.5,.5) _Charred("Campfire charcoal",Float)=0}
 SubShader{Tags{"Queue"="Transparent-200" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass{CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 #include "CoastLighting.cginc"
 sampler2D _GroundTex;float4 _ClearingRect;float _Charred;
 struct v2f{float4 pos:SV_POSITION;float2 world:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xy;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 fixed4 frag(v2f i):SV_Target
 {
  float2 p=floor(i.world*40)/40,rel=p-_ClearingRect.xy;
  float2 q=rel/max(_ClearingRect.zw,float2(.1,.1));float radius=length(q);
  float grain=hash(floor(p*40)),edge=hash(floor(p*9));
  float angle=atan2(q.y,q.x);float irregular=sin(angle*7+.8)*.055+sin(angle*11)*.035+(edge-.5)*.13;
  float alpha=1-smoothstep(.72,1.30,radius+irregular);clip(alpha-.025);
  float3 earth=tex2D(_GroundTex,float2(.003,.503)+frac(p*.24)*.494).rgb*float3(.63,.55,.44);
  earth+=(grain-.5)*.025;
  float pebble=step(.982,hash(floor(p*19)))*(1-step(.4,edge));earth+=pebble*.045;
  earth*=1-_Charred*(1-smoothstep(.20,.95,radius))*.48;
  float3 light=min(CoastLight(p),float3(1.2,1.2,1.2));float luminosity=dot(light,float3(.2126,.7152,.0722));light=lerp(light,luminosity.xxx,.45);
  return fixed4(earth*light,alpha*.85);
 }
 ENDCG}
 }
}
