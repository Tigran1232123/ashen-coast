using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class CoastGame
    {
        public int AppendSpectrumLights(Vector4[] points,Vector4[] colors,int count)
        {
            foreach(var drop in loot.Where(d=>!d.collected && d.item.kind==ItemKind.Spectrum && d.renderer.enabled && d.renderer.gameObject.activeInHierarchy).OrderBy(d=>(d.position-body.position).sqrMagnitude).Take(3))
            {
                Vector2 centre=drop.InFlight?(Vector2)drop.renderer.bounds.center:drop.position;
                if(count<points.Length){points[count]=new Vector4(centre.x,centre.y,2.4f,.43f+.10f*Mathf.Sin(seconds*7)+.045f*Mathf.Sin(seconds*17));colors[count++]=new Vector4(.30f,.63f,1,1);}
                if(drop.InFlight)continue;
                for(int i=0;i<6 && count<points.Length;i++)
                {
                    float angle=i*Mathf.PI/3+seconds*.24f;var at=centre+new Vector2(Mathf.Cos(angle)*1.05f,Mathf.Sin(angle)*.43f);var color=Color.HSVToRGB(Mathf.Repeat(i/6f+seconds*.035f,1),.88f,1);
                    points[count]=new Vector4(at.x,at.y,1.65f,.34f+.045f*Mathf.Sin(seconds*5+i));colors[count++]=new Vector4(color.r,color.g,color.b,1);
                }
            }
            if(survivor.equipment[0]?.kind==ItemKind.Spectrum && combat.weapon && combat.weapon.enabled && count<points.Length)
            {var at=combat.weapon.bounds.center;var color=Color.HSVToRGB(Mathf.Repeat(seconds*.26f,1),.64f,1);points[count]=new Vector4(at.x,at.y,1.65f,.27f);colors[count++]=new Vector4(color.r,color.g,color.b,1);}
            return count;
        }
    }
}
