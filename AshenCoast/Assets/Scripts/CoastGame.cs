using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AshenCoast
{
    public sealed class Wildflower
    {
        public Vector2 position;
        public ItemKind kind;
        public SpriteRenderer renderer, shadow;
        public MeshRenderer stems;
        public float seed;
        public bool picked;
        public bool procedural;
        public long proceduralId;
    }
    public sealed class Firefly
    {
        public Vector2 home;
        public SpriteRenderer dot;
        public float seed;
        public bool procedural;
    }
    public sealed class DamageNumber
    {
        public Vector2 position;
        public float damage;
        public float age;
        string cachedLabel;
        public string Label=>cachedLabel??(cachedLabel=damage.ToString("0.#"));
    }

    public sealed partial class CoastGame : MonoBehaviour
    {
        public const float WalkSpeed = 3.62f;
        public const float AttackWalkSpeed = 1.55f;
        public bool FastMovement {get;private set;}
        public float BaseMovementSpeed=>(combat.Attacking || combat.BowDrawingV58?AttackWalkSpeed:WalkSpeed)*(FastMovement?5:1)*(cave?.SlowMultiplier??1);
        public float MovementSpeed=>BaseMovementSpeed*(survivor?.Skates!=null && survivor.WearingBootsV54?1+rollers.Bonus:1);
        public void ToggleFastMovement(){FastMovement=!FastMovement;ui.Notice(FastMovement?"Скорость: ×5":"Обычная скорость");}
        public void StartNight()
        {
            float cycle=Mathf.Floor(DaySeconds/360)*360;
            seconds=cycle+210+(DaySeconds>cycle+210?360:0)-(meditation?.DayOffset??0);
            coop?.ClockChanged();UpdateEnvironment(0);hunter.Tick(0);ui.Notice("Наступила ночь");
        }
        [NonSerialized] public ArtBank art;
        [NonSerialized] public Survivor survivor;
        [NonSerialized] public GridInventory shipStorage;
        public Camera cameraView;
        public PixelAtmosphere pixelView;
        public InventoryUI ui;
        public Combat combat;
        public SpatialMagic magic;
        public ArcaneCombat arcane;
        public JourneyWorld journey;
        public ProceduralMeadow meadow;
        public ExplorationMap map;
        public FortWorld fort;
        public CaveDungeon cave;
        public bool WorldBlocked(Vector2 from,Vector2 to,float radius=.23f)=>(amazons!=null && amazons.Blocked(from,to,radius)) || (fort!=null && fort.Inside?fort.Blocked(from,to,radius):cave!=null && cave.Inside && cave.Blocked(from,to,radius));
        public NightHunter hunter;
        public CombatFeedback feedback;
        public InteractionFeedback interactions;
        public PrismWeapon prism;
        public DragonRitual dragons;
        public BuildingSystem buildings;
        public Meditation meditation;
        public bool Meditating=>meditation!=null && meditation.Active;
        public float DaySeconds=>seconds+(meditation?.DayOffset??0);
        public bool Dead=>survivor.health<=0;
        public float ShoreAudibility=>magic.InPocket || !journey.IsCoast?0:1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(9,42,WorldShape.Distance(body.position)));
        public IEnumerable<ICombatTarget> AdditionalTargets
        {get{if(dragons!=null)foreach(var dragon in dragons.bosses)if(dragon.Active && dragon.Alive)yield return dragon;if(magic.InPocket)yield break;if(journey!=null){foreach(var resident in journey.TavernTargets)yield return resident;foreach(var traveller in journey.CaravanTargets)yield return traveller;}if(cave!=null && cave.Inside)foreach(var t in cave.Targets)yield return t;if(fort!=null && fort.Inside)foreach(var b in fort.bandits)if(b.Alive)yield return b;if(hunter!=null && hunter.Active && hunter.Alive)yield return hunter;if(amazons!=null){foreach(var actor in amazons.Targets)yield return actor;if(journey.IsCoast && amazons.plains!=null)foreach(var actor in amazons.plains.Targets)yield return actor;}}}
        public void HurtPlayer(float damage)
        {if(CoopOnline && coop.RedirectPlayerHurt(damage))return;if(survivor.InfiniteVitalsV58)return;if(Dead || combat!=null && combat.ZenInvulnerable || spectrum!=null && spectrum.Riding || spellcraft!=null && spellcraft.Teleporting)return;float applied=Mathf.Min(survivor.health,damage);survivor.health=Mathf.Max(0,survivor.health-damage);if(survivor.health<20)meditation?.Stop();survivor.equipment[(int)GearSlot.Body]?.Wear(.18f);ShowDamage(applied,body.position+Vector2.up*1.5f,hero,true);if(Dead){body.linearVelocity=Vector2.zero;movement=Vector2.zero;ui.CloseContainers();combat.ResetForCheck();magic.CancelStroke();arcane.CancelHeldSkills();arcane.CancelPlacement();}}
        public GroundLoot CreateFortLoot(Vector2 at)=>ScatterLoot(new Item(ItemKind.Coin){count=UnityEngine.Random.Range(1,4)},at,at);
        public GameObject coastRoot;
        public SpriteRenderer hero, ship, dummy, heroShadow;
        public Rigidbody2D body;
        public PolygonCollider2D hull;
        public readonly List<Wildflower> flowers = new List<Wildflower>();
        public readonly List<Firefly> fireflies = new List<Firefly>();
        public readonly List<DamageNumber> numbers = new List<DamageNumber>();
        public readonly List<Woodlouse> woodlice=new List<Woodlouse>();
        public readonly List<GroundLoot> loot=new List<GroundLoot>();
        public readonly List<Vector4> flowerPatches=new List<Vector4>();
        public Wildflower hoveredFlower;
        public GroundLoot hoveredLoot;
        public string HoverName=>interactions?.Name ?? (hoveredFlower!=null && !hoveredFlower.picked?ItemDefinition.All[hoveredFlower.kind].name:hoveredLoot!=null && !hoveredLoot.collected?hoveredLoot.item.Definition.name:null);
        public Soundscape soundscape;
        public static readonly float[] LightningTimes={3.1f,7.6f,12.3f,17.2f};
        public float lastThunderAt=-1;
        public int RightClickConsumedFrame=-1;
        public float seconds, flash, dummyHealth = 500, dummyHitFlash;
        public bool paused, testMode, muted;
        public Spellcraft spellcraft;public SpectrumWeapon spectrum;public CoastArtV37 v37Art;public CoastArtV38 v38Art;
        public bool MovementLocked=>MountedCrossbowV58!=null || Dead || (spellcraft!=null && spellcraft.Teleporting) || (spectrum!=null && spectrum.Riding) || magic.Teleporting || (journey!=null && journey.Cutscene);
        public bool FeetLocked=>MovementLocked || (cave!=null && cave.Snared);
        public int facing;
        public float walkTime;
        public int walkFrame=1;
        public bool walking;
        public PixelGait gait;
        public Vector2 movement;
        [NonSerialized] public Vector2? checkMovement;
        [NonSerialized] public float? checkCameraSize;
        [NonSerialized] public Vector3? checkCameraPosition;
        public Vector2 ShipBase = new Vector2(-7.7f,-1.15f), DummyBase = new Vector2(1.45f,-1.4f);
        public bool ShipReachable => (journey==null || journey.IsCoast) && (magic==null || !magic.InPocket) && hull!=null && hero!=null && Vector2.Distance(hero.transform.position,hull.ClosestPoint(hero.transform.position)) < 2.1f;
        public Vector2 CursorWorld => cameraView.ViewportToWorldPoint(new Vector3(Input.mousePosition.x/Screen.width,Input.mousePosition.y/Screen.height,20));
        public bool CanOpenShipAt(Vector2 point)=>ShipReachable && art.Opaque(ship.sprite,ship.transform.InverseTransformPoint(point));
        public bool TryOpenShipAt(Vector2 point)
        {if(!CanOpenShipAt(point))return false;ui.OpenShip();return true;}
        public static int Depth(float y) => 2000 - Mathf.RoundToInt(y * 30);
        internal AudioSource oceanAudio, rainAudio, sounds;
        internal AudioClip thunder;
        AudioClip swish,pickup,impact;
        Sprite shadowSprite, dotSprite;
        float nextPrint, dummyRespawn;
        Vector2 lastWalkPosition;
        readonly List<Tuple<SpriteRenderer,float>> prints = new List<Tuple<SpriteRenderer,float>>();
        MaterialPropertyBlock hoverBlock;
        CoastEnvironment environment;
        CoastCamera cameraRig;

        void OnDestroy(){feedback?.Dispose();}
        void Awake()
        {
            hoverBlock=new MaterialPropertyBlock();AutoPickup=PlayerPrefs.GetInt("AutoPickup",0)==1;
            testMode=Environment.GetCommandLineArgs().Contains("-sync-check") || Environment.GetCommandLineArgs().Contains("-v54-check") || Environment.GetCommandLineArgs().Contains("-polish-check") || Environment.GetCommandLineArgs().Contains("-frog-check") || Environment.GetCommandLineArgs().Contains("-v51-check") || Environment.GetCommandLineArgs().Contains("-coast-test") || Environment.GetCommandLineArgs().Contains("-snail-check") || Environment.GetCommandLineArgs().Contains("-lan-native-check");
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;
            art=new ArtBank();newArt=new CoastArtV34(art);v43Art=new CoastArtV43(art); survivor=new Survivor(); shipStorage=CoastTime.CreateShipStorage();
            var cam = new GameObject("Orthographic pixel camera"); cameraView=cam.AddComponent<Camera>();
            cameraView.orthographic=true; cameraView.orthographicSize=8; cameraView.clearFlags=CameraClearFlags.SolidColor;
            cameraView.backgroundColor=new Color(.10f,.20f,.22f); cameraView.nearClipPlane=.1f; cameraView.farClipPlane=100;
            cameraView.transform.position=new Vector3(-1.9f,.55f,-20);
            cam.AddComponent<AudioListener>(); pixelView=cam.AddComponent<PixelAtmosphere>(); pixelView.game=this;
            cameraRig=new CoastCamera(this);environment=new CoastEnvironment(this);
            var displayObject=new GameObject("Pixel scene presentation camera");
            var display=displayObject.AddComponent<Camera>();display.depth=10;display.cullingMask=0;display.clearFlags=CameraClearFlags.SolidColor;display.backgroundColor=Color.black;
            displayObject.AddComponent<PixelPresenter>().source=pixelView;
            shadowSprite=MakeEllipse(48,18); dotSprite=MakeDot();
            CreateWorld(); CreateAudio();feedback=new CombatFeedback(this);interactions=new InteractionFeedback(this);
            lastWalkPosition=hero.transform.position;
            combat=gameObject.AddComponent<Combat>(); combat.Initialize(this);
            ui=gameObject.AddComponent<InventoryUI>(); ui.game=this;
            arcane=gameObject.AddComponent<ArcaneCombat>();arcane.Initialize(this);
            magic=gameObject.AddComponent<SpatialMagic>();magic.Initialize(this);
            journey=gameObject.AddComponent<JourneyWorld>();journey.Initialize(this);
            meadow=gameObject.AddComponent<ProceduralMeadow>();meadow.Initialize(this);
            fort=gameObject.AddComponent<FortWorld>();fort.Initialize(this);
            hunter=gameObject.AddComponent<NightHunter>();hunter.Initialize(this,fort.art);
            cave=gameObject.AddComponent<CaveDungeon>();cave.Initialize(this);
            map=gameObject.AddComponent<ExplorationMap>();map.Initialize(this);
            gait=new PixelGait(art,hero,art.hero,.43f);
            gait.Pose(0,0,false,Vector2.down);
            soundscape=gameObject.AddComponent<Soundscape>();soundscape.Initialize(this);
            dragons=gameObject.AddComponent<DragonRitual>();dragons.Initialize(this);
            prism=gameObject.AddComponent<PrismWeapon>();prism.Initialize(this);
            buildings=gameObject.AddComponent<BuildingSystem>();buildings.Initialize(this);
            meditation=new Meditation(this);
            v37Art=new CoastArtV37(art);v38Art=new CoastArtV38(art);CoastArtV43.ApplyBooks(art);v44Art=new CoastArtV44(art);v44=gameObject.AddComponent<CombatStatusV44>();v44.Initialize(this);spellcraft=gameObject.AddComponent<Spellcraft>();spellcraft.Initialize(this);spectrum=gameObject.AddComponent<SpectrumWeapon>();spectrum.Initialize(this);amazons=gameObject.AddComponent<AmazonSettlementV46>();amazons.Initialize(this);
            LoadArtV58();v51Art=new CharacterArtV51(art);snails=gameObject.AddComponent<SnailHomesV50>();snails.Initialize(this);coop=gameObject.AddComponent<CoopSessionV49>();coop.Initialize(this);corpses=gameObject.AddComponent<CorpseWorldV51>();corpses.Initialize(this);InitializeV58();if(snails.SetupOpen){paused=true;Time.timeScale=0;}
            if(Environment.GetCommandLineArgs().Contains("-v58-network-check"))gameObject.AddComponent<ChecksNetworkV58>().game=this;else if(Environment.GetCommandLineArgs().Contains("-v58-check"))gameObject.AddComponent<ChecksV58>().game=this;else if(Environment.GetCommandLineArgs().Contains("-motion-check"))gameObject.AddComponent<ChecksMotionV57>().game=this;else if(Environment.GetCommandLineArgs().Contains("-seed-check"))gameObject.AddComponent<ChecksWorldSeedV56>().game=this;else if(Environment.GetCommandLineArgs().Contains("-sync-check"))gameObject.AddComponent<ChecksSyncV55>().game=this;else if(Environment.GetCommandLineArgs().Contains("-v54-check"))gameObject.AddComponent<ChecksV54>().game=this;else if(Environment.GetCommandLineArgs().Contains("-polish-check"))gameObject.AddComponent<PolishChecksV53>().game=this;else if(Environment.GetCommandLineArgs().Contains("-frog-check"))gameObject.AddComponent<FrogChecksV52>().game=this;else if(Environment.GetCommandLineArgs().Contains("-v51-check"))gameObject.AddComponent<ChecksV51>().game=this;else if(Environment.GetCommandLineArgs().Contains("-snail-check"))gameObject.AddComponent<SnailChecksV50>().game=this;else if(Environment.GetCommandLineArgs().Contains("-lan-native-check"))gameObject.AddComponent<LanNativeChecksV49>().game=this;else if(testMode)gameObject.AddComponent<RuntimeChecks>().game=this;
        }




        void Update()
        {
            if(snails!=null && snails.SetupOpen)return;
            if(Input.GetKeyDown(KeyCode.F11) || (Input.GetKeyDown(KeyCode.Return) && (Input.GetKey(KeyCode.LeftAlt)||Input.GetKey(KeyCode.RightAlt))))
            {Screen.fullScreenMode=Screen.fullScreen?FullScreenMode.Windowed:FullScreenMode.FullScreenWindow;magic.CancelStroke();}
            if(!LocalInputLockedV49 && Input.GetKeyDown(KeyCode.M)){map.Toggle();return;}
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(ui.open || ui.Dragging){ui.CloseFromEscapeV58();return;}
                if(MountedCrossbowV58!=null){DismountV58();return;}
                if(coop!=null && coop.MenuOpen){coop.MenuOpen=false;return;}
                if(map.Open){map.SetOpen(false);return;}
                combat.CancelBowV58();
                if(CoopOnline){coop.LocalPaused=!coop.LocalPaused;Time.timeScale=1;}else{paused=!paused;Time.timeScale=paused?0:1;}if(LocalMenuPausedV49){ui.CancelInteraction();magic.CancelStroke();}
            }
            if(Input.GetKeyDown(KeyCode.F10)) { muted=!muted; AudioListener.volume=muted?0:1; }
            if(paused) return;
            if(!LocalInputLockedV49 && !Dead && !journey.Cutscene){if(Input.GetKeyDown(KeyCode.T))ToggleCheatsV58();if(Input.GetKeyDown(KeyCode.U))ToggleDayV44();}
            if(!testMode){meadow.Refresh(body.position);map.RecordVisit();}
            float dt=Time.deltaTime;interactions.Tick();
            if(!LocalInputLockedV49 && Input.GetKeyDown(KeyCode.Q) && !testMode && !meditation.Toggle() && survivor.health<20)ui.Notice("Для медитации нужно хотя бы 20 здоровья");
            bool movePressed=Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.UpArrow)||Input.GetKey(KeyCode.DownArrow)||Input.GetKey(KeyCode.LeftArrow)||Input.GetKey(KeyCode.RightArrow);
            if(!testMode)meditation.Tick(Time.unscaledDeltaTime,movePressed);
            if(!testMode) seconds+=dt;
            survivor.Tick(dt);
            foreach(var item in survivor.equipment)if(item!=null)item.Wear(dt*(item.kind==ItemKind.Boots && movement.sqrMagnitude>.01f?.004f:.001f));
            if(!LocalInputLockedV49 && Input.GetKeyDown(KeyCode.E) && !MovementLocked){ui.Toggle();magic.CancelStroke();}
            bool blocked=LocalInputLockedV49 || (CoopOnline && coop.inventory.Busy) || MovementLocked || Meditating || ui.IsPointerOver || ui.Dragging || magic.Drawing || magic.Teleporting || arcane.Placing || journey.Cutscene || journey.HireOffer;
            movement=new Vector2((Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0),(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0)).normalized;
            if(LocalInputLockedV49 || MovementLocked || Meditating || testMode) movement=Vector2.zero;
            if(testMode && checkMovement.HasValue)movement=checkMovement.Value;
            AnimateWalk(dt);
            art.UpdateOrb(Time.time);
            if(!testMode)HighlightAt(ui.IsPointerOver || ui.Dragging || magic.Drawing?(Vector2?)null:CursorWorld);
            if(!testMode){TickLoot(dt);TickAutoPickup(dt);}
            hero.sortingOrder=Depth(hero.transform.position.y);
            heroShadow.sortingOrder=Mathf.Min(-2000,hero.sortingOrder-1600);
            if(!testMode && !magic.InPocket && journey.IsCoast)foreach(var mob in woodlice)TickCoopActorV49(mob.Position,()=>mob.Tick(dt));
            if(!blocked && !magic.InPocket && Input.GetKeyDown(KeyCode.F)) CollectNearest();
            if(!blocked && Input.GetMouseButtonDown(1))UseWorldRightClick(CursorWorld);
            if(arcane.Placing && !ui.IsPointerOver && Input.GetMouseButtonDown(0))arcane.PlacePendingFire(CursorWorld);
            combat.TickBowInputV58(blocked,CursorWorld);
            if(!blocked && survivor.equipment[0]?.kind!=ItemKind.Bow && (Input.GetMouseButtonDown(0) || survivor.equipment[0]?.kind==ItemKind.Zen && Input.GetMouseButton(0)))
            {
                combat.Begin(CursorWorld-((Vector2)hero.transform.position+new Vector2(0,.72f)));
            }
            if(!blocked && survivor.equipment[0]?.kind!=ItemKind.Bow && Input.GetKeyDown(KeyCode.Space)) combat.Begin(CursorWorld-((Vector2)hero.transform.position+new Vector2(0,.72f)));
            UpdateEnvironment(dt);if(!testMode)TickFoodV43(DaySeconds);
            for(int i=numbers.Count-1;i>=0;i--) { numbers[i].age+=dt; if(numbers[i].age>1.25f) numbers.RemoveAt(i); }
            dummyHitFlash=Mathf.Max(0,dummyHitFlash-dt*5);
            dummy.color=Color.Lerp(Color.white,new Color(1,.53f,.38f),dummyHitFlash);
            if(dummyHealth<=0) { dummyRespawn+=dt; if(dummyRespawn>=4) {dummyHealth=500;dummyRespawn=0;} }
            if(!testMode && !magic.InPocket && movement.sqrMagnitude>.01f && Time.time>=nextPrint && hero.transform.position.y<4)
            {
                nextPrint=Time.time+.27f;
                var s=Shadow("Footstep",hero.transform.position,new Vector2(.15f,.07f),.18f); prints.Add(Tuple.Create(s,Time.time));
            }
            for(int i=prints.Count-1;i>=0;i--) if(Time.time-prints[i].Item2>5) {Destroy(prints[i].Item1.gameObject);prints.RemoveAt(i);}
        }
        void FixedUpdate()
        {
            if(!body) return;
            if(LocalInputLockedV49 || paused || FeetLocked || Meditating) { rollers.Reset();body.linearVelocity=Vector2.zero; return; }
            if(combat.ZenDashing){body.linearVelocity=Vector2.zero;combat.MoveZenDashV44(Time.fixedDeltaTime);return;}
            var desired=MotionVelocity(movement,Time.fixedDeltaTime);
            var p=body.position;
            if(magic.InPocket)
            {
                if((p.x< -4.05f && desired.x<0)||(p.x>4.05f && desired.x>0))desired.x=0;
                if(p.y< -4.05f && desired.y<0)desired.y=0;
                if(p.y>4.05f && desired.y>0 && Mathf.Abs(p.x)>.55f)desired.y=0;
                body.linearVelocity=desired;return;
            }
            if(!journey.IsCoast)
            {var clamped=journey.Clamp(p+desired*.08f);if(Mathf.Abs(clamped.x-(p.x+desired.x*.08f))>.01f)desired.x=0;if(Mathf.Abs(clamped.y-(p.y+desired.y*.08f))>.01f)desired.y=0;body.linearVelocity=desired;return;}
            var next=p+desired*Time.fixedDeltaTime;
            if(!MeadowLayout.Walkable(next)){if(!MeadowLayout.Walkable(p+new Vector2(desired.x*Time.fixedDeltaTime,0)))desired.x=0;if(!MeadowLayout.Walkable(p+new Vector2(0,desired.y*Time.fixedDeltaTime)))desired.y=0;if(!MeadowLayout.Walkable(p+desired*Time.fixedDeltaTime))desired=Vector2.zero;}
            body.linearVelocity=desired;
        }
        void LateUpdate()
        {
            gait?.Pose(facing,walkTime,walking && !MovementLocked,movement,hero.flipX);
            meditation?.Pose();
            PoseMountedV58();meditation?.PoseOperatorV58(MountedCrossbowV58!=null);combat.PoseBowV58();
            cameraRig.Tick();
            TickSetupAudioV58();
        }
        public void ResetCamera(Vector3 position){cameraRig.Reset(position);lastWalkPosition=hero.transform.position;walkTime=0;}
        public void ShakeCamera(float power=.12f,float duration=.30f)=>cameraRig.Shake(power,duration);
        public int CameraShakes=>cameraRig.Shakes;
        public void AnimateWalk(float dt)
        {
            float distance=Vector2.Distance(hero.transform.position,lastWalkPosition);lastWalkPosition=hero.transform.position;
            walking=movement.sqrMagnitude>.01f && body.linearVelocity.sqrMagnitude>.025f;
            if(walking)
            {
                walkTime+=Mathf.Min(distance,.15f)/1.65f;
                if(!combat.Attacking && !combat.BowDrawingV58)facing=Facing(body.linearVelocity);
                walkFrame=Mathf.FloorToInt(Mathf.Repeat(walkTime,1)*8);
            }
            else {walkTime=0;walkFrame=1;}
            // The east profile mirrors the registered west profile without changing the travel direction.
            hero.flipX=facing==5 || facing==6;
            var sheet=survivor.character==CharacterV51.Frog?HeroFramesV51(survivor.character):journey!=null && survivor.equipment[0]?.kind==ItemKind.Lamp?journey.art.lampHero:art.hero;if(survivor.character==CharacterV51.Frog)hero.flipX=FrogFrameMirroredV52(facing,walking?walkFrame:3);
            var sprite=sheet[facing,walking?walkFrame:3];
            if(hero.sprite!=sprite){hero.sprite=sprite;art.PixelStyle(hero);}
        }
        public static int Facing(Vector2 direction)
        {
            float a=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg;
            return ((Mathf.RoundToInt((-a-90)/45)%8)+8)%8;
        }
        public void UpdateEnvironment(float dt)=>environment.Tick(dt);
        public bool Collect(Wildflower flower)
        {
            if(CoopOnline && !coop.ApplyingNetwork && !coop.inventory.Mutating)return coop.inventory.Mutate(()=>Collect(flower));
            if(magic.InPocket || !(journey.IsCoast || journey.Area==WorldArea.GoldenField) || !flower.renderer.gameObject.activeInHierarchy || flower.picked || Vector2.Distance(hero.transform.position,flower.position)>1.85f) return false;
            var item=new Item(flower.kind==ItemKind.Clover?CloverHarvestV43(UnityEngine.Random.value):flower.kind);
            if(!survivor.inventory.Add(item) && (survivor.Bag==null || !survivor.Bag.Add(item))) { ui.Notice("Нет места"); return false; }
            interactions.Pulse(flower.stems);flower.picked=true; flower.renderer.enabled=false; flower.shadow.enabled=false;flower.stems.enabled=false;
            meadow.RecordPickup(flower);
            if(CoopOnline)coop.inventory.Action(new LanWorldActionV49{kind="flower",key=coop.world.FlowerKey(flower),region=coop.Region,x=flower.position.x,y=flower.position.y});
            sounds.PlayOneShot(pickup,.28f); return true;
        }
        public bool TryCollectAt(Vector2 p)
        {
            var flower=FlowerAt(p,true);
            return flower!=null && Collect(flower);
        }
        public static float FireflyVisibility(float time,int index)
        {float phase=Mathf.Repeat(time,360);return phase>=180?(1-CoastTime.Daylight(time))*Mathf.SmoothStep(0,1,Mathf.Clamp01((phase-180-index*.35f)/12)):0;}
        public Wildflower FlowerAt(Vector2 p,bool inReach=false)
        {
            if(magic.InPocket || !(journey.IsCoast || journey.Area==WorldArea.GoldenField))return null;
            Wildflower best=null;float nearest=9;
            foreach(var flower in flowers)
            {
                if(flower.picked || !flower.renderer.gameObject.activeInHierarchy)continue;float d=(flower.position-p).sqrMagnitude;
                if(d>=nearest || (inReach && ((Vector2)hero.transform.position-flower.position).sqrMagnitude>1.85f*1.85f))continue;
                if(Vector2.Distance(flower.renderer.bounds.ClosestPoint(new Vector3(p.x,p.y,0)),p)>=.13f)continue;
                best=flower;nearest=d;
            }
            return best;
        }
        public bool CanCollectAt(Vector2 p)=>FlowerAt(p,true)!=null || loot.Any(l=>!l.collected && !l.InFlight && l.renderer.gameObject.activeInHierarchy && Vector2.Distance(hero.transform.position,l.position)<=1.85f && GroundPixelV45(l,p));
        public bool UseWorldRightClick(Vector2 point)
        {var selected=interactions.At(point,out _);bool done=(snails!=null && snails.Interact(point)) || (snails!=null && snails.InteractAlly(point)) || (amazons!=null && amazons.Interact(point)) || buildings.Interact(point) || OpenGroundVesselV43(point) || TryCollectLootAt(point) || TryCollectAt(point) || arcane.TryExplodeFire(point) || fort.Interact(point) || cave.Interact(point) || journey.Interact(point) || TryOpenShipAt(point);if(done){RightClickConsumedFrame=Time.frameCount;interactions.Pulse(selected);}return done;}
        public void HighlightAt(Vector2? point)
        {
            string label=null;var target=point.HasValue?interactions.At(point.Value,out label):null;
            interactions.Hover(target,label);
            var next=point.HasValue?FlowerAt(point.Value):null;
            if(next!=hoveredFlower)
            {
                if(hoveredFlower!=null){var b=hoverBlock;hoveredFlower.stems.GetPropertyBlock(b);b.SetFloat("_Hover",0);hoveredFlower.stems.SetPropertyBlock(b);}
                hoveredFlower=next;
                if(next!=null){var b=hoverBlock;next.stems.GetPropertyBlock(b);b.SetFloat("_Hover",1);next.stems.SetPropertyBlock(b);}
            }
            hoveredLoot=null;
            if(point.HasValue)foreach(var drop in loot)
                if(!drop.collected && !drop.InFlight && drop.renderer.gameObject.activeInHierarchy && GroundPixelV45(drop,point.Value)){hoveredLoot=drop;break;}
        }
        public void CollectNearest()
        {
            var dropped=loot.Where(l=>!l.collected && !l.InFlight && l.renderer.enabled && l.renderer.gameObject.activeInHierarchy && Vector2.Distance(hero.transform.position,l.position)<=1.85f).OrderBy(l=>((Vector2)hero.transform.position-l.position).sqrMagnitude).FirstOrDefault();
            if(dropped!=null){CollectLoot(dropped);return;}
            var flower=flowers.Where(f=>!f.picked && f.renderer.gameObject.activeInHierarchy).OrderBy(f=>((Vector2)hero.transform.position-f.position).sqrMagnitude).FirstOrDefault();
            if(flower!=null) Collect(flower);
        }
        public void DamageDummy(int damage,bool player=true)
        {
            if(dummyHealth<=0) return;
            if(CoopOnline && !coop.ApplyingNetwork && !coop.CanSimulate){coop.Send(new LanPacketV49{kind="damage",region=coop.Region,body=JsonUtility.ToJson(new LanDamageV49{key="dummy",amount=damage,effect=player?1:0})});return;}
            if(player){PlayerHit(dummy);if(CoopOnline && coop.Context!=null)coop.SendDpsV49(coop.Context.Peer,"dummy",Mathf.Min(dummyHealth,damage));else RecordDummyV48(dummy,Mathf.Min(dummyHealth,damage));}dummyHealth=Mathf.Max(0,dummyHealth-damage); dummyHitFlash=1;
            numbers.Add(new DamageNumber {damage=damage,position=DummyBase+new Vector2(UnityEngine.Random.Range(1.35f,1.5f),1.85f)});
            feedback.Hit(dummy,damage);
        }
        public void ShowDamage(float damage,Vector2 position,SpriteRenderer actor=null,bool player=false,bool shell=false)
        {if(damage<=0)return;numbers.Add(new DamageNumber{damage=damage,position=position});if(actor)feedback.Hit(actor,damage,player,shell);else sounds.PlayOneShot(impact,.35f);}
        public GroundLoot DropMeat(Vector2 at)
        {
            return ScatterLoot(new Item(ItemKind.BeetleMeat),at,at);
        }
        GroundLoot CreateLoot(Item item,Vector2 at,bool thrown,Transform rootV53=null,string regionV53=null)
        {
            if(rootV53==null)rootV53=arcane.WorldRoot;if(item.Definition.Book)item.opened=false;
            var drop=new GroundLoot{item=item,position=at,thrownFrom=(Vector2)hero.transform.position+new Vector2(0,.8f),flightAge=thrown?0:1};
            drop.renderer=art.Renderer("Ground item / "+item.Definition.name,art.icons[item.Definition.icon],at+new Vector2(0,.12f),Depth(at.y));
            bool coin=item.kind==ItemKind.Coin;float width=coin?.26f:item.Definition.Melee?1.05f:item.Definition.width==1?.62f:.85f;
            if(item.kind==ItemKind.LastPrism)width*=ArtBank.PrismVisualScale;
            if(item.kind==ItemKind.Orb)width=.52f;
            drop.renderer.transform.localScale=Vector3.one*(width/drop.renderer.sprite.bounds.size.x);CoastArtV41.SizeGroundBook(drop);SizeGroundItemV44(drop);art.PixelStyle(drop.renderer);drop.renderer.transform.SetParent(rootV53,true);
            if(item.kind==ItemKind.Orb)
            {
                drop.renderer.sortingOrder=Depth(drop.renderer.bounds.min.y);drop.cloth=art.Renderer("Linen beneath the sphere",art.orbCloth,new Vector2(at.x,drop.renderer.bounds.min.y+.10f),drop.renderer.sortingOrder-1);drop.cloth.transform.localScale=Vector3.one*(1.03f/drop.cloth.sprite.bounds.size.x);drop.cloth.transform.SetParent(rootV53,true);art.PixelStyle(drop.cloth);drop.cloth.enabled=!drop.InFlight;
                var orbBlock=new MaterialPropertyBlock();drop.renderer.GetPropertyBlock(orbBlock);orbBlock.SetFloat("_Orb",1);orbBlock.SetFloat("_Emission",.36f);drop.renderer.SetPropertyBlock(orbBlock);
            }
            if(coin)
            {
                var scale=drop.renderer.transform.localScale;scale.y*=.66f;drop.renderer.transform.localScale=scale;
                drop.renderer.transform.position=at+Vector2.up*.045f;
                var properties=new MaterialPropertyBlock();drop.renderer.GetPropertyBlock(properties);properties.SetFloat("_GroundCoin",1);drop.renderer.SetPropertyBlock(properties);
            }
            drop.shadow=Shadow("Thrown item ground shadow",at,coin?new Vector2(.29f,.11f):new Vector2(.55f,.17f),coin?.18f:.27f);drop.shadow.transform.SetParent(rootV53,true);loot.Add(drop);if(!thrown)PoseGroundItemV50(drop);
            if(CoopOnline && !coop.ApplyingNetwork)coop.inventory.Action(new LanWorldActionV49{kind="drop",key=drop.netIdV49,region=regionV53??coop.Region,item=LanItemV49.Capture(item),x=at.x,y=at.y});if(thrown)TickLoot(0);return drop;
        }
        public GroundLoot ThrowItem(Item item,Vector2 target)
        {
            var start=(Vector2)hero.transform.position;var delta=target-start;if(delta.sqrMagnitude<.1f)delta=Vector2.right;
            target=start+Vector2.ClampMagnitude(delta,2.7f);
            if(magic.InPocket)target=new Vector2(Mathf.Clamp(target.x,-3.8f,3.8f),Mathf.Clamp(target.y,-3.8f,3.8f));
            else if(!journey.IsCoast)target=journey.Clamp(target);
            else target=MeadowLayout.Clamp(target);
            if(Physics2D.OverlapCircle(target,.12f))return null;return CreateLoot(item,target,true);
        }
        public GroundLoot ScatterLoot(Item item,Vector2 destination,Vector2 from,Transform rootV53=null,string regionV53=null)
        {
            if((destination-from).sqrMagnitude<.01f){destination=from+UnityEngine.Random.insideUnitCircle.normalized*UnityEngine.Random.Range(.65f,1.35f);from+=Vector2.up*.55f;}
            var drop=CreateLoot(item,destination,true,rootV53,regionV53);drop.thrownFrom=from;TickLoot(0);return drop;
        }
        public bool EquipItem(Item item,GridInventory source,GearSlot slot)
        {
            if(!survivor.EquipOrDrop(item,source,slot,out var dropped))return false;
            if(dropped!=null)ScatterLoot(dropped,body.position+new Vector2(1.3f,.15f),body.position+Vector2.up*.75f);
            combat.ResetForCheck();prism.StopChannel();PlayPickup();return true;
        }
        public bool AutoEquipItem(Item item,GridInventory source)
        {
            if(item.kind==ItemKind.RollerSkates)return EquipItem(item,source,GearSlot.Feet);
            for(int i=0;i<survivor.equipment.Length;i++)if(survivor.equipment[i]==null && Survivor.Fits(item,(GearSlot)i))return EquipItem(item,source,(GearSlot)i);
            for(int i=0;i<survivor.equipment.Length;i++)if(Survivor.Fits(item,(GearSlot)i))return EquipItem(item,source,(GearSlot)i);
            return false;
        }
        public void TickLoot(float dt)
        {
            foreach(var l in loot)if(!l.collected && l.InFlight)
            {
                l.flightAge=Mathf.Min(l.flightDuration,l.flightAge+dt);float t=l.flightAge/l.flightDuration;
                var p=Vector2.Lerp(l.thrownFrom,l.position,t)+Vector2.up*(Mathf.Sin(t*Mathf.PI)*1.0f+.12f*t);
                if(l.item.kind==ItemKind.Spectrum && spectrum!=null){spectrum.PoseThrownLoot(l,t,p);l.shadow.transform.position=Vector2.Lerp(l.thrownFrom-new Vector2(0,.8f),l.position,t);continue;}
                l.renderer.transform.position=p;l.renderer.transform.rotation=Quaternion.Euler(0,0,Mathf.Sin(t*Mathf.PI)*150);l.renderer.sortingOrder=Depth(Vector2.Lerp(l.thrownFrom,l.position,t).y)+2;
                l.shadow.transform.position=Vector2.Lerp(l.thrownFrom-new Vector2(0,.8f),l.position,t);l.shadow.color=new Color(.04f,.04f,.03f,.10f+.17f*t);
            }
            if(spectrum!=null)foreach(var l in loot)spectrum.PoseGroundLoot(l,dt);
            foreach(var l in loot){PoseGroundItemV50(l);PoseGroundZenV44(l);}
            foreach(var l in loot)if(l.cloth){l.cloth.enabled=!l.collected && !l.InFlight;if(!l.InFlight){l.renderer.sortingOrder=Depth(l.renderer.bounds.min.y);l.cloth.sortingOrder=l.renderer.sortingOrder-1;}}
        }
        public bool CollectLoot(GroundLoot drop)
        {
            if(CoopOnline && !coop.ApplyingNetwork && !coop.inventory.Mutating)return coop.inventory.Mutate(()=>CollectLoot(drop));
            if(!drop.renderer.gameObject.activeInHierarchy || drop.InFlight || drop.collected || Vector2.Distance(hero.transform.position,drop.position)>1.85f)return false;
            bool wearable=false;for(int i=0;i<8;i++)if(Survivor.Fits(drop.item,(GearSlot)i))wearable=true;
            if(wearable){var transient=new GridInventory(6,6,"Поднятый предмет");transient.Add(drop.item);if(!AutoEquipItem(drop.item,transient))return false;}
            else if(!survivor.inventory.Add(drop.item) && (survivor.Bag==null || !survivor.Bag.Add(drop.item))){ui.Notice("Нет места");return false;}
            FinishLootPickup(drop);return true;
        }
        public bool TryCollectLootAt(Vector2 point)
        {
            var drop=loot.FirstOrDefault(l=>!l.collected && !l.InFlight && l.renderer.gameObject.activeInHierarchy && GroundPixelV45(l,point));
            return drop!=null && CollectLoot(drop);
        }
        public void PlaySwish() { if(snails!=null && snails.SetupOpen)return;sounds.PlayOneShot(swish,.24f); }
        public void PlayPickup() { if(snails!=null && snails.SetupOpen)return;sounds.PlayOneShot(pickup,.24f); }
        public void Restart() { Time.timeScale=1; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
        void CreateAudio()
        {
            sounds=gameObject.AddComponent<AudioSource>(); sounds.spatialBlend=0;
            oceanAudio=gameObject.AddComponent<AudioSource>(); oceanAudio.loop=true; oceanAudio.clip=NoiseClip("Surf",5,0); oceanAudio.Play();
            rainAudio=gameObject.AddComponent<AudioSource>(); rainAudio.loop=true; rainAudio.clip=NoiseClip("Rain",3,1); rainAudio.Play();
            thunder=NoiseClip("Thunder",2.7f,2); swish=NoiseClip("Weapon",.22f,3); pickup=NoiseClip("Gather",.14f,4); impact=NoiseClip("Straw impact",.12f,5);
        }
        static AudioClip NoiseClip(string name,float duration,int mode)
        {
            const int rate=22050; int length=(int)(rate*duration); var samples=new float[length];
            var random=new System.Random(1743+mode); float smooth=0;
            for(int i=0;i<length;i++)
            {
                float t=(float)i/rate,n=(float)(random.NextDouble()*2-1); smooth=Mathf.Lerp(smooth,n,mode==2?.025f:.09f);
                float env=1;
                if(mode==0) samples[i]=(smooth*.8f+n*.025f)*(.6f+.4f*Mathf.Sin(t*Mathf.PI*2/duration));
                if(mode==1) samples[i]=n*.17f+smooth*.15f;
                if(mode>=2)
                {
                    env=Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/duration))*Mathf.Exp(-t*(mode==2?1:8));
                    if(mode==2) samples[i]=(smooth*2.8f+Mathf.Sin(t*42)*.08f)*env;
                    else if(mode==3) samples[i]=n*.4f*env;
                    else if(mode==4) samples[i]=Mathf.Sin(t*(1400+t*1500))*.16f*env;
                    else samples[i]=(smooth+n*.12f)*env*.6f;
                }
            }
            var clip=AudioClip.Create(name,length,1,rate,false);clip.SetData(samples,0);return clip;
        }
    }
}
