Shader "AshenCoast/CaveStone"
{
 Properties { _MainTex("Stone atlas",2D)="white"{} _Surface("Surface",Float)=0 }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 #include "CoastLighting.cginc"
 sampler2D _MainTex;float _Surface;
 struct appdata{float4 vertex:POSITION;float2 uv:TEXCOORD0;};
 struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;};
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=mul(unity_ObjectToWorld,v.vertex).xy;return o;}
 fixed4 frag(v2f i):SV_Target
 {
  float2 p=(floor(i.world*40)+.5)/40;float2 uv;
  if(_Surface<.5){float2 q=frac(p/4.5);uv=float2(q.x*.492+.004,.504+q.y*.492);float n=sin(p.x*.53)+sin(p.y*.72);if(n>1.25)uv=float2(.504+q.x*.492,.004+q.y*.492);}
  else if(_Surface<1.5)uv=float2(frac(p.x/3)*.492+.004,.004+saturate(i.uv.y)*.492);
  else uv=float2(.504+frac(p.x/3)*.492,.504+saturate(i.uv.y)*.492);
  fixed4 c=tex2D(_MainTex,uv);c.rgb=floor(c.rgb*31+.5)/31*CoastLight(p);if(_Surface<.5)c.rgb*=.95;else if(_Surface<1.5)c.rgb*=.64;else c.rgb*=1.12;return c;
 }
 ENDCG }
 }
}
