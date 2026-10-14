using UnityEngine;
namespace AshenCoast
{
    public sealed class HunterPresentation
    {
        readonly NightHunter hunter;
        readonly MeshFilter filter;
        Mesh mesh;
        readonly Mesh[,] poseMeshes=new Mesh[3,8];
        readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
        readonly SpriteRenderer[] contacts=new SpriteRenderer[4];
        static readonly int[] runPivots={Shader.PropertyToID("_RunPivot0"),Shader.PropertyToID("_RunPivot1"),Shader.PropertyToID("_RunPivot2"),Shader.PropertyToID("_RunPivot3")};
        static readonly int[] runMotions={Shader.PropertyToID("_RunMotion0"),Shader.PropertyToID("_RunMotion1"),Shader.PropertyToID("_RunMotion2"),Shader.PropertyToID("_RunMotion3")};
        static readonly Vector2[][] feet={
            new[]{new Vector2(.27f,.065f),new Vector2(.73f,.065f),new Vector2(.07f,.24f),new Vector2(.89f,.24f)},
            new[]{new Vector2(.14f,.185f),new Vector2(.45f,.23f),new Vector2(.75f,.21f),new Vector2(.58f,.20f)},
            new[]{new Vector2(.12f,.185f),new Vector2(.42f,.32f),new Vector2(.63f,.15f),new Vector2(.27f,.35f)}};
        public HunterPresentation(NightHunter owner)
        {
            hunter=owner;filter=hunter.skin.GetComponent<MeshFilter>();
            for(int i=0;i<4;i++)
            {
                contacts[i]=hunter.game.Shadow("Axolotl paw contact "+i,hunter.Position,new Vector2(.62f,.19f),.30f);
                contacts[i].transform.SetParent(hunter.sprite.transform,true);
            }
        }
        public void Pose()
        {
            if(hunter.AttackAnimating && hunter.game.v37Art!=null){hunter.PoseAttackV37();return;}hunter.sprite.forceRenderingOff=true;hunter.skin.enabled=true;
            float side=Mathf.Abs(hunter.look.x);
            if(hunter.view==1?side<.43f:side>.64f)hunter.view=side>.55f?1:hunter.look.y>.25f?2:0;
            else if(hunter.view!=1 && (hunter.view==2?hunter.look.y<.10f:hunter.look.y>.38f))hunter.view=hunter.look.y>.25f?2:0;
            // Four separately weighted legs move through 144 phases without swapping mismatched whole-body poses.
            int sourceFrame=0;
            hunter.sprite.sprite=hunter.art.axolotl[hunter.view,sourceFrame];hunter.sprite.flipX=hunter.look.x>0;
            // Mirror the whole registered mesh, including its gill weights, as one object.
            hunter.skin.transform.localScale=new Vector3(hunter.sprite.flipX?-1:1,1,1);
            // A fixed world scale preserves the creature's size while the window is resized.
            float scale=hunter.view==0?1:NightHunter.ProfileLength/hunter.art.axolotl[hunter.view,0].bounds.size.x;
            hunter.sprite.transform.localScale=Vector3.one*scale;
            hunter.sprite.sortingOrder=CoastGame.Depth(hunter.Position.y);hunter.skin.sortingOrder=hunter.sprite.sortingOrder;
            if(poseMeshes[hunter.view,sourceFrame]==null){
            mesh=new Mesh{name="Painted axolotl pose "+hunter.view+" / "+hunter.frame};
            const int nx=64,ny=80;var vertices=new Vector3[(nx+1)*(ny+1)];var uv=new Vector2[vertices.Length];var weights=new Color[vertices.Length];var legs=new System.Collections.Generic.List<Vector4>(vertices.Length);var indices=new int[nx*ny*6];var bounds=hunter.sprite.sprite.bounds;
            for(int y=0;y<=ny;y++)for(int x=0;x<=nx;x++){int i=y*(nx+1)+x;uv[i]=new Vector2(x/(float)nx,y/(float)ny);vertices[i]=new Vector3(Mathf.Lerp(bounds.min.x,bounds.max.x,uv[i].x),Mathf.Lerp(bounds.min.y,bounds.max.y,uv[i].y),0);weights[i]=new Color(NightHunter.GillWeight(hunter.view,uv[i]),uv[i].x<.5f?0:1,NightHunter.TailWeight(hunter.view,uv[i]),NightHunter.PawWeight(hunter.view,uv[i]));legs.Add(NightHunter.LegWeights(hunter.view,uv[i]));}
            int n=0;for(int y=0;y<ny;y++)for(int x=0;x<nx;x++){int i=y*(nx+1)+x;indices[n++]=i;indices[n++]=i+nx+1;indices[n++]=i+1;indices[n++]=i+1;indices[n++]=i+nx+1;indices[n++]=i+nx+2;}
            mesh.Clear();mesh.vertices=vertices;mesh.uv=uv;mesh.colors=weights;mesh.SetUVs(1,legs);mesh.triangles=indices;mesh.RecalculateBounds();var expanded=mesh.bounds;expanded.Expand(2);mesh.bounds=expanded;poseMeshes[hunter.view,sourceFrame]=mesh;}
            mesh=poseMeshes[hunter.view,sourceFrame];filter.sharedMesh=mesh;
            var t=hunter.sprite.sprite.texture;var r=hunter.sprite.sprite.rect;block.SetTexture("_MainTex",t);block.SetVector("_SpriteRect",new Vector4(r.x/t.width,r.y/t.height,r.width/t.width,r.height/t.height));block.SetFloat("_GillTime",hunter.clock);block.SetFloat("_TailTime",hunter.clock);block.SetFloat("_ArtStep",WorldArtStyle.SamplingStep(hunter.sprite.sprite,hunter.sprite.transform.lossyScale.x));
            Vector2 Local(Vector2 uv)=>new Vector2(hunter.sprite.sprite.bounds.min.x+hunter.sprite.sprite.bounds.size.x*uv.x,hunter.sprite.sprite.bounds.min.y+hunter.sprite.sprite.bounds.size.y*uv.y);
            var tail=Local(hunter.TailPivot);var paw=Local(hunter.PawPivot);var motion=NightHunter.PawMotion(hunter.IdleSeconds,hunter.view);
            block.SetVector("_TailPivot",new Vector4(tail.x,tail.y,0,0));block.SetVector("_PawPivot",new Vector4(paw.x,paw.y,0,0));
            block.SetVector("_PawMotion",new Vector4(motion.x,motion.y,motion.z,0));
            for(int leg=0;leg<4;leg++){
                var pivot=Local(NightHunter.LegPivots[hunter.view][leg]);var gait=NightHunter.LegMotion(hunter.view,leg,hunter.MotionFrame)*(.80f*hunter.MotionBlend);
                block.SetVector(runPivots[leg],new Vector4(pivot.x,pivot.y,0,0));block.SetVector(runMotions[leg],new Vector4(gait.x,gait.y,gait.z,0));
            }
            hunter.skin.SetPropertyBlock(block);
            if(hunter.QuoteAge<NightHunter.QuoteDuration)hunter.QuoteAnchor=hunter.HeadTop+Vector2.up*.48f;
            UpdateGroundContact();
        }
        void UpdateGroundContact()
        {
            var scale=hunter.sprite.transform.localScale.x;var centre=Vector2.zero;
            for(int i=0;i<4;i++)
            {
                var point=hunter.ArtPoint(feet[hunter.view][i]);centre+=point;
                if(hunter.MotionBlend>0){var move=NightHunter.LegMotion(hunter.view,i,hunter.MotionFrame)*(.80f*hunter.MotionBlend);point.x+=(hunter.sprite.flipX?-1:1)*move.x*scale;point.y+=move.y*scale*.25f;}
                contacts[i].transform.position=point+Vector2.up*.025f;
                contacts[i].transform.localScale=new Vector3(.62f/scale,.19f/scale,1);
                float lift=hunter.MotionBlend>0?Mathf.Max(0,NightHunter.LegMotion(hunter.view,i,hunter.MotionFrame).y)*hunter.MotionBlend:(i==0?Mathf.Max(0,NightHunter.PawMotion(hunter.IdleSeconds,hunter.view).y):0);
                contacts[i].color=new Color(.018f,.020f,.027f,.30f*(1-lift*2.5f));
                contacts[i].sortingOrder=Mathf.Min(-2000,hunter.skin.sortingOrder-1600);
            }
            centre/=4;
            hunter.shadow.transform.position=centre+Vector2.up*.15f;
            hunter.shadow.transform.localScale=new Vector3((hunter.view==0?4.3f:5.3f)/scale,1.10f/scale,1);
            hunter.shadow.color=new Color(.018f,.020f,.027f,.30f);
        }
        public void Dispose()
        {
            foreach(var pose in poseMeshes)if(pose)Object.Destroy(pose);
            if(hunter.skin && hunter.skin.sharedMaterial)Object.Destroy(hunter.skin.sharedMaterial);
        }
    }
}
