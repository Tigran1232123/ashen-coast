Shader "AshenCoast/Atmosphere"
{
 Properties { _MainTex ("Scene", 2D)="white" {} _FogTex ("Fog texture", 2D)="white" {} }
 SubShader
 {
  Cull Off ZWrite Off ZTest Always
  Pass
  {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex, _FogTex;
   float _Fog, _Rain, _Flash, _Clock, _Night;
   float _Meditation,_BattleHeat,_EffectClock;
   float4 _HeroView;
   float4 _PixelSize,_FogWorldView;
   int _ZenWellCount;float4 _ZenWells[8];
   float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
   fixed4 frag(v2f_img i):SV_Target
   {
    float2 uv=i.uv;
    float protectedHero=1-smoothstep(.85,1.15,length((uv-_HeroView.xy)/max(float2(.001,.001),_HeroView.zw)));
    float2 heatUV=uv;
    float2 zenWorld=_FogWorldView.xy+(uv-.5)*_FogWorldView.zw,zenWarp=0;
    for(int well=0;well<8;well++)
    {
     if(well>=_ZenWellCount)break;
     float2 q=(zenWorld-_ZenWells[well].xy)/max(.01,_ZenWells[well].z);
     float zenDistance=length(q),strength=pow(1-smoothstep(.08,1,zenDistance),2)*_ZenWells[well].w;
     float phase=zenDistance*19-_EffectClock*4.8;
     float2 radial=q/max(.08,zenDistance),tangent=float2(-radial.y,radial.x);
     zenWarp+=3*(radial*sin(phase)*1.8+tangent*sin(zenDistance*10+_EffectClock*2.9)*1.25)*strength;
    }
    // Refract sharp pixels rather than blending a blurred copy of the scene.
    heatUV+=round(clamp(zenWarp,-7.5,7.5))*_PixelSize.zw;
    float hotBand=pow(.5+.5*sin(uv.y*23+uv.x*9-_EffectClock*1.7),3);
    float2 ripple=float2(sin(uv.y*116-_EffectClock*12+sin(uv.x*24)),cos(uv.x*91+_EffectClock*9));
    heatUV+=ripple*_PixelSize.zw*(1.2+hotBand*2.2)*_BattleHeat*(1-protectedHero*.8);
    heatUV=saturate(heatUV);
    fixed3 c=tex2D(_MainTex,heatUV).rgb;
    float edge=smoothstep(.13,.59,length((uv-.5)*float2(1,.88)));
    float blur=edge*_Meditation*(1-protectedHero);
    float2 radius=_PixelSize.zw*(2+blur*7);
    fixed3 softened=c*.20;
    softened+=(tex2D(_MainTex,heatUV+float2(radius.x,0)).rgb+tex2D(_MainTex,heatUV-float2(radius.x,0)).rgb+tex2D(_MainTex,heatUV+float2(0,radius.y)).rgb+tex2D(_MainTex,heatUV-float2(0,radius.y)).rgb)*.12;
    softened+=(tex2D(_MainTex,heatUV+radius*.71).rgb+tex2D(_MainTex,heatUV-radius*.71).rgb+tex2D(_MainTex,heatUV+float2(radius.x,-radius.y)*.71).rgb+tex2D(_MainTex,heatUV+float2(-radius.x,radius.y)*.71).rgb)*.08;
    c=lerp(c,softened,saturate(blur));
    c=lerp(c,c*float3(1.065,.985,.93)+float3(.008,.003,0),_BattleHeat*hotBand*.6);
    float2 world=_FogWorldView.xy+(uv-.5)*_FogWorldView.zw;
    float cloud=tex2D(_FogTex,world*.06+float2(_Clock*.004,_Clock*.002)).r;
    float cloud2=tex2D(_FogTex,world*.13-float2(_Clock*.006,0)).r;
    float fogArea=1;
    float fogDensity=_Fog*fogArea*(.69+cloud*.24+cloud2*.12);
    c=lerp(c,lerp(float3(.65,.72,.69),float3(.26,.32,.40),_Night),saturate(fogDensity));
    float2 rainUV=uv*float2(172,39);
    rainUV.x+=rainUV.y*.58;
    float lane=floor(rainUV.x);
    float speed=19+hash(float2(lane,4))*9;
    float fall=rainUV.y+_Clock*speed;
    float rainCell=floor(fall);
    float tail=frac(fall);
    float rain=step(.46,hash(float2(lane,rainCell)))*smoothstep(.48,.69,tail)*step(frac(rainUV.x),.22);
    float2 distant=uv*float2(235,51);distant.x+=distant.y*.50;
    float fall2=distant.y+_Clock*17;
    float fine=step(.63,hash(float2(floor(distant.x),floor(fall2))))*step(.82,frac(fall2))*step(frac(distant.x),.18);
    float visibility=.65+.35*uv.y;
    c=lerp(c,float3(.46,.57,.65),rain*_Rain*.28*visibility);
    c=lerp(c,float3(.42,.51,.59),fine*_Rain*.11);
    float2 splashCell=floor(uv*float2(100,64));
    float2 splashLocal=frac(uv*float2(100,64));
    float splashShape=step(abs(splashLocal.y-.5),.11)*step(abs(splashLocal.x-.5),.3);
    float splash=step(.991,hash(splashCell+floor(_Clock*11)))*splashShape*_Rain;
    c+=splash*.043;
    c=lerp(c,float3(.66,.75,.85),_Flash*.28);
    float vignette=smoothstep(.36,.78,length((uv-.5)*float2(1,.87)));
    c*=1-vignette*.21;
    return fixed4(c,1);
   }
   ENDCG
  }
 }
}
