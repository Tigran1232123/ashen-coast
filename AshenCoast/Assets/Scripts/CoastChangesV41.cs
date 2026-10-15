using System;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed class CoastArtV41
    {
        public const float SquareAspect=.84f;public static float FireAspect=.60f;
        public CoastArtV41(ArtBank bank)
        {
            foreach(var spec in new[]{new{file="FireBookV41",icon=24},new{file="AbyssBookV41",icon=26}})
            {
                var texture=bank.Load(spec.file);var columns=bank.Bands(texture,false,2);
                for(int i=0;i<2;i++){var b=columns[i];bank.icons[spec.icon+i]=bank.Slice(texture,new RectInt(b.x,0,b.y-b.x+1,texture.height),Vector2.one*.5f,260);}
            }
            ArtBank.ClosedSpatialAspect=SquareAspect;
        }
        public static float DisplayAspect(Item item)=>item.opened?0:item.kind==ItemKind.Pyromancy?FireAspect:item.kind==ItemKind.Tome || item.kind==ItemKind.AbyssTome?SquareAspect:0;
        public static void SizeGroundBook(GroundLoot drop)
        {
            float aspect=DisplayAspect(drop.item);if(aspect<=0)return;
            float height=drop.item.kind==ItemKind.Pyromancy?.91f:.98f;var sprite=drop.renderer.sprite;
            drop.renderer.transform.localScale=new Vector3(height*aspect/sprite.bounds.size.x,height/sprite.bounds.size.y,1);
        }
    }
    public sealed partial class CoastGame
    {
        public bool AutoPickup {get;private set;}
        float autoPickupClock;
        public void SetAutoPickup(bool enabled){AutoPickup=enabled;autoPickupClock=0;if(!testMode){PlayerPrefs.SetInt("AutoPickup",enabled?1:0);PlayerPrefs.Save();}}
        public void TickAutoPickup(float dt)
        {
            if(!AutoPickup || !loot.Any(l=>!l.collected && !l.InFlight && l.renderer.gameObject.activeInHierarchy && Vector2.Distance(body.position,l.position)<1.8f))return;
            if(CoopOnline && !coop.ApplyingNetwork && !coop.inventory.Mutating){coop.inventory.Mutate(()=>TickAutoPickup(dt));return;}
            if(!AutoPickup || paused || Dead || spectrum.Riding || magic.Teleporting || spellcraft.Teleporting)return;
            autoPickupClock-=dt;if(autoPickupClock>0)return;autoPickupClock=.15f;
            foreach(var drop in loot.Where(l=>!l.collected && !l.InFlight && l.renderer.enabled && l.renderer.gameObject.activeInHierarchy && Vector2.Distance(body.position,l.position)<=1.85f).OrderBy(l=>(l.position-body.position).sqrMagnitude).ToArray())
                if(!WorldBlocked(body.position,drop.position,.03f) && survivor.inventory.Add(drop.item))FinishLootPickup(drop);
        }
        void FinishLootPickup(GroundLoot drop)
        {
            if(CoopOnline && !coop.ApplyingNetwork)coop.inventory.Action(new LanWorldActionV49{kind="pickup",key=drop.netIdV49,region=coop.Region,x=drop.position.x,y=drop.position.y,item=LanItemV49.Capture(drop.item)});
            interactions.Pulse(drop.renderer);drop.collected=true;drop.renderer.enabled=drop.shadow.enabled=false;
            if(drop.cloth)drop.cloth.enabled=false;if(drop.spectrumGlow)drop.spectrumGlow.enabled=false;PlayPickup();
            if(hoveredLoot==drop)HighlightAt(null);
        }
    }
    public sealed partial class InventoryUI
    {
        public Rect AutoPickupButton=>new Rect(CanvasWidth/2-135,CanvasHeight/2+60,270,42);
        GUIStyle hoverShadowV41,spectrumHoverV41;
        void DrawWorldHoverV41(Rect area,string label,GUIStyle style)
        {
            if(hoverShadowV41==null){hoverShadowV41=new GUIStyle(style);hoverShadowV41.normal.textColor=Color.black;spectrumHoverV41=new GUIStyle(style);}
            GUI.color=Color.white;GUI.Label(new Rect(area.x+1,area.y+1,area.width,area.height),label,hoverShadowV41);
            if(label!=ItemDefinition.All[ItemKind.Spectrum].name && label!=ItemDefinition.All[ItemKind.Zen].name){GUI.Label(area,label,style);return;}
            spectrumHoverV41.normal.textColor=SpectrumNameColorV43(Time.time);GUI.Label(area,label,spectrumHoverV41);
        }
    }
    public sealed partial class FrogMage
    {
        SpriteRenderer aggressor;ICombatTarget victim;
        public SpriteRenderer TargetRenderer=>victim?.Renderer;
        public SpriteRenderer Aggressor=>aggressor;
        ICombatTarget FrogAttacker(SpriteRenderer source)=>source && source!=renderer?game.WorldCreatures.FirstOrDefault(t=>t.Renderer==source && t.Alive && source.enabled && source.gameObject.activeInHierarchy):null;
        void RememberFrogAttacker(SpriteRenderer source){if(FrogAttacker(source)!=null)aggressor=source;ChooseFrogVictim();}
        void ChooseFrogVictim()
        {victim=FrogAttacker(aggressor);Hostile=victim!=null;if(!Hostile){aggressor=null;attackClock=emission=0;}}
    }
    public sealed partial class ExplorationMap
    {
        Event queuedMapEvent;
        public void QueueInputForCheck(Vector2 point,EventType type,int button,Vector2 delta=default)
        {if(!game.testMode)throw new InvalidOperationException("Runtime verification only");queuedMapEvent=new Event{type=type,button=button,mousePosition=point,delta=delta};}
        void ProcessMapInputV41(Event e)
        {
            if(e.type==EventType.Repaint && queuedMapEvent!=null){e=queuedMapEvent;queuedMapEvent=null;}
            if(Plot.Contains(e.mousePosition))
            {
                if(e.type==EventType.ScrollWheel){ChangeZoom(Mathf.Pow(1.18f,-e.delta.y),e.mousePosition-Plot.center);e.Use();}
                if(e.type==EventType.MouseDown && e.button==0){panning=true;e.Use();}
                if(e.type==EventType.MouseDown && e.button==1 && SnailCommandV50){SelectSnailV50(e.mousePosition);game.RightClickConsumedFrame=Time.frameCount;e.Use();}
                if(e.type==EventType.MouseDown && e.button==1 && TravelSelection){SelectTravel(e.mousePosition);game.RightClickConsumedFrame=Time.frameCount;e.Use();}
            }
            if(e.type==EventType.MouseDrag && e.button==0 && panning){Pan(e.delta);e.Use();}
            if(e.type==EventType.MouseUp && e.button==0)panning=false;
        }
    }
}
