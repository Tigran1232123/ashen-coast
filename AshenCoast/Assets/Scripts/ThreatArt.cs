using System;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public interface ICombatTarget
    {
        SpriteRenderer Renderer {get;}
        Vector2 Position {get;}
        bool Alive {get;}
        float Hurt(float amount);
    }
    public sealed class ThreatArt
    {
        public readonly Sprite[] masonry=new Sprite[16];
        public readonly Sprite[,] bandits=new Sprite[3,8],attacks=new Sprite[3,8],rats=new Sprite[3,8],axolotl=new Sprite[3,8];
        public Sprite exterior,descent;
        public Texture2D stone;
        public ThreatArt(ArtBank art)
        {
            var props=art.Load("FortMasonryV18");
            for(int i=0;i<16;i++)masonry[i]=art.Slice(props,new RectInt(i%4*props.width/4,i/4*props.height/4,props.width/4,props.height/4),new Vector2(.5f,0),100);
            var facade=art.Load("FortExteriorV18");exterior=art.Slice(facade,new RectInt(0,0,facade.width,facade.height),new Vector2(.5f,0),100);
            stone=art.Load("FortStoneFloorV18");
            LoadBandits(art,art.Load("BanditActionsV23"));
            var stairs=art.Load("FortDescentV23");descent=art.Slice(stairs,new RectInt(0,0,stairs.width,stairs.height),new Vector2(.5f,0),100);
            Groups(art.Load("FortRatsV18"),rats,.24f,new Vector2(.5f,.10f));
            var front=new Sprite[2,4];Grid(art.Load("AxolotlFrontV18"),front,4,2,7.5f,new Vector2(.5f,.24f));
            for(int i=0;i<8;i++)axolotl[0,i]=front[i/4,i%4];
            var views=new Sprite[4,4];Grid(art.Load("AxolotlViewsV18"),views,4,4,5.2f,new Vector2(.5f,.24f));
            for(int i=0;i<8;i++){axolotl[1,i]=views[i/4,i%4];axolotl[2,i]=views[2+i/4,i%4];}
            RegisterHunterFeet();
        }
        void LoadBandits(ArtBank bank,Texture2D texture)
        {
            // Artist row gutters are measured rather than assumed. Connected silhouettes
            // retain entire weapons and toes even where they cross a nominal grid column.
            int[] cuts={0,194,369,553,719,876,1024};var pixels=texture.GetPixels32();
            var poses=new Sprite[6,8];float[] scale=new float[3];
            for(int row=0;row<6;row++)
            {
                int top=Mathf.RoundToInt(cuts[row]/1024f*texture.height),end=Mathf.RoundToInt(cuts[row+1]/1024f*texture.height);
                int bottom=texture.height-end,height=end-top,width=texture.width;var labels=new int[width*height];
                var components=new System.Collections.Generic.List<System.Collections.Generic.List<int>>();
                var queue=new System.Collections.Generic.Queue<int>();
                bool Solid(int index)=>pixels[(bottom+index/width)*width+index%width].a>96;
                for(int seed=0;seed<labels.Length;seed++)if(labels[seed]==0 && Solid(seed))
                {
                    int id=components.Count+1;var members=new System.Collections.Generic.List<int>();labels[seed]=id;queue.Enqueue(seed);
                    while(queue.Count>0)
                    {
                        int current=queue.Dequeue(),x=current%width,y=current/width;members.Add(current);
                        for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
                        {
                            int xx=x+dx,yy=y+dy;if(xx<0 || xx>=width || yy<0 || yy>=height || (dx==0 && dy==0))continue;
                            int next=yy*width+xx;if(labels[next]!=0 || !Solid(next))continue;labels[next]=id;queue.Enqueue(next);
                        }
                    }
                    components.Add(members);
                }
                var main=System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Take(System.Linq.Enumerable.OrderByDescending(System.Linq.Enumerable.Range(0,components.Count),i=>components[i].Count),8));
                if(main.Length!=8 || main.Any(i=>components[i].Count<1000))throw new InvalidOperationException("Bandit atlas requires eight separated complete bodies in row "+row);
                float Centre(int component)=>components[component].Average(index=>(float)(index%width));
                main=main.OrderBy(Centre).ToArray();var owner=new int[components.Count];
                for(int component=0;component<components.Count;component++)
                {
                    int exact=System.Array.IndexOf(main,component);if(exact>=0){owner[component]=exact;continue;}
                    float centre=Centre(component),nearest=float.MaxValue;
                    for(int col=0;col<8;col++){float distance=Mathf.Abs(centre-Centre(main[col]));if(distance<nearest){nearest=distance;owner[component]=col;}}
                }
                for(int col=0;col<8;col++)
                {
                    int minX=width,maxX=-1,minY=height,maxY=-1;
                    foreach(int component in Enumerable.Range(0,components.Count).Where(i=>owner[i]==col))foreach(int index in components[component]){minX=Mathf.Min(minX,index%width);maxX=Mathf.Max(maxX,index%width);minY=Mathf.Min(minY,index/width);maxY=Mathf.Max(maxY,index/width);}
                    int rw=maxX-minX+1,rh=maxY-minY+1;var mask=new bool[rw*rh];
                    foreach(int component in Enumerable.Range(0,components.Count).Where(i=>owner[i]==col))foreach(int index in components[component])mask[(index/width-minY)*rw+index%width-minX]=true;
                    int opaqueLow=height,opaqueHigh=-1;foreach(int index in components[main[col]])if(pixels[(bottom+index/width)*width+index%width].a>160){opaqueLow=Mathf.Min(opaqueLow,index/width);opaqueHigh=Mathf.Max(opaqueHigh,index/width);}
                    if(row%2==0)scale[row/2]=Mathf.Max(scale[row/2],(opaqueHigh-opaqueLow+1)/1.65f);
                    // Store pixels and pivots first; all eight phases use one scale for the role.
                    float ppu=scale[row/2];var pivot=new Vector2(((col+.5f)*width/8-minX)/rw,(opaqueLow-minY+1f)/rh);
                    var pose=Sprite.Create(texture,new Rect(minX,bottom+minY,rw,rh),pivot,ppu,0,SpriteMeshType.FullRect);
                    poses[row,col]=pose;bank.RegisterHitMask(pose,mask);
                }
                // Recreate after the maximum walking height is known, with one fixed PPU.
                for(int col=0;col<8;col++)
                {
                    var source=poses[row,col];var mask=bank.HitMask(source);var rect=source.rect;
                    var pose=Sprite.Create(texture,rect,new Vector2(source.pivot.x/rect.width,source.pivot.y/rect.height),scale[row/2],0,SpriteMeshType.FullRect);bank.RegisterHitMask(pose,mask);
                    var vertices=new System.Collections.Generic.List<Vector2>();var indices=new System.Collections.Generic.List<ushort>();int w=(int)rect.width,h=(int)rect.height;
                    for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(mask[y*w+x])
                    {
                        int first=x;while(x+1<w && mask[y*w+x+1])x++;int v=vertices.Count;
                        if(v+4>65535)throw new InvalidOperationException("Bandit silhouette mesh too large");
                        // OverrideGeometry accepts rectangle pixels and applies pivot/PPU itself.
                        Vector2 Local(float xx,float yy)=>new Vector2(xx,yy);
                        vertices.Add(Local(first,y));vertices.Add(Local(x+1,y));vertices.Add(Local(first,y+1));vertices.Add(Local(x+1,y+1));
                        indices.Add((ushort)v);indices.Add((ushort)(v+2));indices.Add((ushort)(v+1));indices.Add((ushort)(v+1));indices.Add((ushort)(v+2));indices.Add((ushort)(v+3));
                    }
                    pose.OverrideGeometry(vertices.ToArray(),indices.ToArray());bank.RemoveHitMask(source);UnityEngine.Object.Destroy(source);
                    if(row%2==0)bandits[row/2,col]=pose;else attacks[row/2,col]=pose;
                }
            }
        }
        void RegisterHunterFeet()
        {
            // A fixed scale per view; only the contact pivot changes per frame.
            Texture2D cachedTexture=null;Color32[] data=null;
            for(int view=0;view<3;view++)
            {
                float ppu=axolotl[view,0].pixelsPerUnit;
                for(int frame=0;frame<8;frame++)
                {
                    var source=axolotl[view,frame];var r=source.rect;if(cachedTexture!=source.texture){cachedTexture=source.texture;data=cachedTexture.GetPixels32();}
                    int low=(int)r.height;float sum=0,count=0;
                    for(int y=0;y<r.height*.44f;y++)for(int x=0;x<r.width;x++)
                        if(data[((int)r.y+y)*source.texture.width+(int)r.x+x].a>160)low=Mathf.Min(low,y);
                    for(int y=low;y<Mathf.Min(r.height,low+5);y++)for(int x=0;x<r.width;x++)
                        if(data[((int)r.y+y)*source.texture.width+(int)r.x+x].a>160){sum+=x+.5f;count++;}
                    // Preserve horizontal registration: centring individual lifted paws makes the body jump.
                    var pivot=new Vector2(.5f,(low+1)/r.height);
                    axolotl[view,frame]=Sprite.Create(source.texture,r,pivot,ppu,0,SpriteMeshType.FullRect);
                    UnityEngine.Object.Destroy(source);
                }
            }
        }
        static void Groups(Texture2D texture,Sprite[,] output,float height,Vector2 pivot)
        {
            int w=texture.width/4;var pixels=texture.GetPixels32();
            float[] cuts=texture.name=="BanditCombatV18"?new[]{0f,.1834f,.3692f,.5359f,.7002f,.8525f,1f}:new[]{0f,.1667f,.3333f,.5072f,.6858f,.8333f,1f};
            Rect Cell(int row,int col){int top=Mathf.RoundToInt(cuts[row]*texture.height),end=Mathf.RoundToInt(cuts[row+1]*texture.height);return new Rect(col*w,texture.height-end,w,end-top);}
            for(int group=0;group<3;group++)
            {
                int tallest=0;
                for(int row=group*2;row<group*2+2;row++){var rect=Cell(row,0);int bottom=(int)rect.y,h=(int)rect.height,low=h,high=-1;for(int y=0;y<h;y++)for(int x=0;x<texture.width;x++)if(pixels[(bottom+y)*texture.width+x].a>96){low=Mathf.Min(low,y);high=Mathf.Max(high,y);}tallest=Mathf.Max(tallest,high-low+1);}
                if(tallest<2)throw new InvalidOperationException(texture.name+" missing animation group");
                for(int i=0;i<8;i++){var rect=Cell(group*2+i/4,i%4);int low=(int)rect.height;for(int y=0;y<rect.height;y++)for(int x=0;x<rect.width;x++)if(pixels[((int)rect.y+y)*texture.width+(int)rect.x+x].a>96)low=Mathf.Min(low,y);var footPivot=new Vector2(pivot.x,Mathf.Max(0,low+2)/rect.height);output[group,i]=Sprite.Create(texture,rect,footPivot,tallest/height,0,SpriteMeshType.FullRect);}
            }
        }
        static void Grid(Texture2D texture,Sprite[,] output,int columns,int rows,float height,Vector2 pivot)
        {
            int w=texture.width/columns,h=texture.height/rows;var pixels=texture.GetPixels32();
            for(int row=0;row<rows;row++)
            {
                int bottom=texture.height-(row+1)*h,low=h,high=-1;
                for(int y=0;y<h;y++)for(int x=0;x<texture.width;x++)if(pixels[(bottom+y)*texture.width+x].a>96){low=Mathf.Min(low,y);high=Mathf.Max(high,y);}
                if(high<low)throw new InvalidOperationException(texture.name+" has an empty actor row "+row);
                float ppu=(high-low+1)/height;
                for(int col=0;col<columns;col++)
                {
                    int count=0;for(int y=0;y<h;y++)for(int x=col*w;x<(col+1)*w;x++)if(pixels[(bottom+y)*texture.width+x].a>96)count++;
                    if(count<20)throw new InvalidOperationException(texture.name+" has an empty cell "+row+", "+col);
                    output[row,col]=Sprite.Create(texture,new Rect(col*w,bottom,w,h),pivot,ppu,0,SpriteMeshType.FullRect);
                }
            }
        }
    }
}
