using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    [Serializable] public sealed class SnailStateV50 {public float x,y,hp=1000,hidden,damage,window;public bool moving;}
    [Serializable] public sealed class TroopStateV50 {public string key;public int home,seat;public bool resting,returning,campRestV51,seatedV51,fluteV51,makerV51;public float campTimeV51;public float hp,mana,x,y;}
    [Serializable] public sealed class CaravanStateV50 {public int count;public int[] peers;public CampStateV51 camp;public SnailStateV50[] snails;public TroopStateV50[] troops;public float stop,x,y;public bool ordered;}
    [Serializable] public sealed class CaravanCommandV50 {public string kind,key;public bool known;public int home;public float x,y,amount;}

    public sealed partial class SnailHouseV50:ICombatTarget
    {
        public readonly SnailHomesV50 caravan;public readonly int index;
        public readonly SpriteRenderer Renderer,shadow;public readonly GameObject room;
        SpriteRenderer ICombatTarget.Renderer=>Renderer;
        public readonly List<GameObject> personal=new List<GameObject>(),barracks=new List<GameObject>();
        public Vector2 position;public float health=1000,hidden,damage,window;public bool moving;
        public bool Alive=>health>0;public Vector2 Position=>position;public bool Hidden=>hidden>0 || health<=0;
        public Vector2 Door=>position+new Vector2(Renderer.flipX?-.25f:.25f,.65f);
        public SnailHouseV50(SnailHomesV50 owner,int i,Vector2 at)
        {
            caravan=owner;index=i;position=at;var g=owner.game;
            Renderer=g.art.Renderer("Улитка "+(i+1),owner.shells[0],at,CoastGame.Depth(at.y));Renderer.transform.SetParent(g.coastRoot.transform,true);
            shadow=g.Shadow("Snail contact",at,new Vector2(7,1.9f),.29f);shadow.transform.SetParent(g.coastRoot.transform,true);
            room=new GameObject("Snail room "+(i+1));owner.FurnishV51(this);room.SetActive(false);Pose(0);
        }
        public float Hurt(float amount)
        {
            if(caravan.game.CoopOnline && !caravan.game.coop.IsHost){caravan.Command(new CaravanCommandV50{kind="damage",home=index,amount=amount});return 0;}
            return HurtLocal(amount);
        }
        public float HurtLocal(float amount)
        {
            if(Hidden || amount<=0)return 0;float taken=Mathf.Min(Mathf.Max(0,health-(caravan.Occupied(index)?1:0)),amount);health-=taken;damage+=taken;
            if(window<=0)window=5;
            caravan.game.ShowDamage(taken,position+Vector2.up*4,Renderer);
            if((damage>=250 || health<=0) && !caravan.Occupied(index))Hide();return taken;
        }
        public void Hide(){if(caravan.Occupied(index))return;hidden=15;moving=false;caravan.Eject(index);Pose(0);}
        public void Pose(float clock)
        {
            PoseBodyV51(clock);shadow.transform.position=position;caravan.DecorateV51(this);
            foreach(var obj in personal)obj.SetActive(caravan.Personal(index) && caravan.PlayerCount==1);foreach(var obj in barracks)obj.SetActive(!caravan.Personal(index) || caravan.PlayerCount>1);
        }
    }

    public sealed partial class SnailHomesV50:MonoBehaviour
    {
        public CoastGame game;public readonly List<SnailHouseV50> houses=new List<SnailHouseV50>();
        public readonly Dictionary<string,TroopStateV50> troops=new Dictionary<string,TroopStateV50>();
        public Sprite[] shells,furniture;public int PlayerCount=1;public int[] peers={0,-1,-1,-1};
        public bool SetupOpen=true,Choosing,CommitHire;public int ChosenHome=-1,Inside=-1;public string HireKey;public string AllyOffer;
        Action hireAction;public float stopped;public Vector2 destination;public bool ordered;
        float clock,nextReveal,nextSync,doorWait;readonly Dictionary<SpriteRenderer,Vector2> lurePoints=new Dictionary<SpriteRenderer,Vector2>();readonly List<Vector2> route=new List<Vector2>();int routeAt;
        AudioClip whistle;readonly Dictionary<SpriteRenderer,float> lured=new Dictionary<SpriteRenderer,float>();
        public bool Authority=>!game.CoopOnline || game.coop.IsHost;
        public int LocalPeer=>game.CoopOnline?game.coop.Peer:0;
        public string Key(SpriteRenderer actor)=>actor==game.journey.companion.sprite?"mage":"amazon/"+game.amazons.people.FindIndex(a=>a.Renderer==actor);
        public SpriteRenderer Sprite(string key)=>key=="mage"?game.journey.companion.sprite:int.TryParse(key?.Replace("amazon/",""),out var i) && i>=0 && i<game.amazons.people.Count?game.amazons.people[i].Renderer:null;
        public VillageActorV46 Woman(string key)=>key=="mage"?null:game.amazons.people.FirstOrDefault(a=>Key(a.Renderer)==key);
        public bool Resting(SpriteRenderer actor)=>actor && troops.TryGetValue(Key(actor),out var t) && (t.resting || t.campRestV51);
        public bool Returning(SpriteRenderer actor)=>actor && troops.TryGetValue(Key(actor),out var t) && t.returning;
        public int Cost(string key)=>key=="mage"?3:Woman(key)?.IsSlave==true?1:2;
        public int Seat(int peer)=>Array.IndexOf(peers,peer);
        public int OwnerSeat(int home)=>PlayerCount==1?0:PlayerCount==2?home/2:PlayerCount==3 && home==3?-1:home;
        public bool Personal(int home)=>PlayerCount==1?home==0:PlayerCount==2?home%2==0:home<PlayerCount;
        public int Capacity(int home)=>PlayerCount==1 && home==0?0:10;
        public int Used(int home)=>troops.Values.Where(t=>t.home==home && (Living(t) || game.corpses!=null && game.corpses.HasBody(t.key))).Sum(t=>Cost(t.key));
        public bool CanUse(int home,int peer)=>home>=0 && home<4 && Seat(peer)>=0 && (OwnerSeat(home)<0 || OwnerSeat(home)==Seat(peer));
        public int OwnerPeer(TroopStateV50 t)=>!game.CoopOnline?0:t.seat>=0 && t.seat<peers.Length?peers[t.seat]:-1;
        public bool Own(TroopStateV50 t)=>!game.CoopOnline || OwnerPeer(t)==LocalPeer;
        public bool Living(TroopStateV50 t)=>t.key=="mage"?game.journey.companion.Alive:Woman(t.key)?.Alive==true;
        public bool Available(int home,string key,int peer)=>CanUse(home,peer) && Capacity(home)-Used(home)>=Cost(key) && !houses[home].Hidden;
        public bool Occupied(int home)=>Inside==home || game.CoopOnline && game.coop.players.Values.Any(p=>p.region=="snail/"+home);
        public void Initialize(CoastGame g)
        {
            game=g;SetupOpen=!g.testMode && !Environment.GetCommandLineArgs().Contains("-snail-check");
            var shellsTexture=game.art.Load("SnailHousesV50");shells=new[]{new RectInt(0,0,900,512),new RectInt(900,0,636,512),new RectInt(0,512,900,512),new RectInt(900,512,636,512)}.Select(rect=>game.art.Slice(shellsTexture,rect,new Vector2(.5f,0),200)).ToArray();var ft=game.art.Load("SnailFurnitureV50");var rects=new[]{new RectInt(0,0,520,570),new RectInt(520,0,504,590),new RectInt(0,575,520,465),new RectInt(520,590,504,500),new RectInt(0,1040,520,496),new RectInt(520,1100,504,436)};furniture=rects.Select(r=>game.art.Slice(ft,r,new Vector2(.5f,0),200)).ToArray();
            var start=g.journey.CartPosition+Vector2.up*10;
            for(int i=0;i<4;i++){var p=FindLand(start+new Vector2(i%2*10-4,i/2*8));houses.Add(new SnailHouseV50(this,i,p));}
            const int rate=22050;var samples=new float[rate];for(int i=0;i<samples.Length;i++){float t=(float)i/rate;float env=Mathf.Sin(Mathf.PI*t)*Mathf.Min(1,t*25);samples[i]=Mathf.Sin(2*Mathf.PI*(1650*t+260*t*t+8*Mathf.Sin(t*9)))*env*.25f;}whistle=AudioClip.Create("Player whistle",rate,1,rate,false);whistle.SetData(samples,0);
            InitializeCampV51();Reveal();
        }
        Sprite[] Atlas(string name,int columns,int rows)
        {var texture=game.art.Load(name);var result=new Sprite[columns*rows];for(int i=0;i<result.Length;i++)result[i]=game.art.Slice(texture,new RectInt(i%columns*texture.width/columns,i/columns*texture.height/rows,texture.width/columns,texture.height/rows),new Vector2(.5f,0),200);return result;}
        public void Configure(int count)
        {PlayerCount=Mathf.Clamp(count,1,4);foreach(var t in troops.Values){if(!CanUse(t.home,peers[Mathf.Max(0,t.seat)]) || Capacity(t.home)==0){int h=Enumerable.Range(0,4).FirstOrDefault(i=>Capacity(i)>0 && Used(i)+Cost(t.key)<=Capacity(i));t.home=h;t.seat=Mathf.Max(0,OwnerSeat(h));}}foreach(var h in houses)h.Pose(clock);}
        public int JoinSeat(int peer)
        {int slot=Array.IndexOf(peers,peer);if(slot>=0)return slot;for(int i=1;i<PlayerCount;i++)if(peers[i]<0){peers[i]=peer;SyncOwners();return i;}return -1;}
        public void Left(int peer){int slot=Seat(peer);if(slot>=0)peers[slot]=-1;SyncOwners();}
        public void SyncOwners()
        {if(!game.CoopOnline)return;foreach(var t in troops.Values){game.coop.world.allyOwners[t.key]=OwnerPeer(t);if(t.resting || t.returning || t.campRestV51)game.coop.world.allyRegions[t.key]="area/0";}}
        public bool SelectHire(string key,Action action)
        {
            if(CommitHire || game.coop!=null && game.coop.ApplyingNetwork)return false;
            HireKey=key;hireAction=action;Choosing=true;game.journey.HireOffer=false;game.amazons.Offer=null;return true;
        }
        public void FinishHire(int home)
        {
            if(!Available(home,HireKey,LocalPeer)){game.ui.Notice("Нет места");return;}
            ChosenHome=home;CommitHire=true;Choosing=false;var key=HireKey;
            try{hireAction?.Invoke();if(!game.CoopOnline){bool hired=key=="mage"?game.journey.companion.Hired:Woman(key)?.Hired==true;if(hired)Assign(key,home,0);}}
            finally{CommitHire=false;hireAction=null;HireKey=null;}
        }
        public void Assign(string key,int home,int peer)
        {if(troops.ContainsKey(key))return;troops[key]=new TroopStateV50{key=key,home=home,seat=Mathf.Max(0,Seat(peer))};SyncOwners();}
        public IEnumerable<TroopStateV50> Following=>troops.Values.Where(t=>Own(t) && Living(t) && !t.resting && !t.returning && !t.campRestV51);
        public bool SendHome(string key,int peer)
        {
            if(!troops.TryGetValue(key,out var t) || OwnerPeer(t)!=peer || !Living(t))return false;
            if(houses[t.home].Hidden){game.ui.Notice("Улитка недоступна");return false;}
            t.campRestV51=t.seatedV51=false;t.resting=false;t.returning=true;var sprite=Sprite(key);if(!sprite || !sprite.transform.IsChildOf(game.coastRoot.transform))PlaceOutside(t);SyncOwners();return true;
        }
        void PlaceOutside(TroopStateV50 t)
        {
            var sprite=Sprite(t.key);if(!sprite)return;var h=houses[t.home];var at=h.Door+Vector2.down*1.6f+Vector2.right*((troops.Values.ToList().IndexOf(t)%3-1)*.6f);
            sprite.transform.SetParent(game.coastRoot.transform,true);
            if(t.key=="mage"){game.journey.companion.remoteArea=null;game.journey.companion.Warp(at);}else Woman(t.key).Warp(at);
            if(game.CoopOnline){game.coop.world.allyRegions[t.key]="area/0";var b=game.coop.world.Find(t.key);if(b!=null)b.region="area/0";}
            sprite.gameObject.SetActive(game.journey.IsCoast && !game.magic.InPocket);
        }
        public void Eject(int home)
        {foreach(var t in troops.Values.Where(t=>t.home==home && (t.resting || t.returning))){t.resting=t.returning=t.campRestV51=t.seatedV51=false;PlaceOutside(t);}SyncOwners();}
        public void Recall(string key,int peer)
        {
            if(!troops.TryGetValue(key,out var t) || OwnerPeer(t)!=peer || !Living(t))return;
            if(houses[t.home].Hidden)return;t.resting=t.returning=t.campRestV51=t.seatedV51=false;
            var p=game.CoopOnline && game.coop.players.TryGetValue(peer,out var player)?new Vector2(player.x,player.y):game.body.position;
            string region=game.CoopOnline && game.coop.players.TryGetValue(peer,out var state)?state.region:Inside>=0?"snail/"+Inside:"area/"+(int)game.journey.Area;
            var sprite=Sprite(key);sprite.transform.SetParent(game.CoopOnline?game.coop.world.Root(region):game.arcane.WorldRoot,true);
            if(key=="mage")game.journey.companion.Warp(p+Vector2.left);else Woman(key).Warp(p+Vector2.left);
            sprite.gameObject.SetActive(!game.CoopOnline || region==game.coop.Region);SyncOwners();
        }
        readonly int[] roomOrdinalsV53=new int[4];
        void TickTroops(float dt)
        {
            Array.Clear(roomOrdinalsV53,0,4);foreach(var t in troops.Values)
            {
                if(!Living(t))continue;
                var sprite=Sprite(t.key);if(!sprite)continue;
                if(t.campRestV51){TickCampTroopV51(t,dt);continue;}
                if(t.resting)
                {
                    if(t.key=="mage"){var m=game.journey.companion;m.health=Mathf.Min(CompanionMage.MaxHealth,m.health+dt*8);m.mana=Mathf.Min(CompanionMage.MaxMana,m.mana+dt*10);}
                    else{var woman=Woman(t.key);woman.health=Mathf.Min(woman.MaxHealth,woman.health+dt*8);}
                    sprite.transform.SetParent(houses[t.home].room.transform,true);int ordinal=roomOrdinalsV53[t.home]++;var restingPoint=new Vector2(-.6f+(ordinal%4)*.6f,-1.35f-(ordinal/4)*.5f);if(t.key=="mage")game.journey.companion.Warp(restingPoint);else Woman(t.key).Warp(restingPoint);sprite.gameObject.SetActive(Inside==t.home);continue;
                }
                if(t.returning)
                {
                    if(houses[t.home].Hidden){t.returning=false;continue;}
                    var goal=houses[t.home].Door;var from=(Vector2)sprite.transform.position;
                    if(game.spellcraft.ImmobilizedV44(sprite))continue;
                    var seek=Vector2.Distance(from,game.amazons.Centre)<55 && Vector2.Distance(goal,game.amazons.Centre)>60?game.amazons.Centre+(goal-game.amazons.Centre).normalized*58:goal;
                    if(t.key=="mage")game.journey.companion.ReturnHomeV50(seek,dt);else Woman(t.key).ReturnHomeV50(seek,dt);
                    sprite.sortingOrder=CoastGame.Depth(sprite.transform.position.y);
                    if(Vector2.Distance(sprite.transform.position,goal)<1.0f){t.returning=false;t.resting=true;}
                }
            }
        }
        void Update()
        {
            if(game==null || game.paused || SetupOpen)return;float dt=Time.deltaTime;clock+=dt;doorWait=Mathf.Max(0,doorWait-dt);
            if(!game.LocalInputLockedV49 && !game.Dead && Input.GetKeyDown(KeyCode.Z))Command(new CaravanCommandV50{kind="whistle",x=game.body.position.x,y=game.body.position.y});
            if(Authority)
            {
                stopped=Mathf.Max(0,stopped-dt);
                foreach(var h in houses){h.health=Mathf.Min(1000,h.health+dt*2);h.window-=dt;if(h.window<=0)h.damage=0;if(h.hidden>0 && h.health>0)h.hidden=Mathf.Max(0,h.hidden-dt);if(h.damage>=250 && !h.Hidden && !Occupied(h.index))h.Hide();}
                bool campBusy=CampTickV51(dt);if(ordered && stopped<=0 && !campBusy)Move(dt);else if(!campBusy)foreach(var h in houses)h.moving=false;
                TickTroops(dt);
                if(game.CoopOnline && Time.unscaledTime>=nextSync){nextSync=Time.unscaledTime+.12f;game.coop.BroadcastV49(new LanPacketV49{kind="caravanState",body=JsonUtility.ToJson(Capture())});}
            }
            else foreach(var t in troops.Values.Where(t=>t.resting)){var sprite=Sprite(t.key);if(sprite)sprite.gameObject.SetActive(Inside==t.home);}
            foreach(var h in houses)h.Pose(clock);CampVisualsV51();
            if(clock>=nextReveal){nextReveal=clock+.7f;Reveal();}
            if(Inside>=0 && doorWait<=0 && Vector2.Distance(game.body.position,RoomDoorV51(Inside))<.50f)Leave();
            TickLure(dt);
        }
        void Reveal(){if(game.map==null)return;foreach(var h in houses)game.map.regions[0].Reveal(h.position,11);}
        void Move(float dt)
        {
            travelTimeV53+=dt;float speed=CoastGame.WalkSpeed*1.5f;var leader=houses[0];Vector2 target=routeAt<route.Count?route[routeAt]:destination;
            if(!leader.Hidden){TravelV53(leader,destination,speed,dt);if(Vector2.Distance(leader.position,target)<.3f && routeAt<route.Count)routeAt++;}
            for(int i=1;i<4;i++){var h=houses[i];var previous=houses[i-1];if(h.Hidden || previous.Hidden){h.moving=false;continue;}var delta=previous.position-h.position;if(travelTimeV53>=i*1.4f)TravelV53(h,destination,speed,dt);else h.moving=false;}
            // Arrival is completed only after all four homes have reached their camp slots.
        }
        void Step(SnailHouseV50 h,Vector2 target,float speed,float dt)
        {var from=h.position;var next=Vector2.MoveTowards(from,target,speed*dt);if(Land(next) && !game.amazons.CoastBlockedV53(from,next,.35f)){h.position=next;h.moving=(next-from).sqrMagnitude>.000001f;if(Mathf.Abs(next.x-from.x)>.001f)h.Renderer.flipX=next.x<from.x;}else h.moving=false;}
        public static bool Land(Vector2 point)=>MeadowLayout.MapBounds.Contains(point) && WorldShape.Distance(point)>1.5f;
        public static Vector2 FindLand(Vector2 point)
        {if(Land(point))return point;for(float r=2;r<100;r+=2)for(int i=0;i<24;i++){var p=point+new Vector2(Mathf.Cos(i*Mathf.PI/12),Mathf.Sin(i*Mathf.PI/12))*r;if(Land(p))return p;}return point;}
        public bool Order(Vector2 point)
        {
            if(!Land(point)){game.ui.Notice("Недоступное место");return false;}
            var planned=CoastPathV53(houses[0].position,point);
            if(planned==null){game.ui.Notice("Недоступное место");return false;}
            route.Clear();route.AddRange(planned);destination=point;ordered=true;routeAt=0;PrepareRoutesV53(point);BeginGatherV51();return true;
        }

        public void Command(CaravanCommandV50 command)
        {if(command.kind=="order"){command.known=game.map.IsVisited(MapRegion.Coast,new Vector2(command.x,command.y));if(!command.known){game.ui.Notice("Недоступное место");return;}}if(game.CoopOnline)game.coop.Send(new LanPacketV49{kind="caravanCommand",region=game.coop.Region,body=JsonUtility.ToJson(command)});else ApplyCommand(command,0,null);}
        public void ApplyCommand(CaravanCommandV50 command,int peer,string region)
        {
            if(command==null)return;
            if(command.kind=="order"){if(command.known && Seat(peer)>=0 && LanItemV49.Finite(command.x) && LanItemV49.Finite(command.y))Order(new Vector2(command.x,command.y));}
            else if(command.kind=="home")SendHome(command.key,peer);
            else if(command.kind=="recall")Recall(command.key,peer);
            else if(command.kind=="homeAll")foreach(var t in troops.Values.Where(t=>OwnerPeer(t)==peer && !t.resting && !t.returning).ToArray())SendHome(t.key,peer);
            else if(command.kind=="damage" && command.home>=0 && command.home<4 && region=="area/0" && command.amount>0 && LanItemV49.Finite(command.amount))houses[command.home].HurtLocal(Mathf.Min(10000,command.amount));
            else if(command.kind=="whistle")
            {stopped=15;var at=new Vector2(command.x,command.y);if(!game.CoopOnline || region==game.coop.Region){Lure(at);PlayWhistle();}if(game.CoopOnline)game.coop.BroadcastV49(new LanPacketV49{kind="whistle",region=region,body=JsonUtility.ToJson(command)});}
            else if(command.kind=="explode" && troops.TryGetValue(command.key??"",out var mage) && mage.key=="mage" && OwnerPeer(mage)==peer && !mage.resting && !mage.returning)
            {if(!game.CoopOnline){if(!game.journey.companion.Explode())game.ui.Notice("Недостаточно маны");}else if(game.coop.owners.TryGetValue(region,out var authority) && authority>=0)game.coop.SendToV49(authority,new LanPacketV49{kind="mageCommand",region=region,target=peer});}
        }
        public void Lure(Vector2 at)
        {
            float radius=game.cameraView.orthographicSize*4*Mathf.Max(1,game.cameraView.aspect);
            foreach(var target in game.WorldCreatures.Where(t=>t.Alive && !(t is HeroCombatActor || t is RemotePlayerV49 || t is MageCombatActor || t is SnailHouseV50 || t is PracticeTargetV46 || t is VillageActorV46 woman && !woman.Hostile)).ToArray())if(Vector2.Distance(target.Position,at)<=radius){lured[target.Renderer]=clock+15;lurePoints[target.Renderer]=at;}
        }
        public bool Lured(SpriteRenderer actor)=>actor && lured.TryGetValue(actor,out var until) && until>clock;
        void TickLure(float dt)
        {
            if(!game.SimulateWorldV49)return;
            foreach(var pair in lured.ToArray())
            {if(!pair.Key || pair.Value<=clock || !pair.Key.enabled){lured.Remove(pair.Key);continue;}var from=(Vector2)pair.Key.transform.position;var goal=lurePoints.TryGetValue(pair.Key,out var point)?point:game.body.position;var delta=goal-from;if(delta.magnitude>2.5f && !game.spellcraft.ImmobilizedV44(pair.Key)){var next=from+delta.normalized*Mathf.Min(dt*2.1f,delta.magnitude-2.5f);if(!game.WorldBlocked(from,next,.2f))pair.Key.transform.position=next;}if(game.arcane.frogs.Any(f=>f.renderer==pair.Key))game.arcane.frogs.First(f=>f.renderer==pair.Key).LureV50(game.hero);}
        }
        public CaravanStateV50 Capture()
        {foreach(var t in troops.Values){var woman=Woman(t.key);var sprite=Sprite(t.key);if(sprite){t.x=sprite.transform.position.x;t.y=sprite.transform.position.y;}t.hp=t.key=="mage"?game.journey.companion.health:woman?.health??0;t.mana=t.key=="mage"?game.journey.companion.mana:0;}return new CaravanStateV50{camp=CaptureCampV51(),count=PlayerCount,peers=peers,snails=houses.Select(h=>new SnailStateV50{x=h.position.x,y=h.position.y,hp=h.health,hidden=h.hidden,damage=h.damage,window=h.window,moving=h.moving}).ToArray(),troops=troops.Values.ToArray(),stop=stopped,x=destination.x,y=destination.y,ordered=ordered};}
        public void Apply(CaravanStateV50 state)
        {
            if(Authority || state?.snails?.Length!=4 || state.peers?.Length!=4)return;ApplyCampV51(state.camp);PlayerCount=state.count;peers=state.peers;stopped=state.stop;destination=new Vector2(state.x,state.y);ordered=state.ordered;
            for(int i=0;i<4;i++){var data=state.snails[i];var h=houses[i];h.position=Vector2.Distance(h.position,new Vector2(data.x,data.y))>10?new Vector2(data.x,data.y):Vector2.Lerp(h.position,new Vector2(data.x,data.y),.75f);h.health=data.hp;h.hidden=data.hidden;h.moving=data.moving;}
            troops.Clear();foreach(var t in state.troops??Array.Empty<TroopStateV50>()){troops[t.key]=t;if(t.returning || t.campRestV51){var sprite=Sprite(t.key);if(sprite){sprite.transform.SetParent(game.coastRoot.transform,true);if(t.key=="mage")game.journey.companion.WalkHomeV50(new Vector2(t.x,t.y));else Woman(t.key)?.WalkHomeV50(new Vector2(t.x,t.y));sprite.gameObject.SetActive(game.journey.IsCoast && !game.magic.InPocket);}}if(t.resting){var sprite=Sprite(t.key);if(sprite){sprite.transform.SetParent(houses[t.home].room.transform,true);sprite.transform.position=new Vector2(-2.1f+(troops.Count%4)*1.1f,-1.8f);sprite.gameObject.SetActive(Inside==t.home);}if(t.key=="mage"){game.journey.companion.health=t.hp;game.journey.companion.mana=t.mana;}else if(Woman(t.key)!=null)Woman(t.key).health=t.hp;}}SyncOwners();
        }
        public Renderer Interaction(Vector2 point,out string name)
        {
            name=null;if(Inside>=0){if(Vector2.Distance(point,RoomDoorV51(Inside)+Vector2.up*.6f)<1.1f){name="Выйти из улитки";return exit[Inside];}return null;}
            if(!game.journey.IsCoast || game.magic.InPocket)return null;
            foreach(var h in houses.AsEnumerable().Reverse())if(game.art.Opaque(h.Renderer.sprite,h.Renderer.transform.InverseTransformPoint(point))){name="Улитка "+(h.index+1)+(h.Hidden?" · в раковине":"");return h.Renderer;}return null;
        }
        public bool Interact(Vector2 point)
        {
            if(Inside>=0 && Vector2.Distance(point,RoomDoorV51(Inside)+Vector2.up*.6f)<1.1f){Leave();return true;}
            var renderer=Interaction(point,out _);var h=houses.FirstOrDefault(s=>s.Renderer==renderer);if(h==null)return false;
            if(!h.Hidden && new Rect(h.Door.x-.9f,h.Door.y-.5f,1.8f,2.2f).Contains(point) && Vector2.Distance(game.body.position,h.Door)<3.4f){Enter(h.index);return true;}
            game.map.OpenSnailCommand(h.position);return true;
        }
        public bool InteractAlly(Vector2 point)
        {
            foreach(var t in troops.Values.Where(t=>Own(t) && Living(t)))
            {var sprite=Sprite(t.key);if(sprite && sprite.gameObject.activeInHierarchy && Vector2.Distance(game.body.position,sprite.transform.position)<3.5f && game.art.Opaque(sprite.sprite,sprite.transform.InverseTransformPoint(point))){if(t.resting)Command(new CaravanCommandV50{kind="recall",key=t.key});else AllyOffer=t.key;return true;}}return false;
        }
        public void PlayWhistle()=>game.sounds.PlayOneShot(whistle,.8f);
        public void Enter(int home)
        {
            if(home<0 || home>=4 || houses[home].Hidden || doorWait>0)return;Inside=home;game.journey.SetArea(WorldArea.SnailHouse,RoomDoorV51(home)+Vector2.down*1.3f);doorWait=1;game.cameraView.orthographicSize=4.8f;game.ResetCamera(new Vector3(.4f,-.25f,-20));Visibility(false);
        }
        public void Leave()
        {if(Inside<0)return;var h=houses[Inside];Inside=-1;game.journey.SetArea(WorldArea.Coast,h.Door+Vector2.down*1.4f);Visibility(false);doorWait=1;}
        public void Visibility(bool pocket){foreach(var h in houses)h.room.SetActive(!pocket && game.journey.Area==WorldArea.SnailHouse && Inside==h.index);}
        public Vector2 Clamp(Vector2 point)
        {var door=RoomDoorV51(Mathf.Clamp(Inside,0,3));if(Mathf.Abs(point.x-door.x)<.6f && point.y>-.8f)return new Vector2(Mathf.Clamp(point.x,door.x-.5f,door.x+.5f),Mathf.Clamp(point.y,-.85f,.15f));var centre=new Vector2(.6f,-1.95f);var p=point-centre;float x=p.x/3.65f,y=p.y/1.4f;float length=Mathf.Sqrt(x*x+y*y);if(length>1)p/=length;return centre+p;}
        public readonly List<SpriteRenderer> exit=new List<SpriteRenderer>();
        SpriteRenderer Prop(SnailHouseV50 h,string name,Sprite sprite,Vector2 at,float width,int depth=-999)
        {var r=game.art.Renderer(name,sprite,at,depth==-999?CoastGame.Depth(at.y):depth);r.transform.SetParent(h.room.transform,true);r.transform.localScale=Vector3.one*(width/sprite.bounds.size.x);return r;}
        public void Furnish(SnailHouseV50 h)
        {
            var floor=GameObject.CreatePrimitive(PrimitiveType.Quad);Destroy(floor.GetComponent<Collider>());floor.name="Snail warm timber floor";floor.transform.SetParent(h.room.transform);floor.transform.position=new Vector3(0,0,4);floor.transform.localScale=new Vector3(8.5f,8,1);
            var renderer=floor.GetComponent<MeshRenderer>();renderer.sortingOrder=-3500;var material=new Material(Shader.Find("AshenCoast/JourneyFloor"));material.SetTexture("_GroundTex",Resources.Load<Texture2D>("Art/TerrainTiles"));material.SetTexture("_WoodTex",Resources.Load<Texture2D>("Art/TavernSurfacesV10"));material.SetFloat("_Indoor",1);renderer.sharedMaterial=material;
            Prop(h,"Soft spiral rug",furniture[5],new Vector2(0,-.7f),3.1f,-3400);
            for(int i=-1;i<=1;i++)Prop(h,"Curved shell wall and warm window",furniture[2],new Vector2(i*2.6f,2.0f),2.85f);
            Prop(h,"Small desk, candle and books",furniture[3],new Vector2(2.3f,-.4f),1.8f);
            h.personal.Add(Prop(h,"Personal bed",furniture[1],new Vector2(-2.4f,-.4f),2.15f).gameObject);
            for(int i=0;i<2;i++)h.barracks.Add(Prop(h,"Triple bunk bed",furniture[0],new Vector2(i==0?-2.6f:1.3f,.2f),2.1f).gameObject);
            exit.Add(Prop(h,"Snail exit",furniture[4],new Vector2(0,-3.7f),1.5f));
            game.buildings.CreateStation(ItemKind.StoragePot,new Vector2Int(-3,-2),false,null,h.room.transform);
        }
    }
    public sealed partial class CoastGame {public SnailHomesV50 snails;}
}
