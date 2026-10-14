using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public enum SpellKind { None, Pocket, WorldTravel, FireBlast, Tentacles, Abyss, Shackles, Curse }
    [Serializable] public sealed class SpellSlot
    {
        public SpellKind kind;
        public bool locked;
        public Item scroll;
        public float cooldownUntil;
        public float Remaining(float clock)=>Mathf.Max(0,cooldownUntil-clock);
    }
    public static class BookSpells
    {
        public static SpellSlot[] Create(ItemKind book)
        {
            if(book!=ItemKind.Tome && book!=ItemKind.Pyromancy && book!=ItemKind.AbyssTome)return null;
            var slots=Enumerable.Range(0,book==ItemKind.Pyromancy?4:6).Select(_=>new SpellSlot()).ToArray();
            void Builtin(int at,SpellKind kind){slots[at].kind=kind;slots[at].locked=true;}
            if(book==ItemKind.Tome){Builtin(0,SpellKind.Pocket);Builtin(1,SpellKind.WorldTravel);}
            else if(book==ItemKind.AbyssTome){Builtin(0,SpellKind.Tentacles);Builtin(1,SpellKind.Abyss);}
            else Builtin(0,SpellKind.FireBlast);
            return slots;
        }
        public static SpellKind ScrollSpell(ItemKind kind)=>kind==ItemKind.ShacklesScroll?SpellKind.Shackles:kind==ItemKind.CurseScroll?SpellKind.Curse:SpellKind.None;
        public static bool Insert(Item book,int index,Item scroll,GridInventory source)
        {
            if(book?.spells==null || index<0 || index>=book.spells.Length || scroll==null || source==null || !source.items.Contains(scroll))return false;
            var slot=book.spells[index];var spell=ScrollSpell(scroll.kind);
            if(slot.locked || slot.kind!=SpellKind.None || spell==SpellKind.None)return false;
            source.items.Remove(scroll);slot.kind=spell;slot.scroll=scroll;slot.cooldownUntil=Mathf.Max(slot.cooldownUntil,scroll.spellCooldown);return true;
        }
        public static Item Remove(Item book,int index)
        {
            if(book?.spells==null || index<0 || index>=book.spells.Length)return null;
            var slot=book.spells[index];if(slot.locked || slot.scroll==null)return null;
            var item=slot.scroll;item.spellCooldown=slot.cooldownUntil;book.spells[index]=new SpellSlot();return item;
        }
        public static int Cost(Item book,int baseMana)=>book?.kind==ItemKind.Pyromancy?Mathf.CeilToInt(baseMana*.7f):baseMana;
        public static string Name(SpellKind kind)=>kind==SpellKind.Pocket?"Карманное измерение":kind==SpellKind.WorldTravel?"Пространственный переход":kind==SpellKind.FireBlast?"Взрыв костра":kind==SpellKind.Tentacles?"Щупальца":kind==SpellKind.Abyss?"Бездна":kind==SpellKind.Shackles?"Небесные оковы":kind==SpellKind.Curse?"Порча":"Пустая ячейка";
    }
    public sealed partial class Survivor
    {
        public readonly GridInventory spatialInventory=new GridInventory(10,5,"Пространственный слой"){allowOpenBooks=true,craftingEnabled=false};
        public IEnumerable<Item> Books=>inventory.items.Concat(spatialInventory.items).Where(i=>i.spells!=null);
        public Item OpenBook(ItemKind kind)=>Books.FirstOrDefault(i=>i.kind==kind && i.opened && i.Usable);
        public bool SpatialLayerOpen=>OpenBook(ItemKind.Tome)!=null;
        public Vector2Int? abyssCell;
        public int AbyssCapacity(Item item,int x,int y)=>abyssCell.HasValue && abyssCell.Value==new Vector2Int(x,y) && item.Definition.width==1 && item.Definition.height==1 && !item.Definition.Durable?25:item.kind==ItemKind.Coin?10:1;
    }
}
