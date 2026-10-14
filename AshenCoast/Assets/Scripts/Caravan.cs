using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed partial class JourneyWorld
    {
        public readonly List<CaravanWoodlouse> caravanAnimals=new List<CaravanWoodlouse>();
        public CaravanDriver cartDriver;
        float coastRouteY;
        public float CoastRouteY=>coastRouteY;
        public bool CaravanReady=>cartDriver!=null && cartDriver.Alive && !cartDriver.Fleeing && caravanAnimals.All(a=>a.Alive && !a.vitals.Curled);
        bool CaravanMoving=>Cutscene && (ridePhase==2 || ridePhase==4) || IsCoast && CaravanReady && !NearCart();
        float RouteY(WorldArea destination)=>destination==WorldArea.Coast?coastRouteY:-3.35f;
        void TickCaravan(float dt)
        {
            if(!cartRoot.activeInHierarchy || hidden)return;
            foreach(var a in caravanAnimals)a.Tick(dt);cartDriver.Tick(dt);
        }
        public IEnumerable<ICombatTarget> CaravanTargets
        {
            get{if(hidden || Cutscene || !cartRoot.activeInHierarchy)yield break;
                if(cartDriver.Alive)yield return cartDriver;foreach(var a in caravanAnimals)if(a.Alive)yield return a;}
        }
    }
    public sealed class CaravanWoodlouse : ICombatTarget
    {
        public readonly WoodlouseVitals vitals=new WoodlouseVitals();
        public SpriteRenderer Renderer {get;}
        public Vector2 Position=>(Vector2)Renderer.transform.position;
        public bool Alive=>vitals.Alive;
        public readonly CircleCollider2D feet;
        readonly JourneyWorld world;
        readonly SpriteRenderer shadow;
        readonly int index;
        float flash;
        public int Frame {get;private set;}
        public CaravanWoodlouse(JourneyWorld owner,SpriteRenderer renderer,int id)
        {
            world=owner;Renderer=renderer;index=id;renderer.transform.localScale=Vector3.one;
            feet=renderer.gameObject.AddComponent<CircleCollider2D>();feet.radius=.53f;feet.offset=new Vector2(0,.24f);
            shadow=world.game.Shadow("Harnessed woodlouse shadow",Position+Vector2.up*.12f,new Vector2(2.2f,.54f),.31f);shadow.transform.SetParent(world.cartRoot.transform,true);
        }
        public void Tick(float dt){vitals.Tick(dt);flash=Mathf.Max(0,flash-dt*5);}
        public void Pose(bool moving)
        {
            Renderer.transform.localPosition=new Vector3(3.8f,index==0?-.72f:.72f,0);
            if(!Alive){Renderer.enabled=shadow.enabled=feet.enabled=false;return;}
            if(vitals.Curled){float age=WoodlouseVitals.CurlDuration-vitals.curlRemaining;Frame=age<.4f?16+Mathf.Min(7,Mathf.FloorToInt(age/.05f)):vitals.curlRemaining<.4f?16+Mathf.Min(7,Mathf.FloorToInt(vitals.curlRemaining/.05f)):24+Mathf.FloorToInt(world.clock*12)%8;}
            else Frame=moving?Mathf.FloorToInt(world.clock*14+index*5)%16:0;
            Renderer.sprite=world.game.art.woodlouse[Frame];Renderer.sortingOrder=CoastGame.Depth(Position.y);Renderer.transform.localScale=Vector3.one;
            Renderer.color=Color.Lerp(Color.white,new Color(1,.57f,.43f),flash);world.game.art.PixelStyle(Renderer);
            shadow.transform.position=Position+Vector2.up*.12f;shadow.transform.localScale=new Vector3(vitals.Curled?1.7f:2.2f,.54f*shadow.sprite.pixelsPerUnit/shadow.sprite.rect.height,1);
        }
        public float Hurt(float damage)
        {
            float applied=vitals.Hurt(damage*(world.game.spellcraft?.DamageFactor(Renderer)??1));if(applied<=0)return 0;flash=1;world.cartDriver.Frighten();
            world.game.ShowDamage(applied,Position+Vector2.up*1.8f,Renderer,false,vitals.Curled);Pose(false);
            if(!Alive)world.game.DropMeat(Position);return applied;
        }
    }
    public sealed class CaravanDriver : ICombatTarget
    {
        public const float MaximumHealth=100;
        public float health=MaximumHealth;
        public SpriteRenderer Renderer {get;}
        public Vector2 Position=>(Vector2)Renderer.transform.position;
        public bool Alive=>health>0;
        public bool Fleeing {get;private set;}
        public readonly CircleCollider2D feet;
        readonly JourneyWorld world;
        readonly SpriteRenderer shadow;
        float fleeAge,distance,flash;
        Vector2 start,axis=Vector2.left;
        int view,frame;
        public CaravanDriver(JourneyWorld owner,SpriteRenderer sprite)
        {
            world=owner;Renderer=sprite;feet=sprite.gameObject.AddComponent<CircleCollider2D>();feet.radius=.22f;feet.offset=Vector2.up*.13f;feet.enabled=false;
            shadow=world.game.Shadow("Fleeing driver shadow",Position,new Vector2(.5f,.18f),.3f);shadow.transform.SetParent(world.cartRoot.transform,true);shadow.enabled=false;
        }
        public void Frighten(){if(!Alive || Fleeing)return;Fleeing=true;start=Position;fleeAge=0;feet.enabled=true;}
        public void Tick(float dt)
        {
            flash=Mathf.Max(0,flash-dt*5);if(!Alive || !Fleeing || world.game.spellcraft!=null && world.game.spellcraft.ImmobilizedV44(Renderer))return;fleeAge+=dt;
            if(fleeAge<.5f){var target=world.CartPosition+new Vector2(-.8f,-.85f);Renderer.transform.position=Vector2.Lerp(start,target,Mathf.SmoothStep(0,1,fleeAge/.5f));axis=(target-start).normalized;}
            else
            {
                var away=Position-world.game.body.position;if(away.sqrMagnitude>.01f)axis=away.normalized;
                Vector2 direction=Vector2.zero;float best=-999;
                foreach(float turn in new[]{0,30f,-30,60,-60,90,-90,120,-120})
                {
                    var d=(Vector2)(Quaternion.Euler(0,0,turn)*axis);
                    if(Physics2D.CircleCastAll(Position+Vector2.up*.13f,.21f,d,Mathf.Max(.4f,dt*4.4f)).Any(h=>!h.collider.isTrigger && h.collider!=feet && h.collider.gameObject!=world.game.hero.gameObject))continue;
                    float score=Vector2.Dot(d,axis)-Mathf.Abs(turn)*.001f;if(score>best){best=score;direction=d;}
                }
                var next=Position+direction*dt*4.4f;next=world.IsCoast?MeadowLayout.Clamp(next):world.Clamp(next);distance+=Vector2.Distance(Position,next);Renderer.transform.position=next;
            }
            view=Mathf.Abs(axis.x)>Mathf.Abs(axis.y)?axis.x<0?1:3:axis.y>0?2:0;frame=Mathf.FloorToInt(distance/1.75f*8)%8;
        }
        public void Pose(float bob)
        {
            if(!Alive){Renderer.enabled=feet.enabled=shadow.enabled=false;return;}
            if(!Fleeing){Renderer.sprite=world.art.people[7,Mathf.FloorToInt(world.clock*4)%8];Renderer.transform.localPosition=new Vector3(.85f,1.1f+bob,0);Renderer.sortingOrder=world.wagon.sortingOrder-1;}
            else{Renderer.sprite=world.art.driverRun[view,frame];Renderer.transform.localScale=Vector3.one;Renderer.sortingOrder=CoastGame.Depth(Position.y);shadow.enabled=true;shadow.transform.position=Position+Vector2.up*.08f;}
            Renderer.color=Color.Lerp(Color.white,new Color(1,.57f,.43f),flash);world.game.art.PixelStyle(Renderer);
        }
        public float Hurt(float damage)
        {
            if(!Alive || damage<=0)return 0;damage*=world.game.spellcraft?.DamageFactor(Renderer)??1;float applied=Mathf.Min(health,damage);health-=applied;flash=1;Frighten();world.game.ShowDamage(applied,Position+Vector2.up*1.8f,Renderer);Pose(0);return applied;
        }
    }
}
