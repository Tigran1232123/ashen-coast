using System;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed class RemotePlayerV49:ICombatTarget
    {
        readonly CoopSessionV49 session;readonly SpriteRenderer shadow,weapon;PixelGait gait;
        public int Peer{get;}public LanPlayerV49 State{get;private set;}public SpriteRenderer Renderer{get;}public Rigidbody2D Body{get;}public Survivor Mirror{get;}=new Survivor();
        public Vector2 Position=>Renderer.transform.position;public bool Alive=>State!=null && State.health>0;Vector2 target;
        public RemotePlayerV49(CoopSessionV49 owner,LanPlayerV49 state)
        {
            session=owner;Peer=state.peer;var g=session.game;Renderer=g.art.Renderer("Союзник / "+Peer,g.art.hero[0,1],new Vector2(state.x,state.y),0);
            Body=Renderer.gameObject.AddComponent<Rigidbody2D>();Body.bodyType=RigidbodyType2D.Kinematic;Body.gravityScale=0;Body.simulated=false;
            shadow=g.Shadow("Co-op player shadow",Renderer.transform.position,new Vector2(.62f,.24f),.34f);shadow.transform.SetParent(Renderer.transform,true);
            weapon=g.art.Renderer("Co-op held weapon",g.art.icons[4],Renderer.transform.position,0);weapon.transform.SetParent(Renderer.transform,true);weapon.enabled=false;
            gait=new PixelGait(g.art,Renderer,g.HeroFramesV51((CharacterV51)state.character),.43f);Renderer.transform.position=target=new Vector2(state.x,state.y);Mirror.inventory.items.Clear();Mirror.spatialInventory.items.Clear();Receive(state);
        }
        public void Receive(LanPlayerV49 state)
        {
            if(State!=null && State.character!=state.character){if(gait.Lower)UnityEngine.Object.Destroy(gait.Lower.gameObject);gait=new PixelGait(session.game.art,Renderer,session.game.HeroFramesV51((CharacterV51)state.character),.43f);}
            bool changed=State==null || State.region!=state.region || Vector2.Distance(target,new Vector2(state.x,state.y))>6;State=state;target=new Vector2(state.x,state.y);if(changed)Renderer.transform.position=target;
            Mirror.character=(CharacterV51)state.character;Mirror.health=state.health;Mirror.hunger=state.hunger;Mirror.mana=state.mana;Mirror.immunity=state.immunity;
            if(state.equipment!=null && state.equipment.Length==Mirror.equipment.Length)for(int i=0;i<Mirror.equipment.Length;i++)Mirror.equipment[i]=state.equipment[i]?.Restore();
        }
        public void Tick(float dt)
        {
            bool visible=State!=null && State.region==session.Region;Renderer.gameObject.SetActive(visible);if(!visible)return;
            var g=session.game;var point=Vector2.Lerp(Position,target,1-Mathf.Exp(-dt*23));Renderer.transform.position=point;Body.position=point;
            Renderer.enabled=Alive;Renderer.sprite=g.HeroFramesV51((CharacterV51)State.character)[Mathf.Clamp(State.facing,0,7),Mathf.Clamp(State.frame,0,7)];Renderer.flipX=State.character==1?CoastGame.FrogFrameMirroredV52(State.facing,State.frame):State.facing>=5;
            Renderer.sortingOrder=CoastGame.Depth(point.y);Renderer.transform.localScale=Vector3.one*Mathf.Clamp(State.scale,.8f,1.2f);g.art.PixelStyle(Renderer);
            gait.Pose(State.facing,State.walk,State.moving,Vector2.zero);if(!Alive && gait.Lower)gait.Lower.enabled=false;shadow.enabled=Alive;shadow.transform.position=point;shadow.sortingOrder=Mathf.Min(-2000,Renderer.sortingOrder-1600);
            bool held=State.weapon>=0 && Enum.IsDefined(typeof(ItemKind),State.weapon);var kind=held?(ItemKind)State.weapon:ItemKind.Dagger;
            weapon.enabled=Alive && held && (State.attacking || kind==ItemKind.Orb || kind==ItemKind.Lamp || kind==ItemKind.LastPrism || kind==ItemKind.Spectrum);
            if(weapon.enabled)
            {
                var definition=ItemDefinition.All[kind];weapon.sprite=g.art.icons[definition.icon];bool vertical=Combat.VerticalV44(kind);float length=kind==ItemKind.Bow?1.5f:Combat.WeaponLengthV44(kind);
                float size=length/((vertical?weapon.sprite.rect.height:weapon.sprite.rect.width)/weapon.sprite.pixelsPerUnit);weapon.transform.localScale=Vector3.one*size;
                weapon.transform.localScale=Vector3.one*State.weaponScale;weapon.transform.position=new Vector2(State.weaponX,State.weaponY)+(point-target);weapon.transform.rotation=Quaternion.Euler(0,0,State.angle);weapon.sortingOrder=Renderer.sortingOrder+2;g.art.PixelStyle(weapon);
                var props=new MaterialPropertyBlock();weapon.GetPropertyBlock(props);props.SetFloat("_Spectrum",kind==ItemKind.Spectrum?1:0);props.SetFloat("_Prism",kind==ItemKind.LastPrism?1:0);weapon.SetPropertyBlock(props);
            }
        }
        public float Hurt(float amount)
        {
            if(!Alive || State.invulnerable || amount<=0 || !session.CanSimulate)return 0;
            float taken=Mathf.Min(State.health,amount);State.health-=taken;Mirror.health=State.health;
            session.Send(new LanPacketV49{kind="playerDamage",target=Peer,region=State.region,body=amount.ToString("R",System.Globalization.CultureInfo.InvariantCulture)});return taken;
        }
        public void Dispose(){if(Renderer)UnityEngine.Object.Destroy(Renderer.gameObject);}
    }
    public sealed partial class CoopSessionV49
    {
        public LanPlayerV49 CapturePlayer()
        {
            var g=game;return new LanPlayerV49{peer=Peer,name=PlayerName,character=(int)g.survivor.character,region=Region,x=g.body.position.x,y=g.body.position.y,health=g.survivor.health,hunger=g.survivor.hunger,mana=g.survivor.mana,immunity=g.survivor.immunity,facing=g.facing,frame=g.walkFrame,moving=g.walking,walk=g.walkTime,weapon=(int?)g.survivor.equipment[0]?.kind??-1,attacking=g.combat.Attacking,invulnerable=g.combat.ZenInvulnerable || g.spectrum.Riding || g.spellcraft.Teleporting,elapsed=g.combat.elapsed,angle=g.combat.weapon.transform.eulerAngles.z,scale=g.hero.transform.localScale.x,weaponX=g.combat.weapon.transform.position.x,weaponY=g.combat.weapon.transform.position.y,weaponScale=g.combat.weapon.transform.lossyScale.x,equipment=g.survivor.equipment.Select(i=>LanItemV49.Capture(i)).ToArray()};
        }
        public void ForNearest(Vector2 point,Action action,int? preferred=null)
        {
            if(preferred.HasValue && preferred.Value==Peer){ForLocalV49(action);return;}
            if(!Online || !CanSimulate){action();return;}var selected=preferred.HasValue && preferred.Value!=Peer?remote.Values.FirstOrDefault(p=>p.Peer==preferred.Value && p.Alive && p.State.region==Region):remote.Values.Where(p=>p.Alive && p.State.region==Region).OrderBy(p=>(p.Position-point).sqrMagnitude).FirstOrDefault();
            if(selected==null || !preferred.HasValue && !game.Dead && (game.body.position-point).sqrMagnitude<=(selected.Position-point).sqrMagnitude){action();return;}
            var oldHero=game.hero;var oldBody=game.body;var oldSurvivor=game.survivor;var oldContext=Context;Context=selected;
            try{game.hero=selected.Renderer;game.body=selected.Body;game.survivor=selected.Mirror;action();}
            finally{game.hero=oldHero;game.body=oldBody;game.survivor=oldSurvivor;Context=oldContext;}
        }
        public bool RedirectPlayerHurt(float damage)
        {if(!Online || Context==null || ApplyingNetwork)return false;Context.Hurt(damage);return true;}
        public bool IsPlayer(SpriteRenderer source)=>source==game.hero || remote.Values.Any(p=>p.Renderer==source);
        public ICombatTarget CurrentHeroTarget()=>Context!=null?(ICombatTarget)Context:new HeroCombatActor(game,true);
    }
}
