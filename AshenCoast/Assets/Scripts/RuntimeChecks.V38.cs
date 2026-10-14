using System.Collections;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        IEnumerator SpectrumChecksV38()
        {
            var g=game;var s=g.survivor;HandClearV34();g.ui.CloseContainers();s.inventory.items.Clear();s.spatialInventory.items.Clear();s.health=100;s.mana=100;
            g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));g.seconds=50;g.UpdateEnvironment(0);s.equipment[0]=new Item(ItemKind.Spectrum);
            var heroPosition=g.body.position;var item=new Item(ItemKind.Spectrum);var drop=g.ThrowItem(item,heroPosition+new Vector2(1.5f,-.8f));
            Check(drop!=null && drop.InFlight && g.body.position==heroPosition && !g.spectrum.Riding,"throwing Spectrum preserves player position and never starts travel");
            g.TickLoot(.20f);ViewV34(drop.position+Vector2.up*.7f,2.1f);yield return Capture("v38-spectrum-throw.png");
            var near=drop.renderer.transform.position;g.TickLoot(.34f);var before=drop.renderer.transform.position;g.TickLoot(.015f);
            Check(Vector2.Distance(before,drop.renderer.transform.position)<.13f && drop.renderer.sprite==g.v38Art.planted && drop.renderer.transform.rotation==Quaternion.identity,"dagger plants continuously at the same blade tip without a sprite swap or landing jump");
            var pivot=drop.renderer.transform.position;var bounds=drop.renderer.bounds;var sprite=drop.renderer.sprite;
            for(int i=0;i<60;i++){g.seconds+=.05f;g.TickLoot(.05f);}
            Check(drop.position==(Vector2)pivot && drop.renderer.transform.position==pivot && drop.renderer.bounds==bounds && drop.renderer.sprite==sprite,"sixty ground animation updates leave the dagger's position, scale, pivot and blade unchanged");
            Check(sprite.texture.name=="SpectrumV39" && g.art.icons[43].texture==sprite.texture,"held and planted dagger share the new worn steel and leather artwork");
            ViewV34(drop.position+Vector2.up*.6f,1.45f);g.cameraView.Render();var a=ReadPixels();g.seconds+=.65f;g.TickLoot(0);g.cameraView.Render();var b=ReadPixels();
            float rope=RegionDifference(a,b,new Rect(drop.position.x+.04f,drop.position.y+.58f,.38f,.48f));Destroy(a);Destroy(b);
            Check(rope>.0001f,"native rendered cord moves independently above the fixed blade (difference "+rope.ToString("F5")+")");
            yield return Capture("v38-spectrum-planted-detail.png");
            var points=new Vector4[32];var colors=new Vector4[32];float low=1,high=0;int n=0;
            for(int i=0;i<16;i++){g.seconds+=.13f;n=g.AppendOrbLights(points,colors,0);float value=points.First(p=>Mathf.Abs(p.x-drop.position.x)<.01f).w;low=Mathf.Min(low,value);high=Mathf.Max(high,value);}
            Check(n>=1 && high-low>.10f,"planted dagger casts an unstable changing ground light across its animation cycle");
            Check(g.CollectLoot(drop) && drop.collected && !drop.renderer.enabled,"new grounded sprite remains selectable and returns the original dagger item");
            g.ui.open=true;g.combat.manual=false;yield return DrawUIV34();g.combat.manual=true;ViewV34(heroPosition+Vector2.up*.7f,3);yield return Capture("v38-spectrum-inventory-and-hand.png");g.ui.CloseContainers();
        }
        IEnumerator RainbowChecksV38()
        {
            var g=game;var s=g.survivor;g.journey.SetArea(WorldArea.Coast,new Vector2(0,-1.65f));g.seconds=50;s.health=100;s.mana=100;g.ui.CloseContainers();
            var at=new Vector2(5.7f,1.3f);ViewV34(new Vector2(.7f,1.7f),8.4f);yield return null;
            Check(g.spectrum.Summon(at) && s.mana==60,"new rainbow still costs forty mana and creates the active wall");
            float top=g.cameraView.transform.position.y+g.cameraView.orthographicSize;
            Check(g.spectrum.wall.bounds.max.y>top+3 && g.spectrum.FallingHead>top-at.y,"flow begins above the viewport and its mesh continues beyond the screen");
            g.spectrum.Tick(.18f);yield return Capture("v38-rainbow-fall-01.png");
            float head=g.spectrum.FallingHead;g.spectrum.Tick(.20f);Check(g.spectrum.FallingHead<head && g.spectrum.ImpactStrength==0,"falling front travels downward before impact clouds appear");yield return Capture("v38-rainbow-fall-02.png");
            g.spectrum.Tick(.22f);yield return Capture("v38-rainbow-fall-03.png");
            g.spectrum.Tick(.30f);Check(g.spectrum.FallingHead<.01f && g.spectrum.ImpactStrength>.95f && g.spectrum.clouds.Count==12,"arrival produces large layered billowing clouds and waterfall-foot foam");yield return Capture("v38-rainbow-impact.png");
            var cloud=g.spectrum.clouds[3];var frame=cloud.sprite;float left=g.spectrum.clouds.Min(c=>c.bounds.min.x),right=g.spectrum.clouds.Max(c=>c.bounds.max.x);
            Check(right-left>8 && g.spectrum.clouds.Max(c=>c.bounds.size.y)>2,"cloud bank is substantially broader than the five-unit stream and has large detailed lobes");
            g.cameraView.Render();var a=ReadPixels();g.spectrum.Tick(.31f);g.cameraView.Render();var b=ReadPixels();
            float flow=RegionDifference(a,b,new Rect(at.x-1.8f,at.y+3.4f,3.6f,4));float clouds=RegionDifference(a,b,new Rect(at.x-4.4f,at.y-.3f,8.8f,2.3f));Destroy(a);Destroy(b);
            Check(flow>.018f,"actual rendered rainbow flows across frames instead of remaining static (difference "+flow.ToString("F4")+")");
            Check(clouds>.008f && cloud.sprite!=frame,"actual cloud silhouette and foam animate independently (difference "+clouds.ToString("F4")+")");yield return Capture("v38-rainbow-flow-second-pose.png");
            var points=new Vector4[32];var colors=new Vector4[32];int n=g.spectrum.AppendRainbowLights(points,colors,0);
            Check(n==5 && colors[0]!=colors[4] && points.All(p=>p.w<=.52f),"rainbow emits spatially separated colored lights rather than a flat blue ground plate");
            ViewV34(at+Vector2.up*8,10);g.spectrum.RefreshPresentation();top=g.cameraView.transform.position.y+g.cameraView.orthographicSize;
            Check(g.spectrum.wall.bounds.max.y>top+3,"camera movement and larger window view cannot reveal a cut-off top");
            ViewV34(new Vector2(.7f,1.7f),8.4f);g.spectrum.RefreshPresentation();
            float mana=s.mana;g.spectrum.Tick(1);Check(Mathf.Abs(mana-s.mana-2)<.001f,"animated rainbow retains two mana per second maintenance");
            var frog=g.arcane.frogs.First();frog.health=180;frog.Warp(at);g.spectrum.Tick(.05f);Check(!frog.Alive && g.spectrum.ContactHits>0,"waterfall contact still inflicts colossal actual damage");
            g.spectrum.Dismiss();float tail=g.spectrum.TailHeight,height=g.spectrum.wall.bounds.size.y;g.spectrum.Tick(.48f);
            Check(!g.spectrum.Active && g.spectrum.TailHeight<tail && Mathf.Abs(g.spectrum.wall.bounds.size.y-height)<.01f,"exit lowers a trailing edge while retaining strand texture scale");yield return Capture("v38-rainbow-tail-01.png");
            g.spectrum.Tick(.52f);yield return Capture("v38-rainbow-tail-02.png");g.spectrum.Tick(.70f);
            Check(g.spectrum.wall && g.spectrum.TailHeight==0 && g.spectrum.ImpactStrength>0,"stream tail disappears into lingering clouds before the whole effect is removed");yield return Capture("v38-rainbow-cloud-afterglow.png");
            g.spectrum.Tick(.85f);Check(!g.spectrum.wall && g.spectrum.clouds.Count==0 && g.spectrum.AppendRainbowLights(points,colors,0)==0,"ending clears clouds, spray, materials and lighting together");
            s.mana=100;Check(g.spectrum.Summon(at),"rainbow can be summoned again after the animated exit");g.spectrum.Tick(.9f);s.mana=.1f;g.spectrum.Tick(.1f);Check(!g.spectrum.Active && g.spectrum.wall,"mana depletion starts the same visible tail animation");g.spectrum.Tick(2.6f);
        }
    }
}
