using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class VillageActorV46:ICombatTarget
    {
        public readonly AmazonSettlementV46 village;
        public readonly SpriteRenderer Renderer,weaponSprite,shadow;public PixelGait gaitV53;
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
            gaitV53=PixelGait.ForHumanoidV53(bank,Renderer,.43f);Pose(false);
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
                attackAge+=dt;AttackFootworkV54(dt);float p=Mathf.Clamp01(attackAge/duration);
                if(!delivered && p>=(Weapon?.kind==ItemKind.Bow?.62f:.42f)){delivered=true;Deliver();}
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
            else if(Hired)destination=game.FormationPointV54(Renderer);
            else if(Slaver)destination=home;else if(!WeaponMerchantV48)destination=PatrolV48(dt);
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
        {var d=Weapon.Definition;attackAge=0;delivered=false;duration=Weapon.kind==ItemKind.Bow?.85f:Mathf.Max(.36f,d.attackSeconds/(IsSlave?1:1.5f));ready=Weapon.kind==ItemKind.Bow?2:d.cooldown/(IsSlave?1:1.5f);AttackCount++;village.game.WorldSwishV53(Renderer);}
        void Deliver()
        {
            if(SparringV48 && victim==SparringPartnerV48){if(Weapon?.kind==ItemKind.Bow)village.ShootPracticeV48(this,aim);else village.TrainingImpactV48(SparringPartnerV48.Position+Vector2.up*.9f);return;}
            if(victim==null || !victim.Alive || Weapon==null)return;
            if(Weapon.kind==ItemKind.Bow){village.Shoot(this,aim);return;}
            if(Vector2.Distance(victim.Position,Position)>Weapon.Definition.reach+.3f || village.game.WorldBlocked(Position,victim.Position,.06f))return;
            if(Weapon.Definition.arc>0 && Weapon.Definition.arc<360 && Vector2.Angle(aim,victim.Position-Position)>Weapon.Definition.arc*.5f)return;
            if(!village.game.CanReachV54(victim,DamageDeliveryV54.Melee))return;if(Weapon.kind==ItemKind.GoldenSpear)village.game.v44.Apply(victim);village.game.HurtTarget(victim,AmazonSettlementV46.WeaponDamage(Weapon,victim),Renderer,DamageDeliveryV54.Melee);
        }
        void Pose(bool walking)
        {
            PoseV54(walking);
        }
    }

}
