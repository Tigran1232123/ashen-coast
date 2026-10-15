using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class SnailChecksV50:MonoBehaviour
    {
        public CoastGame game;int passed,failed;string folder;
        void Check(bool value,string name){if(value)passed++;else failed++;Debug.Log((value?"PASS ":"FAIL ")+name);}
        IEnumerator Start()
        {
            yield return null;folder=Environment.GetEnvironmentVariable("ASHEN_SNAIL_CHECK")??Path.Combine(Application.persistentDataPath,"Snail-V50");Directory.CreateDirectory(folder);
            if(Environment.GetCommandLineArgs().Contains("-snail-rest-check")){yield return NetworkRest();yield break;}
            if(Environment.GetCommandLineArgs().Contains("-snail-lan-check")){yield return Network();yield break;}
            var c=game.snails;game.testMode=true;c.SetupOpen=false;
            Check(c.houses.Count==4 && c.houses.All(h=>SnailHomesV50.Land(h.position)),"four houses start on land above the cart");
            for(int n=1;n<=4;n++){c.Configure(n);c.peers=new[]{0,1,2,3};for(int p=0;p<n;p++){int owned=Enumerable.Range(0,4).Count(h=>c.CanUse(h,p));Check(owned==(n==1?4:n==2?2:n==3?2:1),"ownership distribution "+n+" players, seat "+p);}}
            c.Configure(1);c.peers=new[]{0,-1,-1,-1};var women=game.amazons.people.Where(p=>p.IsSlave || p.Hunter).Take(5).ToArray();var mage=game.journey.companion;mage.Hire(game.body.position);c.Assign("mage",1,0);
            foreach(var woman in women){woman.Hired=true;woman.Captive=false;c.Assign(c.Key(woman.Renderer),1,0);}
            Check(c.Cost("mage")==3 && c.Cost(c.Key(women.First(w=>w.Hunter).Renderer))==2 && c.Cost(c.Key(women.First(w=>w.IsSlave).Renderer))==1,"mage amazon and captive capacity weights");
            Check(c.Used(1)<=10 && !c.Available(0,"mage",0),"personal solo room has no barracks places");
            mage.health=20;mage.mana=5;mage.Warp(c.houses[1].Door+Vector2.down*.5f);c.SendHome("mage",0);yield return new WaitForSeconds(1.5f);Check(c.troops["mage"].resting && mage.health>20 && mage.mana>5,"sending mage home heals health and mana");
            var house=c.houses[1];house.HurtLocal(251);Check(house.Hidden && !c.troops["mage"].resting && !c.troops["mage"].returning,"heavy damage retracts shell and instantly ejects troops");
            house.hidden=0;house.damage=0;house.health=1000;c.Enter(1);house.HurtLocal(300);Check(!house.Hidden && game.journey.Area==WorldArea.SnailHouse && game.arcane.WorldRoot==house.room.transform,"occupied snail cannot retract and has isolated room root");c.Leave();
            house.hidden=0;house.damage=0;Check(c.Order(c.houses[0].position+Vector2.right*24),"map order finds traversable route");var before=c.houses[0].position;yield return new WaitForSeconds(.8f);Check(Vector2.Distance(before,c.houses[0].position)>3,"snail crawl speed is 1.5 player speed");
            c.Command(new CaravanCommandV50{kind="whistle",x=game.body.position.x,y=game.body.position.y});before=c.houses[0].position;yield return new WaitForSeconds(.5f);Check(c.stopped>14 && (before-c.houses[0].position).sqrMagnitude<.001f,"whistle stops the entire caravan for fifteen seconds");
            int i=0;foreach(var kind in ItemDefinition.All.Keys){var at=new Vector2(40+(i%10)*4,60+(i/10)*4);var drop=game.CreateLooseLootV46(new Item(kind),at);game.TickLoot(0);var r=drop.renderer;Check(r.bounds.size.x>0 && r.bounds.size.y>0 && r.bounds.size.magnitude<10 && (kind==ItemKind.Spectrum || kind==ItemKind.Zen || Mathf.Abs(r.bounds.center.x-at.x)<.05f),"ground footprint "+kind);i++;}
            Capture("snail-houses.png",new Vector2(8,game.journey.CartPosition.y+17),17);c.Enter(2);Capture("snail-barracks.png",Vector2.zero,5.3f);c.Leave();yield return new WaitForSeconds(1.1f);c.Enter(0);Capture("snail-personal.png",Vector2.zero,5.3f);c.Leave();Capture("ground-items.png",new Vector2(58,70),16);
            File.WriteAllText(Path.Combine(folder,"results.txt"),"V50 focused snail and ground checks: "+passed+" passed; "+failed+" failed\n");Debug.Log("SNAIL_CHECK_RESULT "+passed+" "+failed);Application.Quit(failed==0?0:1);
        }
        void Capture(string name,Vector2 centre,float size)
        {
            var camera=new GameObject("Focused visual check camera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=size;camera.transform.position=new Vector3(centre.x,centre.y,-20);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.08f,.08f,.05f);camera.nearClipPlane=.1f;camera.farClipPlane=100;
            var target=new RenderTexture(1440,900,24);camera.targetTexture=target;camera.Render();var old=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(1440,900,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1440,900),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name),image.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;target.Release();Destroy(target);Destroy(image);Destroy(camera.gameObject);
        }
    }
}
