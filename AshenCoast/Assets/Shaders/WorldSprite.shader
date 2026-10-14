Shader "AshenCoast/WorldSprite"
{
 Properties { [PerRendererData] _MainTex ("Sprite", 2D) = "white" {} _Color ("Color", Color) = (1,1,1,1) _Emission ("Emission",Range(0,1))=0 _AlphaCut ("Alpha cut",Range(0,1))=0 _OcclusionAlpha("Occlusion alpha",Range(0,1))=1 _GroundCoin("Ground coin",Float)=0 }
 SubShader
 {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
  Cull Off Lighting Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   #include "CoastLighting.cginc"
   #include "OrbSurface.cginc"
   #include "PrismSurface.cginc"
   #include "SpectrumSurface.cginc"
   sampler2D _MainTex; fixed4 _Color;float4 _MainTex_TexelSize,_SpriteRect;float _ClothTone;float _ArtStep,_BodyFloor,_WindStrength,_WindHeight,_WindPhase,_StormWind,_WindTime,_Emission,_AlphaCut,_GroundClip,_GroundY,_Hover,_InteractPulse,_Orb,_OrbClock,_OcclusionAlpha,_GroundCoin,_Prism,_PrismClock,_Spectrum;
   struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
   float _SeatedLegs,_SpectrumGround,_SpectrumClock,_SpectrumPlanted;float4 _SpectrumRopeRect;
   float _CorpseDissolve,_RaisedArm;float4 _RaisedArmRect;
   float LegDistance(float2 p,float2 a,float2 b)
   {float2 v=b-a;return length(p-a-v*saturate(dot(p-a,v)/dot(v,v)));}
   struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float2 world:TEXCOORD1; fixed4 color:COLOR; };
   v2f vert(appdata v)
   {
    v2f o;
    if(_WindStrength>0)
    {
     float height=saturate(v.vertex.y/max(.01,_WindHeight));
     float worldX=unity_ObjectToWorld._m03;
     float gust=.65+.35*sin(_WindTime*.63-worldX*.19);
     float sway=sin(_WindTime*1.35+_WindPhase*.27+worldX*.12)*gust;
     sway+=.22*sin(_WindTime*2.1+_WindPhase+height*1.4);
     v.vertex.x+=height*height*_WindStrength*(1+_StormWind*1.4)*sway;
    }
    o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=mul(unity_ObjectToWorld,v.vertex).xy;o.color=v.color*_Color;return o;
   }
   fixed4 frag(v2f i):SV_Target
   {
    float2 uv=i.uv;
    if(_RaisedArm>.5){float2 local=(uv-_SpriteRect.xy)/_SpriteRect.zw;float mask=step(_RaisedArmRect.x,local.x)*step(local.x,_RaisedArmRect.x+_RaisedArmRect.z)*step(_RaisedArmRect.y,local.y)*step(local.y,_RaisedArmRect.y+_RaisedArmRect.w);clip(.5-mask);}
    if(_GroundCoin>0 && _SpriteRect.z>0)
    {
     float2 coin=(uv-_SpriteRect.xy)/_SpriteRect.zw-.5;
     clip(.245-dot(coin,coin));
    }
    if(_ArtStep>1 && _SpriteRect.z>0)
    {
     uv=_SpriteRect.xy+(floor((uv-_SpriteRect.xy)*_MainTex_TexelSize.zw/_ArtStep)+.5)*_ArtStep*_MainTex_TexelSize.xy;
     uv=clamp(uv,_SpriteRect.xy+_MainTex_TexelSize.xy*.5,_SpriteRect.xy+_SpriteRect.zw-_MainTex_TexelSize.xy*.5);
    }
    if(_SpectrumGround>0 && _SpriteRect.z>0)
    {
     float2 rope=(uv-_SpriteRect.xy)/_SpriteRect.zw;
     float weight=smoothstep(_SpectrumRopeRect.x,_SpectrumRopeRect.x+.05,rope.x)*smoothstep(_SpectrumRopeRect.y,_SpectrumRopeRect.y+.03,rope.y);
     float freeEnd=saturate((_SpectrumRopeRect.w-rope.y)/.30);
     float sway=(sin(_SpectrumClock*2.0+rope.y*5)*.090+sin(_SpectrumClock*3.7+rope.y*11)*.028)*freeEnd*weight;
     uv.x+=sway*_SpriteRect.z;
     uv.y+=sin(_SpectrumClock*2.0+rope.x*7)*.017*freeEnd*weight*_SpriteRect.w;
    }
    fixed4 c=(_Prism>0?PrismSurface(_MainTex,uv,_SpriteRect,_PrismClock):_Orb>0?OrbSurface(_MainTex,uv,_SpriteRect,_OrbClock):tex2D(_MainTex,uv))*i.color;
    if(_CorpseDissolve>0)
    {
     float2 p=(uv-_SpriteRect.xy)/_SpriteRect.zw;
     float grain=frac(sin(dot(floor(p*float2(70,45)),float2(127.1,311.7)))*43758.54);
     float remain=p.y*.67+grain*.33;
     clip(remain-_CorpseDissolve);
     c.rgb=lerp(c.rgb,float3(.70,.63,.51),saturate(1-(remain-_CorpseDissolve)*13)*.25);
    }
    if(_SeatedLegs>.01)
    {
     float2 p=(uv-_SpriteRect.xy)/_SpriteRect.zw;
     p=(floor(p*float2(40,20))+.5)/float2(40,20);
     float leftLeg=min(LegDistance(p,float2(.43,.82),float2(.12,.30)),LegDistance(p,float2(.12,.30),float2(.62,.14)));
     float rightLeg=min(LegDistance(p,float2(.57,.82),float2(.88,.30)),LegDistance(p,float2(.88,.30),float2(.38,.14)));
     float d=min(leftLeg,rightLeg),trouser=step(d,.095);
     float boots=step(min(LegDistance(p,float2(.34,.11),float2(.18,.18)),LegDistance(p,float2(.66,.11),float2(.82,.18))),.080);
     float seam=step(.056,d)*trouser;
     fixed4 folded=fixed4(lerp(float3(.16,.19,.18),float3(.08,.10,.10),seam),trouser);
     folded.rgb+=step(d,.055)*float3(.035,.043,.032);
     folded.rgb=lerp(folded.rgb,float3(.18,.10,.055)+p.y*.035,boots);folded.a=max(folded.a,boots);
     c=lerp(c,folded*i.color,smoothstep(.10,.70,_SeatedLegs));
    }
    if(_BodyFloor>0 && _SpriteRect.w>0)
    {
     float height=(uv.y-_SpriteRect.y)/_SpriteRect.w;
     // Retain coat hems and hands beside the pelvis. A horizontal crop of the
     // entire character used to amputate these along with the obsolete legs.
     float coat=step(_BodyFloor-.06,height)*step(c.g*1.28,c.r)*step(.24,c.r);
     float hand=step(_BodyFloor-.10,height)*step(.42,c.r)*step(.25,c.g)*step(c.g*1.18,c.r);
     clip(max(height-_BodyFloor,(_BodyFloor>.3?max(coat,hand):0)-.5));
    }
    if(_Spectrum>0 && _SpriteRect.z>0)
    {
     float2 s=(uv-_SpriteRect.xy)/_SpriteRect.zw;
     c=SpectrumSurface(c,s,_SpectrumGround>.5?_SpectrumClock:_Time.y,_SpectrumPlanted);
    }
    if(_ClothTone>0 && _ClothTone<1)c.rgb*=lerp(1,_ClothTone,smoothstep(.99,1.20,c.b/max(.001,c.r)));
    if(_Hover>0)
    {
     float2 stepUV=_MainTex_TexelSize.xy*max(1,_ArtStep);
     float near=max(max(tex2D(_MainTex,uv+float2(stepUV.x,0)).a,tex2D(_MainTex,uv-float2(stepUV.x,0)).a),max(tex2D(_MainTex,uv+float2(0,stepUV.y)).a,tex2D(_MainTex,uv-float2(0,stepUV.y)).a));
     c.rgb=lerp(c.rgb,float3(1,.91,.57),.30);if(c.a<.1 && near>.6)c=fixed4(1,.89,.48,.78);
    }
    clip(c.a-_AlphaCut);if(_GroundClip>0)clip(i.world.y-_GroundY);
    c.a*=_OcclusionAlpha;c.rgb=floor(c.rgb*31+.5)/31;c.rgb*=lerp(CoastLight(i.world),float3(1,1,1),saturate(_Emission));
    if(_InteractPulse>0){float strength=_InteractPulse*_InteractPulse;c.rgb+=float3(.36,.22,.06)*strength*c.a;}
    return c;
   }
   ENDCG
  }
 }
}
