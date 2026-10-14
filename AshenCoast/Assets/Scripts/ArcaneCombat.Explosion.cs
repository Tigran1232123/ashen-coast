using System.Collections.Generic;
using UnityEngine;
namespace AshenCoast
{
    public sealed class BurstSpark
    {public SpriteRenderer renderer;public Vector2 origin,velocity;public float age,life,size;}
    public sealed partial class ArcaneCombat
    {
        public readonly List<BurstSpark> burstSparks=new List<BurstSpark>();
        Material blastRingMaterial;
        Sprite burstPixel;
        readonly MaterialPropertyBlock burstProperties=new MaterialPropertyBlock();
        void BurstResources()
        {
            if(burstPixel)return;var t=Texture2D.whiteTexture;burstPixel=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f,t.width/2f);
            blastRingMaterial=new Material(Shader.Find("AshenCoast/BlastRing"));
        }
        void CreateBlastRing(FireExplosion e)
        {
            BurstResources();e.ring=EffectSprite("Pixel shockwave on ground",burstPixel,e.position,CoastGame.Depth(e.position.y)+4);
            e.ring.sharedMaterial=blastRingMaterial;e.ring.transform.localScale=new Vector3(3,1.85f,1);
        }
        void PoseExplosion(FireExplosion e)
        {
            float phase=e.age<.36f?e.age/.36f*9:e.age<.86f?9+(e.age-.36f)/.5f*8:17+(e.age-.86f)/.59f*6;
            e.renderer.sprite=game.art.explosion[Mathf.Min(23,Mathf.FloorToInt(phase))];e.renderer.transform.localScale=Vector3.one;game.art.PixelStyle(e.renderer);
            e.renderer.color=new Color(1,1,1,Mathf.Clamp01((1.45f-e.age)/.18f));
            if(e.ring){e.ring.GetPropertyBlock(burstProperties);float p=Mathf.Clamp01(e.age/.36f);burstProperties.SetFloat("_Radius",.025f+.96f*(1-(1-p)*(1-p)));burstProperties.SetFloat("_BurstFade",Mathf.Clamp01(1-e.age/.58f));e.ring.SetPropertyBlock(burstProperties);}
        }
        void CreateBurstSparks(Vector2 point)
        {
            BurstResources();for(int i=0;i<32;i++)
            {
                float angle=i*2.39996f,speed=2.1f+(i%7)*.34f;var sr=EffectSprite("Arc of golden blast embers",burstPixel,point,CoastGame.Depth(point.y)+12);
                sr.GetPropertyBlock(burstProperties);burstProperties.SetFloat("_Emission",1);sr.SetPropertyBlock(burstProperties);
                float size=.020f+i%4*.009f;sr.transform.localScale=new Vector3(size*.5f,size*1.2f,1);sr.color=new Color(1,.93f,.58f,1);
                burstSparks.Add(new BurstSpark{renderer=sr,origin=point,velocity=new Vector2(Mathf.Cos(angle)*speed,Mathf.Sin(angle)*speed*.45f+2),life=.7f+i%6*.14f,size=size});
            }
        }
        void TickBurstSparks(float dt)
        {
            for(int i=burstSparks.Count-1;i>=0;i--)
            {
                var s=burstSparks[i];s.age+=dt;if(s.age>=s.life){Destroy(s.renderer.gameObject);burstSparks.RemoveAt(i);continue;}
                var at=s.origin+s.velocity*s.age+Vector2.down*(s.age*s.age*2.0f);at=new Vector2(Mathf.Round(at.x*40)/40,Mathf.Round(at.y*40)/40);s.renderer.transform.position=at;
                float p=s.age/s.life;s.renderer.transform.localScale=new Vector3(s.size*.5f,s.size*(p<.2f?1.2f:.5f),1);s.renderer.color=Color.Lerp(new Color(1,.93f,.58f,1),new Color(.81f,.20f,.045f,0),p);
            }
        }
    }
}
