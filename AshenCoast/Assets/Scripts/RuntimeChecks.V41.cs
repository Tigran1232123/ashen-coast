using System.Collections;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        IEnumerator ClickBookV41(Item book,GridInventory source)
        {var at=Cell(source,book.x,book.y);yield return Pointer(at,EventType.MouseDown,1);yield return Pointer(at,EventType.MouseUp,1);}
        IEnumerator BooksPickupV41()
        {
            var g=game;var s=g.survivor;var ui=g.ui;HandClearV34();s.inventory.items.Clear();s.spatialInventory.items.Clear();s.abyssCell=s.inventory.abyssCell=null;s.health=100;g.seconds=50;g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));s.equipment[0]=null;g.SetAutoPickup(false);
            var pyro=new Item(ItemKind.Pyromancy);var tome=new Item(ItemKind.Tome);var abyss=new Item(ItemKind.AbyssTome);s.inventory.Place(pyro,0,0);s.inventory.Place(tome,3,0);s.inventory.Place(abyss,6,0);s.inventory.Place(new Item(ItemKind.Spectrum),9,0);ui.open=true;yield return DrawUIV34();
            Check(g.art.icons[24].texture.name=="FireBookV44" && g.art.icons[25].texture.name=="FireBookV44" && g.art.icons[26].texture.name=="AbyssBookV43" && g.art.icons[27].texture.name=="AbyssBookV43","new minimal fire and abyss books load both closed and opened poses");
            Check(g.art.icons[17].texture.name=="TomesV5" && g.art.icons[18].texture.name=="OpenFolioV43","spatial folio keeps its original closed and open illustrations");
            Check(InventoryUI.ItemIconArea(tome,Vector2.zero).size==InventoryUI.ItemIconArea(abyss,Vector2.zero).size && CoastArtV41.DisplayAspect(tome)==CoastArtV41.DisplayAspect(abyss) && InventoryUI.ItemIconArea(pyro,Vector2.zero).height>74,"closed 2x2 tomes have equal near-square visual size and the fire book fills its 1x2 footprint");yield return Capture("v41-closed-books-and-muted-spectrum-cells.png");
            foreach(var book in new[]{pyro,tome,abyss})
            {
                yield return ClickBookV41(book,s.inventory);Check(book.opened && ui.bookSelected==null,"short RMB click opens "+book.kind+" itself without spell slots");
                yield return SelectBookFrameV40(book,s.inventory);Check(ui.bookSelected==book && book.opened,"RMB drag frame opens "+book.kind+" spell slots without changing book state");
                yield return ClickBookV41(book,s.inventory);Check(!book.opened && ui.bookSelected==null,"short RMB click closes "+book.kind+" without spell slots");
            }
            yield return ClickBookV41(pyro,s.inventory);yield return ClickBookV41(abyss,s.inventory);yield return Capture("v41-open-books.png");
            s.Bag.items.Clear();s.inventory.Transfer(abyss,s.Bag,0,0);ui.ToggleBag();yield return DrawUIV34();yield return ClickBookV41(abyss,s.Bag);Check(!abyss.opened && ui.bookSelected==null,"short RMB click cannot open a book in other storage");yield return SelectBookFrameV40(abyss,s.Bag);Check(ui.bookSelected==abyss && !abyss.opened,"frame selection still exposes spell slots in other storage");
            ui.CloseContainers();ui.Notice(null);s.inventory.items.Clear();s.spatialInventory.items.Clear();s.Bag.items.Clear();var sack=new Item(ItemKind.Sack){durability=0};sack.Wear(50);Check(!sack.Definition.Durable && sack.Usable && sack.durability==0,"sack has no durability restriction or wear even for a previously damaged item");
            var equipped=new Item(ItemKind.Sword);s.equipment[0]=equipped;
            var brain=g.ScatterLoot(new Item(ItemKind.Brain),g.body.position+Vector2.right*.65f,g.body.position);var dagger=g.ScatterLoot(new Item(ItemKind.Dagger),g.body.position+Vector2.right*1.2f,g.body.position);var far=g.ScatterLoot(new Item(ItemKind.Clay),g.body.position+Vector2.right*2.3f,g.body.position);g.TickLoot(.7f);
            g.TickAutoPickup(.2f);Check(!brain.collected && !dagger.collected,"disabled automatic pickup leaves nearby ground items untouched");g.SetAutoPickup(true);g.TickAutoPickup(.2f);
            Check(brain.collected && dagger.collected && s.inventory.items.Contains(brain.item) && s.inventory.items.Contains(dagger.item) && s.equipment[0]==equipped && !far.collected,"auto pickup stores nearby original items in main inventory without auto-equipping or reaching distant loot");
            s.inventory.items.Clear();for(int y=0;y<5;y++)for(int x=0;x<10;x++)s.inventory.Place(new Item(ItemKind.Clay),x,y);var blocked=g.ScatterLoot(new Item(ItemKind.Brain),g.body.position+Vector2.down*.7f,g.body.position);g.TickLoot(.7f);g.TickAutoPickup(.2f);
            Check(!blocked.collected && s.inventory.items.Count==50 && s.Bag.items.Count==0,"full main inventory leaves auto loot on the floor and never uses the empty sack");s.inventory.items.RemoveAt(49);g.TickAutoPickup(.2f);Check(blocked.collected && s.inventory.items.Contains(blocked.item),"auto pickup retries successfully after a main inventory cell is freed");
            s.inventory.items.Clear();var airborne=g.ScatterLoot(new Item(ItemKind.Brain),g.body.position+Vector2.left*.7f,g.body.position);g.TickAutoPickup(.2f);Check(airborne.InFlight && !airborne.collected,"auto pickup waits until thrown objects have landed");g.TickLoot(.7f);g.TickAutoPickup(.2f);Check(airborne.collected,"landed objects become eligible for automatic pickup");
            int picked=g.flowers.Count(f=>f.picked);g.TickAutoPickup(.2f);Check(g.flowers.Count(f=>f.picked)==picked,"automatic pickup never harvests growing flowers");
            g.SetAutoPickup(false);g.paused=true;Time.timeScale=0;yield return DrawUIV34();yield return Pointer(ui.AutoPickupButton.center,EventType.MouseDown,0);Check(g.AutoPickup,"actual settings button turns automatic pickup on");yield return Capture("v41-auto-pickup-settings.png");yield return Pointer(ui.AutoPickupButton.center,EventType.MouseDown,0);Check(!g.AutoPickup,"actual settings button turns automatic pickup off");g.paused=false;Time.timeScale=1;ui.ClearCheckPointer();
            var spectrum=g.ScatterLoot(new Item(ItemKind.Spectrum),g.body.position+Vector2.down*.7f,g.body.position);g.TickLoot(.7f);ViewV34(spectrum.position+Vector2.up*.4f,3);
            var bounds=spectrum.renderer.bounds;var hover=Enumerable.Range(0,32*32).Select(i=>new Vector2(Mathf.Lerp(bounds.min.x,bounds.max.x,(i%32+.5f)/32),Mathf.Lerp(bounds.min.y,bounds.max.y,(i/32+.5f)/32))).First(p=>g.interactions.At(p,out _)==spectrum.renderer);
            g.HighlightAt(hover);var v=g.cameraView.WorldToViewportPoint(hover);ui.QueuePointerForCheck(new Vector2(v.x*ui.CanvasWidth,(1-v.y)*ui.CanvasHeight),EventType.Repaint,0);yield return Capture("v41-large-rainbow-spectrum-hover-name.png");
            Check(g.HoverName==ItemDefinition.All[ItemKind.Spectrum].name,"ground Spectrum exposes its name to the larger animated rainbow hover label");yield return new WaitForSecondsRealtime(.35f);yield return Capture("v41-spectrum-hover-next-color.png");ui.ClearCheckPointer();g.HighlightAt(null);s.equipment[0]=null;
        }
        IEnumerator FrogCombatV41()
        {
            var g=game;var a=g.arcane.frogs.First();a.Reset();a.Warp(new Vector2(20,20));g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));g.survivor.health=100;
            var b=new FrogMage(g.arcane,a.Position+Vector2.right*2);g.arcane.frogs.Add(b);g.PlayerStruck.Clear();g.feedback.Forget(a.renderer);
            Check(g.HurtTarget(new FrogActor(a),8,b.renderer)>0 && a.Hostile && a.Aggressor==b.renderer && a.TargetRenderer==b.renderer,"frog attacked by another creature targets that exact living attacker");
            Check(g.feedback.WasHit(a.renderer) && !g.PlayerStruck.Contains(a.renderer),"NPC damage independently reveals the victim HP bar without being labeled a player attack");
            a.Tick(.65f,false);Check(g.arcane.flames.Any(f=>f.source==a.renderer && f.targetOnly==b.renderer && f.hostile),"frog fire carries the frog source and actual NPC victim");
            float hp=b.health,player=g.survivor.health;for(int i=0;i<48;i++)g.arcane.Tick(.025f);Check(b.health<hp && g.survivor.health==player,"real frog fire damages its creature attacker while leaving the uninvolved player unharmed");
            g.body.position=a.Position+Vector2.down*3;g.hero.transform.position=g.body.position;ViewV34(a.Position+Vector2.up,5);yield return Capture("v41-npc-combat-reveals-health-bars.png");Check(g.ui.actorLabelBounds.Count>0,"native HUD draws visible HP bars after NPC combat");
            g.HurtTarget(new FrogActor(a),1,g.hero);Check(a.TargetRenderer==g.hero,"a new real player attack retargets the frog to the player");
            g.HurtTarget(new FrogActor(a),1,b.renderer);b.health=0;b.renderer.enabled=false;a.Tick(.01f,false);Check(!a.Hostile && a.TargetRenderer==null,"frog loses a dead aggressor without automatically blaming the player");
            a.Reset();a.Warp(new Vector2(20,20));g.HurtTarget(new FrogActor(a),2,null);Check(!a.Hostile && a.TargetRenderer==null,"unattributed damage does not invent player aggression");
            var mage=g.journey.companion;mage.Hire(a.Position+Vector2.right*2);mage.health=CompanionMage.MaxHealth;g.arcane.DamageArea(a.Position,.3f,3,false);Check(a.Aggressor==mage.sprite && a.TargetRenderer==mage.sprite,"allied-mage area damage attributes frog retaliation to the mage");a.Reset();mage.sprite.gameObject.SetActive(false);
        }
        IEnumerator MapPointerV41(Vector2 point,EventType type,int button,Vector2 delta=default)
        {game.map.QueueInputForCheck(point,type,button,delta);yield return DrawUIV34();}
        IEnumerator RainbowMapV41()
        {
            var g=game;var rainbow=g.spectrum;HandClearV34();g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));g.survivor.mana=100;g.survivor.equipment[0]=new Item(ItemKind.Spectrum);foreach(var f in g.arcane.flames)Destroy(f.renderer.gameObject);g.arcane.flames.Clear();g.ui.Notice(null);
            var origin=g.body.position+Vector2.right*2;ViewV34(origin+Vector2.up*3,8);Check(rainbow.Summon(origin),"rainbow summons for departure and map verification");rainbow.Tick(.8f);Check(g.map.OpenTravel(-1,true),"rainbow map opens in unrestricted travel mode");yield return DrawUIV34();var centre=g.map.Centre;
            yield return MapPointerV41(g.map.Plot.center,EventType.MouseDown,0);Check(g.map.Open && !rainbow.Riding,"LMB down on rainbow map begins pan without starting flight");yield return MapPointerV41(g.map.Plot.center+new Vector2(-96,48),EventType.MouseDrag,0,new Vector2(-96,48));Check(Vector2.Distance(g.map.Centre,centre)>5 && g.map.Open && !rainbow.Riding,"holding LMB actually pans the rainbow map");yield return MapPointerV41(g.map.Plot.center,EventType.MouseUp,0);yield return Capture("v41-rainbow-map-pan-and-rmb-travel.png");
            var destination=g.map.Centre;Check(g.spellcraft.CanLand(g.map.CurrentRegion,destination),"panned rainbow map centre is a valid landing point");yield return MapPointerV41(g.map.Plot.center,EventType.MouseDown,1);Check(!g.map.Open && rainbow.Riding,"RMB on rainbow map selects destination and starts the flight");
            rainbow.Tick(.3f);Check(rainbow.DepartureEffects==0 && rainbow.clouds.All(c=>c.enabled),"ground clouds stay attached during the initial player jump");yield return Capture("v41-rainbow-player-jump.png");rainbow.Tick(.3f);
            Check(rainbow.DepartureEffects==1 && rainbow.DeparturePosition==origin && rainbow.DepartureClouds.All(c=>c.enabled) && rainbow.clouds.All(c=>!c.enabled),"on takeoff old clouds stay at the departure point and flying rainbow has no clouds");
            float tail=rainbow.DepartureTail;rainbow.Tick(.25f);Check(rainbow.DepartureTail<tail && rainbow.DepartureClouds.All(c=>Mathf.Abs(c.transform.position.y-origin.y)<1),"departure curtain falls into stationary clouds using the dismissal animation");ViewV34(origin+Vector2.up*5,10);yield return Capture("v41-rainbow-leaves-clouds-on-ground.png");
            rainbow.Tick(1.4f);rainbow.Tick(.35f);Check(rainbow.Position==destination && rainbow.clouds.All(c=>c.enabled) && rainbow.clouds.All(c=>Mathf.Abs(c.transform.position.y-rainbow.wall.transform.position.y)<1),"arrival clouds appear at the falling rainbow end in the new location");ViewV34(destination+Vector2.up*10,9);yield return Capture("v41-rainbow-arrival-with-clouds.png");Check(g.hero.sortingOrder>rainbow.clouds.Max(c=>c.sortingOrder),"arriving rider renders above the waterfall spray and remains visible");rainbow.Tick(.45f);
            Check(!rainbow.Riding && rainbow.Active && Vector2.Distance(g.body.position,destination)<.01f,"player lands at the RMB-selected destination with the rainbow remaining active");rainbow.Tick(.2f);Check(rainbow.DepartureEffects==0,"departure mist and curtain clean up after their complete fading animation");yield return Capture("v41-rainbow-landed.png");rainbow.Dismiss();rainbow.Tick(2.6f);
        }
    }
}
