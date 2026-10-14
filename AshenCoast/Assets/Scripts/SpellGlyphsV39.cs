using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public enum SpellGlyph { None,Star,Infinity,Triangle,Circle,Diamond,Tentacles,Fog,Abyss,Zigzag }
    public static class SpellGlyphs
    {
        public static readonly Vector2[] Fog={new Vector2(.02f,.78f),new Vector2(.98f,.18f),new Vector2(.12f,.02f),new Vector2(.76f,.98f)};
        public static readonly Vector2[] Zigzag={new Vector2(.12f,.12f),new Vector2(.88f,.36f),new Vector2(.12f,.64f),new Vector2(.88f,.88f)};
        static readonly Vector2[] Circle=Enumerable.Range(0,65).Select(i=>Vector2.one*.5f+new Vector2(Mathf.Cos(i*Mathf.PI/32),Mathf.Sin(i*Mathf.PI/32))*.46f).ToArray();
        static readonly Vector2[] Infinity=Enumerable.Range(0,65).Select(i=>new Vector2(.5f+.46f*Mathf.Sin(i*Mathf.PI/32),.5f+.35f*Mathf.Sin(i*Mathf.PI/16))).ToArray();
        static readonly Vector2[] Whirlpool=Enumerable.Range(0,81).Select(i=>{float t=i/80f,a=t*Mathf.PI*5;return Vector2.one*.5f+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(.45f*(1-t));}).ToArray();
        static readonly Vector2[] Diamond={new Vector2(.5f,.06f),new Vector2(.94f,.5f),new Vector2(.5f,.94f),new Vector2(.06f,.5f),new Vector2(.5f,.06f)};
        static readonly Vector2[] Triangle={new Vector2(.5f,.06f),new Vector2(.94f,.90f),new Vector2(.06f,.90f),new Vector2(.5f,.06f)};
        public static Vector2[] Path(SpellGlyph glyph)=>glyph==SpellGlyph.Star?StarSigil.Template:glyph==SpellGlyph.Infinity?Infinity:glyph==SpellGlyph.Circle?Circle:glyph==SpellGlyph.Abyss?Whirlpool:glyph==SpellGlyph.Diamond?Diamond:glyph==SpellGlyph.Triangle?Triangle:glyph==SpellGlyph.Tentacles?SpatialSigil.Template:glyph==SpellGlyph.Fog?Fog:glyph==SpellGlyph.Zigzag?Zigzag:System.Array.Empty<Vector2>();
        public static SpellGlyph For(Item book,SpellSlot slot)
        {
            if(slot==null || slot.kind==SpellKind.None)return SpellGlyph.None;
            if(!slot.locked && book.kind==ItemKind.Tome)return SpellGlyph.Diamond;
            if(!slot.locked && book.kind==ItemKind.AbyssTome)return SpellGlyph.Fog;
            return slot.kind==SpellKind.Pocket?SpellGlyph.Star:slot.kind==SpellKind.WorldTravel?SpellGlyph.Infinity:slot.kind==SpellKind.FireBlast?SpellGlyph.Zigzag:slot.kind==SpellKind.Tentacles?SpellGlyph.Tentacles:slot.kind==SpellKind.Abyss?SpellGlyph.Abyss:slot.kind==SpellKind.Shackles?SpellGlyph.Triangle:SpellGlyph.Circle;
        }
        public static Vector2[] Normalize(IList<Vector2> p)
        {float x=p.Min(v=>v.x),y=p.Min(v=>v.y),w=Mathf.Max(.0001f,p.Max(v=>v.x)-x),h=Mathf.Max(.0001f,p.Max(v=>v.y)-y);return p.Select(v=>new Vector2((v.x-x)/w,(v.y-y)/h)).ToArray();}
        public static float Error(IList<Vector2> stroke,IList<Vector2> template)
        {
            if(stroke.Count<8)return 100;
            var p=SpatialSigil.Resample(Normalize(stroke),96);var q=SpatialSigil.Resample(Normalize(template),96);float f=0,b=0;
            for(int i=0;i<96;i++){f+=(p[i]-q[i]).sqrMagnitude;b+=(p[i]-q[95-i]).sqrMagnitude;}
            return Mathf.Sqrt(Mathf.Min(f,b)/96);
        }
        public static SpellGlyph OpenRune(IList<Vector2> stroke)
        {
            if(stroke.Count<8 || stroke.Max(p=>p.x)-stroke.Min(p=>p.x)<45 || stroke.Max(p=>p.y)-stroke.Min(p=>p.y)<45)return SpellGlyph.None;
            var normalized=Normalize(stroke);if(Vector2.Distance(normalized[0],normalized[normalized.Length-1])<.42f)return SpellGlyph.None;
            float fog=Error(stroke,Fog),tentacle=Error(stroke,SpatialSigil.Template),zigzag=Error(stroke,Zigzag);
            float best=Mathf.Min(fog,tentacle,zigzag);if(best>.23f)return SpellGlyph.None;
            return best==tentacle?SpellGlyph.Tentacles:best==fog?SpellGlyph.Fog:SpellGlyph.Zigzag;
        }
    }
}
