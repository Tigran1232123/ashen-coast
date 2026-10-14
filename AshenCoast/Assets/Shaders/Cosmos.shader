Shader "AshenCoast/Cosmos"
{
 Properties {_MainTex("Nebula",2D)="black"{} _ObjectsTex("Pixel planet",2D)="black"{} _GalaxyTex("Detailed spiral",2D)="black"{} _RingsTex("Dust and fragments",2D)="black"{} _MeteorVisibility("Meteors",Range(0,1))=1}
 SubShader
 {
  Tags {"Queue"="Background"} Cull Off ZWrite Off
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   sampler2D _MainTex,_ObjectsTex,_GalaxyTex,_RingsTex;float _CosmicTime,_MeteorVisibility;float4 _PlanetRect,_GalaxyRect;
   struct v2f{float4 pos:SV_POSITION;float2 p:TEXCOORD0;};
   v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.p=mul(unity_ObjectToWorld,v.vertex).xy;return o;}
   float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float2 rotate(float2 p,float a){return float2(p.x*cos(a)-p.y*sin(a),p.x*sin(a)+p.y*cos(a));}
   float inside(float2 uv){return step(0,uv.x)*step(uv.x,1)*step(0,uv.y)*step(uv.y,1);}
   float2 pointUV(float2 uv){return clamp((floor(uv*640)+.5)/640,.001,.999);}
   fixed4 frag(v2f i):SV_Target
   {
    float2 p=(floor(i.p*40)+.5)/40;float t=_CosmicTime;
    float2 bg=(floor((p/float2(46,31)+.5+float2(t*.00012,0))*float2(1024,720))+.5)/float2(1024,720);
    float3 nebula=tex2D(_MainTex,bg).rgb;float luma=dot(nebula,float3(.2126,.7152,.0722));
    float3 c=saturate(lerp(luma.xxx,nebula,1.24))*.57;
    float2 cell=floor(p*7);float seed=hash(cell);
    float star=step(.984,seed)*step(length(frac(p*7)-.5),.13)*(.65+.35*sin(t*(1+seed)+seed*91));c+=star*float3(.19,.24,.37);
    float2 guv=rotate((p-float2(-8.1,3.3))/float2(8.8,7.8),-t*.018)*.5+.5;
    float4 galaxy=tex2D(_GalaxyTex,pointUV(guv));galaxy.a*=inside(guv);
    float gl=dot(galaxy.rgb,float3(.2126,.7152,.0722));galaxy.rgb=saturate(lerp(gl.xxx,galaxy.rgb,1.16));
    c=lerp(c,galaxy.rgb*(.82+.012*sin(t*2.7)),galaxy.a*.95);
    float2 center=float2(9.0,6.4),rel=p-center;float radius=5.1,r=length(rel)/radius;
    float2 projected=rotate(rel,-.28-sin(t*.012)*.022);
    float2 ringUV=rotate(float2(projected.x,projected.y/.30)/(radius*2.2),-t*.045)*.5+.5;
    float4 ring=tex2D(_RingsTex,pointUV(ringUV));ring.a*=inside(ringUV)*smoothstep(1.12,1.16,length(float2(projected.x,projected.y/.30))/radius);
    float angle=atan2(projected.y/.30,projected.x);float depth=projected.y<0?1:.61;
    float planetShadow=lerp(.46,1,smoothstep(.20,.80,abs(sin(angle+.8))));ring.rgb*=depth*planetShadow*.82;
    if(r>=1 || projected.y>=0)c=lerp(c,ring.rgb,ring.a*.88);
    float2 puv=rotate(rel/radius,-t*.006)*.5+.5;
    float4 planet=tex2D(_ObjectsTex,_PlanetRect.xy+pointUV(puv)*_PlanetRect.zw);planet.a*=inside(puv);
    c=lerp(c,planet.rgb*.75,planet.a);
    if(projected.y<0)c=lerp(c,ring.rgb,ring.a*.95);
    for(int k=0;k<5;k++)
    {
     float cycle=floor(t/(3.8+k*.73)),phase=frac(t/(3.8+k*.73));float random=hash(float2(cycle,k+9)),travel=saturate(phase/.29);
     float2 head=float2(lerp(-22,18,random)+travel*19,18-travel*30+hash(float2(k,cycle))*9);head=floor(head*40)/40;
     float2 dir=normalize(float2(.54,-.84)),delta=p-head;float along=dot(delta,dir),across=abs(delta.x*dir.y-delta.y*dir.x);
     float trail=step(across,.034)*floor(saturate(1+along/3.6)*5)/5*step(along,0)*step(-3.6,along)*step(phase,.29);
     float meteor=step(length(delta),.055)*step(phase,.29);c+=(trail*float3(.25,.46,.73)+meteor*float3(.76,.93,1))*_MeteorVisibility;
    }
    return fixed4(floor(saturate(c)*63+.5)/63,1);
   }
   ENDCG
  }
 }
}
