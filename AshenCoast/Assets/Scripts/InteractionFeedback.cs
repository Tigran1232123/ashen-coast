using System.Collections.Generic;
using UnityEngine;
namespace AshenCoast
{
    public sealed class InteractionFeedback
    {
        readonly CoastGame game;readonly MaterialPropertyBlock properties=new MaterialPropertyBlock();
        readonly Dictionary<Renderer,float> pulses=new Dictionary<Renderer,float>();
        readonly List<Renderer> finished=new List<Renderer>();
        public Renderer Hovered {get;private set;}
        public string Name {get;private set;}
        public InteractionFeedback(CoastGame owner){game=owner;}
        void Apply(Renderer renderer,float hover,float pulse)
        {if(!renderer)return;renderer.GetPropertyBlock(properties);properties.SetFloat("_Hover",hover);properties.SetFloat("_InteractPulse",pulse);renderer.SetPropertyBlock(properties);}
        public void Hover(Renderer renderer,string name)
        {
            if(Hovered!=renderer){Apply(Hovered,0,Strength(Hovered));Hovered=renderer;}
            Name=name;Apply(Hovered,1,Strength(Hovered));
        }
        public float Strength(Renderer renderer)=>renderer && pulses.TryGetValue(renderer,out var at)?Mathf.Clamp01(1-(Time.unscaledTime-at)/.65f):0;
        public void Pulse(Renderer renderer)
        {if(!renderer)return;pulses[renderer]=Time.unscaledTime;Apply(renderer,renderer==Hovered?1:0,1);}
        public void Tick()
        {
            finished.Clear();foreach(var pair in pulses){float pulse=Strength(pair.Key);Apply(pair.Key,pair.Key==Hovered?1:0,pulse);if(pulse<=0)finished.Add(pair.Key);}
            foreach(var renderer in finished)pulses.Remove(renderer);
        }
        public Renderer At(Vector2 point,out string name)
        {
            name=null;
            if(game.amazons!=null){var village=game.amazons.InteractionAt(point,out name);if(village)return village;}
            var station=game.buildings?.At(point);if(station!=null){name=station.inventory.name+"";return station.renderer;}
            foreach(var drop in game.loot)if(!drop.collected && !drop.InFlight && drop.renderer.gameObject.activeInHierarchy && game.GroundPixelV45(drop,point)){name=drop.item.Definition.name;return drop.renderer;}
            var flower=game.FlowerAt(point);if(flower!=null){name=ItemDefinition.All[flower.kind].name;return flower.stems;}
            var cave=game.cave.InteractionAt(point);if(cave){name=game.cave.Inside?"Выйти из катакомб":"Спуститься в катакомбы";return cave;}
            var passage=game.fort.InteractionAt(point);if(passage){name=game.fort.Inside?"Подняться из подземелья":"Спуститься в форт";return passage;}
            var journey=game.journey.InteractionAt(point,out name);if(journey)return journey;
            if(game.CanOpenShipAt(point)){name="Трюм корабля";return game.ship;}
            foreach(var fire in game.arcane.campfires)if(!fire.exploded && fire.logs.gameObject.activeInHierarchy && Vector2.Distance(fire.position,game.body.position)<=6 && Vector2.Distance(fire.position,point)<.95f){name="Костёр";return fire.logs;}
            if(game.magic.InPocket && Vector2.Distance(game.body.position,new Vector2(0,3.84f))<2 && game.magic.door.bounds.Contains(point)){name="Вернуться";return game.magic.door;}
            return null;
        }
    }
}
