using System;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class CoastGame
    {
        public bool SimulateWorldV49=>!CoopOnline || coop.CanSimulate;
        public void TickCoopActorV49(Vector2 point,Action action,SpriteRenderer ally=null)
        {if(!SimulateWorldV49)return;if(CoopOnline){if(ally && coop.world.allyRegions.TryGetValue(coop.world.Key(ally)??"",out var region) && region!=coop.Region){ally.gameObject.SetActive(false);return;}coop.ForNearest(point,action,ally?coop.world.AllyOwner(ally):null);}else action();}
        public GroundLoot NetworkDropV49(Item item,Vector2 at,bool thrown)
        {return coop.inventory.Mutate(()=>CreateLoot(item,at,thrown));}
    }
    public sealed partial class AmazonSettlementV46
    {public Transform RoomRootV49(int i)=>rooms[houses[i]].transform;}
    public sealed partial class InventoryUI
    {
        public void RestoreHandV49()
        {dragItem=holding.items.FirstOrDefault();dragSource=dragItem!=null?holding:null;origin=null;dragGear=-1;dragMerchant=false;grab=Vector2Int.zero;dragPress=Pointer;abyssSelecting=false;bookSelected=null;hammer=false;}
        public void RefreshSelectionV49(){if(bookSelected!=null && !game.survivor.Books.Contains(bookSelected))bookSelected=null;}
        public bool NetworkInventoryBusyV49=>game.CoopOnline && game.coop.inventory.Busy;
    }
    public sealed partial class ProceduralMeadow
    {public void SetPickedV49(long id,bool value){if(value)picked.Add(id);else picked.Remove(id);}}
    public sealed partial class CoopSessionV49
    {
        public void ForLocalV49(Action action)
        {var oldHero=game.hero;var oldBody=game.body;var oldSurvivor=game.survivor;var old=Context;try{Context=null;game.hero=LocalHero;game.body=LocalBody;game.survivor=LocalSurvivor;action();}finally{Context=old;game.hero=oldHero;game.body=oldBody;game.survivor=oldSurvivor;}}
        public bool RequestHireV49(string key)
        {if(inventory.Busy || game.survivor.Coins<(key=="mage"?4:1))return false;HirePendingV49=true;Send(new LanPacketV49{kind="hire",region=Region,body=key});return true;}
        public void ApplyHireV49(LanPacketV49 packet)
        {
            Debug.Log("LAN hire evaluate: key="+packet.body+" peer="+packet.peer);
            var actor=world.Find(packet.body);if(actor==null || actor.target==null || !actor.target.Alive || world.allyOwners.ContainsKey(packet.body) || !players.TryGetValue(packet.peer,out var player)){Send(new LanPacketV49{kind="hireRejected",target=packet.peer,region=packet.region,body="Недоступно"});return;}
            Debug.Log("LAN hire distance="+Vector2.Distance(actor.renders[0].transform.position,new Vector2(player.x,player.y))+" actor="+actor.renders[0].transform.position+" player="+player.x+","+player.y);
            if(Vector2.Distance(actor.renders[0].transform.position,new Vector2(player.x,player.y))>5){Send(new LanPacketV49{kind="hireRejected",target=packet.peer,region=packet.region,body="Слишком далеко"});return;}
            if(IsHost)ApproveHireV49(packet.body,packet.peer,packet.region);else Send(new LanPacketV49{kind="hireApproved",region=packet.region,body=packet.body,target=packet.peer});
        }
        void ApproveHireV49(string key,int peer,string region)
        {
            Debug.Log("LAN hire approve: coins="+inventory.CoinsV49(peer)+" key="+key);
            if(world.allyOwners.ContainsKey(key) || !inventory.PayHireV49(peer,key=="mage"?4:1)){SendToV49(peer,new LanPacketV49{kind="hireFailed",body="Недостаточно монет"});return;}world.allyOwners[key]=peer;world.allyRegions[key]=region;BroadcastV49(new LanPacketV49{kind="hired",peer=peer,target=peer,region=region,body=key});
        }
        void ReceiveHireV49(LanPacketV49 packet)
        {
            Debug.Log("LAN hire receive: target="+packet.target+" local="+Peer);
            world.allyOwners[packet.body]=packet.target;world.allyRegions[packet.body]=packet.region;var b=world.Find(packet.body);if(b==null)return;
            if(packet.target==Peer)
            {
                HirePendingV49=false;
                ApplyingNetwork=true;try{if(b.driver is VillageActorV46 woman){if(!game.amazons.Hire(woman)){world.allyOwners.Remove(packet.body);game.ui.Notice("Недостаточно монет");}}else if(b.driver is CompanionMage){if(!game.journey.HireMage())game.ui.Notice("Недостаточно монет");}}finally{ApplyingNetwork=false;}
            }
            else if(b.driver is VillageActorV46 actor){actor.Hired=true;actor.Captive=false;actor.hostile=false;if(actor.cage)actor.cage.enabled=false;}else if(b.driver is CompanionMage mage){mage.Hire(mage.Position);game.journey.drinker.sprite.enabled=false;}
            world.UpdateAlliesV49();
        }
        public void ApplyEffectV49(LanPacketV49 packet)
        {
            var effect=JsonUtility.FromJson<LanEffectV49>(packet.body);var target=world.Find(effect.key)?.target;
            if(effect.kind=="explodeFire"){ForNearest(new Vector2(effect.x,effect.y),()=>game.arcane.ExplodeNetworkFireV49(new Vector2(effect.x,effect.y)),packet.peer);return;}
            if(effect.kind=="fog"){game.spellcraft.CreateFog(new Vector2(effect.x,effect.y));return;}
            if(effect.kind=="tentacles"){ForNearest(new Vector2(effect.x,effect.y),()=>game.arcane.SummonTentacles(new Vector2(effect.x,effect.y),Mathf.Clamp(effect.value,1,6)),packet.peer);return;}
            if(target!=null){if(effect.kind=="stun")game.v44.Apply(target);else if(effect.kind=="bind")game.spellcraft.Bind(target);else if(effect.kind=="curse")game.spellcraft.Curse(target);}
        }
        public bool NotifyEffectV49(string kind,ICombatTarget target)
        {if(Online && !ApplyingNetwork && !CanSimulate){Send(new LanPacketV49{kind="effect",region=Region,body=JsonUtility.ToJson(new LanEffectV49{kind=kind,key=world.Key(target)})});return true;}return false;}
    }
}
