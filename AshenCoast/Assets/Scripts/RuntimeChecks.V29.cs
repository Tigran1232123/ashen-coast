using System.Collections;
using UnityEngine;

namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        IEnumerator PlacementGridChecks()
        {
            var g=game;var b=g.buildings;var ui=g.ui;g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));g.seconds=40;g.survivor.health=100;
            g.checkCameraPosition=new Vector3(16,16,-20);g.checkCameraSize=6;g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);
            var point=new Vector2(17.2f,15.1f);
            foreach(var kind in new[]{ItemKind.Campfire,ItemKind.CraftingTable,ItemKind.Anvil})
            {
                b.Cancel();ui.CloseContainers();g.survivor.inventory.items.Clear();var item=new Item(kind);g.survivor.inventory.Add(item);ui.open=true;
                yield return null;yield return new WaitForEndOfFrame();yield return Pointer(Cell(g.survivor.inventory,0,0),EventType.MouseDown,0);
                Check(ui.DraggedItem==item && !b.ChoosingPlacement,"inventory LMB pick preserves grid hiding over the inventory: "+kind);
                var viewport=g.cameraView.WorldToViewportPoint(point);var screen=new Vector2(viewport.x*ui.CanvasWidth,(1-viewport.y)*ui.CanvasHeight);
                yield return Pointer(screen,EventType.MouseMove,0);b.RefreshPreview(point);
                Check(b.ChoosingPlacement && !b.Placing && b.ghost && b.ghost.enabled,"world placement grid and ghost appear for an LMB-carried item: "+kind);
                var bounds=b.ghost.bounds;g.cameraView.Render();var visible=ReadPixels();b.ghost.enabled=false;g.cameraView.Render();var hidden=ReadPixels();
                float pixels=RegionDifference(visible,hidden,new Rect(bounds.min.x,bounds.min.y,bounds.size.x,bounds.size.y));Destroy(visible);Destroy(hidden);b.ghost.enabled=true;
                Check(pixels>.008f,"translucent construction preview actually renders on the ground: "+kind+" ("+pixels.ToString("F3")+")");
                if(kind==ItemKind.CraftingTable){b.Rotate();Check(!b.vertical && BuildingSystem.Footprint(kind,b.vertical)==new Vector2Int(2,1),"table can rotate while carried on LMB before committing placement");}
                yield return Capture("v29-visible-grid-carried-"+kind+".png");
                if(kind==ItemKind.CraftingTable)
                {
                    yield return Pointer(screen,EventType.MouseDown,0);
                    Check(!ui.Dragging && b.stations[b.stations.Count-1].size==new Vector2Int(2,1),"LMB ground click places the carried table using its selected orientation");
                }
                else {ui.CancelInteraction();b.Cancel();}
            }
            ui.CloseContainers();b.Cancel();g.survivor.inventory.items.Clear();var fire=new Item(ItemKind.Campfire);g.survivor.inventory.Add(fire);
            Check(b.Begin(fire,g.survivor.inventory) && b.ChoosingPlacement,"RMB construction mode still displays the same grid");b.RefreshPreview(new Vector2(16.1f,18.1f));yield return Capture("v29-visible-grid-rmb-campfire.png");
            b.Cancel();Check(!b.ChoosingPlacement,"cancelling placement removes the construction grid");g.checkCameraPosition=null;g.checkCameraSize=null;ui.ClearCheckPointer();
        }
    }
}
