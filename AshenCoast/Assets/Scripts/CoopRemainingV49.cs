using System;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class BuildingSystem
    {
        void RecordPlacementV49(Item item,Vector2Int cell,bool upright)
        {
            if(!game.CoopOnline || game.coop.ApplyingNetwork || !game.coop.inventory.Mutating)return;
            var net=game.coop;string key=item.kind==ItemKind.Campfire?net.Region+"/fire/"+cell.x+"/"+cell.y:net.inventory.Key(stations.Last().inventory);
            net.inventory.Action(new LanWorldActionV49{kind="build",key=key,region=net.Region,x=cell.x,y=cell.y,value=(int)item.kind,upright=upright,item=LanItemV49.Capture(item)});
        }
    }
    public sealed partial class CoopSessionV49
    {
        public CoopFxV49 fx;public bool HirePendingV49;
        float networkBase,networkReceived;
        public float NetworkTimeV49=>IsHost?Time.realtimeSinceStartup:networkBase+Time.realtimeSinceStartup-networkReceived;
        public void SendDpsV49(int peer,string key,float amount)
        {
            var p=new LanPacketV49{kind="dps",target=peer,region=Region,body=JsonUtility.ToJson(new LanDamageV49{key=key,amount=amount})};
            if(IsHost)SendToV49(peer,p);else Send(p);
        }
    }
    [Serializable] public sealed class LanClockV49 {public float seconds,networkTime;}
    [Serializable] public sealed class LanWebV49 {public float x,y,radius,age;}
    [Serializable] public sealed class LanFogV49 {public float x,y,remaining;}
    public sealed partial class CoopWorldV49
    {
        public void UpdateAlliesV49()
        {
            foreach(var pair in allyOwners.ToArray())
            {
                var b=Find(pair.Key);if(b==null)continue;if(G.snails.Resting(b.renders[0]) || G.snails.Returning(b.renders[0]))continue;if(!session.players.TryGetValue(pair.Value,out var player)){b.renders[0].gameObject.SetActive(b.region==session.Region);continue;}
                bool moved=!allyRegions.TryGetValue(pair.Key,out var previous) || previous!=player.region;
                allyRegions[pair.Key]=b.region=player.region;
                if(moved)
                {
                    var point=new Vector2(player.x-1.15f,player.y-.45f);
                    if(b.driver is VillageActorV46 woman){woman.HoldPosition=false;woman.Warp(point);}else if(b.driver is CompanionMage mage){mage.remoteArea=null;mage.Warp(point);}
                    foreach(var r in b.renders)if(r && !r.transform.IsChildOf(b.renders[0].transform))r.transform.SetParent(Root(player.region),true);
                }
                b.renders[0].transform.SetParent(Root(player.region),true);b.renders[0].gameObject.SetActive(player.region==session.Region);
            }
        }
        public void ExistingAlliesV49()
        {
            foreach(var b in entities.Values)if(b.driver is VillageActorV46 v && v.Hired || b.driver is CompanionMage m && m.Hired){allyOwners[b.key]=0;allyRegions[b.key]=session.Region;}
        }
        public LanWebV49[] CaptureWebsV49(string region)=>region=="area/4"?G.cave.webs.Select(w=>new LanWebV49{x=w.point.x,y=w.point.y,radius=w.radius,age=w.age}).ToArray():Array.Empty<LanWebV49>();
        public void ApplyWebsV49(LanWebV49[] data)
        {
            if(data==null)return;
            foreach(var w in G.cave.webs)if(w.sprite)UnityEngine.Object.Destroy(w.sprite.gameObject);G.cave.webs.Clear();
            foreach(var d in data.Take(150)){var w=G.cave.AddWeb(new Vector2(d.x,d.y));w.radius=d.radius;w.age=d.age;}
        }
        public float StatusV49(SpriteRenderer actor,bool curse)=>G.spellcraft.statuses.TryGetValue(actor,out var s)?curse?s.curse:s.shackles:0;
        public void ApplyStatusV49(CoopBindingV49 b,LanActorV49 data,bool takeover)
        {
            if(!b.renders[0])return;var actor=b.renders[0];
            if(!G.spellcraft.statuses.TryGetValue(actor,out var status)){status=new SpellStatus{actor=actor};G.spellcraft.statuses[actor]=status;}
            if(takeover && b.target!=null){if(data.bound>0)G.spellcraft.Bind(b.target);if(data.curse>0)G.spellcraft.Curse(b.target);}
            status.shackles=data.bound;status.curse=data.curse;status.anchor=new Vector2(data.anchorX,data.anchorY);G.v44.SetNetworkV49(actor,data.stun,takeover);
        }
        public LanFogV49[] CaptureFogsV49(string region)=>G.spellcraft.fogs.Where(f=>RegionOf(f.world)==region).Select(f=>new LanFogV49{x=f.point.x,y=f.point.y,remaining=f.remaining}).ToArray();
        public void ApplyFogsV49(LanFogV49[] data,string region,bool takeover)
        {
            if(data==null)return;var root=Root(region);var present=G.spellcraft.fogs.Where(f=>f.world==root).ToArray();
            foreach(var f in present)if(!data.Any(d=>Vector2.Distance(f.point,new Vector2(d.x,d.y))<.05f)){if(f.visual)UnityEngine.Object.Destroy(f.visual);G.spellcraft.fogs.Remove(f);}
            foreach(var d in data.Take(40))
            {
                var fog=G.spellcraft.fogs.FirstOrDefault(f=>f.world==root && Vector2.Distance(f.point,new Vector2(d.x,d.y))<.05f);
                if(fog==null){fog=new AbyssFog{point=new Vector2(d.x,d.y),world=root,visual=new GameObject("Network fog mechanics")};fog.visual.transform.SetParent(root,false);G.spellcraft.fogs.Add(fog);}
                if(takeover && !fog.visual.GetComponent<Renderer>()){UnityEngine.Object.Destroy(fog.visual);G.spellcraft.fogs.Remove(fog);G.spellcraft.CreateFog(fog.point);fog=G.spellcraft.fogs.Last();}
                fog.remaining=d.remaining;
            }
        }
    }
    public sealed partial class CombatStatusV44
    {
        public float RemainingV49(SpriteRenderer actor)=>actor && stuns.TryGetValue(actor,out var s)?Mathf.Max(0,s.until-Time.time):0;
        public void SetNetworkV49(SpriteRenderer actor,float remaining,bool takeover)
        {
            if(remaining<=0){if(stuns.TryGetValue(actor,out var old))old.until=Time.time;return;}
            if(!stuns.TryGetValue(actor,out var s)){s=new Stun{actor=actor};stuns[actor]=s;}
            if(takeover && s.particles.Count==0)ApplyActor(actor);s.until=Time.time+remaining;
        }
    }
}
