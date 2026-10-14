using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class CoastGame
    {
        void CreateWorld()
        {
            var prior=new HashSet<GameObject>(FindObjectsByType<Transform>(FindObjectsSortMode.None).Select(t=>t.gameObject));
            coastRoot=new GameObject("Coastal world");
            var terrain=GameObject.CreatePrimitive(PrimitiveType.Quad); terrain.name="Textured sand, meadow, animated sea";
            terrain.transform.position=new Vector3(0,500,5); terrain.transform.localScale=new Vector3(1100,1100,1);
            Destroy(terrain.GetComponent<Collider>());var groundMaterial=new Material(Shader.Find("AshenCoast/Terrain"));
            groundMaterial.SetTexture("_GroundTex",Resources.Load<Texture2D>("Art/TerrainTiles"));terrain.GetComponent<MeshRenderer>().sharedMaterial=groundMaterial;
            Shadow("Ship shadow",ShipBase+new Vector2(0,.45f),new Vector2(12.8f,2.5f),.28f);
            ship=art.Renderer("Wreck / storage",art.ship,ShipBase,Depth(ShipBase.y));
            var footprint=new GameObject("Hull walk collision (ground footprint)"); footprint.transform.position=ShipBase;
            var polygon=footprint.AddComponent<PolygonCollider2D>();
            hull=polygon;
            polygon.points=new[] { new Vector2(-5.9f,1.1f),new Vector2(-5.0f,2.5f),new Vector2(3.8f,2.25f),new Vector2(6.2f,1.15f),new Vector2(5.6f,.35f),new Vector2(1.9f,.04f),new Vector2(-1.5f,.3f),new Vector2(-4.2f,.65f) };
            Shadow("Mannequin shadow",DummyBase+new Vector2(.05f,.1f),new Vector2(.9f,.3f),.29f);
            dummy=art.Renderer("Training mannequin",art.mannequin,DummyBase,Depth(DummyBase.y));
            var dc=new GameObject("Mannequin base collision"); dc.transform.position=DummyBase+new Vector2(0,.12f);
            var collider=dc.AddComponent<CircleCollider2D>(); collider.radius=.25f;
            dc.transform.SetParent(dummy.transform,true);
            hero=art.Renderer("Castaway / eight-direction animation",art.hero[0,1],new Vector2(-1.5f,-1.65f),Depth(-1.65f));
            body=hero.gameObject.AddComponent<Rigidbody2D>(); body.gravityScale=0; body.constraints=RigidbodyConstraints2D.FreezeRotation;
            body.collisionDetectionMode=CollisionDetectionMode2D.Continuous; body.interpolation=RigidbodyInterpolation2D.Interpolate;
            body.linearDamping=0;
            var feet=hero.gameObject.AddComponent<CircleCollider2D>(); feet.radius=.22f; feet.offset=new Vector2(0,.13f);
            var playerShadow=Shadow("Castaway shadow",(Vector2)hero.transform.position+new Vector2(0,.1f),new Vector2(.62f,.24f),.34f);
            playerShadow.transform.SetParent(hero.transform,true);
            heroShadow=playerShadow;
            woodlice.Add(new Woodlouse(this,new Vector2(7.7f,.15f)));
            woodlice.Add(new Woodlouse(this,new Vector2(-.5f,6.8f),true));
            woodlice.Add(new Woodlouse(this,new Vector2(3.7f,8.4f),true));
            woodlice.Add(new Woodlouse(this,new Vector2(8.5f,7.8f),true));
            UnityEngine.Random.InitState(74391);
            for(int i=0;i<15;i++)flowerPatches.Add(new Vector4(UnityEngine.Random.Range(-26f,26f),UnityEngine.Random.Range(7f,27f),i%3,UnityEngine.Random.Range(2.8f,4.7f)));
            var accepted=new List<Vector2>();
            for(int attempt=0;attempt<28000 && flowers.Count<1050;attempt++)
            {
                var p=new Vector2(UnityEngine.Random.Range(-29f,29f),UnityEngine.Random.Range(4.7f,29f));
                if(p.y < 4.8f+Mathf.Sin(p.x*.15f)*.55f) continue;
                if(accepted.Any(q=>(p-q).sqrMagnitude<.87f*.87f)) continue;
                accepted.Add(p); int type=UnityEngine.Random.Range(0,6);
                var patch=flowerPatches.OrderBy(c=>Vector2.Distance(p,new Vector2(c.x,c.y))/c.w).First();
                if(type<3 && Vector2.Distance(p,new Vector2(patch.x,patch.y))<patch.w)type=(int)patch.z;
                float scale=UnityEngine.Random.Range(.60f,.83f);
                var flower=new Wildflower { position=p, kind=CoastArtV43.PlantKind(type), seed=UnityEngine.Random.value*20 };
                flower.renderer=art.Renderer("Collectible " + ItemDefinition.All[flower.kind].name,art.flowers[CoastArtV43.PlantImage(type,flowers.Count%2)],p,Depth(p.y));
                flower.renderer.transform.localScale=Vector3.one*scale;
                art.PixelStyle(flower.renderer);
                flower.stems=art.FlowerWind(flower.renderer,CoastArtV43.PlantImage(type,flowers.Count%2),flower.seed);
                flower.shadow=Shadow("Flower shadow",p+new Vector2(0,.035f),new Vector2(.35f,.13f),.15f);
                flowers.Add(flower);
            }
            InitialCloversV43();
            for(int i=0;i<32;i++)
            {
                var f=new Firefly { home=new Vector2(UnityEngine.Random.Range(-28f,28f),UnityEngine.Random.Range(5,28)),seed=UnityEngine.Random.Range(0f,30f) };
                f.dot=art.Renderer("Night firefly",dotSprite,f.home,3500); f.dot.sharedMaterial=new Material(Shader.Find("Sprites/Default"));
                fireflies.Add(f);
            }
            foreach(var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if(t.parent==null && !prior.Contains(t.gameObject) && t.gameObject!=coastRoot && t.gameObject!=hero.gameObject)t.SetParent(coastRoot.transform,true);
        }
        public SpriteRenderer Shadow(string name, Vector2 at, Vector2 size, float alpha)
        {
            var sr=art.Renderer(name,shadowSprite,at,Mathf.Min(-2000,Depth(at.y)-1600)); sr.color=new Color(.08f,.10f,.09f,alpha);
            if(coastRoot)sr.transform.SetParent(coastRoot.transform,true);
            sr.transform.localScale=new Vector3(size.x,size.y,1); return sr;
        }
        static Sprite MakeEllipse(int w,int h)
        {
            var t=new Texture2D(w,h,TextureFormat.RGBA32,false); t.filterMode=FilterMode.Point;
            var data=new Color[w*h];
            for(int y=0;y<h;y++) for(int x=0;x<w;x++)
            {
                float dx=(x+.5f-w*.5f)/(w*.5f),dy=(y+.5f-h*.5f)/(h*.5f);
                data[y*w+x]=new Color(1,1,1,Mathf.Clamp01((1-dx*dx-dy*dy)*3));
            }
            t.SetPixels(data); t.Apply(); return Sprite.Create(t,new Rect(0,0,w,h),new Vector2(.5f,.5f),w);
        }
        static Sprite MakeDot()
        {
            var t=new Texture2D(3,3,TextureFormat.RGBA32,false); t.filterMode=FilterMode.Point;
            t.SetPixels(new[] {Color.clear,Color.white,Color.clear,Color.white,Color.white,Color.white,Color.clear,Color.white,Color.clear}); t.Apply();
            return Sprite.Create(t,new Rect(0,0,3,3),new Vector2(.5f,.5f),50);
        }
    }
}
