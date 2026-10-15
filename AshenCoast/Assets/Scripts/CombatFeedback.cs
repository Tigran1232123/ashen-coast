using System.Collections.Generic;
using UnityEngine;
namespace AshenCoast
{
    public sealed class CombatFeedback
    {
        readonly CoastGame game;
        readonly Dictionary<SpriteRenderer,float> hits=new Dictionary<SpriteRenderer,float>();
        readonly HashSet<SpriteRenderer> struck=new HashSet<SpriteRenderer>();
        public bool WasHit(SpriteRenderer actor)=>actor && struck.Contains(actor);
        public void Forget(SpriteRenderer actor){hits.Remove(actor);struck.Remove(actor);}
        readonly AudioSource[] voices=new AudioSource[6];
        public readonly AudioClip PlayerImpact,MobImpact,ShellImpact;
        int voice;
        public int PlayerHits {get;private set;}
        public int MobHits {get;private set;}
        public CombatFeedback(CoastGame owner)
        {
            game=owner;PlayerImpact=Make("Player hit: cloth, body thud and short breath",0);
            MobImpact=Make("Creature hit: blade contact and flesh thud",1);ShellImpact=Make("Armoured shell impact",2);
            for(int i=0;i<voices.Length;i++){voices[i]=game.gameObject.AddComponent<AudioSource>();voices[i].spatialBlend=0;voices[i].playOnAwake=false;}
        }
        public float Flash(SpriteRenderer actor)=>actor && hits.TryGetValue(actor,out var time)?Mathf.Clamp01(1-(Time.unscaledTime-time)/.48f):0;
        public bool Recent(SpriteRenderer actor)=>actor && hits.TryGetValue(actor,out var time) && Time.unscaledTime-time<.6f;
        public AudioSource LastVoice=>voices[(voice+voices.Length-1)%voices.Length];
        public void Hit(SpriteRenderer actor,float damage,bool player=false,bool shell=false)
        {
            if(damage<=0 || !actor)return;hits[actor]=Time.unscaledTime;struck.Add(actor);
            if(!player && (!actor.gameObject.activeInHierarchy || !actor.transform.IsChildOf(game.arcane.WorldRoot) || Vector2.Distance(game.coop.LocalBody.position,actor.transform.position)>17))return;
            var source=voices[voice];voice=(voice+1)%voices.Length;source.Stop();
            bool prism=!player && game.prism!=null && game.prism.ApplyingDamage;
            source.clip=prism?game.prism.Audio.Impact:player?PlayerImpact:shell?ShellImpact:MobImpact;source.pitch=.94f+.035f*(voice%4);
            float distance=Vector2.Distance(game.coop.LocalBody.position,actor.transform.position);source.volume=player?.60f:Mathf.Lerp(.46f,.10f,Mathf.Clamp01(distance/15));source.Play();
            if(player)PlayerHits++;else MobHits++;
        }
        static AudioClip Make(string name,int kind)
        {
            const int rate=22050;float duration=kind==0?.30f:.22f;var samples=new float[(int)(rate*duration)];
            var random=new System.Random(8041+kind);float low=0;
            for(int i=0;i<samples.Length;i++)
            {
                float t=i/(float)rate,n=(float)random.NextDouble()*2-1;low=Mathf.Lerp(low,n,.085f);
                float attack=Mathf.Min(1,t/.002f),thud=Mathf.Sin(2*Mathf.PI*(kind==0?86:118)*t)*Mathf.Exp(-t*24);
                float snap=(n*.25f+low*.8f)*Mathf.Exp(-t*70);
                float breath=kind==0?low*.65f*Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/duration))*Mathf.Exp(-t*7):0;
                float metal=kind==2?(Mathf.Sin(t*4860)+Mathf.Sin(t*7010)*.4f)*.22f*Mathf.Exp(-t*21):0;
                samples[i]=Mathf.Clamp((thud*.6f+snap+breath+metal)*attack,-.95f,.95f);
            }
            var clip=AudioClip.Create(name,samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        public void Dispose(){Object.Destroy(PlayerImpact);Object.Destroy(MobImpact);Object.Destroy(ShellImpact);hits.Clear();struck.Clear();}
    }
}
