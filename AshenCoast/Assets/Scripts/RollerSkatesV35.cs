using UnityEngine;
namespace AshenCoast
{
    public sealed partial class Survivor
    {
        public readonly GridInventory skateSlot=new GridInventory(2,2,"Ролики"){craftingEnabled=false,onlyKind=ItemKind.RollerSkates};
        public Item Skates=>skateSlot.items.Count>0?skateSlot.items[0]:null;
        public bool EquipSkates(Item item,GridInventory source,bool canDrop,out Item dropped)
        {
            dropped=null;if(item?.kind!=ItemKind.RollerSkates || source==null || !source.items.Contains(item))return false;
            if(source==skateSlot)return true;var previous=Skates;
            if(previous!=null && !canDrop && !inventory.FindSpace(previous,out _,new[]{item}))return false;
            source.items.Remove(item);skateSlot.items.Clear();
            if(previous!=null && !inventory.Add(previous))dropped=previous;
            return skateSlot.Place(item,0,0);
        }
    }
    public sealed class RollerMotion
    {
        public const float AccelerationSeconds=3,MaximumBonus=.30f;
        public float movingSeconds {get;private set;}
        public float Bonus=>MaximumBonus*Mathf.Clamp01(movingSeconds/AccelerationSeconds);
        public Vector2 Step(Vector2 input,Vector2 current,float baseSpeed,float dt)
        {
            if(input.sqrMagnitude>.01f)
            {
                movingSeconds=current.sqrMagnitude>.01f?Mathf.Min(AccelerationSeconds,movingSeconds+dt):0;
                if(current.sqrMagnitude>.1f && Vector2.Dot(current.normalized,input)<-.3f)movingSeconds=0;
                return Vector2.MoveTowards(current,input.normalized*baseSpeed*(1+Bonus),baseSpeed*3.0f*dt);
            }
            movingSeconds=0;return Vector2.MoveTowards(current,Vector2.zero,baseSpeed*1.25f*dt);
        }
        public void Reset(){movingSeconds=0;}
    }
    public sealed partial class CoastGame
    {
        public readonly RollerMotion rollers=new RollerMotion();
        public Vector2 MotionVelocity(Vector2 input,float dt)
        {
            if(survivor.Skates!=null)return rollers.Step(input,body.linearVelocity,BaseMovementSpeed,dt);
            rollers.Reset();return input*BaseMovementSpeed;
        }
    }
}
