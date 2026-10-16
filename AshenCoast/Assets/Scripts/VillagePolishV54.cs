using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public static class AtlasV54
    {
        // Alpha bands, rather than an assumed grid, keep every authored limb inside its frame.
        public static Sprite[,] Actors(ArtBank bank,string name,int rows,int columns,float height,bool width=false)
        {
            var t=bank.Load(name);var bands=bank.Bands(t,true,rows);var result=new Sprite[rows,columns];
            float ppu=0;var rects=new RectInt[rows,columns];
            for(int row=0;row<rows;row++)
            {
                var band=bands[row];var xs=bank.ColumnBands(t,band.x,band.y+1);
                if(xs.Count!=columns)throw new InvalidOperationException(name+" row "+row+" needs "+columns+" frames, got "+xs.Count);
                for(int col=0;col<columns;col++)
                {var x=xs[col];rects[row,col]=new RectInt(x.x,t.height-1-band.y,x.y-x.x+1,band.y-band.x+1);ppu=Mathf.Max(ppu,(width?x.y-x.x+1:band.y-band.x+1)/height);}
            }
            for(int row=0;row<rows;row++)for(int col=0;col<columns;col++)
            {var r=rects[row,col];var pivot=new Vector2(.5f,width?.5f:0);if(name=="AmazonCombatFramesV54")pivot=new Vector2((col*400+200-r.x)/(float)r.width,(t.height-1-(row*270+255)-r.y)/(float)r.height);result[row,col]=Sprite.Create(t,new Rect(r.x,r.y,r.width,r.height),pivot,ppu,0,SpriteMeshType.FullRect);result[row,col].name=name+" / "+row+" / "+col;}
            return result;
        }
    }
    public sealed partial class AmazonSettlementV46
    {
        public int LayoutSeedV54 {get;private set;}=Guid.NewGuid().GetHashCode();
        public Sprite[,] combatV54,captivesV54,leanV54,amazonLowerV58;
        GameObject pavingV54;readonly List<Vector2> roadNodesV54=new List<Vector2>();
        public IReadOnlyList<Vector2> RoadNodesV54=>roadNodesV54;
        void LoadArtV54()
        {amazonLowerV58=AtlasV54.Actors(game.art,"AmazonLowerV58",2,5,.8f);combatV54=AtlasV54.Actors(game.art,"AmazonCombatFramesV54",4,8,1.72f);captivesV54=AtlasV54.Actors(game.art,"CaptivesV54",4,8,1.65f);leanV54=AtlasV54.Actors(game.art,"SlaverLeanV54",1,6,1.72f);}
        public Vector2[] LayoutPointsV54(int seed)
        {
            var rng=new System.Random(seed);var positions=new List<Vector2>();
            positions.Add(Centre+new Vector2((float)rng.NextDouble()*12-6,29+(float)rng.NextDouble()*4));
            for(int index=1;index<10;index++)
            {
                Vector2 chosen=Centre;float best=-1;
                for(int tries=0;tries<1500;tries++)
                {
                    float angle=(float)rng.NextDouble()*Mathf.PI*2,radius=21+(float)rng.NextDouble()*15;
                    var p=Centre+new Vector2(Mathf.Sin(angle)*radius,Mathf.Cos(angle)*radius*.91f);
                    if(new Rect(Centre.x-20,Centre.y-25,18,10).Contains(p))continue;
                    float separation=positions.Select((v,i)=>Vector2.Distance(v,p)-(i==0?2:0)).Min();
                    if(separation>best){best=separation;chosen=p;}if(separation>9)break;
                }
                positions.Add(chosen);
            }
            return positions.ToArray();
        }
        public void ApplyLayoutSeedV54(int seed)
        {
            if(seed==LayoutSeedV54)return;LayoutSeedV54=seed;var points=LayoutPointsV54(seed);
            for(int i=0;i<houses.Count;i++)
            {
                var h=houses[i];var delta=points[i]-h.entrance;h.entrance=points[i];h.sprite.transform.position+=(Vector3)delta;h.sprite.sortingOrder=CoastGame.Depth(h.entrance.y);
                h.foundationV54.transform.position+=(Vector3)delta;var b=barriers[h.barrierIndexV54];b.position+=delta;barriers[h.barrierIndexV54]=b;
                if(h.fireV54!=null){var f=h.fireV54;f.point+=delta;f.logs.transform.position+=(Vector3)delta;f.flame.transform.position+=(Vector3)delta;f.logs.sortingOrder=CoastGame.Depth(f.point.y);f.flame.sortingOrder=f.logs.sortingOrder+2;}
            }
            BuildRoadsV54();foreach(var actor in people)actor.ResetRouteV48();Physics2D.SyncTransforms();
        }
        void BuildRoadsV54()
        {
            if(pavingV54){pavingV54.SetActive(false);Destroy(pavingV54);}pavingV54=new GameObject("Irregular connected stone lanes");pavingV54.transform.SetParent(root.transform,false);roadNodesV54.Clear();
            var connected=new List<Vector2>{Centre+Vector2.down*9.5f};var remaining=houses.Select(h=>h.entrance+Vector2.down*.6f).ToList();var rng=new System.Random(LayoutSeedV54^54179);
            while(remaining.Count>0)
            {
                Vector2 start=remaining[0],end=connected[0];float best=float.MaxValue;
                foreach(var a in remaining)foreach(var b in connected){float d=(a-b).sqrMagnitude;if(d<best){best=d;start=a;end=b;}}
                var path=PathV48(start,end);var before=start;roadNodesV54.Add(start);
                foreach(var p in path){if(Vector2.Distance(before,p)>.7f){PaveV48(before,p);before=p;}}PaveV48(before,end);
                connected.Add(start);remaining.Remove(start);
            }
            PaveV48(Centre+Vector2.down*9.5f,Centre+Vector2.down*6.7f);
            // The hunting and trading corners branch off the household lanes.
            foreach(var p in new[]{Centre+new Vector2(-16,-17),Centre+new Vector2(17,-8)})
            {var closest=connected.OrderBy(v=>(v-p).sqrMagnitude).First();var before=p;foreach(var q in PathV48(p,closest)){PaveV48(before,q);before=q;}}
        }
        void SharedHearthV54()
        {
            Barrier("Communal hearth stone and hot coals",Centre+Vector2.up*.76f,new Vector2(7.9f,3.5f));
            var t=game.art.Load("AlvielHearthV54");var s=game.art.Slice(t,new RectInt(0,0,t.width,t.height),new Vector2(.5f,.5f),100);
            var hearth=Prop("Single communal Alviel hearth",s,Centre+Vector2.up*.76f,8.8f,root.transform,statue.sortingOrder-4);
            // The front and rear of the same continuous ring straddle the pedestal in depth.
            var rect=s.rect;var front=Sprite.Create(s.texture,new Rect(rect.x,rect.y,rect.width,rect.height*.48f),new Vector2(.5f,.5f/.48f),s.pixelsPerUnit,0,SpriteMeshType.FullRect);
            Prop("Front rim of communal hearth",front,Centre+Vector2.up*.76f,8.8f,root.transform,statue.sortingOrder+3);
            for(int i=0;i<28;i++)
            {
                float a=i*Mathf.PI*2/28;var p=Centre+new Vector2(Mathf.Cos(a)*3.45f,Mathf.Sin(a)*1.62f)+Vector2.up*.76f;
                var f=new AmazonFireV46{point=p,phase=i*.43f,logs=hearth,widthV54=1.16f};f.flame=Prop("Communal hearth living flame",game.art.flame[0],p,1.16f,root.transform,statue.sortingOrder+(Mathf.Sin(a)<0?6:-2));fires.Add(f);
            }
        }
        void TrainingRacksV54()
        {
            for(int side=-1;side<=1;side+=2)
            {var at=Centre+new Vector2(side*5.6f,-5.6f);Prop("Training arms secured on a wooden rack",furnishingsV48[3],at,2.6f,root.transform);Barrier("Training rack feet",at+Vector2.up*.18f,new Vector2(2.4f,.45f));}
        }
        void CreateRoomsV54()
        {
            exit=Prop("Выйти из дома",game.journey.art.walls[2],new Vector2(0,-4.65f),1.9f,roomRoot.transform,-3400);exit.color=new Color(1,1,1,0);
            for(int index=0;index<houses.Count;index++)
            {
                var house=houses[index];var group=new GameObject("Dwelling "+index+" / "+HouseRoleV54(index));group.transform.SetParent(roomRoot.transform,false);rooms[house]=group;group.SetActive(false);
                var t=game.art.Load("AmazonHome"+index+"V54");var sprite=game.art.Slice(t,new RectInt(0,0,t.width,t.height),new Vector2(.5f,.5f),100);
                var r=Prop("Hand-furnished "+HouseRoleV54(index),sprite,new Vector2(0,.65f),15.1f,group.transform,-3500);var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);block.SetFloat("_ArtStep",1);r.SetPropertyBlock(block);
                // Furniture has floor footprints; the central passage and front threshold stay clear.
                floorsV58[house]=new FloorPlanV58(r,FloorPlanV58.Village);FurnitureFromArtV58(group.transform,r,index);
                var pot=game.buildings.CreateStation(ItemKind.StoragePot,new Vector2Int(2,-3),false,null,group.transform);pot.inventory.Add(new Item(ItemKind.RawMeat));pot.inventory.Add(new Item(ItemKind.BeetleMeat));pot.inventory.Add(new Item(ItemKind.Wood));house.furnishings.Add(pot);
                if(house.chief){var supply=game.buildings.CreateStation(ItemKind.StoragePot,new Vector2Int(-3,-3),false,null,group.transform);supply.inventory.Add(new Item(ItemKind.Axe));supply.inventory.Add(new Item(ItemKind.Stake));house.furnishings.Add(supply);var flame=Prop("Chieftain hearth animated flame",game.art.flame[0],new Vector2(-2.1f,.03f),.67f,group.transform);roomFlames.Add(flame);}
                foreach(var storage in house.furnishings){storage.inventory.preserveFoodV58=true;}
                foreach(var storage in house.furnishings)storage.renderer.transform.position+=Vector3.up*storage.renderer.bounds.extents.y;
            }
        }
        void RoomObstacleV54(Transform parent,Vector2 at,Vector2 size)
        {var o=new GameObject("Solid furniture footprint");o.transform.SetParent(parent,false);o.transform.localPosition=at;o.AddComponent<BoxCollider2D>().size=size;}
        public static string HouseRoleV54(int i)=>new[]{"Дом вождя","Дом охотницы","Дом следопытки","Дом ткачихи","Дом травницы","Дом лучницы","Дом воительницы","Дом торговки","Общая кухня","Дом хранительницы венков"}[i];
        public bool RoomWalkableV54(Vector2 p)=>CurrentFloorV58!=null && CurrentFloorV58.Contains(p,.20f);
    }
}
