using UnityEngine;

namespace AshenCoast
{
    public sealed partial class ExplorationMap
    {
        public bool TravelSelection {get;private set;}
        public bool RainbowTravel {get;private set;}
        public float TravelRadius {get;private set;}
        Vector2 travelOrigin;
        public bool OpenTravel(float radius,bool rainbow)
        {
            if(!SetOpen(true))return false;TravelSelection=true;RainbowTravel=rainbow;TravelRadius=radius;travelOrigin=game.body.position;
            Zoom=radius>0?Mathf.Min(12,Mathf.Min(game.ui.CanvasWidth-240,game.ui.CanvasHeight-350)/(radius*2.1f)):12;
            game.ui.Notice(rainbow?"Выберите место для полёта на радуге":"Выберите точку пространственного перехода");return true;
        }
        bool SelectTravel(Vector2 canvasPoint)
        {
            if(!TravelSelection)return false;
            var point=Centre+new Vector2(canvasPoint.x-Plot.center.x,Plot.center.y-canvasPoint.y)/Zoom;
            if(!RainbowTravel && Region!=CurrentRegion || TravelRadius>0 && Vector2.Distance(point,travelOrigin)>TravelRadius || !game.spellcraft.CanLand(Region,point))
            {game.ui.Notice("Недоступная точка карты");return false;}
            bool rainbow=RainbowTravel;TravelSelection=false;SetOpen(false);
            if(rainbow)game.spectrum.BeginRide(point,Region);else game.spellcraft.StartTeleport(point,true);
            return true;
        }
        void DrawTravelRadius()
        {
            if(!TravelSelection || Region!=CurrentRegion || TravelRadius<=0)return;
            var centre=MapPoint(travelOrigin);float radius=TravelRadius*Zoom;
            for(int i=0;i<240;i++){float a=i*Mathf.PI*2/240;Fill(new Rect(centre.x+Mathf.Cos(a)*radius-1,centre.y+Mathf.Sin(a)*radius-1,3,3),new Color(.45f,.77f,1,.85f));}
        }
    }
}
