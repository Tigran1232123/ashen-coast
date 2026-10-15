using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed class VillageArrowV46
    {
        public LineRenderer line;public SpriteRenderer shadow,source;public Vector2 point,ground,velocity;
        public bool player,practice,stuck;public VillageActorV46 practiceTargetV53;public float age,height,verticalSpeed;
    }
    public sealed partial class AmazonSettlementV46
    {
        public readonly List<VillageArrowV46> arrows=new List<VillageArrowV46>();
        public bool ShootPlayer(Vector2 direction)
        {
            if(Time.time<game.combat.readyAt)return false;var bow=game.survivor.equipment[0];if(bow?.kind!=ItemKind.Bow || !bow.Usable)return false;
            bow.Wear(.3f);game.combat.readyAt=Time.time+bow.Definition.cooldown;
            SpawnArrow(game.body.position,direction,game.hero,true,Mathf.Clamp(direction.magnitude,2.5f,11.5f));game.PlaySwish();return true;
        }
        public void Shoot(VillageActorV46 actor,Vector2 direction)=>SpawnArrow(actor.Position,direction,actor.Renderer,actor.Hired,actor.TargetDistanceV54);
        public void ShootPracticeV48(VillageActorV46 actor,Vector2 direction)=>SpawnArrow(actor.Position,direction,actor.Renderer,false,actor.TargetDistanceV54,true,actor.SparringPartnerV48);
        public void ShootBanditV48(PlainBanditV48 actor,Vector2 direction)=>SpawnArrow(actor.Position,direction,actor.Renderer,false,actor.TargetDistanceV54);
        public void ShootFortV54(FortBandit actor,Vector2 direction)=>SpawnArrow(actor.Position,direction,actor.Renderer,false,Vector2.Distance(actor.Position,game.body.position));
        public VillageArrowV46 SpawnArrow(Vector2 ground,Vector2 direction,SpriteRenderer source,bool player,float range=7,bool practice=false,VillageActorV46 partner=null)
        {
            while(arrows.Count>=64){DestroyArrowV54(arrows[0]);arrows.RemoveAt(0);}
            var line=new GameObject("Fired arrow / ballistic shaft").AddComponent<LineRenderer>();line.transform.SetParent(source.transform.parent,true);line.sharedMaterial=game.art.lit;line.positionCount=3;line.startWidth=.038f;line.endWidth=.016f;line.startColor=new Color(.38f,.26f,.14f);line.endColor=new Color(.89f,.84f,.69f);
            float height=source==game.hero?.90f*game.CharacterScaleV53:.90f;float time=Mathf.Clamp(range,2.5f,11.5f)/13.5f;
            var a=new VillageArrowV46{line=line,ground=ground,point=ground+Vector2.up*height,height=height,verticalSpeed=(.72f-height+4.9f*time*time)/time,velocity=direction.normalized*13.5f,source=source,player=player,practice=practice,practiceTargetV53=partner};
            a.shadow=game.Shadow("Arrow ground shadow",ground,new Vector2(.42f,.08f),.17f);a.shadow.transform.SetParent(line.transform.parent,true);arrows.Add(a);PoseArrowV54(a);return a;
        }
        void DestroyArrowV54(VillageArrowV46 a){if(a.line)Destroy(a.line.gameObject);if(a.shadow)Destroy(a.shadow.gameObject);}
        void PoseArrowV54(VillageArrowV46 a)
        {
            var tangent=a.velocity+Vector2.up*a.verticalSpeed;var direction=tangent.sqrMagnitude>.01f?tangent.normalized:a.velocity.normalized;
            a.line.SetPosition(0,a.point-direction*.58f);a.line.SetPosition(1,a.point-direction*.13f);a.line.SetPosition(2,a.point);a.line.sortingOrder=CoastGame.Depth(a.ground.y)+4;
            a.shadow.transform.position=a.ground;a.shadow.color=new Color(.04f,.05f,.04f,.20f/(1+a.height));
        }
        void TickArrows(float dt)
        {
            var creatures=game.WorldCreatures.ToArray();
            for(int i=arrows.Count-1;i>=0;i--)
            {
                var a=arrows[i];a.age+=dt;
                if(!a.line || !a.line.gameObject.activeInHierarchy || a.age>(a.practice?3:18)) {DestroyArrowV54(a);arrows.RemoveAt(i);continue;}
                if(a.stuck)continue;
                if(!game.SimulateWorldV49 && (!a.player || a.source!=game.coop.LocalHero))continue;
                int pieces=Mathf.Max(1,Mathf.CeilToInt(dt/.025f));float step=dt/pieces;
                for(int sub=0;sub<pieces && !a.stuck;sub++)
                {
                    var old=a.point;var nextGround=a.ground+a.velocity*step;a.verticalSpeed-=9.8f*step;float nextHeight=a.height+a.verticalSpeed*step;var next=nextGround+Vector2.up*Mathf.Max(0,nextHeight);
                    bool wall=game.WorldBlocked(a.ground,nextGround,.025f),hit=false;
                    if(!wall && a.practice && a.practiceTargetV53!=null)
                    {var centre=(Vector2)a.practiceTargetV53.Renderer.bounds.center;if(FortWorld.SegmentDistance(centre,old,next)<.46f){TrainingImpactV48(centre);hit=true;}}
                    if(!wall && !hit && !a.practice)foreach(var target in creatures)
                    {
                        if(!target.Alive || target.Renderer==a.source || target is PracticeTargetV46)continue;
                        bool hunter=people.Any(p=>p.Renderer==a.source && !p.Hostile && !p.Hired);
                        bool bandit=plains.bandits.Any(p=>p.Renderer==a.source) || game.fort.bandits.Any(p=>p.Renderer==a.source);
                        if(hunter && !(target is PlainBanditV48 || target is WoodlouseActor || Enemy(target) && !(target is VillageActorV46)))continue;
                        if(!hunter && a.player && (target is HeroCombatActor || target is RemotePlayerV49 || target is MageCombatActor || target is SnailHouseV50 || target is VillageActorV46 ally && ally.Hired))continue;
                        if(!hunter && !a.player && !(target is HeroCombatActor || target is RemotePlayerV49 || target is MageCombatActor || target is SnailHouseV50 || target is VillageActorV46 friendly && (friendly.Hired || bandit)))continue;
                        var centre=target is SnailHouseV50?target.Position+Vector2.up*.9f:(Vector2)target.Renderer.bounds.center;
                        float radius=target is SnailHouseV50?1.5f:Mathf.Clamp(Mathf.Min(target.Renderer.bounds.size.x,target.Renderer.bounds.size.y)*.44f,.20f,.7f);
                        if(FortWorld.SegmentDistance(centre,old,next)>radius)continue;
                        game.HurtTarget(target,36,a.source);hit=true;break;
                    }
                    if(wall || hit){a.stuck=true;a.age=15.5f;a.point=wall?old:next;a.ground=wall?a.ground:nextGround;a.height=Mathf.Max(0,nextHeight);}
                    else {a.ground=nextGround;a.height=Mathf.Max(0,nextHeight);a.point=next;if(nextHeight<=0){a.stuck=true;a.verticalSpeed=0;a.point=a.ground+Vector2.up*.035f;}}
                    PoseArrowV54(a);
                }
            }
        }
    }
    public sealed partial class VillageActorV46 {public float TargetDistanceV54=>victim!=null?Vector2.Distance(Position,victim.Position):7;}
    public sealed partial class PlainBanditV48 {public float TargetDistanceV54=>victim!=null?Vector2.Distance(Position,victim.Position):7;}
}
