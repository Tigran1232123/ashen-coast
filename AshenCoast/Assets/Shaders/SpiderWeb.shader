Shader "AshenCoast/SpiderWeb"
{
 Properties { [PerRendererData] _MainTex("Sprite",2D)="white"{} }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 #include "CoastLighting.cginc"
 struct appdata{float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
 struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;fixed4 color:COLOR;};
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;o.world=mul(unity_ObjectToWorld,v.vertex).xy;return o;}
 fixed4 frag(v2f i):SV_Target
 {
  // Ground silk follows its circular collision radius; finer cells keep large webs detailed.
  float2 p=(floor(i.uv*192)+.5)/192-.5;
  float r=length(p),a=atan2(p.y,p.x);
  float spokes=abs(sin(a*6+.10*sin(r*23)));
  float rings=abs(sin((r+.009*sin(a*7))*76));
  float thread=max(1-step(.16,spokes),1-step(.23,rings));
  float knots=(1-step(.29,spokes))*(1-step(.32,rings));
  float fade=saturate((.5-r)*36),fibre=.88+.12*sin(p.x*123+p.y*97);
  float silk=max(thread,knots)*fade*fibre;
  float3 lit=float3(.89,.94,.96)*max(CoastLight(i.world),float3(.78,.78,.82));
  return fixed4(lit,silk*i.color.a);
 }
 ENDCG }
 }
}
