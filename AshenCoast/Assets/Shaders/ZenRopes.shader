Shader "AshenCoast/ZenRopes"
{
 Properties { [PerRendererData] _MainTex("Texture",2D)="white"{} _Color("Color",Color)=(1,1,1,1) }
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   #include "CoastLighting.cginc"
   sampler2D _MainTex;float4 _MainTex_TexelSize,_SpriteRect,_Color;float _RopeClock,_GroundRopes,_GroundY,_Hover,_ArtStep;
   struct appdata{float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   struct v2f{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;fixed4 color:COLOR;};
   v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=mul(unity_ObjectToWorld,v.vertex).xy;o.color=v.color*_Color;return o;}
   fixed4 frag(v2f i):SV_Target
   {
    float2 uv=i.uv,local=(uv-_SpriteRect.xy)/max(_SpriteRect.zw,.00001);
    float staff=1.08-.50*local.y;
    float ropes=smoothstep(.24,.34,local.y)*(1-smoothstep(.85,.94,local.y))*(1-smoothstep(staff-.12,staff-.035,local.x));
    float attachment=local.y>.72?.83:local.y>.54?.71:.47;float weight=pow(saturate((attachment-local.y)/.14),1.5);
    uv.x+=_GroundRopes*ropes*weight*_SpriteRect.z*(sin(_RopeClock*2.6+local.y*10)*.045+sin(_RopeClock*4.2+local.y*17)*.015);
    if(_ArtStep>1)uv=_SpriteRect.xy+(floor((uv-_SpriteRect.xy)*_MainTex_TexelSize.zw/_ArtStep)+.5)*_ArtStep*_MainTex_TexelSize.xy;
    fixed4 c=tex2D(_MainTex,uv)*i.color;clip(c.a-.04);if(_GroundRopes>.5)clip(i.world.y-_GroundY+.015);c.rgb=floor(c.rgb*31+.5)/31;c.rgb*=CoastLight(i.world);c.rgb=lerp(c.rgb,float3(1,.91,.59),saturate(_Hover)*.25);return c;
   }
   ENDCG }
 }
}
