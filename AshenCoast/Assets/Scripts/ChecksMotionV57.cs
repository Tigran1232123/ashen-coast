using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using Debug=UnityEngine.Debug;

namespace AshenCoast
{
    public sealed class ChecksMotionV57:MonoBehaviour
    {
        public CoastGame game;string folder,role;int passed,failed;
        readonly List<string> report=new List<string>();readonly Dictionary<string,double> metrics=new Dictionary<string,double>();
        readonly HashSet<int> npcCaptured=new HashSet<int>();readonly HashSet<string> captured=new HashSet<string>();readonly List<GameObject> clones=new List<GameObject>();
        readonly List<float> frameTimes=new List<float>();
        TavernResident walker;bool walking,collecting;float started;double maxLegError;int samples;
        string FileFor(string name)=>Path.Combine(folder,name);
        void Mark(string name)=>File.WriteAllText(FileFor(name),"ready");
        void Check(bool ok,string label){var line=(ok?"PASS ":"FAIL ")+label;report.Add(line);Debug.Log(line);if(ok)passed++;else failed++;}
        IEnumerator Until(Func<bool> predicate,string label,float seconds=45)
        {float until=Time.realtimeSinceStartup+seconds;while(!predicate() && Time.realtimeSinceStartup<until)yield return null;Check(predicate(),label);}
        void Warp(WorldArea area,Vector2 at){game.journey.SetArea(area,at);game.body.position=at;game.hero.transform.position=at;game.body.linearVelocity=Vector2.zero;}
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();folder=args[Array.IndexOf(args,"-motion-output")+1];role=args[Array.IndexOf(args,"-motion-role")+1];Directory.CreateDirectory(folder);
            game.snails.SetupOpen=false;game.paused=false;game.SetAutoPickup(false);Time.timeScale=1;
            Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=60;AudioListener.volume=0;
            game.survivor.Bag.Add(new Item(ItemKind.Clay));game.survivor.Bag.Add(new Item(ItemKind.Coin){count=2});
            Check(Application.version=="57.0" && LanProtocolV49.Build==57 && new LanPacketV49().version==57 && new LanHelloV49().build==57,"V57 application and handshake agree");
            yield return null;
            var run=role=="host"?Host():Guest();
            while(true){bool more;object current=null;try{more=run.MoveNext();if(more)current=run.Current;}catch(Exception e){Check(false,e.ToString());break;}if(!more)break;yield return current;}
            report.Add("RESULT "+passed+" passed; "+failed+" failed");File.WriteAllLines(FileFor(role+"-results.txt"),report);
            File.WriteAllText(FileFor(role+"-metrics.json"),"{"+string.Join(",",metrics.Select(p=>"\""+p.Key+"\":"+p.Value.ToString("R",System.Globalization.CultureInfo.InvariantCulture)))+"}");
            game.coop.Disconnect(false);Application.Quit(failed==0?0:1);
        }
        void QueueCheck()
        {
            using(var queue=new LanSendQueueV57())
            {
                queue.Enqueue("before",null);
                for(int i=0;i<1000;i++){if(i==500)queue.Enqueue("damage-command",null);CheckEnqueue(queue.Enqueue("state-"+i,"player/1"));}
                var received=new List<string>();while(queue.TryTake(out var json))received.Add(json);
                Check(received.SequenceEqual(new[]{"before","damage-command","state-999"}),"slow-link queue retains commands in order and replaces obsolete movement");
                Check(queue.MaxObserved<=3 && queue.Superseded==999,"one thousand movement samples occupy only one unsent state slot");
            }
        }
        void CheckEnqueue(bool ok){if(!ok)throw new InvalidOperationException("Latest-state queue rejected a replaceable frame");}
        void Benchmark()
        {
            var world=game.coop.world;
            world.Capture("area/0");world.CaptureMotionV57("area/0");
            var timer=Stopwatch.StartNew();long allocated=GC.GetAllocatedBytesForCurrentThread();string full=null;
            for(int i=0;i<3;i++)full=JsonUtility.ToJson(world.Capture("area/0"));
            timer.Stop();metrics["full_capture_ms"]=timer.Elapsed.TotalMilliseconds/3;metrics["full_capture_allocated_bytes"]=(GC.GetAllocatedBytesForCurrentThread()-allocated)/3;
            allocated=GC.GetAllocatedBytesForCurrentThread();timer.Restart();string motion=null;
            for(int i=0;i<8;i++)motion=JsonUtility.ToJson(world.CaptureMotionV57("area/0"));
            timer.Stop();metrics["motion_capture_ms"]=timer.Elapsed.TotalMilliseconds/8;metrics["motion_capture_allocated_bytes"]=(GC.GetAllocatedBytesForCurrentThread()-allocated)/8;
            var fullBytes=System.Text.Encoding.UTF8.GetBytes(full);var motionBytes=System.Text.Encoding.UTF8.GetBytes(motion);
            metrics["full_json_bytes"]=fullBytes.Length;metrics["nearby_motion_json_bytes"]=motionBytes.Length;
            metrics["former_full_snapshot_bytes_per_second"]=fullBytes.Length/.12;
            metrics["new_full_and_motion_bytes_per_second"]=fullBytes.Length*2+motionBytes.Length*10;
            metrics["new_to_former_payload_ratio"]=metrics["new_full_and_motion_bytes_per_second"]/metrics["former_full_snapshot_bytes_per_second"];
            Check(metrics["new_to_former_payload_ratio"]<.75,"coast full-plus-nearby stream is at least 25 percent smaller than former full-snapshot cadence");
            var player=game.coop.CapturePlayer();
            Check(player.equipment.All(item=>item.contents==null),"20-Hz player poses do not contain equipped bag contents");
            Check(game.survivor.equipment.Any(i=>i?.contents!=null && i.contents.items.Count>0),"private equipped bag contents remain intact locally");
            var binding=game.coop.world.Find("resident/0");var lower=binding.renders[0].GetComponentsInChildren<SpriteRenderer>(true).First(r=>r.name=="Resident walking legs");
            var root=binding.renders[0].transform;var clone=Instantiate(root.gameObject);var child=clone.GetComponentsInChildren<SpriteRenderer>(true).First(r=>r.name==lower.name).transform;
            var offset=child.position-clone.transform.position;var destination=clone.transform.position+Vector3.right;
            clone.transform.position=Vector3.Lerp(clone.transform.position,destination,.25f);
            child.position=Vector3.Lerp(child.position,destination+offset,.25f);
            Check((child.position-clone.transform.position-offset).magnitude>.15f,"old independent world-position interpolation reproduces detached legs");Destroy(clone);
        }
        IEnumerator Host()
        {
            QueueCheck();Benchmark();
            Warp(WorldArea.Tavern,new Vector2(-2,-1));game.journey.AlarmTavern();
            walker=game.journey.residents.First(r=>r.actor.row==3);walker.health=95;
            game.journey.companion.target=game.hero;game.amazons.people[0].Aggressor=game.hero;
            Check(game.coop.StartHost(27958,true),"host starts after tavern combat already created weapons and walking legs");game.paused=true;
            Check(!game.coop.world.Find("mage").renders.Contains(game.hero) && !game.coop.world.Find("amazon/0").renders.Contains(game.hero),"combat target and aggressor references never become replicated body parts");
            Mark("host-ready");yield return Until(()=>File.Exists(FileFor("guest-ready")),"guest is ready in the tavern");
            walking=true;started=Time.unscaledTime;Mark("walk-start");
            yield return new WaitForSecondsRealtime(16.5f);walking=false;Mark("walk-done");
            yield return Until(()=>File.Exists(FileFor("guest-walk-done")),"guest captured the full network walking cycle");
            game.coop.transport.ArtificialSendDelayV57=70;walking=true;started=Time.unscaledTime;Mark("slow-start");
            yield return new WaitForSecondsRealtime(7);walking=false;game.coop.transport.ArtificialSendDelayV57=0;Mark("slow-done");
            Check(game.coop.Online && game.coop.players.Count==2,"slow writer does not disconnect the session");
            Check(game.coop.transport.PendingFramesV57<40,"slow writer does not accumulate hundreds of stale snapshots");
            Check(game.coop.transport.CompressedFramesV57>0 && game.coop.transport.CompressionThreadV57!=Thread.CurrentThread.ManagedThreadId,"packet compression runs on a socket worker rather than the Unity thread");
            metrics["sent_bytes"]=game.coop.transport.SentBytesV57;metrics["compressed_frames"]=game.coop.transport.CompressedFramesV57;
            metrics["full_frames"]=game.coop.FullFramesV57;metrics["motion_frames"]=game.coop.MotionFramesV57;
            walker.health=73;game.coop.Send(new LanPacketV49{kind="motion",region=game.coop.Region,body=JsonUtility.ToJson(game.coop.world.CaptureMotionV57(game.coop.Region))});
            Warp(WorldArea.Coast,new Vector2(-2,-1));game.paused=true;Mark("host-left-tavern");
            yield return Until(()=>File.Exists(FileFor("guest-authority")),"guest receives current actor state when taking tavern authority");
            Warp(WorldArea.Tavern,new Vector2(-2,-1));game.paused=true;
            yield return Until(()=>File.Exists(FileFor("guest-owner-returned")),"guest completes motion and authority checks");
            Check(game.journey.TavernAlarm && walker.feet && walker.blade,"tavern combat controllers survived the return to host");
            game.journey.ApplyAlarmV57(false);game.coop.PublishWorldV49();Mark("calm-world");
            yield return Until(()=>File.Exists(FileFor("guest-done")),"guest also receives a fresh peaceful tavern state");
            Check(!game.journey.TavernAlarm,"peaceful tavern state can replace a previously alarmed world");
        }
        IEnumerator Guest()
        {
            yield return Until(()=>File.Exists(FileFor("host-ready")),"host ready");game.coop.Join("127.0.0.1",27958);
            yield return Until(()=>game.coop.Online && game.coop.inventory.Ready,"guest receives a complete late-join snapshot");
            Warp(WorldArea.Tavern,new Vector2(1,-2));game.paused=true;
            yield return Until(()=>game.coop.remote.Count>0 && !game.coop.CanSimulate,"host and guest share the tavern with a single authority");
            Check(game.journey.TavernAlarm && game.journey.residents.All(r=>r.feet || !r.Alive),"late join reconstructs tavern combat before visual parts are matched");
            var inventory=game.survivor.equipment.Where(i=>i?.contents!=null).SelectMany(i=>i.contents.items).Select(i=>i.id).OrderBy(i=>i).ToArray();
            collecting=true;Mark("guest-ready");yield return Until(()=>File.Exists(FileFor("walk-done")),"network walking sequence finished",35);collecting=false;
            Check(captured.Count==64,"all eight directions contain all eight received paired-leg phases");
            Check(npcCaptured.Count==8,"late-joined tavern resident has a complete received walking cycle");
            SaveContactSheet();
            Check(samples>100 && maxLegError<.01,"body and every NPC leg/weapon part stay on one interpolated anchor");
            var stale=game.art.Renderer("Client-only obsolete visual",game.art.hero[0,0],game.body.position,0);stale.transform.SetParent(game.journey.residents[0].Renderer.transform,true);
            yield return Until(()=>!stale.enabled,"parts absent from the authority frame are hidden rather than left behind");Destroy(stale.gameObject);
            Mark("guest-walk-done");yield return Until(()=>File.Exists(FileFor("slow-start")),"slow-link stage starts");
            maxLegError=0;samples=0;frameTimes.Clear();yield return Until(()=>File.Exists(FileFor("slow-done")),"slow-link stage finishes",20);
            Check(game.coop.Online && samples>100 && maxLegError<.01,"legs remain attached while the sender is deliberately delayed");
            Check(string.IsNullOrEmpty(game.coop.SyncErrorReportV55),"no synchronization exception during normal or slow-link movement");
            metrics["max_leg_anchor_error"]=maxLegError;metrics["anchor_samples"]=samples;
            if(frameTimes.Count>0){frameTimes.Sort();metrics["frame_time_p95_ms"]=frameTimes[Mathf.Min(frameTimes.Count-1,(int)(frameTimes.Count*.95))]*1000;}
            var current=game.survivor.equipment.Where(i=>i?.contents!=null).SelectMany(i=>i.contents.items).Select(i=>i.id).OrderBy(i=>i).ToArray();
            Check(inventory.SequenceEqual(current),"private bag contents survive repeated small player and motion packets");
            yield return Until(()=>File.Exists(FileFor("host-left-tavern")) && game.coop.CanSimulate,"guest becomes tavern authority");
            Check(Mathf.Abs(game.journey.residents.First(r=>r.actor.row==3).health-73)<.001f,"latest motion health survives authority transfer between complete snapshots");
            Check(game.journey.TavernAlarm && game.journey.residents.Where(r=>!r.Flee).All(r=>r.blade),"authority takeover retains tavern alarm and usable weapons");Mark("guest-authority");
            yield return Until(()=>!game.coop.CanSimulate,"host takes tavern authority back");
            Mark("guest-owner-returned");yield return Until(()=>File.Exists(FileFor("calm-world")) && !game.journey.TavernAlarm,"guest replaces old combat mode with a peaceful tavern");
            Check(game.journey.residents.All(r=>(!r.feet || !r.feet.enabled) && (!r.blade || !r.blade.enabled)),"old combat weapons and collision feet stay disabled after the reset");
            Mark("guest-done");
        }
        void LateUpdate()
        {
            if(game==null || string.IsNullOrEmpty(role) || !game.coop.Online)return;
            if(role=="host" && walking)
            {
                float phase=Time.unscaledTime-started;int step=Mathf.Min(63,(int)(phase/.25f)),direction=step/8,frame=step%8;
                var point=new Vector2(-2+Mathf.Sin(phase*.8f)*.65f,-1+Mathf.Cos(phase*.8f)*.4f);
                game.body.position=point;game.hero.transform.position=point;game.facing=direction;game.walkFrame=frame;game.walkTime=frame/8f;game.walking=true;game.movement=Vector2.right;
                game.hero.sprite=game.HeroFramesV51(CharacterV51.Human)[direction,frame];game.hero.flipX=direction>=5;game.art.PixelStyle(game.hero);game.gait.Pose(direction,game.walkTime,true,Vector2.right);
                walker.Renderer.transform.position=new Vector2(-.6f+Mathf.Sin(phase)*1.1f,1.2f);
                walker.Renderer.sprite=game.newArt.motion[walker.actor.row,frame];walker.Renderer.sortingOrder=CoastGame.Depth(walker.Position.y);game.art.PixelStyle(walker.Renderer);
                walker.walkingGaitV53.PoseSideV53(game.walkTime,true,Vector2.right);
                if(walker.blade){walker.blade.transform.position=walker.Position+Vector2.up*.8f+Vector2.right*.2f;walker.blade.sortingOrder=walker.Renderer.sortingOrder+2;}
            }
            if(role!="guest")return;
            frameTimes.Add(Time.unscaledDeltaTime);
            foreach(var binding in game.coop.world.BindingsV51().Where(b=>b.region==game.coop.Region && b.received!=null))
            {
                var body=binding.received.renders.FirstOrDefault(r=>r.part=="body");if(body==null)continue;
                foreach(var pose in binding.received.renders.Where(r=>r.part!="body"))
                    if(binding.partsV57.TryGetValue(pose.part,out var renderer) && renderer)
                    {var expected=new Vector3(pose.x-body.x,pose.y-body.y,pose.z-body.z);maxLegError=Math.Max(maxLegError,((renderer.transform.position-binding.renders[0].transform.position)-expected).magnitude);samples++;}
            }
            var proxy=game.coop.remote.Values.FirstOrDefault();if(!collecting || proxy?.State==null || !proxy.State.moving || !proxy.LowerV57.enabled)return;
            int d=proxy.State.facing,f=Mathf.FloorToInt(Mathf.Repeat(proxy.State.walk,1)*8);string key=d+"/"+f;
            var resident=game.journey.residents.First(r=>r.actor.row==3);int npcFrame=-1;
            for(int n=0;n<8;n++)if(resident.Renderer.sprite.texture.name==game.newArt.motion[resident.actor.row,n].texture.name && resident.Renderer.sprite.rect==game.newArt.motion[resident.actor.row,n].rect){npcFrame=n;break;}
            if(npcFrame>=0 && npcCaptured.Add(npcFrame))
            {
                var npc=Instantiate(resident.Renderer.gameObject);npc.name="Network tavern walking "+npcFrame;npc.transform.position=new Vector3(1000+npcFrame*1.25f,982,0);
                foreach(var child in npc.GetComponentsInChildren<Transform>(true))child.gameObject.layer=31;clones.Add(npc);
            }
            if(!captured.Add(key))return;
            var clone=Instantiate(proxy.Renderer.gameObject);clone.name="Network walking "+key;clone.transform.position=new Vector3(1000+f*1.25f,1000+(7-d)*2.2f,0);
            foreach(var child in clone.GetComponentsInChildren<Transform>(true))child.gameObject.layer=31;clones.Add(clone);
        }
        void SaveContactSheet()
        {
            var obj=new GameObject("V57 rendered network walk contact sheet");var camera=obj.AddComponent<Camera>();
            camera.orthographic=true;camera.orthographicSize=9;camera.aspect=1;camera.cullingMask=1<<31;camera.backgroundColor=new Color(.16f,.19f,.20f);camera.clearFlags=CameraClearFlags.SolidColor;
            camera.transform.position=new Vector3(1004.4f,1008.5f,-20);
            var target=new RenderTexture(1536,1536,24);camera.targetTexture=target;camera.Render();
            var previous=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(1536,1536,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1536,1536),0,0);image.Apply();
            File.WriteAllBytes(FileFor("network-walk-8-directions.png"),image.EncodeToPNG());RenderTexture.active=previous;camera.targetTexture=null;target.Release();Destroy(target);Destroy(image);
            var npcTarget=new RenderTexture(1536,384,24);camera.targetTexture=npcTarget;camera.orthographicSize=1.8f;camera.aspect=4;camera.transform.position=new Vector3(1004.4f,982.9f,-20);camera.Render();
            previous=RenderTexture.active;RenderTexture.active=npcTarget;var npcImage=new Texture2D(1536,384,TextureFormat.RGB24,false);npcImage.ReadPixels(new Rect(0,0,1536,384),0,0);npcImage.Apply();
            File.WriteAllBytes(FileFor("network-tavern-walk.png"),npcImage.EncodeToPNG());RenderTexture.active=previous;camera.targetTexture=null;npcTarget.Release();Destroy(npcTarget);Destroy(npcImage);Destroy(obj);
            foreach(var clone in clones)Destroy(clone);
        }
    }
}
