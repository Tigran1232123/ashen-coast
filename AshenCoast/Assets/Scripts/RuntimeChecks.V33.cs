using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        IEnumerator CatacombChecks()
        {
            var g=game;var c=g.cave;g.seconds=40;g.survivor.health=100;g.survivor.mana=100;g.ui.CloseContainers();g.prism.StopChannel();
            Check(g.shipStorage.Valid() && g.shipStorage.items.Count(i=>i.kind==ItemKind.Clay)==14,"ship hold fits fourteen additional clay without overlapping existing resources");
            var pattern=new[]{new Vector2Int(0,0),new Vector2Int(2,0),new Vector2Int(0,1),new Vector2Int(2,1),new Vector2Int(0,2),new Vector2Int(1,2),new Vector2Int(2,2)};
            var inventory=g.survivor.inventory;inventory.items.Clear();foreach(var p in pattern)inventory.Place(new Item(ItemKind.Clay),p.x,p.y);
            Check(inventory.Craft(new RectInt(0,0,3,3),out var pot) && pot.kind==ItemKind.StoragePot && pot.contents.columns==5 && pot.contents.rows==5 && !pot.contents.craftingEnabled,"exact seven-clay U crafts a 25-slot storage pot in the main inventory");
            var table=new GridInventory(5,5,"craft table"){craftLimit=4};foreach(var p in pattern)table.Place(new Item(ItemKind.Clay),p.x,p.y);Check(table.Craft(new RectInt(0,0,3,3),out var second) && second.kind==ItemKind.StoragePot,"the same seven-clay recipe works on a crafting table");
            pot.contents.Add(new Item(ItemKind.Pie));g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));Check(g.buildings.Begin(pot,inventory) && g.buildings.Place(new Vector2(17.2f,15.4f)),"carried pot can be placed from the inventory onto valid ground");
            var station=g.buildings.stations.Last();Check(station.size==Vector2Int.one && station.Position==new Vector2(17.5f,15.5f) && station.renderer.bounds.size.x<1 && station.inventory==pot.contents && station.inventory.items.Count==1,"placed pot occupies exactly one world cell and preserves its contents");
            Check(g.buildings.Interact(station.Position) && g.ui.stationOpen==station,"right click opens the pot as the intended persistent storage");
            g.checkCameraPosition=new Vector3(17,16,-20);g.checkCameraSize=4;g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);yield return Capture("v33-pot-storage.png");g.ui.CloseContainers();
            Check(c.Position==CatacombLayout.Entrance(g.meadow.Seed) && CatacombLayout.Entrance(g.meadow.Seed+1)!=c.Position && WorldShape.Land(c.Position,15) && Vector2.Distance(c.Position,g.fort.Position)>35,"catacomb entrance is seeded random, reproducible, on land and separate from the fort");
            g.journey.SetArea(WorldArea.Coast,c.Position+Vector2.down);g.checkCameraPosition=new Vector3(c.Position.x,c.Position.y+1.8f,-20);g.checkCameraSize=5;g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);yield return Capture("v33-catacomb-entrance.png");
            Check(c.CanInteractAt(c.entrance.bounds.center) && g.UseWorldRightClick(c.entrance.bounds.center) && c.Inside && c.root.activeInHierarchy && !g.coastRoot.activeInHierarchy,"actual right click enters a distinct catacomb world and hides the coast");
            Check(g.map.CurrentRegion==MapRegion.Catacombs && g.map.regions.Length==6 && g.journey.ActiveRoot==c.root.transform,"new area integrates with exploration map and the active world root");
            Check(CatacombLayout.Rooms.Length==5 && CatacombLayout.Radii.Last()==CatacombLayout.Radii.Max() && CatacombLayout.Rooms.All(p=>c.Path(new Vector2(-8,-18.5f),p).Count>0),"all five irregular rooms connect to the entrance and the final boss room is largest");
            Check(c.walls.All(w=>w.Value.size==Vector2.one && (Vector2)w.Value.transform.position==new Vector2(w.Key.x+.5f,w.Key.y+.5f) && !c.floor.Contains(w.Key)),"every stone wall collider occupies its drawn wall cell rather than an open floor cell");
            bool sealedEdges=true;foreach(var p in c.floor)foreach(var d in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right})if(!c.floor.Contains(p+d)){var start=(Vector2)p+Vector2.one*.5f;var end=start+(Vector2)d;sealedEdges&=g.WorldBlocked(start,end,.22f);}
            Check(sealedEdges,"all floor-to-wall edges block swept character and combat motion");
            g.body.position=new Vector2(-8,-16);g.hero.transform.position=g.body.position;g.checkCameraPosition=new Vector3(-8,-14.5f,-20);g.checkCameraSize=6;g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);c.Tick(0);yield return Capture("v33-first-catacomb-room.png");
            var spider=c.spiders[0];g.body.position=spider.Position+Vector2.down*2.5f;g.hero.transform.position=g.body.position;spider.cooldown=0;spider.Tick(.01f);spider.Tick(.36f);
            Check(spider.Attacking && c.bolts.Count>0 && spider.sprite==spider.Renderer && spider.Renderer.sprite.texture.name=="SpiderV33","spider winds up an authored spit animation and fires a visible web projectile");
            for(int i=0;i<30;i++)c.Tick(.02f);Check(c.Snared && g.FeetLocked && !g.MovementLocked && c.webs.Count>0,"actual web contact immobilizes only the legs and deposits floor webbing");
            g.checkCameraPosition=new Vector3(g.body.position.x,g.body.position.y+1.5f,-20);g.ResetCamera(g.checkCameraPosition.Value);yield return Capture("v33-spider-web-hit.png");
            c.snare=0;var web=c.AddWeb(g.body.position);Check(c.SlowMultiplier<.5f && g.MovementSpeed<CoastGame.WalkSpeed*.5f,"standing in floor webbing slows ordinary movement after the snare ends");
            var visibleWeb=c.AddWeb(g.body.position+Vector2.right);g.cameraView.Render();var webOn=ReadPixels();visibleWeb.sprite.enabled=false;g.cameraView.Render();var webOff=ReadPixels();float webPixels=RegionDifference(webOn,webOff,new Rect(visibleWeb.point.x-.8f,visibleWeb.point.y-.8f,1.6f,1.6f));Destroy(webOn);Destroy(webOff);visibleWeb.sprite.enabled=true;Check(webPixels>.003f,"full floor web renders visibly on stone rather than a clipped corner ("+webPixels.ToString("F4")+")");yield return Capture("v33-full-floor-web.png");
            web.age=9.8f;c.Tick(.21f);Check(!c.webs.Contains(web),"ground web lasts ten seconds and is then removed");
            foreach(var s in c.spiders){s.cooldown=99;s.Attacking=false;}c.snare=0;foreach(var w in c.webs)w.sprite.gameObject.SetActive(false);c.webs.Clear();
            var worm=c.worms.Single();Check(worm.parts.Count==10 && worm.Health==2000 && worm.parts.All(p=>p.health==200) && c.Targets.Count(t=>t is WormPart)==10,"boss begins with ten independently hittable segments summing to 2000 HP");
            var middle=worm.parts[4];Check(middle.Hurt(150)==20 && middle.health==180 && worm.Health==1980,"one ordinary hit is capped at twenty damage against its individual segment");middle.health=200;
            g.body.position=new Vector2(0,18);g.hero.transform.position=g.body.position;g.checkCameraPosition=new Vector3(0,23,-20);g.checkCameraSize=9;g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);
            var poses=new HashSet<Sprite>();for(int i=0;i<32;i++){c.Tick(.03f);poses.Add(worm.parts[0].sprite.sprite);}
            Check(poses.Count>=12 && worm.parts.All(p=>c.Walkable(p.Position,.30f)) && worm.parts.All(p=>p.sprite.sharedMaterial.shader.name=="AshenCoast/SpectralWorm"),"spectral worm uses many authored animation frames and follows navigable ground");
            yield return Capture("v33-spectral-worm-boss.png");
            for(int i=0;i<10;i++)middle.Hurt(100);
            Check(!middle.Alive && c.worms.Count==2 && c.worms.Sum(w=>w.parts.Count)==9 && c.worms.Sum(w=>w.Health)==1800 && c.worms.All(w=>w.parts[0].IsHead),"destroying a middle segment splits two independent complete worms without adding or losing surviving HP");
            var oldPositions=c.worms.Select(w=>w.Position).ToArray();for(int i=0;i<20;i++)c.Tick(.03f);Check(c.worms.Where((w,i)=>Vector2.Distance(w.Position,oldPositions[i])>.3f).Count()==2,"both split worms actively pursue the player with their own head and trail");yield return Capture("v33-worm-splits.png");
            var part=c.worms[0].parts[0];foreach(var p in c.worms.SelectMany(w=>w.parts))p.sprite.enabled=false;
            g.body.position=new Vector2(-5,23);g.hero.transform.position=g.body.position;g.facing=6;g.survivor.equipment[0]=new Item(ItemKind.LastPrism);g.survivor.mana=100;c.snare=0;g.combat.PoseHeldPrism();
            Check(g.prism.BeginChannel(Vector2.right),"Last Prism can begin channeling inside the catacombs");g.prism.Tick(1.81f);
            part.sprite.enabled=true;part.health=200;part.sprite.transform.position=g.prism.Origin+Vector2.right*4;g.prism.SetAim(Vector2.right);float hp=part.health;g.prism.Tick(.081f);
            Check(hp-part.health>20 && !g.prism.ApplyingDamage,"actual focused Last Prism damage bypasses the per-segment twenty-damage cap");g.prism.StopChannel();foreach(var p in c.worms.SelectMany(w=>w.parts))p.sprite.enabled=true;
            Check(TentacleField.PulseDamage==35,"tentacles now deal thirty-five damage on every pulse instead of nine");
            Check(!g.dragons.OpenTerrain(g.body.position,c.root.transform) && g.dragons.Schedule(g.body.position,c.root.transform)==null && !g.dragons.OpenTerrain(Vector2.zero,g.journey.tavernRoot.transform) && !g.dragons.OpenTerrain(Vector2.zero,g.magic.pocket.transform),"rooster invocation is denied in catacombs, tavern and pocket dimension");
            var routes=new[]{WorldArea.Coast,WorldArea.GoldenField,WorldArea.Tavern,WorldArea.Fort,WorldArea.Catacombs};bool sane=true;
            foreach(var area in routes){g.journey.SetArea(area,area==WorldArea.Catacombs?new Vector2(-8,-18.5f):area==WorldArea.Fort?new Vector2(0,-12.8f):new Vector2(0,-2));Physics2D.SyncTransforms();foreach(var collider in Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None))if(collider.enabled && collider.gameObject.activeInHierarchy)sane&=collider.bounds.size.x>0 && collider.bounds.size.y>0 && !float.IsNaN(collider.bounds.center.x);}
            Check(sane,"active colliders in every main location have valid nonzero bounds");
            Check(g.hero.GetComponent<CircleCollider2D>().radius==.22f && g.hull.points.Length==8 && g.journey.trees.All(t=>t.sprite.GetComponent<CircleCollider2D>().radius*t.sprite.transform.lossyScale.x<.31f),"hero feet, ship hull and poplar trunks preserve their matching ground footprints");
            Check(g.woodlice.All(m=>m.renderer.GetComponent<Collider2D>()!=null) && g.arcane.frogs.All(f=>f.renderer.GetComponent<Collider2D>()!=null) && c.spiders.All(s=>s.sprite.GetComponent<CircleCollider2D>().radius==.32f) && c.worms.SelectMany(w=>w.parts).All(p=>p.sprite.GetComponent<CapsuleCollider2D>().size==new Vector2(1.90f,.85f)),"woodlice, frogs, spiders and every worm segment have their intended body or feet colliders");
            Check(c.worms.SelectMany(w=>w.parts).All(p=>p.sprite.bounds.size.magnitude>2),"worm segment silhouettes are large and readable beside the player");
            c.Leave();Check(g.journey.IsCoast && !c.Snared && Vector2.Distance(g.body.position,c.Position)<2,"leaving catacombs restores the correct exterior position and clears web immobilization");
            g.journey.SetArea(WorldArea.GoldenField,new Vector2(15,5));g.seconds=225;g.survivor.health=100;g.survivor.equipment[0]=new Item(ItemKind.Lamp);g.hunter.Tick(0);
            var directions=new[]{Vector2.right,Vector2.left,Vector2.up,Vector2.down,new Vector2(1,1).normalized,new Vector2(-1,1).normalized,new Vector2(1,-1).normalized,new Vector2(-1,-1).normalized};bool retreat=true;var frames=new HashSet<Sprite>();
            for(int d=0;d<directions.Length;d++){g.hunter.Warp(g.body.position-directions[d]*4);float start=g.hunter.distance;for(int i=0;i<18;i++){g.hunter.Tick(.025f);frames.Add(g.hunter.sprite.sprite);}retreat&=g.hunter.Retreating && g.hunter.distance<start;}
            Check(retreat && frames.Count>=12,"axolotl retreats in all eight directions with a reversed gait cycle and changing painted body poses");
            g.checkCameraPosition=new Vector3(g.hunter.Position.x,g.hunter.Position.y+2,-20);g.checkCameraSize=7;g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);yield return Capture("v33-axolotl-retreat.png");
            g.survivor.equipment[0]=null;g.hunter.Warp(g.body.position+Vector2.up*10);var pursuit=new HashSet<Sprite>();for(int i=0;i<10;i++){g.hunter.Tick(.02f);pursuit.Add(g.hunter.sprite.sprite);}Check(pursuit.Count>=3 && !g.hunter.Retreating,"forward axolotl pursuit uses the same articulated movement instead of sliding");
            g.journey.SetArea(WorldArea.Coast,new Vector2(14,13));g.seconds=40;g.survivor.health=100;g.survivor.mana=100;g.dragons.Clear();
            Check(g.dragons.Schedule(new Vector2(17,15),g.coastRoot.transform)!=null,"rooster invocation remains available on open meadow ground");g.dragons.Tick(5.01f);
            var rooster=g.dragons.bosses.Last();rooster.Hurt(1500);g.dragons.Tick(1.8f);g.TickLoot(.7f);var meat=g.loot.Where(l=>l.item.kind==ItemKind.RawMeat).ToArray();
            Check(meat.Length==7 && meat.All(l=>Vector2.Distance(rooster.Renderer.bounds.ClosestPoint(l.position),l.position)<.70f) && meat.Select(l=>Mathf.RoundToInt(Vector2.Distance(l.position,rooster.Position)*10)).Distinct().Count()>3,"seven raw-meat drops have varied distances and scatter close to the defeated rooster silhouette");
            bool visibleMeat=true;foreach(var l in meat){var p=(Vector2)rooster.Renderer.transform.InverseTransformPoint(l.renderer.bounds.center);if(rooster.Renderer.flipX)p.x=-p.x;visibleMeat&=!g.art.Opaque(rooster.Renderer.sprite,p);}Check(visibleMeat,"all seven meat centres lie outside the resting corpse texture instead of being hidden under wings");
            g.checkCameraPosition=new Vector3(rooster.Position.x,rooster.Position.y+2,-20);g.checkCameraSize=5;g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);yield return Capture("v33-close-chaotic-meat-drops.png");g.dragons.Clear();
            g.checkCameraPosition=null;g.checkCameraSize=null;g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));g.seconds=40;g.UpdateEnvironment(0);
        }
    }
}
