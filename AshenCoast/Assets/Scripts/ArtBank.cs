using System.Collections.Generic;
using UnityEngine;

namespace AshenCoast
{
    public sealed class ArtBank
    {
        public const float PrismVisualScale=.70f;
        public Sprite ship, mannequin, door, galaxy, planet;
        public readonly Sprite[] icons = new Sprite[58], flowers = new Sprite[11];
        public Sprite orbCloth;
        public readonly Sprite[] skateAttachments=new Sprite[2];
        public readonly Sprite[] ruins=new Sprite[6];
        public readonly Sprite[] woodlouse=new Sprite[32];
        public readonly Mesh[] flowerMeshes=new Mesh[11];
        public readonly Sprite[,] hero = new Sprite[8, 8],frog=new Sprite[4,8];
        public readonly Sprite[,] lowerPoses=new Sprite[4,9];
        public readonly Sprite[] flame=new Sprite[16],explosion=new Sprite[24];
        public Sprite tentacle;
        public readonly Material lit;
        readonly Dictionary<Texture2D, Color32[]> pixels = new Dictionary<Texture2D, Color32[]>();
        readonly Dictionary<Sprite,bool[]> hitMasks=new Dictionary<Sprite,bool[]>();
        public void RegisterHitMask(Sprite sprite,bool[] mask)=>hitMasks[sprite]=mask;
        public bool[] HitMask(Sprite sprite)=>hitMasks[sprite];
        public void RemoveHitMask(Sprite sprite)=>hitMasks.Remove(sprite);
        readonly Dictionary<Sprite,Vector2[]> hands=new Dictionary<Sprite,Vector2[]>();
        readonly MaterialPropertyBlock style=new MaterialPropertyBlock();
        public static RenderTexture OrbIcon;
        public static Sprite OrbBase;
        public static RenderTexture PrismIcon;
        public static Sprite PrismBase;
        public static RenderTexture SpectrumIcon;public static Sprite SpectrumBase;Material spectrumMaterial;
        Material prismMaterial;
        public Sprite tableUpright,campfireBase;
        public static float ClosedSpatialAspect=1,OpenSpatialAspect=1.8f;
        Material orbMaterial;

