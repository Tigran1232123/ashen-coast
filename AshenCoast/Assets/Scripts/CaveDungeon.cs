using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public static class CatacombLayout
    {
        public static readonly Vector2[] Rooms={new Vector2(-8,-16),new Vector2(8,-10),new Vector2(-9,-1),new Vector2(8,7),new Vector2(0,23)};
        public static readonly float[] Radii={5,5.5f,6,6,10};
        public static readonly Rect Bounds=Rect.MinMaxRect(-21,-23,21,35);
        public static bool Floor(Vector2 p)
        {
            for(int i=0;i<Rooms.Length;i++)
            {
                var d=p-Rooms[i];float angle=Mathf.Atan2(d.y,d.x),edge=Radii[i]+.35f*Mathf.Sin(angle*5+i)+.22f*Mathf.Sin(angle*9-i);
                if(d.magnitude<edge)return true;
                if(i>0 && FortWorld.SegmentDistance(p,Rooms[i-1],Rooms[i])<1.65f)return true;
            }
            return false;
        }
        public static Vector2 Entrance(int seed)
        {
            var random=new System.Random(seed^0x38c710);
            for(int i=0;i<300;i++)
            {var p=new Vector2((float)random.NextDouble()*420-210,55+(float)random.NextDouble()*330);if(WorldShape.Land(p,15) && Vector2.Distance(p,WorldShape.FortPosition)>35)return p;}
            return WorldShape.NearestLand(new Vector2(-45,65),15);
        }
    }
    public sealed class CaveArt
    {
        public readonly Sprite[,] spider=new Sprite[4,8];
        public readonly Sprite[] head=new Sprite[16],body=new Sprite[16],tail=new Sprite[16];
        public readonly Texture2D stone;public readonly Sprite entrance;
        public CaveArt(ArtBank bank)
        {
            stone=bank.Load("CaveTilesV33");var portal=bank.Load("CatacombEntranceV33");entrance=bank.Slice(portal,new RectInt(0,0,portal.width,portal.height),new Vector2(.5f,0),200);
            LoadRows(bank,"SpiderV33",4,8,(r,c,s)=>spider[r,c]=s,1.25f,false);
            LoadWormV54(bank,"WormHeadV54",head,2.50f);
            LoadWormV54(bank,"WormBodyV54",body,2.50f);
            LoadWormV54(bank,"WormTailV54",tail,2.50f);
        }
        static void LoadWormV54(ArtBank bank,string name,Sprite[] destination,float width)
        {var frames=AtlasV54.Actors(bank,name,4,4,width,true);for(int r=0;r<4;r++)for(int c=0;c<4;c++)destination[r*4+c]=frames[r,c];}
        static void LoadRows(ArtBank bank,string name,int rows,int columns,System.Action<int,int,Sprite> put,float size,bool width)
        {
            var t=bank.Load(name);var bands=bank.Bands(t,true,rows);var data=t.GetPixels32();
            for(int r=0;r<rows;r++)for(int c=0;c<columns;c++)
            {
                int left=c*t.width/columns,right=(c+1)*t.width/columns,minX=right,maxX=left,minY=bands[r].y,maxY=bands[r].x;
                for(int y=bands[r].x;y<=bands[r].y;y++)for(int x=left;x<right;x++)if(data[(t.height-1-y)*t.width+x].a>96){minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);}
                // Fixed row height and body registration prevent crawling frames from bobbing.
                float ppu=width?(t.width/columns*.85f)/size:(bands[0].y-bands[0].x+1)/size;
                int w=maxX-minX+1,h=bands[r].y-bands[r].x+1;
                var sprite=Sprite.Create(t,new Rect(minX,t.height-1-bands[r].y,w,h),width?new Vector2(.5f,.5f):new Vector2(.5f,.20f),ppu,0,SpriteMeshType.FullRect);
                sprite.name=name+" / "+r+" / "+c;put(r,c,sprite);
            }
        }
    }
    public sealed partial class CaveDungeon : MonoBehaviour
    {
        public CoastGame game;public GameObject root;public GameObject entranceRootV56;public CaveArt art;public SpriteRenderer entrance,exit;
        public Vector2 Position;public readonly Vector2 ExitPoint=new Vector2(-8,-20);
        public readonly HashSet<Vector2Int> floor=new HashSet<Vector2Int>();
        public readonly Dictionary<Vector2Int,BoxCollider2D> walls=new Dictionary<Vector2Int,BoxCollider2D>();
        public readonly List<CaveSpider> spiders=new List<CaveSpider>();public readonly List<SpectralWorm> worms=new List<SpectralWorm>();
        public readonly List<CaveWeb> webs=new List<CaveWeb>();public readonly List<WebBolt> bolts=new List<WebBolt>();public readonly List<FortTorch> torches=new List<FortTorch>();
        public float clock,snare;public bool Inside=>game.journey.InCatacombs && !game.magic.InPocket;
        public const float WebRadius=2.55f;
        public bool Snared=>Inside && snare>0;
        public bool OnWeb=>Inside && webs.Any(w=>w.age<10 && Vector2.Distance(w.point,game.body.position)<w.radius);
        public float SlowMultiplier=>Snared?0:OnWeb?.40f:1;
        public IEnumerable<ICombatTarget> Targets=>spiders.Cast<ICombatTarget>().Concat(worms.SelectMany(w=>w.parts).Cast<ICombatTarget>()).Where(t=>t.Alive).ToArray();
        readonly MaterialPropertyBlock properties=new MaterialPropertyBlock();
        public void Initialize(CoastGame owner)
        {
            game=owner;art=new CaveArt(game.art);Position=CatacombLayout.Entrance(game.meadow.Seed);root=new GameObject("Winding limestone catacombs");
            entranceRootV56=new GameObject("Seeded catacomb entrance");entranceRootV56.transform.SetParent(game.coastRoot.transform,false);
            entrance=Portal("Catacomb descent",Position,4.8f,entranceRootV56.transform);
            for(int side=-1;side<=1;side+=2){var o=new GameObject("Descent stone side footprint");o.transform.SetParent(entranceRootV56.transform,false);o.transform.position=Position+new Vector2(side*1.65f,.8f);o.AddComponent<BoxCollider2D>().size=new Vector2(1.1f,1.6f);}
            for(int y=-23;y<35;y++)for(int x=-21;x<21;x++){var cell=new Vector2Int(x,y);if(CatacombLayout.Floor(new Vector2(x+.5f,y+.5f)))floor.Add(cell);}
            BuildStone();exit=Portal("Stone stairs up to the meadow",ExitPoint,2.2f,root.transform);
            foreach(var p in new[]{new Vector2(-6,-14),new Vector2(8,-9),new Vector2(11,-11),new Vector2(-10,-2),new Vector2(-7,1),new Vector2(7,7),new Vector2(10,9)})spiders.Add(new CaveSpider(this,p,spiders.Count));
            var boss=new SpectralWorm(this);for(int i=0;i<10;i++)boss.parts.Add(new WormPart(this,new Vector2(7-i*SpectralWorm.Spacing,23),i));worms.Add(boss);boss.ResetTrail();root.SetActive(false);
        }
        SpriteRenderer Portal(string name,Vector2 at,float width,Transform parent)
        {var s=game.art.Renderer(name,art.entrance,at,CoastGame.Depth(at.y));s.transform.SetParent(parent,true);s.transform.localScale=Vector3.one*(width/s.sprite.bounds.size.x);game.art.PixelStyle(s);return s;}
        void BuildStone()
        {
            var wallCells=new HashSet<Vector2Int>();foreach(var p in floor)for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++){var q=p+new Vector2Int(x,y);if(!floor.Contains(q))wallCells.Add(q);}
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();foreach(var p in floor)Quad(vertices,uv,indices,p.x,p.y,1,1,5);
            MeshObject("Organic stone floor",vertices,uv,indices,0,-2000);
            foreach(var p in wallCells)
            {
                var o=new GameObject("Stone footprint / "+p);o.transform.SetParent(root.transform,false);o.transform.position=new Vector2(p.x+.5f,p.y+.5f);var collider=o.AddComponent<BoxCollider2D>();collider.size=Vector2.one;walls.Add(p,collider);
                vertices=new List<Vector3>();uv=new List<Vector2>();indices=new List<int>();
                // Both wall layers rise from their occupied ground cell, never from an open passage.
                Quad(vertices,uv,indices,p.x,p.y,1,.72f,0);MeshObject("Chiselled catacomb wall face",vertices,uv,indices,1,CoastGame.Depth(p.y));
                vertices=new List<Vector3>();uv=new List<Vector2>();indices=new List<int>();Quad(vertices,uv,indices,p.x,p.y+.70f,1,.95f,0);MeshObject("Rough limestone cap",vertices,uv,indices,2,CoastGame.Depth(p.y)+1);
            }
        }
        static void Quad(List<Vector3> v,List<Vector2> uv,List<int> triangles,float x,float y,float w,float h,float z)
        {int n=v.Count;v.AddRange(new[]{new Vector3(x,y,z),new Vector3(x+w,y,z),new Vector3(x+w,y+h,z),new Vector3(x,y+h,z)});uv.AddRange(new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up});triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
        void MeshObject(string name,List<Vector3> v,List<Vector2> uv,List<int> triangles,int surface,int order)
        {var o=new GameObject(name);o.transform.SetParent(root.transform,false);var m=new Mesh();m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(triangles,0);m.RecalculateBounds();o.AddComponent<MeshFilter>().sharedMesh=m;var renderer=o.AddComponent<MeshRenderer>();var material=new Material(Shader.Find("AshenCoast/CaveStone"));material.SetTexture("_MainTex",art.stone);material.SetFloat("_Surface",surface);renderer.sharedMaterial=material;renderer.sortingOrder=order;}
        public bool Walkable(Vector2 p,float radius=.23f)
        {for(int i=0;i<9;i++){float a=i*Mathf.PI/4;var q=i==8?p:p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;if(!floor.Contains(WorldGrid.Cell(q)))return false;}return true;}
        public bool Blocked(Vector2 from,Vector2 to,float radius=.23f)
        {int samples=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(from,to)/.12f));for(int i=0;i<=samples;i++)if(!Walkable(Vector2.Lerp(from,to,i/(float)samples),radius))return true;return false;}
        public Vector2 Clamp(Vector2 p)=>new Vector2(Mathf.Clamp(p.x,-20.7f,20.7f),Mathf.Clamp(p.y,-22.7f,34.7f));
        public Vector2 Step(Vector2 from,Vector2 delta,float radius=.23f)
        {foreach(float a in new[]{0,25f,-25f,50f,-50f,80f,-80f}){var d=(Vector2)(Quaternion.Euler(0,0,a)*delta);if(!Blocked(from,from+d,radius))return d;}return Vector2.zero;}
        public List<Vector2> Path(Vector2 start,Vector2 end)
        {
            var a=WorldGrid.Cell(start);var b=WorldGrid.Cell(end);var queue=new Queue<Vector2Int>();var seen=new Dictionary<Vector2Int,Vector2Int>();queue.Enqueue(a);seen[a]=a;
            while(queue.Count>0){var p=queue.Dequeue();if(p==b)break;foreach(var d in new[]{Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left}){var q=p+d;if(!seen.ContainsKey(q) && floor.Contains(q) && Walkable(q+Vector2.one*.5f,.4f)){seen[q]=p;queue.Enqueue(q);}}}
            var result=new List<Vector2>();if(!seen.ContainsKey(b))return result;for(var p=b;p!=a;p=seen[p])result.Add(p+Vector2.one*.5f);result.Reverse();return result;
        }
        public bool CanInteractAt(Vector2 p)
        {if(game.magic.InPocket || game.journey.Cutscene)return false;var s=Inside?exit:entrance;return (Inside || game.journey.IsCoast) && Vector2.Distance(game.body.position,Inside?ExitPoint:Position)<2.7f && game.art.Opaque(s.sprite,s.transform.InverseTransformPoint(p));}
        public SpriteRenderer InteractionAt(Vector2 p)=>CanInteractAt(p)?Inside?exit:entrance:null;
        public bool Interact(Vector2 p){if(!CanInteractAt(p))return false;if(Inside)Leave();else Enter();return true;}
        public void Enter(){snare=0;game.journey.SetArea(WorldArea.Catacombs,new Vector2(-8,-18.5f));}
        public void Leave(){snare=0;game.journey.SetArea(WorldArea.Coast,Position+Vector2.down*1.3f);}
        void Update(){if(game && !game.testMode && !game.paused)Tick(Time.deltaTime);}
        public void Tick(float dt)
        {
            if(!Inside){PoseCocoonV43();return;}clock+=dt;snare=Mathf.Max(0,snare-dt);PoseCocoonV43();
            if(!game.SimulateWorldV49)return;foreach(var s in spiders)game.TickCoopActorV49(s.Position,()=>s.Tick(dt));foreach(var w in worms.ToArray())game.TickCoopActorV49(w.Position,()=>w.Tick(dt));
            for(int i=bolts.Count-1;i>=0;i--)
            {var b=bolts[i];b.age+=dt;var end=b.point+b.velocity*dt;bool hit=false;Vector2 hitPoint=game.body.position;game.TickCoopActorV49((b.point+end)*.5f,()=>{if(FortWorld.SegmentDistance(game.body.position+Vector2.up*.45f,b.point,end)<.35f){hit=true;hitPoint=game.body.position;Snare(2);}});bool wall=Blocked(b.point-Vector2.up*.45f,end-Vector2.up*.45f,.08f);if(hit || wall || b.age>=b.life){var ground=hit?hitPoint:wall?b.point-Vector2.up*.45f:end-Vector2.up*.45f;if(Walkable(ground,.15f))AddWeb(ground);Destroy(b.sprite.gameObject);bolts.RemoveAt(i);}else{b.point=end;b.sprite.transform.position=end;b.sprite.transform.Rotate(0,0,dt*120);}}
            for(int i=webs.Count-1;i>=0;i--){var w=webs[i];w.age+=dt;if(w.age>=10){Destroy(w.sprite.gameObject);webs.RemoveAt(i);}else{w.sprite.color=new Color(1,1,1,Mathf.Clamp01((10-w.age)*2)*.96f);}}
        }
        public void Snare(float duration){if(game.CoopOnline && game.coop.Context!=null){game.coop.Send(new LanPacketV49{kind="snare",target=game.coop.Context.Peer,region=game.coop.Region,body=duration.ToString("R",System.Globalization.CultureInfo.InvariantCulture)});return;}snare=Mathf.Max(snare,duration);game.body.linearVelocity=Vector2.zero;game.ui.Notice("Кокон · 2 секунды");}
        SpriteRenderer WebSprite(Vector2 p,float diameter,int order)
        {var s=game.art.Renderer("Silken web",game.art.icons[0],p,order);var white=Texture2D.whiteTexture;s.sprite=Sprite.Create(white,new Rect(0,0,white.width,white.height),Vector2.one*.5f,white.width);s.transform.localScale=Vector3.one*diameter;s.transform.SetParent(root.transform,true);s.sharedMaterial=new Material(Shader.Find("AshenCoast/SpiderWeb"));return s;}
        public CaveWeb AddWeb(Vector2 p){var w=new CaveWeb{point=p,radius=WebRadius,sprite=WebSprite(p,WebRadius*2,-1600)};webs.Add(w);return w;}
        public void ShootWeb(Vector2 origin,Vector2 aim)
        {var d=aim.normalized;float reach=Mathf.Clamp(aim.magnitude+.7f,1,8);bolts.Add(new WebBolt{point=origin,velocity=d*8,life=reach/8,sprite=WebSprite(origin,.38f,CoastGame.Depth(origin.y)+3)});}
        public int AppendLights(Vector4[] p,Vector4[] c,int n)
        {return n;}
        public void Sever(WormPart part)
        {
            var owner=worms.FirstOrDefault(w=>w.parts.Contains(part));if(owner==null)return;int index=owner.parts.IndexOf(part);var before=owner.parts.Take(index).ToList();var after=owner.parts.Skip(index+1).ToList();worms.Remove(owner);
            foreach(var list in new[]{before,after})if(list.Count>0){var worm=new SpectralWorm(this);worm.parts.AddRange(list);worm.ResetTrail();worms.Add(worm);}
        }
    }
    public sealed class CaveWeb {public Vector2 point;public float radius,age;public SpriteRenderer sprite;}
    public sealed class WebBolt {public Vector2 point,velocity;public float age,life;public SpriteRenderer sprite;}
    public sealed class CaveSpider : ICombatTarget
    {
        readonly CaveDungeon cave;readonly CoastGame game;public SpriteRenderer sprite;public float health=80,clock,distance,attackClock,cooldown;public int view,frame,Bites;public bool Attacking,Biting,Grabbed;readonly float seed;Vector2 biteAim;
        public Vector2 Position=>sprite.transform.position;public SpriteRenderer Renderer=>sprite;public bool Alive=>health>0;
        public CaveSpider(CaveDungeon owner,Vector2 p,int index)
        {cave=owner;game=owner.game;seed=index*1.73f;sprite=game.art.Renderer("Catacomb spider",cave.art.spider[0,0],p,CoastGame.Depth(p.y));sprite.transform.SetParent(cave.root.transform,true);var feet=sprite.gameObject.AddComponent<CircleCollider2D>();feet.radius=.32f;feet.offset=Vector2.up*.12f;var block=new MaterialPropertyBlock();sprite.GetPropertyBlock(block);block.SetFloat("_Emission",.20f);sprite.SetPropertyBlock(block);Pose();}
        public float Hurt(float amount){if(!Alive)return 0;amount*=game.spellcraft?.DamageFactor(sprite)??1;float hit=Mathf.Min(health,amount);health-=hit;game.ShowDamage(hit,Position+Vector2.up*.85f,sprite);if(!Alive){sprite.enabled=false;sprite.GetComponent<Collider2D>().enabled=false;}return hit;}
        public void Tick(float dt)
        {
            if(!Alive || Grabbed || cave.game.spellcraft!=null && cave.game.spellcraft.ImmobilizedV44(sprite))return;clock+=dt;cooldown-=dt;var delta=game.body.position-Position;
            if(Attacking)
            {
                attackClock+=dt;float duration=Biting?.62f:.72f;frame=Mathf.Min(7,(int)(attackClock/duration*8));
                if(Biting)
                {
                    float thrust=attackClock<.16f?-.30f:attackClock<.30f?2.6f:-.55f;
                    sprite.transform.position+=(Vector3)cave.Step(Position,biteAim*dt*thrust,.28f);
                    if(attackClock>=.28f && attackClock-dt<.28f && !game.Dead && Vector2.Distance(Position,game.body.position)<1.4f && !cave.Blocked(Position,game.body.position,.08f))
                    {game.HurtPlayer(12);Bites++;}
                }
                else if(attackClock>=.35f && attackClock-dt<.35f)cave.ShootWeb(Position+Vector2.up*.45f,game.body.position-Position);
                if(attackClock>=duration){Attacking=false;cooldown=Biting?1.15f:2.6f+seed*.08f;}
            }
            else if(!game.Dead && delta.magnitude<7*(game.spellcraft?.Vision(sprite)??1) && cooldown<=0 && !cave.Blocked(Position,game.body.position,.08f))
            {Attacking=true;Biting=delta.magnitude<1.6f;attackClock=0;frame=0;biteAim=delta.normalized;}
            else if(!game.Dead && delta.magnitude<12 && delta.magnitude>(cooldown>0?1.05f:3.4f))
            {var move=cave.Step(Position,delta.normalized*dt*1.75f,.28f);sprite.transform.position+=(Vector3)move;distance+=move.magnitude;frame=(int)(distance/.85f*8)%8;}
            view=Mathf.Abs(delta.x)>Mathf.Abs(delta.y)?1:delta.y>0?2:0;
            sprite.flipX=view==1 && delta.x>0;Pose();
        }
        void Pose(){sprite.sprite=cave.art.spider[Attacking?3:view,frame];sprite.sortingOrder=CoastGame.Depth(Position.y);game.art.PixelStyle(sprite);}
    }
    public sealed class WormPart : ICombatTarget
    {
        public const float MaximumHealth=200,DamageLimit=20;public float health=MaximumHealth;public SpriteRenderer sprite;public readonly CaveDungeon cave;public readonly int seed;public bool IsHead;
        public Vector2 Position=>sprite.transform.position;public SpriteRenderer Renderer=>sprite;public bool Alive=>health>0;
        public WormPart(CaveDungeon owner,Vector2 p,int id)
        {cave=owner;seed=id;sprite=cave.game.art.Renderer("Spectral worm segment / "+id,cave.art.body[id%cave.art.body.Length],p,CoastGame.Depth(p.y));sprite.transform.SetParent(cave.root.transform,true);sprite.sharedMaterial=new Material(Shader.Find("AshenCoast/SpectralWorm"));var c=sprite.gameObject.AddComponent<CapsuleCollider2D>();c.direction=CapsuleDirection2D.Horizontal;c.size=new Vector2(1.90f,.85f);c.isTrigger=true;Pose(Vector2.right,0,false,false);}
        public float Hurt(float amount)
        {if(!Alive)return 0;amount*=cave.game.spellcraft?.DamageFactor(sprite)??1;float applied=Mathf.Min(health,(cave.game.prism!=null && cave.game.prism.ApplyingDamage || cave.game.spellcraft!=null && (cave.game.spellcraft.BypassDamage || cave.game.spellcraft.Bound(sprite)))?amount:Mathf.Min(DamageLimit,amount));health-=applied;cave.game.ShowDamage(applied,Position+Vector2.up*.55f,sprite);if(!Alive){sprite.enabled=false;sprite.GetComponent<Collider2D>().enabled=false;cave.Sever(this);}return applied;}
        public void Pose(Vector2 direction,float clock,bool head,bool tail)
        {IsHead=head;int phase=Mathf.FloorToInt(clock*13+seed*2.7f);sprite.sprite=head?cave.art.head[phase%cave.art.head.Length]:tail?cave.art.tail[phase%cave.art.tail.Length]:cave.art.body[phase%cave.art.body.Length];sprite.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg+(tail?180:0));sprite.sortingOrder=CoastGame.Depth(Position.y)+1;cave.game.art.PixelStyle(sprite);sprite.transform.localScale=new Vector3(1+.035f*Mathf.Sin(clock*5.2f-seed*.8f),1+.07f*Mathf.Sin(clock*5.2f-seed*.8f+1.4f),1);var block=new MaterialPropertyBlock();sprite.GetPropertyBlock(block);block.SetFloat("_ArtStep",1);block.SetFloat("_InkTime",clock+seed*1.71f);sprite.SetPropertyBlock(block);}
    }
    public sealed class SpectralWorm
    {
        public const float Spacing=1.65f;
        public readonly List<WormPart> parts=new List<WormPart>();
        readonly CaveDungeon cave;readonly List<Vector2> trail=new List<Vector2>();
        List<Vector2> route=new List<Vector2>();int routeIndex;float repath,contact,lastSeen,lungeTime,lungeCooldown,patrolClock;
        Vector2 direction=Vector2.right,routeGoal,patrolGoal,lastGoal;float speed;
        public float Health=>parts.Sum(p=>p.health);public float MaximumHealth=>parts.Count*WormPart.MaximumHealth;public Vector2 Position=>parts[0].Position;
        public bool Hunting {get;private set;}public bool Lunging=>lungeTime>0;public int TrailSamples=>trail.Count;
        public float SpeedFactor=>Mathf.Lerp(1.8f,1,Mathf.Clamp01((parts.Count-1)/9f));
        public SpectralWorm(CaveDungeon owner){cave=owner;}
        public void ResetTrail()
        {
            trail.Clear();foreach(var p in parts)trail.Add(p.Position);
            direction=parts.Count>1?(parts[0].Position-parts[1].Position).normalized:Vector2.right;
            lastGoal=patrolGoal=Position;speed=0;
            for(int i=0;i<parts.Count;i++)parts[i].Pose(i==0?direction:parts[i-1].Position-parts[i].Position,cave.clock,i==0,i==parts.Count-1 && i>0);
        }
        Vector2 Navigate(Vector2 target)
        {
            if(!cave.Blocked(Position,target,.46f)){route.Clear();return target;}
            if(repath<=0 || routeIndex>=route.Count || Vector2.Distance(target,routeGoal)>1.5f)
            {route=cave.Path(Position,target);routeIndex=0;repath=.35f;routeGoal=target;}
            while(routeIndex<route.Count && Vector2.Distance(Position,route[routeIndex])<.35f)routeIndex++;
            // Look past cell centres whenever the corridor permits a smooth arc.
            for(int i=routeIndex+1;i<route.Count;i++){if(cave.Blocked(Position,route[i],.46f))break;routeIndex=i;}
            return routeIndex<route.Count?route[routeIndex]:Position;
        }
        public void Tick(float dt)
        {
            if(parts.Count==0 || cave.game.spellcraft!=null && parts.Any(p=>cave.game.spellcraft.ImmobilizedV44(p.sprite)))return;repath-=dt;contact-=dt;lungeCooldown-=dt;lungeTime=Mathf.Max(0,lungeTime-dt);patrolClock-=dt;
            var hero=cave.game.body.position;var delta=hero-Position;bool seen=!cave.game.Dead && delta.magnitude<32*(cave.game.spellcraft?.Vision(parts[0].sprite)??1);
            if(seen){lastSeen=14;lastGoal=hero;}else lastSeen=Mathf.Max(0,lastSeen-dt);
            Hunting=seen || lastSeen>0;Vector2 target=lastGoal;
            if(seen)
            {
                // Lead a moving player only on clear ground; never aim through a wall.
                var lead=hero+cave.game.body.linearVelocity*.55f;
                if(cave.Walkable(lead,.45f) && !cave.Blocked(hero,lead,.40f))target=lead;
                if(lungeCooldown<=0 && delta.magnitude>1.6f && delta.magnitude<7 && !cave.Blocked(Position,target,.50f))
                {lungeTime=.80f;lungeCooldown=1.7f;}
            }
            if(!Hunting)
            {
                if(patrolClock<=0 || Vector2.Distance(Position,patrolGoal)<.6f)
                {patrolClock=4.5f;float a=cave.clock*.39f+parts[0].seed*1.2f;var p=CatacombLayout.Rooms.Last()+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*5.3f;if(cave.Walkable(p,.5f))patrolGoal=p;else patrolGoal=Position;}
                target=patrolGoal;
            }
            var goal=Navigate(target);var desired=goal-Position;
            if(desired.sqrMagnitude>.04f)
            {
                float current=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg,wanted=Mathf.Atan2(desired.y,desired.x)*Mathf.Rad2Deg;
                float angle=Mathf.MoveTowardsAngle(current,wanted,dt*(Lunging?140:180)*SpeedFactor)*Mathf.Deg2Rad;
                var heading=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                speed=Mathf.MoveTowards(speed,(Lunging?5.8f:Hunting?3.7f:1.25f)*SpeedFactor,dt*7*SpeedFactor);
                var move=cave.Step(Position,heading*Mathf.Min(dt*speed,desired.magnitude),.46f);
                if(move.sqrMagnitude<.0000000001f)
                {lungeTime=0;move=cave.Step(Position,desired.normalized*dt*speed*.6f,.46f);}
                if(move.sqrMagnitude>.0000000001f)
                {
                    direction=Vector2.Lerp(direction,move.normalized,1-Mathf.Exp(-dt*12)).normalized;
                    parts[0].sprite.transform.position+=(Vector3)move;
                    // Retain the last sampled point: replacing it every short step prevented the trail from advancing at high FPS.
                    if(trail.Count==0 || Vector2.Distance(trail[0],Position)>=.045f)trail.Insert(0,Position);
                }
            }
            else speed=Mathf.MoveTowards(speed,0,dt*8);
            float total=0;int segment=1;var previous=Position;
            for(int i=0;i<trail.Count && segment<parts.Count;i++)
            {
                float length=Vector2.Distance(previous,trail[i]);
                while(segment<parts.Count && total+length>=segment*Spacing)
                {
                    var point=Vector2.Lerp(previous,trail[i],(segment*Spacing-total)/Mathf.Max(.0001f,length));
                    var tangent=(previous-trail[i]).normalized;var wave=new Vector2(-tangent.y,tangent.x)*(.045f*Mathf.Sin(cave.clock*4.5f-segment*.9f));
                    if(cave.Walkable(point+wave,.45f))point+=wave;
                    parts[segment].sprite.transform.position=point;segment++;
                }
                total+=length;previous=trail[i];
            }
            if(trail.Count>700)trail.RemoveRange(700,trail.Count-700);
            for(int i=parts.Count-1;i>=0;i--)
            {
                var p=parts[i];var aim=i==0?direction:i<parts.Count-1?parts[i-1].Position-parts[i+1].Position:parts[i-1].Position-p.Position;
                float old=p.sprite.transform.eulerAngles.z-(i==parts.Count-1 && i>0?180:0);
                float turn=Mathf.MoveTowardsAngle(old,Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg,dt*210)*Mathf.Deg2Rad;
                p.Pose(new Vector2(Mathf.Cos(turn),Mathf.Sin(turn)),cave.clock*SpeedFactor,i==0,i==parts.Count-1 && i>0);
            }
            if(contact<=0 && !cave.game.Dead)
            {
                var touching=parts.FirstOrDefault(p=>Vector2.Distance(p.Position,hero+Vector2.up*.38f)<1.35f && !cave.Blocked(p.Position,hero,.03f) && cave.game.art.SpriteContact(p.sprite,cave.game.hero,0));
                bool struck=false;if(touching!=null){cave.game.HurtPlayer(touching.IsHead?20:10);struck=true;}
                var mage=cave.game.journey.companion;if(mage.Hired && mage.Alive){var hit=parts.FirstOrDefault(p=>Vector2.Distance(p.Position,mage.Position+Vector2.up*.38f)<1.35f && !cave.Blocked(p.Position,mage.Position,.03f) && cave.game.art.SpriteContact(p.sprite,mage.sprite,0));if(hit!=null){mage.Hurt(hit.IsHead?20:10);struck=true;}}
                if(struck)contact=.65f;
            }
        }
    }
}
