using System.Collections.Generic;
using UnityEngine;
namespace AshenCoast
{
    public sealed class DummyDamageV48
    {
        readonly Queue<Vector2> hits=new Queue<Vector2>();float first,last=-100;
        public void Record(float amount,float now)
        {if(amount<=0)return;if(now-last>=3){hits.Clear();first=now;}hits.Enqueue(new Vector2(now,amount));last=now;}
        public float Value(float now)
        {while(hits.Count>0 && hits.Peek().x<now-3)hits.Dequeue();if(now-last>=3)return 0;float damage=0;foreach(var hit in hits)damage+=hit.y;return damage/Mathf.Clamp(now-first,1,3);}
    }
    public sealed partial class CoastGame
    {
        readonly Dictionary<SpriteRenderer,DummyDamageV48> dummyDamageV48=new Dictionary<SpriteRenderer,DummyDamageV48>();
        public void RecordDummyV48(SpriteRenderer target,float amount)
        {if(!dummyDamageV48.TryGetValue(target,out var meter)){meter=new DummyDamageV48();dummyDamageV48[target]=meter;}meter.Record(amount,Time.time);}
        public float DummyDpsV48(SpriteRenderer target)=>dummyDamageV48.TryGetValue(target,out var meter)?meter.Value(Time.time):0;
    }
    public sealed partial class InventoryUI
    {
        void DrawDummyDpsV48(SpriteRenderer target)
        {
            float dps=game.DummyDpsV48(target);if(dps<=0 || !target.gameObject.activeInHierarchy)return;
            var at=World(new Vector2(target.bounds.center.x,target.bounds.max.y+.55f));if(at.x<50 || at.x>CanvasWidth*WorldFraction-50 || at.y<25 || at.y>CanvasHeight-25)return;
            var style=CachedStyle(bold,TextAnchor.MiddleCenter,18,FontStyle.Bold);var box=new Rect(at.x-100,at.y-30,200,28);string label=dps.ToString("0.#")+" урон/с";
            GUI.color=new Color(.05f,.035f,.02f,.9f);GUI.Label(new Rect(box.x+1,box.y+1,box.width,box.height),label,style);GUI.color=new Color(1,.84f,.50f);GUI.Label(box,label,style);GUI.color=Color.white;
        }
    }
}
