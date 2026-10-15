using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AshenCoast
{
    [Serializable] public sealed class LanMotionV57
    {public string region;public LanActorV49[] actors;}

    public readonly struct SpriteKeyV57:IEquatable<SpriteKeyV57>
    {
        readonly string texture;readonly Rect rect;readonly Vector2 pivot;readonly float ppu;
        public SpriteKeyV57(LanRenderV49 d){texture=d.texture;rect=new Rect(d.rx,d.ry,d.rw,d.rh);pivot=new Vector2(d.px,d.py);ppu=d.ppu;}
        public bool Equals(SpriteKeyV57 other)=>texture==other.texture && rect.Equals(other.rect) && pivot.Equals(other.pivot) && ppu.Equals(other.ppu);
        public override bool Equals(object other)=>other is SpriteKeyV57 key && Equals(key);
        public override int GetHashCode(){unchecked{return (((texture?.GetHashCode()??0)*397^rect.GetHashCode())*397^pivot.GetHashCode())*397^ppu.GetHashCode();}}
    }

    public sealed partial class CoopWorldV49
    {
        static readonly Dictionary<Type,FieldInfo[]> fieldListsV57=new Dictionary<Type,FieldInfo[]>();
        static readonly Dictionary<Type,Dictionary<string,FieldInfo>> fieldNamesV57=new Dictionary<Type,Dictionary<string,FieldInfo>>();
        static FieldInfo[] NetworkFieldsV57(Type type)
        {
            if(!fieldListsV57.TryGetValue(type,out var fields))
            {fields=type.GetFields(Fields);fieldListsV57[type]=fields;fieldNamesV57[type]=fields.GroupBy(f=>f.Name).ToDictionary(g=>g.Key,g=>g.First());}
            return fields;
        }
        static FieldInfo NetworkFieldV57(Type type,string name)
        {NetworkFieldsV57(type);return fieldNamesV57[type].TryGetValue(name,out var field)?field:null;}
        static string ChildPathV57(Transform child,Transform root)
        {
            if(child==root)return "body";
            int ordinal=0;var parent=child.parent;
            for(int i=0;i<parent.childCount;i++){var sibling=parent.GetChild(i);if(sibling==child)break;if(sibling.name==child.name)ordinal++;}
            string leaf=child.name.Replace("/","_")+"#"+ordinal;
            return parent==root?"child/"+leaf:ChildPathV57(parent,root)+"/"+leaf;
        }
        public void RefreshPartsV57(CoopBindingV49 binding)
        {
            var main=binding.renders[0];binding.partsV57.Clear();binding.partsV57["body"]=main;
            if(binding.driver!=G)foreach(var field in NetworkFieldsV57(binding.driver.GetType()))
            {
                if(field.FieldType!=typeof(SpriteRenderer) || !(field.GetValue(binding.driver) is SpriteRenderer renderer) || !renderer || renderer==main)continue;
                string name=field.Name.ToLowerInvariant();
                // References to an enemy, aggressor, player or light target aren't owned visuals.
                bool owned=renderer.transform.IsChildOf(main.transform) || name.Contains("shadow") || name.Contains("weapon") || name=="blade" || name=="cage";
                if(owned)binding.partsV57[renderer.transform.IsChildOf(main.transform)?ChildPathV57(renderer.transform,main.transform):"field/"+field.Name]=renderer;
            }
            foreach(var child in main.GetComponentsInChildren<SpriteRenderer>(true))
                if(child && child!=main)binding.partsV57[ChildPathV57(child.transform,main.transform)]=child;
            binding.renders=new[]{main}.Concat(binding.partsV57.Where(p=>p.Key!="body").OrderBy(p=>p.Key,StringComparer.Ordinal).Select(p=>p.Value)).ToArray();
        }
        LanRenderV49[] CapturePartsV57(CoopBindingV49 binding)
        {
            RefreshPartsV57(binding);
            var result=new List<LanRenderV49>(binding.partsV57.Count);
            foreach(var entry in binding.partsV57)
                if(entry.Value && entry.Value.sprite){var data=LanRenderV49.Capture(entry.Value);data.part=entry.Key;result.Add(data);}
            return result.ToArray();
        }
        LanActorV49 CaptureActorV57(CoopBindingV49 binding,bool fields)
        {
            return new LanActorV49{key=binding.key,type=binding.kind,region=binding.region,health=binding.health(),
                bound=StatusV49(binding.renders[0],false),curse=StatusV49(binding.renders[0],true),stun=G.v44.RemainingV49(binding.renders[0]),
                anchorX=binding.renders[0].transform.position.x,anchorY=binding.renders[0].transform.position.y,
                owner=allyOwners.TryGetValue(binding.key,out var peer)?peer:-1,
                hired=binding.driver is VillageActorV46 v?v.Hired:binding.driver is CompanionMage m && m.Hired,
                renders=CapturePartsV57(binding),fields=binding.driver!=G?CaptureFields(binding.driver,fields):null};
        }
        public LanMotionV57 CaptureMotionV57(string region)
        {
            var points=session.players.Values.Where(p=>p.region==region).Select(p=>new Vector2(p.x,p.y)).ToArray();
            if(points.Length==0)points=new[]{G.body.position};
            float radius=Mathf.Max(36,G.cameraView.orthographicSize*G.cameraView.aspect+10);
            var actors=entities.Values.Where(b=>b.region==region && b.renders[0] && !G.snails.Resting(b.renders[0]) && !G.snails.Returning(b.renders[0]) &&
                points.Any(p=>((Vector2)b.renders[0].transform.position-p).sqrMagnitude<radius*radius)).Select(b=>CaptureActorV57(b,false)).ToArray();
            return new LanMotionV57{region=region,actors=actors};
        }
        public void ApplyActorsV57(LanActorV49[] actors,string region,bool takeover)
        {
            if(actors==null || actors.Length>1000 || !ValidRegion(region))throw new InvalidOperationException("Invalid actor frame");
            foreach(var data in actors)
            {
                if(data?.renders==null || data.renders.Length==0)continue;
                var binding=Find(data.key);
                if(binding==null && (data.type=="plain-bow" || data.type=="plain-sword"))
                {var pose=data.renders.First(r=>r.part=="body");var actor=G.amazons.plains.Spawn(new Vector2(pose.x,pose.y),data.type=="plain-bow");Add(data.key,data.type,region,actor,actor,actor.Renderer,()=>actor.health,v=>actor.health=v);binding=Find(data.key);}
                if(binding==null && data.type=="dragon")
                {var pose=data.renders.First(r=>r.part=="body");var actor=new RoosterDragon(G.dragons,new Vector2(pose.x,pose.y),Root(region));G.dragons.bosses.Add(actor);Add(data.key,"dragon",region,actor,actor,actor.Renderer,()=>actor.health,v=>actor.health=v);binding=Find(data.key);}
                if(binding==null || binding.kind!=data.type)continue;
                float previous=binding.health();ApplyFields(binding.driver,data.fields);binding.region=region;binding.setHealth(Mathf.Clamp(data.health,0,100000));
                if(previous>data.health && binding.target!=null)G.feedback.Hit(binding.renders[0],previous-data.health);
                if(data.owner>=0)
                {allyOwners[binding.key]=data.owner;if(!allyRegions.ContainsKey(binding.key))allyRegions[binding.key]=region;if(binding.driver is VillageActorV46 woman){woman.Hired=true;woman.Captive=false;if(woman.cage)woman.cage.enabled=false;}binding.renders[0].transform.SetParent(Root(region),true);}
                else if(!data.hired){allyOwners.Remove(binding.key);allyRegions.Remove(binding.key);}
                ApplyStatusV49(binding,data,takeover);binding.received=data;
                RefreshPartsV57(binding);
                var root=data.renders.FirstOrDefault(r=>r.part=="body");if(root==null)continue;
                var main=binding.renders[0];bool snap=takeover || Vector2.Distance(main.transform.position,new Vector2(root.x,root.y))>6;
                var present=new HashSet<string>(data.renders.Select(pose=>pose.part));
                foreach(var part in binding.partsV57)if(part.Value && !present.Contains(part.Key))part.Value.enabled=false;
                foreach(var pose in data.renders)
                    if(pose.part!=null && binding.partsV57.TryGetValue(pose.part,out var renderer) && renderer)ApplyRender(renderer,pose,false);
                PoseAssemblyV57(binding,root,snap?1:0);
                foreach(var collider in main.GetComponents<Collider2D>())collider.enabled=(data.health>0 || binding.kind=="tree" || binding.kind=="target") && !(binding.driver is TavernResident resident && collider==resident.feet && !G.journey.TavernAlarm);
            }
        }
        void PoseAssemblyV57(CoopBindingV49 binding,LanRenderV49 body,float blend)
        {
            var main=binding.renders[0];var target=new Vector3(body.x,body.y,body.z);
            var point=Vector3.Lerp(main.transform.position,target,blend);
            main.transform.position=point;main.transform.rotation=Quaternion.Euler(0,0,body.angle);
            // One interpolated anchor drives the complete character. Children never chase world positions independently.
            foreach(var pose in binding.received.renders)
                if(pose.part!="body" && pose.part!=null && binding.partsV57.TryGetValue(pose.part,out var renderer) && renderer)
                {renderer.transform.position=point+new Vector3(pose.x-body.x,pose.y-body.y,pose.z-body.z);renderer.transform.rotation=Quaternion.Euler(0,0,pose.angle);}
        }
    }

    public sealed partial class JourneyWorld
    {
        public void ApplyAlarmV57(bool alarm)
        {
            if(alarm){if(!TavernAlarm)AlarmTavern();return;}
            if(!TavernAlarm)return;TavernAlarm=false;
            foreach(var resident in residents)
            {
                if(resident.feet)resident.feet.enabled=false;if(resident.blade)resident.blade.enabled=false;
                if(resident.walkingGaitV53?.Lower)resident.walkingGaitV53.Lower.enabled=false;
                var properties=new MaterialPropertyBlock();resident.Renderer.GetPropertyBlock(properties);properties.SetFloat("_BodyFloor",0);properties.SetFloat("_SeatedLegs",0);resident.Renderer.SetPropertyBlock(properties);
            }
            TickActors();
        }
    }

    public sealed partial class CoopSessionV49
    {
        readonly Dictionary<string,Dictionary<string,LanActorV49>> motionSnapshotsV57=new Dictionary<string,Dictionary<string,LanActorV49>>();
        float nextMotionV57,nextAuthorityV57;
        public int FullFramesV57{get;private set;}public int MotionFramesV57{get;private set;}
        public static string LatestKeyV57(LanPacketV49 packet)
        {
            switch(packet.kind)
            {case "player":return "player/"+packet.peer;case "world":case "motion":return packet.kind+"/"+packet.region;case "fx":return "fx/"+packet.peer+"/"+packet.region;case "time":return "time";default:return null;}
        }
        void SendWireV57(int peer,LanPacketV49 packet)=>transport?.Send(peer,JsonUtility.ToJson(packet),LatestKeyV57(packet));
        void BroadcastWireV57(LanPacketV49 packet,int except=-1)=>transport?.Broadcast(JsonUtility.ToJson(packet),except,LatestKeyV57(packet));
        void ReceiveMotionV57(LanPacketV49 packet)
        {
            var frame=JsonUtility.FromJson<LanMotionV57>(packet.body);
            if(frame==null || frame.region!=packet.region || !world.ValidRegion(frame.region) || frame.actors==null || frame.actors.Length>1000)throw new InvalidOperationException("Invalid motion packet");
            if(!motionSnapshotsV57.TryGetValue(frame.region,out var latest))motionSnapshotsV57[frame.region]=latest=new Dictionary<string,LanActorV49>();
            foreach(var actor in frame.actors)if(actor?.key!=null)latest[actor.key]=actor;
            if(Online && (packet.region==Region || IsHost) && packet.peer!=Peer)
            {ApplyingNetwork=true;try{world.ApplyActorsV57(frame.actors,frame.region,false);}finally{ApplyingNetwork=false;}}
        }
        string SnapshotV57(string region,string json)
        {
            if(!motionSnapshotsV57.TryGetValue(region,out var latest) || latest.Count==0)return json;
            var full=JsonUtility.FromJson<LanRegionV49>(json);
            foreach(var entry in full.actors.Select((actor,index)=>new{actor,index}).ToArray())
                if(latest.TryGetValue(entry.actor.key,out var motion))
                {full.actors[entry.index]=new LanActorV49{key=motion.key,type=motion.type,region=motion.region,health=motion.health,bound=motion.bound,curse=motion.curse,stun=motion.stun,anchorX=motion.anchorX,anchorY=motion.anchorY,owner=motion.owner,hired=motion.hired,renders=motion.renders,fields=motion.fields==null?entry.actor.fields:(entry.actor.fields??Array.Empty<LanFieldV49>()).Where(f=>f.type==4).Concat(motion.fields).ToArray()};}
            return JsonUtility.ToJson(full);
        }
    }
}
