using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public static class SpatialSigil
    {
        // The continuous stroke follows the user's reference, in screen coordinates (Y points down).
        public static readonly Vector2[] Template={new Vector2(.27f,.915f),new Vector2(.90f,.366f),new Vector2(.05f,.102f),new Vector2(.967f,.018f),new Vector2(.283f,.492f),new Vector2(.862f,.984f)};
        public static List<Vector2> Resample(IList<Vector2> source,int count)
        {
            var output=new List<Vector2>();if(source.Count<2)return output;
            float total=0;for(int i=1;i<source.Count;i++)total+=Vector2.Distance(source[i-1],source[i]);
            if(total<.0001f)return output;
            float spacing=total/(count-1),distance=0;int segment=1;output.Add(source[0]);
            for(int k=1;k<count;k++)
            {
                float target=k*spacing;
                while(segment<source.Count-1 && distance+Vector2.Distance(source[segment-1],source[segment])<target)
                {distance+=Vector2.Distance(source[segment-1],source[segment]);segment++;}
                float length=Vector2.Distance(source[segment-1],source[segment]);
                output.Add(Vector2.Lerp(source[segment-1],source[segment],length<.0001f?0:(target-distance)/length));
            }
            return output;
        }
        public static bool Matches(IList<Vector2> stroke)
        {
            if(stroke.Count<8)return false;
            float minX=stroke.Min(p=>p.x),maxX=stroke.Max(p=>p.x),minY=stroke.Min(p=>p.y),maxY=stroke.Max(p=>p.y);
            float w=maxX-minX,h=maxY-minY;
            if(w<55 || h<55 || w/h<.24f || w/h>2.8f)return false;
            var normalized=stroke.Select(p=>new Vector2((p.x-minX)/w,(p.y-minY)/h)).ToList();
            var points=Resample(normalized,96);var reference=Resample(Normalize(Template),96);
            if(points.Count!=96)return false;
            float forward=0,backward=0;
            for(int i=0;i<96;i++){forward+=(points[i]-reference[i]).sqrMagnitude;backward+=(points[i]-reference[95-i]).sqrMagnitude;}
            float error=Mathf.Sqrt(Mathf.Min(forward,backward)/96);
            var corners=new List<Vector2>{normalized[0]};Simplify(normalized,0,normalized.Count-1,.065f,corners);corners.Add(normalized[normalized.Count-1]);
            int crossings=0;
            for(int a=1;a<corners.Count;a++)for(int b=a+2;b<corners.Count;b++)if(Crosses(corners[a-1],corners[a],corners[b-1],corners[b]))crossings++;
            return error<.25f && corners.Count>=4 && crossings>=1 && crossings<=8;
        }
        static List<Vector2> Normalize(IList<Vector2> source)
        {
            float x=source.Min(p=>p.x),y=source.Min(p=>p.y),w=source.Max(p=>p.x)-x,h=source.Max(p=>p.y)-y;
            return source.Select(p=>new Vector2((p.x-x)/w,(p.y-y)/h)).ToList();
        }
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        static bool Crosses(Vector2 a,Vector2 b,Vector2 c,Vector2 d)
        {
            float denominator=Cross(b-a,d-c);if(Mathf.Abs(denominator)<.00001f)return false;
            float t=Cross(c-a,d-c)/denominator,u=Cross(c-a,b-a)/denominator;
            return t>.02f && t<.98f && u>.02f && u<.98f;
        }
        static void Simplify(List<Vector2> p,int first,int last,float tolerance,List<Vector2> result)
        {
            float max=0;int split=-1;var line=p[last]-p[first];float length=line.sqrMagnitude;
            for(int i=first+1;i<last;i++)
            {
                float t=length<.00001f?0:Mathf.Clamp01(Vector2.Dot(p[i]-p[first],line)/length);
                float distance=Vector2.Distance(p[i],p[first]+line*t);if(distance>max){max=distance;split=i;}
            }
            if(max>tolerance && split>first){Simplify(p,first,split,tolerance,result);result.Add(p[split]);Simplify(p,split,last,tolerance,result);}
        }
    }

    public sealed class InkSpark
    {
        public float index,age,life,seed;
        public Vector2 origin,position,previous,velocity,direction;
    }
    public sealed class FixedInkMote { public Vector2 position;public float seed; }
    public static class StarSigil
    {
        public static readonly Vector2[] Template=Create();
        static Vector2[] Create()
        {
            var p=new Vector2[6];int[] order={0,2,4,1,3,0};
            for(int i=0;i<6;i++){float a=(-90+order[i]*72)*Mathf.Deg2Rad;p[i]=Vector2.one*.5f+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.48f;}return p;
        }
        public static bool Matches(IList<Vector2> stroke)
        {
            if(stroke.Count<8)return false;
            float x=stroke.Min(p=>p.x),y=stroke.Min(p=>p.y),w=stroke.Max(p=>p.x)-x,h=stroke.Max(p=>p.y)-y;
            if(w<55 || h<55 || w/h<.4f || w/h>2.2f || Vector2.Distance(stroke[0],stroke[stroke.Count-1])>Mathf.Max(w,h)*.30f)return false;
            var normalized=SpatialSigil.Resample(stroke.Select(p=>new Vector2((p.x-x)/w,(p.y-y)/h)).ToList(),96);
            float best=float.PositiveInfinity;
            for(int shift=0;shift<5;shift++)
            {
                var reference=new List<Vector2>();for(int i=0;i<6;i++)reference.Add(Template[(i+shift)%5]);
                float rx=reference.Min(p=>p.x),ry=reference.Min(p=>p.y),rw=reference.Max(p=>p.x)-rx,rh=reference.Max(p=>p.y)-ry;
                var expected=SpatialSigil.Resample(reference.Select(p=>new Vector2((p.x-rx)/rw,(p.y-ry)/rh)).ToList(),96);
                float f=0,b=0;for(int i=0;i<96;i++){f+=(normalized[i]-expected[i]).sqrMagnitude;b+=(normalized[i]-expected[95-i]).sqrMagnitude;}
                best=Mathf.Min(best,Mathf.Sqrt(Mathf.Min(f,b)/96));
            }
            return best<.20f;
        }
    }
    public struct RibbonSample
    {
        public float distance,arc;
        public Vector2 offset;
    }
    public sealed class ReturnDoor : MonoBehaviour
    {
        public SpatialMagic magic;
        void OnTriggerEnter2D(Collider2D other){TryExit(other);}
        void OnTriggerStay2D(Collider2D other){TryExit(other);}
        void TryExit(Collider2D other){if(magic.InPocket && !magic.game.paused && other.gameObject==magic.game.hero.gameObject && Mathf.Abs(magic.game.body.position.x)<.45f && magic.game.body.position.y>3.8f)magic.ExitThroughDoor();}
    }
    public sealed class SpatialMagic : MonoBehaviour
    {
        public CoastGame game;
        public bool InPocket {get;private set;}
        public bool Drawing {get;private set;}
        public bool Casting=>enterAt>=0;
        public bool Teleporting=>Casting || Time.unscaledTime<arrivalUntil;
        public float AcceptedFade=>successfulInk?Mathf.Clamp01((inkUntil-Time.unscaledTime)/.5f):0;
        public Vector2 TeleportCenter
        {
            get {var v=game.cameraView.WorldToViewportPoint(game.hero.transform.position+new Vector3(0,.72f,0));return new Vector2(v.x*Screen.width,(1-v.y)*Screen.height);}
        }
        public readonly List<Vector2> stroke=new List<Vector2>();
        public bool HasOpenTome=>game.arcane.HasOpenBook(ItemKind.Tome);
        public bool HasOrb=>(game.survivor.equipment[0]?.kind==ItemKind.Orb || game.survivor.equipment[0]?.kind==ItemKind.LastPrism) && game.survivor.equipment[0].Usable;
        public bool SilverInk {get;private set;}
        public bool HasOpenAbyss=>game.arcane.HasOpenBook(ItemKind.AbyssTome);
        public bool LastCastTentacles {get;private set;}
        public GameObject pocket;
        public SpriteRenderer door;
        readonly List<SpriteRenderer> doorwaySparks=new List<SpriteRenderer>();
        public readonly List<SpriteRenderer> ruins=new List<SpriteRenderer>();
        public readonly List<InkSpark> sparks=new List<InkSpark>();
        public readonly List<FixedInkMote> fixedMotes=new List<FixedInkMote>();
        public readonly List<Vector2> innerFlowPoints=new List<Vector2>();
        readonly List<float> pathDistances=new List<float>();
        public float PixelUnit=>Mathf.Max(2,Mathf.Round(Screen.height/225f));
        public float StrokeWidth=>PixelUnit*3;
        public float DetailUnit=>Mathf.Max(1,Mathf.Floor(PixelUnit*.5f));
        public const float InkBodyOpacity=.68f;
        public Texture2D ribbonTexture {get;private set;}
        public Rect RibbonBounds {get;private set;}
        readonly List<Vector2> raster=new List<Vector2>();
        readonly Dictionary<Vector2Int,RibbonSample> ribbonSamples=new Dictionary<Vector2Int,RibbonSample>();
        Color32[] ribbonPixels;
        Vector2Int ribbonMin,ribbonSize;
        float nextRibbonPaint;
        bool paintedSuccess;
        int rasterCount=-1,sparkSerial;
        [NonSerialized] public float? checkClock;
        Vector2 returnPosition;
        float inkUntil,castStarted,enterAt=-1,arrivalUntil;
        bool successfulInk;
        double lastPortal=-10;
        public void Initialize(CoastGame owner)
        {
            game=owner;
            pocket=new GameObject("Private square chamber in the cosmos");
            Quad("Animated deep space, galaxy, ringed planet and meteors",new Vector3(0,0,7),new Vector3(120,100,1),"Cosmos",pocket.transform);
            Quad("Carved stone chamber",new Vector3(0,0,2),new Vector3(10.2f,11.2f,1),"PocketRoom",pocket.transform);
            var placements=new[]{new Vector4(-3.9f,3.65f,1,2.75f),new Vector4(-2.0f,4.02f,0,1.3f),new Vector4(2.20f,3.75f,4,3.25f),new Vector4(4.03f,2.05f,2,2.3f),new Vector4(-4.02f,1.05f,3,3.35f),new Vector4(-3.8f,-1.50f,0,1.6f),new Vector4(4.0f,-.9f,1,2.65f),new Vector4(3.9f,-3.65f,4,2.8f),new Vector4(-2.6f,-4.02f,5,3.05f),new Vector4(.6f,-4.0f,0,1.0f),new Vector4(-4.0f,-3.60f,2,1.6f),new Vector4(3.85f,4.08f,0,1.3f)};
            var bases=new Vector4[16];
            for(int i=0;i<placements.Length;i++)
            {
                var p=placements[i];int kind=(int)p.z;
                var ruin=game.art.Renderer("Ruined column or arch "+i,game.art.ruins[kind],Vector2.zero,0);ruin.transform.SetParent(pocket.transform,true);
                ruin.transform.localScale=Vector3.one*(p.w/ruin.sprite.bounds.size.y);
                p.x=Mathf.Clamp(p.x*.99f,-4.24f+ruin.bounds.size.x*.5f,4.24f-ruin.bounds.size.x*.5f);p.y=Mathf.Clamp(p.y*1.02f,-4.08f,4.10f);
                ruin.transform.position=new Vector2(p.x,p.y);ruin.sortingOrder=CoastGame.Depth(p.y);game.art.PixelStyle(ruin);ruins.Add(ruin);
                bases[i]=new Vector4(p.x,p.y,kind==5?1.15f:.5f,p.w);
                game.art.AddStoneFootprints(ruin);
            }
            var room=pocket.GetComponentsInChildren<MeshRenderer>().First(r=>r.sharedMaterial.shader.name=="AshenCoast/PocketRoom").sharedMaterial;
            room.SetVectorArray("_RuinBases",bases);room.SetInt("_RuinCount",placements.Length);
            door=game.art.Renderer("Ancient blue return doorway",game.art.door,new Vector2(0,3.84f),CoastGame.Depth(3.84f));door.transform.SetParent(pocket.transform,true);
            float scale=3.15f/door.sprite.bounds.size.y;door.transform.localScale=Vector3.one*scale;game.art.PixelStyle(door);
            Quad("Animated doorway blue flame and stepped glow",new Vector3(0,5.02f,-.1f),new Vector3(.88f,2.22f,1),"DoorPortal",pocket.transform);
            var dot=new Texture2D(3,3,TextureFormat.RGBA32,false);dot.filterMode=FilterMode.Point;
            dot.SetPixels(new[]{Color.clear,Color.white,Color.clear,Color.white,Color.white,Color.white,Color.clear,Color.white,Color.clear});dot.Apply();
            var sparkSprite=Sprite.Create(dot,new Rect(0,0,3,3),Vector2.one*.5f,50);
            var glowMaterial=new Material(Shader.Find("Sprites/Default"));
            for(int i=0;i<24;i++){var spark=game.art.Renderer("Door rising blue mote "+i,sparkSprite,Vector2.zero,3700);spark.sharedMaterial=glowMaterial;spark.transform.SetParent(pocket.transform,true);doorwaySparks.Add(spark);}
            var exit=new GameObject("Walk through return doorway");exit.transform.SetParent(pocket.transform);exit.transform.position=new Vector2(0,4.45f);
            var trigger=exit.AddComponent<BoxCollider2D>();trigger.size=new Vector2(1.25f,.60f);trigger.isTrigger=true;exit.AddComponent<ReturnDoor>().magic=this;
            pocket.SetActive(false);
        }
        void Quad(string name,Vector3 position,Vector3 scale,string shader,Transform parent)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Quad);go.name=name;go.transform.SetParent(parent);go.transform.position=position;go.transform.localScale=scale;
            Destroy(go.GetComponent<Collider>());var material=new Material(Shader.Find("AshenCoast/"+shader));
            if(shader=="Cosmos")
            {
                material.mainTexture=Resources.Load<Texture2D>("Art/Cosmos");
                material.SetTexture("_ObjectsTex",Resources.Load<Texture2D>("Art/CosmicSpritesV3"));
                material.SetTexture("_GalaxyTex",Resources.Load<Texture2D>("Art/GalaxyV4"));
                material.SetTexture("_RingsTex",Resources.Load<Texture2D>("Art/PlanetRingV4"));
                var g=game.art.galaxy;var p=game.art.planet;
                material.SetVector("_GalaxyRect",new Vector4(g.rect.x/g.texture.width,g.rect.y/g.texture.height,g.rect.width/g.texture.width,g.rect.height/g.texture.height));
                material.SetVector("_PlanetRect",new Vector4(p.rect.x/p.texture.width,p.rect.y/p.texture.height,p.rect.width/p.texture.width,p.rect.height/p.texture.height));
            }
            if(shader=="PocketRoom")material.SetTexture("_RoomTex",Resources.Load<Texture2D>("Art/PocketFloorV4"));
            var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;
            // The portal must overlay the painted opening while preserving the surrounding arch.
            if(shader=="DoorPortal")renderer.sortingOrder=door.sortingOrder+1;
        }
        void Update()
        {
            float clock=game.testMode && checkClock.HasValue?checkClock.Value:Time.time;
            Shader.SetGlobalFloat("_CosmicTime",clock);
            if(InPocket)for(int i=0;i<doorwaySparks.Count;i++)
            {
                var spark=doorwaySparks[i];float phase=Mathf.Repeat(clock*.19f+i*.173f,1);
                spark.transform.position=new Vector3(Mathf.Sin(i*3.71f+clock*1.4f)*(.27f+phase*.16f),4.06f+phase*2.50f,0);
                spark.transform.localScale=Vector3.one*(.45f+.5f*Mathf.Sin(phase*Mathf.PI));
                spark.color=new Color(.20f+.5f*(i%3)/2,.68f,1,Mathf.Sin(phase*Mathf.PI)*.8f);
            }
            if(game.paused)return;TickSparks(Time.unscaledDeltaTime);
            if(Casting && Time.unscaledTime>=enterAt){enterAt=-1;SwitchDimension();PlayArrival();}
            if(game.testMode)return;
            bool available=!game.MovementLocked && !game.Meditating && !game.arcane.Placing && HasOrb && !Teleporting && !game.ui.IsPointerOver && !game.ui.Dragging && !game.ui.IsCrafting;
            var mouse=new Vector2(Input.mousePosition.x,Screen.height-Input.mousePosition.y);
            if(Input.GetMouseButtonDown(1) && available && !game.journey.Cutscene && !game.journey.HireOffer && !game.buildings.CanInteract(game.CursorWorld) && !game.journey.CanInteractAt(game.CursorWorld) && !game.fort.CanInteractAt(game.CursorWorld) && !game.cave.CanInteractAt(game.CursorWorld) && game.RightClickConsumedFrame!=Time.frameCount && !game.CanOpenShipAt(game.CursorWorld) && !game.CanCollectAt(game.CursorWorld))BeginStroke(mouse);
            if(Drawing)
            {
                if(!available){CancelStroke();return;}
                if(Input.GetMouseButton(1))AddPoint(mouse);
                if(Input.GetMouseButtonUp(1)){AddPoint(mouse);EndStroke();}
            }
        }
        public bool BeginStroke(Vector2 point)
        {
            if(game.MovementLocked || game.Meditating || !HasOrb || game.paused || game.journey.Cutscene || Teleporting || Time.unscaledTimeAsDouble-lastPortal<1.2)return false;
            game.prism?.StopChannel();SilverInk=game.survivor.equipment[0]?.kind==ItemKind.LastPrism;
            stroke.Clear();sparks.Clear();fixedMotes.Clear();innerFlowPoints.Clear();successfulInk=false;LastCastTentacles=false;rasterCount=-1;sparkSerial=0;stroke.Add(point);Drawing=true;inkUntil=float.PositiveInfinity;return true;
        }
        public void AddPoint(Vector2 point)
        {
            if(!Drawing || stroke.Count==0)return;
            var previous=stroke[stroke.Count-1];float distance=Vector2.Distance(previous,point);
            if(distance<2)return;
            int steps=Mathf.Clamp(Mathf.CeilToInt(distance/6),1,100);
            for(int i=1;i<=steps;i++)
            {
                stroke.Add(Vector2.Lerp(previous,point,(float)i/steps));
                if(sparkSerial++%2==0)SpawnSpark(stroke[stroke.Count-1],(point-previous).normalized);
                if(sparkSerial%5==0){if(fixedMotes.Count>=240)fixedMotes.RemoveAt(0);fixedMotes.Add(new FixedInkMote{position=stroke[stroke.Count-1],seed=sparkSerial*.79f});}
            }
            if(stroke.Count>1600){var sampled=SpatialSigil.Resample(stroke,800);stroke.Clear();stroke.AddRange(sampled);}
        }
        public bool EndStroke()
        {
            if(!Drawing)return false;Drawing=false;inkUntil=Time.unscaledTime+1.15f;
            if(game.spellcraft!=null && game.spellcraft.CastGesture(stroke)){successfulInk=true;inkUntil=Time.unscaledTime+.5f;lastPortal=Time.unscaledTimeAsDouble;return true;}
            bool correct=!InPocket && HasOrb && HasOpenTome && StarSigil.Matches(stroke);
            if(correct)
            {
                if(!game.survivor.SpendMana(20)){game.ui.Notice("Недостаточно маны");return false;}
                game.survivor.equipment[0].Wear(.20f);
                game.survivor.WearBook(ItemKind.Tome,.20f);
                successfulInk=true;castStarted=Time.unscaledTime;enterAt=castStarted+.5f;inkUntil=enterAt;
                game.movement=Vector2.zero;game.body.linearVelocity=Vector2.zero;
                lastPortal=Time.unscaledTimeAsDouble;game.PlayPickup();
            }
            return correct;
        }
        public void CancelStroke(){Drawing=false;successfulInk=false;enterAt=-1;arrivalUntil=0;stroke.Clear();sparks.Clear();fixedMotes.Clear();innerFlowPoints.Clear();raster.Clear();ribbonSamples.Clear();rasterCount=-1;inkUntil=0;}
        public bool ExitThroughDoor()
        {
            game.interactions.Pulse(door);
            if(!InPocket || Teleporting)return false;CancelStroke();SwitchDimension();PlayArrival();game.PlayPickup();return true;
        }
        public void PlayArrival(){arrivalUntil=Time.unscaledTime+.26f;}
        void SpawnSpark(Vector2 point,Vector2 direction)
        {
            // Always keep the newest motes: a long stroke must not exhaust the emitter at its start.
            if(sparks.Count>=256)sparks.RemoveAt(0);
            float seed=sparkSerial*.71f;
            float scatter=Mathf.Repeat(Mathf.Sin(seed*17.37f)*4375.83f,1);
            float drift=Mathf.Repeat(Mathf.Sin(seed*23.91f)*7131.21f,1);
            float side=scatter<.5f?-1:1;
            Vector2 normal=new Vector2(-direction.y,direction.x);
            var position=point-direction*PixelUnit*drift*2.4f+normal*side*(StrokeWidth*.5f+DetailUnit*(1+scatter*4));
            sparks.Add(new InkSpark{index=stroke.Count-1,origin=point,position=position,previous=position,direction=direction,seed=seed,
                life=.85f+drift*.4f,velocity=-direction*(PixelUnit*(3+drift*7))+normal*side*PixelUnit*(.6f+scatter*3)});
        }
        public void TickSparks(float dt)
        {
            for(int i=sparks.Count-1;i>=0;i--)
            {
                var spark=sparks[i];spark.age+=dt;if(spark.age>=spark.life || stroke.Count<2){sparks.RemoveAt(i);continue;}
                spark.previous=spark.position;
                var normal=new Vector2(-spark.direction.y,spark.direction.x);
                if(SilverInk)
                {
                    float spin=spark.seed+spark.age*(7+spark.seed%4);
                    spark.position+=spark.velocity*dt*.55f+(normal*Mathf.Cos(spin)+spark.direction*Mathf.Sin(spin))*PixelUnit*dt*4;
                    spark.velocity*=Mathf.Exp(-dt*.7f);continue;
                }
                spark.position+=spark.velocity*dt+normal*Mathf.Sin(spark.age*7+spark.seed)*PixelUnit*dt*2.6f;
                spark.velocity*=Mathf.Exp(-dt*1.15f);
            }
        }
        public void SwitchDimension()
        {
            game.combat.ResetForCheck();game.numbers.Clear();game.ui.CancelInteraction();game.ui.tab=0;
            game.arcane.CancelHeldSkills();
            game.prism?.StopChannel();
            if(!InPocket)returnPosition=game.body.position;
            InPocket=!InPocket;game.journey.SetVisibility(InPocket);pocket.SetActive(InPocket);
            var position=InPocket?new Vector2(0,-1.7f):returnPosition;
            game.body.linearVelocity=Vector2.zero;game.body.position=position;game.hero.transform.position=position;
            if(game.journey.companion.Hired && (!game.CoopOnline || game.coop.world.OwnAlly(game.journey.companion.sprite)))game.journey.companion.Warp(position+new Vector2(-.8f,-.4f));
            game.cameraView.orthographicSize=InPocket?7.8f:8;
            float width=game.cameraView.orthographicSize*2*game.cameraView.aspect;
            float offset=width*(1-game.ui.WorldFraction)/2;
            game.ResetCamera(InPocket?new Vector3(offset,.65f,-20):new Vector3(position.x+offset,position.y+2.2f,-20));
            game.UpdateEnvironment(0);
        }
        void OnGUI()
        {
            if(game==null || Event.current.type!=EventType.Repaint)return;
            GUI.depth=-6;
            if(stroke.Count>1 && (Drawing || Time.unscaledTime<inkUntil))
            {
                float fade=Drawing?1:successfulInk?AcceptedFade:Mathf.Clamp01((inkUntil-Time.unscaledTime)/1.15f);
                if(rasterCount!=stroke.Count)Rasterize();
                float progress=1-fade;
                if(Time.unscaledTime>=nextRibbonPaint || paintedSuccess!=successfulInk)PaintRibbon(progress);
                // One composite avoids repeated alpha blending at neighboring points and crossings.
                if(ribbonTexture && ribbonSamples.Count>0){GUI.color=new Color(1,1,1,fade);GUI.DrawTexture(RibbonBounds,ribbonTexture);GUI.color=Color.white;}
                DrawInnerMotes(fade);
                if(successfulInk)for(int i=0;i<raster.Count;i+=19)
                {
                    Square(SnapDetail(raster[i]+new Vector2((i%3-1)*PixelUnit*4*progress,-PixelUnit*16*progress)),DetailUnit,new Color(.68f,.91f,1,fade));
                }
            }
            foreach(var spark in sparks)
            {
                float fade=Mathf.Pow(Mathf.Clamp01(1-spark.age/spark.life),.75f)*(successfulInk?AcceptedFade:1);
                var p=SnapDetail(spark.position);float size=DetailUnit*(spark.seed%3>1 && spark.age<spark.life*.55f?2:1);
                if(SilverInk)
                {
                    Crystal(p,DetailUnit*(3+spark.seed%2),spark.seed*41+spark.age*110,fade);
                    if(spark.seed%4>2){Square(p+Vector2.right*DetailUnit*3,DetailUnit,new Color(.9f,.94f,1,fade*.6f));Square(p+Vector2.up*DetailUnit*3,DetailUnit,new Color(1,1,1,fade*.5f));}
                    continue;
                }
                Square(SnapDetail(spark.previous),DetailUnit,new Color(.23f,.50f,.95f,fade*.32f));
                Square(p,size+DetailUnit*2,new Color(.17f,.44f,.90f,fade*.11f));
                Square(p,size,new Color(.58f,.86f,1,fade*.94f));
                if(spark.seed%5>3 && spark.age<.48f)
                {Square(p+Vector2.right*DetailUnit,size*.5f,new Color(.36f,.68f,1,fade*.55f));Square(p+Vector2.up*DetailUnit,size*.5f,new Color(.81f,.95f,1,fade*.75f));}
            }
            if(Teleporting)DrawTeleport();
            GUI.color=Color.white;
        }
        void DrawTeleport()
        {
            var center=TeleportCenter;
            float p=Casting?Mathf.Clamp01((Time.unscaledTime-castStarted)/.5f):1-Mathf.Clamp01((arrivalUntil-Time.unscaledTime)/.26f);
            float radius=PixelUnit*(Casting?Mathf.Lerp(17,4,p):Mathf.Lerp(5,21,p));
            float alpha=Casting?Mathf.Sin(p*Mathf.PI*.8f):1-p;
            for(int i=0;i<32;i++)
            {
                float angle=i*Mathf.PI/16+Time.unscaledTime*7;
                var point=Snap(center+new Vector2(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius*.46f-PixelUnit*p*8));
                Square(point,PixelUnit*3,new Color(.16f,.46f,.90f,alpha*.22f));Square(point,PixelUnit,new Color(.59f,.87f,1,alpha));
                if(i%2==0){var ray=Snap(center+new Vector2(Mathf.Sin(i*3.7f)*radius*.6f,PixelUnit*(5-p*27+i%5)));Square(ray,PixelUnit,new Color(.71f,.94f,1,alpha*.82f));}
            }
        }
        Vector2 Snap(Vector2 p)=>new Vector2(Mathf.Round(p.x/PixelUnit)*PixelUnit,Mathf.Round(p.y/PixelUnit)*PixelUnit);
        Vector2 SnapDetail(Vector2 p)=>new Vector2(Mathf.Round(p.x/DetailUnit)*DetailUnit,Mathf.Round(p.y/DetailUnit)*DetailUnit);
        void DrawInnerMotes(float fade)
        {
            innerFlowPoints.Clear();if(pathDistances.Count<2)return;float length=pathDistances[pathDistances.Count-1];
            int count=Mathf.Clamp(Mathf.FloorToInt(length/(PixelUnit*15)),2,64);
            for(int i=0;i<count;i++)
            {
                float distance=Mathf.Repeat(i*length/count+Time.unscaledTime*(50+(i%4)*11),length);int index=pathDistances.BinarySearch(distance);if(index<0)index=~index;index=Mathf.Clamp(index,1,stroke.Count-1);
                float segment=pathDistances[index]-pathDistances[index-1];var p=SnapDetail(Vector2.Lerp(stroke[index-1],stroke[index],segment>.01f?(distance-pathDistances[index-1])/segment:0));
                innerFlowPoints.Add(p);
                if(SilverInk){Crystal(p,DetailUnit*2.6f,i*37+Time.unscaledTime*64,fade*.90f);continue;}
                Square(p,DetailUnit,new Color(.77f,.97f,1,fade*.85f));
                var direction=(stroke[index]-stroke[index-1]).normalized;Square(SnapDetail(p-direction*DetailUnit),DetailUnit,new Color(.42f,.76f,1,fade*.55f));
            }
            foreach(var mote in fixedMotes)
            {
                float glint=.24f+.60f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(Time.unscaledTime*3.2f+mote.seed)),5);
                if(SilverInk)Crystal(SnapDetail(mote.position),DetailUnit*2,45,glint*fade);else Square(SnapDetail(mote.position),DetailUnit,new Color(.35f,.74f,1,glint*fade));
            }
        }
        void Rasterize()
        {
            pathDistances.Clear();pathDistances.Add(0);for(int i=1;i<stroke.Count;i++)pathDistances.Add(pathDistances[i-1]+Vector2.Distance(stroke[i-1],stroke[i]));
            raster.Clear();var visited=new HashSet<Vector2Int>();
            for(int i=1;i<stroke.Count;i++)
            {
                var a=Snap(stroke[i-1])/PixelUnit;var b=Snap(stroke[i])/PixelUnit;
                int steps=Mathf.Max(1,Mathf.RoundToInt(Mathf.Max(Mathf.Abs(b.x-a.x),Mathf.Abs(b.y-a.y))));
                for(int step=0;step<=steps;step++)
                {
                    var p=Vector2.Lerp(a,b,(float)step/steps);var cell=new Vector2Int(Mathf.RoundToInt(p.x),Mathf.RoundToInt(p.y));
                    if(visited.Add(cell))raster.Add((Vector2)cell*PixelUnit);
                }
            }
            rasterCount=stroke.Count;
            BuildRibbon();
        }
        void BuildRibbon()
        {
            ribbonSamples.Clear();float unit=DetailUnit,radius=StrokeWidth*.5f+unit;
            int reach=Mathf.CeilToInt(radius/unit),maxX=Mathf.CeilToInt(Screen.width/unit),maxY=Mathf.CeilToInt(Screen.height/unit);
            var min=new Vector2Int(maxX,maxY);var max=Vector2Int.zero;float arc=0;
            for(int i=0;i<raster.Count;i++)
            {
                if(i>0)arc+=Vector2.Distance(raster[i-1],raster[i]);
                var p=raster[i];int cx=Mathf.FloorToInt(p.x/unit),cy=Mathf.FloorToInt(p.y/unit);
                for(int y=cy-reach;y<=cy+reach;y++)for(int x=cx-reach;x<=cx+reach;x++)
                {
                    if(x<0 || y<0 || x>=maxX || y>=maxY)continue;
                    var offset=new Vector2((x+.5f)*unit-p.x,(y+.5f)*unit-p.y);float distance=offset.magnitude;if(distance>radius)continue;
                    var cell=new Vector2Int(x,y);
                    if(!ribbonSamples.TryGetValue(cell,out var old) || distance<=old.distance)
                        ribbonSamples[cell]=new RibbonSample{distance=distance,offset=offset,arc=arc};
                    min=Vector2Int.Min(min,cell);max=Vector2Int.Max(max,cell);
                }
            }
            if(ribbonSamples.Count==0)return;
            ribbonMin=min;var needed=max-min+Vector2Int.one;
            if(!ribbonTexture || ribbonTexture.width<needed.x || ribbonTexture.height<needed.y)
            {
                int width=Mathf.CeilToInt(Mathf.Max(needed.x,ribbonTexture?ribbonTexture.width:0)/64f)*64;
                int height=Mathf.CeilToInt(Mathf.Max(needed.y,ribbonTexture?ribbonTexture.height:0)/64f)*64;
                if(ribbonTexture)Destroy(ribbonTexture);
                ribbonTexture=new Texture2D(width,height,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
                ribbonPixels=new Color32[width*height];
            }
            ribbonSize=new Vector2Int(ribbonTexture.width,ribbonTexture.height);
            RibbonBounds=new Rect(min.x*unit,min.y*unit,ribbonSize.x*unit,ribbonSize.y*unit);
            nextRibbonPaint=0;
        }
        void PaintRibbon(float progress)
        {
            if(!ribbonTexture || ribbonSamples.Count==0)return;
            Array.Clear(ribbonPixels,0,ribbonPixels.Length);float radius=StrokeWidth*.5f;
            foreach(var entry in ribbonSamples)
            {
                var cell=entry.Key;var s=entry.Value;
                if(successfulInk && Mathf.Repeat(cell.x*.173f+cell.y*.317f,1)<Mathf.Max(0,progress-.25f)*1.1f)continue;
                float u=Mathf.Clamp01(s.distance/radius),roundness=Mathf.Sqrt(Mathf.Max(0,1-u*u));
                float side=Vector2.Dot(s.offset.normalized,new Vector2(-.4f,-.9165f));
                float lighting=Mathf.Clamp01(roundness*.68f+side*u*.32f);
                Color edge=successfulInk?new Color(.13f,.38f,.66f):new Color(.07f,.15f,.36f);
                Color face=successfulInk?new Color(.58f,.85f,1):new Color(.26f,.56f,.91f);
                Color color=Color.Lerp(edge,face,.20f+lighting*.80f);
                float flow=Mathf.Pow(Mathf.Max(0,Mathf.Sin(s.arc/(PixelUnit*7)-Time.unscaledTime*4.8f)),6);
                float spine=Mathf.Max(0,1-s.distance/(radius*.36f));
                if(SilverInk)
                {
                    float facet=Mathf.Repeat(Mathf.Floor(s.arc/(PixelUnit*4))+Mathf.Floor(side*3),3)/2;
                    color=Color.Lerp(new Color(.29f,.34f,.43f),new Color(.91f,.96f,1),lighting*.65f+facet*.20f);
                    color=Color.Lerp(color,Color.white,spine*(.25f+flow*.42f));
                }
                color=Color.Lerp(color,new Color(.71f,.92f,1),spine*(.24f+.35f*flow));
                float edgeFade=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.78f,1,u));
                float alpha=(.44f+.24f*roundness)*(1-edgeFade);
                if(s.distance>radius)alpha=.045f*Mathf.Clamp01(1-(s.distance-radius)/DetailUnit);
                color=InkColor(color);color.r=Mathf.Round(color.r*31)/31;color.g=Mathf.Round(color.g*31)/31;color.b=Mathf.Round(color.b*31)/31;color.a=Mathf.Min(InkBodyOpacity,alpha);
                int x=cell.x-ribbonMin.x,y=ribbonSize.y-1-(cell.y-ribbonMin.y);
                ribbonPixels[y*ribbonSize.x+x]=(Color32)color;
            }
            ribbonTexture.SetPixels32(ribbonPixels);ribbonTexture.Apply(false,false);
            paintedSuccess=successfulInk;nextRibbonPaint=Time.unscaledTime+1f/30;
        }
        Color InkColor(Color color){if(!SilverInk)return color;float light=Mathf.Max(color.r,Mathf.Max(color.g,color.b));return new Color(Mathf.Min(1,.04f+light*.93f),Mathf.Min(1,.04f+light*.95f),Mathf.Min(1,.04f+light*.97f),color.a);}
        void Crystal(Vector2 p,float size,float angle,float alpha)
        {
            var matrix=GUI.matrix;GUIUtility.RotateAroundPivot(angle,p);float unit=Mathf.Max(DetailUnit,size/5);
            for(int row=-2;row<=2;row++)
            {
                float width=(3-Mathf.Abs(row))*unit;
                GUI.color=new Color(row<0?.66f:.87f,row<0?.76f:.94f,1,alpha);
                GUI.DrawTexture(new Rect(p.x-width*.5f,p.y+row*unit-unit*.5f,width,unit),Texture2D.whiteTexture);
            }
            GUI.color=new Color(1,1,1,alpha);GUI.DrawTexture(new Rect(p.x-unit*.5f,p.y-unit,unit,unit*2),Texture2D.whiteTexture);GUI.matrix=matrix;
        }
        void Square(Vector2 p,float size,Color color){GUI.color=InkColor(color);GUI.DrawTexture(new Rect(p.x-size*.5f,p.y-size*.5f,size,size),Texture2D.whiteTexture);}
        void OnApplicationFocus(bool focused){if(!focused)CancelStroke();}
        void OnDestroy(){if(ribbonTexture)Destroy(ribbonTexture);}
    }
}
