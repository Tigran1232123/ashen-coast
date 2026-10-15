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
    public sealed class CorpseVisualV51 {public CorpseDataV51 data;public SpriteRenderer sprite;}
    [DefaultExecutionOrder(700)]
    public sealed class CorpseWorldV51:MonoBehaviour
    {
        public CoastGame game;public readonly Dictionary<string,CorpseVisualV51> bodies=new Dictionary<string,CorpseVisualV51>();
        readonly HashSet<string> dead=new HashSet<string>();readonly Dictionary<string,float> raisedUntil=new Dictionary<string,float>();float nextSync;int sequence;public string Pending;int pendingCost;
        bool Authority=>!game.CoopOnline || game.coop.IsHost;
        public void Initialize(CoastGame g){game=g;}
        public bool HasBody(string actor)=>bodies.Values.Any(b=>b.data.actor==actor && !b.data.spent && b.data.age<60);
        public IEnumerable<CoopBindingV49> Bindings=>game.coop.world.BindingsV51();
        public int Family(CoopBindingV49 b)=>b.kind=="woodlouse" || b.kind=="caravan"?3:b.kind=="frog"?4:b.kind=="spider"?5:b.kind=="gull"?6:b.kind=="worm"?7:b.kind=="mage"?1:b.driver is VillageActorV46 woman && !woman.IsSlave?2:0;
        public float MaxHealth(CoopBindingV49 b)=>b.driver is VillageActorV46 woman?woman.MaxHealth:b.driver is Woodlouse bug?bug.vitals.maximumHealth:b.kind=="mage"?CompanionMage.MaxHealth:b.kind=="frog"?FrogMage.MaximumHealth:b.kind=="spider"?80:b.kind=="gull"?30:b.kind=="worm"?WormPart.MaximumHealth:b.kind=="caravan"?WoodlouseVitals.MaximumHealth:100;
        float Width(int family)=>family==3?2.6f:family==4?2.3f:family==5?1.35f:family==6?.85f:family==7?1.5f:1.85f;
        void Track(string actor,string region,Vector2 at,int family,float hp,float maximum,int peer=-1)
        {
            if(hp>0){dead.Remove(actor);foreach(var body in bodies.Values.Where(b=>b.data.actor==actor).ToArray())Remove(body.data.key);return;}
            if(raisedUntil.TryGetValue(actor,out var until) && Time.unscaledTime<until)return;
            if(!dead.Add(actor))return;var data=new CorpseDataV51{key=actor+"/body/"+(++sequence),actor=actor,region=region,x=at.x,y=at.y,family=family,peer=peer,maxHealth=maximum,width=Width(family)};Ensure(data);
        }
        void Ensure(CorpseDataV51 data)
        {
            if(!bodies.TryGetValue(data.key,out var body))
            {var sprite=game.art.Renderer("Тело / "+data.actor,game.v51Art.corpses[Mathf.Clamp(data.family,0,8)],new Vector2(data.x,data.y),CoastGame.Depth(data.y)+1);sprite.transform.SetParent(game.coop.world.Root(data.region),true);sprite.transform.localScale=Vector3.one*(data.width/sprite.sprite.bounds.size.x);body=new CorpseVisualV51{data=data,sprite=sprite};bodies[data.key]=body;}
            body.data=data;body.sprite.enabled=!data.spent && data.age<60;body.sprite.color=new Color(1,1,1,Mathf.Clamp01((60-data.age)/6));
        }
        void Remove(string key){if(bodies.TryGetValue(key,out var body)){if(body.sprite)Destroy(body.sprite.gameObject);bodies.Remove(key);}}
        void Update()
        {
            if(game==null || game.paused || game.snails.SetupOpen)return;float dt=Time.deltaTime;
            if(Authority)
            {
                foreach(var b in Bindings.ToArray())if(b.renders[0] && b.kind!="dummy" && b.kind!="target" && b.kind!="tree" && b.kind!="dragon" && b.kind!="hunter")Track(b.key,game.coop.world.RegionOf(b.renders[0].transform),b.renders[0].transform.position,Family(b),b.health(),MaxHealth(b));
                if(game.CoopOnline)foreach(var p in game.coop.players.Values)Track("player/"+p.peer,p.region,new Vector2(p.x,p.y),p.character==1?8:0,p.health,p.character==1?50:100,p.peer);
                else Track("player/0",game.coop.Region,game.body.position,game.survivor.character==CharacterV51.Frog?8:0,game.survivor.health,game.survivor.MaxHealth,0);
            }
            foreach(var body in bodies.Values.ToArray()){body.data.age+=dt;if(body.data.age>=60)Remove(body.data.key);else Ensure(body.data);}
            if(Authority && game.CoopOnline && Time.unscaledTime>=nextSync){nextSync=Time.unscaledTime+.2f;game.coop.BroadcastV49(new LanPacketV49{kind="corpses",body=JsonUtility.ToJson(new CorpseListV51{bodies=bodies.Values.Select(b=>b.data).ToArray()})});}
        }
        public void Receive(string json)
        {if(Authority)return;var data=JsonUtility.FromJson<CorpseListV51>(json);var keys=new HashSet<string>((data.bodies??Array.Empty<CorpseDataV51>()).Select(b=>b.key));foreach(var key in bodies.Keys.ToArray())if(!keys.Contains(key))Remove(key);foreach(var body in data.bodies??Array.Empty<CorpseDataV51>())Ensure(body);}
        void LateUpdate()
        {
            if(game==null)return;foreach(var b in Bindings)if(b.kind!="dummy" && b.kind!="target" && b.kind!="tree" && b.kind!="dragon" && b.kind!="hunter" && b.health()<=0){foreach(var r in b.renders)if(r)r.enabled=false;if(b.renders[0])foreach(var collider in b.renders[0].GetComponents<Collider2D>())collider.enabled=false;}
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
            if(b.driver is VillageActorV46 woman)woman.Warp(new Vector2(data.x,data.y));else if(b.driver is CompanionMage mage)mage.Warp(new Vector2(data.x,data.y));else if(b.driver is Woodlouse bug){bug.Grabbed=false;bug.vitals.curlRemaining=0;}else if(b.driver is FrogMage frog)frog.Warp(new Vector2(data.x,data.y));else if(b.driver is CaveSpider spider){spider.Grabbed=spider.Attacking=spider.Biting=false;spider.cooldown=1;}
            else if(b.driver is WormPart part && !game.cave.worms.Any(w=>w.parts.Contains(part))){var worm=new SpectralWorm(game.cave);worm.parts.Add(part);worm.ResetTrail();game.cave.worms.Add(worm);}
            if(game.spellcraft.statuses.TryGetValue(main,out var status))status.shackles=status.curse=0;
            game.feedback.Forget(main);Pulse(new Vector2(data.x,data.y),main.transform.parent);
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
