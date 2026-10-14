Shader "AshenCoast/InventoryMagicV44"
{
 Properties { _MainTex("Texture",2D)="white"{} _Mode("Mode",Float)=0 _Phase("Phase",Float)=0 _Rows("Rows",Float)=5 _WaterRows("Water rows",Float)=1 _Boil("Boiling",Float)=0 }
 SubShader { Cull Off ZWrite Off ZTest Always
  Pass { CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #include "UnityCG.cginc"
   float _Mode,_Phase,_Rows,_WaterRows,_Boil;
   float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   fixed4 frag(v2f_img i):SV_Target
   {
    if(_Mode>.5)
    {
     float2 p=(floor(i.uv*48)+.5)/48-.5;p.y*=1.10;float r=length(p),a=atan2(p.y,p.x);
     float curl=a*3+r*35-_Phase*2.5,band=pow(saturate(.5+.5*sin(curl)),3);
     float grain=Hash(floor(p*48));float ring=smoothstep(.035,.17,r)*(1-smoothstep(.34,.48,r));
     float glow=band*ring*(.52+grain*.25);
     float3 color=lerp(float3(.018,.015,.030),float3(.40,.19,.58),glow);color+=float3(.08,.04,.13)*pow(band,4)*ring;
     return float4(floor(color*31+.5)/31,(1-smoothstep(.41,.49,r))*(.82+glow*.18));
    }
    float2 pix=floor(i.uv*float2(210,210));float x=pix.x,y=pix.y;
    float waterLevel=_WaterRows/max(1,_Rows)*210;float wave=sin(x*.08+_Phase*1.7)*1.4+sin(x*.039-_Phase*1.1)*1.2;
    float depth=waterLevel+wave-y;float alpha=smoothstep(-2,0,depth)*.86;
    float grain=Hash(pix),ripples=sin(x*.14+y*.38+_Phase*1.3)*sin(y*.16-_Phase*.8);
    float3 color=lerp(float3(.095,.18,.20),float3(.18,.31,.33),saturate(depth/42));color+=(grain-.5)*.045+ripples*.018;
    float crest=(1-smoothstep(0,3,abs(depth)))*(.42+.2*sin(x*.067+_Phase));color=lerp(color,float3(.45,.59,.55),crest);
    if(_Boil>.5)
    {
     float cell=floor(x/17),local=x-cell*17;float cycle=frac(_Phase*(.5+Hash(float2(cell,4))*.8)+Hash(float2(cell,2)));
     float bx=4+Hash(float2(cell,3))*8,by=waterLevel-2+wave-cycle*11,rad=1+cycle*2;
     float dist=length(float2(local-bx,(y-by)*.8));float bubble=(1-smoothstep(.5,1.4,abs(dist-rad)))*sin(cycle*3.14159);
     color=lerp(color,float3(.54,.63,.55),bubble*.65);alpha=max(alpha,bubble*.6);
    }
    return float4(floor(color*31+.5)/31,alpha);
   }
   ENDCG }
 }
}
