Shader "AshenCoast/Axolotl"
{
 Properties { _MainTex("Monster atlas",2D)="white"{} }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma target 3.0
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 #include "CoastLighting.cginc"
 sampler2D _MainTex;float4 _SpriteRect;float _GillTime,_TailTime,_ArtStep;float4 _MainTex_TexelSize;float4 _TailPivot,_PawPivot,_PawMotion;float4 _RunPivot0,_RunPivot1,_RunPivot2,_RunPivot3,_RunMotion0,_RunMotion1,_RunMotion2,_RunMotion3;
 struct appdata{float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;float4 legs:TEXCOORD1;};
 struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;};
 float2 LegPose(float2 position,float4 pivot,float4 motion){float2 p=position-pivot.xy;return pivot.xy+float2(p.x*cos(motion.z)-p.y*sin(motion.z),p.x*sin(motion.z)+p.y*cos(motion.z))+motion.xy;}
 v2f vert(appdata v)
 {
  v2f o;float2 t=v.uv;
  // Registered independent bones keep the face and all other paws rigid.
  float angle=(sin(_TailTime*1.25)*.18+sin(_TailTime*2.05+.6)*.025)*v.color.b;
  float2 tail=v.vertex.xy-_TailPivot.xy;
  v.vertex.xy=_TailPivot.xy+float2(tail.x*cos(angle)-tail.y*sin(angle),tail.x*sin(angle)+tail.y*cos(angle));
  float2 paw=v.vertex.xy-_PawPivot.xy;
  float2 bent=_PawPivot.xy+float2(paw.x*cos(_PawMotion.z)-paw.y*sin(_PawMotion.z),paw.x*sin(_PawMotion.z)+paw.y*cos(_PawMotion.z))+_PawMotion.xy;
  v.vertex.xy=lerp(v.vertex.xy,bent,v.color.a);
  float2 original=v.vertex.xy;
  v.vertex.xy+=(LegPose(original,_RunPivot0,_RunMotion0)-original)*v.legs.x;
  v.vertex.xy+=(LegPose(original,_RunPivot1,_RunMotion1)-original)*v.legs.y;
  v.vertex.xy+=(LegPose(original,_RunPivot2,_RunMotion2)-original)*v.legs.z;
  v.vertex.xy+=(LegPose(original,_RunPivot3,_RunMotion3)-original)*v.legs.w;
  // Only the outer gill branches have a nonzero red weight.
  float phase=v.color.g*1.6;
  v.vertex.x+=sin(_GillTime*2.1+phase)*v.color.r*.11;
  v.vertex.y+=sin(_GillTime*1.7+phase+1.1)*v.color.r*.065;
  o.pos=UnityObjectToClipPos(v.vertex);o.uv=t;o.world=mul(unity_ObjectToWorld,v.vertex).xy;return o;
 }
 fixed4 frag(v2f i):SV_Target
 {
  float2 uv=_SpriteRect.xy+i.uv*_SpriteRect.zw;
  uv=_SpriteRect.xy+(floor((uv-_SpriteRect.xy)*_MainTex_TexelSize.zw/max(1,_ArtStep))+.5)*max(1,_ArtStep)*_MainTex_TexelSize.xy;
  uv=clamp(uv,_SpriteRect.xy+_MainTex_TexelSize.xy*.5,_SpriteRect.xy+_SpriteRect.zw-_MainTex_TexelSize.xy*.5);
  fixed4 c=tex2D(_MainTex,uv);clip(c.a-.36);
  // Keep only a trace of cold reflected light on the skin, never a luminous creature.
  float2 d=_MainTex_TexelSize.xy*max(1,_ArtStep);
  float border=saturate(c.a-min(tex2D(_MainTex,uv+float2(-d.x,0)).a,tex2D(_MainTex,uv+float2(0,d.y)).a));
  float night=saturate(1-_CoastAmbient.r*2);
  float l=dot(c.rgb,float3(.299,.587,.114));c.rgb=lerp(float3(l,l,l),c.rgb,.90);
  c.rgb=floor(c.rgb*31+.5)/31*max(CoastLight(i.world),float3(.09,.10,.16));
  c.rgb+=float3(.022,.024,.042)*border*night;return c;
 }
 ENDCG }
 }
}
