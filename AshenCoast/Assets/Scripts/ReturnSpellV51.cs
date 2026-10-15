using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public static class ReturnRuneV51
    {
        public static readonly Vector2[] Template=Enumerable.Range(0,41).Select(i=>new Vector2(.5f+.46f*Mathf.Cos(i*Mathf.PI/40),.88f-.80f*Mathf.Sin(i*Mathf.PI/40))).Concat(Enumerable.Range(1,17).Select(i=>new Vector2(.04f+.92f*i/16,.88f))).ToArray();
        public static bool Recognize(IList<Vector2> raw)
        {
            if(raw==null || raw.Count<12)return false;float size=Mathf.Max(raw.Max(p=>p.x)-raw.Min(p=>p.x),raw.Max(p=>p.y)-raw.Min(p=>p.y));if(size<45 || Vector2.Distance(raw[0],raw[raw.Count-1])>size*.24f)return false;
            var samples=SpatialSigil.Resample(raw,96);var norm=SpellGlyphs.Normalize(samples);float best=10;
            // A half-circle needs both a rounded filled area and a long straight chord.
            // Template distance alone can mistake triangles and full circles for this rune.
            float area=0;for(int i=0;i<norm.Length;i++){var a=norm[i];var b=norm[(i+1)%norm.Length];area+=a.x*b.y-b.x*a.y;}if(Mathf.Abs(area)*.5f<.64f)return false;
            bool chord=false;for(int start=0;start<norm.Length && !chord;start++)
            {
                var a=norm[start];var b=norm[(start+24)%norm.Length];var axis=b-a;float length=axis.magnitude;if(length<.55f)continue;float error=0,maximum=0;
                for(int k=1;k<24;k++){var p=norm[(start+k)%norm.Length]-a;float distance=Mathf.Abs(axis.x*p.y-axis.y*p.x)/length;error+=distance*distance;maximum=Mathf.Max(maximum,distance);}
                chord=Mathf.Sqrt(error/23)<.035f && maximum<.075f;
            }
            if(!chord)return false;
            // A cyclic starting point and both directions accept the same closed half-circle.
            for(int rotate=0;rotate<4;rotate++)
            {
                var transformed=norm.Select(p=>(Vector2)(Quaternion.Euler(0,0,rotate*90)*(p-Vector2.one*.5f))+Vector2.one*.5f).ToArray();var target=SpatialSigil.Resample(SpellGlyphs.Normalize(Template),96);
                for(int offset=0;offset<96;offset+=3){float f=0,b=0;for(int i=0;i<96;i++){f+=(transformed[(i+offset)%96]-target[i]).sqrMagnitude;b+=(transformed[(offset-i+192)%96]-target[i]).sqrMagnitude;}best=Mathf.Min(best,Mathf.Sqrt(Mathf.Min(f,b)/96));}
            }
            return best<.13f;
        }
    }
    public sealed partial class Spellcraft
    {
        public bool CastReturnV51(IList<Vector2> screen,IList<Vector2> world,Vector2 centre)
        {
            if(!ReturnRuneV51.Recognize(screen))return false;var book=game.survivor.OpenBook(ItemKind.Pyromancy);if(book==null || !book.spells.Any(s=>s.kind==SpellKind.Return))return false;
            var corpse=game.corpses.Select(world,centre);if(corpse==null){game.ui.Notice("Нет цели");return true;}game.corpses.RequestRaise(corpse,book);return true;
        }
    }
}
