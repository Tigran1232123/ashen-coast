using UnityEngine;
namespace AshenCoast
{
    public sealed partial class AmazonSettlementV46
    {
        const int NavSideV59=161;const float NavCellV59=.75f;
        readonly byte[] navCellsV59=new byte[NavSideV59*NavSideV59];
        bool navReadyV59,navCoastV59,navRiverV59;Vector2 navCentreV59,navJunctionV59;
        int navFrameV59=-1,navTreesV59,navTreeCountV59,navSeedV59,navBarriersV59,navRoundedV59;
        public static bool ReferenceNavigationV59;
        void PrepareNavigationV59()
        {
            int hash=navTreesV59;
            if(navFrameV59!=Time.frameCount || navTreeCountV59!=trees.Count)
            {hash=17;unchecked{foreach(var tree in trees)hash=hash*31+(tree.Standing?1:0);}navFrameV59=Time.frameCount;}
            bool coast=game.journey.IsCoast && !game.magic.InPocket;
            if(!navReadyV59 || navCentreV59!=Centre || navTreesV59!=hash || navSeedV59!=WorldShape.Seed || navBarriersV59!=barriers.Count || navRoundedV59!=roundedV58.Count || navCoastV59!=coast || navRiverV59!=RiverV58.Ready || navJunctionV59!=RiverV58.Junction)
            {System.Array.Clear(navCellsV59,0,navCellsV59.Length);navReadyV59=true;}
            navCentreV59=Centre;navTreesV59=hash;navTreeCountV59=trees.Count;navSeedV59=WorldShape.Seed;navBarriersV59=barriers.Count;navRoundedV59=roundedV58.Count;navCoastV59=coast;navRiverV59=RiverV58.Ready;navJunctionV59=RiverV58.Junction;
        }
        public bool AuditNavigationV59(){PrepareNavigationV59();for(int id=0;id<navCellsV59.Length;id++)if(NavigationCellV59(id)!=WalkableV48(Centre-Vector2.one*60+new Vector2(id%NavSideV59,id/NavSideV59)*NavCellV59))return false;return true;}
        bool NavigationCellV59(int id)
        {
            if(ReferenceNavigationV59 || navCellsV59[id]==0)
            {bool open=WalkableV48(Centre-Vector2.one*60+new Vector2(id%NavSideV59,id/NavSideV59)*NavCellV59);if(ReferenceNavigationV59)return open;navCellsV59[id]=(byte)(open?1:2);}
            return navCellsV59[id]==1;
        }
    }
}
