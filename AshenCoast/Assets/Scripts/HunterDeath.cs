using UnityEngine;
namespace AshenCoast
{
    public sealed partial class NightHunter
    {
        public float deathAge=20;float deathDust;
        public bool CorpseVisible=>health<=0 && deathAge<11.5f && game.journey.Area==WorldArea.GoldenField && !game.magic.InPocket;
        void StartDeath()
        {
            deathAge=0;GaitMoving=false;Running=false;footsteps.Stop();skin.enabled=false;sprite.forceRenderingOff=false;sprite.sharedMaterial=game.art.lit;
            foreach(var child in sprite.GetComponentsInChildren<SpriteRenderer>())if(child!=sprite && child!=shadow)child.enabled=false;
            foreach(var c in sprite.GetComponents<Collider2D>())c.enabled=false;TickDeath(0);
        }
        void TickDeath(float dt)
        {
            deathAge+=dt;sprite.gameObject.SetActive(CorpseVisible);if(!CorpseVisible)return;
            sprite.sprite=game.newArt.corpse[Mathf.Min(7,(int)(deathAge*8))];sprite.transform.localScale=Vector3.one*game.newArt.CorpseScale;sprite.flipX=look.x>0;sprite.transform.rotation=Quaternion.identity;sprite.sortingOrder=CoastGame.Depth(Position.y)+1;game.art.PixelStyle(sprite);
            float fade=Mathf.Clamp01((deathAge-10)/1.5f);var block=new MaterialPropertyBlock();sprite.GetPropertyBlock(block);block.SetFloat("_CorpseDissolve",fade);sprite.SetPropertyBlock(block);sprite.color=new Color(1,1,1,1-fade*.5f);shadow.color=new Color(.025f,.018f,.035f,(1-fade)*.3f);
            if(deathAge>10){deathDust+=dt;while(deathDust>.04f){deathDust-=.04f;float seed=deathAge*37.3f;game.dragons.Evaporate(Position+new Vector2(Mathf.Sin(seed)*3.4f,Mathf.Repeat(seed*.31f,1)*1.6f),game.journey.fieldRoot.transform);}}
        }
        void RestoreLiving()
        {ClearAggression();deathAge=20;game.PlayerStruck.Remove(sprite);game.feedback.Forget(sprite);foreach(var child in sprite.GetComponentsInChildren<SpriteRenderer>())child.enabled=true;sprite.forceRenderingOff=true;skin.enabled=true;sprite.color=Color.white;foreach(var c in sprite.GetComponents<Collider2D>())c.enabled=true;var block=new MaterialPropertyBlock();sprite.GetPropertyBlock(block);block.SetFloat("_CorpseDissolve",0);sprite.SetPropertyBlock(block);}
    }
}
