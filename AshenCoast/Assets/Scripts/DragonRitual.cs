using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed class DragonSummon
    {
        public Vector2 position;public Transform world;public float age;
        public SpriteRenderer pillar;public Material material;
    }
    public sealed class BurntPatch
    {
        public Vector2Int cell;public Transform world;public float remaining=10;
        public SpriteRenderer renderer;
    }
    public sealed class FallingDragonFire
    {
        public SpriteRenderer renderer;public Vector2 start,ground;public float age,life;public bool landed;public Transform world;
        public RoosterDragon source;
    }
    public sealed class DragonDust
    {public SpriteRenderer renderer;public Vector2 origin;public float age,seed;}
    public sealed class DragonRitual : MonoBehaviour
    {
        public CoastGame game;public DragonArt art;
        public readonly List<DragonSummon> summons=new List<DragonSummon>();
        public readonly List<RoosterDragon> bosses=new List<RoosterDragon>();
        public readonly List<BurntPatch> scorches=new List<BurntPatch>();
        public readonly List<FallingDragonFire> falling=new List<FallingDragonFire>();
        public readonly List<DragonDust> dust=new List<DragonDust>();
        public bool HasActiveBoss {get{foreach(var boss in bosses)if(boss.Active && boss.Alive)return true;return false;}}
        public bool InBattle {get{foreach(var boss in bosses)if(boss.Active && boss.Alive && !game.Dead && Vector2.Distance(boss.Position,game.body.position)<32)return true;return false;}}
        public int SpawnCount {get;private set;}
        public int GroundImpacts {get;private set;}
        public float clock;
        Material scorchMaterial;
        Sprite patchSprite;
        Sprite dustSprite;
        readonly Dictionary<SpriteRenderer,float> creatureFireTimes=new Dictionary<SpriteRenderer,float>();
        readonly HashSet<Vector2Int> burnedFlowerCells=new HashSet<Vector2Int>();
        readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
        readonly Dictionary<Transform,Dictionary<Vector2Int,BurntPatch>> burntCells=new Dictionary<Transform,Dictionary<Vector2Int,BurntPatch>>();
        float playerFireAt,companionFireAt;
        public void Initialize(CoastGame owner)
        {
            game=owner;art=new DragonArt(game.art);scorchMaterial=new Material(Shader.Find("AshenCoast/ScorchedGround"));
            var white=Texture2D.whiteTexture;
            patchSprite=Sprite.Create(white,new Rect(0,0,white.width,white.height),Vector2.one*.5f,white.width/2f);
            dustSprite=Sprite.Create(white,new Rect(0,0,white.width,white.height),Vector2.one*.5f,white.width);
        }
        public DragonSummon Schedule(Vector2 point,Transform world)
        {if(summons.Count>0 || bosses.Exists(b=>!b.Finished)){game.ui.Notice("Петух-дракон уже призван");return null;}if(!OpenTerrain(point,world)){game.ui.Notice("Для призыва нужна открытая местность");return null;}var summon=new DragonSummon{position=point,world=world};summons.Add(summon);return summon;}
        public bool OpenTerrain(Vector2 point,Transform world)
        {
            if(world==game.coastRoot.transform)return MeadowLayout.Walkable(point) && !game.art.Opaque(game.ship.sprite,game.ship.transform.InverseTransformPoint(point)) && !game.art.Opaque(game.fort.exterior.sprite,game.fort.exterior.transform.InverseTransformPoint(point)) && !game.art.Opaque(game.cave.entrance.sprite,game.cave.entrance.transform.InverseTransformPoint(point));
            if(world!=game.journey.fieldRoot.transform)return false;
            if(game.art.Opaque(game.journey.inn.sprite,game.journey.inn.transform.InverseTransformPoint(point)))return false;
            foreach(var tree in game.journey.trees)if(tree.sprite.bounds.Contains(new Vector3(point.x,point.y,tree.sprite.bounds.center.z)))return false;
            return true;
        }
        void Update(){if(!game || game.paused || game.testMode)return;Tick(Time.deltaTime);}
        public void Tick(float dt)
        {
            // Advance the bird and its moving fire together, including slow frames.
            if(dt<=0){Step(0);return;}
            float left=dt;while(left>.00001f){float step=Mathf.Min(1f/60,left);left-=step;Step(step);}
        }
        void Step(float dt)
        {
            clock+=dt;
            for(int i=summons.Count-1;i>=0;i--)
            {
                var s=summons[i];if(!s.world){DisposeSummon(s);summons.RemoveAt(i);continue;}
                s.age+=dt;
                if(s.age>=2 && !s.pillar)CreateColumn(s);
                if(s.pillar)PoseColumn(s);
                if(s.age>=5){Spawn(s.position,s.world);DisposeSummon(s);summons.RemoveAt(i);}
            }
            foreach(var boss in bosses)if(boss.Active)game.TickCoopActorV49(boss.Position,()=>boss.Tick(dt));
            for(int i=bosses.Count-1;i>=0;i--)if(bosses[i].Finished){bosses[i].Dispose();bosses.RemoveAt(i);}
            for(int i=scorches.Count-1;i>=0;i--)
            {
                var patch=scorches[i];patch.remaining-=dt;
                if(patch.remaining<=0 || !patch.world){if(patch.renderer)Destroy(patch.renderer.gameObject);ForgetBurn(patch);scorches.RemoveAt(i);continue;}
                patch.renderer.GetPropertyBlock(block);block.SetFloat("_Fade",Mathf.SmoothStep(0,1,Mathf.Clamp01(patch.remaining/1.1f)));patch.renderer.SetPropertyBlock(block);
            }
            TickFalling(dt);
            for(int i=dust.Count-1;i>=0;i--)
            {
                var mote=dust[i];mote.age+=dt;if(mote.age>=1.6f){Destroy(mote.renderer.gameObject);dust.RemoveAt(i);continue;}
                mote.renderer.transform.position=mote.origin+new Vector2(Mathf.Sin(mote.age*3+mote.seed)*.12f,mote.age*(.60f+mote.seed%1));
                mote.renderer.color=new Color(.66f,.59f,.48f,Mathf.Clamp01((1.6f-mote.age)/.6f)*.7f);
            }
        }
        void CreateColumn(DragonSummon summon)
        {
            summon.pillar=game.art.Renderer("Wide dragon invocation fire column",art.column[0],summon.position,CoastGame.Depth(summon.position.y)+12);
            summon.pillar.transform.SetParent(summon.world,true);summon.material=new Material(Shader.Find("AshenCoast/HearthFire"));summon.pillar.sharedMaterial=summon.material;
            // A fixed full canvas prevents width/height pumping between painted flame frames.
            summon.pillar.transform.localScale=new Vector3(4.7f/art.column[0].bounds.size.x,1,1);BurnArea(summon.position,4.3f,summon.world);
        }
        void PoseColumn(DragonSummon s)
        {
            float t=Mathf.Repeat((s.age-2)*1.1f,1)*24;int a=Mathf.FloorToInt(t),b=(a+1)%24;var texture=art.column[0].texture;
            Vector4 UV(Sprite frame){var r=frame.rect;return new Vector4(r.x/texture.width,r.y/texture.height,r.width/texture.width,r.height/texture.height);}
            Vector2 Pivot(Sprite frame)=>new Vector2(frame.pivot.x/frame.rect.width,frame.pivot.y/frame.rect.height);
            var anchor=Pivot(art.column[0]);s.pillar.GetPropertyBlock(block);
            float skyHeight=Mathf.Max(14,game.cameraView.transform.position.y+game.cameraView.orthographicSize-s.position.y+4);
            s.pillar.transform.localScale=new Vector3(4.7f/art.column[0].bounds.size.x,skyHeight/art.column[0].bounds.size.y,1);
            block.SetVector("_FrameA",UV(art.column[a]));block.SetVector("_FrameB",UV(art.column[b]));block.SetVector("_ShiftA",Pivot(art.column[a])-anchor);block.SetVector("_ShiftB",Pivot(art.column[b])-anchor);
            block.SetVector("_Pixels",new Vector4(188,skyHeight*40,0,0));block.SetFloat("_Blend",Mathf.SmoothStep(0,1,t-a));block.SetFloat("_HearthClock",(s.age-2)*6.9f);
            block.SetFloat("_SkyColumn",1);block.SetFloat("_SkyHeight",skyHeight);block.SetFloat("_SkyWidth",4.7f);s.pillar.SetPropertyBlock(block);
        }
        void DisposeSummon(DragonSummon s){if(s.pillar)Destroy(s.pillar.gameObject);if(s.material)Destroy(s.material);}
        public RoosterDragon Spawn(Vector2 position,Transform world)
        {
            if(bosses.Exists(b=>!b.Finished))return null;var boss=new RoosterDragon(this,position,world);bosses.Add(boss);SpawnCount++;game.PlaySwish();return boss;
        }
        public void Burn(Vector2 at,Transform world)
        {
            if(!world)return;
            if(world==game.coastRoot.transform && !WorldShape.Land(at))return;
            if(world==game.magic.pocket.transform && (Mathf.Abs(at.x)>4.3f || Mathf.Abs(at.y)>4.3f))return;
            var cell=WorldGrid.Cell(at);
            if(world==game.coastRoot.transform && burnedFlowerCells.Add(cell))BurnFlowers(cell);
            if(!burntCells.TryGetValue(world,out var cells)){cells=new Dictionary<Vector2Int,BurntPatch>();burntCells[world]=cells;}
            if(cells.TryGetValue(cell,out var existing)){existing.remaining=10;return;}
            if(scorches.Count>=768){ForgetBurn(scorches[0]);Destroy(scorches[0].renderer.gameObject);scorches.RemoveAt(0);}
            var sr=game.art.Renderer("Temporary scorched soil",patchSprite,WorldGrid.Snap(at),-2100);sr.transform.SetParent(world,true);sr.transform.localScale=new Vector3(.95f,.80f,1);sr.sharedMaterial=scorchMaterial;
            sr.GetPropertyBlock(block);block.SetFloat("_Fade",1);block.SetFloat("_Seed",at.x*8.73f+at.y*3.19f);sr.SetPropertyBlock(block);
            var patch=new BurntPatch{cell=cell,world=world,renderer=sr};scorches.Add(patch);cells[cell]=patch;
        }
        void BurnFlowers(Vector2Int cell)
        {
            foreach(var flower in game.flowers)
            {
                if(flower.picked || !flower.renderer.transform.IsChildOf(game.coastRoot.transform) || WorldGrid.Cell(flower.position)!=cell)continue;
                flower.picked=true;flower.renderer.enabled=false;flower.shadow.enabled=false;flower.stems.enabled=false;game.meadow.RecordPickup(flower);
                Evaporate(flower.position+Vector2.up*.35f,game.coastRoot.transform,5);
                if(game.hoveredFlower==flower)game.HighlightAt(null);
            }
        }
        public void Evaporate(Vector2 point,Transform world,int amount=2)
        {
            for(int i=0;i<amount;i++)
            {
                float seed=clock*17.73f+i*2.4f;
                var at=point+new Vector2(Mathf.Sin(seed)*.3f,Mathf.Cos(seed*.71f)*.12f);
                var sr=game.art.Renderer("Rising pixel ash",dustSprite,at,CoastGame.Depth(point.y)+9);sr.transform.SetParent(world,true);
                sr.transform.localScale=Vector3.one*(.035f+i%3*.012f);dust.Add(new DragonDust{renderer=sr,origin=at,seed=seed});
            }
        }
        void ForgetBurn(BurntPatch patch){if(burntCells.TryGetValue(patch.world,out var cells))cells.Remove(patch.cell);}
        public void BurnArea(Vector2 centre,float radius,Transform world)
        {
            var cell=WorldGrid.Cell(centre);int r=Mathf.CeilToInt(radius);
            for(int x=-r;x<=r;x++)for(int y=-r;y<=r;y++)
            {
                var p=new Vector2(cell.x+x+.5f,cell.y+y+.5f);var delta=p-centre;float angle=Mathf.Atan2(delta.y,delta.x);float edge=radius*(1+.10f*Mathf.Sin(angle*3+centre.x*.7f)+.06f*Mathf.Sin(angle*7+centre.y*.4f));if(delta.sqrMagnitude>edge*edge)continue;
                if(world==game.journey.ActiveRoot && game.WorldBlocked(centre,p,.04f))continue;Burn(p,world);
            }
        }
        public void BurnCone(Vector2 centre,Vector2 direction,float reach,Transform world)
        {
            var normal=new Vector2(-direction.y,direction.x);
            for(float d=.7f;d<=reach;d+=.75f)
            {
                float width=.8f+d*.40f;var middle=centre+direction*d;
                if(world==game.journey.ActiveRoot && game.WorldBlocked(centre,middle,.04f))break;
                for(float side=-width;side<=width;side+=.75f)BurnArea(middle+normal*side,.80f,world);
            }
        }
        public void FireFromAbove(Vector2 mouth,Vector2 point,Transform world,float flightTime=.16f,RoosterDragon source=null)
        {
            if(falling.Count>=80)return;
            var sr=game.arcane.EffectSprite("Dragon downward fire torrent",game.art.flame[0],mouth,CoastGame.Depth(point.y)+18);sr.transform.SetParent(world,true);
            falling.Add(new FallingDragonFire{renderer=sr,start=mouth,ground=point,life=flightTime,world=world,source=source});
            sr.transform.localScale=Vector3.one*(.4f/sr.sprite.bounds.size.y);game.art.PixelStyle(sr);
        }
        void TickFalling(float dt)
        {
            for(int i=falling.Count-1;i>=0;i--)
            {
                var fire=falling[i];if(!fire.world){Destroy(fire.renderer.gameObject);falling.RemoveAt(i);continue;}
                if(!fire.world.gameObject.activeInHierarchy)continue;fire.age+=dt;
                if(fire.age>=fire.life+.80f){Destroy(fire.renderer.gameObject);falling.RemoveAt(i);continue;}
                // Until impact this is a jet carried by the flying bird, rather
                // than a projectile left hanging at a previous mouth position.
                if(!fire.landed && fire.source!=null && fire.source.Active && fire.source.Alive)
                {
                    fire.start=fire.source.Mouth;
                    fire.ground=fire.source.FlightImpact;
                }
                float p=Mathf.Clamp01(fire.age/fire.life);fire.renderer.transform.position=Vector2.Lerp(fire.start,fire.ground,p);
                fire.renderer.sprite=game.art.flame[Mathf.FloorToInt(fire.age*30)%16];
                float size=p>=1?1.35f:.68f+p*.47f;fire.renderer.transform.localScale=Vector3.one*(size/fire.renderer.sprite.bounds.size.y);game.art.PixelStyle(fire.renderer);
                var delta=fire.ground-fire.start;fire.renderer.transform.rotation=Quaternion.Euler(0,0,p>=1?0:Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg-90);
                fire.renderer.sortingOrder=CoastGame.Depth(fire.ground.y)+(p>=1?3:28);
                fire.renderer.color=new Color(1,1,1,fire.landed?Mathf.Clamp01((fire.life+.80f-fire.age)/.42f):1);
                if(p>=1 && !fire.landed){fire.landed=true;GroundImpacts++;BurnArea(fire.ground,2.2f,fire.world);}
                if(fire.renderer.color.a>.18f)HurtByFireContact(fire.renderer,20,fire.source?.Renderer);
            }
        }
        public void HurtByFireContact(SpriteRenderer fire,float damage,SpriteRenderer source=null)
        {
            if(!source)source=bosses.FirstOrDefault(b=>b.Active && b.Alive)?.Renderer;
            if(!fire || !fire.enabled || !fire.gameObject.activeInHierarchy)return;
            bool contact=game.meditation.FireContact(fire);
            if(!game.Dead && clock>=playerFireAt && contact){game.HurtPlayer(damage);playerFireAt=clock+.40f;}
            foreach(var victim in game.WorldCreatures.ToArray())if(!(victim is HeroCombatActor) && !(victim is MageCombatActor) && !(victim is RoosterDragon) && victim.Alive && (!creatureFireTimes.TryGetValue(victim.Renderer,out float due) || clock>=due) && !game.WorldBlocked(fire.transform.position,victim.Position,.03f) && game.art.SpriteContact(fire,victim.Renderer,0)){game.HurtTarget(victim,damage,source);creatureFireTimes[victim.Renderer]=clock+.4f;}
            var mage=game.journey.companion;
            if(!mage.Hired || !mage.Alive)return;
            bool mageContact=game.art.SpriteContact(fire,mage.sprite,mage.gait.TorsoFloor) || (mage.gait.Lower.enabled && game.art.SpriteContact(fire,mage.gait.Lower,float.NegativeInfinity));
            if(mage.Hired && mage.Alive && clock>=companionFireAt && mageContact){mage.Hurt(damage);companionFireAt=clock+.4f;}
        }
        public int AppendLights(Vector4[] positions,Vector4[] colors,int count)
        {
            foreach(var summon in summons)if(summon.pillar && summon.world.gameObject.activeInHierarchy && count<positions.Length)
            {positions[count]=new Vector4(summon.position.x,summon.position.y+1,8.5f,1.45f+.22f*Mathf.Sin(clock*13));colors[count++]=new Vector4(1,.43f,.08f,1);}
            foreach(var boss in bosses)if(boss.Active && boss.Alive && boss.Breathing && count<positions.Length)
            {var p=boss.Mouth;positions[count]=new Vector4(p.x,p.y,3.9f,1.2f+.14f*Mathf.Sin(clock*19));colors[count++]=new Vector4(1,.39f,.08f,1);}
            foreach(var fire in falling)if(fire.world && fire.world.gameObject.activeInHierarchy && count<positions.Length)
            {var p=fire.renderer.transform.position;positions[count]=new Vector4(p.x,p.y,1.8f,.55f);colors[count++]=new Vector4(1,.40f,.08f,1);if(count>28)break;}
            return count;
        }
        public void Clear()
        {
            foreach(var s in summons)DisposeSummon(s);summons.Clear();foreach(var b in bosses)b.Dispose();bosses.Clear();
            foreach(var p in scorches)if(p.renderer)Destroy(p.renderer.gameObject);scorches.Clear();burntCells.Clear();burnedFlowerCells.Clear();foreach(var f in falling)if(f.renderer)Destroy(f.renderer.gameObject);falling.Clear();
            foreach(var p in dust)if(p.renderer)Destroy(p.renderer.gameObject);dust.Clear();
        }
        void OnDestroy(){Clear();if(scorchMaterial)Destroy(scorchMaterial);if(patchSprite)Destroy(patchSprite);if(dustSprite)Destroy(dustSprite);}
    }

    public enum DragonAction { Hunt,Peck,Claw,Breath,Flight,Dead }
    public sealed class RoosterDragon : ICombatTarget
    {
        public const float MaxHealth=1500,FlightInterval=20,FlightDuration=6.0f;
        readonly DragonRitual system;readonly CoastGame game;readonly Transform world;
        readonly Vector3 shadowScale;
        public SpriteRenderer Renderer {get;private set;}
        public SpriteRenderer shadow;
        public Vector2 Position {get;private set;}
        public bool Alive=>health>0;
        public bool Active=>Renderer && Renderer.gameObject.activeInHierarchy;
        public bool Finished=>!Alive && stateTime>fallDuration+11.5f;
        public bool DeathLanded {get;private set;}
        public int DroppedMeat {get;private set;}
        public float health=MaxHealth,age;
        public float Altitude {get;private set;}
        public DragonAction Action {get;private set;}
        public int Flights {get;private set;}
        public Vector2 FlightVelocity {get;private set;}
        public Vector2 FlightDirection=>aim;
        public Vector2 FlightImpact=>Mouth+aim*(Altitude+3.6f+Mathf.Min(FlightVelocity.magnitude,20)*.18f)-Vector2.up*Altitude;
        public bool Breathing=>Action==DragonAction.Breath && stateTime>.42f && stateTime<2.15f || Action==DragonAction.Flight && stateTime>.85f && stateTime<5.1f;
        public Vector2 Mouth
        {
            get
            {
                var p=system.art.Mouth(Renderer.sprite);
                if(Renderer.flipX)p.x=-p.x;return Renderer.transform.TransformPoint(p);
            }
        }
        Vector2 aim=Vector2.left,flyStart,flyTarget;
        float stateTime,nextAttack=1.0f,nextFlight=20,emit,damageAt,walkDistance;
        bool struck;int attackCounter,view;
        float deathAltitude,fallDuration=1.10f,dustClock;
        bool deathFlip;
        readonly MaterialPropertyBlock corpseProperties=new MaterialPropertyBlock();
        readonly RaycastHit2D[] hits=new RaycastHit2D[16];
        public RoosterDragon(DragonRitual owner,Vector2 at,Transform parent)
        {
            system=owner;game=owner.game;world=parent;Position=at;Action=DragonAction.Hunt;
            Renderer=game.art.Renderer("Hostile rooster-dragon",system.art.walk[0,0],at,CoastGame.Depth(at.y));Renderer.transform.SetParent(world,true);
            shadow=game.Shadow("Rooster-dragon ground shadow",at,new Vector2(2.4f,.85f),.52f);shadow.transform.SetParent(world,true);shadowScale=shadow.transform.localScale;Pose();
        }
        public float Hurt(float damage)
        {
            if(!Alive || damage<=0)return 0;damage*=game.spellcraft?.DamageFactor(Renderer)??1;float taken=Mathf.Min(health,damage);health=Mathf.Max(0,health-damage);
            game.ShowDamage(taken,Position+Vector2.up*(2.0f+Altitude),Renderer);
            if(!Alive){deathAltitude=Altitude;deathFlip=Renderer.flipX;Action=DragonAction.Dead;stateTime=0;fallDuration=1.1f+Mathf.Min(.3f,Altitude*.1f);FlightVelocity=Vector2.zero;}return taken;
        }
        public void Warp(Vector2 at){Position=at;Pose();}
        public void BeginAttack(DragonAction action,Vector2 direction)
        {
            if(!Alive)return;Action=action;stateTime=0;struck=false;emit=0;aim=direction.sqrMagnitude>.001f?direction.normalized:Vector2.left;
            if(action==DragonAction.Flight){Flights++;nextFlight=age+FlightInterval;flyStart=Position;flyTarget=game.WorldCreatures.Where(t=>t.Alive && t.Renderer!=Renderer).OrderBy(t=>(t.Position-Position).sqrMagnitude).Select(t=>t.Position).FirstOrDefault();FlightVelocity=Vector2.zero;}
            Pose();
        }
        public void Tick(float dt)
        {
            if(!Active || game.spellcraft!=null && game.spellcraft.ImmobilizedV44(Renderer))return;float left=dt;
            while(left>.00001f){float step=Mathf.Min(.035f,left);left-=step;Step(step);}Pose();
        }
        void Step(float dt)
        {
            age+=dt;stateTime+=dt;if(!Alive){TickDeath(dt);return;}
            var target=game.WorldCreatures.Where(t=>t.Alive && t.Renderer!=Renderer && t.Renderer.gameObject.activeInHierarchy && (game.spellcraft==null || game.spellcraft.Vision(Renderer)>=1 || Vector2.Distance(t.Position,Position)<=13)).OrderBy(t=>(t.Position-Position).sqrMagnitude).FirstOrDefault();
            Vector2 delta=(target?.Position??Position)-Position;float distance=delta.magnitude;
            if(age>=nextFlight && Action!=DragonAction.Flight){BeginAttack(DragonAction.Flight,delta);return;}
            if(Action==DragonAction.Hunt)
            {
                if(distance>1.9f && target!=null)
                {
                    var moved=Move(delta.normalized,dt*2.45f);Position+=moved;walkDistance+=moved.magnitude;
                    if(moved.sqrMagnitude>.00001f)aim=moved.normalized;
                }
                if(target!=null && age>=nextAttack && distance<13)
                {var action=distance>3.0f?DragonAction.Breath:(attackCounter++%2==0?DragonAction.Peck:DragonAction.Claw);BeginAttack(action,delta);}
                return;
            }
            if(Action==DragonAction.Peck || Action==DragonAction.Claw)
            {
                float duration=Action==DragonAction.Peck?.90f:1.08f;
                if(!struck && stateTime>=duration*.51f)
                {
                    Pose();struck=true;var contact=Action==DragonAction.Peck?Mouth-Vector2.up*.8f:Position+aim*1.05f;float reach=Action==DragonAction.Peck?.86f:1.07f;
                    foreach(var victim in game.WorldCreatures.ToArray())if(victim.Alive && victim.Renderer!=Renderer && Vector2.Distance(victim.Position,contact)<reach && Clear(Position,victim.Position))game.HurtTarget(victim,Action==DragonAction.Peck?32:44,Renderer);
                    game.PlaySwish();
                }
                if(stateTime>=duration)Rest(.75f);return;
            }
            if(Action==DragonAction.Breath)
            {
                if(stateTime>=.42f && stateTime<2.15f)
                {
                    Pose();
                    emit+=dt;
                    while(emit>=.036f)
                    {emit-=.036f;game.arcane.EmitCone(Mouth,aim,true,3,13,44,null,true,Renderer);}
                }
                if(stateTime>=2.5f)Rest(1.5f);return;
            }
            if(Action==DragonAction.Flight)
            {
                float ascent=Mathf.SmoothStep(0,1,Mathf.Clamp01(stateTime/.85f));float descent=Mathf.SmoothStep(0,1,Mathf.Clamp01((FlightDuration-stateTime)/.9f));
                Altitude=3.2f*Mathf.Min(ascent,descent)+Mathf.Sin(stateTime*8)*.08f*Mathf.Min(ascent,descent);
                if(stateTime>.85f && stateTime<5.1f)
                {
                    float p=(stateTime-.85f)/4.25f;var previous=Position;var path=Vector2.Lerp(flyStart,flyTarget,p)+new Vector2(Mathf.Sin(p*Mathf.PI*2)*4.2f,Mathf.Sin(p*Mathf.PI)*1.6f);
                    Position=Clamp(path);var direction=Position-previous;FlightVelocity=direction/dt;if(direction.sqrMagnitude>.00001f)aim=direction.normalized;Pose();
                    // The flame inherits the flight speed and lands ahead of the
                    // moving beak, rather than trailing behind a fast fly-by.
                    emit+=dt;while(emit>=.025f){emit-=.025f;system.FireFromAbove(Mouth,FlightImpact,world,.16f,this);}
                }
                if(stateTime>=FlightDuration){Altitude=0;Rest(1.1f);}return;
            }
        }
        void Rest(float pause){Action=DragonAction.Hunt;stateTime=0;nextAttack=age+pause;emit=0;damageAt=0;FlightVelocity=Vector2.zero;}
        void TickDeath(float dt)
        {
            if(!DeathLanded && stateTime>=fallDuration)
            {
                DeathLanded=true;Altitude=0;game.ShakeCamera();game.RoosterImpact();PoseDeath();
                for(int i=0;i<7;i++)
                {
                    float seed=Position.x*12.71f+Position.y*43.27f+stateTime*17.91f+i*37.13f;
                    float jitter=Mathf.Repeat(Mathf.Sin(seed)*43758.54f,1),angle=i*2.39996f+jitter*1.25f;
                    var direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));var centre=(Vector2)Renderer.bounds.center;float edge=0;
                    // Scatter just beyond the actual resting silhouette, including its spread wings.
                    for(float r=0;r<Renderer.bounds.extents.magnitude+.5f;r+=.04f)
                    {var local=(Vector2)Renderer.transform.InverseTransformPoint(centre+direction*r);if(Renderer.flipX)local.x=-local.x;if(Renderer.flipY)local.y=-local.y;foreach(var restingPose in system.art.defeat)if(game.art.Opaque(restingPose,local)){edge=r;break;}}
                    var destination=centre+direction*(edge+.32f+jitter*.28f);
                    destination=world==game.magic.pocket.transform?new Vector2(Mathf.Clamp(destination.x,-3.8f,3.8f),Mathf.Clamp(destination.y,-3.8f,3.8f)):world==game.coastRoot.transform?MeadowLayout.Clamp(destination):game.journey.Clamp(destination);
                    var drop=game.ScatterLoot(new Item(ItemKind.RawMeat),destination,Position+Vector2.up*.8f);
                    drop.renderer.transform.SetParent(world,true);drop.shadow.transform.SetParent(world,true);DroppedMeat++;
                }

            }
            if(stateTime>fallDuration+10)
            {
                dustClock+=dt;while(dustClock>.035f){dustClock-=.035f;float seed=stateTime*39.73f;system.Evaporate(Position+new Vector2(Mathf.Sin(seed)*2,1+Mathf.Repeat(seed*.7f,1)*2),world);}
            }
        }
        void PoseDeath()
        {
            float rested=Mathf.Max(0,stateTime-fallDuration);
            int frame=rested<.80f?Mathf.Min(7,Mathf.FloorToInt(rested*10)):8+Mathf.FloorToInt((rested-.80f)*5)%8;
            Renderer.sprite=DeathLanded?system.art.defeat[frame]:system.art.corpse;
            Renderer.flipX=DeathLanded?!deathFlip:deathFlip;Renderer.transform.localScale=Vector3.one;
            float p=Mathf.Clamp01(stateTime/fallDuration),roll=Mathf.SmoothStep(0,1,Mathf.Clamp01((p-.10f)/.68f));
            float angle=180*roll+Mathf.Sin(p*Mathf.PI*3)*13*(1-p);
            float lift=(deathAltitude+1.0f)*(1-Mathf.SmoothStep(0,1,Mathf.Clamp01((p-.22f)/.78f)))+Mathf.Sin(p*Mathf.PI)*.35f;
            if(DeathLanded){angle=Mathf.Sin(rested*27)*2*Mathf.Exp(-rested*9);lift=Mathf.Abs(Mathf.Sin(rested*17))*.09f*Mathf.Exp(-rested*9);}
            float fade=Mathf.Clamp01((stateTime-fallDuration-10)/1.5f);
            Renderer.transform.position=Position+Vector2.up*((DeathLanded?system.art.DefeatRestHeight:system.art.corpse.bounds.size.y*.5f)+lift+fade*.3f);
            Renderer.transform.rotation=Quaternion.Euler(0,0,angle);Renderer.sortingOrder=CoastGame.Depth(Position.y)+2;
            Renderer.color=new Color(1,1,1,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.75f,1,fade)));game.art.PixelStyle(Renderer);
            Renderer.GetPropertyBlock(corpseProperties);corpseProperties.SetFloat("_CorpseDissolve",fade);Renderer.SetPropertyBlock(corpseProperties);
            shadow.transform.position=Position;shadow.color=new Color(0,0,0,.35f*(1-fade));shadow.transform.localScale=shadowScale;
        }

        bool Clear(Vector2 start,Vector2 end)=>!game.WorldBlocked(start,end,.06f);
        Vector2 Clamp(Vector2 p)
        {
            if(game.magic.InPocket)return new Vector2(Mathf.Clamp(p.x,-2.7f,2.7f),Mathf.Clamp(p.y,-2.8f,2.8f));
            return game.journey.Clamp(p);
        }
        Vector2 Move(Vector2 direction,float distance)
        {
            for(int turn=0;turn<5;turn++)
            {
                float angle=turn==0?0:turn%2==1?turn*23:-(turn-1)*23;Vector2 d=Quaternion.Euler(0,0,angle)*direction;
                int n=Physics2D.CircleCastNonAlloc(Position+Vector2.up*.15f,.48f,d,hits,distance+.18f);bool blocked=false;
                for(int i=0;i<n;i++)if(hits[i].collider && !hits[i].collider.isTrigger && hits[i].collider.gameObject!=game.hero.gameObject){blocked=true;break;}
                if(!blocked){var target=Clamp(Position+d*distance);return target-Position;}
            }
            return Vector2.zero;
        }
        public void Pose()
        {
            if(!Alive){PoseDeath();return;}
            bool flip=aim.x>0;Sprite sprite;
            if(Action==DragonAction.Flight){sprite=system.art.flight[Mathf.FloorToInt(stateTime*20)%16];}
            else if(Action==DragonAction.Peck || Action==DragonAction.Claw || Action==DragonAction.Breath)
            {
                int row=Action==DragonAction.Breath?0:Action==DragonAction.Peck?1:2;float duration=row==1?.90f:row==2?1.08f:2.5f;
                int frame=row==0 && stateTime>.42f && stateTime<2.15f?2+Mathf.FloorToInt(stateTime*9)%3:Mathf.Min(7,Mathf.FloorToInt(stateTime/duration*8));sprite=system.art.attack[row,frame];
            }
            else
            {
                view=Mathf.Abs(aim.x)>Mathf.Abs(aim.y)*.55f?1:aim.y>0?2:0;int frame=Mathf.FloorToInt(walkDistance/.90f*8)%8;
                sprite=system.art.walk[view,frame];flip=view==1 && aim.x>0;
            }
            Renderer.sprite=sprite;Renderer.flipX=flip;
            Vector2 flightOffset=Vector2.zero;
            if(Action==DragonAction.Flight){flightOffset=system.art.Mouth(system.art.walk[1,0])-system.art.Mouth(sprite);if(flip)flightOffset.x=-flightOffset.x;}
            Renderer.transform.position=Position+Vector2.up*Altitude+flightOffset;
            Renderer.sortingOrder=CoastGame.Depth(Position.y)+(Action==DragonAction.Flight?35:2);
            float alpha=Alive?Mathf.Clamp01(age/.35f):Mathf.Clamp01(1-stateTime/1.5f);
            Renderer.color=new Color(1,1,1,alpha);Renderer.transform.localScale=Alive?Vector3.one:new Vector3(1,Mathf.Max(.20f,1-stateTime*.65f),1);game.art.PixelStyle(Renderer);
            shadow.transform.position=Position;shadow.color=new Color(0,0,0,Alive?Mathf.Lerp(.48f,.21f,Altitude/3.2f):alpha*.35f);
            shadow.transform.localScale=Vector3.Scale(shadowScale,new Vector3(Mathf.Lerp(1,.74f,Altitude/3.2f),Mathf.Lerp(1,.85f,Altitude/3.2f),1));
        }
        public void Dispose(){if(Renderer)Object.Destroy(Renderer.gameObject);if(shadow)Object.Destroy(shadow.gameObject);}
    }
}
