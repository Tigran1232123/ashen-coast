using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class ChecksV54:MonoBehaviour
    {
        public CoastGame game;string folder,role;int passed,failed;readonly List<string> report=new List<string>();
        void Check(bool ok,string label){report.Add((ok?"PASS ":"FAIL ")+label);Debug.Log(report.Last());if(ok)passed++;else failed++;}
        object Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(o,args);
        void Set(object o,string field,object value)=>o.GetType().GetField(field,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(o,value);
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-v54-output");folder=at>=0?args[at+1]:Path.Combine(Application.persistentDataPath,"ChecksV54");Directory.CreateDirectory(folder);at=Array.IndexOf(args,"-v54-role");role=at>=0?args[at+1]:"offline";
            Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=60;AudioListener.volume=0;yield return null;game.snails.SetupOpen=false;game.paused=false;Time.timeScale=1;game.seconds=60;
            var work=args.Contains("-v54-poses")?PosesV54():args.Contains("-v54-storage")?StorageV54():args.Contains("-v54-refine")?RefineV54():args.Contains("-v54-final")?FinalV54():role=="host"?HostV54():role=="guest"?GuestV54():OfflineV54();
            while(true){bool next=false;object result=null;try{next=work.MoveNext();if(next)result=work.Current;}catch(Exception e){Check(false,e.ToString());break;}if(!next)break;yield return result;}
            report.Add("RESULT "+passed+" passed; "+failed+" failed");File.WriteAllLines(Path.Combine(folder,role+"-results.txt"),report);Application.Quit(failed==0?0:1);
        }
        IEnumerator OfflineV54()
        {
            var g=game;var v=g.amazons;g.paused=true;
            Check(Application.version=="54.0" && new LanHelloV49().build==54 && new LanPacketV49().version==54,"V54 local and LAN build identifiers agree");
            Check(g.survivor.WearingBootsV54,"boots equipped from beginning");
            var s=new Survivor();s.equipment[(int)GearSlot.Feet]=null;var inventory=new GridInventory(4,4,"check");var skates=new Item(ItemKind.RollerSkates);inventory.Add(skates);
            Check(!s.EquipSkates(skates,inventory,false,out _),"rollers cannot be equipped without boots");s.equipment[(int)GearSlot.Feet]=new Item(ItemKind.Boots);Check(s.EquipSkates(skates,inventory,false,out _) && s.Skates==skates,"rollers install over boots");
            var roller=new RollerMotion();Vector2 velocity=Vector2.right*5;for(int n=0;n<400;n++)velocity=roller.Step(Vector2.right,velocity,5,.01f);float low=velocity.x;roller.Reset();velocity=Vector2.right*10;for(int n=0;n<400;n++)velocity=roller.Step(Vector2.right,velocity,10,.01f);Check(Mathf.Abs(velocity.x-13)<.01f && Mathf.Abs(low-6.5f)<.01f,"rollers multiply the entire current speed by 1.3");
            g.journey.SetArea(WorldArea.Coast,v.Centre+Vector2.down*12);g.body.position=v.Centre+Vector2.down*12;g.hero.transform.position=g.body.position;
            Check(v.houses.Count==10 && v.houses.Select(h=>h.entrance).Distinct().Count()==10,"ten distinct irregular households");
            var first=v.houses.Select(h=>h.entrance).ToArray();int seed=v.LayoutSeedV54;v.ApplyLayoutSeedV54(seed^5419);var second=v.houses.Select(h=>h.entrance).ToArray();Check(first.Where((p,i)=>Vector2.Distance(p,second[i])>1).Count()>=8,"new seed changes household placement");v.ApplyLayoutSeedV54(seed);Check(v.houses.Where((h,i)=>Vector2.Distance(h.entrance,first[i])>.001f).Count()==0,"layout seed deterministically restores all houses");
            Check(v.houses.All(h=>v.PathV48(h.entrance+Vector2.down*.6f,v.Centre+Vector2.down*9.5f).Count>0),"every house has a route to the central training gate");
            Check(v.fires.Take(28).Select(f=>f.logs).Distinct().Count()==1 && v.fires.Take(28).All(f=>f.widthV54<1.3f),"one shared stone hearth, many small animated tongues");
            Check(!g.loot.Any(l=>Vector2.Distance(l.position,v.Centre)<9 && (l.item.kind==ItemKind.Stake || l.item.kind==ItemKind.Axe)),"training weapons are on racks instead of the ground");
            Capture("village-day.png",v.Centre+Vector2.up*1.6f,21);
            g.seconds=200;v.Tick(.01f);Capture("village-night.png",v.Centre+Vector2.up*1.6f,13);
            for(int i=0;i<10;i++)
            {v.EnterHouse(v.houses[i]);g.body.position=new Vector2(0,-2.8f);g.hero.transform.position=g.body.position;Capture("house-"+i+".png",new Vector2(0,.65f),6.2f);Check(v.RoomWalkableV54(new Vector2(0,-3.4f)) && !Physics2D.OverlapCircleAll(new Vector2(0,-3.4f),.25f).Any(c=>!c.isTrigger && c.gameObject!=g.hero.gameObject),"clear entry passage, house "+i);v.LeaveHouse();yield return null;}
            g.seconds=60;g.journey.SetArea(WorldArea.Coast,v.Centre+Vector2.down*12);g.paused=true;
            var slaver=v.people.First(p=>p.Slaver);var cage=v.people.First(p=>p.Captive).cage;var slaverAt=slaver.Position;for(int n=0;n<300;n++)slaver.Tick(.1f);
            Check(Vector2.Distance(slaverAt,slaver.Position)<.05f && slaver.Renderer.sprite.texture.name=="SlaverLeanV54" && Vector2.Distance(slaver.Position,cage.transform.position)<2,"slaver stays leaning at the cages during daytime");
            var captives=v.people.Where(p=>p.IsSlave).ToArray();Check(captives.Select(p=>p.Renderer.sprite.rect).Distinct().Count()==4 && captives.All(p=>p.Renderer.sprite.texture.name=="CaptivesV54"),"four prisoners have distinct authored appearances");
            var woman=v.people.First(p=>!p.IsSlave && !p.Slaver && p.Weapon.kind==ItemKind.Stake);woman.Warp(v.Centre+new Vector2(-5,-2));var steps=new HashSet<Sprite>();for(int n=0;n<8;n++){woman.WalkHomeV50(woman.Position+Vector2.right*.175f);steps.Add(woman.gaitV53.Lower.sprite);}Check(steps.Count>=7,"Valkyrie walking uses actual alternating foot poses");
            var poses=new GameObject("V54 combat pose sheet");woman.SparringPartnerV48=v.people.First(p=>p!=woman && p.Weapon?.kind==ItemKind.Axe);woman.TrainingSideV48=1;Set(woman,"victim",woman.SparringPartnerV48);Call(woman,"BeginAttack");float duration=(float)woman.GetType().GetField("duration",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(woman);
            for(int n=0;n<8;n++){Set(woman,"attackAge",duration*(n+.1f)/8);Call(woman,"Pose",false);var clone=Instantiate(woman.Renderer.gameObject);clone.transform.SetParent(poses.transform,true);clone.transform.position=new Vector2(1000+n*2.0f,1000);foreach(var t in clone.GetComponentsInChildren<Transform>())t.gameObject.layer=31;}
            Capture("training-thrust.png",new Vector2(1007,1000.85f),2.4f,1<<31);Check(!woman.gaitV53.Lower.enabled && woman.BodyFrameV54==7,"attacks use full authored eight-frame body and footwork");Destroy(poses);woman.PeaceV53();
            var mob=g.woodlice.First(m=>!m.Small);mob.vitals.health=200;mob.vitals.curlRemaining=0;g.combat.active=new Item(ItemKind.GoldenSpear);Call(g.combat,"StrikeV44",new WoodlouseActor(mob));Check(Mathf.Abs(mob.vitals.health-170)<.01f && !mob.vitals.Curled && g.v44.Stunned(mob.renderer),"Golden Sting stuns an open woodlouse before dealing full damage");mob.vitals.health=200;mob.vitals.curlRemaining=4;Call(g.combat,"StrikeV44",new WoodlouseActor(mob));Check(Mathf.Abs(mob.vitals.health-194)<.01f,"already curled woodlouse retains shell protection");
            var dragon=new RoosterDragon(g.dragons,v.Centre+Vector2.up*15,g.coastRoot.transform);dragon.BeginAttack(DragonAction.Flight,Vector2.right);float hp=dragon.health;
            Check(g.HurtTarget(dragon,50,g.hero,DamageDeliveryV54.Melee)==0 && g.HurtTarget(dragon,50,g.hero,DamageDeliveryV54.Tentacle)==0 && dragon.health==hp,"flying rooster immune to melee and tentacles");Check(g.HurtTarget(dragon,36,g.hero)>0,"flying rooster can still be struck by ranged damage");Destroy(dragon.Renderer.gameObject);Destroy(dragon.shadow.gameObject);
            Check(ItemDefinition.All[ItemKind.Bow].damage==36 && ItemDefinition.All[ItemKind.Bow].cooldown==2,"bow damage and two-second interval");
            var a=v.SpawnArrow(v.Centre+new Vector2(-4,-3),Vector2.right,g.hero,true,7);float startHeight=a.height;Call(v,"TickArrows",.2f);Check(a.height>startHeight && a.verticalSpeed>0,"arrow initially rises along ballistic arc");for(int n=0;n<100 && !a.stuck;n++)Call(v,"TickArrows",.02f);Check(a.stuck && a.height<.001f,"arrow falls to ground and remains visible");
            var target=v.people.First(p=>p.WeaponMerchantV48);target.Warp(v.Centre+new Vector2(5,-3));target.health=150;var b=v.SpawnArrow(v.Centre+new Vector2(-1,-3),Vector2.right,g.hero,true,6);for(int n=0;n<50 && !b.stuck;n++)Call(v,"TickArrows",.02f);Check(b.stuck && Mathf.Abs(target.health-(150-36*.8f))<.01f,"swept arrow hits creature once with expected armor-adjusted damage");
            var wall=v.SpawnArrow(v.Centre+new Vector2(9,-2),Vector2.right,g.hero,true,7);for(int n=0;n<25 && !wall.stuck;n++)Call(v,"TickArrows",.02f);Check(wall.stuck && wall.ground.x<v.Centre.x+10.3f,"arrow collides with training fence");
            var allies=captives.Take(4).ToArray();foreach(var ally in allies){ally.Hired=true;ally.Captive=false;ally.Warp(g.body.position+Vector2.down*3);}
            var points=allies.Select(x=>g.FormationPointV54(x.Renderer)).ToArray();Check(Mathf.Abs(points[0].y-points[1].y)<.01f && Mathf.Abs(points[2].y-points[3].y)<.01f && points[2].y<points[0].y && Mathf.Abs(points[0].x-points[1].x)>.9f,"allies form two columns and successive rows");
            g.paused=false;g.ui.open=true;g.ui.QueuePointerForCheck(new Vector2(g.ui.CanvasWidth-4,g.ui.CanvasHeight-4),EventType.Repaint,0);Set(g.ui,"hoverItem",new Item(ItemKind.Zen));var rect=g.ui.TooltipRectV54;Check(g.ui.TooltipVisibleV54 && rect.xMax<g.ui.CanvasWidth && rect.yMax<g.ui.CanvasHeight,"tooltip fits beside pointer at screen edge");Set(g.ui,"hoverItem",null);Check(!g.ui.TooltipVisibleV54,"tooltip disappears immediately without a hovered item");g.ui.ClearCheckPointer();g.ui.open=false;
            GroundSheetV54();
            g.journey.SetArea(WorldArea.Catacombs,new Vector2(0,19));g.paused=true;var worm=g.cave.worms[0];for(int n=0;n<120;n++)worm.Tick(.025f);Capture("worm-v54.png",worm.Position,7);Check(worm.parts.All(p=>p.Renderer.sprite.texture.name.EndsWith("V54")) && g.cave.art.head.Select(x=>x.rect).Distinct().Count()==16,"worm has sixteen detailed frames for head, rings and tail");
            Check(CoastGame.ZenGroundAngleV45==60,"Zen is another thirty degrees further left");yield return null;
        }
        void GroundSheetV54()
        {
            var kinds=Enum.GetValues(typeof(ItemKind)).Cast<ItemKind>().Where(k=>!k.ToString().StartsWith("Open")).ToArray();var all=new List<GroundLoot>();
            for(int i=0;i<kinds.Length;i++){var at=new Vector2(1000+(i/9)*20+(i%3)*3.2f,1000-((i/3)%3)*3.2f);all.Add(game.CreateLooseLootV46(new Item(kinds[i]),at));}game.TickLoot(0);
            for(int page=0;page<Mathf.CeilToInt(kinds.Length/9f);page++)Capture("ground-items-"+page+".png",new Vector2(1003.2f+page*20,996.8f),4.6f);
            var bow=all.First(l=>l.item.kind==ItemKind.Bow);Check(bow.renderer.bounds.size.x<1.35f && bow.renderer.bounds.size.y<1.35f && Mathf.Abs(Mathf.DeltaAngle(bow.renderer.transform.eulerAngles.z,-68))<.1f,"ground bow has natural smaller size and resting orientation");
            Check(all.All(l=>l.renderer.bounds.size.x>0 && l.renderer.bounds.size.x<5 && l.renderer.bounds.size.y>0 && l.renderer.bounds.size.y<5),"all ground item dimensions are finite and within natural scale");
            File.WriteAllLines(Path.Combine(folder,"ground-sheet-items.txt"),kinds.Select((k,i)=>i+": row "+i/9+" col "+i%3+" "+k));foreach(var l in all){l.collected=true;l.renderer.enabled=l.shadow.enabled=false;if(l.cloth)l.cloth.enabled=false;}
        }
        void Capture(string name,Vector2 centre,float size,int layers=-1)
        {
            var camera=new GameObject("V54 capture").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=size;camera.transform.position=new Vector3(centre.x,centre.y,-20);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.09f,.09f,.07f);camera.nearClipPlane=.1f;camera.farClipPlane=100;camera.cullingMask=layers;Shader.SetGlobalColor("_CoastAmbient",new Color(.93f,.89f,.80f));var target=new RenderTexture(1920,1200,24);camera.targetTexture=target;camera.Render();var old=RenderTexture.active;RenderTexture.active=target;var tex=new Texture2D(1920,1200,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1920,1200),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(folder,name),tex.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;target.Release();Destroy(target);Destroy(tex);Destroy(camera.gameObject);
        }
    }
}
