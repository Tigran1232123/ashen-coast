using UnityEngine;

namespace AshenCoast
{
    public sealed class CoastArtV38
    {
        public readonly Sprite flow,planted,quad;
        public readonly Sprite[,] mist=new Sprite[3,4];
        public readonly Vector4 ropeRect;
        public CoastArtV38(ArtBank bank)
        {
            var stream=bank.Load("RainbowFlowV38");
            flow=Sprite.Create(stream,new Rect(0,0,stream.width,stream.height),new Vector2(.5f,0),200,0,SpriteMeshType.FullRect);
            var clouds=bank.Load("RainbowCloudsV38");var rows=bank.Bands(clouds,true,3);
            for(int row=0;row<3;row++)for(int col=0;col<4;col++)
            {
                var band=rows[row];int bottom=clouds.height-band.y-1;
                var rect=new Rect(col*clouds.width/4,bottom,clouds.width/4,band.y-band.x+1);
                mist[row,col]=Sprite.Create(clouds,rect,new Vector2(.5f,0),100,0,SpriteMeshType.FullRect);
            }
            var blade=bank.Load("SpectrumV39");
            bank.icons[43]=bank.Slice(blade,new RectInt(0,0,1100,blade.height),new Vector2(.18f,.5f),260);
            bank.ConfigureSpectrumIcon();
            var cut=bank.Slice(blade,new RectInt(1100,0,blade.width-1100,blade.height),Vector2.zero,260).rect;
            // Register the blade tip itself. Cord motion never changes this sprite or its pivot.
            var pixels=blade.GetPixels32();int tipY=(int)cut.y,tipX=0,count=0;
            for(int y=tipY;y<tipY+5;y++)for(int x=(int)cut.x;x<cut.xMax;x++)
                if(pixels[y*blade.width+x].a>96){tipX+=x;count++;}
            float anchor=count>0?(float)tipX/count:cut.center.x;
            planted=Sprite.Create(blade,cut,new Vector2((anchor-cut.x)/cut.width,0),260,0,SpriteMeshType.FullRect);
            ropeRect=new Vector4((anchor+38-cut.x)/cut.width,.60f,.98f,.99f);
            quad=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),Vector2.one*.5f,Texture2D.whiteTexture.width,0,SpriteMeshType.FullRect);
        }
    }
}
