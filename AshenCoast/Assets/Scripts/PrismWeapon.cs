using UnityEngine;

namespace AshenCoast
{
    [DefaultExecutionOrder(1100)]
    public sealed class PrismWeapon : MonoBehaviour
    {
        public const float ChargeSeconds=1.8f,ThinDrain=6.5f,FusedDrain=28,FullDps=420;
        public CoastGame game;
        public bool Channeling {get;private set;}
        public float ChannelTime {get;private set;}
        public float Charge=>Mathf.SmoothStep(0,1,Mathf.Clamp01(ChannelTime/ChargeSeconds));
        public float Spread=>60*(1-Charge);
        public float Angle {get;private set;}
        public float Range=>Mathf.Max(64,game.cameraView.orthographicSize*4*Mathf.Sqrt(1+game.cameraView.aspect*game.cameraView.aspect));
        public Vector2 Origin=>game.combat.weapon.enabled?(Vector2)game.combat.weapon.transform.TransformPoint(new Vector3(0,game.combat.weapon.sprite.bounds.max.y,0)):(Vector2)game.hero.transform.position+new Vector2(0,.9f);
        public readonly LineRenderer[] rays=new LineRenderer[7];
        public readonly Vector2[] ends=new Vector2[6];
        readonly float[] flowPhase=new float[6];
        readonly Vector4[] lightSegments=new Vector4[7],lightStyle=new Vector4[7];
        readonly AnimationCurve[] throatCurves=new AnimationCurve[7];
        readonly float[] curveLengths=new float[7];
        public bool ApplyingDamage {get;private set;}
        public PrismAudio Audio {get;private set;}
        public int DamageTicks {get;private set;}
        public float PaidMana {get;private set;}
        float targetAngle,angleVelocity,damageClock,clock,fade,focusSeed;
        Vector2 lastBeamOrigin;
        int channels;
        Material beamMaterial;
        readonly RaycastHit2D[] obstacles=new RaycastHit2D[24];
        readonly MaterialPropertyBlock properties=new MaterialPropertyBlock();
        bool Ready=>game.survivor.equipment[0]?.kind==ItemKind.LastPrism && game.survivor.equipment[0].Usable && game.survivor.mana>0;
        bool Free=>!game.Dead && !game.Meditating && !game.MovementLocked && !game.paused && !game.ui.IsPointerOver && !game.ui.Dragging && !game.ui.IsCrafting && !game.magic.Drawing && !game.arcane.Placing && !game.journey.HireOffer;
        public void Initialize(CoastGame owner)
        {
            game=owner;beamMaterial=new Material(Shader.Find("AshenCoast/PrismBeam"));Audio=new PrismAudio(game);Shader.SetGlobalInt("_PrismSegmentCount",0);
            for(int i=0;i<rays.Length;i++)
            {
                var go=new GameObject(i==6?"Concentrated Last Prism beam":"Converging Last Prism ray "+i);go.transform.SetParent(transform,false);
                var line=go.AddComponent<LineRenderer>();line.sharedMaterial=beamMaterial;line.useWorldSpace=true;line.positionCount=4;line.textureMode=LineTextureMode.Stretch;
                line.numCapVertices=0;line.numCornerVertices=0;line.enabled=false;rays[i]=line;
                throatCurves[i]=new AnimationCurve(new Keyframe(0,.008f),new Keyframe(.01f,.20f),new Keyframe(.02f,1),new Keyframe(1,.94f));
            }
        }
        public bool BeginChannel(Vector2 direction)
        {
            if(!Ready || !Free)return false;
            if(Channeling)return true;
            Channeling=true;ChannelTime=0;damageClock=0;fade=1;angleVelocity=0;
            focusSeed=++channels*1.6180339f;for(int i=0;i<6;i++)flowPhase[i]=i*1.71f+focusSeed;
            Angle=targetAngle=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg;Pose();return true;
        }
        public void SetAim(Vector2 direction){if(direction.sqrMagnitude>.0001f)targetAngle=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg;}
        void Update()
        {
            if(!game || game.paused || game.testMode)return;
            if(Input.GetMouseButton(0) && Free && Ready){if(!Channeling)BeginChannel(game.CursorWorld-Origin);SetAim(game.CursorWorld-Origin);}
            else StopChannel();
            Tick(Time.deltaTime);
        }
        void LateUpdate()
        {
            // The hero's final gait pose changes hand anchors in LateUpdate.
            // Render the weapon and beam from that same final anchor.
            if(game && game.paused){Audio?.Silence();return;}
            if(game && !game.Meditating && (Channeling || fade>0 || game.survivor.equipment[0]?.kind==ItemKind.LastPrism))Pose();
        }
        public void Tick(float dt)
        {
            clock+=dt;
            if(Channeling && (!Ready || game.Dead || game.Meditating || game.magic.Teleporting || game.magic.Drawing || game.journey.Cutscene || game.survivor.equipment[0]?.kind!=ItemKind.LastPrism))StopChannel();
            float remaining=dt;
            while(Channeling && remaining>.00001f)
            {
                float step=Mathf.Min(.04f,remaining);
                if(ChannelTime<ChargeSeconds && ChargeSeconds-ChannelTime<.0001f)ChannelTime=ChargeSeconds;
                if(ChannelTime<ChargeSeconds)step=Mathf.Min(step,ChargeSeconds-ChannelTime);
                float drain=ChannelTime<ChargeSeconds?ThinDrain:FusedDrain;
                float paid=Mathf.Min(step,game.survivor.mana/drain);if(paid<=.000001f){StopChannel();break;}
                float cost=paid*drain;game.survivor.SpendMana(cost);PaidMana+=cost;game.survivor.equipment[0].Wear(paid*.07f);
                ChannelTime+=paid;remaining-=paid;fade=1;
                for(int i=0;i<6;i++)flowPhase[i]+=paid*38*RayFlowSpeed(i);
                Angle=Mathf.SmoothDampAngle(Angle,targetAngle,ref angleVelocity,.22f,230,paid);
                if(!game.walking)game.facing=CoastGame.Facing(Direction(Angle));
                Pose();damageClock+=paid;
                while(damageClock>=.08f){damageClock-=.08f;DealDamage(.08f);}
                if(game.survivor.mana<=.00001f || !game.survivor.equipment[0].Usable)StopChannel();
            }
            if(!Channeling)fade=Mathf.Max(0,fade-dt/.16f);
            Pose();Audio.Tick(Channeling,Charge,dt);
        }
        public void StopChannel(){Channeling=false;damageClock=0;}
        public float RayOffset(int ray)
        {
            float p=Mathf.Clamp01(ChannelTime/ChargeSeconds);if(p>=1)return 0;
            float seed=focusSeed+ray*2.399963f;
            float speed=.73f+.68f*(.5f+.5f*Mathf.Sin(seed*1.71f));
            float focus=Mathf.Pow(Mathf.SmoothStep(0,1,p),speed);
            float baseAngle=Mathf.Lerp(-30,30,ray/5f)*(1-focus);
            float envelope=Mathf.Sin(p*Mathf.PI)*Mathf.Pow(1-p,.65f);
            float drift=Mathf.Sin(seed+p*(13+ray*1.3f)+Mathf.Sin(p*19+seed)*.85f)*6;
            drift+=Mathf.Sin(seed*1.4f+p*(24-ray))*2.8f;
            return Mathf.Clamp(baseAngle+drift*envelope,-30,30);
        }
        public float RayFlowSpeed(int ray)
        {
            float seed=focusSeed+ray*2.399963f;
            float speed=1.05f+.38f*Mathf.Sin(seed+ChannelTime*(5.3f+ray*.9f));
            speed+=.22f*Mathf.Sin(seed*1.7f+ChannelTime*(11.7f-ray*.7f));
            return Mathf.Lerp(speed,1,Charge);
        }
        static Vector2 Direction(float degrees)=>new Vector2(Mathf.Cos(degrees*Mathf.Deg2Rad),Mathf.Sin(degrees*Mathf.Deg2Rad));
        void Pose()
        {
            if(!game)return;
            if(game.Meditating){foreach(var ray in rays)ray.enabled=false;Shader.SetGlobalInt("_PrismSegmentCount",0);return;}
            game.combat.PoseHeldPrism();
            if(fade<=0){foreach(var ray in rays)ray.enabled=false;Shader.SetGlobalInt("_PrismSegmentCount",0);return;}
            float progress=Charge;var origin=Channeling?Origin:lastBeamOrigin;if(Channeling)lastBeamOrigin=origin;
            beamMaterial.SetFloat("_Charge",progress);beamMaterial.SetFloat("_BeamTime",clock);beamMaterial.SetFloat("_Length",Range);
            for(int i=0;i<6;i++)
            {
                Vector2 direction=Direction(Angle+RayOffset(i));
                float length=UnblockedLength(origin,direction,Range);ends[i]=origin+direction*length;
                var line=rays[i];line.enabled=fade>0 && progress<.997f;
                line.widthMultiplier=Mathf.Lerp(.042f,.075f,progress);Taper(i,origin,direction,length,.18f);
                line.startColor=line.endColor=new Color(.86f+i*.015f,.94f,1,fade*.88f);
                line.sortingOrder=game.hero.sortingOrder+4;
                line.GetPropertyBlock(properties);properties.SetFloat("_RayFlow",1);properties.SetFloat("_RayPhase",flowPhase[i]);properties.SetFloat("_Length",length);properties.SetFloat("_Fused",0);line.SetPropertyBlock(properties);
            }
            var fused=rays[6];fused.enabled=fade>0 && progress>.68f;var main=Direction(Angle);
            float width=Mathf.Lerp(.08f,.52f,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.68f,1,progress)))*(1+.035f*Mathf.Sin(clock*27));
            fused.widthMultiplier=width;float alpha=fade*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.68f,1,progress));
            fused.startColor=fused.endColor=new Color(.99f,.98f,1,alpha);fused.sortingOrder=game.hero.sortingOrder+5;
            float fusedLength=UnblockedLength(origin,main,Range);
            Taper(6,origin,main,fusedLength,.68f);
            fused.GetPropertyBlock(properties);properties.SetFloat("_Length",fusedLength);properties.SetFloat("_Fused",1);properties.SetFloat("_RayFlow",0);fused.SetPropertyBlock(properties);
            PublishLight();
        }
        void Taper(int index,Vector2 start,Vector2 direction,float length,float throat)
        {
            var line=rays[index];var curve=throatCurves[index];
            float t=Mathf.Clamp(throat/Mathf.Max(.001f,length),.0001f,.49f);
            if(Mathf.Abs(curveLengths[index]-length)>.001f){curve.MoveKey(1,new Keyframe(t*.35f,.20f));curve.MoveKey(2,new Keyframe(t,1));line.widthCurve=curve;curveLengths[index]=length;}
            line.SetPosition(0,start);line.SetPosition(1,start+direction*(length*t*.35f));line.SetPosition(2,start+direction*(length*t));line.SetPosition(3,start+direction*length);
        }
        void PublishLight()
        {
            int count=0;
            for(int i=0;i<rays.Length;i++)if(rays[i].enabled)
            {
                var start=rays[i].GetPosition(0);var end=rays[i].GetPosition(3);
                lightSegments[count]=new Vector4(start.x,start.y,end.x,end.y);
                lightStyle[count]=new Vector4(i==6?2.6f:1.15f,rays[i].startColor.a*(i==6?1.15f:.52f),0,0);count++;
            }
            Shader.SetGlobalVectorArray("_PrismSegments",lightSegments);Shader.SetGlobalVectorArray("_PrismBeamStyle",lightStyle);Shader.SetGlobalInt("_PrismSegmentCount",count);
        }
        float UnblockedLength(Vector2 origin,Vector2 direction,float reach)
        {
            int count=Physics2D.RaycastNonAlloc(origin-Vector2.up*.72f,direction,obstacles,reach);float result=reach;
            for(int i=0;i<count;i++)
            {
                var h=obstacles[i];if(!h.collider || h.collider.isTrigger || h.collider.gameObject==game.hero.gameObject || Character(h.collider))continue;
                if(h.distance>.08f)result=Mathf.Min(result,h.distance);
            }
            return result;
        }
        bool Character(Collider2D collider)
        {
            bool Matches(SpriteRenderer actor)=>actor && (collider.transform==actor.transform || collider.transform.IsChildOf(actor.transform));
            if(Matches(game.dummy) || Matches(game.journey.companion.sprite))return true;
            foreach(var enemy in game.AdditionalTargets)if(Matches(enemy.Renderer))return true;
            foreach(var mob in game.woodlice)if(Matches(mob.renderer))return true;
            foreach(var frog in game.arcane.frogs)if(Matches(frog.renderer))return true;
            return false;
        }
        bool Hits(SpriteRenderer actor,Vector2 end,float width)
        {
            if(!actor || !actor.enabled || !actor.gameObject.activeInHierarchy)return false;
            var origin=Origin;var delta=end-origin;float length=delta.magnitude;if(length<.01f)return false;
            var bounds=actor.bounds;bounds.Expand(new Vector3(width,width,.1f));
            var direction=delta/length;var normal=new Vector2(-direction.y,direction.x);
            if(!bounds.IntersectRay(new Ray(new Vector3(origin.x,origin.y,bounds.center.z),new Vector3(direction.x,direction.y,0)),out float entry) || entry>length)return false;
            float finish=Mathf.Min(length,entry+bounds.size.magnitude);
            for(float at=Mathf.Max(0,entry-.04f);at<=finish;at+=.055f)for(int side=-1;side<=1;side++)
            {
                Vector2 local=actor.transform.InverseTransformPoint(origin+direction*at+normal*(side*width*.45f));
                if(actor.flipX)local.x=-local.x;if(actor.flipY)local.y=-local.y;
                if(game.art.Opaque(actor.sprite,local))return true;
            }
            return false;
        }
        float DamageTo(SpriteRenderer target,float seconds)
        {
            if(Charge>.97f)return Hits(target,rays[6].GetPosition(3),.52f)?FullDps*seconds:0;
            int hits=0;for(int ray=0;ray<6;ray++)if(Hits(target,ends[ray],.07f))hits++;
            return hits*(12+Charge*58)*seconds;
        }
        void DealDamage(float seconds)
        {
            ApplyingDamage=true;try{DealContactDamage(seconds);}finally{ApplyingDamage=false;}
        }
        void DealContactDamage(float seconds)
        {
            DamageTicks++;
            foreach(var enemy in game.AdditionalTargets)
            {
                float amount=DamageTo(enemy.Renderer,seconds);
                if(amount>0 && enemy.Alive && (!game.WorldBlocked(game.body.position,enemy.Position,.03f))){game.PlayerHit(enemy.Renderer);game.HurtTarget(enemy,amount,game.hero);game.journey.companion.Engage(enemy.Renderer);}
            }
            if(game.magic.InPocket || !game.journey.IsCoast)return;
            float dummy=DamageTo(game.dummy,seconds);if(dummy>0 && game.dummyHealth>0)game.DamageDummy(Mathf.RoundToInt(dummy));
            foreach(var mob in game.woodlice)if(mob.vitals.Alive && !mob.Grabbed){float amount=DamageTo(mob.renderer,seconds);if(amount>0){game.PlayerHit(mob.renderer);mob.Hurt(amount);}}
            foreach(var frog in game.arcane.frogs)if(frog.Alive){float amount=DamageTo(frog.renderer,seconds);if(amount>0){game.PlayerHit(frog.renderer);frog.Hurt(amount);}}
        }
        public int AppendLights(Vector4[] positions,Vector4[] colors,int count)
        {
            bool held=game.survivor.equipment[0]?.kind==ItemKind.LastPrism && game.survivor.equipment[0].Usable;
            if(held && count<positions.Length){positions[count]=new Vector4(Origin.x,Origin.y,2.1f,.46f+.07f*Mathf.Sin(clock*4));colors[count++]=new Vector4(.65f,.78f,1,1);}
            return count;
        }
        void OnApplicationFocus(bool focused){if(!focused){StopChannel();Audio?.Silence();}}
        void OnDestroy(){Shader.SetGlobalInt("_PrismSegmentCount",0);Audio?.Dispose();if(beamMaterial)Destroy(beamMaterial);}
    }
}
