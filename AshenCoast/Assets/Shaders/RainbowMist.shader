Shader "AshenCoast/RainbowMist"
{
 Properties { _MainTex("Authored rolling clouds",2D)="white" {} _Phase("Clock",Float)=0 }
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct app {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
 sampler2D _MainTex;float4 _SpriteRect,_NextRect,_MainTex_TexelSize;float _Phase,_ArtStep,_FrameBlend;
 v2f vert(app v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
 fixed4 frag(v2f i):SV_Target
 {
    float2 p=(i.uv-_SpriteRect.xy)/_SpriteRect.zw;
    float2 warp=float2(sin(p.y*21+_Phase*2.7),cos(p.x*19-_Phase*2.1))*.006;
    warp*=sin(saturate(p)*3.14159);
    float2 uv=_SpriteRect.xy+(p+warp)*_SpriteRect.zw;
    uv=_SpriteRect.xy+(floor((uv-_SpriteRect.xy)*_MainTex_TexelSize.zw/max(1,_ArtStep))+.5)*max(1,_ArtStep)*_MainTex_TexelSize.xy;
    uv=clamp(uv,_SpriteRect.xy+_MainTex_TexelSize.xy*.5,_SpriteRect.xy+_SpriteRect.zw-_MainTex_TexelSize.xy*.5);
    float2 nextUV=_NextRect.xy+(uv-_SpriteRect.xy)/_SpriteRect.zw*_NextRect.zw;
    nextUV=clamp(nextUV,_NextRect.xy+_MainTex_TexelSize.xy*.5,_NextRect.xy+_NextRect.zw-_MainTex_TexelSize.xy*.5);
    fixed4 a=tex2D(_MainTex,uv),b=tex2D(_MainTex,nextUV);
    float alpha=lerp(a.a,b.a,_FrameBlend);
    float3 color=lerp(a.rgb*a.a,b.rgb*b.a,_FrameBlend)/max(.0001,alpha);
    fixed4 c=fixed4(color,alpha)*i.color;
    c.rgb*=.97+.03*sin(p.y*24+p.x*19+_Phase*3);
    c.rgb=floor(c.rgb*31+.5)/31;return c;
 }
 ENDCG }
 }
}
