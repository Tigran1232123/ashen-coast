using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    [Serializable] public sealed class VillageCheckManifestV54 {public int seed,mob;public Vector2[] houses;public Vector2 player;}
    public sealed partial class ChecksV54
    {
        void Mark(string name)=>File.WriteAllText(Path.Combine(folder,name),"ready");
        bool Marked(string name)=>File.Exists(Path.Combine(folder,name));
        IEnumerator Until(Func<bool> condition,string message,float duration=25)
        {float until=Time.realtimeSinceStartup+duration;while(!condition() && Time.realtimeSinceStartup<until)yield return null;Check(condition(),message);}
        IEnumerator HostV54()
        {
            var g=game;var v=g.amazons;g.journey.SetArea(WorldArea.Coast,v.Centre+new Vector2(-1,-3));g.body.position=v.Centre+new Vector2(-1,-3);g.hero.transform.position=g.body.position;
            var mob=g.woodlice.First(m=>!m.Small);mob.renderer.transform.position=v.Centre+new Vector2(5,-3);mob.vitals.health=200;mob.vitals.curlRemaining=0;
            Check(g.coop.StartHost(27954,true),"V54 host starts");File.WriteAllText(Path.Combine(folder,"manifest.json"),JsonUtility.ToJson(new VillageCheckManifestV54{seed=v.LayoutSeedV54,houses=v.houses.Select(h=>h.entrance).ToArray(),mob=g.woodlice.IndexOf(mob),player=g.body.position}));Mark("host-ready");
            yield return Until(()=>g.coop.players.Count==2,"V54 guest joins");yield return Until(()=>Marked("guest-layout"),"guest confirms matching random layout");
            yield return Until(()=>g.coop.players.Values.Any(p=>p.region=="house/3"),"guest independently enters redesigned weaver home");Check(g.journey.IsCoast,"host stays outdoors while guest is inside");Mark("host-house-seen");
            yield return Until(()=>mob.vitals.health<200,"client-owned ballistic arrow reaches authoritative creature",35);Check(Mathf.Abs(mob.vitals.health-164)<.1f,"host applies bow damage exactly once");Mark("host-hit");
            yield return Until(()=>Marked("guest-done"),"guest receives damage result");g.coop.Disconnect(false);
        }
        IEnumerator GuestV54()
        {
            var g=game;yield return Until(()=>Marked("host-ready"),"host ready");var manifest=JsonUtility.FromJson<VillageCheckManifestV54>(File.ReadAllText(Path.Combine(folder,"manifest.json")));g.coop.Join("127.0.0.1",27954);
            yield return Until(()=>g.coop.Online && g.coop.Peer>0,"V54 guest connects");Check(g.amazons.LayoutSeedV54==manifest.seed && g.amazons.houses.Where((h,i)=>Vector2.Distance(h.entrance,manifest.houses[i])>.001f).Count()==0,"welcome synchronizes every random household position");Mark("guest-layout");
            g.amazons.EnterHouse(g.amazons.houses[3]);yield return Until(()=>g.coop.CanSimulate,"guest owns its independent house simulation");Check(g.amazons.RoomRootV49(3).GetComponentsInChildren<SpriteRenderer>().Any(r=>r.sprite.texture.name=="AmazonHome3V54"),"client sees matching weaver interior");yield return Until(()=>Marked("host-house-seen"),"host sees independent house visit");g.amazons.LeaveHouse();g.body.position=manifest.player;g.hero.transform.position=g.body.position;yield return new WaitForSeconds(.6f);
            Check(!g.coop.CanSimulate,"host owns shared outdoor creature simulation");var bow=new Item(ItemKind.Bow);g.survivor.equipment[0]=bow;g.combat.readyAt=0;Check(g.amazons.ShootPlayer(Vector2.right*6),"non-authority player fires a real bow");
            float until=Time.realtimeSinceStartup+5;while(Time.realtimeSinceStartup<until && !Marked("host-hit")){Call(g.amazons,"TickArrows",Time.deltaTime);yield return null;}
            yield return Until(()=>g.woodlice[manifest.mob].vitals.health<200,"authoritative arrow damage synchronizes to shooter");Check(Mathf.Abs(g.woodlice[manifest.mob].vitals.health-164)<.1f,"guest sees exactly 36 damage");Mark("guest-done");yield return new WaitForSeconds(.4f);g.coop.Disconnect(false);
        }
    }
}
