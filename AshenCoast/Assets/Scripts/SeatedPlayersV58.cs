using UnityEngine;
namespace AshenCoast
{
    public sealed partial class Meditation
    {
        public void PoseOperatorV58(bool mounted)
        {
            if(!mounted)return;
            if(game.survivor.character==CharacterV51.Frog){game.hero.enabled=true;game.hero.transform.localScale=new Vector3(1,.82f,1);torso.enabled=legs.enabled=false;game.gait.Lower.enabled=false;return;}
            game.hero.enabled=false;game.gait.Lower.enabled=false;torso.enabled=legs.enabled=true;
            torso.sprite=game.hero.sprite;torso.flipX=game.hero.flipX;torso.color=game.hero.color;torso.sortingOrder=game.hero.sortingOrder;torso.transform.localPosition=Vector2.down*.37f;game.art.PixelStyle(torso);
            torso.GetPropertyBlock(properties);properties.SetFloat("_BodyFloor",.43f);torso.SetPropertyBlock(properties);
            legs.sprite=game.operatorLegsV58;legs.flipX=game.hero.flipX;legs.sortingOrder=torso.sortingOrder-1;legs.color=torso.color;legs.transform.localPosition=new Vector2(0,.02f);legs.transform.localScale=Vector3.one*(.52f/legs.sprite.bounds.size.y);game.art.PixelStyle(legs);legs.GetPropertyBlock(properties);properties.SetFloat("_SeatedLegs",0);legs.SetPropertyBlock(properties);
        }
    }
}
