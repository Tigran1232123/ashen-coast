using System.Collections;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        IEnumerator BuildingVictoryChecks()
        {
            var g=game;var s=g.survivor;var b=g.buildings;var r=g.dragons;var ui=g.ui;
            g.meditation.Stop();g.ui.CloseContainers();g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));g.seconds=40;
            g.checkCameraPosition=new Vector3(16,16,-20);g.checkCameraSize=6;g.ResetCamera(g.checkCameraPosition.Value);s.health=100;s.mana=100;
            Check(g.shipStorage.items.Count(i=>i.kind==ItemKind.IronIngot)==5,"five iron ingots are physically present in the starting ship hold");
            Check(g.art.icons.Skip(31).All(p=>p && (p.texture.name.EndsWith("V28") || p.texture.name=="AnvilV30")),"new raw meat, worktable, ingot and anvil sprites are loaded");
            s.inventory.items.Clear();var table=new Item(ItemKind.CraftingTable);s.inventory.Add(table);
            Vector2 target=new Vector2(17.2f,15.1f);Physics2D.SyncTransforms();
            Check(b.Begin(table,s.inventory),"right-clickable table enters shared construction mode");b.RefreshPreview(target);
            Check(b.vertical && b.ghost.color.a<.5f && b.ghost.transform.position==new Vector3(17.5f,16,0),"translucent table preview aligns to the exact vertical 1x2 cells");
            yield return Capture("v28-table-grid-preview-vertical.png");b.Rotate();b.RefreshPreview(target);
            Check(!b.vertical && b.ghost.transform.position==new Vector3(18,15.5f,0),"wheel rotation changes table preview to exact horizontal 2x1 cells");
            yield return Capture("v28-table-grid-preview-horizontal.png");
            Check(b.Place(target) && !b.Placing && !s.inventory.items.Contains(table),"placing a table consumes exactly the carried structure item");
            var placed=b.stations.Last();Check(placed.size==new Vector2Int(2,1) && placed.collider.size.x>1.9f && placed.collider.size.y<1,"table collider follows its two occupied grid cells");
            Check(!b.CanPlace(target,ItemKind.Campfire,true),"another building cannot overlap the occupied table cells");
            Check(g.UseWorldRightClick(placed.Position) && ui.stationOpen==placed,"world RMB on the placed table opens its station");
            yield return null;yield return new WaitForEndOfFrame();
            Check(ui.grids.Any(v=>v.inventory==placed.inventory) && placed.inventory.columns==5 && placed.inventory.rows==5 && placed.inventory.craftLimit==4,"table exposes a real 5x5 inventory and four-cell crafting limit");
            foreach(var p in new[]{new Vector2Int(0,0),new Vector2Int(2,0),new Vector2Int(1,1),new Vector2Int(0,2),new Vector2Int(2,2)})placed.inventory.Place(new Item(ItemKind.IronIngot),p.x,p.y);
            yield return Capture("v28-anvil-recipe-at-table.png");
            yield return Pointer(Cell(placed.inventory,0,0),EventType.MouseDown,1);yield return Pointer(Cell(placed.inventory,3,2),EventType.MouseDrag,1);yield return Pointer(Cell(placed.inventory,3,2),EventType.MouseUp,1);
            Check(placed.inventory.items.Count==1 && placed.inventory.items[0].kind==ItemKind.Anvil,"actual held-RMB selection crafts the anvil using the attached diagram");
            var anvilItem=placed.inventory.items[0];ui.CloseContainers();g.body.position=new Vector2(20,17);g.hero.transform.position=g.body.position;Physics2D.SyncTransforms();
            Check(b.Begin(anvilItem,placed.inventory),"crafted anvil is placeable from the table inventory");b.RefreshPreview(new Vector2(22.1f,17.1f));
            Check(b.Place(new Vector2(22.1f,17.1f)),"anvil commits its aligned ground footprint");var anvil=b.stations.Last();
            Check(anvil.size==new Vector2Int(2,2) && anvil.inventory.rows==5 && !anvil.inventory.craftingEnabled,"anvil has a 5x5 window and no crafting applications yet");
            var f1=g.arcane.CreateCampfire(new Vector2(21.5f,17.5f));b.Tick(10);float one=anvil.temperature;
            var f2=g.arcane.CreateCampfire(new Vector2(23.5f,16.5f));b.Tick(10);Check(anvil.temperature>one+50,"more adjacent campfires raise anvil temperature");
            g.body.position=new Vector2(20.5f,18);g.hero.transform.position=g.body.position;ui.OpenStation(anvil);yield return Capture("v28-anvil-temperature.png");ui.CloseContainers();
            s.inventory.items.Clear();var campfire=new Item(ItemKind.Campfire);s.inventory.Add(campfire);g.body.position=new Vector2(15,18);g.hero.transform.position=g.body.position;Physics2D.SyncTransforms();
            Check(b.Begin(campfire,s.inventory),"campfire uses the same translucent full-world grid preview");b.RefreshPreview(new Vector2(16.1f,18.9f));
            Check(b.ghost.transform.position==new Vector3(16.5f,18.5f,0) && b.Place(new Vector2(16.1f,18.9f)),"campfire always occupies one exact cell despite an arbitrary cursor position");
            s.inventory.items.Clear();var dagger=new Item(ItemKind.Dagger);var sword=new Item(ItemKind.Sword);s.inventory.Place(dagger,0,0);s.equipment[0]=sword;ui.open=true;
            yield return null;yield return new WaitForEndOfFrame();yield return Pointer(Cell(s.inventory,0,0),EventType.MouseDown,1);
            Check(s.equipment[0]==dagger && s.inventory.items.Contains(sword),"RMB equips into an occupied slot and returns the old weapon to inventory");
            s.inventory.items.Clear();s.inventory.Place(sword,0,0);yield return Pointer(Cell(s.inventory,0,0),EventType.MouseDown,0);yield return Pointer(Gear(GearSlot.Hand),EventType.MouseDown,0);
            Check(s.equipment[0]==sword && !ui.Dragging && s.inventory.items.Contains(dagger),"picked-up item equips by LMB on the weapon slot without duplicating old gear");
            var original=s.inventory;s.inventory=new GridInventory(2,1,"full");var yoyo=new Item(ItemKind.YoYo);s.inventory.Place(yoyo,0,0);s.inventory.Place(new Item(ItemKind.Coin),1,0);int oldLoot=g.loot.Count;
            Check(g.AutoEquipItem(yoyo,s.inventory) && s.equipment[0]==yoyo && g.loot.Count==oldLoot+1 && g.loot.Last().item==sword && g.loot.Last().InFlight,"old weapon is thrown with animation when the player inventory cannot hold it");s.inventory=original;ui.CloseContainers();
            var flower=g.flowers.First(x=>!x.picked);int pickups=g.meadow.RememberedPickups;
            r.Burn(flower.position,g.coastRoot.transform);Check(flower.picked && !flower.stems.enabled && r.dust.Count>=5,"dragon fire destroys visible flowers and releases pixel ash");
            if(flower.procedural)Check(g.meadow.RememberedPickups>pickups,"burnt procedural flowers remain gone after leaving their chunk");
            r.Clear();g.body.position=new Vector2(18,13);g.hero.transform.position=g.body.position;s.health=100;var boss=r.Spawn(new Vector2(22,15),g.coastRoot.transform);boss.Tick(.4f);
            int shakes=g.CameraShakes,meat=g.loot.Count(l=>l.item.kind==ItemKind.RawMeat);boss.Hurt(2000);
            r.Tick(.3f);Check(!boss.DeathLanded && boss.DroppedMeat==0 && g.CameraShakes==shakes,"defeated dragon visibly rolls before landing and dropping loot");yield return Capture("v28-dragon-backflip.png");
            r.Tick(1);g.TickLoot(.5f);
            Check(boss.DeathLanded && boss.DroppedMeat==7 && g.CameraShakes==shakes+1 && g.loot.Count(l=>l.item.kind==ItemKind.RawMeat)==meat+7,"landing gives one small camera shake and exactly seven separate raw meat drops");
            Check(boss.Renderer.sprite.texture.name=="RoosterDefeatV30" && Mathf.Abs(Mathf.DeltaAngle(boss.Renderer.transform.eulerAngles.z,0))<8,"dragon corpse rests on its back");g.numbers.Clear();yield return Capture("v28-dragon-corpse-tongue-and-loot.png");
            r.Tick(9.5f);Check(!boss.Finished && boss.Renderer.color.a>.95f,"corpse persists for ten seconds after landing");r.Tick(.6f);
            Check(!boss.Finished && r.dust.Count>0,"corpse evaporates with drifting pixel particles after ten seconds");yield return Capture("v28-dragon-evaporation.png");r.Tick(2);Check(!r.bosses.Contains(boss),"evaporation removes the complete corpse and tongue");
            s.health=100;Check(g.meditation.Toggle(),"meditation can be started normally");g.meditation.Tick(2,false);Check(Time.timeScale>1 && Time.timeScale<5,"whole-world acceleration ramps smoothly");g.meditation.Tick(2,false);
            Check(Time.timeScale==5 && g.meditation.DayOffset==0,"meditation accelerates world time, physics, enemies and fire together at five times");
            g.UpdateEnvironment(0);yield return Capture("v28-weakened-meditation-blur.png");
            Check(g.pixelView.atmosphere.GetFloat("_Meditation")<.17f,"meditation blur is reduced to sixteen percent of the original strength");g.meditation.Stop();g.meditation.Tick(.4f,false);Check(Time.timeScale==1,"leaving meditation restores normal world speed");
            Check(g.soundscape.battle.clip.name=="DragonViolinV28" && g.soundscape.battle.clip.length>50,"more prominent original epic violin battle arrangement is included");
            g.checkCameraPosition=null;g.checkCameraSize=null;g.ui.CloseContainers();s.health=100;
        }
    }
}
