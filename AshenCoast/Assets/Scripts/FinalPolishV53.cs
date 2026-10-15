using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class PolishChecksV53
    {
        IEnumerator ArmOnlyV53()
        {
            var g=game;g.paused=true;g.snails.SetupOpen=true;g.ChooseCharacterV51(CharacterV51.Frog);g.snails.SetupOpen=false;g.survivor.equipment[0]=new Item(ItemKind.Spectrum);g.spectrum.AdvanceCharge(.6f,g.body.position+Vector2.right);g.spectrum.PoseRaiseV43();
            Check(g.spectrum.raisedArmV43.sprite.bounds.size.x<.15f,"frog arm crop excludes torso");Check(g.spectrum.RaisedHandV43.y-g.hero.transform.position.y>.85f && g.spectrum.RaisedHandV43.y-g.hero.transform.position.y<1.05f,"frog hand raises to head height");Capture("frog-arm-final.png",(Vector2)g.hero.transform.position+Vector2.up*.65f,1.8f);
            g.snails.SetupOpen=true;g.ChooseCharacterV51(CharacterV51.Human);Check(!g.spectrum.raisedArmV43,"race change clears old raised limb");yield return null;
        }
        IEnumerator FinalV53()
        {
            var g=game;g.paused=true;Time.timeScale=0;
            var state=g.coop.CapturePlayer();state.peer=97;state.character=1;state.region="area/2";state.health=50;state.frame=2;state.facing=0;state.scale=1;var proxy=new RemotePlayerV49(g.coop,state);proxy.Tick(.1f);Check(proxy.Renderer.sprite.texture.name=="FrogHeroV51" && proxy.Renderer.sprite.bounds.size.y<1.1f && !proxy.Renderer.gameObject.activeSelf,"hidden remote frog keeps race sprite in another location");proxy.Dispose();
            g.snails.SetupOpen=true;g.ChooseCharacterV51(CharacterV51.Frog);g.snails.SetupOpen=false;g.survivor.equipment[0]=new Item(ItemKind.Spectrum);Call(g.combat,"SetupWeapon",g.survivor.equipment[0]);Call(g.combat,"PoseIdle",g.survivor.equipment[0]);Check(g.combat.weapon.transform.position.y-g.hero.transform.position.y<.6f,"frog holds blade at actual hand height");
            g.spectrum.AdvanceCharge(.6f,g.body.position+Vector2.right*2);g.spectrum.PoseRaiseV43();Check(g.spectrum.raisedArmV43 && g.spectrum.RaisedHandV43.y-g.hero.transform.position.y<1.05f,"frog raises blade with scaled shoulder and arm");Capture("frog-spectrum-raised.png",(Vector2)g.hero.transform.position+Vector2.up*.6f,1.8f);
            g.snails.SetupOpen=true;g.ChooseCharacterV51(CharacterV51.Human);g.snails.SetupOpen=false;g.survivor.equipment[0]=null;g.combat.ResetForCheck();
            for(int home=0;home<4;home++){typeof(SnailHomesV50).GetField("doorWait",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g.snails,0f);g.snails.Enter(home);g.hero.transform.position=g.body.position=new Vector2(.2f,-1.85f);Capture("room-final-"+home+".png",new Vector2(.3f,-.15f),4.8f);var pot=g.buildings.stations.First(s=>s.world==g.snails.houses[home].room.transform);Check(pot.cell==new Vector2Int(1,-3) && g.snails.exit[home].bounds.min.y<.2f,"door grounded and vessel inside floor, room "+home);g.snails.Leave();}
            var h=g.snails.houses[0];h.moving=true;h.PoseBodyV51(.1f);var a=h.HeadVerticesV51;h.PoseBodyV51(.7f);var b=h.HeadVerticesV51;Check(a.Where((v,i)=>Vector3.Distance(v,b[i])>.0001f).Count()>50,"cached snail mesh still animates head and travelling foot");h.moving=false;
            var resident=g.journey.residents[0];resident.health=50;resident.PeaceV53();Check(!g.amazons.Enemy(resident),"allies do not reacquire a resurrected peaceful resident");
            var bindings=g.coop.world.BindingsV51().Where(x=>x.kind!="dummy" && x.kind!="target" && x.kind!="tree" && x.kind!="dragon" && x.kind!="hunter").GroupBy(x=>g.corpses.Family(x)).Select(x=>x.First()).ToArray();foreach(var binding in bindings)binding.setHealth(0);g.corpses.RefreshBindingsV53();g.corpses.ScanV53();var gallery=new GameObject("Death animation comparison");int row=0;
            foreach(var binding in bindings)
            {
                var visual=g.corpses.bodies.Values.First(x=>x.data.actor==binding.key);Check(visual.fall!=null,"death begins from live pose for "+binding.kind);
                for(int frame=0;frame<3;frame++)
                {visual.data.age=frame==0?.05f:frame==1?.35f:.8f;Call(g.corpses,"PoseBodyV53",visual);var source=visual.fall?visual.fall:visual.sprite;var r=g.art.Renderer("Death stage",source.sprite,new Vector2(1000+frame*4,1000-row*2.8f),100+row);r.transform.SetParent(gallery.transform,true);r.gameObject.layer=31;r.transform.localScale=source.transform.lossyScale;r.transform.rotation=source.transform.rotation;r.color=Color.white;}
                row++;
            }
            Capture("death-families.png",new Vector2(1004,1001-(row-1)*1.4f),Mathf.Max(4,row*1.5f),1<<31);Destroy(gallery);yield return null;
        }
        IEnumerator ProxyHostV53()
        {
            var g=game;Check(g.coop.StartHost(27954,true),"proxy host starts");MarkV53("proxy-host-ready");yield return UntilV53(()=>g.coop.remote.Values.Any(p=>p.State.character==1 && p.State.region=="area/2"),"frog guest transitions to another location");
            var proxy=g.coop.remote.Values.First(p=>p.State.character==1);Check(proxy.Renderer.sprite.texture.name=="FrogHeroV51" && proxy.Renderer.sprite.bounds.size.y<1.1f,"real hidden proxy has correct frog height");
            yield return UntilV53(()=>g.corpses.bodies.Values.Any(b=>b.data.peer==proxy.Peer && b.fall),"remote player death begins animation");var body=g.corpses.bodies.Values.First(b=>b.data.peer==proxy.Peer);Check(body.fall.sprite.texture.name=="FrogHeroV51" && body.data.family==8,"remote frog dies using frog pose while host is elsewhere");MarkV53("proxy-host-verified");yield return UntilV53(()=>MarkedV53("proxy-guest-finished"),"proxy guest completed");g.coop.Disconnect(false);
        }
        IEnumerator ProxyGuestV53()
        {
            var g=game;g.snails.SetupOpen=true;g.ChooseCharacterV51(CharacterV51.Frog);g.snails.SetupOpen=false;yield return UntilV53(()=>MarkedV53("proxy-host-ready"),"proxy host ready");g.coop.Join("127.0.0.1",27954);yield return UntilV53(()=>g.coop.Online && g.coop.Peer>0,"proxy guest connected");g.journey.SetArea(WorldArea.Tavern,new Vector2(0,-3.7f));yield return UntilV53(()=>g.coop.CanSimulate,"proxy guest owns tavern");g.survivor.health=0;yield return UntilV53(()=>MarkedV53("proxy-host-verified"),"host verified remote frog corpse");MarkV53("proxy-guest-finished");yield return new WaitForSeconds(.3f);g.coop.Disconnect(false);
        }
    }
}
