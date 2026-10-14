using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed class CoastArtV34
    {
        public Sprite[,] gulls,motion;public Sprite[] campfire,corpse;public Sprite spoon;
        public CoastArtV34(ArtBank bank)
        {
            campfire=CampfireFrames(bank);
            gulls=Rows(bank,"GullsV34",4,8,1.35f);motion=Rows(bank,"TavernMotionV34",7,8,1.65f);
            corpse=Rows(bank,"AxolotlCorpseV34",2,4,4.25f).Cast<Sprite>().ToArray();
            float size=8.53f/corpse[0].bounds.size.x;
            CorpseScale=size;
            var kitchen=bank.Load("KitchenItemsV34");spoon=bank.Slice(kitchen,new RectInt(kitchen.width/2,kitchen.height/2,kitchen.width/2,kitchen.height/2),Vector2.one*.5f,240);
        }
        public float CorpseScale;
        static Sprite[] CampfireFrames(ArtBank bank)
        {
            var texture=bank.Load("CampfireFlame40V34");var bands=bank.Bands(texture,true,5);int canvasHeight=bands.Max(b=>b.y-b.x+1);
            var frames=new Sprite[40];for(int row=0;row<5;row++)for(int col=0;col<8;col++)
            {int left=col*texture.width/8,width=(col+1)*texture.width/8-left,y=texture.height-1-bands[row].y,height=bands[row].y-bands[row].x+1;frames[row*8+col]=Sprite.Create(texture,new Rect(left,y,width,height),new Vector2(.5f,0),canvasHeight/1.15f,0,SpriteMeshType.FullRect);}
            return frames;
        }
        static Sprite[,] Rows(ArtBank bank,string name,int rows,int cols,float height)
        {
            var tex=bank.Load(name);var bands=name=="GullsV34"?new List<Vector2Int>{new Vector2Int(28,267),new Vector2Int(308,460),new Vector2Int(467,675),new Vector2Int(688,868)}:bank.Bands(tex,true,rows);var data=tex.GetPixels32();var output=new Sprite[rows,cols];
            float ppu=(bands[rows==4 && name.StartsWith("Gulls")?1:0].y-bands[rows==4 && name.StartsWith("Gulls")?1:0].x+1)/height;
            for(int row=0;row<rows;row++)
            {
                var band=bands[row];var groups=new List<Vector2Int>();int first=-1,last=-1;
                for(int x=0;x<tex.width;x++){bool solid=false;for(int y=band.x;y<=band.y;y++)if(data[(tex.height-1-y)*tex.width+x].a>96){solid=true;break;}if(solid){if(first<0)first=x;last=x;}else if(first>=0 && x-last>3){groups.Add(new Vector2Int(first,last));first=-1;}}
                if(first>=0)groups.Add(new Vector2Int(first,last));bool adaptive=name.StartsWith("Gulls") && groups.Count>=6 && groups.Count<=9;
                for(int col=0;col<cols;col++)
                {
                    int x=adaptive?groups[Mathf.Min(groups.Count-1,col*groups.Count/cols)].x:col*tex.width/cols;
                    int w=adaptive?groups[Mathf.Min(groups.Count-1,col*groups.Count/cols)].y-x+1:(col+1)*tex.width/cols-x;
                    var s=bank.Slice(tex,new RectInt(x,band.x,w,band.y-band.x+1),new Vector2(.5f,0),ppu);
                    // Fixed row floor avoids shifting the ground anchor during raised wing/foot poses.
                    var r=s.rect;int bottom=tex.height-1-band.y;var pivot=new Vector2(.5f,(bottom-r.y)/r.height);
                    output[row,col]=Sprite.Create(tex,r,pivot,ppu,0,SpriteMeshType.FullRect);
                }
            }
            return output;
        }
    }
    public sealed partial class CoastGame
    {
        public CoastArtV34 newArt;
        public readonly HashSet<SpriteRenderer> PlayerStruck=new HashSet<SpriteRenderer>();
        public void PlayerHit(SpriteRenderer actor){if(actor)PlayerStruck.Add(actor);}
        public IEnumerable<ICombatTarget> WorldCreatures
        {
            get
            {
                yield return new HeroCombatActor(this);if(CoopOnline){if(coop.Context!=null)yield return new HeroCombatActor(this,true);foreach(var peer in coop.remote.Values)if(peer.Alive && peer.State.region==coop.Region && peer!=coop.Context)yield return peer;}if(amazons!=null)foreach(var ally in amazons.Allies)yield return ally;if(journey.companion.Hired && journey.companion.Alive)yield return new MageCombatActor(journey.companion);
                foreach(var actor in AdditionalTargets.ToArray())if(actor.Alive && actor.Renderer.enabled && actor.Renderer.gameObject.activeInHierarchy)yield return actor;
                if(journey.IsCoast && !magic.InPocket){foreach(var m in woodlice)if(m.vitals.Alive)yield return new WoodlouseActor(m);foreach(var f in arcane.frogs)if(f.Alive)yield return new FrogActor(f);}
            }
        }
        public int RoosterImpactSounds {get;private set;}
        public void RoosterImpact(){RoosterImpactSounds++;sounds.PlayOneShot(Resources.Load<AudioClip>("Audio/RoosterImpactV34"),.70f);}
    }
    public sealed class HeroCombatActor:ICombatTarget
    {readonly CoastGame g;readonly RemotePlayerV49 remote;public HeroCombatActor(CoastGame game,bool local=false){g=game;if(!local && g.CoopOnline)remote=g.coop.Context;}public SpriteRenderer Renderer=>remote!=null?remote.Renderer:g.CoopOnline?g.coop.LocalHero:g.hero;public Vector2 Position=>remote!=null?remote.Position:g.CoopOnline?g.coop.LocalBody.position:g.body.position;public bool Alive=>remote!=null?remote.Alive:g.CoopOnline?g.coop.LocalSurvivor.health>0:!g.Dead;public float Hurt(float n){if(remote!=null)return remote.Hurt(n);float hp=g.CoopOnline?g.coop.LocalSurvivor.health:g.survivor.health;if(g.CoopOnline)g.coop.ForLocalV49(()=>g.HurtPlayer(n));else g.HurtPlayer(n);return hp-(g.CoopOnline?g.coop.LocalSurvivor.health:g.survivor.health);}}
    public sealed class MageCombatActor:ICombatTarget
    {readonly CompanionMage m;public MageCombatActor(CompanionMage mage){m=mage;}public SpriteRenderer Renderer=>m.sprite;public Vector2 Position=>m.Position;public bool Alive=>m.Alive;public float Hurt(float n){float hp=m.health;m.Hurt(n);return hp-m.health;}}
    public sealed class WoodlouseActor:ICombatTarget
    {readonly Woodlouse m;public WoodlouseActor(Woodlouse mob){m=mob;}public SpriteRenderer Renderer=>m.renderer;public Vector2 Position=>m.Position;public bool Alive=>m.vitals.Alive;public float Hurt(float n)=>m.Hurt(n);}
    public sealed class FrogActor:ICombatTarget
    {readonly FrogMage m;public FrogActor(FrogMage mob){m=mob;}public SpriteRenderer Renderer=>m.renderer;public Vector2 Position=>m.Position;public bool Alive=>m.Alive;public float Hurt(float n)=>m.Hurt(n);public float HurtFrom(float n,SpriteRenderer source)=>m.HurtFrom(n,source);}
}
