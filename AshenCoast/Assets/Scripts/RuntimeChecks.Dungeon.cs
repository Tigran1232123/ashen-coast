using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        IEnumerator DungeonFeedbackChecks()
        {
            var f=game.fort;var h=game.hunter;var ui=game.ui;
            game.checkMovement=null;game.movement=Vector2.zero;game.body.linearVelocity=Vector2.zero;
            ui.CloseContainers();game.survivor.health=100;game.survivor.equipment[0]=null;game.seconds=30;
            game.journey.SetArea(WorldArea.Coast,f.Position+Vector2.down*2);game.meadow.Refresh(game.body.position,true);
            game.checkCameraSize=6;game.checkCameraPosition=new Vector3(f.Position.x,f.Position.y+2,-20);game.ResetCamera(game.checkCameraPosition.Value);game.UpdateEnvironment(0);yield return null;
            Check(f.descent.sprite.texture.name=="FortDescentV23" && f.descent.bounds.size.x>3,"fort has a detailed sunken stone stairwell covering the empty grass entrance");
            Check(f.CanInteractAt(f.descent.bounds.center) && !f.CanInteractAt(f.Position+new Vector2(6,5)),"only the actual stairway accepts entry clicks; tower facade is not an invisible doorway");
            game.HighlightAt(null);game.cameraView.Render();var plain=ReadPixels();
            game.HighlightAt(f.descent.bounds.center);game.cameraView.Render();var hover=ReadPixels();
            var bounds=f.descent.bounds;float glow=RegionDifference(plain,hover,new Rect(bounds.min.x,bounds.min.y,bounds.size.x,bounds.size.y));
            Check(glow>.005f && game.HoverName.Contains("Спуститься"),"actual stone descent glows and names its action on hover ("+glow.ToString("F4")+")");Destroy(plain);Destroy(hover);
            yield return Capture("v23-fort-descent-highlight.png");
            Check(game.UseWorldRightClick(f.descent.bounds.center) && f.Inside && game.interactions.Strength(f.exit)>.9f,"clicking descent enters dungeon and highlights arrival passage");game.HighlightAt(null);
            var saved=f.bandits.Select(b=>b.Position).ToArray();
            foreach(var b in f.bandits){b.ClearAggro();b.feet.enabled=false;}
            Physics2D.SyncTransforms();
            var bases=f.root.GetComponentsInChildren<SpriteRenderer>().Where(s=>s.name.Contains("tower") || s.name.Contains("north stone") || s.name.Contains("partition south")).ToArray();
            foreach(var masonry in bases)
            {
                var fitted=masonry.GetComponentsInChildren<BoxCollider2D>();
                Check(fitted.Length>0 && fitted.All(c=>f.stoneColliders.Contains(c)),"visible masonry has registered opaque-ground collision: "+masonry.name+" at "+masonry.transform.position);
            }
            var tested=new HashSet<string>();
            foreach(var masonry in bases)
            {
                if(!tested.Add(masonry.name))continue;
                var obstacle=masonry.GetComponentsInChildren<BoxCollider2D>().OrderByDescending(c=>c.bounds.size.x).First();var bb=obstacle.bounds;
                var start=new Vector2(bb.center.x,bb.min.y-.13f-.22f-.45f);
                game.body.position=start;game.hero.transform.position=start;Physics2D.SyncTransforms();game.checkMovement=Vector2.up;
                for(int tick=0;tick<22;tick++)yield return new WaitForFixedUpdate();
                Check(game.body.position.y+.13f+.22f<=bb.min.y+.09f,"real hero feet stop below painted foundation: "+masonry.name);
                game.ToggleFastMovement();for(int tick=0;tick<20;tick++)yield return new WaitForFixedUpdate();game.ToggleFastMovement();
                Check(game.body.position.y+.13f+.22f<=bb.min.y+.09f,"five-times-speed body cannot tunnel through foundation: "+masonry.name);
                game.checkMovement=null;game.movement=Vector2.zero;game.body.linearVelocity=Vector2.zero;
            }
            Vector2[] openings={new Vector2(0,-13),new Vector2(-9.4f,-2.1f),new Vector2(9.4f,-2.1f)};
            foreach(var opening in openings)
            {
                var from=opening+Vector2.left*2.2f;var to=opening+Vector2.right*2.2f;
                Check(!f.Blocked(from,to,.23f),"hero-width opening is traversable at "+opening);
                game.body.position=from;game.hero.transform.position=from;Physics2D.SyncTransforms();game.checkMovement=Vector2.right;
                for(int tick=0;tick<55;tick++)yield return new WaitForFixedUpdate();
                Check(game.body.position.x>opening.x+.9f,"actual rigidbody walks through doorway without hidden hitbox at "+opening);
                game.checkMovement=null;game.movement=Vector2.zero;game.body.linearVelocity=Vector2.zero;
            }
            Check(f.Blocked(new Vector2(-10.1f,-8),new Vector2(-8.4f,-8),.05f),"line of attack is blocked by the stone partition");
            foreach(var b in f.bandits)b.health=100;
            var target=f.bandits[0];target.Warp(new Vector2(-8.4f,-8));target.sprite.enabled=true;
            game.arcane.DamageArea(new Vector2(-10.1f,-8),3,60);Check(target.health==100,"player blast cannot damage a bandit through dungeon masonry");
            game.body.position=new Vector2(-8.4f,-8);game.hero.transform.position=game.body.position;game.survivor.health=100;
            game.arcane.CreateExplosion(new Vector2(-10.1f,-8),true);game.arcane.Tick(.30f);Check(game.survivor.health==100,"enemy explosion cannot damage player through dungeon masonry");game.arcane.Tick(2);
            var arrowHealth=f.bandits.Select(b=>b.health).ToArray();foreach(var b in f.bandits)b.health=0;f.ClearProjectiles();
            f.Shoot(new Vector2(-11,-7.4f),Vector2.right);for(int step=0;step<30;step++)f.Tick(.02f);
            Check(game.survivor.health==100 && f.arrows.Count==0,"swept arrow stops at the wall instead of hitting the hero behind it");for(int i=0;i<f.bandits.Count;i++)f.bandits[i].health=arrowHealth[i];
            game.body.position=new Vector2(-11,-8);game.hero.transform.position=game.body.position;game.survivor.equipment[0]=new Item(ItemKind.Spear);game.combat.ResetForCheck();game.combat.Begin(Vector2.right);game.combat.Advance(.85f);
            Check(target.health==100,"textured spear cannot pass through wall into enemy hitbox");game.combat.ResetForCheck();game.survivor.equipment[0]=null;
            for(int role=0;role<3;role++)for(int pose=0;pose<8;pose++)
            {
                var walk=f.art.bandits[role,pose];var attack=f.art.attacks[role,pose];
                Check(walk.vertices.Length>4 && attack.vertices.Length>4 && game.art.HitMask(walk).Count(solid=>solid)>9000 && game.art.HitMask(attack).Count(solid=>solid)>9000,"complete walk/attack silhouettes retain weapons and toes without adjacent-frame ghosts for role "+role+" phase "+pose);
                Check(walk.texture.name=="BanditActionsV23" && attack.texture==walk.texture && Mathf.Abs(walk.pixelsPerUnit-attack.pixelsPerUnit)<.01f,"role "+role+" walk/attack "+pose+" share game-style atlas and fixed scale");
            }
            for(int i=0;i<f.bandits.Count;i++){var b=f.bandits[i];b.health=100;b.sprite.enabled=true;b.feet.enabled=true;b.ClearAggro();b.Warp(saved[i]);}
            game.body.position=new Vector2(1.5f,4);game.hero.transform.position=game.body.position;game.checkCameraSize=8;game.checkCameraPosition=new Vector3(1.5f,6.2f,-20);game.ResetCamera(game.checkCameraPosition.Value);game.UpdateEnvironment(0);
            yield return Capture("v23-fort-enemies-and-masonry.png");
            target.Warp(new Vector2(1.5f,3));game.survivor.equipment[0]=new Item(ItemKind.Sword);game.combat.ResetForCheck();game.combat.Begin(Vector2.down);game.combat.Advance(.8f);
            Check(target.health<100,"actual sword contacts the newly registered visible bandit silhouette in the dungeon");target.health=100;target.ClearAggro();game.combat.ResetForCheck();game.survivor.equipment[0]=null;
            int sounds=game.feedback.MobHits;target.Warp(game.body.position+Vector2.right*2);float hp=target.health;target.Hurt(1);
            Check(target.health==hp-1 && game.feedback.Flash(target.sprite)>.95f && game.feedback.MobHits==sounds+1 && game.feedback.LastVoice.isPlaying,"bandit damage flashes HP and plays actual creature impact source");
            yield return Capture("v23-bandit-hit-flash.png");
            game.survivor.health=100;yield return new WaitForSecondsRealtime(.7f);yield return new WaitForEndOfFrame();var quiet=ScreenCapture.CaptureScreenshotAsTexture();
            int hits=game.feedback.PlayerHits;game.HurtPlayer(1);yield return null;yield return new WaitForEndOfFrame();var struck=ScreenCapture.CaptureScreenshotAsTexture();
            float difference=0;int count=0;float scale=ui.UIScale;
            for(int y=(int)(59*scale);y<(int)(70*scale);y++)for(int x=(int)(62*scale);x<(int)(260*scale);x++){var a=quiet.GetPixel(x,quiet.height-1-y);var b=struck.GetPixel(x,struck.height-1-y);difference+=Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b);count++;}
            Check(game.feedback.PlayerHits==hits+1 && game.feedback.LastVoice.isPlaying && difference/Mathf.Max(1,count)>.1f,"player hit has distinct audio and visible health-track flash in actual screen pixels");Destroy(quiet);Destroy(struck);
            yield return Capture("v23-player-hit-flash.png");game.survivor.health=100;
            foreach(var clip in new[]{game.feedback.PlayerImpact,game.feedback.MobImpact,game.feedback.ShellImpact})
            {var data=new float[clip.samples];clip.GetData(data,0);Check(data.Max(x=>Mathf.Abs(x))>.1f && data.Any(x=>x<-.05f) && clip.length>.15f,"non-silent, signed impact waveform: "+clip.name);}
            Check(game.feedback.PlayerImpact!=game.feedback.MobImpact,"player and enemy use separate impact sounds");
            target.Hurt(1000);Check(!target.Alive && game.feedback.Recent(target.sprite) && game.feedback.Flash(target.sprite)>.95f,"lethal hit retains a briefly flashing zero-HP bar");yield return Capture("v23-lethal-hit-flash.png");
            yield return new WaitForSecondsRealtime(.7f);Check(game.feedback.Flash(target.sprite)==0 && !game.feedback.Recent(target.sprite),"damage highlighting fades fully and does not remain stuck");
            target.health=100;target.sprite.enabled=target.feet.enabled=true;target.ClearAggro();target.Warp(saved[0]);
            var companion=game.journey.companion;companion.Hire(game.body.position+Vector2.left);companion.health=CompanionMage.MaxHealth;companion.Hurt(1);
            Check(game.feedback.Flash(companion.sprite)>.95f,"companion mage damage also flashes health and plays hit sound");companion.health=CompanionMage.MaxHealth;
            game.journey.SetArea(WorldArea.Coast,new Vector2(-1.5f,-1.65f));var creature=game.woodlice[0];creature.vitals.health=creature.vitals.maximumHealth;creature.renderer.enabled=creature.feet.enabled=true;creature.Hurt(1);
            Check(game.feedback.Flash(creature.renderer)>.95f,"woodlouse damage uses shared HP flash and impact audio");creature.vitals.health=creature.vitals.maximumHealth;
            var frog=game.arcane.frogs[0];frog.Reset();frog.Hurt(1);Check(game.feedback.Flash(frog.renderer)>.95f,"frog mage damage uses shared HP flash and impact audio");frog.Reset();
            game.DamageDummy(1);Check(game.feedback.Flash(game.dummy)>.95f,"training dummy health also flashes on hit");
            game.journey.SetArea(WorldArea.Tavern,new Vector2(-.7f,1.2f));Physics2D.SyncTransforms();
            var keeper=game.journey.keeper.sprite;game.HighlightAt(keeper.bounds.center);Check(game.interactions.Hovered==keeper && game.HoverName.Contains("Торговля"),"innkeeper shares world interaction highlight");
            game.UseWorldRightClick(keeper.bounds.center);Check(ui.vendorOpen && game.interactions.Strength(keeper)>.9f,"successful trade click pulses selected innkeeper");ui.CloseContainers();game.HighlightAt(null);
            game.journey.SetArea(WorldArea.Coast,new Vector2(-8,-2));Vector2 shipPoint=game.ship.bounds.center;bool found=false;
            for(int y=4;y<19 && !found;y++)for(int x=4;x<19 && !found;x++){var point=game.ship.bounds.min+Vector3.Scale(game.ship.bounds.size,new Vector3(x/20f,y/20f,0));if(game.art.Opaque(game.ship.sprite,game.ship.transform.InverseTransformPoint(point))){shipPoint=point;found=true;}}
            game.HighlightAt(shipPoint);Check(game.interactions.Hovered==game.ship,"ship cargo has interaction highlight");game.HighlightAt(null);
            game.journey.SetArea(WorldArea.GoldenField,new Vector2(-11,-14.4f));game.seconds=210;game.survivor.equipment[0]=new Item(ItemKind.Lamp);h.Tick(0);h.Warp(new Vector2(-11,-7));h.QuoteAge=10;
            Check(h.skin.sharedMaterial.shader.isSupported,"axolotl material has a supported compiled shader on the actual GPU");
            Vector2[] directions={Vector2.down,Vector2.right,Vector2.up};Rect[] faces={new Rect(.35f,.40f,.30f,.15f),new Rect(.095f,.43f,.17f,.15f),new Rect(.22f,.75f,.12f,.07f)};
            for(int view=0;view<3;view++)
            {
                h.look=directions[view];h.frame=0;h.clock=0;h.GaitMoving=true;h.distance=0;h.Pose();
                game.checkCameraSize=7;game.checkCameraPosition=new Vector3(h.Position.x,h.Position.y+2.5f,-20);game.ResetCamera(game.checkCameraPosition.Value);yield return null;
                Shader.SetGlobalColor("_CoastAmbient",new Color(.6f,.65f,.7f));Shader.SetGlobalInt("_CoastLightCount",0);
                game.cameraView.Render();var rest=ReadPixels();var a=h.ArtPoint(faces[view].min);var b=h.ArtPoint(faces[view].max);var face=Rect.MinMaxRect(Mathf.Min(a.x,b.x),Mathf.Min(a.y,b.y),Mathf.Max(a.x,b.x),Mathf.Max(a.y,b.y));
                var hashes=new HashSet<ulong>();float faceChange=0;var phases=new HashSet<int>();
                for(int phase=0;phase<NightHunter.MovementFrames;phase++)
                {
                    h.distance=(phase+.01f)*3.6f/NightHunter.MovementFrames;h.Pose();phases.Add(h.MotionFrame);game.cameraView.Render();var image=ReadPixels();
                    ulong hash=1469598103934665603UL;foreach(var pixel in image.GetPixels32()){hash=(hash^pixel.r)*1099511628211UL;hash=(hash^pixel.g)*1099511628211UL;hash=(hash^pixel.b)*1099511628211UL;}hashes.Add(hash);
                    faceChange=Mathf.Max(faceChange,RegionDifference(rest,image,face));
                    if(phase%12==0)File.WriteAllBytes(Path.Combine(directory,"v23-hunter-run-"+view+"-"+phase.ToString("D2")+".png"),image.EncodeToPNG());Destroy(image);
                }
                Check(phases.Count==48 && hashes.Count>=42,"view "+view+" renders forty-eight gait phases with at least forty-two distinct pixel images ("+hashes.Count+")");
                Check(faceChange<.00001f,"head remains pixel-identical across every movement phase in view "+view+" ("+faceChange.ToString("F6")+")");
                Check(NightHunter.LegWeights(view,faces[view].center)==Vector4.zero,"face receives no gait bone deformation in view "+view);
                var block=new MaterialPropertyBlock();h.skin.GetPropertyBlock(block);var pivot=h.TailPivot;var local=h.sprite.sprite.bounds.min+(Vector3)Vector2.Scale(h.sprite.sprite.bounds.size,pivot);
                Check(Vector2.Distance(block.GetVector("_TailPivot"),local)<.001f && block.GetVector("_RunMotion0")!=Vector4.zero,"actual shader receives correct tail pivot and independent leg motion in view "+view);Destroy(rest);
            }
            h.GaitMoving=false;h.frame=0;h.Warp(h.Position);h.look=Vector2.down;h.Pose();h.Hurt(1);Check(game.feedback.Flash(h.sprite)>.95f,"giant hunter uses same damage-bar feedback");
            game.seconds=360;h.Tick(0);game.survivor.health=100;game.survivor.equipment[0]=null;game.checkCameraSize=null;game.checkCameraPosition=null;
            game.journey.SetArea(WorldArea.Coast,new Vector2(-1.5f,-1.65f));game.UpdateEnvironment(0);game.HighlightAt(null);
        }
    }
}
