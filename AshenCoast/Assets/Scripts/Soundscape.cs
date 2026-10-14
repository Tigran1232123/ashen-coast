using UnityEngine;

namespace AshenCoast
{
    public sealed class Soundscape : MonoBehaviour
    {
        public AudioSource cello,cosmos,gulls,battle;
        public readonly AudioClip[] gullCalls=new AudioClip[2];
        public int gullCount;
        CoastGame game;
        float clock,nextGull=8;
        public void Initialize(CoastGame owner)
        {
            game=owner;cello=Loop("Beach cello lament","BeachCelloV9");cosmos=Loop("Pocket celestial choir","PocketCosmosV9");
            battle=Loop("Rooster-dragon violin battle","DragonViolinV28");battle.Stop();
            gulls=gameObject.AddComponent<AudioSource>();gulls.spatialBlend=0;
            gullCalls[0]=Resources.Load<AudioClip>("Audio/SeagullSquawksV34");gullCalls[1]=Resources.Load<AudioClip>("Audio/SeagullFlightV34");
        }
        public void GullFlight(){if(game.ShoreAudibility>.05f){gulls.volume=game.ShoreAudibility;gulls.PlayOneShot(gullCalls[1],.25f);}}
        AudioSource Loop(string name,string clip)
        {
            var source=gameObject.AddComponent<AudioSource>();source.clip=Resources.Load<AudioClip>("Audio/"+clip);source.loop=true;source.volume=0;source.spatialBlend=0;source.Play();return source;
        }
        void Update(){if(game==null || game.testMode)return;Tick(Time.deltaTime);}
        public void Tick(float dt)
        {
            if(game.paused){cello.volume=Mathf.MoveTowards(cello.volume,0,dt*.25f);cosmos.volume=Mathf.MoveTowards(cosmos.volume,0,dt*.25f);battle.volume=Mathf.MoveTowards(battle.volume,0,dt*.4f);return;}
            clock+=dt;bool pocket=game.magic.InPocket;gulls.volume=game.ShoreAudibility;if(gulls.volume<=0 && gulls.isPlaying)gulls.Stop();
            bool village=game.amazons!=null && game.amazons.InTerritory;bool fighting=game.dragons!=null && game.dragons.InBattle;
            if(fighting && !battle.isPlaying){battle.time=0;battle.Play();}
            battle.volume=Mathf.MoveTowards(battle.volume,fighting?.62f:0,dt*.65f);
            if(!fighting && battle.volume<.001f && battle.isPlaying)battle.Stop();
            cello.volume=Mathf.MoveTowards(cello.volume,village || fighting || pocket || !game.journey.IsCoast?0:.26f*game.ShoreAudibility,dt*.45f);
            cosmos.volume=Mathf.MoveTowards(cosmos.volume,fighting?0:pocket?.40f:0,dt*.45f);
            if(!pocket && game.journey.IsCoast && game.ShoreAudibility>.05f && CoastTime.Rain(game.seconds)<.35f && clock>=nextGull)
            {
                gulls.panStereo=Mathf.Sin(clock*.29f)*.65f;gulls.PlayOneShot(gullCalls[gullCount%2],.18f);gullCount++;
                nextGull=clock+9+Mathf.Repeat(gullCount*7.13f,10);
            }
        }
    }
}
