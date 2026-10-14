Shader "AshenCoast/CookingSteam"
{
 Properties { [PerRendererData] _MainTex("Sprite",2D)="white"{} }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float _SteamClock,_Ready;
 struct appdata{float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
 fixed4 frag(v2f i):SV_Target
 {float2 p=(floor(i.uv*float2(30,50))+.5)/float2(30,50);float steam=0;for(int n=0;n<4;n++){float y=frac(_SteamClock*(.40+n*.045)+n*.27),x=.5+sin(y*9+n*2.7+_SteamClock)*(.08+y*.14);float2 d=(p-float2(x,y))/float2(.08+y*.07,.15);steam+=saturate(1-dot(d,d))*(1-y)*.30;}return fixed4(.74,.73,.65,steam);}
 ENDCG }
 }
}
