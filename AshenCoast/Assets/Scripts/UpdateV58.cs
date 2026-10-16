using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class Survivor
    {
        public bool InfiniteVitalsV58;
        public IEnumerable<GridInventory> AmmoGridsV58=>new[]{inventory,Bag,SpatialLayerOpen?spatialInventory:null}.Where(g=>g!=null);
        public int ArrowsV58=>AmmoGridsV58.Sum(g=>g.items.Where(i=>i.kind==ItemKind.Arrow).Sum(i=>i.count));
        public bool ConsumeArrowV58()
        {foreach(var grid in AmmoGridsV58){var arrow=grid.items.FirstOrDefault(i=>i.kind==ItemKind.Arrow);if(arrow==null)continue;if(--arrow.count==0)grid.items.Remove(arrow);return true;}return false;}
    }
    public sealed partial class GridInventory
    {
        bool AddArrowsV58(Item item)
        {
            if(items.Contains(item))return true;if(item.count<1)return false;
            var stacks=items.Where(i=>i.kind==ItemKind.Arrow && i.count<30).ToList();
            int capacity=stacks.Sum(i=>30-i.count);var places=new List<Vector2Int>();
            for(int y=0;y<rows;y++)for(int x=0;x<columns;x++)if(CanPlace(item,x,y) && !places.Any(p=>new RectInt(p.x,p.y,1,3).Overlaps(new RectInt(x,y,1,3))))places.Add(new Vector2Int(x,y));
            if(capacity+places.Count*30<item.count)return false;
            foreach(var stack in stacks){int n=Mathf.Min(30-stack.count,item.count);stack.count+=n;item.count-=n;if(item.count==0)return true;}
            int left=item.count;for(int i=0;left>0;i++){int n=Mathf.Min(30,left);var next=i==0?item:new Item(ItemKind.Arrow);next.count=n;Place(next,places[i].x,places[i].y);left-=n;}return true;
        }
    }
    public sealed partial class LanItemV49
    {
        public bool preservedV58;
        public static LanItemV49 CapturePreservedV58(Item item,int depth,bool preserved)
        {var result=Capture(item,depth);Mark(result,preserved);return result;}
        static void Mark(LanItemV49 item,bool preserved)
        {if(item==null)return;item.preservedV58=preserved;if(item.contents!=null){foreach(var child in item.contents.items)Mark(child,preserved);item.contents=item.contents;}}
    }
    public sealed partial class CoopSessionV49
    {
        float worldSpeedV58=1;
        public bool AllMeditatingV58=>game.Meditating && players.Count>0 && players.Where(p=>p.Key!=Peer).All(p=>p.Value.meditating && p.Value.health>0);
    }
    public sealed partial class InventoryUI
    {
        public void CloseFromEscapeV58()
        {
            game.coop.inventory.Mutate(()=>{foreach(var item in holding.items.ToArray())
            {if((origin!=null && origin!=holding && origin.Add(item)) || game.survivor.inventory.Add(item) || game.survivor.Bag!=null && game.survivor.Bag.Add(item))holding.items.Remove(item);}
            dragItem=null;dragMerchant=false;CloseContainers();});
        }
    }
    public sealed partial class CoastGame
    {
        public CraftingStation MountedCrossbowV58;public Sprite operatorLegsV58;
        public string MountedKeyV58=>MountedCrossbowV58==null?null:MountKeyV58(MountedCrossbowV58);
        public string MountKeyV58(CraftingStation s)=>coop.world.RegionOf(s.world)+"/crossbow/"+s.cell.x+","+s.cell.y;
        float mountedReadyV58,rockBlendV58;
        public void CheckMountedPoseV58(){PoseMountedV58();meditation.PoseOperatorV58(MountedCrossbowV58!=null);}
        public void ToggleCheatsV58()
        {ToggleFastMovement();survivor.InfiniteVitalsV58=FastMovement;if(FastMovement){survivor.health=survivor.MaxHealth;survivor.mana=survivor.MaxMana;}ui.Notice(FastMovement?"T: скорость ×5 · здоровье и мана бесконечны":"T: обычная скорость · здоровье и мана расходуются");}
        public void LoadArtV58()
        {
            Sprite Whole(string name){var t=art.Load(name);return art.Slice(t,new RectInt(0,0,t.width,t.height),Vector2.one*.5f,200);}
            var op=art.Load("OperatorLowerV58");operatorLegsV58=art.Slice(op,new RectInt(0,0,op.width,op.height),new Vector2(.5f,0),200);
            art.icons[58]=Whole("ArrowV58");art.icons[59]=Whole("FeatherV58");art.icons[60]=Whole("CrossbowV58");
            var book=art.Load("LeafBookV58");int cut=Mathf.RoundToInt(book.width*.445f);
            art.icons[61]=art.Slice(book,new RectInt(0,0,cut,book.height),Vector2.one*.5f,200);
            art.icons[62]=art.Slice(book,new RectInt(cut,0,book.width-cut,book.height),Vector2.one*.5f,200);
        }
        void InitializeV58(){AudioListener.pause=snails.SetupOpen;ConfigureRiverWorldV58();map.ResetWorldV56();}
        void TickSetupAudioV58(){AudioListener.pause=snails!=null && snails.SetupOpen;}
        public bool MountV58(CraftingStation station)
        {
            if(station?.kind!=ItemKind.Crossbow || Dead || Meditating || Vector2.Distance(body.position,station.Position)>3.1f)return false;
            if(CoopOnline && coop.remote.Values.Any(p=>p.State.region==coop.Region && p.State.mountKeyV58==MountKeyV58(station))){ui.Notice("Арбалет уже занят");return false;}
            MountedCrossbowV58=station;combat.ResetForCheck();ui.CloseContainers();body.linearVelocity=Vector2.zero;movement=Vector2.zero;
            var seat=station.Position-Vector2.right*1.2f-Vector2.up*.25f;body.position=seat;hero.transform.position=seat;
            ui.Notice("Арбалет: ЛКМ — выстрел · Esc / ПКМ / движение — встать");return true;
        }
        public void DismountV58(){if(MountedCrossbowV58!=null)PoseCrossbowV58(MountedCrossbowV58,false);MountedCrossbowV58=null;hero.transform.localScale=Vector3.one;}
        public void PoseCrossbowV58(CraftingStation station,bool left){station.renderer.flipX=left;station.collider.offset=new Vector2(left?.58f:-.58f,.16f);}
        public bool FireCrossbowV58(Vector2 direction)
        {
            if(MountedCrossbowV58==null || Time.time<mountedReadyV58)return false;
            if(!survivor.ConsumeArrowV58()){ui.Notice("Нужны стрелы");return false;}
            mountedReadyV58=Time.time+2;amazons.SpawnStraightV58(MountedCrossbowV58.Position,direction,hero);PlaySwish();return true;
        }
        void PoseMountedV58()
        {
            if(MountedCrossbowV58==null || paused || LocalMenuPausedV49)return;if(Dead || !MountedCrossbowV58.world.gameObject.activeInHierarchy || CoopOnline && coop.remote.Values.Any(p=>p.Peer<coop.Peer && p.State.mountKeyV58==MountedKeyV58)){DismountV58();return;}
            if(!LocalInputLockedV49 && (Input.GetKeyDown(KeyCode.W)||Input.GetKeyDown(KeyCode.A)||Input.GetKeyDown(KeyCode.S)||Input.GetKeyDown(KeyCode.D)||Input.GetMouseButtonDown(1))){DismountV58();return;}
            var at=MountedCrossbowV58.Position;var delta=CursorWorld-at;var aim=delta.sqrMagnitude>.001f?delta.normalized:Vector2.right;
            var seat=at-aim*1.2f-Vector2.up*.2f;body.position=seat;hero.transform.position=seat;facing=Facing(aim);hero.transform.localScale=Vector3.one;hero.flipX=aim.x>=0;
            PoseCrossbowV58(MountedCrossbowV58,aim.x<0);
            if(!LocalInputLockedV49 && !ui.IsPointerOver && Input.GetMouseButton(0))FireCrossbowV58(aim);
        }
        public Vector3 SnailRockV58()
        {
            if(journey.Area!=WorldArea.SnailHouse){rockBlendV58=0;return Vector3.zero;}
            bool moving=journey.Area==WorldArea.SnailHouse && snails.Inside>=0 && snails.houses[snails.Inside].moving && !paused;
            rockBlendV58=Mathf.MoveTowards(rockBlendV58,moving?1:0,Time.unscaledDeltaTime*1.6f);
            float clock=Time.time;return new Vector3(Mathf.Sin(clock*2.8f)*.075f,Mathf.Sin(clock*5.6f+.4f)*.045f,Mathf.Sin(clock*2.8f)*.32f)*rockBlendV58;
        }
    }
}
