using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class PolishChecksV53
    {
        void MarkV53(string name)=>File.WriteAllText(Path.Combine(folder,name),"ready");
        bool MarkedV53(string name)=>File.Exists(Path.Combine(folder,name));
        IEnumerator UntilV53(Func<bool> ready,string label,float timeout=25)
        {float deadline=Time.realtimeSinceStartup+timeout;while(!ready() && Time.realtimeSinceStartup<deadline)yield return null;Check(ready(),label);}
        IEnumerator NetworkHostV53()
        {
            var g=game;Check(g.coop.StartHost(27953,true),"V53 LAN host starts");MarkV53("host-ready");yield return UntilV53(()=>g.coop.players.Count==2 && g.coop.players.Values.Any(p=>p.region=="area/2"),"guest independently enters tavern");
            Check(g.coop.remote.Values.Any(p=>p.State.character==1 && p.Renderer.sprite.bounds.size.y<1.1f),"small frog renders correctly for host");
            var small=g.woodlice.First(w=>w.Small);while(small.vitals.Alive)small.Hurt(10000);g.corpses.RefreshBindingsV53();g.corpses.ScanV53();
            yield return UntilV53(()=>MarkedV53("guest-killed"),"guest kills neutral resident");yield return UntilV53(()=>g.corpses.bodies.Values.Any(b=>b.data.actor=="resident/0"),"host receives death from another location");
            var body=g.corpses.bodies.Values.First(b=>b.data.actor=="resident/0").data;Check(body.region=="area/2","body belongs to correct independent location");
            yield return UntilV53(()=>MarkedV53("guest-loot"),"guest receives outward loot through region authority");
            g.corpses.FinishRaise(new RaiseRequestV51{key=body.key,cost=0,request="v53-remote-raise"},0,"area/2");MarkV53("host-raised");
            yield return UntilV53(()=>MarkedV53("guest-raised"),"remote neutral resurrection confirmed");
            typeof(SnailHomesV50).GetField("doorWait",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g.snails,0f);g.snails.Enter(0);MarkV53("host-inside");
            yield return UntilV53(()=>g.snails.ordered && g.snails.campPhaseV51==1,"guest route command reaches host inside snail");var start=g.snails.houses[0].position;Call(g.snails,"CampTickV51",60f);
            yield return UntilV53(()=>Vector2.Distance(start,g.snails.houses[0].position)>3,"global caravan moves while host remains inside",12);MarkV53("host-moved");
            yield return UntilV53(()=>MarkedV53("guest-finished"),"guest sees moving caravan replica");g.coop.Disconnect(false);
        }
        IEnumerator NetworkGuestV53()
        {
            var g=game;g.snails.SetupOpen=true;g.ChooseCharacterV51(CharacterV51.Frog);g.snails.SetupOpen=false;yield return UntilV53(()=>MarkedV53("host-ready"),"host ready");g.coop.Join("127.0.0.1",27953);yield return UntilV53(()=>g.coop.Online && g.coop.Peer>0,"V53 client connects");
            g.journey.SetArea(WorldArea.Tavern,new Vector2(0,-3.7f));yield return UntilV53(()=>g.coop.CanSimulate,"guest owns independent tavern simulation");var resident=g.journey.residents[0];resident.Hurt(10000);MarkV53("guest-killed");
            yield return UntilV53(()=>g.corpses.bodies.Values.Any(b=>b.data.actor.StartsWith("woodlouse/") && b.data.width<1.1f),"small woodlouse corpse width synchronizes");
            yield return UntilV53(()=>g.loot.Any(l=>!l.collected && l.renderer.transform.IsChildOf(g.journey.tavernRoot.transform) && Vector2.Distance(l.position,resident.Position)<2),"loot spawns at remote death location");var drop=g.loot.Last(l=>l.renderer.transform.IsChildOf(g.journey.tavernRoot.transform) && Vector2.Distance(l.position,resident.Position)<2);Check(Vector2.Distance(drop.thrownFrom,drop.position)>.5f,"network-authoritative drop has outward flight");MarkV53("guest-loot");
            yield return UntilV53(()=>MarkedV53("host-raised") && resident.Alive,"resurrection reaches guest authority");Check(resident.peacefulV53,"resurrected neutral stays peaceful in co-op");MarkV53("guest-raised");
            yield return UntilV53(()=>MarkedV53("host-inside"),"host enters snail independently");var start=g.snails.houses[0].position;var destination=SnailHomesV50.FindLand(start+Vector2.up*35);g.map.regions[0].Reveal(destination,12);g.snails.Command(new CaravanCommandV50{kind="order",x=destination.x,y=destination.y});
            yield return UntilV53(()=>MarkedV53("host-moved") && Vector2.Distance(start,g.snails.houses[0].position)>2,"moving snail state arrives while guest stays in tavern",20);MarkV53("guest-finished");yield return new WaitForSeconds(.3f);g.coop.Disconnect(false);
        }
    }
}
