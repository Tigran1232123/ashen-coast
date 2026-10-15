using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public enum ItemKind { Brain, Soup, BeetleMeat, Rope, Sword, Dagger, Scythe, Spear, Sack, Helmet, Armor, Amulet, Ring, BlueFlower, WhiteFlower, PinkFlower, Orb, Tome, OpenTome, Boots, YoYo, Lamp, Wood, Campfire, Pyromancy, OpenPyromancy, AbyssTome, OpenAbyssTome, Coin, Pie, LastPrism, RawMeat, CraftingTable, IronIngot, Anvil, Clay, StoragePot, RoastMeat, RoastBeetle, MeatAssortment, RollerSkates, ShacklesScroll, CurseScroll, Spectrum, Cauldron, WaterBottle, Buttercup, Poppy, Bellflower, Clover, FourLeafClover, Rot, GoldenSpear, Zen, Axe, Stake, Bow, ReturnScroll }
    public enum GearSlot { Hand, Head, Body, Back, Amulet, RingLeft, RingRight, Feet }

    [Serializable]
    public sealed class ItemDefinition
    {
        public ItemKind kind;
        public string name;
        public int width, height, icon;
        public float nourishment, foodDuration;
        public int mana, damage;
        public float reach, attackSeconds, cooldown, arc;
        public bool Food => foodDuration > 0;
        public bool Flower => kind == ItemKind.BlueFlower || kind == ItemKind.WhiteFlower || kind == ItemKind.PinkFlower || kind==ItemKind.Buttercup || kind==ItemKind.Poppy || kind==ItemKind.Bellflower;
        public bool Weapon => damage > 0;
        public bool Divine => kind==ItemKind.Spectrum || kind==ItemKind.Zen;
        public bool Melee => kind==ItemKind.Axe || kind==ItemKind.Stake || kind==ItemKind.Zen || kind==ItemKind.GoldenSpear || kind==ItemKind.Spectrum || kind==ItemKind.Sword || kind==ItemKind.Dagger || kind==ItemKind.Scythe || kind==ItemKind.Spear;
        public bool Durable=>Weapon || kind==ItemKind.Orb || kind==ItemKind.Helmet || kind==ItemKind.Armor || kind==ItemKind.Amulet || kind==ItemKind.Ring || kind==ItemKind.Boots;
        public bool Book => kind==ItemKind.Tome || kind==ItemKind.OpenTome || kind==ItemKind.Pyromancy || kind==ItemKind.OpenPyromancy || kind==ItemKind.AbyssTome || kind==ItemKind.OpenAbyssTome;
        public static readonly Dictionary<ItemKind, ItemDefinition> All = CreateDefinitions();

        static Dictionary<ItemKind, ItemDefinition> CreateDefinitions()
        {
            var d = new Dictionary<ItemKind, ItemDefinition>();
            Add(d, ItemKind.Brain, "Мозговик", 1, 1, 0, 8, 150, 100);
            Add(d, ItemKind.Soup, "Суп", 2, 1, 1, 35, 180);
            Add(d, ItemKind.BeetleMeat, "Жучье мясо", 1, 1, 2, 25, 110);
            Add(d, ItemKind.Rope, "Верёвка", 2, 1, 3);
            AddWeapon(d, ItemKind.Sword, "Меч", 1, 3, 4, 24, 2, .38f, .51f, 60);
            AddWeapon(d, ItemKind.Dagger, "Кинжал", 1, 2, 5, 20, 1, .19f, .26f, 60);
            AddWeapon(d, ItemKind.Scythe, "Коса", 2, 3, 6, 24, 2, .46f, .58f, 360);
            AddWeapon(d, ItemKind.Spear, "Копьё", 1, 4, 7, 24, 3, .22f, .29f, 0);
            Add(d, ItemKind.Sack, "Мешок", 2, 2, 8);
            Add(d, ItemKind.Helmet, "Шлем", 2, 2, 9);
            Add(d, ItemKind.Armor, "Броня", 2, 3, 10);
            Add(d, ItemKind.Amulet, "Амулет", 1, 1, 11);
            Add(d, ItemKind.Ring, "Кольцо", 1, 1, 12);
            Add(d, ItemKind.BlueFlower, "Василёк", 1, 2, 13);
            Add(d, ItemKind.WhiteFlower, "Ромашка", 1, 2, 14);
            Add(d, ItemKind.PinkFlower, "Полевой цветок", 1, 2, 15);
            Add(d, ItemKind.Orb, "Магическая сфера", 2, 2, 16);
            Add(d, ItemKind.Tome, "Пространственный фолиант", 2, 2, 17);
            Add(d, ItemKind.OpenTome, "Пространственный фолиант", 3, 2, 18);
            Add(d, ItemKind.Boots, "Кожаные сапоги", 2, 2, 19);
            AddWeapon(d,ItemKind.YoYo,"Кундали",1,1,20,18,5,.16f,.28f,0);
            AddWeapon(d,ItemKind.Lamp,"Огненная лампа",2,2,21,10,5,.08f,.12f,120);
            Add(d,ItemKind.Wood,"Древесина",1,2,22);
            Add(d,ItemKind.Campfire,"Костёр",2,2,23);
            Add(d,ItemKind.Pyromancy,"Искусство пиромантии",1,2,24);
            Add(d,ItemKind.OpenPyromancy,"Искусство пиромантии",2,2,25);
            Add(d,ItemKind.AbyssTome,"Гримуар бездны",2,2,26);
            Add(d,ItemKind.OpenAbyssTome,"Гримуар бездны",3,2,27);
            Add(d,ItemKind.Coin,"Серебряная монета",1,1,28);
            Add(d,ItemKind.Pie,"Пирог",2,1,29,45,220);
            AddWeapon(d,ItemKind.LastPrism,"Последняя призма",2,2,30,420,96,.10f,.10f,60);
            Add(d,ItemKind.RawMeat,"Сырое мясо",2,1,31,16,80);
            Add(d,ItemKind.CraftingTable,"Стол для ремесла",3,3,32);
            Add(d,ItemKind.IronIngot,"Железный слиток",2,1,33);
            Add(d,ItemKind.Anvil,"Наковальня",2,2,34);
            Add(d,ItemKind.Clay,"Глина",1,1,35);
            Add(d,ItemKind.StoragePot,"Глиняный горшок",4,4,36);
            Add(d,ItemKind.RoastMeat,"Жареное мясо",2,1,37,35,160);Add(d,ItemKind.RoastBeetle,"Жареное жучье мясо",1,1,38,38,170);Add(d,ItemKind.MeatAssortment,"Мясное ассорти",2,2,39,65,260);
            Add(d,ItemKind.RollerSkates,"Накладные ролики",2,2,40);
            Add(d,ItemKind.ShacklesScroll,"Свиток: Небесные оковы",1,2,41);
            Add(d,ItemKind.CurseScroll,"Свиток: Порча",1,2,42);
            AddWeapon(d,ItemKind.Spectrum,"Спектр",1,2,43,180,1.5f,.23f,.34f,70);
            Add(d,ItemKind.Cauldron,"Железный котелок",3,3,44);
            Add(d,ItemKind.WaterBottle,"Бутылка воды",1,2,45);
            Add(d,ItemKind.Buttercup,"Лютик",1,2,46);Add(d,ItemKind.Poppy,"Полевой мак",1,2,47);Add(d,ItemKind.Bellflower,"Колокольчик",1,2,48);
            Add(d,ItemKind.Clover,"Клевер",1,1,49);Add(d,ItemKind.FourLeafClover,"Четырёхлистный клевер",1,1,50);
            Add(d,ItemKind.Rot,"Гниль",1,1,51);
            AddWeapon(d,ItemKind.GoldenSpear,"Золотое жало",1,4,52,30,3,.22f,.29f,0);
            AddWeapon(d,ItemKind.Zen,"Дзен",2,4,53,90,2,.46f/3,.58f/3,360);
            AddWeapon(d,ItemKind.Axe,"Топор",1,3,54,30,2,.45f,1,120);
            AddWeapon(d,ItemKind.Stake,"Боевой кол",1,4,55,24,3,.22f,.29f,0);
            AddWeapon(d,ItemKind.Bow,"Лук",2,3,56,20,9,.30f,.8f,0);
            Add(d,ItemKind.ReturnScroll,"Свиток: Возвращение",1,2,57);
            return d;
        }
        static void Add(Dictionary<ItemKind, ItemDefinition> d, ItemKind k, string n, int w, int h, int icon, float food = 0, float duration = 0, int mana = 0)
        { d[k] = new ItemDefinition { kind = k, name = n, width = w, height = h, icon = icon, nourishment = food, foodDuration = duration, mana = mana }; }
        static void AddWeapon(Dictionary<ItemKind, ItemDefinition> d, ItemKind k, string n, int w, int h, int icon, int damage, float reach, float duration, float cooldown, float arc)
        { Add(d, k, n, w, h, icon); var v = d[k]; v.damage = damage; v.reach = reach; v.attackSeconds = duration; v.cooldown = cooldown; v.arc = arc; }
    }

    [Serializable]
    public sealed class Item
    {
        public string id = Guid.NewGuid().ToString("N");
        public ItemKind kind;
        public int x, y;
        [NonSerialized] public GridInventory contents;
        public bool opened;
        public SpellSlot[] spells;
        public float spellCooldown;
        public int count=1;
        public float durability=100,fuel=100;
        public float freshness=720,shelfLife=720,water=2;
        public bool Spoiled=>kind!=ItemKind.Brain && Definition.Food && freshness<=0;
        public void RefreshCookedFood(){freshness=shelfLife=360;}
        public bool Usable=>!Definition.Durable || durability>0;
        public void Wear(float amount){if(Definition.Durable)durability=Mathf.Max(0,durability-amount);}
        public ItemDefinition Definition => ItemDefinition.All[opened?(kind==ItemKind.Tome?ItemKind.OpenTome:kind==ItemKind.Pyromancy?ItemKind.OpenPyromancy:kind==ItemKind.AbyssTome?ItemKind.OpenAbyssTome:kind):kind];
        public Item(ItemKind kind) { this.kind=kind;spells=BookSpells.Create(kind);if(kind==ItemKind.RoastMeat || kind==ItemKind.RoastBeetle || kind==ItemKind.MeatAssortment)RefreshCookedFood();
            if(kind==ItemKind.Sack || VesselWaterV43.Vessel(kind)){int size=kind==ItemKind.Cauldron?4:5;contents=new GridInventory(size,size,kind==ItemKind.Sack?"Мешок":kind==ItemKind.Cauldron?"Котелок":"Горшок"){craftingEnabled=kind==ItemKind.Sack,containerKind=kind};} }
    }

    [Serializable]
    public sealed class GridInventory
    {
        public int columns, rows,waterRows;
        public string name;
        public List<Item> items = new List<Item>();
        public int craftLimit=3;
        public bool craftingEnabled=true;
        public ItemKind? onlyKind,containerKind;
        public bool allowOpenBooks,preserveOpenBooks;
        public Vector2Int? abyssCell;
        [NonSerialized] public Func<bool> insertionAllowed;
        public ItemDefinition PlacementDefinition(Item item)=>item.Definition.Book && !allowOpenBooks && !preserveOpenBooks?ItemDefinition.All[item.kind]:item.Definition;
        public int StackLimit(Item item,int x,int y)=>abyssCell.HasValue && abyssCell.Value==new Vector2Int(x,y) && item.Definition.width==1 && item.Definition.height==1 && !item.Definition.Durable?25:item.kind==ItemKind.Coin?10:1;
        public GridInventory(int columns, int rows, string name) { this.columns = columns; this.rows = rows; this.name = name; }
        public Item At(int x, int y) => items.FirstOrDefault(i => x >= i.x && y >= i.y && x < i.x + i.Definition.width && y < i.y + i.Definition.height);
        public System.Func<Item,bool> acceptsItem;
        public bool singleItem;
        public bool CanPlace(Item item, int x, int y, IEnumerable<Item> ignore = null)
        {
            if(acceptsItem!=null && !acceptsItem(item))return false;
            if(singleItem && items.Any(i=>i!=item && (ignore==null || !ignore.Contains(i))))return false;
            var a = PlacementDefinition(item);
            if(insertionAllowed!=null && !items.Contains(item) && !insertionAllowed())return false;
            if(onlyKind.HasValue && item.kind!=onlyKind.Value)return false;
            if (x < 0 || y < 0 || x + a.width > columns || y + a.height > rows) return false;
            foreach (var other in items)
            {
                if (other == item || (ignore != null && ignore.Contains(other))) continue;
                if (x < other.x + other.Definition.width && x + a.width > other.x && y < other.y + other.Definition.height && y + a.height > other.y) return false;
            }
            return true;
        }
        public bool FindSpace(Item item, out Vector2Int pos, IEnumerable<Item> ignore = null)
        {
            for (int y = 0; y < rows; y++) for (int x = 0; x < columns; x++)
                if (CanPlace(item, x, y, ignore)) { pos = new Vector2Int(x, y); return true; }
            pos = default; return false;
        }
        public bool Add(Item item)
        {
            if(insertionAllowed!=null && !insertionAllowed())return false;
            if(abyssCell.HasValue && StackLimit(item,abyssCell.Value.x,abyssCell.Value.y)==25){var old=At(abyssCell.Value.x,abyssCell.Value.y);if(old!=null && old.kind==item.kind && old.count+item.count<=25){old.count+=item.count;if(item.Definition.Food)old.freshness=Mathf.Min(old.freshness,item.freshness);return true;}if(old==null && item.count<=25)return Place(item,abyssCell.Value.x,abyssCell.Value.y);}
            if(item.kind==ItemKind.Coin)
            {
                if(items.Contains(item))return true;if(item.count<=0)return false;
                var stacks=items.Where(i=>i.kind==ItemKind.Coin && i!=item && i.count<10).ToList();int capacity=stacks.Sum(i=>10-i.count);
                var cells=new List<Vector2Int>();for(int y=0;y<rows;y++)for(int x=0;x<columns;x++)if(CanPlace(item,x,y))cells.Add(new Vector2Int(x,y));
                if(capacity+cells.Count*10<item.count)return false;
                foreach(var stack in stacks){int amount=Mathf.Min(10-stack.count,item.count);stack.count+=amount;item.count-=amount;if(item.count==0)return true;}
                int remaining=item.count,cell=0;item.count=Mathf.Min(10,remaining);Place(item,cells[cell].x,cells[cell].y);remaining-=item.count;cell++;
                while(remaining>0){int amount=Mathf.Min(10,remaining);Place(new Item(ItemKind.Coin){count=amount},cells[cell].x,cells[cell].y);remaining-=amount;cell++;}return true;
            }
            if (!FindSpace(item, out var p)) return false; return Place(item, p.x, p.y);
        }
        public bool Place(Item item, int x, int y) { if (!CanPlace(item, x, y)) return false; if(item.Definition.Book && !allowOpenBooks && !preserveOpenBooks)item.opened=false; if (!items.Contains(item)) items.Add(item); item.x = x; item.y = y; return true; }
        public bool Transfer(Item item, GridInventory target, int x, int y)
        {
            if (!items.Contains(item) || !CanEnter(item,target) || !target.CanPlace(item, x, y)) return false;
            items.Remove(item); return target.Place(item, x, y);
        }
        public static bool CanEnter(Item item,GridInventory target)
        {
            if(target.acceptsItem!=null && !target.acceptsItem(item))return false;
            if(target.onlyKind.HasValue && item.kind!=target.onlyKind.Value)return false;
            if(item.contents==null)return true;if(item.contents==target)return false;
            return item.contents.items.All(child=>CanEnter(child,target));
        }
        public bool Swap(Item incoming,GridInventory target,Item outgoing)
        {
            if(incoming==outgoing || !items.Contains(incoming) || !target.items.Contains(outgoing) || !CanEnter(incoming,target) || !CanEnter(outgoing,this))return false;
            int ax=incoming.x,ay=incoming.y,bx=outgoing.x,by=outgoing.y;var ignored=new[]{incoming,outgoing};
            if(!target.CanPlace(incoming,bx,by,ignored) || !CanPlace(outgoing,ax,ay,ignored))return false;
            if(target==this && new RectInt(bx,by,incoming.Definition.width,incoming.Definition.height).Overlaps(new RectInt(ax,ay,outgoing.Definition.width,outgoing.Definition.height)))return false;
            items.Remove(incoming);target.items.Remove(outgoing);
            target.Place(incoming,bx,by);Place(outgoing,ax,ay);return true;
        }
        public bool ToggleTome(Item item)
        {
            if(!allowOpenBooks || !item.Definition.Book || !items.Contains(item)) return false;
            item.opened=!item.opened;
            if(CanPlace(item,item.x,item.y)) return true;
            if(FindSpace(item,out var p)) {item.x=p.x;item.y=p.y;return true;}
            item.opened=!item.opened;return false;
        }
        public bool Craft(RectInt cells, out Item result,GridInventory fallback=null)
        {
            result = null;
            if(!TryRecipe(cells,out var output))return false;
            var selected = items.Where(i => cells.Overlaps(new RectInt(i.x, i.y, i.Definition.width, i.Definition.height))).ToList();
            // Selection identifies whole resources even when only their occupied first row is outlined.
            var rope = new Item(output);
            // Check output space before consuming ingredients. This makes crafting atomic.
            GridInventory target=this;
            if(!FindSpace(rope,out var position,selected)){if(fallback==null || fallback==this || !fallback.FindSpace(rope,out position))return false;target=fallback;}
            foreach(var i in selected)items.Remove(i);
            target.Place(rope,position.x,position.y);result=rope;return true;
        }
        public bool TryRecipe(RectInt cells,out ItemKind output)
        {
            output=(ItemKind)(-1);if(!craftingEnabled || cells.width<1 || cells.height<1 || cells.width>craftLimit || cells.height>craftLimit)return false;
            var selected=items.Where(i=>cells.Overlaps(new RectInt(i.x,i.y,i.Definition.width,i.Definition.height))).ToList();
            if(containerKind==ItemKind.Anvil){output=MatchesCauldron(selected)?ItemKind.Cauldron:(ItemKind)(-1);return (int)output>=0;}
            output=selected.Count==2 && selected.All(i=>i.Definition.Flower)?ItemKind.Rope:selected.Count==2 && selected.All(i=>i.kind==ItemKind.Wood)?ItemKind.Campfire:selected.Count==3 && selected.All(i=>i.kind==ItemKind.Wood)?ItemKind.CraftingTable:MatchesAnvil(selected,cells)?ItemKind.Anvil:MatchesPot(selected)?ItemKind.StoragePot:(ItemKind)(-1);
            return (int)output>=0;
        }
        bool MatchesAnvil(List<Item> selected,RectInt cells)
        {
            if(selected.Count!=5 || selected.Any(i=>i.kind!=ItemKind.IronIngot))return false;
            int x=selected.Min(i=>i.x),y=selected.Min(i=>i.y);
            var layout=new[]{new Vector2Int(0,0),new Vector2Int(2,0),new Vector2Int(1,1),new Vector2Int(0,2),new Vector2Int(2,2)};
            return layout.All(p=>selected.Any(i=>i.x==x+p.x && i.y==y+p.y));
        }
        bool MatchesCauldron(List<Item> selected)
        {
            if(selected.Count!=5 || selected.Any(i=>i.kind!=ItemKind.IronIngot))return false;
            int x=selected.Min(i=>i.x),y=selected.Min(i=>i.y);var layout=new[]{new Vector2Int(0,0),new Vector2Int(2,0),new Vector2Int(0,1),new Vector2Int(2,1),new Vector2Int(1,2)};
            return layout.All(p=>selected.Any(i=>i.x==x+p.x && i.y==y+p.y));
        }
        bool MatchesPot(List<Item> selected)
        {
            if(selected.Count!=7 || selected.Any(i=>i.kind!=ItemKind.Clay))return false;
            int x=selected.Min(i=>i.x),y=selected.Min(i=>i.y);
            var pattern=new[]{new Vector2Int(0,0),new Vector2Int(2,0),new Vector2Int(0,1),new Vector2Int(2,1),new Vector2Int(0,2),new Vector2Int(1,2),new Vector2Int(2,2)};
            return pattern.All(p=>selected.Any(i=>i.x==x+p.x && i.y==y+p.y));
        }
        public bool Valid()
        {
            if (items.Select(i => i.id).Distinct().Count() != items.Count) return false;
            return items.All(i => CanPlace(i, i.x, i.y));
        }
    }

    [Serializable]
    public sealed class FoodEffect
    {
        public ItemKind kind;
        public float remaining, duration;
    }

    [Serializable]
    public sealed partial class Survivor
    {
        public float health = 100, hunger = 100, immunity = 100, mana = 0;
        public List<FoodEffect> foods = new List<FoodEffect>();
        public readonly Item[] equipment = new Item[8];
        public GridInventory inventory = new GridInventory(10, 5, "Инвентарь"){allowOpenBooks=true};
        public float HungerMax => 100 + (foods.Count >= 2 ? 20 : 0) + (foods.Count >= 4 ? 30 : 0);
        public GridInventory Bag => equipment[(int)GearSlot.Back]?.contents;
        public Survivor() { equipment[(int)GearSlot.Back] = new Item(ItemKind.Sack); inventory.Add(new Item(ItemKind.Lamp));inventory.Add(new Item(ItemKind.Zen));inventory.Add(new Item(ItemKind.GoldenSpear));inventory.Add(new Item(ItemKind.Pyromancy));inventory.Add(new Item(ItemKind.AbyssTome));inventory.Add(new Item(ItemKind.YoYo));inventory.Add(new Item(ItemKind.Coin){count=10});inventory.Add(new Item(ItemKind.LastPrism));inventory.Add(new Item(ItemKind.RollerSkates)); inventory.Add(new Item(ItemKind.ShacklesScroll));inventory.Add(new Item(ItemKind.CurseScroll));inventory.Add(new Item(ItemKind.Spectrum));inventory.Add(new Item(ItemKind.WaterBottle));inventory.Add(new Item(ItemKind.ReturnScroll)); spatialInventory.insertionAllowed=()=>SpatialLayerOpen; }
        public int Coins=>inventory.items.Where(i=>i.kind==ItemKind.Coin).Sum(i=>i.count)+(Bag==null?0:Bag.items.Where(i=>i.kind==ItemKind.Coin).Sum(i=>i.count));
        public bool SpendCoins(int price)
        {
            if(price<0 || Coins<price)return false;if(price==0)return true;
            foreach(var grid in new[]{inventory,Bag})if(grid!=null)foreach(var coin in grid.items.Where(i=>i.kind==ItemKind.Coin).ToList())
            {int take=Mathf.Min(price,coin.count);coin.count-=take;price-=take;if(coin.count==0)grid.items.Remove(coin);if(price==0)return true;}
            return false;
        }
        public bool SpendMana(float amount){if(amount<0 || mana+.0001f<amount)return false;mana=Mathf.Max(0,mana-amount);return true;}
        public void WearBook(ItemKind kind,float amount)
        {var book=OpenBook(kind);book?.Wear(amount);}
        public bool Refuel(Item wood,GridInventory source,Item lamp)
        {if(wood?.kind!=ItemKind.Wood || source==null || !source.items.Contains(wood) || lamp?.kind!=ItemKind.Lamp || lamp.fuel>=100)return false;source.items.Remove(wood);lamp.fuel=Mathf.Min(100,lamp.fuel+50);return true;}
        public bool Purchase(GridInventory seller,Item stock,GridInventory target,int x,int y)
        {
            int price=MerchantPriceV48(stock.kind);
            if(price==0 || seller==target || !seller.items.Contains(stock) || (target!=inventory && target!=Bag && !(target==spatialInventory && SpatialLayerOpen)) || Coins<price || !target.CanPlace(stock,x,y))return false;
            if(!SpendCoins(price))return false;seller.items.Remove(stock);return target.Place(stock,x,y);
        }
        public static int MerchantPriceV48(ItemKind kind)=>kind==ItemKind.Pie?2:kind==ItemKind.BeetleMeat || kind==ItemKind.RawMeat?1:kind==ItemKind.Dagger?3:kind==ItemKind.Stake?4:kind==ItemKind.Spear?5:kind==ItemKind.Axe?6:kind==ItemKind.Sword?8:0;
        public bool Eat(Item item, GridInventory source)
        {
            var d = item.Definition;
            if (!d.Food || item.Spoiled || source == null || !source.items.Contains(item)) return false;
            if(item.kind!=ItemKind.Brain)
            {
                var effect = foods.FirstOrDefault(f => f.kind == item.kind);
                if (effect == null && foods.Count >= 5) return false;
                if (effect == null) { effect = new FoodEffect { kind = item.kind }; foods.Add(effect); }
                effect.remaining = effect.duration = d.foodDuration;
            }
            hunger = Mathf.Min(HungerMax, hunger + d.nourishment);
            mana = Mathf.Min(MaxMana, mana + d.mana);
            if(item.count>1)item.count--;else source.items.Remove(item); return true;
        }
        public void Tick(float delta)
        {
            foreach (var food in foods) food.remaining -= delta;
            foods.RemoveAll(f => f.remaining <= 0);
            hunger = Mathf.Clamp(hunger - delta * .035f, 0, HungerMax);
            if (health>0 && foods.Count >= 3) health = Mathf.Min(MaxHealth, health + delta * .6f);
            if (foods.Count >= 5) immunity = Mathf.Min(100, immunity + delta * .18f);
            if (hunger <= 0) health = Mathf.Max(0, health - delta * .1f);
            // Mana deliberately has no regeneration path.
        }
        public static bool Fits(Item item, GearSlot slot)
        {
            if (slot == GearSlot.Hand) return item.Definition.Weapon || item.kind == ItemKind.Orb;
            if (slot == GearSlot.Head) return item.kind == ItemKind.Helmet;
            if (slot == GearSlot.Body) return item.kind == ItemKind.Armor;
            if (slot == GearSlot.Back) return item.kind == ItemKind.Sack;
            if (slot == GearSlot.Amulet) return item.kind == ItemKind.Amulet;
            if (slot == GearSlot.Feet) return item.kind == ItemKind.Boots || item.kind==ItemKind.RollerSkates;
            return (slot == GearSlot.RingLeft || slot == GearSlot.RingRight) && item.kind == ItemKind.Ring;
        }
        public bool Equip(Item item, GridInventory source, GearSlot slot)
        {
            if(item.kind==ItemKind.RollerSkates && slot==GearSlot.Feet)return EquipSkates(item,source,false,out _);
            if (!Fits(item, slot) || !source.items.Contains(item)) return false;
            var previous = equipment[(int)slot];
            Vector2Int p = default;
            if (previous != null && !source.FindSpace(previous, out p, new[] { item })) return false;
            source.items.Remove(item);
            if (previous != null) source.Place(previous, p.x, p.y);
            equipment[(int)slot] = item; return true;
        }
        public bool AutoEquipEmpty(Item item,GridInventory source)
        {
            if(item.kind==ItemKind.RollerSkates)return Skates==null && EquipSkates(item,source,false,out _);
            for(int i=0;i<equipment.Length;i++)if(equipment[i]==null && Fits(item,(GearSlot)i))return Equip(item,source,(GearSlot)i);
            return false;
        }
        public bool EquipOrDrop(Item item,GridInventory source,GearSlot slot,out Item dropped)
        {
            if(item.kind==ItemKind.RollerSkates && slot==GearSlot.Feet)return EquipSkates(item,source,true,out dropped);
            dropped=null;if(!Fits(item,slot) || source==null || !source.items.Contains(item))return false;
            var previous=equipment[(int)slot];source.items.Remove(item);
            if(previous!=null && !inventory.Add(previous))dropped=previous;
            equipment[(int)slot]=item;return true;
        }
    }

    public static class CoastTime
    {
        public const float DayLength = 180, NightLength = 180, FogEnd = 10, StormEnd = 20;
        public static bool IsNight(float seconds) => Mathf.Repeat(seconds, 360) >= 180;
        public static float Daylight(float seconds)
        {
            float p = Mathf.Repeat(seconds, 360);
            if (p < 160) return 1;
            if (p < 180) return 1 - Mathf.SmoothStep(0, 1, (p - 160) / 20);
            if (p < 340) return 0;
            return Mathf.SmoothStep(0, 1, (p - 340) / 20);
        }
        public static float Fog(float seconds) => seconds < 7 ? .91f : .91f * (1 - Mathf.SmoothStep(0, 1, (seconds - 7) / 3));
        public static float Rain(float seconds) => 1 - Mathf.SmoothStep(0, 1, (seconds - 18) / 2);
        public static GridInventory CreateShipStorage()
        {
            var ship = new GridInventory(10, 8, "Трюм");
            for (int i = 0; i < 3; i++) ship.Add(new Item(ItemKind.Brain));
            ship.Add(new Item(ItemKind.Soup));
            for (int i = 0; i < 3; i++) ship.Add(new Item(ItemKind.BeetleMeat));
            ship.Place(new Item(ItemKind.Sword), 0, 2);
            ship.Place(new Item(ItemKind.Dagger), 2, 2);
            ship.Place(new Item(ItemKind.Scythe), 4, 2);
            ship.Place(new Item(ItemKind.Spear), 7, 2);
            ship.Place(new Item(ItemKind.Orb), 8, 0);
            ship.Place(new Item(ItemKind.Tome), 8, 5);
            for(int i=0;i<5;i++)ship.Place(new Item(ItemKind.Wood),i,6);
            for(int i=0;i<2;i++)if(!ship.Add(new Item(ItemKind.Wood)))throw new InvalidOperationException("Extra wood does not fit in hold");
            for(int i=0;i<5;i++)if(!ship.Add(new Item(ItemKind.IronIngot)))throw new InvalidOperationException("Iron ingots do not fit in the hold");
            for(int i=0;i<14;i++)if(!ship.Add(new Item(ItemKind.Clay)))throw new InvalidOperationException("Clay does not fit in the hold");
            return ship;
        }
    }
}
