using UnityEngine;

namespace AshenCoast
{
    public sealed class CoastArtV37
    {
        public readonly Sprite[] planted=new Sprite[3];
        public readonly Sprite[,] hunterAttack=new Sprite[3,4];
        public readonly Sprite[] spells=new Sprite[8];
        public readonly Sprite chain,rainbow,cloud,mote;
        public readonly Material glowMaterial=new Material(Shader.Find("Sprites/Default"));
        public CoastArtV37(ArtBank bank)
        {
            var books=bank.Load("BooksV37");int[] rowBounds={0,355,700,books.height};int[] bookIcons={17,18,24,25,26,27};
            for(int row=1;row<3;row++)for(int col=0;col<2;col++)bank.icons[bookIcons[row*2+col]]=bank.Slice(books,new RectInt(col*books.width/2,rowBounds[row],books.width/2,rowBounds[row+1]-rowBounds[row]),Vector2.one*.5f,260);
            ArtBank.ClosedSpatialAspect=bank.icons[17].rect.width/bank.icons[17].rect.height;ArtBank.OpenSpatialAspect=bank.icons[18].rect.width/bank.icons[18].rect.height;
            var spectrum=bank.Load("SpectrumV37");
            bank.icons[43]=bank.Slice(spectrum,new RectInt(0,0,980,spectrum.height/2),new Vector2(.18f,.5f),260);
            planted[0]=bank.Slice(spectrum,new RectInt(980,0,spectrum.width-980,spectrum.height/2),new Vector2(.5f,0),260);
            planted[1]=bank.Slice(spectrum,new RectInt(0,spectrum.height/2,spectrum.width/2,spectrum.height/2),new Vector2(.5f,0),260);
            planted[2]=bank.Slice(spectrum,new RectInt(spectrum.width/2,spectrum.height/2,spectrum.width/2,spectrum.height/2),new Vector2(.5f,0),260);
            var effects=bank.Load("SpellEffectsV37");var pieces=new Sprite[8];
            for(int i=0;i<8;i++)pieces[i]=bank.Slice(effects,new RectInt(i%4*effects.width/4,i/4*effects.height/2,effects.width/4,effects.height/2),Vector2.one*.5f,260);
            spells[(int)SpellKind.Pocket]=pieces[0];spells[(int)SpellKind.WorldTravel]=pieces[1];spells[(int)SpellKind.Abyss]=pieces[2];spells[(int)SpellKind.Shackles]=pieces[3];spells[(int)SpellKind.Curse]=pieces[4];spells[(int)SpellKind.Tentacles]=bank.tentacle;spells[(int)SpellKind.FireBlast]=bank.explosion[10];chain=pieces[5];rainbow=bank.Slice(effects,new RectInt(900,444,350,effects.height-444),Vector2.one*.5f,260);cloud=bank.Slice(effects,new RectInt(1260,444,effects.width-1260,effects.height-444),Vector2.one*.5f,260);
            var spark=new Texture2D(5,5,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};var pixels=new Color[25];for(int y=0;y<5;y++)for(int x=0;x<5;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(2,2));pixels[y*5+x]=new Color(1,1,1,d<1.5f?1:d<2.5f?.35f:0);}spark.SetPixels(pixels);spark.Apply();mote=Sprite.Create(spark,new Rect(0,0,5,5),Vector2.one*.5f,5);
            var scrolls=bank.Load("ScrollsV37");for(int i=0;i<2;i++)bank.icons[41+i]=bank.Slice(scrolls,new RectInt(i*scrolls.width/2,0,scrolls.width/2,scrolls.height),Vector2.one*.5f,260);
            var attack=bank.Load("AxolotlAttackV37");
            for(int row=0;row<3;row++)for(int col=0;col<4;col++)
            {var trimmed=bank.Slice(attack,new RectInt(col*attack.width/4,row*attack.height/3,attack.width/4,attack.height/3),new Vector2(.5f,0),100);var r=trimmed.rect;float ground=attack.height-(row+1)*attack.height/3+13;hunterAttack[row,col]=Sprite.Create(attack,r,new Vector2(.5f,(ground-r.y)/r.height),r.width/8.2f,0,SpriteMeshType.FullRect);}
        }
        public Sprite SpellIcon(SpellKind kind)=>spells[(int)kind];
    }
    public sealed partial class NightHunter
    {
        public float attackAge=-1;
        public bool AttackAnimating=>attackAge>=0 && attackAge<.7f;
        public void PoseAttackV37()
        {
            if(!AttackAnimating || game.v37Art==null)return;
            int attackView=Mathf.Abs(look.x)>.55f?1:look.y>.3f?2:0;
            sprite.sprite=game.v37Art.hunterAttack[attackView,Mathf.Min(3,Mathf.FloorToInt(attackAge/.175f))];
            sprite.transform.localScale=Vector3.one;sprite.flipX=look.x>0;sprite.forceRenderingOff=false;skin.enabled=false;game.art.PixelStyle(sprite);
        }
    }
}
