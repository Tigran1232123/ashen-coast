Shader "AshenCoast/SpectralWorm"
{
 Properties { [PerRendererData] _MainTex("Sprite",2D)="white"{} }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 #include "CoastLighting.cginc"
 sampler2D _MainTex;float4 _MainTex_TexelSize,_SpriteRect;float _ArtStep,_InkTime;
 struct appdata{float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
 struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;fixed4 color:COLOR;};
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=mul(unity_ObjectToWorld,v.vertex).xy;o.color=v.color;return o;}
 fixed4 frag(v2f i):SV_Target
 {
  float2 uv=_SpriteRect.xy+(floor((i.uv-_SpriteRect.xy)*_MainTex_TexelSize.zw/max(1,_ArtStep))+.5)*max(1,_ArtStep)*_MainTex_TexelSize.xy;uv=clamp(uv,_SpriteRect.xy+_MainTex_TexelSize.xy*.5,_SpriteRect.xy+_SpriteRect.zw-_MainTex_TexelSize.xy*.5);
  fixed4 c=tex2D(_MainTex,uv);clip(c.a-.36);float2 p=(floor((uv-_SpriteRect.xy)/_SpriteRect.zw*float2(48,24))+.5)/float2(48,24);
  float t=_InkTime;float ink=sin(p.x*21+sin(p.y*17+t*.77)*2.7-t*.86)+sin(p.y*31+sin(p.x*9-t*.54)*3.4+t*1.37)+sin(p.x*39+p.y*23-t*1.9);
  float cloud=smoothstep(-.35,1.7,ink)*saturate(1-abs(p.y-.5)*1.75);c.rgb=lerp(c.rgb,float3(.035,.016,.068),cloud*.68);c.rgb=floor(c.rgb*31+.5)/31*max(CoastLight(i.world),float3(.75,.68,.90));c.rgb+=float3(.035,.02,.055)*(1-cloud);c.a*=.72+.08*sin(ink);return c*i.color;
 }
 ENDCG }
 }
}
