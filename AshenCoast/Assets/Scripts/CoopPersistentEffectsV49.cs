using System;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    [Serializable] public sealed class LanPullV49 {public string actor;public float age,x,y;}
    [Serializable] public sealed class LanTentacleV49
    {public string key;public int caster;public float x,y,radius,age;public LanPullV49[] woodlice,spiders;}
    [Serializable] public sealed class LanInvocationV49 {public float x,y,age;}
    public sealed partial class CoopWorldV49
    {
        public LanTentacleV49[] CaptureTentaclesV49(string region)=>G.arcane.fields.Where(f=>RegionOf(f.root.transform)==region).Select(f=>new LanTentacleV49{key=f.netKeyV49,caster=session.PlayerPeer(f.caster),x=f.position.x,y=f.position.y,radius=f.radius,age=f.age,woodlice=f.pulled.Select(p=>new LanPullV49{actor=Key(p.mob.renderer),age=p.age,x=p.start.x,y=p.start.y}).ToArray(),spiders=f.pulledSpiders.Select(p=>new LanPullV49{actor=Key(p.spider.Renderer),age=p.age,x=p.start.x,y=p.start.y}).ToArray()}).ToArray();
        public void ApplyTentaclesV49(LanTentacleV49[] data,string region,bool takeover)
        {
            if(data==null)return;
            foreach(var old in G.arcane.fields.Where(f=>RegionOf(f.root.transform)==region).ToArray())
            {if(!takeover || !data.Any(d=>d.key==old.netKeyV49)){old.Dispose();G.arcane.fields.Remove(old);}}
            if(!takeover)return;
            foreach(var d in data.Take(30))
            {
                var f=G.arcane.fields.FirstOrDefault(f=>f.netKeyV49==d.key);if(f==null){f=new TentacleField(G.arcane,new Vector2(d.x,d.y),Mathf.Clamp(d.radius,1,6));f.root.transform.SetParent(Root(region),true);G.arcane.fields.Add(f);}
                f.netKeyV49=d.key;f.caster=d.caster==session.Peer?G.hero:session.remote.TryGetValue(d.caster,out var peer)?peer.Renderer:G.hero;f.ResumeNetworkV49(d.age);
                foreach(var p in d.woodlice??Array.Empty<LanPullV49>())if(Find(p.actor)?.driver is Woodlouse mob){mob.Grabbed=true;f.pulled.Add(new PulledCreature{mob=mob,age=p.age,start=new Vector2(p.x,p.y)});}
                foreach(var p in d.spiders??Array.Empty<LanPullV49>())if(Find(p.actor)?.driver is CaveSpider spider){spider.Grabbed=true;f.pulledSpiders.Add(new PulledSpider{spider=spider,age=p.age,start=new Vector2(p.x,p.y)});}
            }
        }
        public LanInvocationV49[] CaptureInvocationsV49(string region)=>G.dragons.summons.Where(s=>RegionOf(s.world)==region).Select(s=>new LanInvocationV49{x=s.position.x,y=s.position.y,age=s.age}).ToArray();
        public void ApplyInvocationsV49(LanInvocationV49[] data,string region,bool takeover)
        {
            if(data==null)return;
            foreach(var old in G.dragons.summons.Where(s=>RegionOf(s.world)==region).ToArray())
            {if(!takeover || !data.Any(d=>Vector2.Distance(old.position,new Vector2(d.x,d.y))<.05f)){if(old.pillar)UnityEngine.Object.Destroy(old.pillar.gameObject);G.dragons.summons.Remove(old);}}
            if(takeover)foreach(var d in data.Take(4)){var summon=G.dragons.summons.FirstOrDefault(s=>s.world==Root(region) && Vector2.Distance(s.position,new Vector2(d.x,d.y))<.05f);if(summon==null){summon=new DragonSummon{position=new Vector2(d.x,d.y),world=Root(region)};G.dragons.summons.Add(summon);}summon.age=d.age;}
        }
    }
    public sealed partial class TentacleField
    {
        public string netKeyV49=Guid.NewGuid().ToString("N");
        public void ResumeNetworkV49(float elapsed){age=elapsed;nextDamage=.7f+Mathf.Max(0,Mathf.Floor((age-.7f)/.4f)+1)*.4f;}
    }
}
