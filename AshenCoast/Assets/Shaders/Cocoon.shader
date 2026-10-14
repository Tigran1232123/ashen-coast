Shader "AshenCoast/Cocoon"
{
 Properties{_MainTex("White",2D)="white"{} _Clock("Clock",Float)=0 _Fade("Fade",Float)=1}
 SubShader{Tags{"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass{CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 #include "CoastLighting.cginc"
 struct app{float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;};float _Clock,_Fade;
 v2f vert(app v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=mul(unity_ObjectToWorld,v.vertex).xy;return o;}
 fixed4 frag(v2f i):SV_Target
 {
  float2 p=(floor(i.uv*float2(40,64))+.5)/float2(40,64),q=(p-.5)*2;float oval=dot(q,q);clip(.98-oval);
  float a=atan2(q.y,q.x),radius=sqrt(oval);float strands=min(abs(sin(p.y*100+p.x*43+sin(p.y*17)*1.8)),abs(sin(p.y*83-p.x*68)));
  float silk=1-smoothstep(.07,.23,strands),rim=smoothstep(.72,.98,oval);
  float pulse=.88+.12*sin(_Clock*7+a*3);float alpha=(silk*.55+rim*.30+.08)*_Fade;
  float3 color=lerp(float3(.57,.61,.65),float3(.89,.91,.89),silk)*pulse*max(float3(.32,.32,.35),CoastLight(i.world));return fixed4(color,alpha);
 }
 ENDCG}}
}
