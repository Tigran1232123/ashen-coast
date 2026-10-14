using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed partial class RuntimeChecks : MonoBehaviour
    {
        public CoastGame game;
        readonly List<string> results=new List<string>();
        string directory;
        bool failed;
        void Check(bool ok,string name){results.Add((ok?"PASS ":"FAIL ")+name);if(!ok)failed=true;Debug.Log(results[results.Count-1]);}
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-test-output");
            directory=at>=0 && at+1<args.Length?args[at+1]:Path.Combine(Application.persistentDataPath,"Verification");Directory.CreateDirectory(directory);
            game.combat.manual=true;
            AudioListener.volume=0;
            while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;
            yield return null;yield return new WaitForEndOfFrame();
            if(args.Contains("-v48-final-only"))
            {
                yield return GuardV37(VillageFinalV48());results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v48-checks-only"))
            {
                yield return GuardV37(VillageChangesV48());results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v46-visual-only"))
            {
                yield return GuardV37(AmazonVisualV46());results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v46-final-only"))
            {
                yield return GuardV37(AmazonFinalV46());results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v46-checks-only"))
            {
                yield return GuardV37(AmazonChecksV46());results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v45-checks-only"))
            {
                yield return GuardV37(ChangesV45());results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v44-checks-only"))
            {
                yield return GuardV37(ChangesV44());yield return GuardV37(GlyphLayoutV40());results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v43-checks-only"))
            {
                yield return GuardV37(ChangesV43());yield return GuardV37(GlyphLayoutV40());results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v41-checks-only"))
            {
                yield return GuardV37(BooksPickupV41());yield return GuardV37(FrogCombatV41());yield return GuardV37(RainbowMapV41());yield return GuardV37(GlyphLayoutV40());results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v40-checks-only"))
            {
                yield return GuardV37(BookFramesV40());yield return GuardV37(GlyphLayoutV40());results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v39-checks-only"))
            {
                yield return GuardV37(BooksAndLayersV39());yield return GuardV37(GlyphsAndCameraV39());yield return GuardV37(SpectrumAndRainbowV39());results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v38-checks-only"))
            {
                yield return GuardV37(SpectrumChecksV38());yield return GuardV37(RainbowChecksV38());results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v37-checks-only"))
            {
                yield return GuardV37(BookChecksV37());yield return GuardV37(SpellChecksV37());yield return GuardV37(TravelAndSpectrumV37());results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v36-checks-only"))
            {
                yield return SphereAndWebV36();yield return HunterAggressionV36();results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v35-checks-only"))
            {
                yield return RollerChecksV35();yield return ArtCookingCraftV35();results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v34-checks-only"))
            {
                yield return InventoryCookingV34();yield return CreaturesV34();yield return CaveV34();results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v33-checks-only"))
            {
                yield return CaravanExplosionChecks();yield return CatacombChecks();results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v32-checks-only"))
            {
                yield return CaravanExplosionChecks();results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v31-checks-only"))
            {
                yield return BuildingPerspectiveChecks();results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v30-checks-only"))
            {
                yield return WheelDefeatChecks();results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v29-checks-only"))
            {
                yield return PlacementGridChecks();results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v28-checks-only"))
            {
                yield return BuildingVictoryChecks();results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v27-checks-only"))
            {
                yield return FireMeditationChecks();results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            if(args.Contains("-v24-checks-only"))
            {
                yield return PrismDragonChecks();results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
                File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);Debug.Log(results.Last());Application.Quit(failed?1:0);yield break;
            }
            Check(game.hero!=null && game.ship!=null && game.dummy!=null,"world initialized with generated sprites");
            Check(game.art.hero.Cast<Sprite>().All(s=>s.texture.name=="HeroWalkV14") && game.journey.art.lampHero.Cast<Sprite>().All(s=>s.texture.name=="HeroLampV14"),"adult slim-legged protagonist remains consistent in every direction and lamp pose");
            Check(game.journey.art.people.Cast<Sprite>().All(s=>s.texture.name=="TavernPeopleV14b") && game.journey.art.gestures.Cast<Sprite>().All(s=>s.texture.name=="TavernGesturesV14") && game.journey.art.mage.Cast<Sprite>().All(s=>s.texture.name=="MageActorsV14"),"all human NPC walk and gesture frames use adult proportions and new uncovered hairstyles");
            Check(game.survivor.inventory.items.Any(i=>i.kind==ItemKind.Lamp) && game.survivor.inventory.items.Any(i=>i.kind==ItemKind.Pyromancy) && game.survivor.inventory.items.Any(i=>i.kind==ItemKind.AbyssTome) && game.survivor.inventory.items.Any(i=>i.kind==ItemKind.YoYo),"new magical equipment starts in player inventory");game.survivor.inventory.items.Clear();
            Check(game.flowers.Count>=1000,"more than one thousand collectible wildflowers");
            Check(game.pixelView.pixels.filterMode==FilterMode.Point,"pixel-perfect nearest sampling");
            game.seconds=0;game.UpdateEnvironment(0);
            yield return Capture("01-storm-fog.png");
            Check(CoastTime.Fog(10)==0 && CoastTime.Rain(10)>0,"fog clears while rain continues");
            game.seconds=12;game.UpdateEnvironment(0);yield return Capture("01b-rain.png");
            game.seconds=20;game.UpdateEnvironment(0);yield return Capture("02-clear-coast.png");
            Check(CoastTime.Fog(game.seconds)==0 && CoastTime.Rain(game.seconds)==0,"clear weather after 20 seconds");
            yield return SurfChecks();
            var start=(Vector2)game.hero.transform.position;
            var first=game.flowers.First(f=>!f.picked);game.body.position=first.position+new Vector2(0,-.3f);game.hero.transform.position=game.body.position;
            Check(game.Collect(first),"flower collected in reach");Check(game.survivor.inventory.items.Count==1 && first.picked && !first.renderer.enabled,"flower moves to one unstacked inventory item");
            var second=game.flowers.First(f=>!f.picked);game.body.position=second.position+new Vector2(0,-.4f);game.hero.transform.position=game.body.position;
            Check(game.TryCollectAt(second.renderer.bounds.center),"click on rendered flower collects it");
            var third=game.flowers.First(f=>!f.picked);game.body.position=third.position;game.hero.transform.position=game.body.position;game.CollectNearest();
            Check(third.picked,"nearby flower can be collected with the F action");
            game.ResetCamera(game.hero.transform.position+new Vector3(0,2.2f,-20));
            yield return Capture("02b-detailed-flower-meadow.png");
            yield return FlowerWindChecks();
            yield return WalkingChecks();yield return GaitChecks();
            game.survivor.inventory.items.Clear();game.body.position=start;game.hero.transform.position=start;
            var types=new[]{ItemKind.Sword,ItemKind.Dagger,ItemKind.Scythe,ItemKind.Spear};
            var distances=new[]{1.45f,.67f,1.45f,2.55f};
            for(int k=0;k<types.Length;k++)
            {
                for(int direction=0;direction<8;direction++)
                {
                    float angle=direction*45*Mathf.Deg2Rad;var aim=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                    Vector2 position=game.DummyBase+new Vector2(0,.28f)-aim*distances[k];
                    game.body.position=position;game.hero.transform.position=position;
                    game.numbers.Clear();
                    game.survivor.equipment[0]=new Item(types[k]);game.dummyHealth=500;game.combat.ResetForCheck();
                    Check(game.combat.Begin(aim),types[k]+" begins direction "+direction);
                    game.combat.Advance(.08f);Check(game.dummyHealth==500,types[k]+" windup has no premature damage direction "+direction);
                    for(int step=9;step<=88;step++)game.combat.Advance(step/100f);
                    Check(game.dummyHealth==500-ItemDefinition.All[types[k]].damage,types[k]+" textured contact, fixed damage, one hit direction "+direction);
                }
                game.hero.transform.position=game.DummyBase+new Vector2(-distances[k],.28f);game.body.position=game.hero.transform.position;
                game.numbers.Clear();
                game.dummyHealth=500;game.combat.ResetForCheck();game.combat.Begin(Vector2.right);game.combat.Advance(.5f);
                game.cameraView.orthographicSize=4;game.ResetCamera(game.hero.transform.position+new Vector3(0,1,-20));
                yield return Capture("weapon-"+types[k]+".png");
                game.cameraView.orthographicSize=8;
                game.hero.transform.position=game.DummyBase+new Vector2(-8,.28f);game.body.position=game.hero.transform.position;
                game.dummyHealth=500;game.combat.ResetForCheck();game.combat.Begin(Vector2.right);game.combat.Advance(.9f);
                Check(game.dummyHealth==500,types[k]+" misses outside range");
            }
            game.combat.ResetForCheck();game.survivor.equipment[0]=null;
            yield return WoodlouseChecks();
            game.numbers.Clear();
            game.body.position=start;game.hero.transform.position=start;game.ui.open=true;game.ui.tab=1;
            for(int i=0;i<40;i++)yield return null;
            yield return Capture("03-inventory-storage.png");
            yield return InventoryInteractions();yield return ScreenChecks();yield return OrbChecks();game.ui.open=false;game.ui.ClearCheckPointer();
            var fly=game.fireflies.OrderBy(f=>(f.home-new Vector2(0,7)).sqrMagnitude).First();
            game.body.position=fly.home+new Vector2(0,-1.7f);game.hero.transform.position=game.body.position;
            for(int i=0;i<45;i++)yield return null;
            game.seconds=30;game.UpdateEnvironment(0);yield return new WaitForEndOfFrame();game.cameraView.Render();
            float day=MeanBrightness();
            game.seconds=210;game.UpdateEnvironment(0);yield return new WaitForEndOfFrame();game.cameraView.Render();
            float night=MeanBrightness();Check(day>night*1.8f,"night rendering is darker than daylight ("+day.ToString("F3")+" / "+night.ToString("F3")+")");
            var sample=(Vector2)fly.dot.transform.position;
            Shader.SetGlobalInt("_CoastLightCount",0);game.cameraView.Render();float unlit=BrightnessAt(sample);
            game.UpdateEnvironment(0);game.cameraView.Render();float lit=BrightnessAt(sample);
            Check(lit>unlit+.02f,"firefly light illuminates nearby terrain ("+lit.ToString("F3")+" > "+unlit.ToString("F3")+")");
            Check(game.fireflies.All(f=>f.dot.enabled),"fireflies visible at night");yield return Capture("04-night-fireflies.png");
            game.seconds=360;game.UpdateEnvironment(0);Check(game.fireflies.All(f=>!f.dot.enabled),"fireflies hidden next day");
            Check(Shader.GetGlobalInt("_CoastLightCount")==game.arcane.lastLightCount,"day retains lamp lighting and removes stale firefly lights");
            game.body.position=start;game.hero.transform.position=start;yield return MagicInteractions();yield return NewMagicChecks();yield return RevisionChecks();yield return JourneyChecks();
            yield return MeadowAndMapChecks();
            yield return FortAndHunterChecks();
            yield return HunterRevisionChecks();
            yield return HunterIdleChecks();
            yield return PolishChecks();
            yield return FortPhotoChecks();
            yield return DungeonFeedbackChecks();
            yield return PrismDragonChecks();
            yield return PerformanceSurvey.Run(game,directory);
            results.Add("RESULT "+(failed?"FAILED":"PASSED")+"; "+results.Count+" runtime checks");
            File.WriteAllLines(Path.Combine(directory,"runtime-verification.txt"),results);
            Debug.Log(results[results.Count-1]);Application.Quit(failed?2:0);
        }
        
        IEnumerator FortAndHunterChecks()
        {
            var f=game.fort;var h=game.hunter;var map=game.map;
            game.survivor.health=100;game.journey.companion.health=120;game.ui.CloseContainers();game.combat.ResetForCheck();game.survivor.equipment[0]=null;
            Check(WorldShape.Land(f.Position,48) && !WorldShape.Land(new Vector2(450,950)),"fort is safely inside irregular land, former rectangular corner is sea");
            Check(!MeadowLayout.Generate(MeadowLayout.ChunkAt(f.Position),game.meadow.Seed).Any(s=>WorldShape.FortFootprint(s.position)),"procedural vegetation leaves fort footprint clear");
            map.SetOpen(true);map.Overview();yield return Capture("v18-irregular-world-map.png");map.SetOpen(false);
            Vector2 edge=new Vector2(0,450);while(WorldShape.Distance(edge+Vector2.right*.1f)>.5f)edge+=Vector2.right*.1f;game.body.position=edge-Vector2.right*.2f;game.hero.transform.position=game.body.position;game.checkMovement=Vector2.right;for(int k=0;k<25;k++)yield return new WaitForFixedUpdate();Check(WorldShape.Land(game.body.position,.30f) && game.body.position.x<edge.x+.3f,"actual player movement stops at irregular coastline before entering sea");game.checkMovement=Vector2.left;for(int k=0;k<12;k++)yield return new WaitForFixedUpdate();Check(game.body.position.x<edge.x-.45f,"player can walk back inland from curved boundary without getting stuck");game.checkMovement=null;game.movement=Vector2.zero;game.body.linearVelocity=Vector2.zero;
            game.body.position=new Vector2(0,100);game.hero.transform.position=game.body.position;game.soundscape.Tick(50);game.UpdateEnvironment(0);int gulls=game.soundscape.gullCount;
            Check(game.ShoreAudibility==0 && game.soundscape.gulls.volume==0 && game.soundscape.cello.volume==0,"surf ambience and gull calls are silent far inland");
            game.soundscape.Tick(50);Check(game.soundscape.gullCount==gulls,"no new gull calls are scheduled far from shore");
            game.body.position=new Vector2(0,-1.5f);game.hero.transform.position=game.body.position;game.soundscape.Tick(3);Check(game.ShoreAudibility>.95f && game.soundscape.cello.volume>.2f,"shore sounds return smoothly near coastline");
            game.journey.SetArea(WorldArea.Coast,f.Position+Vector2.down*1.1f);game.meadow.Refresh(game.body.position,true);game.ResetCamera(new Vector3(f.Position.x,f.Position.y+4.7f,-20));yield return Capture("v18-fort-exterior.png");
            Check(f.Interact(f.descent.bounds.center) && f.Inside && f.root.activeSelf && !game.coastRoot.activeSelf,"open fort gateway enters separate stone location");
            Check(f.bandits.Count==16 && f.bandits.All(b=>b.health==100) && f.bandits.Count(b=>b.role==BanditRole.Warrior)==10 && f.bandits.Count(b=>b.role==BanditRole.Archer)==5 && f.bandits.Count(b=>b.role==BanditRole.Mage)==1,"fort contains ten dash warriors, five archers and one mage, each with 100 HP");
            Check(f.torches.Count==13 && f.stoneColliders.Count>=12 && f.rats.Count==6,"stone fort is furnished, lit with thirteen torches and braziers and inhabited by six small rats");
            game.checkCameraSize=18;game.checkCameraPosition=new Vector3(0,3,-20);game.ResetCamera(game.checkCameraPosition.Value);game.UpdateEnvironment(0);yield return Capture("v18-fort-overview.png");game.checkCameraSize=null;game.checkCameraPosition=null;
            game.body.position=new Vector2(1.5f,4);game.hero.transform.position=game.body.position;game.cameraView.orthographicSize=8;game.ResetCamera(new Vector3(1.5f,6.2f,-20));yield return Capture("v18-fort-stone-chambers.png");
            var saved=f.bandits.Select(b=>b.Position).ToArray();
            for(int i=0;i<10;i++){var b=f.bandits[i];b.ClearAggro();b.Warp(new Vector2(0,-8));game.body.position=new Vector2(0,-3);game.hero.transform.position=game.body.position;int before=b.DashCount;b.Tick(2);Vector2 start=b.Position;for(int k=0;k<34;k++)b.Tick(.02f);Check(b.DashCount>before && Vector2.Distance(start,b.Position)>2.5f,"warrior "+i+" performs a telegraphed forward dash");b.Warp(saved[i]);game.survivor.health=100;}
            Check(f.Step(new Vector2(-10.1f,-7),Vector2.right*3,.23f,false)==Vector2.zero,"warrior swept dash cannot tunnel through a stone partition");
            var mage=f.bandits.Last();mage.ClearAggro();mage.Warp(new Vector2(0,-4));game.body.position=new Vector2(0,-2);game.hero.transform.position=game.body.position;int blasts=mage.BlastCount;mage.Tick(2);Check(mage.PreparingBlast,"lamp mage visibly charges radial explosion");mage.Tick(1.07f);game.survivor.health=100;game.arcane.Tick(.30f);Check(mage.BlastCount==blasts+1 && game.survivor.health==45,"mage explosion uses animated campfire blast and inflicts 55 radial damage");yield return Capture("v18-fort-mage-explosion.png");
            foreach(var b in f.bandits){b.health=0;b.sprite.enabled=false;b.feet.enabled=false;}
            game.survivor.health=100;game.body.position=new Vector2(-2,0);game.hero.transform.position=game.body.position;f.Shoot(new Vector2(-2,-5),Vector2.up);for(int k=0;k<30;k++)f.Tick(.02f);Check(game.survivor.health==78,"archer arrow travels and damages the player on swept collision");
            var rat=f.rats[0];game.body.position=rat.Position+Vector2.right*10;game.hero.transform.position=game.body.position;rat.Tick(.1f);Check(rat.Eating,"small rat sometimes stops to nibble food");Vector2 ratStart=rat.Position;game.body.position=ratStart+Vector2.right;game.hero.transform.position=game.body.position;rat.Tick(.15f);Check(rat.Fleeing && Vector2.Distance(rat.Position,game.body.position)>Vector2.Distance(ratStart,game.body.position),"rat flees rapidly when player approaches");
            for(int i=0;i<f.bandits.Count;i++){var b=f.bandits[i];b.health=100;b.sprite.enabled=true;b.feet.enabled=true;b.ClearAggro();b.Warp(saved[i]);}
            var victim=f.bandits[0];victim.Warp(new Vector2(0,-8));game.arcane.DamageArea(victim.Position,.5f,25);Check(victim.health==75,"player area magic also damages fort bandits");victim.Warp(saved[0]);
            map.RecordVisit();Check(map.IsVisited(MapRegion.Fort,game.body.position),"fort exploration has its own discovered map");f.Leave();Check(game.journey.IsCoast && Vector2.Distance(game.body.position,f.Position)<2,"fort exit returns to its random world position");
            game.journey.SetArea(WorldArea.GoldenField,new Vector2(-11,-15.4f));game.seconds=30;h.Tick(.01f);Check(!h.Active,"night hunter does not spawn during day");game.seconds=210;h.Tick(.01f);Check(h.Active,"huge axolotl spawns at night near tavern");h.Warp(new Vector2(-11,-8));
            var lamp=new Item(ItemKind.Lamp);game.survivor.equipment[0]=lamp;game.survivor.mana=0;game.survivor.health=100;h.Tick(.02f);Vector2 guarded=h.Position;for(int i=0;i<30;i++)h.Tick(.02f);Check(h.HeldBack && Vector2.Distance(h.Position,game.body.position)>=NightHunter.LampDistance-.16f && Vector2.Distance(h.Position,guarded)<.1f,"usable fueled lamp keeps hunter in shadow even with zero mana");
            for(int i=0;i<260;i++)h.Tick(.02f);Check(h.QuoteAge<2 && h.QuoteAnchor!=Vector2.zero,"standing before hunter with lamp triggers exact mysterious whisper after five seconds");Vector2 anchor=h.QuoteAnchor;game.UpdateEnvironment(0);game.ResetCamera(new Vector3(-11,-8.7f,-20));yield return Capture("v20-night-hunter-whisper.png");h.Warp(h.Position+Vector2.right*.3f);h.Tick(.8f);Check(Vector2.Distance(h.QuoteAnchor,h.HeadTop+Vector2.up*.48f)<.01f && Vector2.Distance(h.QuoteAnchor,anchor)>.05f,"whisper follows the head instead of the sprite top or tail");h.QuoteAge=3.65f;yield return Capture("v20-night-hunter-whisper-evaporates.png");
            h.Warp(new Vector2(-11,-8));lamp.fuel=0;Vector2 chase=h.Position;int feet=h.FootstepCount;h.Tick(.2f);Check(!h.HeldBack && h.Running && Vector2.Distance(h.Position,chase)>2 && h.FootstepCount>feet,"empty lamp allows fast charge with rapid foot scrambling audio");
            h.Warp(new Vector2(-11,-8));lamp.fuel=100;lamp.durability=0;h.Tick(.1f);Check(!h.HeldBack && h.Running,"broken fueled lamp provides no protection");
            h.Warp(new Vector2(-11,-8));lamp.durability=100;lamp.fuel=100;h.Tick(.01f);game.UpdateEnvironment(0);Check(Shader.GetGlobalColor("_CoastAmbient").r<.09f && Shader.GetGlobalColor("_CoastAmbient").b>.10f,"golden-field night becomes much darker while retaining cold ambient visibility");yield return Capture("v20-night-hunter-lamp.png");
            var poses=new HashSet<Sprite>();for(int i=0;i<8;i++){h.distance=i*3.6f/8;h.frame=i;h.Pose();poses.Add(h.sprite.sprite);}Check(poses.Count==8 && h.skin.sharedMaterial.shader.name=="AshenCoast/Axolotl","hunter uses eight painted full body running phases plus flexible animated gill mesh");
            CaptureHunterAnimations();
            game.survivor.equipment[0]=null;h.Warp(game.body.position+Vector2.up*2);h.Tick(.10f);Check(game.Dead && game.MovementLocked && game.survivor.health==0,"axolotl contact one-shots player and locks movement at zero HP");game.survivor.Tick(2);Check(game.survivor.health==0,"food regeneration cannot revive a killed player");yield return Capture("v20-night-hunter-death.png");
            game.survivor.health=100;game.seconds=360;h.Tick(.01f);Check(!h.Active,"hunter disappears at dawn");game.journey.SetArea(WorldArea.Coast,new Vector2(-1.5f,-1.65f));game.seconds=30;game.UpdateEnvironment(0);
        }

        
        void CaptureHunterAnimations()
        {
            var h=game.hunter;Vector2 player=game.body.position;Vector2 monster=h.Position;float oldSize=game.cameraView.orthographicSize;
            var directions=new[]{Vector2.down,Vector2.right,Vector2.up};float largestChange=0;
            for(int view=0;view<3;view++)
            {
                h.Warp(new Vector2(-11,-7));game.body.position=h.Position+directions[view]*NightHunter.LampDistance;game.hero.transform.position=game.body.position;h.Tick(.001f);
                game.cameraView.orthographicSize=7;game.ResetCamera(new Vector3(h.Position.x,h.Position.y+2,-20));Shader.SetGlobalColor("_CoastAmbient",new Color(.6f,.65f,.7f));Shader.SetGlobalInt("_CoastLightCount",0);
                int w=game.pixelView.pixels.width,ht=game.pixelView.pixels.height;var strip=new Texture2D(w*8,ht,TextureFormat.RGB24,false);Texture2D previous=null;
                for(int frame=0;frame<8;frame++)
                {
                    h.frame=frame;h.clock=frame*.18f;h.Pose();game.cameraView.Render();var picture=ReadPixels();strip.SetPixels(frame*w,0,w,ht,picture.GetPixels());if(frame==0 || frame==4)File.WriteAllBytes(Path.Combine(directory,"v18-axolotl-run-view-"+view+"-pose-"+frame+".png"),picture.EncodeToPNG());
                    if(previous){largestChange=Mathf.Max(largestChange,RegionDifference(previous,picture,new Rect(h.Position.x-5,h.Position.y-2,10,10)));Destroy(previous);}previous=picture;
                }
                if(previous)Destroy(previous);strip.Apply();File.WriteAllBytes(Path.Combine(directory,"v18-axolotl-run-filmstrip-"+view+".png"),strip.EncodeToPNG());Destroy(strip);
            }
            h.frame=0;h.clock=0;h.Pose();game.cameraView.Render();var a=ReadPixels();h.clock=.9f;h.Pose();game.cameraView.Render();var b=ReadPixels();float gills=RegionDifference(a,b,new Rect(h.Position.x-5,h.Position.y-2,10,10));Destroy(a);Destroy(b);
            Check(largestChange>.003f,"actual rendered axolotl limbs visibly change across eight run phases in three views");Check(gills>.0001f,"animated gill mesh visibly moves even with fixed idle sprite");
            game.body.position=player;game.hero.transform.position=player;h.Warp(monster);h.Tick(0);game.cameraView.orthographicSize=oldSize;game.UpdateEnvironment(0);game.ResetCamera(new Vector3(player.x,player.y+6.7f,-20));
        }

        IEnumerator HunterRevisionChecks()
        {
            var h=game.hunter;
            Check(InventoryUI.DeathTitle=="\u0412\u044b \u043f\u043e\u0433\u0438\u0431\u043b\u0438" && InventoryUI.RestartLabel=="\u041d\u0430\u0447\u0430\u0442\u044c \u0437\u0430\u043d\u043e\u0432\u043e","death UI uses correct Cyrillic strings compiled into Windows player");
            game.survivor.health=100;game.survivor.equipment[0]=null;game.ui.CloseContainers();game.combat.ResetForCheck();
            game.journey.SetArea(WorldArea.Coast,new Vector2(0,100));game.meadow.Refresh(game.body.position,true);
            game.ToggleFastMovement();
            for(int d=0;d<8;d++)
            {
                game.checkMovement=new Vector2(Mathf.Cos(d*Mathf.PI/4),Mathf.Sin(d*Mathf.PI/4));
                yield return null; // Let Update sample the changed direction before checking physics.
                for(int i=0;i<3;i++)yield return new WaitForFixedUpdate();
                Check(Mathf.Abs(game.body.linearVelocity.magnitude-CoastGame.WalkSpeed*5)<.08f,"T acceleration applies actual fivefold Rigidbody movement in direction "+d);
            }
            game.ToggleFastMovement();for(int i=0;i<3;i++)yield return new WaitForFixedUpdate();
            Check(!game.FastMovement && Mathf.Abs(game.body.linearVelocity.magnitude-CoastGame.WalkSpeed)<.08f,"pressing T again restores original movement speed");
            game.checkMovement=null;game.movement=Vector2.zero;game.body.linearVelocity=Vector2.zero;
            game.survivor.equipment[0]=new Item(ItemKind.Lamp);
            game.journey.SetArea(WorldArea.GoldenField,new Vector2(-11,-15.4f));game.seconds=30;h.Tick(0);game.StartNight();
            Check(CoastTime.Daylight(game.seconds)==0 && h.Active && !game.Dead && Shader.GetGlobalColor("_CoastAmbient").r<.09f,"U immediately sets full night lighting and spawns outdoor hunter");
            float firstNight=game.seconds;game.seconds+=20;game.StartNight();
            Check(game.seconds>firstNight+20 && CoastTime.Daylight(game.seconds)==0,"U can start another night without moving the world clock backwards");
            game.survivor.health=100;game.survivor.equipment[0]=new Item(ItemKind.Lamp);game.movement=Vector2.zero;
            h.Warp(new Vector2(-11,-8));h.Tick(.001f);h.Speak();int quotes=h.QuoteCount;
            for(int i=0;i<195;i++)h.Tick(.02f);
            Check(h.QuoteVisible && h.QuoteAge>3.8f,"detailed whisper remains visible for almost four seconds");
            game.UpdateEnvironment(0);game.ResetCamera(new Vector3(-11,-8.7f,-20));h.QuoteAge=1.6f;
            yield return Capture("v20-whisper-particles.png");h.QuoteAge=3.9f;
            for(int i=0;i<10;i++)h.Tick(.02f);
            Check(!h.QuoteVisible,"whisper finishes its fade and disappears after four seconds");
            // Speak() is an actual event: repeat interval is measured between event starts.
            h.Speak();quotes=h.QuoteCount;
            for(int i=0;i<740;i++)h.Tick(.02f);
            Check(h.QuoteCount==quotes,"whisper cannot repeat earlier than fifteen seconds");
            for(int i=0;i<20;i++)h.Tick(.02f);
            Check(h.QuoteCount==quotes+1 && h.QuoteAge<.4f,"stationary lamp encounter repeats whisper every fifteen seconds without another seven-second wait");
            h.QuoteAge=10;
            var directions=new[]{Vector2.down,Vector2.right,Vector2.up};
            Rect[] faces={new Rect(.35f,.35f,.30f,.20f),new Rect(.095f,.43f,.17f,.15f),new Rect(.22f,.75f,.12f,.07f)};
            Rect[] ears={new Rect(.035f,.47f,.20f,.28f),new Rect(.35f,.38f,.14f,.40f),new Rect(.035f,.76f,.145f,.18f)};
            for(int view=0;view<3;view++)
            {
                h.Warp(new Vector2(-11,-7));game.body.position=h.Position+directions[view]*NightHunter.LampDistance;game.hero.transform.position=game.body.position;h.Tick(.001f);
                game.cameraView.orthographicSize=7;game.ResetCamera(new Vector3(h.Position.x,h.Position.y+2,-20));Shader.SetGlobalColor("_CoastAmbient",new Color(.6f,.65f,.7f));Shader.SetGlobalInt("_CoastLightCount",0);
                h.frame=0;h.clock=0;h.Pose();game.cameraView.Render();var a=ReadPixels();
                File.WriteAllBytes(Path.Combine(directory,"v20-idle-view-"+view+"-a.png"),a.EncodeToPNG());
                h.clock=.9f;h.Pose();game.cameraView.Render();var b=ReadPixels();
                File.WriteAllBytes(Path.Combine(directory,"v20-idle-view-"+view+"-b.png"),b.EncodeToPNG());
                Rect WorldRect(Rect uv)
                {var start=h.ArtPoint(uv.min);var end=h.ArtPoint(uv.max);return Rect.MinMaxRect(Mathf.Min(start.x,end.x),Mathf.Min(start.y,end.y),Mathf.Max(start.x,end.x),Mathf.Max(start.y,end.y));}
                float face=RegionDifference(a,b,WorldRect(faces[view])),ear=RegionDifference(a,b,WorldRect(ears[view]));
                Check(face<.00001f,"idle face pixels remain rigid without jelly distortion in view "+view+" ("+face.ToString("F6")+")");
                Check(ear>.0001f,"idle gills visibly sway while body stays still in view "+view+" ("+ear.ToString("F6")+")");
                Destroy(a);Destroy(b);
            }
            game.survivor.health=100;game.survivor.equipment[0]=null;game.cameraView.orthographicSize=8;
            game.journey.SetArea(WorldArea.Coast,new Vector2(-1.5f,-1.65f));game.seconds=30;h.Tick(0);game.UpdateEnvironment(0);
        }

        IEnumerator HunterIdleChecks()
        {
            var h=game.hunter;game.survivor.health=100;game.survivor.equipment[0]=new Item(ItemKind.Lamp);game.survivor.mana=0;
            game.journey.SetArea(WorldArea.GoldenField,new Vector2(-11,-14.4f));game.seconds=750;h.Tick(0);game.StartNight();
            game.ui.CloseContainers();game.combat.ResetForCheck();game.movement=Vector2.zero;
            var directions=new[]{Vector2.down,Vector2.right,Vector2.up};
            Rect[] tails={new Rect(.48f,.76f,.40f,.22f),new Rect(.72f,.48f,.27f,.35f),new Rect(.60f,.07f,.38f,.48f)};
            Rect[] paws={new Rect(.11f,.025f,.32f,.30f),new Rect(.04f,.15f,.32f,.23f),new Rect(.02f,.12f,.25f,.30f)};
            Rect[] otherPaws={new Rect(.60f,.02f,.29f,.25f),new Rect(.64f,.10f,.22f,.27f),new Rect(.31f,.03f,.25f,.27f)};
            Rect[] faces={new Rect(.35f,.35f,.30f,.20f),new Rect(.095f,.43f,.17f,.15f),new Rect(.22f,.75f,.12f,.07f)};
            var properties=new MaterialPropertyBlock();
            Rect WorldRect(Rect uv)
            {var a=h.ArtPoint(uv.min);var b=h.ArtPoint(uv.max);return Rect.MinMaxRect(Mathf.Min(a.x,b.x),Mathf.Min(a.y,b.y),Mathf.Max(a.x,b.x),Mathf.Max(a.y,b.y));}
            void FreezeGills(float tail)
            {h.skin.GetPropertyBlock(properties);properties.SetFloat("_GillTime",0);properties.SetFloat("_TailTime",tail);h.skin.SetPropertyBlock(properties);}
            void AdvanceTo(float target)
            {while(h.IdleSeconds<target-.00001f)h.Tick(Mathf.Min(.02f,target-h.IdleSeconds));}
            for(int view=0;view<3;view++)
            {
                h.Warp(new Vector2(-11,-7));game.body.position=h.Position+directions[view]*NightHunter.LampDistance;game.hero.transform.position=game.body.position;h.Tick(.01f);
                game.cameraView.orthographicSize=7;game.ResetCamera(new Vector3(h.Position.x,h.Position.y+2,-20));Shader.SetGlobalColor("_CoastAmbient",new Color(.6f,.65f,.7f));Shader.SetGlobalInt("_CoastLightCount",0);
                h.Pose();FreezeGills(0);game.cameraView.Render();var still=ReadPixels();
                FreezeGills(1.4f);game.cameraView.Render();var sway=ReadPixels();
                float tail=RegionDifference(still,sway,WorldRect(tails[view]));
                Check(tail>.001f,"tail visibly swings with gills frozen in rendered view "+view+" ("+tail.ToString("F6")+")");
                File.WriteAllBytes(Path.Combine(directory,"v20-tail-view-"+view+"-a.png"),still.EncodeToPNG());File.WriteAllBytes(Path.Combine(directory,"v20-tail-view-"+view+"-b.png"),sway.EncodeToPNG());Destroy(still);Destroy(sway);
                int quotes=h.QuoteCount;AdvanceTo(4.8f);
                Check(!h.Scratching && h.IdleSeconds<5,"no paw scraping before five seconds in view "+view);
                if(view==0)Check(h.QuoteCount==quotes,"first whisper stays hidden before five-second stand delay");
                h.Pose();FreezeGills(0);game.cameraView.Render();var before=ReadPixels();
                File.WriteAllBytes(Path.Combine(directory,"v20-paw-view-"+view+"-rest.png"),before.EncodeToPNG());
                AdvanceTo(5.16f);h.Pose();FreezeGills(0);game.cameraView.Render();var lifted=ReadPixels();
                File.WriteAllBytes(Path.Combine(directory,"v20-paw-view-"+view+"-lift.png"),lifted.EncodeToPNG());
                float foot=RegionDifference(before,lifted,WorldRect(paws[view]));
                float fixedFoot=RegionDifference(before,lifted,WorldRect(otherPaws[view]));
                float face=RegionDifference(before,lifted,WorldRect(faces[view]));
                Check(h.Scratching && foot>.001f,"one forepaw lifts to scrape after five seconds in view "+view+" ("+foot.ToString("F6")+")");
                Check(fixedFoot<.00001f && face<.00001f,"other paw and face stay rigid during scraping in view "+view);
                if(view==0)Check(h.QuoteCount==quotes+1 && h.QuoteAge<.3f,"first whispered line starts at five seconds rather than seven");
                Destroy(before);Destroy(lifted);int strokes=h.ScrapeCount;
                AdvanceTo(5.63f);h.Pose();FreezeGills(0);game.cameraView.Render();var raked=ReadPixels();
                File.WriteAllBytes(Path.Combine(directory,"v20-paw-view-"+view+"-rake.png"),raked.EncodeToPNG());Destroy(raked);
                Check(h.ScrapeCount>strokes && game.journey.fieldRoot.GetComponentsInChildren<LineRenderer>().Any(l=>l.name.StartsWith("Claw furrow") && l.enabled),"scraping paw leaves a furrow and scattered earth in view "+view);
                // Export actual rendered motion frames for a short preview, with normal moving gills/tail.
                h.Warp(h.Position);h.Tick(.01f);AdvanceTo(4.9f);
                for(int i=0;i<32;i++)
                {h.Tick(.075f);h.Pose();game.cameraView.Render();var shot=ReadPixels();File.WriteAllBytes(Path.Combine(directory,"v20-scrape-motion-"+view+"-"+i.ToString("D2")+".png"),shot.EncodeToPNG());Destroy(shot);}
            }
            h.Speak();h.QuoteAge=1.6f;game.UpdateEnvironment(0);game.checkCameraSize=9;game.ResetCamera(new Vector3(h.HeadTop.x,h.HeadTop.y-2,-20));
            yield return Capture("v20-dark-violet-whisper.png");game.checkCameraSize=null;
            bool dark=true;
            for(int i=0;i<100;i++){var c=NightHunter.WhisperInk(i*.17f,i,1);dark&=c.r<.26f && c.g<.07f && c.b<.38f && c.b>c.r && c.r>c.g;}
            Check(dark,"whisper and spark palette contains only very dark violet shades without white");
            game.body.position=h.Position+Vector2.up*12;game.hero.transform.position=game.body.position;h.Tick(.1f);
            Check(h.IdleSeconds==0 && !h.Scratching,"walking cancels scratching and resets five-second idle delay");
            game.seconds=1080;h.Tick(.01f);Check(!h.Active && h.IdleSeconds==0,"dawn hides hunter and resets idle animation state");
            game.journey.SetArea(WorldArea.Coast,new Vector2(-1.5f,-1.65f));game.seconds=30;game.survivor.equipment[0]=null;game.cameraView.orthographicSize=8;game.UpdateEnvironment(0);
        }

        IEnumerator FortPhotoChecks()
        {
            game.checkCameraPosition=null;game.checkCameraSize=null;game.ui.CloseContainers();game.combat.ResetForCheck();
            game.survivor.health=100;game.survivor.equipment[0]=new Item(ItemKind.Lamp);game.survivor.inventory.items.Clear();game.seconds=60;
            game.journey.SetArea(WorldArea.Fort,new Vector2(1.5f,4));game.arcane.Tick(3);game.fort.Tick(0);
            var actors=game.fort.bandits.Select(b=>b.sprite.enabled).ToArray();
            foreach(var b in game.fort.bandits)b.ClearAggro();
            var coin=game.CreateFortLoot(new Vector2(2,4.5f));game.UpdateEnvironment(0);yield return null;
            Check(coin.renderer.bounds.size.x<.3f && coin.renderer.bounds.size.y<.22f,"dropped coins have small foreshortened ground dimensions rather than item-size squares");
            coin.renderer.enabled=false;coin.shadow.enabled=false;game.cameraView.Render();var background=ReadPixels();
            coin.renderer.enabled=true;game.cameraView.Render();var foreground=ReadPixels();
            var bounds=coin.renderer.bounds;var lo=game.cameraView.WorldToViewportPoint(bounds.min);var hi=game.cameraView.WorldToViewportPoint(bounds.max);
            int drawn=0,corner=0;
            for(int y=Mathf.CeilToInt(lo.y*foreground.height);y<hi.y*foreground.height;y++)for(int x=Mathf.CeilToInt(lo.x*foreground.width);x<hi.x*foreground.width;x++)
            {
                var a=background.GetPixel(x,y);var b=foreground.GetPixel(x,y);
                if(Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b)>.015f)
                {
                    drawn++;float u=(x+.5f-lo.x*foreground.width)/((hi.x-lo.x)*foreground.width),v=(y+.5f-lo.y*foreground.height)/((hi.y-lo.y)*foreground.height);
                    if((u<.14f || u>.86f) && (v<.14f || v>.86f))corner++;
                }
            }
            Check(drawn>2 && corner==0,"actual ground coin render keeps a visible engraving and clips all four square corners ("+drawn+" pixels)");Destroy(background);Destroy(foreground);coin.shadow.enabled=true;
            int amount=coin.item.count,balance=game.survivor.Coins;game.body.position=coin.position;game.hero.transform.position=coin.position;
            Check(game.CollectLoot(coin) && game.survivor.Coins==balance+amount,"small coin keeps its original pickup and money value");
            for(int i=0;i<16;i++)game.CreateFortLoot(new Vector2((i%5)*1.3f-2.3f,2+i/5*1.25f));
            game.survivor.health=16;game.survivor.mana=74;
            var sizes=new[]{new Vector2Int(1440,900),new Vector2Int(1365,767),new Vector2Int(640,900),new Vector2Int(1920,600),new Vector2Int(Screen.currentResolution.width,Screen.currentResolution.height)};
            var edges=new[]{new Vector2(-19.35f,-14.4f),new Vector2(-19.35f,16.6f),new Vector2(19.35f,16.6f),new Vector2(19.35f,-14.4f)};
            for(int mode=0;mode<sizes.Length;mode++)
            {
                var size=sizes[mode];Screen.SetResolution(size.x,size.y,mode==sizes.Length-1?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(.5f);yield return new WaitForEndOfFrame();
                var presenter=FindFirstObjectByType<PixelPresenter>();
                Check(game.cameraView.rect==new Rect(0,0,1,1) && presenter.GetComponent<Camera>().rect==new Rect(0,0,1,1) && Mathf.Abs(game.cameraView.aspect-(float)Screen.width/Screen.height)<.002f,"world and presentation occupy the full viewport after resize/fullscreen mode "+mode);
                Check(game.pixelView.pixels.width==(Screen.width+1)/2 && game.pixelView.pixels.height==(Screen.height+1)/2,"original terrain pixel sampling follows actual window dimensions in mode "+mode);
                Check(game.ui.TryActorLabelPosition(new Vector2(1,game.ui.CanvasHeight/2),out var leftLabel) && leftLabel.x-80>0,"enemy label at left edge keeps its full name inside the screen in mode "+mode);
                Check(game.ui.TryActorLabelPosition(new Vector2(game.ui.CanvasWidth-1,game.ui.CanvasHeight/2),out var rightLabel) && rightLabel.x+80<game.ui.CanvasWidth,"enemy label at right edge keeps its full name inside the screen in mode "+mode);
                Check(!game.ui.TryActorLabelPosition(game.ui.StatusPanel.center,out var hiddenLabel),"enemy label cannot cover hero status bars in mode "+mode);
                Check(!game.ui.TryActorLabelPosition(game.ui.FoodPanel.center,out hiddenLabel),"enemy label cannot cover food slots in mode "+mode);
                for(int edge=0;edge<edges.Length;edge++)
                {
                    game.ui.open=false;game.body.position=edges[edge];game.hero.transform.position=game.body.position;
                    game.ResetCamera(new Vector3(edges[edge].x,edges[edge].y+2.2f,-20));yield return null;yield return new WaitForEndOfFrame();
                    var floor=FortWorld.FloorBounds;var left=game.cameraView.ViewportToWorldPoint(Vector3.zero);var right=game.cameraView.ViewportToWorldPoint(Vector3.one);
                    Check(left.x>=floor.xMin && left.y>=floor.yMin && right.x<=floor.xMax && right.y<=floor.yMax,"camera stays above stone at fort edge "+edge+" in mode "+mode);
                    game.cameraView.Render();var image=ReadPixels();float rim=0;int count=0;
                    for(int x=3;x<image.width-3;x+=7){rim+=image.GetPixel(x,2).grayscale+image.GetPixel(x,image.height-3).grayscale;count+=2;}
                    Check(rim/count>.037f,"actual top and bottom fort edges contain stone instead of black background at edge "+edge+" in mode "+mode);Destroy(image);
                    if(mode==0 && edge==0)yield return Capture("v22-fort-west-edge.png");
                    if(mode==3 && edge==2)yield return Capture("v22-fort-wide-window.png");
                }
                game.body.position=new Vector2(19.35f,0);game.hero.transform.position=game.body.position;game.ui.open=true;
                game.ResetCamera(new Vector3(40,2.2f,-20));for(int i=0;i<30;i++)yield return null;
                var player=game.cameraView.WorldToViewportPoint(game.hero.bounds.center);
                Check(player.x>.02f && player.x<game.ui.WorldFraction-.02f,"hero stays in the visible world beside the east wall with inventory open in mode "+mode);
                if(mode==0)yield return Capture("v22-fort-east-inventory.png");
                game.ui.CloseContainers();game.body.position=new Vector2(1.5f,4);game.hero.transform.position=game.body.position;
                game.ResetCamera(new Vector3(1.5f,6.2f,-20));yield return null;yield return new WaitForEndOfFrame();
                // Hold the world texture so animated lighting cannot be mistaken for UI pixels.
                bool cameraEnabled=game.cameraView.enabled;game.cameraView.enabled=false;
                var hud=ScreenCapture.CaptureScreenshotAsTexture();game.ui.enabled=false;yield return new WaitForEndOfFrame();var bare=ScreenCapture.CaptureScreenshotAsTexture();game.ui.enabled=true;game.cameraView.enabled=cameraEnabled;
                int minX=Screen.width,minY=Screen.height,maxY=0,hudPixels=0;
                for(int y=0;y<Screen.height*.30f;y++)for(int x=0;x<Screen.width*.24f;x++)
                {
                    var a=hud.GetPixel(x,Screen.height-1-y);var b=bare.GetPixel(x,Screen.height-1-y);
                    if(Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b)>.025f){minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);hudPixels++;}
                }
                Check(hudPixels>300 && minX>0 && minY>0 && maxY>Screen.height*.07f,"actual HUD pixels are inset from the left/top edges and remain visible in mode "+mode);Destroy(hud);Destroy(bare);
                if(mode==0)yield return Capture("v22-fort-coins.png");
                if(mode==sizes.Length-1)yield return Capture("v22-fort-fullscreen.png");
            }
            Screen.SetResolution(1440,900,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);
            for(int i=0;i<actors.Length;i++)game.fort.bandits[i].sprite.enabled=actors[i];
            game.survivor.health=100;game.survivor.equipment[0]=null;
            game.journey.SetArea(WorldArea.Coast,new Vector2(-1.5f,-1.65f));game.meadow.Refresh(game.body.position,true);game.UpdateEnvironment(0);
            yield return Capture("v22-original-coast-ground.png");
            game.journey.SetArea(WorldArea.Coast,new Vector2(0,220));game.meadow.Refresh(game.body.position,true);game.UpdateEnvironment(0);yield return Capture("v22-original-meadow-ground.png");
            game.journey.SetArea(WorldArea.Coast,game.fort.Position+Vector2.down*2);game.meadow.Refresh(game.body.position,true);game.UpdateEnvironment(0);
            yield return Capture("v22-original-fort-exterior.png");
            game.journey.SetArea(WorldArea.GoldenField,new Vector2(0,2));game.UpdateEnvironment(0);yield return Capture("v22-original-tavern-ground.png");
            game.journey.SetArea(WorldArea.Tavern,new Vector2(0,-3.2f));game.UpdateEnvironment(0);yield return Capture("v22-tavern-interior.png");
            game.journey.SetArea(WorldArea.Coast,new Vector2(-1.5f,-1.65f));
        }

        IEnumerator PolishChecks()
        {
            game.ui.CloseContainers();game.combat.ResetForCheck();game.survivor.health=100;game.journey.SetArea(WorldArea.Coast,new Vector2(0,220));game.meadow.Refresh(game.body.position,true);game.seconds=60;game.UpdateEnvironment(0);
            int plans=game.meadow.RefreshPlans;long before=System.GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<1000;i++)game.meadow.Refresh(game.body.position);
            long allocated=System.GC.GetAllocatedBytesForCurrentThread()-before;
            Check(plans==game.meadow.RefreshPlans,"stationary meadow refresh reuses its plan across 1000 calls without sorting or rebuilding chunk lists");
            game.checkCameraSize=8;game.checkCameraPosition=new Vector3(0,222.2f,-20);game.ResetCamera(game.checkCameraPosition.Value);yield return null;yield return null;
            Check(game.pixelView.pixels.width==(Screen.width+1)/2 && game.pixelView.pixels.height==(Screen.height+1)/2,"original half-window pixel presentation restored");yield return Capture("v21-coast-meadow-hud.png");
            game.journey.SetArea(WorldArea.GoldenField,new Vector2(-11,-14.4f));game.seconds=930;game.survivor.equipment[0]=new Item(ItemKind.Lamp);game.hunter.Tick(0);
            var h=game.hunter;var directions=new[]{Vector2.down,Vector2.right,Vector2.up};
            for(int view=0;view<3;view++)
            {
                h.Warp(new Vector2(-11,-7));game.body.position=h.Position+directions[view]*NightHunter.LampDistance;game.hero.transform.position=game.body.position;h.Tick(.01f);h.Pose();
                var scale=h.sprite.transform.localScale;Check(Mathf.Abs(scale.x-scale.y)<.00001f,"axolotl painted aspect ratio preserved without horizontal compression or vertical stretching in view "+view);
                for(int frame=0;frame<8;frame++)
                {
                    h.frame=frame;h.Pose();var source=h.sprite.sprite;var rect=source.rect;var pixels=source.texture.GetPixels32();int low=(int)rect.height;
                    for(int y=0;y<rect.height*.44f;y++)for(int x=0;x<rect.width;x++)if(pixels[((int)rect.y+y)*source.texture.width+(int)rect.x+x].a>160)low=Mathf.Min(low,y);
                    Vector2 toe=h.ArtPoint(new Vector2(.5f,(low+1)/rect.height));
                    Check(Mathf.Abs(toe.y-h.Position.y)<.015f,"painted ground contact stays registered for hunter view "+view+" frame "+frame);
                }
                h.frame=0;h.Pose();game.checkCameraSize=7;game.checkCameraPosition=new Vector3(h.Position.x,h.Position.y+2.5f,-20);game.ResetCamera(game.checkCameraPosition.Value);Shader.SetGlobalColor("_CoastAmbient",new Color(.6f,.65f,.7f));Shader.SetGlobalInt("_CoastLightCount",0);yield return null;
                yield return Capture("v21-hunter-grounded-view-"+view+".png");
            }
            Check(game.journey.fieldRoot.GetComponentsInChildren<SpriteRenderer>().Count(r=>r.name.StartsWith("Axolotl paw contact"))==4,"four dedicated contact shadows ground the monster paws");
            game.body.position=new Vector2(-11,-14.4f);game.hero.transform.position=game.body.position;h.Warp(new Vector2(-11,-7));h.Tick(.01f);game.UpdateEnvironment(0);
            game.checkCameraSize=9;game.checkCameraPosition=new Vector3(-11,-8.5f,-20);game.ResetCamera(game.checkCameraPosition.Value);yield return null;yield return Capture("v21-night-silhouette.png");
            h.Speak();h.QuoteAge=1.6f;yield return Capture("v21-dark-violet-dialogue.png");
            game.seconds=60;h.Tick(0);game.journey.SetArea(WorldArea.GoldenField,new Vector2(0,2));game.survivor.equipment[0]=null;game.UpdateEnvironment(0);game.checkCameraSize=8;game.checkCameraPosition=new Vector3(0,5,-20);game.ResetCamera(game.checkCameraPosition.Value);yield return Capture("v21-tavern-ground-day.png");
            game.journey.SetArea(WorldArea.Fort,new Vector2(1.5f,4));game.UpdateEnvironment(0);game.checkCameraPosition=new Vector3(1.5f,6.2f,-20);game.ResetCamera(game.checkCameraPosition.Value);yield return Capture("v21-fort-interface.png");
            game.journey.SetArea(WorldArea.Coast,new Vector2(-1.5f,-1.65f));game.ui.open=true;game.UpdateEnvironment(0);game.checkCameraPosition=new Vector3(-1.5f,1,-20);game.ResetCamera(game.checkCameraPosition.Value);yield return Capture("v21-inventory.png");game.ui.CloseContainers();
            game.checkCameraSize=null;game.checkCameraPosition=null;
        }

        IEnumerator MeadowAndMapChecks()
        {
            var map=game.map;var meadow=game.meadow;var start=new Vector2(-1.5f,-1.65f);
            game.journey.SetArea(WorldArea.Coast,start);game.ui.CloseContainers();game.combat.ResetForCheck();game.seconds=30;game.UpdateEnvironment(0);
            Check(map.IsVisited(MapRegion.Coast,start) && !map.IsVisited(MapRegion.Coast,new Vector2(240,420)),"initial map reveals player surroundings and keeps distant field hidden");
            Check(map.SetOpen(true) && map.Open && game.paused && Time.timeScale==0 && game.ui.IsPointerOver,"map opens as modal pause and blocks world mouse actions");
            yield return Capture("v17-map-first-visit.png");
            float initialZoom=map.Zoom;map.ChangeZoom(1.5f,Vector2.zero);map.Pan(new Vector2(90,-45));Check(map.Zoom>initialZoom && Vector2.Distance(map.Centre,start)>1,"map supports zoom and drag panning");
            map.Recenter();Check(map.Centre==start,"map recentres on current player");
            bool sound=game.muted;map.SetOpen(false);Check(!map.Open && !game.paused && Time.timeScale==1 && game.muted==sound,"closing map restores play without toggling audio");
            // Walk through the former y=30 border with the actual Rigidbody2D.
            game.body.position=new Vector2(0,29.7f);game.hero.transform.position=game.body.position;game.checkMovement=Vector2.up;
            for(int i=0;i<40;i++)yield return new WaitForFixedUpdate();game.checkMovement=null;game.movement=Vector2.zero;game.body.linearVelocity=Vector2.zero;
            Check(game.body.position.y>32,"player walks continuously beyond old flower-field boundary");
            var trail=new Vector2(8+Mathf.Sin((100-24)*.017f)*27+Mathf.Sin((100-24)*.048f)*9,100);
            game.body.position=trail;game.hero.transform.position=trail;meadow.Refresh(trail,true);game.ResetCamera(new Vector3(trail.x,trail.y+2.2f,-20));yield return null;
            yield return Capture("v17-meadow-footpath.png");
            var distant=new Vector2(240,420);game.body.position=distant;game.hero.transform.position=distant;meadow.Refresh(distant,true);map.RecordVisit();game.ResetCamera(new Vector3(distant.x,distant.y+2.2f,-20));
            yield return null;
            Check(meadow.LoadedChunks<=9 && meadow.LoadedFlowers>1200 && game.flowers.Count<5000,"distant meadow loads dense flowers in bounded nearby chunks");
            Check(game.flowers.Any(f=>f.procedural && Vector2.Distance(f.position,distant)<12),"collectible flowers exist hundreds of units beyond original field");
            Check(map.IsVisited(MapRegion.Coast,distant) && map.IsVisited(MapRegion.Coast,start) && !map.IsVisited(MapRegion.Coast,new Vector2(120,210)),"map preserves old visits and does not reveal untravelled space between visits");
            yield return Capture("v17-procedural-distant-meadow.png");
            map.SetOpen(true);yield return Capture("v17-map-distant-visit.png");map.Overview();yield return Capture("v17-map-whole-coast.png");map.SetOpen(false);
            var flower=game.flowers.First(f=>f.procedural && !f.picked && Vector2.Distance(f.position,distant)<10);long id=flower.proceduralId;
            game.survivor.inventory.items.Clear();game.body.position=flower.position+Vector2.down*.3f;game.hero.transform.position=game.body.position;
            Check(game.Collect(flower) && meadow.WasPicked(id),"procedural flower pickup gives inventory item and records persistent identity");
            var stable=game.flowers.First(f=>f.procedural && !f.picked && Vector2.Distance(f.position,distant)<10);var stablePosition=stable.position;long stableId=stable.proceduralId;
            meadow.Refresh(new Vector2(-300,850),true);yield return null;
            Check(!game.flowers.Contains(stable) && meadow.LoadedChunks<=9,"leaving region unloads distant vegetation");
            meadow.Refresh(distant,true);yield return null;
            Check(!game.flowers.Any(f=>f.procedural && f.proceduralId==id) && game.flowers.Any(f=>f.procedural && f.proceduralId==stableId && f.position==stablePosition),"returning regenerates identical unpicked flowers and keeps collected flowers absent");
            game.body.position=distant;game.hero.transform.position=distant;
            var drop=game.ThrowItem(new Item(ItemKind.Wood),distant+Vector2.right*2);
            Check(drop!=null && drop.position.x>200 && drop.position.y>400,"item throwing uses expanded world bounds");
            var fireSpot=WorldGrid.Snap(distant+Vector2.left*1.5f);Check(game.arcane.CanPlaceFire(fireSpot),"campfires can be placed in procedural continuation");
            game.body.position=new Vector2(0,1023.97f);game.hero.transform.position=game.body.position;game.checkMovement=Vector2.up;
            for(int i=0;i<8;i++)yield return new WaitForFixedUpdate();game.checkMovement=null;game.movement=Vector2.zero;game.body.linearVelocity=Vector2.zero;
            Check(game.body.position.y<=MeadowLayout.MaxY,"outer meadow boundary blocks movement before leaving rendered terrain");
            var far=WorldShape.NearestLand(new Vector2(-110,870),70);game.body.position=far;game.hero.transform.position=far;meadow.Refresh(far,true);game.ResetCamera(new Vector3(far.x,far.y+2.2f,-20));yield return null;
            Check(game.flowers.Where(f=>f.procedural).All(f=>f.shadow.sortingOrder<f.renderer.sortingOrder),"distant shadows stay behind flowers despite large world depth");
            Check(game.heroShadow.sortingOrder<game.hero.sortingOrder,"player shadow remains behind adult sprite in far northern meadow");
            yield return Capture("v17-far-northern-meadow.png");
            var northernFly=game.fireflies.Where(f=>f.procedural).OrderBy(f=>Vector2.Distance(f.home,far)).First();game.body.position=northernFly.home+Vector2.down;game.hero.transform.position=game.body.position;
            game.ResetCamera(game.hero.transform.position+new Vector3(0,2.2f,-20));game.seconds=210;game.UpdateEnvironment(0);
            Check(northernFly.dot.enabled,"streamed meadow retains animated nighttime fireflies");
            yield return new WaitForEndOfFrame();Shader.SetGlobalInt("_CoastLightCount",0);game.cameraView.Render();float dark=BrightnessAt(northernFly.dot.transform.position);
            game.UpdateEnvironment(0);game.cameraView.Render();float glow=BrightnessAt(northernFly.dot.transform.position);Check(glow>dark+.02f,"procedural firefly visibly illuminates distant grass");
            yield return Capture("v17-distant-night-meadow.png");game.seconds=30;game.UpdateEnvironment(0);
            game.journey.SetArea(WorldArea.GoldenField,new Vector2(0,-3));map.RecordVisit();Check(map.IsVisited(MapRegion.GoldenField,game.body.position) && !map.IsVisited(MapRegion.GoldenField,game.journey.InnEntrance),"golden-field map keeps unvisited tavern hidden");
            game.journey.SetArea(WorldArea.GoldenField,game.journey.InnEntrance+Vector2.down);map.RecordVisit();Check(map.IsVisited(MapRegion.GoldenField,game.journey.InnEntrance),"tavern landmark opens only when approached");
            map.SetOpen(true);yield return Capture("v17-map-golden-field.png");map.SetOpen(false);
            game.journey.SetArea(WorldArea.Tavern,new Vector2(0,-4));map.RecordVisit();Check(map.IsVisited(MapRegion.Tavern,new Vector2(0,-4)) && !map.IsVisited(MapRegion.Tavern,game.journey.keeper.position),"tavern exploration is independent from exterior");
            Check(map.IsVisited(MapRegion.Coast,start) && map.IsVisited(MapRegion.Coast,distant),"coast discoveries survive area changes and chunk unloading");
            game.journey.SetArea(WorldArea.Coast,start);meadow.Refresh(start,true);game.ResetCamera(new Vector3(-1.9f,.55f,-20));
        }
        IEnumerator SurfChecks()
        {
            game.cameraView.Render();var a=ReadPixels();
            Shader.SetGlobalFloat("_CoastClock",22.4f);game.cameraView.Render();var b=ReadPixels();
            float delta=RegionDifference(a,b,new Rect(-9,-7.2f,18,3.1f));
            Check(delta>.012f,"surf breakers and foam visibly advance across shoreline pixels ("+delta.ToString("F3")+")");Destroy(a);Destroy(b);
            game.seconds=22.4f;game.UpdateEnvironment(0);yield return Capture("02d-animated-surf.png");
            game.seconds=20;game.UpdateEnvironment(0);
            Vector2 click=Vector2.zero;bool found=false;
            for(float x=game.ship.sprite.bounds.max.x-.1f;x>game.ship.sprite.bounds.max.x-4 && !found;x-=.1f)
                for(float y=.4f;y<2.5f && !found;y+=.1f)if(game.art.Opaque(game.ship.sprite,new Vector2(x,y))){click=game.ship.transform.TransformPoint(new Vector2(x,y));found=true;}
            Check(found && game.TryOpenShipAt(click) && game.ui.shipOpen && !game.ui.bagOpen,"right-click world ship route opens only hold in reach");
            game.ui.open=game.ui.shipOpen=false;
            Check(!game.TryOpenShipAt(new Vector2(25,20)) && !game.ui.open,"right-click empty world does not open storage");
        }
        IEnumerator JourneyChecks()
        {
            var j=game.journey;var ui=game.ui;var a=game.arcane;var m=game.magic;
            ui.CloseContainers();ui.ClearCheckPointer();m.CancelStroke();game.combat.ResetForCheck();game.combat.manual=true;
            foreach(var flame in a.flames)Destroy(flame.renderer.gameObject);a.flames.Clear();
            foreach(var field in a.fields)field.Dispose();a.fields.Clear();
            game.survivor=new Survivor();var s=game.survivor;s.mana=100;game.seconds=30;
            j.SetArea(WorldArea.Coast,j.CartPosition+new Vector2(0,-1));game.UpdateEnvironment(0);
            Check(s.Coins==10 && s.inventory.items.Single(i=>i.kind==ItemKind.Coin).count==10,"ten silver coins start in one stack");
            Check(j.pullers.Count==2 && j.pullers.All(p=>p.sprite!=null) && j.driver.sprite!=null,"human driver and two separately animated harnessed woodlice");
            Check(j.CanInteractAt(j.wagon.bounds.center),"passing wagon can be right clicked while approached");
            yield return Capture("v10-wagon-on-beach.png");
            Check(game.UseWorldRightClick(j.wagon.bounds.center) && j.Cutscene && !game.body.simulated && game.MovementLocked,"right click boards cart and starts controlled travel cutscene");
            for(int i=0;i<16;i++)j.Tick(.1f);yield return Capture("v10-wagon-departure.png");
            Check(j.RidePhase==2,"boarding animation precedes departure");
            for(int i=0;i<40;i++)j.Tick(.1f);
            Check(j.BlackFade>.95f && j.RidePhase==3,"cart departs beyond screen before black transition");yield return Capture("v10-travel-black-screen.png");
            for(int i=0;i<10;i++)j.Tick(.1f);
            Check(j.Area==WorldArea.GoldenField && j.RidePhase==4 && !game.coastRoot.activeSelf && j.fieldRoot.activeSelf,"black transition loads golden meadow and incoming cart");
            for(int i=0;i<50;i++)j.Tick(.1f);
            Check(!j.Cutscene && game.body.simulated && !game.MovementLocked && Vector2.Distance(game.body.position,j.CartPosition)<2,"arrival finishes with player dismounting and movement restored");
            Check(j.trees.Count>=8 && j.trees.All(t=>t.sprite.bounds.size.y>7) && j.inn!=null,"golden meadow has sparse very tall poplars and one tavern");
            game.ResetCamera(new Vector3(0,3,-20));yield return Capture("v10-golden-field-and-tavern.png");
            var tree=j.trees[0];game.body.position=tree.position+Vector2.up;game.hero.transform.position=game.body.position;j.Tick(.3f);
            Check(tree.alpha<.4f,"poplar becomes translucent when player is behind crown");
            var wind=new MaterialPropertyBlock();tree.canopy.GetPropertyBlock(wind);Check(wind.GetFloat("_WindStrength")>.1f && tree.canopy.GetComponent<MeshFilter>().sharedMesh.vertexCount==18,"poplar bends with subdivided anchored wind mesh");
            game.body.position=tree.position+Vector2.down;game.hero.transform.position=game.body.position;j.Tick(.4f);Check(tree.alpha>.95f,"poplar opacity restores after leaving overlap");
            game.body.position=j.InnEntrance+Vector2.down*.6f;game.hero.transform.position=game.body.position;
            Check(j.Interact(j.InnEntrance+Vector2.up*.4f) && j.Interior,"right click tavern entrance opens compact interior");
            Check(!j.fieldRoot.activeSelf && j.tavernRoot.activeSelf && !game.coastRoot.activeSelf,"interior disables exterior and beach colliders");
            Check(j.actors.Count==7 && j.actors.Any(n=>n.row==6) && j.fireplace!=null,"tavern contains dancer keeper sleeper mage conversing pair and pantry girl");
            Check(j.tavernRoot.GetComponentsInChildren<SpriteRenderer>().Count(r=>r.gameObject.name=="Round oak table")==4,"four round tavern tables");
            Check(j.tavernRoot.GetComponentsInChildren<SpriteRenderer>().Any(r=>r.name.Contains("Bear porcupine")),"bear porcupine hide lies on tavern floor");
            j.clock=4.3f;j.Tick(.01f);int drank=j.drinker.frame;j.clock=5;j.Tick(.01f);Check(drank!=j.drinker.frame,"seated mage drinks from mug on five-second cycle");
            int keeperFrame=j.keeper.frame;j.Tick(.2f);Check(j.keeper.frame!=keeperFrame,"innkeeper cloth moves across plate");
            game.ResetCamera(new Vector3(.5f,.35f,-20));yield return Capture("v10-tavern-interior.png");
            yield return TavernLayoutChecks();
            yield return TavernAnimationChecks();
            game.body.position=j.keeper.position+Vector2.down*2;game.hero.transform.position=game.body.position;
            Check(game.UseWorldRightClick(j.keeper.sprite.bounds.center) && ui.vendorOpen && ui.open,"right click nearby innkeeper opens trade and player inventory");
            yield return Capture("v10-tavern-trade.png");
            Check(ui.VendorPanel.x==0 && ui.VendorPanel.width==ui.PanelWidth && ui.grids.Any(v=>v.inventory==j.seller),"seller occupies left third opposite player inventory");
            var pie=j.seller.items.First(i=>i.kind==ItemKind.Pie);int balance=s.Coins;yield return Pointer(Cell(j.seller,pie.x,pie.y),EventType.MouseDown,0);yield return Pointer(Cell(s.inventory,8,4),EventType.MouseDown,0);
            Check(s.inventory.items.Contains(pie) && s.Coins==balance-2 && !ui.Dragging,"dragging pie into player inventory charges two coins once");
            yield return Pointer(Cell(s.inventory,pie.x,pie.y),EventType.MouseDown,0);yield return Pointer(Cell(j.seller,0,0),EventType.MouseDown,0);
            Check(ui.Dragging && s.inventory.items.Contains(pie) && !j.seller.items.Contains(pie) && s.Coins==8,"dragging purchased food back to seller is refused");ui.CancelInteraction();
            var stock=j.seller.items.First(i=>i.kind==ItemKind.BeetleMeat);var coin=s.inventory.items.Single(i=>i.kind==ItemKind.Coin);coin.count=0;
            yield return Pointer(Cell(j.seller,stock.x,stock.y),EventType.MouseDown,0);yield return Pointer(Cell(s.inventory,9,3),EventType.MouseDown,0);
            Check(ui.Dragging && j.seller.items.Contains(stock) && !s.inventory.items.Contains(stock),"unfunded purchase retains seller item without duplication");ui.CancelInteraction();coin.count=8;ui.CloseContainers();
            game.body.position=j.drinker.position+Vector2.down*1.5f;game.hero.transform.position=game.body.position;
            Check(game.UseWorldRightClick(j.drinker.sprite.bounds.center) && j.HireOffer && !j.companion.Hired,"nearby mage offers hire before taking payment");yield return Capture("v10-hire-mage-offer.png");
            Check(j.HireMage() && j.companion.Hired && s.Coins==4 && !j.drinker.sprite.enabled,"hiring costs four silver coins and replaces seated mage with companion");
            Check(!j.HireMage() && s.Coins==4,"mage cannot be hired or charged twice");
            var mageAt=j.companion.Position;game.cameraView.orthographicSize=2.5f;game.cameraView.transform.position=(Vector3)mageAt+new Vector3(0,.8f,-20);
            for(int d=0;d<4;d++)
            {
                var forward=d==0?Vector2.down:d==1?Vector2.left:d==2?Vector2.up:Vector2.right;j.companion.sprite.sprite=j.art.mage[d,3];game.art.PixelStyle(j.companion.sprite);
                j.companion.gait.Pose(d,.25f,true,forward);game.cameraView.Render();var left=ReadPixels();File.WriteAllBytes(Path.Combine(directory,"v12-mage-step-"+d+"-left.png"),left.EncodeToPNG());
                j.companion.gait.Pose(d,.75f,true,forward);game.cameraView.Render();var right=ReadPixels();File.WriteAllBytes(Path.Combine(directory,"v12-mage-step-"+d+"-right.png"),right.EncodeToPNG());
                float difference=RegionDifference(left,right,new Rect(mageAt.x-.8f,mageAt.y-.3f,1.6f,.9f));Check(difference>.002f,"rendered companion leg silhouettes differ between opposite passing poses in direction "+d+" ("+difference.ToString("F4")+")");Destroy(left);Destroy(right);
            }
            j.companion.gait.Pose(0,0,false,Vector2.zero);game.cameraView.orthographicSize=6.25f;
            var oldPosition=j.companion.Position;game.body.position=new Vector2(3.2f,-4.2f);game.hero.transform.position=game.body.position;var frames=new HashSet<int>();
            for(int i=0;i<30;i++){j.companion.Tick(.05f);frames.Add(j.companion.frame);}
            Check(Vector2.Distance(oldPosition,j.companion.Position)>.5f && frames.Count>=5,"companion follows player through several walking poses");yield return Capture("v10-companion-in-tavern.png");
            game.body.position=new Vector2(6.0f,2.50f);game.hero.transform.position=game.body.position;j.companion.Warp(new Vector2(3.0f,3.10f));Physics2D.SyncTransforms();for(int i=0;i<140;i++)j.companion.Tick(.05f);
            Check(j.companion.Position.x>4.35f && Vector2.Distance(j.companion.Position,game.body.position)<1.7f,"companion reaches pantry through opening instead of stopping across a solid wall ("+j.companion.Position.ToString("F2")+")");
            game.body.position=new Vector2(3.2f,-4.2f);game.hero.transform.position=game.body.position;j.companion.Warp(new Vector2(2.2f,-3.5f));
            Check(j.companion.Explode() && j.companion.mana==60 && a.explosions.Count>0,"companion command creates shared pyromantic blast and costs forty mana");
            a.Tick(.32f);game.UpdateEnvironment(0);yield return Capture("v10-companion-explosion.png");Check(j.companion.Explode() && j.companion.mana==20 && !j.companion.Explode(),"companion cannot command explosion with fewer than forty mana");a.Tick(2);
            j.SetArea(WorldArea.Coast,new Vector2(4,5));j.companion.Warp(new Vector2(5,6));j.companion.mana=100;var frog=a.frogs[0];frog.Reset();frog.Warp(new Vector2(9,6));frog.Hurt(1);j.companion.Engage(frog.renderer);j.companion.Tick(.2f);
            Check(j.companion.Firing && j.companion.mana<100 && a.flames.Any(f=>f.targetOnly==frog.renderer && f.reach==7),"companion assists combat with seven-unit fire cone and mana expenditure");
            var shots=a.flames.Where(f=>f.targetOnly==frog.renderer).ToList();Vector2 aim=(Vector2)frog.renderer.bounds.center-j.companion.Position-Vector2.up*.8f;
            Check(shots.All(f=>Vector2.Angle(f.velocity,aim)<=22.6f),"companion flame cone is forty five degrees");
            foreach(var f in a.flames)Destroy(f.renderer.gameObject);a.flames.Clear();frog.Reset();j.companion.target=null;
            var lamp=s.inventory.items.First(i=>i.kind==ItemKind.Lamp);s.AutoEquipEmpty(lamp,s.inventory);s.mana=100;lamp.fuel=100;float durability=lamp.durability;
            Check(a.UseLamp(1) && Mathf.Abs(s.mana-97)<.01f && Mathf.Abs(lamp.fuel-96.8f)<.01f && lamp.durability<durability && durability-lamp.durability<.1f,"lamp flame consumes mana and fuel much faster than durability");
            lamp.fuel=0;Check(!a.FireLampBurst(Vector2.right),"empty lamp cannot produce fire");lamp.fuel=100;s.mana=0;Check(!a.FireLampBurst(Vector2.right),"lamp cannot shoot without mana");s.mana=100;
            s.inventory.Add(new Item(ItemKind.Wood));var wood=s.inventory.items.First(i=>i.kind==ItemKind.Wood);lamp.fuel=10;ui.open=true;yield return Capture("v10-lamp-fuel-and-durability.png");yield return Pointer(Cell(s.inventory,wood.x,wood.y),EventType.MouseDown,0);yield return Pointer(ui.GearRect(GearSlot.Hand).center,EventType.MouseDown,0);
            Check(lamp.fuel==60 && !s.inventory.items.Contains(wood) && !ui.Dragging,"wood held on cursor refuels equipped lamp by clicking it");
            var durabilityArea=InventoryUI.ItemIconArea(lamp,ui.GearRect(GearSlot.Hand).center);Check(durabilityArea.width==InventoryUI.ItemIconArea(lamp,Vector2.zero).width,"equipment icon scale remains identical when moved to gear");
            var dragged=s.inventory.items.First(i=>i.kind==ItemKind.YoYo);yield return Pointer(Cell(s.inventory,dragged.x,dragged.y),EventType.MouseDown,0);game.checkMovement=Vector2.right;var dragStart=game.body.position;yield return new WaitForSecondsRealtime(.2f);game.checkMovement=null;
            Check(ui.Dragging && Vector2.Distance(game.body.position,dragStart)>.3f,"inventory drag keeps character movement available");ui.CancelInteraction();
            ui.CloseContainers();game.body.position=new Vector2(15,8);game.hero.transform.position=game.body.position;s.inventory.Add(new Item(ItemKind.Campfire));var fireItem=s.inventory.items.First(i=>i.kind==ItemKind.Campfire);
            Check(a.BeginPlaceCampfire(fireItem,s.inventory) && !game.MovementLocked,"campfire preview allows movement");game.checkMovement=Vector2.right;var before=game.body.position;yield return new WaitForSecondsRealtime(.2f);game.checkMovement=null;
            Check(Vector2.Distance(game.body.position,before)>.3f,"player moves while placing world-cell campfire");yield return Capture("v10-world-grid-placement.png");
            var firePoint=game.body.position+new Vector2(1,-.2f);Check(a.PlacePendingFire(firePoint),"campfire can be placed on a free world cell");var placed=a.campfires.Last();Check(placed.position==WorldGrid.Snap(firePoint) && placed.logs.bounds.size.x<=1 && placed.collider!=null,"campfire occupies one snapped world cell with matching footprint");
            Check(!a.CanPlaceFire(placed.position),"occupied campfire world cell rejects another fire");
            var pyro=s.inventory.items.First(i=>i.kind==ItemKind.Pyromancy);pyro.opened=true;s.mana=9;Check(!a.TryExplodeFire(placed.position) && !placed.exploded && s.mana==9,"unpaid campfire blast leaves placed fire intact");s.mana=100;Check(a.TryExplodeFire(placed.position) && s.mana==90 && !placed.collider.enabled,"campfire blast costs ten mana and removes cell collision");a.Tick(2);
            Check(InventoryUI.ItemIconArea(new Item(ItemKind.Orb),Vector2.zero).width<50,"sphere icon is smaller while retaining two by two footprint");
            Check(ItemDefinition.All[ItemKind.Pyromancy].width==1 && InventoryUI.ItemIconArea(new Item(ItemKind.Pyromancy),Vector2.zero).width>30,"fire book sprite grows without changing inventory footprint");
            game.shipStorage=CoastTime.CreateShipStorage();Check(game.shipStorage.items.Count(i=>i.kind==ItemKind.Wood)==5,"ship now has five separate logs");
            j.clock=47;j.Tick(.1f);Check(j.gulls.Count==4 && j.gulls.Any(g=>g.sprite.enabled) && game.soundscape.gullCalls.All(c=>c!=null && c.length>2),"four-wing gull landing and revised natural-rasp sounds are present");
            j.SetArea(WorldArea.Tavern,new Vector2(0,-3));s.equipment[0]=new Item(ItemKind.Orb);s.inventory.items.Clear();s.inventory.Add(new Item(ItemKind.Tome){opened=true});s.mana=19;
            var star=SpatialSigil.Resample(StarSigil.Template,120).Select(p=>p*240+new Vector2(400,230)).ToList();m.BeginStroke(star[0]);foreach(var p in star)m.AddPoint(p);Check(!m.EndStroke() && !m.Casting && s.mana==19,"pocket cast requires twenty mana before teleport");m.CancelStroke();s.mana=100;
            m.BeginStroke(star[0]);foreach(var p in star)m.AddPoint(p);Check(m.EndStroke() && s.mana==80,"pocket star cast consumes twenty mana");m.CancelStroke();var tavernPosition=game.body.position;m.SwitchDimension();Check(!j.tavernRoot.activeSelf && !game.coastRoot.activeSelf && m.pocket.activeSelf,"pocket entered from tavern hides the correct world");m.SwitchDimension();Check(j.Interior && j.tavernRoot.activeSelf && game.body.position==tavernPosition,"pocket exit returns to same tavern position");
            yield return new WaitForSecondsRealtime(1.3f);s.inventory.items.Clear();s.inventory.Add(new Item(ItemKind.AbyssTome){opened=true});s.mana=100;m.CancelStroke();var rune=SpatialSigil.Resample(SpatialSigil.Template,120).Select(p=>p*240+new Vector2(400,230)).ToList();m.BeginStroke(rune[0]);foreach(var p in rune)m.AddPoint(p);Check(m.EndStroke() && s.mana==70 && a.fields.Last().radius>=5.6f,"tentacles cost thirty mana and have doubled latest radius");foreach(var f in a.fields)f.Dispose();a.fields.Clear();m.CancelStroke();
            ui.open=true;yield return Capture("v10-fullscreen-inventory.png");ui.SetFullscreen(true);yield return new WaitForSecondsRealtime(.3f);Check(Screen.fullScreen,"settings can enable borderless fullscreen");ui.SetFullscreen(false);yield return new WaitForSecondsRealtime(.3f);Check(!Screen.fullScreen,"settings can restore windowed mode");
            ui.CloseContainers();j.SetArea(WorldArea.Coast,new Vector2(-1.5f,-1.65f));
        }
        IEnumerator TavernLayoutChecks()
        {
            var j=game.journey;
            Check(j.actors.Where(a=>a.row>=2 && a.row<=6).All(a=>{var block=new MaterialPropertyBlock();a.sprite.GetPropertyBlock(block);return block.GetFloat("_BodyFloor")==0;}),"seated bodies retain full legs and use furniture occlusion instead of shader amputation");
            Check(j.tavernRoot.GetComponentsInChildren<SpriteRenderer>().Count(r=>r.name.StartsWith("Occupied oak chair"))==5,"all five seated visitors have an actual chair");
            Check(!game.art.Opaque(j.exitDoor.sprite,j.exitDoor.transform.InverseTransformPoint(j.exitDoor.bounds.center)),"tavern doorway opening is transparent rather than filled by a door slab");
            Check(j.tavernRoot.GetComponentsInChildren<Collider2D>().Any(c=>c.name=="Preparation table footprint"),"pantry has a preparation table with matching solid footprint");
            game.body.position=new Vector2(3.1f,.65f);game.hero.transform.position=game.body.position;Physics2D.SyncTransforms();game.checkMovement=Vector2.right;yield return new WaitForSecondsRealtime(.65f);game.checkMovement=null;
            Check(game.body.position.x>4.6f,"player can walk through the pantry opening");
            game.body.position=new Vector2(3.15f,3.0f);game.hero.transform.position=game.body.position;Physics2D.SyncTransforms();game.checkMovement=Vector2.right;yield return new WaitForSecondsRealtime(.35f);game.checkMovement=null;
            Check(game.body.position.x<3.8f,"continuous pantry wall blocks walking outside its doorway");
            game.body.position=new Vector2(0,-3.75f);game.hero.transform.position=game.body.position;j.clock=1.2f;j.Tick(0);game.ResetCamera(new Vector3(.5f,.35f,-20));game.UpdateEnvironment(0);yield return Capture("v11-tavern-complete-interior.png");
            j.clock=4.4f;j.Tick(0);yield return Capture("v11-tavern-drinking-and-preparation.png");
            game.cameraView.orthographicSize=4;game.ResetCamera(new Vector3(-3.4f,-.10f,-20));yield return Capture("v11-seated-visitors-detail.png");game.cameraView.orthographicSize=6.25f;
        }
        IEnumerator FlowerWindChecks()
        {
            game.seconds=35;game.UpdateEnvironment(0);game.cameraView.Render();var a=ReadPixels();
            Shader.SetGlobalFloat("_WindTime",37.7f);game.cameraView.Render();var b=ReadPixels();
            float delta=RegionDifference(a,b,new Rect(game.hero.transform.position.x-5,game.hero.transform.position.y,10,6));
            Check(delta>.0008f,"flower upper stems visibly bend while terrain and lighting stay fixed ("+delta.ToString("F4")+")");
            Check(game.flowers.Where(f=>!f.picked).All(f=>f.renderer.transform.position==(Vector3)f.position && f.renderer.transform.rotation==Quaternion.identity),"wind leaves roots and shadows anchored instead of rotating whole bouquets");Destroy(a);Destroy(b);
            game.seconds=37.7f;game.UpdateEnvironment(0);yield return Capture("02e-flower-wind.png");game.seconds=20;game.UpdateEnvironment(0);
        }
        IEnumerator WoodlouseChecks()
        {
            var mob=game.woodlice.Single(m=>!m.Small);var home=mob.Position;var s=game.survivor;
            game.body.position=home+new Vector2(-2,-.1f);game.hero.transform.position=game.body.position;
            game.cameraView.orthographicSize=4;game.ResetCamera(game.hero.transform.position+new Vector3(.8f,1.8f,-20));game.ui.open=false;
            Check(mob.vitals.health==200 && !mob.vitals.Curled,"harmless woodlouse starts with 200 HP");
            yield return Capture("woodlouse-walking-0.png");var oldFrame=mob.frame;float health=s.health;
            mob.Tick(2);Check(mob.frame!=oldFrame && Vector2.Distance(mob.Position,home)>.1f && s.health==health,"woodlouse wanders with walking frames and never damages player");
            yield return Capture("woodlouse-walking-1.png");mob.renderer.transform.position=home;
            var kinds=new[]{ItemKind.Sword,ItemKind.Dagger,ItemKind.Scythe,ItemKind.Spear};float[] reach={2.1f,1.5f,2.0f,3.2f};
            for(int k=0;k<kinds.Length;k++)
            {
                mob.vitals.health=200;mob.vitals.firstHit=false;mob.vitals.curlRemaining=0;mob.Tick(0,false);
                game.body.position=home+new Vector2(-reach[k],0);game.hero.transform.position=game.body.position;
                s.equipment[0]=new Item(kinds[k]);game.numbers.Clear();game.combat.ResetForCheck();game.combat.Begin(Vector2.right);
                game.combat.Advance(.08f);Check(mob.vitals.health==200,kinds[k]+" woodlouse has no windup damage");
                for(int step=9;step<=90;step++)game.combat.Advance(step/100f);
                float damage=ItemDefinition.All[kinds[k]].damage;
                Check(mob.vitals.health==200-damage && game.numbers.Count==1,kinds[k]+" contacts actual woodlouse texture once and triggers first-hit curl");
                Check(mob.vitals.Curled && mob.vitals.curlRemaining==5,"first contact starts exactly five seconds for "+kinds[k]);
                mob.Tick(.46f,false);Check(mob.frame>=24,"woodlouse closes into complete armored ball for "+kinds[k]);
                float timer=mob.vitals.curlRemaining;game.combat.ResetForCheck();game.combat.Begin(Vector2.right);
                for(int step=1;step<=90;step++)game.combat.Advance(step/100f);
                Check(Mathf.Abs(mob.vitals.health-(200-damage-damage*.2f))<.001f && game.numbers.Count==2,kinds[k]+" curled armor absorbs exactly 80 percent on textured contact");
                Check(mob.vitals.curlRemaining==timer,"additional hit does not extend five-second defense for "+kinds[k]);
                if(k==0){mob.Tick(.20f,false);yield return Capture("woodlouse-curled.png");}
                mob.Tick(mob.vitals.curlRemaining-.02f,false);Check(mob.vitals.Curled,"armor stays active up to five-second boundary for "+kinds[k]);
                mob.Tick(.021f,false);Check(!mob.vitals.Curled && mob.frame<16,"woodlouse unrolls when five seconds expire for "+kinds[k]);
            }
            game.combat.ResetForCheck();s.equipment[0]=null;game.numbers.Clear();
            float hp=mob.vitals.health;Check(mob.Hurt(24)==24 && mob.vitals.health==hp-24 && mob.vitals.Curled,"another hit after unfolding starts a new five-second defensive reaction");
            game.body.position=home+new Vector2(-6,0);game.hero.transform.position=game.body.position;Check(!game.TryCollectLootAt(home),"no meat can be collected before animal dies");
            mob.vitals.health=4;int count=game.loot.Count;mob.Hurt(24);
            Check(!mob.vitals.Alive && !mob.renderer.enabled && !mob.feet.enabled && game.loot.Count==count+1,"woodlouse death disables contact and drops one beetle meat");
            mob.Hurt(24);Check(game.loot.Count==count+1,"dead woodlouse cannot duplicate loot");
            var drop=game.loot.Last();Check(!game.CollectLoot(drop) && !drop.collected,"drop cannot be gathered outside reach");
            game.body.position=home+new Vector2(-.65f,0);game.hero.transform.position=game.body.position;game.numbers.Clear();
            s.inventory.items.Clear();s.Bag.items.Clear();for(int i=0;i<50;i++)s.inventory.Add(new Item(ItemKind.Brain));for(int i=0;i<25;i++)s.Bag.Add(new Item(ItemKind.Brain));
            Check(!game.CollectLoot(drop) && !drop.collected && drop.renderer.enabled && s.inventory.items.Count==50 && s.Bag.items.Count==25,"full inventories leave original meat on ground without consuming or duplicating it");s.inventory.items.Clear();s.Bag.items.Clear();game.ui.Notice(null);
            game.ResetCamera(game.hero.transform.position+new Vector3(0,1.5f,-20));yield return Capture("woodlouse-meat-drop.png");
            s.inventory.items.Clear();Check(game.TryCollectLootAt(drop.renderer.bounds.center) && drop.collected && !drop.renderer.enabled && s.inventory.items.Contains(drop.item),"clicking fallen meat gathers the original unstacked item");
            Check(!game.CollectLoot(drop) && s.inventory.items.Count==1,"collected meat cannot duplicate");
            mob.vitals.health=200;mob.vitals.firstHit=false;mob.vitals.curlRemaining=0;mob.renderer.enabled=mob.shadow.enabled=mob.feet.enabled=true;mob.Tick(0,false);
            s.inventory.items.Clear();game.numbers.Clear();game.cameraView.orthographicSize=8;
        }
        float RegionDifference(Texture2D a,Texture2D b,Rect region)
        {
            var low=game.cameraView.WorldToViewportPoint(region.min);var high=game.cameraView.WorldToViewportPoint(region.max);
            int x0=Mathf.Clamp((int)(low.x*a.width),0,a.width-1),x1=Mathf.Clamp((int)(high.x*a.width),x0+1,a.width);
            int y0=Mathf.Clamp((int)(low.y*a.height),0,a.height-1),y1=Mathf.Clamp((int)(high.y*a.height),y0+1,a.height);
            float sum=0;for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++){var ca=a.GetPixel(x,y);var cb=b.GetPixel(x,y);sum+=(Mathf.Abs(ca.r-cb.r)+Mathf.Abs(ca.g-cb.g)+Mathf.Abs(ca.b-cb.b))/3;}
            return sum/((x1-x0)*(y1-y0));
        }
        IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name));yield return null;yield return new WaitForEndOfFrame();
        }
        IEnumerator Pointer(Vector2 position,EventType type,int button,bool shift=false,int clicks=1)
        {
            game.ui.QueuePointerForCheck(position,type,button,shift,clicks);yield return new WaitForEndOfFrame();yield return null;yield return new WaitForEndOfFrame();
        }
        Vector2 Gear(GearSlot slot)=>game.ui.GearRect(slot).center;
        Vector2 Cell(GridInventory grid,int x,int y)=>game.ui.CellCenter(grid,x,y);
        IEnumerator InventoryInteractions()
        {
            var s=game.survivor;var ui=game.ui;
            ui.open=false;ui.Toggle();yield return null;yield return new WaitForEndOfFrame();
            Check(ui.grids.Count==1 && !ui.bagOpen && !ui.shipOpen,"E opens only main inventory and gear");
            yield return Pointer(Gear(GearSlot.Back),EventType.MouseDown,1);
            yield return Pointer(Gear(GearSlot.Back),EventType.MouseUp,1);
            Check(ui.bagOpen && !ui.IsCrafting && !ui.Dragging && ui.grids.Any(g=>g.inventory==s.Bag) && ui.BagPanel.xMax<ui.PanelX,"right click sack opens independent window to the left without picking it up");
            s.inventory.items.Clear();s.inventory.Place(new Item(ItemKind.BlueFlower),0,0);s.inventory.Place(new Item(ItemKind.WhiteFlower),1,0);
            yield return Pointer(Cell(s.inventory,0,0),EventType.MouseDown,1);
            yield return Pointer(Cell(s.inventory,1,1),EventType.MouseDrag,1);
            Check(ui.IsCrafting,"right mouse begins hammer selection");yield return Capture("05-crafting-hammer.png");
            yield return Pointer(Cell(s.inventory,1,1),EventType.MouseUp,1);
            Check(!ui.IsCrafting && s.inventory.items.Count==1 && s.inventory.items[0].kind==ItemKind.Rope,"release right mouse crafts 2x1 rope using both flowers");
            var rope=s.inventory.items[0];
            yield return Pointer(Cell(s.inventory,0,0),EventType.MouseDown,0);
            yield return Pointer(Cell(s.inventory,0,0),EventType.MouseUp,0);
            Check(ui.Dragging && s.inventory.items.Contains(rope),"single left click picks item up without holding button or removing source");
            yield return Pointer(Cell(s.Bag,0,0),EventType.MouseDown,0);
            yield return Pointer(Cell(s.Bag,0,0),EventType.MouseUp,0);
            Check(!ui.Dragging && s.Bag.items.Contains(rope) && !s.inventory.items.Contains(rope),"second left click places item in separate sack without duplication");
            ui.OpenShip();yield return null;yield return new WaitForEndOfFrame();
            Check(ui.shipOpen && ui.ShipPanel.xMax<ui.PanelX && ui.grids.Any(g=>g.inventory==game.shipStorage),"ship storage opens independently to left of main inventory");
            yield return Pointer(Cell(game.shipStorage,0,2),EventType.MouseDown,0,true);
            var sword=s.inventory.items.FirstOrDefault(i=>i.kind==ItemKind.Sword);Check(sword!=null,"shift click transfers sword from separate ship window");
            yield return Pointer(Cell(s.inventory,sword.x,sword.y),EventType.MouseDown,0,false,2);
            Check(s.equipment[0]==sword && !s.inventory.items.Contains(sword),"double click equips sword from inventory");
            game.combat.manual=false;yield return null;yield return null;
            Check(!game.combat.weapon.enabled,"equipped melee weapon hidden while idle");game.combat.manual=true;
            Check(Enumerable.Range(0,8).All(i=>ui.GearRect((GearSlot)i).width>=90 && ui.GearRect((GearSlot)i).height>=80),"all eight equipment slots are enlarged");
            var boots=new Item(ItemKind.Boots);s.inventory.Add(boots);
            yield return Pointer(Cell(s.inventory,boots.x,boots.y),EventType.MouseDown,0);
            yield return Pointer(Cell(s.inventory,boots.x,boots.y),EventType.MouseUp,0);
            yield return Pointer(Gear(GearSlot.Feet),EventType.MouseDown,0);
            yield return Pointer(Gear(GearSlot.Feet),EventType.MouseUp,0);
            Check(s.equipment[(int)GearSlot.Feet]==boots && !s.inventory.items.Contains(boots),"click pickup and click placement equips footwear in new slot");
            var snack=new Item(ItemKind.Brain);s.inventory.Add(snack);int foodCount=s.foods.Count;
            yield return Pointer(Cell(s.inventory,snack.x,snack.y),EventType.MouseDown,0);
            yield return Pointer(Cell(s.inventory,snack.x,snack.y),EventType.MouseUp,0);
            Check(ui.Dragging && s.inventory.items.Contains(snack) && s.foods.Count==foodCount,"left click on food only picks it up");
            yield return Pointer(Cell(s.inventory,snack.x,snack.y),EventType.MouseDown,1);
            Check(!ui.Dragging && s.inventory.items.Contains(snack),"right click cancels item pickup safely");
            yield return Pointer(Cell(s.inventory,snack.x,snack.y),EventType.MouseDown,1);
            yield return Pointer(Cell(s.inventory,snack.x,snack.y),EventType.MouseUp,1);
            Check(!ui.IsCrafting && !s.inventory.items.Contains(snack) && s.mana==100,"right click eats food without starting crafting");
            ui.shipOpen=false;yield return null;var bag=s.equipment[(int)GearSlot.Back];
            yield return Pointer(Gear(GearSlot.Back),EventType.MouseDown,0);
            yield return Pointer(Cell(s.inventory,0,0),EventType.MouseUp,0);
            Check(s.Bag==null && !ui.bagOpen && s.inventory.items.Contains(bag) && bag.contents.items.Contains(rope),"remove sack, close its window, retain contents");
            yield return Pointer(Cell(s.inventory,bag.x,bag.y),EventType.MouseDown,0,false,2);
            Check(s.equipment[(int)GearSlot.Back]==bag && s.Bag.items.Contains(rope) && !ui.bagOpen,"reequip sack without automatically opening its slots");
            yield return Pointer(Gear(GearSlot.Back),EventType.MouseDown,1);
            yield return Pointer(Gear(GearSlot.Hand),EventType.MouseDown,0);
            yield return Pointer(Gear(GearSlot.Head),EventType.MouseUp,0);
            Check(s.equipment[0]==sword && s.equipment[1]==null,"invalid equipment drop preserves weapon");
            yield return Pointer(Cell(s.Bag,0,1),EventType.MouseDown,0);
            yield return Pointer(Cell(s.Bag,0,1),EventType.MouseUp,0);
            Check(s.equipment[0]==null && s.Bag.items.Contains(sword),"unequip sword into separate bag window");
            yield return Pointer(Cell(s.Bag,sword.x,sword.y),EventType.MouseDown,0,false,2);
            Check(s.equipment[0]==sword && !s.Bag.items.Contains(sword),"equip sword from separately opened bag");
            ui.OpenShip();yield return null;yield return new WaitForEndOfFrame();
            foreach(var kind in new[]{ItemKind.Brain,ItemKind.Soup,ItemKind.BeetleMeat})
            {
                var food=game.shipStorage.items.First(i=>i.kind==kind);
                yield return Pointer(Cell(game.shipStorage,food.x,food.y),EventType.MouseDown,1);
                yield return Pointer(Cell(game.shipStorage,food.x,food.y),EventType.MouseUp,1);
                Check(!game.shipStorage.items.Contains(food) && !ui.IsCrafting,"right click consumes "+kind+" from storage");
            }
            Check(s.mana==100 && s.foods.Count==2 && s.HungerMax==120,"food HUD: brain restores mana, two dishes occupy satiety cells");
            s.inventory.Add(new Item(ItemKind.BlueFlower));s.inventory.Add(new Item(ItemKind.PinkFlower));
            game.combat.manual=false;yield return Capture("06-inventory-bag-food.png");game.combat.manual=true;
            yield return Pointer(Gear(GearSlot.Hand),EventType.Repaint,0);yield return Capture("06b-vertical-weapon-detail.png");
        }
        IEnumerator ScreenChecks()
        {
            var ui=game.ui;var s=game.survivor;s.mana=100;s.inventory.items.Clear();ui.bagOpen=ui.shipOpen=false;ui.ClearCheckPointer();
            var kinds=new[]{ItemKind.Sword,ItemKind.Dagger,ItemKind.Scythe,ItemKind.Spear};int[] columns={0,2,4,7};
            for(int i=0;i<4;i++)s.inventory.Place(new Item(kinds[i]),columns[i],0);
            var book=new Item(ItemKind.Tome){opened=true};s.inventory.Place(book,0,3);
            var sizes=new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(Screen.currentResolution.width,Screen.currentResolution.height)};
            for(int size=0;size<sizes.Length;size++)
            {
                var resolution=sizes[size];Screen.SetResolution(resolution.x,resolution.y,size==2?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(.8f);yield return new WaitForEndOfFrame();
                Check(Mathf.Abs(game.cameraView.aspect-(float)Screen.width/Screen.height)<.002f,"camera retains screen aspect at size "+size);
                Check(ui.MainX>ui.PanelX && (ui.MainX+420)*ui.UIScale<Screen.width,"main inventory stays inside right third at size "+size);
                var image=ScreenCapture.CaptureScreenshotAsTexture();
                foreach(var item in s.inventory.items.Where(i=>i.Definition.Weapon))
                {
                    var def=item.Definition;int bright=0;
                    int x0=Mathf.RoundToInt((ui.MainX+item.x*42+8)*ui.UIScale),x1=Mathf.RoundToInt((ui.MainX+(item.x+def.width)*42-8)*ui.UIScale);
                    int y0=Mathf.RoundToInt((InventoryUI.MainGridY+item.y*42+8)*ui.UIScale),y1=Mathf.RoundToInt((InventoryUI.MainGridY+(item.y+def.height)*42-8)*ui.UIScale);
                    for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++){var c=image.GetPixel(x,image.height-1-y);if(Mathf.Max(c.r,Mathf.Max(c.g,c.b))>.32f)bright++;}
                    Check(bright>16,item.kind+" texture visibly occupies its inventory cells at size "+size);
                }
                Destroy(image);yield return Capture("screen-"+size+"-weapons.png");
                ui.ToggleBag();ui.OpenShip();yield return null;yield return new WaitForEndOfFrame();
                Check(ui.grids.Count==3 && ui.ShipPanel.x>=0 && ui.BagPanel.yMax<=ui.CanvasHeight,"independent storage windows fit screen at size "+size);
                var sword=s.inventory.items.First(i=>i.kind==ItemKind.Sword);
                yield return Pointer(Cell(s.inventory,sword.x,sword.y),EventType.MouseDown,0);
                yield return Pointer(Cell(s.inventory,1,0),EventType.MouseUp,0);
                Check(sword.x==1 && sword.y==0 && s.inventory.Valid(),"drag coordinates remain correct after resize "+size);
                sword.x=0;yield return Capture("screen-"+size+"-storage.png");ui.bagOpen=ui.shipOpen=false;
            }
            Screen.SetResolution(1440,900,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.8f);
        }
        IEnumerator OrbChecks()
        {
            var ui=game.ui;ui.open=false;ui.ClearCheckPointer();game.survivor.equipment[0]=new Item(ItemKind.Orb);game.combat.manual=false;
            game.body.position=new Vector2(10,12);game.hero.transform.position=game.body.position;game.cameraView.orthographicSize=3.4f;
            game.ResetCamera(game.hero.transform.position+new Vector3(0,2.2f,-20));
            for(int d=0;d<8;d++)
            {
                game.facing=d;yield return null;yield return new WaitForEndOfFrame();
                Check(game.combat.weapon.enabled && game.combat.weapon.bounds.Intersects(game.hero.bounds),"orb remains attached to visible hand region in direction "+d);
                Check(game.combat.weapon.bounds.size.x<.35f && game.combat.weapon.bounds.size.y<.35f,"held sphere has hand-sized proportions in direction "+d);
                Check((game.combat.weapon.sortingOrder<game.hero.sortingOrder)==(d>=3 && d<=5),"orb has correct body occlusion in direction "+d);
                yield return Capture("orb-direction-"+d+".png");
            }
            game.cameraView.orthographicSize=8;game.combat.manual=true;
        }
        IEnumerator MagicInteractions()
        {
            var s=game.survivor;var ui=game.ui;var magic=game.magic;s.inventory.items.Clear();s.equipment[0]=null;
            ui.bagOpen=false;ui.OpenShip();game.seconds=30;game.UpdateEnvironment(0);
            for(int i=0;i<30;i++)yield return null;
            yield return Pointer(Cell(game.shipStorage,9,0),EventType.MouseDown,0,true);
            var orb=s.inventory.items.FirstOrDefault(i=>i.kind==ItemKind.Orb);Check(orb!=null,"orb transferred from ship through actual UI");
            yield return Pointer(Cell(s.inventory,0,0),EventType.MouseDown,0,false,2);
            Check(magic.HasOrb && s.equipment[0]==orb,"double click equips orb in hand");
            Check(!game.combat.Begin(Vector2.right),"orb does not perform weapon attack");
            yield return Pointer(Cell(game.shipStorage,8,5),EventType.MouseDown,0,true);
            var tome=s.inventory.items.FirstOrDefault(i=>i.kind==ItemKind.Tome);Check(tome!=null && tome.Definition.width==2 && tome.Definition.height==2,"closed folio is a 2x2 unstacked item");
            ui.tab=0;
            var points=SpatialSigil.Resample(StarSigil.Template,140).Select(p=>p*280+new Vector2(360,225)).ToList();
            Check(magic.BeginStroke(points[0]),"sphere begins cursor drawing");foreach(var p in points)magic.AddPoint(p);
            Check(!magic.EndStroke() && !magic.InPocket,"correct sigil requires an open carried tome");
            yield return Pointer(Cell(s.inventory,tome.x,tome.y),EventType.MouseDown,0);
            yield return Pointer(Cell(s.inventory,tome.x,tome.y),EventType.MouseUp,0);
            Check(ui.Dragging && !tome.opened,"left click picks up book without opening it");
            yield return Pointer(Cell(s.inventory,tome.x,tome.y),EventType.MouseDown,1);
            yield return Pointer(Cell(s.inventory,tome.x,tome.y),EventType.MouseDown,1);
            yield return Pointer(Cell(s.inventory,tome.x,tome.y),EventType.MouseUp,1);
            Check(!ui.Dragging && !ui.IsCrafting && tome.opened && tome.Definition.width==3 && tome.Definition.height==2 && s.inventory.Valid(),"right click opens book into 3 columns and 2 rows");
            yield return Pointer(Cell(s.inventory,tome.x,tome.y),EventType.MouseDown,1);yield return Pointer(Cell(s.inventory,tome.x,tome.y),EventType.MouseUp,1);
            Check(!tome.opened && tome.Definition.width==2 && s.inventory.Valid(),"right click closes book and restores footprint");
            yield return Pointer(Cell(s.inventory,tome.x,tome.y),EventType.MouseDown,1);yield return Pointer(Cell(s.inventory,tome.x,tome.y),EventType.MouseUp,1);
            Check(tome.opened && s.inventory.Valid(),"right click reopens spatial folio");
            magic.CancelStroke();ui.Notice(null);game.combat.manual=false;yield return Capture("07-magic-items.png");
            yield return InkTrailChecks();
            Check(magic.BeginStroke(points[0]),"begin reference stroke with open tome");foreach(var p in points)magic.AddPoint(p);
            var beforeMovement=game.body.position;Check(!game.MovementLocked,"drawing with sphere does not lock movement");
            game.checkMovement=Vector2.right;yield return new WaitForSecondsRealtime(.16f);game.checkMovement=null;
            yield return new WaitForFixedUpdate();yield return null;
            Check(magic.Drawing && Vector2.Distance(beforeMovement,game.body.position)>.30f,"hero moves while continuous sphere stroke remains active");
            var position=game.body.position;
            Check(magic.StrokeWidth>=8 && magic.StrokeWidth<magic.PixelUnit*4 && magic.sparks.Count>5,"slimmer translucent pixel ink emits trailing particles");
            var sparkPosition=magic.sparks[0].position;magic.TickSparks(.15f);Check(Vector2.Distance(sparkPosition,magic.sparks[0].position)>1,"ink particles drift behind the moving stroke");
            yield return Capture("08-drawing-sigil.png");
            Check(magic.EndStroke() && magic.Casting && !magic.InPocket,"accepted sigil starts short teleport animation before changing dimension");
            Check(magic.AcceptedFade>.95f && game.MovementLocked,"accepted sigil begins half-second blue fade");
            yield return new WaitForSecondsRealtime(.14f);yield return Capture("08b-accepted-blue-sigil.png");
            Check(TeleportParticlesVisible(),"pixel teleport particles visibly surround actual on-screen hero");
            Check(magic.Casting && magic.AcceptedFade>0 && magic.AcceptedFade<.80f,"accepted blue sigil fades progressively before teleport");
            yield return new WaitForSecondsRealtime(.38f);yield return Capture("08c-teleport-arrival.png");
            Check(magic.InPocket && !magic.Casting && magic.AcceptedFade==0,"teleport completes and successful sigil disappears after half a second");
            yield return new WaitForSecondsRealtime(.30f);
            Check(!game.coastRoot.activeSelf && magic.pocket.activeSelf && !game.ShipReachable,"coastal collision and storage disabled in pocket");
            Check(game.body.position==new Vector2(0,-1.7f) && s.equipment[0]==orb && s.inventory.items.Contains(tome),"teleport preserves inventory and gear");
            game.seconds=5;game.UpdateEnvironment(0);game.cameraView.Render();
            Check(game.pixelView.atmosphere.GetFloat("_Rain")==0 && game.pixelView.atmosphere.GetFloat("_Fog")==0,"coastal storm does not leak into cosmic chamber");
            ui.open=false;ui.ClearCheckPointer();
            game.combat.manual=false;yield return new WaitForSecondsRealtime(1.3f);game.combat.manual=true;
            var cameraPosition=game.cameraView.transform.position;game.body.position=new Vector2(2,2);game.hero.transform.position=game.body.position;
            for(int i=0;i<30;i++)yield return null;
            Check(Vector2.Distance(cameraPosition,game.cameraView.transform.position)<.05f,"pocket camera remains fixed when hero moves");
            var low=game.cameraView.WorldToViewportPoint(new Vector2(-5.8f,-4.75f));var high=game.cameraView.WorldToViewportPoint(new Vector2(5.8f,7.1f));
            Check(low.x>0 && low.y>0 && high.x<1 && high.y<1,"whole medieval chamber and door fit viewport");
            game.body.position=new Vector2(0,-1.7f);game.hero.transform.position=game.body.position;
            game.UpdateEnvironment(0);game.cameraView.Render();float doorwayLit=BrightnessAt(new Vector2(.4f,3));
            Shader.SetGlobalInt("_CoastLightCount",0);game.cameraView.Render();float doorwayUnlit=BrightnessAt(new Vector2(.4f,3));
            Check(doorwayLit>doorwayUnlit+.03f,"blue door light illuminates nearby flagstone");game.UpdateEnvironment(0);
            Check(magic.ruins.Count==12 && !magic.pocket.GetComponentsInChildren<Transform>().Any(t=>t.name.Contains("wall collision")),"room perimeter uses ruined columns and arches without walls");
            Check(magic.ruins.All(r=>r.bounds.min.x>=-4.25f && r.bounds.max.x<=4.25f && r.transform.position.y>=-4.09f),"ruined columns and arches sit closer to edges while remaining on the platform");
            game.UpdateEnvironment(0);var fixedLights=Shader.GetGlobalVectorArray("_CoastLights");game.cameraView.Render();float lightA=BrightnessAt(new Vector2(.4f,3));
            yield return new WaitForSecondsRealtime(.16f);game.UpdateEnvironment(0);game.cameraView.Render();float lightB=BrightnessAt(new Vector2(.4f,3));
            var laterLights=Shader.GetGlobalVectorArray("_CoastLights");
            Check(Shader.GetGlobalFloat("_CoastClock")==0 && fixedLights.Take(3).SequenceEqual(laterLights.Take(3)) && Mathf.Abs(lightA-lightB)<.002f,"door lighting remains steady while its portal animation continues");
            magic.checkClock=12;game.cameraView.Render();yield return Capture("09-pocket-dimension.png");
            var doorA=ReadPixels();Shader.SetGlobalFloat("_CosmicTime",13.7f);game.cameraView.Render();var doorB=ReadPixels();
            var doorPoint=game.cameraView.WorldToViewportPoint(new Vector2(0,5.0f));float doorChange=0;
            int dx=(int)(doorPoint.x*doorA.width),dy=(int)(doorPoint.y*doorA.height);
            for(int y=-12;y<=12;y++)for(int x=-8;x<=8;x++)doorChange+=Mathf.Abs(doorA.GetPixel(dx+x,dy+y).grayscale-doorB.GetPixel(dx+x,dy+y).grayscale);
            Check(doorChange/425>.008f,"door vortex visibly animates its own rendered pixels");Destroy(doorA);Destroy(doorB);
            yield return PocketScreenChecks();
            magic.checkClock=12;game.cameraView.Render();
            var a=ReadPixels();Shader.SetGlobalFloat("_CosmicTime",38);game.cameraView.Render();var b=ReadPixels();
            var ap=a.GetPixels();var bp=b.GetPixels();float delta=0;for(int i=0;i<ap.Length;i++)delta+=Mathf.Abs(ap[i].grayscale-bp[i].grayscale);
            Check(delta/ap.Length>.001f,"cosmic background animation changes rendered pixels");Destroy(a);Destroy(b);
            magic.checkClock=38;yield return Capture("09b-cosmic-animation.png");
            var sky=magic.pocket.GetComponentsInChildren<MeshRenderer>().First(r=>r.sharedMaterial.shader.name=="AshenCoast/Cosmos").material;
            float best=0,bestTime=0;
            for(int time=0;time<18;time++)
            {
                float clock=.8f+time*2.15f;Shader.SetGlobalFloat("_CosmicTime",clock);
                sky.SetFloat("_MeteorVisibility",1);game.cameraView.Render();var on=ReadPixels();
                sky.SetFloat("_MeteorVisibility",0);game.cameraView.Render();var off=ReadPixels();
                var x=on.GetPixels();var y=off.GetPixels();float difference=0;
                for(int n=0;n<x.Length;n++)difference+=Mathf.Abs(x[n].grayscale-y[n].grayscale);
                difference/=x.Length;if(difference>best){best=difference;bestTime=clock;}Destroy(on);Destroy(off);
            }
            Check(best>.00001f,"meteor streaks visibly render in cosmic background");
            sky.SetFloat("_MeteorVisibility",1);magic.checkClock=bestTime;yield return Capture("09c-cosmic-meteors.png");magic.checkClock=null;
            // Ruin footings and platform edges use actual movement and physics.
            game.body.position=new Vector2(-1.05f,3.7f);game.hero.transform.position=game.body.position;
            game.checkMovement=Vector2.up;for(int i=0;i<22;i++)yield return new WaitForFixedUpdate();game.checkMovement=null;
            Check(game.body.position.y>4 && game.body.position.y<4.15f,"open platform edge keeps player on floor");
            game.body.position=new Vector2(0,-1.7f);game.hero.transform.position=game.body.position;
            Check(magic.BeginStroke(points[0]),"sphere can still draw inside pocket");foreach(var p in points)magic.AddPoint(p);
            Check(!magic.EndStroke() && magic.InPocket,"exit requires the return doorway");magic.CancelStroke();
            s.inventory.ToggleTome(tome);s.equipment[0]=null;
            game.body.position=new Vector2(0,3.3f);game.hero.transform.position=game.body.position;
            game.checkMovement=Vector2.up;
            for(int i=0;i<40 && magic.InPocket;i++)yield return new WaitForFixedUpdate();game.checkMovement=null;
            Check(!magic.InPocket && Vector2.Distance(game.body.position,position)<.08f,"walking through door returns to exact coastal position without orb or open book");
            Check(game.coastRoot.activeSelf && !magic.pocket.activeSelf && game.ShipReachable,"coast and ship interactions restored on return");
            game.seconds=30;game.UpdateEnvironment(0);magic.CancelStroke();
        }
        IEnumerator InkTrailChecks()
        {
            var magic=game.magic;magic.CancelStroke();bool inventoryWasOpen=game.ui.open;game.ui.open=false;
            // Let the camera settle after closing the inventory before comparing native background pixels.
            for(int i=0;i<120;i++)yield return null;
            yield return new WaitForEndOfFrame();var background=ScreenCapture.CaptureScreenshotAsTexture();
            var start=new Vector2(310,285);Check(magic.BeginStroke(start),"begin continuous particle-tail preview");
            for(int i=1;i<=145;i++)
            {
                magic.AddPoint(start+Vector2.right*i*4);
                if(i%5==0)yield return null;
            }
            yield return Capture("08d-translucent-ink-tail.png");
            var rendered=ScreenCapture.CaptureScreenshotAsTexture();
            Check(magic.ribbonTexture && magic.ribbonTexture.filterMode==FilterMode.Point,"magic ribbon uses crisp pixel sampling with no blur");
            var colors=magic.ribbonTexture.GetPixels32();
            float maxAlpha=colors.Max(c=>c.a)/255f;
            Check(maxAlpha>.55f && maxAlpha<=SpatialMagic.InkBodyOpacity+.01f,"composited stroke retains transparency instead of accumulating alpha");
            float blendError=0;int samples=0,visibleMotes=0;
            for(int x=345;x<860;x+=23)
            {
                int y=288;
                int tx=Mathf.FloorToInt((x+.5f-magic.RibbonBounds.x)/magic.DetailUnit);
                int ty=magic.ribbonTexture.height-1-Mathf.FloorToInt((y+.5f-magic.RibbonBounds.y)/magic.DetailUnit);
                if(tx<0 || ty<0 || tx>=magic.ribbonTexture.width || ty>=magic.ribbonTexture.height)continue;
                Color ink=magic.ribbonTexture.GetPixel(tx,ty);if(ink.a<.3f)continue;
                Color ground=background.GetPixel(x,background.height-1-y),actual=rendered.GetPixel(x,rendered.height-1-y);
                Color expected=Color.Lerp(ground,ink,ink.a);
                blendError+=(Mathf.Abs(expected.r-actual.r)+Mathf.Abs(expected.g-actual.g)+Mathf.Abs(expected.b-actual.b))/3;samples++;
            }
            Check(samples>15 && blendError/samples<.055f,"actual on-screen ribbon blends once with visible world beneath ("+(blendError/Mathf.Max(1,samples)).ToString("F3")+")");
            foreach(var mote in magic.sparks)
            {
                var p=mote.position;int x=Mathf.RoundToInt(Mathf.Round(p.x/magic.DetailUnit)*magic.DetailUnit),y=rendered.height-1-Mathf.RoundToInt(Mathf.Round(p.y/magic.DetailUnit)*magic.DetailUnit);
                if(x<0 || y<0 || x>=rendered.width || y>=rendered.height)continue;
                var c=rendered.GetPixel(x,y);var b=background.GetPixel(x,y);
                if(c.b-b.b>.12f && c.g-b.g>.055f && Mathf.Abs(p.y-285)>magic.StrokeWidth*.5f)visibleMotes++;
            }
            Check(visibleMotes>=12,"pixel particle tail visibly trails outside the ribbon ("+visibleMotes+" motes)");
            Check(magic.sparks.Any(s=>s.age>.2f) && magic.sparks.Any(s=>s.age<.1f),"moving stroke keeps a tail of both fading and fresh particles");
            float nearest=magic.sparks.Min(s=>Vector2.Distance(s.origin,magic.stroke.Last()));
            Check(nearest<8,"new motes continue to spawn at newest stroke tip");
            Destroy(background);Destroy(rendered);
            for(int i=1;i<=55;i++)
            {
                magic.AddPoint(new Vector2(890+i*2,285+65*Mathf.Sin(i*.045f)));
                if(i%3==0)yield return null;
            }
            yield return Capture("08e-moving-pixel-tail.png");
            Check(!magic.EndStroke() && !magic.Casting,"particle preview leaves sigil recognition intact");magic.CancelStroke();
            Check(magic.BeginStroke(start),"start long stroke emitter verification");
            Vector2 last=start;
            for(int i=1;i<=1800;i++){last=start+new Vector2((i%120)*4,(i/120)*12);magic.AddPoint(last);}
            Check(magic.sparks.Count==256 && Vector2.Distance(magic.sparks.Last().origin,last)<8,"long strokes keep bounded particles and prioritize newest tip after resampling");
            magic.TickSparks(.10f);
            Check(magic.sparks.All(s=>!float.IsNaN(s.position.x) && !float.IsNaN(s.position.y)),"tail remains stable when stored stroke points are resampled");
            magic.TickSparks(2);Check(magic.sparks.Count==0,"all tail particles fade and expire without leaking");magic.CancelStroke();game.ui.open=inventoryWasOpen;
        }
        IEnumerator PocketScreenChecks()
        {
            var ui=game.ui;Screen.SetResolution(Screen.currentResolution.width,Screen.currentResolution.height,FullScreenMode.FullScreenWindow);
            yield return new WaitForSecondsRealtime(.9f);ui.open=true;ui.bagOpen=false;yield return new WaitForSecondsRealtime(.8f);
            var corner=game.cameraView.WorldToViewportPoint(new Vector2(5.8f,7.1f));
            Check(corner.x<ui.WorldFraction && corner.y<1,"fullscreen pocket room fits beside inventory");yield return Capture("pocket-fullscreen-inventory.png");
            game.magic.PlayArrival();yield return new WaitForSecondsRealtime(.08f);yield return Capture("pocket-fullscreen-teleport.png");
            Check(TeleportParticlesVisible(),"fullscreen arrival particles surround hero rather than half-resolution position");
            yield return new WaitForSecondsRealtime(.30f);
            ui.ToggleBag();yield return new WaitForSecondsRealtime(.9f);
            corner=game.cameraView.WorldToViewportPoint(new Vector2(5.8f,7.1f));
            Check(corner.x<ui.WorldFraction && corner.y<1,"fullscreen pocket room fits beside separate sack window");yield return Capture("pocket-fullscreen-bag.png");
            ui.open=false;ui.bagOpen=false;Screen.SetResolution(1440,900,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.9f);
        }
        bool TeleportParticlesVisible()
        {
            var image=ScreenCapture.CaptureScreenshotAsTexture();var center=game.magic.TeleportCenter;int cyanPixels=0;
            for(int y=-60;y<=60;y++)for(int x=-85;x<=85;x++)
            {
                if(x*x+y*y<900)continue;int px=Mathf.RoundToInt(center.x)+x,py=image.height-1-Mathf.RoundToInt(center.y)-y;
                if(px<0 || py<0 || px>=image.width || py>=image.height)continue;
                var color=image.GetPixel(px,py);if(color.b>.57f && color.g>.48f && color.b-color.r>.07f)cyanPixels++;
            }
            Destroy(image);return cyanPixels>12;
        }
        IEnumerator NewMagicChecks()
        {
            var a=game.arcane;var s=game.survivor;var ui=game.ui;var frog=a.frogs[0];s.mana=100;
            game.magic.CancelStroke();ui.open=false;ui.ClearCheckPointer();s.inventory.items.Clear();s.health=100;game.numbers.Clear();
            Check(game.GetComponent<PixelWalk>()==null,"walking uses complete registered frames without detached boot overlays");
            Check(game.art.hero.GetLength(1)==8,"hero has eight phases per walking direction");
            var gait=new HashSet<int>();frog.Reset();frog.Warp(new Vector2(7.5f,7.2f));
            for(int i=0;i<20;i++){frog.Tick(.08f);gait.Add(frog.frame);}
            Check(gait.Count>=5 && !frog.Hostile && a.flames.Count==0,"neutral frog walks through multiple complete poses without firing");
            game.body.position=new Vector2(3.2f,5.7f);game.hero.transform.position=game.body.position;frog.Warp(new Vector2(6.2f,5.9f));
            game.ResetCamera(game.hero.transform.position+new Vector3(0,1,-20));yield return Capture("v8-frog-neutral.png");
            var frogHome=frog.Position;var frogCamera=game.cameraView.transform.position;float frogZoom=game.cameraView.orthographicSize;frog.Warp(new Vector2(6.2f,-2.3f));game.cameraView.orthographicSize=3;game.ResetCamera(new Vector3(6.2f,-1.65f,-20));var frogStyle=new MaterialPropertyBlock();frog.renderer.GetPropertyBlock(frogStyle);frogStyle.SetFloat("_Emission",1);frog.renderer.SetPropertyBlock(frogStyle);game.cameraView.Render();
            var frogPixels=ReadPixels();var frogBounds=frog.renderer.bounds;frog.renderer.enabled=false;game.cameraView.Render();var frogBackground=ReadPixels();frog.renderer.enabled=true;
            float hatPixels=RegionDifference(frogPixels,frogBackground,new Rect(frogBounds.min.x,frogBounds.min.y+frogBounds.size.y*.66f,frogBounds.size.x,frogBounds.size.y*.34f));
            float robePixels=RegionDifference(frogPixels,frogBackground,new Rect(frogBounds.min.x,frogBounds.min.y+frogBounds.size.y*.25f,frogBounds.size.x,frogBounds.size.y*.41f));
            float feetPixels=RegionDifference(frogPixels,frogBackground,new Rect(frogBounds.min.x,frogBounds.min.y,frogBounds.size.x,frogBounds.size.y*.25f));
            Check(hatPixels>.02f && robePixels>.02f && feetPixels>.008f,"frog hat, robe and walking feet all render visibly after atlas changes ("+hatPixels.ToString("F3")+" / "+robePixels.ToString("F3")+" / "+feetPixels.ToString("F3")+")");Destroy(frogPixels);Destroy(frogBackground);yield return Capture("v9-frog-readable-closeup.png");frogStyle.SetFloat("_Emission",0);frog.renderer.SetPropertyBlock(frogStyle);frog.Warp(frogHome);game.cameraView.orthographicSize=frogZoom;game.ResetCamera(frogCamera);game.UpdateEnvironment(0);
            float hp=s.health;frog.Hurt(1);Check(frog.Hostile && frog.health==FrogMage.MaximumHealth-1,"first damaging hit provokes frog mage");
            a.Tick(.2f);Check(a.flames.Count==0 && s.health==hp,"frog winds up before firing instead of inflicting advance damage");
            for(int i=0;i<60;i++)a.Tick(.02f);game.UpdateEnvironment(0);yield return Capture("v8-frog-fire-cone.png");
            Check(a.flames.Count>25 && a.flames.Where(f=>f.hostile).All(f=>Vector2.Distance(f.origin,f.renderer.transform.position)<=5.01f),"frog emits turbulent animated flames within five-unit range");
            Check(s.health<hp,"hostile flame contact damages player after travelling from lamp");
            foreach(var f in a.flames)Destroy(f.renderer.gameObject);a.flames.Clear();frog.Reset();s.health=100;
            s.equipment[0]=new Item(ItemKind.Lamp);game.body.position=new Vector2(0,8);game.hero.transform.position=game.body.position;game.seconds=210;
            game.ResetCamera(game.hero.transform.position+new Vector3(0,1,-20));a.clock=.1f;game.UpdateEnvironment(0);
            Check(a.lastLightCount>=2 && Shader.GetGlobalInt("_CoastLightCount")<=32,"player and frog lamps illuminate world alongside fireflies without exceeding light capacity");
            float strength=Shader.GetGlobalVectorArray("_CoastLights")[0].w;a.clock=.32f;game.UpdateEnvironment(0);
            Check(Mathf.Abs(Shader.GetGlobalVectorArray("_CoastLights")[0].w-strength)>.05f,"held brass lamp light flickers visibly");yield return Capture("v8-lamp-light-at-night.png");
            game.seconds=30;game.UpdateEnvironment(0);game.body.position=game.DummyBase+new Vector2(-4,-.2f);game.hero.transform.position=game.body.position;
            game.ResetCamera(game.hero.transform.position+new Vector3(0,1,-20));s.equipment[0]=new Item(ItemKind.YoYo);game.dummyHealth=500;
            Vector2 origin=a.PlayerHand;Check(a.SetYoYoTarget(game.dummy.bounds.center,true),"yo-yo starts extending toward cursor target");a.Tick(.05f);
            Check(Vector2.Distance(a.YoYoPosition,origin)>.05f && Vector2.Distance(a.YoYoPosition,game.dummy.bounds.center)>1,"yo-yo follows with a short delay instead of jumping to cursor");
            for(int i=0;i<40;i++)a.Tick(.016f);yield return Capture("v8-yoyo-contact.png");
            Check(game.dummyHealth<500 && (500-game.dummyHealth)%18==0,"yo-yo applies fixed eighteen damage on actual textured contact");
            a.SetYoYoTarget(a.PlayerHand+Vector2.right*50,true);for(int i=0;i<35;i++)a.Tick(.016f);
            Check(Vector2.Distance(a.YoYoPosition,a.PlayerHand)<=5.01f && Vector2.Distance(a.YoYoPosition,a.PlayerHand)>4.5f,"yo-yo string enforces five-unit reach even for distant cursor");
            yield return Capture("v8-yoyo-maximum-range.png");a.SetYoYoTarget(a.PlayerHand,false);for(int i=0;i<80;i++)a.Tick(.016f);Check(!a.YoYoActive,"released yo-yo retracts to hand and disappears");
            s.inventory.items.Clear();var woodA=new Item(ItemKind.Wood);var woodB=new Item(ItemKind.Wood);s.inventory.Place(woodA,0,0);s.inventory.Place(woodB,1,0);ui.open=true;
            yield return Capture("v8-crafting-wood.png");yield return Pointer(Cell(s.inventory,0,0),EventType.MouseDown,1);yield return Pointer(Cell(s.inventory,1,1),EventType.MouseDrag,1);yield return Pointer(Cell(s.inventory,1,1),EventType.MouseUp,1);
            Check(s.inventory.items.Count==1 && s.inventory.items[0].kind==ItemKind.Campfire,"right-drag crafting consumes two full logs and creates campfire");
            var campfireItem=s.inventory.items[0];var campfireCell=Cell(s.inventory,campfireItem.x,campfireItem.y);yield return Pointer(campfireCell,EventType.MouseDown,1);yield return Pointer(campfireCell,EventType.MouseUp,1);
            Check(a.Placing && !ui.open && s.inventory.items.Contains(campfireItem),"right-click campfire previews placement without prematurely consuming item");
            game.body.position=new Vector2(-1,.5f);game.hero.transform.position=game.body.position;var firePoint=new Vector2(.5f,-.3f);
            Check(!a.PlacePendingFire(firePoint+Vector2.right*20) && s.inventory.items.Contains(campfireItem),"out-of-reach placement keeps original campfire item");
            Check(a.PlacePendingFire(firePoint) && !s.inventory.items.Contains(campfireItem) && !a.Placing,"placing crafted campfire consumes exactly one item");
            game.ResetCamera(game.hero.transform.position+new Vector3(0,1,-20));a.Tick(.14f);game.UpdateEnvironment(0);yield return Capture("v8-placed-campfire.png");
            var pyro=new Item(ItemKind.Pyromancy);var lamp=new Item(ItemKind.Lamp);s.inventory.Add(pyro);s.inventory.Add(lamp);s.equipment[0]=null;ui.open=true;
            yield return Capture("v8-new-books-and-lamp.png");yield return Pointer(Cell(s.inventory,pyro.x,pyro.y),EventType.MouseDown,1);yield return Pointer(Cell(s.inventory,pyro.x,pyro.y),EventType.MouseUp,1);
            Check(pyro.opened && pyro.Definition.width==2 && pyro.Definition.height==2,"right-click pyromancy book opens to two by two cells");
            yield return Pointer(Cell(s.inventory,lamp.x,lamp.y),EventType.MouseDown,0);yield return Pointer(Gear(GearSlot.Hand),EventType.MouseDown,0);
            Check(s.equipment[0]==lamp && !s.inventory.items.Contains(lamp),"lamp equips from inventory with left-click transfer");yield return Capture("v8-lamp-equipped-same-size.png");
            var area=InventoryUI.ItemIconArea(lamp,Vector2.zero);Check(area.width==70 && area.height==72 && ui.GearRect(GearSlot.Hand).height>156,"equipment slot fits full inventory sprite dimensions including long weapons");
            ui.open=false;pyro.opened=false;Check(!a.TryExplodeFire(firePoint),"closed pyromancy book cannot detonate campfire");pyro.opened=true;
            game.body.position=firePoint+Vector2.right*7;game.hero.transform.position=game.body.position;Check(!a.TryExplodeFire(firePoint),"campfire explosion requires distance of six or less");
            game.body.position=new Vector2(-1,.5f);game.hero.transform.position=game.body.position;game.dummyHealth=500;
            Check(a.TryExplodeFire(firePoint) && a.campfires.Last().exploded && a.explosions.Count==1,"open pyromancy and held lamp detonate selected campfire");
            a.Tick(.05f);yield return Capture("v8-explosion-ignition.png");Check(game.dummyHealth==500,"explosion ignition has no damage before visible blast");
            a.Tick(.32f);game.UpdateEnvironment(0);yield return Capture("v8-explosion-fire-and-cinders.png");Check(game.dummyHealth==440,"campfire blast applies fixed damage once");
            Check(!a.TryExplodeFire(firePoint),"consumed campfire cannot explode twice");a.Tick(1.2f);Check(a.explosions.Count==0,"fire explosion disperses and releases sprite objects");
            s.inventory.items.Clear();var abyss=new Item(ItemKind.AbyssTome);s.inventory.Add(abyss);s.equipment[0]=new Item(ItemKind.Orb);ui.open=true;
            yield return Capture("v8-abyss-tome-closed.png");yield return Pointer(Cell(s.inventory,abyss.x,abyss.y),EventType.MouseDown,1);yield return Pointer(Cell(s.inventory,abyss.x,abyss.y),EventType.MouseUp,1);
            Check(abyss.opened && abyss.Definition.width==3 && abyss.Definition.height==2,"tentacle tome opens to three columns and two rows");ui.open=false;ui.ClearCheckPointer();
            var prey=game.woodlice.First(m=>m.Small);var fieldPoint=prey.Position;
            game.body.position=fieldPoint+new Vector2(0,-2.2f);game.hero.transform.position=game.body.position;game.ResetCamera(new Vector3(fieldPoint.x,fieldPoint.y+.8f,-20));
            for(int i=0;i<45;i++)yield return null;
            var v=game.cameraView.WorldToViewportPoint(fieldPoint);var centre=new Vector2(v.x*Screen.width,(1-v.y)*Screen.height);
            var middle=new Vector2((SpatialSigil.Template.Min(p=>p.x)+SpatialSigil.Template.Max(p=>p.x))*.5f,(SpatialSigil.Template.Min(p=>p.y)+SpatialSigil.Template.Max(p=>p.y))*.5f);
            var points=SpatialSigil.Resample(SpatialSigil.Template,160).Select(p=>(p-middle)*240+centre).ToList();game.magic.CancelStroke();
            Check(game.magic.BeginStroke(points[0]),"start crossed rune for tentacle summoning");foreach(var p in points)game.magic.AddPoint(p);yield return Capture("v8-star-inner-particles.png");
            Check(game.magic.fixedMotes.Count>10 && game.magic.innerFlowPoints.Count>5,"magic ink contains moving inner motes and stationary glinting particles");
            var moving=game.magic.innerFlowPoints[0];var fixedPoint=game.magic.fixedMotes[0].position;yield return new WaitForSecondsRealtime(.14f);yield return Capture("v8-star-moving-inner-particles.png");
            Check(Vector2.Distance(moving,game.magic.innerFlowPoints[0])>2 && game.magic.fixedMotes[0].position==fixedPoint,"inner particles travel along stroke while other particles remain attached");
            Check(game.magic.EndStroke() && game.magic.LastCastTentacles && !game.magic.Casting && a.fields.Count==1,"crossed rune now summons tentacles without entering pocket dimension");
            var field=a.fields[0];Check(Vector2.Distance(field.position,fieldPoint)<.15f,"tentacles emerge at drawn rune world position");
            yield return new WaitForSecondsRealtime(.55f);game.numbers.Clear();
            var big=game.woodlice.First(m=>!m.Small);var bigHome=big.Position;big.renderer.transform.position=fieldPoint+new Vector2(.9f,.2f);big.vitals.health=200;big.vitals.firstHit=false;big.vitals.curlRemaining=0;
            var poses=new HashSet<int>();for(int i=0;i<12;i++){a.Tick(1f/30);poses.Add(field.poseFrame);}yield return Capture("v8-tentacles-emerging.png");
            var initialVertex=field.limbs[0].vertices[30];for(int i=0;i<18;i++){a.Tick(1f/30);poses.Add(field.poseFrame);}yield return Capture("v8-tentacles-grasping.png");
            Check(field.limbs.Count>=18 && field.limbs.Select(l=>l.length).Distinct().Count()>10 && field.limbs.All(l=>l.vertices.Length>=50),"many varied brown tentacles have independent articulated geometry");
            Check(field.limbs[0].vertices[30]!=initialVertex && poses.Count>=20,"tentacle field animates through many distinct poses");
            Check(prey.Grabbed && !prey.feet.enabled && field.pulled.Count>0,"tentacles seize small creature and disable its walking collider");
            Check(big.vitals.health<200,"tentacles damage larger nearby creatures over time");
            for(int i=0;i<20;i++)a.Tick(1f/30);yield return Capture("v8-tentacles-pulling-underground.png");
            Check(prey.renderer.transform.position.y<fieldPoint.y && prey.vitals.Alive,"small creature visibly sinks during pull animation before disappearing");
            for(int i=0;i<25;i++)a.Tick(1f/30);yield return Capture("v8-tentacles-after-pull.png");Check(!prey.vitals.Alive && !prey.renderer.enabled,"completed tentacle pull takes small creature underground");
            float healthAfter=big.vitals.health;while(field.age<9.85f)a.Tick(1f/30);
            Check(!field.Finished && big.vitals.health<healthAfter,"field persists and continues damage throughout ten-second duration");a.Tick(.20f);Check(a.fields.Count==0,"tentacles retract and expire after ten seconds");
            big.renderer.transform.position=bigHome;game.magic.CancelStroke();frog.Reset();game.numbers.Clear();s.health=100;ui.open=false;game.seconds=30;game.UpdateEnvironment(0);
        }
        IEnumerator RevisionChecks()
        {
            var s=game.survivor;var ui=game.ui;var a=game.arcane;ui.CancelInteraction();ui.ClearCheckPointer();ui.open=true;ui.bagOpen=ui.shipOpen=false;
            s.inventory.items.Clear();s.equipment[0]=null;s.foods.Clear();s.mana=0;
            var brain=new Item(ItemKind.Brain);s.inventory.Add(brain);yield return null;yield return new WaitForEndOfFrame();
            var brainCell=Cell(s.inventory,brain.x,brain.y);yield return Pointer(brainCell,EventType.MouseDown,1);yield return Pointer(brainCell,EventType.MouseUp,1);
            Check(s.mana==100 && s.foods.Count==0 && !s.inventory.items.Contains(brain),"brain restores one hundred mana without occupying any satiety cell");
            var sword=new Item(ItemKind.Sword);s.inventory.Add(sword);yield return null;var swordCell=Cell(s.inventory,sword.x,sword.y);
            yield return Pointer(swordCell,EventType.MouseDown,1);yield return Pointer(swordCell,EventType.MouseUp,1);
            Check(s.equipment[0]==sword && !s.inventory.items.Contains(sword),"right click automatically equips compatible item into empty hand");
            var dagger=new Item(ItemKind.Dagger);s.inventory.Add(dagger);yield return null;var daggerCell=Cell(s.inventory,dagger.x,dagger.y);
            yield return Pointer(daggerCell,EventType.MouseDown,1);yield return Pointer(daggerCell,EventType.MouseUp,1);
            Check(s.equipment[0]==sword && s.inventory.items.Contains(dagger),"right click does not replace occupied gear");ui.CancelInteraction();
            s.equipment[0]=null;s.inventory.items.Clear();s.inventory.Place(sword,0,0);s.inventory.Place(dagger,3,0);yield return null;
            yield return Pointer(Cell(s.inventory,0,0),EventType.MouseDown,0);yield return Pointer(Cell(s.inventory,3,0),EventType.MouseDown,0);
            Check(sword.x==3 && dagger.x==0 && ui.DraggedItem==dagger && s.inventory.Valid(),"clicking occupied inventory swaps positions and carries displaced item on cursor");
            yield return Pointer(Cell(s.inventory,6,0),EventType.MouseDown,0);
            Check(!ui.Dragging && dagger.x==6 && s.inventory.items.Count==2,"displaced item can be placed with next left click without duplication");
            var lamp=new Item(ItemKind.Lamp);var corner=new Item(ItemKind.Brain);s.inventory.Add(lamp);s.inventory.Place(corner,9,4);yield return null;
            int lx=lamp.x,ly=lamp.y;yield return Pointer(Cell(s.inventory,lx,ly),EventType.MouseDown,0);yield return Pointer(Cell(s.inventory,9,4),EventType.MouseDown,0);
            Check(ui.DraggedItem==lamp && lamp.x==lx && lamp.y==ly && corner.x==9 && s.inventory.Valid(),"failed occupied-cell swap keeps both objects and current cursor item");ui.CancelInteraction();
            game.body.position=new Vector2(3,-2.5f);game.hero.transform.position=game.body.position;game.ResetCamera(game.hero.transform.position+new Vector3(4,2.2f,-20));
            for(int i=0;i<15;i++)yield return null;
            yield return Pointer(Cell(s.inventory,dagger.x,dagger.y),EventType.MouseDown,0);
            var landing=game.body.position+Vector2.right*1.2f;var viewport=game.cameraView.WorldToViewportPoint(landing);var dropPointer=new Vector2(viewport.x*ui.CanvasWidth,(1-viewport.y)*ui.CanvasHeight);
            int lootBefore=game.loot.Count;yield return Pointer(dropPointer,EventType.MouseDown,0);
            Check(!ui.Dragging && !s.inventory.items.Contains(dagger) && game.loot.Count==lootBefore+1,"left click outside inventory throws the original item into world");
            var drop=game.loot.Last();game.TickLoot(.25f);Check(drop.InFlight && drop.renderer.transform.position.y>drop.position.y+.5f,"thrown item follows visible arc above its landing point");yield return Capture("v9-thrown-item-midair.png");
            game.TickLoot(.31f);Check(!drop.InFlight && Vector2.Distance(drop.renderer.transform.position,drop.position+Vector2.up*.12f)<.02f,"thrown item settles exactly on ground and stops spinning");
            Check(game.UseWorldRightClick(drop.renderer.bounds.center) && s.inventory.items.Contains(dagger) && drop.collected,"right click retrieves the same thrown item with no duplication");
            var fireItem=new Item(ItemKind.Campfire);s.inventory.Add(fireItem);yield return null;
            yield return Pointer(Cell(s.inventory,fireItem.x,fireItem.y),EventType.MouseDown,0);
            var firePoint=game.body.position+new Vector2(0,1.6f);viewport=game.cameraView.WorldToViewportPoint(firePoint);var firePointer=new Vector2(viewport.x*ui.CanvasWidth,(1-viewport.y)*ui.CanvasHeight);
            int fireBefore=a.campfires.Count;yield return Pointer(firePointer,EventType.MouseDown,0);
            Check(a.campfires.Count==fireBefore+1 && !s.inventory.items.Contains(fireItem) && !ui.Dragging,"campfire can be placed directly from inventory with left-click placement on ground");ui.open=false;ui.ClearCheckPointer();
            Check(game.flowerPatches.Count>=12 && game.flowerPatches.Count(p=>game.flowers.Count(f=>!f.picked && (int)f.kind==(int)ItemKind.BlueFlower+(int)p.z && Vector2.Distance(f.position,new Vector2(p.x,p.y))<p.w*.6f)>8)>=8,"field contains numerous clusters of the same flower type");
            var flower=game.flowers.First(f=>!f.picked);game.body.position=flower.position+new Vector2(-1,-.4f);game.hero.transform.position=game.body.position;game.ResetCamera(game.hero.transform.position+new Vector3(0,2.2f,-20));
            game.HighlightAt(null);game.cameraView.Render();var flowerDark=ReadPixels();game.HighlightAt(flower.renderer.bounds.center);game.cameraView.Render();var flowerLit=ReadPixels();
            var fb=flower.renderer.bounds;float flowerDelta=RegionDifference(flowerDark,flowerLit,new Rect(fb.min.x,fb.min.y,fb.size.x,fb.size.y));Check(game.HoverName==ItemDefinition.All[flower.kind].name && flowerDelta>.015f,"hover highlights rendered flower pixels and exposes its item name ("+flowerDelta.ToString("F3")+")");Destroy(flowerDark);Destroy(flowerLit);
            var hoverViewport=game.cameraView.WorldToViewportPoint(flower.renderer.bounds.center);ui.QueuePointerForCheck(new Vector2(hoverViewport.x*ui.CanvasWidth,(1-hoverViewport.y)*ui.CanvasHeight),EventType.Repaint,0);yield return Capture("v9-hovered-flower.png");ui.ClearCheckPointer();
            s.equipment[0]=new Item(ItemKind.Orb);Check(game.UseWorldRightClick(flower.renderer.bounds.center) && flower.picked && game.RightClickConsumedFrame==Time.frameCount,"right click collects flower with equipped sphere and consumes click before drawing");game.HighlightAt(null);
            game.seconds=180;game.UpdateEnvironment(0);Check(game.fireflies.All(f=>!f.dot.enabled),"fireflies start hidden at exact dusk boundary");
            game.seconds=185;game.UpdateEnvironment(0);int partial=game.fireflies.Count(f=>f.dot.enabled);float early=game.fireflies.Sum(f=>f.dot.color.a);
            Check(partial>0 && partial<game.fireflies.Count,"fireflies appear in staggered groups during first night seconds");
            game.seconds=210;game.UpdateEnvironment(0);Check(game.fireflies.All(f=>f.dot.enabled) && game.fireflies.Sum(f=>f.dot.color.a)>early*2,"fireflies gradually reach full population and light strength");
            game.seconds=16.2f;game.UpdateEnvironment(0);Check(Mathf.Abs(game.lastThunderAt-16.2f)<.01f && game.flash==0,"thunder begins one second before fourth lightning flash");
            game.seconds=17.2f;game.UpdateEnvironment(0);Check(game.flash>.95f && Mathf.Abs(game.seconds-game.lastThunderAt-1)<.01f,"lightning follows thunder by one second");
            game.seconds=0;game.ResetCamera(new Vector3(0,0,-20));int culling=game.cameraView.cullingMask;game.cameraView.cullingMask=0;game.UpdateEnvironment(0);game.cameraView.Render();float fogA=BrightnessAt(Vector2.zero);
            game.cameraView.transform.position+=Vector3.right*3;game.cameraView.Render();float fogB=BrightnessAt(Vector2.zero);game.cameraView.cullingMask=culling;
            Check(Mathf.Abs(fogA-fogB)<.015f,"fog density at fixed world location stays consistent when camera moves");yield return Capture("v9-world-anchored-fog.png");game.seconds=30;game.UpdateEnvironment(0);
            s.inventory.items.Clear();var orb=new Item(ItemKind.Orb);var abyss=new Item(ItemKind.AbyssTome);var pyro=new Item(ItemKind.Pyromancy);s.inventory.Add(orb);s.inventory.Add(abyss);s.inventory.Add(pyro);s.inventory.Add(new Item(ItemKind.Lamp));s.inventory.Add(new Item(ItemKind.YoYo));ui.open=true;yield return Capture("v9-revised-item-sprites.png");
            Check(orb.Definition.width==2 && orb.Definition.height==2 && s.inventory.Valid(),"larger sphere occupies two columns and two rows without overlaps");
            yield return Pointer(Cell(s.inventory,abyss.x,abyss.y),EventType.MouseDown,1);yield return Pointer(Cell(s.inventory,abyss.x,abyss.y),EventType.MouseUp,1);yield return Capture("v9-open-abyss-rune.png");ui.open=false;ui.ClearCheckPointer();
            game.art.UpdateOrb(.1f);var old=RenderTexture.active;RenderTexture.active=ArtBank.OrbIcon;var orbA=new Texture2D(64,64,TextureFormat.RGBA32,false);orbA.ReadPixels(new Rect(0,0,64,64),0,0);orbA.Apply();
            game.art.UpdateOrb(.9f);var orbB=new Texture2D(64,64,TextureFormat.RGBA32,false);orbB.ReadPixels(new Rect(0,0,64,64),0,0);orbB.Apply();RenderTexture.active=old;
            float orbDelta=0;var ca=orbA.GetPixels();var cb=orbB.GetPixels();for(int i=0;i<ca.Length;i++)orbDelta+=Mathf.Abs(ca[i].r-cb[i].r)+Mathf.Abs(ca[i].g-cb[i].g)+Mathf.Abs(ca[i].b-cb[i].b);Destroy(orbA);Destroy(orbB);
            Check(orbDelta/ca.Length>.05f,"sphere icon texture contains animated internal swirling pixels");
            game.facing=0;game.walking=false;game.hero.sprite=game.art.hero[0,3];game.hero.flipX=false;game.art.PixelStyle(game.hero);game.cameraView.orthographicSize=4;game.ResetCamera(game.hero.transform.position+new Vector3(0,1,-20));
            game.combat.manual=false;yield return null;yield return new WaitForEndOfFrame();game.art.UpdateOrb(.1f);game.cameraView.Render();var worldOrbA=ReadPixels();game.art.UpdateOrb(.9f);game.cameraView.Render();var worldOrbB=ReadPixels();var ob=game.combat.weapon.bounds;
            float heldDelta=RegionDifference(worldOrbA,worldOrbB,new Rect(ob.min.x,ob.min.y,ob.size.x,ob.size.y));Check(heldDelta>.012f,"held sphere sprite uses same moving interior as inventory texture ("+heldDelta.ToString("F3")+")");Destroy(worldOrbA);Destroy(worldOrbB);yield return Capture("v9-animated-held-sphere.png");game.combat.manual=true;game.cameraView.orthographicSize=8;
            var frog=a.frogs[0];frog.Reset();Check(frog.renderer.bounds.size.y>1.2f && frog.renderer.bounds.size.y<1.55f && frog.feet.radius==.19f,"frog is slightly larger with readable world pixel density and registered collision");
            float yoDiameter=a.yoYo.sprite.bounds.size.y*a.yoYo.transform.lossyScale.y;Check(yoDiameter>.80f && yoDiameter<.84f,"yo-yo world sprite is visibly enlarged independent of rotation");
            Check(game.art.woodlouse.Take(16).Select(sp=>sp.rect).Distinct().Count()>12,"woodlouse has sixteen distinct walking frames");
            game.journey.SetArea(WorldArea.Coast,new Vector2(0,-1.5f));var music=game.soundscape;music.Tick(12);Check(music.cello.clip.length>60 && music.cello.volume>.20f && music.cosmos.volume==0 && music.gullCount>0,"coast plays original cello lament and periodic gulls");
            game.magic.SwitchDimension();game.UpdateEnvironment(0);music.Tick(3);Check(music.cosmos.clip.length>40 && music.cosmos.volume>.35f && music.cello.volume==0,"pocket dimension crossfades to majestic cosmic ambience");
            var footprints=game.magic.ruins.SelectMany(r=>r.GetComponentsInChildren<BoxCollider2D>()).ToList();
            Check(footprints.Count>=12 && footprints.All(c=>c.bounds.min.x>=-4.3f && c.bounds.max.x<=4.3f && c.bounds.min.y>=-4.3f && c.bounds.max.y<=4.4f),"ruin collision footprints remain inside room edges");
            int matched=footprints.Count(c=>{var r=c.GetComponentInParent<SpriteRenderer>();return game.art.Opaque(r.sprite,r.transform.InverseTransformPoint(c.bounds.center));});
            Check(matched>=footprints.Count*.85f,"column collision centres align with opaque stone bases");yield return Capture("v9-pocket-edge-ruins.png");game.magic.SwitchDimension();game.UpdateEnvironment(0);music.Tick(3);
            Check(music.cello.volume>.20f && music.cosmos.volume==0,"return to coast restores cello and fades cosmic sounds");
            ui.ClearCheckPointer();ui.open=false;game.HighlightAt(null);game.combat.ResetForCheck();s.health=100;
        }
        IEnumerator WalkingChecks()
        {
            for(int d=0;d<8;d++)
            {
                float angle=d*45*Mathf.Deg2Rad;var direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                game.body.position=new Vector2(10,12);game.hero.transform.position=game.body.position;game.checkMovement=direction;
                var frames=new HashSet<int>();
                for(int i=0;i<35;i++)
                {
                    yield return new WaitForFixedUpdate();frames.Add(game.walkFrame);
                    if(d==0 && i%7==0)yield return Capture("walk-east-"+i.ToString("D2")+".png");
                    if(d==6 && i%7==0)yield return Capture("walk-south-"+i.ToString("D2")+".png");
                }
                Check(frames.Count>=3 && game.facing==CoastGame.Facing(direction),"walk cycle advances in direction "+d);
                Check(Mathf.Abs(game.body.linearVelocity.magnitude-CoastGame.WalkSpeed)<.08f,"faster walking speed remains equal in direction "+d);
            }
            game.checkMovement=null;game.movement=Vector2.zero;game.body.linearVelocity=Vector2.zero;
            yield return null;Check(!game.walking && game.walkFrame==1,"stopping returns to a stable idle frame");
            yield return Capture("02c-eight-direction-walk.png");
        }
        IEnumerator GaitChecks()
        {
            game.checkMovement=null;game.movement=Vector2.zero;game.body.linearVelocity=Vector2.zero;
            game.body.position=new Vector2(10,9);game.hero.transform.position=game.body.position;game.ui.open=false;
            game.cameraView.orthographicSize=2.5f;game.ResetCamera(game.hero.transform.position+new Vector3(0,.8f,-20));
            for(int d=0;d<8;d++)
            {
                float angle=(-90-d*45)*Mathf.Deg2Rad;var forward=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                game.hero.flipX=d==5 || d==6;game.hero.sprite=game.art.hero[d,3];game.art.PixelStyle(game.hero);
                game.gait.Pose(d,.25f,true,forward,game.hero.flipX);
                game.cameraView.Render();var a=ReadPixels();File.WriteAllBytes(Path.Combine(directory,"v12-player-step-"+d+"-left.png"),a.EncodeToPNG());
                game.gait.Pose(d,.75f,true,forward,game.hero.flipX);
                game.cameraView.Render();var b=ReadPixels();File.WriteAllBytes(Path.Combine(directory,"v12-player-step-"+d+"-right.png"),b.EncodeToPNG());
                float difference=RegionDifference(a,b,new Rect(game.body.position.x-.8f,game.body.position.y-.3f,1.6f,.9f));
                Check(difference>.002f,"rendered paired-leg poses differ between opposite passing phases in direction "+d+" ("+difference.ToString("F4")+")");Destroy(a);Destroy(b);
            }
            foreach(var kind in new[]{ItemKind.Tome,ItemKind.Pyromancy,ItemKind.AbyssTome})
            {var book=new Item(kind);book.Wear(500);book.opened=true;book.Wear(500);Check(!book.Definition.Durable && book.Usable,"book remains usable without durability in either state "+kind);}
            var abyssArea=InventoryUI.ItemIconArea(new Item(ItemKind.AbyssTome),Vector2.zero);var refBook=game.art.icons[17];var refArea=InventoryUI.ItemIconArea(new Item(ItemKind.Tome),Vector2.zero);float refW=Mathf.Min(refArea.width,refArea.height*refBook.rect.width/refBook.rect.height);
            Check(abyssArea.width<=refW+.01f,"closed abyss icon cannot exceed spatial tome visible width");
            var rows=game.art.Bands(game.art.woodlouse[0].texture,true,4);
            Check(game.art.woodlouse.Select((sp,i)=>sp.rect.y>=sp.texture.height-rows[i/8].y-1 && sp.rect.yMax<=sp.texture.height-rows[i/8].x).All(ok=>ok),"all woodlouse frames exclude every adjacent row's feet including both cart pullers");
            game.gait.Pose(0,0,false,Vector2.zero);var block=new MaterialPropertyBlock();game.hero.GetPropertyBlock(block);
            Check(!game.gait.Walking && game.gait.Frame==0 && block.GetFloat("_BodyFloor")>0,"stopping selects a separate paired standing pose");
            Check(game.art.lowerPoses.Cast<Sprite>().All(s=>s!=null && s.texture.name.StartsWith("WalkLower")) && game.hero.GetComponentsInChildren<MeshRenderer>().Length==0,"player lower body uses complete authored poses without isolated limb meshes");
            CaptureGaitFilmstrips();
            game.cameraView.orthographicSize=8;game.hero.flipX=false;game.walking=false;yield return null;
        }
        void CaptureGaitFilmstrips()
        {
            // These are photographs of the actual Unity renderers, including their torso
            // seam, lighting, pixel grid and scale. Numeric checks cannot judge anatomy.
            var stage=new Vector2(80,80);var backdrop=new GameObject("Animation visual review backdrop");
            var sr=backdrop.AddComponent<SpriteRenderer>();sr.sprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,2,2),Vector2.one*.5f,1);
            sr.sharedMaterial=game.art.lit;sr.color=new Color(.22f,.25f,.26f);sr.sortingOrder=-10000;backdrop.transform.position=stage;backdrop.transform.localScale=Vector3.one*20;
            var at=game.cameraView.transform.position;float size=game.cameraView.orthographicSize;
            game.cameraView.orthographicSize=2.15f;game.cameraView.transform.position=(Vector3)stage+new Vector3(0,.75f,-20);
            for(int kind=0;kind<3;kind++)for(int d=0;d<(kind==0?8:kind==1?4:1);d++)
            {
                var sheet=kind==0?game.art.hero:kind==1?game.journey.art.mage:game.journey.art.gestures;
                var actors=new List<GameObject>();var rendered=new HashSet<Sprite>();
                for(int frame=0;frame<9;frame++)
                {
                    int upper=frame==0?3:Mathf.Min(frame-1,sheet.GetLength(1)-1);
                    var actor=game.art.Renderer("Visual walk phase "+frame,sheet[d,upper],stage+new Vector2((frame-4)*.74f,0),2000);
                    actor.flipX=kind==0 && (d==5 || d==6);
                    var gait=new PixelGait(game.art,actor,sheet,kind==1?.21f:.43f);
                    gait.Pose(d,frame==0?0:(frame-1+.1f)/8,frame>0,Vector2.down,actor.flipX);
                    rendered.Add(gait.Lower.sprite);actors.Add(actor.gameObject);
                }
                Check(rendered.Count==9,"separate rest and eight authored walking poses render for "+kind+" direction "+d);
                game.cameraView.Render();var picture=ReadPixels();File.WriteAllBytes(Path.Combine(directory,"v16-walk-filmstrip-"+kind+"-"+d+".png"),picture.EncodeToPNG());Destroy(picture);
                foreach(var actor in actors)DestroyImmediate(actor);
            }
            DestroyImmediate(backdrop);game.cameraView.orthographicSize=size;game.cameraView.transform.position=at;
        }
        IEnumerator TavernAnimationChecks()
        {
            var j=game.journey;
            var keeperFrames=new HashSet<int>();
            for(int i=0;i<24;i++){j.clock=(i+.1f)*3/24;j.Tick(0);keeperFrames.Add(j.keeper.frame);Check(j.keeper.sprite.sprite==j.art.keeper[i],"plate-wiping keyframe "+i+" displayed in correct order");}
            Check(keeperFrames.Count==24 && JourneyWorld.ActorFrame(1,3)==0,"plate-wiping loop plays twenty-four frames and closes without skipping");
            Check(j.art.people[2,0].bounds.size.y<1.4f && j.art.mage[4,0].bounds.size.y<1.4f,"seated adults retain natural proportions instead of scaling their heads to standing height");
            for(int i=0;i<6;i++)
            {
                j.clock=i*.5f;j.Tick(0);game.cameraView.orthographicSize=2.5f;game.ResetCamera((Vector3)j.keeper.position+new Vector3(0,.7f,-20));game.cameraView.Render();
                var picture=ReadPixels();File.WriteAllBytes(Path.Combine(directory,"v14-keeper-wiping-"+i+".png"),picture.EncodeToPNG());Destroy(picture);
            }
            game.cameraView.orthographicSize=6.25f;game.ResetCamera(new Vector3(.5f,.35f,-20));
            foreach(int row in new[]{4,5})
            {
                int changes=0,last=JourneyWorld.ActorFrame(row,0);for(int i=1;i<=360;i++){int next=JourneyWorld.ActorFrame(row,i/50f);if(next!=last)changes++;last=next;}
                Check(changes<=18,"conversational gestures pause instead of switching at seven frames per second row "+row);
            }
            for(int i=0;i<8;i++){j.clock=i*.6f;j.Tick(0);yield return Capture("v12-tavern-animation-"+i+".png");}
            var dancer=j.actors.First(a=>a.row==0);j.clock=1;j.Tick(0);Check(dancer.gait!=null && dancer.gait.Walking,"dancer shifts weight with alternating knees and planted torso");j.clock=4.35f;j.Tick(0);Check(!dancer.gait.Walking && dancer.frame>=7,"dancer stops stepping to drink once per five-second cycle");
            yield return FieldAndHearthChecks();
        }
        IEnumerator FieldAndHearthChecks()
        {
            var j=game.journey;var at=game.body.position;var area=j.Area;var held=game.survivor.equipment[0];
            game.ui.open=false;game.survivor.equipment[0]=new Item(ItemKind.Orb);game.combat.manual=false;
            j.SetArea(WorldArea.GoldenField,new Vector2(-2.1f,5.1f));game.ResetCamera(new Vector3(0,8.0f,-20));game.cameraView.orthographicSize=6.4f;
            Check(j.innShadow && j.innShadow.transform.parent==j.fieldRoot.transform && j.innShadow.bounds.size.x>9,"tavern has a wide registered contact shadow on field ground");
            Check(Resources.Load<Texture2D>("Art/GoldenMeadowV15").wrapMode==TextureWrapMode.Repeat,"fine golden meadow is repeatable without clamped tile edges");
            yield return null;yield return Capture("v15-tavern-golden-field.png");
            game.combat.ResetForCheck();game.combat.manual=true;
            j.SetArea(WorldArea.Tavern,new Vector2(0,-3.2f));game.cameraView.orthographicSize=2.5f;game.ResetCamera(new Vector3(-5.75f,4.25f,-20));
            Check(j.art.hearth.Length==16 && j.art.hearth.All(s=>s.texture.name=="HearthFireV15") && j.hearthFire.sharedMaterial.shader.name=="AshenCoast/HearthFire","fireplace uses dedicated detailed sixteen-pose flame atlas and continuous interpolation");
            var scale=j.hearthFire.transform.localScale;var position=j.hearthFire.transform.position;var properties=new MaterialPropertyBlock();
            float maxJump=0;Texture2D previous=null;float minChange=float.MaxValue;
            for(int i=0;i<=144;i++)
            {
                j.clock=i/60f;j.Tick(0);game.cameraView.Render();var picture=ReadPixels();
                if(previous){float delta=RegionDifference(previous,picture,new Rect(-6.35f,3.42f,1.2f,1.1f));maxJump=Mathf.Max(maxJump,delta);if(delta>.00001f)minChange=Mathf.Min(minChange,delta);Destroy(previous);}previous=picture;
                if(i%4==0)File.WriteAllBytes(Path.Combine(directory,"v15-hearth-smooth-"+i+".png"),picture.EncodeToPNG());
            }
            Destroy(previous);Check(minChange<float.MaxValue && maxJump<.045f,"consecutive 60 Hz hearth frames change gradually across the entire loop (maximum delta "+maxJump.ToString("F4")+")");
            Check(j.hearthFire.transform.localScale==scale && j.hearthFire.transform.position==position,"hearth logs and flame base stay registered throughout animation");
            j.clock=0;j.Tick(0);j.hearthFire.GetPropertyBlock(properties);var firstA=properties.GetVector("_FrameA");
            j.clock=JourneyWorld.HearthCycleSeconds;j.Tick(0);j.hearthFire.GetPropertyBlock(properties);Check(properties.GetVector("_FrameA")==firstA && properties.GetFloat("_Blend")==0,"hearth cycle wraps to its first frame exactly");
            game.cameraView.orthographicSize=6.25f;game.ResetCamera(new Vector3(.5f,.35f,-20));game.combat.manual=false;yield return null;yield return Capture("v15-tavern-complete-interior.png");
            game.survivor.equipment[0]=held;game.combat.ResetForCheck();j.SetArea(area,at);game.combat.manual=true;game.cameraView.orthographicSize=8;
        }
        float MeanBrightness()
        {
            var t=ReadPixels();var pixels=t.GetPixels();float sum=0;foreach(var p in pixels)sum+=p.grayscale;Destroy(t);return sum/pixels.Length;
        }
        float BrightnessAt(Vector2 point)
        {
            var t=ReadPixels();var uv=game.cameraView.WorldToViewportPoint(point);int x=Mathf.Clamp((int)(uv.x*t.width),2,t.width-3),y=Mathf.Clamp((int)(uv.y*t.height),2,t.height-3);
            float sum=0;for(int dy=-2;dy<=2;dy++)for(int dx=-2;dx<=2;dx++)sum+=t.GetPixel(x+dx,y+dy).grayscale;
            Destroy(t);return sum/25;
        }
        Texture2D ReadPixels()
        {
            var rt=game.pixelView.pixels;var old=RenderTexture.active;RenderTexture.active=rt;
            var t=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);t.Apply();RenderTexture.active=old;return t;
        }
    }
}
