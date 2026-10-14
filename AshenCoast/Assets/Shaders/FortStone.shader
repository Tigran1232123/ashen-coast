Shader "AshenCoast/FortStone"
{
 Properties { _MainTex("Worn stone",2D)="white"{} }
 SubShader { Tags {"Queue"="Background"} Cull Off ZWrite Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 #include "CoastLighting.cginc"
 sampler2D _MainTex;
 struct v2f{float4 pos:SV_POSITION;float2 world:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xy;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 fixed4 frag(v2f i):SV_Target
 {
  float2 p=floor(i.world*40)/40;
  float3 c=tex2D(_MainTex,frac(p*.12)).rgb;
  float damp=hash(floor(p*.18)),dust=hash(floor(p*17));
  c*=.70+damp*.15;c+=(dust-.5)*.02;
  return fixed4(floor(c*31+.5)/31*CoastLight(p),1);
 }
 ENDCG }
 }
}
