using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed class SpellStatus
    {
        public SpriteRenderer actor;
        public float shackles,curse;
        public Vector2 anchor;
        public readonly List<SpriteRenderer> chains=new List<SpriteRenderer>();
        public readonly List<SpriteRenderer> motes=new List<SpriteRenderer>();
    }
    public sealed class AbyssFog
    {public Vector2 point;public Transform world;public float remaining=60;public GameObject visual;}
    public sealed partial class Spellcraft : MonoBehaviour
    {
        public CoastGame game;
        public Item SelectedBook;
        public SpellSlot SelectedSlot;
        public readonly Dictionary<SpriteRenderer,SpellStatus> statuses=new Dictionary<SpriteRenderer,SpellStatus>();
        public readonly List<AbyssFog> fogs=new List<AbyssFog>();
        public float tentacleReady;
        public bool BypassDamage;
        public bool Teleporting=>teleportAge>=0;
        float teleportAge=-1;
        Vector2 departure,arrival;
        bool moveAllies;
        readonly List<SpriteRenderer> travelMotes=new List<SpriteRenderer>();
        public void Initialize(CoastGame owner){game=owner;}
        public void Select(Item book,SpellSlot slot){SelectedBook=book;SelectedSlot=slot;game.ui.Notice(!slot.locked && book.kind==ItemKind.Tome?"Пространственный ромб":!slot.locked && book.kind==ItemKind.AbyssTome?"Туман бездны":BookSpells.Name(slot.kind));}
        public bool ImmobilizedV44(SpriteRenderer actor)=>Bound(actor) || game.v44.Stunned(actor);
        public bool Bound(SpriteRenderer actor)=>actor && statuses.TryGetValue(actor,out var s) && s.shackles>0;
        public float DamageFactor(SpriteRenderer actor)=>actor && statuses.TryGetValue(actor,out var s) && s.curse>0?1.5f:1;
        public float Vision(SpriteRenderer actor)=>actor && fogs.Any(f=>f.remaining>0 && actor.transform.IsChildOf(f.world) && Vector2.Distance(actor.transform.position,f.point)<=4)?.5f:1;
        public float Health(ICombatTarget actor)
        {
            if(actor is VillageActorV46 village)return village.health;
            if(actor is WormPart part)return game.cave.worms.FirstOrDefault(w=>w.parts.Contains(part))?.Health??part.health;
            if(actor is NightHunter hunter)return hunter.health;
            if(actor is CaveSpider spider)return spider.health;
            if(actor is RoosterDragon dragon)return dragon.health;
            if(actor is FortBandit bandit)return bandit.health;
            if(actor is FrogActor)return game.arcane.frogs.First(f=>f.renderer==actor.Renderer).health;
            if(actor is WoodlouseActor)return game.woodlice.First(m=>m.renderer==actor.Renderer).vitals.health;
            return 100;
        }
        IEnumerable<ICombatTarget> Enemies()=>game.WorldCreatures.Where(t=>!(t is HeroCombatActor) && !(t is RemotePlayerV49) && !(t is MageCombatActor) && !(t is VillageActorV46 v && v.Hired)).ToArray();
        SpellStatus Status(SpriteRenderer actor)
        {if(!statuses.TryGetValue(actor,out var s)){s=new SpellStatus{actor=actor};statuses.Add(actor,s);}return s;}
        public float Damage(ICombatTarget actor,float amount,bool bypass)
        {bool old=BypassDamage;BypassDamage=bypass;try{game.PlayerHit(actor.Renderer);return game.HurtTarget(actor,amount,game.hero);}finally{BypassDamage=old;}}
        public bool CastGesture(IList<Vector2> screenStroke)
        {
            if(screenStroke.Count<8)return false;
            var shape=GestureShape.Recognize(screenStroke);
            var world=screenStroke.Select(p=>(Vector2)game.cameraView.ViewportToWorldPoint(new Vector3(p.x/Screen.width,1-p.y/Screen.height,20))).ToArray();
            Vector2 centre=new Vector2((world.Min(p=>p.x)+world.Max(p=>p.x))*.5f,(world.Min(p=>p.y)+world.Max(p=>p.y))*.5f);
            var tome=game.survivor.OpenBook(ItemKind.Tome);
            if(shape==GestureKind.Diamond && tome!=null)
            {
                var charge=tome.spells.FirstOrDefault(s=>!s.locked && s.kind!=SpellKind.None && s.Remaining(game.seconds)<=0);
                if(charge==null){game.ui.Notice("Все ромбы в откате");return false;}
                if(!CanLand(game.map.CurrentRegion,centre) || !Pay(tome,10))return false;
                charge.cooldownUntil=game.seconds+30;StartTeleport(centre,false,true);return true;
            }
            if(shape==GestureKind.Infinity && tome!=null)
            {if(!Pay(tome,40))return false;return game.map.OpenTravel(10*game.cameraView.orthographicSize*2,false);}
            // In the abyss book every removable spell uses the reference rune as a fog charge.
            var abyss=game.survivor.OpenBook(ItemKind.AbyssTome);
            var openRune=SpellGlyphs.OpenRune(screenStroke);bool abyssRune=openRune==SpellGlyph.Fog;
            if(abyssRune && abyss!=null && abyss.spells.Any(s=>!s.locked && s.kind!=SpellKind.None))
            {
                var chosen=SelectedBook==abyss && SelectedSlot!=null && !SelectedSlot.locked && SelectedSlot.kind!=SpellKind.None?SelectedSlot:null;
                var charge=chosen!=null?(chosen.Remaining(game.seconds)<=0?chosen:null):abyss.spells.FirstOrDefault(s=>!s.locked && s.kind!=SpellKind.None && s.Remaining(game.seconds)<=0);
                if(charge!=null)
                {if(!Pay(abyss,10))return false;charge.cooldownUntil=game.seconds+10;CreateFog(centre);tentacleReady=Mathf.Max(game.seconds,tentacleReady-20);return true;}
                game.ui.Notice("Туман ещё в откате");return false;
            }
            if(abyss!=null && openRune==SpellGlyph.Tentacles)
            {
                if(game.seconds<tentacleReady){game.ui.Notice("Щупальца: "+Mathf.CeilToInt(tentacleReady-game.seconds)+" с");return false;}
                if(!Pay(abyss,30))return false;tentacleReady=game.seconds+60;abyss.spells[0].cooldownUntil=tentacleReady;
                game.arcane.SummonTentacles(centre,5.6f);foreach(var f in fogs.Where(f=>f.remaining>0 && f.world==game.arcane.WorldRoot).ToArray())game.arcane.SummonTentacles(f.point,4);
                game.sounds.PlayOneShot(Resources.Load<AudioClip>("Audio/TentaclesV37"),.8f);return true;
            }
            if(CastReturnV51(screenStroke,world,centre))return true;
            var pyro=game.survivor.OpenBook(ItemKind.Pyromancy);if(pyro==null)return false;
            if(openRune==SpellGlyph.Zigzag)
            {var fire=game.arcane.campfires.Where(f=>!f.exploded && f.logs.gameObject.activeInHierarchy && Vector2.Distance(f.position,centre)<Mathf.Max(1.2f,Vector2.Distance(world[0],centre)+.65f)).OrderBy(f=>Vector2.Distance(f.position,centre)).FirstOrDefault();return fire!=null && game.arcane.TryExplodeFireGlyph(fire.position);}
            SpellSlot selected=SelectedBook==pyro && SelectedSlot!=null && !SelectedSlot.locked?SelectedSlot:null;
            var spell=shape==GestureKind.Triangle?SpellKind.Shackles:shape==GestureKind.Circle?SpellKind.Curse:SpellKind.None;
            var slot=selected?.kind==spell?selected:pyro.spells.FirstOrDefault(s=>s.kind==spell && !s.locked);
            if(slot==null || spell==SpellKind.None)return false;
            var targets=Enemies().Where(t=>t.Alive && GestureShape.Inside(world,t.Position) && !game.WorldBlocked(centre,t.Position,.03f)).ToArray();
            if(targets.Length==0){game.ui.Notice("Внутри символа нет цели");return false;}
            if(!Pay(pyro,spell==SpellKind.Shackles?15:20))return false;
            if(spell==SpellKind.Shackles)
            {
                var target=targets.OrderByDescending(Health).First();Bind(target);
            }
            else foreach(var t in targets)Curse(t);
            return true;
        }
        bool Pay(Item book,int amount)
        {if(!game.survivor.SpendMana(BookSpells.Cost(book,amount))){game.ui.Notice("Недостаточно маны");return false;}return true;}
        public void Bind(ICombatTarget target)
        {
            if(game.coop!=null && game.coop.NotifyEffectV49("bind",target))return;
            if(target is WormPart part)
            {var worm=game.cave.worms.FirstOrDefault(w=>w.parts.Contains(part));if(worm!=null){foreach(var segment in worm.parts)BindActor(segment.Renderer);return;}}
            BindActor(target.Renderer);
        }
        void BindActor(SpriteRenderer actor)
        {
            var s=Status(actor);s.shackles=10;s.anchor=actor.transform.position;
            int count=Mathf.Clamp(Mathf.CeilToInt(actor.bounds.size.magnitude*1.7f),3,16);
            if(s.chains.Count==0)for(int i=0;i<count;i++)
            {var sr=game.art.Renderer("White-blue celestial chain",game.v37Art.chain,s.anchor,actor.sortingOrder+4);sr.transform.SetParent(actor.transform.parent,true);sr.sharedMaterial=game.v37Art.glowMaterial;s.chains.Add(sr);}
        }
        public void Curse(ICombatTarget target)
        {
            if(game.coop!=null && game.coop.NotifyEffectV49("curse",target))return;
            var s=Status(target.Renderer);s.curse=20;
            if(s.motes.Count==0)for(int i=0;i<16;i++)
            {var sr=game.art.Renderer("Curse outgoing violet particle",game.v37Art.mote,target.Position,target.Renderer.sortingOrder+6);sr.sharedMaterial=game.v37Art.glowMaterial;sr.transform.SetParent(target.Renderer.transform.parent,true);sr.transform.localScale=Vector3.one*.035f;sr.color=new Color(.63f,.25f,.88f,0);s.motes.Add(sr);}
        }
        public void CreateFog(Vector2 point)
        {
            if(game.CoopOnline && !game.coop.CanSimulate && !game.coop.ApplyingNetwork){game.coop.Send(new LanPacketV49{kind="effect",region=game.coop.Region,body=JsonUtility.ToJson(new LanEffectV49{kind="fog",x=point.x,y=point.y})});return;}
            var f=new AbyssFog{point=point,world=game.arcane.WorldRoot};
            f.visual=GameObject.CreatePrimitive(PrimitiveType.Quad);Destroy(f.visual.GetComponent<Collider>());f.visual.name="Sixty-second black abyss fog";f.visual.transform.SetParent(f.world,true);f.visual.transform.position=point+Vector2.up*.8f;f.visual.transform.localScale=new Vector3(9,9,1);
            var renderer=f.visual.GetComponent<MeshRenderer>();renderer.sharedMaterial=new Material(Shader.Find("AshenCoast/AbyssFog"));renderer.sortingOrder=CoastGame.Depth(point.y)+30;fogs.Add(f);
        }
        void Update(){if(!game || game.paused || game.testMode)return;Tick(Time.deltaTime);}
        public void Tick(float dt)
        {
            foreach(var s in statuses.Values)
            {
                s.shackles=Mathf.Max(0,s.shackles-dt);s.curse=Mathf.Max(0,s.curse-dt);if(!s.actor)continue;
                float width=Mathf.Max(.6f,s.actor.bounds.size.x);
                for(int i=0;i<s.chains.Count;i++)
                {
                    var sr=s.chains[i];sr.enabled=s.shackles>0 && s.actor.enabled;float angle=i*2.399f;
                    sr.transform.position=s.anchor+new Vector2(Mathf.Cos(angle)*width*.36f,.12f+Mathf.Sin(angle)*.15f);
                    float scale=Mathf.Max(.8f,s.actor.bounds.size.y*.55f)/sr.sprite.bounds.size.y;float rise=Mathf.SmoothStep(0,1,Mathf.Clamp01((10-s.shackles)/.35f));
                    sr.transform.localScale=new Vector3(scale*.42f,scale*rise,1);sr.transform.rotation=Quaternion.Euler(0,0,Mathf.Sin(angle)*30);sr.color=new Color(.68f,.9f,1,Mathf.Clamp01(s.shackles/.4f));
                }
                for(int i=0;i<s.motes.Count;i++)
                {var sr=s.motes[i];sr.enabled=s.curse>0 && s.actor.enabled;float p=Mathf.Repeat(game.seconds*.7f+i*.137f,1),a=i*2.399f+game.seconds*.4f;sr.transform.position=(Vector2)s.actor.transform.position+new Vector2(Mathf.Cos(a)*width*(.2f+p*.38f),.2f+p*1.6f);sr.transform.localScale=Vector3.one*(.035f+Mathf.Sin(p*Mathf.PI)*.10f);sr.color=new Color(.63f,.25f,.88f,Mathf.Sin(p*Mathf.PI));}
            }
            for(int i=fogs.Count-1;i>=0;i--)
            {var f=fogs[i];f.remaining-=dt;if(f.remaining<=0){if(f.visual.GetComponent<Renderer>())Destroy(f.visual.GetComponent<Renderer>().sharedMaterial);Destroy(f.visual);fogs.RemoveAt(i);}else if(f.visual.GetComponent<Renderer>())f.visual.GetComponent<Renderer>().sharedMaterial.SetFloat("_Opacity",Mathf.Min(1,(60-f.remaining)*2,f.remaining)*.94f);}
            if(teleportAge>=0)
            {teleportAge+=dt;if(teleportAge>=.32f && teleportAge-dt<.32f)MoveTo(arrival,moveAllies);PoseTravel();if(teleportAge>=.75f){teleportAge=-1;foreach(var sr in travelMotes)Destroy(sr.gameObject);travelMotes.Clear();}}
            var abyss=game.survivor.OpenBook(ItemKind.AbyssTome);if(abyss!=null)abyss.spells[0].cooldownUntil=tentacleReady;
        }
        void LateUpdate(){foreach(var s in statuses.Values)if(s.shackles>0 && s.actor)s.actor.transform.position=s.anchor;}
        public bool CanLand(MapRegion region,Vector2 p)
        {
            if(region==MapRegion.Pocket && region!=game.map.CurrentRegion)return false;
            bool walkable=region==MapRegion.SnailHouse?Vector2.Distance(p,game.snails.Clamp(p))<.01f:region==MapRegion.AmazonHouse?game.amazons.RoomBounds.Contains(p):region==MapRegion.Coast?MeadowLayout.Walkable(p):region==MapRegion.Catacombs?CatacombLayout.Floor(p):region==MapRegion.Pocket?Mathf.Abs(p.x)<3.7f && Mathf.Abs(p.y)<3.7f:region==MapRegion.Fort?Vector2.Distance(p,game.fort.Clamp(p))<.01f:region==MapRegion.Tavern?p.x>-6.65f && p.x<7.65f && p.y>-4.75f && p.y<4.75f:p.x>-28 && p.x<28 && p.y>-17 && p.y<27;
            if(!walkable)return false;
            if(region==game.map.CurrentRegion)return !Physics2D.OverlapCircleAll(p,.28f).Any(c=>!c.isTrigger && c.gameObject!=game.hero.gameObject);
            var world=region==MapRegion.SnailHouse && game.snails.Inside>=0?game.snails.houses[game.snails.Inside].room.transform:region==MapRegion.AmazonHouse?game.amazons.roomRoot.transform:region==MapRegion.Coast?game.coastRoot.transform:region==MapRegion.GoldenField?game.journey.fieldRoot.transform:region==MapRegion.Tavern?game.journey.tavernRoot.transform:region==MapRegion.Fort?game.fort.root.transform:game.cave.root.transform;
            foreach(var box in world.GetComponentsInChildren<BoxCollider2D>(true))if(!box.isTrigger && box.enabled){var q=(Vector2)box.transform.InverseTransformPoint(p)-box.offset;if(Mathf.Abs(q.x)<box.size.x*.5f+.28f && Mathf.Abs(q.y)<box.size.y*.5f+.28f)return false;}
            return true;
        }
        public void StartTeleport(Vector2 point,bool allies,bool instant=false)
        {departure=game.body.position;arrival=point;moveAllies=allies;teleportAge=instant?.32f:0;if(instant)MoveTo(point,allies,null,true);for(int i=0;i<36;i++){var sr=game.arcane.EffectSprite("Departure / arrival spatial mote",game.art.flame[i%16],departure,CoastGame.Depth(departure.y)+20);sr.sharedMaterial=game.v37Art.glowMaterial;travelMotes.Add(sr);}PoseTravel();}
        public void MoveTo(Vector2 point,bool allies,MapRegion? region=null,bool panCamera=false)
        {
            var mage=game.journey.companion;var previousMage=mage.Position;var previousRegion=game.map.CurrentRegion;
            var previousParty=game.amazons!=null?game.amazons.Allies.Select(a=>new System.Collections.Generic.KeyValuePair<VillageActorV46,Vector2>(a,a.Position)).ToArray():new System.Collections.Generic.KeyValuePair<VillageActorV46,Vector2>[0];var previousRoot=game.arcane.WorldRoot;
            if(region.HasValue && region.Value!=previousRegion){if(game.magic.InPocket)game.magic.ExitThroughDoor();game.journey.SetArea(region.Value==MapRegion.SnailHouse?WorldArea.SnailHouse:region.Value==MapRegion.AmazonHouse?WorldArea.AmazonHouse:(WorldArea)(int)region.Value,point);if(!allies && mage.Hired){mage.remoteArea=previousRegion;mage.Warp(previousMage);mage.sprite.gameObject.SetActive(false);}}
            game.ui.CloseContainers();game.arcane.CancelHeldSkills();game.body.linearVelocity=Vector2.zero;game.movement=Vector2.zero;game.body.position=point;game.hero.transform.position=point;game.rollers.Reset();
            if(game.journey.IsCoast)game.meadow.Refresh(point,true);
            if(allies){game.journey.companion.remoteArea=null;game.journey.companion.sprite.gameObject.SetActive(game.journey.companion.Hired);}
            if(game.amazons!=null){if(allies)game.amazons.WarpParty(point);else foreach(var pair in previousParty){pair.Key.Renderer.transform.SetParent(previousRoot,true);pair.Key.Warp(pair.Value);pair.Key.HoldPosition=true;}}
            if(allies && game.journey.companion.Hired && game.journey.companion.Alive)game.journey.companion.Warp(point+Vector2.left*.8f);
            if(game.journey.companion.Hired)game.journey.companion.suppressCatchUp=!allies;
            if(!panCamera)game.ResetCamera(new Vector3(point.x,point.y+1.1f,-20));game.map.RecordVisit();
        }
        void PoseTravel()
        {for(int i=0;i<travelMotes.Count;i++){var sr=travelMotes[i];bool end=i>=18;var p=end?arrival:departure;float age=end?Mathf.Max(0,teleportAge-.32f):teleportAge,a=i*2.399f+age*7;sr.transform.position=p+new Vector2(Mathf.Cos(a)*(.8f+age),Mathf.Sin(a)*.4f+age*2.5f);sr.transform.localScale=Vector3.one*.13f;sr.color=new Color(.36f,.75f,1,Mathf.Clamp01(1-age*2));}}
    }
    public enum GestureKind { None, Triangle, Diamond, Circle, Infinity }
    public static class GestureShape
    {
        static Vector2[] Normalize(IList<Vector2> p)
        {float x=p.Min(v=>v.x),y=p.Min(v=>v.y),w=p.Max(v=>v.x)-x,h=p.Max(v=>v.y)-y;return p.Select(v=>new Vector2((v.x-x)/Mathf.Max(.0001f,w),(v.y-y)/Mathf.Max(.0001f,h))).ToArray();}
        public static GestureKind Recognize(IList<Vector2> raw)
        {
            if(raw.Count<8)return GestureKind.None;float w=raw.Max(p=>p.x)-raw.Min(p=>p.x),h=raw.Max(p=>p.y)-raw.Min(p=>p.y);if(w<45 || h<45)return GestureKind.None;
            var p=Normalize(raw);if(Vector2.Distance(p[0],p[p.Length-1])>.36f)return GestureKind.None;
            var samples=SpatialSigil.Resample(p,80);
            var inf=Enumerable.Range(0,81).Select(i=>{float a=i*Mathf.PI*2/80;return new Vector2(.5f+.5f*Mathf.Sin(a),.5f+.5f*Mathf.Sin(2*a));}).ToArray();
            if(ClosedError(samples,inf)<.14f && w/h>.95f)return GestureKind.Infinity;
            var diamond=new[]{new Vector2(.5f,0),new Vector2(1,.5f),new Vector2(.5f,1),new Vector2(0,.5f),new Vector2(.5f,0)};
            float diamondError=ClosedError(samples,diamond),triangleError=100;
            for(int angle=0;angle<360;angle+=15)
            {var triangle=Enumerable.Range(0,4).Select(i=>{float a=(angle+(i%3)*120)*Mathf.Deg2Rad;return new Vector2(.5f+Mathf.Cos(a)*.5f,.5f+Mathf.Sin(a)*.5f);}).ToArray();triangleError=Mathf.Min(triangleError,ClosedError(samples,Normalize(triangle)));}
            var circle=Enumerable.Range(0,81).Select(i=>Vector2.one*.5f+new Vector2(Mathf.Cos(i*Mathf.PI/40),Mathf.Sin(i*Mathf.PI/40))*.5f).ToArray();float circleError=ClosedError(samples,circle);
            float turn=0;for(int i=1;i<samples.Count;i++){var a=samples[i-1]-Vector2.one*.5f;var b=samples[i]-Vector2.one*.5f;turn+=Mathf.DeltaAngle(Mathf.Atan2(a.y,a.x)*Mathf.Rad2Deg,Mathf.Atan2(b.y,b.x)*Mathf.Rad2Deg)*Mathf.Deg2Rad;}
            float radial=samples.Average(v=>Mathf.Abs((v-Vector2.one*.5f).magnitude-.5f));
            if(circleError<.18f && radial<.13f && Mathf.Abs(turn)>5 && circleError<Mathf.Min(diamondError,triangleError)+.008f)return GestureKind.Circle;
            if(diamondError<.115f && diamondError<triangleError)return GestureKind.Diamond;
            if(triangleError<.12f)return GestureKind.Triangle;
            return GestureKind.None;
        }
        static float ClosedError(IList<Vector2> p,IList<Vector2> template)
        {var q=SpatialSigil.Resample(template,p.Count);float best=100;for(int shift=0;shift<p.Count;shift+=2){float f=0,b=0;for(int i=0;i<p.Count;i++){f+=(p[i]-q[(i+shift)%q.Count]).sqrMagnitude;b+=(p[i]-q[(q.Count-1-i+shift)%q.Count]).sqrMagnitude;}best=Mathf.Min(best,Mathf.Sqrt(Mathf.Min(f,b)/p.Count));}return best;}
        public static bool FogRune(IList<Vector2> raw)=>SpellGlyphs.OpenRune(raw)==SpellGlyph.Fog;
        public static bool Inside(IList<Vector2> polygon,Vector2 p)
        {bool inside=false;for(int i=0,j=polygon.Count-1;i<polygon.Count;j=i++)if((polygon[i].y>p.y)!=(polygon[j].y>p.y) && p.x<(polygon[j].x-polygon[i].x)*(p.y-polygon[i].y)/(polygon[j].y-polygon[i].y)+polygon[i].x)inside=!inside;return inside;}
    }
}
