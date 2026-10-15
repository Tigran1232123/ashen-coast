using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace AshenCoast
{
    [Serializable] public sealed class LanFieldV49 {public string name;public int type;public float value,y;public Vector2[] points;}
    [Serializable] public sealed class LanRenderV49
    {
        public string texture,part;public float cloth=1,seated;public float rx,ry,rw,rh,px,py,ppu,x,y,z,sx,sy,angle,r,g,b,a,floor;public bool enabled,flipX,flipY;public int order;
        public static LanRenderV49 Capture(SpriteRenderer r)
        {var s=r.sprite;var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);return new LanRenderV49{floor=block.GetFloat("_BodyFloor"),cloth=block.HasFloat("_ClothTone")?block.GetFloat("_ClothTone"):1,seated=block.GetFloat("_SeatedLegs"),texture=s.texture.name,rx=s.rect.x,ry=s.rect.y,rw=s.rect.width,rh=s.rect.height,px=s.pivot.x/s.rect.width,py=s.pivot.y/s.rect.height,ppu=s.pixelsPerUnit,x=r.transform.position.x,y=r.transform.position.y,z=r.transform.position.z,sx=r.transform.lossyScale.x,sy=r.transform.lossyScale.y,angle=r.transform.eulerAngles.z,r=r.color.r,g=r.color.g,b=r.color.b,a=r.color.a,enabled=r.enabled,flipX=r.flipX,flipY=r.flipY,order=r.sortingOrder};}
    }
    [Serializable] public sealed class LanActorV49 {public string key,type,region;public float health,bound,curse,stun,anchorX,anchorY;public int owner=-1;public bool hired;public LanRenderV49[] renders;public LanFieldV49[] fields;}
    [Serializable] public sealed class LanLootV49 {public LanItemV49 item;public string key,region;public float x,y,fromX,fromY,age,duration;public bool collected;}
    [Serializable] public sealed class LanStationV49 {public string key,region;public int kind,x,y;public bool upright;public float temperature,ready;}
    [Serializable] public sealed class LanFireV49 {public string key;public float x,y,phase;public bool exploded;}
    [Serializable] public sealed class LanWormV49 {public string[] parts;public LanFieldV49[] fields;}
    [Serializable] public sealed class LanRegionV49 {public string key;public LanActorV49[] actors;public LanLootV49[] loot;public LanStationV49[] stations;public LanFireV49[] fires;public LanWormV49[] worms;public bool tavernAlarm;public string[] flowers;public float dummyHealth;public LanWebV49[] webs;public LanFogV49[] fogs;public LanTentacleV49[] tentacles;public LanInvocationV49[] invocations;}
    public sealed class CoopBindingV49
    {public string key,kind,region;public object driver;public ICombatTarget target;public SpriteRenderer[] renders;public Func<float> health;public Action<float> setHealth;public LanActorV49 received;public readonly Dictionary<string,SpriteRenderer> partsV57=new Dictionary<string,SpriteRenderer>();}
    public sealed partial class CoopWorldV49
    {
        readonly CoopSessionV49 session;CoastGame G=>session.game;
        public CoopWorldV49(CoopSessionV49 owner){session=owner;}
        readonly Dictionary<string,CoopBindingV49> entities=new Dictionary<string,CoopBindingV49>();
        readonly Dictionary<SpriteRenderer,string> identities=new Dictionary<SpriteRenderer,string>();
        readonly Dictionary<SpriteKeyV57,Sprite> sprites=new Dictionary<SpriteKeyV57,Sprite>();
        readonly Dictionary<string,Transform> privateRootsV49=new Dictionary<string,Transform>();
        public readonly Dictionary<string,int> allyOwners=new Dictionary<string,int>();
        public readonly Dictionary<string,string> allyRegions=new Dictionary<string,string>();
        public readonly HashSet<string> picked=new HashSet<string>();
        public IEnumerable<string> Regions=>Enumerable.Range(0,5).Select(i=>"area/"+i).Concat(Enumerable.Range(0,G.amazons.houses.Count).Select(i=>"house/"+i)).Concat(Enumerable.Range(0,4).Select(i=>"snail/"+i));
        public bool ValidRegion(string key)=>Regions.Contains(key) || key.StartsWith("pocket/") && int.TryParse(key.Substring(7),out var id) && id>=0 && id<10000;
        const BindingFlags Fields=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        public string Key(SpriteRenderer renderer)=>renderer && identities.TryGetValue(renderer,out var key)?key:null;
        public string Key(ICombatTarget target)=>target==null?null:Key(target.Renderer);
        public CoopBindingV49 Find(string key)=>key!=null && entities.TryGetValue(key,out var actor)?actor:null;
        public Transform Root(string region)
        {if(region.StartsWith("snail/"))return G.snails.houses[int.Parse(region.Substring(6))].room.transform;if(region=="area/0")return G.coastRoot.transform;if(region=="area/1")return G.journey.fieldRoot.transform;if(region=="area/2")return G.journey.tavernRoot.transform;if(region=="area/3")return G.fort.root.transform;if(region=="area/4")return G.cave.root.transform;if(region.StartsWith("house/")){int i=int.Parse(region.Substring(6));return G.amazons.RoomRootV49(i);}if(region=="pocket/"+session.Peer)return G.magic.pocket.transform;if(!privateRootsV49.TryGetValue(region,out var root)){var obj=new GameObject("Private network dimension / "+region);obj.SetActive(false);root=obj.transform;privateRootsV49[region]=root;}return root;}
        public string RegionOf(Transform transform)
        {if(transform==G.magic.pocket.transform || transform.IsChildOf(G.magic.pocket.transform))return "pocket/"+session.Peer;foreach(var pair in privateRootsV49)if(transform==pair.Value || transform.IsChildOf(pair.Value))return pair.Key;foreach(var region in Regions)if(transform==Root(region) || transform.IsChildOf(Root(region)))return region;if(transform==G.amazons.roomRoot.transform)return session.Region;return G.magic.InPocket?session.Region:"area/0";}
        public void RegisterInitial()
        {
            entities.Clear();identities.Clear();allyOwners.Clear();allyRegions.Clear();picked.Clear();
            Add("dummy","dummy","area/0",G,null,G.dummy,()=>G.dummyHealth,v=>G.dummyHealth=v);
            for(int i=0;i<G.woodlice.Count;i++){var m=G.woodlice[i];Add("woodlouse/"+i,"woodlouse","area/0",m,new WoodlouseActor(m),m.renderer,()=>m.vitals.health,v=>m.vitals.health=v);}
            for(int i=0;i<G.arcane.frogs.Count;i++){var m=G.arcane.frogs[i];Add("frog/"+i,"frog","area/0",m,new FrogActor(m),m.renderer,()=>m.health,v=>m.health=v);}
            for(int i=0;i<G.amazons.people.Count;i++){var m=G.amazons.people[i];Add("amazon/"+i,"amazon","area/0",m,m,m.Renderer,()=>m.health,v=>m.health=v);}
            for(int i=0;i<G.amazons.targets.Count;i++){var m=G.amazons.targets[i];Add("target/"+i,"target","area/0",m,m,m.Renderer,()=>m.health,v=>m.health=v);}
            for(int i=0;i<G.amazons.trees.Count;i++){var m=G.amazons.trees[i];Add("tree/"+i,"tree","area/0",m,null,m.sprite,()=>1,v=>{});}
            for(int i=0;i<G.fort.bandits.Count;i++){var m=G.fort.bandits[i];Add("fort/"+i,"fort","area/3",m,m,m.sprite,()=>m.health,v=>m.health=v);}
            for(int i=0;i<G.cave.spiders.Count;i++){var m=G.cave.spiders[i];Add("spider/"+i,"spider","area/4",m,m,m.sprite,()=>m.health,v=>m.health=v);}
            foreach(var m in G.cave.worms.SelectMany(w=>w.parts))Add("worm/"+m.seed,"worm","area/4",m,m,m.sprite,()=>m.health,v=>m.health=v);
            for(int i=0;i<G.journey.residents.Count;i++){var m=G.journey.residents[i];Add("resident/"+i,"resident","area/2",m,m,m.Renderer,()=>m.health,v=>m.health=v);}
            for(int i=0;i<G.journey.gulls.Count;i++){var m=G.journey.gulls[i];Add("gull/"+i,"gull","area/0",m,m,m.sprite,()=>m.health,v=>m.health=v);}
            for(int i=0;i<G.journey.caravanAnimals.Count;i++){var m=G.journey.caravanAnimals[i];Add("caravan/"+i,"caravan","area/1",m,m,m.Renderer,()=>m.vitals.health,v=>m.vitals.health=v);}
            {var m=G.journey.cartDriver;Add("driver","driver","area/1",m,m,m.Renderer,()=>m.health,v=>m.health=v);}
            {var m=G.hunter;Add("hunter","hunter","area/1",m,m,m.sprite,()=>m.health,v=>m.health=v);}
            {var m=G.journey.companion;Add("mage","mage","area/2",m,new MageCombatActor(m),m.sprite,()=>m.health,v=>m.health=v);}
            Refresh();
        }
        void Add(string key,string kind,string region,object driver,ICombatTarget target,SpriteRenderer renderer,Func<float> hp,Action<float> set)
        {
            var binding=new CoopBindingV49{key=key,kind=kind,region=region,driver=driver,target=target,renders=new[]{renderer},health=hp,setHealth=set};
            RefreshPartsV57(binding);entities[key]=binding;identities[renderer]=key;
        }
        void Refresh()
        {
            foreach(var m in G.amazons.plains.bandits)if(m.Renderer && !identities.ContainsKey(m.Renderer))Add("plain/"+Guid.NewGuid().ToString("N"),m.Archer?"plain-bow":"plain-sword","area/0",m,m,m.Renderer,()=>m.health,v=>m.health=v);
            foreach(var m in G.dragons.bosses)if(m.Renderer && !identities.ContainsKey(m.Renderer))Add("dragon/"+Guid.NewGuid().ToString("N"),"dragon",RegionOf(m.Renderer.transform),m,m,m.Renderer,()=>m.health,v=>m.health=v);
            foreach(var b in entities.Values.Where(b=>b.renders[0]==null).ToArray()){entities.Remove(b.key);}
            foreach(var b in entities.Values)if(allyOwners.TryGetValue(b.key,out var peer))
            {if(!G.snails.Resting(b.renders[0]) && !G.snails.Returning(b.renders[0]) && session.players.TryGetValue(peer,out var owner))allyRegions[b.key]=owner.region;b.region=allyRegions.TryGetValue(b.key,out var area)?area:b.region;}
        }
        public LanRegionV49 Capture(string region)
        {
            UpdateAlliesV49();Refresh();return new LanRegionV49{key=region,tavernAlarm=G.journey.TavernAlarm,dummyHealth=G.dummyHealth,webs=CaptureWebsV49(region),fogs=CaptureFogsV49(region),tentacles=CaptureTentaclesV49(region),invocations=CaptureInvocationsV49(region),actors=entities.Values.Where(b=>b.region==region && !G.snails.Resting(b.renders[0]) && !G.snails.Returning(b.renders[0])).Select(b=>CaptureActorV57(b,true)).ToArray(),loot=G.loot.Where(l=>!l.collected && l.renderer && RegionOf(l.renderer.transform)==region).Select(l=>new LanLootV49{key=l.netIdV49,region=region,item=LanItemV49.Capture(l.item),x=l.position.x,y=l.position.y,fromX=l.thrownFrom.x,fromY=l.thrownFrom.y,age=l.flightAge,duration=l.flightDuration,collected=l.collected}).ToArray(),stations=G.buildings.stations.Where(s=>s.world && RegionOf(s.world)==region).Select(s=>new LanStationV49{key=session.inventory.Key(s.inventory),region=region,kind=(int)s.kind,x=s.cell.x,y=s.cell.y,upright=s.size.y>s.size.x,temperature=s.temperature,ready=s.ready}).ToArray(),fires=G.arcane.campfires.Where(f=>f.logs && RegionOf(f.logs.transform)==region).Select(f=>new LanFireV49{key=FireKey(f),x=f.position.x,y=f.position.y,phase=f.phase,exploded=f.exploded}).ToArray(),worms=region=="area/4"?G.cave.worms.Select(w=>new LanWormV49{parts=w.parts.Select(p=>Key(p.sprite)).ToArray(),fields=CaptureFields(w)}).ToArray():Array.Empty<LanWormV49>(),flowers=region=="area/0"?picked.Concat(G.flowers.Where(f=>f.picked).Select(FlowerKey)).Distinct().ToArray():Array.Empty<string>()};
        }
        public void Apply(LanRegionV49 state,bool takeover)
        {
            if(state==null || !ValidRegion(state.key) || state.actors==null || state.actors.Length>1000 || state.loot?.Length>10000)throw new InvalidOperationException("Invalid world snapshot");
            if(state.key=="area/2")G.journey.ApplyAlarmV57(state.tavernAlarm);
            ApplyActorsV57(state.actors,state.key,takeover);
            ApplyTentaclesV49(state.tentacles,state.key,takeover);ApplyInvocationsV49(state.invocations,state.key,takeover);
            if(state.key=="area/4")ApplyWebsV49(state.webs);ApplyFogsV49(state.fogs,state.key,takeover);
            if(state.worms!=null && state.key=="area/4")
            {G.cave.worms.Clear();foreach(var group in state.worms){var worm=new SpectralWorm(G.cave);foreach(var id in group.parts)if(Find(id)?.driver is WormPart part && part.Alive)worm.parts.Add(part);if(worm.parts.Count>0){worm.ResetTrail();ApplyFields(worm,group.fields);G.cave.worms.Add(worm);}}}
            var knownLoot=new HashSet<string>((state.loot??Array.Empty<LanLootV49>()).Select(l=>l.key));
            foreach(var old in G.loot.Where(l=>RegionOf(l.renderer.transform)==state.key && !knownLoot.Contains(l.netIdV49) && !session.inventory.PendingWorldV49(l.netIdV49))){old.collected=true;old.renderer.enabled=old.shadow.enabled=false;if(old.cloth)old.cloth.enabled=false;if(old.spectrumGlow)old.spectrumGlow.enabled=false;}
            foreach(var drop in state.loot??Array.Empty<LanLootV49>())if(!session.inventory.PendingWorldV49(drop.key))ApplyLoot(drop);
            foreach(var structure in state.stations??Array.Empty<LanStationV49>())EnsureStation(structure);
            foreach(var fire in state.fires??Array.Empty<LanFireV49>())EnsureFire(fire,state.key);
            foreach(var flower in state.flowers??Array.Empty<string>()){picked.Add(flower);MarkFlower(flower,true);}
            if(state.key=="area/0")G.dummyHealth=state.dummyHealth;Physics2D.SyncTransforms();
        }
        public void TickReplicas(float dt)
        {
            foreach(var b in entities.Values)if(b.region==session.Region && b.received!=null)
            {
                var body=b.received.renders.FirstOrDefault(r=>r.part=="body");
                if(body!=null)PoseAssemblyV57(b,body,1-Mathf.Exp(-dt*22));
                if(b.driver is BeachGull gull)gull.point=b.renders[0].transform.position;
            }
        }
        Sprite ResolveSprite(LanRenderV49 d)
        {
            var key=new SpriteKeyV57(d);
            if(sprites.TryGetValue(key,out var found))return found;var texture=Resources.Load<Texture2D>("Art/"+d.texture);if(!texture && (d.texture.Contains("White") || d.texture.Contains("white")))texture=Texture2D.whiteTexture;if(!texture)return null;
            if(d.rw<=0 || d.rh<=0 || d.rx<0 || d.ry<0 || d.rx+d.rw>texture.width+.1f || d.ry+d.rh>texture.height+.1f || d.ppu<=0)throw new InvalidOperationException("Invalid sprite bounds");
            var sprite=Sprite.Create(texture,new Rect(d.rx,d.ry,d.rw,d.rh),new Vector2(d.px,d.py),d.ppu,0,SpriteMeshType.FullRect);sprites[key]=sprite;return sprite;
        }
        void ApplyRender(SpriteRenderer r,LanRenderV49 d,bool position)
        {
            var sprite=ResolveSprite(d);if(sprite)r.sprite=sprite;r.enabled=d.enabled;r.flipX=d.flipX;r.flipY=d.flipY;r.sortingOrder=d.order;r.color=new Color(d.r,d.g,d.b,d.a);
            if(position){r.transform.position=new Vector3(d.x,d.y,d.z);r.transform.rotation=Quaternion.Euler(0,0,d.angle);}
            var parent=r.transform.parent?r.transform.parent.lossyScale:Vector3.one;r.transform.localScale=new Vector3(d.sx/Mathf.Max(.0001f,parent.x),d.sy/Mathf.Max(.0001f,parent.y),1);G.art.PixelStyle(r);var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);block.SetFloat("_BodyFloor",d.floor);block.SetFloat("_ClothTone",d.cloth);block.SetFloat("_SeatedLegs",d.seated);r.SetPropertyBlock(block);
        }
        public static LanFieldV49[] CaptureFields(object obj,bool includePaths=true)
        {
            var values=new List<LanFieldV49>();foreach(var f in NetworkFieldsV57(obj.GetType()))
            {if(f.IsInitOnly || f.IsLiteral)continue;object value=f.GetValue(obj);var dto=new LanFieldV49{name=f.Name};if(f.FieldType==typeof(float)){dto.type=0;dto.value=(float)value;}else if(f.FieldType==typeof(int) || f.FieldType.IsEnum){dto.type=1;dto.value=Convert.ToInt32(value);}else if(f.FieldType==typeof(bool)){dto.type=2;dto.value=(bool)value?1:0;}else if(f.FieldType==typeof(Vector2)){dto.type=3;dto.value=((Vector2)value).x;dto.y=((Vector2)value).y;}else if(includePaths && value is List<Vector2> list && list.Count<=1000){dto.type=4;dto.points=list.ToArray();}else continue;if(LanItemV49.Finite(dto.value) && LanItemV49.Finite(dto.y))values.Add(dto);}var vitals=NetworkFieldV57(obj.GetType(),"vitals")?.GetValue(obj);if(vitals!=null)foreach(var d in CaptureFields(vitals,includePaths)){d.name="vitals::"+d.name;values.Add(d);}return values.ToArray();
        }
        static void ApplyFields(object obj,LanFieldV49[] data)
        {
            if(obj==null || data==null)return;foreach(var d in data)
            {
                if(d.name.StartsWith("vitals::")){var vitals=NetworkFieldV57(obj.GetType(),"vitals")?.GetValue(obj);if(vitals!=null)ApplyFields(vitals,new[]{new LanFieldV49{name=d.name.Substring(8),type=d.type,value=d.value,y=d.y,points=d.points}});continue;}
                var f=NetworkFieldV57(obj.GetType(),d.name);if(f==null || f.IsInitOnly || f.IsLiteral || !LanItemV49.Finite(d.value) || !LanItemV49.Finite(d.y))continue;
                if(f.FieldType==typeof(float))f.SetValue(obj,d.value);else if(f.FieldType==typeof(int))f.SetValue(obj,(int)d.value);else if(f.FieldType.IsEnum)f.SetValue(obj,Enum.ToObject(f.FieldType,(int)d.value));else if(f.FieldType==typeof(bool))f.SetValue(obj,d.value!=0);else if(f.FieldType==typeof(Vector2))f.SetValue(obj,new Vector2(d.value,d.y));else if(f.FieldType==typeof(List<Vector2>) && d.points?.Length<=1000)f.SetValue(obj,d.points.ToList());
            }
        }
        public void ApplyLoot(LanLootV49 data)
        {
            var drop=G.loot.FirstOrDefault(l=>l.netIdV49==data.key);
            // A consumed drop can still reference an item whose stack was merged or eaten.
            // Its tombstone hides a visual; it must never restore that obsolete inventory item.
            if(data.collected)
            {
                if(drop!=null){drop.collected=true;drop.renderer.enabled=drop.shadow.enabled=false;if(drop.cloth)drop.cloth.enabled=false;if(drop.spectrumGlow)drop.spectrumGlow.enabled=false;}
                return;
            }
            if(drop==null){drop=G.CreateLooseLootV46(data.item.Restore(),new Vector2(data.x,data.y));drop.netIdV49=data.key;drop.renderer.transform.SetParent(Root(data.region),true);drop.shadow.transform.SetParent(Root(data.region),true);if(drop.cloth)drop.cloth.transform.SetParent(Root(data.region),true);}
            drop.position=new Vector2(data.x,data.y);drop.thrownFrom=new Vector2(data.fromX,data.fromY);drop.flightDuration=Mathf.Clamp(data.duration,.1f,2);drop.flightAge=Mathf.Clamp(data.age,0,2);drop.collected=data.collected;drop.renderer.enabled=drop.shadow.enabled=!data.collected;if(drop.cloth)drop.cloth.enabled=!data.collected;
        }
        public CraftingStation EnsureStation(LanStationV49 data)
        {var station=G.buildings.stations.FirstOrDefault(s=>session.inventory.Key(s.inventory)==data.key);if(station==null){station=G.buildings.CreateStation((ItemKind)data.kind,new Vector2Int(data.x,data.y),data.upright,null,Root(data.region));session.inventory.Register(data.key,station.inventory,data.region,station.Position);}station.temperature=data.temperature;station.ready=data.ready;return station;}
        string FireKey(CampfireObject fire)=>RegionOf(fire.logs.transform)+"/fire/"+fire.Cell.x+"/"+fire.Cell.y;
        void EnsureFire(LanFireV49 data,string region)
        {
            var fire=G.arcane.campfires.FirstOrDefault(f=>f.logs && FireKey(f)==data.key);
            if(fire==null){var old=session.ApplyingNetwork;session.ApplyingNetwork=true;fire=G.arcane.CreateCampfire(new Vector2(data.x,data.y));fire.logs.transform.SetParent(Root(region),true);if(fire.collider)fire.collider.transform.SetParent(Root(region),true);foreach(var flame in fire.flames)flame.transform.SetParent(Root(region),true);session.ApplyingNetwork=old;}
            fire.phase=data.phase;fire.exploded=data.exploded;fire.logs.enabled=!data.exploded;if(fire.collider)fire.collider.enabled=!data.exploded;foreach(var flame in fire.flames)flame.enabled=!data.exploded;
        }
        public string FlowerKey(Wildflower flower)=>flower.procedural?"p/"+flower.proceduralId:"f/"+G.flowers.IndexOf(flower);
        public void MarkFlower(string key,bool value)
        {if(key.StartsWith("p/") && long.TryParse(key.Substring(2),out var id))G.meadow.SetPickedV49(id,value);foreach(var f in G.flowers.Where(f=>FlowerKey(f)==key)){f.picked=value;if(f.renderer)f.renderer.enabled=!value;if(f.stems)f.stems.enabled=!value;if(f.shadow)f.shadow.enabled=!value;if(f.procedural)G.meadow.SetPickedV49(f.proceduralId,value);}}
        public void RegionChanged(){session.ReapplyRegion();foreach(var b in entities.Values)if(allyOwners.ContainsKey(b.key))b.renders[0].gameObject.SetActive(b.region==session.Region);}
        public bool RedirectDamage(ICombatTarget target,float damage,SpriteRenderer source,DamageDeliveryV54 delivery=DamageDeliveryV54.Other)
        {
            if(!session.Online || session.ApplyingNetwork || target is HeroCombatActor || target is RemotePlayerV49)return false;
            string id=Key(target);if(id==null)return false;if(!session.CanSimulate){session.Send(new LanPacketV49{kind="damage",region=session.Region,body=JsonUtility.ToJson(new LanDamageV49{key=id,amount=damage,delivery=(int)delivery,bypass=G.spellcraft.BypassDamage || G.prism.ApplyingDamage,prism=G.prism.ApplyingDamage})});return true;}return false;
        }
        public void ApplyCommand(LanPacketV49 packet)
        {
            if(packet.kind=="hire")Debug.Log("LAN hire authority: region="+session.Region+" owns="+session.CanSimulate);
            if(packet.region!=session.Region || !session.CanSimulate)return;
            bool old=session.ApplyingNetwork;session.ApplyingNetwork=true;
            try
            {
                if(packet.kind=="damage")
                {
                    var d=JsonUtility.FromJson<LanDamageV49>(packet.body);var binding=Find(d.key);if(binding==null || d.amount<=0 || !LanItemV49.Finite(d.amount))return;
                    var source=packet.peer==session.Peer?G.hero:session.remote.TryGetValue(packet.peer,out var remote)?remote.Renderer:null;
                    if(!source || Vector2.Distance(source.transform.position,binding.renders[0].transform.position)>45)return;
                    if(d.key=="dummy"){float actual=Mathf.Min(G.dummyHealth,d.amount);G.DamageDummy(Mathf.RoundToInt(d.amount),false);if(d.effect!=0)session.SendDpsV49(packet.peer,"dummy",actual);return;}
                    if(binding.target==null)return;float hp=binding.health();
                    bool previous=G.spellcraft.BypassDamage;G.spellcraft.BypassDamage=d.bypass;
                    try{session.ForNearest(binding.renders[0].transform.position,()=>G.HurtTarget(binding.target,Mathf.Min(d.amount,10000),source,(DamageDeliveryV54)d.delivery),packet.peer);}finally{G.spellcraft.BypassDamage=previous;}
                    if(G.coop.IsPlayer(source))G.PlayerHit(binding.renders[0]);if(binding.target is PracticeTargetV46)session.SendDpsV49(packet.peer,d.key,Mathf.Min(hp,d.amount));
                }
                else if(packet.kind=="effect")ApplyEffectV49(packet);
                else if(packet.kind=="hire")ApplyHireV49(packet);
            }
            finally{session.ApplyingNetwork=old;}
        }
        public bool OwnAlly(SpriteRenderer actor)=>!session.Online || !allyOwners.TryGetValue(Key(actor)??"",out var owner) || owner==session.Peer;
        public int? AllyOwner(SpriteRenderer actor)=>allyOwners.TryGetValue(Key(actor)??"",out var owner)?owner:(int?)null;
        public void ApplyEffectV49(LanPacketV49 packet){session.ApplyEffectV49(packet);}
        public void ApplyHireV49(LanPacketV49 packet){session.ApplyHireV49(packet);}
    }
}
