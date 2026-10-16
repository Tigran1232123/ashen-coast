using UnityEngine;

namespace AshenCoast
{
    public sealed partial class Meditation
    {
        readonly CoastGame game;
        readonly SpriteRenderer torso,legs;
        readonly MaterialPropertyBlock properties=new MaterialPropertyBlock();
        float elapsed;
        public bool Active {get;private set;}
        public float Blend {get;private set;}
        public float DayOffset=>0;
        public float DaySpeed=>Active && (!game.CoopOnline || game.coop.AllMeditatingV58)?1+4*Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/4)):1;
        public SpriteRenderer VisibleTorso=>Blend>.001f?torso:game.hero;
        public Meditation(CoastGame owner)
        {
            game=owner;
            torso=game.art.Renderer("Meditating castaway torso",game.hero.sprite,Vector2.zero,0);
            torso.transform.SetParent(game.hero.transform,false);torso.enabled=false;
            legs=game.art.Renderer("Crossed knees and folded boots",game.gait.Lower.sprite,Vector2.zero,0);
            legs.transform.SetParent(game.hero.transform,false);legs.enabled=false;
        }
        public bool Toggle()
        {
            if(Active){Stop();return true;}
            if(game.survivor.health<20 || game.FeetLocked || game.paused)return false;
            Active=true;elapsed=0;game.movement=Vector2.zero;game.body.linearVelocity=Vector2.zero;
            game.combat.ResetForCheck();game.prism.StopChannel();game.magic.CancelStroke();game.arcane.CancelHeldSkills();game.arcane.CancelPlacement();
            game.facing=0;game.ui.CloseContainers();return true;
        }
        public void Stop(){Active=false;elapsed=0;Time.timeScale=game.paused?0:1;}
        public void Tick(float dt,bool moving)
        {
            if(Active && (moving || game.survivor.health<20 || game.Dead || game.magic.Teleporting || game.journey.Cutscene))Stop();
            if(Active)elapsed+=dt;
            Time.timeScale=game.paused?0:DaySpeed;
            Blend=Mathf.MoveTowards(Blend,Active?1:0,dt/(Active?.75f:.24f));
        }
        public void Pose()
        {
            bool sitting=Blend>.001f;game.hero.enabled=!sitting;torso.enabled=legs.enabled=sitting;
            if(!sitting)return;
            float p=Mathf.SmoothStep(0,1,Blend);
            torso.sprite=game.hero.sprite;torso.flipX=game.hero.flipX;torso.color=game.hero.color;
            torso.sortingOrder=game.hero.sortingOrder;torso.transform.localPosition=Vector2.down*(.42f*p);
            game.art.PixelStyle(torso);torso.GetPropertyBlock(properties);properties.SetFloat("_BodyFloor",.43f);torso.SetPropertyBlock(properties);
            game.gait.Lower.enabled=false;
            legs.sprite=game.gait.Lower.sprite;legs.sortingOrder=torso.sortingOrder-1;legs.color=torso.color;
            float height=.42f;legs.transform.localScale=new Vector3(.95f/legs.sprite.bounds.size.x,height/legs.sprite.bounds.size.y,1);
            legs.transform.localPosition=new Vector3(0,.40f+(1-p)*.31f,0);
            game.art.PixelStyle(legs);legs.GetPropertyBlock(properties);properties.SetFloat("_SeatedLegs",p);legs.SetPropertyBlock(properties);
        }
        public bool FireContact(SpriteRenderer fire)
        {
            var upper=VisibleTorso;
            return game.art.SpriteContact(fire,upper,game.gait.TorsoFloor) ||
                (Blend>.001f?legs.enabled && game.art.SpriteContact(fire,legs,float.NegativeInfinity):game.gait.Lower.enabled && game.art.SpriteContact(fire,game.gait.Lower,float.NegativeInfinity));
        }
    }
}
