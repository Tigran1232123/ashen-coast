using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    [Serializable] public sealed class CampStateV51 {public int phase;public float gather,x,y;public Vector2[] path;}
    public sealed partial class SnailHomesV50
    {
        public int campPhaseV51=4;public float gatherV51;public Vector2 campCentreV51;float nextRestV51,castFireV51;
        public Vector2[] MapRouteV51=Array.Empty<Vector2>();public readonly List<SpriteRenderer> logSeatsV51=new List<SpriteRenderer>();
        Sprite[,] seatedV51;Sprite logSpriteV51;CampfireObject campFireV51;AudioSource fluteSourceV51;AudioClip fluteV51;
        readonly float[] seatReadyV51=new float[4];
        public Vector2 CampSlotV51(int i)=>CampSlotV53(i);
        Vector2 LogPointV51(int i)=>campCentreV51+(CampSlotV51(i)-campCentreV51).normalized*2.8f;
        public void InitializeCampV51()
        {
            campCentreV51=game.journey.CartPosition+new Vector2(4,18);for(int i=0;i<4;i++){houses[i].position=FindLand(CampSlotV51(i));houses[i].Renderer.flipX=houses[i].position.x>campCentreV51.x;}
            var t=game.art.Load("CampPeopleV51");seatedV51=new Sprite[4,4];for(int row=0;row<4;row++)for(int col=0;col<4;col++)seatedV51[row,col]=game.art.Slice(t,new RectInt(col*t.width/4,row*t.height/4,t.width/4,t.height/4),new Vector2(.5f,0),200);
            var log=game.art.Load("CampLogV51");logSpriteV51=game.art.Slice(log,new RectInt(0,0,log.width,log.height),new Vector2(.5f,.3f),200);
            for(int i=0;i<4;i++){var r=game.art.Renderer("Camp log seat "+i,logSpriteV51,LogPointV51(i),0);r.transform.SetParent(game.coastRoot.transform,true);r.transform.localScale=Vector3.one*(1.85f/logSpriteV51.bounds.size.x);logSeatsV51.Add(r);}
            const int rate=22050;float[] notes={659.25f,783.99f,880,987.77f,880,783.99f,659.25f,587.33f,659.25f,783.99f,659.25f,523.25f};var samples=new float[rate*8];
            for(int i=0;i<samples.Length;i++){float time=i/(float)rate,local=Mathf.Repeat(time,.6666667f)/.6666667f,frequency=notes[Mathf.Min(notes.Length-1,(int)(time/ .6666667f))];float envelope=Mathf.Min(1,local*12)*Mathf.Pow(1-local,.32f);float wave=Mathf.Sin(time*frequency*2*Mathf.PI+Mathf.Sin(time*31)*.035f)+.12f*Mathf.Sin(time*frequency*4*Mathf.PI);samples[i]=wave*envelope*.13f;}
            fluteV51=AudioClip.Create("Captive wooden flute melody",samples.Length,1,rate,false);fluteV51.SetData(samples,0);fluteSourceV51=game.gameObject.AddComponent<AudioSource>();fluteSourceV51.clip=fluteV51;fluteSourceV51.loop=true;fluteSourceV51.playOnAwake=false;
        }
        public void BeginGatherV51()
        {
            campPhaseV51=1;gatherV51=60;castFireV51=0;
            if(campFireV51!=null){campFireV51.exploded=true;campFireV51.logs.enabled=false;foreach(var f in campFireV51.flames)if(f)f.enabled=false;if(campFireV51.collider)campFireV51.collider.enabled=false;campFireV51=null;}
            foreach(var t in troops.Values.Where(Living).ToArray()){t.campRestV51=false;t.seatedV51=false;SendHome(t.key,OwnerPeer(t));}
            MapRouteV51=new[]{houses[0].position}.Concat(route).Concat(new[]{destination}).ToArray();
        }
        public bool CampTickV51(float dt)
        {
            if(campPhaseV51==1)
            {
                gatherV51=Mathf.Max(0,gatherV51-dt);foreach(var h in houses)h.moving=false;
                if(gatherV51<=0){foreach(var t in troops.Values.Where(Living)){t.campRestV51=false;t.returning=false;t.resting=!houses[t.home].Hidden;if(!t.resting)PlaceOutside(t);}campPhaseV51=2;}return true;
            }
            if(stopped>0 || houses.Any(h=>h.Hidden || h.WithdrawalV51>.02f)){foreach(var h in houses)h.moving=false;return true;}
            if(campPhaseV51==2)
            {foreach(var t in troops.Values.Where(Living)){t.returning=false;t.resting=true;}if(houses.All(h=>Vector2.Distance(h.position,destination)<.35f)){campCentreV51=destination;campPhaseV51=3;PrepareRoutesV53(destination,true);}return campPhaseV51!=2;}
            if(campPhaseV51==3)
            {
                bool ready=true;for(int i=0;i<4;i++){var h=houses[i];TravelV53(h,CampSlotV51(i),CoastGame.WalkSpeed*1.5f,dt);if(Vector2.Distance(h.position,CampSlotV51(i))>.25f)ready=false;}
                if(ready){campPhaseV51=4;ordered=false;nextRestV51=clock+4;for(int i=0;i<4;i++)houses[i].Renderer.flipX=houses[i].position.x>campCentreV51.x;}
                return true;
            }
            if(campPhaseV51==4)
            {
                foreach(var h in houses)h.moving=false;
                if(troops.TryGetValue("mage",out var mage) && Living(mage) && campFireV51==null && !mage.campRestV51 && (mage.resting || Sprite(mage.key).transform.IsChildOf(game.coastRoot.transform) && Vector2.Distance(Sprite(mage.key).transform.position,campCentreV51)<15) && clock>=nextRestV51)
                {StartCampRestV51(mage,true);nextRestV51=clock+8;}
                if(clock>=nextRestV51)
                {
                    nextRestV51=clock+UnityEngine.Random.Range(7,15);
                    var available=troops.Values.Where(t=>Living(t) && !t.campRestV51 && (t.resting || Sprite(t.key) && Sprite(t.key).transform.IsChildOf(game.coastRoot.transform) && Vector2.Distance(Sprite(t.key).transform.position,campCentreV51)<15) && seatReadyV51[t.home]<clock && !troops.Values.Any(other=>other.home==t.home && other.campRestV51)).ToArray();
                    if(available.Length>0)StartCampRestV51(available[UnityEngine.Random.Range(0,available.Length)],false);
                }
                return true;
            }
            return false;
        }
        void StartCampRestV51(TroopStateV50 t,bool maker)
        {
            t.resting=t.returning=false;t.campRestV51=true;t.seatedV51=false;t.campTimeV51=60;t.makerV51=maker;t.fluteV51=!maker && Woman(t.key)?.IsSlave==true && UnityEngine.Random.value<.5f;PlaceOutside(t);
        }
        public void TickCampTroopV51(TroopStateV50 t,float dt)
        {
            var sprite=Sprite(t.key);if(!sprite)return;var goal=t.makerV51?campCentreV51+Vector2.left*.8f:LogPointV51(t.home)+Vector2.up*.08f;
            if(campPhaseV51!=4 || houses[t.home].Hidden){t.campRestV51=t.seatedV51=false;SendHome(t.key,OwnerPeer(t));return;}
            if(!t.seatedV51)
            {
                if(Vector2.Distance(sprite.transform.position,goal)>.20f){if(t.key=="mage")game.journey.companion.ReturnHomeV50(goal,dt);else Woman(t.key).ReturnHomeV50(goal,dt);return;}t.seatedV51=true;
            }
            if(t.makerV51)
            {
                castFireV51+=dt;sprite.sprite=game.journey.art.mage[0,Mathf.Min(7,(int)(castFireV51*5)%8)];sprite.transform.localScale=Vector3.one;sprite.sortingOrder=CoastGame.Depth(goal.y);game.art.PixelStyle(sprite);
                if(castFireV51>=1.2f){CreateCampFireV51();t.makerV51=false;t.seatedV51=false;castFireV51=0;}return;
            }
            PoseCampTroopV51(t);t.campTimeV51-=dt;
            if(t.campTimeV51<=0){t.campRestV51=t.seatedV51=false;seatReadyV51[t.home]=clock+15;SendHome(t.key,OwnerPeer(t));}
        }
        void CreateCampFireV51()
        {
            // Fire mechanics stay on the coast even while the host is visiting a room.
            campFireV51=game.arcane.CreateCampfire(campCentreV51);campFireV51.logs.transform.SetParent(game.coastRoot.transform,true);foreach(var flame in campFireV51.flames)flame.transform.SetParent(game.coastRoot.transform,true);if(campFireV51.collider)campFireV51.collider.transform.SetParent(game.coastRoot.transform,true);
        }
        void PoseCampTroopV51(TroopStateV50 t)
        {
            var sprite=Sprite(t.key);if(!sprite)return;int row=t.key=="mage"?3:Woman(t.key)?.IsSlave==true?t.fluteV51?0:1:2;int frame=(int)(clock*(t.fluteV51?5:2)+t.home)%4;
            sprite.sprite=seatedV51[row,frame];sprite.transform.localScale=Vector3.one*(1.3f/sprite.sprite.bounds.size.y);sprite.transform.position=LogPointV51(t.home);sprite.flipX=t.home==1 || t.home==2;sprite.sortingOrder=CoastGame.Depth(sprite.transform.position.y)+3;game.art.PixelStyle(sprite);
            if(t.key=="mage"){game.journey.companion.gait.Lower.enabled=false;}else if(Woman(t.key)!=null){Woman(t.key).weaponSprite.enabled=false;Woman(t.key).gaitV53.Lower.enabled=false;var block=new MaterialPropertyBlock();sprite.GetPropertyBlock(block);block.SetFloat("_BodyFloor",0);sprite.SetPropertyBlock(block);}
        }
        public void CampVisualsV51()
        {
            bool active=campPhaseV51==4;
            for(int i=0;i<4;i++){var r=logSeatsV51[i];r.enabled=active;r.transform.position=LogPointV51(i);r.sortingOrder=CoastGame.Depth(r.transform.position.y)-1;r.transform.rotation=Quaternion.Euler(0,0,i%2==0?-12:12);}
            foreach(var t in troops.Values.Where(t=>t.campRestV51 && t.seatedV51 && !t.makerV51))PoseCampTroopV51(t);
            var musician=troops.Values.FirstOrDefault(t=>t.campRestV51 && t.seatedV51 && t.fluteV51);bool audible=musician!=null && game.journey.IsCoast && !game.magic.InPocket && Vector2.Distance(game.body.position,LogPointV51(musician.home))<18;
            if(audible){fluteSourceV51.volume=Mathf.Lerp(.45f,0,Vector2.Distance(game.body.position,LogPointV51(musician.home))/18);if(!fluteSourceV51.isPlaying)fluteSourceV51.Play();}else if(fluteSourceV51.isPlaying)fluteSourceV51.Stop();
        }
        public CampStateV51 CaptureCampV51()=>new CampStateV51{phase=campPhaseV51,gather=gatherV51,x=campCentreV51.x,y=campCentreV51.y,path=MapRouteV51};
        public void ApplyCampV51(CampStateV51 state){if(state==null)return;campPhaseV51=state.phase;gatherV51=state.gather;campCentreV51=new Vector2(state.x,state.y);MapRouteV51=state.path??Array.Empty<Vector2>();}
    }
    public sealed partial class ExplorationMap
    {
        void DrawCaravanRouteV51()
        {
            if(Region!=MapRegion.Coast || game.snails==null || !game.snails.ordered)return;var points=game.snails.MapRouteV51;
            for(int i=1;i<points.Length;i++){var a=MapPoint(points[i-1]);var b=MapPoint(points[i]);int steps=Mathf.Min(300,Mathf.CeilToInt(Vector2.Distance(a,b)/5));for(int k=0;k<=steps;k++){var p=Vector2.Lerp(a,b,steps>0?k/(float)steps:0);Fill(new Rect(p.x-1,p.y-1,3,3),new Color(.83f,.68f,.38f,.85f));}}
        }
    }
}
