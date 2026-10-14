using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed partial class InventoryUI
    {
        public Item bookSelected;
        GridInventory bookSource;
        bool abyssSelecting;
        bool abyssDragPendingV43;Vector2 abyssSpellPressV43;
        public Rect BookPanel=>new Rect(PanelX-326,CanvasHeight-342,312,258);
        public Rect SpellBar=>new Rect(Mathf.Max(324,(CanvasWidth-864)*.5f),CanvasHeight-68,864,58);
        public void CloseStation()
        {
            var station=stationOpen;stationOpen=null;
            if(station==null || VesselWaterV43.Vessel(station.kind) || station.inventory.items.Count==0)return;
            int index=0;
            foreach(var item in station.inventory.items.ToArray())
            {
                float angle=index++*2.399963f;Vector2 destination=station.Position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(.9f+.11f*(index%3));
                for(int attempt=0;attempt<16;attempt++)
                {
                    destination=station.Position+new Vector2(Mathf.Cos(angle+attempt*.4f),Mathf.Sin(angle+attempt*.4f))*(.9f+attempt*.15f);
                    if(!Physics2D.OverlapCircleAll(destination,.13f).Any(c=>!c.isTrigger))break;
                }
                var drop=game.ScatterLoot(item,destination,station.Position+Vector2.up*.85f);
                drop.renderer.transform.SetParent(station.world,true);drop.shadow.transform.SetParent(station.world,true);
                if(drop.cloth)drop.cloth.transform.SetParent(station.world,true);
                station.inventory.items.Remove(item);
            }
        }
        void DrawSpatialLayer()
        {
            var survivor=game.survivor;var r=new Rect(MainX,MainGridY,420,210);
            if(survivor.SpatialLayerOpen || survivor.spatialInventory.items.Count>0)
            {
                var view=new GridView{inventory=survivor.spatialInventory,cell=42,rect=r};grids.Insert(0,view);
                if(survivor.SpatialLayerOpen)for(int y=0;y<5;y++)for(int x=0;x<10;x++)Fill(new Rect(r.x+x*42+1,r.y+y*42+1,40,40),new Color(.20f,.45f,.69f,.12f));
                foreach(var item in survivor.spatialInventory.items)DrawLayerItem(item,view,!survivor.SpatialLayerOpen);
            }
            if(survivor.abyssCell.HasValue)
            {var c=survivor.abyssCell.Value;DrawAbyssCell(new Rect(r.x+c.x*42+2,r.y+c.y*42+2,38,38),survivor.spatialInventory.At(c.x,c.y)==null?.60f:.22f);}
        }
        void DrawLayerItem(Item item,GridView view,bool dormant)
        {
            var d=item.Definition;var box=new Rect(view.rect.x+item.x*42+2,view.rect.y+item.y*42+2,d.width*42-4,d.height*42-4);
            Fill(box,new Color(.13f,.29f,.48f,dormant?.38f:.75f));if(item.Definition.Divine)DrawSpectrumEdge(box);Border(box,new Color(.32f,.56f,.81f));
            DrawItemIcon(item,ItemIconArea(item,box.center),Color.white);DrawCondition(item,box);
            if(item.count>1)GUI.Label(new Rect(box.x,box.yMax-23,box.width-4,23),item.count.ToString(),CachedStyle(small,TextAnchor.MiddleRight,0,FontStyle.Bold));
            if(box.Contains(Pointer))hoverItem=item;
        }
        void DrawAbyssCell(Rect area,float alpha)
        {
            DrawVortexV44(area,alpha);
        }
        void DrawBooks()
        {
            if(bookSelected!=null && bookSelected.spells!=null)
            {
                bool reachable=(bookSource?.items.Contains(bookSelected)??false) || holding.items.Contains(bookSelected) || game.survivor.Books.Contains(bookSelected);
                if(!reachable){bookSelected=null;return;}
                var panel=BookPanel;Frame(panel,dark);GUI.Label(new Rect(panel.x+14,panel.y+12,panel.width-50,30),bookSelected.Definition.name,CachedStyle(text,TextAnchor.MiddleLeft,15));
                if(Button(new Rect(panel.xMax-34,panel.y+12,24,24),"×")){bookSelected=null;return;}
                for(int i=0;i<bookSelected.spells.Length;i++)
                {
                    var slot=bookSelected.spells[i];var box=SpellCell(i);Frame(box,new Color(.08f,.11f,.15f));
                    if(slot.kind!=SpellKind.None)DrawGlyph(SpellGlyphs.For(bookSelected,slot),box,new Color(.84f,.88f,.94f));
                    if(slot.locked)DrawSpellLock(box);
                    if(slot.Remaining(game.seconds)>0)DrawSpellCooldown(box,slot.Remaining(game.seconds),slot.locked?60:bookSelected.kind==ItemKind.Tome?30:10);
                    if(box.Contains(Pointer))GUI.Label(new Rect(panel.x+14,panel.y+184,panel.width-28,28),bookSelected.kind==ItemKind.Tome && !slot.locked && slot.kind!=SpellKind.None?"Ромб · 10 маны · 30 с":bookSelected.kind==ItemKind.AbyssTome && !slot.locked && slot.kind!=SpellKind.None?"Туман · 10 маны · 10 с":BookSpells.Name(slot.kind),CachedStyle(small,TextAnchor.MiddleCenter,14));
                }
                if(Button(new Rect(panel.x+14,panel.y+223,panel.width-28,25),bookSelected.opened?"Закрыть книгу":"Открыть книгу",false,bookSource!=null && bookSource.allowOpenBooks))
                {if(!bookSource.ToggleTome(bookSelected))Notice("Нет места для открытой книги");}
            }
            var entries=game.survivor.Books.Where(b=>b.opened).SelectMany(b=>b.spells.Where(s=>s.kind!=SpellKind.None).Select(s=>new{book=b,slot=s})).ToArray();
            for(int i=0;i<entries.Length;i++)
            {
                var entry=entries[i];var box=new Rect(SpellBar.x+i*54,SpellBar.y,50,50);Frame(box,new Color(.065f,.08f,.11f,.88f));
                DrawGlyph(SpellGlyphs.For(entry.book,entry.slot),box,new Color(.84f,.88f,.94f));
                if(entry.slot.locked)DrawSpellLock(box);
                float remaining=entry.slot.Remaining(game.seconds);if(remaining>0)DrawSpellCooldown(box,remaining,entry.slot.locked?60:entry.book.kind==ItemKind.Tome?30:10);
                if(game.spellcraft.SelectedSlot==entry.slot)Border(box,new Color(.99f,.85f,.43f));
                if(box.Contains(Pointer) && Event.current.type==EventType.MouseDown && Event.current.button==0)
                {game.spellcraft.Select(entry.book,entry.slot);if(entry.slot.kind==SpellKind.Abyss){abyssSelecting=true;open=true;}Event.current.Use();}
            }
        }
        public Rect SpellCell(int index)=>new Rect(BookPanel.x+22+(index%3)*90,BookPanel.y+56+(index/3)*61,78,52);
        void DrawSpellLock(Rect box)
        {var r=new Rect(box.xMax-15,box.yMax-17,11,13);Border(new Rect(r.x+2,r.y,r.width-4,7),new Color(.96f,.77f,.35f));Fill(new Rect(r.x,r.y+5,r.width,8),new Color(.76f,.56f,.22f));Fill(new Rect(r.x+5,r.y+8,2,3),dark);}
        void DrawSpellCooldown(Rect r,float remaining,float duration)
        {Fill(new Rect(r.x,r.y,r.width,r.height*Mathf.Clamp01(remaining/duration)),new Color(.01f,.02f,.04f,.72f));GUI.Label(r,Mathf.CeilToInt(remaining).ToString(),CachedStyle(small,TextAnchor.MiddleCenter,15,FontStyle.Bold));}
        bool ProcessBookInteractions(Event e)
        {
            if(FinishBookSelection(e))return true;
            if(abyssDragPendingV43 && e.button==1 && e.type==EventType.MouseDrag && Vector2.Distance(Pointer,abyssSpellPressV43)>6)
            {abyssDragPendingV43=false;abyssSelecting=true;game.RightClickConsumedFrame=Time.frameCount;e.Use();return true;}
            if(abyssDragPendingV43 && e.button==1 && e.type==EventType.MouseUp)
            {abyssDragPendingV43=false;game.RightClickConsumedFrame=Time.frameCount;e.Use();return true;}
            if(abyssSelecting && e.type==EventType.MouseUp && e.button==1 || abyssSelecting && e.type==EventType.MouseDown && e.button==0)
            {
                var r=new Rect(MainX,MainGridY,420,210);if(r.Contains(Pointer))
                {var c=new Vector2Int(Mathf.FloorToInt((Pointer.x-r.x)/42),Mathf.FloorToInt((Pointer.y-r.y)/42));var old=game.survivor.inventory.At(c.x,c.y);{game.survivor.abyssCell=c;game.survivor.inventory.abyssCell=c;abyssSelecting=false;game.PlayPickup();}e.Use();return true;}
            }
            if(bookSelected==null || !BookPanel.Contains(Pointer))return false;
            for(int i=0;i<bookSelected.spells.Length;i++)if(SpellCell(i).Contains(Pointer))
            {
                if(e.type!=EventType.MouseDown){if(e.type==EventType.MouseUp || e.type==EventType.MouseDrag){game.RightClickConsumedFrame=Time.frameCount;e.Use();return true;}return false;}
                if(e.button==1){var selected=bookSelected.spells[i];game.spellcraft.Select(bookSelected,selected);if(selected.kind==SpellKind.Abyss && selected.locked && game.survivor.OpenBook(ItemKind.AbyssTome)==bookSelected){abyssDragPendingV43=true;abyssSpellPressV43=Pointer;}game.RightClickConsumedFrame=Time.frameCount;e.Use();return true;}
                var slot=bookSelected.spells[i];
                if(dragItem!=null && e.button==0)
                {if(BookSpells.Insert(bookSelected,i,dragItem,holding)){Cancel();game.PlayPickup();}else Notice("Нужен свиток и свободная ячейка");e.Use();return true;}
                if(slot.locked)
                {game.spellcraft.Select(bookSelected,slot);if(slot.kind==SpellKind.Abyss){if(game.survivor.OpenBook(ItemKind.AbyssTome)==bookSelected)abyssSelecting=true;else Notice("Откройте гримуар в инвентаре");}e.Use();return true;}
                if(e.button==0 && slot.scroll!=null)
                {var item=BookSpells.Remove(bookSelected,i);holding.Add(item);dragItem=item;dragSource=holding;origin=null;dragMerchant=false;grab=Vector2Int.zero;dragPress=Pointer;e.Use();return true;}
                if(e.button==1){game.spellcraft.Select(bookSelected,slot);game.RightClickConsumedFrame=Time.frameCount;e.Use();return true;}
            }
            return false;
        }
    }
}
