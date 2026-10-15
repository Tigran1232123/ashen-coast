using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class InventoryUI
    {
        public VillageActorV46 shopAmazonV46,weaponAllyV46;
        public GridInventory VendorInventory=>shopAmazonV46!=null?shopAmazonV46.stock:game.journey.seller;
        bool VendorReachableV46=>shopAmazonV46!=null?shopAmazonV46.Alive && shopAmazonV46.Renderer.gameObject.activeInHierarchy && Vector2.Distance(game.body.position,shopAmazonV46.Position)<=3.5f:game.journey.Interior && Vector2.Distance(game.body.position,game.journey.keeper.position)<=3.5f;
        public bool AllyPanelOpenV46=>weaponAllyV46!=null && weaponAllyV46.Alive && weaponAllyV46.Hired && Vector2.Distance(game.body.position,weaponAllyV46.Position)<3.5f;
        public Rect AllyPanelV46=>new Rect(PanelX-230,120,216,292);
        public void OpenAmazonShopV46(VillageActorV46 hunter){OpenShop();shopAmazonV46=hunter;game.amazons.Offer=null;}
        public void OpenAllyWeaponV46(VillageActorV46 ally){CloseContainers();weaponAllyV46=ally;open=true;}
        public static Item PortionV46(Item original,int amount)=>new Item(original.kind){count=amount,durability=original.durability,fuel=original.fuel,freshness=original.freshness,shelfLife=original.shelfLife,water=original.water};
        bool ProcessStacksV46(Event e)
        {
            if(e.type!=EventType.MouseDown || e.button!=1 || !e.shift || e.control)return false;
            var view=grids.FirstOrDefault(g=>g.accessible && g.rect.Contains(Pointer));
            if(view==null)return false;
            var cell=view.Cell(Pointer);view=VisibleGrid(view,cell);
            if(view.inventory==VendorInventory && vendorOpen || dragMerchant){e.Use();return true;}
            var old=view.inventory.At(cell.x,cell.y);
            if(FirstLayerCovered(old)){e.Use();return true;}
            if(dragItem==null)
            {
                if(old!=null && (old.count>1 || view.inventory.StackLimit(old,old.x,old.y)>1))
                {
                    int amount=(old.count+1)/2;var part=PortionV46(old,amount);
                    if(holding.Add(part)){old.count-=amount;if(old.count==0)view.inventory.items.Remove(old);dragItem=part;dragSource=holding;origin=view.inventory;dragGear=-1;dragMerchant=false;grab=Vector2Int.zero;dragPress=Pointer;game.PlayPickup();}
                }
            }
            else
            {
                if(dragItem.Definition.Durable || dragItem.Definition.Book || dragItem.contents!=null){e.Use();return true;}
                var one=PortionV46(dragItem,1);var target=view.inventory;
                if(target==game.survivor.inventory && (old==null || old.kind!=one.kind || old.count>=target.StackLimit(one,old.x,old.y)))target=PlacementGrid(one,cell,cell);
                old=target.At(cell.x,cell.y);
                bool allowed=GridInventory.CanEnter(one,target) && (target.insertionAllowed==null || target.insertionAllowed());
                bool placed=false;
                if(allowed && old!=null && old.kind==one.kind && old.count<target.StackLimit(one,old.x,old.y)){old.count++;if(one.Definition.Food)old.freshness=Mathf.Min(old.freshness,one.freshness);placed=true;}
                else if(allowed && old==null)placed=target.Place(one,cell.x,cell.y);
                if(placed){dragItem.count--;if(dragItem.count==0){holding.items.Remove(dragItem);Cancel();}game.PlayPickup();}
            }
            game.RightClickConsumedFrame=Time.frameCount;e.Use();return true;
        }
        void DrawAmazonUIV46()
        {
            var village=game.amazons;if(village==null)return;
            if(AllyPanelOpenV46 && open)
            {
                var panel=AllyPanelV46;Window(panel,weaponAllyV46.Name);
                if(Button(new Rect(panel.xMax-33,panel.y+10,25,25),"×")){weaponAllyV46=null;return;}
                GUI.Label(new Rect(panel.x+16,panel.y+46,184,25),"Оружие",text);
                var grid=new GridView{inventory=weaponAllyV46.weaponSlot,cell=42,rect=new Rect(panel.x+66,panel.y+80,84,168)};grids.Add(grid);DrawGrid(grid);
            }
            if(village.TitleRemaining>0)
            {
                var old=GUI.color;GUI.color=new Color(1,1,1,Mathf.Min(1,village.TitleRemaining,.6f+(4.5f-village.TitleRemaining)));
                GUI.Label(new Rect(CanvasWidth*.2f,35,CanvasWidth*.6f,75),"Крылатый стан",CachedStyle(bold,TextAnchor.MiddleCenter,38,FontStyle.Bold));GUI.color=old;
            }
            var person=village.Offer;if(person==null)return;
            var box=new Rect(CanvasWidth/2-220,CanvasHeight/2-175,440,person.Slaver?350:220);Window(box,person.Name);
            if(Button(new Rect(box.xMax-38,box.y+10,26,26),"×")){village.Offer=null;return;}
            if(person.Slaver)
            {
                int row=0;foreach(var captive in village.people.Where(p=>p.Captive))
                {if(Button(new Rect(box.x+20,box.y+65+row++*59,400,43),captive.Name+" · 100 ХП · 1 монета")){if(!village.Hire(captive))Notice("Недостаточно монет");}}
            }
            else
            {
                GUI.Label(new Rect(box.x+22,box.y+54,396,26),"Оружие: "+person.Weapon.Definition.name,text);
                if(!person.WeaponMerchantV48 && Button(new Rect(box.x+22,box.y+95,396,43),"Нанять · 1 монета")){if(!village.Hire(person))Notice("Недостаточно монет");}
                if(Button(new Rect(box.x+22,box.y+149,396,43),"Торговля"))OpenAmazonShopV46(person);
            }
        }
        void DrawAmazonHealthV46()
        {
            if(game.amazons==null)return;
            DrawMageVitalsV54();
            foreach(var target in game.amazons.targets)DrawDummyDpsV48(target.Renderer);
            foreach(var a in game.amazons.people)if(a.Renderer.enabled && a.Renderer.gameObject.activeInHierarchy && (a.Alive || game.feedback.Recent(a.Renderer)) && Vector2.Distance(game.body.position,a.Position)<16)
                ActorVital(World(new Vector2(a.Position.x,a.Renderer.bounds.max.y+.15f)),"",a.health,a.MaxHealth,a.Hired?new Color(.26f,.48f,.28f):new Color(.60f,.26f,.20f),a.Renderer,a.Hired && !game.snails.Resting(a.Renderer));
            foreach(var b in game.amazons.plains.bandits)if(b.Renderer.enabled && b.Renderer.gameObject.activeInHierarchy && game.feedback.Recent(b.Renderer))ActorVital(World(new Vector2(b.Position.x,b.Renderer.bounds.max.y+.15f)),"",b.health,100,new Color(.6f,.26f,.2f),b.Renderer);
        }
    }
}
