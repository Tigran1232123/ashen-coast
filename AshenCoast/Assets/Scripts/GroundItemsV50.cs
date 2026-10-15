using UnityEngine;
namespace AshenCoast
{
    public sealed partial class CoastGame
    {
        public void PoseGroundItemV50(GroundLoot drop)
        {
            if(drop.collected || drop.InFlight || drop.item.kind==ItemKind.Spectrum || drop.item.kind==ItemKind.Zen)return;
            var item=drop.item;var r=drop.renderer;
            bool flat=item.kind==ItemKind.Bow || item.Definition.Melee || item.Definition.Book || item.kind==ItemKind.Rope || item.kind==ItemKind.IronIngot || item.kind==ItemKind.Coin || item.Definition.Flower || item.kind==ItemKind.Clover || item.kind==ItemKind.FourLeafClover;
            bool vertical=Combat.VerticalV44(item.kind) || item.kind==ItemKind.Axe;
            float angle=item.kind==ItemKind.Bow?-68:item.Definition.Melee?(vertical?-72:-18):item.Definition.Flower?-28:0;
            r.transform.rotation=Quaternion.Euler(0,0,angle);
            var bounds=r.sprite.bounds;var offset=(Vector2)(r.transform.rotation*Vector3.Scale(bounds.center,r.transform.localScale));
            r.transform.position=drop.position-offset+Vector2.up*.08f;
            if(!flat)r.transform.position+=Vector3.up*(r.bounds.extents.y-.04f);
            r.sortingOrder=Depth(drop.position.y)+2;
            drop.shadow.transform.position=drop.position;drop.shadow.transform.localScale=new Vector3(Mathf.Max(.18f,r.bounds.size.x*.85f)/drop.shadow.sprite.bounds.size.x,Mathf.Clamp(r.bounds.size.y*.35f,.12f,.4f)/drop.shadow.sprite.bounds.size.y,1);
        }
        public static float MissingGroundSizeV50(Item item)
        {
            switch(item.kind)
            {
                case ItemKind.Sack:return .85f;case ItemKind.Bow:return 1.12f;
                case ItemKind.Tome:case ItemKind.OpenTome:case ItemKind.AbyssTome:case ItemKind.OpenAbyssTome:return .94f;
                case ItemKind.Pyromancy:case ItemKind.OpenPyromancy:return .72f;
                case ItemKind.Orb:return .52f;case ItemKind.Coin:return .26f;
                default:return item.Definition.width==1?.48f:.70f;
            }
        }
    }
}
