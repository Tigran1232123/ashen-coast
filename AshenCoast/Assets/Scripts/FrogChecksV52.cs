using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed class FrogChecksV52:MonoBehaviour
    {
        public CoastGame game;int passed,failed;string folder;readonly List<string> report=new List<string>();
        void Check(bool ok,string name){var line=(ok?"PASS ":"FAIL ")+name;report.Add(line);Debug.Log(line);if(ok)passed++;else failed++;}
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int arg=Array.IndexOf(args,"-frog-output");folder=arg>=0?args[arg+1]:Path.Combine(Application.persistentDataPath,"Frog-V52");Directory.CreateDirectory(folder);
            Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=60;AudioListener.volume=0;yield return null;
            var routine=Run();while(true){bool move;object current=null;try{move=routine.MoveNext();if(move)current=routine.Current;}catch(Exception e){Check(false,e.ToString());break;}if(!move)break;yield return current;}
            report.Add("RESULT "+passed+" passed; "+failed+" failed");File.WriteAllLines(Path.Combine(folder,"results.txt"),report);Application.Quit(failed==0?0:1);
        }
        IEnumerator Run()
        {
            var g=game;g.seconds=60;g.snails.SetupOpen=true;g.ChooseCharacterV51(CharacterV51.Frog);g.paused=true;Time.timeScale=0;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"character-menu.png"));yield return null;
            g.snails.SetupOpen=false;g.paused=false;Time.timeScale=1;
            // Independent expected silhouettes from the authored sheet, rather than equal columns.
            int[,] left={{199,383,564,741},{192,382,565,745},{192,387,550,761},{192,378,561,745},{194,375,559,743},{189,380,560,747},{190,377,551,746},{192,378,549,742}};
            int[,] right={{288,472,657,831},{295,471,663,837},{300,462,648,834},{293,473,656,843},{292,472,659,839},{292,471,657,834},{292,453,645,826},{297,471,656,836}};
            var root=new GameObject("V52 complete frog poses");
            for(int d=0;d<8;d++)for(int f=0;f<4;f++)
            {
                var sprite=g.v51Art.frog[d,f];var r=sprite.rect;
                Check(r.xMin<=left[d,f] && r.xMax>right[d,f] && r.xMin>=left[d,f]-2 && r.xMax<=right[d,f]+3 && r.width>=70 && r.width<=125 && r.height>=150 && r.height<=200,"complete single frog silhouette "+d+"/"+f);
                var view=g.art.Renderer("Frog view "+d+" frame "+f,sprite,new Vector2(1000+d*2,1000-f*2.5f),100);view.flipX=CoastGame.FrogFrameMirroredV52(d,f);view.gameObject.layer=31;view.transform.SetParent(root.transform,true);
            }
            Capture("frog-all-32-poses.png",new Vector2(1007,997.1f),5.7f,1<<31);Destroy(root);
            var start=new Vector2(3,-1.5f);g.body.position=start;g.hero.transform.position=start;g.checkCameraSize=4;g.checkCameraPosition=new Vector3(start.x,start.y+.9f,-20);g.ResetCamera(g.checkCameraPosition.Value);
            for(int d=0;d<8;d++)
            {
                float angle=(-90-d*45)*Mathf.Deg2Rad;g.checkMovement=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));yield return new WaitForSeconds(.32f);
                Check(g.walking && g.facing==d && g.hero.sprite==g.v51Art.frog[d,g.walkFrame] && g.hero.flipX==CoastGame.FrogFrameMirroredV52(d,g.walkFrame) && !g.gait.Lower.enabled,"actual frog movement direction "+d);
            }
            g.checkMovement=Vector2.zero;yield return null;Capture("frog-in-world.png",(Vector2)g.hero.transform.position+Vector2.up*.8f,3.5f);
            var state=g.coop.CapturePlayer();state.peer=97;state.character=1;state.region=g.coop.Region;state.health=50;state.mana=200;state.weapon=-1;state.x=g.body.position.x+3;state.y=g.body.position.y;
            var proxy=new RemotePlayerV49(g.coop,state);
            for(int d=0;d<8;d++){state.facing=d;state.frame=2;proxy.Receive(state);proxy.Tick(.1f);Check(proxy.Renderer.sprite==g.v51Art.frog[d,2] && proxy.Renderer.sprite.rect.width>=70 && proxy.Renderer.flipX==CoastGame.FrogFrameMirroredV52(d,2),"co-op frog rendering direction "+d);}
            Capture("local-and-coop-frogs.png",(Vector2)g.hero.transform.position+new Vector2(1.5f,.8f),3.5f);proxy.Dispose();
            Check(new LanPacketV49().version==52 && new LanHelloV49().build==52 && Application.version=="52.0","release and co-op build identifiers are V52");
        }
        void Capture(string name,Vector2 centre,float size,int layers=-1)
        {
            var camera=new GameObject("V52 frog visual check").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=size;camera.transform.position=new Vector3(centre.x,centre.y,-20);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.09f,.085f,.075f);camera.nearClipPlane=.1f;camera.farClipPlane=100;camera.cullingMask=layers;
            if(layers!=-1)Shader.SetGlobalColor("_CoastAmbient",new Color(.95f,.92f,.86f));
            var target=new RenderTexture(1440,1000,24);camera.targetTexture=target;camera.Render();var old=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(1440,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1440,1000),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name),image.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;target.Release();Destroy(target);Destroy(image);Destroy(camera.gameObject);
        }
    }
}
