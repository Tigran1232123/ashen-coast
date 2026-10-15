using UnityEngine;
namespace AshenCoast
{
    public sealed class CoastCamera
    {
        readonly CoastGame game;Vector3 cameraFollow;
        float shakeLeft,shakeDuration,shakePower;
        public int Shakes {get;private set;}
        public void Shake(float power,float duration){shakeLeft=shakeDuration=duration;shakePower=power;Shakes++;}
        public CoastCamera(CoastGame owner){game=owner;cameraFollow=game.cameraView.transform.position;}
        public void Reset(Vector3 position){cameraFollow=position;game.cameraView.transform.position=position;}
        public void Tick()
        {
            if(!game.cameraView || !game.hero) return;

            if(game.journey.Cutscene){game.cameraView.transform.position=game.journey.CutsceneCamera;cameraFollow=game.journey.CutsceneCamera;return;}
            float visible=game.ui.WorldFraction;
            if(game.magic.InPocket)
                game.cameraView.orthographicSize=Mathf.Max(7.8f,6.4f/(game.cameraView.aspect*visible));
            else if(game.journey.Area==WorldArea.SnailHouse)game.cameraView.orthographicSize=5.25f;else if(game.journey.Interior)game.cameraView.orthographicSize=6.25f;else if(!game.journey.IsCoast)game.cameraView.orthographicSize=8;
            if(game.hunter!=null && game.hunter.Active && game.hunter.HeldBack && Vector2.Distance(game.hunter.Position,game.body.position)<12)game.cameraView.orthographicSize=9;
            if(game.testMode && game.checkCameraSize.HasValue)game.cameraView.orthographicSize=game.checkCameraSize.Value;
            bool fortView=game.journey.InFort && !game.magic.InPocket && !(game.testMode && (game.checkCameraSize.HasValue || game.checkCameraPosition.HasValue));
            if(fortView)
                game.cameraView.orthographicSize=Mathf.Min(game.cameraView.orthographicSize,(FortWorld.FloorBounds.width-.6f)/(2*game.cameraView.aspect*visible));
            float width=game.cameraView.orthographicSize*2*game.cameraView.aspect;
            float startBias=game.magic.InPocket?0:-.4f*Mathf.Clamp01(1-Vector2.Distance(game.hero.transform.position,new Vector2(-1.5f,-1.65f))/10);
            float offset=game.ui.vendorOpen?0:width*(1-visible)/2;
            var target=game.journey.Area==WorldArea.SnailHouse && !game.magic.InPocket?new Vector3(offset,-.6f,-20):game.magic.InPocket?new Vector3(offset,.65f,-20):game.journey.Interior?new Vector3(.5f+offset,.35f,-20):game.hero.transform.position+new Vector3(game.ui.open?offset:startBias,2.2f,-20);
            if(game.hunter!=null && game.hunter.Active && game.hunter.HeldBack && Vector2.Distance(game.hunter.Position,game.body.position)<12)target+=(Vector3)(game.hunter.Position-game.body.position)*.6f;
            if(game.testMode && game.checkCameraPosition.HasValue)target=game.checkCameraPosition.Value;
            target.z=-20;
            if(fortView)target=FortCentre(target,visible);
            cameraFollow=Vector3.Lerp(cameraFollow,target,1-Mathf.Exp(-Time.unscaledDeltaTime*6));
            if(fortView)cameraFollow=FortCentre(cameraFollow,visible);
            float unit=2*game.cameraView.orthographicSize/game.pixelView.pixels.height;
            game.cameraView.transform.position=new Vector3(Mathf.Round(cameraFollow.x/unit)*unit,Mathf.Round(cameraFollow.y/unit)*unit,-20);
            if(shakeLeft>0 && !game.paused)
            {
                shakeLeft=Mathf.Max(0,shakeLeft-Time.deltaTime);float t=shakeDuration-shakeLeft,fade=shakeLeft/Mathf.Max(.001f,shakeDuration);
                var wobble=new Vector3(Mathf.Sin(t*93+1.3f),Mathf.Sin(t*117+.7f),0)*shakePower*fade*fade;
                game.cameraView.transform.position+=new Vector3(Mathf.Round(wobble.x/unit)*unit,Mathf.Round(wobble.y/unit)*unit,0);
            }
        }
        Vector3 FortCentre(Vector3 centre,float visible)
        {
            var floor=FortWorld.FloorBounds;float half=game.cameraView.orthographicSize,wide=half*game.cameraView.aspect;
            // The right-hand inventory covers that part of the world. Clamp the
            // visible viewport, so the hero can still be seen beside the east wall.
            float margin=half*4/game.pixelView.pixels.height;
            centre.x=Mathf.Clamp(centre.x,floor.xMin+wide+margin,floor.xMax-wide*(2*visible-1)-margin);
            centre.y=Mathf.Clamp(centre.y,floor.yMin+half+margin,floor.yMax-half-margin);
            return centre;
        }
    }
}
