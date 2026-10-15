using UnityEngine;
namespace AshenCoast
{
    public sealed class AmazonArtV46
    {
        public readonly Sprite[] props=new Sprite[6];
        public readonly Sprite[,] women=new Sprite[4,6];
        public Sprite statue;
        public AmazonArtV46(ArtBank bank)
        {
            var sheet=bank.Load("AmazonVillageV46");
            int split=Mathf.RoundToInt(sheet.height*602f/1024);
            for(int i=0;i<6;i++)props[i]=bank.Slice(sheet,new RectInt(i%3*sheet.width/3,i<3?0:split,sheet.width/3,i<3?split:sheet.height-split),new Vector2(.5f,0),100);
            var goddess=bank.Load("AlvielStatueV54");statue=bank.Slice(goddess,new RectInt(0,0,goddess.width,goddess.height),new Vector2(.5f,0),100);
            var actors=bank.Load("AmazonActorsV46");
            int[] rows={0,271,533,788,1024};
            for(int row=0;row<4;row++)
            {
                var frames=new Sprite[6];float height=0;
                for(int frame=0;frame<6;frame++){frames[frame]=bank.Slice(actors,new RectInt(frame*actors.width/6,rows[row]*actors.height/1024,actors.width/6,(rows[row+1]-rows[row])*actors.height/1024),new Vector2(.5f,0),100);height=Mathf.Max(height,frames[frame].rect.height);}
                for(int frame=0;frame<6;frame++){var s=frames[frame];women[row,frame]=Sprite.Create(s.texture,s.rect,new Vector2(.5f,0),height/1.72f,0,SpriteMeshType.FullRect);}
            }
            var weapons=bank.Load("AmazonWeaponsV46");
            for(int i=0;i<3;i++)bank.icons[54+i]=bank.Slice(weapons,new RectInt(i*weapons.width/3,0,weapons.width/3,weapons.height),new Vector2(.5f,i==2?.5f:.15f),100);
        }
    }
    public sealed partial class CoastGame { public AmazonSettlementV46 amazons; }
}
