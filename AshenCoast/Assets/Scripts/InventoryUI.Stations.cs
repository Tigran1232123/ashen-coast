using UnityEngine;

namespace AshenCoast
{
    public sealed partial class InventoryUI
    {
        void DrawStation()
        {
            var station=stationOpen;var panel=StationPanel;Window(panel,station.inventory.name);
            if(Button(new Rect(panel.xMax-37,panel.y+10,26,26),"×")){CloseStation();Cancel();return;}
            var grid=new GridView{inventory=station.inventory,cell=42,rect=new Rect(panel.x+92,panel.y+50,station.inventory.columns*42,station.inventory.rows*42)};
            grids.Add(grid);DrawGrid(grid);
            if(station.kind==ItemKind.CraftingTable)
            {
                
                DrawHammer(new Vector2(panel.x+42,panel.y+100));return;
            }
            if(VesselWaterV43.Vessel(station.kind)){if(station.Heated){var meter=new Rect(panel.x+35,panel.y+60,20,165);Frame(meter,new Color(.07f,.06f,.04f));Fill(new Rect(meter.x+4,meter.y+4+(meter.height-8)*(1-station.ready),meter.width-8,(meter.height-8)*station.ready),new Color(.76f,.43f,.16f));GUI.Label(new Rect(panel.x+12,panel.y+232,70,24),Mathf.RoundToInt(station.ready*100)+"%",centered);}return;}
            var stem=new Rect(panel.x+37,panel.y+64,19,161);Frame(stem,new Color(.075f,.075f,.075f));
            float hot=Mathf.Clamp01((station.temperature-20)/980);
            Fill(new Rect(stem.x+5,stem.y+5+(stem.height-10)*(1-hot),9,(stem.height-10)*hot),Color.Lerp(new Color(.55f,.18f,.07f),new Color(1,.77f,.16f),hot));
            Frame(new Rect(stem.x-4,stem.yMax-3,27,23),new Color(.27f+hot*.55f,.11f+hot*.18f,.055f));
            for(int i=0;i<=5;i++)Fill(new Rect(stem.x+stem.width+4,stem.y+5+i*(stem.height-10)/5,6+(i%2)*4,1),gold);
            GUI.Label(new Rect(panel.x+12,panel.y+239,74,22),Mathf.RoundToInt(station.temperature)+"°",centered);
            
        }
    }
}
