Shader "AshenCoast/OrbIcon"
{
 Properties {_MainTex("Sphere source",2D)="white"{}}
 SubShader
 {
  Cull Off ZWrite Off ZTest Always
  Pass
  {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #include "UnityCG.cginc"
   #include "OrbSurface.cginc"
   sampler2D _MainTex;float4 _OrbRect;float _OrbClock;
   fixed4 frag(v2f_img i):SV_Target
   {float2 uv=(floor(i.uv*64)+.5)/64;return OrbSurface(_MainTex,_OrbRect.xy+uv*_OrbRect.zw,_OrbRect,_OrbClock);}
   ENDCG
  }
 }
}
