using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed class AmazonHouseV46
    { public Vector2 entrance;public SpriteRenderer sprite;public bool chief;public readonly List<CraftingStation> furnishings=new List<CraftingStation>(); }
    public sealed class AmazonFireV46
    { public Vector2 point;public SpriteRenderer logs,flame;public float phase; }
    public sealed class PracticeTargetV46:ICombatTarget
    {
        public SpriteRenderer Renderer{get;set;}public Vector2 Position=>(Vector2)Renderer.transform.position;
        public float health=500;public bool Alive=>true;public CoastGame game;
        public float Hurt(float damage){health=Mathf.Max(0,health-damage);game.ShowDamage(damage,Position+Vector2.up*1.5f,Renderer);if(health==0)health=500;return damage;}
    }
    public sealed partial class AmazonSettlementV46:MonoBehaviour
    {
        public CoastGame game;public AmazonArtV46 art;
        public GameObject root,roomRoot;
        public Vector2 Centre{get;private set;}
        public readonly List<AmazonHouseV46> houses=new List<AmazonHouseV46>();
        public readonly List<VillageActorV46> people=new List<VillageActorV46>();
        public readonly List<ChoppablePoplarV46> trees=new List<ChoppablePoplarV46>();
        public readonly List<AmazonFireV46> fires=new List<AmazonFireV46>();
        public readonly List<PracticeTargetV46> targets=new List<PracticeTargetV46>();
        public readonly List<Rect> barriers=new List<Rect>();
        public VillageActorV46 Offer;
        public AmazonHouseV46 currentHouse;
        public bool Interior=>game.journey.Area==WorldArea.AmazonHouse;
        public bool InTerritory=>!game.magic.InPocket && (Interior || game.journey.IsCoast && Vector2.Distance(game.body.position,Centre)<35);
        public float TitleRemaining{get;private set;}
        public AudioSource music;
        public SpriteRenderer statue,exit;
        public IEnumerable<VillageActorV46> Allies=>people.Where(p=>p.Hired && p.Alive && p.Renderer.gameObject.activeInHierarchy);
        public IEnumerable<ICombatTarget> Targets=>people.Where(p=>!p.Hired && p.Alive && p.Renderer.gameObject.activeInHierarchy).Cast<ICombatTarget>().Concat(game.journey.IsCoast?targets:Enumerable.Empty<ICombatTarget>());
        float clock,doorWait;bool entered;
        public void Initialize(CoastGame owner)
        {
            game=owner;art=new AmazonArtV46(game.art);Centre=WorldShape.NearestLand(new Vector2(-135,230),65);
            if(Vector2.Distance(Centre,WorldShape.FortPosition)<90)Centre=WorldShape.NearestLand(new Vector2(150,210),65);
            WorldShape.AmazonPosition=Centre;
            root=new GameObject("Крылатый стан / Amazon village");root.transform.SetParent(game.coastRoot.transform,false);
            roomRoot=new GameObject("Amazon dwellings / persistent interiors");
            LoadArtV48();CreateVillage();VillageLifeV48();CreateRoom();roomRoot.SetActive(false);
            music=gameObject.AddComponent<AudioSource>();music.clip=Resources.Load<AudioClip>("Audio/AmazonBattleV46");music.loop=true;music.volume=0;music.Play();
        }
        public SpriteRenderer Prop(string name,Sprite sprite,Vector2 at,float width,Transform parent,int? order=null)
        {
            var r=game.art.Renderer(name,sprite,at,order??CoastGame.Depth(at.y));r.transform.localScale=Vector3.one*(width/sprite.bounds.size.x);r.transform.SetParent(parent,true);game.art.PixelStyle(r);return r;
        }
        void Barrier(string name,Vector2 at,Vector2 size)
        {var o=new GameObject(name);o.transform.SetParent(root.transform,true);o.transform.position=at;o.AddComponent<BoxCollider2D>().size=size;barriers.Add(new Rect(at-size*.5f,size));}
        void CreateVillage()
        {
            Floor("Worn training ground",root.transform,Centre,new Vector2(21,18),false);
            statue=Prop("Альвиэль",art.statue,Centre+Vector2.up*.2f,4.6f,root.transform);
            Barrier("Alviel stone pedestal",Centre+Vector2.up*.8f,new Vector2(2.3f,1.2f));
            for(int i=0;i<12;i++){float angle=i*Mathf.PI*2/12;CreateFire(Centre+new Vector2(Mathf.Cos(angle)*2.02f,Mathf.Sin(angle)*.89f)+Vector2.up*.28f,1.18f,i*.43f);}
            for(int i=0;i<6;i++)
            {
                float x=-8.6f+i*3.45f;var north=Centre+new Vector2(x,8);Prop("Training fence",art.props[4],north,3.5f,root.transform);Barrier("Training north rail",north+Vector2.up*.3f,new Vector2(3.4f,.22f));
                if(i!=2 && i!=3){var south=Centre+new Vector2(x,-8);Prop("Training fence",art.props[4],south,3.5f,root.transform);Barrier("Training south rail",south+Vector2.up*.3f,new Vector2(3.4f,.22f));}
            }
            SideFenceV48(-10.1f);SideFenceV48(10.1f);
            for(int i=0;i<4;i++)
            {
                var p=Centre+new Vector2(-7+i*4.5f,5.3f);targets.Add(new PracticeTargetV46{game=game,Renderer=Prop("Training target",art.props[5],p,1.25f,root.transform)});
                game.CreateLooseLootV46(new Item(i%2==0?ItemKind.Axe:ItemKind.Stake),Centre+new Vector2(-6+i*3.7f,-4.5f));
            }
            for(int i=0;i<10;i++)
            {
                float a=(i/10f)*Mathf.PI*2;var p=Centre+new Vector2(Mathf.Sin(a)*23,Mathf.Cos(a)*22);
                var h=new AmazonHouseV46{entrance=p,chief=i==0};h.sprite=Prop(i==0?"Дом вождя":"Дом амазонок",art.props[i==0?1:0],p,i==0?10:6,root.transform);houses.Add(h);
                Barrier("Straw house foundation",p+Vector2.up*1.5f,new Vector2(i==0?8.2f:4.7f,2.0f));
                if(i%2==1)CreateFire(p+new Vector2(3.7f,-2.2f),2.4f,i*.7f);
            }
            var weapons=new[]{ItemKind.Stake,ItemKind.Bow,ItemKind.Stake,ItemKind.Axe,ItemKind.Dagger,ItemKind.Bow};
            for(int i=0;i<14;i++)
            {
                float a=i*Mathf.PI*2/14;var p=Centre+new Vector2(Mathf.Sin(a)*15,Mathf.Cos(a)*14);
                people.Add(new VillageActorV46(this,p,i,false, i<2,false,weapons[i%weapons.Length]));
            }
            var slaver=new VillageActorV46(this,Centre+new Vector2(-18,-14),14,false,false,true,ItemKind.Dagger);people.Add(slaver);
            for(int i=0;i<4;i++)
            {
                var p=Centre+new Vector2(-17+i*3.2f,-20);var captive=new VillageActorV46(this,p,i,true,false,false,ItemKind.Dagger);captive.cage=Prop("Bone cage",art.props[3],p-Vector2.up*.35f,2.65f,root.transform,CoastGame.Depth(p.y)+3);people.Add(captive);
            }
            for(int i=0;i<7;i++){float a=(i+.25f)*Mathf.PI*2/7;trees.Add(new ChoppablePoplarV46(this,Centre+new Vector2(Mathf.Cos(a)*37,Mathf.Sin(a)*34),i));}
        }
        void CreateFire(Vector2 point,float width,float phase)
        {
            var fire=new AmazonFireV46{point=point,phase=phase};fire.logs=Prop("Village bonfire logs",game.art.campfireBase,point,width,root.transform);
            fire.flame=Prop("Night bonfire",game.art.flame[0],point+Vector2.up*.12f,width*.84f,root.transform,CoastGame.Depth(point.y)+2);fires.Add(fire);
        }
        void Update(){if(game!=null && !game.testMode && !game.paused)Tick(Time.deltaTime);}
        public void Tick(float dt)
        {
            clock+=dt;doorWait=Mathf.Max(0,doorWait-dt);TitleRemaining=Mathf.Max(0,TitleRemaining-dt);
            bool inside=InTerritory;if(inside && !entered)TitleRemaining=4.5f;entered=inside;
            if(Offer!=null && (!Offer.Alive || !Offer.Renderer.gameObject.activeInHierarchy || Vector2.Distance(Offer.Position,game.body.position)>3.5f))Offer=null;
            music.volume=Mathf.MoveTowards(music.volume,inside?.48f:0,dt*.35f);
            foreach(var fire in fires)
            {fire.flame.enabled=CoastTime.IsNight(game.DaySeconds);fire.flame.sprite=game.art.flame[(int)((clock+fire.phase)*12)%16];fire.flame.transform.localScale=Vector3.one*(fire.logs.bounds.size.x*.86f/fire.flame.sprite.bounds.size.x);game.art.PixelStyle(fire.flame);}
            foreach(var flame in roomFlames)if(flame.gameObject.activeInHierarchy){flame.sprite=game.art.flame[(int)(clock*12)%16];game.art.PixelStyle(flame);}
            if(game.SimulateWorldV49){TickLifeV48(dt);foreach(var actor in people.ToArray())game.TickCoopActorV49(actor.Position,()=>actor.Tick(dt),actor.Hired?actor.Renderer:null);foreach(var tree in trees)tree.Tick(dt);TickArrows(dt);}
            if(Interior && Vector2.Distance(game.body.position,(Vector2)exit.transform.position)<.7f && doorWait<=0)LeaveHouse();
        }
        public int AppendLights(Vector4[] lights,Vector4[] colors,int count)
        {
            if(Interior && currentHouse!=null && currentHouse.chief && count<lights.Length){lights[count]=new Vector4(0,-.2f,6,1);colors[count++]=new Vector4(1,.55f,.22f,1);return count;}
            if(!game.journey.IsCoast || game.magic.InPocket || !CoastTime.IsNight(game.DaySeconds))return count;
            foreach(var f in fires.OrderBy(f=>(f.point-game.body.position).sqrMagnitude).Take(10))
            {if(count>=lights.Length)break;lights[count]=new Vector4(f.point.x,f.point.y+.4f,6.5f,.66f+.05f*Mathf.Sin(clock*7+f.phase));colors[count++]=new Vector4(1,.47f,.16f,1);}return count;
        }
        public bool Blocked(Vector2 from,Vector2 to,float radius)
        {
            if(Interior)return !RoomBounds.Contains(to) || Physics2D.CircleCastAll(from+Vector2.up*.13f,radius,to-from,Vector2.Distance(from,to)).Any(h=>h.collider && !h.collider.isTrigger && h.collider.gameObject!=game.hero.gameObject);
            if(!game.journey.IsCoast || game.magic.InPocket || Vector2.Distance(to,Centre)>55)return false;
            foreach(var b in barriers){var expanded=new Rect(b.xMin-radius,b.yMin-radius,b.width+radius*2,b.height+radius*2);if(expanded.Contains(to) && !expanded.Contains(from))return true;}
            foreach(var t in trees)if(t.Standing && Vector2.Distance(to,t.position)<radius+.23f && Vector2.Distance(from,t.position)>=radius+.23f)return true;return false;
        }
        public Renderer InteractionAt(Vector2 point,out string name)
        {
            name=null;if(Interior){if(exit.bounds.Contains(point)){name="Выйти из дома";return exit;}return null;}
            if(!game.journey.IsCoast || game.magic.InPocket)return null;
            foreach(var p in people)if(p.Alive && PixelAt(p.Renderer,point)){name=p.Name;return p.Renderer;}
            foreach(var b in plains.bandits)if(b.Alive && PixelAt(b.Renderer,point)){name=b.Archer?"Разбойник-лучник":"Разбойник-мечник";return b.Renderer;}
            foreach(var h in houses)if(h.sprite.bounds.Contains(point)){name=h.chief?"Дом вождя":"Дом амазонок";return h.sprite;}
            foreach(var t in trees)if(t.Standing && PixelAt(t.sprite,point)){name="Тополь";return t.sprite;}
            if(PixelAt(statue,point)){name="Альвиэль";return statue;}return null;
        }
        public bool Interact(Vector2 point)
        {
            if(Interior){if(exit.bounds.Contains(point) && Vector2.Distance(game.body.position,exit.transform.position)<3.5f){LeaveHouse();return true;}return false;}
            if(!game.journey.IsCoast || game.magic.InPocket)return false;
            foreach(var p in people.Where(p=>p.Alive && p.Renderer.enabled))if(PixelAt(p.Renderer,point) && Vector2.Distance(game.body.position,p.Position)<3.5f)
            {if(p.Hired)game.snails.AllyOffer=game.snails.Key(p.Renderer);else if(!p.Hostile && !p.Hired && (p.Hunter || p.Slaver || p.WeaponMerchantV48))Offer=p;else return false;return true;}
            foreach(var h in houses)if(h.sprite.bounds.Contains(point) && Vector2.Distance(game.body.position,h.entrance)<3.4f && doorWait<=0){EnterHouse(h);return true;}
            return false;
        }
        public bool Hire(VillageActorV46 person)
        {
            if(game.snails.SelectHire(game.snails.Key(person.Renderer),()=>{if(!Hire(person))game.ui.Notice("Недостаточно монет");}))return true;
            if(game.CoopOnline && !game.coop.ApplyingNetwork)return game.coop.RequestHireV49(game.coop.world.Key(person.Renderer));
            if(person.Hired || !person.Alive || person.Hostile || !(person.Hunter || person.Captive) || !game.survivor.SpendCoins(1))return false;
            person.Hired=true;person.Captive=false;person.hostile=false;if(person.cage)person.cage.enabled=false;
            person.Renderer.transform.SetParent(game.arcane.WorldRoot,true);person.Warp(game.body.position+Vector2.left*(1+Allies.Count()*.55f));Offer=null;game.PlayPickup();return true;
        }
        public bool PixelAt(SpriteRenderer sprite,Vector2 point)
        {if(!sprite || !sprite.enabled || !sprite.gameObject.activeInHierarchy)return false;var local=(Vector2)sprite.transform.InverseTransformPoint(point);if(sprite.flipX)local.x=-local.x;if(sprite.flipY)local.y=-local.y;return game.art.Opaque(sprite.sprite,local);}
        public void Alarm(VillageActorV46 hit,SpriteRenderer source)
        {if((game.CoopOnline?!game.coop.IsPlayer(source):source!=game.hero) || hit.Hired)return;EndTrainingV48();foreach(var p in people)if(!p.Hired && !p.IsSlave && p.Alive && Vector2.Distance(p.Position,hit.Position)<18){p.hostile=true;p.Aggressor=game.hero;}Offer=null;game.ui.CloseContainers();}
        public bool Enemy(ICombatTarget actor)
        {
            if(actor is SnailHouseV50)return false;
            if(actor is VillageActorV46 villager)return villager.Hostile && !villager.Hired;
            if(actor is HeroCombatActor || actor is MageCombatActor || actor is PracticeTargetV46 || actor is BeachGull || actor is TavernResident resident && !game.journey.TavernAlarm)return false;
            if(actor is FrogActor)return game.arcane.frogs.Any(f=>f.renderer==actor.Renderer && f.Hostile);
            return actor.Alive && (game.PlayerStruck.Contains(actor.Renderer) || actor is PlainBanditV48 || actor is FortBandit || actor is CaveSpider || actor is WormPart || actor is NightHunter || actor is RoosterDragon);
        }
        public static bool Armored(ICombatTarget actor)=>actor is TavernResident resident && resident.Armored || actor is PlainBanditV48 plain && !plain.Archer || actor is FortBandit bandit && bandit.role==BanditRole.Warrior || actor is WoodlouseActor || actor is WormPart || actor is VillageActorV46 woman && !woman.IsSlave;
        public static float WeaponDamage(Item weapon,ICombatTarget target)=>weapon.Definition.damage*(weapon.kind==ItemKind.Axe && Armored(target)?1.2f:1);
        public void WarpParty(Vector2 at){int i=0;foreach(var a in people.Where(a=>a.Hired && a.Alive && !game.snails.Resting(a.Renderer) && !game.snails.Returning(a.Renderer) && (!game.CoopOnline || game.coop.world.OwnAlly(a.Renderer)))){a.HoldPosition=false;a.Renderer.transform.SetParent(game.arcane.WorldRoot,true);a.Warp(at+new Vector2(-1.1f-i++*.5f,-.4f));}}
        public void AreaChanged(){if(roomRoot)roomRoot.SetActive(Interior);if(Interior)game.cameraView.backgroundColor=new Color(.025f,.021f,.015f);WarpParty(game.body.position);}
    }
}
