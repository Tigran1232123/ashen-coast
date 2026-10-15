using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed class GridView
    {
        public GridInventory inventory;
        public Rect rect;
        public float cell;
        public bool accessible=true;
        public Vector2Int Cell(Vector2 p) => new Vector2Int(Mathf.FloorToInt((p.x-rect.x)/cell),Mathf.FloorToInt((p.y-rect.y)/cell));
    }

    public sealed partial class InventoryUI : MonoBehaviour
    {
        public CoastGame game;
        public const string DeathTitle="Вы погибли",RestartLabel="Начать заново";
        public bool open;
        public bool bagOpen,shipOpen;
        public bool vendorOpen;
        public bool skatesOpen;
        public Rect SkatesPanel=>new Rect(PanelX-150,280,136,174);
        public CraftingStation stationOpen;
        public Rect StationPanel=>new Rect(PanelX-346,MainGridY-25,332,302);
        public Rect VendorPanel=>new Rect(0,0,PanelWidth,CanvasHeight);
        public Rect MageHUD=>new Rect(CanvasWidth/2-144,24,288,82);
        public Rect StatusPanel=>new Rect(22,22,254,190);
        public Rect FoodPanel=>new Rect(22,CanvasHeight-109,294,87);
        public int tab { get=>shipOpen?1:0;set {shipOpen=value==1 && game.ShipReachable;} }
        public float UIScale=>Mathf.Min(Screen.width/1440f,Screen.height/900f);
        public float CanvasWidth=>Screen.width/UIScale;
        public float CanvasHeight=>Screen.height/UIScale;
        public float PanelWidth=>Mathf.Max(480,CanvasWidth/3);
        public float PanelX=>CanvasWidth-PanelWidth;
        public float MainX=>PanelX+(PanelWidth-420)/2;
        public const float MainGridY=370;
        public Rect ShipPanel=>new Rect(PanelX-474,95,460,405);
        public Rect BagPanel=>new Rect(PanelX-264,shipOpen?514:95,250,286);
        public float WorldFraction=>!open?1:vendorOpen?1f/3:Mathf.Clamp((stationOpen!=null?StationPanel.x:shipOpen?ShipPanel.x:bagOpen?BagPanel.x:skatesOpen?SkatesPanel.x:PanelX)/CanvasWidth,.20f,1);
        public bool Dragging => dragItem!=null;
        public Item DraggedItem=>dragItem;
        public bool IsPointerOver => OverSnailV50 ||  (game.coop!=null && (game.coop.MenuOpen || game.coop.LocalPaused)) || (game.amazons!=null && game.amazons.Offer!=null) || (open && AllyPanelOpenV46 && AllyPanelV46.Contains(Pointer)) ||  (game.survivor.Books.Any(b=>b.opened) && SpellBar.Contains(Pointer)) || (open && bookSelected!=null && BookPanel.Contains(Pointer)) || (game.map!=null && game.map.Open) || (game.journey!=null && (game.journey.HireOffer || (game.snails==null && game.journey.companion.Hired && MageHUD.Contains(Pointer)))) || open && (Pointer.x>=PanelX || (skatesOpen && SkatesPanel.Contains(Pointer)) || (stationOpen!=null && StationPanel.Contains(Pointer)) || (vendorOpen && VendorPanel.Contains(Pointer)) || (bagOpen && BagPanel.Contains(Pointer)) || (shipOpen && ShipPanel.Contains(Pointer)));
        public readonly List<GridView> grids = new List<GridView>();
        public readonly List<Rect> actorLabelBounds=new List<Rect>();
        readonly Rect[] gearRects=new Rect[8];
        readonly string[] gearNames={"Орудие","Шлем","Броня","Спина","Амулет","Кольцо","Кольцо","Обувь"};
        readonly Color gold=new Color(.65f,.51f,.30f), ink=new Color(.88f,.80f,.63f), dark=new Color(.08f,.075f,.06f);
        Texture2D wood;
        GUIStyle text, small, title, centered, number, bold;
        readonly Dictionary<int,GUIStyle> styleCache=new Dictionary<int,GUIStyle>();
        GUIStyle hudName,hudValue,actorName,actorValue;
        readonly Dictionary<int,string> meterLabels=new Dictionary<int,string>();
        readonly GUIContent hoverContent=new GUIContent();
        string cachedHover;float cachedHoverWidth;
        string Meter(float value,float max)
        {
            int current=Mathf.CeilToInt(value),maximum=(int)max,key=(maximum<<12)+current;
            if(!meterLabels.TryGetValue(key,out var label)){label=current+" / "+maximum;meterLabels.Add(key,label);}
            return label;
        }
        Font font;
        Item dragItem, hoverItem;
        GridInventory dragSource;
        public readonly GridInventory holding=new GridInventory(6,6,"В руке"){craftingEnabled=false,preserveOpenBooks=true};
        GridInventory origin;bool dragMerchant;
        int dragGear=-1;
        Vector2Int grab;
        Vector2 dragPress;
        GridView craftGrid;
        Vector2Int craftStart, craftEnd;
        bool hammer;
        public bool IsCrafting => craftGrid!=null;
        Event queuedEvent;
        Vector2? testPointer;
        string notice;
        float noticeUntil;
        Vector2 Pointer => testPointer ?? new Vector2(Input.mousePosition.x,Screen.height-Input.mousePosition.y)/UIScale;

        void Awake()
        {
            font=Font.CreateDynamicFontFromOSFont(new[]{"Georgia","Times New Roman","Arial"},16);
            wood=new Texture2D(128,256,TextureFormat.RGBA32,false); wood.filterMode=FilterMode.Point;
            var data=new Color[128*256]; var random=new System.Random(990);
            for(int y=0;y<256;y++) for(int x=0;x<128;x++)
            {
                float grain=(float)random.NextDouble()*.014f+Mathf.Sin(y*.35f+Mathf.Sin(x*.04f)*3)*.004f;
                data[y*128+x]=new Color(.093f+grain,.083f+grain,.066f+grain);
            }
            wood.SetPixels(data);wood.Apply();
        }
        public void Toggle() {weaponAllyV46=null;CloseStation();ClearBookPress();bookSelected=null;abyssSelecting=false;open=!open;bagOpen=shipOpen=vendorOpen=skatesOpen=false;stationOpen=null;Cancel();}
        public void CloseContainers(){weaponAllyV46=null;CloseStation();ClearBookPress();bookSelected=null;abyssSelecting=false;open=bagOpen=shipOpen=vendorOpen=skatesOpen=false;stationOpen=null;Cancel();}
        public void OpenShop(){shopAmazonV46=null;CloseStation();open=vendorOpen=true;bagOpen=shipOpen=skatesOpen=false;stationOpen=null;Cancel();}
        public void OpenStation(CraftingStation station){if(stationOpen!=station)CloseStation();open=true;stationOpen=station;bagOpen=shipOpen=vendorOpen=skatesOpen=false;Cancel();}
        public void OpenShip() {if(game.ShipReachable){open=true;shipOpen=true;skatesOpen=false;Cancel();}}
        public void ToggleBag() {if(game.survivor.Bag!=null){open=true;bagOpen=!bagOpen;skatesOpen=false;Cancel();}}
        public void Notice(string message) {if(message==null){notice=null;noticeUntil=0;return;}message=GeneralErrorV44(message);if(message==null)return;notice=message;noticeUntil=Time.unscaledTime+1.8f;}
        void Cancel() {craftGrid=null;hammer=false;Cursor.visible=true;if(dragMerchant && (!open || !vendorOpen)){holding.items.Remove(dragItem);origin.Add(dragItem);}if(dragItem==null || !holding.items.Contains(dragItem)){dragItem=null;dragSource=null;dragGear=-1;origin=null;dragMerchant=false;}}
        public void CancelInteraction() {ClearBookPress();abyssSelecting=false;Cancel();}
        public void QueuePointerForCheck(Vector2 position,EventType type,int button,bool shift=false,int clickCount=1,bool control=false)
        {
            if(!game.testMode)throw new InvalidOperationException("Only available to runtime verification");
            testPointer=position;queuedEvent=new Event {type=type,button=button,clickCount=clickCount,modifiers=(shift?EventModifiers.Shift:EventModifiers.None)|(control?EventModifiers.Control:EventModifiers.None)};
        }
        public void ClearCheckPointer() {testPointer=null;}
        void InitStyles()
        {
            if(text!=null)return;
            text=new GUIStyle {font=font,fontSize=18,alignment=TextAnchor.MiddleLeft};text.normal.textColor=ink;
            small=new GUIStyle(text){fontSize=15};
            title=new GUIStyle(text){fontSize=24};
            centered=new GUIStyle(text){alignment=TextAnchor.MiddleCenter};
            number=new GUIStyle(centered){fontSize=26,fontStyle=FontStyle.Bold};
            bold=new GUIStyle(text){fontStyle=FontStyle.Bold};
            hudName=new GUIStyle(text){fontSize=15};hudName.normal.textColor=new Color(.77f,.72f,.61f);
            hudValue=new GUIStyle(text){fontSize=17,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleRight};
            actorName=new GUIStyle(text){fontSize=13,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
            actorValue=new GUIStyle(text){fontSize=12,alignment=TextAnchor.MiddleCenter};
        }
        GUIStyle CachedStyle(GUIStyle basis,TextAnchor alignment,int size=0,FontStyle weight=FontStyle.Normal,bool wrap=false)
        {
            int source=basis==small?0:basis==title?1:basis==bold?2:3;
            int key=source*100000+(int)alignment*1000+size*10+(int)weight+(wrap?50000:0);
            if(!styleCache.TryGetValue(key,out var style))
            {style=new GUIStyle(basis){alignment=alignment,fontSize=size>0?size:basis.fontSize,fontStyle=weight==FontStyle.Normal?basis.fontStyle:weight,wordWrap=wrap};styleCache.Add(key,style);}
            return style;
        }
        void OnGUI()
        {
            if(game==null || game.art==null || (game.map!=null && game.map.Open))return;
            InitStyles(); GUI.depth=-10;var original=GUI.matrix;
            GUI.matrix=Matrix4x4.Scale(new Vector3(UIScale,UIScale,1));
            if(game.survivor.Bag==null)bagOpen=false;
            if(!game.ShipReachable)shipOpen=false;
            if(stationOpen!=null && (!stationOpen.world || !stationOpen.world.gameObject.activeInHierarchy || Vector2.Distance(game.body.position,stationOpen.Position)>3.5f)){CloseStation();Cancel();}
            if(vendorOpen && !VendorReachableV46){vendorOpen=false;Cancel();}
            hoverItem=null;
            if(game.snails.SetupOpen){DrawSnailsV50();GUI.matrix=original;return;}DrawHUD(); DrawWorldLabels();
            if(open) DrawPanel();
            else grids.Clear();
            DrawBooks();DrawAmazonUIV46();
            if((open || Dragging || SpellBar.Contains(Pointer)) && !game.paused && !game.snails.Choosing && game.snails.AllyOffer==null)
            {
                if(queuedEvent!=null && Event.current.type==EventType.Repaint){ProcessInteractions(queuedEvent);queuedEvent=null;}
                else ProcessInteractions(Event.current);
            }
            if(dragItem!=null) DrawDrag();
            if(hammer){if(stationOpen!=null && stationOpen.Heated && craftGrid?.inventory==stationOpen.inventory)DrawSpoon(Pointer);else DrawHammer(Pointer);}
            Cursor.visible=!hammer;
            if(noticeUntil>Time.unscaledTime && !string.IsNullOrEmpty(notice))
            {
                var n=open && bookSelected!=null?new Rect(BookPanel.center.x-180,BookPanel.y-45,360,38):new Rect(CanvasWidth/2-180,CanvasHeight-118,360,38);Frame(n,new Color(.08f,.08f,.06f,.9f));GUI.Label(new Rect(n.x+10,n.y+3,340,30),notice,centered);
            }
            if(game.Dead && !game.paused)DrawDeath();
            if(game.LocalMenuPausedV49) DrawPause();
            else if(game.journey.HireOffer)DrawHireOffer();
            DrawCoopUIV49();DrawSnailsV50();
            GUI.matrix=original;
        }
        void DrawHUD()
        {
            if(Event.current.type!=EventType.Repaint)return;
            var survivor=game.survivor;
            var panel=StatusPanel;
            Frame(panel,new Color(.045f,.052f,.055f,.88f));
            Status(new Rect(34,32,230,46),"Здоровье",survivor.health,survivor.MaxHealth,new Color(.55f,.20f,.16f),0);
            Status(new Rect(34,82,230,36),"Сытость",survivor.hunger,survivor.HungerMax,new Color(.55f,.42f,.21f),1);
            Status(new Rect(34,122,230,36),"Иммунитет",survivor.immunity,100,new Color(.29f,.43f,.34f),2);
            Status(new Rect(34,162,230,36),"Мана",survivor.mana,survivor.MaxMana,new Color(.29f,.36f,.53f),3);
            var foodPanel=FoodPanel;Frame(foodPanel,new Color(.045f,.052f,.055f,.86f));
            GUI.Label(new Rect(34,foodPanel.y+7,230,22),"Насыщенность",hudName);
            for(int i=0;i<5;i++)
            {
                var r=new Rect(34+i*55,foodPanel.y+33,48,43);Frame(r,new Color(.09f,.10f,.095f,.8f));
                if(i<survivor.foods.Count)
                {
                    var f=survivor.foods[i];ArtBank.DrawIcon(game.art.icons[ItemDefinition.All[f.kind].icon],new Rect(r.x+6,r.y+3,36,33),Color.white);
                    Fill(new Rect(r.x+4,r.yMax-5,40,2),new Color(.17f,.19f,.16f));Fill(new Rect(r.x+4,r.yMax-5,40*Mathf.Clamp01(f.remaining/f.duration),2),gold);
                    if(r.Contains(Pointer))GUI.Label(new Rect(r.x,r.y-24,120,22),Mathf.CeilToInt(f.remaining)+" с",small);
                }
                else Fill(new Rect(r.center.x-3,r.center.y,6,1),new Color(.26f,.28f,.24f));
            }
        }
        void Status(Rect r,string label,float value,float max,Color color,int symbol)
        {
            // Small engraved insignia, label/value and a narrow inset track.
            var icon=new Rect(r.x,r.y+3,18,18);Insignia(icon,color,symbol);
            GUI.Label(new Rect(r.x+27,r.y,r.width-108,23),label,hudName);
            GUI.Label(new Rect(r.x+r.width-84,r.y,84,23),Meter(value,max),hudValue);
            if(symbol==0)color=Color.Lerp(color,new Color(1,.56f,.29f),game.feedback.Flash(game.hero));
            float height=symbol==0?11:7;var track=new Rect(r.x+27,r.y+27,r.width-27,height);
            Fill(track,new Color(.025f,.031f,.033f,.95f));Border(track,new Color(.30f,.31f,.27f));
            float width=(track.width-4)*Mathf.Clamp01(value/max);
            Fill(new Rect(track.x+2,track.y+2,width,track.height-4),color);
            Fill(new Rect(track.x+2,track.y+2,width,1),Color.Lerp(color,ink,.32f));
            for(int k=1;k<4;k++)Fill(new Rect(track.x+track.width*k/4,track.y+2,1,track.height-4),new Color(.06f,.065f,.065f,.55f));
        }
        void Insignia(Rect r,Color color,int type)
        {
            for(int y=0;y<16;y++)
            {
                int half=type==0?(y<5?6:y<10?7-y/3:Mathf.Max(1,(16-y)/2)):type==1?Mathf.Max(2,7-Mathf.Abs(y-8)/2):type==2?Mathf.Max(1,7-y/3):Mathf.Max(1,7-Mathf.Abs(y-8));
                Fill(new Rect(r.center.x-half,r.y+y,half*2,1),Color.Lerp(color,ink,(15-y)*.014f));
            }
            Fill(new Rect(r.center.x-1,r.y+4,2,6),new Color(.88f,.81f,.65f,.35f));
        }

        void Bar(Rect r,string label,float value,float max,Color color,SpriteRenderer actor=null)
        {
            color=Color.Lerp(color,new Color(1,.63f,.28f),game.feedback.Flash(actor));
            Fill(r,new Color(.040f,.046f,.049f,.96f));Border(r,new Color(.30f,.31f,.27f));
            float width=(r.width-4)*Mathf.Clamp01(value/max);
            Fill(new Rect(r.x+2,r.yMax-6,width,4),color);
            GUI.Label(new Rect(r.x+7,r.y,r.width-86,r.height-6),label,hudName);
            GUI.Label(new Rect(r.xMax-80,r.y,73,r.height-6),Meter(value,max),hudValue);
        }
        void ActorVital(Vector2 at,string name,float value,float max,Color color,SpriteRenderer actor=null)
        {
            if(actor && !game.PlayerStruck.Contains(actor) && !game.feedback.WasHit(actor))return;
            if(!PackActorLabel(at,out at))return;
            float hit=game.feedback.Flash(actor);color=Color.Lerp(color,new Color(1,.63f,.28f),hit);
            var track=new Rect(at.x-50,at.y,100,15);if(hit>0){Border(new Rect(track.x-2,track.y-2,track.width+4,track.height+4),new Color(1,.64f,.27f,hit));}Fill(track,new Color(.035f,.039f,.041f,.93f));Border(track,new Color(.37f,.36f,.29f));
            Fill(new Rect(track.x+1,track.y+1,98*Mathf.Clamp01(value/max),13),color);
            GUI.Label(track,Meter(value,max),actorValue);
        }
        bool PackActorLabel(Vector2 requested,out Vector2 at)
        {
            for(int i=0;i<11;i++)
            {
                float offset=i==0?0:((i+1)/2)*(i%2==1?-23:23);
                if(!TryActorLabelPosition(requested+Vector2.up*offset,out at))continue;
                var box=new Rect(at.x-52,at.y-2,104,20);bool overlap=false;foreach(var used in actorLabelBounds)if(box.Overlaps(used)){overlap=true;break;}
                if(overlap)continue;actorLabelBounds.Add(box);return true;
            }
            at=requested;return false;
        }

        public bool TryActorLabelPosition(Vector2 requested,out Vector2 at)
        {
            float right=CanvasWidth*WorldFraction;
            at=new Vector2(Mathf.Clamp(requested.x,84,right-84),Mathf.Clamp(requested.y,30,CanvasHeight-32));
            var bounds=new Rect(at.x-80,at.y-26,160,53);
            return right>=168 && !bounds.Overlaps(StatusPanel) && !bounds.Overlaps(FoodPanel)
                && !(game.journey.companion.Hired && bounds.Overlaps(MageHUD));
        }

        Vector2 World(Vector2 p)
        {var v=game.cameraView.WorldToViewportPoint(p);return new Vector2(v.x*CanvasWidth,(1-v.y)*CanvasHeight);}
        void DrawWorldLabels()
        {
            if(Event.current.type!=EventType.Repaint)return;
            actorLabelBounds.Clear();
            var p=World(game.DummyBase+new Vector2(0,2.18f));
            if(game.magic.InPocket || !game.journey.IsCoast)p=new Vector2(-100,-100);
            if(p.x>=0 && p.x<CanvasWidth && p.y>0 && p.y<CanvasHeight-50)
            {
                ActorVital(p,"Манекен",game.dummyHealth,500,new Color(.57f,.24f,.18f),game.dummy);
                DrawDummyDpsV48(game.dummy);
            }
            foreach(var n in game.numbers)
            {
                var at=World(n.position+new Vector2(Mathf.Sin(n.age*4)*.10f,n.age*.80f));
                var r=new Rect(at.x-40,at.y-24,80,40);
                float a=Mathf.Clamp01((1.25f-n.age)/.45f);
                GUI.color=new Color(0,0,0,a);GUI.Label(new Rect(r.x+2,r.y+2,r.width,r.height),n.Label,number);
                GUI.color=new Color(1,.83f,.46f,a);GUI.Label(r,n.Label,number);GUI.color=Color.white;
            }
            if(!game.magic.InPocket)foreach(var mob in game.woodlice)
            {
                if((!mob.vitals.Alive && !game.feedback.Recent(mob.renderer)) || !mob.renderer.gameObject.activeInHierarchy)continue;
                var at=World(new Vector2(mob.Position.x,mob.renderer.bounds.max.y+.18f));
                // HP bars are only shown near the player; the passive animal has no tutorial label.
                if(mob.Grabbed || Vector2.Distance(game.hero.transform.position,mob.Position)>6 || at.x<0 || at.x>CanvasWidth*WorldFraction || at.y<40)continue;
                ActorVital(at,mob.Small?"Маленькая мокрица":"Мокрица",mob.vitals.health,mob.vitals.maximumHealth,mob.vitals.Curled?new Color(.37f,.48f,.55f):new Color(.57f,.24f,.18f),mob.renderer);
            }
            if(!game.magic.InPocket && !game.journey.Cutscene && game.journey.cartRoot.activeInHierarchy)
            {
                foreach(var animal in game.journey.caravanAnimals)if(animal.Alive && Vector2.Distance(game.body.position,animal.Position)<8)
                    ActorVital(World(new Vector2(animal.Position.x,animal.Renderer.bounds.max.y+.18f)),"Упряжная мокрица",animal.vitals.health,animal.vitals.maximumHealth,animal.vitals.Curled?new Color(.37f,.48f,.55f):new Color(.57f,.24f,.18f),animal.Renderer);
                var driver=game.journey.cartDriver;if(driver.Alive && Vector2.Distance(game.body.position,driver.Position)<8)
                    ActorVital(World(new Vector2(driver.Position.x,driver.Renderer.bounds.max.y+.18f)),"Повозчик",driver.health,CaravanDriver.MaximumHealth,new Color(.57f,.24f,.18f),driver.Renderer);
            }
            if(!game.magic.InPocket)foreach(var frog in game.arcane.frogs)
            {
                if((!frog.Alive && !game.feedback.Recent(frog.renderer)) || !frog.renderer.gameObject.activeInHierarchy || Vector2.Distance(game.hero.transform.position,frog.Position)>7)continue;
                var at=World(new Vector2(frog.Position.x,frog.renderer.bounds.max.y+.18f));if(at.x<0 || at.x>CanvasWidth*WorldFraction || at.y<40)continue;
                ActorVital(at,"Лягушка-маг",frog.health,FrogMage.MaximumHealth,new Color(.57f,.24f,.18f),frog.renderer);
            }
            if(game.fort.Inside)foreach(var b in game.fort.bandits){if((!b.Alive && !game.feedback.Recent(b.sprite)) || Vector2.Distance(game.body.position,b.Position)>8)continue;var at=World(new Vector2(b.Position.x,b.sprite.bounds.max.y+.14f));if(at.x<0 || at.x>CanvasWidth*WorldFraction || at.y<35)continue;ActorVital(at,b.Name,b.health,100,new Color(.57f,.24f,.18f),b.sprite);}
            if(game.cave.Inside){foreach(var spider in game.cave.spiders)if(spider.Alive && Vector2.Distance(game.body.position,spider.Position)<7)ActorVital(World(new Vector2(spider.Position.x,spider.sprite.bounds.max.y+.15f)),"",spider.health,80,new Color(.43f,.22f,.22f),spider.sprite);foreach(var worm in game.cave.worms)foreach(var part in worm.parts)if(part.Alive && Vector2.Distance(game.body.position,part.Position)<15)ActorVital(World(new Vector2(part.Position.x,part.sprite.bounds.max.y+.15f)),"",part.health,WormPart.MaximumHealth,new Color(.40f,.22f,.52f),part.sprite);}
            if(game.journey.Interior)foreach(var r in game.journey.residents)if(r.Alive && !r.Escaped)ActorVital(World(new Vector2(r.Position.x,r.Renderer.bounds.max.y+.12f)),"",r.health,100,new Color(.57f,.24f,.18f),r.Renderer);
            if(game.journey.IsCoast)foreach(var bird in game.journey.gulls)if(bird.Alive && bird.sprite.enabled && Vector2.Distance(game.body.position,bird.Position)<10)ActorVital(World(new Vector2(bird.Position.x,bird.Renderer.bounds.max.y+.12f)),"",bird.health,30,new Color(.57f,.24f,.18f),bird.Renderer);
            DrawAmazonHealthV46();var hunter=game.hunter;
            if(game.dragons!=null)foreach(var boss in game.dragons.bosses)
            {
                if(!boss.Active || (!boss.Alive && !game.feedback.Recent(boss.Renderer)) || Vector2.Distance(game.body.position,boss.Position)>18)continue;
                var at=World(new Vector2(boss.Position.x,boss.Renderer.bounds.max.y+.18f));at.y=Mathf.Max(58,at.y);
                if(at.x<0 || at.x>CanvasWidth*WorldFraction)continue;
                ActorVital(at,"Петух-дракон",boss.health,RoosterDragon.MaxHealth,new Color(.64f,.19f,.11f),boss.Renderer);
            }
            if(game.journey.Area==WorldArea.GoldenField && CoastTime.IsNight(game.DaySeconds) && (hunter.Active || game.feedback.Recent(hunter.sprite)) && Vector2.Distance(game.body.position,hunter.Position)<18)
                ActorVital(World(hunter.HeadTop+Vector2.up*1.1f),"Аксолотль",hunter.health,2000,new Color(.33f,.16f,.38f),hunter.sprite);
            if(!IsPointerOver && !Dragging && !game.magic.Drawing && game.HoverName!=null)
            {
                var style=CachedStyle(text,TextAnchor.MiddleLeft,22);
                if(cachedHover!=game.HoverName){cachedHover=game.HoverName;hoverContent.text=cachedHover;cachedHoverWidth=style.CalcSize(hoverContent).x+8;}
                var label=new Rect(Mathf.Clamp(Pointer.x+16,0,CanvasWidth-cachedHoverWidth),Mathf.Clamp(Pointer.y+20,0,CanvasHeight-36),cachedHoverWidth,36);
                DrawWorldHoverV41(label,game.HoverName,style);
            }
        }
        void DrawPanel()
        {
            float x=MainX;
            GUI.color=Color.white;GUI.DrawTexture(new Rect(PanelX,0,PanelWidth,CanvasHeight),wood);
            Fill(new Rect(PanelX,0,4,CanvasHeight),new Color(.20f,.17f,.11f));Fill(new Rect(PanelX+5,0,1,CanvasHeight),gold*.65f);
            GUI.Label(new Rect(x,14,380,34),"Снаряжение",title);
            if(Button(new Rect(CanvasWidth-55,18,28,28),"×")){CloseContainers();return;}
            for(int i=0;i<gearRects.Length;i++)
            {
                var r=GearRect((GearSlot)i);gearRects[i]=r;Frame(r,new Color(.075f,.07f,.052f));
                var item=game.survivor.equipment[i];
                if(item!=null)
                {
                    if(item.Definition.Divine)DrawSpectrumEdge(new Rect(r.x+2,r.y+2,r.width-4,r.height-4));
                    DrawItemIcon(item,GearIconAreaV43(item,r),item==dragItem?new Color(1,1,1,.3f):Color.white);
                    DrawCondition(item,r);
                    if(r.Contains(Pointer))hoverItem=item;
                }
                else DrawSlotSymbol(r,(GearSlot)i);
                if(i==(int)GearSlot.Feet && game.survivor.Skates!=null){Border(r,new Color(1,.83f,.16f));Border(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),new Color(.78f,.60f,.08f));DrawItemIcon(game.survivor.Skates,new Rect(r.xMax-30,r.yMax-30,26,26),Color.white);}
                GUI.Label(new Rect(r.x,r.yMax+3,r.width,24),gearNames[i],CachedStyle(small,TextAnchor.MiddleCenter,13));
                if(dragItem!=null && r.Contains(Pointer)) Border(r,Survivor.Fits(dragItem,(GearSlot)i)?new Color(.60f,.71f,.37f):new Color(.65f,.20f,.15f));
            }
            GUI.Label(new Rect(x,MainGridY-28,380,26),"Инвентарь",text);
            GUI.Label(new Rect(x+250,MainGridY-28,170,26),game.survivor.Coins+" монет",CachedStyle(small,TextAnchor.MiddleRight));
            grids.Clear();
            var main=new GridView {inventory=game.survivor.inventory,cell=42,rect=new Rect(x,MainGridY,420,210)};grids.Add(main);DrawGrid(main);DrawSpatialLayer();
            if(vendorOpen)
            {
                Window(VendorPanel,shopAmazonV46!=null?shopAmazonV46.Name:"Трактирщик");var v=new GridView{inventory=VendorInventory,cell=42,rect=new Rect((PanelWidth-420)/2,100,420,210)};grids.Add(v);DrawGrid(v);
                if(shopAmazonV46?.WeaponMerchantV48==true){int row=0;foreach(var k in VendorInventory.items.Select(i=>i.kind).Distinct())GUI.Label(new Rect(28,335+row++*29,PanelWidth-56,28),ItemDefinition.All[k].name+" — "+Survivor.MerchantPriceV48(k)+" монет",text);}
                else {GUI.Label(new Rect(28,340,PanelWidth-56,30),shopAmazonV46!=null?"Сырое мясо — 1 монета":"Пирог — 2 монеты",text);GUI.Label(new Rect(28,378,PanelWidth-56,30),"Жучье мясо — 1 монета",text);}
                if(Button(new Rect(PanelWidth-50,18,28,28),"×")){vendorOpen=false;Cancel();}
            }
            if(shipOpen)
            {
                var panel=ShipPanel;Window(panel,"Трюм");
                if(Button(new Rect(panel.xMax-38,panel.y+10,26,26),"×")){shipOpen=false;Cancel();}
                else {var g=new GridView {inventory=game.shipStorage,cell=42,rect=new Rect(panel.x+20,panel.y+50,420,336),accessible=game.ShipReachable};grids.Add(g);DrawGrid(g);}
            }
            if(bagOpen)
            {
                var panel=BagPanel;Window(panel,"Мешок");
                if(Button(new Rect(panel.xMax-38,panel.y+10,26,26),"×")){bagOpen=false;Cancel();}
                else {var g=new GridView {inventory=game.survivor.Bag,cell=42,rect=new Rect(panel.x+20,panel.y+50,210,210)};grids.Add(g);DrawGrid(g);}
            }
            if(stationOpen!=null)DrawStation();
            if(skatesOpen)DrawSkatesPanel();
            DrawDetail(new Rect(x,MainGridY+230,420,138));
        }
        void Window(Rect r,string name)
        {
            GUI.color=Color.white;GUI.DrawTexture(r,wood);Border(r,gold*.8f);
            GUI.Label(new Rect(r.x+20,r.y+12,r.width-75,30),name,text);
        }
        public Vector2 CellCenter(GridInventory inventory,int x,int y)
        {
            var grid=grids.First(g=>g.inventory==inventory);return new Vector2(grid.rect.x+(x+.5f)*grid.cell,grid.rect.y+(y+.5f)*grid.cell);
        }
        public Rect GearRect(GearSlot slot)
        {int i=(int)slot;return new Rect(MainX+(i%4)*108,80+(i/4)*134,94,106);}
        public static Rect ItemIconArea(Item item,Vector2 centre)
        {
            var d=item.Definition;float w=d.width*42-14,h=d.height*42-12;
            if(item.kind==ItemKind.Orb){w*=.60f;h*=.60f;}
            if(item.kind==ItemKind.Pyromancy){w=d.width*42-5;h=d.height*42-4;}
            if(item.kind==ItemKind.YoYo){w=38;h=38;}
            if(item.kind==ItemKind.GoldenSpear)w=38;
            if(!item.opened && (item.kind==ItemKind.Tome || item.kind==ItemKind.AbyssTome)){w=78;h=78;}
            if(item.kind==ItemKind.WaterBottle)centre.x+=3;
            return new Rect(centre.x-w*.5f,centre.y-h*.5f,w,h);
        }
        void DrawGrid(GridView view)
        {
            var grid=view.inventory;var r=view.rect;var c=view.cell;
            Frame(new Rect(r.x-2,r.y-2,r.width+4,r.height+4),dark);
            for(int y=0;y<grid.rows;y++)for(int x=0;x<grid.columns;x++)
            {
                var cell=new Rect(r.x+x*c+1,r.y+y*c+1,c-2,c-2);Fill(cell,new Color(.12f,.115f,.088f));
                Fill(new Rect(cell.x,cell.y,cell.width,1),new Color(.20f,.18f,.13f));
            }
            DrawWaterV44(view);
            foreach(var item in grid.items)
            {
                var d=item.Definition;var box=new Rect(r.x+item.x*c+2,r.y+item.y*c+2,d.width*c-4,d.height*c-4);
                Fill(box,new Color(.19f,.18f,.12f,.80f));if(item.Definition.Divine)DrawSpectrumEdge(box);Border(box,new Color(.40f,.34f,.22f));
                Color color=item==dragItem?new Color(1,1,1,.25f):grid==game.survivor.inventory && FirstLayerCovered(item)?new Color(1,1,1,.32f):Color.white;
                var icon=ItemIconArea(item,box.center);
                if(OrbNeedsCloth(item,grid))DrawOrbCloth(icon);
                DrawItemIcon(item,icon,color);
                if(stationOpen!=null && grid==stationOpen.inventory)DrawFrying(item,box);
                DrawCondition(item,box);
                if(item.count>1)GUI.Label(new Rect(box.x,box.yMax-23,box.width-4,23),item.count.ToString(),CachedStyle(small,TextAnchor.MiddleRight,0,FontStyle.Bold));
                if(box.Contains(Pointer) && view.accessible && !FirstLayerCovered(item)) {hoverItem=item;if(dragItem==null && craftGrid==null)Border(box,gold*1.25f);}
            }
            if(craftGrid!=null && craftGrid.inventory==grid)
            {
                var region=Selection();var box=new Rect(r.x+region.x*c,r.y+region.y*c,region.width*c,region.height*c);
                Fill(box,new Color(.75f,.58f,.22f,.17f));Border(box,new Color(.92f,.73f,.36f));
            }
            if(dragItem!=null && view.accessible && r.Contains(Pointer))
            {
                var p=view.Cell(Pointer)-grab;var box=new Rect(r.x+p.x*c,r.y+p.y*c,dragItem.Definition.width*c,dragItem.Definition.height*c);
                var placement=grid==game.survivor.inventory?PlacementGrid(dragItem,p,view.Cell(Pointer)):grid;bool valid=placement!=dragItem.contents && placement.CanPlace(dragItem,p.x,p.y);
                Fill(box,valid?new Color(.38f,.52f,.24f,.35f):new Color(.60f,.15f,.12f,.35f));Border(box,valid?new Color(.65f,.73f,.38f):new Color(.80f,.27f,.20f));
            }
        }
        void DrawDetail(Rect r)
        {
            Frame(r,new Color(.072f,.067f,.05f));
            if(hoverItem==null){DrawOrnament(r.center);return;}
            var d=hoverItem.Definition;
            if(hoverItem.Definition.Divine){var oldColor=GUI.color;GUI.color=SpectrumNameColorV43(Time.time);GUI.Label(new Rect(r.x+12,r.y+7,r.width-24,35),d.name,CachedStyle(bold,TextAnchor.MiddleLeft,16,FontStyle.Bold,true));GUI.color=oldColor;}else GUI.Label(new Rect(r.x+12,r.y+7,r.width-24,35),d.name,CachedStyle(bold,TextAnchor.MiddleLeft,16,FontStyle.Bold,true));
            GUI.Label(new Rect(r.x+12,r.y+42,r.width-24,23),d.width+" × "+d.height,small);
            if(d.Book)GUI.Label(new Rect(r.x+12,r.y+78,r.width-24,25),hoverItem.opened?"Открыт":"Закрыт",text);
            if(d.Weapon)
            {
                GUI.Label(new Rect(r.x+12,r.y+65,r.width-24,25),d.damage+" урона",text);
                GUI.Label(new Rect(r.x+12,r.y+90,r.width-24,23),"Дальность "+d.reach,small);
                if(hoverItem.kind==ItemKind.Zen)GUI.Label(new Rect(r.x+185,r.y+65,r.width-197,25),game.survivor.hunger>0?"4 сытости / удар":"4 здоровья / удар",small);
                if(d.arc>0)GUI.Label(new Rect(r.x+190,r.y+90,r.width-204,23),d.arc+"°",small);
            }
            else if(d.Food)
            {
                GUI.Label(new Rect(r.x+12,r.y+66,r.width-24,24),"Сытость +"+d.nourishment,small);
                if(d.mana>0)GUI.Label(new Rect(r.x+12,r.y+88,r.width-24,24),"Мана +"+d.mana,small);
                if(hoverItem.kind!=ItemKind.Brain)GUI.Label(new Rect(r.x+210,r.y+66,180,24),d.foodDuration+" с",small);
                if(hoverItem.kind!=ItemKind.Brain)GUI.Label(new Rect(r.x+12,r.y+108,r.width-24,22),hoverItem.Spoiled?"Испорчено":"Срок годности: "+(hoverItem.freshness/360).ToString("0.0")+" дн.",CachedStyle(small,TextAnchor.MiddleLeft,12));
            }
        }
        void ProcessInteractions(Event e)
        {
            if(NetworkInventoryBusyV49 || game.LocalInputLockedV49)return;
            if(game.CoopOnline && (e.type==EventType.MouseDown || e.type==EventType.MouseUp || e.type==EventType.KeyDown))game.coop.inventory.Mutate(()=>ProcessLocalV49(e));else ProcessLocalV49(e);
        }
        void ProcessLocalV49(Event e)
        {
            if(ProcessStacksV46(e))return;
            if(ProcessThrowV44(e))return;
            if(ProcessBookInteractions(e))return;
            if(e.type==EventType.MouseDown && e.button==1)
            {
                if(dragItem!=null){if(!TryWaterAtPointerV44())TryRefuelAtPointer();game.RightClickConsumedFrame=Time.frameCount;e.Use();return;}
                if(open && gearRects[(int)GearSlot.Feet].Contains(Pointer)){skatesOpen=!skatesOpen;if(skatesOpen)bagOpen=shipOpen=false;game.RightClickConsumedFrame=Time.frameCount;e.Use();return;}
                if(open && gearRects[(int)GearSlot.Back].Contains(Pointer) && game.survivor.Bag!=null)
                {ToggleBag();e.Use();return;}
                var v=grids.FirstOrDefault(g=>g.accessible && g.rect.Contains(Pointer));
                if(v!=null)
                {
                    if(v.inventory==VendorInventory){e.Use();return;}
                    var cell=v.Cell(Pointer);v=VisibleGrid(v,cell);var item=v.inventory.At(cell.x,cell.y);
                    if(FirstLayerCovered(item)){Notice("Сначала уберите предмет сверху");e.Use();return;}
                    if(stationOpen!=null && stationOpen.Heated && v.inventory==stationOpen.inventory && (item==null || item.kind==ItemKind.RawMeat || item.kind==ItemKind.BeetleMeat)){StartGridSelection(v,cell);e.Use();return;}
                    if(item!=null && game.AutoEquipItem(item,v.inventory))
                    {game.PlayPickup();e.Use();return;}
                    if(item!=null && BuildingSystem.Buildable(item.kind))
                    {if(game.arcane.BeginPlaceCampfire(item,v.inventory)){open=false;bagOpen=shipOpen=false;Cancel();}e.Use();return;}
                    if(item!=null && (item.Definition.Food || item.Definition.Book))
                    {
                        if(item.Definition.Book){StartGridSelection(v,cell,true);e.Use();return;}
                        bool done=game.survivor.Eat(item,v.inventory);
                        if(done)game.PlayPickup();else if(item.Spoiled)Notice("Еда испортилась");
                        e.Use();return;
                    }
                    if(item==null && v.inventory.items.Any(i=>i.Definition.Book)){StartGridSelection(v,cell,true);e.Use();}
                    else if(v.inventory!=game.shipStorage && v.inventory.craftingEnabled){StartGridSelection(v,cell);e.Use();}
                }
            }
            if(craftGrid!=null && (e.type==EventType.MouseDrag || e.type==EventType.Repaint))
            {
                UpdateSelectionEnd();
            }
            if(craftGrid!=null && e.type==EventType.Repaint && stationOpen!=null && stationOpen.Heated && craftGrid.inventory==stationOpen.inventory)stationOpen.stirring=.2f;
            if(e.type==EventType.MouseUp && e.button==1 && craftGrid!=null)
            {
                if(stationOpen!=null && stationOpen.Heated && craftGrid.inventory==stationOpen.inventory){game.buildings.Stir(stationOpen,Selection());}
                else if(craftGrid.inventory.Craft(Selection(),out var result,craftGrid.inventory==game.survivor.inventory && game.survivor.SpatialLayerOpen?game.survivor.spatialInventory:null)){game.PlayPickup();Notice(result.Definition.name);}
                else if(craftGrid.inventory.TryRecipe(Selection(),out var kind)){var d=ItemDefinition.All[kind];Notice("Нет места: "+d.name+" ("+d.width+" × "+d.height+")");}
                else Notice("Рецепт не найден");
                craftGrid=null;hammer=false;ClearBookPress();Cursor.visible=true;e.Use();return;
            }
            if(e.type==EventType.MouseDown && e.button==0)
            {
                if(dragItem!=null)
                {
                    DropAtPointer();
                    e.Use();return;
                }
                for(int i=0;i<gearRects.Length;i++)if(open && gearRects[i].Contains(Pointer) && game.survivor.equipment[i]!=null)
                {
                    dragItem=game.survivor.equipment[i];game.survivor.equipment[i]=null;holding.Add(dragItem);dragSource=holding;origin=null;dragGear=i;grab=Vector2Int.zero;dragPress=Pointer;game.combat.ResetForCheck();game.prism.StopChannel();e.Use();return;
                }
                var v=grids.FirstOrDefault(g=>g.accessible && g.rect.Contains(Pointer));
                if(v==null)return;
                var cell=v.Cell(Pointer);v=VisibleGrid(v,cell);var item=v.inventory.At(cell.x,cell.y);
                    if(FirstLayerCovered(item)){Notice("Сначала уберите предмет сверху");e.Use();return;}
                if(item==null)return;
                if(e.shift){QuickTransfer(item,v.inventory);e.Use();return;}
                if(item.Definition.Book && e.clickCount>=2){if(v.inventory.allowOpenBooks){if(!v.inventory.ToggleTome(item))Notice("Нет места для открытой книги");}else Notice("Книга открывается в инвентаре");e.Use();return;}
                if(v.inventory!=VendorInventory && e.clickCount>=2 && AutoSlot(item).HasValue)
                {
                    var slot=AutoSlot(item);if(game.EquipItem(item,v.inventory,slot.Value))game.PlayPickup();
                    e.Use();return;
                }
                grab=cell-new Vector2Int(item.x,item.y);origin=v.inventory;dragMerchant=v.inventory==VendorInventory;v.inventory.items.Remove(item);holding.Add(item);dragItem=item;dragSource=holding;dragGear=-1;dragPress=Pointer;e.Use();return;
            }
            if(e.type==EventType.MouseUp && e.button==0 && dragItem!=null)
            {
                if(Vector2.Distance(Pointer,dragPress)>6)DropAtPointer();e.Use();
            }
        }
        void DropAtPointer()
        {
            if(TryRefuelAtPointer())return;
            var v=grids.FirstOrDefault(g=>open && g.accessible && g.rect.Contains(Pointer));
            if(v!=null && (v.inventory==game.survivor.inventory || v.inventory==game.survivor.spatialInventory))
            {var clicked=v.Cell(Pointer);var target=PlacementGrid(dragItem,clicked-grab,clicked);v=grids.FirstOrDefault(g=>g.inventory==target)??new GridView{inventory=target,cell=42,rect=new Rect(MainX,MainGridY,420,210)};}
            if(dragMerchant)
            {
                if(v==null || (v.inventory!=game.survivor.inventory && v.inventory!=game.survivor.spatialInventory && v.inventory!=game.survivor.Bag))return;
                var p=v.Cell(Pointer)-grab;if(game.survivor.Purchase(holding,dragItem,v.inventory,p.x,p.y)){dragMerchant=false;Cancel();game.PlayPickup();}else Notice("Недостаточно монет или места");return;
            }
            if(open)for(int i=0;i<gearRects.Length;i++)if(gearRects[i].Contains(Pointer))
            {
                if(i==(int)GearSlot.Feet && dragItem.kind==ItemKind.RollerSkates)
                {var oldSkates=game.survivor.Skates;holding.items.Remove(dragItem);game.survivor.skateSlot.items.Clear();game.survivor.skateSlot.Place(dragItem,0,0);if(oldSkates!=null){holding.Add(oldSkates);dragItem=oldSkates;origin=null;grab=Vector2Int.zero;dragPress=Pointer;}else Cancel();game.PlayPickup();return;}
                if(!Survivor.Fits(dragItem,(GearSlot)i))return;var old=game.survivor.equipment[i];holding.items.Remove(dragItem);game.survivor.equipment[i]=dragItem;game.combat.ResetForCheck();game.prism.StopChannel();
                if(old!=null){holding.Add(old);dragItem=old;origin=null;dragGear=i;grab=Vector2Int.zero;dragPress=Pointer;}else Cancel();game.PlayPickup();return;
            }
            if(v!=null)
            {
                if(v.inventory==VendorInventory || !GridInventory.CanEnter(dragItem,v.inventory))return;
                var clicked=v.Cell(Pointer);var old=v.inventory.At(clicked.x,clicked.y);var p=clicked-grab;
                if(old!=null && old.kind==dragItem.kind && old.count<v.inventory.StackLimit(dragItem,old.x,old.y) && (v.inventory.insertionAllowed==null || v.inventory.insertionAllowed())){int add=Mathf.Min(v.inventory.StackLimit(dragItem,old.x,old.y)-old.count,dragItem.count);old.count+=add;if(dragItem.Definition.Food)old.freshness=Mathf.Min(old.freshness,dragItem.freshness);dragItem.count-=add;if(dragItem.count==0){holding.items.Remove(dragItem);Cancel();}game.PlayPickup();return;}
                if(old!=null)
                {
                    if(!v.inventory.CanPlace(dragItem,old.x,old.y,new[]{old}) || !GridInventory.CanEnter(old,holding))return;
                    int x=old.x,y=old.y;v.inventory.items.Remove(old);holding.items.Remove(dragItem);v.inventory.Place(dragItem,x,y);holding.Add(old);dragItem=old;origin=v.inventory;dragGear=-1;grab=Vector2Int.zero;dragPress=Pointer;game.PlayPickup();return;
                }
                if(holding.Transfer(dragItem,v.inventory,p.x,p.y)){Cancel();game.PlayPickup();}return;
            }
            if(IsPointerOver)return;
            var point=(Vector2)game.cameraView.ViewportToWorldPoint(new Vector3(Pointer.x/CanvasWidth,1-Pointer.y/CanvasHeight,20));
            if(BuildingSystem.Buildable(dragItem.kind)){if(game.buildings.PlaceCarried(dragItem,holding,point))Cancel();}
            else if(game.ThrowItem(dragItem,point)!=null){holding.items.Remove(dragItem);Cancel();}
        }
        void DrawSpoon(Vector2 at)
        {var sprite=game.newArt.spoon;float phase=Time.time*5;ArtBank.DrawIcon(sprite,new Rect(at.x-18+Mathf.Sin(phase)*3,at.y-36,52,68),Color.white);}
        public void QuickTransfer(Item item,GridInventory source)
        {
            if(source==VendorInventory)
            {var purchaseTarget=PlayerSpaceV44(item);if(purchaseTarget!=null && purchaseTarget.FindSpace(item,out var cell) && game.survivor.Purchase(source,item,purchaseTarget,cell.x,cell.y))game.PlayPickup();else Notice(game.survivor.Coins<Survivor.MerchantPriceV48(item.kind)?"Недостаточно монет":"Нет места");return;}
            GridInventory target;
            if(stationOpen!=null)target=source==stationOpen.inventory?game.survivor.inventory:stationOpen.inventory;
            else if(source==game.shipStorage)target=game.survivor.inventory;
            else if(tab==1 && game.ShipReachable)target=game.shipStorage;
            else if(source==game.survivor.inventory)target=game.survivor.Bag;
            else target=game.survivor.inventory;
            if(target==game.survivor.inventory && !target.FindSpace(item,out _) && source!=game.survivor.spatialInventory && game.survivor.SpatialLayerOpen)target=game.survivor.spatialInventory;
            if(target!=null && target!=item.contents && target.FindSpace(item,out var p) && source.Transfer(item,target,p.x,p.y))game.PlayPickup();
            else Notice("Нет места");
        }
        bool DropGear(GearSlot slot)
        {
            if(!Survivor.Fits(dragItem,slot))return false;
            if(dragSource!=null)return game.EquipItem(dragItem,dragSource,slot);
            int target=(int)slot;var old=game.survivor.equipment[target];
            if(old!=null && !Survivor.Fits(old,(GearSlot)dragGear))return false;
            game.survivor.equipment[target]=dragItem;game.survivor.equipment[dragGear]=old;return true;
        }
        static GearSlot? AutoSlot(Item item)
        {
            for(int i=0;i<8;i++)if(Survivor.Fits(item,(GearSlot)i))return (GearSlot)i;return null;
        }
        RectInt Selection() => new RectInt(Mathf.Min(craftStart.x,craftEnd.x),Mathf.Min(craftStart.y,craftEnd.y),Mathf.Abs(craftStart.x-craftEnd.x)+1,Mathf.Abs(craftStart.y-craftEnd.y)+1);
        void DrawDrag()
        {
            if(BuildingSystem.Buildable(dragItem.kind) && !IsPointerOver)return;
            float c=42;var d=dragItem.Definition;
            var r=new Rect(Pointer.x-grab.x*c-20,Pointer.y-grab.y*c-20,d.width*c,d.height*c);
            DrawItemIcon(dragItem,ItemIconArea(dragItem,r.center),new Color(1,1,1,.9f));
            DrawCondition(dragItem,r);
        }
        void DrawHammer(Vector2 at)
        {
            var old=GUI.matrix;ArtBank.RotateUI(-34,at);
            Fill(new Rect(at.x+6,at.y-3,7,31),new Color(.45f,.28f,.13f));Border(new Rect(at.x+6,at.y-3,7,31),new Color(.18f,.13f,.08f));
            Frame(new Rect(at.x-4,at.y-11,29,13),new Color(.50f,.53f,.48f));GUI.matrix=old;
        }
        void DrawSlotSymbol(Rect r,GearSlot slot)
        {
            var color=new Color(.28f,.25f,.17f);
            if(slot==GearSlot.RingLeft||slot==GearSlot.RingRight) {Border(new Rect(r.center.x-9,r.center.y-9,18,18),color);return;}
            Fill(new Rect(r.center.x-1,r.center.y-12,2,24),color);Fill(new Rect(r.center.x-9,r.center.y-1,18,2),color);
        }
        void DrawOrnament(Vector2 p)
        {
            var c=gold*.55f;Fill(new Rect(p.x-42,p.y,33,1),c);Fill(new Rect(p.x+9,p.y,33,1),c);
            var old=GUI.matrix;ArtBank.RotateUI(45,p);Border(new Rect(p.x-5,p.y-5,10,10),c);GUI.matrix=old;
        }
        void DrawDeath()
        {Fill(new Rect(0,0,CanvasWidth,CanvasHeight),new Color(.025f,.01f,.045f,.64f));float x=CanvasWidth/2-190,y=CanvasHeight/2-100;Frame(new Rect(x,y,380,200),new Color(.075f,.055f,.075f));GUI.Label(new Rect(x+20,y+20,340,55),game.CoopOnline && game.corpses.HasBody("player/"+game.coop.Peer)?"Ожидайте воскрешения":DeathTitle,CachedStyle(title,TextAnchor.MiddleCenter));if(Button(new Rect(x+55,y+100,270,44),RestartLabel))game.Restart();}
        void DrawPause()
        {
            Fill(new Rect(0,0,CanvasWidth,CanvasHeight),new Color(.025f,.03f,.025f,.70f));
            float x=CanvasWidth/2-175,y=CanvasHeight/2-190;
            Frame(new Rect(x,y,350,432),new Color(.09f,.08f,.058f));GUI.Label(new Rect(x+20,y+18,310,44),"Настройки",CachedStyle(title,TextAnchor.MiddleCenter));
            if(Button(new Rect(x+40,y+85,270,42),"Продолжить")){game.paused=false;if(game.coop!=null)game.coop.LocalPaused=false;Time.timeScale=1;}
            if(Button(new Rect(x+40,y+140,270,42),"Заново"))game.Restart();
            if(Button(new Rect(x+40,y+195,270,42),Screen.fullScreen?"Полный экран: вкл":"Полный экран: выкл"))SetFullscreen(!Screen.fullScreen);
            if(Button(AutoPickupButton,game.AutoPickup?"Автоподбор: вкл":"Автоподбор: выкл"))game.SetAutoPickup(!game.AutoPickup);
            if(Button(new Rect(x+40,y+305,270,42),"Кооператив")){game.coop.MenuOpen=true;game.coop.LocalPaused=false;game.paused=false;Time.timeScale=1;}
            if(Button(new Rect(x+40,y+360,270,42),"Выход"))Application.Quit();
        }
        public void SetFullscreen(bool enabled){Screen.fullScreenMode=enabled?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed;game.magic.CancelStroke();}
        bool TryRefuelAtPointer()
        {
            if(dragItem==null || dragItem.kind!=ItemKind.Wood || dragSource==null || dragSource==VendorInventory)return false;Item lamp=null;
            for(int i=0;i<gearRects.Length;i++)if(gearRects[i].Contains(Pointer))lamp=game.survivor.equipment[i];
            if(lamp==null){var view=grids.FirstOrDefault(v=>v.rect.Contains(Pointer));if(view!=null){var c=view.Cell(Pointer);lamp=view.inventory.At(c.x,c.y);}}
            if(lamp==null || !game.survivor.Refuel(dragItem,dragSource,lamp))return false;Cancel();game.PlayPickup();return true;
        }
        void DrawCondition(Item item,Rect box)
        {
            DrawFoodWaterConditionV43(item,box);if(!item.Definition.Durable)return;var icon=ItemIconArea(item,box.center);float width=Mathf.Min(box.width-8,Mathf.Max(25,icon.width));float x=box.center.x-width*.5f,y=Mathf.Min(box.yMax-5,icon.yMax+2);
            Fill(new Rect(x,y,width,3),new Color(.09f,.08f,.06f));Fill(new Rect(x,y,width*item.durability/100,3),item.durability<25?new Color(.77f,.22f,.13f):new Color(.63f,.72f,.36f));
            if(item.kind==ItemKind.Lamp){Fill(new Rect(x,y-5,width,3),new Color(.09f,.08f,.06f));Fill(new Rect(x,y-5,width*item.fuel/100,3),new Color(.84f,.48f,.13f));}
        }
        void CleanBarFrame(Rect rect){Fill(rect,dark);Border(rect,new Color(.41f,.33f,.21f));}
        void DrawMageHUD()
        {
            var mage=game.journey.companion;if(!mage.Hired || game.hunter.QuoteVisible)return;var r=MageHUD;Frame(r,new Color(.075f,.07f,.055f,.94f));ArtBank.DrawIcon(game.journey.art.mage[0,0],new Rect(r.x+9,r.y+7,42,68),Color.white);
            Bar(new Rect(r.x+60,r.y+9,218,28),"Здоровье",mage.health,CompanionMage.MaxHealth,new Color(.56f,.18f,.14f),mage.sprite);Bar(new Rect(r.x+60,r.y+44,218,28),"Мана",mage.mana,CompanionMage.MaxMana,new Color(.20f,.34f,.59f));
            if(Event.current.type==EventType.MouseDown && Event.current.button==1 && r.Contains(Pointer) && !game.paused){if(!mage.Explode())Notice("Недостаточно маны");game.RightClickConsumedFrame=Time.frameCount;Event.current.Use();}
        }
        void DrawHireOffer()
        {
            var r=new Rect(CanvasWidth/2-200,CanvasHeight/2-92,400,184);Frame(r,dark);GUI.Label(new Rect(r.x+20,r.y+18,360,40),"Нанять мага — 4 монеты",centered);
            if(Button(new Rect(r.x+28,r.y+92,160,42),"Нанять")){if(!game.journey.HireMage())Notice("Недостаточно монет");}
            if(Button(new Rect(r.x+212,r.y+92,160,42),"Закрыть"))game.journey.HireOffer=false;
        }
        bool Button(Rect r,string label,bool selected=false,bool enabled=true)
        {
            Frame(r,selected?new Color(.25f,.215f,.13f):new Color(.12f,.11f,.082f));
            if(enabled && r.Contains(Pointer))Border(r,gold*1.3f);
            GUI.color=enabled?Color.white:new Color(.6f,.6f,.6f);GUI.Label(r,label,centered);GUI.color=Color.white;
            var e=game.testMode && game.paused && Event.current.type==EventType.Repaint && queuedEvent!=null?queuedEvent:Event.current;
            if(enabled && e.type==EventType.MouseDown && e.button==0 && r.Contains(Pointer)){e.Use();if(e==queuedEvent)queuedEvent=null;return true;}return false;
        }
        void Frame(Rect r,Color fill)
        {
            Fill(new Rect(r.x+2,r.y+3,r.width,r.height),new Color(.012f,.017f,.02f,.35f));
            Fill(r,fill);Border(r,new Color(.29f,.31f,.29f));
            Fill(new Rect(r.x+2,r.y+2,r.width-4,1),new Color(.48f,.43f,.32f,.65f));
            Color corner=new Color(.55f,.47f,.32f,.80f);
            Fill(new Rect(r.x+2,r.y+2,5,1),corner);Fill(new Rect(r.x+2,r.y+2,1,5),corner);
            Fill(new Rect(r.xMax-7,r.y+2,5,1),corner);Fill(new Rect(r.xMax-3,r.y+2,1,5),corner);
            Fill(new Rect(r.x+2,r.yMax-3,5,1),corner);Fill(new Rect(r.x+2,r.yMax-7,1,5),corner);
            Fill(new Rect(r.xMax-7,r.yMax-3,5,1),corner);Fill(new Rect(r.xMax-3,r.yMax-7,1,5),corner);
        }
        static void Fill(Rect r,Color c) {GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
        static void Border(Rect r,Color c)
        {Fill(new Rect(r.x,r.y,r.width,1),c);Fill(new Rect(r.x,r.yMax-1,r.width,1),c);Fill(new Rect(r.x,r.y,1,r.height),c);Fill(new Rect(r.xMax-1,r.y,1,r.height),c);}
        void OnDestroy(){if(font)Destroy(font);if(wood)Destroy(wood);DisposeWaterV44();}
        void OnDisable() {Cursor.visible=true;}
        void OnApplicationFocus(bool focus) {if(!focus){ClearBookPress();Cancel();}}
    }
}
