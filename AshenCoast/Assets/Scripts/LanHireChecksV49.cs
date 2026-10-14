using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class LanNativeChecksV49
    {
        IEnumerator HireHostV49()
        {
            var g=game;var s=g.coop;s.PlayerName="Host";s.StartHost(29449,true);Mark("host-ready.txt");yield return Until(()=>s.players.Count==4,45);
            var world=System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(s.world.Capture(s.Region)));var packed=LanTransportV49.Compress(world);Check(LanTransportV49.Expand(packed).SequenceEqual(world),"compressed world packet restores every byte");Check(packed.Length<world.Length*.5f,"world packets use less than half their raw network bandwidth");
            Step("hire");yield return Until(()=>File.Exists(FilePath("hired.txt")));if(!File.Exists(FilePath("hired.txt"))){Step("done");yield break;}
            string ally=File.ReadAllText(FilePath("hired.txt"));Check(s.world.allyOwners.ContainsKey(ally),"hired hunter has one player owner");Step("ally-house");yield return Until(()=>File.Exists(FilePath("ally-house.txt")));yield return new WaitForSecondsRealtime(.5f);Check(s.world.allyRegions[ally]=="house/2" && !s.world.Find(ally).renders[0].gameObject.activeInHierarchy,"only its owner's hunter follows an independent house transition");Step("done");yield return new WaitForSecondsRealtime(.5f);s.Disconnect();
        }
        IEnumerator HireClientV49()
        {
            var g=game;var s=g.coop;s.PlayerName="Client"+number;yield return Until(()=>File.Exists(FilePath("host-ready.txt")));s.Join("127.0.0.1",29449);yield return Until(()=>s.Online && s.inventory.Ready,45);yield return Until(()=>Stage=="hire");
            if(number==2)
            {
                var woman=g.amazons.people.First(p=>p.Hunter && !p.Hired);Warp(WorldArea.Coast,woman.Position+Vector2.left);yield return new WaitForSecondsRealtime(.5f);int coins=g.survivor.Coins;Check(g.amazons.Hire(woman),"hunter hire request starts with sufficient money");
                yield return Until(()=>woman.Hired || !s.HirePendingV49,15);Check(woman.Hired,"hunter is hired on the requesting client");Check(g.survivor.Coins==coins-1,"hire charges its owner's inventory exactly once");if(woman.Hired)Mark("hired.txt",s.world.Key(woman));
            }
            yield return Until(()=>Stage=="ally-house" || Stage=="done");if(number==2 && Stage=="ally-house"){g.amazons.EnterHouse(g.amazons.houses[2]);yield return new WaitForSecondsRealtime(.6f);Mark("ally-house.txt");}yield return Until(()=>Stage=="done");s.Disconnect();
        }
    }
}
