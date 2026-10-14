Shader "AshenCoast/BlastRing"
{
 Properties{_Color("Color",Color)=(1,1,1,1) _Radius("Radius",Float)=0 _BurstFade("Fade",Float)=1}
 SubShader{Tags{"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass{CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float _Radius,_BurstFade;fixed4 _Color;
 struct appdata{float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
 struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;fixed4 color:COLOR;};
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=mul(unity_ObjectToWorld,v.vertex).xy;o.color=v.color*_Color;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 fixed4 frag(v2f i):SV_Target
 {
  float2 p=(floor(i.uv*float2(240,148))+.5)/float2(240,148);float grain=hash(floor(i.world*40));
  float d=abs(length((p-.5)*2)-_Radius+(grain-.5)*.016);
  float width=.012+.027*_BurstFade;float ring=1-smoothstep(width*.45,width,d);clip(ring-.12);
  float3 color=lerp(float3(1,.36,.08),float3(1,.92,.56),1-saturate(d/width));
  return fixed4(color*i.color.rgb,ring*_BurstFade*.75*i.color.a);
 }
 ENDCG}
 }
}
