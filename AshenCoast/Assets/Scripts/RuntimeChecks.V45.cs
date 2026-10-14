using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        void ZenDistortionPixelsV45()
        {
            const int size=256;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};var pixels=new Color32[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++){byte shade=(byte)(35+(x*47+y*13)%140);pixels[y*size+x]=new Color32(shade,(byte)(shade*.83f),(byte)(shade*.62f),255);}texture.SetPixels32(pixels);texture.Apply();
            var material=new Material(Shader.Find("AshenCoast/Atmosphere"));material.SetVector("_PixelSize",new Vector4(size,size,1f/size,1f/size));material.SetVector("_FogWorldView",new Vector4(0,0,6,6));material.SetVector("_HeroView",new Vector4(-10,-10,.1f,.1f));material.SetVectorArray("_ZenWells",new[]{new Vector4(0,0,CoastGame.ZenWellRadiusV45,.74f),Vector4.zero,Vector4.zero,Vector4.zero,Vector4.zero,Vector4.zero,Vector4.zero,Vector4.zero});
            var target=new RenderTexture(size,size,0,RenderTextureFormat.ARGB32){filterMode=FilterMode.Point};target.Create();
            Texture2D Read(int count,float phase){var prior=RenderTexture.active;material.SetInt("_ZenWellCount",count);material.SetFloat("_EffectClock",phase);Graphics.Blit(texture,target,material);RenderTexture.active=target;var image=new Texture2D(size,size,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,size,size),0,0);image.Apply();RenderTexture.active=prior;return image;}
            var baseline=Read(0,.5f);var first=Read(1,.5f);var next=Read(1,1.5f);var a=baseline.GetPixels32();var b=first.GetPixels32();var c=next.GetPixels32();int changed=0,outside=0,animated=0;
            for(int y=0;y<size;y++)for(int x=0;x<size;x++){int n=y*size+x;float radius=new Vector2((x+.5f)/size-.5f,(y+.5f)/size-.5f).magnitude*6;if(!a[n].Equals(b[n])){changed++;if(radius>CoastGame.ZenWellRadiusV45+.05f)outside++;}if(!b[n].Equals(c[n]))animated++;}
            Check(changed>30 && outside==0,"Zen refraction changes actual scene pixels only inside its small radius; changed="+changed+" outside="+outside);
            Check(animated>30,"ground Zen refraction animates over time; changed="+animated);
            material.SetVector("_HeroView",new Vector4(.5f,.5f,.75f/6,1.05f/6));var nearHero=Read(1,.5f);var d=nearHero.GetPixels32();Check(d.Where((p,n)=>!p.Equals(a[n])).Count()>30,"Zen ripple remains visible when the player stands next to the planted weapon");Destroy(nearHero);
            File.WriteAllBytes(Path.Combine(directory,"v45-local-distortion-baseline.png"),baseline.EncodeToPNG());File.WriteAllBytes(Path.Combine(directory,"v45-local-distortion-first.png"),first.EncodeToPNG());File.WriteAllBytes(Path.Combine(directory,"v45-local-distortion-next.png"),next.EncodeToPNG());
            Destroy(baseline);Destroy(first);Destroy(next);Destroy(texture);Destroy(material);target.Release();Destroy(target);
        }
        IEnumerator ChangesV45()
        {
            var g=game;var s=g.survivor;var combat=g.combat;HandClearV34();s.health=100;s.hunger=100;s.inventory.items.Clear();s.spatialInventory.items.Clear();g.journey.SetArea(WorldArea.Coast,new Vector2(10,15));g.seconds=60;g.SetAutoPickup(false);
            foreach(var drop in g.loot)if(drop.item.kind==ItemKind.Zen){drop.collected=true;drop.renderer.enabled=drop.shadow.enabled=false;}
            foreach(var frog in g.arcane.frogs)frog.Warp(new Vector2(25,25));
            var spear=new Item(ItemKind.GoldenSpear);s.inventory.Place(spear,0,0);var zen=new Item(ItemKind.Zen);s.inventory.Place(zen,2,0);g.ui.open=true;yield return DrawUIV34();yield return Capture("v45-reference-spear-and-zen-inventory.png");HandClearV34();
            s.equipment[0]=spear;combat.ResetForCheck();Check(combat.Begin(Vector2.right),"reference-shaped spear begins the same functional thrust");Check(Mathf.Abs(combat.weapon.transform.localScale.x-combat.weapon.transform.localScale.y)<.001f,"reference spear is uniformly scaled without stretching in combat");combat.Advance(.42f);ViewV34(g.body.position+new Vector2(1,.8f),3);yield return Capture("v45-reference-spear-thrust.png");combat.ResetForCheck();s.equipment[0]=null;
            var ground=g.ScatterLoot(new Item(ItemKind.Zen),g.body.position+new Vector2(.3f,-1),g.body.position);g.TickLoot(.7f);g.PoseGroundZenV44(ground);var planted=ground.renderer.transform.position;
            Check(Mathf.Abs(Mathf.DeltaAngle(ground.renderer.transform.eulerAngles.z,30))<.001f && Mathf.Abs(ground.position.y-planted.y-.16f)<.001f,"Zen rests30 degrees left with its tip planted0.16 units below the floor");
            var block=new MaterialPropertyBlock();ground.renderer.GetPropertyBlock(block);Check(block.GetFloat("_GroundRopes")==1 && Mathf.Abs(block.GetFloat("_GroundY")-ground.position.y)<.001f,"planted Zen material clips the submerged blade exactly at the floor");
            var r=ground.renderer.bounds;var hidden=Enumerable.Range(0,96*96).Select(i=>new Vector2(Mathf.Lerp(r.min.x,r.max.x,(i%96+.5f)/96),Mathf.Lerp(r.min.y,r.max.y,(i/96+.5f)/96))).Where(p=>p.y<ground.position.y-.02f && g.art.Opaque(ground.renderer.sprite,ground.renderer.transform.InverseTransformPoint(p))).ToArray();
            Check(hidden.Length>0 && hidden.All(p=>!g.GroundPixelV45(ground,p)),"buried blade pixels exist but are invisible and cannot be hovered or picked through the floor");
            var wells=new Vector4[8];Check(g.GroundZenWellsV45(wells,ground.position)==1 && Vector2.Distance(wells[0],ground.renderer.bounds.center)<.001f,"landed Zen registers a local distortion centered on the visible weapon");
            ViewV34(ground.position+Vector2.up*.9f,2.4f);g.UpdateEnvironment(0);yield return Capture("v45-zen-tilted-and-buried.png");yield return new WaitForSecondsRealtime(.25f);yield return Capture("v45-zen-space-ripple-next.png");
            var at=ground.position;g.seconds=61;g.PoseGroundZenV44(ground);Check(ground.renderer.transform.position==planted && ground.position==at,"rope and spatial animations never move the planted weapon");
            ground.collected=true;ground.renderer.enabled=false;Check(g.GroundZenWellsV45(wells,at)==0,"picking up Zen immediately removes its spatial distortion");ground.collected=false;ground.renderer.enabled=true;ground.flightAge=0;Check(g.GroundZenWellsV45(wells,at)==0,"flying Zen does not distort the ground");ground.flightAge=ground.flightDuration;g.PoseGroundZenV44(ground);
            ZenDistortionPixelsV45();
            Check(Mathf.Abs(Combat.ZenIntervalV44(100)-.58f/3)<.0001f && Enumerable.Range(0,101).All(h=>1/Combat.ZenIntervalV44(h)<=9.0001f),"Zen is three times faster at full hunger and never exceeds9 attacks per second");
            var victim=g.arcane.frogs[0];victim.Reset();victim.Warp(g.body.position+new Vector2(1,.2f));s.equipment[0]=zen;s.hunger=100;combat.ResetForCheck();float health=victim.health;combat.Begin(Vector2.right);combat.Advance(1);Check(Mathf.Abs(health-victim.health-90)<.001f,"one real Zen sweep deals90 damage, one and a half times its previous60");combat.ResetForCheck();victim.Warp(new Vector2(25,25));
            s.health=100;s.hunger=0;s.equipment[0]=zen;combat.ResetForCheck();combat.manual=false;yield return null;float began=Time.time;int attacks=0;float previous=0,shortest=10;
            while(Time.time-began<.85f){if(!combat.Attacking && Time.time>=combat.readyAt && combat.Begin(Vector2.up)){if(attacks>0)shortest=Mathf.Min(shortest,Time.time-previous);previous=Time.time;attacks++;}yield return null;}
            Check(attacks>=6 && shortest>=1/9f-.002f,"real rapid Zen combat completes repeated animated attacks at the new capped cadence; attacks="+attacks+" min interval="+shortest);
            Check(s.health<=100-attacks*4+.001f && s.health>100-attacks*4-1,"each repeated empty-hunger attack spends exactly4 HP in addition to ordinary starvation");combat.manual=true;combat.ResetForCheck();s.equipment[0]=null;s.health=100;s.hunger=100;g.ui.CloseContainers();
        }
    }
}
