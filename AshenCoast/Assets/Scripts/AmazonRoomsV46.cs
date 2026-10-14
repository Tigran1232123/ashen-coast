using System.Collections.Generic;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class AmazonSettlementV46
    {
        readonly Dictionary<AmazonHouseV46,GameObject> rooms=new Dictionary<AmazonHouseV46,GameObject>();
        public Rect RoomBounds=>new Rect(-7,-5.5f,14,11);
        public Vector2 ClampRoom(Vector2 point)=>new Vector2(Mathf.Clamp(point.x,-6.7f,6.7f),Mathf.Clamp(point.y,-5.3f,5.3f));
        void Floor(string name,Transform parent,Vector2 centre,Vector2 size,bool indoor)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Quad);Destroy(obj.GetComponent<Collider>());obj.name=name;obj.transform.SetParent(parent,true);obj.transform.position=new Vector3(centre.x,centre.y,4);obj.transform.localScale=new Vector3(size.x,size.y,1);
            var r=obj.GetComponent<MeshRenderer>();r.sortingOrder=indoor?-3500:-20000;var mat=new Material(Shader.Find(indoor?"AshenCoast/JourneyFloor":"AshenCoast/AmazonGroundV46"));mat.SetTexture("_GroundTex",Resources.Load<Texture2D>("Art/TerrainTiles"));mat.SetTexture("_WoodTex",Resources.Load<Texture2D>("Art/TavernSurfacesV10"));mat.SetFloat("_Indoor",1);mat.SetVector("_Centre",centre);r.sharedMaterial=mat;
        }
        void CreateRoom()
        {
            Floor("Woven dwelling floor",roomRoot.transform,Vector2.zero,new Vector2(14,11),true);
            exit=Prop("Выйти из дома",game.journey.art.walls[2],new Vector2(0,-5.15f),1.7f,roomRoot.transform,1000);
            for(int i=0;i<4;i++)Prop("Woven straw interior wall",furnishingsV48[6],new Vector2(-5.25f+i*3.5f,4.3f),3.65f,roomRoot.transform,400);
            for(int side=-1;side<=1;side+=2){var post=Prop("Dwelling upright timber post",game.journey.art.walls[1],new Vector2(side*6.8f,-5),.32f,roomRoot.transform,600);post.transform.localScale=new Vector3(post.transform.localScale.x,10/post.sprite.bounds.size.y,1);}
            foreach(var house in houses)
            {
                var group=new GameObject(house.chief?"Chieftain's hearth hall":"Straw dwelling interior");group.transform.SetParent(roomRoot.transform,false);rooms[house]=group;group.SetActive(false);
                var parent=group.transform;
                for(int i=0;i<(house.chief?4:2);i++)
                {
                    Prop("Woven sleeping mat and straw pillow",furnishingsV48[0],new Vector2(i%2==0?-4.3f:4.3f,-.7f+i/2*2.1f),2.3f,parent,-1500);
                    Prop("Clay drinking cup",game.journey.art.props[2],new Vector2(i%2==0?-3.0f:3.0f,-.5f+i/2*2.1f),.30f,parent);
                }
                Prop("Deer hide gathering rug",furnishingsV48[1],new Vector2(0,house.chief?1.9f:.5f),house.chief?3.4f:3.1f,parent,-1490);
                Prop("Flower and medicinal herb drying rack",furnishingsV48[4],new Vector2(-2.6f,3.5f),2.2f,parent);
                Prop("Woven food basket",furnishingsV48[7],new Vector2(2.8f,2.6f),1.05f,parent);
                Prop("Hunting trophies and dried meat",furnishingsV48[5],new Vector2(3.9f,3.4f),2.1f,parent);
                Prop("Warrior's weapon rack",furnishingsV48[3],new Vector2(-4.2f,-2),1.8f,parent);
                var pot=game.buildings.CreateStation(ItemKind.StoragePot,new Vector2Int(-6,3),false,null,parent);pot.inventory.Add(new Item(ItemKind.RawMeat));pot.inventory.Add(new Item(ItemKind.BeetleMeat));pot.inventory.Add(new Item(ItemKind.Wood));house.furnishings.Add(pot);
                if(house.chief)
                {
                    var supply=game.buildings.CreateStation(ItemKind.StoragePot,new Vector2Int(5,3),false,null,parent);supply.inventory.Add(new Item(ItemKind.Axe));supply.inventory.Add(new Item(ItemKind.Stake));house.furnishings.Add(supply);
                    var fire=Prop("Chieftain stone hearth",game.art.campfireBase,new Vector2(0,-.5f),2.4f,parent);var flame=Prop("Hall hearth flame",game.art.flame[0],new Vector2(0,-.38f),1.9f,parent,fire.sortingOrder+3);roomFlames.Add(flame);
                    Prop("Carved fur-lined chieftain throne",furnishingsV48[2],new Vector2(0,2.6f),2.05f,parent);
                }
            }
        }
        readonly List<SpriteRenderer> roomFlames=new List<SpriteRenderer>();
        public void EnterHouse(AmazonHouseV46 house)
        {
            currentHouse=house;foreach(var entry in rooms)entry.Value.SetActive(entry.Key==house);
            game.journey.SetArea(WorldArea.AmazonHouse,new Vector2(0,-3.8f));game.cameraView.orthographicSize=6.2f;game.ResetCamera(new Vector3(0,.15f,-20));doorWait=1;
        }
        public void LeaveHouse()
        {if(currentHouse==null)return;var point=currentHouse.entrance+Vector2.down*1.1f;game.journey.SetArea(WorldArea.Coast,point);doorWait=1;}
    }
    public sealed class ChoppablePoplarV46
    {
        public readonly SpriteRenderer sprite,shadow;
        public readonly Vector2 position;
        public int hits{get;private set;}
        public bool Standing=>hits<10;
        public Vector2 FallDirection{get;private set;}
        public float age{get;private set;}
        readonly AmazonSettlementV46 village;readonly CircleCollider2D trunk;
        readonly Vector3 scale;readonly float phase;float flash;
        public ChoppablePoplarV46(AmazonSettlementV46 owner,Vector2 point,int index)
        {
            village=owner;position=point;phase=index*1.93f;
            sprite=owner.Prop("Тополь",owner.game.journey.art.poplar,point,3.3f+index%2*.4f,owner.root.transform);scale=sprite.transform.localScale;
            shadow=owner.game.Shadow("Poplar ground shadow",point+new Vector2(.8f,.1f),new Vector2(4,.7f),.27f);shadow.transform.SetParent(owner.root.transform,true);
            var obstacle=new GameObject("Poplar trunk");obstacle.transform.SetParent(owner.root.transform,true);obstacle.transform.position=point+Vector2.up*.1f;trunk=obstacle.AddComponent<CircleCollider2D>();trunk.radius=.24f;
        }
        public bool Chop(Vector2 player)
        {
            if(!Standing)return false;hits++;flash=.15f;
            if(hits==10)
            {
                FallDirection=(position-player).normalized;if(FallDirection.sqrMagnitude<.01f)FallDirection=Vector2.right;age=0;trunk.enabled=false;
                int count=UnityEngine.Random.Range(3,6);for(int i=0;i<count;i++)village.game.ScatterLoot(new Item(ItemKind.Wood),position+Vector2.right*(i*.24f-.45f),position);
            }
            return true;
        }
        public void Tick(float dt)
        {
            flash=Mathf.Max(0,flash-dt);
            if(Standing){sprite.transform.rotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*.6f+phase)*.28f);sprite.color=Color.Lerp(Color.white,new Color(1,.76f,.51f),flash/.15f);return;}
            age+=dt;float fall=Mathf.SmoothStep(0,1,Mathf.Clamp01(age/1.1f));float angle=Mathf.Atan2(FallDirection.y,FallDirection.x)*Mathf.Rad2Deg-90;
            sprite.transform.rotation=Quaternion.Euler(0,0,Mathf.DeltaAngle(0,angle)*fall);sprite.transform.position=position+FallDirection*fall*.65f;
            sprite.transform.localScale=new Vector3(scale.x,scale.y*Mathf.Lerp(1,.55f+.3f*Mathf.Abs(FallDirection.x),fall),1);
            float alpha=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((age-6.1f)/.8f));sprite.color=new Color(1,1,1,alpha);shadow.color=new Color(.08f,.10f,.09f,.27f*alpha);
            if(alpha<=0){sprite.enabled=shadow.enabled=false;}
        }
    }
    public sealed partial class CoastGame
    {
        public GroundLoot CreateLooseLootV46(Item item,Vector2 at)=>CreateLoot(item,at,false);
    }
}
