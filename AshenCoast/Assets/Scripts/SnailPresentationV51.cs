using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class SnailHouseV50
    {
        Sprite crawlV51;Vector2[] baseVerticesV51;ushort[] indicesV51;float withdrawalV51;SpriteRenderer shellClosedV51;Mesh crawlMeshV51;MeshRenderer crawlRendererV51;readonly MaterialPropertyBlock crawlBlockV51=new MaterialPropertyBlock();
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
                indicesV51=triangles.ToArray();
                var body=new GameObject("Animated snail body mesh");body.transform.SetParent(Renderer.transform,false);crawlMeshV51=new Mesh{name="Snail head and travelling foot"};var uv=new Vector2[baseVerticesV51.Length];var colors=new Color[uv.Length];for(int i=0;i<uv.Length;i++){var p=baseVerticesV51[i]*source.pixelsPerUnit+source.pivot;uv[i]=new Vector2((r.x+p.x)/source.texture.width,(r.y+p.y)/source.texture.height);colors[i]=Color.white;}crawlMeshV51.vertices=baseVerticesV51.Select(v=>(Vector3)v).ToArray();crawlMeshV51.uv=uv;crawlMeshV51.colors=colors;crawlMeshV51.triangles=indicesV51.Select(v=>(int)v).ToArray();body.AddComponent<MeshFilter>().sharedMesh=crawlMeshV51;crawlRendererV51=body.AddComponent<MeshRenderer>();crawlRendererV51.sharedMaterial=g.art.lit;Renderer.forceRenderingOff=true;
                shellClosedV51=g.art.Renderer("Snail shell withdrawal",caravan.shells[1],position,0);shellClosedV51.transform.SetParent(g.coastRoot.transform,true);
            }
            withdrawalV51=Mathf.MoveTowards(withdrawalV51,Hidden?1:0,Time.deltaTime/(Hidden?.95f:1.25f));
            var vertices=new Vector3[baseVerticesV51.Length];var rect=source.rect;float width=rect.width/source.pixelsPerUnit,height=rect.height/source.pixelsPerUnit;
            for(int i=0;i<vertices.Length;i++)
            {
                var p=baseVerticesV51[i];float u=(p.x+source.pivot.x/source.pixelsPerUnit)/width,v=(p.y+source.pivot.y/source.pixelsPerUnit)/height;float head=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.59f,.90f,u));
                float nod=(moving?Mathf.Sin(clock*3.8f+index*.6f):Mathf.Sin(clock*1.4f+index)*.3f)*(1-withdrawalV51);
                p.x-=Mathf.Max(0,u-.59f)*width*withdrawalV51*.97f;
                p.y+=head*height*.035f*nod*Mathf.Sin(v*Mathf.PI);p.y-=head*withdrawalV51*height*.20f*Mathf.Sin(v*Mathf.PI);
                if(moving && v<.16f)p.y+=Mathf.Sin(u*30-clock*7)*height*.006f*Mathf.Sin(v*Mathf.PI/.16f);
                vertices[i]=p;
            }
            crawlMeshV51.vertices=vertices;crawlMeshV51.RecalculateBounds();Renderer.sprite=crawlV51;float scale=8/source.bounds.size.x;Renderer.transform.localScale=Vector3.one*scale;Renderer.transform.position=position;Renderer.sortingOrder=CoastGame.Depth(position.y);Renderer.color=new Color(1,1,1,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.7f,1,withdrawalV51)));g.art.PixelStyle(Renderer);crawlRendererV51.transform.localScale=new Vector3(Renderer.flipX?-1:1,1,1);crawlRendererV51.sortingOrder=Renderer.sortingOrder;Renderer.GetPropertyBlock(crawlBlockV51);crawlBlockV51.SetColor("_Color",Renderer.color);crawlRendererV51.SetPropertyBlock(crawlBlockV51);
            shellClosedV51.flipX=Renderer.flipX;shellClosedV51.transform.position=position+new Vector2(Renderer.flipX?1.12f:-1.12f,0);shellClosedV51.transform.localScale=Vector3.one*scale;shellClosedV51.sortingOrder=Renderer.sortingOrder+1;shellClosedV51.color=new Color(1,1,1,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.5f,1,withdrawalV51)));g.art.PixelStyle(shellClosedV51);
        }
    }
    public sealed partial class SnailHomesV50
    {
        readonly Dictionary<int,SpriteRenderer> amazonDecorV51=new Dictionary<int,SpriteRenderer>(),bowDecorV51=new Dictionary<int,SpriteRenderer>();
        readonly Dictionary<int,GameObject> spareBedV51=new Dictionary<int,GameObject>();
        public Vector2 RoomDoorV51(int home)=>new Vector2(.8f,-.4f);
        public void FurnishV51(SnailHouseV50 h)
        {
            var parent=h.room.transform;Prop(h,"Detailed spiral shell interior "+h.index,game.v51Art.rooms[h.index],new Vector2(0,-3.8f),10.8f,-3500);
            Prop(h,"Woven wool rug",furniture[5],new Vector2(.8f,-2.7f),2.0f,-3400);
            Prop(h,"Books and candle on a small desk",furniture[3],new Vector2(3.25f,-2.2f),1.35f);
            var bed=Prop(h,"Personal bed",furniture[1],new Vector2(-2.7f,-2.6f),1.7f);h.personal.Add(bed.gameObject);
            for(int i=0;i<2;i++)h.barracks.Add(Prop(h,"Detailed triple bunk bed",furniture[0],new Vector2(i==0?-3.0f:2.55f,-1.4f),1.65f).gameObject);
            spareBedV51[h.index]=Prop(h,"Owner's additional bed",furniture[1],new Vector2(-1.15f,-2.7f),1.5f).gameObject;
            var doors=furniture[4];exit.Add(Prop(h,"Door in the shell's back wall",doors,RoomDoorV51(h.index),1.35f));
            var atlas=game.art.Load("AmazonFurnishingsV48");var rack=game.art.Slice(atlas,new RectInt(0,atlas.height/3,atlas.width/3,atlas.height/3),new Vector2(.5f,0),200);
            amazonDecorV51[h.index]=Prop(h,"Amazon spear and axe rack",rack,new Vector2(-3.5f,-1.0f),1.0f);
            bowDecorV51[h.index]=Prop(h,"Archer's bow in the corner",game.art.icons[(int)ItemKind.Bow],new Vector2(3.0f,-.55f),.65f);bowDecorV51[h.index].transform.rotation=Quaternion.Euler(0,0,-12);
            game.buildings.CreateStation(ItemKind.StoragePot,new Vector2Int(-3,-2),false,null,parent);
        }
        public void DecorateV51(SnailHouseV50 h)
        {
            bool hasAmazon=troops.Values.Any(t=>t.home==h.index && Living(t) && Woman(t.key)!=null && !Woman(t.key).IsSlave);
            bool hasBow=troops.Values.Any(t=>t.home==h.index && Living(t) && Woman(t.key)?.Weapon?.kind==ItemKind.Bow);
            amazonDecorV51[h.index].enabled=hasAmazon;bowDecorV51[h.index].enabled=hasBow;spareBedV51[h.index].SetActive(PlayerCount>1 && Personal(h.index));
        }
    }
}
