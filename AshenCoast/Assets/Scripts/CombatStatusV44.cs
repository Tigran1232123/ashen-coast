using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class CombatStatusV44 : MonoBehaviour
    {
        sealed class Stun { public SpriteRenderer actor;public float until;public readonly List<SpriteRenderer> particles=new List<SpriteRenderer>(); }
        readonly Dictionary<SpriteRenderer,Stun> stuns=new Dictionary<SpriteRenderer,Stun>();
        CoastGame game;
        public void Initialize(CoastGame owner){game=owner;}
        public bool Stunned(SpriteRenderer actor)=>actor && stuns.TryGetValue(actor,out var state) && Time.time<state.until;
        public void Apply(ICombatTarget target)
        {
            if(game.coop!=null && game.coop.NotifyEffectV49("stun",target))return;
            if(target is WormPart part){var worm=game.cave.worms.FirstOrDefault(w=>w.parts.Contains(part));if(worm!=null){foreach(var segment in worm.parts)ApplyActor(segment.Renderer);return;}}
            ApplyActor(target.Renderer);
        }
        void ApplyActor(SpriteRenderer actor)
        {
            if(!actor)return;
            if(!stuns.TryGetValue(actor,out var s))
            {
                s=new Stun{actor=actor};stuns.Add(actor,s);
                for(int i=0;i<9;i++){var r=game.art.Renderer("Golden Sting / stun mote",game.v37Art.mote,actor.bounds.center,actor.sortingOrder+5);r.transform.SetParent(actor.transform.parent,true);r.sharedMaterial=game.v37Art.glowMaterial;r.color=new Color(1,.80f,.25f,0);r.transform.localScale=Vector3.one*.035f;s.particles.Add(r);}
            }
            s.until=Time.time+1;Pose(s);
        }
        void Pose(Stun s)
        {
            for(int i=0;i<s.particles.Count;i++)
            {
                var r=s.particles[i];float age=1-(s.until-Time.time),p=Mathf.Repeat(age*.9f+i*.127f,1),angle=i*2.399f+age*2.5f;
                r.enabled=s.actor.gameObject.activeInHierarchy;r.sortingOrder=s.actor.sortingOrder+5;
                r.transform.position=s.actor.bounds.center+new Vector3(Mathf.Cos(angle)*(.18f+p*.4f)*Mathf.Max(.6f,s.actor.bounds.size.x),Mathf.Sin(angle)*.14f+p*.5f,0);
                r.transform.localScale=Vector3.one*(.035f+Mathf.Sin(p*Mathf.PI)*.055f);r.color=new Color(1,.79f,.22f,Mathf.Sin(p*Mathf.PI)*Mathf.Clamp01((s.until-Time.time)*5));
            }
        }
        void Update()
        {
            if(!game || game.paused)return;
            foreach(var s in stuns.Values.ToArray())if(!s.actor || Time.time>=s.until){foreach(var r in s.particles)if(r)Destroy(r.gameObject);stuns.Remove(s.actor);}else Pose(s);
        }
        void OnDestroy(){foreach(var s in stuns.Values)foreach(var r in s.particles)if(r)Destroy(r.gameObject);stuns.Clear();}
    }
    public sealed partial class Combat
    {
        float durationV44,dashRemainingV44,dashClockV44,attackStartedAtV44;
        public bool ZenInvulnerable=>Attacking && active?.kind==ItemKind.Zen;
        public bool ZenDashing=>ZenInvulnerable && dashRemainingV44>.0001f;
        public float AttackDurationV44=>durationV44;
        public static float ZenIntervalV44(float hunger)=>Mathf.Max(1/9f,Mathf.Lerp(1/3.5f,.58f,Mathf.Clamp01(hunger/100))/3);
        bool PayZenV44()
        {
            var s=game.survivor;float hunger=Mathf.Min(s.hunger,4),healthCost=4-hunger;
            if(s.health<=healthCost){game.ui.Notice("Недостаточно здоровья");return false;}
            s.hunger-=hunger;if(!s.InfiniteVitalsV58)s.health-=healthCost;return true;
        }
        public static bool ThrustV44(ItemKind kind)=>kind==ItemKind.Stake || kind==ItemKind.Spear || kind==ItemKind.GoldenSpear;
        public static bool VerticalV44(ItemKind kind)=>kind==ItemKind.Axe || kind==ItemKind.Stake || kind==ItemKind.Zen || kind==ItemKind.GoldenSpear;
        public static float WeaponLengthV44(ItemKind kind)=>kind==ItemKind.Stake || kind==ItemKind.Spear || kind==ItemKind.GoldenSpear?2.60f:kind==ItemKind.Zen?2.35f:kind==ItemKind.Sword?1.95f:kind==ItemKind.Dagger || kind==ItemKind.Spectrum?.88f:1.65f;
        void StrikeV44(ICombatTarget target)
        {
            if(!game.CanReachV54(target,DamageDeliveryV54.Melee))return;
            if(active.kind==ItemKind.GoldenSpear)game.v44.Apply(target);
            if(active.kind==ItemKind.Spectrum)game.spellcraft.Damage(target,active.Definition.damage,true);else game.HurtTarget(target,AmazonSettlementV46.WeaponDamage(active,target),game.hero,DamageDeliveryV54.Melee);
        }
        public void MoveZenDashV44(float dt)
        {
            if(!ZenDashing)return;
            float duration=Mathf.Min(.14f,durationV44*.55f),step=Mathf.Min(dashRemainingV44,2*dt/duration);dashClockV44+=dt;dashRemainingV44-=step;
            // Check the whole path in small increments; a high-speed dash cannot cross a wall.
            Vector2 at=game.body.position;int pieces=Mathf.Max(1,Mathf.CeilToInt(step/.045f));Vector2 delta=aim*(step/pieces);
            for(int i=0;i<pieces;i++){var next=at+delta;if(!game.spellcraft.CanLand(game.map.CurrentRegion,next) || game.WorldBlocked(at,next,.28f))break;at=next;}
            game.body.position=at;game.hero.transform.position=new Vector3(at.x,at.y,game.hero.transform.position.z);
        }
    }
}
