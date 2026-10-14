Shader "AshenCoast/PrismIcon"
{
 Properties {_MainTex("Opal prism source",2D)="white"{}}
 SubShader {Cull Off ZWrite Off ZTest Always Pass {
 CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 #include "PrismSurface.cginc"
 sampler2D _MainTex;float4 _PrismRect;float _PrismClock;
 fixed4 frag(v2f_img i):SV_Target {float2 uv=(floor(i.uv*64)+.5)/64;return PrismSurface(_MainTex,_PrismRect.xy+uv*_PrismRect.zw,_PrismRect,_PrismClock);}
 ENDCG
 }}
}
