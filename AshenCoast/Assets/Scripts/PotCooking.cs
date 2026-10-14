using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed class CookingBatch
    {public readonly List<Item> ingredients=new List<Item>();public readonly List<Item> results=new List<Item>();public float elapsed,duration;public bool manual;public float Progress=>Mathf.Clamp01(elapsed/duration);}
    public sealed partial class BuildingSystem
    {
        public bool Stir(CraftingStation pot,RectInt selected)
        {
            if(!pot.Heated || selected.width>4 || selected.height>4)return false;
            var items=pot.inventory.items.Where(i=>selected.Overlaps(new RectInt(i.x,i.y,i.Definition.width,i.Definition.height))).ToArray();
            if(items.Length==0 || items.Any(i=>i.kind!=ItemKind.RawMeat && i.kind!=ItemKind.BeetleMeat)){game.ui.Notice("Выделите сырое мясо");return false;}
            if(pot.batch!=null && pot.batch.ingredients.Any(i=>items.Contains(i)))pot.batch=null;
            foreach(var item in items){item.kind=item.kind==ItemKind.RawMeat?ItemKind.RoastMeat:ItemKind.RoastBeetle;item.RefreshCookedFood();}
            pot.fried.Clear();pot.fried.AddRange(items);pot.fryFlash=.7f;pot.stirring=.7f;pot.ready=pot.batch?.Progress??0;game.PlayPickup();return true;
        }
        void TickCooking(CraftingStation pot,float dt)
        {
            pot.stirring=Mathf.Max(0,pot.stirring-dt);
            pot.fryFlash=Mathf.Max(0,pot.fryFlash-dt);
            if(!pot.Heated){if(pot.steam)pot.steam.enabled=false;return;}
            if(pot.batch==null)
            {
                var raw=pot.inventory.items.FirstOrDefault(i=>i.kind==ItemKind.RawMeat);var insect=pot.inventory.items.FirstOrDefault(i=>i.kind==ItemKind.BeetleMeat);
                if(raw!=null && insect!=null){pot.batch=new CookingBatch{duration=12};pot.batch.ingredients.Add(raw);pot.batch.ingredients.Add(insect);pot.batch.results.Add(new Item(ItemKind.MeatAssortment));}
            }
            var batch=pot.batch;
            if(batch!=null && batch.ingredients.Any(i=>!pot.inventory.items.Contains(i))){pot.batch=null;pot.ready=0;batch=null;}
            if(batch!=null)
            {
                batch.elapsed+=dt*(pot.stirring>0?1.7f:1);pot.ready=batch.Progress;
                if(batch.Progress>=1)
                {
                    var test=new GridInventory(pot.inventory.columns,pot.inventory.rows,"cook output");test.items.AddRange(pot.inventory.items.Except(batch.ingredients));
                    if(batch.results.All(i=>test.Add(i))){foreach(var item in batch.ingredients)pot.inventory.items.Remove(item);foreach(var item in batch.results)pot.inventory.Place(item,item.x,item.y);pot.batch=null;pot.ready=1;game.PlayPickup();}
                }
            }
            if(!pot.steam){var white=Texture2D.whiteTexture;var sprite=Sprite.Create(white,new Rect(0,0,white.width,white.height),Vector2.one*.5f,white.width);pot.steam=game.art.Renderer("Cooking steam",sprite,pot.Position,CoastGame.Depth(pot.Position.y)+3);pot.steam.transform.SetParent(pot.world,true);pot.steam.sharedMaterial=new Material(Shader.Find("AshenCoast/CookingSteam"));pot.steam.transform.localScale=new Vector3(.75f,1.25f,1);}
            pot.steam.enabled=pot.batch!=null || pot.fryFlash>0;pot.steam.transform.position=new Vector2(pot.Position.x,pot.renderer.bounds.max.y+.62f);var block=new MaterialPropertyBlock();block.SetFloat("_SteamClock",Time.time);block.SetFloat("_Ready",pot.ready);pot.steam.SetPropertyBlock(block);
            pot.renderer.transform.rotation=Quaternion.Euler(0,0,pot.stirring>0?Mathf.Sin(Time.time*13)*1.5f:0);
        }
    }
}
