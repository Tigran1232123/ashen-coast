using System.Collections.Generic;
using UnityEngine;
namespace AshenCoast
{
    public static class RiverNavigationV58
    {
        public static bool Clear(Vector2 from,Vector2 to,float margin=.25f)
        {
            if(!RiverV58.Ready)return true;int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(from,to)/1.1f));
            for(int i=0;i<=steps;i++){var p=Vector2.Lerp(from,to,i/(float)steps);if(WorldShape.CoastDistanceV58(p)<margin || !RiverV58.Passable(p,margin))return false;}return true;
        }
        // An eight-node graph connects the banks through the three real bridge decks.
        public static List<Vector2> Plan(Vector2 from,Vector2 to)
        {
            if(Clear(from,to))return new List<Vector2>{to};var nodes=new Vector2[8];nodes[0]=from;nodes[1]=to;
            for(int i=0;i<3;i++){var c=RiverV58.Centre(i,RiverV58.BridgeT(i));var n=RiverV58.Normal(i,RiverV58.BridgeT(i));nodes[2+i*2]=c-n*25;nodes[3+i*2]=c+n*25;}
            var costs=new float[8];var previous=new int[8];var closed=new bool[8];var edges=new bool[8,8];for(int i=0;i<8;i++){costs[i]=float.PositiveInfinity;previous[i]=-1;for(int j=0;j<i;j++)edges[i,j]=edges[j,i]=Clear(nodes[i],nodes[j]);}costs[0]=0;
            for(int step=0;step<8;step++){int at=-1;for(int i=0;i<8;i++)if(!closed[i] && (at<0 || costs[i]<costs[at]))at=i;if(at<0 || float.IsInfinity(costs[at]))break;if(at==1){var result=new List<Vector2>();while(at!=0){result.Add(nodes[at]);at=previous[at];}result.Reverse();return result;}closed[at]=true;for(int i=0;i<8;i++)if(!closed[i] && edges[at,i]){float next=costs[at]+Vector2.Distance(nodes[at],nodes[i]);if(next<costs[i]){costs[i]=next;previous[i]=at;}}}return new List<Vector2>();
        }
    }
    public sealed partial class CoastGame
    {
        public bool GroundAllowedV58(Vector2 to,float radius=.22f)=>!journey.IsCoast || magic.InPocket || WorldShape.CoastDistanceV58(to)>radius && RiverV58.Passable(to,radius);
    }
    public sealed partial class CompanionMage
    {
        readonly List<Vector2> coastRouteV58=new List<Vector2>();Vector2 coastGoalV58;float coastPlanAtV58;
        Vector2 FollowRiverV58(Vector2 goal,float dt)
        {
            if(Time.time>=coastPlanAtV58 && (coastRouteV58.Count==0 || Vector2.Distance(coastGoalV58,goal)>3))
            {coastRouteV58.Clear();coastRouteV58.AddRange(RiverNavigationV58.Plan(Position,goal));coastGoalV58=goal;coastPlanAtV58=Time.time+1;}
            while(coastRouteV58.Count>0 && Vector2.Distance(Position,coastRouteV58[0])<.3f)coastRouteV58.RemoveAt(0);
            var delta=(coastRouteV58.Count==0?goal:coastRouteV58[0])-Position;return Steer(delta.normalized,Mathf.Min(dt*FollowSpeed,delta.magnitude));
        }
    }
}
