float _RiverReadyV58;float2 _RiverJunctionV58;
float smoothMinimumV58(float a,float b,float k){float h=max(k-abs(a-b),0)/k;return min(a,b)-h*h*k*.25;}
float coastDistanceV58(float2 p,float phase)
{
 float2 q=float2(p.x/410,(p.y-450)/450);float a=atan2(q.y,q.x);
 float radius=1+.12*sin(a*3+phase)+.055*sin(a*5-phase*1.7)+.025*sin(a*9+phase*.4);
 float south=1.075*pow(max(0,-sin(a)),6);radius=-smoothMinimumV58(-radius,-south,.045);
 return smoothMinimumV58((radius-length(q))*350,p.y-(-4.6+sin(p.x*.17)*.48+sin(p.x*.79)*.16),8);
}
float riverBankV58(float2 p)
{
 if(_RiverReadyV58<.5)return 10000;
 float d=10000;
 for(int i=0;i<3;i++)
 {
  float2 a=normalize(i==0?float2(.65,-.76):i==1?float2(.12,.993):float2(-.994,.11));float len=i==0?720:1100,bend=i==0?24:i==1?-18:20;
  float t=saturate(dot(p-_RiverJunctionV58,a)/len);float2 c=_RiverJunctionV58+a*(t*len)+float2(-a.y,a.x)*sin(t*3.14159265)*bend;
  d=smoothMinimumV58(d,length(p-c)-21,10);
 }
 return d;
}
