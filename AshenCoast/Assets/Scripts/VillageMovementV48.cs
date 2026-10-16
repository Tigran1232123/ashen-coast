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
        List<Vector2> route=new List<Vector2>();Vector2 routeGoal,velocity,patrolGoal;int routeIndex,patrolVisit;float rest,repath;bool restingV54;
        public void ResetRouteV48(){route.Clear();routeIndex=0;velocity=Vector2.zero;repath=0;}
        Vector2 PatrolV48(float dt)
        {
            bool arrived=patrolGoal!=Vector2.zero && Vector2.Distance(Position,patrolGoal)<.4f;
            if(arrived && !restingV54){restingV54=true;rest=5+(index%4)*2.2f;}
            if(restingV54)rest=Mathf.Max(0,rest-dt);
            if(patrolGoal==Vector2.zero || arrived && rest<=0)
            {
                patrolVisit++;restingV54=false;
                float a=(index*1.7f+patrolVisit*1.31f);
                if(HuntingV48)patrolGoal=home+new Vector2(Mathf.Sin(a)*13,Mathf.Cos(a)*10);
                else if(patrolVisit%4==0)patrolGoal=village.houses[(index+patrolVisit)%village.houses.Count].entrance+Vector2.down*1.2f;
                else if(patrolVisit%4==1)patrolGoal=village.Centre+new Vector2((index%2==0?-1:1)*4.2f,-3.4f);
                else if(patrolVisit%4==2)patrolGoal=village.Centre+new Vector2(-7+(index%4)*4.5f,3.8f);
                else patrolGoal=village.houses[(index+patrolVisit/2)%village.houses.Count].entrance+new Vector2(index%2==0?1:-1,-1.1f);
            }
            if(Vector2.Distance(Position,patrolGoal)<.4f && rest>0){aim=Vector2.Lerp(aim,(village.Centre-Position).normalized,Mathf.Min(1,dt*2)).normalized;return Position;}
            return patrolGoal;
        }
        bool MoveV48(Vector2 destination,float speed,float dt)
        {
            if(dt<=0)return false;repath=Mathf.Max(0,repath-dt);
            if(route.Count==0 && repath<=0 || Vector2.Distance(routeGoal,destination)>.55f && repath<=0)
            {routeGoal=destination;route=village.PathV48(Position,destination);routeIndex=0;repath=.35f;}
            while(routeIndex<route.Count && Vector2.Distance(Position,route[routeIndex])<.26f)routeIndex++;
            if(routeIndex>=route.Count){velocity=Vector2.MoveTowards(velocity,Vector2.zero,dt*8);if(Vector2.Distance(Position,destination)>.4f && repath<=0)route.Clear();return false;}
            Vector2 delta=route[routeIndex]-Position;Vector2 wanted=delta.normalized*speed;
            // Soft separation is stable; it never swaps a route's chosen side of a fence.
            foreach(var other in village.people)if(other!=this && other.Alive && !other.Captive && other.Renderer.gameObject.activeInHierarchy)
            {var apart=Position-other.Position;float distance=apart.magnitude;if(distance>.01f && distance<.50f)wanted+=apart/distance*(.50f-distance)*2.5f;}
            velocity=Vector2.MoveTowards(velocity,wanted,dt*13);var move=Vector2.ClampMagnitude(velocity*dt,delta.magnitude);
            if(!village.game.GroundAllowedV58(Position+move,.24f) || village.game.WorldBlocked(Position,Position+move,.24f) || village.Interior && Physics2D.OverlapCircleAll(Position+move+Vector2.up*.13f,.20f).Any(c=>!c.isTrigger && c.gameObject!=village.game.hero.gameObject))
            {velocity=Vector2.zero;route.Clear();return false;}
            Renderer.transform.position+=(Vector3)move;step+=move.magnitude;
            if(move.sqrMagnitude>.000001f){aim=Vector2.Lerp(aim,move.normalized,Mathf.Min(1,dt*9)).normalized;return true;}return false;
        }
        void TickSparringV48(float dt)
        {
            victim=SparringPartnerV48;
            float angle=village.game.DaySeconds*.23f+TrainingPhaseV48;float distance=Weapon.kind==ItemKind.Bow || SparringPartnerV48.Weapon?.kind==ItemKind.Bow?2.5f:1.02f;
            float beat=Mathf.Repeat(village.game.DaySeconds+TrainingPhaseV48*1.7f,5);
            float dodge=Mathf.Sin(beat*Mathf.PI)*.35f;
            var offset=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)*.62f)*(distance+dodge)*TrainingSideV48;
            var destination=TrainingPointV48+offset;
            bool moving=Vector2.Distance(Position,destination)>.30f && MoveV48(destination,2.3f,dt);
            aim=(victim.Position-Position).normalized;
            if(Vector2.Distance(Position,TrainingPointV48)<distance+.8f && Vector2.Distance(victim.Position,TrainingPointV48)<distance+.8f && ready<=0 && (Mathf.Repeat(village.game.DaySeconds+TrainingPhaseV48,2.7f)<1.45f)==(TrainingSideV48>0) && !village.game.WorldBlocked(Position,victim.Position,.04f))BeginAttack();
            Pose(moving);
        }
    }
}
