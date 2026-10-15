using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    [Serializable] public sealed class SeedManifestV56
    {
        public int seed,villageSeed;public float phase;
        public Vector2 fort,cave,centre;
        public Vector2[] houses,barriers,trees,roads,fortColliders,caveColliders;
        public FlowerSite[] plants;
    }
    public sealed class ChecksWorldSeedV56:MonoBehaviour
    {
        public CoastGame game;string folder,role;int passed,failed;
        readonly List<string> report=new List<string>();
        string FileFor(string name)=>Path.Combine(folder,name);
        void Mark(string name)=>File.WriteAllText(FileFor(name),"ready");
        void Check(bool ok,string label)
        {string line=(ok?"PASS ":"FAIL ")+label;report.Add(line);Debug.Log(line);if(ok)passed++;else failed++;}
        IEnumerator Until(Func<bool> predicate,string label,float seconds=45)
        {float until=Time.realtimeSinceStartup+seconds;while(!predicate() && Time.realtimeSinceStartup<until)yield return null;Check(predicate(),label);}
        static Vector2[] Colliders(GameObject root)=>root.GetComponentsInChildren<Collider2D>(true).Select(c=>(Vector2)c.transform.TransformPoint(c.offset)).ToArray();
        static bool Same(Vector2 a,Vector2 b)=>Vector2.Distance(a,b)<.002f;
        static bool Same(Vector2[] a,Vector2[] b)=>a.Length==b.Length && a.Where((p,i)=>!Same(p,b[i])).Count()==0;
        SeedManifestV56 Capture()=>new SeedManifestV56{
            seed=game.meadow.Seed,villageSeed=game.amazons.LayoutSeedV54,phase=WorldShape.Phase,
            fort=game.fort.Position,cave=game.cave.Position,centre=game.amazons.Centre,
            houses=game.amazons.houses.Select(h=>h.entrance).ToArray(),
            barriers=game.amazons.barriers.Select(b=>b.center).ToArray(),
            trees=game.amazons.trees.Select(t=>t.position).ToArray(),roads=game.amazons.RoadNodesV54.ToArray(),
            fortColliders=Colliders(game.fort.exteriorRootV56),caveColliders=Colliders(game.cave.entranceRootV56),
            plants=MeadowLayout.Generate(new Vector2Int(3,6),game.meadow.Seed).ToArray()};
        void Compare(SeedManifestV56 host)
        {
            var actual=Capture();
            Check(actual.seed==host.seed && WorldShape.Seed==host.seed,"guest adopts both host generation seeds");
            Check(Mathf.Abs(actual.phase-host.phase)<.00001f && Mathf.Abs(Shader.GetGlobalFloat("_LandPhase")-host.phase)<.00001f,"coast outline and terrain shader match host");
            Check(Same(actual.fort,host.fort) && Same(game.fort.exterior.transform.position,host.fort),"fort location and its rendered gateway match host");
            Check(Same(actual.fortColliders,host.fortColliders),"all exterior fortress colliders move with the gateway");
            Check(Same(actual.cave,host.cave) && Same(game.cave.entrance.transform.position,host.cave),"catacomb location and its rendered entrance match host");
            Check(Same(actual.caveColliders,host.caveColliders),"catacomb entrance colliders match host");
            Check(Same(actual.centre,host.centre) && WorldShape.AmazonPosition.HasValue && Same(WorldShape.AmazonPosition.Value,host.centre),"village centre and meadow exclusion match host");
            Check(actual.villageSeed==host.villageSeed && Same(actual.houses,host.houses),"every household entrance matches host");
            Check(Same(actual.barriers,host.barriers) && Same(actual.trees,host.trees),"village obstacle and tree data match host");
            Check(Same(actual.roads,host.roads),"village route graph matches host");
            Check(actual.plants.Length==host.plants.Length && actual.plants.Where((p,i)=>p.id!=host.plants[i].id || p.type!=host.plants[i].type || !Same(p.position,host.plants[i].position)).Count()==0,"procedural flower identities, kinds and positions match host");
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();folder=args[Array.IndexOf(args,"-seed-output")+1];role=args[Array.IndexOf(args,"-seed-role")+1];Directory.CreateDirectory(folder);
            game.snails.SetupOpen=false;game.paused=false;game.SetAutoPickup(false);Time.timeScale=1;
            Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=30;AudioListener.volume=0;
            Check(!game.testMode,"normal random-world initialization path is used, without the old shared test seed");
            Check(Application.version==LanProtocolV49.Build.ToString()+".0" && LanProtocolV49.Build==57 && new LanHelloV49().build==LanProtocolV49.Build && new LanPacketV49().version==LanProtocolV49.Build,"application and handshake identify V56");
            yield return null;
            var run=role=="guest"?Guest():Host();
            while(true){bool more;object current=null;try{more=run.MoveNext();if(more)current=run.Current;}catch(Exception e){Check(false,e.ToString());break;}if(!more)break;yield return current;}
            report.Add("RESULT "+passed+" passed; "+failed+" failed");File.WriteAllLines(FileFor(role+"-results.txt"),report);
            game.coop.Disconnect(false);Application.Quit(failed==0?0:1);
        }
        IEnumerator Host()
        {
            int port=role=="host-a"?27956:27957;
            Check(game.coop.StartHost(port,true),role+" starts TCP listener");
            File.WriteAllText(FileFor(role+".json"),JsonUtility.ToJson(Capture()));Mark(role+"-ready");
            yield return Until(()=>File.Exists(FileFor("guest-done")),"guest completed distinct seeds, reconnection and host switch",150);
            Check(game.coop.Online,"host remains running throughout guest world adoption");
        }
        IEnumerator Guest()
        {
            var s=game.coop;
            yield return Until(()=>File.Exists(FileFor("host-a-ready")) && File.Exists(FileFor("host-b-ready")),"both independently generated host worlds are ready");
            var a=JsonUtility.FromJson<SeedManifestV56>(File.ReadAllText(FileFor("host-a.json")));
            var b=JsonUtility.FromJson<SeedManifestV56>(File.ReadAllText(FileFor("host-b.json")));
            int ownSeed=game.meadow.Seed;
            Check(ownSeed!=a.seed && ownSeed!=b.seed && a.seed!=b.seed,"all three processes start with explicitly different seeds");
            Check(!Same(a.centre,b.centre),"second host exercises relocation to a different village centre");
            bool oldFailure=a.seed!=WorldShape.Seed;
            Check(oldFailure,"V55 welcome's equality check would reject this real-world connection");
            const long remembered=5600000000001;
            game.meadow.SetPickedV49(remembered,true);
            var oldMap=game.map.regions[0];var inventoryItem=game.survivor.inventory.items.First();string itemId=inventoryItem.id;
            s.Join("127.0.0.1",27956);
            yield return Until(()=>s.Online && s.inventory.Ready,"guest joins a host with a different seed and receives snapshots");
            if(!s.Online){Mark("guest-done");yield break;}
            Compare(a);
            Check(!game.meadow.WasPicked(remembered) && !ReferenceEquals(oldMap,game.map.regions[0]),"different-world flower cache and explored map are rebuilt");
            Check(game.survivor.inventory.items.Any(i=>i.id==itemId),"guest keeps personal inventory while adopting the world");
            game.meadow.SetPickedV49(remembered,true);var hostMap=game.map.regions[0];
            yield return new WaitForSeconds(8);
            Check(s.Online && string.IsNullOrEmpty(s.SyncErrorReportV55),"guest stays synchronized through live snapshots without a sync exception");
            s.Disconnect(false);yield return new WaitForSeconds(.8f);s.Join("127.0.0.1",27956);
            yield return Until(()=>s.Online && s.inventory.Ready,"guest reconnects to the same host");
            Check(game.meadow.WasPicked(remembered) && ReferenceEquals(hostMap,game.map.regions[0]),"same-world reconnect retains pickup memory and explored map");
            Compare(a);
            s.Disconnect(false);yield return new WaitForSeconds(.8f);s.Join("127.0.0.1",27957);
            yield return Until(()=>s.Online && s.inventory.Ready,"guest switches to a second host with a different world seed");
            Compare(b);
            Check(!game.meadow.WasPicked(remembered) && !ReferenceEquals(hostMap,game.map.regions[0]),"switching worlds invalidates the previous host's terrain caches");
            Check(game.survivor.inventory.items.Any(i=>i.id==itemId),"inventory survives a second world adoption");
            var target=game.amazons.houses[0].entrance;game.journey.SetArea(WorldArea.Coast,target+Vector2.down*2);
            game.meadow.Refresh(game.body.position,true);yield return new WaitForSeconds(2);
            Check(s.Online && game.meadow.LoadedChunks>0,"new host terrain streams near the relocated village");
            Compare(b);
            Check(string.IsNullOrEmpty(s.SyncErrorReportV55),"no synchronization error after switching host and streaming terrain");
            Mark("guest-done");
        }
    }
}
