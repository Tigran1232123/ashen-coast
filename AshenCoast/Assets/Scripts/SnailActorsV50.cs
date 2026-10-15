using UnityEngine;
namespace AshenCoast
{
    public sealed partial class VillageActorV46
    {
        public void ReturnHomeV50(Vector2 goal,float dt){attackAge=-1;bool moving=MoveV48(goal,CoastGame.WalkSpeed,dt);Pose(moving);}
        public void WalkHomeV50(Vector2 at)
        {var move=at-Position;step+=move.magnitude;if(move.sqrMagnitude>.00001f)aim=move.normalized;Renderer.transform.position=at;attackAge=-1;Pose(move.sqrMagnitude>.00001f);}
    }
    public sealed partial class CompanionMage
    {
        readonly System.Collections.Generic.List<Vector2> returnRouteV50=new System.Collections.Generic.List<Vector2>();float returnRepathV50;
        public void ReturnHomeV50(Vector2 goal,float dt)
        {returnRepathV50-=dt;if(returnRouteV50.Count==0 || returnRepathV50<=0){returnRouteV50.Clear();returnRouteV50.AddRange(game.amazons.PathV48(Position,goal));returnRepathV50=1;}while(returnRouteV50.Count>0 && Vector2.Distance(Position,returnRouteV50[0])<.22f)returnRouteV50.RemoveAt(0);var seek=returnRouteV50.Count>0?returnRouteV50[0]:goal;var next=Vector2.MoveTowards(Position,seek,CoastGame.WalkSpeed*dt);if(!game.WorldBlocked(Position,next,.22f) && SnailHomesV50.Land(next))WalkHomeV50(next);}
        public void WalkHomeV50(Vector2 at)
        {var move=at-Position;distance+=move.magnitude;walking=move.sqrMagnitude>.000001f;walkDirection=move;if(walking){direction=Facing(move);frame=Mathf.FloorToInt(distance/1.65f*8)%8;}sprite.transform.position=at;sprite.transform.localScale=Vector3.one;Firing=false;Pose();}
    }
    public sealed partial class FrogMage
    {public void LureV50(SpriteRenderer source){aggressor=source;ChooseFrogVictim();}}
}
