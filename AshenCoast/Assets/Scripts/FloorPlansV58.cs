using System;
using System.Collections.Generic;
using UnityEngine;
namespace AshenCoast
{
    // Coordinates refer to the original artwork, including its transparent border.
    public sealed class FloorPlanV58
    {
        public readonly Vector2[] points;
        public readonly Vector2 centre;
        public FloorPlanV58(SpriteRenderer sprite,Vector2[] topUV)
        {points=Array.ConvertAll(topUV,p=>World(sprite,p));centre=Vector2.zero;foreach(var p in points)centre+=p;centre/=points.Length;}
        public static Vector2 World(SpriteRenderer r,Vector2 uv)
        {var s=r.sprite;return r.transform.TransformPoint(new Vector2((uv.x*s.texture.width-s.rect.x-s.pivot.x)/s.pixelsPerUnit,((1-uv.y)*s.texture.height-s.rect.y-s.pivot.y)/s.pixelsPerUnit));}
        public static Vector2 Closest(Vector2 p,Vector2 a,Vector2 b)
        {var d=b-a;return a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/Mathf.Max(.00001f,d.sqrMagnitude));}
        public bool Contains(Vector2 p,float margin=0)
        {
            bool inside=false;for(int i=0,j=points.Length-1;i<points.Length;j=i++)
            {var a=points[i];var b=points[j];if((a.y>p.y)!=(b.y>p.y) && p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x)inside=!inside;if(margin>0 && Vector2.Distance(p,Closest(p,a,b))<margin)return false;}return inside;
        }
        public Vector2 Clamp(Vector2 p,float margin=.22f)
        {if(Contains(p,margin))return p;var best=centre;float distance=float.MaxValue;for(int i=0;i<points.Length;i++){var q=Closest(p,points[i],points[(i+1)%points.Length]);if((p-q).sqrMagnitude<distance){best=q;distance=(p-q).sqrMagnitude;}}for(int n=0;n<25 && !Contains(best,margin);n++)best=Vector2.MoveTowards(best,centre,.04f);return best;}
        public static readonly Vector2[] Village={new Vector2(.10f,.64f),new Vector2(.19f,.54f),new Vector2(.36f,.49f),new Vector2(.64f,.49f),new Vector2(.81f,.54f),new Vector2(.90f,.64f),new Vector2(.87f,.73f),new Vector2(.74f,.79f),new Vector2(.60f,.82f),new Vector2(.60f,.955f),new Vector2(.40f,.955f),new Vector2(.40f,.82f),new Vector2(.26f,.79f),new Vector2(.13f,.73f)};
        public static readonly Vector2[][] Snails={
            new[]{new Vector2(.22f,.58f),new Vector2(.27f,.49f),new Vector2(.40f,.44f),new Vector2(.68f,.44f),new Vector2(.82f,.49f),new Vector2(.91f,.59f),new Vector2(.92f,.65f),new Vector2(.83f,.73f),new Vector2(.69f,.79f),new Vector2(.49f,.80f),new Vector2(.34f,.74f),new Vector2(.24f,.65f)},
            new[]{new Vector2(.23f,.58f),new Vector2(.27f,.48f),new Vector2(.41f,.40f),new Vector2(.67f,.40f),new Vector2(.81f,.46f),new Vector2(.91f,.57f),new Vector2(.93f,.64f),new Vector2(.84f,.74f),new Vector2(.68f,.81f),new Vector2(.47f,.82f),new Vector2(.32f,.75f),new Vector2(.24f,.65f)},
            new[]{new Vector2(.26f,.59f),new Vector2(.33f,.52f),new Vector2(.46f,.46f),new Vector2(.68f,.48f),new Vector2(.82f,.54f),new Vector2(.91f,.64f),new Vector2(.88f,.71f),new Vector2(.76f,.78f),new Vector2(.65f,.81f),new Vector2(.46f,.80f),new Vector2(.33f,.72f)},
            new[]{new Vector2(.23f,.61f),new Vector2(.29f,.53f),new Vector2(.43f,.47f),new Vector2(.69f,.47f),new Vector2(.83f,.54f),new Vector2(.94f,.65f),new Vector2(.88f,.73f),new Vector2(.75f,.81f),new Vector2(.61f,.86f),new Vector2(.43f,.83f),new Vector2(.31f,.75f),new Vector2(.24f,.67f)}
        };
    }
    public sealed partial class SnailHomesV50
    {
        readonly Dictionary<int,FloorPlanV58> floorsV58=new Dictionary<int,FloorPlanV58>();
        public FloorPlanV58 FloorV58(int home)=>floorsV58[home];
        public Vector2 ClampFloorV58(Vector2 point)
        {int home=Mathf.Clamp(Inside,0,3);var door=RoomDoorV51(home);if(Mathf.Abs(point.x-door.x)<.48f && point.y>door.y-.8f && point.y<door.y+.12f)return new Vector2(point.x,Mathf.Min(point.y,door.y+.1f));return floorsV58[home].Clamp(point,.22f);}
    }
    public sealed partial class AmazonSettlementV46
    {
        readonly Dictionary<AmazonHouseV46,FloorPlanV58> floorsV58=new Dictionary<AmazonHouseV46,FloorPlanV58>();
        readonly HashSet<int> roundedV58=new HashSet<int>();
        public FloorPlanV58 CurrentFloorV58=>currentHouse!=null?floorsV58[currentHouse]:null;
        public bool BarrierContainsV58(int index,Vector2 point,float radius)
        {var b=barriers[index];if(!roundedV58.Contains(index))return new Rect(b.xMin-radius,b.yMin-radius,b.width+2*radius,b.height+2*radius).Contains(point);var q=point-b.center;float x=q.x/(b.width*.5f+radius),y=q.y/(b.height*.5f+radius);return x*x+y*y<1;}
        void RoundBarrierV58(BoxCollider2D box,int index)
        {roundedV58.Add(index);box.enabled=false;var polygon=box.gameObject.AddComponent<PolygonCollider2D>();var points=new Vector2[24];for(int i=0;i<points.Length;i++){float a=i*Mathf.PI*2/points.Length;points[i]=new Vector2(Mathf.Cos(a)*box.size.x*.5f,Mathf.Sin(a)*box.size.y*.5f);}polygon.points=points;}
        void FurnitureFromArtV58(Transform parent,SpriteRenderer art,int home)
        {
            // Lower edges of beds, desks, hearths and the throne; hanging wall decorations have no floor collision.
            Rect R(float x,float y,float w,float h)=>new Rect(x,y,w,h);
            Rect[][] footprints={
                new[]{R(.065f,.53f,.22f,.15f),R(.30f,.51f,.14f,.12f),R(.445f,.43f,.12f,.12f),R(.73f,.49f,.095f,.08f),R(.81f,.55f,.15f,.10f)},
                new[]{R(.14f,.43f,.23f,.17f),R(.43f,.46f,.10f,.09f),R(.76f,.51f,.11f,.11f)},
                new[]{R(.55f,.43f,.22f,.16f),R(.16f,.51f,.15f,.10f),R(.32f,.45f,.10f,.10f)},
                new[]{R(.09f,.50f,.21f,.17f),R(.68f,.43f,.16f,.13f),R(.80f,.55f,.12f,.11f)},
                new[]{R(.71f,.43f,.19f,.20f),R(.11f,.51f,.22f,.12f)},
                new[]{R(.12f,.46f,.24f,.18f),R(.64f,.52f,.24f,.11f)},
                new[]{R(.68f,.48f,.24f,.20f),R(.12f,.52f,.15f,.10f)},
                new[]{R(.29f,.39f,.22f,.21f),R(.10f,.52f,.22f,.11f)},
                new[]{R(.09f,.48f,.22f,.17f),R(.69f,.44f,.21f,.16f)},
                new[]{R(.72f,.46f,.20f,.18f),R(.11f,.55f,.18f,.10f)}
            };
            foreach(var box in footprints[home]){var a=FloorPlanV58.World(art,new Vector2(box.xMin,box.yMin));var b=FloorPlanV58.World(art,new Vector2(box.xMax,box.yMax));RoomObstacleV54(parent,(a+b)*.5f,new Vector2(Mathf.Abs(b.x-a.x),Mathf.Abs(b.y-a.y)));}
        }
    }
}
