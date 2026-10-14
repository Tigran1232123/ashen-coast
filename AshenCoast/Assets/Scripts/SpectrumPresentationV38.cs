using System.Collections.Generic;
using UnityEngine;

namespace AshenCoast
{
    public sealed partial class SpectrumWeapon
    {
        const float FallDuration=.78f,TailDuration=1.65f;
        SpriteRenderer impactGlow;
        Material mistMaterial;
        readonly List<SpriteRenderer> spray=new List<SpriteRenderer>();
        readonly List<LandingSpark> landingSparks=new List<LandingSpark>();
        sealed class LandingSpark{public SpriteRenderer sprite;public Vector2 origin;public float age,seed;}
        public float FlowClock=>age;
        public float TailHeight=>endingAge<0?RainbowHeight:RainbowHeight*Mathf.Pow(1-Mathf.Clamp01(endingAge/TailDuration),1.65f);
        public float FallingHead=>RainbowHeight*Mathf.Pow(1-Mathf.Clamp01(age/FallDuration),2.1f);
        public float RainbowHeight=>Mathf.Max(14,game.cameraView.transform.position.y+game.cameraView.orthographicSize-Position.y+4);
        public float ImpactStrength=>Mathf.SmoothStep(0,1,Mathf.Clamp01((age-.53f)/.32f))*(endingAge<0?1:1-Mathf.SmoothStep(0,1,Mathf.Clamp01((endingAge-1.55f)/.9f)));

        void CreateRainbowVisuals(Vector2 point)
        {
            wall=game.art.Renderer("Animated Spectrum waterfall from beyond the screen",game.v38Art.flow,point,CoastGame.Depth(point.y)+25);
            wall.transform.SetParent(game.arcane.WorldRoot,true);
            wall.sharedMaterial=new Material(Shader.Find("AshenCoast/Rainbow"));wall.sharedMaterial.mainTexture=wall.sprite.texture;
            mistMaterial=new Material(Shader.Find("AshenCoast/RainbowMist"));
            for(int i=0;i<12;i++)
            {
                var cloud=game.art.Renderer("Rolling waterfall cloud / layer "+i,game.v38Art.mist[i<3?0:i<7?1:2,0],point,wall.sortingOrder+1);
                cloud.transform.SetParent(game.arcane.WorldRoot,true);cloud.sharedMaterial=mistMaterial;clouds.Add(cloud);
            }
            impactGlow=game.art.Renderer("Dithered rainbow reflected light",game.v38Art.quad,point,CoastGame.Depth(point.y)-2);
            impactGlow.transform.SetParent(game.arcane.WorldRoot,true);impactGlow.sharedMaterial=new Material(Shader.Find("AshenCoast/RainbowGround"));
            for(int i=0;i<48;i++)
            {
                var dot=game.art.Renderer("Rainbow falling spray droplet "+i,game.v37Art.mote,point,wall.sortingOrder+3);
                dot.sharedMaterial=game.v37Art.glowMaterial;dot.transform.SetParent(game.arcane.WorldRoot,true);spray.Add(dot);
            }
        }
        public void RefreshPresentation()=>PoseRainbowV38();
        void PoseRainbowV38()
        {
            if(!wall)return;
            float height=RainbowHeight,tail=TailHeight;
            float lift=0;
            if(Riding && rideAge>.55f && rideAge<1.6f)lift=(rideAge-.55f)*30;
            if(Riding && rideAge>=2.1f)lift=Mathf.Max(0,(3-rideAge)*24);
            Vector2 origin=Position+Vector2.up*lift;
            wall.transform.localScale=new Vector3(5.2f/wall.sprite.bounds.size.x,height/wall.sprite.bounds.size.y,1);
            wall.transform.position=origin;wall.sortingOrder=CoastGame.Depth(Position.y)+25;
            var material=wall.sharedMaterial;
            material.SetFloat("_Phase",age);material.SetFloat("_Height",height);material.SetFloat("_Head",FallingHead);material.SetFloat("_Tail",tail);material.SetFloat("_Ending",endingAge>=0?1:0);material.SetFloat("_Fade",1);
            material.SetFloat("_Hover",!game.testMode && Hovered(game.CursorWorld)?1:0);
            float impact=ImpactStrength;
            float burst=endingAge>=0?Mathf.Sin(Mathf.Clamp01(endingAge/1.9f)*Mathf.PI)*.28f:Mathf.Exp(-Mathf.Abs(age-.8f)*5)*.22f;
            bool visible=!Riding || rideAge<=.55f || rideAge>=2.1f;
            PoseImpactV41(clouds,spray,impactGlow,origin,age,impact,burst,wall.sortingOrder,visible,lift<.4f);
        }
        void PoseImpactV41(IList<SpriteRenderer> layers,IList<SpriteRenderer> droplets,SpriteRenderer glow,Vector2 origin,float clock,float impact,float burst,int baseOrder,bool visible,bool grounded=true)
        {
            for(int i=0;i<layers.Count;i++)
            {
                int row=i<3?0:i<7?1:2;var cloud=layers[i];cloud.enabled=visible;int local=row==0?i:row==1?i-3:i-7;
                float frameClock=clock*(row==2?9:5)+i*.73f;int frame=Mathf.FloorToInt(frameClock)%4;
                cloud.sprite=game.v38Art.mist[row,frame];
                float wave=Mathf.Sin(clock*1.7f+i*2.31f);
                float width=row==0?3.3f:row==1?3.0f:2.25f;
                width*=1+burst+wave*.045f;
                float x=row==0?(local-1)*2.35f:row==1?(local-1.5f)*1.83f:(local-2)*1.07f;
                float y=row==0?.20f:row==1?-.40f:-.13f;
                cloud.transform.position=origin+new Vector2(x+Mathf.Sin(clock*.8f+i)*.10f,y+wave*.055f);
                float scale=width/cloud.sprite.bounds.size.x;
                cloud.transform.localScale=new Vector3(scale,scale*(row==0?.90f:row==1?.85f:1.25f),1);
                var tint=Color.HSVToRGB(Mathf.Repeat(.02f+(x+3.5f)/8f,1),.24f,1);
                cloud.color=new Color(tint.r,tint.g,tint.b,impact*(row==0?.72f:row==1?.92f:.86f));
                cloud.sortingOrder=baseOrder+(row==0?-1:row==1?4:6);
                game.art.PixelStyle(cloud);var block=new MaterialPropertyBlock();cloud.GetPropertyBlock(block);block.SetFloat("_Phase",clock+i*.53f);
                var next=game.v38Art.mist[row,(frame+1)%4].rect;var texture=cloud.sprite.texture;
                block.SetVector("_NextRect",new Vector4(next.x/texture.width,next.y/texture.height,next.width/texture.width,next.height/texture.height));block.SetFloat("_FrameBlend",Mathf.SmoothStep(0,1,Mathf.Repeat(frameClock,1)));cloud.SetPropertyBlock(block);
            }
            if(glow)
            {
                glow.transform.position=origin;glow.transform.localScale=new Vector3(9.7f,3.2f,1);glow.enabled=visible && grounded;
                glow.sharedMaterial.SetFloat("_Phase",clock);glow.sharedMaterial.SetFloat("_Strength",impact*.44f);glow.sortingOrder=CoastGame.Depth(origin.y)-2;
            }
            for(int i=0;i<droplets.Count;i++)
            {
                float p=Mathf.Repeat(clock*(.55f+i%5*.10f)+i*.381966f,1);
                float seed=Mathf.Sin(i*73.17f),side=seed>0?1:-1;
                float x=seed*2.5f+side*p*(.55f+i%4*.21f);
                float y=-.12f+Mathf.Sin(p*Mathf.PI)*(1.3f+i%7*.15f);
                var dot=droplets[i];dot.enabled=visible;dot.transform.position=origin+new Vector2(x,y);
                float size=.025f+(i%4)*.017f;dot.transform.localScale=new Vector3(size,size*(1.25f+Mathf.Abs(.5f-p)*2),1);
                var color=Color.Lerp(Color.white,Color.HSVToRGB(Mathf.Repeat((x+3.4f)/7,1),.65f,1),.4f);
                color.a=impact*Mathf.Sin(p*Mathf.PI)*.85f;dot.color=color;dot.sortingOrder=baseOrder+5;
            }
        }

