using System;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed class CoastArtV43
    {
        public CoastArtV43(ArtBank bank)
        {
            var plants=bank.Load("FieldPlantsV43");var bands=bank.Bands(plants,false,5);
            for(int i=0;i<5;i++)
            {
                var b=bands[i];var s=bank.Slice(plants,new RectInt(b.x,0,b.y-b.x+1,plants.height),new Vector2(.5f,0),i<3?500:680);
                bank.flowers[6+i]=s;bank.flowerMeshes[6+i]=ArtBank.BendableMesh(s);bank.icons[46+i]=s;
            }
            var kitchen=bank.Load("KitchenV43");var objects=bank.Bands(kitchen,false,2);
            for(int i=0;i<2;i++){var b=objects[i];bank.icons[44+i]=bank.Slice(kitchen,new RectInt(b.x,0,b.y-b.x+1,kitchen.height),Vector2.one*.5f,260);}
        }
        public static void ApplyBooks(ArtBank bank)
        {
            foreach(var spec in new[]{new{file="FireBookV43",icon=24},new{file="AbyssBookV43",icon=26}})
            {
                var t=bank.Load(spec.file);var bands=bank.Bands(t,false,2);
                for(int i=0;i<2;i++){var b=bands[i];bank.icons[spec.icon+i]=bank.Slice(t,new RectInt(b.x,0,b.y-b.x+1,t.height),Vector2.one*.5f,260);}
            }
            var folio=bank.Load("OpenFolioV43");bank.icons[18]=bank.Slice(folio,new RectInt(0,0,folio.width,folio.height),Vector2.one*.5f,260);
            ArtBank.ClosedSpatialAspect=.84f;ArtBank.OpenSpatialAspect=bank.icons[18].rect.width/bank.icons[18].rect.height;
        }
        public static ItemKind PlantKind(int type)=>type<3?(ItemKind)((int)ItemKind.BlueFlower+type):type<6?(ItemKind)((int)ItemKind.Buttercup+type-3):ItemKind.Clover;
        public static int PlantImage(int type,int variant)=>type<3?type+variant*3:6+type-3;
    }

    public sealed partial class CoastGame
    {
        public CoastArtV43 v43Art;
        float foodClockV43;
        public void TickFoodV43(float elapsed)
        {
            // DaySeconds includes accelerated meditation; every item ages once wherever it is held.
            float dt=Mathf.Max(0,elapsed-foodClockV43);foodClockV43=elapsed;
            var seen=new System.Collections.Generic.HashSet<Item>();
            void Age(Item item)
            {
                if(item==null || !seen.Add(item))return;
                if(item.Definition.Food && item.kind!=ItemKind.Brain){item.freshness=Mathf.Max(0,item.freshness-dt);if(item.freshness<=0){item.kind=ItemKind.Rot;item.opened=false;RefreshSpoiledGroundV44(item);}}
                if(item.contents!=null)foreach(var child in item.contents.items)Age(child);
                if(item.spells!=null)foreach(var slot in item.spells)Age(slot.scroll);
            }
            foreach(var grid in new[]{survivor.inventory,survivor.spatialInventory,shipStorage,ui.holding})foreach(var item in grid.items)Age(item);
            // NPC merchants preserve their stock until it changes ownership.
            foreach(var item in survivor.equipment)Age(item);
            foreach(var station in buildings.stations)if(!station.inventory.preserveFoodV58)foreach(var item in station.inventory.items)Age(item);
            foreach(var drop in loot)if(!drop.collected)Age(drop.item);
        }
        public Wildflower AddPlantV43(Vector2 at,int type,Transform parent,float scale=1,bool procedural=false,long id=0,float phase=0)
        {
            int image=CoastArtV43.PlantImage(type,0);
            var plant=new Wildflower{position=at,kind=CoastArtV43.PlantKind(type),seed=phase,procedural=procedural,proceduralId=id};
            plant.renderer=art.Renderer("Collectible "+ItemDefinition.All[plant.kind].name,art.flowers[image],at,Depth(at.y));plant.renderer.transform.SetParent(parent,true);plant.renderer.transform.localScale=Vector3.one*scale;art.PixelStyle(plant.renderer);
            plant.stems=art.FlowerWind(plant.renderer,image,phase);plant.shadow=Shadow("Plant ground shadow",at,new Vector2(.28f,.09f),.12f);plant.shadow.transform.SetParent(parent,true);flowers.Add(plant);return plant;
        }
        void InitialCloversV43()
        {
            for(int y=-1;y<30;y+=2)for(int x=-29;x<30;x+=2)
            {
                float v=MeadowLayout.Value(x,y,74391,441);var p=new Vector2(x+v*1.4f,y+MeadowLayout.Value(x,y,74391,442)*1.4f);
                if(!MeadowLayout.Walkable(p) || p.y<MeadowLayout.Shore(p.x)+1.1f || Physics2D.OverlapCircle(p,.32f))continue;
                AddPlantV43(p,6,coastRoot.transform,.60f+v*.20f,phase:v*20);
            }
        }
        public void FieldCloversV43(Transform parent)
        {
            for(int y=-16;y<27;y+=3)for(int x=-27;x<28;x+=3)
            {
                var p=new Vector2(x+MeadowLayout.Value(x,y,74391,448),y+MeadowLayout.Value(x,y,74391,449));
                if(Physics2D.OverlapCircle(p,.3f))continue;AddPlantV43(p,6,parent,.65f,phase:x+y);
            }
        }
        public bool OpenGroundVesselV43(Vector2 point)
        {
            var drop=loot.FirstOrDefault(l=>!l.collected && !l.InFlight && VesselWaterV43.Vessel(l.item.kind) && l.renderer.enabled && l.renderer.gameObject.activeInHierarchy && art.Opaque(l.renderer.sprite,l.renderer.transform.InverseTransformPoint(point)));
            if(drop==null || Vector2.Distance(body.position,drop.position)>3.1f)return false;
            var station=new CraftingStation{kind=drop.item.kind,cell=WorldGrid.Cell(drop.position),size=Vector2Int.one,world=drop.renderer.transform.parent,renderer=drop.renderer,inventory=drop.item.contents};
            ui.OpenStation(station);return true;
        }
        public ItemKind CloverHarvestV43(float roll)=>roll<.10f?ItemKind.FourLeafClover:ItemKind.Clover;
    }

    public sealed partial class InventoryUI
    {
        public static Color SpectrumNameColorV43(float clock)=>Color.HSVToRGB(Mathf.Repeat(clock*.23f,1),.52f,1);
        public static float SpectrumEdgeAlphaV43(float edge)=>Mathf.Pow(Mathf.Clamp01(edge),3)*.24f;
        public float FoodRemainingV43(Item item)=>Mathf.Clamp01(item.freshness/Mathf.Max(1,item.shelfLife));
        public static Rect GearIconAreaV43(Item item,Rect box)
        {
            var area=ItemIconArea(item,box.center);float scale=Mathf.Min(1,Mathf.Min((box.width-12)/area.width,(box.height-14)/area.height));
            return new Rect(box.center.x-area.width*scale*.5f,box.center.y-area.height*scale*.5f,area.width*scale,area.height*scale);
        }
        void DrawFoodWaterConditionV43(Item item,Rect box)
        {
            var icon=ItemIconArea(item,box.center);float width=Mathf.Min(box.width-8,Mathf.Max(25,icon.width));var bar=new Rect(box.center.x-width*.5f,Mathf.Min(box.yMax-5,icon.yMax+2),width,3);
            if(item.kind==ItemKind.WaterBottle){Fill(bar,new Color(.055f,.075f,.10f));Fill(new Rect(bar.x,bar.y,bar.width*item.water/2,3),new Color(.26f,.58f,.85f));}
            if(item.Definition.Food && item.kind!=ItemKind.Brain){float amount=FoodRemainingV43(item);Fill(bar,new Color(.09f,.075f,.055f));Fill(new Rect(bar.x,bar.y,bar.width*amount,3),Color.Lerp(new Color(.66f,.23f,.13f),new Color(.55f,.64f,.33f),amount));if(item.Spoiled)Border(box,new Color(.39f,.21f,.14f,.6f));}
        }
    }
}
