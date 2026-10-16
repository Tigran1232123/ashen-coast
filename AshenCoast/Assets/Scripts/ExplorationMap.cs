using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public enum MapRegion { Coast, GoldenField, Tavern, Fort, Catacombs, Pocket, AmazonHouse, SnailHouse }

    public sealed class ExploredRegion
    {
        public readonly Rect bounds;
        public readonly float cellSize;
        public readonly Texture2D texture;
        readonly byte[] visibility;
        readonly bool[] land;
        readonly int landCells;
        readonly Color32[] colors;
        readonly MapRegion region;
        readonly int seed, width, height;
        public int VisitedCells {get;private set;}
        public float Percent => 100f*VisitedCells/Mathf.Max(1,landCells);
        public ExploredRegion(MapRegion kind,Rect area,float resolution,int worldSeed)
        {
            region=kind;bounds=area;cellSize=resolution;seed=worldSeed;
            width=Mathf.CeilToInt(area.width/resolution);height=Mathf.CeilToInt(area.height/resolution);
            visibility=new byte[width*height];colors=new Color32[visibility.Length];
            land=new bool[visibility.Length];int total=0;
            for(int y=0;y<height;y++)for(int x=0;x<width;x++){int i=y*width+x;Vector2 p=new Vector2(area.xMin+(x+.5f)*resolution,area.yMin+(y+.5f)*resolution);land[i]=kind!=MapRegion.Coast || WorldShape.Land(p);colors[i]=land[i]?Unknown(x,y):kind==MapRegion.Coast && WorldShape.CoastDistanceV58(p)>0 && RiverV58.Bank(p)<0?new Color(.12f,.27f,.32f):Color.clear;if(land[i])total++;}landCells=total;
            texture=new Texture2D(width,height,TextureFormat.RGBA32,false){name="Visited map / "+kind,filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            texture.SetPixels32(colors);texture.Apply(false);
        }
        static Color Unknown(int x,int y)
        {
            float n=MeadowLayout.Value(x,y,19,2);
            return new Color(.115f+n*.018f,.132f+n*.017f,.126f+n*.015f);
        }
        public bool IsVisited(Vector2 p)
        {
            int x=Mathf.FloorToInt((p.x-bounds.xMin)/cellSize),y=Mathf.FloorToInt((p.y-bounds.yMin)/cellSize);
            return x>=0 && x<width && y>=0 && y<height && visibility[y*width+x]>0;
        }
        public bool Reveal(Vector2 at,float radius)
        {
            bool changed=false;
            int x0=Mathf.Max(0,Mathf.FloorToInt((at.x-radius-bounds.xMin)/cellSize)),x1=Mathf.Min(width-1,Mathf.FloorToInt((at.x+radius-bounds.xMin)/cellSize));
            int y0=Mathf.Max(0,Mathf.FloorToInt((at.y-radius-bounds.yMin)/cellSize)),y1=Mathf.Min(height-1,Mathf.FloorToInt((at.y+radius-bounds.yMin)/cellSize));
            for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)
            {
                Vector2 p=new Vector2(bounds.xMin+(x+.5f)*cellSize,bounds.yMin+(y+.5f)*cellSize);
                float distance=Vector2.Distance(p,at);if(distance>radius)continue;
                byte strength=(byte)Mathf.RoundToInt(Mathf.Lerp(36,255,Mathf.Clamp01((radius-distance)/(radius*.24f))));
                int i=y*width+x;if(!land[i] || strength<=visibility[i])continue;
                if(visibility[i]==0)VisitedCells++;visibility[i]=strength;
                colors[i]=Color.Lerp(Unknown(x,y),Terrain(p),strength/255f);changed=true;
            }
            if(changed){texture.SetPixels32(colors);texture.Apply(false);}
            return changed;
        }
        Color Terrain(Vector2 p)
        {
            float grain=MeadowLayout.Value(Mathf.FloorToInt(p.x/cellSize),Mathf.FloorToInt(p.y/cellSize),seed,13);
            if(region==MapRegion.Pocket)return new Color(.30f,.36f,.46f)*(1+(grain-.5f)*.12f);
            if(region==MapRegion.Catacombs)return CatacombLayout.Floor(p)?new Color(.43f,.42f,.39f)*(1+(grain-.5f)*.12f):new Color(.16f,.17f,.19f);
            if(region==MapRegion.Fort)return new Color(.46f,.48f,.46f)*(1+(grain-.5f)*.1f);
            if(region==MapRegion.Tavern)
            {
                bool wall=Mathf.Abs(p.x-4.1f)<.35f && (p.y>2 || p.y<-.5f);
                return wall?new Color(.36f,.29f,.19f):new Color(.63f,.49f,.30f)*(1+(grain-.5f)*.12f);
            }
            if(region==MapRegion.GoldenField)return Color.Lerp(new Color(.52f,.47f,.24f),new Color(.66f,.57f,.31f),grain);
            if(RiverV58.OnBridge(p))return new Color(.48f,.32f,.17f);
            if(RiverV58.Bank(p)<0 || WorldShape.CoastDistanceV58(p)<0)return new Color(.27f,.40f,.43f)*(1+(grain-.5f)*.1f);
            if(p.y<4.1f+Mathf.Sin(p.x*.15f)*.55f)return new Color(.70f,.62f,.43f)*(1+(grain-.5f)*.1f);
            if(p.y>32 && MeadowLayout.PathDistance(p)<1.9f)return new Color(.61f,.55f,.36f);
            var grass=Color.Lerp(new Color(.39f,.45f,.27f),new Color(.47f,.51f,.31f),grain);
            if(grain>.67f)
            {
                Color[] petals={new Color(.43f,.53f,.64f),new Color(.63f,.43f,.58f),new Color(.78f,.72f,.48f),new Color(.76f,.66f,.24f),new Color(.65f,.31f,.24f),new Color(.57f,.43f,.66f)};
                grass=Color.Lerp(grass,petals[MeadowLayout.DominantFlower(p,seed)],.48f);
            }
            return grass;
        }
        public void Dispose(){if(Application.isPlaying)UnityEngine.Object.Destroy(texture);else UnityEngine.Object.DestroyImmediate(texture);}
    }

    public sealed partial class ExplorationMap : MonoBehaviour
    {
        public CoastGame game;
        public bool Open {get;private set;}
        public MapRegion Region {get;private set;}
        public readonly ExploredRegion[] regions=new ExploredRegion[8];
        public Vector2 Centre {get;private set;}
        public float Zoom {get;private set;}=12;
        public Rect Panel {get;private set;}
        public Rect Plot {get;private set;}
        Vector2? lastVisit;
        MapRegion lastRegion;
        bool panning;
        readonly List<Rect> labelSpace=new List<Rect>();
        GUIStyle text,small,title,centered;
        Font font;
        readonly Color ink=new Color(.86f,.81f,.65f),gold=new Color(.57f,.48f,.29f);
        public MapRegion CurrentRegion => game.snails!=null && game.journey.Area==WorldArea.SnailHouse && game.snails.Inside>=0 && !game.magic.InPocket?MapRegion.SnailHouse: game.magic.InPocket?MapRegion.Pocket:game.journey.Area==WorldArea.AmazonHouse?MapRegion.AmazonHouse:(MapRegion)((int)game.journey.Area);
        public void Initialize(CoastGame owner)
        {
            game=owner;int seed=game.meadow.Seed;
            regions[0]=new ExploredRegion(MapRegion.Coast,MeadowLayout.MapBounds,2,seed);
            regions[1]=new ExploredRegion(MapRegion.GoldenField,new Rect(-30,-19,60,48),1,seed);
            regions[2]=new ExploredRegion(MapRegion.Tavern,new Rect(-7.5f,-5.4f,16,11),.25f,seed);
            regions[3]=new ExploredRegion(MapRegion.Fort,new Rect(-21,-16,42,35),.5f,seed);
            regions[4]=new ExploredRegion(MapRegion.Catacombs,CatacombLayout.Bounds,.5f,seed);
            regions[5]=new ExploredRegion(MapRegion.Pocket,new Rect(-4.5f,-4.5f,9,11),.25f,seed);
            regions[6]=new ExploredRegion(MapRegion.AmazonHouse,new Rect(-9,-7,18,14),.25f,seed);
            regions[7]=new ExploredRegion(MapRegion.SnailHouse,new Rect(-4.5f,-4.5f,9,9),.25f,seed);RecordVisit();
        }
        public void RecordVisit()
        {
            if(game.journey.Cutscene || game.magic.Teleporting || game.paused)return;
            var at=game.body.position;var area=CurrentRegion;
            if(lastVisit.HasValue && lastRegion==area && (at-lastVisit.Value).sqrMagnitude<.36f)return;
            regions[(int)area].Reveal(at,area==MapRegion.Coast?11:area==MapRegion.GoldenField?9:area==MapRegion.Fort || area==MapRegion.Catacombs?5:1.65f);
            lastVisit=at;lastRegion=area;
        }
        public bool IsVisited(MapRegion area,Vector2 at)=>regions[(int)area].IsVisited(at);
        public bool SetOpen(bool value)
        {
            if(value==Open)return true;
            if(value && (game.paused || game.MovementLocked))return false;
            if(value)
            {
                RecordVisit();game.ui.CloseContainers();game.journey.HireOffer=false;game.magic.CancelStroke();game.arcane.CancelPlacement();game.arcane.CancelHeldSkills();game.HighlightAt(null);
                Region=CurrentRegion;Centre=game.body.position;Zoom=Region==MapRegion.Coast?12:Region==MapRegion.GoldenField?12:38;
            }
            if(!value){TravelSelection=false;SnailCommandV50=false;}
            Open=value;game.paused=value && !game.CoopOnline;Time.timeScale=value && !game.CoopOnline?0:1;panning=false;
            game.body.linearVelocity=Vector2.zero;game.movement=Vector2.zero;Cursor.visible=true;
            return true;
        }
        public void Toggle()=>SetOpen(!Open);
        public void Pan(Vector2 screenDelta)
        {Centre+=new Vector2(-screenDelta.x,screenDelta.y)/Zoom;ClampCentre();}
        public void ChangeZoom(float factor,Vector2 anchor)
        {
            Vector2 before=Centre+new Vector2(anchor.x,-anchor.y)/Zoom;
            Zoom=Mathf.Clamp(Zoom*factor,.45f,64);
            Centre=before-new Vector2(anchor.x,-anchor.y)/Zoom;ClampCentre();
        }
        void ClampCentre()
        {var b=regions[(int)Region].bounds;Centre=new Vector2(Mathf.Clamp(Centre.x,b.xMin,b.xMax),Mathf.Clamp(Centre.y,b.yMin,b.yMax));}
        public void Recenter(){Region=CurrentRegion;Centre=game.body.position;}
        public void Overview()
        {var b=regions[(int)Region].bounds;Centre=b.center;Zoom=Mathf.Min(Plot.width/b.width,Plot.height/b.height)*.93f;}
        void InitStyles()
        {
            if(text!=null)return;
            font=Font.CreateDynamicFontFromOSFont(new[]{"Georgia","Times New Roman","Arial"},16);
            text=new GUIStyle{font=font,fontSize=18,alignment=TextAnchor.MiddleLeft};text.normal.textColor=ink;
            small=new GUIStyle(text){fontSize=14};title=new GUIStyle(text){fontSize=28};centered=new GUIStyle(text){alignment=TextAnchor.MiddleCenter};
        }
        static void Fill(Rect r,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        void Border(Rect r,Color color)
        {Fill(new Rect(r.x,r.y,r.width,1),color);Fill(new Rect(r.x,r.yMax-1,r.width,1),color);Fill(new Rect(r.x,r.y,1,r.height),color);Fill(new Rect(r.xMax-1,r.y,1,r.height),color);}
        bool Button(Rect r,string label)
        {
            var mouse=Event.current.mousePosition;Fill(r,r.Contains(mouse)?new Color(.24f,.25f,.19f):new Color(.17f,.18f,.14f));Border(r,gold);GUI.Label(r,label,centered);
            if(Event.current.type!=EventType.MouseDown || Event.current.button!=0 || !r.Contains(mouse))return false;
            Event.current.Use();return true;
        }
        string RegionName(MapRegion area)=>area==MapRegion.SnailHouse?"Улитка "+(game.snails.Inside+1):area==MapRegion.AmazonHouse?"Дома Крылатого стана":area==MapRegion.Coast?"Цветочный берег":area==MapRegion.GoldenField?"Золотое поле":area==MapRegion.Tavern?"Таверна":area==MapRegion.Fort?"Разрушенный форт":area==MapRegion.Catacombs?"Катакомбы":"Карманное измерение";
        public Vector2 MapPoint(Vector2 world)=>new Vector2(Plot.width/2+(world.x-Centre.x)*Zoom,Plot.height/2-(world.y-Centre.y)*Zoom);
        void Landmark(Vector2 at,string glyph,string name,Color color,bool always=false)
        {
            if(!always && !IsVisited(Region,at))return;var p=MapPoint(at);
            if(p.x<20 || p.x>Plot.width-20 || p.y<20 || p.y>Plot.height-20){if(!always)return;p=new Vector2(Mathf.Clamp(p.x,20,Plot.width-20),Mathf.Clamp(p.y,20,Plot.height-20));}
            Fill(new Rect(p.x-9,p.y-9,18,18),new Color(.10f,.12f,.10f,.9f));Border(new Rect(p.x-9,p.y-9,18,18),color);
            if(glyph=="♜"){Fill(new Rect(p.x-6,p.y-1,12,7),color);for(int i=-1;i<=1;i++)Fill(new Rect(p.x+i*5-1.5f,p.y-6,3,9),color);Fill(new Rect(p.x-1,p.y+2,2,4),new Color(.1f,.12f,.1f));}
            else if(glyph=="⌂"){for(int row=0;row<5;row++)Fill(new Rect(p.x-row-1,p.y-7+row,row*2+2,1),color);Fill(new Rect(p.x-5,p.y-2,10,8),color);Fill(new Rect(p.x-1,p.y+1,3,5),new Color(.1f,.12f,.1f));}
            else GUI.Label(new Rect(p.x-10,p.y-13,20,24),glyph,centered);
            if(Zoom>=3 || glyph=="⌂")
            {
                float width=small.CalcSize(new GUIContent(name)).x+12;
                for(int attempt=0;attempt<9;attempt++)
                {
                    int step=(attempt+1)/2;float shift=attempt==0?0:(attempt%2==1?-1:1)*step*27;
                    var r=new Rect(always && p.x>Plot.width*.6f?p.x-15-width:p.x+15,p.y-12+shift,width,24);
                    if(r.xMax>Plot.width || r.y<0 || r.yMax>Plot.height || labelSpace.Any(other=>r.Overlaps(other)))continue;
                    labelSpace.Add(r);Fill(r,new Color(.10f,.12f,.10f,.92f));Border(r,new Color(.34f,.35f,.25f));
                    if(attempt>0)Fill(new Rect(p.x+11,Mathf.Min(p.y,r.center.y),1,Mathf.Abs(p.y-r.center.y)),gold);
                    GUI.Label(new Rect(r.x+6,r.y,r.width-12,r.height),name,small);break;
                }
            }
        }
        void OnGUI()
        {
            if(!Open || game==null)return;InitStyles();GUI.depth=-30;
            var matrix=GUI.matrix;float scale=game.ui.UIScale;GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            float w=game.ui.CanvasWidth,h=game.ui.CanvasHeight;
            Fill(new Rect(0,0,w,h),new Color(.02f,.025f,.02f,.86f));Panel=new Rect(72,50,w-144,h-100);
            Fill(Panel,new Color(.105f,.118f,.096f));Border(Panel,gold);
            GUI.Label(new Rect(Panel.x+28,Panel.y+15,450,40),"Карта · "+RegionName(Region),title);
            if(Button(new Rect(Panel.xMax-65,Panel.y+17,36,36),"×")){SetOpen(false);GUI.matrix=matrix;return;}
            float tx=Panel.x+28;
            for(int i=0;i<regions.Length;i++)if(regions[i].VisitedCells>0)
            {
                float width=Mathf.Min(i>=3?218:168,(Panel.width-56)/regions.Count(r=>r.VisitedCells>0)-8);
                if(Button(new Rect(tx,Panel.y+67,width,32),RegionName((MapRegion)i)))
                {Region=(MapRegion)i;Centre=Region==CurrentRegion?game.body.position:regions[i].bounds.center;Zoom=i<2?12:38;panning=false;}
                if(i==(int)Region)Fill(new Rect(tx,Panel.y+98,width,2),ink);tx+=width+8;
            }
            Plot=new Rect(Panel.x+28,Panel.y+116,Panel.width-56,Panel.height-212);
            ProcessMapInputV41(Event.current);
            Fill(Plot,new Color(.035f,.075f,.10f));Border(Plot,gold*.65f);
            GUI.BeginGroup(Plot);
            var layer=regions[(int)Region];var b=layer.bounds;
            Vector2 top=MapPoint(new Vector2(b.xMin,b.yMax));
            GUI.DrawTexture(new Rect(top.x,top.y,b.width*Zoom,b.height*Zoom),layer.texture,ScaleMode.StretchToFill);
            labelSpace.Clear();if(Region==MapRegion.Coast && game.amazons!=null)Landmark(game.amazons.Centre,"⌂","Крылатый стан",new Color(.91f,.70f,.39f),true);
            if(Region==CurrentRegion){var playerPoint=MapPoint(game.body.position);labelSpace.Add(new Rect(playerPoint.x-14,playerPoint.y-14,28,28));}
            if(Region==MapRegion.Coast)
            {Landmark(game.cave.Position,"◇","Катакомбы",new Color(.66f,.62f,.77f),true);Landmark(game.fort.Position,"♜","Разрушенный форт",new Color(.78f,.65f,.40f),true);Landmark(game.ShipBase,"◆","Корабль",gold);Landmark(game.DummyBase,"+","Манекен",gold);if(game.journey.IsCoast)Landmark(game.journey.CartPosition,"◇","Повозка",gold);}
            if(Region==MapRegion.Coast)for(int arm=0;arm<3;arm++)Landmark(RiverV58.Centre(arm,RiverV58.BridgeT(arm)),"=","Речной мост",new Color(.74f,.57f,.31f),true);
            if(Region==MapRegion.GoldenField)Landmark(game.journey.InnEntrance,"⌂","Таверна",gold);
            if(Region==MapRegion.Tavern){Landmark(game.journey.TavernExit,"↓","Выход",gold);Landmark(game.journey.keeper.position,"◆","Трактирщик",gold);}
            if(Region==MapRegion.Fort)Landmark(game.fort.ExitPoint,"↓","Выход",gold);
            if(Region==MapRegion.Catacombs)Landmark(game.cave.ExitPoint,"↑","Выход",gold);
            if(Region==MapRegion.Pocket)Landmark(new Vector2(0,4.15f),"↓","Возвращение",gold);
            if(Region==CurrentRegion)
            {
                var p=MapPoint(game.body.position);
                if(p.x>0 && p.x<Plot.width && p.y>0 && p.y<Plot.height)
                {Fill(new Rect(p.x-7,p.y-7,14,14),new Color(.16f,.20f,.19f));Border(new Rect(p.x-7,p.y-7,14,14),ink);Fill(new Rect(p.x-3,p.y-3,6,6),new Color(.92f,.76f,.37f));}
            }
            DrawCaravanRouteV51();DrawSnailMapV50();DrawTravelRadius();
            GUI.EndGroup();
            float bottom=Plot.yMax+14;
            if(Button(new Rect(Plot.x,bottom,42,34),"−"))ChangeZoom(1/1.35f,Vector2.zero);
            if(Button(new Rect(Plot.x+50,bottom,42,34),"+"))ChangeZoom(1.35f,Vector2.zero);
            if(Button(new Rect(Plot.x+108,bottom,162,34),"К персонажу"))Recenter();
            if(Button(new Rect(Plot.x+280,bottom,150,34),"Весь край"))Overview();
            if(Region==MapRegion.Coast && Button(new Rect(Plot.x+440,bottom,140,34),"К форту")){Centre=game.fort.Position;Zoom=12;}
            GUI.Label(new Rect(Plot.xMax-235,bottom,235,34),"Исследовано: "+layer.Percent.ToString("0.00")+" %",new GUIStyle(small){alignment=TextAnchor.MiddleRight});
            
            GUI.matrix=matrix;
        }
        void OnDestroy(){foreach(var region in regions)region?.Dispose();if(font)Destroy(font);}
    }
}
