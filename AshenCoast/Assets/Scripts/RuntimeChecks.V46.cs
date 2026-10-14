using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        IEnumerator ShiftRightV46(Vector2 point){game.ui.QueuePointerForCheck(point,EventType.MouseDown,1,true,1,false);yield return DrawUIV34();}
        IEnumerator AmazonVisualV46()
        {
            var g=game;var town=g.amazons;HandClearV34();g.ui.CloseContainers();g.journey.SetArea(WorldArea.Coast,town.Centre+Vector2.down*6);g.seconds=60;g.meadow.Refresh(g.body.position,true);town.Tick(.1f);g.UpdateEnvironment(0);
            g.checkCameraSize=26;g.checkCameraPosition=new Vector3(town.Centre.x,town.Centre.y+5,-20);g.ResetCamera(g.checkCameraPosition.Value);yield return DrawUIV34();yield return Capture("v46-village-overview.png");
            var ground=town.root.GetComponentsInChildren<MeshRenderer>().First(r=>r.sharedMaterial.shader.name=="AshenCoast/AmazonGroundV46");Check(ground.sharedMaterial.renderQueue==2800 && ground.sortingOrder<-15000,"training dirt renders over terrain while remaining beneath characters and props");
            g.seconds=240;town.Tick(.1f);g.UpdateEnvironment(0);g.checkCameraSize=9;g.checkCameraPosition=new Vector3(town.Centre.x,town.Centre.y+3,-20);g.ResetCamera(g.checkCameraPosition.Value);yield return DrawUIV34();yield return Capture("v46-alviel-night.png");
            var tree=town.trees[1];g.seconds=60;g.body.position=tree.position+Vector2.left;g.hero.transform.position=g.body.position;g.checkCameraSize=5;g.checkCameraPosition=new Vector3(tree.position.x,tree.position.y+2,-20);g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);
            g.survivor.equipment[0]=new Item(ItemKind.Axe);g.combat.ResetForCheck();g.combat.Begin(Vector2.right);g.combat.Advance(.65f);yield return DrawUIV34();yield return Capture("v46-axe-tree-contact.png");Check(Vector2.Distance((Vector2)g.cameraView.transform.position,tree.position+Vector2.up*2)<.1f,"native axe/tree view is centred on the actual poplar");g.combat.ResetForCheck();g.survivor.equipment[0]=null;
            for(int i=tree.hits;i<10;i++)tree.Chop(g.body.position);tree.Tick(1.3f);yield return DrawUIV34();yield return Capture("v46-fallen-poplar.png");
            var chief=town.houses.First(h=>h.chief);town.EnterHouse(chief);g.checkCameraSize=6.4f;g.checkCameraPosition=new Vector3(0,.7f,-20);g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);yield return DrawUIV34();yield return Capture("v46-chief-interior.png");
            Check(town.roomRoot.activeSelf && town.exit.sprite.texture.name=="TavernWallsV11","corrected interior walls and ordinary wooden exit are visible in the active house");
        }
        IEnumerator AmazonFinalV46()
        {
            var g=game;var town=g.amazons;var ui=g.ui;var s=g.survivor;HandClearV34();s.inventory.items.Clear();s.spatialInventory.items.Clear();s.skateSlot.items.Clear();s.equipment[(int)GearSlot.Back]=null;
            g.journey.SetArea(WorldArea.Coast,town.Centre+Vector2.down*6);g.seconds=60;g.meadow.Refresh(g.body.position,true);town.Tick(.1f);g.UpdateEnvironment(0);
            var fence=town.art.props[4];var data=fence.texture.GetPixels32();int rows=0;bool previous=false;
            for(int y=(int)fence.rect.y;y<(int)fence.rect.yMax;y++){bool opaque=false;for(int x=(int)fence.rect.x;x<(int)fence.rect.xMax;x++)if(data[y*fence.texture.width+x].a>96){opaque=true;break;}if(opaque && !previous)rows++;previous=opaque;}
            Check(rows==1,"new fence crop contains one continuous fence without floating neighbouring house strips");
            Check(town.art.props[0].rect.height>500 && town.art.props[5].rect.yMax<424,"houses retain their bases and training targets exclude the old statue pedestal");
            g.checkCameraSize=24;g.checkCameraPosition=new Vector3(town.Centre.x,town.Centre.y+5,-20);yield return DrawUIV34();yield return Capture("v46-village-overview.png");
            g.seconds=240;town.Tick(.15f);g.UpdateEnvironment(0);g.checkCameraSize=9;g.checkCameraPosition=new Vector3(town.Centre.x,town.Centre.y+4,-20);yield return DrawUIV34();yield return Capture("v46-alviel-night.png");
            var tree=town.trees[1];g.seconds=60;g.body.position=tree.position+Vector2.left*1.0f;g.hero.transform.position=g.body.position;g.combat.ResetForCheck();s.equipment[0]=new Item(ItemKind.Axe);g.UpdateEnvironment(0);g.checkCameraSize=5;g.checkCameraPosition=new Vector3(tree.position.x,tree.position.y+2,-20);
            Check(g.combat.Begin(Vector2.right),"player begins a real animated axe swing");g.combat.Advance(.65f);Check(tree.hits==1,"textured axe contact physically chops a poplar once per swing");yield return Capture("v46-axe-tree-contact.png");g.combat.ResetForCheck();s.equipment[0]=null;
            for(int i=tree.hits;i<10;i++)tree.Chop(g.body.position);tree.Tick(1.2f);Check(tree.sprite.transform.position.x>tree.position.x && tree.sprite.color.a>.95f,"tree falls toward the side opposite the axe-wielding player");yield return Capture("v46-fallen-poplar.png");
            g.body.position=town.Centre+Vector2.down*6;g.hero.transform.position=g.body.position;g.checkCameraSize=8;g.checkCameraPosition=null;
            s.inventory.Add(new Item(ItemKind.Axe));s.inventory.Add(new Item(ItemKind.Stake));s.inventory.Add(new Item(ItemKind.Bow));s.inventory.Add(new Item(ItemKind.Coin){count=10});ui.open=true;yield return DrawUIV34();yield return Capture("v46-new-weapons.png");ui.CloseContainers();
            var hunter=town.people.First(p=>p.Hunter);var captive=town.people.First(p=>p.Captive);town.Hire(hunter);town.Hire(captive);captive.weaponSlot.Add(new Item(ItemKind.Zen));int attacks=captive.AttackCount;
            var defender=town.people.First(p=>!p.IsSlave && !p.Hired && !p.Hunter);defender.hostile=true;defender.Warp(captive.Position+Vector2.right*.8f);captive.Tick(.01f);float hp=captive.health;g.HurtTarget(captive,20,defender.Renderer);
            Check(captive.AttackCount>attacks && captive.health==hp,"equipped Zen protects an ally only during his real attack");captive.Tick(1);float taken=g.HurtTarget(captive,5,defender.Renderer);Check(taken>0,"ally Zen invulnerability ends after the swing");defender.hostile=false;
            var party=town.Allies.ToArray();var old=party.Select(a=>a.Position).ToArray();g.spellcraft.MoveTo(g.body.position+Vector2.right*45,false);
            Check(party.Select((a,i)=>Vector2.Distance(a.Position,old[i])<.01f && a.HoldPosition).All(v=>v),"player-only travel leaves all new allies at their former location");g.spellcraft.MoveTo(town.Centre+Vector2.down*6,true);
            Check(town.Allies.All(a=>!a.HoldPosition && Vector2.Distance(a.Position,g.body.position)<5),"allies-inclusive spatial travel brings the new party members");
            var chief=town.houses.First(h=>h.chief);town.EnterHouse(chief);g.checkCameraSize=6.4f;g.checkCameraPosition=new Vector3(0,.8f,-20);g.UpdateEnvironment(0);yield return DrawUIV34();
            Check(town.exit.sprite.texture.name=="TavernWallsV11","dwelling exit is a wooden door rather than a pocket portal");yield return Capture("v46-chief-interior.png");town.LeaveHouse();g.checkCameraSize=null;g.checkCameraPosition=null;
        }
        IEnumerator AmazonChecksV46()
        {
            var g=game;var town=g.amazons;var ui=g.ui;var s=g.survivor;
            HandClearV34();s.inventory.items.Clear();s.spatialInventory.items.Clear();s.skateSlot.items.Clear();s.equipment[(int)GearSlot.Back]=null;s.inventory.abyssCell=null;
            g.journey.SetArea(WorldArea.Coast,town.Centre+Vector2.down*10);g.seconds=60;g.meadow.Refresh(g.body.position,true);town.Tick(.1f);g.UpdateEnvironment(0);
            Check(town.houses.Count==10 && town.houses.Count(h=>h.chief)==1,"distant village contains ten enterable cone houses, including one large chief house");
            Check(town.Centre.magnitude>180 && WorldShape.Land(town.Centre,40) && Vector2.Distance(town.Centre,WorldShape.FortPosition)>70,"village is far across playable plains and avoids the old fort");
            Check(town.people.Count(p=>p.Hunter)==2 && town.people.Count(p=>p.Slaver)==1 && town.people.Count(p=>p.Captive)==4,"village has exactly two hunters, one slaver and four caged humans");
            Check(town.people.Where(p=>!p.IsSlave).All(p=>p.MaxHealth==150) && town.people.Where(p=>p.IsSlave).All(p=>p.MaxHealth==100),"Amazon health is150; captives have100 health");
            Check(town.statue.sprite.texture.name=="AlvielStatueV46" && town.TitleRemaining>4 && town.targets.Count==4 && town.barriers.Count>20,"reference-based Alviel statue, fenced training targets and entry title are installed");
            Check(town.music.clip!=null && town.music.clip.length>30 && town.music.volume>0,"original38-second Amazon combat score plays within settlement");
            Check(g.loot.Count(l=>!l.collected && (l.item.kind==ItemKind.Axe || l.item.kind==ItemKind.Stake))>=4,"training area contains collectible axes and stakes");
            var axe=new Item(ItemKind.Axe);Check(axe.Definition.damage==30 && axe.Definition.arc==120 && axe.Definition.cooldown==1,"axe has30 damage,120-degree sweep and one attack per second");
            var knight=g.journey.residents.First(r=>r.Armored);Check(AmazonSettlementV46.WeaponDamage(axe,knight)==36 && AmazonSettlementV46.WeaponDamage(axe,new HeroCombatActor(g))==30,"axe amplifies outgoing damage to armoured targets by20 percent");
            Check(town.people.Where(p=>!p.IsSlave).Select(p=>p.Renderer.sprite.rect).Distinct().Count()>=4 && town.people.Any(p=>p.Weapon?.kind==ItemKind.Bow),"four distinct Amazon appearances and varied melee/bow equipment are present");
            g.cameraView.orthographicSize=22;g.ResetCamera(new Vector3(town.Centre.x,town.Centre.y+2,-20));yield return DrawUIV34();yield return Capture("v46-village-overview.png");
            g.cameraView.orthographicSize=8;g.ResetCamera(new Vector3(g.body.position.x,g.body.position.y+2,-20));
            g.seconds=240;town.Tick(.1f);g.UpdateEnvironment(0);var positions=new Vector4[32];var colors=new Vector4[32];int lights=town.AppendLights(positions,colors,0);
            Check(town.fires.All(f=>f.flame.enabled) && lights>=8,"night bonfires encircle the stone pedestal and illuminate lanes between houses");
            g.body.position=town.Centre+Vector2.down*4;g.hero.transform.position=g.body.position;g.ResetCamera(new Vector3(town.Centre.x,town.Centre.y+1,-20));yield return DrawUIV34();yield return Capture("v46-alviel-night.png");g.seconds=60;town.Tick(.1f);
            Check(town.fires.All(f=>!f.flame.enabled),"outdoor village flames turn off in daylight");
            s.inventory.Add(new Item(ItemKind.Coin){count=10});var hunter=town.people.First(p=>p.Hunter);g.body.position=hunter.Position+Vector2.down;g.hero.transform.position=g.body.position;
            Check(town.Interact(hunter.Renderer.bounds.center) && town.Offer==hunter,"right clicking nearby hunter opens hire/trade choice");yield return DrawUIV34();yield return Capture("v46-hunter-offer.png");
            ui.OpenAmazonShopV46(hunter);yield return DrawUIV34();Check(ui.vendorOpen && ui.VendorInventory==hunter.stock,"hunter uses the existing merchant inventory interface");
            var meat=hunter.stock.items.First(i=>i.kind==ItemKind.RawMeat);int coins=s.Coins;ui.QuickTransfer(meat,hunter.stock);
            Check(!hunter.stock.items.Contains(meat) && s.inventory.items.Contains(meat) && s.Coins==coins-1,"meat purchase moves real stock and deducts one coin");ui.CloseContainers();
            coins=s.Coins;Check(town.Hire(hunter) && hunter.Hired && s.Coins==coins-1 && hunter.Weapon!=null,"hunter can be hired for one coin while retaining her own weapon");
            var prisoner=town.people.First(p=>p.Captive);coins=s.Coins;Check(town.Hire(prisoner) && !prisoner.Captive && !prisoner.cage.enabled && s.Coins==coins-1,"one-coin captive purchase releases him from the bone cage");
            prisoner.Warp(g.body.position+Vector2.left*4);var before=prisoner.Position;prisoner.Tick(.2f);Check(prisoner.Weapon==null && prisoner.AttackCount==0 && Vector2.Distance(prisoner.Position,g.body.position)<Vector2.Distance(before,g.body.position),"unarmed purchased human follows the player and cannot attack");
            ui.OpenAllyWeaponV46(prisoner);yield return DrawUIV34();Check(ui.grids.Any(v=>v.inventory==prisoner.weaponSlot),"right-click ally equipment panel exposes his weapon slot");
            var dagger=new Item(ItemKind.Dagger);s.inventory.Add(dagger);yield return DrawUIV34();yield return Pointer(Cell(s.inventory,dagger.x,dagger.y),EventType.MouseDown,0);yield return Pointer(Cell(prisoner.weaponSlot,0,0),EventType.MouseDown,0);
            Check(prisoner.Weapon==dagger && !s.inventory.items.Contains(dagger),"dragging a melee weapon equips the purchased human without copying it");
            Check(!prisoner.weaponSlot.Add(new Item(ItemKind.Bow)) && !prisoner.weaponSlot.Add(new Item(ItemKind.Axe)),"ally slot rejects ranged weapons and a second simultaneous weapon");yield return Capture("v46-prisoner-weapon.png");ui.CloseContainers();
            var victim=town.people.First(p=>!p.Hired && !p.IsSlave && !p.Hunter && !p.Slaver);hunter.Warp(victim.Position+Vector2.right);prisoner.Warp(victim.Position+Vector2.left);
            float hp=hunter.health;g.HurtTarget(victim,1,g.hero);Check(victim.Hostile && town.people.Count(p=>p.Hostile)>1 && !hunter.Hostile && hunter.health==hp,"attacking an Amazon alerts nearby defenders; hired Amazons remain allied");
            var hostileArcher=town.people.First(p=>p.Weapon?.kind==ItemKind.Bow && !p.Hired);hostileArcher.hostile=true;hostileArcher.Warp(g.body.position+Vector2.right*5);int arrows=town.arrows.Count;for(int i=0;i<12;i++)hostileArcher.Tick(.04f);
            Check(hostileArcher.AttackCount>0 && town.arrows.Count>arrows,"Amazon archer visibly attacks with real travelling arrows");
            int attacks=prisoner.AttackCount;for(int i=0;i<16;i++)prisoner.Tick(.04f);Check(prisoner.AttackCount>attacks,"armed purchased human attacks hostile defenders");
            foreach(var person in town.people)person.hostile=false;
            var tree=town.trees.First();int wood=g.loot.Count(l=>l.item.kind==ItemKind.Wood);var player=tree.position+Vector2.left*2;
            for(int i=0;i<9;i++)tree.Chop(player);Check(tree.Standing && tree.hits==9,"rare poplar remains standing after nine axe hits");tree.Chop(player);
            int logs=g.loot.Count(l=>l.item.kind==ItemKind.Wood)-wood;Check(!tree.Standing && logs>=3 && logs<=5 && Vector2.Dot(tree.FallDirection,tree.position-player)>0,"tenth hit fells poplar away from player and drops three to five logs");tree.Tick(1.2f);tree.Tick(4.8f);Check(tree.sprite.enabled && tree.sprite.color.a>.9f,"fallen tree remains visible for five seconds on the ground");tree.Tick(1.5f);Check(!tree.sprite.enabled,"fallen tree fades out smoothly after its ground delay");
            var chief=town.houses.First(h=>h.chief);town.EnterHouse(chief);yield return DrawUIV34();Check(g.journey.Area==WorldArea.AmazonHouse && town.roomRoot.activeSelf && !g.coastRoot.activeSelf && g.map.CurrentRegion==MapRegion.AmazonHouse,"chief doorway enters a separate mapped dwelling interior");
            Check(chief.furnishings.Count==2 && chief.furnishings.All(f=>f.renderer.gameObject.activeInHierarchy),"chief hall contains persistent usable stores, furniture and a hearth");yield return Capture("v46-chief-interior.png");var store=chief.furnishings[0];int storeItems=store.inventory.items.Count;town.LeaveHouse();town.EnterHouse(chief);Check(store.inventory.items.Count==storeItems,"house contents survive exiting and re-entering");town.LeaveHouse();
            ui.CloseContainers();s.inventory.items.Clear();s.spatialInventory.items.Clear();ui.holding.items.Clear();var stack=new Item(ItemKind.Coin){count=7};s.inventory.Place(stack,0,0);ui.open=true;yield return DrawUIV34();yield return ShiftRightV46(Cell(s.inventory,0,0));
            Check(stack.count==3 && ui.DraggedItem?.count==4 && ui.DraggedItem!=stack,"Shift+RMB takes rounded-up half of an odd stack");yield return ShiftRightV46(Cell(s.inventory,2,0));
            Check(s.inventory.At(2,0)?.count==1 && ui.DraggedItem?.count==3,"Shift+RMB places precisely one held item into an empty cell");yield return ShiftRightV46(Cell(s.inventory,2,0));Check(s.inventory.At(2,0)?.count==2 && ui.DraggedItem?.count==2,"repeated single placement merges into existing stack");
            yield return Pointer(Cell(s.inventory,4,0),EventType.MouseDown,0);int total=s.inventory.items.Sum(i=>i.count);Check(total==7 && !ui.Dragging,"stack splitting and one-at-a-time placement conserve the entire quantity");
            int drops=g.loot.Count;yield return ControlPointerV44(Cell(s.inventory,2,0));Check(g.loot.Count==drops+1 && s.inventory.At(2,0)==null,"Ctrl+RMB retains the previously requested throw action");
            ui.CloseContainers();g.journey.SetArea(WorldArea.Coast,town.Centre+Vector2.down*8);g.seconds=60;town.Tick(.1f);g.UpdateEnvironment(0);
        }
    }
}
