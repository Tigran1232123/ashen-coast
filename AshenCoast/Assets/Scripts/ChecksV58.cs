using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class ChecksV58:MonoBehaviour
    {
        public CoastGame game;string folder;int passed,failed;readonly List<string> report=new List<string>();
        void Check(bool ok,string name){report.Add((ok?"PASS ":"FAIL ")+name);Debug.Log(report.Last());if(ok)passed++;else failed++;}
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-v58-output");folder=i>=0?args[i+1]:Application.dataPath+"/../V58-Checks";Directory.CreateDirectory(folder);
            game.snails.SetupOpen=false;game.paused=false;game.testMode=true;game.combat.manual=true;Time.timeScale=1;AudioListener.volume=0;QualitySettings.vSyncCount=0;Application.runInBackground=true;
            yield return null;var work=Run();while(true){bool more;object current=null;try{more=work.MoveNext();if(more)current=work.Current;}catch(Exception e){Check(false,e.ToString());break;}if(!more)break;yield return current;}
            report.Add("RESULT "+passed+" passed; "+failed+" failed");File.WriteAllLines(Path.Combine(folder,"results.txt"),report);Application.Quit(failed==0?0:1);
        }
        void Warp(WorldArea area,Vector2 p){game.journey.SetArea(area,p);game.body.position=p;game.hero.transform.position=p;game.body.linearVelocity=Vector2.zero;}
        void View(string name,Vector2 p,float size=6)
        {
            game.seconds=80;game.UpdateEnvironment(0);var cam=game.cameraView;var old=cam.targetTexture;var target=new RenderTexture(960,720,24);cam.targetTexture=target;cam.orthographicSize=size;cam.transform.position=new Vector3(p.x,p.y,-20);cam.transform.rotation=Quaternion.identity;cam.Render();var prior=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(960,720,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,960,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());RenderTexture.active=prior;cam.targetTexture=old;Destroy(image);target.Release();Destroy(target);
        }
        IEnumerator Run()
        {
            var g=game;var s=g.survivor;Check(Application.version=="58.0" && LanProtocolV49.Build==58,"V58 application and network version");
            Check(g.shipStorage.columns*g.shipStorage.rows==160 && g.shipStorage.Valid() && g.shipStorage.items.Count(i=>i.kind==ItemKind.Crossbow)==1,"hold doubled and one crossbow present");
            var book=new Item(ItemKind.LeafTome);Check(book.Definition.width==2 && book.Definition.height==1 && book.spells.Length==4 && book.spells.All(x=>x.kind==SpellKind.None && !x.locked),"closed leaf book and four empty slots");book.opened=true;Check(book.Definition.width==2 && book.Definition.height==3,"open leaf book footprint");
            var grid=new GridInventory(10,5,"Recipe test");for(int run=0;run<12;run++){grid.items.Clear();for(int n=0;n<5;n++)grid.Place(new Item(n<3?ItemKind.Rope:ItemKind.Wood),((n+run)%5)*2,n%2*2);if(grid.items.Count!=5){Check(false,"recipe test arrangement");break;}Check(grid.Craft(new RectInt(0,0,10,5),out var bow) && bow.kind==ItemKind.Bow && bow.Definition.width==1 && bow.Definition.height==3,"shapeless bow recipe "+run);}
            grid.items.Clear();grid.Place(new Item(ItemKind.Feather),4,3);grid.Place(new Item(ItemKind.Wood),1,0);Check(grid.Craft(new RectInt(0,0,6,5),out var arrows) && arrows.kind==ItemKind.Arrow && arrows.count==5,"feather and wood yield five arrows");
            grid.items.Clear();grid.Add(new Item(ItemKind.Arrow){count=29});grid.Add(new Item(ItemKind.Arrow){count=5});Check(grid.items.Sum(x=>x.count)==34 && grid.items.Any(x=>x.count==30) && grid.Valid(),"arrow stacks cap at thirty and spill without overlap");var dto=LanGridV49.Capture(grid,"test",0);var restored=new GridInventory(10,5,"restore");dto.Apply(restored);Check(restored.items.Sum(x=>x.count)==34,"thirty-arrow stacks survive network serialization");
            g.ToggleCheatsV58();s.health=1;s.mana=0;s.Tick(0);g.HurtPlayer(200);Check(s.health==s.MaxHealth && s.SpendMana(1000) && s.mana==s.MaxMana,"T infinite health and mana");g.ToggleCheatsV58();g.HurtPlayer(1);Check(s.health==s.MaxHealth-1,"T restores normal damage");s.health=s.MaxHealth;
            var food=new Item(ItemKind.RawMeat){freshness=10};g.journey.seller.items.Clear();g.journey.seller.Add(food);var carried=new Item(ItemKind.RawMeat){freshness=10};s.inventory.items.Clear();s.inventory.Add(carried);g.TickFoodV43(100);g.TickFoodV43(120);Check(food.kind==ItemKind.RawMeat && food.freshness==10 && carried.kind==ItemKind.Rot,"NPC stock stays fresh while carried food spoils");
            s.inventory.items.Clear();s.inventory.Add(new Item(ItemKind.Arrow){count=3});s.equipment[0]=new Item(ItemKind.Bow);g.combat.ResetForCheck();Warp(WorldArea.Coast,new Vector2(20,30));Check(g.combat.Begin(Vector2.right),"bow draw starts with arrows");g.combat.ChargeForCheckV58(1);Check(!g.combat.ReleaseBowV58() && s.ArrowsV58==3,"early release consumes no arrow");g.combat.Begin(Vector2.right);g.combat.ChargeForCheckV58(2);g.AnimateWalk(0);g.combat.PoseBowV58();Check(g.hero.GetComponentsInChildren<SpriteRenderer>().Count(l=>l.name.StartsWith("Bow articulated arm"))==4,"bow uses four articulated arm segments");View("bow-fully-drawn",g.body.position+Vector2.up,2.5f);Check(g.combat.ReleaseBowV58() && s.ArrowsV58==2,"two-second draw fires and consumes exactly one arrow");s.inventory.items.Clear();g.combat.ResetForCheck();Check(!g.combat.Begin(Vector2.right),"bow cannot fire without arrows");
            var actor=g.amazons.people[0];Warp(WorldArea.Coast,g.amazons.Centre+Vector2.down*11);g.amazons.Tick(0);View("amazon-village",g.amazons.Centre+Vector2.down*2,9);var descriptions=new List<string>();foreach(var p in g.amazons.people){var r=p.Renderer;var b=new MaterialPropertyBlock();r.GetPropertyBlock(b);descriptions.Add(p.Name+" "+r.sprite.name+" "+r.sprite.rect+" pivot="+r.sprite.pivot+" ppu="+r.sprite.pixelsPerUnit+" bounds="+r.bounds+" floor="+b.GetFloat("_BodyFloor"));}File.WriteAllLines(Path.Combine(folder,"amazon-geometry.txt"),descriptions);
            for(int frame=0;frame<8;frame++){actor.CheckPoseV58(frame,true,false);Check(actor.gaitV53.Lower.sprite.texture.name=="AmazonLowerV58" && actor.gaitV53.Lower.flipX,"Amazon authored walk step and right-facing boots "+frame);View("amazon-walk-"+frame,actor.Position+Vector2.up*.8f,1.4f);}actor.CheckPoseV58(4,false,true);View("amazon-attack",actor.Position+Vector2.up*.8f,1.8f);actor.PeaceV53();
            var chief=g.amazons.houses[0];View("chief-house",chief.entrance+Vector2.up*4,6);g.amazons.EnterHouse(chief);View("amazon-house-interior",Vector2.zero,6);g.amazons.LeaveHouse();
            g.snails.Enter(0);View("snail-interior",new Vector2(.2f,-.5f),5.5f);var h=g.snails.houses[0];h.moving=true;Vector3 rock=Vector3.zero;for(int n=0;n<40;n++){h.moving=true;rock=g.SnailRockV58();yield return null;}Check(rock.sqrMagnitude>.0001f,"moving snail rocks interior camera");h.moving=false;for(int n=0;n<80;n++){rock=g.SnailRockV58();yield return null;}Check(rock.sqrMagnitude<.00001f,"interior rocking fades after stop");g.snails.Leave();
            for(int arm=0;arm<3;arm++){float t=RiverV58.BridgeT(arm);var c=RiverV58.Centre(arm,t);var route=RiverNavigationV58.Plan(c-RiverV58.Normal(arm,t)*26,c+RiverV58.Normal(arm,t)*26);Check(route.Count>0 && route.All(p=>RiverV58.Passable(p)),"ground route uses bridge "+arm);Check(RiverV58.Bank(c)<-20 && RiverV58.OnBridge(c) && WorldShape.Land(c),"bridge "+arm+" crosses full river");Check(!g.GroundAllowedV58(RiverV58.Centre(arm,t+.04f)) && !WorldShape.Land(RiverV58.Centre(arm,t+.04f)),"water blocks passage beside bridge "+arm);View("river-bridge-"+arm,c,29);}
            var map=g.map.regions[0];var riverPoint=RiverV58.Centre(1,.2f);int mx=Mathf.FloorToInt((riverPoint.x-map.bounds.xMin)/map.cellSize),my=Mathf.FloorToInt((riverPoint.y-map.bounds.yMin)/map.cellSize);var water=map.texture.GetPixel(mx,my);Check(water.a>.9f && water.b>water.r,"river is painted blue on world map");File.WriteAllBytes(Path.Combine(folder,"world-map-river.png"),map.texture.EncodeToPNG());
            int originalSeed=WorldShape.Seed;bool landmarksSafe=true;for(int seed=1;seed<=64;seed++){WorldShape.Configure(seed*7919);var village=AmazonSettlementV46.CentreForWorldV56();RiverV58.Configure(village);WorldShape.AvoidRiverV58(village);landmarksSafe&=WorldShape.Land(WorldShape.FortPosition,30) && WorldShape.Land(CatacombLayout.Entrance(seed*7919),14);}
            WorldShape.Configure(originalSeed);WorldShape.AmazonPosition=g.amazons.Centre;RiverV58.Configure(g.amazons.Centre);WorldShape.AvoidRiverV58(g.amazons.Centre);Check(landmarksSafe,"fort and catacomb entries remain dry across sixty-four random worlds");
            View("river-fork",RiverV58.Junction,90);Check(RiverV58.Bank(RiverV58.Junction)<-20,"rounded Y junction");
            Warp(WorldArea.Coast,new Vector2(18,35));var station=g.buildings.CreateStation(ItemKind.Crossbow,new Vector2Int(20,35),false);s.inventory.items.Clear();s.inventory.Add(new Item(ItemKind.Arrow){count=3});Check(station.collider.size.x<1 && station.collider.offset.x<-.5f,"crossbow collision follows the wooden pedestal");Check(g.MountV58(station) && g.MovementLocked,"stationary crossbow can be mounted");Check(g.FireCrossbowV58(Vector2.right) && s.ArrowsV58==2 && !g.FireCrossbowV58(Vector2.right),"crossbow consumes ammo and enforces two seconds");var bolt=g.amazons.arrows.Last();Check(bolt.damageV58==100 && bolt.velocity.magnitude==90 && bolt.straightV58 && station.renderer.bounds.size.x<2.31f,"compact crossbow fires high-speed straight piercing projectile for 100");g.CheckMountedPoseV58();View("crossbow-mounted",station.Position+Vector2.up,3.5f);g.DismountV58();
            g.ui.open=true;g.ui.OpenShip();g.ui.CloseFromEscapeV58();Check(!g.ui.open && !g.ui.shipOpen && !g.paused,"Escape closes inventory without pausing");
            yield return AuditFloorsV58();
            yield return null;
        }
    }
    public sealed partial class VillageActorV46
    {public void CheckPoseV58(int frame,bool walking,bool attacking){step=frame/8f*1.4f;duration=2;attackAge=attacking?frame/8f*duration:-1;aim=Vector2.right;PoseV54(walking);}}
}
