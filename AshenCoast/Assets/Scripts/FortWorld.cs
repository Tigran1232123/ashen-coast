using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public enum BanditRole { Warrior, Archer, Mage }
    public sealed class FortTorch { public Vector2 point;public SpriteRenderer flame;public float phase; }
    public sealed class BanditArrow { public LineRenderer line;public Vector2 position,velocity;public float age; }
    public sealed class FortWorld : MonoBehaviour
    {
        public CoastGame game;
        public ThreatArt art;
        public GameObject root;
        public SpriteRenderer exterior,exit,descent;
        readonly HashSet<Collider2D> solids=new HashSet<Collider2D>();
        readonly RaycastHit2D[] castHits=new RaycastHit2D[128];
        readonly ContactFilter2D solidFilter=new ContactFilter2D{useTriggers=false};
        static readonly float[] turnAngles={0,25,-25,50,-50,80,-80,110,-110};
        public readonly List<FortBandit> bandits=new List<FortBandit>();
        public readonly List<FortRat> rats=new List<FortRat>();
        public readonly List<FortTorch> torches=new List<FortTorch>();
        public readonly List<Collider2D> stoneColliders=new List<Collider2D>();
        public readonly List<BanditArrow> arrows=new List<BanditArrow>();
        public Vector2 Position=>WorldShape.FortPosition;
        public Vector2 ExitPoint=>new Vector2(0,-14.2f);
        public static readonly Rect FloorBounds=Rect.MinMaxRect(-30,-22,30,26);
        public bool Inside=>game.journey.Area==WorldArea.Fort && !game.magic.InPocket;
        public float clock;
        float doorCooldown;
        AudioSource ambience,effects;
        AudioClip bowSound,stoneWind;
        public void Initialize(CoastGame owner)
        {
            game=owner;art=new ThreatArt(game.art);root=new GameObject("Ruined stone fortress / bandit hideout");root.SetActive(false);
            Exterior();Interior();CreateSounds();
            Vector2[] soldiers={new Vector2(-7,-6),new Vector2(6,-5),new Vector2(-13,-1),new Vector2(12,0),new Vector2(-5,2),new Vector2(5,4),new Vector2(-14,8),new Vector2(13,10),new Vector2(-4,10),new Vector2(3,12)};
            foreach(var point in soldiers)bandits.Add(new FortBandit(this,BanditRole.Warrior,point,bandits.Count));
            Vector2[] archers={new Vector2(-15,4),new Vector2(15,5),new Vector2(-10,12),new Vector2(10,13),new Vector2(0,6)};
            foreach(var point in archers)bandits.Add(new FortBandit(this,BanditRole.Archer,point,bandits.Count));
            bandits.Add(new FortBandit(this,BanditRole.Mage,new Vector2(0,13),bandits.Count));
            Vector2[] nests={new Vector2(-12,-8),new Vector2(13,-7),new Vector2(-15,13),new Vector2(14,11),new Vector2(6,1),new Vector2(-5,5)};
            for(int i=0;i<nests.Length;i++)rats.Add(new FortRat(this,nests[i],i));
        }
        SpriteRenderer Prop(string name,Sprite image,Vector2 at,float width,Transform parent,int? order=null)
        {
            var sr=game.art.Renderer(name,image,at,order??CoastGame.Depth(at.y));sr.transform.localScale=Vector3.one*(width/image.bounds.size.x);sr.transform.SetParent(parent,true);game.art.PixelStyle(sr);
            var block=new MaterialPropertyBlock();sr.GetPropertyBlock(block);block.SetFloat("_AlphaCut",.36f);sr.SetPropertyBlock(block);return sr;
        }
        void Box(string name,Vector2 centre,Vector2 size,Transform parent,bool interior=true)
        {var o=new GameObject(name);o.transform.SetParent(parent,false);o.transform.position=centre;var c=o.AddComponent<BoxCollider2D>();c.size=size;if(interior){stoneColliders.Add(c);solids.Add(c);}}
        SpriteRenderer Stone(string name,Sprite image,Vector2 at,float width)
        {
            var sr=Prop(name,image,at,width,root.transform);game.art.AddStoneFootprints(sr,.55f);
            foreach(var c in sr.GetComponentsInChildren<Collider2D>()){stoneColliders.Add(c);solids.Add(c);}return sr;
        }
        void Exterior()
        {
            exterior=Prop("Ruined fortress gateway and towers",art.exterior,Position,16,game.coastRoot.transform);
            // This arch-shaped shadow masks grass behind the empty facade while leaving the old ground untouched.
            var mouth=new GameObject("Dark underground throat inside the gateway");mouth.transform.SetParent(game.coastRoot.transform,false);mouth.transform.position=Position;
            var throat=new Mesh{name="Gateway inner shadow silhouette"};
            throat.vertices=new[]{new Vector3(-1.5f,.05f,0),new Vector3(1.5f,.05f,0),new Vector3(1.5f,3.8f,0),new Vector3(.9f,4.7f,0),new Vector3(0,5.35f,0),new Vector3(-.9f,4.7f,0),new Vector3(-1.5f,3.8f,0)};
            throat.uv=new[]{Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero};throat.colors=Enumerable.Repeat(new Color(.017f,.023f,.026f,1),7).ToArray();
            throat.triangles=new[]{0,1,2,0,2,3,0,3,4,0,4,5,0,5,6};throat.RecalculateBounds();mouth.AddComponent<MeshFilter>().sharedMesh=throat;
            var throatRenderer=mouth.AddComponent<MeshRenderer>();throatRenderer.sharedMaterial=game.art.lit;throatRenderer.sortingOrder=exterior.sortingOrder-2;
            descent=Prop("Sunken stairway down into fort dungeon",art.descent,Position+new Vector2(0,-.35f),3.5f,game.coastRoot.transform,exterior.sortingOrder-1);
            game.art.AddStoneFootprints(exterior,.65f);
            Box("Descent west parapet",Position+new Vector2(-1.40f,.6f),new Vector2(.28f,1.6f),game.coastRoot.transform,false);
            Box("Descent east parapet",Position+new Vector2(1.40f,.6f),new Vector2(.28f,1.6f),game.coastRoot.transform,false);
            var shade=game.Shadow("Fortress foundation shadow",Position+Vector2.up*.6f,new Vector2(16,2.1f),.36f);shade.transform.SetParent(game.coastRoot.transform,true);
            Box("Fort exterior west tower base",Position+new Vector2(-5.2f,1.35f),new Vector2(3.7f,2.1f),game.coastRoot.transform,false);
            Box("Fort exterior east tower base",Position+new Vector2(5.2f,1.35f),new Vector2(3.7f,2.1f),game.coastRoot.transform,false);
            Box("Fort exterior rear footprint",Position+new Vector2(0,6.2f),new Vector2(16,1.0f),game.coastRoot.transform,false);
        }
        void Interior()
        {
            var floor=GameObject.CreatePrimitive(PrimitiveType.Quad);floor.name="Worn damp stone courtyard flagstones";floor.transform.SetParent(root.transform,false);floor.transform.position=new Vector3(FloorBounds.center.x,FloorBounds.center.y,5);floor.transform.localScale=new Vector3(FloorBounds.width,FloorBounds.height,1);Destroy(floor.GetComponent<Collider>());
            var material=new Material(Shader.Find("AshenCoast/FortStone"));material.SetTexture("_MainTex",art.stone);floor.GetComponent<MeshRenderer>().sharedMaterial=material;
            Box("Fort north retaining wall",new Vector2(0,17.0f),new Vector2(40,.6f),root.transform);
            Box("Fort west retaining wall",new Vector2(-20,1.5f),new Vector2(.6f,32),root.transform);Box("Fort east retaining wall",new Vector2(20,1.5f),new Vector2(.6f,32),root.transform);
            Box("Fort south wall west",new Vector2(-11,-15),new Vector2(18,.6f),root.transform);Box("Fort south wall east",new Vector2(11,-15),new Vector2(18,.6f),root.transform);
            for(int i=0;i<6;i++)Stone("Carved ruined north stone masonry",art.masonry[0],new Vector2(-16.7f+i*6.7f,16.7f),6.9f);
            for(int side=-1;side<=1;side+=2)
            {
                for(int y=-12;y<=12;y+=6)Stone("Broken side fortress wall",art.masonry[3],new Vector2(side*19.1f,y),3.3f);
                Stone("Crumbling corner tower",art.masonry[1],new Vector2(side*16.8f,12.6f),4.8f);
                Stone("Entrance ruined tower",art.masonry[1],new Vector2(side*17.8f,-14.3f),3.9f);
            }
            exit=Stone("Open fort gate to flower meadow",art.masonry[2],new Vector2(0,-15.2f),4.1f);
            // Two ruined rooms at the sides; all openings remain wide enough for bodies and dashes.
            for(int side=-1;side<=1;side+=2)
            {
                float x=side*9.4f;
                Box("Stone partition lower",new Vector2(x,-8.8f),new Vector2(.65f,6.5f),root.transform);
                Box("Stone partition upper",new Vector2(x,5.8f),new Vector2(.65f,9),root.transform);
                for(int section=0;section<3;section++)Stone("Ruined partition south",art.masonry[3],new Vector2(x,-12.0f+section*2.65f),3.1f);
                Stone("Collapsed chamber lintel",art.masonry[5],new Vector2(x,0),4.0f);
                for(int section=0;section<4;section++)Stone("Old chamber masonry",art.masonry[3],new Vector2(x,1.5f+section*2.65f),3.3f);
            }
            Vector2[] pillars={new Vector2(-4,-2),new Vector2(4,-2),new Vector2(-4,7),new Vector2(4,7)};
            foreach(var p in pillars){Stone("Weathered carved stone pillar",art.masonry[4],p,1.0f);Box("Column ground footprint",p+Vector2.up*.1f,new Vector2(.65f,.55f),root.transform);}
            Furniture(10,new Vector2(-15,-5.2f),1.8f,new Vector2(1.5f,.65f));Furniture(10,new Vector2(14,-3.0f),1.8f,new Vector2(1.5f,.65f));
            Furniture(11,new Vector2(-16,2),2.3f,new Vector2(1.8f,.65f));Furniture(11,new Vector2(15,7.5f),2.0f,new Vector2(1.7f,.65f));
            Furniture(12,new Vector2(0,15.1f),3.0f,new Vector2(2.6f,1.1f));
            Furniture(14,new Vector2(-16,-10),2.1f,new Vector2(1.8f,.7f));Furniture(14,new Vector2(15,-10),2.0f,new Vector2(1.7f,.7f));
            Furniture(15,new Vector2(16,1),2.7f,new Vector2(2.2f,.9f));
            foreach(var p in new[]{new Vector2(-14,-7),new Vector2(14,-6),new Vector2(-13,10),new Vector2(13,13)})Prop("Bandit sleeping bedrolls",art.masonry[13],p,2.3f,root.transform,-1750);
            for(int i=0;i<14;i++)
            {
                float x=(i%2==0?-1:1)*(11.5f+(i%4)*1.65f),y=-11+(i*7%26);
                Prop("Loose broken stones and moss",art.masonry[6],new Vector2(x,y),.9f+(i%3)*.27f,root.transform);
            }
            Prop("Ancient worn gate steps",art.masonry[7],new Vector2(0,-12.1f),3.3f,root.transform,-1790);
            Vector2[] lights={new Vector2(-17,-11),new Vector2(17,-11),new Vector2(-17,0),new Vector2(17,0),new Vector2(-17,10),new Vector2(17,10),new Vector2(-6,14.5f),new Vector2(6,14.5f),new Vector2(-6,-7),new Vector2(6,4),new Vector2(0,-10),new Vector2(0,-2),new Vector2(0,8)};
            for(int i=0;i<lights.Length;i++)
            {
                var p=lights[i];Prop("Iron torch in old stone bracket",art.masonry[i<8?8:9],p,i<8?.6f:.85f,root.transform);
                var flame=game.art.Renderer("Fort torch flame",game.art.flame[i%16],p+Vector2.up*(i<8?1.15f:.55f),CoastGame.Depth(p.y)+3);flame.transform.SetParent(root.transform,true);
                var emission=new Material(game.art.lit);emission.SetFloat("_Emission",1);flame.sharedMaterial=emission;
                torches.Add(new FortTorch{point=(Vector2)flame.transform.position,flame=flame,phase=i*1.31f});
            }
        }
        void Furniture(int image,Vector2 p,float width,Vector2 footprint)
        {Prop("Fort supplies / "+image,art.masonry[image],p,width,root.transform);Box("Supply obstacle / "+image,p+Vector2.up*.2f,footprint,root.transform);}
        public Vector2 Clamp(Vector2 p)=>new Vector2(Mathf.Clamp(p.x,-19.4f,19.4f),Mathf.Clamp(p.y,-14.5f,16.3f));
        public SpriteRenderer InteractionAt(Vector2 point)=>CanInteractAt(point)?Inside?exit:descent:null;
        public bool CanInteractAt(Vector2 point)
        {
            if(game.magic.InPocket || game.journey.Cutscene)return false;
            if(game.journey.IsCoast)return Vector2.Distance(game.body.position,Position)<3.4f && descent.bounds.Contains(new Vector3(point.x,point.y,0));
            return Inside && Vector2.Distance(game.body.position,ExitPoint)<2.1f && exit.bounds.Contains(new Vector3(point.x,point.y,0));
        }
        public bool Interact(Vector2 point){if(!CanInteractAt(point))return false;if(Inside)Leave();else Enter();return true;}
        public void Enter(){ClearProjectiles();game.interactions.Pulse(descent);game.journey.SetArea(WorldArea.Fort,new Vector2(0,-12.8f));doorCooldown=1.0f;game.interactions.Pulse(exit);foreach(var enemy in bandits)enemy.ClearAggro();}
        public void Leave(){ClearProjectiles();game.interactions.Pulse(exit);game.journey.SetArea(WorldArea.Coast,Position+Vector2.down*1.6f);doorCooldown=1.2f;game.interactions.Pulse(descent);}
        void Update(){if(game==null || game.testMode || game.paused)return;Tick(Time.deltaTime);}
        public void Tick(float dt)
        {
            clock+=dt;doorCooldown=Mathf.Max(0,doorCooldown-dt);
            ambience.volume=Inside?.055f:0;
            if(!game.magic.InPocket && !game.journey.Cutscene && !game.Dead && doorCooldown<=0 && !game.magic.Teleporting)
            {if(game.journey.IsCoast && Vector2.Distance(game.body.position,Position)<.60f){Enter();return;}if(Inside && Vector2.Distance(game.body.position,ExitPoint)<.45f){Leave();return;}}
            if(!Inside)return;
            foreach(var torch in torches)
            {
                torch.flame.sprite=game.art.flame[(Mathf.FloorToInt(clock*23)+Mathf.FloorToInt(torch.phase*3))%16];
                float height=.46f+.065f*Mathf.Sin(clock*8+torch.phase);torch.flame.transform.localScale=Vector3.one*(height/torch.flame.sprite.bounds.size.y);game.art.PixelStyle(torch.flame);
            }
            if(!game.SimulateWorldV49)return;foreach(var rat in rats)rat.Tick(dt);
            if(!game.Dead)foreach(var enemy in bandits)game.TickCoopActorV49(enemy.Position,()=>enemy.Tick(dt));
            TickArrows(dt);
        }
        public bool Blocked(Vector2 from,Vector2 to,float radius=.23f)
        {
            var d=to-from;if(d.sqrMagnitude<.000001f)return false;
            int count=Physics2D.CircleCast(from+Vector2.up*.12f,radius,d.normalized,solidFilter,castHits,d.magnitude);
            if(count==castHits.Length)return true;
            for(int i=0;i<count;i++)if(castHits[i].collider && solids.Contains(castHits[i].collider))return true;
            return false;
        }
        public Vector2 Step(Vector2 from,Vector2 delta,float radius=.23f,bool steer=true)
        {
            if(delta.sqrMagnitude<.000001f)return Vector2.zero;
            int turns=steer?turnAngles.Length:1;
            for(int index=0;index<turns;index++)
            {
                var step=(Vector2)(Quaternion.Euler(0,0,turnAngles[index])*delta);var to=Clamp(from+step);
                if((to-(from+step)).sqrMagnitude>.0001f || Blocked(from,to,radius))continue;
                return step;
            }
            return Vector2.zero;
        }
        public void ClearProjectiles()
        {foreach(var arrow in arrows){Destroy(arrow.line.sharedMaterial);Destroy(arrow.line.gameObject);}arrows.Clear();}
        public void Shoot(Vector2 origin,Vector2 aim)
        {
            var o=new GameObject("Bandit arrow in flight");o.transform.SetParent(root.transform,false);var line=o.AddComponent<LineRenderer>();line.sharedMaterial=new Material(Shader.Find("Sprites/Default"));line.startWidth=.042f;line.endWidth=.018f;line.positionCount=3;line.startColor=new Color(.56f,.45f,.31f);line.endColor=new Color(.84f,.84f,.71f);line.sortingOrder=CoastGame.Depth(origin.y)+4;
            arrows.Add(new BanditArrow{line=line,position=origin,velocity=aim.normalized*12.5f});effects.PlayOneShot(bowSound,.21f);
        }
        void TickArrows(float dt)
        {
            for(int i=arrows.Count-1;i>=0;i--)
            {
                var arrow=arrows[i];var end=arrow.position+arrow.velocity*dt;arrow.age+=dt;
                bool stop=arrow.age>2.4f || Blocked(arrow.position,end,.045f);
                if(!stop)game.TickCoopActorV49((arrow.position+end)*.5f,()=>{if(SegmentDistance(game.body.position+Vector2.up*.6f,arrow.position,end)<.33f){game.HurtPlayer(22);stop=true;}});
                var companion=game.journey.companion;if(!stop && companion.Hired && companion.Alive && SegmentDistance(companion.Position+Vector2.up*.6f,arrow.position,end)<.33f){companion.Hurt(22);stop=true;}
                if(stop){Destroy(arrow.line.sharedMaterial);Destroy(arrow.line.gameObject);arrows.RemoveAt(i);continue;}
                arrow.position=end;var direction=arrow.velocity.normalized;arrow.line.SetPositions(new[]{(Vector3)(end-direction*.62f),(Vector3)(end-direction*.12f),(Vector3)end});arrow.line.sortingOrder=CoastGame.Depth(end.y)+5;
            }
        }
        public static float SegmentDistance(Vector2 p,Vector2 a,Vector2 b)
        {var d=b-a;return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/Mathf.Max(.000001f,d.sqrMagnitude)));}
        public int AppendLights(Vector4[] p,Vector4[] c,int n)
        {
            if(!Inside)return n;
            foreach(var torch in torches.OrderBy(t=>(t.point-game.body.position).sqrMagnitude))if(n<p.Length)
            {p[n]=new Vector4(torch.point.x,torch.point.y,6.2f,1.0f+.12f*Mathf.Sin(clock*8+torch.phase));c[n++]=new Vector4(1,.47f,.18f,1);}
            foreach(var mage in bandits.Where(b=>b.Alive && b.role==BanditRole.Mage))if(n<p.Length){var point=mage.Position+Vector2.up*.9f;p[n]=new Vector4(point.x,point.y,3.3f,.9f+.12f*Mathf.Sin(clock*9));c[n++]=new Vector4(1,.42f,.15f,1);}
            return n;
        }
        void CreateSounds()
        {
            effects=gameObject.AddComponent<AudioSource>();ambience=gameObject.AddComponent<AudioSource>();ambience.loop=true;ambience.volume=0;
            int rate=22050;var random=new System.Random(711);var wind=new float[rate*8];
            float low=0;for(int i=0;i<wind.Length;i++){float t=i/(float)rate;low=low*.98f+((float)random.NextDouble()*2-1)*.02f;wind[i]=low*.8f+Mathf.Sin(t*17)*.009f*Mathf.Sin(t*.77f)+Mathf.Sin(t*39)*.003f;}
            stoneWind=AudioClip.Create("Wind through hollow ruined stone and faint embers",wind.Length,1,rate,false);stoneWind.SetData(wind,0);ambience.clip=stoneWind;ambience.Play();
            var bow=new float[rate/4];for(int i=0;i<bow.Length;i++){float t=i/(float)rate;bow[i]=(Mathf.Sin(t*840)*.22f+Mathf.Sin(t*1550)*.09f+((float)random.NextDouble()*2-1)*.05f)*Mathf.Exp(-t*23);}
            bowSound=AudioClip.Create("Taut bow string release",bow.Length,1,rate,false);bowSound.SetData(bow,0);
        }
    }
    public sealed class FortBandit : ICombatTarget
    {
        public readonly FortWorld world;
        public readonly BanditRole role;
        public readonly SpriteRenderer sprite;
        public readonly CircleCollider2D feet;
        public float health=100;
        public bool Alive=>health>0;
        public SpriteRenderer Renderer=>sprite;
        public Vector2 Position=>(Vector2)sprite.transform.position;
        public bool Alert {get;private set;}
        public bool Dashing {get;private set;}
        public bool PreparingBlast=>role==BanditRole.Mage && attackTime>0 && attackTime<1.05f;
        public int frame;
        public int DashCount {get;private set;}
        public int BlastCount {get;private set;}
        public string Name=>role==BanditRole.Warrior?"Разбойник":role==BanditRole.Archer?"Лучник":"Маг с лампой";
        readonly Vector2 home;
        readonly int index;
        readonly LineRenderer warning;
        float clock,distance,attackTime,cooldown,dashCooldown,hitFlash,lampEmission;
        Vector2 aim=Vector2.down;
        bool attackDelivered;
        int attackKind;
        public FortBandit(FortWorld owner,BanditRole type,Vector2 at,int id)
        {
            world=owner;role=type;home=at;index=id;sprite=world.game.art.Renderer("Fort "+Name,world.art.bandits[(int)role,0],at,CoastGame.Depth(at.y));sprite.transform.SetParent(world.root.transform,true);
            feet=sprite.gameObject.AddComponent<CircleCollider2D>();feet.radius=.22f;feet.offset=Vector2.up*.13f;
            var marker=new GameObject("Bandit visible attack preparation");marker.transform.SetParent(world.root.transform,false);warning=marker.AddComponent<LineRenderer>();warning.sharedMaterial=new Material(Shader.Find("Sprites/Default"));warning.startWidth=warning.endWidth=.035f;warning.startColor=warning.endColor=new Color(1,.43f,.17f,.7f);warning.positionCount=0;
            cooldown=.7f+id*.05f;dashCooldown=1+id*.08f;Pose(false);
        }
        public void ClearAggro(){Alert=false;attackTime=0;Dashing=false;warning.positionCount=0;}
        public void Warp(Vector2 p){sprite.transform.position=p;Pose(false);Physics2D.SyncTransforms();}
        public void Tick(float dt)
        {
            if(!Alive || !world.Inside || world.game.spellcraft!=null && world.game.spellcraft.ImmobilizedV44(sprite))return;clock+=dt;cooldown=Mathf.Max(0,cooldown-dt);dashCooldown=Mathf.Max(0,dashCooldown-dt);hitFlash=Mathf.Max(0,hitFlash-dt*5);
            var game=world.game;var delta=game.body.position-Position;float range=delta.magnitude;
            if(game.spellcraft!=null && game.spellcraft.Vision(sprite)<1 && range>=5){Alert=false;attackTime=0;}
            if(range<10*(game.spellcraft?.Vision(sprite)??1) && !world.Blocked(Position,game.body.position,.08f))Alert=true;
            Vector2 moved=Vector2.zero;
            if(attackTime>0)
            {
                attackTime+=dt;
                if(attackKind==1)
                {
                    Dashing=attackTime>=.36f && attackTime<.66f;
                    if(Dashing){moved=world.Step(Position,aim*12*dt,.23f,false);sprite.transform.position+=(Vector3)moved;if(!attackDelivered && Vector2.Distance(Position,game.body.position)<1.15f && !world.Blocked(Position,game.body.position,.08f)){game.HurtPlayer(28);attackDelivered=true;}}
                    else if(attackTime<.36f){warning.positionCount=2;warning.SetPositions(new[]{(Vector3)Position,(Vector3)(Position+aim*3.6f)});}
                    if(attackTime>=.88f)FinishAttack(1.0f);
                }
                else if(attackKind==2)
                {
                    if(!attackDelivered && attackTime>=.62f){world.Shoot(Position+Vector2.up*.80f,aim);attackDelivered=true;}
                    if(attackTime>=.95f)FinishAttack(1.3f);
                }
                else if(attackKind==3)
                {
                    Ring(3.1f,attackTime/1.05f);
                    if(!attackDelivered && attackTime>=1.05f){game.arcane.CreateExplosion(Position,true);attackDelivered=true;BlastCount++;}
                    if(attackTime>=1.38f)FinishAttack(3.8f);
                }
                else
                {
                    if(!attackDelivered && attackTime>=.32f){if(range<1.55f && !world.Blocked(Position,game.body.position,.08f))game.HurtPlayer(18);attackDelivered=true;}
                    if(attackTime>=.7f)FinishAttack(.7f);
                }
            }
            else if(Alert)
            {
                if(role==BanditRole.Warrior)
                {
                    if(dashCooldown<=0 && range>2.0f && range<8 && !world.Blocked(Position,Position+delta.normalized*3.6f,.23f)){BeginAttack(1,delta);dashCooldown=4.0f+index*.06f;DashCount++;}
                    else if(range<1.25f && cooldown<=0)BeginAttack(0,delta);
                    else if(range>1.0f)moved=world.Step(Position,delta.normalized*2.25f*dt);
                }
                else if(role==BanditRole.Archer)
                {
                    if(range<4)moved=world.Step(Position,-delta.normalized*1.9f*dt);
                    else if(range>9 || world.Blocked(Position,game.body.position,.08f))moved=world.Step(Position,delta.normalized*1.7f*dt);
                    if(cooldown<=0 && range<13 && !world.Blocked(Position,game.body.position,.07f))BeginAttack(2,game.body.position+Vector2.up*.6f-(Position+Vector2.up*.8f));
                }
                else
                {
                    if(cooldown<=0 && range<4.1f)BeginAttack(3,delta);
                    else
                    {
                        if(range>4.4f)moved=world.Step(Position,delta.normalized*1.45f*dt);
                        if(range<6.1f && clock%2.8f>=.6f && clock%2.8f<1.85f && !world.Blocked(Position,game.body.position,.03f))
                        {lampEmission+=dt;while(lampEmission>=.035f){lampEmission-=.035f;game.arcane.EmitCone(Position+Vector2.up*.9f,game.body.position+Vector2.up*.6f-(Position+Vector2.up*.9f),true,3,FrogMage.FireReach,FrogMage.FireArc);}}
                        else lampEmission=0;
                    }
                }
                sprite.transform.position+=(Vector3)moved;
            }
            else
            {
                var target=home+new Vector2(Mathf.Sin(clock*.19f+index)*.6f,Mathf.Sin(clock*.24f+index)*.45f);
                if((target-Position).magnitude>.08f){moved=world.Step(Position,(target-Position).normalized*.45f*dt);sprite.transform.position+=(Vector3)moved;}
            }
            distance+=moved.magnitude;bool moving=moved.sqrMagnitude>.000001f;if(moving)aim=moved.normalized;
            Pose(moving);
        }
        void BeginAttack(int kind,Vector2 direction){attackKind=kind;attackTime=.001f;aim=direction.normalized;attackDelivered=false;}
        void FinishAttack(float wait){attackTime=0;cooldown=wait;Dashing=false;warning.positionCount=0;}
        void Ring(float radius,float charge)
        {
            warning.positionCount=49;warning.sortingOrder=CoastGame.Depth(Position.y)+5;
            warning.startColor=warning.endColor=new Color(1,.29f+.25f*charge,.13f,.35f+.4f*charge);
            for(int i=0;i<49;i++){float a=i*Mathf.PI*2/48;warning.SetPosition(i,Position+new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.6f));}
        }
        void Pose(bool moving)
        {
            frame=attackTime>0?Mathf.Min(7,Mathf.FloorToInt(attackTime/(attackKind==3?1.38f:.95f)*8)):moving?Mathf.FloorToInt(distance/1.5f*8)%8:0;
            sprite.sprite=attackTime>0?world.art.attacks[(int)role,frame]:world.art.bandits[(int)role,frame];sprite.flipX=aim.x>.15f;sprite.sortingOrder=CoastGame.Depth(Position.y);world.game.art.PixelStyle(sprite);sprite.color=Color.Lerp(Color.white,new Color(1,.46f,.33f),hitFlash);
        }
        public float Hurt(float amount)
        {
            if(!Alive)return 0;amount*=world.game.spellcraft?.DamageFactor(sprite)??1;float applied=Mathf.Min(health,Mathf.Max(0,amount));health-=applied;Alert=true;hitFlash=1;world.game.ShowDamage(applied,Position+Vector2.up*1.7f,sprite);
            if(!Alive){sprite.enabled=false;feet.enabled=false;warning.positionCount=0;world.game.CreateFortLoot(Position);}return applied;
        }
    }
    public sealed class FortRat
    {
        public readonly SpriteRenderer sprite;
        public bool Eating {get;private set;}
        public bool Fleeing {get;private set;}
        public Vector2 Position=>(Vector2)sprite.transform.position;
        readonly FortWorld world;
        readonly Vector2 home;
        readonly float seed;
        float clock,fleeUntil,distance;
        public FortRat(FortWorld owner,Vector2 point,int index)
        {
            world=owner;home=point;seed=index*2.71f;sprite=world.game.art.Renderer("Small fort rat sniffing food",world.art.rats[0,0],point,CoastGame.Depth(point.y));sprite.transform.SetParent(world.root.transform,true);sprite.transform.localScale=Vector3.one*(.46f/sprite.sprite.bounds.size.x);world.game.art.PixelStyle(sprite);
            for(int i=0;i<3;i++){var crumb=world.game.art.Renderer("Dropped food crumb",world.game.art.icons[2],point+new Vector2((i-1)*.11f,.13f),CoastGame.Depth(point.y)-20);crumb.transform.SetParent(world.root.transform,true);crumb.transform.localScale=Vector3.one*(.085f/crumb.sprite.bounds.size.x);world.game.art.PixelStyle(crumb);}
        }
        public void Tick(float dt)
        {
            clock+=dt;float range=Vector2.Distance(Position,world.game.body.position);if(range<3.25f)fleeUntil=clock+2.4f;Fleeing=clock<fleeUntil;Eating=!Fleeing && Mathf.Repeat(clock+seed,17)<4.1f && Vector2.Distance(Position,home)<.5f;
            Vector2 move=Vector2.zero;
            if(Fleeing)move=world.Step(Position,(Position-world.game.body.position+new Vector2(Mathf.Sin(seed)*.3f,.1f)).normalized*3.9f*dt,.10f);
            else if(!Eating){var goal=home+new Vector2(Mathf.Sin(clock*.8f+seed)*.5f,Mathf.Cos(clock*.65f+seed)*.3f);if(Vector2.Distance(goal,Position)>.07f)move=world.Step(Position,(goal-Position).normalized*.6f*dt,.10f);}
            sprite.transform.position+=(Vector3)move;distance+=move.magnitude;
            int row=Eating?2:Mathf.Abs(move.y)>Mathf.Abs(move.x) && move.y>0?1:0;int frame=Eating?Mathf.FloorToInt(clock*9)%8:move.sqrMagnitude>.000001f?Mathf.FloorToInt(distance/.38f*8)%8:0;
            sprite.sprite=world.art.rats[row,frame];sprite.flipX=move.x>0;sprite.sortingOrder=CoastGame.Depth(Position.y);world.game.art.PixelStyle(sprite);
        }
    }
}
