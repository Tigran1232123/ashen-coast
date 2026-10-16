using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class SnailHouseV50
    {
        Vector3[] animatedVerticesV53;float[] headWeightsV53,sineWeightsV53;Sprite crawlV51;Vector2[] baseVerticesV51;ushort[] indicesV51;float withdrawalV51;SpriteRenderer shellClosedV51;Mesh crawlMeshV51;MeshRenderer crawlRendererV51;readonly MaterialPropertyBlock crawlBlockV51=new MaterialPropertyBlock();
        public float WithdrawalV51=>withdrawalV51;
        public Vector3[] HeadVerticesV51=>crawlMeshV51.vertices;
        public void PoseBodyV51(float clock)
        {
            var g=caravan.game;var source=caravan.shells[0];
            if(crawlV51==null)
            {
                var r=source.rect;crawlV51=Sprite.Create(source.texture,r,source.pivot/r.size,source.pixelsPerUnit,0,SpriteMeshType.FullRect);
                const int nx=40,ny=18;baseVerticesV51=new Vector2[(nx+1)*(ny+1)];var triangles=new List<ushort>();
                for(int y=0;y<=ny;y++)for(int x=0;x<=nx;x++){int i=y*(nx+1)+x;baseVerticesV51[i]=new Vector2((x/(float)nx*r.width-source.pivot.x)/source.pixelsPerUnit,(y/(float)ny*r.height-source.pivot.y)/source.pixelsPerUnit);if(x<nx && y<ny){triangles.AddRange(new[]{(ushort)i,(ushort)(i+1),(ushort)(i+nx+1),(ushort)(i+1),(ushort)(i+nx+2),(ushort)(i+nx+1)});}}
                indicesV51=triangles.ToArray();animatedVerticesV53=new Vector3[baseVerticesV51.Length];headWeightsV53=new float[baseVerticesV51.Length];sineWeightsV53=new float[baseVerticesV51.Length];for(int v=0;v<=ny;v++)for(int u=0;u<=nx;u++){int n=v*(nx+1)+u;headWeightsV53[n]=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.59f,.90f,u/(float)nx));sineWeightsV53[n]=Mathf.Sin(v/(float)ny*Mathf.PI);}
                var body=new GameObject("Animated snail body mesh");body.transform.SetParent(Renderer.transform,false);crawlMeshV51=new Mesh{name="Snail head and travelling foot"};var uv=new Vector2[baseVerticesV51.Length];var colors=new Color[uv.Length];for(int i=0;i<uv.Length;i++){var p=baseVerticesV51[i]*source.pixelsPerUnit+source.pivot;uv[i]=new Vector2((r.x+p.x)/source.texture.width,(r.y+p.y)/source.texture.height);colors[i]=Color.white;}crawlMeshV51.vertices=baseVerticesV51.Select(v=>(Vector3)v).ToArray();crawlMeshV51.uv=uv;crawlMeshV51.colors=colors;crawlMeshV51.triangles=indicesV51.Select(v=>(int)v).ToArray();body.AddComponent<MeshFilter>().sharedMesh=crawlMeshV51;crawlRendererV51=body.AddComponent<MeshRenderer>();crawlRendererV51.sharedMaterial=g.art.lit;Renderer.forceRenderingOff=true;
                shellClosedV51=g.art.Renderer("Snail shell withdrawal",caravan.shells[1],position,0);shellClosedV51.transform.SetParent(g.coastRoot.transform,true);
            }
            withdrawalV51=Mathf.MoveTowards(withdrawalV51,Hidden?1:0,Time.deltaTime/(Hidden?.95f:1.25f));
            var vertices=animatedVerticesV53;var rect=source.rect;float width=rect.width/source.pixelsPerUnit,height=rect.height/source.pixelsPerUnit;
            float nod=(moving?Mathf.Sin(clock*3.8f+index*.6f):Mathf.Sin(clock*1.4f+index)*.3f)*(1-withdrawalV51);
            for(int i=0;i<vertices.Length;i++)
            {
                var p=baseVerticesV51[i];float u=(p.x+source.pivot.x/source.pixelsPerUnit)/width,v=(p.y+source.pivot.y/source.pixelsPerUnit)/height;float head=headWeightsV53[i];
                p.x-=Mathf.Max(0,u-.59f)*width*withdrawalV51*.97f;
                p.y+=head*height*.035f*nod*sineWeightsV53[i];p.y-=head*withdrawalV51*height*.20f*sineWeightsV53[i];
                if(moving && v<.16f)p.y+=Mathf.Sin(u*30-clock*7)*height*.006f*Mathf.Sin(v*Mathf.PI/.16f);
                vertices[i]=p;
            }
            crawlMeshV51.vertices=vertices;crawlMeshV51.bounds=new Bounds(source.bounds.center,new Vector3(source.bounds.size.x*1.15f,source.bounds.size.y*1.2f,1));Renderer.sprite=crawlV51;float scale=8/source.bounds.size.x;Renderer.transform.localScale=Vector3.one*scale;Renderer.transform.position=position;Renderer.sortingOrder=CoastGame.Depth(position.y);Renderer.color=new Color(1,1,1,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.7f,1,withdrawalV51)));g.art.PixelStyle(Renderer);crawlRendererV51.transform.localScale=new Vector3(Renderer.flipX?-1:1,1,1);crawlRendererV51.sortingOrder=Renderer.sortingOrder;Renderer.GetPropertyBlock(crawlBlockV51);crawlBlockV51.SetColor("_Color",Renderer.color);crawlRendererV51.SetPropertyBlock(crawlBlockV51);
            shellClosedV51.flipX=Renderer.flipX;shellClosedV51.transform.position=position+new Vector2(Renderer.flipX?1.12f:-1.12f,0);shellClosedV51.transform.localScale=Vector3.one*scale;shellClosedV51.sortingOrder=Renderer.sortingOrder+1;shellClosedV51.color=new Color(1,1,1,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.5f,1,withdrawalV51)));g.art.PixelStyle(shellClosedV51);
        }
    }
    public sealed partial class SnailHomesV50
    {
        readonly Dictionary<int,SpriteRenderer> amazonDecorV51=new Dictionary<int,SpriteRenderer>(),bowDecorV51=new Dictionary<int,SpriteRenderer>();
        readonly Dictionary<int,GameObject> spareBedV51=new Dictionary<int,GameObject>();
        Sprite[] furnitureV53;
        readonly Dictionary<int,List<SpriteRenderer>> furnitureObjectsV53=new Dictionary<int,List<SpriteRenderer>>();
        public Vector2 RoomDoorV51(int home)=>new Vector2(home==1?1.38f:home==3?1.58f:1.48f,home==0?.13f:home==1?-.27f:home==2?-.54f:-.34f);
        Sprite RoomSpriteV53(string name)
        {var t=game.art.Load(name);return game.art.Slice(t,new RectInt(0,0,t.width,t.height),new Vector2(.5f,0),200);}
        void LoadFurnitureV53()
        {
            if(furnitureV53!=null)return;var t=game.art.Load("ShellFurnitureV53");
            // Measured individual object bounds, not equal grid cells.
            var rects=new[]{new RectInt(45,5,355,520),new RectInt(430,140,375,390),new RectInt(840,155,365,370),new RectInt(1290,255,195,250),new RectInt(25,680,425,300),new RectInt(465,520,280,480),new RectInt(825,560,260,420),new RectInt(1180,700,330,290)};
            furnitureV53=rects.Select(r=>game.art.Slice(t,r,new Vector2(.5f,0),200)).ToArray();
        }
        SpriteRenderer RoomPropV53(SnailHouseV50 h,string name,int item,Vector2 at,float width,bool solid=true,int order=-999)
        {
            var r=Prop(h,name,furnitureV53[item],at,width,order);game.art.PixelStyle(r);
            // Furnishings keep their fine drawn detail instead of a second coarse sampling pass.
            var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);block.SetFloat("_ArtStep",1);r.SetPropertyBlock(block);
            if(solid){var box=r.gameObject.AddComponent<BoxCollider2D>();float depth=Mathf.Clamp(r.bounds.size.y*.46f,.4f,1.05f);box.size=new Vector2(r.sprite.bounds.size.x*.90f,depth/r.transform.localScale.y);box.offset=new Vector2(0,depth*.5f/r.transform.localScale.y);}
            furnitureObjectsV53[h.index].Add(r);return r;
        }
        public void FurnishV51(SnailHouseV50 h)
        {
            LoadFurnitureV53();furnitureObjectsV53[h.index]=new List<SpriteRenderer>();
            var names=new[]{"ShellAmberV53","ShellIvoryV53","ShellTerraV53","ShellVioletV53"};
            var walls=Prop(h,"Fine carved shell interior "+h.index,RoomSpriteV53(names[h.index]),new Vector2(0,-4.15f),12,-3500);game.art.PixelStyle(walls);var block=new MaterialPropertyBlock();walls.GetPropertyBlock(block);block.SetFloat("_ArtStep",1);walls.SetPropertyBlock(block);
            floorsV58[h.index]=new FloorPlanV58(walls,FloorPlanV58.Snails[h.index]);
            RoomPropV53(h,"Woven spiral rug",4,new Vector2(.65f,-2.9f),2.2f,false,-3400);
            RoomPropV53(h,"Writing desk against right wall",2,new Vector2(3.45f,-2.65f),1.55f);
            RoomPropV53(h,"Separate desk stool",3,new Vector2(2.4f,-2.85f),.55f);
            h.personal.Add(RoomPropV53(h,"Owner bed against left wall",1,new Vector2(-2.45f,-1.45f),1.75f).gameObject);
            for(int i=0;i<2;i++)h.barracks.Add(RoomPropV53(h,"Three-tier troop bed",0,new Vector2(i==0?-2.7f:3.3f,-1.25f),1.65f).gameObject);
            spareBedV51[h.index]=RoomPropV53(h,"Owner additional bed",1,new Vector2(-.95f,-2.8f),1.35f).gameObject;
            exit.Add(RoomPropV53(h,"Wooden door aligned with shell arch",6,RoomDoorV51(h.index),.92f,false));
            amazonDecorV51[h.index]=RoomPropV53(h,"Resident spear and axe rack",5,new Vector2(-3.5f,-1.1f),.85f);
            bowDecorV51[h.index]=Prop(h,"Resident bow in wall corner",game.art.icons[(int)ItemKind.Bow],new Vector2(3.1f,-.3f),.65f);bowDecorV51[h.index].transform.rotation=Quaternion.Euler(0,0,-12);
            game.buildings.CreateStation(ItemKind.StoragePot,new Vector2Int(1,-3),false,null,h.room.transform);
        }
        public void DecorateV51(SnailHouseV50 h)
        {
            bool hasAmazon=troops.Values.Any(t=>t.home==h.index && Living(t) && Woman(t.key)!=null && !Woman(t.key).IsSlave);
            bool hasBow=troops.Values.Any(t=>t.home==h.index && Living(t) && Woman(t.key)?.Weapon?.kind==ItemKind.Bow);
            amazonDecorV51[h.index].enabled=hasAmazon;bowDecorV51[h.index].enabled=hasBow;spareBedV51[h.index].SetActive(PlayerCount>1 && Personal(h.index));
        }
    }
}
