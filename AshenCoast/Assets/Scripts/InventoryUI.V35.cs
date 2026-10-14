using UnityEngine;
namespace AshenCoast
{
    public sealed partial class InventoryUI
    {
        void DrawSkatesPanel()
        {
            var panel=SkatesPanel;Frame(panel,dark);GUI.Label(new Rect(panel.x+10,panel.y+12,90,24),"Ролики",CachedStyle(text,TextAnchor.MiddleLeft,16));if(Button(new Rect(panel.xMax-28,panel.y+10,22,24),"×")){skatesOpen=false;return;}
            var view=new GridView{inventory=game.survivor.skateSlot,cell=42,rect=new Rect(panel.x+26,panel.y+50,84,84)};grids.Add(view);DrawGrid(view);
            
        }
        public bool OrbNeedsCloth(Item item,GridInventory grid)=>item.kind==ItemKind.Orb && grid!=game.survivor.inventory && grid.containerKind!=ItemKind.Sack && grid!=holding;
        void DrawOrbCloth(Rect orb)
        {ArtBank.DrawIcon(game.art.orbCloth,new Rect(orb.center.x-orb.width*.68f,orb.yMax-orb.height*.29f,orb.width*1.36f,orb.height*.56f),Color.white);}
        void DrawItemIcon(Item item,Rect area,Color color)
        {
            if(item.kind!=ItemKind.RollerSkates){ArtBank.DrawIcon(game.art.icons[item.Definition.icon],area,color,item.Definition.Melee && item.kind!=ItemKind.Zen && item.kind!=ItemKind.GoldenSpear && item.kind!=ItemKind.Axe && item.kind!=ItemKind.Stake,CoastArtV41.DisplayAspect(item));return;}
            float w=area.width*.82f,h=area.height*.60f;ArtBank.DrawIcon(game.art.skateAttachments[0],new Rect(area.x,area.y+area.height*.04f,w,h),color);ArtBank.DrawIcon(game.art.skateAttachments[1],new Rect(area.xMax-w,area.yMax-h,w,h),color);
        }
        void DrawFrying(Item item,Rect box)
        {
            var pot=stationOpen;if(pot.fryFlash<=0 || !pot.fried.Contains(item))return;float age=.7f-pot.fryFlash;
            Border(box,new Color(.92f,.59f,.16f,pot.fryFlash/.7f));
            for(int i=0;i<8;i++){float seed=i*2.399f;var p=box.center+new Vector2(Mathf.Sin(seed)*box.width*.32f,Mathf.Cos(seed)*box.height*.25f-age*(18+i));Fill(new Rect(Mathf.Round(p.x),Mathf.Round(p.y),2,2),new Color(1,.58f,.13f,pot.fryFlash/.7f));}
        }
    }
}
