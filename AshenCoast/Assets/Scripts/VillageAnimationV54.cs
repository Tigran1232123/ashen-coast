using UnityEngine;
namespace AshenCoast
{
    public sealed partial class VillageActorV46
    {
        readonly MaterialPropertyBlock poseBlockV54=new MaterialPropertyBlock();
        public int BodyFrameV54 {get;private set;}
        static readonly Vector2[][] gripsV54={
            new[]{new Vector2(.20f,.92f),new Vector2(.23f,.94f),new Vector2(.31f,.95f),new Vector2(.49f,.96f),new Vector2(.82f,.96f),new Vector2(.66f,.95f),new Vector2(.36f,.94f),new Vector2(.22f,.93f)},
            new[]{new Vector2(.20f,1.00f),new Vector2(.08f,1.60f),new Vector2(.12f,1.54f),new Vector2(.55f,1.05f),new Vector2(.70f,.94f),new Vector2(.15f,1.00f),new Vector2(.20f,1.04f),new Vector2(.20f,1.02f)},
            new[]{new Vector2(.25f,.99f),new Vector2(.62f,1.10f),new Vector2(.80f,.90f),new Vector2(.67f,.65f),new Vector2(.58f,.68f),new Vector2(.50f,.65f),new Vector2(.35f,1.05f),new Vector2(.23f,1.07f)},
            new[]{new Vector2(.28f,1.03f),new Vector2(.67f,1.09f),new Vector2(.69f,1.09f),new Vector2(.69f,1.09f),new Vector2(.59f,1.06f),new Vector2(.35f,1.02f),new Vector2(.30f,.99f),new Vector2(.26f,1.03f)}
        };
        int CombatRowV54=>Weapon?.kind==ItemKind.Bow?3:Weapon?.kind==ItemKind.Axe?1:Weapon?.kind==ItemKind.Dagger?2:0;
        void PoseV54(bool walking)
        {
            var game=village.game;float p=Attacking?Mathf.Clamp01(attackAge/Mathf.Max(.01f,duration)):0;
            bool leaning=Slaver && !Hired && !Hostile && !Attacking;
            BodyFrameV54=Attacking?Mathf.Min(7,(int)(p*8)):walking?(int)(step/1.4f*8)%8:0;
            Renderer.sprite=IsSlave?village.captivesV54[index%4,walking?BodyFrameV54:0]:leaning?village.leanV54[0,(int)(clock*2)%6]:village.combatV54[CombatRowV54,Attacking?BodyFrameV54:0];
            if(!leaning && Mathf.Abs(aim.x)>.32f)Renderer.flipX=aim.x<0;else if(leaning)Renderer.flipX=false;
            Renderer.transform.localScale=Vector3.one;Renderer.sortingOrder=CoastGame.Depth(Position.y);Renderer.color=tint;game.art.PixelStyle(Renderer);
            if(Attacking || IsSlave || leaning)
            {gaitV53.Lower.enabled=false;Renderer.GetPropertyBlock(poseBlockV54);poseBlockV54.SetFloat("_BodyFloor",0);Renderer.SetPropertyBlock(poseBlockV54);}
            else gaitV53.PoseSideV53(step/1.4f,walking,aim);
            shadow.transform.position=Position;shadow.sortingOrder=-2000;
            var weapon=Weapon;weaponSprite.enabled=weapon!=null && Attacking;if(!weaponSprite.enabled)return;
            weaponSprite.sprite=game.art.icons[weapon.Definition.icon];bool bow=weapon.kind==ItemKind.Bow,vertical=Combat.VerticalV44(weapon.kind) || bow;
            float length=bow?1.40f:Combat.WeaponLengthV44(weapon.kind);
            weaponSprite.transform.localScale=Vector3.one*(length/((vertical?weaponSprite.sprite.rect.height:weaponSprite.sprite.rect.width)/weaponSprite.sprite.pixelsPerUnit));
            float direction=Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg;
            float swing=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.12f,.68f,p));
            var grip=gripsV54[CombatRowV54][BodyFrameV54];var hand=Position+new Vector2(Renderer.flipX?-grip.x:grip.x,grip.y);
            if(IsSlave)hand=Position+new Vector2(Renderer.flipX?-.22f:.22f,.92f);
            if(bow)direction+=90;
            else if(!Combat.ThrustV44(weapon.kind))direction+=(.5f-swing)*weapon.Definition.arc;
            weaponSprite.transform.position=hand;weaponSprite.transform.rotation=Quaternion.Euler(0,0,direction-(vertical?90:0));weaponSprite.sortingOrder=Renderer.sortingOrder+2;game.art.PixelStyle(weaponSprite);
        }
        void AttackFootworkV54(float dt)
        {
            if(IsSlave || Weapon?.kind==ItemKind.Bow || victim==null)return;
            float p=attackAge/Mathf.Max(.01f,duration);float speed=p>.16f && p<.43f?1.9f:p>.64f && p<.92f?-1.3f:0;
            var move=aim*(speed*dt);
            if(move.sqrMagnitude>0 && !village.game.WorldBlocked(Position,Position+move,.24f)){Renderer.transform.position+=(Vector3)move;step+=move.magnitude;}
        }
    }
}
