using System.Collections.Generic;
using UnityEngine;

namespace AshenCoast
{
    public sealed class ExpansionArt
    {
        public Sprite inn,cart,poplar,poplar2;
        public readonly Sprite[] props=new Sprite[8];
        public readonly Sprite[] chairs=new Sprite[4];
        public readonly Sprite[] furniture=new Sprite[4],walls=new Sprite[4];
        public readonly Sprite[,] people=new Sprite[8,8],mage=new Sprite[5,8],lampHero=new Sprite[8,8],gulls=new Sprite[3,8];
        public readonly Sprite[,] gestures=new Sprite[3,12];
        public readonly Sprite[] keeper=new Sprite[24];
        public readonly Sprite[] hearth=new Sprite[16];
        public readonly Sprite[,] driverRun=new Sprite[4,8];
        readonly ArtBank art;
        public ExpansionArt(ArtBank bank)
        {
            art=bank;var p=art.Load("TavernPropsV10");
            var regions=new[]{new RectInt(0,0,388,500),new RectInt(388,0,479,519),new RectInt(868,0,357,500),new RectInt(1230,0,306,500),
                new RectInt(0,520,514,480),new RectInt(516,500,352,524),new RectInt(870,585,355,439),new RectInt(1230,500,306,524)};
            for(int i=0;i<8;i++)props[i]=art.Slice(p,regions[i],new Vector2(.5f,i==3 || i==4?.5f:0),160);
            var v=art.Load("VillagePropsV10");
            inn=art.Slice(v,new RectInt(0,0,960,700),new Vector2(.5f,0),110);
            poplar=art.Slice(v,new RectInt(962,0,350,700),new Vector2(.5f,0),70);
            cart=art.Slice(v,new RectInt(0,730,960,469),new Vector2(.5f,0),170);
            poplar2=art.Slice(v,new RectInt(970,700,342,499),new Vector2(.5f,0),55);
            var money=art.Load("TradeItemsV10");
            art.icons[28]=art.Slice(money,new RectInt(0,0,money.width/2,money.height),Vector2.one*.5f,240);
            art.icons[29]=art.Slice(money,new RectInt(money.width/2,0,money.width/2,money.height),Vector2.one*.5f,240);
            LoadActors("TavernPeopleV14b",people,8,1.65f);
            LoadActors("DriverRunV32",driverRun,4,1.7f);
            LoadActors("MageActorsV14",mage,5,1.70f);
            LoadActors("TavernGesturesV14",gestures,3,1.65f);
            var wipe=new Sprite[2,12];LoadActors("KeeperWipeV14",wipe,2,1.65f);
            for(int i=0;i<keeper.Length;i++)keeper[i]=wipe[i/12,i%12];
            var hh=new Sprite[6,8];LoadActors("HeroLampV14",hh,6,1.65f);
            int[] order={0,1,2,3,4,3,2,5};for(int d=0;d<8;d++)for(int f=0;f<8;f++)lampHero[d,f]=hh[order[d],f];
            LoadActors("GullsV10",gulls,3,.85f);
            var ch=art.Load("TavernChairsV10");for(int i=0;i<4;i++)chairs[i]=art.Slice(ch,new RectInt(i%2*ch.width/2,i/2*ch.height/2,ch.width/2,ch.height/2),new Vector2(.5f,0),180);
            var fixtures=art.Load("TavernFurnitureV11");
            var fixtureRegions=new[]{new RectInt(0,0,550,465),new RectInt(555,0,981,402),new RectInt(0,510,595,450),new RectInt(600,405,936,619)};
            for(int i=0;i<4;i++)furniture[i]=art.Slice(fixtures,fixtureRegions[i],new Vector2(.5f,i==3?.5f:0),180);
            var structure=art.Load("TavernWallsV11");
            var wallRegions=new[]{new RectInt(90,190,810,305),new RectInt(1160,60,100,490),new RectInt(125,595,400,375),new RectInt(675,770,795,200)};
            for(int i=0;i<4;i++)walls[i]=art.Slice(structure,wallRegions[i],new Vector2(.5f,0),180);
            var fire=art.Load("HearthFireV15");
            for(int i=0;i<16;i++)
            {
                int x=i%4*fire.width/4,top=i/4*fire.height/4,w=fire.width/4,h=fire.height/4;
                var trimmed=art.Slice(fire,new RectInt(x,top,w,h),Vector2.zero,300);int bottom=fire.height-top-h;
                hearth[i]=Sprite.Create(fire,new Rect(x,bottom,w,h),new Vector2(.5f,(trimmed.rect.y-bottom)/h),300,0,SpriteMeshType.FullRect);
            }
        }
        void LoadActors(string name,Sprite[,] destination,int rows,float height)
        {
            var t=art.Load(name);var data=t.GetPixels32();List<Vector2Int> bands=ScanRows(t,data);int frames=destination.GetLength(1);
            bool strict=name.Contains("V12") || name.Contains("V13") || name.Contains("V14");
            if(strict && bands.Count!=rows)throw new System.InvalidOperationException(name+" requires "+rows+" isolated rows; got "+bands.Count);
            if(bands.Count!=rows){bands.Clear();for(int r=0;r<rows;r++)bands.Add(new Vector2Int(r*t.height/rows,(r+1)*t.height/rows-1));}
            for(int r=0;r<rows;r++)
            {
                var rb=bands[r];var columns=ScanColumns(t,data,rb);
                if(strict && columns.Count!=frames)throw new System.InvalidOperationException(name+" row "+r+" requires "+frames+" frames; got "+columns.Count);
                if(columns.Count<frames-1 || columns.Count>frames+1){columns.Clear();for(int c=0;c<frames;c++)columns.Add(new Vector2Int(c*t.width/frames,(c+1)*t.width/frames-1));}
                float actorHeight=height;
                if((name.StartsWith("TavernPeopleV14") && r>=2) || (name=="MageActorsV14" && r==4) || (name=="TavernGesturesV14" && r>0))actorHeight=1.30f;
                float ppu=(rb.y-rb.x+1)/actorHeight;
                for(int c=0;c<frames;c++)
                {
                    var cb=columns[Mathf.Min(c,columns.Count-1)];int x=Mathf.Max(0,cb.x-(strict?0:2)),right=Mathf.Min(t.width,cb.y+(strict?1:3));
                    if(name=="TavernPeopleV14b")
                    {
                        // Locate the opaque body first, then include its whole cell and hands without borrowing a neighbour.
                        x=c==0?Mathf.Max(0,cb.x-20):(columns[c-1].y+cb.x+1)/2;
                        right=c==frames-1?Mathf.Min(t.width,cb.y+21):(cb.y+columns[c+1].x+1)/2;
                    }
                    int top=Mathf.Max(0,rb.x-2),bottom=Mathf.Min(t.height,rb.y+3),w=right-x,h=bottom-top;
                    float anchor=0,count=0;
                    // Register to the pelvis, excluding hands that extend during a conversation.
                    for(int yy=rb.x+Mathf.RoundToInt((rb.y-rb.x)*.53f);yy<rb.x+Mathf.RoundToInt((rb.y-rb.x)*.70f);yy++)for(int xx=x;xx<right;xx++)if(data[(t.height-1-yy)*t.width+xx].a>96){anchor+=xx;count++;}
                    anchor=count>0?anchor/count:(x+right)*.5f;
                    destination[r,c]=Sprite.Create(t,new Rect(x,t.height-bottom,w,h),new Vector2((anchor-x)/w,2f/h),ppu,0,SpriteMeshType.FullRect);
                }
            }
        }
        static List<Vector2Int> ScanRows(Texture2D t,Color32[] data)
        {var a=new List<Vector2Int>();int start=-1,last=-1;int threshold=t.name=="TavernPeopleV10"?35:6;int bridge=t.name.Contains("V14")?1:2;for(int y=0;y<t.height;y++){int n=0;for(int x=0;x<t.width;x++)if(data[(t.height-1-y)*t.width+x].a>96)n++;if(n>threshold){if(start<0)start=y;last=y;}else if(start>=0 && y-last>bridge){a.Add(new Vector2Int(start,last));start=-1;}}if(start>=0)a.Add(new Vector2Int(start,last));return a;}
        static List<Vector2Int> ScanColumns(Texture2D t,Color32[] data,Vector2Int row)
        {var a=new List<Vector2Int>();int start=-1,last=-1;int bridge=t.name.Contains("V14")?0:3;int threshold=t.name=="TavernPeopleV14b"?35:2;for(int x=0;x<t.width;x++){int n=0;for(int y=row.x;y<=row.y;y++)if(data[(t.height-1-y)*t.width+x].a>96)n++;if(n>threshold){if(start<0)start=x;last=x;}else if(start>=0 && x-last>bridge){a.Add(new Vector2Int(start,last));start=-1;}}if(start>=0)a.Add(new Vector2Int(start,last));return a;}
    }
}
