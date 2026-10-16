using UnityEngine;
namespace AshenCoast
{
    public static partial class WorldShape
    {
        public static void AvoidRiverV58(Vector2 village)
        {
            if(RiverV58.Bank(FortPosition)>36)return;
            var original=FortPosition;
            for(float radius=12;radius<=480;radius+=12)for(int i=0;i<32;i++)
            {float a=Phase+i*Mathf.PI/16;var p=original+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;
                if(CoastDistanceV58(p)>48 && RiverV58.Bank(p)>36 && Vector2.Distance(p,village)>75 && MeadowLayout.PathDistance(p)>17){FortPosition=p;return;}}
        }
    }
    public sealed partial class CoastGame
    {
        public void ConfigureRiverWorldV58()
        {RiverV58.Configure(amazons.Centre);WorldShape.AvoidRiverV58(amazons.Centre);fort.AdoptSeedV56();cave.AdoptSeedV56();RiverV58.Build(this);}
    }
}
