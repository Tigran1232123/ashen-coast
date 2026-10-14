Shader "AshenCoast/AbyssFog"
{
 Properties { _Opacity("Opacity",Float)=1 }
 SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" } Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct app {float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;};
 float _Opacity;
 v2f vert(app v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=mul(unity_ObjectToWorld,v.vertex).xy;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 fixed4 frag(v2f i):SV_Target {float2 p=floor(i.world*40)/40;float n=noise(p*.9+float2(_Time.y*.13,-_Time.y*.06))*.6+noise(p*2.3-float2(_Time.y*.12,_Time.y*.03))*.4;float edge=1-smoothstep(.36,.5,length(i.uv-.5));return fixed4(.01+n*.025,.008+n*.02,.025+n*.04,edge*(.34+n*.73)*_Opacity);}
 ENDCG }
 }
}
