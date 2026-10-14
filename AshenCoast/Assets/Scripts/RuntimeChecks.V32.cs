using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        IEnumerator CaravanExplosionChecks()
        {
            var g=game;var j=g.journey;var a=g.arcane;g.seconds=40;g.survivor.health=100;g.survivor.mana=100;
            j.SetArea(WorldArea.Coast,j.CartPosition+Vector2.down);j.Tick(0);g.UpdateEnvironment(0);
            g.checkCameraPosition=new Vector3(-1,g.ship.bounds.center.y+3,-20);g.checkCameraSize=8;g.ResetCamera(g.checkCameraPosition.Value);
            var giant=g.woodlice.First(m=>!m.Small);giant.Tick(0,false);
            Check(j.CartPosition.y>g.ship.bounds.max.y+.65f,"caravan route is above the ship sprite");
            Check(j.pullers.All(p=>p.transform.localScale==giant.renderer.transform.localScale && Mathf.Abs(p.bounds.size.x-giant.renderer.bounds.size.x)<.02f),"both harnessed woodlice have the same size as the standalone giant mob");
            Check(j.caravanAnimals.All(p=>p.vitals.health==200) && j.cartDriver.health==100 && g.AdditionalTargets.Count(t=>t is CaravanWoodlouse || t is CaravanDriver)==3,"two 200 HP pullers and one 100 HP driver join all shared combat targets");
            yield return Capture("v32-caravan-above-ship.png");Check(j.BeginRide(),"healthy caravan still offers the original journey");
            foreach(float dt in new[]{1.25f,4.15f,.9f,4.1f,.95f})j.Tick(dt);
            Check(!j.Cutscene && j.Area==WorldArea.GoldenField && g.body.simulated,"boarding travel black-screen arrival and dismount remain functional");
            Check(j.BeginRide(),"return ride is available with healthy animals");foreach(float dt in new[]{1.25f,4.15f,.9f,4.1f,.95f})j.Tick(dt);
            Check(!j.Cutscene && j.IsCoast && Mathf.Abs(j.CartPosition.y-j.CoastRouteY)<.01f,"return arrival uses the new route above the ship");
            g.checkCameraPosition=new Vector3(j.CartPosition.x+2,j.CartPosition.y+1.8f,-20);g.checkCameraSize=6;g.ResetCamera(g.checkCameraPosition.Value);
            var pet=j.caravanAnimals[0];var other=j.caravanAnimals[1];a.DamageArea(pet.Position,.3f,20);
            Check(pet.vitals.health==180 && pet.vitals.Curled && j.cartDriver.Fleeing && !j.CaravanReady,"actual area attack curls the puller and makes the driver abandon his wagon");
            Check(pet.Hurt(100)==20 && pet.vitals.health==160,"curled harness animal absorbs eighty percent of incoming damage");
            var cart=j.CartPosition;for(int i=0;i<20;i++)j.Tick(.03f);float start=Vector2.Distance(j.cartDriver.Position,g.body.position);
            var runFrames=new HashSet<Sprite>();for(int i=0;i<30;i++){j.Tick(.03f);runFrames.Add(j.driver.sprite);}
            Check(j.CartPosition==cart && Vector2.Distance(j.cartDriver.Position,g.body.position)>start+.4f && runFrames.Count>=5,"wagon stops while driver runs away on foot with distinct movement frames");
            yield return Capture("v32-curled-animal-fleeing-driver.png");
            j.Tick(5.1f);Check(!pet.vitals.Curled,"harnessed woodlouse unfolds after the same five-second duration");pet.Hurt(10);Check(pet.vitals.Curled,"another hit triggers a new curl after recovery");
            other.Hurt(10);Check(other.vitals.Curled && other.vitals.health==190,"the second puller has independent health and defence state");j.cartDriver.Hurt(30);Check(j.cartDriver.health==70,"driver takes damage against his own one-hundred-point health pool");
            j.cartDriver.Hurt(70);Check(!j.cartDriver.Alive && !j.driver.enabled && !g.AdditionalTargets.Contains(j.cartDriver),"defeated driver is removed from combat and travel interactions");
            j.SetArea(WorldArea.Coast,new Vector2(12,12));g.checkCameraPosition=new Vector3(16,15.5f,-20);g.checkCameraSize=6;g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);g.numbers.Clear();
            var explosion=a.CreateExplosion(new Vector2(16,14));a.Tick(.10f);yield return Capture("v32-explosion-ignition.png");a.Tick(.22f);yield return Capture("v32-explosion-shockwave.png");a.Tick(.35f);yield return Capture("v32-explosion-fireball.png");
            Check(explosion.renderer.sprite.texture.name=="PyromanticExplosionV32" && explosion.ring && a.burstSparks.Count>0 && g.art.explosion.Length==24,"shared detonation uses 24 frames plus animated ground shockwave and ballistic pixel cinders");
            a.Tick(.40f);yield return Capture("v32-explosion-smoke.png");a.Tick(.5f);yield return null;yield return new WaitForEndOfFrame();Check(!a.explosions.Contains(explosion) && !explosion.ring,"whole explosion and ring finish without leaving effects behind");
            g.survivor.equipment[0]=new Item(ItemKind.Lamp);g.survivor.inventory.items.Clear();g.survivor.inventory.Add(new Item(ItemKind.Pyromancy){opened=true});
            g.body.position=new Vector2(14,13);g.hero.transform.position=g.body.position;var fire=a.CreateCampfire(new Vector2(16.5f,14.5f));
            Check(a.TryExplodeFire(fire.position) && a.explosions.Last().renderer.sprite.texture.name=="PyromanticExplosionV32","actual campfire detonation calls the upgraded shared animation");g.dragons.Tick(2.3f);
            Check(g.dragons.summons.Last().pillar.sprite.texture.name=="DragonFireColumnV32","summoning pillar uses the new fire-only pixel atlas without rocks");yield return Capture("v32-pixel-column-natural-scorch.png");g.dragons.Clear();a.Tick(2);
            j.companion.Hire(new Vector2(16,14));Check(j.companion.Explode() && j.companion.mana==60 && a.explosions.Last().renderer.sprite.texture.name=="PyromanticExplosionV32","ally command uses the same upgraded explosion and preserves mana cost");a.Tick(2);
            g.fort.Enter();var mage=g.fort.bandits.Last();mage.Warp(new Vector2(0,-7));g.body.position=new Vector2(0,-2);g.hero.transform.position=g.body.position;mage.ClearAggro();mage.Tick(.7f);
            var jet=a.flames.Where(f=>f.damaging && f.hostile && f.renderer.transform.parent==g.fort.root.transform).ToArray();
            Check(jet.Length>=6 && jet.All(f=>f.reach==FrogMage.FireReach) && jet.All(f=>Vector2.Angle(f.velocity,g.body.position+Vector2.up*.6f-f.origin)<=FrogMage.FireArc*.5f+.1f),"dungeon mage emits a timed turbulent five-unit 120-degree cone like the frog");
            g.checkCameraPosition=new Vector3(0,-3.5f,-20);g.checkCameraSize=5;g.ResetCamera(g.checkCameraPosition.Value);a.Tick(.16f);g.UpdateEnvironment(0);yield return Capture("v32-dungeon-mage-wide-fire.png");
            g.body.position=mage.Position+Vector2.up*1.5f;g.hero.transform.position=g.body.position;mage.Tick(.8f);int blasts=mage.BlastCount;mage.Tick(1.08f);
            Check(mage.BlastCount==blasts+1 && a.explosions.Last().hostile && a.explosions.Last().renderer.sprite.texture.name=="PyromanticExplosionV32","dungeon mage still delivers the same upgraded hostile radial blast");
            yield return null;yield return new WaitForEndOfFrame();
            bool overlap=false;var labels=g.ui.actorLabelBounds;for(int x=0;x<labels.Count;x++)for(int y=x+1;y<labels.Count;y++)overlap|=labels[x].Overlaps(labels[y]);Check(!overlap,"world names and health bars reserve separate nonoverlapping rectangles");
            g.checkCameraPosition=null;g.checkCameraSize=null;g.ui.CloseContainers();
        }
    }
}
