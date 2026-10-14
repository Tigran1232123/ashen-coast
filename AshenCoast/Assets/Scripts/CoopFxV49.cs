using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
namespace AshenCoast
{
    [Serializable] public sealed class LanMaterialV49
    {public string shader;public LanPropertyV49[] properties;}
    [Serializable] public sealed class LanPropertyV49
    {public string name,texture;public int type;public float scalar;public Vector4 vector,transform;}
    [Serializable] public sealed class LanFxV49
    {
        public int id,kind,order;public string name;public Vector3 position,scale;public float angle,width;public LanRenderV49 sprite;
        public Vector3[] vertices;public Vector2[] uv;public int[] triangles;public Color[] colors;public Color start,end;public LanMaterialV49 material;
    }
    [Serializable] public sealed class LanFxFrameV49 {public LanFxV49[] effects;}
    // These replicas render an effect only. Its damage remains on its originating player or location authority.
    public sealed class CoopFxV49:IDisposable
    {
        readonly CoopSessionV49 session;readonly Dictionary<string,Renderer> replicas=new Dictionary<string,Renderer>();
        readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();readonly Dictionary<int,Renderer[]> sources=new Dictionary<int,Renderer[]>();
        float nextScan,nextSend;Renderer[] active=Array.Empty<Renderer>();readonly HashSet<Renderer> tracked=new HashSet<Renderer>();
        public void Track(Renderer renderer){if(renderer)tracked.Add(renderer);}
        readonly Dictionary<Renderer,int> ids=new Dictionary<Renderer,int>();int nextId;
        static readonly string[] names={"Animated Spectrum","Rolling waterfall","Dithered rainbow reflected","Rainbow falling","Rainbow landing","Spectrum comet","Rainbow departure","White-blue celestial","Curse outgoing","Brown tentacle","Broken earth beneath","Sixty-second black abyss","Spinning battle yo-yo","Yo-yo taut","Concentrated Last Prism","Converging Last Prism","Turbulent lamp","Pixel shockwave","Arc of golden blast","Wide dragon invocation","Dragon downward fire","Rising pixel ash","Temporary scorched","Golden Sting / stun","Silken web","Bandit arrow","Fort arrow","Fired arrow","Sparring spark"};
        static readonly HashSet<string> shaders=new HashSet<string>{"AshenCoast/Rainbow","AshenCoast/RainbowMist","AshenCoast/AbyssFog","AshenCoast/PrismBeam","AshenCoast/BlastRing"};
        public int ReplicaCount=>replicas.Count;
        public CoopFxV49(CoopSessionV49 owner){session=owner;}
        bool IsSource(Renderer r)
        {
            if(!r || r.gameObject.name.StartsWith("LAN effect /"))return false;
            return tracked.Contains(r) || names.Any(n=>r.gameObject.name.StartsWith(n,StringComparison.Ordinal)) || r.sharedMaterial && shaders.Contains(r.sharedMaterial.shader.name);
        }
        public void Tick()
        {
            if(!session.Online)return;
            if(Time.unscaledTime>=nextScan){nextScan=Time.unscaledTime+.3f;active=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(IsSource).ToArray();}
            if(Time.unscaledTime>=nextSend)
            {
                nextSend=Time.unscaledTime+.1f;
                var list=new List<LanFxV49>();foreach(var r in active)
                {
                    if(!r || !r.enabled || !r.gameObject.activeInHierarchy || !r.sharedMaterial)continue;
                    // Remote fog mechanics have no renderer. Non-authorities don't re-broadcast status visuals.
                    if(!session.CanSimulate && (r.name.StartsWith("White-blue") || r.name.StartsWith("Curse outgoing") || r.name.StartsWith("Golden Sting") || r.name.StartsWith("Brown tentacle") || r.name.StartsWith("Broken earth beneath")))continue;
                    var effect=Capture(r);if(effect!=null)list.Add(effect);if(list.Count==240)break;
                }
                session.Send(new LanPacketV49{kind="fx",region=session.Region,body=JsonUtility.ToJson(new LanFxFrameV49{effects=list.ToArray()})});
            }
        }
        LanFxV49 Capture(Renderer r)
        {
            if(!ids.TryGetValue(r,out var id)){id=++nextId;ids[r]=id;}
            var result=new LanFxV49{id=id,name=r.name,position=r.transform.position,scale=r.transform.lossyScale,angle=r.transform.eulerAngles.z,order=r.sortingOrder,material=CaptureMaterial(r)};
            if(r is SpriteRenderer sprite){if(!sprite.sprite)return null;result.kind=0;result.sprite=LanRenderV49.Capture(sprite);}
            else if(r is LineRenderer line){result.kind=1;result.vertices=new Vector3[line.positionCount];line.GetPositions(result.vertices);if(!line.useWorldSpace)for(int i=0;i<result.vertices.Length;i++)result.vertices[i]=line.transform.TransformPoint(result.vertices[i]);result.width=line.widthMultiplier;result.start=line.startColor;result.end=line.endColor;}
            else
            {
                var mesh=r.GetComponent<MeshFilter>()?.sharedMesh;if(!mesh || mesh.vertexCount>2000)return null;
                result.kind=2;result.vertices=mesh.vertices;result.uv=mesh.uv;result.triangles=mesh.triangles;result.colors=mesh.colors;
            }
            return result;
        }
        static LanMaterialV49 CaptureMaterial(Renderer r)
        {
            var mat=r.sharedMaterial;var shader=mat.shader;var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);var properties=new List<LanPropertyV49>();
            for(int i=0;i<shader.GetPropertyCount();i++)
            {
                string name=shader.GetPropertyName(i);int id=Shader.PropertyToID(name);var type=shader.GetPropertyType(i);var p=new LanPropertyV49{name=name};
                if(type==ShaderPropertyType.Texture){p.type=2;var texture=block.HasProperty(id)?block.GetTexture(id):mat.GetTexture(id);p.texture=texture?texture.name:"";var s=mat.GetTextureScale(name);var o=mat.GetTextureOffset(name);p.transform=new Vector4(s.x,s.y,o.x,o.y);}
                else if(type==ShaderPropertyType.Color){p.type=1;p.vector=block.HasProperty(id)?(Vector4)block.GetColor(id):(Vector4)mat.GetColor(id);}
                else if(type==ShaderPropertyType.Vector){p.type=1;p.vector=block.HasProperty(id)?block.GetVector(id):mat.GetVector(id);}
                else{p.type=0;p.scalar=block.HasProperty(id)?block.GetFloat(id):mat.GetFloat(id);}
                properties.Add(p);
            }
            return new LanMaterialV49{shader=shader.name,properties=properties.ToArray()};
        }
        Texture2D Texture(string name)=>string.IsNullOrEmpty(name)?null:Resources.Load<Texture2D>("Art/"+name)??Texture2D.whiteTexture;
        Sprite Sprite(LanRenderV49 d)
        {
            string key=d.texture+":"+d.rx+":"+d.ry+":"+d.rw+":"+d.rh+":"+d.ppu+":"+d.px+":"+d.py;
            if(sprites.TryGetValue(key,out var s))return s;var t=Texture(d.texture);if(!t || d.rw<=0 || d.rh<=0 || d.rx<0 || d.ry<0 || d.rx+d.rw>t.width+.1f || d.ry+d.rh>t.height+.1f || d.ppu<=0)return null;
            s=UnityEngine.Sprite.Create(t,new Rect(d.rx,d.ry,d.rw,d.rh),new Vector2(d.px,d.py),d.ppu,0,SpriteMeshType.FullRect);sprites[key]=s;return s;
        }
        public void Receive(int peer,string region,string json)
        {
            if(peer==session.Peer)return;var data=JsonUtility.FromJson<LanFxFrameV49>(json);if(data?.effects==null || data.effects.Length>240)return;
            string prefix=peer+"/";var present=new HashSet<string>();
            if(region==session.Region)foreach(var d in data.effects)
            {
                if(d.material==null || d.material.properties?.Length>100 || d.vertices?.Length>2000 || d.triangles?.Length>12000)continue;
                string key=prefix+d.id;present.Add(key);if(!replicas.TryGetValue(key,out var r))
                {
                    var go=new GameObject("LAN effect / "+d.name);go.transform.SetParent(session.world.Root(region),false);
                    if(d.kind==0)r=go.AddComponent<SpriteRenderer>();else if(d.kind==1)r=go.AddComponent<LineRenderer>();else{go.AddComponent<MeshFilter>().sharedMesh=new Mesh();r=go.AddComponent<MeshRenderer>();}
                    var shader=Shader.Find(d.material.shader);if(!shader){UnityEngine.Object.Destroy(go);continue;}r.sharedMaterial=new Material(shader);replicas[key]=r;
                }
                r.transform.position=d.position;r.transform.localScale=d.kind==1?Vector3.one:d.scale;r.transform.rotation=d.kind==1?Quaternion.identity:Quaternion.Euler(0,0,d.angle);r.sortingOrder=d.order;
                if(r is SpriteRenderer sr){sr.sprite=Sprite(d.sprite);sr.flipX=d.sprite.flipX;sr.flipY=d.sprite.flipY;sr.color=new Color(d.sprite.r,d.sprite.g,d.sprite.b,d.sprite.a);}
                else if(r is LineRenderer line){line.useWorldSpace=true;line.positionCount=d.vertices.Length;line.SetPositions(d.vertices);line.widthMultiplier=d.width;line.startColor=d.start;line.endColor=d.end;line.textureMode=LineTextureMode.Stretch;}
                else{var mesh=r.GetComponent<MeshFilter>().sharedMesh;mesh.Clear();mesh.vertices=d.vertices;mesh.uv=d.uv;if(d.colors?.Length==d.vertices.Length)mesh.colors=d.colors;mesh.triangles=d.triangles;mesh.RecalculateBounds();}
                foreach(var p in d.material.properties??Array.Empty<LanPropertyV49>())
                {if(!r.sharedMaterial.HasProperty(p.name))continue;if(p.type==0)r.sharedMaterial.SetFloat(p.name,p.scalar);else if(p.type==1)r.sharedMaterial.SetVector(p.name,p.vector);else{r.sharedMaterial.SetTexture(p.name,Texture(p.texture));r.sharedMaterial.SetTextureScale(p.name,new Vector2(p.transform.x,p.transform.y));r.sharedMaterial.SetTextureOffset(p.name,new Vector2(p.transform.z,p.transform.w));}}
            }
            foreach(var key in replicas.Keys.Where(k=>k.StartsWith(prefix) && !present.Contains(k)).ToArray())Remove(key);
        }
        public void RemovePeer(int peer){foreach(var k in replicas.Keys.Where(k=>k.StartsWith(peer+"/")).ToArray())Remove(k);}
        public void RegionChanged(){foreach(var k in replicas.Keys.ToArray())Remove(k);}
        void Remove(string key){if(replicas.TryGetValue(key,out var r) && r){if(r.sharedMaterial)UnityEngine.Object.Destroy(r.sharedMaterial);var mesh=r.GetComponent<MeshFilter>()?.sharedMesh;if(mesh)UnityEngine.Object.Destroy(mesh);UnityEngine.Object.Destroy(r.gameObject);}replicas.Remove(key);}
        public void Dispose(){RegionChanged();foreach(var s in sprites.Values)if(s)UnityEngine.Object.Destroy(s);sprites.Clear();}
    }
}
