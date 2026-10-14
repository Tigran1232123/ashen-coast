using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class LanNativeChecksV49:MonoBehaviour
    {
        public CoastGame game;string directory,role;int number;bool failed;readonly List<string> report=new List<string>();
        void Check(bool ok,string name){var line=(ok?"PASS ":"FAIL ")+name;report.Add(line);if(!ok)failed=true;Debug.Log(line);}
        string FilePath(string name)=>Path.Combine(directory,name);
        void Mark(string name,string text="ready")=>File.WriteAllText(FilePath(name),text);
        string Stage=>File.Exists(FilePath("stage.txt"))?File.ReadAllText(FilePath("stage.txt")):"";
        void Step(string stage)=>Mark("stage.txt",stage);
        IEnumerator Until(Func<bool> condition,float timeout=35)
        {float end=Time.realtimeSinceStartup+timeout;while(!condition() && Time.realtimeSinceStartup<end)yield return null;Check(condition(),"LAN barrier: "+role+number+" "+Stage);}
        void Warp(WorldArea area,Vector2 point){game.journey.SetArea(area,point);game.hero.transform.position=point;game.body.position=point;game.seconds=60;}
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();string Arg(string key,string fallback){int i=Array.IndexOf(args,key);return i>=0 && i+1<args.Length?args[i+1]:fallback;}
            directory=Arg("-lan-output",Path.Combine(Application.persistentDataPath,"LANVerification"));Directory.CreateDirectory(directory);role=Arg("-lan-role","host");number=int.Parse(Arg("-lan-number","0"));
            Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=60;AudioListener.volume=0;yield return null;
            bool remaining=args.Contains("-lan-remaining-check");bool hire=args.Contains("-lan-hire-check");IEnumerator routine=args.Contains("-lan-pocket-check")?PocketChecksV49():hire?(role=="host"?HireHostV49():HireClientV49()):remaining?(role=="host"?RemainingHostV49():RemainingClientV49()):(role=="host"?Host():Client());
            while(true){bool move;object current=null;try{move=routine.MoveNext();if(move)current=routine.Current;}catch(Exception e){Check(false,e.ToString());break;}if(!move)break;yield return current;}
            report.Add("RESULT "+(failed?"FAILED":"PASSED"));File.WriteAllLines(FilePath(role+number+"-report.txt"),report);Application.Quit(failed?1:0);
        }
        IEnumerator Host()
        {
            var g=game;var s=g.coop;s.PlayerName="Host";Check(s.StartHost(29449,true),"TCP host starts on a real loopback listener");Mark("host-ready.txt");
            yield return Until(()=>s.players.Count==4 && s.remote.Count==3,45);Check(s.players.Values.All(p=>p.region=="area/0"),"four real game processes initially share the coast");
            g.checkCameraSize=6;g.checkCameraPosition=new Vector3(-3,0,-20);g.ResetCamera(g.checkCameraPosition.Value);s.MenuOpen=true;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(FilePath("v49-lan-four-players-menu.png"));yield return null;s.MenuOpen=false;
            var fifth=new LanTransportV49();fifth.Connect("127.0.0.1",29449);yield return new WaitForSecondsRealtime(.6f);Check(s.players.Count==4 && s.transport.ConnectionCount==3,"fifth connection cannot enter the four-player session");fifth.Dispose();
            Step("independent");yield return Until(()=>Enumerable.Range(1,3).All(n=>File.Exists(FilePath("client"+n+"-independent.txt"))));
            Check(s.players.Values.Select(p=>p.region).Distinct().Count()==4,"coast, fort, catacombs and Amazon house run independently");
            Check(s.owners["area/3"]==s.players.Values.First(p=>p.name=="Client1").peer && s.owners["area/4"]==s.players.Values.First(p=>p.name=="Client2").peer,"simulation authority is assigned separately per occupied location");
            Warp(WorldArea.Fort,new Vector2(0,-12));yield return new WaitForSecondsRealtime(.8f);Check(Mathf.Abs(g.fort.bandits[0].health-70)<.01f,"entering another player's fort restores its already damaged enemy");
            Check(s.CanSimulate,"host takes over the fort when joining its occupants");Warp(WorldArea.Coast,g.ShipBase+Vector2.right*3);Step("return");yield return Until(()=>s.players.Values.All(p=>p.region=="area/0"));yield return new WaitForSecondsRealtime(.6f);
            string id=g.shipStorage.items.First(i=>i.kind==ItemKind.Clay).id;Mark("race-item.txt",id);Step("race");yield return Until(()=>File.Exists(FilePath("client1-race.txt")) && File.Exists(FilePath("client2-race.txt")));
            int winners=int.Parse(File.ReadAllText(FilePath("client1-race.txt")))+int.Parse(File.ReadAllText(FilePath("client2-race.txt")));
            Check(winners==1 && !g.shipStorage.items.Any(i=>i.id==id),"simultaneous shared-storage takes produce exactly one owner and no duplicate");
            Step("drop");yield return Until(()=>File.Exists(FilePath("drop-key.txt")));Step("pickup");yield return Until(()=>File.Exists(FilePath("client3-pickup.txt")));
            string dropKey=File.ReadAllText(FilePath("drop-key.txt"));Check(g.loot.Single(l=>l.netIdV49==dropKey).collected,"shared drop is marked collected on the host after another player picks it up");
            Step("redrop");yield return Until(()=>File.Exists(FilePath("second-drop-key.txt")));Check(File.ReadAllText(FilePath("second-drop-key.txt"))!=dropKey,"dropping the same item creates a distinct world instance");
            Step("repick");yield return Until(()=>File.Exists(FilePath("client1-repick.txt")));Check(File.ReadAllText(FilePath("client1-repick.txt"))=="1","previously picked item can be dropped and picked again by a teammate");
            Step("leave");yield return Until(()=>s.players.Count==3);Check(s.Online && s.transport.ConnectionCount==2,"one client disconnects without ending the remaining session");
            Warp(WorldArea.Fort,new Vector2(0,-12));yield return new WaitForSecondsRealtime(.6f);Check(Mathf.Abs(g.fort.bandits[0].health-70)<.01f,"region state survives its former simulator disconnecting");
            Step("done");yield return new WaitForSecondsRealtime(.5f);s.Disconnect();
        }
        IEnumerator Client()
        {
            var g=game;var s=g.coop;s.PlayerName="Client"+number;yield return Until(()=>File.Exists(FilePath("host-ready.txt")));s.Join("127.0.0.1",29449);yield return Until(()=>s.Online && s.inventory.Ready,45);Check(s.Peer>0 && s.players.Count>=1,"client receives its assigned peer ID and shared inventory snapshot");
            yield return Until(()=>Stage=="independent");
            if(number==1){Warp(WorldArea.Fort,new Vector2(0,-12));yield return Until(()=>s.CanSimulate);var enemy=g.fort.bandits[0];g.body.position=enemy.Position+Vector2.left;g.hero.transform.position=g.body.position;g.HurtTarget(enemy,30,g.hero);s.PublishWorldV49();Check(Mathf.Abs(enemy.health-70)<.01f,"fort owner applies real combat damage");}
            else if(number==2){Warp(WorldArea.Catacombs,new Vector2(-8,-18));yield return Until(()=>s.CanSimulate);Check(g.cave.worms.Count>0,"independent catacomb player retains worm controllers");}
            else {g.amazons.EnterHouse(g.amazons.houses[1]);yield return Until(()=>s.CanSimulate);Check(s.Region=="house/1","house identity is independent from other interiors");}
            Mark("client"+number+"-independent.txt");yield return Until(()=>Stage=="return");Warp(WorldArea.Coast,g.ShipBase+Vector2.right*3);yield return Until(()=>Stage=="race");string id=File.ReadAllText(FilePath("race-item.txt"));
            if(number<=2)
            {
                var item=g.shipStorage.items.First(i=>i.id==id);s.inventory.Mutate(()=>{g.shipStorage.items.Remove(item);if(!g.survivor.inventory.Add(item))throw new Exception("No private inventory space");});yield return Until(()=>!s.inventory.Busy);int winner=g.survivor.inventory.items.Any(i=>i.id==id)?1:0;Mark("client"+number+"-race.txt",winner.ToString());
            }
            yield return Until(()=>Stage=="drop");if(number<=2 && g.survivor.inventory.items.Any(i=>i.id==id))
            {var item=g.survivor.inventory.items.First(i=>i.id==id);GroundLoot drop=null;s.inventory.Mutate(()=>{g.survivor.inventory.items.Remove(item);drop=g.ScatterLoot(item,g.body.position+Vector2.right*.5f,g.body.position);});yield return Until(()=>!s.inventory.Busy);Mark("drop-key.txt",drop.netIdV49);}
            yield return Until(()=>Stage=="pickup");if(number==3)
            {string key=File.ReadAllText(FilePath("drop-key.txt"));yield return Until(()=>g.loot.Any(l=>l.netIdV49==key && !l.collected));var drop=g.loot.First(l=>l.netIdV49==key);drop.flightAge=1;Check(g.CollectLoot(drop),"teammate collects a synchronized dropped item");yield return Until(()=>!s.inventory.Busy);Check(g.survivor.inventory.items.Any(i=>i.id==id),"item appears only in its recipient's private inventory");Mark("client3-pickup.txt");}
            yield return Until(()=>Stage=="redrop");if(number==3)
            {var item=g.survivor.inventory.items.First(i=>i.id==id);GroundLoot drop=null;s.inventory.Mutate(()=>{g.survivor.inventory.items.Remove(item);drop=g.ScatterLoot(item,g.body.position+Vector2.right*.5f,g.body.position);});yield return Until(()=>!s.inventory.Busy);Mark("second-drop-key.txt",drop.netIdV49);}
            yield return Until(()=>Stage=="repick");if(number==1)
            {string key=File.ReadAllText(FilePath("second-drop-key.txt"));yield return Until(()=>g.loot.Any(l=>l.netIdV49==key && !l.collected));var drop=g.loot.First(l=>l.netIdV49==key);drop.flightAge=1;g.CollectLoot(drop);yield return Until(()=>!s.inventory.Busy);Mark("client1-repick.txt",g.survivor.inventory.items.Any(i=>i.id==id)?"1":"0");}
            yield return Until(()=>Stage=="leave");if(number==1){s.Disconnect();yield break;}yield return Until(()=>Stage=="done");s.Disconnect();
        }
    }
}
