using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    [Serializable] public sealed class LanGridsV49 {public LanGridV49[] grids;}
    public sealed class SharedGridV49 {public string key,region;public GridInventory grid;public Vector2 point;}
    public sealed class CoopInventoryV49
    {
        readonly CoopSessionV49 session;CoastGame G=>session.game;
        readonly Dictionary<string,SharedGridV49> bindings=new Dictionary<string,SharedGridV49>();
        public readonly Dictionary<string,LanGridV49> canonical=new Dictionary<string,LanGridV49>();
        readonly Dictionary<int,LanPrivateV49> privateStates=new Dictionary<int,LanPrivateV49>();
        readonly HashSet<string> consumedLoot=new HashSet<string>(),pickedFlowers=new HashSet<string>(),buildClaims=new HashSet<string>();
        readonly HashSet<string> processed=new HashSet<string>();
        LanPrivateV49 before;Dictionary<string,LanGridV49> beforeGrids;List<LanWorldActionV49> actions;string pendingId;int depth;float nextPrivate;
        public int CoinsV49(int peer) => privateStates.TryGetValue(peer,out var state)?state.main.items.Where(i=>i.kind==(int)ItemKind.Coin).Sum(i=>i.count)+(state.equipment[3]?.contents?.items.Where(i=>i.kind==(int)ItemKind.Coin).Sum(i=>i.count)??0):0;
        public bool PayHireV49(int peer,int price){if(CoinsV49(peer)<price)return false;var state=privateStates[peer];foreach(var grid in new[]{state.main,state.equipment[3]?.contents})if(grid!=null){foreach(var coin in grid.items.Where(i=>i.kind==(int)ItemKind.Coin)){int take=Mathf.Min(price,coin.count);coin.count-=take;price-=take;if(price==0)break;}grid.items=grid.items.Where(i=>i.count>0).ToArray();if(price==0)break;}if(state.equipment[3]?.contents!=null)state.equipment[3].contents=state.equipment[3].contents;return true;}
        public bool Ready{get;private set;}public bool Busy=>pendingId!=null || !Ready || session.HirePendingV49;
        public bool Mutating=>depth>0;
        public bool PendingWorldV49(string key)=>pendingId!=null && actions!=null && actions.Any(a=>a.key==key);
        public CoopInventoryV49(CoopSessionV49 owner){session=owner;}
        public string Key(GridInventory grid)
        {
            if(grid==null)return null;var old=bindings.Values.FirstOrDefault(b=>b.grid==grid);if(old!=null)return old.key;
            var station=G.buildings.stations.FirstOrDefault(s=>s.inventory==grid);
            if(station==null)return null;string region=session.world.RegionOf(station.world);string key="station/"+region+"/"+(int)station.kind+"/"+station.cell.x+","+station.cell.y;Register(key,grid,region,station.Position);return key;
        }
        public void Register(string key,GridInventory grid,string region,Vector2 point)
        {
            bindings[key]=new SharedGridV49{key=key,grid=grid,region=region,point=point};
            if(canonical.TryGetValue(key,out var data) && !session.IsHost && !Mutating)data.Apply(grid);
        }
        void RegisterAll()
        {
            bindings.Clear();Register("ship",G.shipStorage,"area/0",G.ShipBase);Register("vendor/tavern",G.journey.seller,"area/2",G.journey.keeper.position);
            for(int i=0;i<G.amazons.people.Count;i++){var p=G.amazons.people[i];Register("vendor/amazon/"+i,p.stock,"area/0",p.Position);Register("weapon/amazon/"+i,p.weaponSlot,"area/0",p.Position);}
            foreach(var station in G.buildings.stations)Key(station.inventory);
        }
        public void InitializeHost()
        {Disconnect();RegisterAll();foreach(var b in bindings.Values)canonical[b.key]=LanGridV49.Capture(b.grid,b.key,0);privateStates[0]=LanPrivateV49.Capture(G);Ready=true;}
        public void Joined(){Disconnect();RegisterAll();session.Send(new LanPacketV49{kind="inventory",body=JsonUtility.ToJson(LanPrivateV49.Capture(G))});}
        public void Disconnect(){Ready=false;pendingId=null;before=null;depth=0;canonical.Clear();bindings.Clear();privateStates.Clear();consumedLoot.Clear();pickedFlowers.Clear();processed.Clear();buildClaims.Clear();}
        public void SendInitial(int peer)=>session.SendToV49(peer,new LanPacketV49{kind="grids",body=JsonUtility.ToJson(new LanGridsV49{grids=canonical.Values.ToArray()})});
        public void ReceiveGrids(string json)
        {var all=JsonUtility.FromJson<LanGridsV49>(json);foreach(var data in all.grids){canonical[data.key]=data;if(bindings.TryGetValue(data.key,out var binding) && pendingId==null)data.Apply(binding.grid);}Ready=true;}
        public void HostPrivate(LanPacketV49 packet)
        {var data=JsonUtility.FromJson<LanPrivateV49>(packet.body);ValidatePrivate(data);privateStates[packet.peer]=data;}
        void ValidatePrivate(LanPrivateV49 state)
        {
            if(state==null || state.equipment?.Length!=8)throw new InvalidOperationException("Invalid player inventory");
            foreach(var grid in new[]{state.main,state.spatial,state.holding,state.skates}){var temp=new GridInventory(grid.columns,grid.rows,grid.name){allowOpenBooks=true,preserveOpenBooks=true};if(grid.columns>10 || grid.rows>6 || grid.columns<1 || grid.rows<1)throw new InvalidOperationException("Invalid private grid size");grid.Apply(temp);}
            for(int i=0;i<8;i++)if(state.equipment[i]!=null && state.equipment[i].kind>=0 && !Survivor.Fits(state.equipment[i].Restore(),(GearSlot)i))throw new InvalidOperationException("Invalid equipment");
            var ids=Items(state).Select(i=>i.id).ToArray();if(ids.Distinct().Count()!=ids.Length)throw new InvalidOperationException("Duplicated private item");
        }
        static IEnumerable<LanItemV49> ItemTree(LanItemV49 item)
        {if(item==null || item.kind<0)yield break;yield return item;if(item.contents!=null)foreach(var child in item.contents.items.SelectMany(ItemTree))yield return child;if(item.spells!=null)foreach(var child in item.spells.SelectMany(s=>ItemTree(s.scroll)))yield return child;}
        static IEnumerable<LanItemV49> Items(LanPrivateV49 state)=>new[]{state.main,state.spatial,state.holding,state.skates}.SelectMany(g=>g.items).Concat(state.equipment).SelectMany(ItemTree);
        static IEnumerable<LanItemV49> Items(LanGridV49 state)=>state.items.SelectMany(ItemTree);
        static string Fingerprint(LanGridV49 data)
        {
            // Passive expiry/cooldowns do not create sixty inventory edits per second.
            var copy=JsonUtility.FromJson<LanGridV49>(JsonUtility.ToJson(data));foreach(var item in copy.items)NormalizeV49(item);copy.revision=0;return JsonUtility.ToJson(copy);
        }
        static void NormalizeV49(LanItemV49 item)
        {if(item==null || item.kind<0)return;item.freshness=0;item.capturedAt=0;var contents=item.contents;if(contents!=null){foreach(var child in contents.items)NormalizeV49(child);item.contents=contents;}if(item.spells!=null)foreach(var spell in item.spells){spell.remaining=0;var scroll=spell.scroll;NormalizeV49(scroll);spell.scroll=scroll;}}
        public T Mutate<T>(Func<T> operation)
        {
            if(!session.Online || session.ApplyingNetwork)return operation();if(Busy && depth==0)return default;
            bool outer=depth==0;if(outer)Begin();depth++;
            try{return operation();}
            finally{depth--;if(outer)End();}
        }
        public void Mutate(Action operation)=>Mutate(()=>{operation();return true;});
        void Begin()
        {
            if(session.CanSimulate)session.PublishWorldV49();RefreshBindings();before=LanPrivateV49.Capture(G);beforeGrids=bindings.Values.ToDictionary(b=>b.key,b=>LanGridV49.Capture(b.grid,b.key,canonical.TryGetValue(b.key,out var old)?old.revision:0));actions=new List<LanWorldActionV49>();
            session.Send(new LanPacketV49{kind="inventory",body=JsonUtility.ToJson(before)});
        }
        void RefreshBindings()
        {
            for(int i=0;i<G.amazons.people.Count;i++){var p=G.amazons.people[i];foreach(var key in new[]{"vendor/amazon/"+i,"weapon/amazon/"+i})if(bindings.TryGetValue(key,out var b)){b.point=p.Position;b.region=session.world.allyRegions.TryGetValue("amazon/"+i,out var region)?region:"area/0";}}
            foreach(var station in G.buildings.stations)Key(station.inventory);
        }
        void End()
        {
            RefreshBindings();var changed=new List<LanGridV49>();foreach(var b in bindings.Values)
            {var dto=LanGridV49.Capture(b.grid,b.key,canonical.TryGetValue(b.key,out var original)?original.revision:0);if(!beforeGrids.TryGetValue(b.key,out var old) || Fingerprint(old)!=Fingerprint(dto))changed.Add(dto);}
            if(changed.Count==0 && actions.Count==0){before=null;session.Send(new LanPacketV49{kind="inventory",body=JsonUtility.ToJson(LanPrivateV49.Capture(G))});return;}
            pendingId=Guid.NewGuid().ToString("N");var tx=new LanTransactionV49{id=pendingId,grids=changed.ToArray(),inventory=LanPrivateV49.Capture(G),actions=actions.ToArray()};session.Send(new LanPacketV49{kind="transaction",region=session.Region,body=JsonUtility.ToJson(tx)});
        }
        public void Action(LanWorldActionV49 action){if(Mutating)actions.Add(action);}
        public void HostTransaction(LanPacketV49 packet)
        {
            var tx=JsonUtility.FromJson<LanTransactionV49>(packet.body);bool accepted=true;
            if(tx==null || tx.id==null || tx.grids==null || tx.actions==null || tx.grids.Length>10 || tx.actions.Length>20 || !processed.Add(packet.peer+"/"+tx.id))return;
            try
            {
                ValidatePrivate(tx.inventory);RefreshBindings();var player=session.players[packet.peer];
                foreach(var grid in tx.grids)
                {
                    if(canonical.TryGetValue(grid.key,out var old))
                    {if(old.revision!=grid.revision)throw new InvalidOperationException("Stale inventory revision");var b=bindings[grid.key];if(grid.key.StartsWith("weapon/amazon/") && session.world.allyOwners.TryGetValue("amazon/"+grid.key.Substring(14),out var allyOwner) && allyOwner!=packet.peer)throw new InvalidOperationException("Another player owns this ally");if(b.region!=player.region || Vector2.Distance(b.point,new Vector2(player.x,player.y))>(grid.key=="ship"?14:4.2f))throw new InvalidOperationException("Inventory out of reach");var test=new GridInventory(b.grid.columns,b.grid.rows,b.grid.name){allowOpenBooks=b.grid.allowOpenBooks,preserveOpenBooks=b.grid.preserveOpenBooks,acceptsItem=b.grid.acceptsItem,singleItem=b.grid.singleItem};grid.Apply(test);}
                    else if(!tx.actions.Any(a=>a.kind=="build" && a.key==grid.key))throw new InvalidOperationException("Unknown shared inventory");
                }
                foreach(var a in tx.actions)
                {
                    if(a.region!=player.region || !LanItemV49.Finite(a.x) || !LanItemV49.Finite(a.y) || Vector2.Distance(new Vector2(player.x,player.y),new Vector2(a.x,a.y))>16)throw new InvalidOperationException("World action out of reach");
                    if(a.kind=="pickup") {if(consumedLoot.Contains(a.key) || !session.TryLootV49(a.key,out var drop) || drop.collected || Vector2.Distance(new Vector2(player.x,player.y),new Vector2(drop.x,drop.y))>2.1f)throw new InvalidOperationException("Loot already claimed");}
                    else if(a.kind=="flower"){if(pickedFlowers.Contains(a.key))throw new InvalidOperationException("Plant already claimed");}
                    else if(a.kind=="build"){if(buildClaims.Contains(a.key) || a.item==null || !BuildingSystem.Buildable((ItemKind)a.value))throw new InvalidOperationException("Occupied building cell");}
                    else if(a.kind=="drop"){if(a.item==null)throw new InvalidOperationException("Missing drop");a.item.Restore();}
                    else throw new InvalidOperationException("Unknown world action");
                }
                var ids=Items(tx.inventory).Concat(canonical.Where(p=>!tx.grids.Any(g=>g.key==p.Key)).SelectMany(p=>Items(p.Value))).Concat(tx.grids.SelectMany(Items)).Concat(privateStates.Where(p=>p.Key!=packet.peer).SelectMany(p=>Items(p.Value))).Select(i=>i.id).ToArray();
                if(ids.Distinct().Count()!=ids.Length)throw new InvalidOperationException("Item already held by another player");
            }
            catch(Exception e){accepted=false;Debug.Log("LAN inventory conflict: "+e.Message);}
            if(accepted)
            {
                foreach(var a in tx.actions){if(a.kind=="pickup")consumedLoot.Add(a.key);else if(a.kind=="flower")pickedFlowers.Add(a.key);else if(a.kind=="build")buildClaims.Add(a.key);}
                foreach(var grid in tx.grids){grid.revision++;canonical[grid.key]=grid;}
                privateStates[packet.peer]=tx.inventory;
            }
            var response=new LanCommitV49{id=tx.id,accepted=accepted,grids=tx.grids.Select(g=>canonical.TryGetValue(g.key,out var current)?current:null).Where(g=>g!=null).ToArray(),actions=accepted?tx.actions:Array.Empty<LanWorldActionV49>()};
            var message=new LanPacketV49{kind="commit",peer=packet.peer,target=packet.peer,body=JsonUtility.ToJson(response)};
            if(accepted)session.BroadcastV49(message);else session.SendToV49(packet.peer,message);
        }
        public void ReceiveCommit(LanCommitV49 commit)
        {
            bool own=commit.id==pendingId;if(own && !commit.accepted)
            {session.ApplyingNetwork=true;try{before?.Apply(G);foreach(var a in actions)Undo(a);G.ui.Notice("Предмет уже недоступен");}finally{session.ApplyingNetwork=false;}}
            if(commit.accepted)
            {
                session.ApplyingNetwork=true;try{foreach(var a in commit.actions)ApplyAction(a);}finally{session.ApplyingNetwork=false;}
            }
            foreach(var data in commit.grids){canonical[data.key]=data;if(bindings.TryGetValue(data.key,out var b))data.Apply(b.grid);}
            if(own){pendingId=null;before=null;actions=null;G.ui.RefreshSelectionV49();}
        }
        void ApplyAction(LanWorldActionV49 a)
        {
            if(a.kind=="pickup"){consumedLoot.Add(a.key);foreach(var l in G.loot.Where(l=>l.netIdV49==a.key)){l.collected=true;l.renderer.enabled=l.shadow.enabled=false;if(l.cloth)l.cloth.enabled=false;if(l.spectrumGlow)l.spectrumGlow.enabled=false;}}
            else if(a.kind=="flower"){pickedFlowers.Add(a.key);session.world.picked.Add(a.key);session.world.MarkFlower(a.key,true);}
            else if(a.kind=="drop")session.world.ApplyLoot(new LanLootV49{key=a.key,item=a.item,region=a.region,x=a.x,y=a.y,fromX=a.x,fromY=a.y,age=1,duration=.55f});
            else if(a.kind=="build")
            {buildClaims.Add(a.key);var item=a.item.Restore();if(item.kind==ItemKind.Campfire){if(!G.arcane.campfires.Any(f=>session.world.RegionOf(f.logs.transform)==a.region && f.Cell==WorldGrid.Cell(new Vector2(a.x,a.y)))){var fire=G.arcane.CreateCampfire(new Vector2(a.x,a.y));var root=session.world.Root(a.region);fire.logs.transform.SetParent(root,true);if(fire.collider)fire.collider.transform.SetParent(root,true);foreach(var flame in fire.flames)flame.transform.SetParent(root,true);}}else{var station=session.world.EnsureStation(new LanStationV49{key=a.key,region=a.region,kind=a.value,x=(int)a.x,y=(int)a.y,upright=a.upright});if(item.contents!=null && !canonical.ContainsKey(a.key))LanGridV49.Capture(item.contents,a.key,0).Apply(station.inventory);}}
        }
        void Undo(LanWorldActionV49 a)
        {
            if(a.kind=="flower")session.world.MarkFlower(a.key,false);
            else if(a.kind=="pickup" && !consumedLoot.Contains(a.key))foreach(var l in G.loot.Where(l=>l.netIdV49==a.key)){l.collected=false;l.renderer.enabled=l.shadow.enabled=true;}
            else if(a.kind=="drop")foreach(var l in G.loot.Where(l=>l.netIdV49==a.key)){l.collected=true;l.renderer.enabled=l.shadow.enabled=false;}
            else if(a.kind=="build" && a.value==(int)ItemKind.Campfire){var f=G.arcane.campfires.FirstOrDefault(f=>session.world.RegionOf(f.logs.transform)==a.region && f.Cell==WorldGrid.Cell(new Vector2(a.x,a.y)));if(f!=null){UnityEngine.Object.Destroy(f.logs.gameObject);if(f.collider)UnityEngine.Object.Destroy(f.collider.gameObject);foreach(var flame in f.flames)UnityEngine.Object.Destroy(flame.gameObject);G.arcane.campfires.Remove(f);}}
            else if(a.kind=="build")foreach(var s in G.buildings.stations.Where(s=>Key(s.inventory)==a.key).ToArray()){UnityEngine.Object.Destroy(s.renderer.gameObject);UnityEngine.Object.Destroy(s.collider.gameObject);G.buildings.stations.Remove(s);bindings.Remove(a.key);}
        }
        public void MergeWorld(LanRegionV49 state)
        {foreach(var drop in state.loot??Array.Empty<LanLootV49>())if(consumedLoot.Contains(drop.key))drop.collected=true;state.flowers=(state.flowers??Array.Empty<string>()).Concat(pickedFlowers).Distinct().ToArray();}
        public void TickAuthority()
        {
            RefreshBindings();if(!session.CanSimulate)return;var changed=new List<LanGridV49>();foreach(var b in bindings.Values.Where(b=>b.region==session.Region))
            {var data=LanGridV49.Capture(b.grid,b.key,canonical.TryGetValue(b.key,out var old)?old.revision:0);if(old==null || Fingerprint(data)!=Fingerprint(old)){if(session.IsHost){data.revision++;canonical[b.key]=data;changed.Add(data);}else if(!Busy)changed.Add(data);}}
            if(changed.Count>0){if(session.IsHost)session.BroadcastV49(new LanPacketV49{kind="grids",body=JsonUtility.ToJson(new LanGridsV49{grids=changed.ToArray()})});else session.Send(new LanPacketV49{kind="systemInventory",region=session.Region,body=JsonUtility.ToJson(new LanGridsV49{grids=changed.ToArray()})});}
            TickPrivateV49();
        }
        public void TickPrivateV49(){if(session.Online && Time.unscaledTime>=nextPrivate && !Busy){nextPrivate=Time.unscaledTime+.5f;session.Send(new LanPacketV49{kind="inventory",body=JsonUtility.ToJson(LanPrivateV49.Capture(G))});}}
        public void HostSystemV49(LanPacketV49 packet)
        {
            if(!session.owners.TryGetValue(packet.region,out var owner) || owner!=packet.peer)return;var data=JsonUtility.FromJson<LanGridsV49>(packet.body);var response=new List<LanGridV49>();
            foreach(var grid in data.grids){if(!canonical.TryGetValue(grid.key,out var old))continue;if(grid.revision==old.revision){var actual=bindings[grid.key];if(actual.region!=packet.region)continue;var test=new GridInventory(grid.columns,grid.rows,grid.name){allowOpenBooks=actual.grid.allowOpenBooks,preserveOpenBooks=actual.grid.preserveOpenBooks};grid.Apply(test);grid.revision++;canonical[grid.key]=grid;response.Add(grid);}else response.Add(old);}
            session.BroadcastV49(new LanPacketV49{kind="grids",body=JsonUtility.ToJson(new LanGridsV49{grids=response.ToArray()})});
        }
    }
}
