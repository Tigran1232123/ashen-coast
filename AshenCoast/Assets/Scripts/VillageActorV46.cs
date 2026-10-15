using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class VillageActorV46:ICombatTarget
    {
        public readonly AmazonSettlementV46 village;
        public readonly SpriteRenderer Renderer,weaponSprite,shadow;
        SpriteRenderer ICombatTarget.Renderer=>Renderer;
        public readonly GridInventory stock=new GridInventory(10,5,"Охотница"),weaponSlot=new GridInventory(2,4,"Оружие"){singleItem=true,craftingEnabled=false,acceptsItem=i=>i.Definition.Melee};
        public SpriteRenderer cage,Aggressor;
        public readonly bool IsSlave,Hunter,Slaver;
        public bool Hired,Captive,hostile,HoldPosition;
        public bool Hostile=>hostile && !Hired;
        public float health;public float MaxHealth=>IsSlave?100:150;
        public bool Alive=>health>0;
        public string Name=>IsSlave?"Пленник "+(index+1):WeaponMerchantV48?"Амазонка-оружейница":Hunter || HuntingV48?"Амазонка-охотница":Slaver?"Амазонка-работорговец":"Амазонка";
        public Vector2 Position=>(Vector2)Renderer.transform.position;
        public Item Weapon=>IsSlave?weaponSlot.items.FirstOrDefault():innate;
        public int AttackCount{get;private set;}
        public bool Attacking=>attackAge>=0;
        readonly int index;readonly Item innate;readonly Vector2 home;
        float clock,step,ready,attackAge=-1,duration;Vector2 aim=Vector2.right;
        ICombatTarget victim;bool delivered;readonly Color tint;
        public VillageActorV46(AmazonSettlementV46 owner,Vector2 at,int number,bool slave,bool hunter,bool slaver,ItemKind weapon)
        {
            village=owner;index=number;home=at;IsSlave=slave;Hunter=hunter;Slaver=slaver;Captive=slave;health=MaxHealth;innate=slave?null:new Item(weapon);
            var bank=village.game.art;var sprite=slave?village.game.journey.art.driverRun[number%4,0]:village.art.women[number%4,0];
            Renderer=bank.Renderer(Name,sprite,at,CoastGame.Depth(at.y));Renderer.transform.SetParent(village.root.transform,true);
            tint=slave?Color.Lerp(Color.white,new Color(.73f,.79f,.65f),number*.075f):Color.Lerp(Color.white,new Color(.85f,.78f,.68f),(number%3)*.045f);Renderer.color=tint;
            weaponSprite=bank.Renderer("Amazon held weapon",bank.icons[54],at,Renderer.sortingOrder+2);weaponSprite.transform.SetParent(Renderer.transform,true);weaponSprite.enabled=!slave;
            shadow=village.game.Shadow("Amazon contact shadow",at,new Vector2(.68f,.23f),.25f);shadow.transform.SetParent(Renderer.transform,true);
            if(hunter){for(int i=0;i<6;i++)stock.Add(new Item(ItemKind.RawMeat));for(int i=0;i<10;i++)stock.Add(new Item(ItemKind.BeetleMeat));}
            Pose(false);
        }
        public void Warp(Vector2 at){Renderer.transform.position=at;victim=null;attackAge=-1;step=0;ResetRouteV48();Pose(false);}
        public float Hurt(float amount)=>HurtFrom(amount,village.game.hero);
        public float HurtFrom(float amount,SpriteRenderer source)
        {
            if(!Alive || Hired && (source==village.game.hero || village.Allies.Any(a=>a.Renderer==source)) || Weapon?.kind==ItemKind.Zen && Attacking)return 0;
            var game=village.game;amount*=game.spellcraft.DamageFactor(Renderer);if(!IsSlave)amount*=.8f;
            float taken=Mathf.Min(health,Mathf.Max(0,amount));health-=taken;game.ShowDamage(taken,Position+Vector2.up*1.75f,Renderer);Aggressor=source;village.Alarm(this,source);
            if(!Alive){Renderer.enabled=weaponSprite.enabled=shadow.enabled=false;if(Weapon!=null && !IsSlave)game.ScatterLoot(new Item(Weapon.kind),Position,Position);}
            return taken;
        }
        public void Tick(float dt)
        {
            if(village.game.snails!=null && (village.game.snails.Resting(Renderer) || village.game.snails.Returning(Renderer)))return;if(!Alive || !Renderer.gameObject.activeInHierarchy || Captive)return;
            var game=village.game;clock+=dt;ready=Mathf.Max(0,ready-dt);
            if(game.spellcraft.ImmobilizedV44(Renderer)){attackAge=-1;Pose(false);return;}
            if(Hired && HoldPosition){if(Vector2.Distance(Position,game.body.position)>12)return;HoldPosition=false;}
            if(attackAge>=0)
            {
                attackAge+=dt;float p=Mathf.Clamp01(attackAge/duration);
                if(!delivered && p>=.42f){delivered=true;Deliver();}
                if(p>=1)attackAge=-1;Pose(false);return;
            }
            victim=FindEnemy();Vector2 destination=Position;
            if(victim==null && SparringV48){TickSparringV48(dt);return;}
            if(victim!=null && Weapon!=null)
            {
                var delta=victim.Position-Position;aim=delta.normalized;float reach=Weapon.kind==ItemKind.Bow?8:Weapon.Definition.reach*.90f;
                if(delta.magnitude<=reach && !game.WorldBlocked(Position,victim.Position,.06f) && ready<=0)BeginAttack();
                else if(delta.magnitude>reach*.83f)destination=victim.Position-aim*reach*.65f;
            }
            else if(Hired){int formation=village.Allies.ToList().IndexOf(this);destination=game.body.position+new Vector2(-1.3f-(formation%3)*.65f,-.7f-(formation/3)*.75f);}
            else if(!Slaver && !Hunter && !WeaponMerchantV48)destination=PatrolV48(dt);
            var movement=destination-Position;bool walking=movement.magnitude>.30f;
            if(walking)
            {
                float speed=Hired?CoastGame.WalkSpeed+.35f:victim!=null?3.7f:HuntingV48?2.6f:2.15f;
                walking=MoveV48(destination,speed,dt);
                if(Hired && movement.magnitude>19 && !game.MovementLocked)Warp(game.body.position+Vector2.left*(1.6f+index*.08f));
            }
            Pose(walking);
        }
        ICombatTarget FindEnemy()
        {
            var game=village.game;
            if(Weapon==null)return null;
            if(Hostile)
            {
                var fighters=new List<ICombatTarget>{new HeroCombatActor(game)};fighters.AddRange(village.Allies);if(game.journey.IsCoast)fighters.AddRange(game.snails.houses.Where(h=>!h.Hidden));if(game.journey.companion.Hired && game.journey.companion.Alive)fighters.Add(new MageCombatActor(game.journey.companion));
                return fighters.Where(f=>f.Alive && Vector2.Distance(f.Position,Position)<16).OrderBy(f=>Vector2.SqrMagnitude(f.Position-Position)).FirstOrDefault();
            }
            if(Hired || !IsSlave && !Slaver && !Hunter && !WeaponMerchantV48)return game.WorldCreatures.Where(f=>f.Alive && f.Renderer!=Renderer && (village.Enemy(f) && (Hired || !(f is VillageActorV46)) || HuntingV48 && f is WoodlouseActor && Vector2.Distance(f.Position,home)<24) && Vector2.Distance(f.Position,Position)<(HuntingV48?16:9)).OrderBy(f=>Vector2.SqrMagnitude(f.Position-Position)).FirstOrDefault();
            return null;
        }
        Vector2 Steer(Vector2 desired,float length)
        {
            var game=village.game;foreach(float angle in new[]{0f,35,-35,70,-70,100,-100})
            {
                var move=(Vector2)(Quaternion.Euler(0,0,angle)*desired)*length;var next=Position+move;
                if(game.WorldBlocked(Position,next,.24f))continue;
                bool solid=Physics2D.OverlapCircleAll(next+Vector2.up*.13f,.22f).Any(c=>!c.isTrigger && c.gameObject!=game.hero.gameObject);
                if(!solid)return move;
            }
            return Vector2.zero;
        }
        void BeginAttack()
        {var d=Weapon.Definition;attackAge=0;delivered=false;duration=d.attackSeconds/(IsSlave?1:1.5f);ready=d.cooldown/(IsSlave?1:1.5f);AttackCount++;village.game.PlaySwish();}
        void Deliver()
        {
            if(SparringV48 && victim==SparringPartnerV48){village.TrainingImpactV48(SparringPartnerV48.Position+Vector2.up*.9f);if(Weapon?.kind==ItemKind.Bow)village.ShootPracticeV48(this,aim);return;}
            if(victim==null || !victim.Alive || Weapon==null)return;
            if(Weapon.kind==ItemKind.Bow){village.Shoot(this,aim);return;}
            if(Vector2.Distance(victim.Position,Position)>Weapon.Definition.reach+.3f || village.game.WorldBlocked(Position,victim.Position,.06f))return;
            if(Weapon.Definition.arc>0 && Weapon.Definition.arc<360 && Vector2.Angle(aim,victim.Position-Position)>Weapon.Definition.arc*.5f)return;
            village.game.HurtTarget(victim,AmazonSettlementV46.WeaponDamage(Weapon,victim),Renderer);if(Weapon.kind==ItemKind.GoldenSpear)village.game.v44.Apply(victim);
        }
        void Pose(bool walking)
        {
            var game=village.game;int frame=walking?(int)(step/1.4f*6)%6:0;
            Renderer.sprite=IsSlave?game.journey.art.driverRun[index%4,walking?(int)(step/1.4f*8)%8:0]:village.art.women[index%4,frame];if(Mathf.Abs(aim.x)>.25f)Renderer.flipX=aim.x<0;Renderer.sortingOrder=CoastGame.Depth(Position.y);game.art.PixelStyle(Renderer);Renderer.color=tint;
            shadow.transform.position=Position;shadow.sortingOrder=-2000;
            var weapon=Weapon;weaponSprite.enabled=weapon!=null && Attacking;if(!Attacking)Renderer.transform.localScale=Vector3.one;if(!weaponSprite.enabled)return;
            weaponSprite.sprite=game.art.icons[weapon.Definition.icon];bool vertical=Combat.VerticalV44(weapon.kind) || weapon.kind==ItemKind.Bow;
            float length=weapon.kind==ItemKind.Bow?1.5f:Combat.WeaponLengthV44(weapon.kind);weaponSprite.transform.localScale=Vector3.one*(length/((vertical?weaponSprite.sprite.rect.height:weaponSprite.sprite.rect.width)/weaponSprite.sprite.pixelsPerUnit));
            float degrees=Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg;
            var hand=Position+new Vector2(aim.x<0?-.18f:.18f,.91f);
            if(attackAge>=0)
            {float progress=Mathf.SmoothStep(0,1,Mathf.Clamp01(attackAge/duration));if(Combat.ThrustV44(weapon.kind))hand+=aim*Mathf.Sin(progress*Mathf.PI)*.6f;else degrees+=weapon.Definition.arc*(progress-.5f);Renderer.transform.localScale=new Vector3(1+Mathf.Sin(progress*Mathf.PI)*.035f,1-Mathf.Sin(progress*Mathf.PI)*.025f,1);}
            else {degrees=weapon.kind==ItemKind.Bow?90:aim.x<0?120:65;Renderer.transform.localScale=Vector3.one;}
            weaponSprite.transform.position=hand;weaponSprite.transform.rotation=Quaternion.Euler(0,0,degrees-(vertical?90:0));weaponSprite.sortingOrder=Renderer.sortingOrder+2;game.art.PixelStyle(weaponSprite);
        }
    }
    public sealed class VillageArrowV46
    {public LineRenderer line;public Vector2 point,velocity;public SpriteRenderer source;public bool player,practice;public float age;}
    public sealed partial class AmazonSettlementV46
    {
        public readonly List<VillageArrowV46> arrows=new List<VillageArrowV46>();
        public bool ShootPlayer(Vector2 direction)
        {
            if(Time.time<game.combat.readyAt)return false;var bow=game.survivor.equipment[0];if(bow?.kind!=ItemKind.Bow || !bow.Usable)return false;
            bow.Wear(.3f);game.combat.readyAt=Time.time+bow.Definition.cooldown;SpawnArrow(game.body.position+Vector2.up*.9f,direction,game.hero,true);game.PlaySwish();return true;
        }
        public void Shoot(VillageActorV46 actor,Vector2 direction)=>SpawnArrow(actor.Position+Vector2.up*.9f,direction,actor.Renderer,actor.Hired);
        public void ShootPracticeV48(VillageActorV46 actor,Vector2 direction)=>SpawnArrow(actor.Position+Vector2.up*.9f,direction,actor.Renderer,false,true);
        public void ShootBanditV48(PlainBanditV48 actor,Vector2 direction)=>SpawnArrow(actor.Position+Vector2.up*.9f,direction,actor.Renderer,false);
        void SpawnArrow(Vector2 from,Vector2 direction,SpriteRenderer source,bool player,bool practice=false)
        {
            var line=new GameObject("Amazon flying arrow").AddComponent<LineRenderer>();line.transform.SetParent(source.transform.parent,true);line.sharedMaterial=game.art.lit;line.positionCount=2;line.startWidth=.045f;line.endWidth=.02f;line.startColor=new Color(.51f,.35f,.16f);line.endColor=new Color(.84f,.79f,.61f);line.sortingOrder=CoastGame.Depth(from.y)+5;
            var a=new VillageArrowV46{line=line,point=from,velocity=direction.normalized*15,source=source,player=player,practice=practice};line.SetPosition(0,from);line.SetPosition(1,from-direction.normalized*.35f);arrows.Add(a);
        }
        void TickArrows(float dt)
        {
            for(int i=arrows.Count-1;i>=0;i--)
            {
                var a=arrows[i];a.age+=dt;var next=a.point+a.velocity*dt;bool remove=a.age>1.2f || game.WorldBlocked(a.point-Vector2.up*.9f,next-Vector2.up*.9f,.03f);
                if(!remove)foreach(var target in game.WorldCreatures.ToArray())
                {
                    if(!target.Alive || target.Renderer==a.source || target is PracticeTargetV46)continue;
                    bool hunts=a.source && people.Any(p=>p.Renderer==a.source && !p.Hostile && !p.Hired);
                    if(!a.practice && hunts && !(target is PlainBanditV48 || target is WoodlouseActor || Enemy(target) && !(target is VillageActorV46)))continue;
                    if(!a.practice && !hunts && a.player && (target is HeroCombatActor || target is MageCombatActor || target is SnailHouseV50 || target is VillageActorV46 ally && ally.Hired))continue;
                    if(!a.practice && !hunts && !a.player && !(target is HeroCombatActor || target is MageCombatActor || target is SnailHouseV50 || target is VillageActorV46 friendly && (friendly.Hired || a.source && plains.bandits.Any(b=>b.Renderer==a.source))))continue;
                    var centre=target is SnailHouseV50?target.Position+Vector2.up*.9f:(Vector2)target.Renderer.bounds.center;var segment=next-a.point;float t=segment.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(centre-a.point,segment)/segment.sqrMagnitude):0;
                    if(Vector2.Distance(centre,a.point+segment*t)>.36f)continue;
                    if(a.practice)TrainingImpactV48(centre);else game.HurtTarget(target,20,a.source);remove=true;break;
                }
                if(remove){Destroy(a.line.gameObject);arrows.RemoveAt(i);}else{a.point=next;a.line.SetPosition(0,next);a.line.SetPosition(1,next-a.velocity.normalized*.35f);}
            }
        }
    }
}
