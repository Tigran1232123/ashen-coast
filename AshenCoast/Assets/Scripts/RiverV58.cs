using UnityEngine;
namespace AshenCoast
{
    public static class RiverV58
    {
        public const float HalfWidth=21;
        public static bool Ready;
        public static Vector2 Junction;
        public static readonly Vector2[] Axes={new Vector2(.65f,-.76f),new Vector2(.12f,.993f),new Vector2(-.994f,.11f)};
        public static readonly float[] Lengths={720,1100,1100},Bends={24,-18,20};
        public static Vector2 Centre(int arm,float t)
        {var a=Axes[arm].normalized;return Junction+a*(t*Lengths[arm])+new Vector2(-a.y,a.x)*Mathf.Sin(t*Mathf.PI)*Bends[arm];}
        public static Vector2 Normal(int arm,float t)
        {var a=Axes[arm].normalized;var tangent=a*Lengths[arm]+new Vector2(-a.y,a.x)*(Mathf.Cos(t*Mathf.PI)*Mathf.PI*Bends[arm]);return new Vector2(-tangent.y,tangent.x).normalized;}
        public static float SmoothMin(float a,float b,float k)
        {float h=Mathf.Max(k-Mathf.Abs(a-b),0)/k;return Mathf.Min(a,b)-h*h*k*.25f;}
        public static void Configure(Vector2 village)
        {Junction=village+new Vector2(80,-65);Ready=true;Shader.SetGlobalVector("_RiverJunctionV58",Junction);Shader.SetGlobalFloat("_RiverReadyV58",1);}
        public static float Bank(Vector2 p)
        {
            if(!Ready)return 10000;float d=10000;
            for(int i=0;i<3;i++){var a=Axes[i].normalized;float t=Mathf.Clamp01(Vector2.Dot(p-Junction,a)/Lengths[i]);float next=Vector2.Distance(p,Centre(i,t))-HalfWidth;d=SmoothMin(d,next,10);}return d;
        }
        public static float BridgeT(int arm)=>arm==0?.16f:arm==1?.14f:.13f;
        public static bool OnBridge(Vector2 p,float margin=0)
        {if(!Ready)return false;for(int i=0;i<3;i++){float t=BridgeT(i);var delta=p-Centre(i,t);var n=Normal(i,t);var a=new Vector2(-n.y,n.x);if(Mathf.Abs(Vector2.Dot(delta,n))<=HalfWidth+3-margin && Mathf.Abs(Vector2.Dot(delta,a))<=4-margin)return true;}return false;}
        public static bool Passable(Vector2 p,float margin=.4f)=>Bank(p)>margin || OnBridge(p,margin);
        static GameObject bridges;
        public static void Build(CoastGame game)
        {
            if(bridges){bridges.SetActive(false);Object.Destroy(bridges);}bridges=new GameObject("Three river bridges V58");bridges.transform.SetParent(game.coastRoot.transform,false);
            var wood=game.art.Load("TavernSurfacesV10");var plankArt=Sprite.Create(wood,new Rect(0,wood.height-64,Mathf.RoundToInt(wood.width*.48f),60),Vector2.one*.5f,100);
            for(int arm=0;arm<3;arm++)
            {
                float t=BridgeT(arm);var c=Centre(arm,t);var n=Normal(arm,t);var across=new Vector2(-n.y,n.x);float angle=Mathf.Atan2(n.y,n.x)*Mathf.Rad2Deg;
                for(int plank=0;plank<80;plank++)
                {
                    float z=(plank/79f-.5f)*48;var at=c+n*z;
                    var r=game.art.Renderer("Bridge "+arm+" / worn timber plank",plankArt,at,-19000);
                    r.transform.SetParent(bridges.transform,true);r.transform.localScale=new Vector3(8/plankArt.bounds.size.x,.55f/plankArt.bounds.size.y,1);r.transform.rotation=Quaternion.Euler(0,0,angle+90);r.color=plank%3==0?new Color(.82f,.77f,.66f):new Color(.94f,.89f,.78f);game.art.PixelStyle(r);
                    if(plank%5==0)foreach(int side in new[]{-1,1})
                    {var post=game.art.Renderer("Bridge rail post",Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f,1),at+across*(side*3.85f)+Vector2.up*.24f,CoastGame.Depth(at.y)+1);post.transform.SetParent(bridges.transform,true);post.transform.localScale=new Vector3(.16f,.65f,1);post.color=new Color(.28f,.17f,.09f);}
                }
                foreach(int side in new[]{-1,1}){var l=new GameObject("Bridge timber handrail").AddComponent<LineRenderer>();l.transform.SetParent(bridges.transform);l.sharedMaterial=game.art.lit;l.positionCount=2;l.SetPosition(0,c-n*24+across*side*3.85f+Vector2.up*.55f);l.SetPosition(1,c+n*24+across*side*3.85f+Vector2.up*.55f);l.startWidth=l.endWidth=.13f;l.startColor=l.endColor=new Color(.32f,.20f,.10f);l.sortingOrder=-18000;}
            }
        }
    }
}
