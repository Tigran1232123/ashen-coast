using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    [DefaultExecutionOrder(150)]
    public sealed partial class SpectrumWeapon : MonoBehaviour
    {
        public const float MeleeDamage=180,ContactDamage=300,ContactHalfWidth=3.1f,ContactHalfDepth=1.2f;
        public bool DamageFootprint(Vector2 point){var d=point-Position;return d.x*d.x/(ContactHalfWidth*ContactHalfWidth)+d.y*d.y/(ContactHalfDepth*ContactHalfDepth)<=1;}
        public CoastGame game;
        public SpriteRenderer wall;
        public readonly List<SpriteRenderer> clouds=new List<SpriteRenderer>();
        public bool Active=>wall && endingAge<0;
        public bool Raising=>charge>0;
        public bool Riding=>rideAge>=0;
        public Vector2 Position;
        public float Charge=>charge;
        float charge,age,endingAge=-1,rideAge=-1,loopClock;
        Vector2 target,rideFrom,rideTo;MapRegion rideRegion;
        AudioSource laser;
        readonly Dictionary<SpriteRenderer,float> contacts=new Dictionary<SpriteRenderer,float>();
        public int Summons,ContactHits,Rides;
        public void Initialize(CoastGame owner)
        {game=owner;laser=gameObject.AddComponent<AudioSource>();laser.loop=true;laser.clip=Resources.Load<AudioClip>("Audio/RainbowLoopV37");laser.volume=.23f;}
        void Update()
        {
            if(!game || game.paused || game.testMode)return;
            bool held=game.survivor.equipment[0]?.kind==ItemKind.Spectrum && !game.Dead;
            if(Input.GetMouseButtonDown(1) && Active && !game.ui.IsPointerOver && Hovered(game.CursorWorld))
            {game.map.OpenTravel(-1,true);game.RightClickConsumedFrame=Time.frameCount;charge=0;return;}
            if(held && Input.GetMouseButton(1) && !game.ui.IsPointerOver && !game.MovementLocked && !game.ui.Dragging)
            {AdvanceCharge(Time.deltaTime,game.CursorWorld);}
            else charge=0;
            Tick(Time.deltaTime);
        }
        public void AdvanceCharge(float dt,Vector2 point){charge+=dt;target=point;if(charge>=2){if(Active)Dismiss();else Summon(target);charge=-1000;}}
        public bool Summon(Vector2 point)
        {
            if(wall || !game.survivor.SpendMana(40)){game.ui.Notice("Недостаточно маны");return false;}
            Position=point;age=0;endingAge=-1;
            CreateRainbowVisuals(point);
            contacts.Clear();laser.Play();Summons++;game.sounds.PlayOneShot(Resources.Load<AudioClip>("Audio/RainbowArrivalV37"),.9f);Pose();return true;
        }
        public void Dismiss(){if(!wall || endingAge>=0)return;endingAge=0;laser.Stop();game.sounds.PlayOneShot(Resources.Load<AudioClip>("Audio/RainbowDepartureV37"),.55f);}
        public bool Hovered(Vector2 point)=>Active && age>.7f && (DamageFootprint(point) || Mathf.Abs(point.x-Position.x)<2.6f && point.y>=Position.y-.45f && point.y<wall.bounds.max.y);
        public void Tick(float dt)
        {
            TickLandingSparks(dt);TickDeparturesV41(dt);
            if(Riding)TickRide(dt);
            if(!wall)return;age+=dt;loopClock+=dt;
            if(endingAge<0 && !game.survivor.SpendMana(dt*2))Dismiss();
            if(endingAge>=0)endingAge+=dt;
            Pose();
            if(age>.72f && endingAge<0 && !Riding)
            {
                foreach(var target in game.WorldCreatures.ToArray())
                {
                    if(!target.Alive || target.Renderer==game.hero || target is MageCombatActor || !target.Renderer.gameObject.activeInHierarchy)continue;
                    var d=target.Position-Position;
                    if(!DamageFootprint(target.Position) || game.WorldBlocked(Position,target.Position,.03f) || contacts.TryGetValue(target.Renderer,out var previous) && loopClock-previous<.4f)continue;
                    contacts[target.Renderer]=loopClock;game.spellcraft.Damage(target,ContactDamage,true);ContactHits++;game.sounds.PlayOneShot(Resources.Load<AudioClip>("Audio/RainbowHitV37"),.7f);
                }
            }
            if(endingAge>=2.5f)ClearRainbowVisuals();
        }
        void Pose()=>PoseRainbowV38();
        public bool BeginRide(Vector2 destination,MapRegion? region=null)
        {if(!Active || Riding)return false;rideFrom=game.body.position;rideTo=destination;rideRegion=region??game.map.CurrentRegion;rideAge=0;rideCloudsDetached=false;Rides++;game.body.simulated=false;game.combat.ResetForCheck();game.ui.CloseContainers();game.arcane.CancelHeldSkills();game.body.linearVelocity=Vector2.zero;game.movement=Vector2.zero;return true;}
        void TickRide(float dt)
        {
            float old=rideAge;rideAge+=dt;if(!rideCloudsDetached && rideAge>=.55f){DetachDepartureV41();rideCloudsDetached=true;}
            if(rideAge<.55f)
            {float t=rideAge/.55f;game.hero.transform.position=Vector2.Lerp(rideFrom,Position,t)+Vector2.up*Mathf.Sin(t*Mathf.PI)*1.1f;}
            else if(rideAge<1.6f)
            {game.hero.transform.position=Position+Vector2.up*(1.5f+(rideAge-.55f)*30);}
            else if(rideAge>=2.1f && old<2.1f)
            {game.spellcraft.MoveTo(rideTo,false,rideRegion);Position=rideTo;if(wall){wall.transform.SetParent(game.arcane.WorldRoot,true);ReparentRainbowVisuals();}game.sounds.PlayOneShot(Resources.Load<AudioClip>("Audio/RainbowArrivalV37"),.65f);}
            else if(rideAge>=2.1f && rideAge<3)
            {game.hero.transform.position=rideTo+Vector2.up*((3-rideAge)*24);}
            if(rideAge>=3)
            {game.body.position=rideTo;game.hero.transform.position=rideTo;game.body.simulated=true;rideAge=-1;game.ResetCamera(new Vector3(rideTo.x,rideTo.y+1.1f,-20));}
        }
        void LateUpdate()
        {
            if(Riding){int order=wall?wall.sortingOrder+12:CoastGame.Depth(Position.y)+50;foreach(var renderer in game.hero.GetComponentsInChildren<Renderer>())renderer.sortingOrder=order;game.combat.weapon.enabled=false;}
            PoseRaiseV43();
        }
        void OnGUI()
        {if(!Riding)return;float fade=rideAge<1.1f?0:rideAge<1.6f?(rideAge-1.1f)/.5f:rideAge<2.2f?1:Mathf.Clamp01((2.6f-rideAge)/.4f);GUI.depth=-110;GUI.color=new Color(0,0,0,fade);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;}
        public void PoseGroundLoot(GroundLoot drop,float dt=0)=>PoseGroundLootV38(drop,dt);
    }
}
