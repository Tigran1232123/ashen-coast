using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class JourneyWorld
    {
        public readonly List<TavernResident> residents=new List<TavernResident>();public bool TavernAlarm {get;private set;}
        public void CreateResidents(){foreach(var a in actors)residents.Add(new TavernResident(this,a));}
        public void AlarmTavern()
        {if(TavernAlarm)return;TavernAlarm=true;HireOffer=false;game.ui.CloseContainers();foreach(var r in residents)r.Stand();}
        public IEnumerable<ICombatTarget> TavernTargets=>Interior?residents.Where(r=>r.Alive && !r.Escaped && !(r.actor==drinker && companion.Hired)).Cast<ICombatTarget>().Concat(gulls.Where(g=>g.Alive && g.sprite.enabled).Cast<ICombatTarget>()):gulls.Where(g=>g.Alive && g.sprite.enabled && IsCoast).Cast<ICombatTarget>();
    }
    public sealed partial class TavernResident:ICombatTarget
    {
        readonly JourneyWorld world;readonly CoastGame game;public readonly TavernActor actor;public float health=100,walkDistance,attackTime,cooldown;public bool Escaped;public SpriteRenderer blade;public CircleCollider2D feet;
        public bool peacefulV53;public PixelGait walkingGaitV53;public void PeaceV53(){peacefulV53=true;Escaped=false;attackTime=cooldown=0;if(blade)blade.enabled=false;}
        public bool Flee=>actor.row==1 || actor.row==6;public bool Armored=>actor.row==5;
        public bool Alive=>health>0;public Vector2 Position=>actor.sprite.transform.position;public SpriteRenderer Renderer=>actor.sprite;
        public TavernResident(JourneyWorld owner,TavernActor person){world=owner;game=owner.game;actor=person;walkingGaitV53=PixelGait.ForHumanoidV53(game.art,Renderer,.43f,game.newArt.motion[actor.row,0]);}
        public float Hurt(float amount)
        {if(!Alive)return 0;peacefulV53=false;float hit=Mathf.Min(health,amount*(game.spellcraft?.DamageFactor(Renderer)??1)*(Armored?.5f:1));health-=hit;game.ShowDamage(hit,Position+Vector2.up*1.6f,Renderer);world.AlarmTavern();if(!Alive){Renderer.enabled=false;if(feet)feet.enabled=false;if(blade)blade.enabled=false;}return hit;}
        public void Stand()
        {
            if(!Alive || actor==world.drinker && world.companion.Hired)return;
            if(actor.gait!=null)actor.gait.Lower.enabled=false;var block=new MaterialPropertyBlock();Renderer.GetPropertyBlock(block);block.SetFloat("_BodyFloor",0);block.SetFloat("_SeatedLegs",0);Renderer.SetPropertyBlock(block);
            Renderer.transform.localScale=Vector3.one;var start=Position;
            for(float r=.2f;r<=2;r+=.2f){bool found=false;for(int i=0;i<16;i++){var p=world.Clamp(start+new Vector2(Mathf.Cos(i*Mathf.PI/8),Mathf.Sin(i*Mathf.PI/8))*r);if(!Physics2D.OverlapCircle(p+Vector2.up*.12f,.23f)){Renderer.transform.position=p;found=true;break;}}if(found)break;}
            feet=Renderer.gameObject.AddComponent<CircleCollider2D>();feet.radius=.23f;feet.offset=Vector2.up*.12f;
            if(!Flee){blade=game.art.Renderer(Armored?"Angry knight sword":"Tavern knife",game.art.icons[Armored?4:5],Position,Renderer.sortingOrder+2);blade.transform.SetParent(Renderer.transform,true);blade.transform.localScale=Vector3.one*((Armored?1.65f:.72f)/blade.sprite.bounds.size.x);}
        }
        public void Tick(float dt)
        {
            if(peacefulV53)return;if(!Alive || game.spellcraft!=null && game.spellcraft.ImmobilizedV44(Renderer) || Escaped || actor==world.drinker && world.companion.Hired)return;
            if(walkingGaitV53==null)walkingGaitV53=PixelGait.ForHumanoidV53(game.art,Renderer,.43f);var beforeV53=Position;
            cooldown=Mathf.Max(0,cooldown-dt);var delta=game.body.position-Position;var seek=Flee?-delta.normalized:delta.normalized;
            if(Flee || delta.magnitude>1.15f)
            {
                Vector2 move=Vector2.zero;foreach(float angle in new[]{0,35f,-35f,70f,-70f,110f,-110f}){var d=(Vector2)(Quaternion.Euler(0,0,angle)*seek);float step=dt*(Flee?3.5f:2.6f);var candidate=world.Clamp(Position+d*step);if((candidate-Position-d*step).sqrMagnitude>.00001f)continue;var hits=Physics2D.CircleCastAll(Position+Vector2.up*.12f,.22f,d,Mathf.Max(.35f,step));if(hits.Any(h=>h.collider && !h.collider.isTrigger && h.collider!=feet && h.collider.gameObject!=game.hero.gameObject))continue;move=d*step;break;}
                Renderer.transform.position+=(Vector3)move;walkDistance+=move.magnitude;
                if(Flee && Vector2.Distance(Position,world.TavernExit)<.55f){Escaped=true;Renderer.enabled=false;feet.enabled=false;return;}
            }
            int frame=(int)(walkDistance/1.45f*8)%8;Renderer.sprite=game.newArt.motion[actor.row,frame];Renderer.flipX=seek.x>0;Renderer.sortingOrder=CoastGame.Depth(Position.y);game.art.PixelStyle(Renderer);walkingGaitV53.PoseSideV53(walkDistance/1.45f,(Position-beforeV53).sqrMagnitude>.000001f,seek);
            if(Flee)return;
            if(attackTime>0){attackTime+=dt;if(attackTime>=.25f && attackTime-dt<.25f && delta.magnitude<1.8f && !game.WorldBlocked(Position,game.body.position,.10f)){game.HurtPlayer(Armored?24:20);game.WorldSwishV53(Renderer);}if(attackTime>=.5f){attackTime=0;cooldown=.55f;}}
            else if(delta.magnitude<1.5f && cooldown<=0 && !game.Dead)attackTime=.001f;
            float aim=Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg;float sweep=attackTime>0?Mathf.Lerp(-55,55,attackTime/.5f):0;
            blade.transform.position=Position+Vector2.up*.8f+delta.normalized*.2f;blade.transform.rotation=Quaternion.Euler(0,0,aim+sweep);blade.sortingOrder=Renderer.sortingOrder+2;
        }
    }
    public sealed partial class JourneyWorld
    {
        public float GullScareDistance=>game.cameraView.orthographicSize*2/3;
        public void AnimateGulls(float dt)
        {
            foreach(var g in gulls)
            {
                if(!g.Alive){g.sprite.enabled=false;continue;}if(game.spellcraft.ImmobilizedV44(g.sprite))continue;
                bool visible=IsCoast && !hidden;g.sprite.enabled=visible && g.state!=4;if(!visible)continue;
                g.age+=dt;
                if(g.state==4){if(g.age<18+g.phase || FortWorld.SegmentDistance(game.body.position,g.home+new Vector2(-5,5),g.home)<8)continue;g.state=0;g.age=0;g.point=g.home+new Vector2(-5,5);}
                if((g.state==0 || g.state==1) && Vector2.Distance(game.body.position,g.point)<GullScareDistance)g.TakeOff(game.body.position,game);
                int row,frame;
                if(g.state==0){row=0;frame=Mathf.Min(7,(int)(g.age*4));g.point=Vector2.Lerp(g.home+new Vector2(-5,5),g.home,Mathf.Clamp01(g.age/2));if(g.age>=2){g.state=1;g.age=0;}}
                else if(g.state==1){row=1;frame=(int)(g.age*10+g.phase)%8;g.point=g.home+new Vector2(Mathf.Sin(g.age+g.phase)*.28f,Mathf.Sin(g.age*.7f)*.06f);if(g.age>12+g.phase)g.TakeOff(game.body.position,game);}
                else if(g.state==2){row=2;frame=Mathf.Min(7,(int)(g.age/.6f*8));g.point+=g.flight*dt*(1+g.age*3)+Vector2.up*dt;if(g.age>=.6f){g.state=3;g.age=0;}}
                else {row=3;frame=(int)(g.age*16+g.phase)%8;g.point+=g.flight*dt*7.4f+Vector2.up*dt*.25f;if(g.age>1 && Outside(g.point)){g.state=4;g.age=0;g.sprite.enabled=false;}}
                g.frame=frame;g.sprite.sprite=game.newArt.gulls[row,frame];g.sprite.transform.localScale=Vector3.one*.60f;g.sprite.flipX=g.flight.x<0 && g.state>=2;g.sprite.transform.position=g.point+Vector2.up*(g.state==3?Mathf.Sin(g.age*9)*.06f:0);g.sprite.sortingOrder=CoastGame.Depth(g.home.y)+4;game.art.PixelStyle(g.sprite);
            }
        }
        bool Outside(Vector2 point){var p=game.cameraView.WorldToViewportPoint(point);return p.x<-.18f || p.x>1.18f || p.y<-.18f || p.y>1.18f;}
    }
}
