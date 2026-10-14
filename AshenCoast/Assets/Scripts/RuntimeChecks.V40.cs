using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        IEnumerator SelectBookFrameV40(Item book,GridInventory source)
        {
            yield return Pointer(Cell(source,book.x,book.y),EventType.MouseDown,1);
            var end=Cell(source,book.x+book.Definition.width-1,book.y+book.Definition.height-1);
            yield return Pointer(end,EventType.MouseDrag,1);yield return Pointer(end,EventType.MouseUp,1);
        }
        IEnumerator BookFramesV40()
        {
            var g=game;var s=g.survivor;var ui=g.ui;HandClearV34();s.inventory.items.Clear();s.spatialInventory.items.Clear();s.abyssCell=s.inventory.abyssCell=null;s.health=100;
            g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));g.seconds=50;s.equipment[0]=null;
            var book=new Item(ItemKind.AbyssTome);s.inventory.Place(book,2,1);ui.open=true;yield return DrawUIV34();
            var start=Cell(s.inventory,2,1);var end=Cell(s.inventory,3,2);
            yield return Pointer(start,EventType.MouseDown,1);Check(ui.bookSelected==null,"RMB down on book never opens its spell window");
            yield return Pointer(start,EventType.MouseUp,1);Check(ui.bookSelected==null,"short RMB click and release never opens spell slots");
            yield return Pointer(start,EventType.MouseDown,1);yield return Pointer(Cell(s.inventory,3,1),EventType.MouseDrag,1);yield return Pointer(Cell(s.inventory,3,1),EventType.MouseUp,1);
            Check(ui.bookSelected==null,"a frame covering only part of a book does not open its slots");
            yield return Pointer(start,EventType.MouseDown,1);yield return Pointer(end,EventType.MouseDrag,1);yield return DrawUIV34();
            Check(ui.bookSelected==null && ui.IsCrafting,"full book frame stays visible while RMB is held and popup stays closed");yield return Capture("v40-book-frame-before-release.png");
            yield return Pointer(end,EventType.MouseUp,1);Check(ui.bookSelected==book && !ui.IsCrafting && !book.opened,"releasing a complete frame opens slots without opening the book");yield return Capture("v40-book-symbols-after-release.png");
            yield return Pointer(start,EventType.MouseDown,1);Check(ui.bookSelected==null,"starting a new frame hides the old popup immediately");yield return Pointer(start,EventType.MouseUp,1);
            yield return Pointer(Cell(s.inventory,1,0),EventType.MouseDown,1);yield return Pointer(Cell(s.inventory,4,3),EventType.MouseDrag,1);yield return Pointer(Cell(s.inventory,4,3),EventType.MouseUp,1);
            Check(ui.bookSelected==book,"a frame starting on an empty cell can select the entire book");
            yield return Pointer(end,EventType.MouseDown,1);yield return Pointer(start,EventType.MouseDrag,1);yield return Pointer(start,EventType.MouseUp,1);
            Check(ui.bookSelected==book,"reverse-direction book selection opens only after release");
            yield return Pointer(start,EventType.MouseDown,1);ui.CloseContainers();ui.open=true;yield return Pointer(end,EventType.MouseUp,1);Check(ui.bookSelected==null,"closing inventory cancels a pending book selection");
            s.Bag.items.Clear();s.inventory.Transfer(book,s.Bag,1,1);ui.ToggleBag();yield return DrawUIV34();yield return SelectBookFrameV40(book,s.Bag);
            Check(ui.bookSelected==book && !book.opened,"closed books in a bag expose spell slots after frame selection");yield return Capture("v40-book-frame-in-storage.png");
            yield return Pointer(Cell(s.Bag,1,1),EventType.MouseDown,1);yield return Pointer(Cell(s.Bag,1,1),EventType.MouseUp,1);Check(ui.bookSelected==null,"short RMB clicks in other storage do not open slots either");
            ui.CloseContainers();s.Bag.Transfer(book,s.inventory,2,1);var tome=BookV39(ItemKind.Tome,5,0);var top=new Item(ItemKind.Clay);s.spatialInventory.Place(top,2,1);ui.open=true;yield return DrawUIV34();
            yield return Pointer(Cell(s.inventory,3,1),EventType.MouseDown,1);yield return Pointer(Cell(s.inventory,2,2),EventType.MouseDrag,1);yield return Pointer(Cell(s.inventory,2,2),EventType.MouseUp,1);
            Check(ui.bookSelected==null,"a covered first-layer book cannot be selected through its remaining visible cells");
            s.spatialInventory.items.Clear();yield return DrawUIV34();yield return SelectBookFrameV40(book,s.inventory);Check(ui.bookSelected==book,"removing the covering item makes book frame selection available again");
            var wood=new[]{new Item(ItemKind.Wood),new Item(ItemKind.Wood),new Item(ItemKind.Wood)};for(int i=0;i<3;i++)s.inventory.Place(wood[i],i,3);
            yield return Pointer(Cell(s.inventory,0,3),EventType.MouseDown,1);yield return Pointer(Cell(s.inventory,2,4),EventType.MouseDrag,1);yield return Pointer(Cell(s.inventory,2,4),EventType.MouseUp,1);
            Check(s.inventory.items.Any(i=>i.kind==ItemKind.CraftingTable) && wood.All(i=>!s.inventory.items.Contains(i)),"ordinary RMB frame crafting still consumes three logs and creates a table");
            ui.ClearCheckPointer();ui.CloseContainers();
        }
        IEnumerator GlyphLayoutV40()
        {
            var probe=gameObject.AddComponent<GlyphLayoutProbeV40>();probe.ui=game.ui;yield return null;yield return new WaitForEndOfFrame();
            var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(directory,"v40-glyph-layout-"+Screen.width+"x"+Screen.height+".png"),texture.EncodeToPNG());
            var data=texture.GetPixels32();var hits=new int[probe.boxes.Length];int outside=0;
            for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)
            {
                var p=data[y*texture.width+x];if(p.r<160 || p.g<160 || p.b<160)continue;
                var local=new Vector2((x+.5f)/game.ui.UIScale,(texture.height-y-.5f)/game.ui.UIScale);int index=System.Array.FindIndex(probe.boxes,r=>r.Contains(local));
                if(index<0)outside++;else hits[index]++;
            }
            Check(outside==0,"all rendered glyph pixels stay inside their own popup and HUD cells at "+Screen.width+"x"+Screen.height+" scale "+game.ui.UIScale.ToString("F2")+"; escaped="+outside);
            for(int i=0;i<hits.Length;i++)Check(hits[i]>15,"visible "+probe.glyphs[i%9]+" glyph in "+(i<9?"popup":"HUD")+" cell; pixels="+hits[i]);
            Check(probe.matrixRestored,"glyph rendering preserves the enclosing scaled GUI matrix");Destroy(texture);Destroy(probe);yield return null;
        }
    }
    internal sealed class GlyphLayoutProbeV40:MonoBehaviour
    {
        internal InventoryUI ui;internal Rect[] boxes;internal bool matrixRestored=true;
        internal readonly SpellGlyph[] glyphs={SpellGlyph.Star,SpellGlyph.Infinity,SpellGlyph.Triangle,SpellGlyph.Circle,SpellGlyph.Diamond,SpellGlyph.Tentacles,SpellGlyph.Fog,SpellGlyph.Abyss,SpellGlyph.Zigzag};
        void OnGUI()
        {
            if(ui==null || !ui.game.testMode)return;var saved=GUI.matrix;var color=GUI.color;GUI.depth=-1000;
            GUI.matrix=Matrix4x4.Scale(new Vector3(ui.UIScale,ui.UIScale,1));GUI.color=Color.black;GUI.DrawTexture(new Rect(0,0,ui.CanvasWidth,ui.CanvasHeight),Texture2D.whiteTexture);
            boxes=new Rect[18];for(int i=0;i<9;i++){boxes[i]=ui.SpellCell(i);boxes[i+9]=new Rect(ui.SpellBar.x+i*54,ui.SpellBar.y,50,50);}
            for(int i=0;i<boxes.Length;i++){var original=GUI.matrix;ui.DrawGlyph(glyphs[i%9],boxes[i],Color.white);matrixRestored&=GUI.matrix==original;}
            GUI.matrix=saved;GUI.color=color;
        }
    }
}
