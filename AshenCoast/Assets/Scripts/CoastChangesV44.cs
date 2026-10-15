using System;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed class CoastArtV44
    {
        public Sprite zenPlanted;
        public readonly Material zenMaterial;
        public CoastArtV44(ArtBank bank)
        {
            var spear=bank.Load("SpearV45");bank.icons[52]=bank.Slice(spear,new RectInt(0,0,spear.width,spear.height),new Vector2(.5f,.025f),260);
            var zen=bank.Load("ZenV44");var bands=bank.Bands(zen,false,2);
            var held=bands[0];bank.icons[53]=bank.Slice(zen,new RectInt(held.x,0,held.y-held.x+1,zen.height),new Vector2(.38f,.31f),260);
            var ground=bands[1];zenPlanted=bank.Slice(zen,new RectInt(ground.x,0,ground.y-ground.x+1,zen.height),new Vector2(.1f,0),260);
            // The planted point is the far left blade tip. The buried lower edge is clipped at ground level.
            var rect=zenPlanted.rect;var data=zen.GetPixels32();int tipX=(int)rect.width,tipY=0;
            for(int y=0;y<rect.height*.25f;y++)for(int x=0;x<rect.width;x++){var p=data[((int)rect.y+y)*zen.width+(int)rect.x+x];if(p.a>100 && x<tipX){tipX=x;tipY=y;}}
            zenPlanted=Sprite.Create(zen,rect,new Vector2(tipX/rect.width,tipY/rect.height),260,0,SpriteMeshType.FullRect);
            var food=bank.Load("FoodV44");var foods=bank.Bands(food,false,2);
            for(int i=0;i<2;i++){var b=foods[i];bank.icons[i==0?0:51]=bank.Slice(food,new RectInt(b.x,0,b.y-b.x+1,food.height),Vector2.one*.5f,260);}
            var fire=bank.Load("FireBookV44");var books=bank.Bands(fire,false,2);for(int i=0;i<2;i++){var b=books[i];bank.icons[24+i]=bank.Slice(fire,new RectInt(b.x,0,b.y-b.x+1,fire.height),Vector2.one*.5f,260);}
            CoastArtV41.FireAspect=bank.icons[24].rect.width/bank.icons[24].rect.height;
            zenMaterial=new Material(Shader.Find("AshenCoast/ZenRopes"));
        }
    }
    public sealed partial class CoastGame
    {
        public CoastArtV44 v44Art;
        public CombatStatusV44 v44;
        public void ToggleDayV44()
        {
            if(!CoastTime.IsNight(DaySeconds)){StartNight();return;}
            seconds=(Mathf.Floor(DaySeconds/360)+1)*360+30-(meditation?.DayOffset??0);coop?.ClockChanged();UpdateEnvironment(0);hunter.Tick(0);
        }
        void RefreshSpoiledGroundV44(Item item)
        {
            foreach(var drop in loot.Where(d=>!d.collected && d.item==item)){drop.renderer.sprite=art.icons[51];SizeGroundItemV44(drop);art.PixelStyle(drop.renderer);}
        }
        public void SizeGroundItemV44(GroundLoot drop)
        {
            var item=drop.item;var r=drop.renderer;var kind=item.kind;
            if(kind==ItemKind.Spectrum)return;
            if(item.Definition.Book){CoastArtV41.SizeGroundBook(drop);return;}
            if(kind==ItemKind.Zen){r.sprite=v44Art.zenPlanted;r.sharedMaterial=v44Art.zenMaterial;r.transform.localScale=Vector3.one*(Combat.WeaponLengthV44(kind)/r.sprite.bounds.size.y);PoseGroundZenV44(drop);return;}
            if(kind==ItemKind.Lamp || kind==ItemKind.LastPrism){float width=kind==ItemKind.Lamp?.58f:.46f*ArtBank.PrismVisualScale;r.transform.localScale=Vector3.one*(width/r.sprite.bounds.size.x);return;}
            float size=item.Definition.Melee?Combat.WeaponLengthV44(kind):NaturalGroundSizeV44(item);
            if(size>0)r.transform.localScale=Vector3.one*(size/Mathf.Max(r.sprite.bounds.size.x,r.sprite.bounds.size.y));
        }
        public static float NaturalGroundSizeV44(Item item)
        {
            if(item.Definition.Flower)return .78f;
            if(item.Definition.Food)return item.Definition.width==1?.48f:item.kind==ItemKind.MeatAssortment?.78f:.72f;
            switch(item.kind)
            {
                case ItemKind.Rot:return .48f;
                case ItemKind.WaterBottle:return .76f;
                case ItemKind.StoragePot:return 1.23f;
                case ItemKind.Cauldron:return 1.17f;
                case ItemKind.Wood:return .94f;
                case ItemKind.YoYo:return .492f;
                case ItemKind.ShacklesScroll:case ItemKind.CurseScroll:return .69f;
                case ItemKind.Clover:case ItemKind.FourLeafClover:return .33f;
                case ItemKind.Ring:return .24f;
                case ItemKind.Amulet:return .26f;
                case ItemKind.Boots:case ItemKind.RollerSkates:return .66f;
                case ItemKind.Helmet:return .72f;
                case ItemKind.Armor:return .95f;
                case ItemKind.Clay:return .38f;
                case ItemKind.Rope:return .75f;
                case ItemKind.IronIngot:return .85f;
                case ItemKind.CraftingTable:return 1.8f;
                case ItemKind.Anvil:return 1.05f;
                case ItemKind.Campfire:return 1.1f;
                default:return MissingGroundSizeV50(item);
            }
        }
        public void PoseGroundZenV44(GroundLoot drop)
        {
            if(drop.item.kind!=ItemKind.Zen || drop.collected)return;
            if(!drop.InFlight){drop.renderer.transform.position=drop.position-Vector2.up*ZenBuryDepthV45;drop.renderer.transform.rotation=Quaternion.Euler(0,0,ZenGroundAngleV45);drop.renderer.sortingOrder=Depth(drop.position.y)+2;}
            var p=new MaterialPropertyBlock();drop.renderer.GetPropertyBlock(p);var r=drop.renderer.sprite.rect;var t=drop.renderer.sprite.texture;p.SetVector("_SpriteRect",new Vector4(r.x/t.width,r.y/t.height,r.width/t.width,r.height/t.height));p.SetFloat("_RopeClock",seconds);p.SetFloat("_GroundRopes",drop.InFlight?0:1);p.SetFloat("_GroundY",drop.position.y);drop.renderer.SetPropertyBlock(p);
        }
    }
    public sealed partial class InventoryUI
    {
        public static string GeneralErrorV44(string message)
        {
            if(string.IsNullOrEmpty(message))return null;
            if(message.StartsWith("Недостаточно маны"))return "Недостаточно маны";
            if(message.StartsWith("Недостаточно монет"))return "Недостаточно монет";
            if(message.StartsWith("Нет места"))return "Нет места";
            if(message.Contains("откат") || message.StartsWith("Щупальца:"))return "Заклинание восстанавливается";
            if(message.Contains("нет цели"))return "Нет цели";
            if(message=="Сначала уберите предмет сверху")return "Предмет закрыт";
            if(message=="Выделите сырое мясо")return "Неверные ингредиенты";
            if(message.StartsWith("Для медитации") || message.StartsWith("Недостаточно здоровья"))return "Недостаточно здоровья";
            if(message=="Книга открывается в инвентаре" || message=="Откройте гримуар в инвентаре")return "Книга недоступна";
            if(message.StartsWith("Нужен свиток"))return "Нет свободной ячейки";
            if(message.StartsWith("Для призыва") || message=="Недоступная точка карты")return "Недоступное место";
            if(message=="Петух-дракон уже призван")return "Уже призван";
            if(message=="Рецепт не найден" || message=="Бутылка пуста" || message=="Посуда наполнена" || message=="Нет воды")return message;
            return null;
        }
        GridInventory PlayerSpaceV44(Item item)=>game.survivor.inventory.FindSpace(item,out _)?game.survivor.inventory:game.survivor.SpatialLayerOpen && game.survivor.spatialInventory.FindSpace(item,out _)?game.survivor.spatialInventory:null;
        bool ProcessThrowV44(Event e)
        {
            if(e.type!=EventType.MouseDown || e.button!=1 || !e.control)return false;
            Item item=dragItem;GridInventory source=holding;int gear=-1;
            if(item!=null && dragMerchant){e.Use();return true;}
            if(item==null)
            {
                var view=grids.FirstOrDefault(g=>g.accessible && g.rect.Contains(Pointer));
                if(view!=null){var cell=view.Cell(Pointer);view=VisibleGrid(view,cell);source=view.inventory;item=source.At(cell.x,cell.y);if(source==game.journey.seller || FirstLayerCovered(item)){e.Use();return true;}}
                if(item==null && open)for(int i=0;i<gearRects.Length;i++)if(gearRects[i].Contains(Pointer)){item=game.survivor.equipment[i];gear=i;source=null;break;}
            }
            if(item==null)return false;
            var at=game.body.position;
            GroundLoot drop=null;for(int i=0;i<16 && drop==null;i++){float angle=i*2.399f;drop=game.ThrowItem(item,at+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(1.25f+i*.065f));}
            if(drop!=null){source?.items.Remove(item);if(gear>=0){game.survivor.equipment[gear]=null;game.combat.ResetForCheck();game.prism.StopChannel();}if(item==dragItem)Cancel();game.PlayPickup();}else Notice("Недоступное место");
            game.RightClickConsumedFrame=Time.frameCount;e.Use();return true;
        }
    }
}