        public ArtBank()
        {
            lit = new Material(Shader.Find("AshenCoast/WorldSprite"));
            var shipTexture = Load("Ship");
            ship = Slice(shipTexture, new RectInt(0, 0, shipTexture.width, shipTexture.height), new Vector2(.5f, 0), 110);
            var h = Load("HeroWalkV14");
            var rowBands=Bands(h,true,6);var colBands=Bands(h,false,8);
            int[] directions={0,1,2,3,4,3,2,5};
            for (int direction = 0; direction < 8; direction++) for (int col = 0; col < 8; col++)
            {
                int row=directions[direction];
                float heroPPU=(rowBands[row].y-rowBands[row].x+1)/1.65f;
                int x=colBands[col].x-3,y=h.height-rowBands[row].y-3;
                int w=colBands[col].y-colBands[col].x+7,height=rowBands[row].y-rowBands[row].x+7;
                // Fixed row canvas and ground baseline; the torso is registered horizontally across the cycle.
                float sum=0,count=0;var data=pixels[h];
                for(int yy=y+Mathf.RoundToInt(height*.56f);yy<y+Mathf.RoundToInt(height*.79f);yy++)for(int xx=x;xx<x+w;xx++)
                    if(xx>=0 && xx<h.width && yy>=0 && yy<h.height && data[yy*h.width+xx].a>96){sum+=xx;count++;}
                float anchor=count>0?sum/count:x+w*.5f;
                hero[direction,col]=Sprite.Create(h,new Rect(x,y,w,height),new Vector2((anchor-x)/w,3f/height),heroPPU,0,SpriteMeshType.FullRect);
            }
            string[] lowerNames={"WalkLowerFrontV16","WalkLowerSideV16","WalkLowerBackV16","WalkLowerDiagonalV16"};
            for(int view=0;view<4;view++)
            {
                var sheet=Load(lowerNames[view]);var rows=Bands(sheet,true,2);
                for(int row=0;row<2;row++)
                {
                    var rb=rows[row];var columns=ColumnBands(sheet,rb.x,rb.y+1);
                    if(columns.Count!=5)throw new System.InvalidOperationException(sheet.name+" row "+row+" needs five complete paired poses; got "+columns.Count);
                    for(int col=0;col<5;col++)
                    {
                        int frame=row*5+col;if(frame>=9)continue;
                        var cb=columns[col];var pose=Slice(sheet,new RectInt(cb.x,rb.x,cb.y-cb.x+1,rb.y-rb.x+1),new Vector2(.5f,1),400);
                        var r=pose.rect;var data=pixels[sheet];float sum=0,count=0;
                        for(int y=(int)(r.y+r.height*.94f);y<r.yMax;y++)for(int x=(int)r.x;x<r.xMax;x++)
                            if(data[y*sheet.width+x].a>96){sum+=x+.5f;count++;}
                        float pelvis=count>0?sum/count:r.center.x;
                        lowerPoses[view,frame]=Sprite.Create(sheet,r,new Vector2((pelvis-r.x)/r.width,1),400,0,SpriteMeshType.FullRect);
                    }
                }
            }
            var frogSheet=Load("FrogMageV9");
            for(int row=0;row<4;row++)for(int col=0;col<8;col++)frog[row,col]=RegisteredCell(frogSheet,col,row,8,4,1.35f);
            var newItems=Load("ItemsV8");
            int[] itemOrder={20,21,22,23,24,25,26,27};
            for(int i=0;i<8;i++)icons[itemOrder[i]]=Slice(newItems,new RectInt(i%4*newItems.width/4,i/4*newItems.height/2,newItems.width/4,newItems.height/2),Vector2.one*.5f,240);
            var revisedItems=Load("ItemsV9a");var revisedRows=Bands(revisedItems,true,2);int[] revisedOrder={20,21,24,26,27,25};
            for(int row=0;row<2;row++)
            {
                var rb=revisedRows[row];var columns=ColumnBands(revisedItems,rb.x,rb.y+1);
                if(columns.Count!=3)throw new System.InvalidOperationException("Revised item row requires three complete sprites");
                for(int col=0;col<3;col++){var cb=columns[col];icons[revisedOrder[row*3+col]]=Slice(revisedItems,new RectInt(cb.x,rb.x,cb.y-cb.x+1,rb.y-rb.x+1),Vector2.one*.5f,240);}
            }
            var fireSheet=Load("FireV8");
            for(int i=0;i<16;i++)
            {
                flame[i]=RegisteredCell(fireSheet,i%8,i/8,8,4,1.35f);
                int cellWidth=fireSheet.width/8,cellHeight=fireSheet.height/4,column=i%8,row=2+i/8;
                var e=Slice(fireSheet,new RectInt(column*cellWidth,row*cellHeight,cellWidth,cellHeight),Vector2.one*.5f,240);var er=e.rect;
                explosion[i]=Sprite.Create(fireSheet,er,new Vector2((column*cellWidth+cellWidth*.5f-er.x)/er.width,(fireSheet.height-(row+.5f)*cellHeight-er.y)/er.height),240,0,SpriteMeshType.FullRect);
            }
            var blast=Load("PyromanticExplosionV32");
            float[] blastRows={0,.260f,.525f,.778f,1};
            for(int i=0;i<24;i++)
            {
                int row=i/6,left=Mathf.RoundToInt(i%6*blast.width/6f),right=Mathf.RoundToInt((i%6+1)*blast.width/6f),top=Mathf.RoundToInt(blastRows[row]*blast.height),bottom=Mathf.RoundToInt(blastRows[row+1]*blast.height);
                explosion[i]=Sprite.Create(blast,new Rect(left,blast.height-bottom,right-left,bottom-top),new Vector2(.5f,.10f),(right-left)/6f,0,SpriteMeshType.FullRect);
            }
            var tentacleTexture=Load("TentacleV8");tentacle=Slice(tentacleTexture,new RectInt(0,0,tentacleTexture.width,tentacleTexture.height),new Vector2(.5f,0),450);
            var t = Load("Items");
            var regions = new[]
            {
                new RectInt(0,0,324,337), new RectInt(328,0,332,337), new RectInt(664,0,293,337), new RectInt(962,0,299,337),
                new RectInt(0,390,350,190), new RectInt(350,400,273,179), new RectInt(624,382,319,244), new RectInt(944,417,317,170),
                new RectInt(0,625,318,309), new RectInt(327,624,304,310), new RectInt(634,625,324,310), new RectInt(968,624,293,310),
                new RectInt(0,938,313,309), new RectInt(331,935,299,312), new RectInt(667,935,291,312), new RectInt(972,935,289,312)
            };
            for (int i = 0; i < 16; i++) icons[i] = Slice(t, regions[i], i >= 4 && i <= 7 ? new Vector2(.045f, i == 6 ? .68f : .5f) : new Vector2(.5f,.5f), 100);
            var blade=Load("SwordV5");icons[4]=Slice(blade,new RectInt(0,0,blade.width,blade.height),new Vector2(.085f,.5f),100);
            var spear=icons[7];icons[7]=Sprite.Create(spear.texture,spear.rect,new Vector2(.32f,.5f),spear.pixelsPerUnit,0,SpriteMeshType.FullRect);
            var boots=Load("BootsV5");icons[19]=Slice(boots,new RectInt(0,0,boots.width,boots.height),new Vector2(.5f,.5f),100);
            var magic=Load("TomesV5");var bookBands=Bands(magic,false,2);
            for(int i=0;i<2;i++)icons[17+i]=Slice(magic,new RectInt(bookBands[i].x,0,bookBands[i].y-bookBands[i].x+1,magic.height),new Vector2(.5f,.5f),260);
            var openBook=Load("OpenTomeV6");icons[18]=Slice(openBook,new RectInt(0,0,openBook.width,openBook.height),new Vector2(.5f,.5f),260);
            ClosedSpatialAspect=icons[17].rect.width/icons[17].rect.height;OpenSpatialAspect=icons[18].rect.width/icons[18].rect.height;
            var animal=Load("WoodlouseV9");float animalPPU=animal.width/8f/2.65f;
            var animalRows=Bands(animal,true,4);
            for(int row=0;row<4;row++)
            {
                var rb=animalRows[row];var bands=ColumnBands(animal,rb.x,rb.y+1);int ground=animal.height-rb.y-1;
                if(bands.Count<7 || bands.Count>8)throw new System.InvalidOperationException("Woodlouse row "+row+" contains "+bands.Count+" frames");
                for(int col=0;col<bands.Count;col++)
                {var b=bands[col];var region=Slice(animal,new RectInt(b.x,rb.x,b.y-b.x+1,rb.y-rb.x+1),Vector2.one*.5f,animalPPU);ground=Mathf.Min(ground,(int)region.rect.y);}
                for(int col=0;col<8;col++)
                {
                    var b=bands[Mathf.Min(col,bands.Count-1)];var trimmed=Slice(animal,new RectInt(b.x,rb.x,b.y-b.x+1,rb.y-rb.x+1),Vector2.one*.5f,animalPPU);var r=trimmed.rect;
                    woodlouse[row*8+col]=Sprite.Create(animal,r,new Vector2(.5f,(ground-r.y)/r.height),animalPPU,0,SpriteMeshType.FullRect);
                }
            }
            var magicProps=Load("MagicPropsV3");
            var magicBands=Bands(magicProps,false,2);
            icons[16]=Slice(magicProps,new RectInt(magicBands[0].x,0,magicBands[0].y-magicBands[0].x+1,magicProps.height),new Vector2(.5f,.5f),260);
            OrbBase=icons[16];if(OrbIcon){OrbIcon.Release();Object.Destroy(OrbIcon);}OrbIcon=new RenderTexture(64,64,0,RenderTextureFormat.ARGB32){filterMode=FilterMode.Point};OrbIcon.Create();
            orbMaterial=new Material(Shader.Find("AshenCoast/OrbIcon"));var orbRect=OrbBase.rect;var orbTexture=OrbBase.texture;
            orbMaterial.SetVector("_OrbRect",new Vector4(orbRect.x/orbTexture.width,orbRect.y/orbTexture.height,orbRect.width/orbTexture.width,orbRect.height/orbTexture.height));UpdateOrb(0);
            door=Slice(magicProps,new RectInt(magicBands[1].x,0,magicBands[1].y-magicBands[1].x+1,magicProps.height),new Vector2(.5f,0),260);
            var cosmic=Load("CosmicSpritesV3");var cosmicBands=Bands(cosmic,false,2);
            galaxy=Slice(cosmic,new RectInt(cosmicBands[0].x,0,cosmicBands[0].y-cosmicBands[0].x+1,cosmic.height),new Vector2(.5f,.5f),100);
            planet=Slice(cosmic,new RectInt(cosmicBands[1].x,0,cosmicBands[1].y-cosmicBands[1].x+1,cosmic.height),new Vector2(.5f,.5f),100);
            var detailedGalaxy=Load("GalaxyV4");galaxy=Slice(detailedGalaxy,new RectInt(0,0,detailedGalaxy.width,detailedGalaxy.height),new Vector2(.5f,.5f),100);
            var rubble=Load("RuinsV4");
            var ruinRegions=new[]{new RectInt(0,0,490,464),new RectInt(490,0,490,464),new RectInt(980,0,556,464),new RectInt(0,464,490,560),new RectInt(490,464,420,560),new RectInt(910,464,626,560)};
            for(int i=0;i<6;i++)ruins[i]=Slice(rubble,ruinRegions[i],new Vector2(.5f,0),150);
            var props = Load("Props");
            mannequin = Slice(props, new RectInt(0,0,626,715), new Vector2(.5f,0), 305);
            var blooms=Load("FlowersV3");var bloomRows=Bands(blooms,true,2);var bloomColumns=Bands(blooms,false,3);
            for(int row=0;row<2;row++)for(int col=0;col<3;col++)
            {
                flowers[row*3+col]=Slice(blooms,new RectInt(bloomColumns[col].x,bloomRows[row].x,bloomColumns[col].y-bloomColumns[col].x+1,bloomRows[row].y-bloomRows[row].x+1),new Vector2(.5f,0),(bloomRows[row].y-bloomRows[row].x)/1.05f);
                flowerMeshes[row*3+col]=BendableMesh(flowers[row*3+col]);
                if(row==0)icons[13+col]=flowers[col];
            }
            var prism=Load("LastPrismV24c");PrismBase=icons[30]=Slice(prism,new RectInt(0,0,prism.width,prism.height),Vector2.one*.5f,260);
            if(PrismIcon){PrismIcon.Release();Object.Destroy(PrismIcon);}PrismIcon=new RenderTexture(64,64,0,RenderTextureFormat.ARGB32){filterMode=FilterMode.Point};PrismIcon.Create();
            prismMaterial=new Material(Shader.Find("AshenCoast/PrismIcon"));var pr=PrismBase.rect;
            prismMaterial.SetVector("_PrismRect",new Vector4(pr.x/prism.width,pr.y/prism.height,pr.width/prism.width,pr.height/prism.height));UpdateOrb(0);
            string[] buildItems={"RawMeatV28","CraftingTableV28","IronIngotV28","AnvilV31"};
            for(int i=0;i<buildItems.Length;i++){var item=Load(buildItems[i]);icons[31+i]=Slice(item,new RectInt(0,0,item.width,item.height),Vector2.one*.5f,240);}
            var upright=Load("CraftingTableUprightV31");tableUpright=Slice(upright,new RectInt(0,0,upright.width,upright.height),Vector2.one*.5f,240);
            var logs=Load("CampfireBaseV31");campfireBase=Slice(logs,new RectInt(0,0,logs.width,logs.height),Vector2.one*.5f,240);
            var kitchen=Load("KitchenItemsV34");for(int i=0;i<3;i++)icons[37+i]=Slice(kitchen,new RectInt(i%2*kitchen.width/2,i/2*kitchen.height/2,kitchen.width/2,kitchen.height/2),Vector2.one*.5f,240);
            var clay=Load("ClayV33");icons[35]=Slice(clay,new RectInt(0,0,clay.width,clay.height),Vector2.one*.5f,240);
            var pot=Load("StoragePotV33");icons[36]=Slice(pot,new RectInt(0,0,pot.width,pot.height),Vector2.one*.5f,240);
            var skates=Load("RollerSkatesV35");icons[40]=Slice(skates,new RectInt(0,0,skates.width,skates.height),Vector2.one*.5f,240);
            for(int i=0;i<2;i++)skateAttachments[i]=Slice(skates,new RectInt(i*skates.width/2,0,skates.width/2,skates.height),Vector2.one*.5f,240);
            var yoyo=Load("BattleYoyoV35");icons[20]=Slice(yoyo,new RectInt(0,0,yoyo.width,yoyo.height),Vector2.one*.5f,240);
            var cloth=Load("OrbClothV35");orbCloth=Slice(cloth,new RectInt(0,0,cloth.width,cloth.height),Vector2.one*.5f,240);
        }

