using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Unity.Profiling;
using UnityEngine;
namespace AshenCoast
{
    public static class PerformanceSurvey
    {
        const int Samples=180;
        static double Percentile(List<double> data,float fraction){data.Sort();return data[Mathf.Clamp(Mathf.RoundToInt((data.Count-1)*fraction),0,data.Count-1)];}
        public static IEnumerator Run(CoastGame game,string directory)
        {
            long allocationStart=GC.GetAllocatedBytesForCurrentThread();var allocationProbe=new byte[65536];allocationProbe[65535]=1;GC.KeepAlive(allocationProbe);
            bool threadCounterValid=GC.GetAllocatedBytesForCurrentThread()-allocationStart>=65536;
            var rows=new List<string>{"scene,samples,frame_p50_ms,frame_p95_ms,main_thread_p95_ms,gc_main_thread_p95_bytes,draw_calls_p95,script_tick_p95_ms,loaded_flowers,managed_heap_mib,unity_allocated_mib,gc_collections"};
            var frame=new List<double>(Samples);var main=new List<double>(Samples);var gc=new List<double>(Samples);var draws=new List<double>(Samples);var ticks=new List<double>(Samples);
            using(var cpu=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",1))
            using(var allocations=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1))
            using(var drawCalls=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count",1))
            {
                var watch=new Stopwatch();
                for(int scene=0;scene<3;scene++)
                {
                    game.ui.CloseContainers();game.combat.ResetForCheck();game.checkMovement=null;game.body.linearVelocity=Vector2.zero;game.survivor.health=100;
                    game.journey.SetArea(scene==0?WorldArea.Coast:scene==1?WorldArea.GoldenField:WorldArea.Fort,scene==0?new Vector2(0,220):scene==1?new Vector2(-11,-14.4f):new Vector2(0,-4));
                    game.seconds=scene==1?930:60;game.survivor.equipment[0]=scene==1?new Item(ItemKind.Lamp):null;
                    if(scene==0)game.meadow.Refresh(game.body.position,true);
                    game.hunter.Tick(0);if(scene==1)game.hunter.Warp(game.body.position+Vector2.up*NightHunter.LampDistance);
                    game.checkCameraSize=8;game.checkCameraPosition=game.hero.transform.position+new Vector3(0,2.2f,-20);game.ResetCamera(game.checkCameraPosition.Value);
                    for(int i=0;i<30;i++)yield return null;
                    int collections=GC.CollectionCount(0);frame.Clear();main.Clear();gc.Clear();draws.Clear();ticks.Clear();
                    for(int i=0;i<Samples;i++)
                    {
                        watch.Restart();
                        if(scene==0){game.body.position=new Vector2(-6+i*.11f,220+i*.09f);game.hero.transform.position=game.body.position;game.meadow.Refresh(game.body.position);game.checkCameraPosition=game.hero.transform.position+new Vector3(0,2.2f,-20);}
                        if(scene==1)game.hunter.Tick(1f/60);
                        if(scene==2){foreach(var enemy in game.fort.bandits)enemy.Tick(1f/60);foreach(var rat in game.fort.rats)rat.Tick(1f/60);game.survivor.health=100;}
                        game.UpdateEnvironment(0);watch.Stop();ticks.Add(watch.Elapsed.TotalMilliseconds);
                        long frameAllocation=GC.GetAllocatedBytesForCurrentThread();yield return null;
                        frame.Add(Time.unscaledDeltaTime*1000);main.Add(cpu.Valid?cpu.LastValue/1000000.0:double.NaN);gc.Add(allocations.Valid?allocations.LastValue:threadCounterValid?GC.GetAllocatedBytesForCurrentThread()-frameAllocation:double.NaN);draws.Add(drawCalls.Valid?drawCalls.LastValue:double.NaN);
                    }
                    string label=scene==0?"streamed_meadow":scene==1?"night_hunter_lamp":"fort_combat";
                    rows.Add(FormattableString.Invariant($"{label},{Samples},{Percentile(frame,.5f):F3},{Percentile(frame,.95f):F3},{Percentile(main,.95f):F3},{Percentile(gc,.95f):F0},{Percentile(draws,.95f):F0},{Percentile(ticks,.95f):F3},{game.meadow.LoadedFlowers},{GC.GetTotalMemory(false)/1048576.0:F2},{UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()/1048576.0:F2},{GC.CollectionCount(0)-collections}"));
                }
                File.WriteAllLines(Path.Combine(directory,"performance-v23.csv"),rows);
                File.WriteAllText(Path.Combine(directory,"performance-machine-v22.txt"),"Unity "+Application.unityVersion+"; CPU: "+SystemInfo.processorType+"; GPU: "+SystemInfo.graphicsDeviceName+"; resolution: "+Screen.width+"x"+Screen.height+"; scripted non-development Windows player; vSync on; profiler main="+cpu.Valid+", GC="+allocations.Valid+", draw calls="+drawCalls.Valid+". Global GC allocation recorder valid="+allocations.Valid+"; validated thread allocation counter="+threadCounterValid+". NaN means the allocation metric is unavailable in this player; managed heap, engine allocations and GC collection counts are reported separately. Frame times include vSync waiting, operating-system scheduling and foreground/background effects; no claim for other computers.");
            }
            game.checkCameraSize=null;game.checkCameraPosition=null;game.survivor.health=100;game.survivor.equipment[0]=null;game.journey.SetArea(WorldArea.Coast,new Vector2(-1.5f,-1.65f));game.seconds=30;game.hunter.Tick(0);game.UpdateEnvironment(0);
        }
    }
}
