Shader "AshenCoast/Rainbow"
{
 Properties { _MainTex("Flowing pixel strands",2D)="white" {} _Phase("Flow clock",Float)=0 _Fade("Fade",Float)=1 _Height("World height",Float)=14 _Head("Falling leading edge",Float)=0 _Tail("Trailing tail height",Float)=14 _Ending("Ending",Float)=0 _Hover("Hover",Float)=0 }
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct app {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};sampler2D _MainTex;float _Phase,_Fade,_Height,_Head,_Tail,_Ending,_Hover;
 v2f vert(app v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
 float noise(float x){return frac(sin(x*127.1)*43758.5453);}
 fixed4 frag(v2f i):SV_Target
 {
    float x=(floor(i.uv.x*208)+.5)/208,h=(floor(i.uv.y*_Height*40)+.5)/40;
    float jag=(noise(floor(x*86))-.5)*.46;
    float head=smoothstep(_Head+jag-.15,_Head+jag+.18,h);
    float end=_Ending<.5?1:1-smoothstep(_Tail+jag-.2,_Tail+jag+.28,h);
    float flare=1+.18*exp(-h*1.8);
    float sx=(x-.5)/flare+.5;
    sx+=sin(h*2.3-_Phase*4.3+x*17)*.008+sin(h*6.1-_Phase*8+x*31)*.003;
    float y=frac(h*.18+_Phase*1.45);
    fixed4 a=tex2D(_MainTex,float2(sx,y));
    fixed4 b=tex2D(_MainTex,float2(sx,frac(y+.5)));
    float seam=pow(abs(y-.5)*2,8);
    fixed4 c=lerp(a,b,seam*.8);
    float filament=pow(saturate(sin(x*281+h*14+_Phase*21)),12);
    float moving=.78+.22*sin(h*19+_Phase*32+x*113);
    float3 rgb=c.rgb;
    rgb*=moving*1.48;rgb+=filament*.22;
    float impact=saturate(1-h/.5)*(.65+.35*sin(_Phase*24+x*63));
    rgb=lerp(rgb,float3(.92,.97,1),impact*.36);
    rgb+=_Hover*float3(.16,.13,.05);
    float edges=1-smoothstep(.45,.50,abs(x-.5));
    float alpha=c.a*edges*head*end*_Fade;
    float tip=_Ending>.5?exp(-abs(h-_Tail)*8):exp(-abs(h-_Head)*8);
    rgb+=tip*.25;
    return fixed4(floor(rgb*31+.5)/31,alpha*.58)*i.color;
 }
 ENDCG }
 }
}
