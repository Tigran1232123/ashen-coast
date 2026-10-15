using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public enum DamageDeliveryV54 { Other,Melee,Tentacle }
    public sealed partial class CoastGame
    {
        public bool CanReachV54(ICombatTarget target,DamageDeliveryV54 delivery)
            =>target!=null && !(target is RoosterDragon dragon && dragon.Action==DragonAction.Flight && delivery!=DamageDeliveryV54.Other);
        readonly Dictionary<int,PartyTrailV54> partyTrailsV54=new Dictionary<int,PartyTrailV54>();
        public Vector2 FormationPointV54(SpriteRenderer actor)
        {
            int owner=CoopOnline?(coop.world.AllyOwner(actor)??coop.Peer):0;
            var leader=CoopOnline && owner!=coop.Peer && coop.remote.TryGetValue(owner,out var remote)?remote.Position:CoopOnline?coop.LocalBody.position:body.position;
            if(!partyTrailsV54.TryGetValue(owner,out var trail)){trail=new PartyTrailV54();partyTrailsV54[owner]=trail;}
            trail.Sample(leader,map.CurrentRegion);
            var members=new List<SpriteRenderer>();var mage=journey.companion;
            if(mage.Hired && mage.Alive && !snails.Resting(mage.sprite) && !snails.Returning(mage.sprite) && (!CoopOnline || (coop.world.AllyOwner(mage.sprite)??coop.Peer)==owner))members.Add(mage.sprite);
            foreach(var a in amazons.people)if(a.Hired && a.Alive && !snails.Resting(a.Renderer) && !snails.Returning(a.Renderer) && (!CoopOnline || (coop.world.AllyOwner(a.Renderer)??coop.Peer)==owner))members.Add(a.Renderer);
            int index=Mathf.Max(0,members.IndexOf(actor)),row=index/2;var centre=trail.Behind(1.35f+row*.95f,out var heading);var side=new Vector2(-heading.y,heading.x)*((index%2==0?-1:1)*.49f);
            var goal=centre+side;
            if(WorldBlocked(centre,goal,.23f))goal=centre;
            return goal;
        }
    }
    sealed class PartyTrailV54
    {
        readonly List<Vector2> points=new List<Vector2>();MapRegion region;
        public void Sample(Vector2 at,MapRegion next)
        {
            if(points.Count==0 || region!=next || Vector2.Distance(at,points[0])>12){points.Clear();points.Add(at);region=next;return;}
            if(Vector2.Distance(at,points[0])>.16f)points.Insert(0,at);
            if(points.Count>240)points.RemoveRange(240,points.Count-240);
        }
        public Vector2 Behind(float distance,out Vector2 heading)
        {
            heading=Vector2.up;float total=0;
            for(int i=1;i<points.Count;i++)
            {var delta=points[i-1]-points[i];float length=delta.magnitude;if(length<.001f)continue;heading=delta/length;if(total+length>=distance)return Vector2.Lerp(points[i-1],points[i],(distance-total)/length);total+=length;}
            return points[points.Count-1]-heading*(distance-total);
        }
    }
    public sealed partial class InventoryUI
    {
        void DrawMageVitalsV54()
        {
            var mage=game.journey.companion;if(!mage.Hired || !mage.Alive || !mage.sprite.enabled || !mage.sprite.gameObject.activeInHierarchy || game.snails.Resting(mage.sprite))return;
            var at=World(new Vector2(mage.Position.x,mage.sprite.bounds.max.y+.16f));
            ActorVital(at,"",mage.health,CompanionMage.MaxHealth,new Color(.26f,.48f,.28f),mage.sprite,true);
            var mana=new Rect(at.x-50,at.y+17,100,5);Fill(mana,new Color(.035f,.04f,.05f,.9f));Fill(new Rect(mana.x+1,mana.y+1,98*Mathf.Clamp01(mage.mana/CompanionMage.MaxMana),3),new Color(.26f,.48f,.75f));
        }
        public bool TooltipVisibleV54=>open && hoverItem!=null && !Dragging && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !IsCrafting && !game.LocalMenuPausedV49;
        public Rect TooltipRectV54
        {
            get{const float w=300,h=145;float x=Pointer.x+20,y=Pointer.y+19;if(x+w>CanvasWidth-8)x=Pointer.x-w-16;if(y+h>CanvasHeight-8)y=Pointer.y-h-12;return new Rect(Mathf.Clamp(x,8,CanvasWidth-w-8),Mathf.Clamp(y,8,CanvasHeight-h-8),w,h);}
        }
        void DrawItemTooltipV54()
        {
            if(!TooltipVisibleV54)return;var r=TooltipRectV54;Frame(r,new Color(.055f,.052f,.042f,.98f));var item=hoverItem;var d=item.Definition;
            var color=GUI.color;if(d.Divine)GUI.color=SpectrumNameColorV43(Time.time);GUI.Label(new Rect(r.x+13,r.y+8,r.width-26,31),d.name,CachedStyle(bold,TextAnchor.MiddleLeft,18,FontStyle.Bold));GUI.color=color;
            var lines=new List<string>();
            if(d.Weapon){lines.Add(d.damage+" урона");lines.Add("Дальность "+d.reach+" · "+(1/Mathf.Max(.01f,item.kind==ItemKind.Zen?Combat.ZenIntervalV44(game.survivor.hunger):d.cooldown)).ToString("0.0")+" атак/с");if(item.kind==ItemKind.Zen)lines.Add(game.survivor.hunger>0?"4 сытости за удар":"4 здоровья за удар");if(item.kind==ItemKind.Axe)lines.Add("+20% урона по бронированным целям");}
            else if(d.Book)lines.Add(item.opened?"Книга открыта":"Книга закрыта");
            else if(d.Food){lines.Add("Сытость +"+d.nourishment+(d.mana>0?" · Мана +"+d.mana:""));if(item.kind!=ItemKind.Brain)lines.Add("Свежесть: "+(item.freshness/360).ToString("0.0")+" дн.");}
            if(d.Durable)lines.Add("Прочность: "+item.durability.ToString("0")+"%");
            if(item.kind==ItemKind.WaterBottle)lines.Add("Вода: "+(item.water*50).ToString("0")+"%");
            for(int i=0;i<Mathf.Min(4,lines.Count);i++)GUI.Label(new Rect(r.x+13,r.y+42+i*22,r.width-26,23),lines[i],small);
        }
    }
}
