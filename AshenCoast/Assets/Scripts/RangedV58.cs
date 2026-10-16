using System.Collections.Generic;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class Combat
    {
        public bool BowDrawingV58 {get;private set;}
        public float BowChargeV58 {get;private set;}
        LineRenderer bowStringV58;
        readonly SpriteRenderer[] limbsV58=new SpriteRenderer[4];
        SpriteRenderer nockedV58;
        float bowStartV58;
        public bool BeginBowV58(Vector2 direction)
        {
            if(BowDrawingV58 || game.survivor.ArrowsV58<1){if(game.survivor.ArrowsV58<1)game.ui.Notice("Нужны стрелы");return false;}
            BowDrawingV58=true;bowStartV58=Time.time;BowChargeV58=0;active=game.survivor.equipment[0];aim=direction.sqrMagnitude>.001f?direction.normalized:Vector2.right;game.facing=CoastGame.Facing(aim);return true;
        }
        public void CancelBowV58()
        {BowDrawingV58=false;BowChargeV58=0;if(bowStringV58)bowStringV58.enabled=false;foreach(var limb in limbsV58)if(limb)limb.enabled=false;if(nockedV58)nockedV58.enabled=false;if(game){var block=new MaterialPropertyBlock();game.hero.GetPropertyBlock(block);block.SetFloat("_ArcheryV58",0);game.hero.SetPropertyBlock(block);}}
        public void TickBowInputV58(bool blocked,Vector2 cursor)
        {
            if(BowDrawingV58 && (blocked || game.survivor.equipment[0]!=active || game.Meditating || game.Dead)){CancelBowV58();return;}
            if(!blocked && game.survivor.equipment[0]?.kind==ItemKind.Bow && Input.GetMouseButtonDown(0))Begin(cursor-(Vector2)game.hero.transform.position);
            if(!BowDrawingV58)return;
            BowChargeV58=Mathf.Clamp01((Time.time-bowStartV58)/2);var delta=cursor-(Vector2)game.hero.transform.position;if(delta.sqrMagnitude>.01f)aim=delta.normalized;
            game.facing=CoastGame.Facing(aim);
            if(Input.GetMouseButtonUp(0))ReleaseBowV58();
        }
        public bool ReleaseBowV58()
        {
            if(!BowDrawingV58)return false;bool charged=BowChargeV58>=.999f;CancelBowV58();
            if(!charged)return false;return game.amazons.ShootPlayer(aim*11.5f);
        }
        LineRenderer LineV58(string name,float width,Color color)
        {var l=new GameObject(name).AddComponent<LineRenderer>();l.transform.SetParent(game.hero.transform,false);l.sharedMaterial=game.art.lit;l.useWorldSpace=true;l.positionCount=3;l.startWidth=l.endWidth=width;l.startColor=l.endColor=color;return l;}
        public void PoseBowV58()
        {
            if(!BowDrawingV58)return;
            if(!bowStringV58)
            {
                bowStringV58=LineV58("Bow drawn string",.014f,new Color(.78f,.70f,.50f));
                var pixel=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f,1);
                for(int i=0;i<4;i++){limbsV58[i]=game.art.Renderer("Bow articulated arm "+i,pixel,Vector2.zero,0);limbsV58[i].transform.SetParent(game.hero.transform,false);}
                nockedV58=game.art.Renderer("Bow nocked arrow",game.art.icons[58],Vector2.zero,0);nockedV58.transform.SetParent(game.hero.transform,false);
            }
            float scale=game.CharacterScaleV53;var origin=(Vector2)game.hero.transform.position+Vector2.up*.88f*scale;var side=new Vector2(-aim.y,aim.x);
            float draw=Mathf.SmoothStep(0,1,BowChargeV58),breath=Mathf.Sin(Time.time*5)*.008f*draw;
            var hand=origin+aim*(.55f*scale)+side*breath;var pull=origin+aim*Mathf.Lerp(.42f,-.25f,draw)*scale;
            weapon.enabled=true;weapon.flipX=true;weapon.sprite=game.art.icons[56];weapon.sharedMaterial=game.art.lit;weapon.transform.localScale=Vector3.one*(1.35f*scale/weapon.sprite.bounds.size.y);weapon.transform.position=hand;weapon.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg);weapon.sortingOrder=game.hero.sortingOrder+2;game.art.PixelStyle(weapon);
            bowStringV58.enabled=nockedV58.enabled=true;
            bowStringV58.SetPosition(0,hand-aim*.07f*scale+side*.57f*scale);bowStringV58.SetPosition(1,pull);bowStringV58.SetPosition(2,hand-aim*.07f*scale-side*.57f*scale);bowStringV58.sortingOrder=weapon.sortingOrder+1;
            var skin=game.survivor.character==CharacterV51.Frog?new Color(.35f,.47f,.25f):new Color(.69f,.44f,.29f);
            Vector2 frontShoulder=origin+side*.08f*scale,frontElbow=origin+aim*.23f*scale-side*.055f*scale;
            Vector2 rearShoulder=origin-side*.12f*scale,rearElbow=origin-aim*Mathf.Lerp(.05f,.35f,draw)*scale-side*.17f*scale;
            SegmentV58(limbsV58[0],frontShoulder,frontElbow,.075f*scale,skin);SegmentV58(limbsV58[1],frontElbow,hand,.064f*scale,skin);
            SegmentV58(limbsV58[2],rearShoulder,rearElbow,.075f*scale,skin);SegmentV58(limbsV58[3],rearElbow,pull,.064f*scale,skin);
            nockedV58.transform.position=(pull+hand+aim*.33f*scale)*.5f;nockedV58.transform.localScale=Vector3.one*((Vector2.Distance(pull,hand)+.33f*scale)/nockedV58.sprite.bounds.size.y);nockedV58.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg-90);nockedV58.sortingOrder=weapon.sortingOrder+3;game.art.PixelStyle(nockedV58);
            var block=new MaterialPropertyBlock();game.hero.GetPropertyBlock(block);block.SetFloat("_ArcheryV58",1);game.hero.SetPropertyBlock(block);
        }
        void SegmentV58(SpriteRenderer r,Vector2 a,Vector2 b,float width,Color color)
        {var delta=b-a;r.enabled=true;r.color=color;r.transform.position=(a+b)*.5f;r.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);r.transform.localScale=new Vector3(delta.magnitude,width,1);r.sortingOrder=weapon.sortingOrder+2;game.art.PixelStyle(r);}
        public void ChargeForCheckV58(float seconds){BowChargeV58=Mathf.Clamp01(seconds/2);PoseBowV58();}
    }
    public sealed partial class AmazonSettlementV46
    {
        public VillageArrowV46 SpawnStraightV58(Vector2 ground,Vector2 direction,SpriteRenderer source)
        {
            var arrow=SpawnArrow(ground,direction,source,true);arrow.straightV58=true;arrow.damageV58=100;arrow.rangeV58=game.cameraView.orthographicSize*4*game.cameraView.aspect;arrow.velocity=direction.normalized*90;arrow.height=.8f;arrow.verticalSpeed=0;arrow.point=ground+Vector2.up*.8f;return arrow;
        }
    }
    public sealed partial class RemotePlayerV49
    {
        readonly SpriteRenderer[] bowArmsV58=new SpriteRenderer[4];
        SpriteRenderer arrowV58;LineRenderer stringV58;
        void PoseBowV58()
        {
            bool drawn=Alive && State.bowDraw && State.weapon==(int)ItemKind.Bow;
            foreach(var arm in bowArmsV58)if(arm)arm.enabled=drawn;if(arrowV58)arrowV58.enabled=drawn;if(stringV58)stringV58.enabled=drawn;if(!drawn){weapon.flipX=false;return;}
            var g=session.game;
            if(!stringV58)
            {
                stringV58=new GameObject("Co-op drawn bow string").AddComponent<LineRenderer>();stringV58.transform.SetParent(Renderer.transform,false);stringV58.sharedMaterial=g.art.lit;stringV58.useWorldSpace=true;stringV58.positionCount=3;stringV58.startWidth=stringV58.endWidth=.014f;stringV58.startColor=stringV58.endColor=new Color(.78f,.70f,.50f);
                var pixel=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f,1);
                for(int i=0;i<4;i++){bowArmsV58[i]=g.art.Renderer("Co-op bow arm "+i,pixel,Vector2.zero,0);bowArmsV58[i].transform.SetParent(Renderer.transform,false);}
                arrowV58=g.art.Renderer("Co-op nocked arrow",g.art.icons[58],Vector2.zero,0);arrowV58.transform.SetParent(Renderer.transform,false);
            }
            float scale=State.character==1?.6f:1,draw=Mathf.SmoothStep(0,1,State.bowChargeV58);var aim=new Vector2(Mathf.Cos(State.angle*Mathf.Deg2Rad),Mathf.Sin(State.angle*Mathf.Deg2Rad));var side=new Vector2(-aim.y,aim.x);var origin=Position+Vector2.up*.88f*scale;
            var hand=origin+aim*.55f*scale;var pull=origin+aim*Mathf.Lerp(.42f,-.25f,draw)*scale;
            weapon.flipX=true;weapon.transform.position=hand;stringV58.SetPosition(0,hand-aim*.07f*scale+side*.57f*scale);stringV58.SetPosition(1,pull);stringV58.SetPosition(2,hand-aim*.07f*scale-side*.57f*scale);stringV58.sortingOrder=weapon.sortingOrder+1;
            var skin=State.character==1?new Color(.35f,.47f,.25f):new Color(.69f,.44f,.29f);
            Vector2 shoulder=origin+side*.08f*scale,elbow=origin+aim*.23f*scale-side*.055f*scale;
            Segment(bowArmsV58[0],shoulder,elbow,.075f*scale);Segment(bowArmsV58[1],elbow,hand,.064f*scale);
            shoulder=origin-side*.12f*scale;elbow=origin-aim*Mathf.Lerp(.05f,.35f,draw)*scale-side*.17f*scale;
            Segment(bowArmsV58[2],shoulder,elbow,.075f*scale);Segment(bowArmsV58[3],elbow,pull,.064f*scale);
            arrowV58.transform.position=(pull+hand+aim*.33f*scale)*.5f;arrowV58.transform.localScale=Vector3.one*((Vector2.Distance(pull,hand)+.33f*scale)/arrowV58.sprite.bounds.size.y);arrowV58.transform.rotation=Quaternion.Euler(0,0,State.angle-90);arrowV58.sortingOrder=weapon.sortingOrder+3;g.art.PixelStyle(arrowV58);
            void Segment(SpriteRenderer r,Vector2 a,Vector2 b,float width){var delta=b-a;r.enabled=true;r.color=skin;r.transform.position=(a+b)*.5f;r.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);r.transform.localScale=new Vector3(delta.magnitude,width,1);r.sortingOrder=weapon.sortingOrder+2;g.art.PixelStyle(r);}
        }
    }
}
