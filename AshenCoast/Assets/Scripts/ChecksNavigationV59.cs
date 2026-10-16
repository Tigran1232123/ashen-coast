using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed class ChecksNavigationV59:MonoBehaviour
    {
        readonly List<string> results=new List<string>();int failed;
        void Check(bool ok,string name){results.Add((ok?"PASS ":"FAIL ")+name);if(!ok)failed++;}
        IEnumerator Start()
        {
            yield return null;var g=FindFirstObjectByType<CoastGame>();g.snails.SetupOpen=false;g.snails.enabled=false;g.paused=false;g.testMode=true;Time.timeScale=1;AudioListener.volume=0;
            Check(Application.version=="59.0" && LanProtocolV49.Build==59,"V59 application and handshake version");
            var v=g.amazons;Check(v.AuditNavigationV59(),"all 25921 cached cells match exact ground and obstacle geometry");
            foreach(var h in v.houses){var start=h.entrance+Vector2.down*.6f;var goal=v.Centre+Vector2.down*9.5f;AmazonSettlementV46.ReferenceNavigationV59=true;var original=v.PathV48(start,goal);AmazonSettlementV46.ReferenceNavigationV59=false;var optimized=v.PathV48(start,goal);Check(original.Count>0 && original.SequenceEqual(optimized),"house route unchanged "+h.entrance);}
            var tree=v.trees.First(t=>t.Standing);for(int i=0;i<10;i++)tree.Chop(tree.position+Vector2.down);yield return null;Check(!tree.Standing && v.AuditNavigationV59(),"cut tree invalidates cached obstacles on next frame");
            RiverV58.Configure(v.Centre+Vector2.right*20);Check(v.AuditNavigationV59(),"changed river geometry invalidates cache");RiverV58.Configure(v.Centre);
            for(int i=0;i<3;i++){var c=RiverV58.Centre(i,RiverV58.BridgeT(i));Check(RiverV58.Passable(c) && !RiverV58.Passable(RiverV58.Centre(i,RiverV58.BridgeT(i)+.04f)),"bridge passes and adjacent water blocks "+i);}
            int seed=WorldShape.Seed;WorldShape.Configure(seed+1);Check(v.AuditNavigationV59(),"changed world seed invalidates cache");
            var args=Environment.GetCommandLineArgs();results.Add("RESULT "+(results.Count-failed)+" passed; "+failed+" failed");File.WriteAllLines(args[Array.IndexOf(args,"-performance-output")+1],results);Application.Quit(failed==0?0:1);
        }
    }
    public sealed class ChecksNavigationNetworkV59:MonoBehaviour
    {
        string folder,role;CoastGame game;int failed;readonly List<string> results=new List<string>();
        void Check(bool ok,string name){results.Add((ok?"PASS ":"FAIL ")+name);if(!ok)failed++;}
        bool Seen(string name)=>File.Exists(Path.Combine(folder,name));void Mark(string name)=>File.WriteAllText(Path.Combine(folder,name),"ready");
        IEnumerator Until(Func<bool> ready,string label){float end=Time.realtimeSinceStartup+40;while(!ready() && Time.realtimeSinceStartup<end)yield return null;Check(ready(),label);}
        string Routes(){var v=game.amazons;return string.Join(";",v.houses.Select(h=>string.Join("/",v.PathV48(h.entrance+Vector2.down*.6f,v.Centre+Vector2.down*9.5f).Select(p=>Mathf.RoundToInt(p.x*100)+","+Mathf.RoundToInt(p.y*100)))));}
        IEnumerator Start()
        {
            yield return null;var args=Environment.GetCommandLineArgs();folder=args[Array.IndexOf(args,"-v59-output")+1];role=args[Array.IndexOf(args,"-v59-role")+1];game=FindFirstObjectByType<CoastGame>();game.snails.SetupOpen=false;game.paused=false;game.testMode=true;Time.timeScale=1;AudioListener.volume=0;
            if(role=="host"){Check(game.coop.StartHost(27859,true),"host starts V59");File.WriteAllText(Path.Combine(folder,"host-routes"),Routes());Mark("host-ready");yield return Until(()=>game.coop.remote.Count==1,"V59 guest joins");yield return Until(()=>Seen("guest-done"),"guest completed seed and path checks");}
            else{yield return Until(()=>Seen("host-ready"),"host ready");game.coop.Join("127.0.0.1",27859);yield return Until(()=>game.CoopOnline && game.coop.inventory.Ready,"V59 handshake and inventories synchronized");Check(WorldShape.Seed==74391,"guest adopts host seed");Check(game.amazons.AuditNavigationV59(),"guest cache matches adopted world");Check(Routes()==File.ReadAllText(Path.Combine(folder,"host-routes")),"host and guest obtain same ten house routes");Mark("guest-done");}
            results.Add("RESULT "+(results.Count-failed)+" passed; "+failed+" failed");File.WriteAllLines(Path.Combine(folder,role+"-results.txt"),results);game.coop.Disconnect(false);Application.Quit(failed==0?0:1);
        }
    }
}
