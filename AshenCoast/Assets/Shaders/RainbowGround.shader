Shader "AshenCoast/RainbowGround"
{
 Properties { _MainTex("White",2D)="white" {} _Phase("Clock",Float)=0 _Strength("Reflected light",Float)=0 _Saturation("Saturation",Float)=.7 _HueShift("Hue phase",Float)=0 }
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct app {float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};float _Phase,_Strength,_Saturation,_HueShift;
 v2f vert(app v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
 fixed4 frag(v2f i):SV_Target
 {
    float2 p=(floor(i.uv*float2(360,110))+.5)/float2(360,110);
    float2 q=(p-.5)*2;float r=dot(q,q);
    float grain=frac(sin(dot(floor(p*float2(360,110)),float2(127.1,311.7)))*43758.5453);
    float alpha=saturate(1-r)*saturate(1-r)*_Strength;
    alpha*=.75+.25*sin(q.x*29+q.y*17+_Phase*8);
    clip(alpha-grain*.08);
    float3 rainbow=lerp(float3(1,1,1),.5+.5*cos(p.x*6.28318+_HueShift+float3(0,2.0944,4.1888)),_Saturation);
    return fixed4(rainbow,alpha);
 }
 ENDCG }
 }
}
