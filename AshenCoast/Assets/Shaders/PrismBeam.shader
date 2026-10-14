Shader "AshenCoast/PrismBeam"
{
 Properties {_MainTex("Beam",2D)="white"{} _Charge("Charge",Float)=0 _BeamTime("Time",Float)=0 _Length("Length",Float)=64 _RayFlow("Separate ray flow",Float)=0 _RayPhase("Ray flow phase",Float)=0 _Fused("Focused filaments",Float)=0}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 #include "PrismSurface.cginc"
 struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
 struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;fixed4 color:COLOR;};
 float _Charge,_BeamTime,_Length,_RayFlow,_RayPhase,_Fused;
 v2f vert(appdata i){v2f o;o.vertex=UnityObjectToClipPos(i.vertex);o.world=mul(unity_ObjectToWorld,i.vertex).xy;o.uv=i.uv;o.color=i.color;return o;}
 fixed4 frag(v2f i):SV_Target
 {
  float signedAcross=(i.uv.y-.5)*2;float across=abs(signedAcross);float u=floor(i.uv.x*_Length*40)/40;
  float grain=frac(sin(dot(floor(i.world*40),float2(127.1,311.7)))*43758.54);
  clip(1-across-(grain-.5)*.10);float core=pow(saturate(1-across),.38);
  float stream=pow(max(0,sin(u*8-lerp(_BeamTime*38,_RayPhase,_RayFlow))),9);
  float3 color=lerp(PrismTint(u*.015-_BeamTime*.1)*i.color.rgb,float3(.99,.99,.98),core*.89);
  color=lerp(color,float3(1,1,1),stream*.16*core);color=floor(color*31+.5)/31;
  if(_Fused>.5)
  {
   float filaments=0;
   for(int strand=0;strand<5;strand++)
   {
    float centre=(strand-2)*.30+sin(u*(1.2+strand*.16)-_BeamTime*(5.3+strand*1.7)+strand)*.065;
    float filamentBand=pow(saturate(1-abs(signedAcross-centre)/.115),1.5);
    float travel=.40+.60*pow(max(0,sin(u*(3.5+strand*.33)-_BeamTime*(19+strand*3))),4);
    filaments=max(filaments,filamentBand*travel);
   }
   float spine=pow(saturate(1-across/.20),.6);
   float packet=pow(max(0,sin(u*4.3-_BeamTime*24)),10);
   color=lerp(float3(.30,.47,.64),PrismTint(u*.042-_BeamTime*.11),core*.75);
   color=lerp(color,float3(.98,.99,1),max(spine*.83,filaments*.96));
   color+=packet*core*.09;
   color+=step(.975,grain)*(.06+.08*stream);
   color=floor(saturate(color)*31+.5)/31;
  }
  return fixed4(color,(.43+.54*core)*i.color.a);
 }
 ENDCG
 }}
}
