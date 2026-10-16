using System.Collections;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class ChecksV58
    {
        GameObject Outline(FloorPlanV58 plan)
        {
            var obj=new GameObject("QA floor footprint");var l=obj.AddComponent<LineRenderer>();l.sharedMaterial=game.art.lit;l.positionCount=plan.points.Length;l.loop=true;l.startWidth=l.endWidth=.035f;l.startColor=l.endColor=new Color(.35f,1,.42f);l.sortingOrder=20000;l.SetPositions(plan.points.Select(p=>(Vector3)p).ToArray());return obj;
        }
        IEnumerator AuditFloorsV58()
        {
            var g=game;
            for(int home=0;home<10;home++)
            {
                g.amazons.EnterHouse(g.amazons.houses[home]);var floor=g.amazons.CurrentFloorV58;var outline=Outline(floor);Check(floor.Contains(g.body.position,.15f),"Amazon home "+home+" entry lies inside authored floor");
                Check(Vector2.Distance(g.amazons.ClampRoom(new Vector2(20,20)),new Vector2(20,20))>10,"Amazon home "+home+" walls constrain movement");
                Check(!Physics2D.OverlapCircleAll(g.body.position+Vector2.up*.13f,.20f).Any(c=>c.enabled && !c.isTrigger && c.gameObject!=g.hero.gameObject),"Amazon home "+home+" entry has no invisible furniture collision");
                View("floor-amazon-"+home,Vector2.zero,6);outline.SetActive(false);Destroy(outline);g.amazons.LeaveHouse();
            }
            for(int home=0;home<4;home++)
            {
                g.snails.EnterForCheckV58(home);var floor=g.snails.FloorV58(home);var outline=Outline(floor);
                Check(floor.Contains(g.body.position,.1f),"snail "+home+" entry lies on timber floor");
                Check(Vector2.Distance(g.snails.Clamp(new Vector2(-8,3)),new Vector2(-8,3))>3,"snail "+home+" curved wall constrains movement");
                View("floor-snail-"+home,new Vector2(.2f,-.5f),5.5f);outline.SetActive(false);Destroy(outline);g.snails.Leave();
            }
            Warp(WorldArea.Coast,new Vector2(20,30));var before=g.loot.Count(l=>l.item.kind==ItemKind.Feather);var gull=g.journey.gulls[0];gull.health=30;gull.Hurt(31);int feathers=g.loot.Count(l=>l.item.kind==ItemKind.Feather)-before;Check(feathers>=1 && feathers<=2,"gull death drops one or two feathers");
            var frames=g.amazons.people[0].gaitV53;var sprites=new System.Collections.Generic.HashSet<Sprite>();for(int n=0;n<8;n++){g.amazons.people[0].CheckPoseV58(n,true,false);sprites.Add(frames.Lower.sprite);}Check(sprites.Count==8,"all eight Amazon lower-body gait frames are distinct");
            g.amazons.people[0].CheckLeftPoseV58();Check(!frames.Lower.flipX,"Amazon boots face left when moving left");
            g.ui.CloseContainers();Warp(WorldArea.Coast,new Vector2(-1.5f,-1.65f));g.ui.OpenShip();Check(g.ui.ShipPanel.x>=0 && g.ui.ShipPanel.xMax<g.ui.PanelX && g.ui.ShipPanel.height>=370,"enlarged hold fits the inventory canvas");yield return null;g.ui.CloseContainers();
        }
    }
    public sealed partial class SnailHomesV50 {public void EnterForCheckV58(int home){doorWait=0;Enter(home);}}
    public sealed partial class VillageActorV46 {public void CheckLeftPoseV58(){attackAge=-1;aim=Vector2.left;PoseV54(true);}}
}
