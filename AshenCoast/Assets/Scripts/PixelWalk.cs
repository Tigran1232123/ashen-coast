using UnityEngine;

namespace AshenCoast
{
    // Two textured lower limbs counter-swing beneath the registered directional torso.
    public sealed class PixelWalk : MonoBehaviour
    {
        CoastGame game;
        readonly SpriteRenderer[] limbs=new SpriteRenderer[2];
        readonly Sprite[,] sprites=new Sprite[8,2];
        readonly Vector2[,] joints=new Vector2[8,2];
        readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
        public const float LegTop=.35f,BodyFloor=.30f;

        public void Initialize(CoastGame owner)
        {
            game=owner;
            for(int d=0;d<8;d++)for(int leg=0;leg<2;leg++)
                sprites[d,leg]=game.art.Leg(game.art.hero[d,1],leg,LegTop,out joints[d,leg]);
            for(int leg=0;leg<2;leg++)
            {
                limbs[leg]=game.art.Renderer("Animated boot "+leg,sprites[0,leg],Vector2.zero,0);
                limbs[leg].transform.SetParent(game.hero.transform,false);limbs[leg].enabled=false;
            }
        }
        void LateUpdate()
        {
            if(game==null)return;
            game.hero.GetPropertyBlock(block);block.SetFloat("_BodyFloor",game.walking?BodyFloor:0);game.hero.SetPropertyBlock(block);
            for(int leg=0;leg<2;leg++)
            {
                var limb=limbs[leg];limb.enabled=game.walking;
                if(!game.walking)continue;
                int d=game.facing;float stride=Mathf.Sin(game.walkTime*Mathf.PI*2)*(leg==0?1:-1);
                float side=Mathf.Abs(Mathf.Sin(d*Mathf.PI/4));
                float forward=d>=1 && d<=3?-1:1;
                int source=d==6?2:d;float mirror=d==6?-1:1;
                limb.sprite=sprites[source,leg];limb.flipX=d==6;game.art.PixelStyle(limb);
                var joint=joints[source,leg];joint.x*=mirror;
                limb.transform.localPosition=new Vector3(joint.x+forward*side*stride*.045f,joint.y+(1-side*.35f)*stride*.075f,0);
                limb.transform.localRotation=Quaternion.Euler(0,0,mirror*stride*(3+side*12));
                limb.sortingOrder=game.hero.sortingOrder-(stride>0?2:1);
                limb.color=game.hero.color;
            }
        }
    }
}
