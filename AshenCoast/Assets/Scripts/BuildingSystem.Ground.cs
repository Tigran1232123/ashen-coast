using System.Collections.Generic;
using UnityEngine;

namespace AshenCoast
{
    public sealed class BuildingClearing
    {
        public Rect plants;
        public RectInt cells;
        public Transform world;
        public MeshRenderer ground;
    }
    public sealed partial class BuildingSystem
    {
        public readonly List<BuildingClearing> clearings=new List<BuildingClearing>();
        Material groundMaterial;
        public bool IsCleared(Vector2 point,Transform world)
        {
            foreach(var c in clearings)if(c.world==world && c.plants.Contains(point))return true;
            return false;
        }
        public void CreateClearing(Vector2Int cell,Vector2Int size,Transform world,bool scorched)
        {
            if(world!=game.coastRoot.transform && world!=game.journey.fieldRoot.transform)return;
            var centre=new Vector2(cell.x+size.x*.5f,cell.y+size.y*.5f);
            var clearing=new BuildingClearing{cells=new RectInt(cell,size),world=world,
                plants=new Rect(cell.x-.46f,cell.y-.70f,size.x+.92f,size.y+1.12f)};
            clearings.Add(clearing);
            foreach(var flower in game.flowers)if(!flower.picked && flower.renderer.transform.IsChildOf(world) && clearing.plants.Contains(flower.position))
            {
                flower.picked=true;flower.renderer.enabled=false;flower.stems.enabled=false;flower.shadow.enabled=false;game.meadow.RecordPickup(flower);
                if(game.hoveredFlower==flower)game.HighlightAt(null);
            }
            if(!groundMaterial){groundMaterial=new Material(Shader.Find("AshenCoast/BuildingGround"));groundMaterial.SetTexture("_GroundTex",game.art.Load("TerrainTiles"));}
            var go=GameObject.CreatePrimitive(PrimitiveType.Quad);go.name="Trampled earth beneath "+cell;Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(world,false);go.transform.position=new Vector3(centre.x,centre.y,1);go.transform.localScale=new Vector3(size.x+.48f,size.y+.48f,1);
            clearing.ground=go.GetComponent<MeshRenderer>();clearing.ground.sharedMaterial=groundMaterial;clearing.ground.sortingOrder=-3000;
            var properties=new MaterialPropertyBlock();properties.SetVector("_ClearingRect",new Vector4(centre.x,centre.y,size.x*.5f,size.y*.5f));properties.SetFloat("_Charred",scorched?1:0);clearing.ground.SetPropertyBlock(properties);
        }
        void OnDestroy(){if(groundMaterial)Destroy(groundMaterial);}
    }
}
