using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    // Runs only with an explicit developer benchmark argument; never changes a normal session.
    public sealed class PerformanceProbeV59:MonoBehaviour
    {
        public static bool Measuring;
        public static long PathTicks;public static int PathCalls;
        readonly List<string> report=new List<string>();CoastGame game;Texture2D readback;bool render=true;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach(){var args=Environment.GetCommandLineArgs();if(args.Contains("-performance-probe"))new GameObject("Performance probe").AddComponent<PerformanceProbeV59>();if(args.Contains("-navigation-check"))new GameObject("Navigation checks").AddComponent<ChecksNavigationV59>();if(args.Contains("-navigation-network-check"))new GameObject("Navigation network checks").AddComponent<ChecksNavigationNetworkV59>();}
        IEnumerator Start()
        {
            yield return null;game=FindFirstObjectByType<CoastGame>();game.snails.SetupOpen=false;game.paused=false;Time.timeScale=1;AudioListener.volume=0;QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;Application.runInBackground=true;
            report.Add("GPU "+SystemInfo.graphicsDeviceName+"; CPU "+SystemInfo.processorType+"; screen "+Screen.width+"x"+Screen.height);
            readback=new Texture2D(1,1,TextureFormat.RGBA32,false);foreach(var cam in Camera.allCameras)cam.enabled=false;
            var reference=new List<List<Vector2>>();var watch=System.Diagnostics.Stopwatch.StartNew();AmazonSettlementV46.ReferenceNavigationV59=true;
            foreach(var h in game.amazons.houses)reference.Add(game.amazons.PathV48(h.entrance+Vector2.down*.6f,game.amazons.Centre+Vector2.down*9.5f));
            watch.Stop();report.Add("Reference house routes: "+watch.Elapsed.TotalMilliseconds.ToString("F1")+"ms");AmazonSettlementV46.ReferenceNavigationV59=false;watch.Restart();bool same=true;int route=0;
            foreach(var h in game.amazons.houses){var path=game.amazons.PathV48(h.entrance+Vector2.down*.6f,game.amazons.Centre+Vector2.down*9.5f);same&=path.SequenceEqual(reference[route++]);}
            watch.Stop();report.Add("Cached house routes: "+watch.Elapsed.TotalMilliseconds.ToString("F1")+"ms; exact same paths="+same);
            yield return Measure("spawn / normal");
            game.testMode=true;game.snails.enabled=false;yield return Measure("spawn / AI disabled");
            game.testMode=false;game.snails.enabled=true;var p=game.amazons.Centre+Vector2.down*12;game.journey.SetArea(WorldArea.Coast,p);game.body.position=p;game.hero.transform.position=p;game.ResetCamera(new Vector3(p.x,p.y,-20));
            yield return Measure("village / normal");
            game.testMode=true;game.snails.enabled=false;yield return Measure("village / AI disabled");
            render=false;game.testMode=false;game.snails.enabled=true;yield return Measure("village / rendering disabled");
            render=true;yield return Measure("village / normal after warmup",15);
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-performance-output");var output=at<0?Application.dataPath+"/../performance.txt":args[at+1];File.WriteAllLines(output,report);Application.Quit(0);
        }
        IEnumerator Measure(string name,float seconds=4)
        {
            yield return new WaitForSecondsRealtime(1.5f);PathTicks=0;PathCalls=0;Measuring=true;var times=new List<float>();float start=Time.realtimeSinceStartup,prior=start;
            while(Time.realtimeSinceStartup-start<seconds){if(render){game.cameraView.Render();var old=RenderTexture.active;RenderTexture.active=game.pixelView.pixels;readback.ReadPixels(new Rect(0,0,1,1),0,0,false);RenderTexture.active=old;}yield return null;float now=Time.realtimeSinceStartup;times.Add((now-prior)*1000);prior=now;}
            Measuring=false;times.Sort();float elapsed=Time.realtimeSinceStartup-start;var line=name+": FPS="+(times.Count/elapsed).ToString("F1")+" p50="+times[times.Count/2].ToString("F1")+"ms p95="+times[Mathf.Min(times.Count-1,(int)(times.Count*.95f))].ToString("F1")+"ms max="+times.Last().ToString("F1")+"ms slow33="+times.Count(t=>t>33)+"; navigation="+(PathTicks*1000.0/System.Diagnostics.Stopwatch.Frequency/elapsed).ToString("F1")+"ms/sec calls="+PathCalls;report.Add(line);Debug.Log(line);
        }
    }
}
