using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class SnailChecksV50
    {
        int number;string Stage=>File.Exists(Path.Combine(folder,"stage.txt"))?File.ReadAllText(Path.Combine(folder,"stage.txt")):"";
        void StageSet(string step)=>File.WriteAllText(Path.Combine(folder,"stage.txt"),step);
        void Mark(string name,string value="ready")=>File.WriteAllText(Path.Combine(folder,name),value);
        IEnumerator Until(Func<bool> condition,float timeout=35){float deadline=Time.realtimeSinceStartup+timeout;while(!condition() && Time.realtimeSinceStartup<deadline)yield return null;Check(condition(),"wait condition "+Stage);}
        IEnumerator Network()
        {
            number=int.Parse(Environment.GetEnvironmentVariable("ASHEN_SNAIL_PEER")??"0");var s=game.coop;var c=game.snails;c.SetupOpen=false;s.PlayerName="SnailTest"+number;
            if(number==0)
            {
                Check(s.StartHost(29450,true),"new host starts V50 protocol");Mark("host.txt");yield return Until(()=>s.players.Count==4);yield return new WaitForSecondsRealtime(1);
                Check(c.peers.Count(p=>p>=0)==4,"four stable player seats");StageSet("hire");yield return Until(()=>File.Exists(Path.Combine(folder,"hired.txt")));
                string key=File.ReadAllText(Path.Combine(folder,"hired.txt"));Check(c.troops.TryGetValue(key,out var t) && t.home==2 && c.OwnerPeer(t)==c.peers[2],"network hire reserves the requesting player's home");
                StageSet("room");yield return Until(()=>File.Exists(Path.Combine(folder,"room.txt")));yield return new WaitForSecondsRealtime(.4f);
                Check(s.players[c.peers[3]].region=="snail/3" && s.players[0].region=="area/0","players enter distinct snail rooms independently");c.houses[3].HurtLocal(300);Check(!c.houses[3].Hidden,"remote room occupant prevents host shell retraction");
                StageSet("commands");yield return Until(()=>File.Exists(Path.Combine(folder,"ordered.txt")));yield return new WaitForSecondsRealtime(1.2f);Check(c.ordered && c.houses[0].position.x>0,"guest can command all four host-simulated houses");StageSet("whistle");yield return Until(()=>File.Exists(Path.Combine(folder,"whistled.txt")));yield return new WaitForSecondsRealtime(.2f);Check(c.stopped>14,"guest whistle reaches the host while guest is indoors");
                StageSet("rest");yield return Until(()=>File.Exists(Path.Combine(folder,"rested.txt")));Check(c.troops[key].resting,"guest troop returns to its assigned barracks");StageSet("done");yield return new WaitForSecondsRealtime(.8f);
            }
            else
            {
                yield return Until(()=>File.Exists(Path.Combine(folder,"host.txt")));if(number>1)yield return Until(()=>File.Exists(Path.Combine(folder,"joined-"+(number-1)+".txt")));s.Join("127.0.0.1",29450);yield return Until(()=>s.Online && s.inventory.Ready);Mark("joined-"+number+".txt");yield return Until(()=>c.peers.Count(p=>p>=0)==4);
                Check(c.PlayerCount==4 && c.Seat(s.Peer)==number,"joining player receives their own snail seat");yield return Until(()=>Stage=="hire");
                if(number==2)
                {
                    var woman=game.amazons.people.First(p=>p.Hunter);game.journey.SetArea(WorldArea.Coast,woman.Position+Vector2.left);yield return new WaitForSecondsRealtime(.6f);int coins=game.survivor.Coins;
                    Check(game.amazons.Hire(woman) && c.Choosing && !woman.Hired,"hire opens home chooser before spending coins");c.FinishHire(2);yield return Until(()=>woman.Hired);Check(game.survivor.Coins==coins-1,"network hire charges once");Check(c.troops[c.Key(woman.Renderer)].home==2,"network hire assigned proper home");Mark("hired.txt",c.Key(woman.Renderer));
                }
                yield return Until(()=>Stage=="room");if(number==3){c.Enter(3);yield return new WaitForSecondsRealtime(.5f);Check(s.Region=="snail/3","guest room has dedicated network region");Mark("room.txt");}
                yield return Until(()=>Stage=="commands");if(number==1){c.Command(new CaravanCommandV50{kind="order",x=c.houses[0].position.x+26,y=c.houses[0].position.y});yield return new WaitForSecondsRealtime(.5f);Mark("ordered.txt");}
                yield return Until(()=>Stage=="whistle");if(number==3){c.Command(new CaravanCommandV50{kind="whistle",x=game.body.position.x,y=game.body.position.y});yield return new WaitForSecondsRealtime(.4f);Check(c.stopped>14,"caravan stop replicates into guest room");Mark("whistled.txt");}
                yield return Until(()=>Stage=="rest");if(number==2){var key=File.ReadAllText(Path.Combine(folder,"hired.txt"));var woman=c.Woman(key);woman.Warp(c.houses[2].Door+Vector2.down*.3f);yield return new WaitForSecondsRealtime(.4f);c.Command(new CaravanCommandV50{kind="home",key=key});yield return Until(()=>c.troops[key].resting,25);Check(c.Own(c.troops[key]) && c.troops[key].home==2,"resting ally retains personal ownership");Mark("rested.txt");}
                yield return Until(()=>Stage=="done");
            }
            File.WriteAllText(Path.Combine(folder,"peer-"+number+".txt"),"V50 network: "+passed+" passed; "+failed+" failed\n");s.Disconnect();Application.Quit(failed==0?0:1);
        }
        IEnumerator NetworkRest()
        {
            number=int.Parse(Environment.GetEnvironmentVariable("ASHEN_SNAIL_PEER")??"0");var s=game.coop;var c=game.snails;c.SetupOpen=false;s.PlayerName="RestTest"+number;
            if(number==0){s.StartHost(29450,true);Mark("host.txt");yield return Until(()=>s.players.Count==4);}
            else{yield return Until(()=>File.Exists(Path.Combine(folder,"host.txt")));if(number>1)yield return Until(()=>File.Exists(Path.Combine(folder,"joined-"+(number-1)+".txt")));s.Join("127.0.0.1",29450);yield return Until(()=>s.Online && s.inventory.Ready);Mark("joined-"+number+".txt");yield return Until(()=>c.peers.Count(p=>p>=0)==4);}
            var woman=game.amazons.people.First(p=>p.Hunter);string key=c.Key(woman.Renderer);
            if(number==2){game.journey.SetArea(WorldArea.Coast,c.houses[2].Door+Vector2.left);yield return new WaitForSecondsRealtime(.7f);Mark("position.txt");}
            if(number==0){yield return Until(()=>File.Exists(Path.Combine(folder,"position.txt")));woman.Hired=true;woman.Captive=false;woman.health=60;c.Assign(key,2,c.peers[2]);s.world.allyOwners[key]=c.peers[2];s.world.allyRegions[key]="area/0";woman.Warp(c.houses[2].Door+Vector2.down*.3f);yield return new WaitForSecondsRealtime(.6f);StageSet("unauthorized");}
            yield return Until(()=>Stage=="unauthorized");
            if(number==1){c.Command(new CaravanCommandV50{kind="home",key=key});yield return new WaitForSecondsRealtime(.5f);Mark("unauthorized.txt");}
            if(number==0){yield return Until(()=>File.Exists(Path.Combine(folder,"unauthorized.txt")));Check(!c.troops[key].resting && !c.troops[key].returning,"another player cannot command a private troop");StageSet("return");}
            yield return Until(()=>Stage=="return");
            if(number==2){yield return Until(()=>c.troops.ContainsKey(key));c.Command(new CaravanCommandV50{kind="home",key=key});yield return Until(()=>c.troops[key].resting,8);Check(c.troops[key].resting,"return-to-home state is replicated to owner");Mark("rested.txt");}
            if(number==0){yield return Until(()=>File.Exists(Path.Combine(folder,"rested.txt")));yield return new WaitForSecondsRealtime(1);Check(c.troops[key].resting && woman.health>60,"host heals garrison while every player remains outdoors");StageSet("recall");}
            yield return Until(()=>Stage=="recall");
            if(number==2){c.Enter(2);yield return new WaitForSecondsRealtime(.5f);c.Command(new CaravanCommandV50{kind="recall",key=key});yield return Until(()=>!c.troops[key].resting,8);yield return new WaitForSecondsRealtime(.5f);Check(s.world.allyRegions[key]=="snail/2","recalled own troop joins player in their room");Mark("recalled.txt");}
            if(number==0){yield return Until(()=>File.Exists(Path.Combine(folder,"recalled.txt")));Check(!c.troops[key].resting && c.OwnerPeer(c.troops[key])==c.peers[2],"recall preserves ally ownership");StageSet("done");yield return new WaitForSecondsRealtime(.5f);}else yield return Until(()=>Stage=="done");
            File.WriteAllText(Path.Combine(folder,"peer-"+number+".txt"),"V50 targeted garrison: "+passed+" passed; "+failed+" failed\n");s.Disconnect();Application.Quit(failed==0?0:1);
        }
    }
}