        void ReparentRainbowVisuals()
        {
            foreach(var cloud in clouds)cloud.transform.SetParent(game.arcane.WorldRoot,true);
            foreach(var dot in spray)dot.transform.SetParent(game.arcane.WorldRoot,true);
            if(impactGlow)impactGlow.transform.SetParent(game.arcane.WorldRoot,true);
        }
        void ClearRainbowVisuals()
        {
            if(wall){Destroy(wall.sharedMaterial);Destroy(wall.gameObject);}wall=null;
            foreach(var cloud in clouds)if(cloud)Destroy(cloud.gameObject);clouds.Clear();
            foreach(var dot in spray)if(dot)Destroy(dot.gameObject);spray.Clear();
            if(impactGlow){Destroy(impactGlow.sharedMaterial);Destroy(impactGlow.gameObject);}impactGlow=null;
            if(mistMaterial)Destroy(mistMaterial);mistMaterial=null;
        }
        public int AppendRainbowLights(Vector4[] points,Vector4[] colors,int count)
        {
            if(!wall || !wall.gameObject.activeInHierarchy || Riding)return count;
            for(int i=0;i<5 && count<points.Length;i++)
            {
                var color=Color.Lerp(Color.HSVToRGB(i*.17f,.58f,1),Color.white,.18f);
                points[count]=new Vector4(Position.x+(i-2)*1.1f,Position.y+.12f,4.2f,ImpactStrength*(.20f+.024f*Mathf.Sin(age*7+i)));
                colors[count++]=new Vector4(color.r,color.g,color.b,1);
            }
            return count;
        }
        void SpectrumGroundProperties(GroundLoot drop,bool grounded)
        {
            game.art.PixelStyle(drop.renderer);var block=new MaterialPropertyBlock();drop.renderer.GetPropertyBlock(block);
            block.SetFloat("_Spectrum",1);block.SetFloat("_SpectrumPlanted",1);block.SetFloat("_SpectrumGround",grounded?1:0);block.SetVector("_SpectrumRopeRect",game.v38Art.ropeRect);
            block.SetFloat("_SpectrumClock",game.seconds+drop.position.x*.37f);block.SetFloat("_Emission",.60f);
            block.SetFloat("_GroundClip",grounded?1:0);block.SetFloat("_GroundY",drop.position.y+.025f);drop.renderer.SetPropertyBlock(block);
        }
        public void PoseThrownLoot(GroundLoot drop,float t,Vector2 position)
        {
            drop.renderer.sprite=game.v38Art.planted;
            drop.renderer.transform.localScale=Vector3.one*(1.02f/drop.renderer.sprite.bounds.size.y);
            float angle=Mathf.Lerp(drop.position.x<drop.thrownFrom.x?75:-75,0,Mathf.SmoothStep(0,1,t));
            drop.renderer.transform.SetPositionAndRotation(position-Vector2.up*(.12f*t),Quaternion.Euler(0,0,angle));
            drop.renderer.sortingOrder=CoastGame.Depth(Vector2.Lerp(drop.thrownFrom,drop.position,t).y)+2;
            SpectrumGroundProperties(drop,false);
        }
        void PoseGroundLootV38(GroundLoot drop,float dt)
        {
            if(drop.item.kind!=ItemKind.Spectrum)return;
            if(drop.spectrumGlow)drop.spectrumGlow.enabled=!drop.collected && !drop.InFlight && drop.renderer.enabled;
            if(drop.collected || drop.InFlight)return;
            drop.groundAge+=dt;drop.renderer.sprite=game.v38Art.planted;
            drop.renderer.transform.SetPositionAndRotation(drop.position,Quaternion.identity);
            drop.renderer.transform.localScale=Vector3.one*(1.02f/drop.renderer.sprite.bounds.size.y);
            drop.renderer.sortingOrder=CoastGame.Depth(drop.position.y)+1;
            drop.shadow.transform.position=drop.position;drop.shadow.color=new Color(.04f,.04f,.03f,.24f);
            SpectrumGroundProperties(drop,true);
            if(!drop.spectrumGlow)
            {
                drop.spectrumGlow=game.art.Renderer("Spectrum iridescent ground reflection",game.v38Art.quad,drop.position,drop.renderer.sortingOrder-4);
                drop.spectrumGlow.transform.SetParent(drop.renderer.transform.parent,true);drop.spectrumGlow.sharedMaterial=new Material(Shader.Find("AshenCoast/RainbowGround"));
            }
            drop.spectrumGlow.transform.position=drop.position;drop.spectrumGlow.transform.localScale=new Vector3(4.8f,2.1f,1);
            drop.spectrumGlow.sharedMaterial.SetFloat("_Phase",game.seconds);drop.spectrumGlow.sharedMaterial.SetFloat("_HueShift",game.seconds*.48f);drop.spectrumGlow.sharedMaterial.SetFloat("_Saturation",1);
            drop.spectrumGlow.sharedMaterial.SetFloat("_Strength",.31f+.05f*Mathf.Sin(game.seconds*7));
            if(!drop.spectrumLanded)
            {
                drop.spectrumLanded=true;
                for(int i=0;i<9;i++)
                {
                    var spark=game.art.Renderer("Spectrum landing grit",game.v37Art.mote,drop.position,drop.renderer.sortingOrder+2);
                    spark.transform.SetParent(drop.renderer.transform.parent,true);spark.sharedMaterial=game.v37Art.glowMaterial;
                    spark.transform.localScale=Vector3.one*.035f;spark.color=new Color(.72f,.68f,.48f,.8f);
                    landingSparks.Add(new LandingSpark{sprite=spark,origin=drop.position,seed=i});
                }
            }
        }
        void TickLandingSparks(float dt)
        {
            for(int i=landingSparks.Count-1;i>=0;i--)
            {
                var p=landingSparks[i];p.age+=dt;
                if(p.age>.65f){if(p.sprite)Destroy(p.sprite.gameObject);landingSparks.RemoveAt(i);continue;}
                float t=p.age/.65f,angle=p.seed*2.399963f;
                p.sprite.transform.position=p.origin+new Vector2(Mathf.Cos(angle)*t*.43f,Mathf.Sin(angle)*t*.12f+Mathf.Sin(t*Mathf.PI)*.24f);
                p.sprite.transform.localScale=Vector3.one*(.035f*(1-t));p.sprite.color=new Color(.72f,.68f,.48f,1-t);
            }
        }
    }
}
