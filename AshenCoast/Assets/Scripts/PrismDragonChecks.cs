using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        IEnumerator PrismDragonChecks()
        {
            var g=game;var s=g.survivor;var prism=g.prism;var ritual=g.dragons;
            g.magic.CancelStroke();if(g.magic.InPocket)g.magic.SwitchDimension();
            g.journey.SetArea(WorldArea.Coast,new Vector2(-3.55f,-1.7f));g.seconds=25;g.UpdateEnvironment(0);
            ritual.Clear();prism.StopChannel();s.health=100;s.mana=100;g.ui.CloseContainers();
            g.survivor.equipment[0]=new Item(ItemKind.LastPrism);g.facing=6;g.combat.manual=false;
            yield return null;
            Check(ItemDefinition.All[ItemKind.LastPrism].width==2 && ItemDefinition.All[ItemKind.LastPrism].height==2 && Survivor.Fits(s.equipment[0],GearSlot.Hand),"Last Prism uses a compatible 2x2 hand-weapon footprint");
            Check(g.art.icons[30].texture.name=="LastPrismV24c" && g.combat.weapon.enabled,"detailed prism reference sprite is actually held");
            g.art.UpdateOrb(0);var iconA=ReadPrismIcon();g.art.UpdateOrb(3.1f);var iconB=ReadPrismIcon();
            var pa=iconA.GetPixels();var pb=iconB.GetPixels();float colorChange=0;for(int i=0;i<pa.Length;i++)colorChange+=Mathf.Abs(pa[i].r-pb[i].r)+Mathf.Abs(pa[i].g-pb[i].g)+Mathf.Abs(pa[i].b-pb[i].b);
            Check(colorChange/pa.Length>.004f,"prism icon GPU material visibly changes its opalescent facets over time");Destroy(iconA);Destroy(iconB);
            g.art.UpdateOrb(0);g.cameraView.Render();var heldA=ReadPixels();g.art.UpdateOrb(3.1f);g.cameraView.Render();var heldB=ReadPixels();var heldBounds=g.combat.weapon.bounds;
            float heldChange=RegionDifference(heldA,heldB,new Rect(heldBounds.min.x,heldBounds.min.y,heldBounds.size.x,heldBounds.size.y));
            Check(heldChange>.003f,"held prism facets animate with the same opal palette as the icon ("+heldChange.ToString("F3")+")");Destroy(heldA);Destroy(heldB);
            for(int i=0;i<12;i++){g.art.UpdateOrb(i*.65f);var icon=ReadPrismIcon();File.WriteAllBytes(Path.Combine(directory,"v24-prism-opal-phase-"+i.ToString("D2")+".png"),icon.EncodeToPNG());Destroy(icon);}
            yield return PrismFocusChecks();
            s.inventory.items.Clear();s.inventory.Add(new Item(ItemKind.LastPrism));g.ui.open=true;yield return Capture("v24-prism-inventory.png");g.ui.open=false;
            g.checkCameraSize=7;g.checkCameraPosition=new Vector3(-.5f,.6f,-20);g.dummyHealth=500;
            var aim=(Vector2)g.dummy.bounds.center-prism.Origin;
            Check(prism.BeginChannel(aim) && prism.Spread>59.9f,"initial Last Prism fan starts at sixty degrees");
            float mana=s.mana;prism.Tick(.35f);Check(s.mana<mana && prism.Spread>50 && prism.rays.Take(6).All(r=>r.enabled),"thin separated rays consume mana during initial focus");
            yield return Capture("v24-prism-six-rays.png");
            prism.Tick(1.45f);Check(prism.Charge>.999f && prism.Spread<.01f && prism.rays[6].enabled && prism.rays[6].widthMultiplier>.45f,"six rays converge into a thick charged beam");
            // Let the actual held tip finish turning, then measure sustained damage
            // against a fresh target rather than health already lost during focus.
            g.combat.manual=true;prism.SetAim((Vector2)g.dummy.bounds.center-prism.Origin);prism.Tick(.70f);g.dummyHealth=500;s.mana=100;
            float hp=g.dummyHealth,atMana=s.mana;prism.Tick(.42f);
            Check(hp-g.dummyHealth>=140 && atMana-s.mana>11,"charged beam deals large real contact damage and drains mana rapidly (damage "+(hp-g.dummyHealth)+", mana "+(atMana-s.mana).ToString("F2")+")");
            float diagonal=g.cameraView.orthographicSize*2*Mathf.Sqrt(1+g.cameraView.aspect*g.cameraView.aspect);
            Check(prism.Range>=diagonal*2-.01f,"ray range covers at least two full viewport diagonals");
            yield return Capture("v24-prism-focused-beam.png");
            float angle=prism.Angle;prism.SetAim(Vector2.up);prism.Tick(.05f);Check(Mathf.Abs(Mathf.DeltaAngle(angle,prism.Angle))<15 && Mathf.Abs(Mathf.DeltaAngle(prism.Angle,90))>30,"beam angle follows a sudden cursor turn with delay");
            prism.Tick(.7f);Check(Mathf.Abs(Mathf.DeltaAngle(prism.Angle,90))<8,"delayed beam catches up to held cursor direction");
            s.mana=.20f;prism.Tick(.3f);Check(!prism.Channeling && s.mana==0,"beam stops at zero mana without negative mana");
            Check(!prism.BeginChannel(Vector2.right),"empty mana cannot start a prism beam");
            s.mana=100;Check(prism.BeginChannel(Vector2.right),"replenished mana can start a fresh beam");s.equipment[0]=new Item(ItemKind.Orb);prism.Tick(.01f);Check(!prism.Channeling,"changing the held item cancels the prism channel");prism.Tick(.20f);
            g.combat.manual=false;s.equipment[0]=new Item(ItemKind.LastPrism);s.inventory.items.Clear();s.inventory.Add(new Item(ItemKind.AbyssTome){opened=true});s.inventory.Add(new Item(ItemKind.Tome){opened=true});
            g.magic.CancelStroke();yield return new WaitForSecondsRealtime(1.25f);
            var centre=new Vector2(Screen.width*.45f,Screen.height*.45f);
            var wrong=SpatialSigil.Resample(SpatialSigil.Template,100).Select(p=>p*180+centre).ToList();
            Check(g.magic.BeginStroke(wrong[0]) && g.magic.SilverInk,"prism begins drawing silver spell ink");foreach(var p in wrong.Skip(1))g.magic.AddPoint(p);
            yield return Capture("v24-prism-silver-stroke.png");int fields=g.arcane.fields.Count;float balance=s.mana;
            Check(!g.magic.EndStroke() && g.arcane.fields.Count==fields && s.mana==balance,"prism cannot read the tentacle spell even with the abyss book open");
            g.magic.CancelStroke();var star=SpatialSigil.Resample(StarSigil.Template,130).Select(p=>p*210+centre).ToList();
            Check(g.magic.BeginStroke(star[0]),"prism starts a pocket-dimension star");foreach(var p in star.Skip(1))g.magic.AddPoint(p);
            Check(g.magic.EndStroke() && s.mana==balance-20,"prism accepts pocket spell and charges exactly twenty mana");
            yield return new WaitForSecondsRealtime(.54f);Check(g.magic.InPocket,"accepted prism star actually teleports into pocket world");yield return new WaitForSecondsRealtime(.30f);g.magic.ExitThroughDoor();yield return new WaitForSecondsRealtime(.28f);
            g.checkCameraSize=8;g.checkCameraPosition=new Vector3(4,3,-20);
            s.health=100;s.mana=100;s.equipment[0]=new Item(ItemKind.Lamp);s.inventory.items.Clear();s.inventory.Add(new Item(ItemKind.Pyromancy){opened=true});
            g.body.position=new Vector2(3,0);g.hero.transform.position=g.body.position;
            var fire=g.arcane.CreateCampfire(new Vector2(5,0));
            Check(g.arcane.TryExplodeFire(fire.position) && ritual.summons.Count==1,"real campfire detonation schedules a dragon ritual");
            g.arcane.Tick(1.8f);
            ritual.Tick(1.99f);Check(!ritual.summons[0].pillar && ritual.bosses.Count==0,"no pillar or boss appears before two seconds");
            ritual.Tick(.011f);Check(ritual.summons[0].pillar && ritual.summons[0].pillar.bounds.size.x>4.3f && ritual.bosses.Count==0,"wide full-height fire column erupts after two seconds");
            Check(ritual.summons[0].pillar.bounds.max.y>g.cameraView.transform.position.y+g.cameraView.orthographicSize+2,"summoning fire reaches beyond the sky edge of the viewport");
            Check(ritual.scorches.Count>35,"sky-fire impact burns a broad connected area instead of one cell");
            g.UpdateEnvironment(0);yield return Capture("v24-wide-invocation-column.png");
            var summon=ritual.summons[0];
            for(int i=0;i<24;i++){summon.age=2+i/26.4f;ritual.Tick(0);g.cameraView.Render();var picture=ReadPixels();File.WriteAllBytes(Path.Combine(directory,"v24-column-frame-"+i.ToString("D2")+".png"),picture.EncodeToPNG());Destroy(picture);}
            summon.age=4.99f;ritual.Tick(0);Check(ritual.bosses.Count==0 && summon.pillar,"column stays present for the full three-second interval");
            ritual.Tick(.011f);Check(ritual.bosses.Count==1 && ritual.summons.Count==0 && ritual.bosses[0].health==1500,"pillar ends at five seconds and creates a 1500 HP hostile boss");
            var boss=ritual.bosses[0];boss.Tick(.38f);g.UpdateEnvironment(0);yield return Capture("v24-rooster-dragon-arrival.png");
            s.health=100;boss.BeginAttack(DragonAction.Peck,Vector2.left);boss.Tick(.44f);float before=s.health;
            Check(before==100,"beak windup does not deal premature damage");
            boss.Tick(.04f);g.body.position=boss.Mouth-Vector2.up*.8f;g.hero.transform.position=g.body.position;
            // Restart at the true beak contact position to test the rendered anchor.
            s.health=100;boss.BeginAttack(DragonAction.Peck,Vector2.left);boss.Tick(.44f);Check(s.health==100,"second peck preserves HP until contact phase");boss.Tick(.04f);
            Check(s.health==68,"beak contact phase applies one fixed thirty-two damage hit");boss.Tick(.1f);Check(s.health==68,"one peck cannot deal repeated per-frame contact damage");yield return Capture("v24-dragon-peck-contact.png");
            s.health=100;g.body.position=boss.Position+Vector2.left*1.1f;g.hero.transform.position=g.body.position;boss.BeginAttack(DragonAction.Claw,Vector2.left);boss.Tick(.53f);
            Check(s.health==100,"claw windup preserves player HP");boss.Tick(.04f);Check(s.health==56,"claw contact applies fixed forty-four damage");yield return Capture("v24-dragon-claw-contact.png");
            g.body.position=new Vector2(-10,0);g.hero.transform.position=g.body.position;s.health=100;int flames=g.arcane.flames.Count(f=>f.hostile);
            boss.BeginAttack(DragonAction.Breath,Vector2.left);boss.Tick(.3f);Check(g.arcane.flames.Count(f=>f.hostile)==flames,"breath has a visible preparation before emitting fire");boss.Tick(.28f);
            Check(g.arcane.flames.Count(f=>f.hostile)>flames && g.arcane.flames.Any(f=>f.hostile && f.reach==13),"boss mouth emits a long-range animated hostile fire stream");
            g.arcane.Tick(.28f);Check(ritual.scorches.Count>3,"ground reached by traveling breath receives textured burnt patches");g.UpdateEnvironment(0);yield return Capture("v24-dragon-long-fire-breath.png");
            ritual.Clear();foreach(var f in g.arcane.flames)Destroy(f.renderer.gameObject);g.arcane.flames.Clear();
            g.body.position=new Vector2(90,9);g.hero.transform.position=g.body.position;s.health=100;
            boss=ritual.Spawn(new Vector2(10,9),g.coastRoot.transform);boss.Tick(19.98f);Check(boss.Flights==0,"dragon stays on ground before twenty-second flight interval");boss.Tick(.04f);
            Check(boss.Action==DragonAction.Flight && boss.Flights==1,"dragon takes off at twenty seconds");boss.Tick(1.65f);
            Check(boss.Altitude>3 && ritual.falling.Count>0,"airborne dragon flies high and fires downward from its mouth");
            var lastFire=ritual.falling.Last();Check(lastFire.life<.25f && Vector2.Dot(lastFire.ground-boss.Position,boss.FlightDirection)>3,"airborne flame inherits flight lead and lands in front of the dragon");
            ritual.Tick(.45f);Check(ritual.GroundImpacts>0 && ritual.scorches.Count>0,"falling dragon fire reaches ground and burns its flight path");
            g.checkCameraPosition=(Vector3)boss.Position+new Vector3(0,3,-20);g.checkCameraSize=6.5f;g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);yield return Capture("v24-dragon-air-raid.png");
            for(int i=0;i<16;i++){boss.BeginAttack(DragonAction.Flight,Vector2.left);boss.Tick(.90f+i*.05f);g.checkCameraPosition=(Vector3)boss.Position+new Vector3(0,3,-20);g.ResetCamera(g.checkCameraPosition.Value);yield return Capture("v24-wingbeat-frame-"+i.ToString("D2")+".png");}
            ritual.Clear();boss=ritual.Spawn(new Vector2(10,9),g.coastRoot.transform);boss.Tick(40.04f);Check(boss.Flights==2,"takeoffs repeat on twenty-second intervals");
            ritual.Clear();ritual.Burn(new Vector2(12,3),g.coastRoot.transform);ritual.Tick(9.99f);Check(ritual.scorches.Count==1,"scorched ground lasts through 9.99 seconds");ritual.Tick(.02f);Check(ritual.scorches.Count==0,"burnt texture disappears after ten seconds");
            ritual.Clear();s.health=100;s.mana=100;s.equipment[0]=new Item(ItemKind.LastPrism);g.body.position=new Vector2(1,2);g.hero.transform.position=g.body.position;boss=ritual.Spawn(new Vector2(7,2),g.coastRoot.transform);boss.Tick(.4f);
            yield return null;Check(g.AdditionalTargets.Any(t=>t==boss),"boss joins the shared melee fire yo-yo and explosion target system");
            g.checkCameraSize=8;g.checkCameraPosition=new Vector3(4,4,-20);g.ResetCamera(g.checkCameraPosition.Value);prism.BeginChannel((Vector2)boss.Renderer.bounds.center-prism.Origin);prism.Tick(1.8f);hp=boss.health;prism.Tick(.6f);
            Check(hp-boss.health>220,"focused prism beam actually damages the rooster-dragon's opaque sprite");yield return Capture("v24-prism-versus-dragon.png");
            prism.StopChannel();prism.Tick(.2f);float taken=boss.Hurt(9999);Check(taken>0 && !boss.Alive && boss.health==0,"boss can die and clamps health to zero");ritual.Tick(1.6f);Check(ritual.bosses.Count==0,"boss death animation finishes and removes its renderers");
            ritual.Clear();ritual.Burn(new Vector2(12,3),g.coastRoot.transform);var patch=ritual.scorches[0];var corners=patch.renderer.sprite.uv;
            Check(corners.Min(v=>v.x)==0 && corners.Max(v=>v.x)==1 && corners.Min(v=>v.y)==0 && corners.Max(v=>v.y)==1,"burn patch uses full normalized UVs for a soft ragged circular footprint");
            int burns=ritual.scorches.Count;ritual.Burn(new Vector2(0,-18),g.coastRoot.transform);Check(ritual.scorches.Count==burns,"dragon fire never creates scorched soil on sea water");
            g.checkCameraPosition=new Vector3(12,4,-20);g.checkCameraSize=3.5f;g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);yield return Capture("v24-scorched-ground-detail.png");
            yield return PrismSkyChecks();
            ritual.Clear();prism.StopChannel();g.combat.manual=true;s.health=100;s.mana=100;g.checkCameraPosition=null;g.checkCameraSize=null;g.ui.open=false;
        }
        Texture2D ReadPrismIcon()
        {
            var old=RenderTexture.active;RenderTexture.active=ArtBank.PrismIcon;var image=new Texture2D(64,64,TextureFormat.RGBA32,false);
            image.ReadPixels(new Rect(0,0,64,64),0,0);image.Apply();RenderTexture.active=old;return image;
        }
        IEnumerator PrismFocusChecks()
        {
            var g=game;var prism=g.prism;var item=g.survivor.equipment[0];var weapon=g.combat.weapon;
            prism.StopChannel();prism.Tick(.20f);g.combat.PoseHeldPrism();
            Check(Mathf.Abs(weapon.bounds.size.x-.322f)<.002f && item.Definition.width==2 && item.Definition.height==2,"prism artwork is thirty percent smaller while its 2x2 inventory footprint is preserved");
            Check(Vector2.Dot(weapon.transform.up,Vector2.up)>.999f,"prism remains upright when not firing");
            var at=g.body.position;bool moving=g.walking;g.checkCameraPosition=new Vector3(at.x,at.y+1,-20);g.checkCameraSize=3;g.ResetCamera(g.checkCameraPosition.Value);
            for(int i=0;i<8;i++)
            {
                prism.StopChannel();prism.Tick(.2f);g.survivor.mana=100;
                float a=i*45*Mathf.Deg2Rad;var direction=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                prism.BeginChannel(direction);prism.Tick(.15f);yield return null;yield return new WaitForEndOfFrame();
                var axis=(Vector2)weapon.transform.up;var actual=new Vector2(Mathf.Cos(prism.Angle*Mathf.Deg2Rad),Mathf.Sin(prism.Angle*Mathf.Deg2Rad));
                var tip=(Vector2)weapon.transform.TransformPoint(new Vector3(0,weapon.sprite.bounds.max.y,0));
                float axisAgreement=Vector2.Dot(axis,actual),tipGap=Vector2.Distance(prism.rays[0].GetPosition(0),tip);
                Check(axisAgreement>.9999f && tipGap<.001f,"held prism axis and ray origin agree in firing direction "+i+" (dot "+axisAgreement.ToString("F4")+", gap "+tipGap.ToString("F4")+")");
                yield return Capture("v25-prism-aim-"+i+".png");
            }
            g.walking=true;g.body.position=at+new Vector2(.6f,.3f);g.hero.transform.position=g.body.position;yield return null;yield return new WaitForEndOfFrame();
            var movedTip=(Vector2)weapon.transform.TransformPoint(new Vector3(0,weapon.sprite.bounds.max.y,0));
            Check(Vector2.Distance(prism.rays[0].GetPosition(0),movedTip)<.001f,"prism rays remain attached to the final animated hand while moving");
            g.walking=moving;g.body.position=at;g.hero.transform.position=at;prism.StopChannel();prism.Tick(.2f);g.combat.PoseHeldPrism();
            Check(Vector2.Dot(weapon.transform.up,Vector2.up)>.999f,"releasing fire restores the original upright prism pose");
            g.survivor.mana=100;prism.BeginChannel(Vector2.right);
            var oldOffsets=Enumerable.Range(0,6).Select(prism.RayOffset).ToArray();float irregular=0,asymmetric=0,minSpeed=10,maxSpeed=0;bool reverses=false;var velocities=new float[6];
            for(int frame=0;frame<36;frame++)
            {
                prism.Tick(.04f);var current=Enumerable.Range(0,6).Select(prism.RayOffset).ToArray();
                for(int ray=0;ray<6;ray++)
                {
                    float velocity=(current[ray]-oldOffsets[ray])/.04f;
                    if(frame>1 && velocities[ray]*velocity<0 && Mathf.Abs(velocity)>1)reverses=true;
                    velocities[ray]=velocity;float speed=prism.RayFlowSpeed(ray);minSpeed=Mathf.Min(minSpeed,speed);maxSpeed=Mathf.Max(maxSpeed,speed);
                    irregular=Mathf.Max(irregular,Mathf.Abs(velocity-velocities[0]));
                }
                asymmetric=Mathf.Max(asymmetric,Mathf.Abs(current[0]+current[5]));oldOffsets=current;
                if(frame%6==0)yield return Capture("v25-focus-phase-"+(frame/6).ToString("D2")+".png");
            }
            Check(asymmetric>2 && irregular>10 && reverses,"converging rays have unequal curved timings and visibly change drift direction");
            Check(maxSpeed-minSpeed>.35f,"individual light streams change speed independently during focus");
            prism.Tick(.36f);Check(Enumerable.Range(0,6).All(i=>Mathf.Abs(prism.RayOffset(i))<.001f) && prism.rays[6].enabled,"chaotic thin rays still finish as one stable powerful beam");
            prism.StopChannel();prism.Tick(.2f);g.survivor.mana=100;g.checkCameraPosition=null;g.checkCameraSize=null;g.combat.manual=false;
        }
        IEnumerator PrismSkyChecks()
        {
            var g=game;var prism=g.prism;var ritual=g.dragons;var s=g.survivor;
            ritual.Clear();g.magic.CancelStroke();prism.StopChannel();prism.Tick(.2f);g.ui.CloseContainers();
            g.journey.SetArea(WorldArea.Coast,new Vector2(4,3));s.health=100;s.mana=100;s.equipment[0]=new Item(ItemKind.LastPrism);g.combat.manual=false;
            g.checkCameraSize=7;g.checkCameraPosition=new Vector3(11,3,-20);g.ResetCamera(g.checkCameraPosition.Value);g.seconds=25;yield return null;
            var oldDummy=g.dummy.transform.position;g.dummy.transform.position=new Vector3(11,3,0);g.dummyHealth=500;
            Check(prism.BeginChannel(Vector2.right),"sky/prism presentation scenario starts with a valid channel");prism.Tick(.4f);yield return new WaitForEndOfFrame();
            Check(Shader.GetGlobalInt("_PrismSegmentCount")==6,"each of the six separate rays publishes its own continuous light segment");
            var segments=Shader.GetGlobalVectorArray("_PrismSegments");var styles=Shader.GetGlobalVectorArray("_PrismBeamStyle");
            Check(Enumerable.Range(0,6).All(i=>styles[i].x>1 && styles[i].y>.2f && Vector2.Distance(new Vector2(segments[i].z,segments[i].w),prism.rays[i].GetPosition(3))<.01f),"ray lighting follows actual endpoints and collision clipping");
            g.UpdateEnvironment(0);Shader.SetGlobalColor("_CoastAmbient",new Color(.13f,.16f,.22f));Shader.SetGlobalInt("_CoastLightCount",0);
            g.cameraView.Render();var light=ReadPixels();Shader.SetGlobalInt("_PrismSegmentCount",0);g.cameraView.Render();var dark=ReadPixels();int visiblyLit=0;
            for(int i=0;i<6;i++)
            {
                var a=(Vector2)prism.rays[i].GetPosition(0);var b=(Vector2)prism.rays[i].GetPosition(3);var d=(b-a).normalized;var normal=new Vector2(-d.y,d.x);var p=a+d*Mathf.Min(8,(b-a).magnitude*.65f)+normal*.32f;
                if(RegionDifference(light,dark,new Rect(p.x-.22f,p.y-.22f,.44f,.44f))>.006f)visiblyLit++;
            }
            Check(visiblyLit==6,"GPU-rendered ground visibly brightens beside every thin prism ray ("+visiblyLit+")");Destroy(light);Destroy(dark);prism.Tick(0);yield return Capture("v26-six-ray-lighting.png");
            prism.Tick(1.4f);yield return new WaitForEndOfFrame();var fused=prism.rays[6];
            Check(fused.positionCount==4 && fused.widthCurve.Evaluate(0)<.02f && fused.widthCurve.Evaluate(.5f)>.8f,"focused beam grows from a fine tip into its full width rather than a rectangle");
            Check(Vector2.Distance(fused.GetPosition(0),g.combat.weapon.transform.TransformPoint(new Vector3(0,g.combat.weapon.sprite.bounds.max.y,0)))<.001f,"focused beam starts at the actual painted prism apex");
            Check(Shader.GetGlobalInt("_PrismSegmentCount")==1 && Shader.GetGlobalVectorArray("_PrismBeamStyle")[0].x>2,"focused beam has continuous wide local lighting along its whole length");
            g.cameraView.Render();var phaseA=ReadPixels();prism.Tick(.13f);g.cameraView.Render();var phaseB=ReadPixels();
            var centre=(Vector2)fused.GetPosition(0)+Vector2.right*5;
            Check(RegionDifference(phaseA,phaseB,new Rect(centre.x-1.7f,centre.y-.22f,3.4f,.44f))>.004f,"focused beam GPU filaments and travelling packets animate visibly");Destroy(phaseA);Destroy(phaseB);yield return Capture("v26-focused-filament-beam.png");
            var voice=g.feedback.LastVoice;Check(voice.clip==prism.Audio.Impact && prism.Audio.Source.isPlaying,"real prism contact uses the unique energy impact while the laser loop plays");
            var samples=new float[prism.Audio.LaserLoop.samples*prism.Audio.LaserLoop.channels];prism.Audio.LaserLoop.GetData(samples,0);
            Check(prism.Audio.LaserLoop.channels==2 && samples.All(v=>!float.IsNaN(v) && Mathf.Abs(v)<=1) && samples.Sum(v=>Mathf.Abs(v))/samples.Length>.04f,"laser loop contains a finite audible stereo waveform");
            prism.StopChannel();prism.Tick(.4f);Check(!prism.Audio.Source.isPlaying && Shader.GetGlobalInt("_PrismSegmentCount")==0,"releasing fire stops laser audio and beam lighting after fading");
            g.dummy.transform.position=oldDummy;
            ritual.Clear();ritual.BurnCone(new Vector2(12,10),Vector2.right,13,g.coastRoot.transform);
            Check(ritual.scorches.Count>85 && ritual.scorches.Max(p=>p.cell.y)-ritual.scorches.Min(p=>p.cell.y)>6,"dragon breath scorches a wide cone over many ground cells");
            g.checkCameraPosition=new Vector3(18,10,-20);g.checkCameraSize=7;g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);yield return Capture("v26-mass-scorched-cone.png");
            ritual.Clear();var summon=ritual.Schedule(new Vector2(15,8),g.coastRoot.transform);ritual.Tick(2.1f);g.UpdateEnvironment(0);yield return Capture("v26-fire-from-sky.png");
            for(int i=0;i<6;i++){ritual.Tick(.1f);yield return Capture("v26-sky-fire-phase-"+i+".png");}
            ritual.Clear();var dragon=ritual.Spawn(new Vector2(12,12),g.coastRoot.transform);g.body.position=new Vector2(27,16);g.hero.transform.position=g.body.position;dragon.BeginAttack(DragonAction.Flight,Vector2.right);
            for(int i=0;i<95;i++){ritual.Tick(.02f);g.arcane.Tick(.02f);}
            g.checkCameraPosition=(Vector3)dragon.Position+new Vector3(3,3,-20);g.checkCameraSize=6.5f;g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);yield return Capture("v26-leading-air-fire.png");
            ritual.Clear();s.health=100;s.mana=100;s.equipment[0]=new Item(ItemKind.LastPrism);g.magic.CancelStroke();yield return new WaitForSecondsRealtime(1.25f);
            var line=SpatialSigil.Resample(StarSigil.Template,120).Select(p=>p*210+new Vector2(Screen.width*.4f,Screen.height*.4f)).ToArray();
            Check(g.magic.BeginStroke(line[0]) && g.magic.SilverInk,"prism begins its distinct silver crystal drawing effect");foreach(var p in line.Skip(1))g.magic.AddPoint(p);g.magic.TickSparks(.18f);yield return Capture("v26-prism-crystal-drawing.png");
            g.magic.CancelStroke();prism.StopChannel();prism.Tick(.3f);ritual.Clear();s.health=100;s.mana=100;g.checkCameraPosition=null;g.checkCameraSize=null;
        }
    }
}
