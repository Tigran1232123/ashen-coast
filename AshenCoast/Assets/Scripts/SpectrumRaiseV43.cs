using UnityEngine;
namespace AshenCoast
{
    public sealed partial class SpectrumWeapon
    {
        public SpriteRenderer raisedArmV43;
        public Vector2 RaisedHandV43 {get;private set;}
        Vector2 shoulderV43=new Vector2(-.24f,1.14f),restHandV43;
        Vector4 armMaskV43;
        public void PoseRaiseV43()
        {
            if(!game || !game.hero)return;
            bool raising=Raising && game.survivor.equipment[0]?.kind==ItemKind.Spectrum && !Riding;
            var block=new MaterialPropertyBlock();game.hero.GetPropertyBlock(block);
            if(!raising){block.SetFloat("_RaisedArm",0);game.hero.SetPropertyBlock(block);if(raisedArmV43)raisedArmV43.enabled=false;return;}
            game.facing=0;game.hero.flipX=false;game.hero.sprite=game.HeroFramesV51(game.survivor.character)[0,3];game.art.PixelStyle(game.hero);
            if(!raisedArmV43)
            {
                var source=game.HeroFramesV51(game.survivor.character)[0,3];float ppu=source.pixelsPerUnit;var r=source.rect;
                float left=r.x+source.pivot.x-.44f*ppu,bottom=r.y+source.pivot.y+.60f*ppu,width=.32f*ppu,height=.68f*ppu;
                var crop=new Rect(left,bottom,width,height);var pivot=new Vector2((source.pivot.x+r.x+shoulderV43.x*ppu-left)/width,(source.pivot.y+r.y+shoulderV43.y*ppu-bottom)/height);
                var arm=Sprite.Create(source.texture,crop,pivot,ppu,0,SpriteMeshType.FullRect);
                raisedArmV43=game.art.Renderer("Castaway raises Spectrum with his arm",arm,game.hero.transform.position,game.hero.sortingOrder+3);
                restHandV43=game.art.HandAnchor(source,false);armMaskV43=new Vector4((left-r.x)/r.width,(bottom-r.y)/r.height,width/r.width,height/r.height);
            }
            raisedArmV43.enabled=true;float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(charge/.75f)),angle=t*174;
            raisedArmV43.transform.SetPositionAndRotation(game.hero.transform.position+(Vector3)shoulderV43,Quaternion.Euler(0,0,angle));raisedArmV43.sortingOrder=game.hero.sortingOrder+3;game.art.PixelStyle(raisedArmV43);
            game.hero.GetPropertyBlock(block);block.SetFloat("_RaisedArm",1);block.SetVector("_RaisedArmRect",armMaskV43);game.hero.SetPropertyBlock(block);
            RaisedHandV43=(Vector2)game.hero.transform.position+shoulderV43+(Vector2)(Quaternion.Euler(0,0,angle)*(restHandV43-shoulderV43));
            var weapon=game.combat.weapon;weapon.enabled=true;weapon.sprite=game.art.icons[43];weapon.transform.localScale=Vector3.one*(.88f/weapon.sprite.bounds.size.x);weapon.transform.position=RaisedHandV43+Vector2.up*.08f;weapon.transform.rotation=Quaternion.Euler(0,0,90);weapon.sortingOrder=game.hero.sortingOrder+5;game.art.PixelStyle(weapon);
            weapon.GetPropertyBlock(block);block.SetFloat("_Spectrum",1);block.SetFloat("_SpectrumGround",0);block.SetFloat("_SpectrumPlanted",0);block.SetFloat("_Emission",.54f);weapon.SetPropertyBlock(block);
        }
    }
}