        Sprite RegisteredCell(Texture2D texture,int column,int row,int columns,int rows,float worldHeight)
        {
            int w=texture.width/columns,h=texture.height/rows;var trimmed=Slice(texture,new RectInt(column*w,row*h,w,h),new Vector2(.5f,0),(h-18)/worldHeight);
            var r=trimmed.rect;float pivotX=(column*w+w*.5f-r.x)/r.width;
            // Register each direction to a shared ground line instead of the changing robe/foot crop.
            float ground=texture.height-(row+1)*h+6;
            return Sprite.Create(texture,r,new Vector2(pivotX,(ground-r.y)/r.height),(h-18)/worldHeight,0,SpriteMeshType.FullRect);
        }
        List<Vector2Int> RowBands(Texture2D texture,int row,int rows)
        {
            var result=ColumnBands(texture,row*texture.height/rows,(row+1)*texture.height/rows);
            if(result.Count<7 || result.Count>8)throw new System.InvalidOperationException("Woodlouse row "+row+" contains "+result.Count+" frames");return result;
        }
        List<Vector2Int> ColumnBands(Texture2D texture,int fromTop,int toTop)
        {
            var result=new List<Vector2Int>();var data=pixels[texture];int bottom=texture.height-toTop,top=texture.height-fromTop,start=-1,last=-1;
            for(int x=0;x<texture.width;x++)
            {
                int count=0;for(int y=bottom;y<top;y++)if(data[y*texture.width+x].a>96)count++;
                if(count>2){if(start<0)start=x;last=x;}else if(start>=0 && x-last>3){result.Add(new Vector2Int(start,last));start=-1;}
            }
            if(start>=0)result.Add(new Vector2Int(start,last));return result;
        }

