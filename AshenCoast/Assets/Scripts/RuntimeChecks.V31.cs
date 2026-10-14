using System.Collections;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        IEnumerator BuildingPerspectiveChecks()
        {
            var g=game;var b=g.buildings;var ui=g.ui;g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));g.seconds=40;g.survivor.health=100;g.survivor.mana=100;
            g.checkCameraPosition=new Vector3(16,16,-20);g.checkCameraSize=4;g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);ui.CloseContainers();
            g.survivor.inventory.items.Clear();var table=new Item(ItemKind.CraftingTable);g.survivor.inventory.Add(table);b.Begin(table,g.survivor.inventory);b.RefreshPreview(new Vector2(17.1f,15.1f));
            Check(b.ghost.sprite==g.art.tableUpright && b.ghost.transform.eulerAngles.z==0 && b.ghost.bounds.size.y>b.ghost.bounds.size.x,"vertical table is drawn in perspective instead of rotating its legs sideways");
            yield return Capture("v31-upright-table-placement.png");
            b.HandleWheel(1);Check(b.ghost.sprite==g.art.icons[32] && b.ghost.transform.eulerAngles.z==0 && !b.vertical,"wheel changes the table drawing and footprint without tilting furniture");
            Check(b.Place(new Vector2(17.1f,15.1f)) && b.stations.Last().size==new Vector2Int(2,1),"horizontal table installs using the selected two world cells");
            var upright=b.CreateStation(ItemKind.CraftingTable,new Vector2Int(13,16),true);
            var anvilItem=new Item(ItemKind.Anvil);g.survivor.inventory.Add(anvilItem);b.Begin(anvilItem,g.survivor.inventory);b.RefreshPreview(new Vector2(15.1f,18.1f));
            Check(b.ghost.bounds.size.x<1 && BuildingSystem.Footprint(ItemKind.Anvil,b.vertical)==Vector2Int.one,"anvil preview and selected area fit one world cell");
            yield return Capture("v31-one-cell-anvil-preview.png");
            Check(b.Place(new Vector2(15.1f,18.1f)) && b.stations.Last().size==Vector2Int.one && b.stations.Last().Position==new Vector2(15.5f,18.5f),"anvil commits exactly one cell with the correct centre");
            Check(b.stations.Last().collider.size==new Vector2(.80f,.64f) && b.stations.Last().renderer.sprite.texture.name=="AnvilV31","one-cell anvil uses the new compact silhouette and matching collider");
            var near=g.flowers.First(f=>!f.picked && Vector2.Distance(f.position,new Vector2(16,13))<1.6f);var fire=g.arcane.CreateCampfire(near.position);
            Check(near.picked && !near.stems.enabled && !near.shadow.enabled && b.IsCleared(near.position,g.coastRoot.transform),"placing campfire actually removes the overlapping flower and its shadow");
            Check(g.flowers.Where(f=>b.IsCleared(f.position,g.coastRoot.transform)).All(f=>f.picked || !f.stems.enabled),"all placed structures clear flowers from their footprints");
            Check(b.clearings.Count==4 && b.clearings.All(c=>c.ground.sharedMaterial.shader.name=="AshenCoast/BuildingGround"),"all four outdoor structures render trampled-earth patches instead of grass underneath");
            Check(fire.logs.sprite==g.art.campfireBase && fire.flames.All(f=>f.transform.position.y>fire.position.y+.2f),"crossed logs and stone ring remain separate beneath the animated flames");
            yield return Capture("v31-furniture-cleared-ground.png");
            var patch=b.clearings[0];g.cameraView.Render();var visible=ReadPixels();foreach(var c in b.clearings)c.ground.enabled=false;g.cameraView.Render();var hidden=ReadPixels();
            float diff=RegionDifference(visible,hidden,new Rect(17,15,2,1));Destroy(visible);Destroy(hidden);foreach(var c in b.clearings)c.ground.enabled=true;
            Check(diff>.015f,"bare-earth patch renders visibly on the GPU ("+diff.ToString("F3")+")");
            g.checkCameraPosition=null;g.checkCameraSize=null;b.Cancel();ui.ClearCheckPointer();
        }
    }
}
