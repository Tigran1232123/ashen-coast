using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class VillageActorV46
    {
        public bool WeaponMerchantV48,HuntingV48;
        public VillageActorV46 SparringPartnerV48;
        public Vector2 TrainingPointV48;
        public int TrainingSideV48;public float TrainingPhaseV48;
        public bool SparringV48=>SparringPartnerV48!=null && SparringPartnerV48.Alive && !Hostile && !Hired && !SparringPartnerV48.Hostile && village.TrainingTimeV48;
        List<Vector2> route=new List<Vector2>();Vector2 routeGoal,velocity,patrolGoal;int routeIndex,patrolVisit;float rest,repath;
        public void ResetRouteV48(){route.Clear();routeIndex=0;velocity=Vector2.zero;repath=0;}
        Vector2 PatrolV48(float dt)
        {
            rest=Mathf.Max(0,rest-dt);
            if(patrolGoal==Vector2.zero || rest<=0 && Vector2.Distance(Position,patrolGoal)<.4f)
            {
                patrolVisit++;rest=1.2f+(index%4)*.5f;
                float a=(index*1.7f+patrolVisit*1.31f);
                if(HuntingV48)patrolGoal=home+new Vector2(Mathf.Sin(a)*13,Mathf.Cos(a)*10);
                else if(patrolVisit%3==0)patrolGoal=village.houses[(index+patrolVisit)%village.houses.Count].entrance+Vector2.down*1.2f;
                else patrolGoal=village.Centre+new Vector2(Mathf.Sin(a),Mathf.Cos(a))*(12.5f+index%4*1.5f);
            }
            return patrolGoal;
        }
        bool MoveV48(Vector2 destination,float speed,float dt)
        {
            if(dt<=0)return false;repath=Mathf.Max(0,repath-dt);
            if(route.Count==0 && repath<=0 || Vector2.Distance(routeGoal,destination)>1.25f && repath<=0)
            {routeGoal=destination;route=village.PathV48(Position,destination);routeIndex=0;repath=.9f;}
            while(routeIndex<route.Count && Vector2.Distance(Position,route[routeIndex])<.26f)routeIndex++;
            if(routeIndex>=route.Count){velocity=Vector2.zero;if(Vector2.Distance(Position,destination)>.7f && repath<=0)route.Clear();return false;}
            Vector2 delta=route[routeIndex]-Position;Vector2 wanted=delta.normalized*speed;
            // Soft separation is stable; it never swaps a route's chosen side of a fence.
            foreach(var other in village.people)if(other!=this && other.Alive && !other.Captive && other.Renderer.gameObject.activeInHierarchy)
            {var apart=Position-other.Position;float distance=apart.magnitude;if(distance>.01f && distance<.50f)wanted+=apart/distance*(.50f-distance)*2.5f;}
            velocity=Vector2.MoveTowards(velocity,wanted,dt*13);var move=Vector2.ClampMagnitude(velocity*dt,delta.magnitude);
            if(village.game.WorldBlocked(Position,Position+move,.24f) || village.Interior && Physics2D.OverlapCircleAll(Position+move+Vector2.up*.13f,.20f).Any(c=>!c.isTrigger && c.gameObject!=village.game.hero.gameObject))
            {velocity=Vector2.zero;route.Clear();return false;}
            Renderer.transform.position+=(Vector3)move;step+=move.magnitude;
            if(move.sqrMagnitude>.000001f){aim=Vector2.Lerp(aim,move.normalized,Mathf.Min(1,dt*9)).normalized;return true;}return false;
        }
        void TickSparringV48(float dt)
        {
            victim=SparringPartnerV48;
            float angle=village.game.DaySeconds*.30f+TrainingPhaseV48;float distance=Weapon.kind==ItemKind.Bow || SparringPartnerV48.Weapon?.kind==ItemKind.Bow?2.5f:1.02f;
            var offset=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)*.6f)*distance*TrainingSideV48;
            var destination=TrainingPointV48+offset;
            bool moving=Vector2.Distance(Position,destination)>.30f && MoveV48(destination,2.3f,dt);
            aim=(victim.Position-Position).normalized;
            if(Vector2.Distance(Position,TrainingPointV48)<distance+.8f && Vector2.Distance(victim.Position,TrainingPointV48)<distance+.8f && ready<=0 && !village.game.WorldBlocked(Position,victim.Position,.04f))BeginAttack();
            Pose(moving);
        }
    }
}