        public static Mesh BendableMesh(Sprite sprite)
        {
            const int rows=8;var v=new Vector3[(rows+1)*2];var uv=new Vector2[v.Length];var colors=new Color[v.Length];var indices=new int[rows*6];
            for(int y=0;y<=rows;y++)for(int x=0;x<2;x++)
            {
                int i=y*2+x;float px=x*sprite.rect.width,py=y*sprite.rect.height/rows;
                v[i]=new Vector3((px-sprite.pivot.x)/sprite.pixelsPerUnit,(py-sprite.pivot.y)/sprite.pixelsPerUnit,0);
                uv[i]=new Vector2((sprite.rect.x+px)/sprite.texture.width,(sprite.rect.y+py)/sprite.texture.height);colors[i]=Color.white;
            }
            for(int y=0;y<rows;y++){int t=y*6,b=y*2;indices[t]=b;indices[t+1]=b+2;indices[t+2]=b+1;indices[t+3]=b+1;indices[t+4]=b+2;indices[t+5]=b+3;}
            var mesh=new Mesh{name="Eight-segment anchored flower stem"};mesh.vertices=v;mesh.uv=uv;mesh.colors=colors;mesh.triangles=indices;mesh.RecalculateBounds();var bounds=mesh.bounds;bounds.Expand(new Vector3(.6f,0,0));mesh.bounds=bounds;return mesh;
        }

