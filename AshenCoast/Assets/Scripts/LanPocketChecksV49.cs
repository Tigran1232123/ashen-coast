using System.Collections;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class LanNativeChecksV49
    {
        IEnumerator PocketChecksV49()
        {
            var g=game;var s=g.coop;s.StartHost(29449,true);
            Check(s.world.RegionOf(g.magic.pocket.transform)=="pocket/0","personal dimension keeps its region while its owner is outside");
            var other=s.world.Root("pocket/2");Check(other!=g.magic.pocket.transform && !other.gameObject.activeInHierarchy,"another player's dimension cannot appear inside the local dimension");
            var item=g.CreateLooseLootV46(new Item(ItemKind.Clay),Vector2.zero);item.renderer.transform.SetParent(g.magic.pocket.transform,true);item.shadow.transform.SetParent(g.magic.pocket.transform,true);
            Check(!s.world.Capture("area/0").loot.Any(l=>l.key==item.netIdV49) && s.world.Capture("pocket/0").loot.Any(l=>l.key==item.netIdV49),"pocket loot cannot leak into a coast world snapshot");
            var pot=g.buildings.CreateStation(ItemKind.StoragePot,Vector2Int.zero,false,null,other);Check(s.inventory.Key(pot.inventory).Contains("pocket/2"),"a foreign dimension's storage keeps its own region key");
            s.Disconnect();yield return null;
        }
    }
}
