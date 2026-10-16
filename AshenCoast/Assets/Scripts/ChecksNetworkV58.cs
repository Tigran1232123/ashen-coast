using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed class ChecksNetworkV58:MonoBehaviour
    {
        public CoastGame game;string folder,role;int passed,failed;readonly List<string> results=new List<string>();
        void Check(bool condition,string label){results.Add((condition?"PASS ":"FAIL ")+label);Debug.Log(results.Last());if(condition)passed++;else failed++;}
        void Mark(string name)=>File.WriteAllText(Path.Combine(folder,name),"ready");
        bool Seen(string name)=>File.Exists(Path.Combine(folder,name));
        IEnumerator Until(Func<bool> condition,string label,float timeout=35){float end=Time.realtimeSinceStartup+timeout;while(!condition() && Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
        void Warp(Vector2 at){game.journey.SetArea(WorldArea.Coast,at);game.body.position=at;game.hero.transform.position=at;game.body.linearVelocity=Vector2.zero;}
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();folder=args[Array.IndexOf(args,"-v58-output")+1];role=args[Array.IndexOf(args,"-v58-role")+1];Directory.CreateDirectory(folder);
            game.snails.SetupOpen=false;game.paused=false;game.testMode=true;Time.timeScale=1;AudioListener.volume=0;Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=60;
            var work=role=="host"?Host():Guest();while(true){bool more;object current=null;try{more=work.MoveNext();if(more)current=work.Current;}catch(Exception e){Check(false,e.ToString());break;}if(!more)break;yield return current;}
            results.Add("RESULT "+passed+" passed; "+failed+" failed");File.WriteAllLines(Path.Combine(folder,role+"-results.txt"),results);game.coop.Disconnect(false);Application.Quit(failed==0?0:1);
        }
        IEnumerator Host()
        {
            var g=game;Check(g.coop.StartHost(27858,true),"host starts V58 session");Mark("host-ready");yield return Until(()=>g.coop.remote.Count==1,"guest joined");
            Check(g.shipStorage.columns*g.shipStorage.rows==160 && g.shipStorage.items.Any(i=>i.kind==ItemKind.Arrow && i.count==30),"shared enlarged hold and stack thirty");
            g.survivor.inventory.items.Clear();g.survivor.inventory.Add(new Item(ItemKind.Arrow){count=3});g.survivor.equipment[0]=new Item(ItemKind.Bow);g.combat.ResetForCheck();Check(g.combat.Begin(Vector2.right),"host begins bow draw with ammunition");
            yield return Until(()=>Seen("guest-bow"),"guest sees fully drawn bow pose");g.combat.CancelBowV58();
            g.meditation.Toggle();g.meditation.Tick(4,false);Check(g.Meditating && g.meditation.DaySpeed==1 && Time.timeScale==1,"host alone cannot accelerate time");Mark("host-sitting");
            yield return Until(()=>g.coop.remote.Values.Any(p=>p.State.meditating),"guest meditation arrives over TCP");g.meditation.Tick(4,false);Check(g.coop.AllMeditatingV58 && g.meditation.DaySpeed>4.9f,"both meditating enable acceleration");Mark("host-both");
            yield return Until(()=>Seen("guest-both"),"guest also enabled acceleration");g.meditation.Stop();Mark("host-awake");yield return Until(()=>Seen("guest-speed-one"),"guest stops acceleration when host stands");
            Warp(new Vector2(20,35));var station=g.buildings.CreateStation(ItemKind.Crossbow,new Vector2Int(20,35),false);
            for(int i=0;i<2;i++){var mob=g.woodlice[i];mob.renderer.transform.position=new Vector2(25+i*5,35.5f);mob.vitals.health=mob.vitals.maximumHealth=500;mob.vitals.curlRemaining=0;}
            g.coop.PublishWorldV49();Mark("crossbow-ready");yield return Until(()=>Seen("guest-fired"),"guest fires crossbow");
            yield return Until(()=>g.woodlice.Take(2).All(m=>m.vitals.health<500),"piercing damage reaches authority for both targets");
            File.WriteAllText(Path.Combine(folder,"damage-health.txt"),string.Join(",",g.woodlice.Take(2).Select(m=>m.vitals.health.ToString())));Check(g.woodlice.Take(2).All(m=>Mathf.Abs(m.vitals.health-400)<.01f),"each aligned enemy receives exactly 100 once");Mark("damage-checked");
            yield return Until(()=>Seen("guest-done"),"guest checks complete");
        }
        IEnumerator Guest()
        {
            var g=game;yield return Until(()=>Seen("host-ready"),"host ready");g.coop.Join("127.0.0.1",27858);yield return Until(()=>g.CoopOnline && g.coop.inventory.Ready,"V58 welcome and inventory synchronized");
            Check(WorldShape.Seed==74391 && RiverV58.Ready && Vector2.Distance(RiverV58.Junction,g.amazons.Centre+new Vector2(80,-65))<.001f,"guest adopts same river geometry from different seed");
            Check(g.shipStorage.columns*g.shipStorage.rows==160 && g.shipStorage.items.Any(i=>i.kind==ItemKind.Arrow && i.count==30),"shared hold and thirty arrows survive synchronization");
            yield return Until(()=>g.coop.remote.Values.Any(p=>p.State.bowDraw && p.State.bowChargeV58>.99f && p.Renderer.GetComponentsInChildren<SpriteRenderer>().Count(r=>r.name.StartsWith("Co-op bow arm") && r.enabled)==4),"bow charge and articulated arms synchronize");Mark("guest-bow");
            yield return Until(()=>Seen("host-sitting") && g.coop.remote.Values.Any(p=>p.State.meditating),"host meditation state synchronized");g.meditation.Tick(4,false);Check(Time.timeScale==1,"awake guest remains at normal speed");
            g.meditation.Toggle();yield return Until(()=>Seen("host-both"),"host confirms both meditate");g.meditation.Tick(4,false);Check(g.coop.AllMeditatingV58 && Time.timeScale>4.9f,"guest accelerates only with all players");Mark("guest-both");
            yield return Until(()=>Seen("host-awake") && g.coop.remote.Values.All(p=>!p.State.meditating),"host standing state arrives");g.meditation.Tick(.1f,false);Check(Time.timeScale==1,"host standing stops guest acceleration");g.meditation.Stop();Mark("guest-speed-one");
            yield return Until(()=>Seen("crossbow-ready") && g.buildings.stations.Any(s=>s.kind==ItemKind.Crossbow),"placed crossbow synchronized");
            var station=g.buildings.stations.First(s=>s.kind==ItemKind.Crossbow);Warp(station.Position+Vector2.down*1.4f);g.survivor.inventory.items.Clear();g.survivor.inventory.Add(new Item(ItemKind.Arrow){count=30});g.survivor.equipment[0]=null;
            yield return new WaitForSecondsRealtime(.15f);Check(g.MountV58(station),"guest mounts placed stationary weapon");Check(g.FireCrossbowV58(Vector2.right) && g.survivor.ArrowsV58==29,"guest fires using own arrow stack");
            for(int n=0;n<35;n++){g.amazons.CheckArrowsV58(.02f);yield return null;}Mark("guest-fired");yield return Until(()=>Seen("damage-checked"),"authority acknowledges damage");Check(g.coop.Online && !File.Exists(Application.dataPath+"/../Coop-Sync-Error.txt"),"no synchronization failure during new mechanics");g.DismountV58();Mark("guest-done");
        }
    }
    public sealed partial class AmazonSettlementV46 {public void CheckArrowsV58(float dt)=>TickArrows(dt);}
}