        public MeshRenderer FlowerWind(SpriteRenderer anchor,int variant,float phase)
        {
            var child=new GameObject("Flexible flower stems");child.transform.SetParent(anchor.transform,false);
            child.AddComponent<MeshFilter>().sharedMesh=flowerMeshes[variant];var renderer=child.AddComponent<MeshRenderer>();renderer.sharedMaterial=lit;renderer.sortingOrder=anchor.sortingOrder;
            anchor.GetPropertyBlock(style);style.SetTexture("_MainTex",anchor.sprite.texture);style.SetFloat("_WindStrength",.13f);style.SetFloat("_WindHeight",anchor.sprite.bounds.size.y);style.SetFloat("_WindPhase",phase);renderer.SetPropertyBlock(style);
            // Keep the registered SpriteRenderer as the selection bounds; the subdivided mesh draws it.
            anchor.forceRenderingOff=true;return renderer;
        }

        public List<Vector2Int> Bands(Texture2D texture,bool horizontal,int expected)
        {
            var data=pixels[texture];int length=horizontal?texture.height:texture.width,other=horizontal?texture.width:texture.height;
            var bands=new List<Vector2Int>();int first=-1,last=-1;int bridge=texture.name.StartsWith("HeroWalk")?1:Mathf.Max(2,length/(expected*36));
            for(int i=0;i<length;i++)
            {
                int active=0;for(int j=0;j<other;j++){int x=horizontal?j:i,y=horizontal?texture.height-1-i:j;if(data[y*texture.width+x].a>96)active++;}
                if(active>3){if(first<0)first=i;last=i;}
                else if(first>=0 && i-last>bridge){bands.Add(new Vector2Int(first,last));first=-1;}
            }
            if(first>=0)bands.Add(new Vector2Int(first,last));
            if(bands.Count!=expected)throw new System.InvalidOperationException(texture.name+" needs "+expected+" sprite bands; got "+bands.Count);
            return bands;
        }

        public void PixelStyle(SpriteRenderer renderer)
        {
            var sprite=renderer.sprite;var r=sprite.rect;var t=sprite.texture;
            renderer.GetPropertyBlock(style);
            style.SetTexture("_MainTex",t);
            style.SetFloat("_ArtStep",Mathf.Max(1,sprite.pixelsPerUnit/(Mathf.Max(.001f,Mathf.Abs(renderer.transform.lossyScale.x))*40)));
            style.SetVector("_SpriteRect",new Vector4(r.x/t.width,r.y/t.height,r.width/t.width,r.height/t.height));
            renderer.SetPropertyBlock(style);
        }

