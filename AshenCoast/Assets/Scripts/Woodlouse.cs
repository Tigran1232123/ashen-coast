using UnityEngine;

namespace AshenCoast
{
    public sealed class WoodlouseVitals
    {
        public const float MaximumHealth=200, CurlDuration=5;
        public float health=MaximumHealth, curlRemaining;
        public float maximumHealth=MaximumHealth;
        public bool firstHit;
        public bool Alive=>health>0;
        public bool Curled=>Alive && curlRemaining>0;
        public float Hurt(float damage,bool preventCurl=false)
        {
            if(!Alive || damage<=0)return 0;
            float applied=damage*(Curled?.20f:1);
            health=Mathf.Max(0,health-applied);
            if(!Curled && Alive && !preventCurl){firstHit=true;curlRemaining=CurlDuration;}
            return applied;
        }
        public void Tick(float dt){curlRemaining=Mathf.Max(0,curlRemaining-Mathf.Max(0,dt));}
    }

    public sealed class Woodlouse
    {
        public readonly WoodlouseVitals vitals=new WoodlouseVitals();
        public readonly SpriteRenderer renderer,shadow;
        public readonly CircleCollider2D feet;
        public bool Small {get;private set;}
        public bool Grabbed;
        public Vector2 Position=>(Vector2)renderer.transform.position;
        public int frame {get;private set;}
        readonly CoastGame game;
        readonly Vector2 home;
        float clock,walkDistance,hitFlash;
        bool lootDropped;

        public Woodlouse(CoastGame owner,Vector2 position,bool small=false)
        {
            game=owner;home=position;Small=small;if(small)vitals.health=vitals.maximumHealth=40;
            renderer=game.art.Renderer("Harmless giant woodlouse / 200 HP",game.art.woodlouse[0],position,CoastGame.Depth(position.y));
            renderer.transform.SetParent(game.coastRoot.transform,true);
            if(small){renderer.transform.localScale=Vector3.one*.48f;game.art.PixelStyle(renderer);}
            shadow=game.Shadow("Woodlouse ground shadow",position+new Vector2(0,.12f),new Vector2(2.2f,.54f),.31f);
            feet=renderer.gameObject.AddComponent<CircleCollider2D>();feet.radius=.53f;feet.offset=new Vector2(0,.24f);
            if(small){shadow.transform.localScale=new Vector3(1.05f,.20f*shadow.sprite.pixelsPerUnit/shadow.sprite.rect.height,1);}
            Pose();
        }
        public void Tick(float dt,bool wander=true)
        {
            if(!vitals.Alive || Grabbed || game.spellcraft!=null && game.spellcraft.ImmobilizedV44(renderer))return;
            vitals.Tick(dt);clock+=dt;hitFlash=Mathf.Max(0,hitFlash-dt*5);
            if(!vitals.Curled && wander)
            {
                var target=home+new Vector2(Mathf.Sin(clock*.17f)*1.4f,Mathf.Sin(clock*.11f)*.64f);
                var direction=target-Position;
                // A peaceful animal pauses near the player's feet; it never attacks or chases.
                if(Vector2.Distance(Position,game.body.position)>1.2f && direction.magnitude>.025f)
                {
                    var offset=Vector2.ClampMagnitude(direction,dt*.42f);
                    renderer.transform.position+=(Vector3)offset;walkDistance+=offset.magnitude;
                    if(Mathf.Abs(offset.x)>.00001f)renderer.flipX=offset.x<0;
                }
            }
            Pose();
        }
        void Pose()
        {
            if(vitals.Curled)
            {
                float age=WoodlouseVitals.CurlDuration-vitals.curlRemaining;
                frame=age<.40f?16+Mathf.Min(7,Mathf.FloorToInt(age/.05f)):vitals.curlRemaining<.40f?16+Mathf.Min(7,Mathf.FloorToInt(vitals.curlRemaining/.05f)):24+Mathf.FloorToInt(clock*12)%8;
            }
            else frame=Mathf.FloorToInt(walkDistance/.055f)%16;
            renderer.sprite=game.art.woodlouse[frame];game.art.PixelStyle(renderer);
            renderer.sortingOrder=CoastGame.Depth(Position.y);
            renderer.color=Color.Lerp(Color.white,new Color(1,.57f,.43f),hitFlash);
            shadow.transform.position=Position+new Vector2(0,.12f);
            float width=(vitals.Curled?1.7f:2.2f)*(Small?.48f:1);shadow.transform.localScale=new Vector3(width,.54f*(Small?.48f:1)*shadow.sprite.pixelsPerUnit/shadow.sprite.rect.height,1);
        }
        public float Hurt(float damage)
        {
            if(game.CoopOnline && game.coop.world.RedirectDamage(new WoodlouseActor(this),damage,game.hero))return 0;
            float applied=vitals.Hurt(damage*(game.spellcraft?.DamageFactor(renderer)??1),game.v44!=null && game.v44.Stunned(renderer));if(applied<=0)return 0;
            hitFlash=1;
            game.ShowDamage(applied,Position+new Vector2(.9f,1.75f),renderer,false,vitals.Curled);Pose();
            if(!vitals.Alive)
            {
                renderer.enabled=false;shadow.enabled=false;feet.enabled=false;
                if(!lootDropped){lootDropped=true;game.DropMeat(Position);}
            }
            return applied;
        }
    }
    public sealed class GroundLoot
    {
        public string netIdV49=System.Guid.NewGuid().ToString("N");
        public Item item;
        public Vector2 position;
        public SpriteRenderer renderer,shadow,cloth,spectrumGlow;
        public bool collected;
        public Vector2 thrownFrom;
        public float flightAge=1,flightDuration=.55f,groundAge;public bool spectrumLanded;
        public bool InFlight=>flightAge<flightDuration;
    }
}
