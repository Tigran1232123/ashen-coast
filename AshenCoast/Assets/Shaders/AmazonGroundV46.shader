Shader "AshenCoast/AmazonGroundV46"
{
 Properties{_GroundTex("Ground",2D)="white"{} _Centre("Centre",Vector)=(0,0,0,0)}
 SubShader{Tags{"Queue"="Transparent-200" "RenderType"="Transparent"}Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off
 Pass{CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 #include "CoastLighting.cginc"
 sampler2D _GroundTex;float4 _Centre;
 struct v2f{float4 pos:SV_POSITION;float2 p:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.p=mul(unity_ObjectToWorld,v.vertex).xy;return o;}
 fixed4 frag(v2f i):SV_Target
 {float2 p=floor(i.p*40)/40;float d=length((p-_Centre.xy)/float2(10.5,9));float n=frac(sin(dot(floor(p*2),float2(12.98,78.23)))*43758.5);float alpha=1-smoothstep(.75,1.02,d+n*.07);float3 earth=tex2D(_GroundTex,float2(0,.5)+(.003+frac(p*.16)*.49)).rgb*float3(.59,.52,.40);return fixed4(earth*CoastLight(p),alpha);}
 ENDCG}
 }
}
