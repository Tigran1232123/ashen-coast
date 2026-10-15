using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed class PlainsLifeV48
    {
        readonly AmazonSettlementV46 town;float nextEncounter=25;
        public readonly List<PlainBanditV48> bandits=new List<PlainBanditV48>();
        readonly HashSet<Vector2Int> explored=new HashSet<Vector2Int>();
        public IEnumerable<ICombatTarget> Targets=>bandits.Where(b=>b.Alive && b.Renderer.gameObject.activeInHierarchy);
        public PlainsLifeV48(AmazonSettlementV46 village){town=village;}
        public PlainBanditV48 Spawn(Vector2 point,bool archer)
        {var actor=new PlainBanditV48(town,point,archer);bandits.Add(actor);return actor;}
        public void Tick(float dt)
        {
            foreach(var b in bandits.ToArray())
            {
                town.game.TickCoopActorV49(b.Position,()=>b.Tick(dt));
                if(Vector2.Distance(b.Position,town.game.body.position)>110 && Vector2.Distance(b.Position,town.Centre)>85){Object.Destroy(b.Renderer.gameObject);bandits.Remove(b);}
            }
            nextEncounter-=dt;if(nextEncounter>0 || town.game.testMode)return;nextEncounter=22;
            var hero=town.game.body.position;var chunk=new Vector2Int(Mathf.FloorToInt(hero.x/48),Mathf.FloorToInt(hero.y/48));
            if(Vector2.Distance(hero,Vector2.zero)<55 || Vector2.Distance(hero,town.Centre)<43 || WorldShape.Distance(hero)<18 || bandits.Count(b=>b.Alive)>=5 || !explored.Add(chunk) || Random.value>.14f)return;
            var point=hero+Random.insideUnitCircle.normalized*Random.Range(29f,39f);
            if(WorldShape.Distance(point)<20 || Vector2.Distance(point,town.Centre)<38 || Vector2.Distance(point,WorldShape.FortPosition)<30)return;
            for(int i=0;i<Random.Range(1,3);i++)Spawn(point+Vector2.right*i*1.6f,Random.value>.5f);
        }
    }
    public sealed partial class PlainBanditV48:ICombatTarget
    {
        readonly AmazonSettlementV46 town;readonly Vector2 home;readonly SpriteRenderer shadow;
        public SpriteRenderer Renderer{get;}public Vector2 Position=>(Vector2)Renderer.transform.position;public float health=100;public bool Alive=>health>0;public readonly bool Archer;
        public PixelGait gaitV53;float clock,step,ready,attack=-1;bool delivered;Vector2 aim=Vector2.left,velocity;ICombatTarget victim;
        public bool Attacking=>attack>=0;
        public PlainBanditV48(AmazonSettlementV46 village,Vector2 at,bool archer)
        {
            town=village;home=at;Archer=archer;Renderer=town.game.art.Renderer(archer?"Разбойник-лучник":"Разбойник-мечник",town.game.fort.art.bandits[archer?1:0,0],at,CoastGame.Depth(at.y));Renderer.transform.SetParent(town.game.coastRoot.transform,true);
            shadow=town.game.Shadow("Plains bandit shadow",at,new Vector2(.68f,.22f),.27f);shadow.transform.SetParent(Renderer.transform,true);gaitV53=PixelGait.ForHumanoidV53(town.game.art,Renderer,.42f);Pose(false);
        }
        public float Hurt(float amount)
        {
            if(!Alive)return 0;float n=Mathf.Min(health,amount*town.game.spellcraft.DamageFactor(Renderer));health-=n;town.game.ShowDamage(n,Position+Vector2.up*1.8f,Renderer);
            if(!Alive){Renderer.enabled=shadow.enabled=false;town.game.ScatterLoot(new Item(ItemKind.Coin){count=Random.Range(1,4)},Position,Position);town.game.ScatterLoot(new Item(Archer?ItemKind.Bow:ItemKind.Sword){durability=55},Position,Position);}
            return n;
        }
        public void Tick(float dt)
        {
            if(!Alive || !Renderer.gameObject.activeInHierarchy)return;clock+=dt;ready=Mathf.Max(0,ready-dt);
            if(town.game.spellcraft.ImmobilizedV44(Renderer)){attack=-1;Pose(false);return;}
            if(attack>=0)
            {
                attack+=dt;
                if(!delivered && attack>=(Archer?.48f:.18f)){delivered=true;if(victim!=null && victim.Alive){if(Archer)town.ShootBanditV48(this,aim);else if(Vector2.Distance(Position,victim.Position)<2.25f && !town.game.WorldBlocked(Position,victim.Position,.05f))town.game.HurtTarget(victim,26,Renderer);}}
                if(attack>=(Archer?.80f:.40f))attack=-1;Pose(false);return;
            }
            var candidates=new List<ICombatTarget>{new HeroCombatActor(town.game)};
            candidates.AddRange(town.people.Where(p=>p.Alive && !p.Captive && p.Renderer.gameObject.activeInHierarchy && (!p.IsSlave || p.Hired)));
            if(town.game.journey.companion.Hired && town.game.journey.companion.Alive)candidates.Add(new MageCombatActor(town.game.journey.companion));
            candidates.AddRange(town.game.snails.houses.Where(h=>!h.Hidden));victim=candidates.Where(t=>t.Alive && Vector2.Distance(Position,t.Position)<18).OrderBy(t=>(t.Position-Position).sqrMagnitude).FirstOrDefault();
            var destination=home+new Vector2(Mathf.Sin(clock*.08f)*3,Mathf.Cos(clock*.10f)*2);
            if(victim!=null)
            {var delta=victim.Position-Position;aim=delta.normalized;float reach=Archer?8:1.8f;if(delta.magnitude<=reach && ready<=0 && !town.game.WorldBlocked(Position,victim.Position,.05f)){attack=0;delivered=false;ready=Archer?2f:1.05f;town.game.WorldSwishV53(Renderer);}else if(delta.magnitude>reach*.85f)destination=victim.Position-aim*reach*.7f;else destination=Position;}
            var direction=destination-Position;velocity=Vector2.MoveTowards(velocity,direction.magnitude>.2f?direction.normalized*(victim!=null?3:1.6f):Vector2.zero,dt*12);
            var move=Vector2.ClampMagnitude(velocity*dt,direction.magnitude);if(town.game.WorldBlocked(Position,Position+move,.25f))move=Vector2.zero;Renderer.transform.position+=(Vector3)move;step+=move.magnitude;if(move.sqrMagnitude>.000001f && victim==null)aim=move.normalized;Pose(move.sqrMagnitude>.000001f);
        }
        void Pose(bool walking)
        {
            int role=Archer?1:0;Renderer.sprite=attack>=0?town.game.fort.art.attacks[role,Mathf.Min(7,(int)(attack/(Archer?.8f:.4f)*8))]:town.game.fort.art.bandits[role,walking?(int)(step*4)%8:0];
            if(Mathf.Abs(aim.x)>.25f)Renderer.flipX=aim.x<0;Renderer.sortingOrder=CoastGame.Depth(Position.y);town.game.art.PixelStyle(Renderer);shadow.transform.position=Position;gaitV53.PoseSideV53(step/1.5f,walking,aim);
        }
    }
}
