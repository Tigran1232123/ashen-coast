using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug=UnityEngine.Debug;
namespace AshenCoast
{
    public sealed partial class PolishChecksV53:MonoBehaviour
    {
        public CoastGame game;int passed,failed;string folder;readonly List<string> lines=new List<string>();
        void Check(bool ok,string name){string s=(ok?"PASS ":"FAIL ")+name;lines.Add(s);Debug.Log(s);if(ok)passed++;else failed++;}
        object Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(o,args);
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-polish-output");folder=i>=0?args[i+1]:Path.Combine(Application.persistentDataPath,"PolishV53");Directory.CreateDirectory(folder);Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=60;AudioListener.volume=0;yield return null;
            game.snails.SetupOpen=false;game.paused=false;Time.timeScale=1;game.seconds=60;int roleAt=Array.IndexOf(args,"-polish-role");string role=roleAt>=0?args[roleAt+1]:"offline";var routine=args.Contains("-polish-arm-check")?ArmOnlyV53():args.Contains("-polish-proxy-check")?(role=="host"?ProxyHostV53():ProxyGuestV53()):args.Contains("-polish-final-check")?FinalV53():role=="host"?NetworkHostV53():role=="guest"?NetworkGuestV53():Run();while(true){bool move;object current=null;try{move=routine.MoveNext();if(move)current=routine.Current;}catch(Exception e){Check(false,e.ToString());break;}if(!move)break;yield return current;}
            lines.Add("RESULT "+passed+" passed; "+failed+" failed");File.WriteAllLines(Path.Combine(folder,role+"-results.txt"),lines);if(role=="offline")File.WriteAllLines(Path.Combine(folder,"results.txt"),lines);Application.Quit(failed==0?0:1);
        }
        IEnumerator Run()
        {
            var g=game;var sn=g.snails;
            Check(Application.version=="53.0" && new LanPacketV49().version==53 && new LanHelloV49().build==53,"local and LAN version V53");
            sn.SetupOpen=true;g.ChooseCharacterV51(CharacterV51.Frog);sn.SetupOpen=false;
            Check(Mathf.Abs(g.hero.sprite.bounds.size.y-1.068f)<.001f && g.arcane.frogs[0].renderer.sprite.bounds.size.y<1.05f,"frog player is 40 percent shorter, frog mage around half human height");
            Check(g.arcane.PlayerHand.y-g.hero.transform.position.y<.60f,"frog lamp and yo-yo hand follows smaller body");
            sn.SetupOpen=true;g.ChooseCharacterV51(CharacterV51.Human);sn.SetupOpen=false;
            g.corpses.RefreshBindingsV53();g.corpses.ScanV53();
            long allocated=GC.GetAllocatedBytesForCurrentThread();var timer=Stopwatch.StartNew();for(int j=0;j<100;j++)g.corpses.ScanV53();timer.Stop();long optimized=GC.GetAllocatedBytesForCurrentThread()-allocated;double optimizedMs=timer.Elapsed.TotalMilliseconds;
            allocated=GC.GetAllocatedBytesForCurrentThread();timer.Restart();for(int j=0;j<100;j++)foreach(var b in g.coop.world.BindingsV51().ToArray()){if(b.renders[0]){string region=g.coop.world.RegionOf(b.renders[0].transform);if(b.health()>0){var old=g.corpses.bodies.Values.Where(body=>body.data.actor==b.key).ToArray();}}}timer.Stop();long oldBytes=GC.GetAllocatedBytesForCurrentThread()-allocated;
            lines.Add("MEASURE corpse scans x100: before "+timer.Elapsed.TotalMilliseconds.ToString("0.00")+"ms; after "+optimizedMs.ToString("0.00")+"ms. Mono allocation counter "+oldBytes+"/"+optimized+" (zero counters unavailable)");Check(optimizedMs<timer.Elapsed.TotalMilliseconds*.5,"corpse scanning takes less than half the previous CPU time");
            Vector2 destination=SnailHomesV50.FindLand(sn.houses[0].position+new Vector2(42,46));g.map.regions[0].Reveal(destination,12);g.map.OpenSnailCommand(sn.houses[0].position);yield return null;
            if(g.map.Plot.width<=0)typeof(ExplorationMap).GetProperty("Plot").SetValue(g.map,new Rect(100,160,1100,650));var pan=g.map.Centre-destination;g.map.Pan(new Vector2(pan.x,-pan.y)*g.map.Zoom);
            lines.Add("MAP before input centre="+g.map.Centre+" known="+g.map.IsVisited(MapRegion.Coast,g.map.Centre)+" land="+SnailHomesV50.Land(g.map.Centre)+" mode="+g.map.SnailCommandV50+" plot="+g.map.Plot);
            var click=new Event{type=EventType.MouseDown,button=1,mousePosition=g.map.Plot.center};Call(g.map,"ProcessMapInputV41",click);lines.Add("MAP event after="+click.type+" region="+g.map.Region);if(!sn.ordered)lines.Add("MAP direct selection="+Call(g.map,"SelectSnailV50",g.map.Plot.center));yield return null;
            lines.Add("MAP ordered="+sn.ordered+" phase="+sn.campPhaseV51+" open="+g.map.Open+" paused="+g.paused+" destination="+sn.destination+" expected="+destination);
            Check(sn.ordered && sn.campPhaseV51==1 && !g.map.Open && !g.paused,"RMB destination handler starts gathering and closes map");
            g.paused=true;Call(sn,"CampTickV51",60f);for(int j=0;j<1800 && sn.campPhaseV51!=4;j++){bool busy=(bool)Call(sn,"CampTickV51",.05f);if(!busy)Call(sn,"Move",.05f);}
            Check(sn.campPhaseV51==4 && Vector2.Distance(sn.destination,destination)<.1f && sn.houses.All(h=>Vector2.Distance(h.position,sn.CampSlotV51(h.index))<.3f),"all four snails complete map route and form arrival circle");
            sn.Enter(1);var indoor=sn.houses[0].position+Vector2.up*26;Check(sn.Order(indoor),"outdoor path can be ordered while host is inside a snail");Call(sn,"CampTickV51",60f);var before=sn.houses[0].position;Call(sn,"CampTickV51",.05f);Call(sn,"Move",.5f);Check(Vector2.Distance(before,sn.houses[0].position)>.1f,"caravan moves independently of host interior");sn.ordered=false;sn.campPhaseV51=4;sn.Leave();
            for(int home=0;home<4;home++){typeof(SnailHomesV50).GetField("doorWait",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(sn,0f);sn.Enter(home);g.body.position=new Vector2(.4f,-1.85f);g.hero.transform.position=g.body.position;g.gait.Pose(0,0,false,Vector2.down);Capture("room-"+home+".png",new Vector2(.3f,-.15f),4.8f);Check(sn.Inside==home && sn.houses[home].room.activeInHierarchy && sn.houses[home].room.GetComponentsInChildren<BoxCollider2D>().Length>=4,"solid furniture and clear door, room "+home);sn.Leave();yield return null;}
            g.paused=true;g.body.position=g.amazons.Centre;g.hero.transform.position=g.body.position;
            var woman=g.amazons.people.First(p=>!p.IsSlave && !p.Hunter && !p.Slaver);woman.Warp(g.body.position+Vector2.right*3);var frames=new HashSet<Sprite>();var view=new GameObject("NPC walking sequence");
            for(int f=0;f<8;f++){woman.WalkHomeV50(woman.Position+Vector2.right*.175f);frames.Add(woman.gaitV53.Lower.sprite);var clone=Instantiate(woman.Renderer.gameObject);clone.transform.SetParent(view.transform,true);clone.transform.position=new Vector2(1000+f*1.4f,1000);foreach(var t in clone.GetComponentsInChildren<Transform>())t.gameObject.layer=31;}
            Check(frames.Count>=7 && woman.gaitV53.Walking,"Amazon steps through distinct paired-leg contact and passing poses");Capture("amazon-walk.png",new Vector2(1004.9f,1000.8f),2.5f,1<<31);Destroy(view);
            var slave=g.amazons.people.First(p=>p.IsSlave);slave.WalkHomeV50(slave.Position+Vector2.right*.2f);Check(slave.gaitV53.Walking,"captive has actual leg gait");
            var bandit=g.amazons.plains.Spawn(g.body.position+Vector2.right*10,false);Call(bandit,"Pose",true);Check(bandit.gaitV53.Lower.enabled && bandit.gaitV53.Walking,"plains bandit has leg gait");
            var fort=g.fort.bandits[0];Call(fort,"Pose",true);Check(fort.gaitV53.Lower.enabled && fort.gaitV53.Walking,"fort bandit has leg gait");
            g.WorldSwishV53(woman.Renderer);for(int n=0;n<100;n++)g.WorldSwishV53(woman.Renderer);Check(g.worldAudioV53.ActiveVoices<=3,"training attack audio uses at most three voices");Check(!g.worldAudioV53.Audible(fort.Renderer),"other location cannot emit training audio");woman.Warp(g.body.position+Vector2.right*30);Check(!g.worldAudioV53.Audible(woman.Renderer),"distant combat is inaudible");g.paused=false;yield return null;Check(g.worldAudioV53.ActiveVoices==0,"voices stop after source leaves audible range");g.paused=true;
            g.journey.SetArea(WorldArea.Coast,new Vector2(2,2));var small=g.woodlice.First(w=>w.Small);var large=g.woodlice.First(w=>!w.Small);g.corpses.RefreshBindingsV53();var sb=g.coop.world.BindingsV51().First(b=>b.driver==small);var lb=g.coop.world.BindingsV51().First(b=>b.driver==large);
            Check(Mathf.Abs(g.corpses.WidthV53(sb,3)/g.corpses.WidthV53(lb,3)-.48f)<.001f,"woodlouse corpse dimensions preserve small to large ratio");
            int loot=g.loot.Count;while(small.vitals.Alive)small.Hurt(10000);g.corpses.ScanV53();var body=g.corpses.bodies.Values.First(b=>b.data.actor==sb.key);Check(body.fall && body.sprite.color.a<.1f,"creature begins falling animation before corpse");Check(g.loot.Count>loot && g.loot.Last().InFlight && Vector2.Distance(g.loot.Last().position,g.loot.Last().thrownFrom)>.5f,"creature loot flies outward with height");
            var frog=g.arcane.frogs[0];frog.HurtFrom(10000,g.hero);g.corpses.ScanV53();var fb=g.corpses.bodies.Values.First(b=>b.data.actor=="frog/0").data;g.corpses.RaiseActor(fb);Check(frog.Alive && !frog.Hostile,"resurrected neutral frog forgets aggression");
            woman.hostile=true;woman.health=0;g.corpses.ScanV53();var wb=g.corpses.bodies.Values.First(b=>b.data.actor==g.coop.world.Key(woman.Renderer)).data;g.corpses.RaiseActor(wb);Check(woman.Alive && !woman.Hostile && !woman.Hired,"resurrected neutral Amazon is peaceful");
            slave.Hired=true;slave.health=0;g.corpses.ScanV53();var ab=g.corpses.bodies.Values.First(b=>b.data.actor==g.coop.world.Key(slave.Renderer)).data;g.corpses.RaiseActor(ab);Check(slave.Alive && slave.Hired,"resurrected hired captive remains allied");
            fort.health=0;g.corpses.ScanV53();var eb=g.corpses.bodies.Values.First(b=>b.data.actor==g.coop.world.Key(fort.Renderer)).data;g.corpses.RaiseActor(eb);Check(fort.Alive && !((ICombatTarget)fort is VillageActorV46),"natural hostile bandit retains enemy type");
            body.data.age=.8f;Call(g.corpses,"PoseBodyV53",body);Check(!body.fall && body.sprite.color.a>.99f,"death transitions into a full corpse");Capture("woodlouse-corpse.png",small.Position,2.5f);
        }
        void Capture(string file,Vector2 centre,float size,int layers=-1)
        {
            var camera=new GameObject("V53 visual verification").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=size;camera.transform.position=new Vector3(centre.x,centre.y,-20);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.03f,.025f);camera.nearClipPlane=.1f;camera.farClipPlane=100;camera.cullingMask=layers;Shader.SetGlobalColor("_CoastAmbient",new Color(.95f,.92f,.86f));var target=new RenderTexture(1440,1000,24);camera.targetTexture=target;camera.Render();var old=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(1440,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1440,1000),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,file),image.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;target.Release();Destroy(target);Destroy(image);Destroy(camera.gameObject);
        }
    }
}
