using System.Collections;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        IEnumerator SphereAndWebV36()
        {
            var g=game;HandClearV34();g.survivor.inventory.items.Clear();g.seconds=225;g.survivor.health=100;g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));
            var orb=new Item(ItemKind.Orb);var uiArea=InventoryUI.ItemIconArea(orb,Vector2.zero);g.survivor.equipment[0]=orb;g.combat.manual=false;yield return null;yield return new WaitForEndOfFrame();g.combat.manual=true;
            Check(Mathf.Abs(g.combat.weapon.bounds.size.x-.32f)<.001f && orb.Definition.width==2 && orb.Definition.height==2,"sphere hand size and 2x2 inventory footprint remain unchanged");
            var drop=g.ThrowItem(new Item(ItemKind.Orb),g.body.position+new Vector2(1.6f,-.6f));g.TickLoot(.6f);
            Check(drop!=null && Mathf.Abs(drop.renderer.bounds.size.x-.60f)<.001f && drop.cloth.enabled,"ground sphere is thirty percent smaller and still rests on linen");
            var p=new Vector4[32];var colors=new Vector4[32];int n=g.AppendOrbLights(p,colors,0);
            Check(n==2 && p.Take(n).All(v=>v.z<1.5f && v.w>.15f && v.w<.3f),"held and ground spheres emit restrained local blue light");
            var block=new MaterialPropertyBlock();drop.renderer.GetPropertyBlock(block);Check(block.GetFloat("_Emission")>.3f,"sphere surface retains a light glow in dark surroundings");
            ViewV34(drop.position+Vector2.up*.55f,3.5f);yield return Capture("v36-small-glowing-ground-sphere.png");
            g.body.position=drop.position+Vector2.left*.8f;g.hero.transform.position=g.body.position;bool collected=g.CollectLoot(drop);g.combat.manual=false;yield return null;yield return new WaitForEndOfFrame();g.combat.manual=true;
            Check(collected && drop.collected && g.AppendOrbLights(p,colors,0)==1 && InventoryUI.ItemIconArea(orb,Vector2.zero)==uiArea,"collection removes ground light without altering the inventory sprite size");
            HandClearV34();g.survivor.equipment[0]=null;var c=g.cave;c.Enter();g.body.position=new Vector2(-8,-16);g.hero.transform.position=g.body.position;g.survivor.health=100;
            Check(c.torches.Count==0 && !c.root.GetComponentsInChildren<Transform>(true).Any(t=>t.name.Contains("Catacomb torch") || t.name.Contains("Catacomb iron torch")) && c.AppendLights(p,colors,0)==0,"catacombs have no torch fixtures, torch flames or torch illumination");
            foreach(var spider in c.spiders)spider.health=0;
            c.Snare(2);g.checkMovement=Vector2.right;g.body.linearVelocity=Vector2.right*4;var at=g.body.position;yield return new WaitForSeconds(.12f);
            Check(c.Snared && g.FeetLocked && !g.MovementLocked && g.body.linearVelocity.sqrMagnitude<.0001f && Vector2.Distance(at,g.body.position)<.03f,"web stops actual movement and momentum while leaving action controls unlocked");
            g.survivor.equipment[0]=new Item(ItemKind.Sword);g.combat.ResetForCheck();g.combat.readyAt=0;
            Check(g.combat.Begin(Vector2.right),"a snared player can still start a melee attack");g.combat.Advance(.4f);g.combat.ResetForCheck();
            g.survivor.equipment[0]=new Item(ItemKind.Lamp);g.survivor.mana=100;Check(g.arcane.FireLampBurst(Vector2.right),"a snared player can still fire the lamp");g.arcane.CancelHeldSkills();
            g.survivor.equipment[0]=new Item(ItemKind.LastPrism);g.combat.PoseHeldPrism();Check(g.prism.BeginChannel(Vector2.right),"a snared player can still channel Last Prism beams");g.prism.Tick(.06f);Check(g.prism.Channeling,"web does not interrupt an active prism attack");g.prism.StopChannel();g.prism.Tick(1);
            g.checkMovement=null;g.body.linearVelocity=Vector2.zero;g.survivor.equipment[0]=null;c.snare=0;
            var web=c.AddWeb(g.body.position);Check(Mathf.Abs(web.radius-3*.85f)<.001f && Mathf.Abs(web.sprite.bounds.size.x-5.1f)<.001f && c.Snared && g.MovementSpeed==0,"floor web has triple radius and stops movement rather than attacks");
            ViewV34(web.point+Vector2.up*.45f,5);g.cameraView.Render();var on=ReadPixels();web.sprite.enabled=false;g.cameraView.Render();var off=ReadPixels();float contrast=RegionDifference(on,off,new Rect(web.point.x-2,web.point.y-2,4,4));Destroy(on);Destroy(off);web.sprite.enabled=true;
            Check(contrast>.015f,"large pale floor-web threads are clearly visible on dark stone ("+contrast.ToString("F4")+")");yield return Capture("v36-large-visible-web-no-torches.png");
            g.body.position=web.point+Vector2.right*2.65f;g.hero.transform.position=g.body.position;Check(!c.Snared && g.MovementSpeed>0,"movement is restored outside the circular web footprint");
            g.body.position=web.point;g.hero.transform.position=g.body.position;web.age=9.95f;c.Tick(.06f);Check(!c.webs.Contains(web) && !c.Snared,"floor web expires after ten seconds and releases the player");c.Leave();g.survivor.health=100;
        }
        IEnumerator HunterAggressionV36()
        {
            var g=game;HandClearV34();g.survivor.equipment[0]=null;g.seconds=225;g.survivor.health=100;g.journey.SetArea(WorldArea.GoldenField,new Vector2(-20,-8));var h=g.hunter;h.Tick(0);h.Warp(new Vector2(-16,16));
            var mage=g.journey.companion;mage.Hire(new Vector2(-8,16));mage.health=CompanionMage.MaxHealth;Physics2D.SyncTransforms();
            Check(!Physics2D.OverlapCircle(h.Position+Vector2.up*.1f,1.1f),"retaliation scenario starts on open ground clear of poplar trunks");
            float hp=h.health;g.arcane.DamageArea(h.Position,.4f,10,false);
            Check(h.health==hp-10 && h.Aggressor==mage.sprite && h.TargetRenderer==mage.sprite,"a real allied-mage explosion attributes its hit and provokes retaliation against the mage");
            var before=h.Position;Physics2D.SyncTransforms();h.Tick(.08f);Check(Vector2.Distance(h.Position,mage.Position)<Vector2.Distance(before,mage.Position),"axolotl actually moves toward its distant attacker rather than the player");
            g.body.position=h.Position+Vector2.down*3;g.hero.transform.position=g.body.position;h.Tick(0);
            Check(h.TargetRenderer==g.hero && h.Aggressor==mage.sprite,"player within three and a half units takes priority without erasing the original attacker");
            g.body.position=new Vector2(-20,-8);g.hero.transform.position=g.body.position;h.Tick(0);Check(h.TargetRenderer==mage.sprite,"axolotl returns to the remembered attacker when the player moves away");
            mage.Warp(h.Position+Vector2.right*1.05f);Physics2D.SyncTransforms();float mageHP=mage.health;h.Tick(.02f);
            Check(mageHP-mage.health==100,"contact against a non-player aggressor deals exactly one hundred damage");float wounded=mage.health;h.Tick(.02f);Check(mage.health==wounded,"axolotl contact damage has a one-second interval rather than applying every frame");
            mage.health=0;mage.sprite.enabled=false;h.Tick(0);Check(h.Aggressor==null && h.TargetRenderer==g.hero,"dead attacker is discarded and pursuit falls back to the player");
            g.dragons.Clear();var boss=g.dragons.Spawn(new Vector2(-7,13),g.journey.fieldRoot.transform);g.HurtTarget(h,10,boss.Renderer);h.Tick(0);
            Check(h.TargetRenderer==boss.Renderer,"axolotl also retaliates against a creature that damages it");
            g.arcane.DamageArea(h.Position,.5f,2,true);Check(h.TargetRenderer==g.hero && h.Aggressor==g.hero,"a subsequent player attack replaces the remembered aggressor");
            g.HurtTarget(h,1,boss.Renderer);g.survivor.health=150;g.body.position=h.Position+Vector2.down*1.1f;g.hero.transform.position=g.body.position;Physics2D.SyncTransforms();h.Tick(1.1f);
            Check(g.survivor.health==50 && h.TargetRenderer==g.hero,"close-player priority applies the new one-hundred-damage attack to the player");g.survivor.health=100;
            g.survivor.equipment[0]=new Item(ItemKind.Lamp);var range=Vector2.Distance(g.body.position,h.Position);h.Tick(.1f);
            Check(g.survivor.health==100 && h.HeldBack && Vector2.Distance(g.body.position,h.Position)>range,"personal lamp still repels the axolotl and prevents its contact attack");
            g.HurtTarget(h,1,boss.Renderer);g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));h.Tick(0);Check(h.Aggressor==null && h.TargetRenderer==null,"leaving the field clears retaliation against creatures from the previous location");
            g.dragons.Clear();g.survivor.equipment[0]=null;g.survivor.health=100;g.survivor.mana=100;g.seconds=40;g.checkCameraPosition=null;g.checkCameraSize=null;g.UpdateEnvironment(0);yield return null;
        }
    }
}
