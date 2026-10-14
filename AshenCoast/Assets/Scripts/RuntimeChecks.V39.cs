using System.Collections;
using System.Linq;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        Item BookV39(ItemKind kind,int x,int y)
        {var book=new Item(kind);game.survivor.inventory.Place(book,x,y);game.survivor.inventory.ToggleTome(book);return book;}
        void ScrollV39(Item book,int slot,ItemKind kind)
        {var source=new GridInventory(4,4,"scroll preparation");var scroll=new Item(kind);source.Add(scroll);BookSpells.Insert(book,slot,scroll,source);}
        Vector2[] RuneV39(SpellGlyph glyph)=>SpatialSigil.Resample(SpellGlyphs.Path(glyph),120).Select(p=>p*200+new Vector2(330,230)).ToArray();
        IEnumerator BooksAndLayersV39()
        {
            var g=game;var s=g.survivor;var ui=g.ui;HandClearV34();ui.CloseContainers();s.inventory.items.Clear();s.spatialInventory.items.Clear();s.abyssCell=s.inventory.abyssCell=null;s.health=100;s.mana=100;g.seconds=50;g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));
            var tome=BookV39(ItemKind.Tome,0,0);var pyro=BookV39(ItemKind.Pyromancy,4,0);var abyss=BookV39(ItemKind.AbyssTome,7,0);ScrollV39(tome,2,ItemKind.CurseScroll);ScrollV39(pyro,1,ItemKind.ShacklesScroll);ScrollV39(pyro,2,ItemKind.CurseScroll);ScrollV39(abyss,2,ItemKind.CurseScroll);
            var wood=new Item(ItemKind.Wood);var clay=new Item(ItemKind.Clay);s.inventory.Place(wood,0,3);s.inventory.Place(clay,9,4);s.inventory.Place(new Item(ItemKind.Spectrum),8,3);s.equipment[0]=new Item(ItemKind.Spectrum);
            ui.open=true;yield return DrawUIV34();var point=Cell(s.inventory,pyro.x,pyro.y);yield return Pointer(point,EventType.MouseDown,1);
            Check(ui.bookSelected==null,"pressing RMB on a book keeps the spell popup closed until frame release");var frameEnd=Cell(s.inventory,pyro.x+pyro.Definition.width-1,pyro.y+pyro.Definition.height-1);yield return Pointer(frameEnd,EventType.MouseDrag,1);yield return Pointer(frameEnd,EventType.MouseUp,1);
            Check(ui.bookSelected==pyro && pyro.opened,"releasing a complete RMB book frame opens its spell slots");
            Check(ui.SpellCell(0).width>72 && ui.SpellCell(0).height>42,"spell cells have larger visible and interactive areas");
            Check(SpellGlyphs.For(pyro,pyro.spells[1])==SpellGlyph.Triangle && SpellGlyphs.For(pyro,pyro.spells[2])==SpellGlyph.Circle && SpellGlyphs.For(tome,tome.spells[2])==SpellGlyph.Diamond && SpellGlyphs.For(abyss,abyss.spells[2])==SpellGlyph.Fog,"scroll icons show real casting symbols; only spatial folio scrolls become diamonds");
            Check(SpellGlyphs.For(abyss,abyss.spells[1])==SpellGlyph.Abyss && SpellGlyphs.For(pyro,pyro.spells[0])==SpellGlyph.Zigzag && SpellGlyphs.For(tome,tome.spells[0])==SpellGlyph.Star,"builtin icons show minimal whirlpool, zigzag and portal star");
            Check(g.art.icons[17].texture.name=="TomesV5" && g.art.icons[18].texture.name=="OpenFolioV43","spatial folio restores its closed and open V36 artwork");yield return Capture("v39-spell-symbols-and-spectrum-gradient.png");
            yield return SelectBookFrameV40(abyss,s.inventory);yield return Capture("v39-abyss-distinct-symbols.png");
            yield return Pointer(Cell(s.inventory,clay.x,clay.y),EventType.MouseDown,0);yield return Pointer(Cell(s.inventory,0,3),EventType.MouseDown,0);
            Check(s.spatialInventory.At(0,3)==clay && s.inventory.items.Contains(wood) && !ui.Dragging,"actual UI drop over an occupied cell automatically places the item in the second layer");
            yield return Pointer(Cell(s.inventory,0,4),EventType.MouseDown,0);
            Check(!ui.Dragging && s.inventory.items.Contains(wood),"a partly covered first-layer item cannot be taken through an uncovered cell");yield return Capture("v39-automatic-inventory-overlay.png");
            yield return Pointer(Cell(s.spatialInventory,0,3),EventType.MouseDown,0);Check(ui.DraggedItem==clay && !s.spatialInventory.items.Contains(clay),"click retrieves the upper item before the lower one");
            yield return Pointer(Cell(s.inventory,5,4),EventType.MouseDown,0);Check(s.inventory.At(5,4)==clay && s.spatialInventory.items.Count==0,"drop on empty cells automatically uses the first layer");
            yield return Pointer(Cell(s.inventory,0,4),EventType.MouseDown,0);yield return Pointer(Cell(s.inventory,3,4),EventType.MouseDown,0);
            Check(s.inventory.At(3,3)==wood && !ui.Dragging,"after removing the upper item the original lower item can be taken and moved");
            yield return Pointer(Cell(s.inventory,5,4),EventType.MouseDown,0);yield return Pointer(Cell(s.inventory,3,3),EventType.MouseDown,0);s.inventory.ToggleTome(tome);yield return DrawUIV34();
            Check(s.spatialInventory.At(3,3)==clay && !s.SpatialLayerOpen,"closing the folio preserves visible upper-layer items");yield return Pointer(Cell(s.spatialInventory,3,3),EventType.MouseDown,0);yield return Pointer(Cell(s.inventory,3,3),EventType.MouseDown,0);
            Check(ui.DraggedItem==clay && s.spatialInventory.items.Count==0 && s.inventory.At(3,3)==wood,"closed folio allows retrieval but refuses replacing an upper item over occupied lower cells");
            yield return Pointer(Cell(s.inventory,5,4),EventType.MouseDown,0);s.Bag.items.Clear();s.inventory.Transfer(pyro,s.Bag,0,0);ui.ToggleBag();yield return DrawUIV34();yield return SelectBookFrameV40(pyro,s.Bag);
            Check(ui.bookSelected==pyro && !pyro.opened,"RMB frame release also selects closed books in other storage without opening the book itself");ui.ClearCheckPointer();ui.CloseContainers();
        }
        IEnumerator GlyphsAndCameraV39()
        {
            var g=game;var s=g.survivor;HandClearV34();s.inventory.items.Clear();s.spatialInventory.items.Clear();g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));g.seconds=50;s.equipment[0]=new Item(ItemKind.Orb);s.mana=100;
            var abyss=BookV39(ItemKind.AbyssTome,0,0);ScrollV39(abyss,2,ItemKind.CurseScroll);var tentacle=RuneV39(SpellGlyph.Tentacles);var fog=RuneV39(SpellGlyph.Fog);var zigzag=RuneV39(SpellGlyph.Zigzag);
            Check(SpellGlyphs.OpenRune(tentacle)==SpellGlyph.Tentacles && !GestureShape.FogRune(tentacle) && SpellGlyphs.OpenRune(fog)==SpellGlyph.Fog && SpellGlyphs.OpenRune(zigzag)==SpellGlyph.Zigzag,"drawing recognizer separates the tentacle rune, fog rune and fire zigzag");
            Check(new[]{SpellGlyph.Star,SpellGlyph.Infinity,SpellGlyph.Triangle,SpellGlyph.Circle,SpellGlyph.Diamond}.All(glyph=>SpellGlyphs.OpenRune(RuneV39(glyph))==SpellGlyph.None),"closed spell symbols cannot be mistaken for the open fog, tentacle or zigzag runes");
            bool variants=true;foreach(var glyph in new[]{SpellGlyph.Fog,SpellGlyph.Tentacles,SpellGlyph.Zigzag})for(int k=0;k<8;k++)
            {var raw=RuneV39(glyph).Select((p,i)=>p+new Vector2(Mathf.Sin(i*.23f+k)*4,Mathf.Cos(i*.19f+k)*4)).ToArray();if(k%2==1)System.Array.Reverse(raw);variants&=SpellGlyphs.OpenRune(raw)==glyph;}
            Check(variants,"twenty-four wobbly and reversed rune drawings keep their distinct spell identity");
            ViewV34(g.body.position,5);yield return null;g.spellcraft.tentacleReady=g.seconds;g.spellcraft.Select(abyss,abyss.spells[2]);int fields=g.arcane.fields.Count,fogs=g.spellcraft.fogs.Count;
            Check(g.spellcraft.CastGesture(tentacle) && g.arcane.fields.Count>fields && g.spellcraft.fogs.Count==fogs && s.mana==70,"actual tentacle drawing summons tentacles even when the fog slot was selected");
            g.spellcraft.Select(abyss,abyss.spells[0]);Check(g.spellcraft.CastGesture(fog) && g.spellcraft.fogs.Count==fogs+1 && s.mana==60,"actual fog drawing creates fog even when the locked tentacle slot was selected");
            g.spellcraft.Tick(.7f);ViewV34(g.spellcraft.fogs.Last().point,5);yield return Capture("v39-denser-black-fog.png");
            foreach(var field in g.arcane.fields)field.Dispose();g.arcane.fields.Clear();
            var pyro=BookV39(ItemKind.Pyromancy,4,0);ScrollV39(pyro,2,ItemKind.CurseScroll);g.spellcraft.SelectedBook=null;g.spellcraft.SelectedSlot=null;
            bool circles=true;for(int k=0;k<12;k++)
            {var raw=Enumerable.Range(0,96).Select(i=>{float a=i/95f*(Mathf.PI*2-.18f),r=1+.10f*Mathf.Sin(a*3+k)+.045f*Mathf.Cos(a*7);return new Vector2(440+145*r*Mathf.Cos(a),310+(100+k*2)*r*Mathf.Sin(a));}).ToArray();if(k%2==1)System.Array.Reverse(raw);circles&=GestureShape.Recognize(raw)==GestureKind.Circle;}
            Check(circles,"curse accepts twelve imperfect oval circles with small gaps and both directions");
            Check(GestureShape.Recognize(ScreenRune(new[]{Vector2.up,Vector2.right,Vector2.down,Vector2.left,Vector2.up}))==GestureKind.Diamond,"more forgiving curse recognition keeps the diamond distinct");
            var frog=g.arcane.frogs.First();var centre=new Vector2(18,15);frog.health=180;frog.Warp(centre);g.body.position=centre+Vector2.left*3;g.hero.transform.position=g.body.position;ViewV34(centre,5);yield return null;
            var imperfect=ScreenRune(Enumerable.Range(0,101).Select(i=>{float a=i/100f*(Mathf.PI*2-.12f),r=1+.08f*Mathf.Sin(a*3);return centre+new Vector2(Mathf.Cos(a)*r*1.65f,Mathf.Sin(a)*r*1.28f);}).ToArray());s.mana=100;
            Check(g.spellcraft.CastGesture(imperfect) && g.spellcraft.DamageFactor(frog.renderer)==1.5f && s.mana==86,"an actual imperfect circle casts curse on its enclosed enemy at the discounted mana cost");
            var fire=g.arcane.CreateCampfire(g.body.position+Vector2.down*2);ViewV34(fire.position,5);yield return null;var fireRune=ScreenRune(SpellGlyphs.Zigzag.Select(p=>fire.position+new Vector2(p.x-.5f,.5f-p.y)*2).ToArray());s.mana=100;
            Check(g.spellcraft.CastGesture(fireRune) && fire.exploded && s.mana==93,"drawn fire zigzag actually detonates the nearby campfire using the sphere for seven mana");
            s.inventory.items.Clear();var tome=BookV39(ItemKind.Tome,0,0);ScrollV39(tome,2,ItemKind.CurseScroll);s.mana=100;g.ui.CloseContainers();g.checkCameraPosition=null;g.checkCameraSize=6;g.body.position=new Vector2(15,15);g.hero.transform.position=g.body.position;g.ResetCamera(new Vector3(15,17.2f,-20));yield return null;
            var oldCamera=g.cameraView.transform.position;var destination=Enumerable.Range(3,8).Select(i=>g.body.position+Vector2.right*i).First(p=>g.spellcraft.CanLand(g.map.CurrentRegion,p));
            var diamond=ScreenRune(new[]{destination+Vector2.up,destination+Vector2.right,destination+Vector2.down,destination+Vector2.left,destination+Vector2.up});
            Check(g.spellcraft.CastGesture(diamond) && Vector2.Distance(g.body.position,destination)<.01f && Vector3.Distance(g.cameraView.transform.position,oldCamera)<.01f,"diamond instantly moves the player while preserving the camera's departure position");
            for(int i=0;i<8;i++)yield return null;var mid=g.cameraView.transform.position;
            Check(mid.x>oldCamera.x+.1f && mid.x<destination.x-.05f,"actual frame updates pan the camera toward the teleport destination");yield return Capture("v39-diamond-camera-moving.png");
            for(int i=0;i<60;i++)yield return null;Check(Mathf.Abs(g.cameraView.transform.position.x-destination.x)<.2f,"camera smoothly settles on the teleported player");yield return Capture("v39-diamond-camera-arrival.png");g.spellcraft.Tick(1);
        }
        IEnumerator SpectrumAndRainbowV39()
        {
            var g=game;var s=g.survivor;g.ui.CloseContainers();g.ui.Notice(null);HandClearV34();g.arcane.Tick(5);s.inventory.items.Clear();s.spatialInventory.items.Clear();g.seconds=225;g.journey.SetArea(WorldArea.Coast,new Vector2(15,15));s.equipment[0]=new Item(ItemKind.Spectrum);s.mana=100;s.health=100;
            var at=new Vector2(18,15);g.arcane.frogs[0].Reset();g.arcane.frogs[0].Warp(at+Vector2.up*4);ViewV34(at+Vector2.up*3,7);yield return null;Check(g.spectrum.Summon(at),"V39 creates the bright translucent rainbow");g.spectrum.Tick(.9f);
            var plate=g.art.Renderer("Temporary transparency probe",g.v38Art.quad,at+Vector2.up*5,g.spectrum.wall.sortingOrder-1);plate.sharedMaterial=g.v37Art.glowMaterial;plate.transform.localScale=new Vector3(1,1,1);plate.color=Color.black;g.cameraView.Render();var a=ReadPixels();plate.color=Color.white;g.cameraView.Render();var b=ReadPixels();float visibility=RegionDifference(a,b,new Rect(at.x-.45f,at.y+4.55f,.9f,.9f));Destroy(a);Destroy(b);Destroy(plate.gameObject);
            Check(visibility>.18f,"background remains visibly readable through the actual rendered rainbow (difference "+visibility.ToString("F3")+")");
            yield return Capture("v39-rainbow-see-enemy-through-flow.png");
            var extra=new FrogMage(g.arcane,at+new Vector2(3,.95f));g.arcane.frogs.Add(extra);var frogs=new[]{g.arcane.frogs[0],extra};frogs[0].Reset();frogs[0].health=frogs[1].health=180;frogs[0].Warp(at+Vector2.up*.95f);frogs[1].Warp(at+new Vector2(3,.95f));g.spectrum.Tick(.05f);
            Check(!frogs[0].Alive && frogs[1].Alive,"larger oval damages an enemy beyond the previous narrow strip while excluding its rectangle corner");
            Check(g.spectrum.DamageFootprint(at+new Vector2(2.5f,.4f)) && !g.spectrum.DamageFootprint(at+new Vector2(3,.95f)),"rainbow damage area has rounded oval boundaries");yield return Capture("v39-bright-translucent-rainbow.png");g.spectrum.Dismiss();g.spectrum.Tick(2.6f);
            var drop=g.ThrowItem(new Item(ItemKind.Spectrum),g.body.position+new Vector2(1.3f,-.8f));g.TickLoot(.7f);g.UpdateEnvironment(0);ViewV34(drop.position+Vector2.up*.65f,3);yield return null;
            var points=new Vector4[32];var colors=new Vector4[32];int n=g.AppendSpectrumLights(points,colors,0);
            Check(n>=7 && colors.Take(7).Select(c=>new Vector3(c.x,c.y,c.z)).Distinct().Count()>=6 && points[0].w>.28f,"ground dagger emits brighter separate rainbow colors into the surrounding scene");
            Check(drop.spectrumGlow && drop.spectrumGlow.enabled && drop.renderer.sprite.texture.name=="SpectrumV39","planted dagger uses the new brighter artwork and animated rainbow reflection");
            var saved=RenderTexture.active;g.art.UpdateOrb(.1f);RenderTexture.active=ArtBank.SpectrumIcon;var iconA=new Texture2D(128,64,TextureFormat.RGBA32,false);iconA.ReadPixels(new Rect(0,0,128,64),0,0);iconA.Apply();g.art.UpdateOrb(.9f);RenderTexture.active=ArtBank.SpectrumIcon;var iconB=new Texture2D(128,64,TextureFormat.RGBA32,false);iconB.ReadPixels(new Rect(0,0,128,64),0,0);iconB.Apply();RenderTexture.active=saved;
            var ca=iconA.GetPixels();var cb=iconB.GetPixels();int changed=ca.Zip(cb,(x,y)=>Mathf.Abs(x.r-y.r)+Mathf.Abs(x.g-y.g)+Mathf.Abs(x.b-y.b)>.15f && x.a>.5f).Count(v=>v);Destroy(iconA);Destroy(iconB);
            Check(changed>350,"inventory dagger itself has clearly animated spectral steel rather than only an animated name");
            g.seconds=225;g.TickLoot(0);g.cameraView.Render();a=ReadPixels();g.seconds=225.7f;g.TickLoot(0);g.cameraView.Render();b=ReadPixels();float steel=RegionDifference(a,b,new Rect(drop.position.x-.15f,drop.position.y+.07f,.3f,.57f));Destroy(a);Destroy(b);
            Check(steel>.01f,"actual planted blade visibly cycles its rainbow metal colors (difference "+steel.ToString("F3")+")");yield return Capture("v39-spectrum-rainbow-ground-light.png");
            s.inventory.Place(new Item(ItemKind.Spectrum),0,0);g.ui.open=true;g.combat.manual=false;yield return DrawUIV34();g.combat.manual=true;yield return Capture("v39-spectrum-yellow-edge-gradient.png");g.ui.CloseContainers();
        }
    }
}
