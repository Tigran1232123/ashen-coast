using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed class CraftingStation
    {
        public ItemKind kind;
        public Vector2Int cell,size;
        public Vector2 Position=>new Vector2(cell.x+size.x*.5f,cell.y+size.y*.5f);
        public RectInt Cells=>new RectInt(cell,size);
        public SpriteRenderer renderer;
        public GridInventory inventory;
        public Transform world;
        public BoxCollider2D collider;
        public float temperature=20;public CampfireObject fire;public CookingBatch batch;public float ready,stirring,fryFlash;public readonly List<Item> fried=new List<Item>();public SpriteRenderer steam;public bool Heated=>VesselWaterV43.Vessel(kind) && fire!=null && !fire.exploded;
    }

    public sealed partial class BuildingSystem : MonoBehaviour
    {
        public CoastGame game;
        public readonly List<CraftingStation> stations=new List<CraftingStation>();
        public Item pending;
        public GridInventory source;
        public SpriteRenderer ghost;
        public bool vertical=true;
        public bool Placing=>pending!=null;
        public Item PreviewItem=>pending??(game && game.ui.Dragging && Buildable(game.ui.DraggedItem.kind)?game.ui.DraggedItem:null);
        public bool ChoosingPlacement=>PreviewItem!=null && (Placing || !game.ui.IsPointerOver);
        Item ghostItem;
        int lastWheelFrame=-1;
        public int WheelRotations {get;private set;}
        public Vector2 PreviewPoint {get;private set;}
        public static bool Buildable(ItemKind kind)=>kind==ItemKind.Crossbow || kind==ItemKind.Campfire || kind==ItemKind.CraftingTable || kind==ItemKind.Anvil || VesselWaterV43.Vessel(kind);
        public static Vector2Int Footprint(ItemKind kind,bool upright)=>kind==ItemKind.CraftingTable?(upright?new Vector2Int(1,2):new Vector2Int(2,1)):Vector2Int.one;
        public void Initialize(CoastGame owner){game=owner;}
        public bool Begin(Item item,GridInventory inventory)
        {
            if(item==null || !Buildable(item.kind) || inventory==null || !inventory.items.Contains(item))return false;
            Cancel();pending=item;source=inventory;vertical=true;
            RefreshPreview(game.CursorWorld);return true;
        }
        public void Rotate(){if(ChoosingPlacement && PreviewItem.kind==ItemKind.CraftingTable){vertical=!vertical;RefreshPreview(PreviewPoint);}}
        public bool HandleWheel(float amount)
        {
            if(!ChoosingPlacement || PreviewItem.kind!=ItemKind.CraftingTable || Mathf.Abs(amount)<.01f)return false;
            if(lastWheelFrame==Time.frameCount)return true;
            lastWheelFrame=Time.frameCount;Rotate();WheelRotations++;return true;
        }
        void Update()
        {
            if(!game || game.paused || game.testMode)return;
            if(ChoosingPlacement)
            {
                if(game.Dead || game.magic.Teleporting || game.journey.Cutscene){Cancel();return;}
                RefreshPreview(game.CursorWorld);
                HandleWheel(Input.mouseScrollDelta.y);
                if(Placing && Input.GetMouseButtonDown(1)){Cancel();game.RightClickConsumedFrame=Time.frameCount;return;}
            }
            else if(ghost){ghost.enabled=false;if(PreviewItem==null){Destroy(ghost.gameObject);ghost=null;ghostItem=null;}}
            Tick(Time.deltaTime);
        }
        public void Tick(float dt)
        {
            if(!game.SimulateWorldV49)return;
            foreach(var pot in stations)if(VesselWaterV43.Vessel(pot.kind) && pot.world.gameObject.activeInHierarchy)TickCooking(pot,dt);
            foreach(var station in stations)if(station.kind==ItemKind.Anvil && station.world && station.world.gameObject.activeInHierarchy)
            {
                float heat=20;
                foreach(var fire in game.arcane.campfires)if(!fire.exploded && fire.logs.transform.parent==station.world)
                {
                    float d=Vector2.Distance(fire.position,station.Position);
                    if(d<3.5f)heat+=230*(1-d/4.3f);
                }
                station.temperature=Mathf.MoveTowards(station.temperature,Mathf.Min(1000,heat),dt*(heat>station.temperature?70:25));
            }
        }
        public bool CanPlace(Vector2 at,ItemKind kind,bool upright)
        {
            var cell=WorldGrid.Cell(at);var size=Footprint(kind,upright);
            var centre=new Vector2(cell.x+size.x*.5f,cell.y+size.y*.5f);
            if(Vector2.Distance(centre,game.body.position)>(kind==ItemKind.Campfire?2.5f:3.8f))return false;
            var onFire=VesselWaterV43.Vessel(kind)?game.arcane.campfires.FirstOrDefault(f=>!f.exploded && f.Cell==cell && f.logs.transform.parent==game.arcane.WorldRoot):null;
            if(Physics2D.OverlapBoxAll(centre,new Vector2(size.x-.10f,size.y-.10f),0).Any(c=>!c.isTrigger && (onFire==null || c!=onFire.collider)))return false;
            var region=new RectInt(cell,size);var world=game.arcane.WorldRoot;
            if(stations.Any(s=>s.world==world && region.Overlaps(s.Cells)))return false;
            if(game.arcane.campfires.Any(f=>!f.exploded && f.logs.transform.parent==world && region.Contains(f.Cell) && f!=onFire))return false;
            for(int x=0;x<size.x;x++)for(int y=0;y<size.y;y++)
            {
                var p=new Vector2(cell.x+x+.5f,cell.y+y+.5f);
                if(game.magic.InPocket){if(Mathf.Abs(p.x)>3.6f || Mathf.Abs(p.y)>3.6f)return false;}
                else if(!game.journey.IsCoast){if(Vector2.Distance(p,game.journey.Clamp(p))>.001f || (game.cave.Inside && !game.cave.Walkable(p,.45f)))return false;}
                else if(!MeadowLayout.Walkable(p) || p.y<=MeadowLayout.Shore(p.x)+.5f)return false;
            }
            return true;
        }
        public void RefreshPreview(Vector2 at)
        {
            var item=PreviewItem;if(item==null)return;
            if(!ghost || ghostItem!=item)
            {
                if(ghost)Destroy(ghost.gameObject);
                ghost=game.art.Renderer("Translucent building preview",game.art.icons[item.Definition.icon],Vector2.zero,4000);
                ghostItem=item;vertical=true;
            }
            ghost.enabled=true;
            PreviewPoint=at;
            var cell=WorldGrid.Cell(at);var size=Footprint(item.kind,vertical);
            ghost.transform.position=new Vector2(cell.x+size.x*.5f,cell.y+size.y*.5f);
            PoseStructure(ghost,item.kind,vertical);if(VesselWaterV43.Vessel(item.kind) && game.arcane.campfires.Any(f=>!f.exploded && f.Cell==cell && f.logs.transform.parent==game.arcane.WorldRoot))ghost.transform.position+=Vector3.up*.29f;
            ghost.color=CanPlace(at,item.kind,vertical)?new Color(.94f,1,.91f,.48f):new Color(1,.38f,.31f,.42f);
        }
        void PoseStructure(SpriteRenderer sr,ItemKind kind,bool upright)
        {
            if(kind==ItemKind.CraftingTable)sr.sprite=upright?game.art.tableUpright:game.art.icons[32];
            else if(kind==ItemKind.Campfire)sr.sprite=game.art.campfireBase;
            float width=kind==ItemKind.Crossbow?2.3f:kind==ItemKind.Campfire?.90f:kind==ItemKind.CraftingTable?(upright?.94f:1.92f):VesselWaterV43.Vessel(kind)?.93f:.91f;
            sr.transform.localScale=Vector3.one*(width/sr.sprite.bounds.size.x);
            sr.transform.rotation=Quaternion.identity;if(kind==ItemKind.Crossbow)sr.transform.position+=Vector3.up*sr.bounds.extents.y;game.art.PixelStyle(sr);
        }
        public bool Place(Vector2 at) => game.coop.inventory.Mutate(()=>PlaceV49(at));
        bool PlaceV49(Vector2 at)
        {
            if(!Placing || !source.items.Contains(pending) || !CanPlace(at,pending.kind,vertical))return false;
            var placed=pending;var kind=pending.kind;var contents=pending.contents;var cell=WorldGrid.Cell(at);source.items.Remove(pending);
            if(kind==ItemKind.Campfire)game.arcane.CreateCampfire(WorldGrid.Snap(at));
            else CreateStation(kind,cell,vertical,contents);
            RecordPlacementV49(placed,cell,vertical);Cancel();game.PlayPickup();return true;
        }
        public bool PlaceCarried(Item item,GridInventory inventory,Vector2 at) => game.coop.inventory.Mutate(()=>PlaceCarriedV49(item,inventory,at));
        bool PlaceCarriedV49(Item item,GridInventory inventory,Vector2 at)
        {
            if(item==null || !Buildable(item.kind) || inventory==null || !inventory.items.Contains(item) || !CanPlace(at,item.kind,vertical))return false;
            inventory.items.Remove(item);
            if(item.kind==ItemKind.Campfire)game.arcane.CreateCampfire(WorldGrid.Snap(at));else CreateStation(item.kind,WorldGrid.Cell(at),vertical,item.contents);
            RecordPlacementV49(item,WorldGrid.Cell(at),vertical);
            Cancel();game.PlayPickup();return true;
        }
        public CraftingStation CreateStation(ItemKind kind,Vector2Int cell,bool upright,GridInventory contents=null,Transform location=null)
        {
            var station=new CraftingStation{kind=kind,cell=cell,size=Footprint(kind,upright),world=location??game.arcane.WorldRoot,
                inventory=contents??new GridInventory(kind==ItemKind.Cauldron?4:5,kind==ItemKind.Cauldron?4:5,kind==ItemKind.Anvil?"Наковальня":kind==ItemKind.Cauldron?"Котелок":kind==ItemKind.StoragePot?"Горшок":"Стол для ремесла"){craftLimit=4,craftingEnabled=kind==ItemKind.CraftingTable || kind==ItemKind.Anvil,containerKind=kind}};
            station.inventory.containerKind=kind;if(kind==ItemKind.Anvil)station.inventory.craftingEnabled=true;
            station.renderer=game.art.Renderer(station.inventory.name,game.art.icons[ItemDefinition.All[kind].icon],station.Position,CoastGame.Depth(station.Position.y));
            station.renderer.transform.SetParent(station.world,true);PoseStructure(station.renderer,kind,upright);
            var obstacle=new GameObject(station.inventory.name+" grid footprint");obstacle.transform.SetParent(station.world,true);obstacle.transform.position=station.Position;
            station.collider=obstacle.AddComponent<BoxCollider2D>();station.collider.size=(Vector2)station.size-Vector2.one*.06f;
            if(kind==ItemKind.Crossbow){station.collider.size=new Vector2(.75f,.35f);station.collider.offset=new Vector2(-.58f,.16f);}
            if(kind==ItemKind.Anvil)station.collider.size=new Vector2(.80f,.64f);
            if(VesselWaterV43.Vessel(kind)){station.collider.size=new Vector2(.76f,.66f);station.fire=game.arcane.campfires.FirstOrDefault(f=>!f.exploded && f.Cell==cell && f.logs.transform.parent==station.world);if(station.fire!=null){station.collider.enabled=false;station.renderer.transform.position+=Vector3.up*.29f;station.renderer.sortingOrder=station.fire.logs.sortingOrder+7;}}
            CreateClearing(station.cell,station.size,station.world,false);
            stations.Add(station);return station;
        }
        public CraftingStation At(Vector2 point)
        {
            foreach(var s in stations)if(s.world && s.world.gameObject.activeInHierarchy &&
                (s.Cells.Contains(WorldGrid.Cell(point)) || game.art.Opaque(s.renderer.sprite,s.renderer.transform.InverseTransformPoint(point))))return s;
            return null;
        }
        public bool CanInteract(Vector2 point)=>At(point)!=null;
        public bool Interact(Vector2 point)
        {
            var station=At(point);if(station==null || Vector2.Distance(station.Position,game.body.position)>3.1f)return false;
            if(station.kind==ItemKind.Crossbow)return game.MountV58(station);game.ui.OpenStation(station);return true;
        }
        public void Cancel(){pending=null;source=null;if(ghost)Destroy(ghost.gameObject);ghost=null;ghostItem=null;}
        void OnGUI()
        {
            if(ChoosingPlacement && !game.paused && Event.current.type==EventType.ScrollWheel && HandleWheel(-Event.current.delta.y)){Event.current.Use();return;}
            if(!ChoosingPlacement || game.paused || Event.current.type!=EventType.Repaint)return;GUI.depth=-5;
            var a=(Vector2)game.cameraView.ViewportToWorldPoint(Vector3.zero);var b=(Vector2)game.cameraView.ViewportToWorldPoint(Vector3.one);
            for(int x=Mathf.FloorToInt(a.x);x<=Mathf.CeilToInt(b.x);x++)GridLine(new Vector2(x,a.y),new Vector2(x,b.y));
            for(int y=Mathf.FloorToInt(a.y);y<=Mathf.CeilToInt(b.y);y++)GridLine(new Vector2(a.x,y),new Vector2(b.x,y));
            var item=PreviewItem;var cell=WorldGrid.Cell(PreviewPoint);var size=Footprint(item.kind,vertical);
            var p=ScreenPoint(cell);var q=ScreenPoint(cell+size);
            bool valid=CanPlace(PreviewPoint,item.kind,vertical);var box=Rect.MinMaxRect(Mathf.Min(p.x,q.x),Mathf.Min(p.y,q.y),Mathf.Max(p.x,q.x),Mathf.Max(p.y,q.y));
            GUI.color=valid?new Color(.64f,.91f,.47f,.18f):new Color(.94f,.29f,.20f,.22f);GUI.DrawTexture(box,Texture2D.whiteTexture);
            Outline(box,4,new Color(.04f,.06f,.035f,.9f));
            Outline(new Rect(box.x+1,box.y+1,box.width-2,box.height-2),2,valid?new Color(.75f,1,.51f,.95f):new Color(1,.35f,.24f,.95f));GUI.color=Color.white;
        }
        Vector2 ScreenPoint(Vector2 p){var v=game.cameraView.WorldToViewportPoint(p);return new Vector2(v.x*Screen.width,(1-v.y)*Screen.height);}
        void GridLine(Vector2 a,Vector2 b)
        {
            a=ScreenPoint(a);b=ScreenPoint(b);bool horizontal=Mathf.Abs(a.y-b.y)<.01f;
            var r=new Rect(Mathf.Round(Mathf.Min(a.x,b.x)),Mathf.Round(Mathf.Min(a.y,b.y)),Mathf.Max(1,Mathf.Abs(a.x-b.x)),Mathf.Max(1,Mathf.Abs(a.y-b.y)));
            var shade=horizontal?new Rect(r.x,r.y-1,r.width,3):new Rect(r.x-1,r.y,3,r.height);
            GUI.color=new Color(.025f,.04f,.025f,.66f);GUI.DrawTexture(shade,Texture2D.whiteTexture);
            GUI.color=new Color(.85f,.92f,.72f,.66f);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;
        }
        static void Outline(Rect r,float thickness,Color color)
        {
            GUI.color=color;GUI.DrawTexture(new Rect(r.x,r.y,r.width,thickness),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(r.x,r.yMax-thickness,r.width,thickness),Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x,r.y,thickness,r.height),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(r.xMax-thickness,r.y,thickness,r.height),Texture2D.whiteTexture);
        }
    }
}
