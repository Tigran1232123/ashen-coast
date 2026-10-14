using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed partial class NightHunter : MonoBehaviour,ICombatTarget
    {
        public const float RunSpeed=11.5f,LampDistance=7.4f,ProfileLength=8.53f;
        public const string Whisper="это не будет длиться вечно";
        public const float QuoteDuration=4,QuoteInterval=15,StandDelay=5,ScratchCycle=3.8f;
        public float IdleSeconds {get;private set;}
        public bool Scratching=>Active && IdleSeconds>=StandDelay && Mathf.Repeat(IdleSeconds-StandDelay,ScratchCycle)<1.5f;
        public static readonly Color InkDark=new Color(.028f,.006f,.040f),InkLight=new Color(.065f,.011f,.086f);
        public static Color WhisperInk(float time,int letter,float alpha)
        {var c=Color.Lerp(InkDark,InkLight,.5f+.5f*Mathf.Sin(time*3.4f-letter*.52f));c.a=alpha;return c;}
        public bool QuoteVisible=>Active && QuoteAge<QuoteDuration;
        public int QuoteCount {get;private set;}
        public Vector2 HeadTop=>ArtPoint(view==0?new Vector2(.5f,.64f):view==1?new Vector2(.19f,.67f):new Vector2(.27f,.83f));
        public Vector2 ArtPoint(Vector2 uv)
        {if(sprite.flipX)uv.x=1-uv.x;var b=sprite.sprite.bounds;return sprite.transform.TransformPoint(new Vector3(b.min.x+b.size.x*uv.x,b.min.y+b.size.y*uv.y,0));}
        public CoastGame game;
        public ThreatArt art;
        public SpriteRenderer sprite,shadow;
        public MeshRenderer skin;
        public AudioSource footsteps;
        public bool Active=>sprite && sprite.gameObject.activeInHierarchy && Alive && game.journey.Area==WorldArea.GoldenField && !game.magic.InPocket && CoastTime.IsNight(game.DaySeconds);
        public bool Alive=>health>0;
        public bool HeldBack {get;private set;}
        public bool Running {get;private set;}
        public bool LampHeld=>game.survivor.equipment[0]?.kind==ItemKind.Lamp && game.survivor.equipment[0].Usable && game.survivor.equipment[0].fuel>0;
        public Vector2 Position=>(Vector2)sprite.transform.position;
        public SpriteRenderer Renderer=>sprite;
        public float health=2000,clock,distance,stare,QuoteAge=10;
        public Vector2 QuoteAnchor {get;internal set;}
        public int frame,view,FootstepCount;
        public const int MovementFrames=144;
        public bool GaitMoving {get;internal set;}
        public float MotionBlend {get;private set;}
        public bool Retreating {get;private set;}
        public int MotionFrame=>Mathf.FloorToInt(Mathf.Repeat(distance/3.6f,1)*MovementFrames)%MovementFrames;
        public static readonly Vector2[][] LegPivots={
            new[]{new Vector2(.28f,.33f),new Vector2(.73f,.33f),new Vector2(.12f,.40f),new Vector2(.88f,.40f)},
            new[]{new Vector2(.31f,.39f),new Vector2(.43f,.32f),new Vector2(.65f,.40f),new Vector2(.58f,.30f)},
            new[]{new Vector2(.18f,.46f),new Vector2(.35f,.55f),new Vector2(.58f,.39f),new Vector2(.34f,.48f)}};
        public static Vector4 LegWeights(int view,Vector2 uv)
        {
            float w0=PawWeight(view,uv),w1=0,w2=0,w3=0;
            float Fade(float y,float top,float span)=>Mathf.SmoothStep(0,1,Mathf.Clamp01((top-y)/span));
            if(view==0){if(uv.x>.56f && uv.x<.89f)w1=Fade(uv.y,.33f,.20f);if(uv.x<.14f)w2=Fade(uv.y,.39f,.16f);if(uv.x>.87f)w3=Fade(uv.y,.39f,.16f);}
            if(view==1){if(uv.x>.38f && uv.x<.53f)w1=Fade(uv.y,.30f,.14f);if(uv.x>.64f && uv.x<.91f)w2=Fade(uv.y,.39f,.19f);if(uv.x>.52f && uv.x<.65f)w3=Fade(uv.y,.29f,.13f);}
            if(view==2){if(uv.x>.31f && uv.x<.46f)w1=Fade(uv.y,.52f,.20f);if(uv.x>.46f && uv.x<.77f)w2=Fade(uv.y,.39f,.20f);if(uv.x>.23f && uv.x<.35f)w3=Fade(uv.y,.45f,.13f);}
            var weights=new Vector4(w0,w1,w2,w3);return weights/Mathf.Max(1,w0+w1+w2+w3);
        }
        public static Vector3 LegMotion(int view,int leg,int phase)
        {
            float t=(phase/(float)MovementFrames+(leg==1 || leg==2?.5f:0))*Mathf.PI*2;
            float swing=Mathf.Cos(t),lift=Mathf.Pow(Mathf.Max(0,Mathf.Sin(t)),2);
            var forward=view==1?Vector2.left:view==0?Vector2.down:Vector2.up;
            var move=forward*(swing*(view==1?.43f:.22f))+Vector2.up*(lift*.30f);
            return new Vector3(move.x,move.y,swing*(view==1?.14f:.075f));
        }
        int night=-1;
        float footDistance,quoteCooldown;
        static readonly float[] turns={0,25,-25,50,-50,80,-80,110,-110};
        static readonly Vector2[] spawnPoints={new Vector2(-18,17),new Vector2(18,18),new Vector2(-18,-8),new Vector2(20,-11)};
        readonly RaycastHit2D[] castHits=new RaycastHit2D[32];
        readonly ContactFilter2D wallFilter=new ContactFilter2D{useTriggers=false};
        LightZone[] lightZones=new LightZone[16];int lightCount;
        bool hasSpoken;
        Vector2 lastPlayer;
        internal Vector2 look=Vector2.down;
        HunterPresentation presentation;
        AudioClip footfall;

        GameObject scrapeRoot;
        Sprite dustSprite;
        Texture2D dustTexture;
        Material earthMaterial;
        readonly List<ScrapeDust> dust=new List<ScrapeDust>();
        readonly List<ScrapeMark> marks=new List<ScrapeMark>();
        float dustBudget;
        int dustCursor,markCursor,lastStroke=-1;
        public int ScrapeCount {get;private set;}
        sealed class ScrapeDust{public SpriteRenderer sprite;public Vector2 velocity;public float age=10,life=.7f;}
        sealed class ScrapeMark{public LineRenderer line;public float age=10;}
        internal Vector2 TailPivot=>view==0?new Vector2(.52f,.74f):view==1?new Vector2(.68f,.58f):new Vector2(.56f,.5f);
        internal Vector2 PawPivot=>view==0?new Vector2(.28f,.33f):view==1?new Vector2(.31f,.39f):new Vector2(.18f,.46f);
        Vector2 PawContact=>view==0?new Vector2(.27f,.065f):view==1?new Vector2(.14f,.185f):new Vector2(.12f,.185f);
        public void Initialize(CoastGame owner,ThreatArt pictures)
        {
            game=owner;art=pictures;sprite=game.art.Renderer("Night axolotl / full body animation",art.axolotl[0,0],new Vector2(-14,17),2000);
            sprite.transform.SetParent(game.journey.fieldRoot.transform,true);sprite.forceRenderingOff=true;
            var surface=new GameObject("Living gills and painted running poses");surface.transform.SetParent(sprite.transform,false);
            surface.AddComponent<MeshFilter>();
            skin=surface.AddComponent<MeshRenderer>();skin.sharedMaterial=new Material(Shader.Find("AshenCoast/Axolotl"));
            shadow=game.Shadow("Giant night hunter contact shadow",Position,new Vector2(5.3f,1.4f),.42f);shadow.transform.SetParent(sprite.transform,true);
            footsteps=gameObject.AddComponent<AudioSource>();footsteps.spatialBlend=0;footfall=CreateFootfall();
            presentation=new HunterPresentation(this);gameObject.AddComponent<HunterWhisperView>().Initialize(this);InitializeScrapes();Pose();sprite.gameObject.SetActive(false);lastPlayer=game.body.position;
        }
        void Update(){if(game==null || game.testMode || game.paused)return;game.TickCoopActorV49(Position,()=>Tick(Time.deltaTime));}
        public void Tick(float dt)
        {
            if(!Alive && deathAge<11.5f){TickDeath(dt);return;}
            clock+=dt;QuoteAge+=dt;quoteCooldown=Mathf.Max(0,quoteCooldown-dt);
            bool outdoors=game.journey.Area==WorldArea.GoldenField && !game.magic.InPocket;
            bool dark=CoastTime.IsNight(game.DaySeconds);
            int cycle=Mathf.FloorToInt(game.DaySeconds/360);
            if(!outdoors || !dark){ClearAggression();sprite.gameObject.SetActive(false);HeldBack=Running=GaitMoving=false;MotionBlend=0;stare=0;IdleSeconds=0;ResetScrapes();footsteps.Stop();lastPlayer=game.body.position;return;}
            if(night!=cycle){night=cycle;health=2000;RestoreLiving();ChooseSpawn();}
            sprite.gameObject.SetActive(Alive);
            attackAge=attackAge<0?-1:attackAge+dt;if(attackAge>=.7f)attackAge=-1;
            if(!Alive || game.Dead || game.journey.Cutscene){Running=GaitMoving=false;IdleSeconds=0;ResetScrapes();if(Alive)Pose();return;}
            if(game.spellcraft!=null && game.spellcraft.ImmobilizedV44(sprite)){Running=GaitMoving=false;Pose();return;}
            ChooseVictim();bool playerTarget=victim.Renderer==game.hero;
            var toward=victim.Position-Position;float targetRange=toward.magnitude;if(game.spellcraft!=null && game.spellcraft.Vision(sprite)<1 && targetRange>12){Running=GaitMoving=false;Pose();return;}look=toward.sqrMagnitude>.01f?toward.normalized:look;
            var playerDirection=(game.body.position-Position).normalized;float range=Vector2.Distance(game.body.position,Position);
            UpdateLightZones();HeldBack=false;Vector2 seek=look;float speed=RunSpeed;
            if(LampHeld && (playerTarget || range<LampDistance+.35f))
            {HeldBack=true;if(range<LampDistance-.15f){seek=-playerDirection;speed=6.8f;}else if(range>LampDistance+.35f){seek=playerDirection;speed=3.7f;}else speed=0;}
            else
            {
                for(int k=0;k<lightCount;k++)
                {var light=lightZones[k];float d=Vector2.Distance(Position,light.point);if(d<light.radius+.4f){seek=(Position-light.point).normalized;speed=6.8f;HeldBack=true;break;}if(Vector2.Distance(victim.Position,light.point)<light.radius && targetRange<light.radius+2.5f){speed=0;HeldBack=true;}}
            }
            Vector2 step=Step(seek,HeldBack?speed*dt:Mathf.Min(speed*dt,Mathf.Max(0,targetRange-1)));
            // The personal lamp is a hard exclusion radius, including a large frame step.
            if(LampHeld && range>=LampDistance-.15f && Vector2.Distance(Position+step,game.body.position)<LampDistance-.15f)
                step=Vector2.zero;
            for(int k=0;k<lightCount;k++){var light=lightZones[k];if(Vector2.Distance(Position,light.point)>=light.radius && Vector2.Distance(Position+step,light.point)<light.radius)step=Vector2.zero;}
            Retreating=step.sqrMagnitude>.00001f && Vector2.Dot(step,look)<0;
            sprite.transform.position+=(Vector3)step;distance+=step.magnitude*(Retreating?-1:1);footDistance+=step.magnitude;Running=step.magnitude>dt*4;
            if(Running)while(footDistance>.90f){footDistance-=.90f;FootstepCount++;footsteps.pitch=.87f+(FootstepCount%4)*.085f;footsteps.panStereo=Mathf.Clamp((Position.x-game.body.position.x)/18,-.75f,.75f);footsteps.PlayOneShot(footfall,Mathf.Clamp01(1-range/27)*.36f);}
            else footDistance=0;
            IdleSeconds=step.sqrMagnitude>.00000001f?0:IdleSeconds+dt;
            GaitMoving=step.sqrMagnitude>.00000001f;MotionBlend=Mathf.MoveTowards(MotionBlend,GaitMoving?1:0,dt*5);frame=GaitMoving?MotionFrame/18:0;
            AttackVictim(Position-step,dt);
            bool stationary=(lastPlayer-game.body.position).sqrMagnitude<.0009f && game.movement.sqrMagnitude<.01f;
            bool waiting=playerTarget && LampHeld && stationary && range>=6.7f && range<10.2f && speed==0;
            if(waiting && quoteCooldown<=0){stare+=dt;if(hasSpoken || stare>=StandDelay)Speak();}
            else if(!waiting)stare=Mathf.Max(0,stare-dt*2);
            lastPlayer=game.body.position;Pose();TickScrapes(dt);
        }
        struct LightZone{public Vector2 point;public float radius;public LightZone(Vector2 p,float r){point=p;radius=r;}}
        void UpdateLightZones()
        {
            int needed=game.arcane.campfires.Count+1;if(needed>lightZones.Length)System.Array.Resize(ref lightZones,Mathf.NextPowerOfTwo(needed));
            lightCount=1;lightZones[0]=new LightZone(game.journey.InnEntrance+Vector2.up*.5f,5.6f);
            foreach(var f in game.arcane.campfires)if(!f.exploded && f.logs.gameObject.activeInHierarchy)lightZones[lightCount++]=new LightZone(f.position,5.4f);
        }
        void ChooseSpawn()
        {
            Vector2 chosen=spawnPoints[0];float far=-1;
            foreach(var point in spawnPoints){float d=(point-game.body.position).sqrMagnitude;if(d>far){far=d;chosen=point;}}
            night=Mathf.FloorToInt(game.DaySeconds/360);Warp(chosen);stare=0;QuoteAge=10;quoteCooldown=0;hasSpoken=false;
        }
        public void Warp(Vector2 point){sprite.transform.position=point;distance=footDistance=0;frame=0;GaitMoving=false;MotionBlend=0;IdleSeconds=0;ResetScrapes();Pose();}
        Vector2 Step(Vector2 seek,float length)
        {
            if(length<=0)return Vector2.zero;
            foreach(float angle in turns)
            {
                var dir=(Vector2)(Quaternion.Euler(0,0,angle)*seek);var next=game.journey.Clamp(Position+dir*length);var d=next-Position;
                if(d.sqrMagnitude<.00001f)continue;
                int hits=Physics2D.CircleCast(Position+Vector2.up*.1f,.8f,d.normalized,wallFilter,castHits,Mathf.Max(.9f,d.magnitude));
                bool wall=hits==castHits.Length;
                for(int i=0;i<hits;i++)if(castHits[i].collider.gameObject!=game.hero.gameObject){wall=true;break;}
                if(!wall)return d;
            }
            return Vector2.zero;
        }
        public void Pose()=>presentation.Pose();
        public float Hurt(float amount)=>HurtFrom(amount,game.hero);
        // The face and body remain rigid. Only the outer branches of the gills have weight.
        public static float GillWeight(int view,Vector2 p)
        {
            float w=0;
            if(view==0)
            {
                if(p.y>.43f && p.y<.79f && (p.x<.25f || p.x>.76f))
                    w=Mathf.SmoothStep(0,1,Mathf.Clamp01((Mathf.Abs(p.x-.505f)-.25f)/.17f));
                if(p.y>.67f && p.y<.83f && ((p.x>.14f && p.x<.30f)||(p.x>.73f && p.x<.88f)))
                    w=Mathf.Max(w,Mathf.SmoothStep(0,1,(p.y-.67f)/.12f));
            }
            else if(view==1)
            {
                if(p.x>.29f && p.x<.51f && p.y>.33f && p.y<.87f)
                {
                    float edge=p.y>.68f?Mathf.Max((p.y-.68f)/.14f,(p.x-.32f)/.17f):(p.x-.33f)/.15f;
                    w=Mathf.SmoothStep(0,1,Mathf.Clamp01(edge));
                    if(p.y>.43f && p.y<.62f && p.x<.345f)w=0;
                }
            }
            else
            {
                if(p.y>.67f && p.y<.98f && p.x<.60f)
                {
                    if(p.x<.19f)w=Mathf.SmoothStep(0,1,(.19f-p.x)/.14f);
                    if(p.y>.85f)w=Mathf.Max(w,Mathf.SmoothStep(0,1,(p.y-.85f)/.12f));
                    if(p.x>.42f && p.y>.78f)w=Mathf.Max(w,Mathf.SmoothStep(0,1,(p.x-.42f)/.13f));
                }
            }
            return Mathf.Clamp01(w);
        }
        public static float TailWeight(int view,Vector2 p)
        {
            if(GillWeight(view,p)>.001f)return 0;
            if(view==0 && p.x>.38f && p.x<.91f && p.y>.75f)
                return Mathf.SmoothStep(0,1,Mathf.Clamp01((p.y-.75f)/.20f));
            if(view==1 && p.x>.69f && p.y>.46f && p.y<.88f)
                return Mathf.SmoothStep(0,1,Mathf.Clamp01((p.x-.69f)/.25f));
            if(view==2 && p.x>.57f && p.y<.60f)
                return Mathf.SmoothStep(0,1,Mathf.Clamp01((p.x-.57f)/.35f));
            return 0;
        }
        public static float PawWeight(int view,Vector2 p)
        {
            if(view==0 && p.x>.11f && p.x<.44f && p.y<.33f)
            {
                if(p.y>.29f && p.x>.28f)return 0;
                return Mathf.SmoothStep(0,1,Mathf.Clamp01((.33f-p.y)/.20f));
            }
            if(view==1 && p.x<.38f && p.y<.39f)
                return Mathf.SmoothStep(0,1,Mathf.Clamp01((.39f-p.y)/.20f))*Mathf.Clamp01((.38f-p.x)/.065f);
            if(view==2 && p.x<.27f && p.y<.46f)
                return Mathf.SmoothStep(0,1,Mathf.Clamp01((.46f-p.y)/.22f))*Mathf.Clamp01((.27f-p.x)/.07f);
            return 0;
        }
        public static Vector3 PawMotion(float standing,int view)
        {
            if(standing<StandDelay)return Vector3.zero;
            float phase=Mathf.Repeat(standing-StandDelay,ScratchCycle);
            if(phase>=1.5f)return Vector3.zero;
            float t=Mathf.Repeat(phase,.75f);
            Vector2 forward=view==1?Vector2.left:view==0?Vector2.down:Vector2.up;
            float angle=view==1?-.12f:view==0?-.06f:.10f;
            var lifted=forward*.08f+Vector2.up*.25f;
            var reach=forward*.26f+Vector2.up*.17f;
            var plant=forward*.30f;
            var rake=-forward*.18f;
            Vector3 Key(Vector2 offset,float rotation)=>new Vector3(offset.x,offset.y,rotation);
            Vector3 Blend(Vector3 a,Vector3 b,float start,float end)=>Vector3.Lerp(a,b,Mathf.SmoothStep(0,1,Mathf.InverseLerp(start,end,t)));
            if(t<.16f)return Blend(Vector3.zero,Key(lifted,angle),0,.16f);
            if(t<.35f)return Blend(Key(lifted,angle),Key(reach,angle*.6f),.16f,.35f);
            if(t<.43f)return Blend(Key(reach,angle*.6f),Key(plant,0),.35f,.43f);
            if(t<.67f)return Blend(Key(plant,0),Key(rake,0),.43f,.67f);
            return Blend(Key(rake,0),Vector3.zero,.67f,.75f);
        }
        Vector2 GroundPaw()
        {
            var bounds=sprite.sprite.bounds;Vector2 Local(Vector2 uv)=>new Vector2(bounds.min.x+bounds.size.x*uv.x,bounds.min.y+bounds.size.y*uv.y);
            var pivot=Local(PawPivot);var foot=Local(PawContact);var motion=PawMotion(IdleSeconds,view);var d=foot-pivot;
            float c=Mathf.Cos(motion.z),s=Mathf.Sin(motion.z);
            foot=pivot+new Vector2(d.x*c-d.y*s,d.x*s+d.y*c)+new Vector2(motion.x,motion.y);
            if(sprite.flipX)foot.x=-foot.x;
            return sprite.transform.TransformPoint(foot);
        }
        void InitializeScrapes()
        {
            scrapeRoot=new GameObject("One forepaw / earth scrapes and dust");scrapeRoot.transform.SetParent(game.journey.fieldRoot.transform,false);
            dustTexture=new Texture2D(4,4,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};
            for(int y=0;y<4;y++)for(int x=0;x<4;x++)dustTexture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(1-(new Vector2(x-1.5f,y-1.5f)).magnitude/2)));
            dustTexture.Apply();dustSprite=Sprite.Create(dustTexture,new Rect(0,0,4,4),new Vector2(.5f,.5f),40);
            earthMaterial=new Material(Shader.Find("Sprites/Default"));
            for(int i=0;i<32;i++)
            {
                var sr=game.art.Renderer("Earth grain "+i,dustSprite,Position,2000);sr.transform.SetParent(scrapeRoot.transform,true);sr.sharedMaterial=earthMaterial;sr.enabled=false;
                dust.Add(new ScrapeDust{sprite=sr});
            }
            for(int i=0;i<4;i++)
            {
                var obj=new GameObject("Claw furrow "+i);obj.transform.SetParent(scrapeRoot.transform,false);var line=obj.AddComponent<LineRenderer>();
                line.sharedMaterial=earthMaterial;line.useWorldSpace=true;line.positionCount=2;line.startWidth=line.endWidth=.018f;line.enabled=false;
                marks.Add(new ScrapeMark{line=line});
            }
            ResetScrapes();
        }
        void ResetScrapes()
        {
            if(scrapeRoot)scrapeRoot.SetActive(false);dustBudget=0;lastStroke=-1;
            foreach(var p in dust){p.age=10;p.sprite.enabled=false;}foreach(var m in marks){m.age=10;m.line.enabled=false;}
        }
        void TickScrapes(float dt)
        {
            scrapeRoot.SetActive(Active);
            foreach(var p in dust)
            {
                p.age+=dt;if(p.age>=p.life){p.sprite.enabled=false;continue;}
                p.sprite.transform.position+=(Vector3)(p.velocity*dt);p.velocity*=Mathf.Exp(-dt*2.5f);
                p.sprite.color=new Color(.22f,.15f,.17f,(1-p.age/p.life)*.55f);
            }
            foreach(var m in marks)
            {m.age+=dt;if(m.age>=3){m.line.enabled=false;continue;}var c=new Color(.025f,.018f,.030f,(1-m.age/3)*.7f);m.line.startColor=m.line.endColor=c;}
            if(!Scratching){dustBudget=0;lastStroke=-1;return;}
            float phase=Mathf.Repeat(IdleSeconds-StandDelay,ScratchCycle),t=Mathf.Repeat(phase,.75f);
            if(t<.43f || t>.67f)return;
            var forward=view==1?(sprite.flipX?Vector2.right:Vector2.left):view==0?Vector2.down:Vector2.up;
            var contact=GroundPaw();int stroke=Mathf.FloorToInt((IdleSeconds-StandDelay)/ScratchCycle)*2+(phase>=.75f?1:0);
            if(stroke!=lastStroke)
            {
                lastStroke=stroke;ScrapeCount++;var m=marks[markCursor++%marks.Count];m.age=0;m.line.enabled=true;m.line.sortingOrder=skin.sortingOrder-1;
                m.line.SetPosition(0,contact+forward*.16f);m.line.SetPosition(1,contact-forward*.30f);
            }
            dustBudget+=dt*32;
            while(dustBudget>=1)
            {
                dustBudget-=1;var p=dust[dustCursor++%dust.Count];p.age=0;p.life=.6f+(dustCursor%5)*.07f;p.sprite.enabled=true;p.sprite.sortingOrder=skin.sortingOrder+1;
                float side=Mathf.Sin(dustCursor*2.39f);var sideways=new Vector2(-forward.y,forward.x);
                p.sprite.transform.position=contact+sideways*side*.09f;p.sprite.transform.localScale=Vector3.one*(.3f+(dustCursor%4)*.14f);
                p.velocity=forward*(.20f+(dustCursor%3)*.11f)+sideways*side*.18f+Vector2.up*.24f;p.sprite.color=new Color(.22f,.15f,.17f,.55f);
            }
        }
        public void Speak()
        {
            QuoteAge=0;quoteCooldown=QuoteInterval;hasSpoken=true;stare=0;QuoteCount++;
            QuoteAnchor=HeadTop+Vector2.up*.48f;
        }

        static AudioClip CreateFootfall()
        {
            const int rate=22050;var samples=new float[3307];var random=new System.Random(512);float smooth=0;
            for(int i=0;i<samples.Length;i++){float t=i/(float)rate,n=(float)random.NextDouble()*2-1;smooth=Mathf.Lerp(smooth,n,.2f);float e=Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/.15f))*Mathf.Exp(-t*20);samples[i]=(smooth*.75f+n*.11f+Mathf.Sin(t*(220-t*400))*.3f)*e;}
            var clip=AudioClip.Create("Rapid clawed feet on dry grass",samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        void OnDestroy(){if(dustSprite)Destroy(dustSprite);if(dustTexture)Destroy(dustTexture);if(earthMaterial)Destroy(earthMaterial);if(scrapeRoot)Destroy(scrapeRoot);presentation?.Dispose();if(footfall)Destroy(footfall);}
    }
}
