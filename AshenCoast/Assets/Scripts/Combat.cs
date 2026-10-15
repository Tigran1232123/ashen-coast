using System.Collections.Generic;
using UnityEngine;

namespace AshenCoast
{
    public sealed partial class Combat : MonoBehaviour
    {
        CoastGame game;
        public SpriteRenderer weapon;
        public bool Attacking { get; private set; }
        public bool hit;
        public int hits;
        public float elapsed, readyAt;
        public Item active;
        public bool manual;
        Vector2 aim=Vector2.right;
        float previousPhase;
        LineRenderer trail;
        readonly Queue<Vector3> tips=new Queue<Vector3>();
        readonly HashSet<SpriteRenderer> touched=new HashSet<SpriteRenderer>();
        public void Initialize(CoastGame game)
        {
            this.game=game;
            weapon=game.art.Renderer("Held weapon / animated contact",game.art.icons[4],Vector2.zero,0); weapon.enabled=false;
            var t=new GameObject("Weapon motion trail"); trail=t.AddComponent<LineRenderer>();
            trail.sharedMaterial=new Material(Shader.Find("AshenCoast/WorldSprite")); trail.startWidth=.06f; trail.endWidth=.015f;
            trail.startColor=new Color(.9f,.80f,.52f,.12f); trail.endColor=new Color(.9f,.80f,.52f,0); trail.useWorldSpace=true;
        }
        public bool Begin(Vector2 direction)
        {
            var item=game.survivor.equipment[(int)GearSlot.Hand];
            if(game.MovementLocked || game.Meditating || item==null || !item.Usable || !item.Definition.Weapon || Attacking || game.journey.Cutscene || Time.time<readyAt) return false;
            if(item.kind==ItemKind.Bow)return game.amazons.ShootPlayer(direction);
            if(item.kind==ItemKind.YoYo)return game.arcane.SetYoYoTarget((Vector2)game.hero.transform.position+new Vector2(0,.72f)+direction.normalized*5,true);
            if(item.kind==ItemKind.Lamp)return game.arcane.FireLampBurst(direction);
            if(item.kind==ItemKind.LastPrism)return game.prism.BeginChannel(direction);
            float interval=item.kind==ItemKind.Zen?ZenIntervalV44(game.survivor.hunger):item.Definition.cooldown;
            if(item.kind==ItemKind.Zen && !PayZenV44())return false;
            durationV44=item.Definition.attackSeconds*(item.kind==ItemKind.Zen?interval/item.Definition.cooldown:1);dashRemainingV44=item.kind==ItemKind.Zen?2:0;dashClockV44=0;
            active=item; aim=direction.sqrMagnitude>.01f?direction.normalized:Vector2.right;
            item.Wear(.30f);
            game.facing=CoastGame.Facing(aim); Attacking=true; hit=false; elapsed=0;attackStartedAtV44=Time.time; previousPhase=0;
            readyAt=Time.time+interval; tips.Clear();touched.Clear();
            SetupWeapon(item); game.PlaySwish(); Pose(0); return true;
        }
        void SetupWeapon(Item item)
        {
            weapon.enabled=true; weapon.sprite=game.art.icons[item.Definition.icon];weapon.color=Color.white;
            float length=item.kind==ItemKind.LastPrism?.46f*ArtBank.PrismVisualScale:item.kind==ItemKind.Orb?.32f:item.kind==ItemKind.Lamp?.58f:WeaponLengthV44(item.kind);
            float scale=length/((VerticalV44(item.kind)?weapon.sprite.rect.height:weapon.sprite.rect.width)/weapon.sprite.pixelsPerUnit);
            weapon.transform.localScale=new Vector3(scale,scale*(item.kind==ItemKind.Spear?.83f:1),1);weapon.sharedMaterial=game.art.lit;
            game.art.PixelStyle(weapon);
            var properties=new MaterialPropertyBlock();weapon.GetPropertyBlock(properties);properties.SetFloat("_SpectrumGround",0);properties.SetFloat("_SpectrumPlanted",0);properties.SetFloat("_Spectrum",item.kind==ItemKind.Spectrum?1:0);properties.SetFloat("_Orb",item.kind==ItemKind.Orb?1:0);weapon.SetPropertyBlock(properties);
            properties.SetFloat("_Prism",item.kind==ItemKind.LastPrism?1:0);weapon.SetPropertyBlock(properties);
            properties.SetFloat("_Emission",item.kind==ItemKind.LastPrism?.55f:item.kind==ItemKind.Orb?.36f:item.kind==ItemKind.Spectrum?.54f:0);weapon.SetPropertyBlock(properties);
        }
        void Update()
        {
            if(!game || game.paused || manual) return;
            if(game.Meditating){weapon.enabled=false;return;}
            if(Attacking)
            {
                elapsed=Mathf.Max(0,Time.time-attackStartedAtV44); Advance(Mathf.Clamp01(elapsed/durationV44));
                if(elapsed>=durationV44) {Attacking=false;weapon.enabled=false;tips.Clear();trail.positionCount=0;game.hero.transform.localScale=Vector3.one;}
            }
            else
            {
                var item=game.survivor.equipment[(int)GearSlot.Hand];
                weapon.enabled=item!=null && (item.kind==ItemKind.Orb || item.kind==ItemKind.Lamp || item.kind==ItemKind.LastPrism || item.kind==ItemKind.Spectrum);
                if(weapon.enabled)
                {
                    SetupWeapon(item);
                    var direction=game.CursorWorld-((Vector2)game.hero.transform.position+new Vector2(0,.72f));
                    if(direction.sqrMagnitude>.05f) aim=direction.normalized;
                    PoseIdle(item);
                }
            }
        }
        void LateUpdate()
        {
            if(!game || game.paused || manual || Attacking || !weapon.enabled)return;
            var item=game.survivor.equipment[(int)GearSlot.Hand];
            if(item!=null && (item.kind==ItemKind.Orb || item.kind==ItemKind.Lamp || item.kind==ItemKind.LastPrism || item.kind==ItemKind.Spectrum))PoseIdle(item);
        }
        public void Advance(float phase)
        {
            // Sweep the SAME textured pose in small time steps, preventing tunnelling during fast swings.
            int steps=Mathf.Max(1,Mathf.CeilToInt((phase-previousPhase)*180));
            for(int i=1;i<=steps;i++)
            {
                float t=Mathf.Lerp(previousPhase,phase,(float)i/steps); Pose(t);
                if((!game.magic.InPocket || game.dragons.HasActiveBoss) && t>=(ThrustV44(active.kind)?.18f:.10f) && t<=(ThrustV44(active.kind)?.70f:.88f) && !(hit && ThrustV44(active.kind)))
                {
                    if(!game.magic.InPocket && game.journey.IsCoast){
                    if(game.dummyHealth>0 && !touched.Contains(game.dummy) && game.art.SpriteContact(weapon,game.dummy))
                    {touched.Add(game.dummy);hit=true;hits++;game.DamageDummy(active.Definition.damage);}
                    foreach(var mob in game.woodlice)
                    {
                        if(hit && ThrustV44(active.kind))break;
                        if(mob.vitals.Alive && !touched.Contains(mob.renderer) && game.art.SpriteContact(weapon,mob.renderer,0))
                        {touched.Add(mob.renderer);hit=true;hits++;game.PlayerHit(mob.renderer);StrikeV44(new WoodlouseActor(mob));game.journey.companion.Engage(mob.renderer);}
                    }
                    foreach(var frog in game.arcane.frogs)
                    {
                        if(hit && ThrustV44(active.kind))break;
                        if(frog.Alive && !touched.Contains(frog.renderer) && game.art.SpriteContact(weapon,frog.renderer,0))
                        {touched.Add(frog.renderer);hit=true;hits++;game.PlayerHit(frog.renderer);StrikeV44(new FrogActor(frog));game.journey.companion.Engage(frog.renderer);}
                    }}
                    if(active.kind==ItemKind.Axe && game.amazons!=null)foreach(var tree in game.amazons.trees)if(tree.Standing && !touched.Contains(tree.sprite) && game.art.SpriteContact(weapon,tree.sprite,0) && Vector2.Distance(game.body.position,tree.position)<3){touched.Add(tree.sprite);tree.Chop(game.body.position);hit=true;}
                    foreach(var target in game.AdditionalTargets)
                    {if(hit && ThrustV44(active.kind))break;if(target.Alive && (!game.WorldBlocked(game.body.position,target.Position,.03f)) && !touched.Contains(target.Renderer) && game.art.SpriteContact(weapon,target.Renderer,0)){touched.Add(target.Renderer);hit=true;hits++;game.PlayerHit(target.Renderer);StrikeV44(target);game.journey.companion.Engage(target.Renderer);}}
                }
            }
            previousPhase=phase; Pose(phase);
            if(phase>.12f && phase<.82f)
            {
                Vector3 tip=weapon.transform.TransformPoint(VerticalV44(active.kind)?new Vector3(0,(weapon.sprite.rect.height-weapon.sprite.pivot.y)/weapon.sprite.pixelsPerUnit,0):new Vector3((weapon.sprite.rect.width-weapon.sprite.pivot.x)/weapon.sprite.pixelsPerUnit,0,0));
                tips.Enqueue(tip); while(tips.Count>6) tips.Dequeue();
                trail.sortingOrder=weapon.sortingOrder-1; trail.positionCount=tips.Count; trail.SetPositions(tips.ToArray());
            }
            game.hero.transform.localScale=ThrustV44(active.kind)?Vector3.one:new Vector3(1+Mathf.Sin(phase*Mathf.PI)*.045f,1-Mathf.Sin(phase*Mathf.PI)*.025f,1);
        }
        void Pose(float phase)
        {
            var d=active.Definition; float degrees=Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg;
            Vector2 origin=(Vector2)game.hero.transform.position+new Vector2(.10f,.72f)*game.CharacterScaleV53;
            float progress=Mathf.SmoothStep(0,1,Mathf.Clamp01((phase-.10f)/.70f));
            if(ThrustV44(active.kind))
            {
                float thrust=phase<.18f?Mathf.Lerp(-.08f,-.22f,Mathf.SmoothStep(0,1,phase/.18f)):phase<.42f?Mathf.Lerp(-.22f,1.12f,Mathf.SmoothStep(0,1,(phase-.18f)/.24f)):Mathf.Lerp(1.12f,-.05f,Mathf.SmoothStep(0,1,(phase-.42f)/.48f));
                origin+=aim*thrust;
            }
            else if(active.kind==ItemKind.Scythe || active.kind==ItemKind.Zen)degrees+=180-progress*360;
            else degrees+=-d.arc*.5f+progress*d.arc;
            origin+=new Vector2(Mathf.Cos(degrees*Mathf.Deg2Rad),Mathf.Sin(degrees*Mathf.Deg2Rad))*.06f;
            weapon.transform.position=origin; weapon.transform.rotation=Quaternion.Euler(0,0,degrees-(VerticalV44(active.kind)?90:0));
            weapon.sortingOrder=game.hero.sortingOrder+(Mathf.Sin(degrees*Mathf.Deg2Rad)>.45f?-1:2);
        }
        void PoseIdle(Item item)
        {
            float degrees=item.kind==ItemKind.Orb || item.kind==ItemKind.Lamp || item.kind==ItemKind.LastPrism?0:90;
            if(item.kind==ItemKind.LastPrism && game.prism!=null && game.prism.Channeling)degrees=game.prism.Angle-90;
            bool right=game.facing>=4;var hand=game.art.HandAnchor(game.hero.sprite,right^game.hero.flipX);
            if(game.hero.flipX)hand.x=-hand.x;
            hand+=new Vector2(right?.025f:-.025f,.12f*game.CharacterScaleV53);
            if(item.kind==ItemKind.Lamp)hand=new Vector2(game.facing>=1 && game.facing<=3?-.22f:game.facing>=5?.22f:0,.94f*game.CharacterScaleV53);
            weapon.transform.position=game.hero.transform.position+(Vector3)hand;
            if(game.walking)weapon.transform.position+=new Vector3(0,Mathf.Sin(game.walkTime*Mathf.PI*2)*.014f,0);
            weapon.transform.rotation=Quaternion.Euler(0,0,degrees);
            weapon.sortingOrder=game.hero.sortingOrder+(game.facing>=3 && game.facing<=5?-1:2);
            weapon.color=item.kind==ItemKind.Orb?new Color(.88f+.12f*Mathf.Sin(Time.time*4),.94f,1):Color.white;
        }
        public void PoseHeldPrism()
        {
            var item=game.survivor.equipment[(int)GearSlot.Hand];
            if(Attacking || game.Meditating || item?.kind!=ItemKind.LastPrism)return;
            SetupWeapon(item);weapon.flipX=false;weapon.flipY=false;PoseIdle(item);
        }
        public void ResetForCheck()
        { Attacking=false;dashRemainingV44=0;weapon.enabled=false;readyAt=0;hit=false;elapsed=0;previousPhase=0;tips.Clear();trail.positionCount=0;game.hero.transform.localScale=Vector3.one; }
    }
}
