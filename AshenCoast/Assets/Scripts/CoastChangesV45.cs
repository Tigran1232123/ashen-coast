using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class CoastGame
    {
        public const float ZenGroundAngleV45=30,ZenBuryDepthV45=.16f,ZenWellRadiusV45=1.45f;
        public bool GroundPixelV45(GroundLoot drop,Vector2 point)=>(drop.item.kind!=ItemKind.Zen || point.y>=drop.position.y-.015f) && art.Opaque(drop.renderer.sprite,drop.renderer.transform.InverseTransformPoint(point));
        public int GroundZenWellsV45(Vector4[] points,Vector2 viewCentre)
        {
            int count=0;
            foreach(var drop in loot.Where(d=>!d.collected && !d.InFlight && d.item.kind==ItemKind.Zen && d.renderer.enabled && d.renderer.gameObject.activeInHierarchy).OrderBy(d=>((Vector2)d.renderer.bounds.center-viewCentre).sqrMagnitude))
            {
                if(count>=points.Length)break;var centre=drop.renderer.bounds.center;
                points[count++]=new Vector4(centre.x,centre.y,ZenWellRadiusV45,.74f);
            }
            return count;
        }
    }
}
