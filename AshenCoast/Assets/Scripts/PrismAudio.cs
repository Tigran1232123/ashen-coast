using UnityEngine;
namespace AshenCoast
{
    public sealed class PrismAudio
    {
        public readonly AudioClip LaserLoop,Impact;
        public readonly AudioSource Source;
        public PrismAudio(CoastGame game)
        {
            LaserLoop=MakeLoop();Impact=MakeImpact();Source=game.gameObject.AddComponent<AudioSource>();
            Source.playOnAwake=false;Source.loop=true;Source.spatialBlend=0;Source.clip=LaserLoop;Source.volume=0;
        }
        public void Tick(bool firing,float charge,float dt)
        {
            if(firing && !Source.isPlaying)Source.Play();
            Source.pitch=Mathf.Lerp(1.12f,.78f,charge)+Mathf.Sin(Time.unscaledTime*7)*.018f;
            Source.volume=Mathf.MoveTowards(Source.volume,firing?Mathf.Lerp(.19f,.29f,charge):0,dt*1.7f);
            if(!firing && Source.volume<.001f)Source.Stop();
        }
        public void Silence(){if(Source){Source.volume=0;Source.Stop();}}
        public void Dispose(){Silence();Object.Destroy(LaserLoop);Object.Destroy(Impact);}
        static AudioClip MakeLoop()
        {
            const int rate=44100,frames=rate*2;var data=new float[frames*2];
            for(int i=0;i<frames;i++)for(int channel=0;channel<2;channel++)
            {
                float t=i/(float)rate,pan=channel*.21f,phase=t*2*Mathf.PI;
                float carrier=Mathf.Sin(phase*194+1.3f*Mathf.Sin(phase*43+pan));
                float edge=Mathf.Sin(phase*997+.55f*Mathf.Sin(phase*17+pan));
                float hiss=(Mathf.Sin(phase*2017+pan)+Mathf.Sin(phase*3089-pan))*.035f;
                float pulse=.80f+.10f*Mathf.Sin(phase*7)+.07f*Mathf.Sin(phase*13+pan);
                data[i*2+channel]=(carrier*.31f+edge*.12f+hiss)*pulse;
            }
            var clip=AudioClip.Create("Last Prism continuous laser",frames,2,rate,false);clip.SetData(data,0);return clip;
        }
        static AudioClip MakeImpact()
        {
            const int rate=44100;var data=new float[(int)(rate*.23f)];
            for(int i=0;i<data.Length;i++)
            {
                float t=i/(float)rate,phase=2*Mathf.PI*(2200*t-2600*t*t);
                float snap=Mathf.Sin(phase+Mathf.Sin(t*470)*.70f)*Mathf.Exp(-t*25);
                float throb=Mathf.Sin(t*2*Mathf.PI*95)*Mathf.Exp(-t*19);
                float glitter=Mathf.Sin(t*2*Mathf.PI*3713)*Mathf.Exp(-t*34);
                data[i]=Mathf.Clamp((snap*.55f+throb*.28f+glitter*.12f)*Mathf.Min(1,t/.002f),-.95f,.95f);
            }
            var clip=AudioClip.Create("Last Prism energy impact",data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
    }
}