        public Vector2 HandAnchor(Sprite sprite,bool right)
        {
            if(!hands.TryGetValue(sprite,out var pair))
            {
                pair=new Vector2[2];var r=sprite.rect;var data=pixels[sprite.texture];
                for(int side=0;side<2;side++)
                {
                    Vector2 sum=Vector2.zero;float count=0;
                    for(int y=(int)(r.height*.32f);y<r.height*.57f;y++)for(int x=0;x<r.width;x++)
                    {
                        float localX=(x-sprite.pivot.x)/sprite.pixelsPerUnit;if(side==0?localX>-.14f:localX<.14f)continue;
                        var p=data[((int)r.y+y)*sprite.texture.width+(int)r.x+x];
                        if(p.a<160 || p.r<145 || p.g<74 || p.r<p.g*1.18f || p.g<p.b*1.15f)continue;
                        sum+=new Vector2(localX,(y-sprite.pivot.y)/sprite.pixelsPerUnit);count++;
                    }
                    pair[side]=count>0?sum/count:new Vector2(side==0?-.29f:.29f,.66f);
                }
                hands.Add(sprite,pair);
            }
            return pair[right?1:0];
        }

        public Sprite Leg(Sprite source,int side,float top,out Vector2 joint)
        {
            var r=source.rect;var data=pixels[source.texture];int split=Mathf.RoundToInt(source.pivot.x);
            int from=side==0?0:split,to=side==0?split:(int)r.width,height=Mathf.RoundToInt(r.height*top);
            int minX=to,maxX=from,minY=height,maxY=0;float sum=0,count=0;
            for(int y=0;y<height;y++)for(int x=from;x<to;x++)
            {
                if(data[(y+(int)r.y)*source.texture.width+x+(int)r.x].a<=96)continue;
                minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);
                if(y>=height-6){sum+=x;count++;}
            }
            float hip=count>0?sum/count:(minX+maxX)*.5f;
            joint=new Vector2((hip-source.pivot.x)/source.pixelsPerUnit,(maxY+.5f-source.pivot.y)/source.pixelsPerUnit);
            return Sprite.Create(source.texture,new Rect(r.x+minX,r.y+minY,maxX-minX+1,maxY-minY+1),
                new Vector2((hip-minX)/(maxX-minX+1),1),source.pixelsPerUnit,0,SpriteMeshType.FullRect);
        }
        public Sprite WholeBootLeg(Sprite source,float top,out Vector2 joint)
        {
            var r=source.rect;var data=pixels[source.texture];int height=Mathf.RoundToInt(r.height*top),bottom=0;
            for(;bottom<height;bottom++){bool found=false;for(int x=0;x<r.width;x++)if(data[((int)r.y+bottom)*source.texture.width+(int)r.x+x].a>96){found=true;break;}if(found)break;}
            int minX=(int)r.width,maxX=0;
            // The lowest contact belongs to one entire boot, rather than two halves of that boot.
            for(int y=bottom;y<Mathf.Min(height,bottom+Mathf.Max(3,Mathf.RoundToInt(source.pixelsPerUnit*.05f)));y++)for(int x=0;x<r.width;x++)if(data[((int)r.y+y)*source.texture.width+(int)r.x+x].a>96){minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);}
            minX=Mathf.Max(0,minX-3);maxX=Mathf.Min((int)r.width-1,maxX+3);joint=new Vector2(0,(height-source.pivot.y)/source.pixelsPerUnit);
            return Sprite.Create(source.texture,new Rect(r.x+minX,r.y+bottom,maxX-minX+1,height-bottom),new Vector2(.5f,1),source.pixelsPerUnit,0,SpriteMeshType.FullRect);
        }
        public Mesh NeutralLegMesh(Sprite source)
        {
            var mesh=BendableMesh(source);var v=mesh.vertices;var rect=source.rect;var data=pixels[source.texture];
            // Remove the leaning contact pose baked into the source; the runtime knee supplies the motion.
            for(int row=0;row<=8;row++)
            {
                int yy=Mathf.Clamp(Mathf.RoundToInt(rect.height*row/8),0,(int)rect.height-1);float sum=0,count=0;
                for(int dy=-2;dy<=2;dy++){int y=Mathf.Clamp(yy+dy,0,(int)rect.height-1);for(int x=0;x<rect.width;x++)if(data[(y+(int)rect.y)*source.texture.width+x+(int)rect.x].a>96){sum+=x+.5f;count++;}}
                float shift=count>0?(sum/count-source.pivot.x)/source.pixelsPerUnit:0;
                v[row*2].x-=shift;v[row*2+1].x-=shift;
            }
            mesh.vertices=v;mesh.RecalculateBounds();var bounds=mesh.bounds;bounds.Expand(new Vector3(.6f,.3f,0));mesh.bounds=bounds;return mesh;
        }

        public Texture2D Load(string name)
        {
            var t = Resources.Load<Texture2D>("Art/" + name);
            if (!t) throw new System.InvalidOperationException("Missing art: " + name);
            t.filterMode = FilterMode.Point;
            pixels[t] = t.GetPixels32(); return t;
        }
        public Sprite Slice(Texture2D t, RectInt top, Vector2 pivot, float ppu,SpriteMeshType meshType=SpriteMeshType.FullRect)
        {
            int minX = t.width, minY = t.height, maxX = -1, maxY = -1;
            int y0 = t.height - top.y - top.height;
            var a = pixels[t];
            for (int y = Mathf.Max(0,y0); y < Mathf.Min(t.height,y0+top.height); y++)
                for (int x = Mathf.Max(0,top.x); x < Mathf.Min(t.width,top.x+top.width); x++)
                    if (a[y*t.width+x].a > 64) { minX=Mathf.Min(minX,x); maxX=Mathf.Max(maxX,x); minY=Mathf.Min(minY,y); maxY=Mathf.Max(maxY,y); }
            if (maxX < minX) throw new System.InvalidOperationException("Empty sprite region");
            var s = Sprite.Create(t, new Rect(minX,minY,maxX-minX+1,maxY-minY+1), pivot, ppu, 0, meshType);
            s.name = t.name + "_" + top.x + "_" + top.y; return s;
        }
        public bool Opaque(Sprite sprite, Vector2 local)
        {
            int x = Mathf.FloorToInt(local.x * sprite.pixelsPerUnit + sprite.pivot.x);
            int y = Mathf.FloorToInt(local.y * sprite.pixelsPerUnit + sprite.pivot.y);
            var r = sprite.rect;
            if (x < 0 || y < 0 || x >= r.width || y >= r.height) return false;
            if(hitMasks.TryGetValue(sprite,out var mask) && !mask[y*(int)r.width+x])return false;
            return pixels[sprite.texture][(y+(int)r.y)*sprite.texture.width+x+(int)r.x].a > 96;
        }
        public bool SpriteContact(SpriteRenderer weapon, SpriteRenderer target,float minimumTargetY=.52f)
        {
            if (!weapon.bounds.Intersects(target.bounds)) return false;
            var s = weapon.sprite; var r = s.rect; var data = pixels[s.texture];
            int step = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(r.width,r.height) / 80));
            for (int y = 0; y < (int)r.height; y += step) for (int x = 0; x < (int)r.width; x += step)
            {
                if (data[(y+(int)r.y)*s.texture.width+x+(int)r.x].a <= 96) continue;
                var local = new Vector3((x+.5f-s.pivot.x)/s.pixelsPerUnit, (y+.5f-s.pivot.y)/s.pixelsPerUnit,0);
                Vector3 world = weapon.transform.TransformPoint(local);
                var targetLocal = target.transform.InverseTransformPoint(world);
                // Only the straw body and head receive hits; the visible wooden stand is scenery.
                if (targetLocal.y < minimumTargetY) continue;
                if (target.flipX)targetLocal.x=-targetLocal.x;
                if (target.flipY)targetLocal.y=-targetLocal.y;
                if (Opaque(target.sprite,targetLocal)) return true;
            }
            return false;
        }
        public SpriteRenderer Renderer(string name, Sprite sprite, Vector2 position, int order)
        {
            var go = new GameObject(name); go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite=sprite; sr.sharedMaterial=lit; sr.sortingOrder=order;
            PixelStyle(sr);return sr;
        }
        public void ConfigureSpectrumIcon()
        {
            SpectrumBase=icons[43];if(SpectrumIcon){SpectrumIcon.Release();Object.Destroy(SpectrumIcon);}SpectrumIcon=new RenderTexture(128,64,0,RenderTextureFormat.ARGB32){filterMode=FilterMode.Point};SpectrumIcon.Create();
            spectrumMaterial=new Material(Shader.Find("AshenCoast/SpectrumIcon"));var r=SpectrumBase.rect;var t=SpectrumBase.texture;spectrumMaterial.SetVector("_SpectrumRect",new Vector4(r.x/t.width,r.y/t.height,r.width/t.width,r.height/t.height));
        }
        public void UpdateOrb(float clock)
        {var previous=RenderTexture.active;Shader.SetGlobalFloat("_OrbClock",clock);orbMaterial.SetFloat("_OrbClock",clock);Graphics.Blit(OrbBase.texture,OrbIcon,orbMaterial);if(prismMaterial){Shader.SetGlobalFloat("_PrismClock",clock);prismMaterial.SetFloat("_PrismClock",clock);Graphics.Blit(PrismBase.texture,PrismIcon,prismMaterial);}if(spectrumMaterial){spectrumMaterial.SetFloat("_SpectrumClock",clock);Graphics.Blit(SpectrumBase.texture,SpectrumIcon,spectrumMaterial);}RenderTexture.active=previous;}
        public void AddStoneFootprints(SpriteRenderer stone,float depth=.24f)
        {
            var s=stone.sprite;var r=s.rect;var data=pixels[s.texture];float scale=Mathf.Abs(stone.transform.lossyScale.x);
            int width=(int)r.width,band=Mathf.Min((int)r.height,Mathf.CeilToInt(depth*s.pixelsPerUnit/scale));var occupied=new bool[width];
            for(int x=0;x<width;x++)for(int y=0;y<band;y++)if(data[((int)r.y+y)*s.texture.width+(int)r.x+x].a>96){occupied[x]=true;break;}
            int gap=Mathf.CeilToInt(.07f*s.pixelsPerUnit/scale);
            for(int x=1;x<width-1;x++)if(!occupied[x] && occupied[x-1]){int end=x;while(end<width && !occupied[end])end++;if(end<width && end-x<=gap)for(int fill=x;fill<end;fill++)occupied[fill]=true;}
            for(int x=0;x<width;x++)if(occupied[x])
            {
                int start=x;while(x+1<width && occupied[x+1])x++;float worldWidth=(x-start+1)/s.pixelsPerUnit*scale;if(worldWidth<.06f)continue;
                int midX=(start+x)/2;var solidRows=new List<int>();
                for(int y=0;y<band;y++)if(data[((int)r.y+y)*s.texture.width+(int)r.x+midX].a>96)solidRows.Add(y);
                if(solidRows.Count==0){midX=start;for(int y=0;y<band;y++)if(data[((int)r.y+y)*s.texture.width+(int)r.x+midX].a>96)solidRows.Add(y);}
                if(solidRows.Count==0)continue;int midY=solidRows[solidRows.Count/2];
                float height=Mathf.Clamp((solidRows[solidRows.Count-1]-solidRows[0]+1)/s.pixelsPerUnit,.06f/scale,depth/scale);
                var go=new GameObject("Ruin footing fitted to opaque stone base");go.transform.SetParent(stone.transform,false);var collider=go.AddComponent<BoxCollider2D>();
                collider.size=new Vector2((x-start+1)/s.pixelsPerUnit,height);collider.offset=new Vector2((midX+.5f-s.pivot.x)/s.pixelsPerUnit,(midY+.5f-s.pivot.y)/s.pixelsPerUnit);
            }
        }
        public static void DrawIcon(Sprite sprite, Rect area, Color color, bool vertical=false,float displayAspect=0)
        {
            if (!sprite) return;
            float aspect=displayAspect>0?displayAspect:sprite.rect.width/sprite.rect.height;
            float w=vertical?Mathf.Min(area.height,area.width*aspect):Mathf.Min(area.width,area.height*aspect), h=w/aspect;
            if(sprite==PrismBase){w*=PrismVisualScale;h*=PrismVisualScale;}
            Rect dst=new Rect(area.center.x-w*.5f,area.center.y-h*.5f,w,h);
            var r=sprite.rect; var t=sprite.texture;
            var old=GUI.matrix;if(vertical)RotateUI(-90,area.center);
            GUI.color=color;
            if(sprite==SpectrumBase && SpectrumIcon)GUI.DrawTextureWithTexCoords(dst,SpectrumIcon,new Rect(0,0,1,1));else if(sprite==PrismBase && PrismIcon)GUI.DrawTextureWithTexCoords(dst,PrismIcon,new Rect(0,0,1,1));else if(sprite==OrbBase && OrbIcon)GUI.DrawTextureWithTexCoords(dst,OrbIcon,new Rect(0,0,1,1));else GUI.DrawTextureWithTexCoords(dst,t,new Rect(r.x/t.width,r.y/t.height,r.width/t.width,r.height/t.height));GUI.color=Color.white;
            GUI.matrix=old;
        }
        public static void RotateUI(float degrees,Vector2 pivot)
        {
            GUI.matrix=GUI.matrix*Matrix4x4.Translate(pivot)*Matrix4x4.Rotate(Quaternion.Euler(0,0,degrees))*Matrix4x4.Translate(-pivot);
        }
    }
}
