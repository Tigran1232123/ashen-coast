using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed class DragonArt
    {
        public readonly Sprite[,] walk=new Sprite[3,8],attack=new Sprite[3,8];
        public readonly Sprite[] flight=new Sprite[16],column=new Sprite[24];
        public readonly Sprite corpse;
        public readonly Sprite[] defeat=new Sprite[16];
        public float DefeatRestHeight {get;private set;}
        readonly ArtBank art;
        readonly Dictionary<Sprite,Vector2> mouths=new Dictionary<Sprite,Vector2>();
        readonly Dictionary<Texture2D,Color32[]> sourcePixels=new Dictionary<Texture2D,Color32[]>();
        Color32[] Pixels(Texture2D texture){if(!sourcePixels.TryGetValue(texture,out var pixels)){pixels=texture.GetPixels32();sourcePixels[texture]=pixels;}return pixels;}
        public DragonArt(ArtBank bank)
        {
            art=bank;var walkRows=new Sprite[6,4];LoadRows("RoosterWalkV24c",walkRows,6,4,3.8f);
            for(int view=0;view<3;view++)for(int phase=0;phase<8;phase++)walk[view,phase]=walkRows[view*2+phase/4,phase%4];
            float head=RedSpan(walk[1,0]);
            var attackRows=new Sprite[6,4];LoadRows("RoosterAttacksV24c",attackRows,6,4,3.8f);
            for(int move=0;move<3;move++)for(int phase=0;phase<8;phase++)attack[move,phase]=attackRows[move*2+phase/4,phase%4];
            MatchHead(attack,walk[1,0].pixelsPerUnit,head,attack[0,0]);
            var wings=new Sprite[4,4];LoadRows("RoosterFlightV24c",wings,4,4,4.7f);
            MatchHead(wings,walk[1,0].pixelsPerUnit,head,wings[0,0]);
            for(int i=0;i<16;i++)flight[i]=wings[i/4,i%4];
            var body=walk[1,0];corpse=Sprite.Create(body.texture,body.rect,Vector2.one*.5f,body.pixelsPerUnit,0,SpriteMeshType.FullRect);Isolate(corpse,art.HitMask(body));
            var defeated=new Sprite[4,4];LoadRows("RoosterDefeatV30",defeated,4,4,3.8f);MatchHead(defeated,body.pixelsPerUnit,head,defeated[0,0]);
            for(int i=0;i<16;i++)defeat[i]=RegisterBelly(defeated[i/4,i%4]);
            DefeatRestHeight=-defeat[8].bounds.min.y+.03f;
            var t=art.Load("DragonFireColumnV32");int w=t.width/6,h=t.height/4;
            for(int i=0;i<24;i++)
            {
                int x=i%6*w,top=i/6*h;var s=art.Slice(t,new RectInt(x,top,w,h),new Vector2(.5f,0),100);
                int bottom=t.height-top-h;
                column[i]=Sprite.Create(t,new Rect(x,bottom,w,h),new Vector2(.5f,(s.rect.y-bottom)/h),h/7.4f,0,SpriteMeshType.FullRect);
            }
        }
        void LoadRows(string name,Sprite[,] destination,int rows,int cols,float height)
        {
            var t=art.Load(name);var pixels=Pixels(t);var bands=art.Bands(t,true,rows);
            for(int row=0;row<rows;row++)
            {
                var band=bands[row];int h=band.y-band.x+1,bottom=t.height-band.y-1;
                int span=h;if(rows==6){var a=bands[row/2*2];var b=bands[row/2*2+1];span=Mathf.Max(a.y-a.x+1,b.y-b.x+1);}float ppu=span/height;
                // Separate connected bodies, rather than vertical projections: a tail
                // may extend past the next beak without actually touching it.
                var labels=new int[t.width*h];var components=new List<List<int>>();var queue=new Queue<int>();
                bool Solid(int index)=>pixels[(bottom+index/t.width)*t.width+index%t.width].a>96;
                for(int seed=0;seed<labels.Length;seed++)if(labels[seed]==0 && Solid(seed))
                {
                    int id=components.Count+1;var members=new List<int>();labels[seed]=id;queue.Enqueue(seed);
                    while(queue.Count>0)
                    {
                        int current=queue.Dequeue(),x=current%t.width,y=current/t.width;members.Add(current);
                        for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
                        {
                            int xx=x+dx,yy=y+dy;if(xx<0 || xx>=t.width || yy<0 || yy>=h || (dx==0 && dy==0))continue;
                            int next=yy*t.width+xx;if(labels[next]!=0 || !Solid(next))continue;labels[next]=id;queue.Enqueue(next);
                        }
                    }
                    components.Add(members);
                }
                float Centre(int component)=>components[component].Average(index=>(float)(index%t.width));
                var main=Enumerable.Range(0,components.Count).OrderByDescending(i=>components[i].Count).Take(cols).OrderBy(Centre).ToArray();
                if(main.Length!=cols || main.Any(i=>components[i].Count<1000))throw new InvalidOperationException(name+" row "+row+" needs "+cols+" separated complete creatures");
                var owner=new int[components.Count];
                for(int component=0;component<components.Count;component++)
                {
                    int exact=Array.IndexOf(main,component);if(exact>=0){owner[component]=exact;continue;}
                    float centre=Centre(component),nearest=float.MaxValue;
                    for(int col=0;col<cols;col++){float distance=Mathf.Abs(centre-Centre(main[col]));if(distance<nearest){nearest=distance;owner[component]=col;}}
                }
                for(int col=0;col<cols;col++)
                {
                    var members=Enumerable.Range(0,components.Count).Where(i=>owner[i]==col).SelectMany(i=>components[i]).ToArray();
                    int x=members.Min(i=>i%t.width),right=members.Max(i=>i%t.width),w=right-x+1,low=h;
                    var mask=new bool[w*h];
                    foreach(int index in members)
                    {
                        int y=index/t.width,xx=index%t.width-x;mask[y*w+xx]=true;
                        if(pixels[(bottom+y)*t.width+x+xx].a>150)low=Mathf.Min(low,y);
                    }
                    // Register to the pelvis, not to whichever talon happens to touch
                    // the floor in this frame; otherwise the body jumps sideways.
                    int view=rows==6?row/2:row;
                    float centre=name.Contains("Walk")?(view==0?w*.42f:view==1?w*.46f:w*.56f):w*.46f;
                    var pose=Sprite.Create(t,new Rect(x,bottom,w,h),new Vector2(centre/w,Mathf.Max(0,low-1)/(float)h),ppu,0,SpriteMeshType.FullRect);
                    Isolate(pose,mask);destination[row,col]=pose;
                }
            }
        }
        float RedSpan(Sprite sprite)
        {
            var r=sprite.rect;var data=Pixels(sprite.texture);var mask=art.HitMask(sprite);int lo=(int)r.height,hi=-1;
            for(int y=0;y<r.height;y++)for(int x=0;x<r.width;x++)
            {var c=data[((int)r.y+y)*sprite.texture.width+(int)r.x+x];if((mask==null || mask[y*(int)r.width+x]) && c.a>150 && c.r>120 && c.g<85 && c.b<75 && c.r>c.g*1.9f){lo=Mathf.Min(lo,y);hi=Mathf.Max(hi,y);}}
            return hi>=lo?hi-lo+1:r.height*.18f;
        }
        Sprite RegisterBelly(Sprite source)
        {
            var r=source.rect;var data=Pixels(source.texture);var mask=art.HitMask(source);Vector2 sum=Vector2.zero;float count=0;
            for(int y=0;y<r.height;y++)for(int x=0;x<r.width;x++)
            {
                var c=data[((int)r.y+y)*source.texture.width+(int)r.x+x];
                if(mask[y*(int)r.width+x] && c.a>150 && c.r>150 && c.g>85 && c.g<210 && c.b<90 && c.r>c.g*1.12f){sum+=new Vector2(x+.5f,y+.5f);count++;}
            }
            var anchor=count>0?sum/count:new Vector2(r.width*.45f,r.height*.45f);
            var pose=Sprite.Create(source.texture,r,new Vector2(anchor.x/r.width,anchor.y/r.height),source.pixelsPerUnit,0,SpriteMeshType.FullRect);
            Isolate(pose,mask);art.RemoveHitMask(source);UnityEngine.Object.Destroy(source);return pose;
        }
        void MatchHead(Sprite[,] poses,float basePpu,float sourceHead,Sprite reference)
        {
            float ppu=basePpu*Mathf.Clamp(RedSpan(reference)/Mathf.Max(1,sourceHead),.40f,2);
            for(int row=0;row<poses.GetLength(0);row++)for(int col=0;col<poses.GetLength(1);col++)
            {var old=poses[row,col];var r=old.rect;var mask=art.HitMask(old);poses[row,col]=Sprite.Create(old.texture,r,new Vector2(old.pivot.x/r.width,old.pivot.y/r.height),ppu,0,SpriteMeshType.FullRect);if(mask!=null)Isolate(poses[row,col],mask);art.RemoveHitMask(old);UnityEngine.Object.Destroy(old);}
        }
        void Isolate(Sprite sprite,bool[] mask)
        {
            art.RegisterHitMask(sprite,mask);int w=(int)sprite.rect.width,h=(int)sprite.rect.height;
            var vertices=new List<Vector2>();var indices=new List<ushort>();
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(mask[y*w+x])
            {
                int first=x;while(x+1<w && mask[y*w+x+1])x++;int v=vertices.Count;
                if(v+4>65535)throw new InvalidOperationException("Dragon silhouette mesh too large");
                vertices.Add(new Vector2(first,y));vertices.Add(new Vector2(x+1,y));vertices.Add(new Vector2(first,y+1));vertices.Add(new Vector2(x+1,y+1));
                indices.Add((ushort)v);indices.Add((ushort)(v+2));indices.Add((ushort)(v+1));indices.Add((ushort)(v+1));indices.Add((ushort)(v+2));indices.Add((ushort)(v+3));
            }
            // Editor model assertions run outside play mode, where Unity disallows
            // changing runtime sprite geometry. The native game applies the mask.
            if(Application.isPlaying)sprite.OverrideGeometry(vertices.ToArray(),indices.ToArray());
        }
        public Vector2 Mouth(Sprite sprite)
        {
            if(mouths.TryGetValue(sprite,out var anchor))return anchor;
            var r=sprite.rect;var data=Pixels(sprite.texture);int minX=(int)r.width;
            bool Beak(Color32 c)=>c.a>150 && c.r>155 && c.g>100 && c.b<100 && c.r>c.g*1.10f;
            for(int y=0;y<r.height;y++)for(int x=0;x<r.width*.40f;x++)if(Beak(data[((int)r.y+y)*sprite.texture.width+(int)r.x+x]))minX=Mathf.Min(minX,x);
            float sum=0,count=0;
            for(int y=0;y<r.height;y++)for(int x=minX;x<Mathf.Min(r.width,minX+8);x++)if(Beak(data[((int)r.y+y)*sprite.texture.width+(int)r.x+x])){sum+=y+.5f;count++;}
            float px=count>0?minX+2:r.width*.14f,py=count>0?sum/count:r.height*.66f;
            anchor=new Vector2((px-sprite.pivot.x)/sprite.pixelsPerUnit,(py-sprite.pivot.y)/sprite.pixelsPerUnit);mouths[sprite]=anchor;return anchor;
        }
    }
}
