using System.Collections;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        IEnumerator VillageChangesV48()
        {
            var g=game;var town=g.amazons;g.ui.CloseContainers();HandClearV34();g.journey.SetArea(WorldArea.Coast,town.Centre+Vector2.down*5);g.seconds=60;g.meadow.Refresh(g.body.position,true);g.UpdateEnvironment(0);
            var meter=new DummyDamageV48();meter.Record(90,0);meter.Record(90,1);meter.Record(90,2);Check(Mathf.Abs(meter.Value(3)-90)<.01f && meter.Value(5)==0,"dummy DPS averages actual player damage over three seconds and expires after inactivity");
            g.DamageDummy(30,false);Check(g.DummyDpsV48(g.dummy)==0,"damage from other actors does not count as the player's DPS");g.DamageDummy(30);Check(g.DummyDpsV48(g.dummy)==30,"player hits feed the shore mannequin DPS display");
            g.HurtTarget(town.targets[0],20,g.hero);Check(g.DummyDpsV48(town.targets[0].Renderer)==20,"village training targets also track player DPS");
            var posts=town.root.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name=="Upright side fence post").ToArray();
            Check(posts.Length==22 && posts.All(r=>Mathf.Abs(Mathf.DeltaAngle(r.transform.eulerAngles.z,0))<.01f),"both side fences use eleven upright posts, without rotated whole panels");
            Check(town.root.GetComponentsInChildren<SpriteRenderer>().Count(r=>r.name=="Connected side fence rail")==40,"side rails join every neighbouring pair of posts");
            Check(town.fires.Take(12).All(f=>Vector2.Distance(f.point,town.Centre)<2.3f),"statue bonfire ring hugs the pedestal");
            Check(town.root.GetComponentsInChildren<SpriteRenderer>().Count(r=>r.name=="Village cobblestone path")>100,"every house joins the paved village ring and training entrance");
            var route=town.PathV48(town.Centre+new Vector2(15,0),town.Centre+new Vector2(5,-3));
            Check(route.Count>5 && route.Any(p=>p.y<town.Centre.y-8),"persistent path goes through the south gate rather than crossing side fences");
            var guards=town.people.Where(p=>!p.IsSlave && !p.Hunter && !p.Slaver && !p.WeaponMerchantV48 && !p.HuntingV48).ToArray();var starts=guards.Select(p=>p.Position).ToArray();
            for(int n=0;n<160;n++){town.Tick(.05f);if(n%40==0)yield return null;}
            Check(guards.Where((p,i)=>Vector2.Distance(p.Position,starts[i])>3).Count()>=8,"most guards traverse meaningful patrol routes instead of tiny idle oscillations");
            Check(town.people.All(p=>!p.weaponSprite.enabled),"valkyries keep weapons out of their hands while not attacking");
            Check(town.people.Count(p=>p.HuntingV48)==3 && town.people.Where(p=>p.HuntingV48).All(p=>Vector2.Distance(p.Position,town.Centre)>25),"three additional hunting Amazons patrol outside the village");
            var merchant=town.people.Single(p=>p.WeaponMerchantV48);Check(merchant.stock.items.Count==7 && merchant.stock.items.All(i=>i.Definition.Melee),"weapon merchant carries ordinary melee weapons");
            g.survivor.inventory.items.Clear();g.survivor.inventory.Add(new Item(ItemKind.Coin){count=20});var sword=merchant.stock.items.First(i=>i.kind==ItemKind.Sword);int coins=g.survivor.Coins;
            Check(g.survivor.inventory.FindSpace(sword,out var at) && g.survivor.Purchase(merchant.stock,sword,g.survivor.inventory,at.x,at.y) && g.survivor.Coins==coins-8 && !merchant.stock.items.Contains(sword),"new sword sale uses the existing paid purchase flow with an eight-coin price");
            g.seconds=119;town.Tick(.01f);Check(!guards.Any(p=>p.SparringV48),"training has not started more than one minute before night");
            var hp=town.people.Select(p=>p.health).ToArray();g.seconds=120;town.Tick(.01f);var participants=guards.Where(p=>p.SparringV48).ToArray();
            Check(participants.Length>=8 && participants.Length%2==0 && participants.All(p=>p.SparringPartnerV48.SparringPartnerV48==p),"a randomized majority pairs up exactly one minute before night");
            for(int n=0;n<1000;n++){town.Tick(.05f);if(n%80==0)yield return null;}
            Check(participants.All(p=>p.AttackCount>0),"each selected woman reaches the yard and performs animated sparring attacks");
            Check(town.people.Select(p=>p.health).SequenceEqual(hp),"practice melee and bow shots do not damage participants or bystanders");
            g.seconds=240;town.Tick(.1f);Check(!guards.Any(p=>p.SparringV48),"late-night training ends and patrols resume");
            var bandit=town.plains.Spawn(town.Centre+new Vector2(33,-12),true);var swordsman=town.plains.Spawn(town.Centre+new Vector2(36,-12),false);
            Check(g.WorldCreatures.Contains(bandit) && g.WorldCreatures.Contains(swordsman) && town.Enemy(bandit),"rare plains archers and swordsmen join combat and Amazon enemy targeting");
            var actor=guards[0];actor.Warp(swordsman.Position+Vector2.left*1.2f);actor.Tick(.01f);Check(actor.Attacking && actor.weaponSprite.enabled,"real combat makes a woman's weapon visible");actor.Tick(1);Check(!actor.weaponSprite.enabled,"weapon disappears as soon as an attack ends");
            town.Alarm(guards[1],g.hero);Check(!town.people.Any(p=>p.SparringPartnerV48!=null),"real aggression cancels every practice pairing");foreach(var p in town.people)p.hostile=false;
            bandit.health=swordsman.health=0;bandit.Renderer.enabled=swordsman.Renderer.enabled=false;town.plains.bandits.Clear();
            g.seconds=150;town.Tick(.1f);g.checkCameraSize=25;g.checkCameraPosition=new Vector3(town.Centre.x,town.Centre.y+5,-20);g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);yield return DrawUIV34();yield return Capture("v48-village-paths-fences.png");
            town.EnterHouse(town.houses.First(h=>h.chief));g.checkCameraSize=6.4f;g.checkCameraPosition=new Vector3(0,.8f,-20);g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);yield return DrawUIV34();
            Check(town.roomRoot.GetComponentsInChildren<SpriteRenderer>().Any(r=>r.name=="Carved fur-lined chieftain throne") && town.roomRoot.GetComponentsInChildren<SpriteRenderer>().Count(r=>r.sprite.texture.name=="AmazonFurnishingsV48")==14,"active chief interior uses fourteen tribal mats, rugs, herbs, baskets, walls and throne");yield return Capture("v48-chief-interior.png");town.LeaveHouse();g.checkCameraSize=null;g.checkCameraPosition=null;
        }
        IEnumerator VillageFinalV48()
        {
            var g=game;var town=g.amazons;g.seconds=150;g.journey.SetArea(WorldArea.Coast,town.Centre+Vector2.down*6);g.meadow.Refresh(g.body.position,true);
            for(int n=0;n<800;n++){town.Tick(.05f);if(n%80==0)yield return null;}
            Check(Enumerable.Range(0,4).All(row=>Enumerable.Range(0,6).Select(frame=>town.art.women[row,frame].pixelsPerUnit).Distinct().Count()==1),"each Amazon uses one fixed pixel scale across her walking frames");
            g.checkCameraSize=10;g.checkCameraPosition=new Vector3(town.Centre.x,town.Centre.y+2,-20);g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);yield return DrawUIV34();yield return Capture("v48-training-fence-detail.png");
            town.EnterHouse(town.houses[0]);g.checkCameraSize=6.4f;g.checkCameraPosition=new Vector3(0,.8f,-20);g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);yield return DrawUIV34();
            var props=town.roomRoot.GetComponentsInChildren<SpriteRenderer>();Check(props.Count(r=>r.sprite.texture.name=="AmazonFurnishingsV48")==14 && props.Any(r=>r.name=="Carved fur-lined chieftain throne"),"chief hall has all fourteen authored furnishings");
            var rug=props.First(r=>r.name=="Deer hide gathering rug");Check(rug.bounds.min.y>.2f && town.currentHouse.furnishings.All(s=>Mathf.Abs(s.Position.x)>=5.4f),"hide rug is clear of the hearth and storage pots sit beside sleeping mats");yield return Capture("v48-chief-interior.png");town.LeaveHouse();
            g.journey.SetArea(WorldArea.Coast,g.DummyBase+Vector2.left*2);g.seconds=60;g.DamageDummy(90);g.checkCameraSize=4;g.checkCameraPosition=new Vector3(g.DummyBase.x,g.DummyBase.y+1,-20);g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);yield return DrawUIV34();yield return Capture("v48-mannequin-dps.png");Check(g.DummyDpsV48(g.dummy)>0,"DPS remains visible above the actual shore mannequin after a player hit");
        }
    }
}
