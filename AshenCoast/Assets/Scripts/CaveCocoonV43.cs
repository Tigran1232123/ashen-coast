using UnityEngine;
namespace AshenCoast
{
    public sealed partial class CaveDungeon
    {
        public SpriteRenderer cocoonV43;
        void PoseCocoonV43()
        {
            if(!cocoonV43 && Snared)
            {
                var white=Texture2D.whiteTexture;var quad=Sprite.Create(white,new Rect(0,0,white.width,white.height),Vector2.one*.5f,white.width);
                cocoonV43=game.art.Renderer("Player wrapped in silk cocoon",quad,game.body.position,game.hero.sortingOrder+8);cocoonV43.sharedMaterial=new Material(Shader.Find("AshenCoast/Cocoon"));
            }
            if(!cocoonV43)return;cocoonV43.enabled=Snared;if(!Snared)return;
            cocoonV43.transform.position=game.hero.transform.position+Vector3.up*.69f;cocoonV43.transform.localScale=new Vector3(.94f,1.55f,1);cocoonV43.sortingOrder=game.hero.sortingOrder+8;
            var block=new MaterialPropertyBlock();block.SetFloat("_Clock",clock);block.SetFloat("_Fade",Mathf.Clamp01(snare/.18f));cocoonV43.SetPropertyBlock(block);
        }
    }
}
