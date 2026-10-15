using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class InventoryUI
    {
        public Rect SnailPanelV50=>new Rect(CanvasWidth/2-290,CanvasHeight/2-195,580,390);
        public bool OverSnailV50=>game.snails!=null && (game.snails.SetupOpen || game.snails.Choosing || game.snails.AllyOffer!=null || SnailHUDRectsV50.Any(r=>r.Contains(Pointer)));
        readonly System.Collections.Generic.List<Rect> SnailHUDRectsV50=new System.Collections.Generic.List<Rect>();
        void DrawSnailsV50()
        {
            var caravan=game.snails;if(caravan==null)return;SnailHUDRectsV50.Clear();
            if(caravan.SetupOpen)
            {
                Fill(new Rect(0,0,CanvasWidth,CanvasHeight),new Color(.025f,.025f,.018f,.86f));var r=SnailPanelV50;Window(r,"Берег обломков · V50");
                GUI.Label(new Rect(r.x+20,r.y+65,r.width-40,34),"Сколько игроков будет в мире?",centered);
                for(int i=1;i<=4;i++)if(Button(new Rect(r.x+45+(i-1)*125,r.y+125,110,56),i==1?"Один":i+" игрока",caravan.PlayerCount==i))caravan.Configure(i);
                GUI.Label(new Rect(r.x+24,r.y+211,r.width-48,44),caravan.PlayerCount==1?"Четыре улитки принадлежат вам":"Улитки распределены на "+caravan.PlayerCount+" игроков",centered);
                if(Button(new Rect(r.x+95,r.y+300,390,46),"Начать")){caravan.SetupOpen=false;game.paused=false;Time.timeScale=1;}
                return;
            }
            var following=caravan.Following.ToArray();float x=CanvasWidth/2-following.GroupBy(t=>t.home).Sum(a=>84+Mathf.Min(10,a.Count())*58+12)/2f;
            foreach(var group in following.GroupBy(t=>t.home).OrderBy(t=>t.Key))
            {
                var r=new Rect(x,22,84+Mathf.Min(10,group.Count())*58,100);SnailHUDRectsV50.Add(r);Frame(r,dark);
                var image=new Rect(r.x+8,r.y+12,66,54);ArtBank.DrawIcon(caravan.shells[0],image,Color.white);GUI.Label(new Rect(r.x+8,r.y+69,66,25),"№ "+(group.Key+1),centered);
                if(Event.current.type==EventType.MouseDown && Event.current.button==0 && image.Contains(Pointer)){caravan.Command(new CaravanCommandV50{kind="homeAll"});Event.current.Use();}
                int i=0;foreach(var troop in group.Take(10))
                {
                    var at=new Rect(r.x+79+i++*58,r.y+8,50,70);var sprite=caravan.Sprite(troop.key);if(sprite)ArtBank.DrawIcon(sprite.sprite,at,Color.white);
                    float hp=troop.key=="mage"?game.journey.companion.health:caravan.Woman(troop.key).health,max=troop.key=="mage"?CompanionMage.MaxHealth:caravan.Woman(troop.key).MaxHealth;
                    Fill(new Rect(at.x,r.y+81,50,4),new Color(.12f,.10f,.07f));Fill(new Rect(at.x,r.y+81,50*hp/max,4),new Color(.58f,.24f,.18f));
                    if(troop.key=="mage"){Fill(new Rect(at.x,r.y+89,50,4),new Color(.10f,.12f,.17f));Fill(new Rect(at.x,r.y+89,50*game.journey.companion.mana/CompanionMage.MaxMana,4),new Color(.25f,.47f,.71f));if(Event.current.type==EventType.MouseDown && Event.current.button==0 && at.Contains(Pointer)){caravan.Command(new CaravanCommandV50{kind="explode",key="mage"});Event.current.Use();}}
                }
                x+=r.width+12;
            }
            if(caravan.Inside>=0)
            {
                int home=caravan.Inside;var guests=caravan.troops.Values.Where(t=>t.home==home && t.resting && caravan.Own(t) && caravan.Living(t)).ToArray();
                if(guests.Length>0){var r=new Rect(25,CanvasHeight-130,290,90);SnailHUDRectsV50.Add(r);Frame(r,dark);GUI.Label(new Rect(r.x+12,r.y+8,266,27),"Улитка "+(home+1)+" · "+caravan.Used(home)+"/10",centered);if(Button(new Rect(r.x+14,r.y+42,262,35),"Позвать войска"))foreach(var t in guests)caravan.Command(new CaravanCommandV50{kind="recall",key=t.key});}
            }
            if(caravan.AllyOffer!=null)
            {
                var key=caravan.AllyOffer;var r=SnailPanelV50;Window(r,key=="mage"?"Союзный маг":caravan.Woman(key)?.Name??"Союзник");
                if(Button(new Rect(r.xMax-39,r.y+12,26,26),"×")){caravan.AllyOffer=null;return;}
                if(Button(new Rect(r.x+40,r.y+85,r.width-80,46),"Отправить в улитку")){caravan.Command(new CaravanCommandV50{kind="home",key=key});caravan.AllyOffer=null;}
                if(key!="mage" && caravan.Woman(key)?.IsSlave==true && Button(new Rect(r.x+40,r.y+151,r.width-80,46),"Оружие")){OpenAllyWeaponV46(caravan.Woman(key));caravan.AllyOffer=null;}
                return;
            }
            if(caravan.Choosing)
            {
                var r=SnailPanelV50;Window(r,"Выберите улитку для воина");
                if(Button(new Rect(r.xMax-39,r.y+12,26,26),"×")){caravan.Choosing=false;return;}
                for(int i=0;i<4;i++)
                {
                    var h=caravan.houses[i];var row=new Rect(r.x+25,r.y+65+i*72,r.width-50,60);ArtBank.DrawIcon(caravan.shells[h.Hidden?1:0],new Rect(row.x,row.y,80,60),Color.white);
                    int capacity=caravan.Capacity(i);string label="Улитка "+(i+1)+" · "+caravan.Used(i)+"/"+capacity+(caravan.OwnerSeat(i)<0?" · общая":"");
                    if(Button(new Rect(row.x+95,row.y+8,row.width-95,45),label,false,caravan.Available(i,caravan.HireKey,caravan.LocalPeer)))caravan.FinishHire(i);
                }
            }
            foreach(var h in caravan.houses)if(h.Renderer.gameObject.activeInHierarchy && (h.Hidden || game.feedback.Recent(h.Renderer)))ActorVital(World(h.position+Vector2.up*5),"Улитка "+(h.index+1),h.health,1000,new Color(.37f,.53f,.29f),h.Renderer);
        }
    }
    public sealed partial class ExplorationMap
    {
        public bool SnailCommandV50;
        public void OpenSnailCommand(Vector2 at){if(!SetOpen(true))return;SnailCommandV50=true;Region=MapRegion.Coast;Centre=at;Zoom=8;}
        bool SelectSnailV50(Vector2 canvas)
        {if(!SnailCommandV50 || Region!=MapRegion.Coast)return false;var point=Centre+new Vector2(canvas.x-Plot.center.x,Plot.center.y-canvas.y)/Zoom;if(!SnailHomesV50.Land(point)){game.ui.Notice("Недоступное место");return false;}game.snails.Command(new CaravanCommandV50{kind="order",x=point.x,y=point.y});SetOpen(false);return true;}
        void DrawSnailMapV50()
        {
            if(Region==MapRegion.Coast && game.snails!=null)foreach(var h in game.snails.houses)
            {
                var p=MapPoint(h.position);p=new Vector2(Mathf.Clamp(p.x,22,Plot.width-22),Mathf.Clamp(p.y,22,Plot.height-22));
                if(Zoom>=3)Landmark(h.position,"", "Улитка "+(h.index+1),new Color(.77f,.70f,.44f),true);ArtBank.DrawIcon(game.snails.shells[h.Hidden?1:0],new Rect(p.x-20,p.y-15,40,30),Color.white);GUI.Label(new Rect(p.x-15,p.y+9,30,22),(h.index+1).ToString(),centered);
            }
        }
    }
}
