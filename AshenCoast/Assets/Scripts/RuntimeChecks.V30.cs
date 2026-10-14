using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        IEnumerator WheelDefeatChecks()
        {
            var g=game;var b=g.buildings;var ui=g.ui;var r=g.dragons;
            g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));g.seconds=40;g.survivor.health=100;
            g.checkCameraPosition=new Vector3(16,16,-20);g.checkCameraSize=6;g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);
            var point=new Vector2(17.2f,15.1f);g.survivor.inventory.items.Clear();var table=new Item(ItemKind.CraftingTable);g.survivor.inventory.Add(table);
            b.Begin(table,g.survivor.inventory);b.RefreshPreview(point);int rotations=b.WheelRotations;
            Check(b.HandleWheel(1) && !b.vertical,"native wheel rotates table in RMB placement mode");
            b.HandleWheel(1);Check(!b.vertical && b.WheelRotations==rotations+1,"Input and GUI wheel delivery do not rotate the same table twice in a frame");
            yield return Capture("v30-table-wheel-horizontal.png");yield return null;
            Check(b.HandleWheel(-1) && b.vertical,"next reverse wheel step returns table to vertical orientation");b.Cancel();
            ui.open=true;yield return null;yield return new WaitForEndOfFrame();yield return Pointer(Cell(g.survivor.inventory,0,0),EventType.MouseDown,0);
            var viewport=g.cameraView.WorldToViewportPoint(point);var screen=new Vector2(viewport.x*ui.CanvasWidth,(1-viewport.y)*ui.CanvasHeight);
            yield return Pointer(screen,EventType.MouseMove,0);b.RefreshPreview(point);
            Check(ui.DraggedItem==table && b.HandleWheel(1) && !b.vertical,"wheel also rotates the table carried by actual LMB inventory interaction");
            yield return Capture("v30-table-wheel-lmb-grid.png");yield return Pointer(screen,EventType.MouseDown,0);
            Check(!ui.Dragging && b.stations.Last().size==new Vector2Int(2,1),"placing carried table retains the wheel-selected horizontal footprint");ui.CloseContainers();b.Cancel();
            var anvil=b.CreateStation(ItemKind.Anvil,new Vector2Int(15,18),true);
            Check(anvil.renderer.sprite.texture.name=="AnvilV30" && Mathf.Abs(anvil.renderer.bounds.size.x-1.15f)<.02f && anvil.collider.size==new Vector2(.98f,.72f),"smaller metal-only anvil and collider are used in the world");
            yield return Capture("v30-anvil-without-stump.png");
            r.Clear();g.body.position=new Vector2(13,13);g.hero.transform.position=g.body.position;
            var boss=r.Spawn(new Vector2(18,15),g.coastRoot.transform);boss.Tick(.4f);int shakes=g.CameraShakes;boss.Hurt(2000);r.Tick(1.12f);
            Check(boss.DeathLanded && boss.DroppedMeat==7 && g.CameraShakes==shakes+1,"new defeat keeps the landing shake and seven meat drops");g.numbers.Clear();
            var frames=new HashSet<Sprite>();
            for(int i=0;i<8;i++){frames.Add(boss.Renderer.sprite);if(i==0 || i==3 || i==7)yield return Capture("v30-corpse-legs-"+i+".png");r.Tick(.1f);}
            Check(frames.Count>=7 && frames.All(s=>s.texture.name=="RoosterDefeatV30"),"distinct authored defeat frames lift the claws gradually");
            Check(Mathf.Abs(Mathf.DeltaAngle(boss.Renderer.transform.eulerAngles.z,0))<1 && boss.Renderer.GetComponentsInChildren<LineRenderer>().Length==0,"grounded wings and detailed tongue are drawn in the corpse sprite");
            g.TickLoot(.6f);yield return Capture("v30-corpse-resting.png");r.Tick(8.1f);
            Check(!boss.Finished && boss.Renderer.color.a>.95f,"new corpse remains visible through the ten-second rest");r.Tick(3);
            Check(!r.bosses.Contains(boss),"corpse still evaporates and is fully removed");
            g.checkCameraPosition=null;g.checkCameraSize=null;ui.ClearCheckPointer();
        }
    }
}
