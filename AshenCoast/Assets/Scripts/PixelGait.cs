using UnityEngine;

namespace AshenCoast
{
    // Both legs and the pelvis belong to one authored pose. No limb is stretched,
    // duplicated or bent by the shader; contact and passing poses are drawn together.
    public sealed class PixelGait
    {
        readonly ArtBank art;
        readonly SpriteRenderer body,lower;
        readonly Sprite[,] poses=new Sprite[4,9];
        readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
        readonly float cutoff,top,clothTone;
        readonly int directions,style;
        public bool Walking {get;private set;}
        public float Phase {get;private set;}
        public Vector2 Axis {get;private set;}
        public int Frame {get;private set;}
        public SpriteRenderer Lower=>lower;
        public float TorsoFloor=>(body.sprite.rect.height*cutoff-body.sprite.pivot.y)/body.sprite.pixelsPerUnit;

        public PixelGait(ArtBank bank,SpriteRenderer torso,Sprite[,] frames,float floor)
        {
            art=bank;body=torso;cutoff=floor;directions=frames.GetLength(0);
            var source=frames[0,3];
            style=source.texture.name.StartsWith("Mage")?2:source.texture.name.StartsWith("Tavern")?1:0;
            // The robe covers adult-sized legs rather than compressing a whole leg below its hem.
            top=style==2?.74f:(source.rect.height*floor-source.pivot.y)/source.pixelsPerUnit+.045f;
            clothTone=style==1?.78f:style==2?.65f:1;
            for(int view=0;view<4;view++)for(int frame=0;frame<9;frame++)
            {
                var s=bank.lowerPoses[view,frame];var r=s.rect;
                poses[view,frame]=Sprite.Create(s.texture,r,new Vector2(s.pivot.x/r.width,1),r.height/(top-.014f),0,SpriteMeshType.FullRect);
            }
            var go=new GameObject("Authored paired leg poses");go.transform.SetParent(body.transform,false);
            lower=go.AddComponent<SpriteRenderer>();lower.sharedMaterial=art.lit;
            lower.transform.localPosition=new Vector3(0,top,0);
        }
        public void Pose(int facing,float cycles,bool walking,Vector2 forward,bool flip=false)
        {
            int direction=Mathf.Clamp(facing,0,directions-1);
            Phase=Mathf.Repeat(cycles,1);Walking=walking;
            if(forward.sqrMagnitude>.0001f)Axis=forward.normalized;
            int view;bool mirror;
            if(style==1){view=3;mirror=true;}
            else if(directions>=7)
            {
                view=direction==0?0:direction==1 || direction==7?3:direction==2 || direction==6?1:2;
                mirror=direction>=5;
            }
            else {view=direction==0?0:direction==2?2:1;mirror=direction==3;}
            Frame=walking?1+Mathf.FloorToInt(Phase*8):0;
            body.GetPropertyBlock(block);block.SetFloat("_BodyFloor",cutoff);body.SetPropertyBlock(block);
            lower.enabled=body.enabled;lower.color=body.color;
            lower.sortingLayerID=body.sortingLayerID;lower.sortingOrder=body.sortingOrder-1;
            lower.sprite=poses[view,Frame];lower.flipX=mirror;
            art.PixelStyle(lower);lower.GetPropertyBlock(block);block.SetFloat("_ClothTone",clothTone);lower.SetPropertyBlock(block);
        }
    }
}
