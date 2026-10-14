Shader "AshenCoast/JourneyFloor"
{
 Properties { _GroundTex("Ground",2D)="white"{} _MeadowTex("Golden meadow",2D)="white"{} _WoodTex("Oak and plaster",2D)="white"{} _Indoor("Indoor",Float)=0 }
 SubShader { Tags {"Queue"="Background"} Cull Off ZWrite Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 #include "CoastLighting.cginc"
 sampler2D _GroundTex,_MeadowTex,_WoodTex;float _Indoor;
 struct v2f{float4 pos:SV_POSITION;float2 world:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xy;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
 float boxDistance(float2 p,float2 size){float2 d=abs(p)-size;return length(max(d,0))+min(max(d.x,d.y),0);}
 fixed4 frag(v2f i):SV_Target
 {
 float2 p=floor(i.world*40)/40;float n=hash(floor(p*40));float3 c;
 if(_Indoor>1.5){float2 uv=float2(.502+frac(p.x*.20)*.495,frac(p.y*.7));c=tex2D(_WoodTex,uv).rgb*.75;}
 else if(_Indoor>.5){float2 uv=float2(.002+frac(p.x*.18)*.495,frac(p.y*.18));c=tex2D(_WoodTex,uv).rgb*.82;c+=(n-.5)*.009;}
 else
 {
  float3 grass=tex2D(_MeadowTex,frac(p*.19)).rgb;
  float3 fine=tex2D(_MeadowTex,frac(p.yx*.267+float2(.31,.63))).rgb;
  grass=lerp(grass,fine,.18);grass*=.80+.13*noise(p*.19);
  float patch=smoothstep(.56,.81,noise(p*.36+19));
  float3 sand=tex2D(_GroundTex,float2(0,.5)+(.003+frac(p*.15)*.494)).rgb*.82;
  c=lerp(grass,sand,patch*.36);
  float road=1-smoothstep(1.0,2.05,abs(p.x-sin(p.y*.16)*1.2));
  float foundation=1-smoothstep(-.12,.65,boxDistance(p-float2(0,6.05),float2(5.22,.48)));
  float apron=1-smoothstep(-.15,.80,boxDistance(p-float2(0,5.28),float2(4.40,.60)));
  c=lerp(c,sand,max(road*.94,max(foundation,apron*.88)));c+=(n-.5)*.016;
 }
 c=floor(c*31+.5)/31;return fixed4(c*CoastLight(p),1);
 }
 ENDCG }
 }
}
