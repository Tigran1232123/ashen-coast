using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class ChecksV54
    {
        IEnumerator PosesV54()
        {
            var g=game;var v=g.amazons;g.paused=true;var sheet=new GameObject("Registered combat grips");bool bounded=true;
            var kinds=new[]{ItemKind.Stake,ItemKind.Axe,ItemKind.Dagger,ItemKind.Bow};
            for(int row=0;row<4;row++)
            {var a=v.people.First(p=>!p.IsSlave && p.Weapon?.kind==kinds[row]);Set(a,"aim",Vector2.right);Call(a,"BeginAttack");float duration=(float)a.GetType().GetField("duration",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(a);for(int n=0;n<8;n++){Set(a,"attackAge",duration*(n+.1f)/8);Call(a,"Pose",false);bounded&=a.weaponSprite.bounds.size.x<3 && a.weaponSprite.bounds.size.y<3;var clone=Instantiate(a.Renderer.gameObject);clone.transform.SetParent(sheet.transform,true);clone.transform.position=new Vector2(1000+n*3.1f,1000-row*2.6f);foreach(var t in clone.GetComponentsInChildren<Transform>())t.gameObject.layer=31;}}
            Check(bounded,"all 32 training weapon poses have natural dimensions and registered grips");Capture("all-training-grips.png",new Vector2(1011,997),8.3f,1<<31);sheet.SetActive(false);Destroy(sheet);yield return null;
        }
        IEnumerator StorageV54()
        {
            var v=game.amazons;game.paused=true;
            Check(v.houses.SelectMany(h=>h.furnishings).All(p=>Mathf.Abs(p.renderer.bounds.min.y-p.Position.y)<.001f),"all eleven storage pots have a floor anchor instead of a centred floating sprite");
            foreach(int i in new[]{0,3,7,9}){v.EnterHouse(v.houses[i]);game.paused=true;game.body.position=new Vector2(0,-2.8f);game.hero.transform.position=game.body.position;Capture("house-"+i+".png",new Vector2(0,.65f),6.2f);v.LeaveHouse();yield return null;}
        }
        IEnumerator RefineV54()
        {
            var g=game;var v=g.amazons;g.paused=true;g.journey.SetArea(WorldArea.Coast,v.Centre+Vector2.down*11);g.paused=true;
            var woman=v.people.First(p=>!p.IsSlave && p.Weapon?.kind==ItemKind.Stake);woman.Warp(v.Centre+new Vector2(-5,-3));Set(woman,"aim",Vector2.right);Call(woman,"BeginAttack");float duration=(float)woman.GetType().GetField("duration",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(woman);var sheet=new GameObject("Registered natural training spears");bool scale=true;
            for(int n=0;n<8;n++)
            {Set(woman,"attackAge",duration*(n+.1f)/8);Call(woman,"Pose",false);scale&=woman.weaponSprite.bounds.size.x<2.75f && woman.weaponSprite.bounds.size.y<.5f;var clone=Instantiate(woman.Renderer.gameObject);clone.transform.SetParent(sheet.transform,true);clone.transform.position=new Vector2(1000+n*3.1f,1000);foreach(var t in clone.GetComponentsInChildren<Transform>())t.gameObject.layer=31;}
            Check(scale,"all eight spear poses preserve natural 2.6-unit horizontal reach");Capture("training-natural.png",new Vector2(1011,1000.85f),8.3f,1<<31);sheet.SetActive(false);Destroy(sheet);woman.PeaceV53();
            for(int home=0;home<10;home++)
            {v.EnterHouse(v.houses[home]);g.paused=true;g.body.position=new Vector2(0,-2.8f);g.hero.transform.position=g.body.position;Check(v.houses[home].furnishings.All(p=>Mathf.Abs(p.Position.x)==2.5f && p.Position.y==-2.5f && v.RoomWalkableV54(p.Position)),"storage footprint on clear inner floor, home "+home);Capture("house-"+home+".png",new Vector2(0,.65f),6.2f);v.LeaveHouse();yield return null;}
        }
        IEnumerator FinalV54()
        {
            var g=game;var v=g.amazons;g.paused=true;g.seconds=200;g.journey.SetArea(WorldArea.Coast,v.Centre+Vector2.down*11);g.paused=true;v.Tick(.01f);
            Capture("communal-hearth-final.png",v.Centre+Vector2.up*2,10);
            Check(v.fires.Take(28).All(f=>f.widthV54>1 && f.flame.enabled) && v.fires.Take(28).Select(f=>f.logs).Distinct().Count()==1,"large common fire surrounds pedestal with animated ring flames");
            Check(!v.WalkableV48(v.Centre+Vector2.left*2.5f),"villagers avoid hot communal coals");
            Check(v.houses.All(h=>v.PathV48(h.entrance+Vector2.down*.6f,v.Centre+Vector2.down*9.5f).Count>0),"house routes remain connected around enlarged common hearth");
            var woman=v.people.First(p=>!p.IsSlave && p.Weapon?.kind==ItemKind.Stake);woman.Warp(v.Centre+new Vector2(-5,-3));Set(woman,"aim",Vector2.right);Call(woman,"BeginAttack");float duration=(float)woman.GetType().GetField("duration",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(woman);var sheet=new GameObject("Corrected training pose sheet");
            for(int n=0;n<8;n++)
            {
                Set(woman,"attackAge",duration*(n+.1f)/8);Call(woman,"Pose",false);Check(woman.weaponSprite.bounds.size.x<2.75f && woman.weaponSprite.bounds.size.y<.5f,"natural training spear scale, frame "+n);
                var clone=Instantiate(woman.Renderer.gameObject);clone.transform.SetParent(sheet.transform,true);clone.transform.position=new Vector2(1000+n*3.1f,1000);foreach(var t in clone.GetComponentsInChildren<Transform>())t.gameObject.layer=31;
            }
            Capture("training-final.png",new Vector2(1011,1000.85f),8.3f,1<<31);sheet.SetActive(false);Destroy(sheet);woman.PeaceV53();
            g.seconds=60;var guard=v.people.First(p=>!p.IsSlave && !p.Hunter && !p.Slaver && !p.HuntingV48 && !p.WeaponMerchantV48 && p!=woman);guard.Warp(v.Centre+new Vector2(-6,-3));Set(guard,"patrolGoal",guard.Position);Set(guard,"restingV54",false);Vector2 before=guard.Position;for(int n=0;n<30;n++)Call(guard,"PatrolV48",.1f);Check(Vector2.Distance(before,(Vector2)Call(guard,"PatrolV48",.1f))<.1f,"daytime rest starts on arrival instead of expiring during travel");
            g.paused=false;g.ui.open=true;g.ui.QueuePointerForCheck(new Vector2(g.ui.CanvasWidth-4,g.ui.CanvasHeight-4),EventType.Repaint,0);Set(g.ui,"hoverItem",new Item(ItemKind.Zen));var r=g.ui.TooltipRectV54;Check(g.ui.TooltipVisibleV54 && r.xMin>=0 && r.yMin>=0 && r.xMax<g.ui.CanvasWidth && r.yMax<g.ui.CanvasHeight,"hover tooltip stays visible and inside screen edges during play");
            var item=new Item(ItemKind.Clay);g.ui.holding.Add(item);Set(g.ui,"dragItem",item);Check(!g.ui.TooltipVisibleV54,"no tooltip while an item is held");Set(g.ui,"dragItem",null);g.ui.holding.items.Clear();Set(g.ui,"hoverItem",null);Check(!g.ui.TooltipVisibleV54,"tooltip clears immediately on pointer departure");g.ui.ClearCheckPointer();g.ui.open=false;g.paused=true;
            for(int home=0;home<10;home++)
            {v.EnterHouse(v.houses[home]);g.paused=true;g.body.position=new Vector2(0,-2.8f);g.hero.transform.position=g.body.position;Capture("house-final-"+home+".png",new Vector2(0,.65f),6.2f);Check(v.houses[home].furnishings.All(p=>Mathf.Abs(p.Position.x)<=4 && p.Position.y==-1),"usable storage stands on inner floor, home "+home);v.LeaveHouse();yield return null;}
            GroundSheetV54();yield return null;
        }
    }
}
