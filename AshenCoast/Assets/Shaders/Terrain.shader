Shader "AshenCoast/Terrain"
{
 Properties { _GroundTex("Sand, shore, grass, sea atlas",2D)="white"{} }
 SubShader
 {
  Tags { "Queue"="Background" }
  Cull Off ZWrite Off
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   #include "CoastLighting.cginc"
   #include "WorldGeometryV58.cginc"
   float _Daylight, _ClearSky, _LandPhase;
   sampler2D _GroundTex;
   struct v2f { float4 pos:SV_POSITION; float2 world:TEXCOORD0; };
   v2f vert(appdata_base v) { v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.world=mul(unity_ObjectToWorld,v.vertex).xy; return o; }
   float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
   float shore(float x) { return -4.6 + sin(x*.17)*.48 + sin(x*.79)*.16; }
   float meadow(float x) { return 4.1 + sin(x*.15)*.55 + sin(x*.48)*.15; }
   float3 tile(float2 p,float2 origin){return tex2D(_GroundTex,origin+(.003+frac(p)*.494)).rgb;}
   fixed4 frag(v2f i):SV_Target
   {
    float2 p=floor(i.world*36)/36;
    float2 pixel=floor(i.world*36);
    float n=hash(pixel), patch=hash(floor(p*1.6)), grain=hash(floor(p*5));
    float shapeDistance=coastDistanceV58(p,_LandPhase);float distance=min(shapeDistance,riverBankV58(p));
    float3 sand=tile(p*.14,float2(0,.5))*.90;
    sand+=(patch-.5)*float3(.025,.023,.015)+(n-.5)*.013;
    float wet=saturate(1-distance/1.1);
    sand=lerp(sand,tile(p*.14,float2(.5,.5))*.77,wet*.70);
    float fieldEdge=meadow(p.x)+(hash(floor(p*7))-.5)*.15;
    float blend=smoothstep(fieldEdge-.6,fieldEdge+.35,p.y);
    float3 grass=tile(p*.17,float2(0,0))*.71;
    // Distant meadow stays seamless across streamed vegetation chunks.
    float continuation=smoothstep(27,48,p.y);
    float variation=sin(p.x*.035+sin(p.y*.021)*1.7)*sin(p.y*.027+p.x*.014);
    grass*=1+variation*.10*continuation;
    float pathX=8+sin((p.y-24)*.017)*27+sin((p.y-24)*.048)*9;
    float path=(1-smoothstep(.9,1.95,abs(p.x-pathX)+(grain-.5)*.45))*continuation;
    grass=lerp(grass,tile(p*.14,float2(0,.5))*.65,path*.72);
    float tufts=step(.87,n)*step(.58,hash(float2(pixel.x,floor(pixel.y/3))));
    grass+=tufts*float3(.018,.029,.010);
    sand=lerp(sand,grass,blend);
    sand=lerp(tile(p*.14,float2(0,.5))*.86,sand,smoothstep(.7,3.0,shapeDistance));
    float3 water=tile(p*float2(.13,.16)+float2(_CoastClock*.003,_CoastClock*.013),float2(.5,0))*.63;
    water*=lerp(.75,1,saturate((p.y+19)/15));
    water += (n-.5)*.024 + sin(p.y*9+_CoastClock*1.2+sin(p.x*2))*.014;
    float ripple=sin(p.y*14+sin(p.x*2.8)+_CoastClock*1.7);
    water+=step(.94,ripple)*float3(.016,.032,.028);
    // Breakers move toward land, then wash a lace of foam over the wet sand.
    float cycle=frac(_CoastClock*.19+p.x*.011+sin(p.x*.39)*.045);
    float runup=sin(cycle*6.28318-1.15)*.48-.05;
    float wetTrail=smoothstep(.78,-.45,distance)*(.62+.18*sin(_CoastClock*.65));
    sand=lerp(sand,sand*float3(.73,.83,.86),wetTrail);
    float foam=0,crestShade=0;
    for(int w=0;w<3;w++)
    {
     float phase=frac(cycle+w*.3333);
     float front=lerp(-4.4,.56,phase)+sin(p.x*.72+w*2.3+_CoastClock*.55)*.09;
     float d=distance-front;
     float broken=hash(floor(float2(p.x*19,d*31+phase*6)));
     float fade=smoothstep(0,.18,phase)*(1-smoothstep(.72,1,phase));
     float lip=1-smoothstep(.035,.145,abs(d));
     float lace=saturate(1-abs(d+.23)/.37)*step(.62,broken);
     foam=max(foam,(lip*.74+lace*.37)*fade);
     crestShade+=saturate(1-abs(d-.23)/.24)*fade;
    }
    water*=1-crestShade*.11;
    water=lerp(water,float3(.67,.77,.73),saturate(foam)*.84);
    float fringe=(1-smoothstep(.035,.16,abs(distance-runup)))*step(.22,hash(floor(float2(p.x*25,(distance-runup)*43))));
    float wash=saturate(1-abs(distance-runup+.18)/.52)*step(.72,hash(floor(float2(p.x*24,distance*27+_CoastClock*3))));
    float shimmerNoise=hash(float2(floor(p.x*9),floor((p.y+_CoastClock*.45)*15)));
    float sunPath=exp(-pow((p.x+2+sin(p.y*.3)*1.4)/6,2));
    float glint=step(.965,shimmerNoise)*step(.45,sin(_CoastClock*3+p.x*7+p.y*8))*sunPath*_Daylight*_ClearSky;
    water+=glint*float3(.75,.68,.40);
    float3 color=distance < runup ? water : sand;
    color=lerp(color,float3(.70,.80,.75),saturate(fringe*.65+wash*.29));
    return fixed4(color*CoastLight(p),1);
   }
   ENDCG
  }
 }
}
