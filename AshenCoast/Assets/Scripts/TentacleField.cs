using System.Collections.Generic;
using UnityEngine;

namespace AshenCoast
{
    public sealed class TentacleLimb
    {
        public Mesh mesh;
        public MeshRenderer renderer;
        public Vector2 root;
        public float length,width,seed,lean;
        public Vector3[] vertices;
    }
    public sealed class PulledCreature
    {
        public Woodlouse mob;
        public Vector2 start;
        public float age;
    }
    public sealed class PulledSpider {public CaveSpider spider;public Vector2 start;public float age;}
    public sealed partial class TentacleField
    {
        public const float Duration=10;
        public const int PulseDamage=70;
        public SpriteRenderer caster;
        public readonly Vector2 position;
        public readonly float radius;
        public readonly GameObject root;
        public readonly List<TentacleLimb> limbs=new List<TentacleLimb>();
        public readonly List<PulledCreature> pulled=new List<PulledCreature>();
        public readonly List<PulledSpider> pulledSpiders=new List<PulledSpider>();
        public float age;
        public int poseFrame;
        public bool Finished=>age>=Duration;
        readonly ArcaneCombat owner;
        readonly CoastGame game;
        float nextDamage=.70f;
        const int Segments=24;
        public TentacleField(ArcaneCombat owner,Vector2 position,float radius)
        {
            this.owner=owner;game=owner.game;caster=game.hero;this.position=position;this.radius=radius;
            root=new GameObject("Ten-second writhing tentacle field");root.transform.SetParent(owner.WorldRoot,true);root.transform.position=position;
            var random=new System.Random(8917+Mathf.RoundToInt(position.x*117+position.y*113));var sprite=game.art.tentacle;var r=sprite.rect;var tex=sprite.texture;
            for(int i=0;i<19;i++)
            {
                float angle=i*2.399f,distance=Mathf.Sqrt((i+.5f)/19)*radius*.77f;
                var limb=new TentacleLimb{root=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)*.64f)*distance,length=1.15f+(float)random.NextDouble()*2.2f,width=.36f+(float)random.NextDouble()*.44f,seed=i*1.791f,lean=(float)(random.NextDouble()*2-1)};
                limb.vertices=new Vector3[(Segments+1)*2];var uv=new Vector2[limb.vertices.Length];var triangles=new int[Segments*6];var colors=new Color[limb.vertices.Length];
                for(int s=0;s<=Segments;s++)for(int side=0;side<2;side++)
                {
                    int v=s*2+side;uv[v]=new Vector2((r.x+side*r.width)/tex.width,(r.y+s*r.height/Segments)/tex.height);
                    colors[v]=Color.Lerp(new Color(.92f,.83f,.71f),new Color(1,.96f,.87f),(i%5)/4f);
                }
                for(int s=0;s<Segments;s++){int n=s*6,v=s*2;triangles[n]=v;triangles[n+1]=v+2;triangles[n+2]=v+1;triangles[n+3]=v+1;triangles[n+4]=v+2;triangles[n+5]=v+3;}
                limb.mesh=new Mesh{name="Tentacle with 24 independently bent segments"};limb.mesh.vertices=limb.vertices;limb.mesh.uv=uv;limb.mesh.colors=colors;limb.mesh.triangles=triangles;limb.mesh.MarkDynamic();
                var go=new GameObject("Brown tentacle "+i);go.transform.SetParent(root.transform,false);go.AddComponent<MeshFilter>().sharedMesh=limb.mesh;limb.renderer=go.AddComponent<MeshRenderer>();
                limb.renderer.sharedMaterial=game.art.lit;limb.renderer.sortingOrder=CoastGame.Depth(position.y+limb.root.y)+3;
                var block=new MaterialPropertyBlock();block.SetTexture("_MainTex",tex);block.SetVector("_SpriteRect",new Vector4(r.x/tex.width,r.y/tex.height,r.width/tex.width,r.height/tex.height));
                block.SetFloat("_ArtStep",Mathf.Max(1,r.height/150));block.SetFloat("_AlphaCut",.38f);block.SetFloat("_GroundClip",1);block.SetFloat("_GroundY",position.y+limb.root.y-.03f);limb.renderer.SetPropertyBlock(block);
                var hole=game.Shadow("Broken earth beneath tentacle",position+limb.root,new Vector2(.48f,.20f),.74f);hole.transform.SetParent(root.transform,true);limbs.Add(limb);
            }
            Pose();
        }
        public void Tick(float dt)
        {
            age+=dt;poseFrame=Mathf.FloorToInt(age*30);Pose();if(game.CoopOnline && !game.coop.CanSimulate)return;
            if(root.activeInHierarchy && game.cave.Inside && age>.55f && age<Duration)foreach(var spider in game.cave.spiders)if(spider.Alive && !spider.Grabbed && Vector2.Distance(spider.Position,position)<radius && !game.WorldBlocked(position,spider.Position,.03f)){spider.Grabbed=true;spider.sprite.GetComponent<Collider2D>().enabled=false;game.PlayerHit(spider.sprite);pulledSpiders.Add(new PulledSpider{spider=spider,start=spider.Position});}
            foreach(var caught in pulledSpiders){if(!caught.spider.Alive)continue;caught.age+=dt;float p=Mathf.Clamp01(caught.age/1.25f);var spider=caught.spider;spider.sprite.transform.position=caught.start+Vector2.down*(.8f*p*p);spider.sprite.transform.rotation=Quaternion.Euler(0,0,Mathf.Sin(caught.age*17)*12*(1-p));var b=new MaterialPropertyBlock();spider.sprite.GetPropertyBlock(b);b.SetFloat("_GroundClip",1);b.SetFloat("_GroundY",caught.start.y+p*.18f);spider.sprite.SetPropertyBlock(b);if(p>=1)game.HurtTarget(spider,1000,caster,DamageDeliveryV54.Tentacle);}
            bool damageTick=root.activeInHierarchy && age<Duration && age>=nextDamage;
            if(damageTick){nextDamage+=.40f;foreach(var enemy in game.AdditionalTargets)if(enemy.Alive && !(enemy is CaveSpider spider && spider.Grabbed) && Vector2.Distance(enemy.Position,position)<radius && (!game.WorldBlocked(position,enemy.Position,.03f))){game.PlayerHit(enemy.Renderer);game.HurtTarget(enemy,PulseDamage,caster,DamageDeliveryV54.Tentacle);}}
            if(root.activeInHierarchy && !game.magic.InPocket && game.journey.IsCoast && age<Duration)
            {
                foreach(var mob in game.woodlice)
                    if(mob.Small && mob.vitals.Alive && !mob.Grabbed && age>.80f && Vector2.Distance(mob.Position,position)<radius)
                    {game.PlayerHit(mob.renderer);mob.Grabbed=true;mob.feet.enabled=false;pulled.Add(new PulledCreature{mob=mob,start=mob.Position});}
                if(damageTick)
                {
                    foreach(var mob in game.woodlice)if(mob.vitals.Alive && !mob.Grabbed && Vector2.Distance(mob.Position,position)<radius){game.PlayerHit(mob.renderer);game.HurtTarget(new WoodlouseActor(mob),PulseDamage,caster,DamageDeliveryV54.Tentacle);}
                    foreach(var frog in owner.frogs)if(frog.Alive && Vector2.Distance(frog.Position,position)<radius){game.PlayerHit(frog.renderer);game.HurtTarget(new FrogActor(frog),PulseDamage,caster,DamageDeliveryV54.Tentacle);}
                    if(game.dummyHealth>0 && Vector2.Distance(game.DummyBase,position)<radius)game.DamageDummy(PulseDamage);
                }
            }
            foreach(var caught in pulled)
            {
                if(!caught.mob.vitals.Alive)continue;caught.age+=dt;float progress=Mathf.Clamp01(caught.age/1.25f);
                var mob=caught.mob;mob.renderer.transform.position=caught.start+new Vector2(Mathf.Sin(caught.age*18)*.055f,-.72f*progress*progress);
                mob.renderer.transform.rotation=Quaternion.Euler(0,0,Mathf.Sin(caught.age*11)*12*(1-progress));
                var block=new MaterialPropertyBlock();mob.renderer.GetPropertyBlock(block);block.SetFloat("_GroundClip",1);block.SetFloat("_GroundY",caught.start.y+progress*.20f);mob.renderer.SetPropertyBlock(block);
                mob.shadow.color=new Color(.04f,.03f,.02f,(1-progress)*.42f);
                if(progress>=1){game.HurtTarget(new WoodlouseActor(mob),mob.vitals.health*10,caster,DamageDeliveryV54.Tentacle);mob.renderer.transform.rotation=Quaternion.identity;}
            }
        }
        void Pose()
        {
            float time=poseFrame/30f;float emergence=Mathf.SmoothStep(0,1,Mathf.Clamp01(age/.72f))*Mathf.SmoothStep(0,1,Mathf.Clamp01((Duration-age)/.65f));
            for(int i=0;i<limbs.Count;i++)
            {
                var limb=limbs[i];float bend=limb.lean*.6f+Mathf.Sin(time*(1.9f+i%4*.41f)+limb.seed)*.70f;
                float curl=Mathf.Sin(time*(2.8f+i%3*.31f)+limb.seed*.8f);
                var centre=new Vector2[Segments+1];
                for(int s=0;s<=Segments;s++)
                {
                    float t=s/(float)Segments;
                    float x=bend*limb.length*t*t+Mathf.Sin(t*5.4f-time*2.6f+limb.seed)*t*.44f;
                    float y=limb.length*(t-.26f*t*t*(1+curl))+Mathf.Sin(time*3.1f+limb.seed+t*4)*t*.12f-limb.length*(1-emergence);
                    centre[s]=limb.root+new Vector2(x,y);
                }
                for(int s=0;s<=Segments;s++)
                {
                    var tangent=centre[Mathf.Min(Segments,s+1)]-centre[Mathf.Max(0,s-1)];var normal=new Vector2(-tangent.y,tangent.x).normalized;
                    float width=limb.width*(1-s/(float)Segments*.40f);
                    for(int side=0;side<2;side++){var p=centre[s]+normal*width*(side==0?-.5f:.5f);limb.vertices[s*2+side]=new Vector3(Mathf.Round(p.x*80)/80,Mathf.Round(p.y*80)/80,0);}
                }
                limb.mesh.vertices=limb.vertices;limb.mesh.RecalculateBounds();
            }
        }
        public void Dispose()
        {
            foreach(var caught in pulled)if(caught.mob.vitals.Alive)
            {caught.mob.Grabbed=false;caught.mob.feet.enabled=true;caught.mob.renderer.transform.position=caught.start;var b=new MaterialPropertyBlock();caught.mob.renderer.GetPropertyBlock(b);b.SetFloat("_GroundClip",0);caught.mob.renderer.SetPropertyBlock(b);}
            foreach(var caught in pulledSpiders)if(caught.spider.Alive){caught.spider.Grabbed=false;caught.spider.sprite.GetComponent<Collider2D>().enabled=true;caught.spider.sprite.transform.position=caught.start;var b=new MaterialPropertyBlock();caught.spider.sprite.GetPropertyBlock(b);b.SetFloat("_GroundClip",0);caught.spider.sprite.SetPropertyBlock(b);}
            foreach(var limb in limbs)Object.Destroy(limb.mesh);Object.Destroy(root);
        }
    }
}
