using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    [Serializable] public sealed class CorpseDataV51 {public string key,actor,region;public int family,peer=-1;public float x,y,age,maxHealth,width;public bool spent;}
    [Serializable] public sealed class CorpseListV51 {public CorpseDataV51[] bodies;}
    [Serializable] public sealed class RaiseRequestV51 {public string key,request;public int cost;}
    [Serializable] public sealed class RaiseResultV51 {public string request;public int cost;public bool success;}
    public sealed class CorpseVisualV51 {public CorpseDataV51 data;public SpriteRenderer sprite,fall;public Vector3 fallScale;public float fallAngle;}
    [DefaultExecutionOrder(700)]
    public sealed class CorpseWorldV51:MonoBehaviour
    {
        public CoastGame game;public readonly Dictionary<string,CorpseVisualV51> bodies=new Dictionary<string,CorpseVisualV51>();
        readonly HashSet<string> dead=new HashSet<string>(),droppedV53=new HashSet<string>();readonly Dictionary<string,float> raisedUntil=new Dictionary<string,float>();float nextSync,nextScan,nextBindings;CoopBindingV49[] cached=Array.Empty<CoopBindingV49>();readonly List<string> expired=new List<string>();int sequence;public string Pending;int pendingCost;
        bool Authority=>!game.CoopOnline || game.coop.IsHost;
        public void Initialize(CoastGame g){game=g;}
        public bool HasBody(string actor)=>bodies.Values.Any(b=>b.data.actor==actor && !b.data.spent && b.data.age<60);
        public IEnumerable<CoopBindingV49> Bindings=>cached;
        public void RefreshBindingsV53(){cached=game.coop.world.BindingsV51().ToArray();nextBindings=Time.unscaledTime+.5f;}
        public int Family(CoopBindingV49 b)=>b.kind=="woodlouse" || b.kind=="caravan"?3:b.kind=="frog"?4:b.kind=="spider"?5:b.kind=="gull"?6:b.kind=="worm"?7:b.kind=="mage"?1:b.driver is VillageActorV46 woman && !woman.IsSlave?2:0;
        public float MaxHealth(CoopBindingV49 b)=>b.driver is VillageActorV46 woman?woman.MaxHealth:b.driver is Woodlouse bug?bug.vitals.maximumHealth:b.kind=="mage"?CompanionMage.MaxHealth:b.kind=="frog"?FrogMage.MaximumHealth:b.kind=="spider"?80:b.kind=="gull"?30:b.kind=="worm"?WormPart.MaximumHealth:b.kind=="caravan"?WoodlouseVitals.MaximumHealth:100;
        public float WidthV53(CoopBindingV49 b,int family)
        {if(b?.driver is Woodlouse bug)return 2.2f*(bug.Small?.48f:1);if(b?.driver is CaravanWoodlouse)return 2.2f;return family==3?2.2f:family==4?1.15f:family==5?1.35f:family==6?.60f:family==7?1.5f:family==8?1.11f:1.85f;}
        bool Eligible(CoopBindingV49 b)=>b.renders[0] && b.kind!="dummy" && b.kind!="target" && b.kind!="tree" && b.kind!="dragon" && b.kind!="hunter";
        public void ScanV53()
        {
            foreach(var b in cached)if(Eligible(b))
            {float hp=b.health();bool wasDead=dead.Contains(b.key);if(hp>0 && !wasDead || hp<=0 && wasDead)continue;var r=b.renders[0];Track(b.key,game.coop.world.RegionOf(r.transform),r.transform.position,Family(b),hp,MaxHealth(b),-1,b);}
            if(game.CoopOnline)foreach(var player in game.coop.players.Values)Track("player/"+player.peer,player.region,new Vector2(player.x,player.y),player.character==1?8:0,player.health,player.character==1?50:100,player.peer);
            else Track("player/0",game.coop.Region,game.body.position,game.survivor.character==CharacterV51.Frog?8:0,game.survivor.health,game.survivor.MaxHealth,0);
        }
        void Track(string actor,string region,Vector2 at,int family,float hp,float maximum,int peer=-1,CoopBindingV49 binding=null)
        {
            if(hp>0){if(dead.Remove(actor)){expired.Clear();foreach(var entry in bodies)if(entry.Value.data.actor==actor)expired.Add(entry.Key);foreach(var key in expired)Remove(key);}return;}
            if(raisedUntil.TryGetValue(actor,out var until) && Time.unscaledTime<until)return;
            if(!dead.Add(actor))return;var data=new CorpseDataV51{key=actor+"/body/"+(++sequence),actor=actor,region=region,x=at.x,y=at.y,family=family,peer=peer,maxHealth=maximum,width=WidthV53(binding,family)};Ensure(data);DeathLootV53(binding,data);
        }
        void DeathLootV53(CoopBindingV49 binding,CorpseDataV51 data)
        {
            // These families already throw their native drops from Hurt().
            if(binding==null || binding.kind=="woodlouse" || binding.kind=="caravan" || binding.kind=="fort" || binding.kind.StartsWith("plain") || binding.driver is VillageActorV46 amazon && !amazon.IsSlave)return;
            if(game.CoopOnline && game.coop.owners.TryGetValue(data.region,out var owner) && owner>0){game.coop.SendToV49(owner,new LanPacketV49{kind="corpseDrop",region=data.region,body=JsonUtility.ToJson(data)});return;}DropBodyLootV53(data);
        }
        public void DropBodyLootV53(CorpseDataV51 data)
        {if(data==null || !droppedV53.Add(data.key))return;ItemKind kind=data.family==4 || data.family==5 || data.family==7?ItemKind.BeetleMeat:data.family==6?ItemKind.RawMeat:ItemKind.Coin;game.ScatterLoot(new Item(kind),new Vector2(data.x,data.y),new Vector2(data.x,data.y),game.coop.world.Root(data.region),data.region);}
        SpriteRenderer SourceV53(CorpseDataV51 data)
        {if(data.peer<0)return game.coop.world.Find(data.actor)?.renders[0];if(!game.CoopOnline || data.peer==game.coop.Peer)return game.hero;return game.coop.remote.TryGetValue(data.peer,out var proxy)?proxy.Renderer:null;}
        void Ensure(CorpseDataV51 data)
        {
            if(!bodies.TryGetValue(data.key,out var body))
            {
                var sprite=game.art.Renderer("Тело / "+data.actor,game.v51Art.corpses[Mathf.Clamp(data.family,0,8)],new Vector2(data.x,data.y),CoastGame.Depth(data.y)+1);sprite.transform.SetParent(game.coop.world.Root(data.region),true);sprite.transform.localScale=Vector3.one*(data.width/sprite.sprite.bounds.size.x);body=new CorpseVisualV51{data=data,sprite=sprite};bodies[data.key]=body;
                var source=SourceV53(data);
                if(data.age<.75f && source && source.sprite)
                {body.fall=game.art.Renderer("Falling creature / "+data.actor,source.sprite,new Vector2(data.x,data.y),sprite.sortingOrder+2);body.fall.transform.SetParent(sprite.transform.parent,true);body.fall.flipX=source.flipX;body.fallScale=source.transform.lossyScale;body.fall.transform.localScale=body.fallScale;body.fallAngle=data.family<=2 || data.family==8?(source.flipX?-78:78):data.family==6?32:12;game.art.PixelStyle(body.fall);}
            }
            body.data=data;PoseBodyV53(body);
        }
        void PoseBodyV53(CorpseVisualV51 body)
        {
            var data=body.data;float t=Mathf.Clamp01(data.age/.7f);body.sprite.enabled=!data.spent && data.age<60;float fade=Mathf.Clamp01((60-data.age)/6);body.sprite.color=new Color(1,1,1,fade*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.36f,.70f,data.age)));
            if(!body.fall)return;
            if(t>=1 || data.spent){Destroy(body.fall.gameObject);body.fall=null;return;}
            float ease=t*t;body.fall.transform.position=new Vector2(data.x,data.y)+new Vector2(Mathf.Sin(t*Mathf.PI)*.12f,Mathf.Sin(t*Mathf.PI)*.16f-.1f*ease);body.fall.transform.rotation=Quaternion.Euler(0,0,body.fallAngle*ease);body.fall.transform.localScale=new Vector3(body.fallScale.x*(1+.09f*t),body.fallScale.y*(1-.34f*t),body.fallScale.z);body.fall.color=new Color(1,1,1,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.5f,.7f,data.age)));
        }
        void Remove(string key){if(bodies.TryGetValue(key,out var body)){if(body.sprite)Destroy(body.sprite.gameObject);if(body.fall)Destroy(body.fall.gameObject);bodies.Remove(key);}}
        void Update()
        {
            if(game==null || game.paused || game.snails.SetupOpen)return;float dt=Time.deltaTime;
            if(Time.unscaledTime>=nextBindings)RefreshBindingsV53();
            if(Authority && Time.unscaledTime>=nextScan){nextScan=Time.unscaledTime+.05f;ScanV53();}
            expired.Clear();foreach(var pair in bodies){var body=pair.Value;body.data.age+=dt;if(body.data.age>=60)expired.Add(pair.Key);else PoseBodyV53(body);}foreach(var key in expired)Remove(key);
            if(Authority && game.CoopOnline && Time.unscaledTime>=nextSync){nextSync=Time.unscaledTime+(bodies.Count>0?.2f:1);game.coop.BroadcastV49(new LanPacketV49{kind="corpses",body=JsonUtility.ToJson(new CorpseListV51{bodies=bodies.Values.Select(b=>b.data).ToArray()})});}
        }
        public void Receive(string json)
        {if(Authority)return;var data=JsonUtility.FromJson<CorpseListV51>(json);var keys=new HashSet<string>((data.bodies??Array.Empty<CorpseDataV51>()).Select(b=>b.key));foreach(var key in bodies.Keys.ToArray())if(!keys.Contains(key))Remove(key);foreach(var body in data.bodies??Array.Empty<CorpseDataV51>())Ensure(body);}
        void LateUpdate()
        {
            if(game==null)return;foreach(var b in cached)if(Eligible(b) && b.health()<=0){foreach(var r in b.renders)if(r)r.enabled=false;if(b.renders[0])foreach(var collider in b.renders[0].GetComponents<Collider2D>())collider.enabled=false;}
            if(!game.Dead)return;game.hero.enabled=false;if(game.gait?.Lower)game.gait.Lower.enabled=false;if(game.combat?.weapon)game.combat.weapon.enabled=false;foreach(var collider in game.hero.GetComponents<Collider2D>())collider.enabled=false;
        }
        public CorpseDataV51 Select(IList<Vector2> polygon,Vector2 centre)
        {return bodies.Values.Where(b=>!b.data.spent && b.data.age<60 && b.data.region==game.coop.Region && GestureShape.Inside(polygon,new Vector2(b.data.x,b.data.y))).OrderBy(b=>Vector2.SqrMagnitude(new Vector2(b.data.x,b.data.y)-centre)).Select(b=>b.data).FirstOrDefault();}
        public bool RequestRaise(CorpseDataV51 body,Item book)
        {
            if(body==null || Pending!=null)return false;int cost=BookSpells.Cost(book,30);if(game.survivor.mana<cost){game.ui.Notice("Недостаточно маны");return false;}
            Pending=Guid.NewGuid().ToString("N");pendingCost=cost;game.survivor.SpendMana(cost);
            var request=new RaiseRequestV51{key=body.key,cost=cost,request=Pending};if(game.CoopOnline)game.coop.Send(new LanPacketV49{kind="raise",region=game.coop.Region,body=JsonUtility.ToJson(request)});else FinishRaise(request,0,game.coop.Region);return true;
        }
        public void FinishRaise(RaiseRequestV51 request,int peer,string region)
        {
            bool success=false;
            if(request!=null && bodies.TryGetValue(request.key,out var body) && !body.data.spent && body.data.age<60 && body.data.region==region)
            {
                var data=body.data;
                if(data.peer<0 || !game.CoopOnline || game.coop.players.ContainsKey(data.peer))
                {
                    data.spent=true;success=true;raisedUntil[data.actor]=Time.unscaledTime+2;
                    if(data.peer>=0)
                    {if(!game.CoopOnline || data.peer==game.coop.Peer)RaiseLocalPlayer(data);else game.coop.SendToV49(data.peer,new LanPacketV49{kind="raisePlayer",region=data.region,body=JsonUtility.ToJson(data)});}
                    else
                    {RaiseActor(data);if(game.CoopOnline && game.coop.owners.TryGetValue(region,out var authority) && authority>0)game.coop.SendToV49(authority,new LanPacketV49{kind="raiseActor",region=region,body=JsonUtility.ToJson(data)});}
                }
            }
            var result=new RaiseResultV51{request=request?.request,cost=request?.cost??0,success=success};if(!game.CoopOnline || peer==game.coop.Peer)RaiseResult(result);else game.coop.SendToV49(peer,new LanPacketV49{kind="raiseResult",body=JsonUtility.ToJson(result)});
        }
        public void RaiseResult(RaiseResultV51 result)
        {if(result==null || result.request!=Pending)return;if(!result.success){game.survivor.mana=Mathf.Min(game.survivor.MaxMana,game.survivor.mana+pendingCost);game.ui.Notice("Нет цели");}Pending=null;pendingCost=0;}
        public void RaiseActor(CorpseDataV51 data)
        {
            var b=game.coop.world.Find(data.actor);if(b==null || !b.renders[0])return;b.setHealth(data.maxHealth*.5f);
            var main=b.renders[0];foreach(var r in b.renders)if(r){r.enabled=r==main || r.name.ToLowerInvariant().Contains("shadow");r.color=Color.white;}
            foreach(var collider in main.GetComponents<Collider2D>())collider.enabled=true;
            if(b.driver is VillageActorV46 woman){woman.PeaceV53();woman.Warp(new Vector2(data.x,data.y));}else if(b.driver is CompanionMage mage)mage.Warp(new Vector2(data.x,data.y));else if(b.driver is Woodlouse bug){bug.Grabbed=false;bug.vitals.curlRemaining=0;}else if(b.driver is FrogMage frog){frog.PeaceV53();frog.Warp(new Vector2(data.x,data.y));}else if(b.driver is TavernResident resident)resident.PeaceV53();else if(b.driver is CaravanDriver driver)driver.PeaceV53();else if(b.driver is BeachGull gull){gull.point=new Vector2(data.x,data.y);gull.home=gull.point;gull.state=1;gull.age=0;}else if(b.driver is CaveSpider spider){spider.Grabbed=spider.Attacking=spider.Biting=false;spider.cooldown=1;}
            else if(b.driver is WormPart part && !game.cave.worms.Any(w=>w.parts.Contains(part))){var worm=new SpectralWorm(game.cave);worm.parts.Add(part);worm.ResetTrail();game.cave.worms.Add(worm);}
            if(game.spellcraft.statuses.TryGetValue(main,out var status))status.shackles=status.curse=0;
            if(game.journey.companion.target==main)game.journey.companion.target=null;foreach(var ally in game.amazons.people)ally.ForgetTargetV53(main);game.PlayerStruck.Remove(main);game.feedback.Forget(main);Pulse(new Vector2(data.x,data.y),main.transform.parent);
        }
        public void RaiseLocalPlayer(CorpseDataV51 data)
        {
            game.survivor.health=game.survivor.MaxHealth*.5f;game.body.position=new Vector2(data.x,data.y);game.hero.transform.position=game.body.position;game.body.simulated=true;game.hero.enabled=true;foreach(var collider in game.hero.GetComponents<Collider2D>())collider.enabled=true;game.combat.ResetForCheck();game.ResetCamera(new Vector3(data.x,data.y+2,-20));Pulse(game.body.position,game.arcane.WorldRoot);
        }
        void Pulse(Vector2 at,Transform parent)
        {for(int i=0;i<14;i++){var r=game.art.Renderer("Return life spark",game.v37Art.mote,at+new Vector2(Mathf.Cos(i*2.4f),Mathf.Sin(i*2.4f))*.65f,CoastGame.Depth(at.y)+4);r.transform.SetParent(parent,true);r.transform.localScale=Vector3.one*.05f;r.color=new Color(.65f,.88f,1);Destroy(r.gameObject,.8f);}game.PlayPickup();}
    }
    public sealed partial class CoopWorldV49
    {public IEnumerable<CoopBindingV49> BindingsV51(){Refresh();return entities.Values;}}
}
