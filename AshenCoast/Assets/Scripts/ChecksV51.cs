using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace AshenCoast
{
    // Focused release checks: only the new V51 systems and their network messages.
    public sealed class ChecksV51:MonoBehaviour
    {
        public CoastGame game;string folder,role;int number,passed,failed;readonly List<string> report=new List<string>();
        string PathFor(string name)=>Path.Combine(folder,name);
        void Check(bool ok,string name){var line=(ok?"PASS ":"FAIL ")+name;report.Add(line);Debug.Log(line);if(ok)passed++;else failed++;}
        void Mark(string name,string value="ready")=>File.WriteAllText(PathFor(name),value);
        string Stage=>File.Exists(PathFor("stage.txt"))?File.ReadAllText(PathFor("stage.txt")):"";
        void Step(string value)=>Mark("stage.txt",value);
        IEnumerator Until(Func<bool> condition,string name,float timeout=25)
        {float end=Time.realtimeSinceStartup+timeout;while(!condition() && Time.realtimeSinceStartup<end)yield return null;Check(condition(),name);}
        IEnumerator Start()
        {
            string[] args=Environment.GetCommandLineArgs();string Arg(string key,string fallback){int i=Array.IndexOf(args,key);return i>=0 && i+1<args.Length?args[i+1]:fallback;}
            folder=Arg("-v51-output",Path.Combine(Application.persistentDataPath,"Verification-V51"));Directory.CreateDirectory(folder);role=Arg("-v51-role","offline");number=int.Parse(Arg("-v51-number","0"));
            Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=60;AudioListener.volume=0;yield return null;game.testMode=true;game.snails.SetupOpen=false;game.paused=false;Time.timeScale=1;game.seconds=60;
            IEnumerator routine=args.Contains("-v51-rune-check")?Rune():args.Contains("-v51-presentation-check")?Presentation():role=="offline"?Offline():role=="host"?Host():Client();
            while(true){bool move;object current=null;try{move=routine.MoveNext();if(move)current=routine.Current;}catch(Exception e){Check(false,e.ToString());break;}if(!move)break;yield return current;}
            report.Add("RESULT "+passed+" passed; "+failed+" failed");File.WriteAllLines(PathFor(role+number+"-report.txt"),report);Application.Quit(failed==0?0:1);
        }
        Item PrepareBook()
        {
            var s=game.survivor;var b=s.inventory.items.First(i=>i.kind==ItemKind.Pyromancy);var kept=s.inventory.items.FirstOrDefault(i=>i.kind==ItemKind.Spectrum);var seed=s.inventory.items.FirstOrDefault(i=>i.kind==ItemKind.ReturnScroll);s.inventory.items.Clear();b.opened=true;s.inventory.Place(b,0,0);if(kept!=null)s.inventory.Place(kept,5,0);if(seed!=null)s.inventory.Place(seed,6,0);
            if(!b.spells.Any(slot=>slot.kind==SpellKind.Return)){var scroll=s.inventory.items.FirstOrDefault(i=>i.kind==ItemKind.ReturnScroll)??new Item(ItemKind.ReturnScroll);if(!s.inventory.items.Contains(scroll))s.inventory.items.Add(scroll);BookSpells.Insert(b,1,scroll,s.inventory);}s.mana=s.MaxMana;return b;
        }
        IEnumerator Rune()
        {
            var seed=ReturnRuneV51.Template.Select(p=>p*240).ToArray();
            for(int r=0;r<4;r++){int turn=r;var points=seed.Select(p=>(Vector2)(Quaternion.Euler(0,0,turn*90)*p)).ToArray();Check(ReturnRuneV51.Recognize(points),"closed half-circle orientation "+r);}
            Check(ReturnRuneV51.Recognize(seed.Reverse().ToArray()),"reverse drawing of half-circle");
            foreach(var glyph in new[]{SpellGlyph.Circle,SpellGlyph.Triangle,SpellGlyph.Diamond})
            {var points=SpatialSigil.Resample(SpellGlyphs.Path(glyph),96).Select(p=>p*240).ToArray();Check(!ReturnRuneV51.Recognize(points),glyph+" cannot trigger resurrection");}
            Check(!ReturnRuneV51.Recognize(seed.Take(41).ToArray()),"open arc needs its closing chord");
            yield return null;
        }
        IEnumerator Presentation()
        {
            var g=game;var c=g.snails;c.SetupOpen=true;g.ChooseCharacterV51(CharacterV51.Frog);c.SetupOpen=false;
            g.body.position=c.campCentreV51;g.hero.transform.position=g.body.position;
            var h=c.houses[0];h.Pose(0);var before=h.HeadVerticesV51;h.moving=true;h.Pose(.6f);
            Check(before.Where((v,i)=>Vector3.Distance(v,h.HeadVerticesV51[i])>.0001f).Any(),"crawl moves the textured head and foot mesh");
            Check(h.Renderer.color.a>.99f,"an emerged snail is fully opaque");
            Check(g.hero.sprite.texture.name=="FrogHeroV51" && g.hero.enabled,"frog uses its full authored body");
            Capture("snails-full-heads.png",c.campCentreV51+Vector2.up*2,13);Capture("frog-player.png",c.campCentreV51+Vector2.up,3.1f);
            h.Hide();yield return new WaitForSeconds(1.2f);Check(h.WithdrawalV51>.95f,"withdrawal reaches the closed shell");Capture("snail-retracted.png",h.position+Vector2.up*2,5);
            h.hidden=0;yield return new WaitForSeconds(1.4f);Check(h.WithdrawalV51<.01f,"head smoothly emerges again");
            var mage=g.journey.companion;mage.Hire(g.body.position);c.Assign("mage",1,0);c.Configure(4);
            for(int i=0;i<4;i++)
            {
                yield return new WaitForSeconds(1.05f);c.Enter(i);g.body.position=new Vector2(.4f,-2.35f);g.hero.transform.position=g.body.position;yield return null;
                Capture("room-"+(i+1)+".png",new Vector2(0,-.6f),4.1f);Check(c.houses[i].room.activeSelf,"unique shell room "+(i+1)+" renders with furniture");c.Leave();
            }
            var centre=c.campCentreV51;var slave=g.amazons.people.First(p=>p.IsSlave);slave.Hired=true;slave.Captive=false;c.Assign(c.Key(slave.Renderer),2,0);
            var t=c.troops[c.Key(slave.Renderer)];t.campRestV51=t.seatedV51=t.fluteV51=true;slave.Renderer.transform.SetParent(g.coastRoot.transform,true);slave.Renderer.gameObject.SetActive(true);
            mage.Warp(centre+Vector2.left*.8f);mage.sprite.transform.SetParent(g.coastRoot.transform,true);var mt=c.troops["mage"];mt.campRestV51=mt.makerV51=true;mt.seatedV51=false;
            c.TickCampTroopV51(mt,1.3f);c.CampVisualsV51();foreach(var snail in c.houses)snail.Pose(1);Capture("circle-camp-flute.png",centre+Vector2.up*2,13);yield return null;
        }
        IEnumerator Offline()
        {
            var g=game;var c=g.snails;c.enabled=false;
            c.SetupOpen=true;g.ChooseCharacterV51(CharacterV51.Frog);Check(g.survivor.health==50 && g.survivor.MaxMana==200,"frog has 50 health and 200 mana");g.survivor.mana=200;g.ChooseCharacterV51(CharacterV51.Human);Check(g.survivor.health==100 && g.survivor.mana==100,"switching back clamps human mana");g.ChooseCharacterV51(CharacterV51.Frog);c.SetupOpen=false;
            Check(g.survivor.inventory.items.Any(i=>i.kind==ItemKind.ReturnScroll),"new resurrection scroll is available at the start");
            var h=c.houses[0];h.Pose(0);var first=h.HeadVerticesV51;h.moving=true;h.Pose(.6f);Check(first.Where((v,i)=>Vector3.Distance(v,h.HeadVerticesV51[i])>.0001f).Any(),"crawl deforms head and foot mesh");Check(c.shells[0].rect.width>800,"full head remains inside sprite bounds");
            Capture("caravan.png",c.campCentreV51+Vector2.up*2,13);
            h.Hide();yield return new WaitForSeconds(1.15f);for(int i=0;i<70;i++)h.Pose(i*.02f);Check(h.WithdrawalV51>.95f,"snail smoothly retracts into its shell");Capture("retracted.png",h.position+Vector2.up*2,5.2f);h.hidden=0;yield return new WaitForSeconds(1.3f);for(int i=0;i<90;i++)h.Pose(i*.02f);
            var mage=g.journey.companion;mage.Hire(g.body.position);c.Assign("mage",1,0);var slave=g.amazons.people.First(p=>p.IsSlave);slave.Hired=true;slave.Captive=false;c.Assign(c.Key(slave.Renderer),2,0);var amazon=g.amazons.people.First(p=>!p.IsSlave && p.Weapon?.kind==ItemKind.Bow);amazon.Hired=true;c.Assign(c.Key(amazon.Renderer),3,0);
            var goal=c.campCentreV51+Vector2.up*24;bool old=c.ordered;c.Command(new CaravanCommandV50{kind="order",x=200,y=220});Check(c.ordered==old,"unexplored destination is rejected");g.map.regions[0].Reveal(goal,12);c.Command(new CaravanCommandV50{kind="order",x=goal.x,y=goal.y});Check(c.campPhaseV51==1 && c.gatherV51==60 && c.troops.Values.All(t=>t.returning),"order starts a minute of gathering for every ally");var start=h.position;c.CampTickV51(59);Check(c.campPhaseV51==1 && h.position==start,"caravan stays still during gathering");c.CampTickV51(1);Check(c.campPhaseV51==2 && c.troops.Values.All(t=>t.resting),"late allies are placed inside after sixty seconds");h.hidden=3;Check(c.CampTickV51(.1f),"a hidden snail blocks the whole caravan");h.hidden=0;c.stopped=15;c.campPhaseV51=3;Check(c.CampTickV51(.1f) && h.position==start,"whistle also stops the final circle formation");c.stopped=0;
            c.campCentreV51=goal;for(int i=0;i<4;i++)c.houses[i].position=c.CampSlotV51(i);c.CampTickV51(.1f);Check(c.campPhaseV51==4 && !c.ordered && c.houses.Select(house=>Vector2.Distance(house.position,goal)).All(d=>d>7),"arrival makes four positions around the camp centre");
            var troop=c.troops["mage"];troop.resting=false;troop.campRestV51=true;troop.makerV51=true;troop.seatedV51=false;mage.sprite.transform.SetParent(g.coastRoot.transform,true);mage.sprite.gameObject.SetActive(true);mage.Warp(goal+Vector2.left*.8f);c.TickCampTroopV51(troop,1.3f);Check(g.arcane.campfires.Any(f=>!f.exploded && Vector2.Distance(f.position,goal)<1.5f),"allied mage creates the campfire");
            var captive=c.troops[c.Key(slave.Renderer)];captive.resting=false;captive.campRestV51=captive.seatedV51=captive.fluteV51=true;slave.Renderer.transform.SetParent(g.coastRoot.transform,true);slave.Renderer.gameObject.SetActive(true);c.CampVisualsV51();Check(c.logSeatsV51.Count==4 && slave.Renderer.sprite.texture.name=="CampPeopleV51","four log seats and animated flute player");foreach(var house in c.houses)house.Pose(1);Capture("settled-camp.png",goal+Vector2.up*2,13);
            c.enabled=true;c.enabled=true;Check(c.Order(goal+Vector2.right*20),"new camp has a traversable route");c.gatherV51=.01f;yield return Until(()=>c.campPhaseV51==4,"real caravan travel finishes in a circle",35);c.Configure(4);for(int i=0;i<4;i++){yield return new WaitForSeconds(1.05f);c.Enter(i);g.body.position=new Vector2(0,-2.5f);g.hero.transform.position=g.body.position;c.houses[i].Pose(1);Capture("shell-room-"+(i+1)+".png",new Vector2(0,-.6f),4.1f);Check(c.houses[i].room.activeSelf && c.exit[i].transform.position.y>g.body.position.y,"room "+(i+1)+" has a reachable back-wall door");c.Leave();}
            c.enabled=true;var book=PrepareBook();var bug=g.woodlice.First();bug.vitals.health=1;bug.Hurt(30);string key=g.coop.world.Key(bug.renderer);yield return null;yield return null;Check(g.corpses.HasBody(key),"dead enemy leaves a corpse");var body=g.corpses.bodies.Values.First(b=>b.data.actor==key).data;var world=ReturnRuneV51.Template.Select(p=>new Vector2(body.x,body.y)+(p-Vector2.one*.5f)*4).ToArray();var screen=ReturnRuneV51.Template.Select(p=>p*240).ToArray();
            Check(ReturnRuneV51.Recognize(screen) && ReturnRuneV51.Recognize(screen.Reverse().ToArray()) && ReturnRuneV51.Recognize(screen.Select(p=>(Vector2)(Quaternion.Euler(0,0,90)*p)).ToArray()),"closed half-circle recognizes both drawing directions and rotation");Check(!ReturnRuneV51.Recognize(screen.Take(41).ToArray()) && !ReturnRuneV51.Recognize(Enumerable.Range(0,65).Select(i=>new Vector2(Mathf.Cos(i*Mathf.PI/32),Mathf.Sin(i*Mathf.PI/32))*120).ToArray()),"open arc and full circle do not resurrect");
            float mana=g.survivor.mana;Check(g.spellcraft.CastReturnV51(screen,world,new Vector2(body.x,body.y)),"new rune activates the scroll");Check(Mathf.Abs(bug.vitals.health-bug.vitals.maximumHealth*.5f)<.01f && g.survivor.mana==mana-21 && g.corpses.Pending==null,"enemy revives at half health and charges discounted mana once");
            mage.health=0;yield return null;yield return null;Check(g.corpses.HasBody("mage"),"allied mage also leaves a corpse");var mbody=g.corpses.bodies.Values.First(b=>b.data.actor=="mage").data;g.corpses.RequestRaise(mbody,book);Check(mage.Hired && mage.health==CompanionMage.MaxHealth*.5f && c.troops.ContainsKey("mage"),"resurrected ally retains allegiance and assigned home");
            var gull=g.journey.gulls[0];gull.health=0;yield return null;yield return null;var dead=g.corpses.bodies.Values.First(b=>b.data.actor==g.coop.world.Key(gull.sprite));dead.data.age=59.99f;yield return new WaitForSeconds(.1f);Check(!g.corpses.HasBody(dead.data.actor),"corpses expire after sixty seconds without respawning");
            g.body.position=bug.Position+Vector2.left;g.hero.transform.position=g.body.position;Capture("frog-and-corpses.png",bug.Position+Vector2.up,5.0f);
            g.dummyHealth=500;g.arcane.EmitCone(g.dummy.bounds.center-Vector3.right*.15f,Vector2.right,false,1,5,0);g.arcane.Tick(.04f);Check(Mathf.Approximately(g.dummyHealth,470),"lamp contact damage is tripled to thirty");
        }
        IEnumerator Host()
        {
            var g=game;var s=g.coop;s.PlayerName="HostV51";Check(s.StartHost(29551,true),"V51 host starts");Mark("host-ready");yield return Until(()=>s.players.Count==4 && s.players.Values.All(p=>p.equipment?.Length==8),"four V51 peers exchange complete states",50);
            yield return Until(()=>s.players.Values.Any(p=>p.name=="Client1" && p.character==1 && p.health==50 && p.mana==200),"frog race and extended mana synchronize");Step("die");yield return Until(()=>g.corpses.bodies.Values.Any(b=>b.data.peer>0 && s.players[b.data.peer].name=="Client1"),"host creates dead player's body");Step("raise");yield return Until(()=>File.Exists(PathFor("client2-raised")),"other client casts resurrection",30);yield return Until(()=>s.players.Values.Any(p=>p.name=="Client1" && p.health==25),"frog player returns with half health");Check(s.players.Values.Count(p=>p.health>0)==4,"all four players remain in the world");
            Step("npc");var bug=g.woodlice.First();bug.vitals.health=1;bug.Hurt(30);yield return Until(()=>g.corpses.HasBody(s.world.Key(bug.renderer)),"host-authoritative enemy leaves synchronized corpse");Mark("enemy-key",s.world.Key(bug.renderer));yield return Until(()=>File.Exists(PathFor("client3-raised")),"third client revives enemy",30);yield return Until(()=>bug.vitals.health>0,"enemy resurrection reaches simulation owner");Check(bug.vitals.health==bug.vitals.maximumHealth*.5f,"network enemy restores half health");
            var c=g.snails;var goal=c.campCentreV51+Vector2.up*24;g.map.regions[0].Reveal(goal,10);c.Command(new CaravanCommandV50{kind="order",x=goal.x,y=goal.y});yield return Until(()=>Enumerable.Range(1,3).All(i=>File.Exists(PathFor("client"+i+"-gather"))),"gathering timer and route reach all peers");c.gatherV51=.05f;yield return Until(()=>c.campPhaseV51==2,"network caravan finishes gathering");c.houses[2].hidden=2;var before=c.houses[0].position;yield return new WaitForSeconds(.4f);Check(Vector2.Distance(before,c.houses[0].position)<.01f,"hidden member stops authoritative network caravan");Step("done");yield return new WaitForSeconds(1);s.Disconnect();
        }
        IEnumerator Client()
        {
            var g=game;var s=g.coop;g.snails.SetupOpen=true;if(number==1)g.ChooseCharacterV51(CharacterV51.Frog);g.snails.SetupOpen=false;g.survivor.mana=g.survivor.MaxMana;string itemID=g.survivor.inventory.items.First(i=>i.kind==ItemKind.Spectrum).id;s.PlayerName="Client"+number;yield return Until(()=>File.Exists(PathFor("host-ready")),"host is ready");s.Join("127.0.0.1",29551);yield return Until(()=>s.Online && s.inventory.Ready,"client joins V51 session",50);yield return Until(()=>Stage=="die","death stage");if(number==1)g.survivor.health=0;
            yield return Until(()=>Stage=="raise","resurrection stage");if(number==2){var book=PrepareBook();yield return Until(()=>g.corpses.bodies.Values.Any(b=>b.data.peer>=0 && s.players.TryGetValue(b.data.peer,out var p) && p.name=="Client1"),"client sees teammate corpse");var body=g.corpses.bodies.Values.First(b=>b.data.peer>=0 && s.players.TryGetValue(b.data.peer,out var p) && p.name=="Client1").data;float before=g.survivor.mana;g.corpses.RequestRaise(body,book);yield return Until(()=>g.corpses.Pending==null,"resurrection acknowledgement");Check(g.survivor.mana==before-21,"client pays resurrection mana exactly once");Mark("client2-raised");}
            if(number==1){yield return Until(()=>g.survivor.health>0,"dead client receives resurrection");Check(g.survivor.health==25 && g.survivor.character==CharacterV51.Frog && g.survivor.inventory.items.Any(i=>i.id==itemID),"resurrection preserves character and private inventory");}
            yield return Until(()=>Stage=="npc","enemy resurrection stage");if(number==3){var book=PrepareBook();yield return Until(()=>File.Exists(PathFor("enemy-key")) && g.corpses.HasBody(File.ReadAllText(PathFor("enemy-key"))),"client receives enemy corpse");var body=g.corpses.bodies.Values.First(b=>b.data.actor==File.ReadAllText(PathFor("enemy-key"))).data;g.corpses.RequestRaise(body,book);yield return Until(()=>g.corpses.Pending==null,"enemy resurrection acknowledgement");Mark("client3-raised");}
            yield return Until(()=>g.snails.campPhaseV51==1 && g.snails.gatherV51>0 && g.snails.MapRouteV51.Length>1,"caravan gathering and route synchronize");Mark("client"+number+"-gather");yield return Until(()=>Stage=="done","host finishes focused V51 checks");s.Disconnect();
        }
        void Capture(string name,Vector2 centre,float size)
        {
            var camera=new GameObject("V51 visual check camera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=size;camera.transform.position=new Vector3(centre.x,centre.y,-20);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.07f,.06f,.045f);camera.nearClipPlane=.1f;camera.farClipPlane=100;
            var target=new RenderTexture(1440,900,24);camera.targetTexture=target;camera.Render();var old=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(1440,900,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1440,900),0,0);image.Apply();File.WriteAllBytes(PathFor(name),image.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;target.Release();Destroy(target);Destroy(image);Destroy(camera.gameObject);
        }
    }
}
