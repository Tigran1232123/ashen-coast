using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public enum WorldArea { Coast,GoldenField,Tavern,Fort,Catacombs,AmazonHouse,SnailHouse }
    public static class WorldGrid
    {
        public const float CellSize=1;
        public static Vector2 Snap(Vector2 p)=>new Vector2(Mathf.Floor(p.x)+.5f,Mathf.Floor(p.y)+.5f);
        public static Vector2Int Cell(Vector2 p)=>new Vector2Int(Mathf.FloorToInt(p.x),Mathf.FloorToInt(p.y));
    }
    public sealed class TavernActor
    {public SpriteRenderer sprite;public int row,frame;public Vector2 position;public PixelGait gait;}
    public sealed class Poplar
    {public SpriteRenderer sprite;public MeshRenderer canopy;public float alpha=1,phase;public Vector2 position;}
    public sealed class BeachGull:ICombatTarget
    {public SpriteRenderer sprite;public Vector2 home,point,flight=Vector2.right;public float phase,age,health=30;public int frame,state;public bool Alive=>health>0;public SpriteRenderer Renderer=>sprite;public Vector2 Position=>point;public float Hurt(float n){float hit=Mathf.Min(n,health);health-=hit;return hit;}public void TakeOff(Vector2 player,CoastGame game){if(state>=2)return;state=2;age=0;flight=(point-player+Vector2.up*1.2f).normalized;game.soundscape?.GullFlight();}}
    public sealed partial class JourneyWorld : MonoBehaviour
    {
        public CoastGame game;
        public ExpansionArt art;
        public GameObject fieldRoot,tavernRoot,cartRoot;
        public WorldArea Area {get;private set;}
        public bool IsCoast=>Area==WorldArea.Coast;
        public bool Interior=>Area==WorldArea.Tavern;
        public bool InCatacombs=>Area==WorldArea.Catacombs;
        public bool InFort=>Area==WorldArea.Fort;
        public Transform ActiveRoot=>Area==WorldArea.SnailHouse && game.snails.Inside>=0?game.snails.houses[game.snails.Inside].room.transform:Area==WorldArea.AmazonHouse?game.amazons.roomRoot.transform:InCatacombs?game.cave.root.transform:InFort?game.fort.root.transform:Interior?tavernRoot.transform:IsCoast?game.coastRoot.transform:fieldRoot.transform;
        public bool Cutscene=>ridePhase!=0;
        public int RidePhase=>ridePhase;
        public float BlackFade {get;private set;}
        public Vector3 CutsceneCamera {get;private set;}
        public readonly GridInventory seller=new GridInventory(10,5,"Трактирщик");
        public readonly List<TavernActor> actors=new List<TavernActor>();
        public readonly List<Poplar> trees=new List<Poplar>();
        public readonly List<BeachGull> gulls=new List<BeachGull>();
        public readonly List<SpriteRenderer> pullers=new List<SpriteRenderer>();
        public SpriteRenderer wagon,driver,inn,exitDoor,fireplace;
        public SpriteRenderer innShadow,hearthFire;
        public const float HearthCycleSeconds=2.4f;
        public TavernActor keeper,drinker;
        public CompanionMage companion;
        public bool HireOffer;
        public float clock;
        public Vector2 InnEntrance=new Vector2(0,6),TavernExit=new Vector2(0,-4.7f);
        public Vector2 CartPosition=>(Vector2)cartRoot.transform.position;
        readonly List<LineRenderer> reins=new List<LineRenderer>();
        int ridePhase;float phaseTime;Vector2 boardingStart,cartStart,dismountStart;WorldArea destination;bool hidden;
        float doorCooldown;
        MaterialPropertyBlock properties;
        public void Initialize(CoastGame owner)
        {
            properties=new MaterialPropertyBlock();game=owner;art=new ExpansionArt(game.art);fieldRoot=new GameObject("Golden poplar meadow");tavernRoot=new GameObject("Poplar tavern and pantry");
            CreateField();game.FieldCloversV43(fieldRoot.transform);CreateTavern();CreateCart();CreateGulls();
            companion=new CompanionMage(this);CreateResidents();RefillShop();SetVisibility(false);
        }
        public void RefillShop()
        {seller.items.Clear();for(int i=0;i<6;i++)seller.Add(new Item(ItemKind.Pie));for(int i=0;i<12;i++)seller.Add(new Item(ItemKind.BeetleMeat));}
        SpriteRenderer Prop(string name,Sprite sprite,Vector2 point,float width,Transform root,int? order=null)
        {var sr=game.art.Renderer(name,sprite,point,order??CoastGame.Depth(point.y));sr.transform.localScale=Vector3.one*(width/sprite.bounds.size.x);sr.transform.SetParent(root,true);game.art.PixelStyle(sr);return sr;}
        void Ground(string name,Transform root,Vector2 point,Vector2 size,float mode)
        {
            var o=GameObject.CreatePrimitive(PrimitiveType.Quad);o.name=name;o.transform.SetParent(root,false);o.transform.position=new Vector3(point.x,point.y,5);o.transform.localScale=new Vector3(size.x,size.y,1);Destroy(o.GetComponent<Collider>());
            var mat=new Material(Shader.Find("AshenCoast/JourneyFloor"));mat.SetTexture("_GroundTex",Resources.Load<Texture2D>("Art/TerrainTiles"));mat.SetTexture("_MeadowTex",Resources.Load<Texture2D>("Art/GoldenMeadowV15"));mat.SetTexture("_WoodTex",Resources.Load<Texture2D>("Art/TavernSurfacesV10"));mat.SetFloat("_Indoor",mode);o.GetComponent<MeshRenderer>().sharedMaterial=mat;
        }
        void Box(string name,Transform root,Vector2 centre,Vector2 size)
        {var obj=new GameObject(name);obj.transform.SetParent(root,false);obj.transform.position=centre;obj.AddComponent<BoxCollider2D>().size=size;}
        void CreateField()
        {
            Ground("Dry golden grass and winding cart road",fieldRoot.transform,new Vector2(0,10),new Vector2(100,80),0);
            innShadow=game.Shadow("Tavern soft contact shadow",InnEntrance+Vector2.up*.16f,new Vector2(10.0f,.86f),.32f);innShadow.transform.SetParent(fieldRoot.transform,true);
            var seam=game.Shadow("Tavern foundation contact",InnEntrance+Vector2.up*.12f,new Vector2(9.4f,.32f),.42f);seam.transform.SetParent(fieldRoot.transform,true);
            inn=Prop("Timber tavern",art.inn,InnEntrance,10.4f,fieldRoot.transform);
            Box("Tavern foundation",fieldRoot.transform,InnEntrance+new Vector2(0,1.8f),new Vector2(9.5f,2.9f));
            Vector2[] points={new Vector2(-9,0),new Vector2(9,3),new Vector2(-15,12),new Vector2(14,16),new Vector2(-22,-9),new Vector2(20,-5),new Vector2(-5,20),new Vector2(25,23),new Vector2(-26,24)};
            for(int i=0;i<points.Length;i++)
            {
                var s=Prop("Very tall wind-blown poplar",i%2==0?art.poplar:art.poplar2,points[i],i%2==0?3.5f:3.1f,fieldRoot.transform);s.transform.localScale*=1.12f;
                var meshObj=new GameObject("Flexible poplar crown");meshObj.transform.SetParent(s.transform,false);meshObj.AddComponent<MeshFilter>().sharedMesh=ArtBank.BendableMesh(s.sprite);
                var canopy=meshObj.AddComponent<MeshRenderer>();canopy.sharedMaterial=game.art.lit;canopy.sortingOrder=s.sortingOrder;
                s.GetPropertyBlock(properties);properties.SetFloat("_WindStrength",.24f);properties.SetFloat("_WindHeight",s.sprite.bounds.size.y);properties.SetFloat("_WindPhase",i*2.37f);properties.SetFloat("_OcclusionAlpha",1);canopy.SetPropertyBlock(properties);s.forceRenderingOff=true;
                var feet=s.gameObject.AddComponent<CircleCollider2D>();feet.radius=.28f/s.transform.localScale.x;feet.offset=new Vector2(0,.10f/s.transform.localScale.y);
                trees.Add(new Poplar{sprite=s,canopy=canopy,position=points[i],phase=i});
            }
        }
        void CreateTavern()
        {
            var room=tavernRoot.transform;
            Ground("Worn tavern oak floor",room,new Vector2(.5f,0),new Vector2(15,10),1);
            Box("North wall",room,new Vector2(.5f,5.1f),new Vector2(15,.35f));Box("West wall",room,new Vector2(-7.1f,0),new Vector2(.35f,10));Box("East wall",room,new Vector2(8.1f,0),new Vector2(.35f,10));
            Box("South wall left",room,new Vector2(-3.9f,-5.1f),new Vector2(6.25f,.35f));Box("South wall right",room,new Vector2(4.4f,-5.1f),new Vector2(7.25f,.35f));
            Box("Pantry partition north",room,new Vector2(4.1f,3.5f),new Vector2(.36f,3));Box("Pantry partition south",room,new Vector2(4.1f,-2.75f),new Vector2(.36f,4.5f));
            for(int i=0;i<3;i++)Wall("Back plaster and timber wall",art.walls[0],new Vector2(-4.5f+i*5,4.62f),new Vector2(5,1.55f));
            for(int i=0;i<3;i++){Wall("West cutaway wall",art.walls[1],new Vector2(-7.0f,-5.05f+i*3.4f),new Vector2(.24f,3.4f));Wall("East cutaway wall",art.walls[1],new Vector2(8.0f,-5.05f+i*3.4f),new Vector2(.24f,3.4f));}
            Wall("Pantry upper timber partition",art.walls[1],new Vector2(4.1f,2.0f),new Vector2(.36f,3.1f));
            Wall("Pantry lower timber partition",art.walls[1],new Vector2(4.1f,-5.05f),new Vector2(.36f,4.55f));
            Wall("Front wall left of entrance",art.walls[3],new Vector2(-3.85f,-5.10f),new Vector2(6.3f,1.05f));
            Wall("Front wall right of entrance",art.walls[3],new Vector2(4.35f,-5.10f),new Vector2(7.3f,1.05f));
            exitDoor=Prop("Tavern exit",art.walls[2],new Vector2(0,-5.10f),1.45f,room);
            Box("Entrance left jamb footprint",room,new Vector2(-.61f,-4.84f),new Vector2(.17f,.36f));Box("Entrance right jamb footprint",room,new Vector2(.61f,-4.84f),new Vector2(.17f,.36f));
            var carpet=Prop("Woven red tavern carpet",art.props[3],new Vector2(-2.6f,-2.5f),2.65f,room,-1800);
            carpet.transform.localScale=new Vector3(carpet.transform.localScale.x,carpet.transform.localScale.y*.66f,1);carpet.color=new Color(.74f,.70f,.66f);game.art.PixelStyle(carpet);
            var hearthRug=Prop("Small woven hearth carpet",art.props[3],new Vector2(-5.35f,1.5f),1.6f,room,-1800);
            hearthRug.transform.localScale=new Vector3(hearthRug.transform.localScale.x,hearthRug.transform.localScale.y*.55f,1);hearthRug.color=carpet.color;game.art.PixelStyle(hearthRug);
            Prop("Bear porcupine taxidermy hide",art.furniture[3],new Vector2(1.35f,.05f),3.0f,room,-1700);
            fireplace=Prop("Carved stone fireplace",art.props[2],new Vector2(-5.75f,3.15f),1.95f,room);
            Box("Fireplace stone base",room,new Vector2(-5.75f,3.45f),new Vector2(1.7f,.65f));
            hearthFire=Prop("Continuous hearth fire and embers",art.hearth[0],new Vector2(-5.75f,3.48f),1.16f,room,fireplace.sortingOrder+3);
            hearthFire.sharedMaterial=new Material(Shader.Find("AshenCoast/HearthFire"));PoseHearth();
            var counter=Prop("Innkeeper counter with plates",art.furniture[1],new Vector2(-.70f,3.0f),4.8f,room);
            Box("Counter footprint",room,new Vector2(-.70f,3.35f),new Vector2(4.7f,.75f));
            keeper=Actor(1,new Vector2(-.70f,3.65f));
            var upper=new MaterialPropertyBlock();keeper.sprite.GetPropertyBlock(upper);upper.SetFloat("_BodyFloor",.40f);keeper.sprite.SetPropertyBlock(upper);keeper.sprite.sortingOrder=counter.sortingOrder+1;
            Vector2[] tables={new Vector2(-4.70f,-1.80f),new Vector2(0,-2.0f),new Vector2(-3.55f,.45f),new Vector2(1.30f,2.0f)};
            foreach(var p in tables)
            {
                var table=Prop("Round oak table",art.furniture[0],p,1.7f,room);table.transform.localScale=new Vector3(table.transform.localScale.x,table.transform.localScale.y*.82f,1);game.art.PixelStyle(table);
                var t=new GameObject("Circular table footprint");t.transform.SetParent(room,false);t.transform.position=p+Vector2.up*.30f;t.AddComponent<CircleCollider2D>().radius=.63f;
            }
            Actor(0,new Vector2(2.65f,-1.2f));
            Seat(2,tables[0]+new Vector2(-.65f,.02f));
            drinker=Seat(3,tables[1]+new Vector2(.88f,-.08f));
            Seat(4,tables[2]+new Vector2(-.82f,-.03f));Seat(5,tables[2]+new Vector2(.82f,-.03f));
            Prop("Empty side chair",art.chairs[2],tables[3]+new Vector2(1.03f,-.18f),.76f,room);
            Prop("Empty table rear chair",art.chairs[0],tables[3]+new Vector2(0,.50f),.72f,room,CoastGame.Depth(tables[3].y)-8);
            var prep=Prop("Pantry preparation table",art.furniture[2],new Vector2(6.10f,.40f),1.9f,room);
            Box("Preparation table footprint",room,new Vector2(6.10f,.75f),new Vector2(1.65f,.65f));
            var girl=Seat(6,new Vector2(6.13f,1.15f));girl.sprite.sortingOrder=prep.sortingOrder-1;
            Prop("Pantry food shelf",art.props[5],new Vector2(6.10f,3.25f),2.25f,room);Box("Food shelf footprint",room,new Vector2(6.10f,3.55f),new Vector2(2.10f,.6f));
            Prop("Food crates and grain sacks",art.props[6],new Vector2(6.15f,-2.55f),1.9f,room);Box("Crate footprint",room,new Vector2(6.15f,-2.25f),new Vector2(1.7f,.65f));
        }
        SpriteRenderer Wall(string name,Sprite sprite,Vector2 foot,Vector2 size)
        {
            var wall=Prop(name,sprite,foot,size.x,tavernRoot.transform,-1500);
            wall.transform.localScale=new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1);game.art.PixelStyle(wall);return wall;
        }
        TavernActor Seat(int row,Vector2 point)
        {
            var chair=Prop("Occupied oak chair "+row,art.chairs[0],point+Vector2.up*.08f,.95f,tavernRoot.transform,CoastGame.Depth(point.y)-2);
            chair.transform.localScale=new Vector3(chair.transform.localScale.x,chair.transform.localScale.y*.90f,1);game.art.PixelStyle(chair);
            var c=chair.gameObject.AddComponent<CircleCollider2D>();c.radius=.19f/chair.transform.localScale.x;c.offset=new Vector2(0,.10f/chair.transform.localScale.y);
            var actor=Actor(row,point);actor.sprite.sortingOrder=chair.sortingOrder+1;return actor;
        }
        TavernActor Actor(int row,Vector2 point)
        {
            if(row>=2 && row<=6)point+=Vector2.up*.20f;
            var actor=new TavernActor{row=row,position=point,sprite=Prop("Tavern actor "+row,art.people[row,0],point,art.people[row,0].bounds.size.x*(row>=2 && row<=6?.88f:1),tavernRoot.transform)};
            actors.Add(actor);return actor;
        }
        void CreateCart()
        {
            coastRouteY=game.ship.bounds.max.y+.70f;cartRoot=new GameObject("Driver's woodlouse cart");cartRoot.transform.position=new Vector2(5,coastRouteY);cartRoot.transform.SetParent(game.coastRoot.transform,true);
            wagon=Prop("Wooden wagon",art.cart,CartPosition,4.2f,cartRoot.transform);
            driver=Prop("Human driver",art.people[7,0],CartPosition+new Vector2(.85f,1.1f),art.people[7,0].bounds.size.x,cartRoot.transform);
            cartDriver=new CaravanDriver(this,driver);
            for(int i=0;i<2;i++)
            {
                var s=Prop("Harnessed woodlouse "+i,game.art.woodlouse[0],CartPosition+new Vector2(3.8f,i==0?-.72f:.72f),game.art.woodlouse[0].bounds.size.x,cartRoot.transform);pullers.Add(s);caravanAnimals.Add(new CaravanWoodlouse(this,s,i));
                var line=new GameObject("Woodlouse harness").AddComponent<LineRenderer>();line.transform.SetParent(cartRoot.transform,false);line.sharedMaterial=new Material(Shader.Find("Sprites/Default"));line.startWidth=.04f;line.endWidth=.025f;line.positionCount=3;line.startColor=line.endColor=new Color(.36f,.24f,.12f);reins.Add(line);
            }
        }
        void CreateGulls()
        {for(int i=0;i<4;i++){var p=new Vector2(10+i*1.4f,-3.7f+(i%2)*.4f);var s=Prop("Four winged beach gull",art.gulls[0,0],p,.9f,game.coastRoot.transform);s.enabled=false;gulls.Add(new BeachGull{sprite=s,home=p,point=p,phase=i*1.8f,state=4,age=18});}}
        public void SetVisibility(bool pocket)
        {hidden=pocket;game.snails?.Visibility(pocket);game.coastRoot.SetActive(!pocket && IsCoast);fieldRoot.SetActive(!pocket && Area==WorldArea.GoldenField);tavernRoot.SetActive(!pocket && Interior);if(game.fort!=null)game.fort.root.SetActive(!pocket && InFort);if(game.cave!=null)game.cave.root.SetActive(!pocket && InCatacombs);if(game.amazons!=null)game.amazons.roomRoot.SetActive(!pocket && Area==WorldArea.AmazonHouse);if(companion!=null)companion.sprite.gameObject.SetActive(companion.Hired);}
        public void SetArea(WorldArea area,Vector2 at)
        {
            if(game.CoopOnline)game.coop.PublishWorldV49();
            if(area!=WorldArea.SnailHouse && game.snails!=null)game.snails.Inside=-1;
            Area=area;game.ui.CloseContainers();game.arcane.CancelHeldSkills();game.arcane.CancelPlacement();game.magic.CancelStroke();
            game.cameraView.backgroundColor=InFort || InCatacombs?new Color(.025f,.032f,.04f):Interior?new Color(.035f,.025f,.018f):new Color(.10f,.20f,.22f);
            SetVisibility(false);game.body.linearVelocity=Vector2.zero;game.body.position=at;game.hero.transform.position=at;doorCooldown=.8f;game.numbers.Clear();
            if(!Cutscene){game.cameraView.orthographicSize=Interior?6.25f:8;game.ResetCamera(new Vector3(at.x,Interior?.2f:at.y+2.2f,-20));}
            if(companion!=null && companion.Hired && !game.snails.Resting(companion.sprite) && !game.snails.Returning(companion.sprite) && (!game.CoopOnline || game.coop.world.OwnAlly(companion.sprite))){if(companion.remoteArea.HasValue){companion.sprite.gameObject.SetActive(companion.remoteArea.Value==(MapRegion)(int)area);if(companion.remoteArea.Value==(MapRegion)(int)area){companion.remoteArea=null;companion.suppressCatchUp=false;}}else companion.Warp(at+new Vector2(-.8f,-.5f));}game.amazons?.AreaChanged();Physics2D.SyncTransforms();game.UpdateEnvironment(0);
        }
        public Vector2 Clamp(Vector2 p)
        {if(Area==WorldArea.SnailHouse)return game.snails.Clamp(p);if(Area==WorldArea.AmazonHouse)return game.amazons.ClampRoom(p);if(InCatacombs)return game.cave.Clamp(p);if(InFort)return game.fort.Clamp(p);if(Interior)return new Vector2(Mathf.Clamp(p.x,-6.65f,7.65f),Mathf.Clamp(p.y,-4.75f,4.75f));if(!IsCoast)return new Vector2(Mathf.Clamp(p.x,-28,28),Mathf.Clamp(p.y,-17,27));return p;}
        public bool CanInteractAt(Vector2 point)
        {
            if(hidden || Cutscene)return false;
            if(!Interior && CaravanReady && wagon.gameObject.activeInHierarchy && NearCart() && (Hit(wagon,point)||Hit(driver,point)||pullers.Any(p=>Hit(p,point))))return true;
            if(Area==WorldArea.GoldenField && Vector2.Distance(game.body.position,InnEntrance)<2.2f && Hit(inn,point))return true;
            return Interior && ((Vector2.Distance(game.body.position,keeper.position)<3.1f && Hit(keeper.sprite,point)) || (!companion.Hired && Vector2.Distance(game.body.position,drinker.position)<2.5f && Hit(drinker.sprite,point)) || (Vector2.Distance(game.body.position,TavernExit)<2 && Hit(exitDoor,point)));
        }
        public SpriteRenderer InteractionAt(Vector2 point,out string name)
        {
            name=null;if(!CanInteractAt(point))return null;
            if(!Interior && NearCart() && (Hit(wagon,point)||Hit(driver,point)||pullers.Any(p=>Hit(p,point)))){name="Повозка";return wagon;}
            if(Area==WorldArea.GoldenField){name="Войти в таверну";return inn;}
            if(Hit(keeper.sprite,point)){name="Трактирщик · Торговля";return keeper.sprite;}
            if(Hit(drinker.sprite,point) && !companion.Hired){name="Маг · Нанять";return drinker.sprite;}
            if(Hit(exitDoor,point)){name="Выйти";return exitDoor;}return null;
        }
        static bool Hit(SpriteRenderer s,Vector2 p)=>s && s.enabled && s.gameObject.activeInHierarchy && s.bounds.Contains(new Vector3(p.x,p.y,s.bounds.center.z));
        public bool NearCart()=>Vector2.Distance(game.body.position,CartPosition+new Vector2(1,.6f))<3.6f;
        public bool Interact(Vector2 point)
        {
            if(!CanInteractAt(point))return false;
            if(TavernAlarm && Interior && !Hit(exitDoor,point))return false;
            if(!Interior && NearCart() && (Hit(wagon,point)||Hit(driver,point)||pullers.Any(p=>Hit(p,point))))return BeginRide();
            if(Area==WorldArea.GoldenField){game.interactions.Pulse(inn);SetArea(WorldArea.Tavern,new Vector2(0,-3.75f));return true;}
            if(Hit(keeper.sprite,point)){game.ui.OpenShop();return true;}
            if(Hit(drinker.sprite,point) && !companion.Hired){HireOffer=true;return true;}
            if(Hit(exitDoor,point)){SetArea(WorldArea.GoldenField,InnEntrance+Vector2.down*.8f);return true;}return false;
        }
        public bool HireMage()
        {if(game.snails.SelectHire("mage",()=>{if(!HireMage())game.ui.Notice("Недостаточно монет");}))return true;if(game.CoopOnline && !game.coop.ApplyingNetwork)return game.coop.RequestHireV49("mage");if(companion.Hired || !Interior || Vector2.Distance(game.body.position,drinker.position)>3 || !game.survivor.SpendCoins(4))return false;HireOffer=false;drinker.sprite.enabled=false;companion.Hire(drinker.position);game.PlayPickup();return true;}
        public bool BeginRide()
        {
            if(Cutscene || hidden || Interior || !NearCart() || !CaravanReady)return false;
            destination=IsCoast?WorldArea.GoldenField:WorldArea.Coast;ridePhase=1;phaseTime=0;boardingStart=game.body.position;cartStart=CartPosition;
            CutsceneCamera=game.cameraView.transform.position;game.ui.CloseContainers();game.magic.CancelStroke();game.arcane.CancelPlacement();game.combat.ResetForCheck();game.body.simulated=false;return true;
        }
        void Update(){if(game==null || game.paused || game.testMode)return;Tick(Time.deltaTime);}
        public void Tick(float dt)
        {
            clock+=dt;doorCooldown=Mathf.Max(0,doorCooldown-dt);
            TickCaravan(dt);if(Cutscene)TickRide(dt);
            else if(!hidden)
            {
                if(IsCoast && CaravanReady && !NearCart()){cartRoot.transform.position+=Vector3.right*dt*.8f;if(CartPosition.x>28)cartRoot.transform.position=new Vector2(-25,coastRouteY);}
                if(doorCooldown<=0 && !game.magic.Teleporting)
                {
                    if(Area==WorldArea.GoldenField && Vector2.Distance(game.body.position,InnEntrance)<.6f){game.interactions.Pulse(inn);SetArea(WorldArea.Tavern,new Vector2(0,-3.75f));}
                    else if(Interior && Vector2.Distance(game.body.position,TavernExit)<.42f){game.interactions.Pulse(exitDoor);SetArea(WorldArea.GoldenField,InnEntrance+Vector2.down*.9f);}
                }
            }
            PoseCart();if(TavernAlarm){if(Interior)foreach(var r in residents)game.TickCoopActorV49(r.Position,()=>r.Tick(dt));PoseHearth();}else if(game.SimulateWorldV49)TickActors();TickTrees(dt);if(game.SimulateWorldV49)AnimateGulls(dt);game.TickCoopActorV49(companion.Position,()=>companion.Tick(dt),companion.sprite);
            if(HireOffer && (!Interior || Vector2.Distance(game.body.position,drinker.position)>3.4f))HireOffer=false;
        }
        void TickRide(float dt)
        {
            phaseTime+=dt;var seat=CartPosition+new Vector2(-.35f,1.05f);
            if(ridePhase==1 || ridePhase==5){game.hero.sprite=game.art.hero[6,Mathf.FloorToInt(phaseTime*12)%8];game.hero.flipX=true;game.art.PixelStyle(game.hero);}
            if(ridePhase==1)
            {game.hero.transform.position=Vector2.Lerp(boardingStart,seat,Mathf.SmoothStep(0,1,Mathf.Clamp01(phaseTime/1.2f)));game.facing=6;if(phaseTime>=1.2f){ridePhase=2;phaseTime=0;}}
            else if(ridePhase==2)
            {cartRoot.transform.position+=Vector3.right*dt*6;game.hero.transform.position=CartPosition+new Vector2(-.35f,1.05f);BlackFade=Mathf.Clamp01((phaseTime-3.4f)/.7f);if(phaseTime>=4.1f){ridePhase=3;phaseTime=0;}}
            else if(ridePhase==3)
            {
                BlackFade=1;if(phaseTime>=.85f){float y=RouteY(destination);SetArea(destination,new Vector2(0,y));cartRoot.transform.SetParent(ActiveRoot,true);cartRoot.transform.position=new Vector2(-22,y);game.hero.transform.position=CartPosition+new Vector2(-.35f,1.05f);CutsceneCamera=new Vector3(0,y+2.25f,-20);game.cameraView.orthographicSize=8;game.ResetCamera(CutsceneCamera);ridePhase=4;phaseTime=0;}
            }
            else if(ridePhase==4)
            {float y=RouteY(destination);BlackFade=Mathf.Max(0,1-phaseTime/.65f);cartRoot.transform.position=Vector2.Lerp(new Vector2(-22,y),new Vector2(3,y),Mathf.SmoothStep(0,1,Mathf.Clamp01(phaseTime/4.0f)));game.hero.transform.position=CartPosition+new Vector2(-.35f,1.05f);if(phaseTime>=4){ridePhase=5;phaseTime=0;dismountStart=game.hero.transform.position;}}
            else if(ridePhase==5)
            {game.hero.transform.position=Vector2.Lerp(dismountStart,CartPosition+new Vector2(-.5f,-.85f),Mathf.SmoothStep(0,1,Mathf.Clamp01(phaseTime/.9f)));if(phaseTime>=.9f){ridePhase=0;BlackFade=0;game.body.position=game.hero.transform.position;game.body.simulated=true;game.body.linearVelocity=Vector2.zero;game.ResetCamera(CutsceneCamera);if(companion.Hired && (!game.CoopOnline || game.coop.world.OwnAlly(companion.sprite)))companion.Warp(game.body.position+Vector2.left);}}
        }
        void PoseCart()
        {
            bool moving=CaravanMoving;float bob=moving?Mathf.Sin(clock*12)*.025f:0;
            wagon.transform.localPosition=new Vector3(0,bob,0);wagon.sortingOrder=CoastGame.Depth(CartPosition.y)+5;cartDriver.Pose(bob);
            for(int i=0;i<2;i++)
            {
                var s=pullers[i];caravanAnimals[i].Pose(moving);reins[i].sortingOrder=s.sortingOrder-1;reins[i].positionCount=caravanAnimals[i].Alive && !cartDriver.Fleeing?3:0;
                if(reins[i].positionCount>0)reins[i].SetPositions(new[]{cartRoot.transform.position+new Vector3(1.5f,.9f),cartRoot.transform.position+new Vector3(2.6f,.55f),s.transform.position+new Vector3(-.5f,.4f)});
            }
        }
        void TickActors()
        {
            foreach(var a in actors)
            {
                a.frame=ActorFrame(a.row,clock);
                int gestureRow=a.row==0?0:a.row==4?1:a.row==5?2:-1;
                a.sprite.sprite=a.row==1?art.keeper[a.frame]:gestureRow>=0?art.gestures[gestureRow,a.frame]:a.row==3?art.mage[4,a.frame]:art.people[a.row,a.frame];
                // Feet stay registered to the chair/floor; gestures pause and return through their in-betweens.
                a.sprite.transform.position=a.position+(a.row==0?new Vector2(Mathf.Sin(clock*Mathf.PI*1.6f)*.065f,0):Vector2.zero);
                game.art.PixelStyle(a.sprite);
                if(a.row==0){if(a.gait==null)a.gait=new PixelGait(game.art,a.sprite,art.gestures,.43f);a.gait.Pose(0,clock/1.8f,Mathf.Repeat(clock,5)<3.6f,Vector2.down);}
            }
            PoseHearth();
        }
        void PoseHearth()
        {
            float phase=Mathf.Repeat(clock/HearthCycleSeconds,1)*16;int a=Mathf.FloorToInt(phase),b=(a+1)%16;
            var t=art.hearth[0].texture;Vector4 UV(Sprite s)=>new Vector4(s.rect.x/t.width,s.rect.y/t.height,s.rect.width/t.width,s.rect.height/t.height);
            Vector2 Pivot(Sprite s)=>new Vector2(s.pivot.x/s.rect.width,s.pivot.y/s.rect.height);
            var basePivot=Pivot(art.hearth[0]);hearthFire.GetPropertyBlock(properties);
            properties.SetVector("_SpriteRect",UV(art.hearth[0]));properties.SetVector("_FrameA",UV(art.hearth[a]));properties.SetVector("_FrameB",UV(art.hearth[b]));
            properties.SetVector("_ShiftA",Pivot(art.hearth[a])-basePivot);properties.SetVector("_ShiftB",Pivot(art.hearth[b])-basePivot);
            properties.SetVector("_Pixels",new Vector4(46,42,0,0));properties.SetFloat("_Blend",Mathf.SmoothStep(0,1,phase-a));properties.SetFloat("_HearthClock",clock/HearthCycleSeconds*Mathf.PI*2);hearthFire.SetPropertyBlock(properties);
        }
        static int Sequence(float t,float seconds,int[] poses)
        {return poses[Mathf.Min(poses.Length-1,Mathf.FloorToInt(Mathf.Repeat(t,seconds)/seconds*poses.Length))];}
        public static int ActorFrame(int row,float t)
        {
            float phase=Mathf.Repeat(t,5);
            if(row==1)return Mathf.FloorToInt(Mathf.Repeat(t,3)/3*24);
            if(row==0)
            {
                // Alternate dance contacts without jumping from a lifted arm to an unrelated pose.
                if(phase<3.6f)return Sequence(phase,1.8f,new[]{0,1,2,3,4,5,4,3,2,1});
                return new[]{6,7,8,8,9,9,10,11}[Mathf.Min(7,Mathf.FloorToInt((phase-3.6f)/1.4f*8))];
            }
            if(row==3)return phase<3.6f?0:new[]{1,2,3,3,3,3,2,1,0,0}[Mathf.Min(9,Mathf.FloorToInt((phase-3.6f)/1.4f*10))];
            if(row==4)return Sequence(t,7.2f,new[]{0,0,1,3,4,5,6,6,5,4,3,1,7,8,9,10,11,0,0,0,0,0,0,0});
            if(row==5)return Sequence(t+3.2f,7.2f,new[]{0,0,1,2,3,4,5,6,7,8,9,10,11,0,0,0,0,0,0,0,0,0,0,0});
            if(row==2)return Sequence(t,4,new[]{0,1,2,3,4,5,6,7,6,5,4,3,2,1});
            return Sequence(t,2.4f,new[]{0,1,2,3,4,5,6,7,6,5,4,3,2,1});
        }
        void TickTrees(float dt)
        {
            foreach(var t in trees)
            {var p=game.body.position;bool behind=!hidden && Area==WorldArea.GoldenField && p.y>t.position.y+.12f && p.y<t.sprite.bounds.max.y && Mathf.Abs(p.x-t.position.x)<t.sprite.bounds.extents.x*.75f;t.alpha=Mathf.MoveTowards(t.alpha,behind?.28f:1,dt*3);t.canopy.GetPropertyBlock(properties);properties.SetFloat("_OcclusionAlpha",t.alpha);t.canopy.SetPropertyBlock(properties);}
        }
        void TickGulls()
        {
            foreach(var g in gulls)
            {float cycle=Mathf.Repeat(clock+g.phase,46);bool scared=Vector2.Distance(game.body.position,g.home)<1.6f;int row=cycle<3?0:cycle<17 && !scared?1:2;g.frame=row==1?Mathf.FloorToInt(clock*5)%8:Mathf.Min(7,Mathf.FloorToInt((row==0?cycle:Mathf.Max(0,cycle-17))*3));g.sprite.sprite=art.gulls[row,g.frame];g.sprite.enabled=IsCoast && !hidden && cycle<20;float elevation=row==0?(3-cycle)*1.2f:row==2?Mathf.Max(0,cycle-17)*1.4f:0;g.sprite.transform.position=g.home+new Vector2(row==1?Mathf.Sin(clock*.7f+g.phase)*.25f:elevation*.35f,elevation);g.sprite.sortingOrder=CoastGame.Depth(g.home.y)+3;game.art.PixelStyle(g.sprite);}
        }
        public int AppendLights(Vector4[] p,Vector4[] c,int n)
        {if(!hidden && Area==WorldArea.GoldenField && n<p.Length){p[n]=new Vector4(InnEntrance.x,InnEntrance.y+.5f,5.4f,.85f);c[n++]=new Vector4(1,.49f,.19f,1);}if(!hidden && Interior && n<p.Length){p[n]=new Vector4(-5.75f,3.48f,4.8f,.95f+.045f*Mathf.Sin(clock*2.2f)+.028f*Mathf.Sin(clock*3.7f));c[n++]=new Vector4(1,.56f,.24f,1);}if(companion.Hired && companion.Alive && n<p.Length){p[n]=new Vector4(companion.Position.x,companion.Position.y+.8f,2.6f,.6f+.1f*Mathf.Sin(clock*9));c[n++]=new Vector4(1,.52f,.17f,1);}return n;}
        void OnGUI()
        {if(BlackFade<=0)return;GUI.depth=-100;GUI.color=new Color(0,0,0,BlackFade);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;}
    }
    public sealed partial class CompanionMage
    {
        public readonly SpriteRenderer sprite;
        public bool Hired {get;private set;}
        public const float MaxHealth=120,MaxMana=100;
        const float FollowSpeed=CoastGame.WalkSpeed+.18f;
        public float health=MaxHealth,mana=MaxMana;
        public bool Alive=>health>0;
        public Vector2 Position=>(Vector2)sprite.transform.position;
        public int frame,direction;
        public bool suppressCatchUp;
        public MapRegion? remoteArea;
        public PixelGait gait;
        bool walking;Vector2 walkDirection;
        public bool Firing {get;private set;}
        public SpriteRenderer target;
        readonly JourneyWorld world;readonly CoastGame game;float clock,distance,emission,targetUntil;
        readonly List<Vector2> route=new List<Vector2>();int routeIndex;float repath;Vector2 routeGoal;
        public CompanionMage(JourneyWorld owner)
        {world=owner;game=world.game;sprite=game.art.Renderer("Hired human lamp mage",world.art.mage[0,0],Vector2.zero,2000);gait=new PixelGait(game.art,sprite,world.art.mage,.21f);sprite.gameObject.SetActive(false);}
        public void Hire(Vector2 at){Hired=true;sprite.gameObject.SetActive(true);Warp(at+new Vector2(.95f,-.25f));}
        public void Warp(Vector2 at){sprite.transform.position=FindFree(at);target=null;Firing=false;distance=0;walking=false;frame=3;route.Clear();repath=0;Pose();}
        Vector2 FindFree(Vector2 at)
        {
            if(!Solid(at))return at;
            for(float radius=.4f;radius<=2.4f;radius+=.3f)for(int i=0;i<12;i++){float a=i*Mathf.PI/6;var p=at+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;if(!Solid(p))return p;}
            return at;
        }
        bool Solid(Vector2 p)=>Physics2D.OverlapCircleAll(p+Vector2.up*.13f,.22f).Any(c=>!c.isTrigger && c.gameObject!=game.hero.gameObject);
        public void Engage(SpriteRenderer enemy){if(enemy && enemy.gameObject.activeInHierarchy){target=enemy;targetUntil=clock+12;}}
        public void Tick(float dt)
        {
            if(game.snails!=null && (game.snails.Resting(sprite) || game.snails.Returning(sprite)))return;if(!Hired || !Alive || world.Cutscene || game.spectrum!=null && game.spectrum.Riding || remoteArea.HasValue && remoteArea.Value!=game.map.CurrentRegion)return;clock+=dt;
            if(target && (!target.enabled || !target.gameObject.activeInHierarchy || clock>targetUntil))target=null;
            if(!target && world.Interior && world.TavernAlarm)target=world.residents.Where(r=>r.Alive && !r.Flee && !r.Escaped).Select(r=>r.Renderer).FirstOrDefault();
            if(!target && world.InCatacombs)target=game.cave.Targets.Where(t=>t.Alive && Vector2.Distance(t.Position,Position)<8).Select(t=>t.Renderer).FirstOrDefault();
            if(!target && world.InFort)target=game.fort.bandits.Where(b=>b.Alive && b.Alert && Vector2.Distance(b.Position,Position)<8).Select(b=>b.sprite).FirstOrDefault();
            if(!target && world.IsCoast && !game.magic.InPocket)target=game.arcane.frogs.Where(f=>f.Alive && f.Hostile && Vector2.Distance(f.Position,Position)<7).Select(f=>f.renderer).FirstOrDefault();
            Vector2 delta=game.body.position-Position;Firing=target && mana>0 && Vector2.Distance(target.transform.position,Position)<=7;
            bool unobstructed=ClearLine(Position,game.body.position);
            if(delta.magnitude>1.5f || ((world.Interior || world.InFort || world.InCatacombs) && !unobstructed))
            {
                Vector2 move=(world.Interior || world.InFort || world.InCatacombs) && !unobstructed?FollowRoute(game.body.position,dt):Steer(delta.normalized,dt*FollowSpeed);sprite.transform.position+=(Vector3)move;distance+=move.magnitude;frame=Mathf.FloorToInt(distance/1.65f*8)%8;walking=move.sqrMagnitude>.00001f;walkDirection=move;if(walking)direction=Facing(move);
                if(delta.magnitude>16 && !suppressCatchUp)Warp(game.body.position+new Vector2(-1,-.4f));
            }
            else {frame=3;walking=false;}
            if(Firing)
            {var aim=(Vector2)target.bounds.center-Position-Vector2.up*.8f;direction=Facing(aim);mana=Mathf.Max(0,mana-dt*2.5f);emission+=dt;while(emission>=.035f){emission-=.035f;game.arcane.EmitCone(Position+Vector2.up*.8f,aim,false,3,7,45,target);}}
            else emission=0;Pose();
        }
        Vector2 Steer(Vector2 seek,float step)
        {
            Vector2 best=Vector2.zero;float score=-999;float[] turns={0,30,-30,60,-60,90,-90,120,-120};
            foreach(float a in turns){var d=(Vector2)(Quaternion.Euler(0,0,a)*seek);bool solid=Physics2D.CircleCastAll(Position+Vector2.up*.13f,.21f,d,Mathf.Max(.40f,step)).Any(h=>!h.collider.isTrigger && h.collider.gameObject!=game.hero.gameObject);if(solid)continue;float s=Vector2.Dot(d,seek)-Mathf.Abs(a)*.001f;if(s>score){score=s;best=d;}}
            return best*step;
        }
        bool ClearLine(Vector2 from,Vector2 to)
        {var d=to-from;return d.sqrMagnitude<.0001f || !Physics2D.CircleCastAll(from+Vector2.up*.13f,.21f,d.normalized,d.magnitude).Any(h=>!h.collider.isTrigger && h.collider.gameObject!=game.hero.gameObject);}
        static Vector2 NodePoint(Vector2Int cell)=>new Vector2(cell.x*.5f,cell.y*.5f);
        bool FreeNode(Vector2Int cell)=>(world.InCatacombs?game.cave.Walkable(NodePoint(cell),.23f):world.InFort?cell.x>=-38 && cell.x<=38 && cell.y>=-28 && cell.y<=32:cell.x>=-13 && cell.x<=15 && cell.y>=-9 && cell.y<=9) && !Solid(NodePoint(cell));
        Vector2Int NearestNode(Vector2 point)
        {
            var seed=new Vector2Int(Mathf.RoundToInt(point.x*2),Mathf.RoundToInt(point.y*2));var best=seed;float score=float.MaxValue;
            for(int y=-2;y<=2;y++)for(int x=-2;x<=2;x++){var n=seed+new Vector2Int(x,y);if(!FreeNode(n) || !ClearLine(point,NodePoint(n)))continue;float d=(point-NodePoint(n)).sqrMagnitude;if(d<score){score=d;best=n;}}return best;
        }
        Vector2 FollowRoute(Vector2 goal,float dt)
        {
            repath-=dt;
            if(route.Count==0 || (routeIndex>=route.Count && repath<=0) || (goal-routeGoal).sqrMagnitude>.5f){BuildRoute(goal);repath=.65f;routeGoal=goal;}
            while(routeIndex<route.Count && Vector2.Distance(Position,route[routeIndex])<.12f)routeIndex++;
            if(routeIndex>=route.Count)return Steer((goal-Position).normalized,dt*FollowSpeed);
            Vector2 d=route[routeIndex]-Position,move=d.normalized*Mathf.Min(dt*FollowSpeed,d.magnitude);
            if(!ClearLine(Position,Position+move)){route.Clear();repath=0;return Vector2.zero;}return move;
        }
        void BuildRoute(Vector2 goal)
        {
            route.Clear();routeIndex=0;var start=NearestNode(Position);var end=NearestNode(goal);
            var open=new List<Vector2Int>{start};var cost=new Dictionary<Vector2Int,float>{{start,0}};var previous=new Dictionary<Vector2Int,Vector2Int>();var visited=new HashSet<Vector2Int>();
            for(int attempts=0;open.Count>0 && attempts<(world.InFort || world.InCatacombs?5000:800);attempts++)
            {
                var current=open.OrderBy(n=>cost[n]+Vector2.Distance(NodePoint(n),NodePoint(end))).First();open.Remove(current);
                if(current==end){var n=end;route.Add(NodePoint(n));while(previous.TryGetValue(n,out var parent)){n=parent;route.Add(NodePoint(n));}route.Reverse();return;}
                visited.Add(current);
                for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++)
                {if(x==0 && y==0)continue;var next=current+new Vector2Int(x,y);if(visited.Contains(next) || !FreeNode(next) || !ClearLine(NodePoint(current),NodePoint(next)))continue;float proposed=cost[current]+Vector2.Distance(NodePoint(current),NodePoint(next));if(cost.TryGetValue(next,out float old) && old<=proposed)continue;cost[next]=proposed;previous[next]=current;if(!open.Contains(next))open.Add(next);}
            }
        }
        static int Facing(Vector2 p)=>Mathf.Abs(p.x)>Mathf.Abs(p.y)?p.x<0?1:3:p.y>0?2:0;
        void Pose(){sprite.sprite=world.art.mage[direction,frame];sprite.sortingOrder=CoastGame.Depth(Position.y);game.art.PixelStyle(sprite);gait.Pose(direction,distance/1.65f,walking,walkDirection);}
        public bool Explode()
        {if(!Hired || !Alive || mana<40 || world.Cutscene)return false;mana-=40;var blast=game.arcane.CreateExplosion(Position);blast.playerOwned=false;game.PlaySwish();return true;}
        public void Hurt(float damage){if(!Hired || !Alive)return;health=Mathf.Max(0,health-damage);game.ShowDamage(damage,Position+Vector2.up*1.8f,sprite);if(!Alive){sprite.enabled=false;Firing=false;walking=false;Pose();}}
    }
}
