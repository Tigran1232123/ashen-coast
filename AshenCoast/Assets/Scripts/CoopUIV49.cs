using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class InventoryUI
    {
        string lanPortV49="27849",lanIpsV49;
        void DrawCoopUIV49()
        {
            var session=game.coop;if(session==null)return;
            if(session.Online && Event.current.type==EventType.Repaint)
            {
                foreach(var peer in session.remote.Values.Where(p=>p.State.region==session.Region && p.Alive))
                {
                    var at=World(peer.Position+Vector2.up*2.05f);if(at.x<45 || at.x>CanvasWidth*WorldFraction-45 || at.y<30 || at.y>CanvasHeight-35)continue;
                    var box=new Rect(at.x-80,at.y-24,160,24);GUI.color=new Color(.03f,.035f,.02f);GUI.Label(new Rect(box.x+1,box.y+1,box.width,box.height),peer.State.name,CachedStyle(bold,TextAnchor.MiddleCenter,16));GUI.color=new Color(.78f,.86f,.68f);GUI.Label(box,peer.State.name,CachedStyle(bold,TextAnchor.MiddleCenter,16));GUI.color=Color.white;
                    var bar=new Rect(at.x-32,at.y+1,64,4);Fill(bar,new Color(.06f,.07f,.05f));Fill(new Rect(bar.x,bar.y,bar.width*peer.State.health/100,bar.height),new Color(.34f,.60f,.35f));
                }
                GUI.Label(new Rect(CanvasWidth*WorldFraction-155,CanvasHeight-35,140,25),"Кооператив · "+session.players.Count+"/"+game.snails.PlayerCount,CachedStyle(small,TextAnchor.MiddleRight));
            }
            if(!session.MenuOpen)return;
            Fill(new Rect(0,0,CanvasWidth,CanvasHeight),new Color(.025f,.03f,.025f,.8f));var r=new Rect(CanvasWidth/2-235,CanvasHeight/2-235,470,470);Window(r,"Кооператив · сеть / VPN");
            if(Button(new Rect(r.xMax-36,r.y+10,26,26),"×")){session.MenuOpen=false;return;}
            GUI.Label(new Rect(r.x+22,r.y+55,105,28),"Имя",text);session.PlayerName=GUI.TextField(new Rect(r.x+135,r.y+55,310,28),session.PlayerName,20,text);
            GUI.Label(new Rect(r.x+22,r.y+96,105,28),"Адрес",text);session.Address=GUI.TextField(new Rect(r.x+135,r.y+96,310,28),session.Address,64,text);
            GUI.Label(new Rect(r.x+22,r.y+137,105,28),"Порт",text);lanPortV49=GUI.TextField(new Rect(r.x+135,r.y+137,110,28),lanPortV49,5,text);
            if(!session.Online && !session.Connecting){GUI.Label(new Rect(r.x+265,r.y+137,75,28),"Игроков",small);for(int i=1;i<=4;i++)if(Button(new Rect(r.x+342+(i-1)*25,r.y+137,24,28),i.ToString(),game.snails.PlayerCount==i))game.snails.Configure(i);}
            int port=int.TryParse(lanPortV49,out var value) && value>1024 && value<65536?value:LanTransportV49.DefaultPort;
            if(!session.Online && !session.Connecting)
            {if(Button(new Rect(r.x+22,r.y+185,200,42),"Создать игру"))session.StartHost(port);if(Button(new Rect(r.x+248,r.y+185,200,42),"Подключиться"))session.Join(session.Address,port);}
            else if(Button(new Rect(r.x+22,r.y+185,426,42),session.Connecting?"Отменить":"Отключиться"))session.Disconnect();
            GUI.Label(new Rect(r.x+22,r.y+240,426,28),session.Status,text);
            if(session.IsHost)
            {if(lanIpsV49==null){try{lanIpsV49=string.Join(", ",NetworkInterface.GetAllNetworkInterfaces().Where(n=>n.OperationalStatus==OperationalStatus.Up).SelectMany(n=>n.GetIPProperties().UnicastAddresses).Select(a=>a.Address).Where(a=>a.AddressFamily==AddressFamily.InterNetwork && !IPAddress.IsLoopback(a)).Select(a=>a.ToString()).Distinct());}catch{lanIpsV49="";}}GUI.Label(new Rect(r.x+22,r.y+271,426,35),"Адрес ведущего: "+lanIpsV49,CachedStyle(small,TextAnchor.MiddleLeft,13,FontStyle.Normal,true));}
            int row=0;foreach(var peer in session.players.Values.OrderBy(p=>p.peer))GUI.Label(new Rect(r.x+22,r.y+315+row++*29,426,28),peer.name+(peer.peer==0?" · ведущий":"")+(peer.peer==session.Peer?" · вы":""),text);
        }
    }
}
