using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    [Serializable] public struct FlowerSite
    {
        public long id;
        public Vector2 position;
        public int type, variant;
        public float scale, phase;
    }

    // Global lattice coordinates make every chunk independent of loading order.
    public static class MeadowLayout
    {
        public const int ChunkSize = 32;
        public const float MinX = -512, MaxX = 512, MaxY = 1024;
        public static readonly Rect MapBounds = new Rect(MinX, -20, MaxX-MinX, MaxY+20);
        public static float Shore(float x) => -4.6f+Mathf.Sin(x*.17f)*.48f+Mathf.Sin(x*.79f)*.16f;
        public static bool IsOriginal(Vector2 p) => Mathf.Abs(p.x)<=30 && p.y<=30;
        public static bool IsExtension(Vector2 p) => !IsOriginal(p) && WorldShape.Land(p,.5f) && p.y>=4.8f+Mathf.Sin(p.x*.15f)*.55f && !WorldShape.FortFootprint(p) && !WorldShape.AmazonFootprint(p);
        public static bool Walkable(Vector2 p) => WorldShape.Land(p,.38f);
        public static Vector2 Clamp(Vector2 p) => WorldShape.NearestLand(p,.6f);
        public static Vector2Int ChunkAt(Vector2 p) => new Vector2Int(Mathf.FloorToInt(p.x/ChunkSize),Mathf.FloorToInt(p.y/ChunkSize));
        public static float PathDistance(Vector2 p) => Mathf.Abs(p.x-(8+Mathf.Sin((p.y-24)*.017f)*27+Mathf.Sin((p.y-24)*.048f)*9));
        public static float Value(int x,int y,int seed,int salt)
        {
            unchecked
            {
                uint h=(uint)x*374761393u+(uint)y*668265263u+(uint)seed*2246822519u+(uint)salt*3266489917u;
                h=(h^(h>>13))*1274126177u;h^=h>>16;
                return (h&0x00ffffff)/16777216f;
            }
        }
        public static int DominantFlower(Vector2 p,int seed) => Mathf.Min(5,(int)(Value(Mathf.FloorToInt(p.x/18),Mathf.FloorToInt(p.y/18),seed,71)*6));
        public static List<FlowerSite> Generate(Vector2Int chunk,int seed)
        {
            var sites=new List<FlowerSite>(240);
            for(int y=0;y<20;y++)for(int x=0;x<20;x++)
            {
                int gx=chunk.x*20+x,gy=chunk.y*20+y;
                var p=new Vector2((gx+.5f+(Value(gx,gy,seed,1)-.5f)*.72f)*1.6f,(gy+.5f+(Value(gx,gy,seed,2)-.5f)*.72f)*1.6f);
                if(!IsExtension(p))continue;
                float density=.48f+.24f*Mathf.PerlinNoise(p.x*.023f+37,p.y*.023f+seed%1000);
                // A continuous, narrow footpath leaves room to walk through the blooms.
                if(p.y>32 && PathDistance(p)<1.7f)density*=.08f;
                if(Value(gx,gy,seed,3)>density)continue;
                int type=Value(gx,gy,seed,4)<.80f?DominantFlower(p,seed):Mathf.Min(5,(int)(Value(gx,gy,seed,5)*6));
                sites.Add(new FlowerSite{id=((long)gx<<32)|(uint)gy,position=p,type=type,variant=Value(gx,gy,seed,6)<.5f?0:1,scale=.60f+Value(gx,gy,seed,7)*.23f,phase=Value(gx,gy,seed,8)*20});
            }
            for(int y=0;y<10;y++)for(int x=0;x<10;x++)
            {
                int gx=chunk.x*10+x,gy=chunk.y*10+y;var p=new Vector2((gx+.3f+Value(gx,gy,seed,443)*.4f)*3.2f,(gy+.3f+Value(gx,gy,seed,444)*.4f)*3.2f);
                if(IsExtension(p))sites.Add(new FlowerSite{id=unchecked(((long)gx<<32)|(uint)gy) ^ long.MinValue,position=p,type=6,scale=.65f,phase=Value(gx,gy,seed,445)*20});
            }
            return sites;
        }
    }

    public sealed class MeadowChunk
    {
        public GameObject root;
        public readonly List<Wildflower> flowers=new List<Wildflower>();
        public readonly List<Firefly> fireflies=new List<Firefly>();
    }

    public sealed partial class ProceduralMeadow : MonoBehaviour
    {
        public CoastGame game;
        public int Seed {get;private set;}
        public int LoadedChunks => chunks.Count;
        public int RememberedPickups => picked.Count;
        public int LoadedFlowers => chunks.Values.Sum(c=>c.flowers.Count);
        readonly Dictionary<Vector2Int,MeadowChunk> chunks=new Dictionary<Vector2Int,MeadowChunk>();
        readonly HashSet<long> picked=new HashSet<long>();
        readonly HashSet<Vector2Int> wanted=new HashSet<Vector2Int>();
        readonly List<Vector2Int> retiring=new List<Vector2Int>(),pending=new List<Vector2Int>();
        Vector2Int lastCentre=new Vector2Int(int.MinValue,int.MinValue);
        int lastRX=-1,lastRY=-1;
        public int RefreshPlans {get;private set;}
        public void Initialize(CoastGame owner)
        {
            game=owner;Seed=game.testMode?74391:new System.Random().Next(1,int.MaxValue);
            var args=Environment.GetCommandLineArgs();int seedAt=Array.IndexOf(args,"-world-seed");
            if(seedAt>=0 && seedAt+1<args.Length && int.TryParse(args[seedAt+1],out var requested) && requested>0)Seed=requested;
            WorldShape.Configure(Seed);
            Refresh(game.body.position,true);
        }
        public void RecordPickup(Wildflower flower){if(flower.procedural)picked.Add(flower.proceduralId);}
        public bool WasPicked(long id)=>picked.Contains(id);
        public void Refresh(Vector2 at,bool immediate=false)
        {
            if(game.magic.InPocket || !game.journey.IsCoast || game.journey.Cutscene)return;
            var centre=MeadowLayout.ChunkAt(at);
            int rx=Mathf.Max(1,Mathf.CeilToInt(game.cameraView.orthographicSize*game.cameraView.aspect/MeadowLayout.ChunkSize));
            int ry=Mathf.Max(1,Mathf.CeilToInt((game.cameraView.orthographicSize+3)/MeadowLayout.ChunkSize));
            if(centre!=lastCentre || rx!=lastRX || ry!=lastRY || immediate)
            {
                RefreshPlans++;lastCentre=centre;lastRX=rx;lastRY=ry;wanted.Clear();pending.Clear();retiring.Clear();
                for(int y=centre.y-ry;y<=centre.y+ry;y++)for(int x=centre.x-rx;x<=centre.x+rx;x++)
                    if(x>=-16 && x<16 && y>=0 && y<32)wanted.Add(new Vector2Int(x,y));
                foreach(var key in chunks.Keys)if(!wanted.Contains(key))retiring.Add(key);
                foreach(var key in retiring)Unload(key);
                foreach(var key in wanted)if(!chunks.ContainsKey(key))pending.Add(key);
                pending.Sort((a,b)=>{int d=(a-centre).sqrMagnitude.CompareTo((b-centre).sqrMagnitude);return d!=0?d:a.y!=b.y?a.y.CompareTo(b.y):a.x.CompareTo(b.x);});
            }
            int budget=immediate?pending.Count:1;
            while(budget-->0 && pending.Count>0){var key=pending[0];pending.RemoveAt(0);Load(key);}
        }
        void Load(Vector2Int key)
        {
            var chunk=new MeadowChunk{root=new GameObject("Flower meadow chunk "+key)};
            chunk.root.transform.SetParent(game.coastRoot.transform,false);
            var sites=MeadowLayout.Generate(key,Seed);
            foreach(var site in sites)
            {
                if(picked.Contains(site.id) || (game.buildings!=null && game.buildings.IsCleared(site.position,game.coastRoot.transform)))continue;
                var flower=new Wildflower{position=site.position,kind=CoastArtV43.PlantKind(site.type),seed=site.phase,procedural=true,proceduralId=site.id};
                int image=CoastArtV43.PlantImage(site.type,site.variant);
                flower.renderer=game.art.Renderer("Collectible meadow flower",game.art.flowers[image],site.position,CoastGame.Depth(site.position.y));
                flower.renderer.transform.SetParent(chunk.root.transform,true);flower.renderer.transform.localScale=Vector3.one*site.scale;game.art.PixelStyle(flower.renderer);
                flower.stems=game.art.FlowerWind(flower.renderer,image,site.phase);
                flower.shadow=game.Shadow("Meadow flower shadow",site.position+new Vector2(0,.035f),new Vector2(.35f,.13f),.15f);
                flower.shadow.transform.SetParent(chunk.root.transform,true);
                chunk.flowers.Add(flower);game.flowers.Add(flower);
            }
            for(int i=0;i<8 && sites.Count>0;i++)
            {
                var site=sites[(sites.Count*(i+1))/9];
                var fly=new Firefly{home=site.position,seed=site.phase,procedural=true};
                fly.dot=game.art.Renderer("Meadow night firefly",game.fireflies[0].dot.sprite,site.position,3500);
                fly.dot.sharedMaterial=game.fireflies[0].dot.sharedMaterial;fly.dot.transform.SetParent(chunk.root.transform,true);fly.dot.enabled=false;
                chunk.fireflies.Add(fly);game.fireflies.Add(fly);
            }
            chunks.Add(key,chunk);
        }
        void Unload(Vector2Int key)
        {
            var chunk=chunks[key];
            if(chunk.flowers.Contains(game.hoveredFlower))game.HighlightAt(null);
            var retiring=new HashSet<Wildflower>(chunk.flowers);game.flowers.RemoveAll(f=>retiring.Contains(f));
            foreach(var fly in chunk.fireflies)game.fireflies.Remove(fly);
            // Disable immediately: Destroy runs at the end of the frame.
            chunk.root.SetActive(false);Destroy(chunk.root);chunks.Remove(key);
        }
    }
}
