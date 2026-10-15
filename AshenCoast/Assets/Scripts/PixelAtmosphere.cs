using UnityEngine;

namespace AshenCoast
{
    [RequireComponent(typeof(Camera))]
    public sealed class PixelAtmosphere : MonoBehaviour
    {
        public CoastGame game;
        public RenderTexture pixels;
        public Material atmosphere;
        Camera view;
        Texture2D fog;
        float battleHeat;
        readonly Vector4[] zenWellsV45=new Vector4[8];
        void Awake()
        {
            view=GetComponent<Camera>();
            atmosphere=new Material(Shader.Find("AshenCoast/Atmosphere"));
            fog=new Texture2D(128,128,TextureFormat.RGBA32,false);
            var colors=new Color[128*128];
            for(int y=0;y<128;y++) for(int x=0;x<128;x++)
            {
                float f=Mathf.PerlinNoise(x*.037f+7,y*.037f+14)*.55f+Mathf.PerlinNoise(x*.11f+3,y*.11f+9)*.3f+Mathf.PerlinNoise(x*.23f,y*.23f)*.15f;
                colors[y*128+x]=new Color(f,f,f,1);
            }
            fog.SetPixels(colors); fog.Apply(); fog.wrapMode=TextureWrapMode.Repeat; fog.filterMode=FilterMode.Bilinear;
            atmosphere.SetTexture("_FogTex",fog); Resize();
        }
        void Resize()
        {
            int w=Mathf.Max(1,(Screen.width+1)/2), h=Mathf.Max(1,(Screen.height+1)/2);
            view.rect=new Rect(0,0,1,1);
            view.aspect=(float)Screen.width/Mathf.Max(1,Screen.height);
            if(pixels && pixels.width==w && pixels.height==h) return;
            if(pixels) { view.targetTexture=null; pixels.Release(); Destroy(pixels); }
            pixels=new RenderTexture(w,h,16,RenderTextureFormat.ARGB32) { filterMode=FilterMode.Point, name="Pixel scene" };
            pixels.Create(); view.targetTexture=pixels;
        }
        void Update() { Resize();if(game && !game.paused)battleHeat=Mathf.MoveTowards(battleHeat,game.dragons!=null && game.dragons.InBattle?1:0,Time.deltaTime*.55f); }
        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            float t=game ? game.seconds : Time.time;
            bool pocket=game && game.magic && game.magic.InPocket;
            bool coast=!pocket && (game==null || game.journey==null || game.journey.IsCoast);
            bool outdoors=!pocket && (game==null || game.journey==null || game.journey.IsCoast || game.journey.Area==WorldArea.GoldenField);
            atmosphere.SetFloat("_Fog",outdoors?CoastTime.Fog(t):0); atmosphere.SetFloat("_Rain",coast?CoastTime.Rain(t):0);
            atmosphere.SetFloat("_Flash",game ? game.flash : 0); atmosphere.SetFloat("_Clock",t);
            atmosphere.SetFloat("_Night",pocket || (game && game.journey && (game.journey.Interior || game.journey.InFort || game.journey.InCatacombs || game.journey.Area==WorldArea.AmazonHouse || game.journey.Area==WorldArea.SnailHouse))?0:1-CoastTime.Daylight(game?game.DaySeconds:t));
            atmosphere.SetFloat("_Meditation",(game?.meditation?.Blend??0)*.16f);
            atmosphere.SetFloat("_BattleHeat",battleHeat*.15f);atmosphere.SetFloat("_EffectClock",Time.time);
            int zenCount=game?game.GroundZenWellsV45(zenWellsV45,view.transform.position):0;
            atmosphere.SetInt("_ZenWellCount",zenCount);atmosphere.SetVectorArray("_ZenWells",zenWellsV45);
            var hero=game?view.WorldToViewportPoint(game.hero.transform.position+Vector3.up*.65f):Vector3.one*.5f;
            atmosphere.SetVector("_HeroView",new Vector4(hero.x,hero.y,.75f/(view.orthographicSize*view.aspect*2),1.05f/(view.orthographicSize*2)));
            atmosphere.SetVector("_FogWorldView",new Vector4(view.transform.position.x,view.transform.position.y,view.orthographicSize*view.aspect*2,view.orthographicSize*2));
            atmosphere.SetVector("_PixelSize",new Vector4(src.width,src.height,1f/src.width,1f/src.height));
            Graphics.Blit(src,dst,atmosphere);
        }
        void OnDestroy() { if(pixels){view.targetTexture=null;pixels.Release();Destroy(pixels);}if(atmosphere)Destroy(atmosphere);if(fog)Destroy(fog); }
        public Vector2 FogCoordinates(Vector2 world)=>world*.06f;
    }

    // A screen camera presents the offscreen world; IMGUI then draws the sharp interface above it.
    public sealed class PixelPresenter : MonoBehaviour
    {
        public PixelAtmosphere source;
        Camera display;
        void Awake(){display=GetComponent<Camera>();}
        void LateUpdate(){display.rect=new Rect(0,0,1,1);display.ResetAspect();}
        void OnRenderImage(RenderTexture src,RenderTexture dst)
        { if(source && source.pixels)Graphics.Blit(source.pixels,dst);else Graphics.Blit(src,dst); }
    }
}
