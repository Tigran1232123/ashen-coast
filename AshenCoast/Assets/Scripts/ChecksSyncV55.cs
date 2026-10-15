using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed class ChecksSyncV55:MonoBehaviour
    {
        public CoastGame game;string folder,role;int passed,failed;
        readonly List<string> report=new List<string>();
        void Check(bool ok,string label){report.Add((ok?"PASS ":"FAIL ")+label);Debug.Log(report.Last());if(ok)passed++;else failed++;}
        string PathFor(string name)=>Path.Combine(folder,name);
        void Mark(string name)=>File.WriteAllText(PathFor(name),"ready");
        IEnumerator Until(Func<bool> predicate,string label,float seconds=30)
        {float until=Time.realtimeSinceStartup+seconds;while(!predicate() && Time.realtimeSinceStartup<until)yield return null;Check(predicate(),label);}
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-sync-output");folder=args[at+1];at=Array.IndexOf(args,"-sync-role");role=args[at+1];Directory.CreateDirectory(folder);
            game.snails.SetupOpen=false;game.paused=false;game.SetAutoPickup(false);Time.timeScale=1;Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=30;AudioListener.volume=0;yield return null;
            var work=role=="host"?Host():Guest();
            while(true){bool more;object current=null;try{more=work.MoveNext();if(more)current=work.Current;}catch(Exception e){Check(false,e.ToString());break;}if(!more)break;yield return current;}
            report.Add("RESULT "+passed+" passed; "+failed+" failed");File.WriteAllLines(PathFor(role+"-results.txt"),report);Application.Quit(failed==0?0:1);
        }
        LanLootV49 Tombstone(string key)=>new LanLootV49{key=key,region="area/0",collected=true,item=LanItemV49.Capture(new Item(ItemKind.Coin){count=0})};
        IEnumerator Host()
        {
            var g=game;var s=g.coop;
            Check(Application.version==LanProtocolV49.Build.ToString()+".0" && new LanHelloV49().build==LanProtocolV49.Build && new LanPacketV49().version==LanProtocolV49.Build,"current identifiers agree");
            var coins=new GridInventory(2,1,"stack");coins.Add(new Item(ItemKind.Coin){count=9});var last=new Item(ItemKind.Coin);coins.Add(last);
            Check(last.count==0 && coins.items.Single().count==10,"real stack merge consumes the source coin item");
            bool rejected=false;try{LanItemV49.Capture(last).Restore();}catch(InvalidOperationException){rejected=true;}
            Check(rejected,"V54 tombstone item fails strict live-item validation, reproducing the old failure path");
            var hidden=g.CreateLooseLootV46(last,g.body.position+Vector2.right*.5f);hidden.collected=true;hidden.netIdV49="sync-v55-old-coin";hidden.renderer.enabled=hidden.shadow.enabled=false;
            var live=g.CreateLooseLootV46(new Item(ItemKind.Clay),g.body.position+Vector2.right);live.netIdV49="sync-v55-live";
            var captured=s.world.Capture("area/0");Check(!captured.loot.Any(l=>l.key==hidden.netIdV49) && captured.loot.Any(l=>l.key==live.netIdV49),"snapshot excludes consumed drops and retains live loot");
            s.world.ApplyLoot(Tombstone("sync-v55-never-spawned"));Check(!g.loot.Any(l=>l.netIdV49=="sync-v55-never-spawned"),"collected tombstone never restores or spawns an obsolete item");
            s.world.ApplyLoot(Tombstone(live.netIdV49));Check(live.collected && !live.renderer.enabled && !live.shadow.enabled,"tombstone hides existing loot without restoring its item");
            live.collected=false;live.renderer.enabled=live.shadow.enabled=true;
            Check(s.StartHost(27955,true),"host starts after the consumed coin already exists");Mark("host-ready");
            yield return Until(()=>s.players.Count==2,"late guest joins an existing world");yield return Until(()=>File.Exists(PathFor("guest-ready")),"guest is ready after initial snapshots");
            var snapshot=s.world.Capture("area/0");snapshot.loot=snapshot.loot.Concat(new[]{Tombstone("sync-v55-legacy")}).ToArray();
            s.SendToV49(1,new LanPacketV49{kind="world",peer=0,region="area/0",body=JsonUtility.ToJson(snapshot)});Mark("sent-tombstone");
            yield return Until(()=>File.Exists(PathFor("guest-stable")),"guest remains synchronized through subsequent live world updates",40);
            Check(s.Online && s.players.Count==2,"host retains its connected guest");
            yield return Until(()=>File.Exists(PathFor("guest-rejoined")),"guest can reconnect to the same host after disconnecting");
            Check(s.Online && s.players.Count==2,"reconnected guest occupies a new peer seat");Mark("done");yield return new WaitForSeconds(.8f);s.Disconnect(false);
        }
        IEnumerator Guest()
        {
            var g=game;var s=g.coop;yield return Until(()=>File.Exists(PathFor("host-ready")),"host ready");s.Join("127.0.0.1",27955);
            yield return Until(()=>s.Online && s.inventory.Ready,"guest receives a complete initial world and inventory");
            Check(g.loot.Any(l=>l.netIdV49=="sync-v55-live" && !l.collected),"live loot appears for the late guest");Check(!g.loot.Any(l=>l.netIdV49=="sync-v55-old-coin"),"consumed zero-count coin is absent for the late guest");Mark("guest-ready");
            yield return Until(()=>File.Exists(PathFor("sent-tombstone")),"legacy tombstone sent");
            float until=Time.realtimeSinceStartup+12;while(s.Online && Time.realtimeSinceStartup<until)yield return null;
            Check(s.Online && s.inventory.Ready,"guest stays online for twelve seconds after a tombstone and repeated world snapshots");Check(!g.loot.Any(l=>l.netIdV49=="sync-v55-legacy"),"legacy tombstone creates no phantom drop");Check(string.IsNullOrEmpty(s.SyncErrorReportV55),"no synchronization exception during connection and steady-state updates");Mark("guest-stable");
            s.Disconnect(false);yield return new WaitForSeconds(1);s.Join("127.0.0.1",27955);yield return Until(()=>s.Online && s.inventory.Ready,"guest reconnects with fresh transport and inventory");Mark("guest-rejoined");yield return Until(()=>File.Exists(PathFor("done")),"host confirms reconnection");s.Disconnect(false);
            s.RecordSyncErrorV55(new LanPacketV49{kind="world",region="area/0"},new InvalidOperationException("V55 diagnostic fixture"));
            Check(File.Exists(s.SyncErrorReportV55) && File.ReadAllText(s.SyncErrorReportV55).Contains("Packet: world") && File.ReadAllText(s.SyncErrorReportV55).Contains("V55 diagnostic fixture"),"standalone sync report identifies the failing stage and exception");
        }
    }
}
