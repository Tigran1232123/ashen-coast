using System;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class RemotePlayerV49:ICombatTarget
    {
        readonly CoopSessionV49 session;readonly SpriteRenderer shadow,weapon,seatedV58;PixelGait gait;
        public int Peer{get;}public LanPlayerV49 State{get;private set;}public SpriteRenderer Renderer{get;}public Rigidbody2D Body{get;}public Survivor Mirror{get;}=new Survivor();
        CraftingStation mountV58;
        string equipmentFingerprintV57;readonly MaterialPropertyBlock weaponPropsV57=new MaterialPropertyBlock();
        Vector2 velocityV57;float receivedV57;
        public SpriteRenderer LowerV57=>gait.Lower;
        public Vector2 Position=>Renderer.transform.position;public bool Alive=>State!=null && State.health>0;Vector2 target;
        public RemotePlayerV49(CoopSessionV49 owner,LanPlayerV49 state)
        {
            session=owner;Peer=state.peer;var g=session.game;Renderer=g.art.Renderer("Союзник / "+Peer,g.art.hero[0,1],new Vector2(state.x,state.y),0);
            Body=Renderer.gameObject.AddComponent<Rigidbody2D>();Body.bodyType=RigidbodyType2D.Kinematic;Body.gravityScale=0;Body.simulated=false;
            shadow=g.Shadow("Co-op player shadow",Renderer.transform.position,new Vector2(.62f,.24f),.34f);shadow.transform.SetParent(Renderer.transform,true);
            weapon=g.art.Renderer("Co-op held weapon",g.art.icons[4],Renderer.transform.position,0);weapon.transform.SetParent(Renderer.transform,true);weapon.enabled=false;
            seatedV58=g.art.Renderer("Co-op seated torso",Renderer.sprite,Vector2.zero,0);seatedV58.transform.SetParent(Renderer.transform,false);seatedV58.enabled=false;
            gait=new PixelGait(g.art,Renderer,g.HeroFramesV51((CharacterV51)state.character),.43f);Renderer.transform.position=target=new Vector2(state.x,state.y);Mirror.inventory.items.Clear();Mirror.spatialInventory.items.Clear();Receive(state);
        }
        public void Receive(LanPlayerV49 state)
        {
            if(State!=null && State.character!=state.character){if(gait.Lower)UnityEngine.Object.Destroy(gait.Lower.gameObject);gait=new PixelGait(session.game.art,Renderer,session.game.HeroFramesV51((CharacterV51)state.character),.43f);}
            float now=Time.unscaledTime;var next=new Vector2(state.x,state.y);
            bool changed=State==null || State.region!=state.region || Vector2.Distance(target,next)>6;
            velocityV57=!changed && state.moving && now>receivedV57+.005f?Vector2.ClampMagnitude((next-target)/(now-receivedV57),24):Vector2.zero;receivedV57=now;State=state;target=new Vector2(state.x,state.y);if(changed)Renderer.transform.position=target;
            Mirror.character=(CharacterV51)state.character;Mirror.health=state.health;Mirror.hunger=state.hunger;Mirror.mana=state.mana;Mirror.immunity=state.immunity;
            // Keep the correct race pose even while the proxy is in another location.
            // Corpse presentation may need that last pose before it becomes visible here.
            Renderer.sprite=session.game.HeroFramesV51((CharacterV51)state.character)[Mathf.Clamp(state.facing,0,7),Mathf.Clamp(state.frame,0,7)];session.game.art.PixelStyle(Renderer);float raceScale=state.character==1?.6f:1;shadow.transform.localScale=new Vector3(.62f*raceScale,.24f*raceScale,1);
            if(state.equipment!=null && state.equipment.Length==Mirror.equipment.Length){var signature=EquipmentSignatureV57(state.equipment);if(signature!=equipmentFingerprintV57){for(int i=0;i<Mirror.equipment.Length;i++)Mirror.equipment[i]=state.equipment[i]?.Restore();equipmentFingerprintV57=signature;}}
        }
        public void Tick(float dt)
        {
            bool visible=State!=null && State.region==session.Region;Renderer.gameObject.SetActive(visible);if(!visible)return;
            var g=session.game;float age=Mathf.Clamp(Time.unscaledTime-receivedV57,0,.10f);
            var point=Vector2.Lerp(Position,target+velocityV57*age,1-Mathf.Exp(-dt*23));Renderer.transform.position=point;Body.position=point;
            Renderer.enabled=Alive;Renderer.sprite=g.HeroFramesV51((CharacterV51)State.character)[Mathf.Clamp(State.facing,0,7),Mathf.Clamp(State.frame,0,7)];Renderer.flipX=State.character==1?CoastGame.FrogFrameMirroredV52(State.facing,State.frame):State.facing>=5;
            Renderer.sortingOrder=CoastGame.Depth(point.y);Renderer.transform.localScale=Vector3.one*Mathf.Clamp(State.scale,.8f,1.2f);g.art.PixelStyle(Renderer);
            if(!State.meditating && !State.mountedV58){gait.Lower.transform.localScale=Vector3.one;gait.ResetAnchorV58();}gait.Pose(State.facing,State.walk+(State.moving?age*velocityV57.magnitude/1.65f:0),State.moving,velocityV57);if(!Alive && gait.Lower)gait.Lower.enabled=false;shadow.enabled=Alive;shadow.transform.position=point;shadow.sortingOrder=Mathf.Min(-2000,Renderer.sortingOrder-1600);
            if(mountV58!=null && State.mountKeyV58!=g.MountKeyV58(mountV58)){g.PoseCrossbowV58(mountV58,false);mountV58=null;}if(State.mountedV58){mountV58=g.buildings.stations.FirstOrDefault(s=>s.kind==ItemKind.Crossbow && g.MountKeyV58(s)==State.mountKeyV58);if(mountV58!=null)g.PoseCrossbowV58(mountV58,State.facing>=1 && State.facing<=3);}
            PoseSeatedV58(point);var bowMask=new MaterialPropertyBlock();Renderer.GetPropertyBlock(bowMask);bowMask.SetFloat("_ArcheryV58",State.bowDraw?1:0);Renderer.SetPropertyBlock(bowMask);
            bool held=State.weapon>=0 && Enum.IsDefined(typeof(ItemKind),State.weapon);var kind=held?(ItemKind)State.weapon:ItemKind.Dagger;
            weapon.enabled=Alive && held && (kind!=ItemKind.Crossbow && State.attacking || kind==ItemKind.Orb || kind==ItemKind.Lamp || kind==ItemKind.LastPrism || kind==ItemKind.Spectrum);
            if(weapon.enabled)
            {
                var definition=ItemDefinition.All[kind];weapon.sprite=g.art.icons[definition.icon];bool vertical=Combat.VerticalV44(kind);float length=kind==ItemKind.Bow?1.5f:Combat.WeaponLengthV44(kind);
                float size=length/((vertical?weapon.sprite.rect.height:weapon.sprite.rect.width)/weapon.sprite.pixelsPerUnit);weapon.transform.localScale=Vector3.one*size;
                weapon.transform.localScale=Vector3.one*State.weaponScale;weapon.transform.position=new Vector2(State.weaponX,State.weaponY)+(point-target);weapon.transform.rotation=Quaternion.Euler(0,0,State.angle);weapon.sortingOrder=Renderer.sortingOrder+2;g.art.PixelStyle(weapon);
                var props=weaponPropsV57;weapon.GetPropertyBlock(props);props.SetFloat("_Spectrum",kind==ItemKind.Spectrum?1:0);props.SetFloat("_Prism",kind==ItemKind.LastPrism?1:0);weapon.SetPropertyBlock(props);
            }
            PoseBowV58();
        }
        void PoseSeatedV58(Vector2 point)
        {
            bool sitting=Alive && (State.meditating || State.mountedV58);seatedV58.enabled=sitting;
            if(sitting && State.character==1){seatedV58.enabled=false;gait.Lower.enabled=false;Renderer.transform.localScale=new Vector3(1,.82f,1);return;}
            var block=new MaterialPropertyBlock();gait.Lower.GetPropertyBlock(block);block.SetFloat("_SeatedLegs",sitting?1:0);gait.Lower.SetPropertyBlock(block);if(!sitting)return;
            var art=session.game.art;Renderer.enabled=false;seatedV58.sprite=Renderer.sprite;seatedV58.flipX=Renderer.flipX;seatedV58.color=Renderer.color;seatedV58.sortingOrder=Renderer.sortingOrder;seatedV58.transform.localPosition=Vector2.down*.37f;art.PixelStyle(seatedV58);seatedV58.GetPropertyBlock(block);block.SetFloat("_BodyFloor",.43f);seatedV58.SetPropertyBlock(block);
            if(State.mountedV58){gait.Lower.sprite=session.game.operatorLegsV58;gait.Lower.flipX=Renderer.flipX;gait.Lower.enabled=true;gait.Lower.transform.localPosition=new Vector2(0,.02f);gait.Lower.transform.localScale=Vector3.one*(.52f/gait.Lower.sprite.bounds.size.y);art.PixelStyle(gait.Lower);gait.Lower.GetPropertyBlock(block);block.SetFloat("_SeatedLegs",0);gait.Lower.SetPropertyBlock(block);return;}
            gait.Lower.enabled=true;gait.Lower.transform.localPosition=new Vector2(0,.37f);gait.Lower.transform.localScale=new Vector3(.85f/gait.Lower.sprite.bounds.size.x,.38f/gait.Lower.sprite.bounds.size.y,1);
        }
        static string EquipmentSignatureV57(LanItemV49[] items)
        {
            var text=new System.Text.StringBuilder();
            foreach(var item in items)AppendEquipmentV57(text,item);
            return text.ToString();
        }
        static void AppendEquipmentV57(System.Text.StringBuilder text,LanItemV49 item)
        {
            if(item==null){text.Append("null;");return;}
            text.Append(item.id).Append('/').Append(item.kind).Append('/').Append(item.count).Append('/').Append(item.opened).Append('/').Append(item.durability).Append(';');
            // Remote combat only uses equipped item identity and slots; private bag contents arrive through private-inventory messages.
            if(item.spells!=null)foreach(var spell in item.spells){text.Append(spell.kind).Append('/').Append(spell.locked);AppendEquipmentV57(text,spell.scroll);}
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
        static LanItemV49 CaptureEquipmentV57(Item item)
        {
            if(item==null)return new LanItemV49{kind=-1};
            return new LanItemV49{capturedAt=LanItemV49.SnapshotClock(),id=item.id,kind=(int)item.kind,x=item.x,y=item.y,count=item.count,opened=item.opened,
                durability=item.durability,fuel=item.fuel,freshness=item.freshness,shelfLife=item.shelfLife,water=item.water,
                spells=item.spells?.Select(slot=>new LanSpellV49{kind=(int)slot.kind,locked=slot.locked,remaining=slot.Remaining(LanItemV49.Clock()),scroll=LanItemV49.Capture(slot.scroll)}).ToArray()};
        }
        public LanPlayerV49 CapturePlayer()
        {
            var g=game;return new LanPlayerV49{peer=Peer,name=PlayerName,character=(int)g.survivor.character,region=Region,x=g.body.position.x,y=g.body.position.y,health=g.survivor.health,hunger=g.survivor.hunger,mana=g.survivor.mana,immunity=g.survivor.immunity,facing=g.facing,frame=g.walkFrame,moving=g.walking,walk=g.walkTime,weapon=g.MountedCrossbowV58!=null?(int)ItemKind.Crossbow:(int?)g.survivor.equipment[0]?.kind??-1,mountedV58=g.MountedCrossbowV58!=null,mountKeyV58=g.MountedKeyV58,meditating=g.Meditating,bowDraw=g.combat.BowDrawingV58,bowChargeV58=g.combat.BowChargeV58,attacking=g.combat.Attacking || g.combat.BowDrawingV58,invulnerable=g.survivor.InfiniteVitalsV58 || g.combat.ZenInvulnerable || g.spectrum.Riding || g.spellcraft.Teleporting,elapsed=g.combat.elapsed,angle=g.combat.weapon.transform.eulerAngles.z,scale=g.hero.transform.localScale.x,weaponX=g.combat.weapon.transform.position.x,weaponY=g.combat.weapon.transform.position.y,weaponScale=g.combat.weapon.transform.lossyScale.x,equipment=g.survivor.equipment.Select(CaptureEquipmentV57).ToArray()};
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
