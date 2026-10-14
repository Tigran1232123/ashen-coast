using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed class FireMote
    {
        public SpriteRenderer renderer;
        public Vector2 origin,velocity;
        public float age,life,size,seed,reach;
        public bool hostile,damaging;
        public bool dragonFire;
        public SpriteRenderer targetOnly,source;
    }
    public sealed class CampfireObject
    {
        public Vector2 position;
        public SpriteRenderer logs;
        public readonly List<SpriteRenderer> flames=new List<SpriteRenderer>();
        public bool exploded;
        public float phase;
        public Collider2D collider;
        public Vector2Int Cell=>WorldGrid.Cell(position);
    }
    public sealed class FireExplosion
    {
        public Vector2 position;
        public SpriteRenderer renderer;
        public SpriteRenderer ring;
        public float age;
        public bool hit,hostile;public bool playerOwned=true;public int casterPeer=-1;
    }
    public sealed partial class ArcaneCombat : MonoBehaviour
    {
        public CoastGame game;
        public readonly List<FrogMage> frogs=new List<FrogMage>();
        public readonly List<CampfireObject> campfires=new List<CampfireObject>();
        public readonly List<FireMote> flames=new List<FireMote>();
        public readonly List<FireExplosion> explosions=new List<FireExplosion>();
        public readonly List<TentacleField> fields=new List<TentacleField>();
        public float clock;
        public int lastLightCount;
        public bool YoYoActive {get;private set;}
        public Vector2 YoYoPosition=>yoYo.transform.position;
        public bool Placing=>game.buildings!=null && game.buildings.Placing;
        public SpriteRenderer yoYo;
        LineRenderer stringLine;
        Vector2 yoVelocity,yoTarget;
        bool yoHeld;
        float lampBurst,emitRemainder;
        public readonly Material fireMaterial=new Material(Shader.Find("AshenCoast/WorldSprite"));
        readonly Dictionary<object,float> lastHit=new Dictionary<object,float>();
        public Transform WorldRoot=>game.magic!=null && game.magic.InPocket?game.magic.pocket.transform:game.journey!=null?game.journey.ActiveRoot:game.coastRoot.transform;
        public Vector2 PlayerHand=>(Vector2)game.hero.transform.position+new Vector2(game.facing>=1 && game.facing<=3?-.22f:game.facing>=5?.22f:0,.94f);
        public bool LampReady=>game.survivor.equipment[0]?.kind==ItemKind.Lamp && game.survivor.equipment[0].Usable && game.survivor.equipment[0].fuel>0 && game.survivor.mana>0;
        public bool UseLamp(float dt)
        {
            var lamp=game.survivor.equipment[0];if(!LampReady)return false;
            float time=Mathf.Min(dt,Mathf.Min(game.survivor.mana/3f,lamp.fuel/3.2f));game.survivor.SpendMana(time*3);lamp.fuel=Mathf.Max(0,lamp.fuel-time*3.2f);lamp.Wear(time*.035f);return time>0;
        }

        public void Initialize(CoastGame owner)
        {
            game=owner;fireMaterial.SetFloat("_Emission",1);
            yoYo=game.art.Renderer("Spinning battle yo-yo",game.art.icons[20],PlayerHand,0);yoYo.enabled=false;
            float scale=.492f/yoYo.sprite.bounds.size.y;yoYo.transform.localScale=Vector3.one*scale;game.art.PixelStyle(yoYo);
            var rope=new GameObject("Yo-yo taut string");stringLine=rope.AddComponent<LineRenderer>();stringLine.sharedMaterial=new Material(Shader.Find("Sprites/Default"));
            stringLine.startWidth=.025f;stringLine.endWidth=.018f;stringLine.startColor=new Color(.73f,.65f,.45f);stringLine.endColor=new Color(.55f,.43f,.27f);stringLine.positionCount=0;
            frogs.Add(new FrogMage(this,new Vector2(6.2f,5.9f)));
        }
        public bool HasOpenBook(ItemKind kind)=>game.survivor.OpenBook(kind)!=null;
        void Update()
        {
            if(game==null || game.paused || game.testMode)return;
            bool free=!game.MovementLocked && !game.Meditating && !game.ui.IsPointerOver && !game.ui.Dragging && !game.magic.Teleporting && !game.magic.Drawing;
            var held=game.survivor.equipment[0];
            if(!Placing && free && held?.kind==ItemKind.YoYo)
            {
                if(Input.GetMouseButton(0))SetYoYoTarget(game.CursorWorld,true);else yoHeld=false;
            }
            else yoHeld=false;
            if(held?.kind==ItemKind.Lamp && held.Usable)held.fuel=Mathf.Max(0,held.fuel-Time.deltaTime*.22f);
            bool fire=free && !Placing && held?.kind==ItemKind.Lamp && (Input.GetMouseButton(0) || lampBurst>0) && UseLamp(Time.deltaTime);
            if(fire)
            {
                game.facing=CoastGame.Facing(game.CursorWorld-PlayerHand);emitRemainder+=Time.deltaTime;
                while(emitRemainder>=.028f){emitRemainder-=.028f;EmitCone(PlayerHand,game.CursorWorld-PlayerHand,false,3);}
            }
            else emitRemainder=0;
            Tick(Time.deltaTime);
        }
        public void Tick(float dt)
        {
            clock+=dt;lampBurst=Mathf.Max(0,lampBurst-dt);
            if(!game.magic.InPocket && (game.journey==null || game.journey.IsCoast))foreach(var frog in frogs)game.TickCoopActorV49(frog.Position,()=>frog.Tick(dt));
            TickYoYo(dt);
            for(int i=flames.Count-1;i>=0;i--)
            {
                var f=flames[i];f.age+=dt;
                if(f.damaging && f.renderer.gameObject.activeInHierarchy && game.WorldBlocked(f.origin-Vector2.up*.8f,f.origin+f.velocity*f.age-Vector2.up*.8f,.03f)){Destroy(f.renderer.gameObject);flames.RemoveAt(i);continue;}
                if(f.age>=f.life){Destroy(f.renderer.gameObject);flames.RemoveAt(i);continue;}
                var normal=new Vector2(-f.velocity.y,f.velocity.x).normalized;
                var offset=f.velocity*f.age+normal*Mathf.Sin(f.age*15+f.seed)*.075f;
                offset=Vector2.ClampMagnitude(offset,Mathf.Max(0,f.reach-f.size));f.renderer.transform.position=f.origin+offset;
                int frame=(Mathf.FloorToInt(f.age*26)+Mathf.FloorToInt(f.seed))%16;f.renderer.sprite=game.art.flame[frame];
                float pulse=Mathf.Sin(Mathf.PI*Mathf.Clamp01(f.age/f.life));
                float height=f.size*(.45f+.55f*pulse);float scale=height/f.renderer.sprite.bounds.size.y;
                f.renderer.transform.localScale=new Vector3(scale,scale,1);game.art.PixelStyle(f.renderer);
                f.renderer.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(f.velocity.y,f.velocity.x)*Mathf.Rad2Deg-90+Mathf.Sin(f.seed+f.age*9)*14);
                f.renderer.sortingOrder=CoastGame.Depth(f.renderer.transform.position.y)+4;
                f.renderer.color=new Color(1,.86f+.14f*pulse,.78f+.22f*pulse,Mathf.Clamp01((f.life-f.age)/.18f)*.91f);
                if(f.dragonFire && f.age>.07f && f.renderer.gameObject.activeInHierarchy && f.renderer.color.a>.25f)
                {
                    // Scorch only the footprint reached by an actual visible tongue.
                    var ground=new Vector2(f.renderer.bounds.center.x,f.renderer.bounds.min.y+.12f);
                    if(!game.WorldBlocked(f.origin-Vector2.up*.8f,ground,.03f))game.dragons.BurnArea(ground,.85f,f.renderer.transform.parent);
                }
                if(f.damaging && f.age>.035f && f.renderer.gameObject.activeInHierarchy)DamageFlame(f);
            }
            foreach(var fire in campfires)if(!fire.exploded)
            {
                fire.phase+=dt;
                for(int i=0;i<fire.flames.Count;i++)
                {
                    var sprite=fire.flames[i];sprite.sprite=game.newArt.campfire[0];float scale=.82f/sprite.sprite.bounds.size.y;sprite.transform.localScale=Vector3.one*scale;game.art.PixelStyle(sprite);
                    sprite.transform.position=fire.position+Vector2.up*.14f;
                    float phase=Mathf.Repeat(fire.phase*30,40);int a=Mathf.FloorToInt(phase),b=(a+1)%40;var texture=sprite.sprite.texture;
                    Vector4 UV(Sprite s){var r=s.rect;return new Vector4(r.x/texture.width,r.y/texture.height,r.width/texture.width,r.height/texture.height);}
                    var block=new MaterialPropertyBlock();sprite.GetPropertyBlock(block);block.SetVector("_FrameA",UV(game.newArt.campfire[a]));block.SetVector("_FrameB",UV(game.newArt.campfire[b]));block.SetVector("_ShiftA",Vector4.zero);block.SetVector("_ShiftB",Vector4.zero);
                    block.SetVector("_Pixels",new Vector4(sprite.sprite.bounds.size.x*scale*40,.82f*40,0,0));block.SetFloat("_Blend",Mathf.SmoothStep(0,1,phase-a));block.SetFloat("_HearthClock",fire.phase*5);sprite.SetPropertyBlock(block);
                    var vessel=game.buildings.stations.FirstOrDefault(p=>p.Heated && p.fire==fire);block.SetFloat("_UnderVessel",vessel!=null?1:0);block.SetFloat("_VesselCeiling",fire.position.y+.31f);sprite.SetPropertyBlock(block);
                }
            }
            for(int i=explosions.Count-1;i>=0;i--)
            {
                var e=explosions[i];e.age+=dt;
                if(e.age>=1.45f){Destroy(e.renderer.gameObject);if(e.ring)Destroy(e.ring.gameObject);explosions.RemoveAt(i);continue;}
                PoseExplosion(e);
                if(!e.hit && e.age>=.28f && e.renderer.gameObject.activeInHierarchy){e.hit=true;if(e.hostile){if(Vector2.Distance(game.body.position,e.position)<3 && (!game.WorldBlocked(e.position,game.body.position,.03f)))game.HurtPlayer(55);if(game.journey.companion.Hired && Vector2.Distance(game.journey.companion.Position,e.position)<3 && (!game.WorldBlocked(e.position,game.journey.companion.Position,.03f)))game.journey.companion.Hurt(55);}else if(game.CoopOnline && e.casterPeer>=0)game.coop.ForNearest(e.position,()=>DamageArea(e.position,3,60,e.playerOwned),e.casterPeer);else DamageArea(e.position,3,60,e.playerOwned);}
            }
            TickBurstSparks(dt);
            for(int i=fields.Count-1;i>=0;i--){fields[i].Tick(dt);if(fields[i].Finished){fields[i].Dispose();fields.RemoveAt(i);}}
        }
        bool Ready(object target,float interval)
        {if(lastHit.TryGetValue(target,out var previous) && clock-previous<interval)return false;lastHit[target]=clock;return true;}
        void DamageFlame(FireMote flame)
        {
            if(flame.hostile)
            {
                if(flame.targetOnly && !flame.dragonFire)
                {
                    var victim=game.WorldCreatures.FirstOrDefault(t=>t.Renderer==flame.targetOnly && t.Alive);
                    if(victim!=null && game.art.SpriteContact(flame.renderer,victim.Renderer,0) && !game.WorldBlocked(flame.origin-Vector2.up*.8f,victim.Position,.03f) && Ready(victim.Renderer,.28f))game.HurtTarget(victim,8,flame.source);
                    return;
                }
                if(flame.dragonFire)
                {
                    if(!game.WorldBlocked(flame.origin-Vector2.up*.8f,game.body.position,.03f))game.dragons.HurtByFireContact(flame.renderer,12,flame.source);
                    return;
                }
                var target=(Vector2)game.hero.transform.position+new Vector2(0,.70f);
                if(Vector2.Distance(target,flame.renderer.bounds.center)<.42f && (!game.WorldBlocked(flame.origin-Vector2.up*.8f,game.body.position,.03f)) && Ready(game.hero,.28f))
                {game.HurtPlayer(8);}
                if(game.journey!=null && game.journey.companion.Hired && game.journey.companion.Alive && Vector2.Distance(game.journey.companion.Position+Vector2.up*.7f,flame.renderer.bounds.center)<.42f && (!game.WorldBlocked(flame.origin-Vector2.up*.8f,game.journey.companion.Position,.03f)) && Ready(game.journey.companion,.28f))game.journey.companion.Hurt(8);
                return;
            }
            foreach(var enemy in game.AdditionalTargets)if(enemy.Alive && (flame.targetOnly==null || flame.targetOnly==enemy.Renderer) && game.art.SpriteContact(flame.renderer,enemy.Renderer,0) && (!game.WorldBlocked(flame.origin-Vector2.up*.8f,enemy.Position,.03f)) && Ready(enemy,.28f)){if(flame.targetOnly==null)game.PlayerHit(enemy.Renderer);game.HurtTarget(enemy,10,flame.source);if(flame.targetOnly==null)game.journey.companion.Engage(enemy.Renderer);}
            if(game.magic.InPocket)return;
            if(!game.journey.IsCoast)return;
            if(flame.targetOnly==null && game.dummyHealth>0 && game.art.SpriteContact(flame.renderer,game.dummy) && Ready(game.dummy,.28f))game.DamageDummy(10);
            foreach(var mob in game.woodlice)if((flame.targetOnly==null || flame.targetOnly==mob.renderer) && mob.vitals.Alive && !mob.Grabbed && game.art.SpriteContact(flame.renderer,mob.renderer,0) && Ready(mob,.28f)){if(flame.targetOnly==null)game.PlayerHit(mob.renderer);mob.Hurt(10);if(flame.targetOnly==null)game.journey?.companion.Engage(mob.renderer);}
            foreach(var frog in frogs)if((flame.targetOnly==null || flame.targetOnly==frog.renderer) && frog.Alive && game.art.SpriteContact(flame.renderer,frog.renderer,0) && Ready(frog,.28f)){if(flame.targetOnly==null)game.PlayerHit(frog.renderer);frog.HurtFrom(10,flame.source);if(flame.targetOnly==null)game.journey?.companion.Engage(frog.renderer);}
        }
        public void EmitCone(Vector2 origin,Vector2 direction,bool hostile,int amount=6,float reach=5,float arc=120,SpriteRenderer targetOnly=null,bool dragonFire=false,SpriteRenderer source=null)
        {
            if(!source && !hostile)source=targetOnly?game.journey.companion.sprite:game.hero;
            if(direction.sqrMagnitude<.01f)direction=Vector2.right;
            float aim=Mathf.Atan2(direction.y,direction.x);
            for(int i=0;i<amount;i++)
            {
                float seed=clock*137.17f+i*19.73f+flames.Count*.173f;
                float scatter=Mathf.Repeat(Mathf.Sin(seed*1.37f)*43758.54f,1);
                float angle=aim+(scatter-.5f)*arc*Mathf.Deg2Rad;
                var velocity=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(7.5f+scatter*2);
                var sr=EffectSprite("Turbulent lamp flame",game.art.flame[i%16],origin,CoastGame.Depth(origin.y)+4);
                float size=.47f+scatter*.53f;
                sr.transform.localScale=Vector3.one*(size*.45f/sr.sprite.bounds.size.y);game.art.PixelStyle(sr);
                flames.Add(new FireMote{renderer=sr,origin=origin,velocity=velocity,life=reach/9.2f+scatter*.1f,size=size,seed=seed,hostile=hostile,damaging=true,reach=reach,targetOnly=targetOnly,dragonFire=dragonFire,source=source});
            }
            while(flames.Count>360){Destroy(flames[0].renderer.gameObject);flames.RemoveAt(0);}
        }
        public SpriteRenderer EffectSprite(string name,Sprite sprite,Vector2 position,int order)
        {var sr=game.art.Renderer(name,sprite,position,order);sr.sharedMaterial=fireMaterial;sr.transform.SetParent(WorldRoot,true);game.coop?.fx?.Track(sr);return sr;}
        public bool FireLampBurst(Vector2 direction)
        {if(!UseLamp(.028f))return false;lampBurst=.55f;EmitCone(PlayerHand,direction,false,3);return true;}
        public bool SetYoYoTarget(Vector2 target,bool held)
        {
            if(game.survivor.equipment[0]?.kind!=ItemKind.YoYo || !game.survivor.equipment[0].Usable)return false;
            if(!YoYoActive){yoYo.transform.position=PlayerHand;yoVelocity=Vector2.zero;YoYoActive=true;yoYo.enabled=true;}
            yoTarget=target;yoHeld=held;return true;
        }
        void TickYoYo(float dt)
        {
            if(!YoYoActive)return;
            if(game.survivor.equipment[0]?.kind!=ItemKind.YoYo || game.magic.Teleporting){CancelHeldSkills();return;}
            var origin=PlayerHand;var target=yoHeld?origin+Vector2.ClampMagnitude(yoTarget-origin,5):origin;
            var position=Vector2.SmoothDamp(YoYoPosition,target,ref yoVelocity,yoHeld?.16f:.10f,24,dt);
            position=origin+Vector2.ClampMagnitude(position-origin,5);yoYo.transform.position=position;yoYo.transform.rotation=Quaternion.Euler(0,0,clock*720);
            yoYo.sortingOrder=CoastGame.Depth(position.y)+3;stringLine.sortingOrder=yoYo.sortingOrder-1;stringLine.positionCount=9;
            for(int i=0;i<9;i++){float p=i/8f;stringLine.SetPosition(i,Vector2.Lerp(origin,position,p)+Vector2.down*Mathf.Sin(p*Mathf.PI)*.08f);}
            if(!yoHeld && Vector2.Distance(position,origin)<.08f){YoYoActive=false;yoYo.enabled=false;stringLine.positionCount=0;return;}
            if(yoHeld)foreach(var enemy in game.AdditionalTargets)if(enemy.Alive && game.art.SpriteContact(yoYo,enemy.Renderer,0) && (!game.WorldBlocked(game.body.position,enemy.Position,.03f)) && Ready(enemy,.32f)){game.PlayerHit(enemy.Renderer);game.HurtTarget(enemy,18,game.hero);game.journey.companion.Engage(enemy.Renderer);game.survivor.equipment[0].Wear(.1f);}
            if(yoHeld && !game.magic.InPocket && (game.journey==null || game.journey.IsCoast))
            {
                if(game.dummyHealth>0 && game.art.SpriteContact(yoYo,game.dummy) && Ready(game.dummy,.32f))game.DamageDummy(18);
                foreach(var mob in game.woodlice)if(mob.vitals.Alive && !mob.Grabbed && game.art.SpriteContact(yoYo,mob.renderer,0) && Ready(mob,.32f)){game.PlayerHit(mob.renderer);mob.Hurt(18);game.journey?.companion.Engage(mob.renderer);game.survivor.equipment[0].Wear(.1f);}
                foreach(var frog in frogs)if(frog.Alive && game.art.SpriteContact(yoYo,frog.renderer,0) && Ready(frog,.32f)){game.PlayerHit(frog.renderer);frog.Hurt(18);game.journey?.companion.Engage(frog.renderer);game.survivor.equipment[0].Wear(.1f);}
            }
        }
        public void CancelHeldSkills(){YoYoActive=false;yoHeld=false;yoYo.enabled=false;stringLine.positionCount=0;lampBurst=0;emitRemainder=0;}
        public bool BeginPlaceCampfire(Item item,GridInventory source)=>game.buildings.Begin(item,source);
        public bool CanPlaceFire(Vector2 point)=>game.buildings.CanPlace(point,ItemKind.Campfire,true);
        public bool PlacePendingFire(Vector2 point)=>game.buildings.Place(point);
        public void CancelPlacement(){game.buildings?.Cancel();}
        public CampfireObject CreateCampfire(Vector2 point)
        {
            point=WorldGrid.Snap(point);
            var fire=new CampfireObject{position=point,phase=UnityEngine.Random.value*1.33f};fire.logs=game.art.Renderer("Crafted campfire",game.art.campfireBase,point,CoastGame.Depth(point.y));
            fire.logs.transform.localScale=Vector3.one*(.90f/fire.logs.sprite.bounds.size.x);fire.logs.transform.SetParent(WorldRoot,true);game.art.PixelStyle(fire.logs);
            var obstacle=new GameObject("Campfire cell collision");obstacle.transform.SetParent(WorldRoot,true);obstacle.transform.position=point;var box=obstacle.AddComponent<BoxCollider2D>();box.size=new Vector2(.85f,.65f);fire.collider=box;
            var flame=EffectSprite("Continuous campfire flame",game.newArt.campfire[0],point,fire.logs.sortingOrder+2);flame.sharedMaterial=new Material(Shader.Find("AshenCoast/HearthFire"));fire.flames.Add(flame);
            game.buildings.CreateClearing(WorldGrid.Cell(point),Vector2Int.one,WorldRoot,true);
            campfires.Add(fire);Tick(0);return fire;
        }
        public bool TryExplodeFire(Vector2 point)=>ExplodeFire(point,false);
        public bool TryExplodeFireGlyph(Vector2 point)=>ExplodeFire(point,true);
        bool ExplodeFire(Vector2 point,bool glyph)
        {
            var held=game.survivor.equipment[0];if(held==null || !held.Usable || !HasOpenBook(ItemKind.Pyromancy) || (!glyph && held.kind!=ItemKind.Lamp) || (glyph && held.kind!=ItemKind.Orb && held.kind!=ItemKind.LastPrism))return false;
            var fire=campfires.Where(f=>!f.exploded && f.logs.gameObject.activeInHierarchy && Vector2.Distance(f.position,game.body.position)<=6 && Vector2.Distance(f.position,point)<.95f).OrderBy(f=>Vector2.Distance(f.position,point)).FirstOrDefault();
            if(fire==null || !game.survivor.SpendMana(BookSpells.Cost(game.survivor.OpenBook(ItemKind.Pyromancy),10)))return false;
            game.survivor.WearBook(ItemKind.Pyromancy,.25f);
            game.survivor.equipment[0].Wear(.25f);
            if(game.CoopOnline && !game.coop.CanSimulate && !game.coop.ApplyingNetwork){game.coop.Send(new LanPacketV49{kind="effect",region=game.coop.Region,body=JsonUtility.ToJson(new LanEffectV49{kind="explodeFire",x=point.x,y=point.y})});return true;}
            return ExplodeNetworkFireV49(point);
        }
        public bool ExplodeNetworkFireV49(Vector2 point)
        {
            var fire=campfires.FirstOrDefault(f=>!f.exploded && f.logs.gameObject.activeInHierarchy && Vector2.Distance(f.position,point)<.95f);if(fire==null)return false;
            fire.exploded=true;fire.logs.enabled=false;if(fire.collider)fire.collider.enabled=false;foreach(var sr in fire.flames){Destroy(sr.sharedMaterial);Destroy(sr.gameObject);}fire.flames.Clear();
            CreateExplosion(fire.position);game.dragons?.Schedule(fire.position,fire.logs.transform.parent);game.PlaySwish();return true;
        }
        public FireExplosion CreateExplosion(Vector2 point,bool hostile=false)
        {
            var effect=new FireExplosion{casterPeer=game.CoopOnline?(game.coop.Context?.Peer??game.coop.Peer):-1,hostile=hostile,position=point,renderer=EffectSprite("Pyromantic detonation",game.art.explosion[0],point,CoastGame.Depth(point.y)+10)};explosions.Add(effect);CreateBlastRing(effect);CreateBurstSparks(point);PoseExplosion(effect);
            for(int i=0;i<18;i++)
            {
                float angle=i*2.399f,rad=1.2f+(i%7)*.32f;var sr=EffectSprite("Explosive flying cinder",game.art.flame[i%16],point,CoastGame.Depth(point.y)+8);
                flames.Add(new FireMote{renderer=sr,origin=point,velocity=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*rad*2.6f,life=.60f+(i%5)*.08f,size=.18f+(i%3)*.08f,seed=i*.7f,reach=4});
            }
            return effect;
        }
        public void DamageArea(Vector2 point,float radius,int damage,bool player=true)
        {
            foreach(var enemy in game.AdditionalTargets)if(enemy.Alive && Vector2.Distance(enemy.Position,point)<radius && (!game.WorldBlocked(point,enemy.Position,.03f))){if(player)game.PlayerHit(enemy.Renderer);game.HurtTarget(enemy,damage,player?game.hero:game.journey.companion.sprite);}
            if(game.magic.InPocket)return;
            if(!game.journey.IsCoast)return;
            if(game.dummyHealth>0 && Vector2.Distance(game.DummyBase,point)<radius)game.DamageDummy(damage,player);
            foreach(var mob in game.woodlice)if(mob.vitals.Alive && !mob.Grabbed && Vector2.Distance(mob.Position,point)<radius){if(player)game.PlayerHit(mob.renderer);mob.Hurt(damage);}
            foreach(var frog in frogs)if(frog.Alive && Vector2.Distance(frog.Position,point)<radius){if(player)game.PlayerHit(frog.renderer);frog.HurtFrom(damage,player?game.hero:game.journey.companion.sprite);}
        }
        public TentacleField SummonTentacles(Vector2 point,float radius)
        {if(game.CoopOnline && !game.coop.CanSimulate && !game.coop.ApplyingNetwork){game.coop.Send(new LanPacketV49{kind="effect",region=game.coop.Region,body=JsonUtility.ToJson(new LanEffectV49{kind="tentacles",x=point.x,y=point.y,value=radius})});return null;}var field=new TentacleField(this,point,radius);fields.Add(field);return field;}
        public int AppendLights(Vector4[] positions,Vector4[] colors,int count)
        {
            int before=count;
            if(game.survivor.equipment[0]?.kind==ItemKind.Lamp && game.survivor.equipment[0].fuel>0 && game.survivor.equipment[0].Usable)AddLight(positions,colors,ref count,PlayerHand,3.7f,.9f+.18f*Mathf.Sin(clock*8)+.11f*Mathf.Sin(clock*17),new Color(1,.49f,.12f));
            if(!game.magic.InPocket && (game.journey==null || game.journey.IsCoast))foreach(var frog in frogs)if(frog.Alive)AddLight(positions,colors,ref count,frog.LampOrigin,3.2f,.75f+.20f*Mathf.Sin(clock*7+3),new Color(1,.48f,.15f));
            foreach(var fire in campfires)if(!fire.exploded && fire.logs.gameObject.activeInHierarchy)AddLight(positions,colors,ref count,fire.position,4.3f,1.2f+.20f*Mathf.Sin(fire.phase*8)+.13f*Mathf.Sin(fire.phase*17),new Color(1,.43f,.11f));
            foreach(var effect in explosions)if(effect.renderer.gameObject.activeInHierarchy)AddLight(positions,colors,ref count,effect.position,5.7f,(Mathf.Exp(-effect.age*4)*1.8f+Mathf.Sin(Mathf.Clamp01(effect.age/1.45f)*Mathf.PI)*.7f),new Color(1,.49f,.18f));
            foreach(var f in flames.Where(f=>f.damaging && f.age<.15f && f.renderer.gameObject.activeInHierarchy).Take(4))AddLight(positions,colors,ref count,f.renderer.transform.position,1.6f,.28f,new Color(1,.40f,.08f));
            lastLightCount=count-before;return count;
        }
        static void AddLight(Vector4[] p,Vector4[] c,ref int count,Vector2 point,float radius,float strength,Color color)
        {if(count>=p.Length)return;p[count]=new Vector4(point.x,point.y,radius,strength);c[count]=color;count++;}
    }

    public sealed partial class FrogMage
    {
        public const float FireReach=5,FireArc=120;
        public readonly SpriteRenderer renderer,lampFlame,shadow;
        public readonly CircleCollider2D feet;
        public const float MaximumHealth=180;
        public float health=MaximumHealth;
        public bool Hostile {get;private set;}
        public bool Alive=>health>0;
        public Vector2 Position=>(Vector2)renderer.transform.position;
        public Vector2 LampOrigin=>Position+new Vector2(direction==1?-.30f:.30f,.378f);
        public int frame,direction;
        public bool Firing=>Hostile && attackClock>=.6f && attackClock<1.85f;
        readonly ArcaneCombat owner;
        readonly CoastGame game;
        readonly Vector2 home;
        float clock,walkDistance,attackClock,emission,hitFlash;
        Vector2 aim=Vector2.down;
        public FrogMage(ArcaneCombat owner,Vector2 point)
        {
            this.owner=owner;game=owner.game;home=point;
            renderer=game.art.Renderer("Neutral frog mage with fire lamp",game.art.frog[0,2],point,CoastGame.Depth(point.y));renderer.transform.SetParent(game.coastRoot.transform,true);
            feet=renderer.gameObject.AddComponent<CircleCollider2D>();feet.radius=.19f;feet.offset=new Vector2(0,.12f);
            shadow=game.Shadow("Frog mage shadow",point,new Vector2(.55f,.16f),.33f);
            lampFlame=owner.EffectSprite("Frog lamp flickering wick",game.art.flame[0],LampOrigin,renderer.sortingOrder+2);Pose();
        }
        public void Tick(float dt,bool wander=true)
        {
            if(!Alive || game.spellcraft!=null && game.spellcraft.ImmobilizedV44(renderer))return;clock+=dt;hitFlash=Mathf.Max(0,hitFlash-dt*5);Vector2 velocity=Vector2.zero;
            ChooseFrogVictim();
            if(Hostile && victim!=null)
            {
                var delta=victim.Position+new Vector2(0,.6f)-LampOrigin;
                aim=Vector2.Lerp(aim,delta.normalized,Mathf.Clamp01(dt*5)).normalized;direction=Direction(aim);
                if(wander && delta.magnitude>4.4f)velocity=delta.normalized*.92f;
                if(delta.magnitude<=6.1f*(game.spellcraft?.Vision(renderer)??1))
                {
                    attackClock=Mathf.Repeat(attackClock+dt,2.8f);
                    if(Firing){emission+=dt;while(emission>=.035f){emission-=.035f;owner.EmitCone(LampOrigin,aim,true,3,FireReach,FireArc,victim.Renderer,false,renderer);}}
                }
            }
            else if(wander)
            {
                var target=home+new Vector2(Mathf.Sin(clock*.32f)*1.35f,Mathf.Sin(clock*.23f)*.65f);var delta=target-Position;
                if(delta.magnitude>.08f && Vector2.Distance(Position,game.body.position)>1.4f){velocity=delta.normalized*.65f;direction=Direction(delta);}
            }
            var move=velocity*dt;renderer.transform.position+=(Vector3)move;walkDistance+=move.magnitude;
            frame=velocity.sqrMagnitude>.01f?Mathf.FloorToInt(walkDistance/.055f)%8:2;Pose();
        }
        static int Direction(Vector2 d)=>Mathf.Abs(d.x)>Mathf.Abs(d.y)?d.x<0?1:3:d.y>0?2:0;
        void Pose()
        {
            renderer.sprite=game.art.frog[direction,frame];renderer.sortingOrder=CoastGame.Depth(Position.y);game.art.PixelStyle(renderer);
            renderer.color=Color.Lerp(Color.white,new Color(1,.50f,.37f),hitFlash);shadow.transform.position=Position+new Vector2(0,.10f);
            lampFlame.sprite=game.art.flame[Mathf.FloorToInt(clock*26)%16];lampFlame.transform.position=LampOrigin;
            float height=.095f+.0125f*Mathf.Sin(clock*14)+(Hostile && attackClock<.6f?.055f*attackClock/.6f:0);
            lampFlame.transform.localScale=Vector3.one*(height/lampFlame.sprite.bounds.size.y);lampFlame.sortingOrder=renderer.sortingOrder+2;game.art.PixelStyle(lampFlame);
        }
        public float Hurt(float damage)=>HurtFrom(damage,game.hero);
        public float HurtFrom(float damage,SpriteRenderer source)
        {
            if(game.CoopOnline && game.coop.world.RedirectDamage(new FrogActor(this),damage,source))return 0;
            if(!Alive || damage<=0)return 0;RememberFrogAttacker(source);damage*=game.spellcraft?.DamageFactor(renderer)??1;float applied=Mathf.Min(health,damage);health-=applied;hitFlash=1;game.ShowDamage(applied,Position+Vector2.up*2.4f,renderer);
            if(!Alive){renderer.enabled=lampFlame.enabled=shadow.enabled=feet.enabled=false;}else Pose();return applied;
        }
        public void Warp(Vector2 point){renderer.transform.position=point;Pose();}
        public void Reset(){health=MaximumHealth;Hostile=false;aggressor=null;victim=null;game.feedback.Forget(renderer);game.PlayerStruck.Remove(renderer);attackClock=emission=0;renderer.enabled=lampFlame.enabled=shadow.enabled=feet.enabled=true;Warp(home);}
    }
}
