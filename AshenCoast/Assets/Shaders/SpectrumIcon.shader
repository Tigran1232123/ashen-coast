Shader "AshenCoast/SpectrumIcon"
{
 Properties {_MainTex("Spectral steel",2D)="white"{}}
 SubShader {Cull Off ZWrite Off ZTest Always
 Pass {CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 #include "SpectrumSurface.cginc"
 sampler2D _MainTex;float4 _SpectrumRect;float _SpectrumClock;
 fixed4 frag(v2f_img i):SV_Target
 {float2 p=(floor(i.uv*float2(128,64))+.5)/float2(128,64);float2 uv=_SpectrumRect.xy+p*_SpectrumRect.zw;return SpectrumSurface(tex2D(_MainTex,uv),p,_SpectrumClock,0);}
 ENDCG }
 }
}
