using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public static class VesselWaterV43
    {
        public static bool Vessel(ItemKind kind)=>kind==ItemKind.StoragePot || kind==ItemKind.Cauldron;
        public static bool Pour(Item bottle,GridInventory vessel)
        {
            if(bottle?.kind!=ItemKind.WaterBottle || bottle.water<1 || vessel==null || !vessel.containerKind.HasValue || !Vessel(vessel.containerKind.Value) || vessel.waterRows>=vessel.rows)return false;
            bottle.water-=1;vessel.waterRows++;return true;
        }
        public static bool Take(Item bottle,GridInventory vessel)
        {
            if(bottle?.kind!=ItemKind.WaterBottle || bottle.water>=2 || vessel==null || !vessel.containerKind.HasValue || !Vessel(vessel.containerKind.Value) || vessel.waterRows<=0)return false;
            bottle.water+=1;vessel.waterRows--;return true;
        }
    }
    public sealed partial class InventoryUI
    {
        bool TryWaterAtPointerV44()
        {
            if(dragItem?.kind!=ItemKind.WaterBottle || dragMerchant)return false;
            GridInventory vessel=null;bool overWater=false;
            if(stationOpen!=null && VesselWaterV43.Vessel(stationOpen.kind) && StationPanel.Contains(Pointer))
            {
                vessel=stationOpen.inventory;var waterView=grids.FirstOrDefault(g=>g.inventory==vessel);
                overWater=waterView!=null && waterView.rect.Contains(Pointer) && Pointer.y>=waterView.rect.y+(vessel.rows-vessel.waterRows)*waterView.cell;
            }
            if(vessel==null)
            {
                var view=grids.FirstOrDefault(g=>g.accessible && g.rect.Contains(Pointer));
                if(view!=null){var cell=view.Cell(Pointer);view=VisibleGrid(view,cell);var pot=view.inventory.At(cell.x,cell.y);if(pot!=null && VesselWaterV43.Vessel(pot.kind))vessel=pot.contents;}
            }
            if(vessel==null && !IsPointerOver)
            {
                var point=(Vector2)game.cameraView.ViewportToWorldPoint(new Vector3(Pointer.x/CanvasWidth,1-Pointer.y/CanvasHeight,20));var station=game.buildings.At(point);
                if(station!=null && VesselWaterV43.Vessel(station.kind) && Vector2.Distance(game.body.position,station.Position)<=3.1f)vessel=station.inventory;
            }
            if(vessel==null)return false;
            bool taking=dragItem.water==0 || overWater && dragItem.water<2;
            if(taking?VesselWaterV43.Take(dragItem,vessel):VesselWaterV43.Pour(dragItem,vessel))game.PlayPickup();else Notice(taking?"Нет воды":dragItem.water<1?"Бутылка пуста":"Посуда наполнена");
            dragPress=Pointer;return true;
        }
    }
}
