using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class SpectrumWeapon
    {
        sealed class DepartureV41
        {public Vector2 position;public float elapsed,phase,height;public SpriteRenderer wall,glow;public SpriteRenderer[] clouds,spray;public Material mist;}
        readonly List<DepartureV41> departures=new List<DepartureV41>();bool rideCloudsDetached;
        public int DepartureEffects=>departures.Count;
        public Vector2 DeparturePosition=>departures.Count>0?departures[0].position:Vector2.zero;
        public IEnumerable<SpriteRenderer> DepartureClouds=>departures.SelectMany(d=>d.clouds);
        public float DepartureTail=>departures.Count==0?0:departures[0].height*Mathf.Pow(1-Mathf.Clamp01(departures[0].elapsed/TailDuration),1.65f);
        void DetachDepartureV41()
        {
            if(!wall)return;
            departures.Add(new DepartureV41{position=Position,phase=age,height=RainbowHeight,wall=wall,glow=impactGlow,clouds=clouds.ToArray(),spray=spray.ToArray(),mist=mistMaterial});
            wall.enabled=false;
            wall=null;impactGlow=null;mistMaterial=null;clouds.Clear();spray.Clear();CreateRainbowVisuals(Position);
        }
        void TickDeparturesV41(float dt)
        {
            foreach(var d in departures.ToArray())
            {
                d.elapsed+=dt;float clock=d.phase+d.elapsed;
                float tail=d.height*Mathf.Pow(1-Mathf.Clamp01(d.elapsed/TailDuration),1.65f);
                var material=d.wall.sharedMaterial;material.SetFloat("_Phase",clock);material.SetFloat("_Height",d.height);material.SetFloat("_Head",0);material.SetFloat("_Tail",tail);material.SetFloat("_Ending",1);d.wall.enabled=false;material.SetFloat("_Hover",0);
                float impact=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((d.elapsed-1.55f)/.9f));float burst=Mathf.Sin(Mathf.Clamp01(d.elapsed/1.9f)*Mathf.PI)*.28f;
                PoseImpactV41(d.clouds,d.spray,d.glow,d.position,clock,impact,burst,d.wall.sortingOrder,true);
                if(d.elapsed>=2.5f){DisposeDepartureV41(d);departures.Remove(d);}
            }
        }
        void DisposeDepartureV41(DepartureV41 d)
        {
            if(d.wall){Destroy(d.wall.sharedMaterial);Destroy(d.wall.gameObject);}if(d.glow){Destroy(d.glow.sharedMaterial);Destroy(d.glow.gameObject);}
            foreach(var r in d.clouds.Concat(d.spray))if(r)Destroy(r.gameObject);if(d.mist)Destroy(d.mist);
        }
        public int AppendDepartureLightsV41(Vector4[] points,Vector4[] colors,int count)
        {
            foreach(var d in departures)if(d.wall && d.wall.gameObject.activeInHierarchy)
                for(int i=0;i<3 && count<points.Length;i++){var c=Color.HSVToRGB(i*.3f,.40f,1);points[count]=new Vector4(d.position.x+(i-1)*1.9f,d.position.y,3.5f,.24f*Mathf.Clamp01(1-d.elapsed/2.5f));colors[count++]=new Vector4(c.r,c.g,c.b,1);}
            return count;
        }
        void OnDestroy(){ClearRainbowVisuals();foreach(var d in departures)DisposeDepartureV41(d);departures.Clear();}
    }
}
