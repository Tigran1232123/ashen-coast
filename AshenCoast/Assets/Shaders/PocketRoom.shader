Shader "AshenCoast/PocketRoom"
{
 Properties {_RoomTex("Unique weathered stone floor",2D)="white"{}}
 SubShader
 {
  Tags {"Queue"="Geometry"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   #include "CoastLighting.cginc"
   sampler2D _RoomTex;float4 _RuinBases[16];int _RuinCount;
   struct v2f{float4 pos:SV_POSITION;float2 p:TEXCOORD0;};
   v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.p=mul(unity_ObjectToWorld,v.vertex).xy;return o;}
   float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   fixed4 frag(v2f i):SV_Target
   {
    float2 p=(floor(i.p*40)+.5)/40;float chip=.025+hash(floor(p.y*6))*.065;
    bool surface=abs(p.x)<4.3-chip && p.y>-4.3 && p.y<4.3;
    bool front=p.y<=-4.3 && p.y>-4.56 && abs(p.x)<4.3-chip;
    bool side=abs(p.x)>=4.3-chip && abs(p.x)<4.4-chip && p.y>-4.3 && p.y<4.3;
    if(!surface && !front && !side)return 0;
    float2 uv=saturate((p+4.3)/8.6);float3 c=tex2D(_RoomTex,uv).rgb;c=floor(c*31+.5)/31;
    if(front){c*=.38+.09*hash(floor(p*14));if(p.y> -4.36)c*=1.45;}
    if(side)c*=.52;
    for(int k=0;k<_RuinCount;k++)
    {
     float4 b=_RuinBases[k];float2 contact=(p-b.xy)/float2(b.z,.25);
     float2 cast=(p-(b.xy+float2(.15,-.23*b.w)))/float2(b.z*.75,.15+b.w*.20);
     c*=1-.27*exp(-dot(contact,contact)*3)-.25*exp(-dot(cast,cast)*2);
    }
    return fixed4(c*CoastLight(p),1);
   }
   ENDCG
  }
 }
}
