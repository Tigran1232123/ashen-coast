using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed partial class CoastGame
    {
        public float HurtTarget(ICombatTarget victim,float amount,SpriteRenderer source,DamageDeliveryV54 delivery=DamageDeliveryV54.Other)
        {
            if(!CanReachV54(victim,delivery))return 0;
            if(victim==null || !victim.Alive || amount<=0 || (spellcraft!=null && spellcraft.ImmobilizedV44(source)))return 0;
            if(CoopOnline && coop.IsPlayer(victim.Renderer) && coop.IsPlayer(source))return 0;
            if(CoopOnline && coop.world.RedirectDamage(victim,amount,source,delivery))return 0;
            if(victim is PracticeTargetV46 practice && source==hero && (!CoopOnline || coop.Context==null))RecordDummyV48(practice.Renderer,Mathf.Min(practice.health,amount));
            return victim is VillageActorV46 villager?villager.HurtFrom(amount,source):victim is NightHunter nightHunter?nightHunter.HurtFrom(amount,source):victim is FrogActor frog?frog.HurtFrom(amount,source):victim.Hurt(amount);
        }

        public int AppendOrbLights(Vector4[] positions,Vector4[] colors,int count)
        {
            void Light(SpriteRenderer orb,float radius)
            {
                if(!orb || !orb.enabled || !orb.gameObject.activeInHierarchy || count>=positions.Length)return;
                var p=orb.bounds.center;float strength=.22f+.025f*Mathf.Sin(seconds*2.4f+p.x);
                positions[count]=new Vector4(p.x,p.y,radius,strength);colors[count++]=new Vector4(.23f,.61f,1,1);
            }
            if(survivor.equipment[0]?.kind==ItemKind.Orb)Light(combat.weapon,1.25f);
            foreach(var drop in loot.Where(d=>!d.collected && d.item.kind==ItemKind.Orb && d.renderer.gameObject.activeInHierarchy)
                .OrderBy(d=>(d.position-body.position).sqrMagnitude).Take(8))Light(drop.renderer,1.45f);
            if(amazons!=null)count=amazons.AppendLights(positions,colors,count);
            if(spectrum!=null)
            {
                count=AppendSpectrumLights(positions,colors,count);
                count=spectrum.AppendRainbowLights(positions,colors,count);count=spectrum.AppendDepartureLightsV41(positions,colors,count);
            }
            return count;
        }
    }

    public sealed partial class NightHunter
    {
        public const float AttackDamage=100,AttackInterval=1,PlayerPriorityDistance=3.5f;
        SpriteRenderer aggressor;
        ICombatTarget victim;
        float attackCooldown;
        public SpriteRenderer Aggressor=>aggressor;
        public SpriteRenderer TargetRenderer=>victim?.Renderer;

        void ClearAggression(){aggressor=null;victim=null;attackCooldown=0;}
        void ChooseVictim()
        {
            var attacker=aggressor?game.WorldCreatures.FirstOrDefault(t=>t.Renderer==aggressor && t.Alive && t.Renderer.enabled && t.Renderer.gameObject.activeInHierarchy):null;
            if(attacker==null)aggressor=null;
            bool close=!game.Dead && Vector2.Distance(Position,game.body.position)<=PlayerPriorityDistance*(game.spellcraft?.Vision(sprite)??1);
            victim=close || attacker==null?new HeroCombatActor(game):attacker;
        }
        public float HurtFrom(float amount,SpriteRenderer source)
        {
            if(!Active || amount<=0)return 0;
            amount*=game.spellcraft?.DamageFactor(sprite)??1;float hit=Mathf.Min(health,amount);health=Mathf.Max(0,health-hit);
            if(source && source!=sprite && game.WorldCreatures.Any(t=>t.Renderer==source && t.Alive && source.enabled && source.gameObject.activeInHierarchy))aggressor=source;
            ChooseVictim();game.ShowDamage(hit,Position+Vector2.up*4,sprite);
            if(!Alive){ClearAggression();StartDeath();}return hit;
        }
        void AttackVictim(Vector2 previous,float dt)
        {
            attackCooldown=Mathf.Max(0,attackCooldown-dt);
            if(HeldBack || victim==null || !victim.Alive || attackCooldown>0 ||
                FortWorld.SegmentDistance(victim.Position,previous,Position)>=1.4f ||
                game.WorldBlocked(Position,victim.Position,.06f))return;
            // The short contact attack must also respect the physical field walls.
            var delta=victim.Position-Position;
            int hits=Physics2D.CircleCast(Position,.06f,delta.normalized,wallFilter,castHits,delta.magnitude);
            if(hits==castHits.Length)return;
            for(int i=0;i<hits;i++)
            {
                var t=castHits[i].collider.transform;
                if(t!=sprite.transform && !t.IsChildOf(sprite.transform) && t!=victim.Renderer.transform && !t.IsChildOf(victim.Renderer.transform))return;
            }
            if(game.HurtTarget(victim,AttackDamage,sprite,DamageDeliveryV54.Melee)>0){attackCooldown=AttackInterval;attackAge=0;}
        }
    }
}
