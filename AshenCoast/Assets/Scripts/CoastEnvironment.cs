using UnityEngine;
namespace AshenCoast
{
    public sealed class CoastEnvironment
    {
        readonly CoastGame game;int lastThunder=-1;
        readonly Vector4[] lightPositions=new Vector4[32],lightColors=new Vector4[32];
        public CoastEnvironment(CoastGame owner){game=owner;}
        public void Tick(float dt)
        {
            if(game.magic!=null && game.magic.InPocket)
            {
                Shader.SetGlobalColor("_CoastAmbient",new Color(.76f,.70f,.64f));Shader.SetGlobalFloat("_CoastClock",0);
                lightPositions[0]=new Vector4(0,4.15f,7.8f,1.8f);lightColors[0]=new Vector4(.08f,.42f,1,1);
                lightPositions[1]=new Vector4(-.55f,4.4f,2.4f,.63f);lightColors[1]=new Vector4(.05f,.37f,1,1);
                lightPositions[2]=new Vector4(.55f,4.4f,2.4f,.63f);lightColors[2]=lightColors[1];
                int pocketLights=game.AppendOrbLights(lightPositions,lightColors,3);pocketLights=game.arcane.AppendLights(lightPositions,lightColors,pocketLights);if(game.journey!=null)pocketLights=game.journey.AppendLights(lightPositions,lightColors,pocketLights);
                if(game.prism!=null)pocketLights=game.prism.AppendLights(lightPositions,lightColors,pocketLights);if(game.dragons!=null)pocketLights=game.dragons.AppendLights(lightPositions,lightColors,pocketLights);
                Shader.SetGlobalVectorArray("_CoastLights",lightPositions);Shader.SetGlobalVectorArray("_CoastLightColors",lightColors);Shader.SetGlobalInt("_CoastLightCount",pocketLights);game.flash=0;
                if(game.oceanAudio)game.oceanAudio.volume=0;if(game.rainAudio)game.rainAudio.volume=0;return;
            }
            float day=CoastTime.Daylight(game.DaySeconds), storm=CoastTime.Rain(game.seconds);
            if(game.journey!=null && !game.journey.IsCoast)storm=0;
            Shader.SetGlobalFloat("_WindTime",game.seconds);
            Color nightAmbient=game.journey.Area==WorldArea.GoldenField?new Color(.075f,.095f,.145f):new Color(.15f,.20f,.32f);
            Color ambient=Color.Lerp(nightAmbient,new Color(1,.97f,.84f),day);
            ambient*=Mathf.Lerp(1,.63f,storm);
            game.flash=0;
            var times=CoastGame.LightningTimes;
            for(int i=0;i<times.Length && (game.journey==null || game.journey.IsCoast);i++)
            {
                float age=game.seconds-times[i];
                if(game.seconds>=times[i]-1 && game.seconds<times[i] && i>lastThunder){lastThunder=i;game.lastThunderAt=game.seconds;game.sounds.PlayOneShot(game.thunder,.6f);}
                if(age>=0 && age<.44f)
                {
                    game.flash=Mathf.Max(game.flash,Mathf.Exp(-age*17)+.65f*Mathf.Exp(-Mathf.Abs(age-.20f)*36));
                }
            }
            ambient=Color.Lerp(ambient,new Color(1.7f,1.8f,2),game.flash*.65f);
            if(game.journey!=null && (game.journey.Interior || game.journey.Area==WorldArea.AmazonHouse))ambient=new Color(.72f,.65f,.52f);
            if(game.journey.InCatacombs)ambient=new Color(.42f,.42f,.48f);
            if(game.journey.InFort)ambient=new Color(.36f,.39f,.43f);
            Shader.SetGlobalColor("_CoastAmbient",ambient); Shader.SetGlobalFloat("_CoastClock",game.seconds);
            Shader.SetGlobalFloat("_StormWind",storm);
            Shader.SetGlobalFloat("_Daylight",day); Shader.SetGlobalFloat("_ClearSky",1-storm);
            int count=game.AppendOrbLights(lightPositions,lightColors,0);if(game.arcane!=null)count=game.arcane.AppendLights(lightPositions,lightColors,count);
            if(game.journey!=null)count=game.journey.AppendLights(lightPositions,lightColors,count);
            if(game.fort!=null)count=game.fort.AppendLights(lightPositions,lightColors,count);
            if(game.cave!=null)count=game.cave.AppendLights(lightPositions,lightColors,count);
            if(game.prism!=null)count=game.prism.AppendLights(lightPositions,lightColors,count);
            if(game.dragons!=null)count=game.dragons.AppendLights(lightPositions,lightColors,count);
            bool night=CoastTime.IsNight(game.DaySeconds);
            for(int i=0;i<game.fireflies.Count;i++)
            {
                var f=game.fireflies[i];
                Vector2 p=f.home+new Vector2(Mathf.Sin(game.seconds*.71f+f.seed)*1.1f+Mathf.Sin(game.seconds*.29f+f.seed)*.4f,Mathf.Cos(game.seconds*.61f+f.seed)*.61f);
                float flicker=(.60f+.24f*Mathf.Sin(game.seconds*6.8f+f.seed)+.16f*Mathf.Sin(game.seconds*13+f.seed*4))*CoastGame.FireflyVisibility(game.DaySeconds,f.procedural?i%32:i);
                f.dot.enabled=night && flicker>.001f && (game.journey==null || game.journey.IsCoast); f.dot.transform.position=p+new Vector2(0,.28f);
                f.dot.color=new Color(.90f,1,.45f,flicker);
                if(f.dot.enabled && count<32 && Vector2.Distance(game.hero.transform.position,p)<20) { lightPositions[count]=new Vector4(p.x,p.y,1.5f,flicker); lightColors[count]=new Vector4(.54f,.59f,.20f,1); count++; }
            }
            Shader.SetGlobalInt("_CoastLightCount",count); Shader.SetGlobalVectorArray("_CoastLights",lightPositions); Shader.SetGlobalVectorArray("_CoastLightColors",lightColors);
            if(game.oceanAudio) game.oceanAudio.volume=.15f*game.ShoreAudibility;
            if(game.rainAudio) game.rainAudio.volume=storm*.21f;
        }
    }
}
