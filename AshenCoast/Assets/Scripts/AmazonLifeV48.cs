using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class AmazonSettlementV46
    {
        public readonly Sprite[] furnishingsV48=new Sprite[9];
        public PlainsLifeV48 plains;
        int trainingDay=-1;bool trainingScheduled;
        public bool TrainingTimeV48=>Mathf.Repeat(game.DaySeconds,360)>=120 && Mathf.Repeat(game.DaySeconds,360)<225;
        void LoadArtV48()
        {
            var t=game.art.Load("AmazonFurnishingsV48");int[] ys={0,395,784,1167};int[][] xs={new[]{0,450,907,1347},new[]{0,465,926,1347},new[]{0,501,929,1347}};
            for(int row=0;row<3;row++)for(int col=0;col<3;col++)
                furnishingsV48[row*3+col]=game.art.Slice(t,new RectInt(xs[row][col]*t.width/1347,ys[row]*t.height/1167,(xs[row][col+1]-xs[row][col])*t.width/1347,(ys[row+1]-ys[row])*t.height/1167),row*3+col==1 || row*3+col==8?new Vector2(.5f,.5f):new Vector2(.5f,0),100);
        }
        void SideFenceV48(float x)
        {
            var s=art.props[4];var rect=s.rect;var post=Sprite.Create(s.texture,new Rect(rect.x+rect.width*.435f,rect.y,rect.width*.13f,rect.height),new Vector2(.5f,0),100);
            var rail=Sprite.Create(s.texture,new Rect(rect.x+rect.width*.13f,rect.y+rect.height*.50f,rect.width*.26f,rect.height*.055f),new Vector2(.5f,.5f),100);
            for(int i=0;i<=10;i++)
            {
                var p=Centre+new Vector2(x,-8+i*1.6f);Prop("Upright side fence post",post,p,.46f,root.transform);
                if(i==10)continue;
                foreach(float height in new[]{.44f,1.10f})
                {
                    var mid=p+new Vector2(0,.8f+height);var r=Prop("Connected side fence rail",rail,mid,1.7f,root.transform,CoastGame.Depth(p.y+.8f));
                    r.transform.rotation=Quaternion.Euler(0,0,90);r.transform.localScale=new Vector3(1.7f/rail.bounds.size.x,.10f/rail.bounds.size.y,1);
                }
                Barrier("Continuous training side barrier",p+Vector2.up*.8f,new Vector2(.25f,1.68f));
            }
        }
        void PaveV48(Vector2 from,Vector2 to)
        {
            float length=Vector2.Distance(from,to);int steps=Mathf.Max(1,Mathf.CeilToInt(length/1.25f));
            for(int i=0;i<steps;i++)
            {
                var p=Vector2.Lerp(from,to,(i+.5f)/steps);var r=Prop("Village cobblestone path",furnishingsV48[8],p,1.95f,pavingV54.transform,-19900);
                r.transform.localScale=new Vector3(r.transform.localScale.x,r.transform.localScale.y*.72f,1);r.color=new Color(.84f,.81f,.75f);
            }
        }
        void VillageLifeV48()
        {
            BuildRoadsV54();
            var trader=new VillageActorV46(this,Centre+new Vector2(16,-8),19,false,false,false,ItemKind.Axe){WeaponMerchantV48=true};
            trader.stock.name="Оружейница";
            foreach(var kind in new[]{ItemKind.Dagger,ItemKind.Dagger,ItemKind.Sword,ItemKind.Stake,ItemKind.Spear,ItemKind.Axe,ItemKind.Axe})trader.stock.Add(new Item(kind));people.Add(trader);
            Prop("Outdoor weapons for sale",furnishingsV48[3],Centre+new Vector2(18,-6.5f),2.1f,root.transform);
            for(int i=0;i<3;i++)
            {
                float angle=(i+.3f)*Mathf.PI*2/3;var p=Centre+new Vector2(Mathf.Cos(angle)*42,Mathf.Sin(angle)*39);
                people.Add(new VillageActorV46(this,p,20+i,false,false,false,i==1?ItemKind.Stake:ItemKind.Bow){HuntingV48=true});
            }
            plains=new PlainsLifeV48(this);
        }
        void TickLifeV48(float dt)
        {
            if(!game.journey.IsCoast || game.magic.InPocket)return;
            int day=Mathf.FloorToInt(game.DaySeconds/360);
            if(day!=trainingDay){trainingDay=day;trainingScheduled=false;EndTrainingV48();}
            if(TrainingTimeV48 && !trainingScheduled)
            {
                trainingScheduled=true;
                var rng=new System.Random(day*7919+4637);var guards=people.Where(p=>!p.IsSlave && !p.Hunter && !p.Slaver && !p.Hired && !p.Hostile && !p.WeaponMerchantV48 && !p.HuntingV48 && p.Alive).OrderBy(p=>rng.Next()).ToArray();
                int count=Math.Min(guards.Length-guards.Length%2,8+2*rng.Next(2));
                Vector2[] rings={new Vector2(-5,1),new Vector2(5,1),new Vector2(-5,-3.5f),new Vector2(5,-3.5f),new Vector2(0,5)};
                for(int i=0;i<count;i++) {guards[i].SparringPartnerV48=guards[i^1];guards[i].TrainingPointV48=Centre+rings[i/2];guards[i].TrainingSideV48=i%2==0?1:-1;guards[i].TrainingPhaseV48=i/2;}
            }
            if(!TrainingTimeV48)EndTrainingV48();plains?.Tick(dt);
        }
        public void EndTrainingV48(){foreach(var p in people)p.SparringPartnerV48=null;}
        public bool WalkableV48(Vector2 point,float radius=.28f)
        {
            if(game.journey.IsCoast && !game.GroundAllowedV58(point,radius))return false;
            for(int i=0;i<barriers.Count;i++)if(BarrierContainsV58(i,point,radius))return false;
            return !trees.Any(t=>t.Standing && Vector2.Distance(t.position,point)<radius+.24f);
        }
        public List<Vector2> PathV48(Vector2 from,Vector2 destination)
        {
            if(!PerformanceProbeV59.Measuring)return PathCoreV59(from,destination);
            long start=System.Diagnostics.Stopwatch.GetTimestamp();try{return PathCoreV59(from,destination);}
            finally{PerformanceProbeV59.PathTicks+=System.Diagnostics.Stopwatch.GetTimestamp()-start;PerformanceProbeV59.PathCalls++;}
        }
        List<Vector2> PathCoreV59(Vector2 from,Vector2 destination)
        {
            // Persistent waypoints prevent frame-by-frame left/right obstacle oscillation.
            if(Vector2.Distance(from,Centre)>60 || Vector2.Distance(destination,Centre)>60)return RiverNavigationV58.Plan(from,destination);
            int samples=Mathf.CeilToInt(Vector2.Distance(from,destination)/.3f);bool clear=true;
            for(int i=1;i<=samples;i++)if(!WalkableV48(Vector2.Lerp(from,destination,(float)i/samples))){clear=false;break;}
            if(clear)return new List<Vector2>{destination};
            PrepareNavigationV59();const int side=NavSideV59;const float cell=NavCellV59;var origin=Centre-Vector2.one*60;
            Vector2Int Cell(Vector2 p)=>new Vector2Int(Mathf.Clamp(Mathf.RoundToInt((p.x-origin.x)/cell),0,side-1),Mathf.Clamp(Mathf.RoundToInt((p.y-origin.y)/cell),0,side-1));
            Vector2 Point(int id)=>origin+new Vector2(id%side,id/side)*cell;
            var start=Cell(from);var end=Cell(destination);int first=start.y*side+start.x,goal=end.y*side+end.x;
            if(!NavigationCellV59(goal)){float best=float.MaxValue;for(int y=-3;y<=3;y++)for(int x=-3;x<=3;x++){var c=end+new Vector2Int(x,y);if(c.x<0 || c.y<0 || c.x>=side || c.y>=side)continue;int id=c.y*side+c.x;float distance=Vector2.SqrMagnitude(Point(id)-destination);if(NavigationCellV59(id) && distance<best){best=distance;goal=id;}}}
            var open=new MinHeapV53<int>();open.Push(first,0);var previous=new Dictionary<int,int>();var costs=new Dictionary<int,float>{{first,0}};var closed=new HashSet<int>();int found=-1;
            for(int iterations=0;open.Count>0 && iterations<4000;iterations++)
            {
                int at=open.Pop();if(closed.Contains(at))continue;if(at==goal){found=at;break;}closed.Add(at);
                for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++)
                {
                    if(x==0 && y==0)continue;int xx=at%side+x,yy=at/side+y;if(xx<0 || yy<0 || xx>=side || yy>=side)continue;int next=yy*side+xx;
                    // A walkable destination already lies outside every barrier and tree footprint.
                    if(closed.Contains(next) || !NavigationCellV59(next) || ReferenceNavigationV59 && CoastBlockedV53(Point(at),Point(next),.28f))continue;
                    if(x!=0 && y!=0 && (!NavigationCellV59(at+x) || !NavigationCellV59(at+y*side)))continue;
                    float cost=costs[at]+cell*(x==0 || y==0?1:1.414f);if(costs.TryGetValue(next,out var old) && old<=cost)continue;costs[next]=cost;previous[next]=at;open.Push(next,cost+Vector2.Distance(Point(next),Point(goal)));
                }
            }
            if(found<0)return new List<Vector2>();var path=new List<Vector2>();for(int id=found;id!=first;id=previous[id])path.Add(Point(id));path.Reverse();if(WalkableV48(destination))path.Add(destination);return path;
        }
        public void TrainingImpactV48(Vector2 at)
        {
            if(!root.activeInHierarchy || Vector2.Distance(game.body.position,at)>24)return;
            var obj=new GameObject("Practice impact");obj.transform.SetParent(root.transform,true);obj.transform.position=at;obj.AddComponent<PracticeSparksV48>().Initialize(game,at);
        }
    }
    public sealed class PracticeSparksV48:MonoBehaviour
    {
        static Sprite sharedSparkV53;readonly List<SpriteRenderer> sparks=new List<SpriteRenderer>();Vector2 origin;float age;
        public void Initialize(CoastGame game,Vector2 at)
        {origin=at;if(!sharedSparkV53)sharedSparkV53=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);for(int i=0;i<6;i++){var r=game.art.Renderer("Sparring spark",sharedSparkV53,at,CoastGame.Depth(at.y)+10);r.transform.SetParent(transform,true);r.transform.localScale=Vector3.one*.035f;sparks.Add(r);}}
        void Update(){age+=Time.deltaTime;for(int i=0;i<sparks.Count;i++){float a=i*Mathf.PI/3;var p=origin+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*age*1.4f;sparks[i].transform.position=p;sparks[i].color=new Color(.87f,.77f,.47f,1-age/.3f);}if(age>=.3f)Destroy(gameObject);}
    }
}
