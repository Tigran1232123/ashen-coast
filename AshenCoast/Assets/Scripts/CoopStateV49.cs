using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    [Serializable] public sealed class LanPacketV49
    {public string kind,region,body;public int peer,target=-1,version=LanProtocolV49.Build;public long sequence;}
    [Serializable] public sealed class LanHelloV49 {public string name;public int build=LanProtocolV49.Build;}
    [Serializable] public sealed class LanWelcomeV49 {public int peer,seed,villageSeed;public float seconds,x,y,networkTime;public string name;}
    [Serializable] public sealed class LanPlayerV49
    {public int peer,facing,frame,character,weapon=-1;public string name,region;public float x,y,health=100,hunger=100,mana,immunity=100,walk,angle,scale=1;public bool moving,attacking,invulnerable,dead;public float duration,elapsed,weaponX,weaponY,weaponScale=1;public LanItemV49[] equipment;}
    [Serializable] public sealed class LanOwnerV49 {public string region;public int peer;}
    [Serializable] public sealed class LanRosterV49 {public LanPlayerV49[] players;public LanOwnerV49[] owners;}
    [Serializable] public sealed class LanItemV49
    {
        public static Func<float> SnapshotClock=()=>Time.realtimeSinceStartup;
        public float capturedAt;
        public static Func<float> Clock=()=>Time.time;
        public string id;public int kind,x,y,count;public bool opened;public float durability,fuel,freshness,shelfLife,water;
        public string containerData;public LanSpellV49[] spells;
        [NonSerialized] LanGridV49 containerCache;
        public LanGridV49 contents{get=>containerCache??(string.IsNullOrEmpty(containerData)?null:(containerCache=JsonUtility.FromJson<LanGridV49>(containerData)));set{containerCache=value;containerData=value==null?"":JsonUtility.ToJson(value);}}
        public static LanItemV49 Capture(Item i,int depth=0)
        {
            if(i==null)return new LanItemV49{kind=-1};if(depth>8)throw new InvalidOperationException("Container nesting limit");
            return new LanItemV49{capturedAt=SnapshotClock(),id=i.id,kind=(int)i.kind,x=i.x,y=i.y,count=i.count,opened=i.opened,durability=i.durability,fuel=i.fuel,freshness=i.freshness,shelfLife=i.shelfLife,water=i.water,contents=i.contents==null?null:LanGridV49.Capture(i.contents,"",0,depth+1),spells=i.spells?.Select(s=>new LanSpellV49{kind=(int)s.kind,locked=s.locked,remaining=s.Remaining(Clock()),scroll=Capture(s.scroll,depth+1)}).ToArray()};
        }
        public Item Restore(int depth=0)
        {
            if(kind<0)return null;
            if(depth>8 || !Enum.IsDefined(typeof(ItemKind),kind) || string.IsNullOrEmpty(id) || count<1 || count>25 || !Finite(durability) || !Finite(fuel) || !Finite(freshness) || !Finite(water))throw new InvalidOperationException("Invalid network item");
            float elapsed=Mathf.Clamp(SnapshotClock()-capturedAt,0,7200);
            var i=new Item((ItemKind)kind){id=id,x=x,y=y,count=count,opened=opened,durability=Mathf.Clamp(durability,0,100),fuel=Mathf.Clamp(fuel,0,100),freshness=Mathf.Max(0,freshness-(ItemDefinition.All[(ItemKind)kind].Food && (ItemKind)kind!=ItemKind.Brain?elapsed:0)),shelfLife=Mathf.Max(1,shelfLife),water=Mathf.Clamp(water,0,2)};
            if(contents!=null){if(i.contents==null || contents.columns!=i.contents.columns || contents.rows!=i.contents.rows)throw new InvalidOperationException("Invalid nested inventory");contents.Apply(i.contents,depth+1);}
            if(spells!=null && i.spells!=null){if(spells.Length!=i.spells.Length)throw new InvalidOperationException("Invalid spell slots");for(int n=0;n<spells.Length;n++){var s=spells[n];if(!Enum.IsDefined(typeof(SpellKind),s.kind) || i.spells[n].locked && (i.spells[n].kind!=(SpellKind)s.kind || !s.locked))throw new InvalidOperationException("Invalid locked spell");i.spells[n]=new SpellSlot{kind=(SpellKind)s.kind,locked=s.locked,scroll=s.scroll?.Restore(depth+1),cooldownUntil=Clock()+Mathf.Clamp(s.remaining-elapsed,0,3600)};}}
            return i;
        }
        public static bool Finite(float value)=>!float.IsNaN(value) && !float.IsInfinity(value);
    }
    [Serializable] public sealed class LanSpellV49
    {
        public int kind;public bool locked;public float remaining;public string scrollData;
        [NonSerialized] LanItemV49 scrollCache;
        public LanItemV49 scroll{get=>scrollCache??(string.IsNullOrEmpty(scrollData)?null:(scrollCache=JsonUtility.FromJson<LanItemV49>(scrollData)));set{scrollCache=value;scrollData=value==null?"":JsonUtility.ToJson(value);}}
    }
    [Serializable] public sealed class LanGridV49
    {
        public string key,name;public int columns,rows,water,abyssX=-1,abyssY=-1;public long revision;public LanItemV49[] items;
        public static LanGridV49 Capture(GridInventory grid,string key,long revision,int depth=0)=>new LanGridV49{key=key,name=grid.name,columns=grid.columns,rows=grid.rows,revision=revision,water=grid.waterRows,abyssX=grid.abyssCell?.x??-1,abyssY=grid.abyssCell?.y??-1,items=grid.items.Select(i=>LanItemV49.Capture(i,depth)).ToArray()};
        public void Apply(GridInventory grid,int depth=0)
        {
            if(columns!=grid.columns || rows!=grid.rows || items==null || items.Length>columns*rows || water<0 || water>rows)throw new InvalidOperationException("Invalid network inventory");
            var restored=items.Select(i=>i.Restore(depth)).ToList();var old=grid.items;var oldAbyss=grid.abyssCell;grid.items=restored;grid.abyssCell=abyssX>=0?new Vector2Int(abyssX,abyssY):(Vector2Int?)null;
            if(!grid.Valid() || restored.Any(i=>i.count>grid.StackLimit(i,i.x,i.y))){grid.items=old;grid.abyssCell=oldAbyss;throw new InvalidOperationException("Overlapping network items");}grid.waterRows=water;
        }
    }
    [Serializable] public sealed class LanPrivateV49
    {
        public LanGridV49 main,spatial,holding,skates;public LanItemV49[] equipment;
        public static LanPrivateV49 Capture(CoastGame g)=>new LanPrivateV49{main=LanGridV49.Capture(g.survivor.inventory,"main",0),spatial=LanGridV49.Capture(g.survivor.spatialInventory,"spatial",0),holding=LanGridV49.Capture(g.ui.holding,"hand",0),skates=LanGridV49.Capture(g.survivor.skateSlot,"skates",0),equipment=g.survivor.equipment.Select(i=>LanItemV49.Capture(i)).ToArray()};
        public void Apply(CoastGame g){main.Apply(g.survivor.inventory);g.survivor.abyssCell=g.survivor.inventory.abyssCell;spatial.Apply(g.survivor.spatialInventory);holding.Apply(g.ui.holding);skates.Apply(g.survivor.skateSlot);for(int i=0;i<g.survivor.equipment.Length;i++)g.survivor.equipment[i]=equipment[i]?.Restore();g.ui.RestoreHandV49();}
    }
    [Serializable] public sealed class LanTransactionV49 {public string id;public LanGridV49[] grids;public LanPrivateV49 inventory;public LanWorldActionV49[] actions;}
    [Serializable] public sealed class LanCommitV49 {public string id;public bool accepted;public LanGridV49[] grids;public LanWorldActionV49[] actions;}
    [Serializable] public sealed class LanWorldActionV49 {public string kind,key,region;public float x,y;public int value;public bool upright;public LanItemV49 item;}
    [Serializable] public sealed class LanDamageV49 {public string key;public float amount;public bool bypass,prism;public int effect,delivery;}
    [Serializable] public sealed class LanEffectV49 {public string kind,key;public float x,y,value,value2;public Vector2[] points;}
}
