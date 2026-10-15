using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    [DefaultExecutionOrder(-500)]
    public sealed partial class CoopSessionV49:MonoBehaviour
    {
        public CoastGame game;public LanTransportV49 transport;public CoopWorldV49 world;public CoopInventoryV49 inventory;
        public readonly Dictionary<int,LanPlayerV49> players=new Dictionary<int,LanPlayerV49>();
        public readonly Dictionary<string,int> owners=new Dictionary<string,int>();
        readonly Dictionary<string,string> snapshots=new Dictionary<string,string>();
        readonly Dictionary<int,float> lastSeen=new Dictionary<int,float>();
        public readonly Dictionary<int,RemotePlayerV49> remote=new Dictionary<int,RemotePlayerV49>();
        public bool Online{get;private set;}public bool Connecting{get;private set;}public bool IsHost=>transport!=null && transport.Host;
        public int Peer{get;private set;}=-1;public int Port=LanTransportV49.DefaultPort;
        public string PlayerName="Игрок",Address="",Status="";public bool MenuOpen,LocalPaused,ApplyingNetwork;
        float nextPlayer,nextWorld,nextTime,worldSeconds,receivedTime;long sequence;string oldRegion;
        public RemotePlayerV49 Context;
        public SpriteRenderer LocalHero;public Rigidbody2D LocalBody;public Survivor LocalSurvivor;
        public string Region=>game.journey.Area==WorldArea.SnailHouse && game.snails.Inside>=0 && !game.magic.InPocket?"snail/"+game.snails.Inside:game.magic.InPocket?"pocket/"+Peer:game.journey.Area==WorldArea.AmazonHouse?"house/"+Mathf.Max(0,game.amazons.houses.IndexOf(game.amazons.currentHouse)):"area/"+(int)game.journey.Area;
        public bool CanSimulate=>!Online || owners.TryGetValue(Region,out var owner) && owner==Peer;
        public void Initialize(CoastGame owner)
        {game=owner;LocalHero=game.hero;LocalBody=game.body;LocalSurvivor=game.survivor;LanItemV49.Clock=()=>game.seconds;LanItemV49.SnapshotClock=()=>Online?NetworkTimeV49:Time.realtimeSinceStartup;world=new CoopWorldV49(this);inventory=new CoopInventoryV49(this);PlayerName=PlayerPrefs.GetString("LANName","Игрок");Address=PlayerPrefs.GetString("LANAddress","192.168.1.2");}
        public bool StartHost(int port=LanTransportV49.DefaultPort,bool localTest=false)
        {
            Disconnect(false);game.ui.CancelInteraction();if(localTest)game.snails.Configure(4);game.snails.peers=new[]{0,-1,-1,-1};
            try
            {
                transport=new LanTransportV49();transport.Listen(port,localTest);Port=port;Peer=0;Online=true;Status="Игра создана";PlayerPrefs.SetString("LANName",PlayerName);
                players[Peer]=CapturePlayer();inventory.InitializeHost();world.RegisterInitial();world.ExistingAlliesV49();fx=new CoopFxV49(this);
                foreach(var key in world.Regions){owners[key]=-1;snapshots[key]=JsonUtility.ToJson(world.Capture(key));}
                Reassign();MenuOpen=false;LocalPaused=false;game.paused=false;Time.timeScale=1;return true;
            }
            catch(Exception e){Debug.LogWarning("LAN host: "+e.Message);Disconnect(false);Status="Не удалось создать игру";return false;}
        }
        public void Join(string address,int port=LanTransportV49.DefaultPort)
        {
            Disconnect(false);game.ui.CancelInteraction();PlayerPrefs.SetString("LANName",PlayerName);PlayerPrefs.SetString("LANAddress",address);Address=address;Port=port;
            transport=new LanTransportV49();Connecting=true;Status="Подключение…";world.RegisterInitial();fx=new CoopFxV49(this);transport.Connect(address,port);game.paused=false;Time.timeScale=1;
        }
        public void Disconnect(bool show=true)
        {
            fx?.Dispose();fx=null;transport?.Dispose();transport=null;foreach(var p in remote.Values)p.Dispose();remote.Clear();players.Clear();owners.Clear();snapshots.Clear();lastSeen.Clear();inventory?.Disconnect();
            HirePendingV49=false;Online=Connecting=false;Peer=-1;Context=null;ApplyingNetwork=false;LocalPaused=false;oldRegion=null;Time.timeScale=1;if(game)game.paused=false;if(show)Status="Отключено";
        }
        void OnDestroy()=>Disconnect(false);
        void Update()
        {
            if(transport==null)return;
            int count=0;while(transport!=null && count++<256 && transport.inbox.TryDequeue(out var incoming))
            {
                try
                {
                    if(incoming.connected){lastSeen[incoming.peer]=Time.unscaledTime;if(!IsHost)Send(new LanPacketV49{kind="hello",body=JsonUtility.ToJson(new LanHelloV49{name=CleanName(PlayerName)})});continue;}
                    if(incoming.disconnected){if(IsHost){players.Remove(incoming.peer);game.snails.Left(incoming.peer);lastSeen.Remove(incoming.peer);Reassign();BroadcastRoster();}else{Disconnect(false);Status="Связь с ведущим потеряна";}continue;}
                    if(incoming.json==null){Status="Не удалось подключиться";if(!IsHost)Disconnect(false);continue;}
                    var packet=JsonUtility.FromJson<LanPacketV49>(incoming.json);if(packet==null || packet.version!=50)throw new InvalidOperationException("Protocol mismatch");
                    if(IsHost){packet.peer=incoming.peer;lastSeen[incoming.peer]=Time.unscaledTime;HandleHost(packet);}else Handle(packet);
                }
                catch(Exception e){Debug.LogWarning("LAN packet rejected: "+e.Message);if(IsHost)transport?.Drop(incoming.peer);else{Disconnect(false);Status="Не удалось синхронизировать игру";}}
            }
            if(!Online)return;
            if(!IsHost)game.seconds=worldSeconds+Time.unscaledTime-receivedTime-(game.meditation?.DayOffset??0);
            inventory.TickPrivateV49();world.UpdateAlliesV49();fx?.Tick();
            foreach(var proxy in remote.Values)proxy.Tick(Time.unscaledDeltaTime);
            if(!CanSimulate)world.TickReplicas(Time.unscaledDeltaTime);
            if(Time.unscaledTime>=nextPlayer)
            {
                nextPlayer=Time.unscaledTime+.05f;var state=CapturePlayer();players[Peer]=state;Send(new LanPacketV49{kind="player",region=state.region,body=JsonUtility.ToJson(state)});
                if(oldRegion!=Region){oldRegion=Region;fx?.RegionChanged();world.RegionChanged();if(IsHost){Reassign();BroadcastRoster();}}
            }
            if(CanSimulate && Time.unscaledTime>=nextWorld)
            {nextWorld=Time.unscaledTime+.12f;inventory.TickAuthority();Send(new LanPacketV49{kind="world",region=Region,body=JsonUtility.ToJson(world.Capture(Region))});}
            if(IsHost && Time.unscaledTime>=nextTime){nextTime=Time.unscaledTime+1;Send(new LanPacketV49{kind="time",body=JsonUtility.ToJson(new LanClockV49{seconds=game.DaySeconds,networkTime=Time.realtimeSinceStartup})});foreach(var p in lastSeen.ToArray())if(Time.unscaledTime-p.Value>10)transport.Drop(p.Key);}
        }
        string CleanName(string name)=>string.IsNullOrWhiteSpace(name)?"Игрок":new string(name.Trim().Where(c=>!char.IsControl(c)).Take(20).ToArray());
        public void Send(LanPacketV49 packet)
        {if(transport==null)return;packet.peer=Peer;packet.sequence=++sequence;if(IsHost)HandleHost(packet);else transport.Send(0,JsonUtility.ToJson(packet));}
        void Deliver(LanPacketV49 packet,int except=-1)
        {Handle(packet);transport.Broadcast(JsonUtility.ToJson(packet),except);}
        void HandleHost(LanPacketV49 packet)
        {
            if(packet.kind=="hello")
            {
                var hello=JsonUtility.FromJson<LanHelloV49>(packet.body);if(hello.build!=50 || players.Count>=game.snails.PlayerCount || game.snails.JoinSeat(packet.peer)<0){transport.Drop(packet.peer);return;}
                players[packet.peer]=new LanPlayerV49{peer=packet.peer,name=CleanName(hello.name),region="area/0",x=-1.5f-packet.peer*.7f,y=-1.65f};
                transport.Send(packet.peer,JsonUtility.ToJson(new LanPacketV49{kind="welcome",peer=0,body=JsonUtility.ToJson(new LanWelcomeV49{peer=packet.peer,seed=WorldShape.Seed,seconds=game.DaySeconds,name=hello.name,x=-1.5f-packet.peer*.7f,y=-1.65f,networkTime=Time.realtimeSinceStartup})}));
                foreach(var entry in snapshots)transport.Send(packet.peer,JsonUtility.ToJson(new LanPacketV49{kind="world",peer=owners.TryGetValue(entry.Key,out var own)?own:0,region=entry.Key,body=entry.Value}));
                transport.Send(packet.peer,JsonUtility.ToJson(new LanPacketV49{kind="caravanState",body=JsonUtility.ToJson(game.snails.Capture())}));inventory.SendInitial(packet.peer);Reassign();BroadcastRoster();return;
            }
            if(!players.ContainsKey(packet.peer))return;
            switch(packet.kind)
            {
                case "player":
                    var player=JsonUtility.FromJson<LanPlayerV49>(packet.body);if(!Validate(player))throw new InvalidOperationException("Invalid player state");player.peer=packet.peer;player.name=players[packet.peer].name;bool transition=players[packet.peer].region!=player.region;players[packet.peer]=player;packet.body=JsonUtility.ToJson(player);Deliver(packet,packet.peer);if(transition){Reassign();BroadcastRoster();}break;
                case "fx":if(packet.region==players[packet.peer].region)Deliver(packet,packet.peer);break;
                case "world":if(owners.TryGetValue(packet.region,out var owner) && owner==packet.peer){var state=JsonUtility.FromJson<LanRegionV49>(packet.body);inventory.MergeWorld(state);packet.body=JsonUtility.ToJson(state);snapshots[packet.region]=packet.body;Deliver(packet,packet.peer);}break;
                case "caravanCommand":game.snails.ApplyCommand(JsonUtility.FromJson<CaravanCommandV50>(packet.body),packet.peer,players[packet.peer].region);break;
                case "transaction":inventory.HostTransaction(packet);break;
                case "systemInventory":inventory.HostSystemV49(packet);break;
                case "dps":if(owners.TryGetValue(packet.region,out var dpsOwner) && dpsOwner==packet.peer)SendToV49(packet.target,packet);break;
                case "hireRejected":if(owners.TryGetValue(packet.region,out var rejectOwner) && rejectOwner==packet.peer)SendToV49(packet.target,new LanPacketV49{kind="hireFailed",body=packet.body});break;
                case "hireApproved":if(owners.TryGetValue(packet.region,out var hirer) && hirer==packet.peer)ApproveHireV49(packet.body,packet.target,packet.region);break;
                case "inventory":inventory.HostPrivate(packet);break;
                case "damage":case "effect":case "hire":
                    if(packet.kind=="hire")Debug.Log("LAN hire route: peer="+packet.peer+" region="+packet.region+" actual="+players[packet.peer].region);
                    if(packet.region!=players[packet.peer].region)return;
                    if(owners.TryGetValue(packet.region,out var authority)){if(authority==0)Handle(packet);else if(authority>0)transport.Send(authority,JsonUtility.ToJson(packet));}break;
                case "snare":case "playerDamage":if(packet.target>=0 && players.ContainsKey(packet.target) && owners.TryGetValue(packet.region,out var simulator) && simulator==packet.peer){if(packet.target==0)Handle(packet);else transport.Send(packet.target,JsonUtility.ToJson(packet));}break;
                case "time":if(packet.peer==0)Deliver(packet);break;
                case "clock":if(float.TryParse(packet.body,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out var time) && LanItemV49.Finite(time)){game.seconds=time-(game.meditation?.DayOffset??0);Send(new LanPacketV49{kind="time",body=JsonUtility.ToJson(new LanClockV49{seconds=time,networkTime=Time.realtimeSinceStartup})});}break;
            }
        }
        bool Validate(LanPlayerV49 p)=>p!=null && p.region!=null && world.ValidRegion(p.region) && LanItemV49.Finite(p.x) && LanItemV49.Finite(p.y) && Mathf.Abs(p.x)<2000 && Mathf.Abs(p.y)<2000 && LanItemV49.Finite(p.health) && p.health>=0 && p.health<=100 && p.equipment?.Length==8;
        void Handle(LanPacketV49 packet)
        {
            switch(packet.kind)
            {
                case "welcome":var welcome=JsonUtility.FromJson<LanWelcomeV49>(packet.body);if(welcome.seed!=WorldShape.Seed)throw new InvalidOperationException("Different world seed");Peer=welcome.peer;Online=true;Connecting=false;worldSeconds=welcome.seconds;receivedTime=Time.unscaledTime;networkBase=welcome.networkTime;networkReceived=Time.realtimeSinceStartup;Status="Подключено";MenuOpen=LocalPaused=false;game.paused=false;Time.timeScale=1;game.journey.SetArea(WorldArea.Coast,new Vector2(welcome.x,welcome.y));game.body.position=new Vector2(welcome.x,welcome.y);game.hero.transform.position=game.body.position;players[Peer]=CapturePlayer();inventory.Joined();break;
                case "player":if(packet.peer!=Peer){var state=JsonUtility.FromJson<LanPlayerV49>(packet.body);players[state.peer]=state;UpdateProxy(state);}break;
                case "roster":var roster=JsonUtility.FromJson<LanRosterV49>(packet.body);var previous=new Dictionary<string,int>(owners);owners.Clear();foreach(var o in roster.owners)owners[o.region]=o.peer;foreach(var p in roster.players){players[p.peer]=p;if(p.peer!=Peer)UpdateProxy(p);}foreach(var p in remote.Keys.ToArray())if(!roster.players.Any(s=>s.peer==p)){remote[p].Dispose();fx?.RemovePeer(p);remote.Remove(p);players.Remove(p);}if(Online && owners.TryGetValue(Region,out var own) && own==Peer && (!previous.TryGetValue(Region,out var old) || old!=Peer) && snapshots.TryGetValue(Region,out var latest))ApplyWorld(Region,latest,true);break;
                case "world":snapshots[packet.region]=packet.body;if(Online && (packet.region==Region || IsHost) && packet.peer!=Peer)ApplyWorld(packet.region,packet.body,false);break;
                case "time":var clock=JsonUtility.FromJson<LanClockV49>(packet.body);worldSeconds=clock.seconds;receivedTime=Time.unscaledTime;networkBase=clock.networkTime;networkReceived=Time.realtimeSinceStartup;break;
                case "mageCommand":if(packet.region==Region && CanSimulate && game.snails.troops.TryGetValue("mage",out var mageOrder) && game.snails.OwnerPeer(mageOrder)==packet.target && !mageOrder.resting){ForNearest(game.journey.companion.Position,()=>{if(!game.journey.companion.Explode())game.ui.Notice("Недостаточно маны");},packet.target);}break;
                case "caravanState":game.snails.Apply(JsonUtility.FromJson<CaravanStateV50>(packet.body));break;
                case "whistle":if(packet.region==Region && !IsHost){var whistle=JsonUtility.FromJson<CaravanCommandV50>(packet.body);game.snails.Lure(new Vector2(whistle.x,whistle.y));game.snails.PlayWhistle();}break;
                case "fx":fx?.Receive(packet.peer,packet.region,packet.body);break;
                case "grids":inventory.ReceiveGrids(packet.body);break;
                case "commit":inventory.ReceiveCommit(JsonUtility.FromJson<LanCommitV49>(packet.body));break;
                case "hireFailed":Debug.Log("LAN hire failed: "+packet.body);HirePendingV49=false;game.ui.Notice(packet.body);break;
                case "hired":ReceiveHireV49(packet);break;
                case "dps":var dps=JsonUtility.FromJson<LanDamageV49>(packet.body);var dummy=world.Find(dps.key)?.renders[0];if(dummy)game.RecordDummyV48(dummy,dps.amount);break;
                case "damage":case "effect":case "hire":world.ApplyCommand(packet);break;
                case "snare":if(packet.target==Peer)game.cave.Snare(Mathf.Clamp(float.Parse(packet.body,System.Globalization.CultureInfo.InvariantCulture),0,10));break;
                case "playerDamage":if(packet.target==Peer){float amount=float.Parse(packet.body,System.Globalization.CultureInfo.InvariantCulture);ApplyingNetwork=true;try{game.HurtPlayer(Mathf.Clamp(amount,0,10000));}finally{ApplyingNetwork=false;}}break;
            }
        }
        public void ApplyWorld(string region,string json,bool takeover)
        {ApplyingNetwork=true;try{world.Apply(JsonUtility.FromJson<LanRegionV49>(json),takeover);}finally{ApplyingNetwork=false;}}
        void UpdateProxy(LanPlayerV49 state){if(!remote.TryGetValue(state.peer,out var proxy)){proxy=new RemotePlayerV49(this,state);remote[state.peer]=proxy;}proxy.Receive(state);}
        void Reassign()
        {foreach(var key in owners.Keys.Concat(players.Values.Select(p=>p.region)).Distinct().ToArray())owners[key]=players.Values.Where(p=>p.region==key).Select(p=>p.peer).DefaultIfEmpty(-1).Min();}
        void BroadcastRoster()=>Deliver(new LanPacketV49{kind="roster",peer=0,body=JsonUtility.ToJson(new LanRosterV49{players=players.Values.ToArray(),owners=owners.Select(o=>new LanOwnerV49{region=o.Key,peer=o.Value}).ToArray()})});
        public void ReapplyRegion(){if(snapshots.TryGetValue(Region,out var json))ApplyWorld(Region,json,true);}
        public void ClockChanged(){if(Online)Send(new LanPacketV49{kind="clock",body=game.DaySeconds.ToString("R",System.Globalization.CultureInfo.InvariantCulture)});}
        public int PlayerPeer(SpriteRenderer sprite)=>sprite==game.hero && Context==null?Peer:remote.Values.FirstOrDefault(p=>p.Renderer==sprite)?.Peer??-1;
        public void SendToV49(int peer,LanPacketV49 packet){if(peer==Peer)Handle(packet);else transport?.Send(peer,JsonUtility.ToJson(packet));}
        public void BroadcastV49(LanPacketV49 packet){Handle(packet);transport?.Broadcast(JsonUtility.ToJson(packet));}
        public void PublishWorldV49(){if(Online && CanSimulate)Send(new LanPacketV49{kind="world",region=Region,body=JsonUtility.ToJson(world.Capture(Region))});}
        public bool TryLootV49(string id,out LanLootV49 loot)
        {
            var current=game.loot.FirstOrDefault(l=>l.netIdV49==id);
            if(current!=null){loot=new LanLootV49{key=id,item=LanItemV49.Capture(current.item),region=world.RegionOf(current.renderer.transform),x=current.position.x,y=current.position.y,collected=current.collected};return true;}
            foreach(var snapshot in snapshots.Values){var state=JsonUtility.FromJson<LanRegionV49>(snapshot);loot=state.loot?.FirstOrDefault(l=>l.key==id);if(loot!=null)return true;}loot=null;return false;
        }
    }
    public sealed partial class CoastGame
    {public CoopSessionV49 coop;public bool CoopOnline=>coop!=null && coop.Online;public bool LocalMenuPausedV49=>paused || coop!=null && coop.LocalPaused;public bool LocalInputLockedV49=>snails!=null && (snails.SetupOpen || snails.Choosing || snails.AllyOffer!=null) || coop!=null && (coop.MenuOpen || coop.LocalPaused) || CoopOnline && map.Open;}
}
