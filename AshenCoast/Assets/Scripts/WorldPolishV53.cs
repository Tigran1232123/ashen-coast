using System;
using System.Collections.Generic;
using UnityEngine;
namespace AshenCoast
{
    // A bounded voice pool: repeated attacks never accumulate PlayOneShot voices.
    public sealed class WorldAudioV53:MonoBehaviour
    {
        public CoastGame game;readonly AudioSource[] voices=new AudioSource[3];readonly SpriteRenderer[] origins=new SpriteRenderer[3];int cursor;float next;
        public int ActiveVoices {get{int n=0;foreach(var v in voices)if(v && v.isPlaying)n++;return n;}}
        public bool Audible(SpriteRenderer actor)=>actor && actor.gameObject.activeInHierarchy && actor.transform.IsChildOf(game.arcane.WorldRoot) && Vector2.Distance(game.coop.LocalBody.position,actor.transform.position)<17;
        public bool Play(SpriteRenderer actor,AudioClip clip,float volume)
        {
            if(!Audible(actor) || Time.unscaledTime<next)return false;next=Time.unscaledTime+.065f;
            int i=cursor++%3;if(!voices[i]){voices[i]=game.gameObject.AddComponent<AudioSource>();voices[i].playOnAwake=false;}
            var v=voices[i];v.Stop();origins[i]=actor;v.clip=clip;v.pitch=.96f+(i*.035f);v.loop=false;v.spatialBlend=0;SetVolume(i,volume);v.Play();return true;
        }
        void SetVolume(int i,float gain=.20f){var p=(Vector2)origins[i].transform.position-game.coop.LocalBody.position;voices[i].volume=gain*Mathf.Pow(Mathf.Clamp01(1-p.magnitude/17),2);voices[i].panStereo=Mathf.Clamp(p.x/15,-.65f,.65f);}
        void Update(){for(int i=0;i<3;i++)if(voices[i] && voices[i].isPlaying){if(game.paused || !Audible(origins[i]))voices[i].Stop();else SetVolume(i);}}
    }
    public sealed partial class CoastGame
    {
        public float CharacterScaleV53=>survivor.character==CharacterV51.Frog?.6f:1;
        public WorldAudioV53 worldAudioV53;
        public void WorldSwishV53(SpriteRenderer actor)
        {if(!worldAudioV53){worldAudioV53=gameObject.AddComponent<WorldAudioV53>();worldAudioV53.game=this;}worldAudioV53.Play(actor,swish,.20f);}
    }
    public sealed partial class FortBandit{public PixelGait gaitV53;}
    public sealed partial class CaravanDriver
    {public PixelGait gaitV53;public void PeaceV53(){Fleeing=false;fleeAge=0;flash=0;feet.enabled=false;gaitV53.Lower.enabled=false;}}
    public sealed partial class FrogMage
    {public void PeaceV53(){Hostile=false;aggressor=null;victim=null;attackClock=emission=hitFlash=0;}}
    public sealed partial class VillageActorV46
    {public void PeaceV53(){hostile=false;Aggressor=null;SparringPartnerV48=null;victim=null;ready=1;attackAge=-1;ResetRouteV48();}public void ForgetTargetV53(SpriteRenderer actor){if(Hired && victim?.Renderer==actor){victim=null;attackAge=-1;weaponSprite.enabled=false;}}}
    public sealed partial class AmazonSettlementV46
    {
        // Caravan navigation always uses outdoor geometry, even while the host is indoors.
        public bool CoastBlockedV53(Vector2 from,Vector2 to,float radius)
        {foreach(var b in barriers){var r=new Rect(b.xMin-radius,b.yMin-radius,b.width+2*radius,b.height+2*radius);if(r.Contains(to) && !r.Contains(from))return true;}foreach(var t in trees)if(t.Standing && Vector2.Distance(to,t.position)<radius+.23f && Vector2.Distance(from,t.position)>=radius+.23f)return true;return false;}
    }
    public sealed class MinHeapV53<T>
    {
        struct Entry{public T value;public float score;}
        readonly List<Entry> heap=new List<Entry>();public int Count=>heap.Count;
        public void Push(T value,float score){int i=heap.Count;heap.Add(new Entry{value=value,score=score});while(i>0){int parent=(i-1)/2;if(heap[parent].score<=score)break;heap[i]=heap[parent];i=parent;}heap[i]=new Entry{value=value,score=score};}
        public T Pop(){var result=heap[0].value;int last=heap.Count-1;var e=heap[last];heap.RemoveAt(last);if(last==0)return result;int i=0;while(i*2+1<last){int child=i*2+1;if(child+1<last && heap[child+1].score<heap[child].score)child++;if(heap[child].score>=e.score)break;heap[i]=heap[child];i=child;}heap[i]=e;return result;}
    }
    public sealed partial class SnailHomesV50
    {
        readonly List<Vector2>[] travelPathsV53=new List<Vector2>[4];readonly int[] travelAtV53=new int[4];float travelTimeV53;Vector2 lastCampV53;bool campCachedV53;readonly Vector2[] slotsV53=new Vector2[4];
        public Vector2 CampSlotV53(int i)
        {if(!campCachedV53 || lastCampV53!=campCentreV51){campCachedV53=true;lastCampV53=campCentreV51;for(int k=0;k<4;k++)slotsV53[k]=CampLandV53(campCentreV51+new Vector2(Mathf.Cos((45+k*90)*Mathf.Deg2Rad)*9.5f,Mathf.Sin((45+k*90)*Mathf.Deg2Rad)*7.4f));}return slotsV53[i];}
        bool ClearCoastV53(Vector2 a,Vector2 b)
        {int n=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(a,b)/.4f));var before=a;for(int i=1;i<=n;i++){var p=Vector2.Lerp(a,b,i/(float)n);if(!Land(p) || game.amazons.CoastBlockedV53(before,p,.4f))return false;before=p;}return true;}
        public Vector2 CampLandV53(Vector2 point)
        {if(Land(point) && game.amazons.WalkableV48(point,.45f))return point;for(float r=.8f;r<20;r+=.8f)for(int i=0;i<24;i++){var p=point+new Vector2(Mathf.Cos(i*Mathf.PI/12),Mathf.Sin(i*Mathf.PI/12))*r;if(Land(p) && game.amazons.WalkableV48(p,.45f))return p;}return FindLand(point);}
        public List<Vector2> CoastPathV53(Vector2 from,Vector2 to)
        {
            if(!Land(to))return null;if(ClearCoastV53(from,to))return new List<Vector2>{to};
            const float cell=2;var start=Vector2Int.RoundToInt(from/cell);var end=Vector2Int.RoundToInt(to/cell);var open=new MinHeapV53<Vector2Int>();open.Push(start,0);
            var costs=new Dictionary<Vector2Int,float>{{start,0}};var prev=new Dictionary<Vector2Int,Vector2Int>();var closed=new HashSet<Vector2Int>();
            for(int iteration=0;open.Count>0 && iteration<24000;iteration++)
            {
                var at=open.Pop();if(!closed.Add(at))continue;var point=(Vector2)at*cell;
                if(Vector2.Distance(point,to)<cell*1.6f && ClearCoastV53(point,to))
                {var path=new List<Vector2>{to};while(at!=start){path.Add((Vector2)at*cell);at=prev[at];}path.Reverse();return path;}
                for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++)
                {if(x==0 && y==0)continue;var next=at+new Vector2Int(x,y);if(closed.Contains(next))continue;var p=(Vector2)next*cell;var origin=at==start?from:point;if(!ClearCoastV53(origin,p))continue;float cost=costs[at]+Vector2.Distance(origin,p);if(costs.TryGetValue(next,out float old) && cost>=old)continue;costs[next]=cost;prev[next]=at;open.Push(next,cost+Vector2.Distance(p,to));}
            }
            return null;
        }
        void PrepareRoutesV53(Vector2 point,bool circle=false)
        {travelTimeV53=0;for(int i=0;i<4;i++){travelPathsV53[i]=CoastPathV53(houses[i].position,circle?CampSlotV51(i):point);travelAtV53[i]=0;}}
        void TravelV53(SnailHouseV50 h,Vector2 goal,float speed,float dt)
        {
            var path=travelPathsV53[h.index];if(path==null){h.moving=false;return;}int at=travelAtV53[h.index];
            while(at<path.Count && Vector2.Distance(h.position,path[at])<.12f)at++;travelAtV53[h.index]=at;
            Step(h,at<path.Count?path[at]:goal,speed,dt);
        }
    }
}
