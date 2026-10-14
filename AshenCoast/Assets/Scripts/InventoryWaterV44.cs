using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class InventoryUI
    {
        sealed class WaterViewV44 {public RenderTexture image;public Material material;}
        readonly Dictionary<GridInventory,WaterViewV44> waterViewsV44=new Dictionary<GridInventory,WaterViewV44>();
        WaterViewV44 vortexV44;
        static WaterViewV44 CreateWaterViewV44(int width,int height)
        {
            var image=new RenderTexture(width,height,0,RenderTextureFormat.ARGB32){filterMode=FilterMode.Point};image.Create();
            return new WaterViewV44{image=image,material=new Material(Shader.Find("AshenCoast/InventoryMagicV44"))};
        }
        void DrawWaterV44(GridView view)
        {
            var inv=view.inventory;if(inv.waterRows<=0 || !inv.containerKind.HasValue || !VesselWaterV43.Vessel(inv.containerKind.Value))return;
            if(!waterViewsV44.TryGetValue(inv,out var water)){water=CreateWaterViewV44(Mathf.RoundToInt(view.rect.width),Mathf.RoundToInt(view.rect.height));waterViewsV44.Add(inv,water);}
            var m=water.material;m.SetFloat("_Mode",0);m.SetFloat("_Phase",Time.time);m.SetFloat("_Rows",inv.rows);m.SetFloat("_WaterRows",inv.waterRows);m.SetFloat("_Boil",stationOpen!=null && stationOpen.inventory==inv && stationOpen.Heated?1:0);
            var target=RenderTexture.active;Graphics.Blit(Texture2D.whiteTexture,water.image,m);RenderTexture.active=target;GUI.DrawTexture(view.rect,water.image,ScaleMode.StretchToFill,true);
        }
        void DrawVortexV44(Rect area,float alpha)
        {
            if(vortexV44==null)vortexV44=CreateWaterViewV44(48,48);
            vortexV44.material.SetFloat("_Mode",1);vortexV44.material.SetFloat("_Phase",Time.time);var target=RenderTexture.active;Graphics.Blit(Texture2D.whiteTexture,vortexV44.image,vortexV44.material);RenderTexture.active=target;
            GUI.color=new Color(1,1,1,alpha);GUI.DrawTexture(area,vortexV44.image,ScaleMode.StretchToFill,true);GUI.color=Color.white;
        }
        void DisposeWaterV44()
        {
            foreach(var view in waterViewsV44.Values.Concat(vortexV44==null?new WaterViewV44[0]:new[]{vortexV44})){view.image.Release();Destroy(view.image);Destroy(view.material);}waterViewsV44.Clear();vortexV44=null;
        }
    }
}
