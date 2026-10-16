using UnityEngine;

namespace AshenCoast
{
    public static partial class WorldShape
    {
        public static Vector2? AmazonPosition;
        public static bool AmazonFootprint(Vector2 point)=>AmazonPosition.HasValue && Vector2.Distance(point,AmazonPosition.Value)<30;
        public static int Seed {get;private set;}=74391;
        public static float Phase {get;private set;}=1.73f;
        public static Vector2 FortPosition {get;private set;}
        public static readonly Vector2 Centre=new Vector2(0,450);
        public static void Configure(int seed)
        {
            AmazonPosition=null;RiverV58.Ready=false;Shader.SetGlobalFloat("_RiverReadyV58",0);Seed=seed;Phase=MeadowLayout.Value(3,7,seed,17)*Mathf.PI*2;
            var random=new System.Random(seed^0x51ab);
            FortPosition=new Vector2(120,240);
            for(int i=0;i<200;i++)
            {
                var p=new Vector2((float)random.NextDouble()*650-325,110+(float)random.NextDouble()*660);
                if(Distance(p)>48 && MeadowLayout.PathDistance(p)>17){FortPosition=p;break;}
            }
            Shader.SetGlobalFloat("_LandPhase",Phase);
        }
        // Smooth radial outline with bays and headlands. Its poles stay inside the old broad bounds.
        public static float CoastDistanceV58(Vector2 p)
        {
            var q=new Vector2(p.x/410,(p.y-450)/450);float a=Mathf.Atan2(q.y,q.x);
            float radius=1+.12f*Mathf.Sin(a*3+Phase)+.055f*Mathf.Sin(a*5-Phase*1.7f)+.025f*Mathf.Sin(a*9+Phase*.4f);
            radius=-RiverV58.SmoothMin(-radius,-1.075f*Mathf.Pow(Mathf.Max(0,-Mathf.Sin(a)),6),.045f);
            return RiverV58.SmoothMin((radius-q.magnitude)*350,p.y-MeadowLayout.Shore(p.x),8);
        }
        public static float Distance(Vector2 p)=>Mathf.Min(CoastDistanceV58(p),RiverV58.OnBridge(p)?1000:RiverV58.Bank(p));
        public static bool Land(Vector2 p,float margin=0)=>Distance(p)>=margin;
        public static bool FortFootprint(Vector2 p)=>Mathf.Abs(p.x-FortPosition.x)<9 && p.y>FortPosition.y-2.5f && p.y<FortPosition.y+12;
        public static Vector2 NearestLand(Vector2 p,float margin=.6f)
        {
            p=new Vector2(Mathf.Clamp(p.x,MeadowLayout.MinX+.5f,MeadowLayout.MaxX-.5f),Mathf.Clamp(p.y,MeadowLayout.Shore(p.x)+margin,MeadowLayout.MaxY-.5f));
            for(int i=0;i<120 && !Land(p,margin);i++)p=Vector2.MoveTowards(p,Centre,4);
            return p;
        }
    }
}
