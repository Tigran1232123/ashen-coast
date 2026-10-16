using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class InventoryUI
    {
        bool bookFrame,selectionMoved;Vector2 selectionPress;
        Texture2D spectrumEdge;
        public bool FirstLayerCovered(Item item)=>item!=null && game.survivor.inventory.items.Contains(item) && game.survivor.spatialInventory.items.Any(top=>ItemCells(top).Overlaps(ItemCells(item)));
        static RectInt ItemCells(Item item)=>new RectInt(item.x,item.y,item.Definition.width,item.Definition.height);
        GridView VisibleGrid(GridView view,Vector2Int cell)
        {
            if(view.inventory==game.survivor.spatialInventory && view.inventory.At(cell.x,cell.y)==null)return grids.First(g=>g.inventory==game.survivor.inventory);
            return view;
        }
        GridInventory PlacementGrid(Item item,Vector2Int cell,Vector2Int clicked)
        {
            var low=game.survivor.inventory;var high=game.survivor.spatialInventory;
            var box=new RectInt(cell.x,cell.y,item.Definition.width,item.Definition.height);
            if(high.items.Any(i=>ItemCells(i).Overlaps(box)))return high;
            if(origin==high && !game.survivor.SpatialLayerOpen && low.items.Any(i=>ItemCells(i).Overlaps(box)))return high;
            if(game.survivor.SpatialLayerOpen && low.items.Any(i=>ItemCells(i).Overlaps(box)))return high;
            return low;
        }
        void StartGridSelection(GridView view,Vector2Int cell,bool books=false)
        {
            craftGrid=view;craftStart=craftEnd=cell;bookFrame=books;hammer=!books;
            selectionPress=Pointer;selectionMoved=false;bookSelected=null;
            game.RightClickConsumedFrame=Time.frameCount;
        }
        void UpdateSelectionEnd()
        {
            var cell=craftGrid.Cell(Pointer);
            int cap=(stationOpen!=null && stationOpen.Heated && craftGrid.inventory==stationOpen.inventory?4:Mathf.Max(craftGrid.inventory.columns,craftGrid.inventory.rows))-1;
            craftEnd=new Vector2Int(Mathf.Clamp(cell.x,bookFrame?0:Mathf.Max(0,craftStart.x-cap),bookFrame?craftGrid.inventory.columns-1:Mathf.Min(craftGrid.inventory.columns-1,craftStart.x+cap)),Mathf.Clamp(cell.y,bookFrame?0:Mathf.Max(0,craftStart.y-cap),bookFrame?craftGrid.inventory.rows-1:Mathf.Min(craftGrid.inventory.rows-1,craftStart.y+cap)));
            selectionMoved|=Vector2.Distance(Pointer,selectionPress)>6 && cell!=craftStart;
        }
        bool FinishBookSelection(Event e)
        {
            if(craftGrid==null || e.type!=EventType.MouseUp || e.button!=1)return false;
            UpdateSelectionEnd();
            if(!selectionMoved && bookFrame && craftGrid.rect.Contains(Pointer))
            {
                var clicked=craftGrid.inventory.At(craftStart.x,craftStart.y);
                if(clicked!=null && clicked.Definition.Book && !FirstLayerCovered(clicked))
                {if(craftGrid.inventory.allowOpenBooks){if(!craftGrid.inventory.ToggleTome(clicked))Notice("Нет места для открытой книги");}else Notice("Книга открывается в инвентаре");}
                craftGrid=null;hammer=false;ClearBookPress();game.RightClickConsumedFrame=Time.frameCount;e.Use();return true;
            }
            var region=Selection();
            var book=selectionMoved && craftGrid.rect.Contains(Pointer)?craftGrid.inventory.items.Where(i=>i.Definition.Book && !FirstLayerCovered(i) && ContainsCells(region,ItemCells(i))).OrderBy(i=>i.y).ThenBy(i=>i.x).FirstOrDefault():null;
            if(book!=null){bookSelected=book;bookSource=craftGrid.inventory;}
            bool touchesBook=craftGrid.inventory.items.Any(i=>i.Definition.Book && ItemCells(i).Overlaps(region));
            if(book==null && (!bookFrame || selectionMoved && !touchesBook && craftGrid.inventory.craftingEnabled))return false;
            craftGrid=null;hammer=false;ClearBookPress();game.RightClickConsumedFrame=Time.frameCount;e.Use();return true;
        }
        static bool ContainsCells(RectInt outer,RectInt inner)=>outer.xMin<=inner.xMin && outer.yMin<=inner.yMin && outer.xMax>=inner.xMax && outer.yMax>=inner.yMax;
        void ClearBookPress(){bookFrame=selectionMoved=abyssDragPendingV43=false;}
        void DrawSpectrumEdge(Rect box)
        {
            if(!spectrumEdge)
            {
                spectrumEdge=new Texture2D(64,64,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};var data=new Color[4096];
                for(int y=0;y<64;y++)for(int x=0;x<64;x++){float t=Mathf.Max(Mathf.Abs((x+.5f)/32-1),Mathf.Abs((y+.5f)/32-1));data[y*64+x]=new Color(.71f,.64f,.39f,SpectrumEdgeAlphaV43(t));}
                spectrumEdge.SetPixels(data);spectrumEdge.Apply();
            }
            GUI.color=Color.white;GUI.DrawTexture(box,spectrumEdge);
        }
        internal void DrawGlyph(SpellGlyph glyph,Rect r,Color color)
        {
            var path=SpellGlyphs.Path(glyph);if(path.Length==0)return;
            float size=Mathf.Min(r.width,r.height)*.69f;var origin=r.center-Vector2.one*size*.5f;
            for(int i=1;i<path.Length;i++)DrawGlyphLine(origin+path[i-1]*size,origin+path[i]*size,color,2.25f);
        }
        void DrawGlyphLine(Vector2 from,Vector2 to,Color color,float thickness)
        {
            var saved=GUI.matrix;var delta=to-from;ArtBank.RotateUI(Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg,from);
            Fill(new Rect(from.x,from.y-thickness*.5f,delta.magnitude,thickness),color);GUI.matrix=saved;
        }
    }
}
