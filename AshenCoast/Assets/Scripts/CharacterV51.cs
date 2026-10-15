using System;
using UnityEngine;
namespace AshenCoast
{
    public enum CharacterV51 { Human,Frog }
    public sealed partial class Survivor
    {public CharacterV51 character;public float MaxHealth=>character==CharacterV51.Frog?50:100;public float MaxMana=>character==CharacterV51.Frog?200:100;}
    public sealed class CharacterArtV51
    {
        public readonly Sprite[,] frog=new Sprite[8,8];public readonly Sprite[] corpses=new Sprite[9],rooms=new Sprite[4];
        public CharacterArtV51(ArtBank bank)
        {
            var texture=bank.Load("FrogHeroV51");int[] rows={0,193,375,557,750,944,1131,1321,1536};
            for(int d=0;d<8;d++)for(int f=0;f<8;f++){var source=bank.Slice(texture,new RectInt(f%4*texture.width/4,rows[d],texture.width/4,rows[d+1]-rows[d]),new Vector2(.5f,0),100);var r=source.rect;frog[d,f]=Sprite.Create(texture,r,new Vector2(.5f,0),r.height/1.78f,0,SpriteMeshType.FullRect);}
            var dead=bank.Load("CorpsesV51");for(int i=0;i<9;i++)corpses[i]=bank.Slice(dead,new RectInt(i%3*dead.width/3,i/3*dead.height/3,dead.width/3,dead.height/3),new Vector2(.5f,.5f),200);
            var room=bank.Load("ShellInteriorsV51");for(int i=0;i<4;i++)rooms[i]=bank.Slice(room,new RectInt(i%2*room.width/2,i/2*room.height/2,room.width/2,room.height/2),new Vector2(.5f,0),200);
            bank.icons[57]=bank.icons[41];
        }
    }
    public sealed partial class CoastGame
    {
        public CharacterArtV51 v51Art;public CorpseWorldV51 corpses;
        public Sprite[,] HeroFramesV51(CharacterV51 kind)=>kind==CharacterV51.Frog && v51Art!=null?v51Art.frog:art.hero;
        public void ChooseCharacterV51(CharacterV51 kind)
        {
            if(CoopOnline || !snails.SetupOpen)return;survivor.character=kind;survivor.health=survivor.MaxHealth;survivor.mana=Mathf.Min(survivor.mana,survivor.MaxMana);
            if(gait?.Lower)Destroy(gait.Lower.gameObject);gait=new PixelGait(art,hero,HeroFramesV51(kind),.43f);hero.sprite=HeroFramesV51(kind)[0,3];hero.flipX=false;art.PixelStyle(hero);
        }
    }
    public sealed partial class InventoryUI
    {
        void CharacterChoiceV51(Rect r)
        {
            var c=game.survivor.character;
            for(int i=0;i<2;i++)
            {
                var kind=(CharacterV51)i;var box=new Rect(r.x+70+i*225,r.y+178,215,100);ArtBank.DrawIcon(game.HeroFramesV51(kind)[0,3],new Rect(box.x+7,box.y+3,48,88),Color.white);
                if(Button(new Rect(box.x+62,box.y+7,148,39),i==0?"Человек":"Лягушка",c==kind))game.ChooseCharacterV51(kind);
                GUI.Label(new Rect(box.x+63,box.y+53,146,30),i==0?"100 ХП · 100 маны":"50 ХП · 200 маны",CachedStyle(small,TextAnchor.MiddleCenter,13));
            }
        }
    }
}
