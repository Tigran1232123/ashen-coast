using System;
using System.IO;
using System.Linq;
using AshenCoast;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class ProjectBuilder
{
    [MenuItem("Берег обломков/Подготовить сцену")]
    public static void Prepare()
    {
        foreach(var path in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Resources/Art"}).Select(AssetDatabase.GUIDToAssetPath))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Default; importer.isReadable=true;
            importer.filterMode=FilterMode.Point; importer.mipmapEnabled=false; importer.alphaIsTransparency=true;
            importer.textureCompression=TextureImporterCompression.Uncompressed; importer.npotScale=TextureImporterNPOTScale.None;
            importer.maxTextureSize=4096; importer.SaveAndReimport();
            if(path.Contains("V4") || path.Contains("V5") || path.Contains("V6") || path.Contains("V8") || path.Contains("V9") || path.Contains("V10") || path.Contains("V11") || path.Contains("V12") || path.Contains("V13") || path.Contains("V14") || path.Contains("V15") || path.Contains("V16") || path.Contains("V18") || path.Contains("V23") || path.Contains("V24")){importer.wrapMode=(path.Contains("GoldenMeadowV15") || path.Contains("FortStoneFloorV18"))?TextureWrapMode.Repeat:TextureWrapMode.Clamp;importer.SaveAndReimport();}
        }
        Directory.CreateDirectory("Assets/Resources/Materials");
        foreach(var shaderName in new[]{"WorldSprite","Terrain","Atmosphere","Cosmos","PocketRoom","DoorPortal","OrbIcon","JourneyFloor","HearthFire","FortStone","Axolotl","PrismIcon","PrismBeam","ScorchedGround","BuildingGround","BlastRing","CaveStone","SpiderWeb","SpectralWorm","CookingSteam","AbyssFog","Rainbow","RainbowMist","RainbowGround","SpectrumIcon","Cocoon","InventoryMagicV44","ZenRopes"})
        {
            var shader=Shader.Find("AshenCoast/"+shaderName);
            if(shader==null)throw new Exception("Shader not found: "+shaderName);
            string path="Assets/Resources/Materials/"+shaderName+".mat";
            if(AssetDatabase.LoadAssetAtPath<Material>(path)==null)AssetDatabase.CreateAsset(new Material(shader),path);
        }
        PlayerSettings.companyName="Ashen Coast";PlayerSettings.productName="Берег обломков";
        PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=900;
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;
        PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Gamma;
        PlayerSettings.SplashScreen.show=false;PlayerSettings.SplashScreen.showUnityLogo=false;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{GraphicsDeviceType.Direct3D11});
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        var input=settings.FindProperty("activeInputHandler");if(input!=null){input.intValue=0;settings.ApplyModifiedPropertiesWithoutUndo();}
        QualitySettings.vSyncCount=1;QualitySettings.antiAliasing=0;
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("Берег обломков · Game systems").AddComponent<CoastGame>();
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Coast.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Coast.unity",true)};
        AssetDatabase.SaveAssets();ModelChecks.Run();
        Debug.Log("COAST_PREPARED_AND_MODEL_CHECKS_PASSED");
    }
    [MenuItem("Берег обломков/Собрать Windows")]
    public static void Build()
    {
        Prepare();
        var project=Path.GetFullPath(".");var destination=Environment.GetEnvironmentVariable("ASHEN_BUILD_DIRECTORY");
        var path=Path.GetFullPath(Path.Combine(string.IsNullOrEmpty(destination)?Path.Combine(project,"..","Windows"):destination,"AshenCoast.exe"));
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes=new[]{"Assets/Scenes/Coast.unity"},locationPathName=path,target=BuildTarget.StandaloneWindows64,
            options=BuildOptions.CompressWithLz4HC
        });
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);
        File.WriteAllText(Path.Combine(project,"..","build-verification.txt"),"Unity "+Application.unityVersion+"\nWindows x64: succeeded\nModel assertions: "+ModelChecks.count+"\nBuilt bytes: "+report.summary.totalSize+"\n");
        Debug.Log("COAST_BUILD_SUCCEEDED "+path);
    }
    // Rebuild the prepared project after a focused change without rerunning unchanged model checks.
    public static void BuildCurrent()
    {
        AssetDatabase.Refresh();AssetDatabase.SaveAssets();
        foreach(var name in new[]{"AmazonVillageV46","AmazonActorsV46","AmazonWeaponsV46","AlvielStatueV46"})
        {
            var changed=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Art/"+name+".png");
            if(changed==null)throw new Exception("Missing Amazon artwork: "+name);
            if(!changed.isReadable || changed.filterMode!=FilterMode.Point)throw new Exception("New Amazon artwork needs readable point sampling: "+name);
        }
        var villageShader=Shader.Find("AshenCoast/AmazonGroundV46");
        const string villageMaterial="Assets/Resources/Materials/AmazonGroundV46.mat";
        if(AssetDatabase.LoadAssetAtPath<Material>(villageMaterial)==null)AssetDatabase.CreateAsset(new Material(villageShader),villageMaterial);
        AssetDatabase.SaveAssets();
        var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Art/SpearV45.png");
        if(importer==null || !importer.isReadable || importer.filterMode!=FilterMode.Point)throw new Exception("Changed spear texture is not configured for native pixel rendering");
        var project=Path.GetFullPath(".");var destination=Environment.GetEnvironmentVariable("ASHEN_BUILD_DIRECTORY");
        var path=Path.GetFullPath(Path.Combine(string.IsNullOrEmpty(destination)?Path.Combine(project,"..","Windows"):destination,"AshenCoast.exe"));Directory.CreateDirectory(Path.GetDirectoryName(path));
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=path,target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);
        File.WriteAllText(Path.Combine(project,"..","build-verification.txt"),"Unity "+Application.unityVersion+"\nWindows x64: succeeded\nUnchanged model checks: not repeated; prior V45 build passed308\nBuilt bytes: "+report.summary.totalSize+"\n");
        Debug.Log("COAST_BUILD_SUCCEEDED "+path);
    }
}

public static class ModelChecks
{
    public static int count;
    static void Assert(bool condition,string message){count++;if(!condition)throw new Exception("Model check failed: "+message);}
    public static void Run()
    {
        count=0;WorldShape.Configure(74391);
        var fort=WorldShape.FortPosition;WorldShape.Configure(19527);Assert(WorldShape.FortPosition!=fort,"fort position changes with world seed");WorldShape.Configure(74391);Assert(WorldShape.FortPosition==fort && WorldShape.Land(fort,48),"same world seed reproduces fort inside safe land");
        Assert(WorldShape.Land(new Vector2(-1.5f,-1.65f),.6f) && WorldShape.Land(new Vector2(-7.7f,-1.15f),.6f),"irregular outline always preserves original spawn and ship beach");
        var s=new Survivor();Assert(s.inventory.columns==10 && s.inventory.rows==5,"50-slot inventory");
        Assert(s.inventory.items.Any(i=>i.kind==ItemKind.Lamp) && s.inventory.items.Any(i=>i.kind==ItemKind.Pyromancy) && s.inventory.items.Any(i=>i.kind==ItemKind.AbyssTome) && s.inventory.items.Any(i=>i.kind==ItemKind.YoYo),"player starts with lamp and both new books and yo-yo");s.inventory.items.Clear();
        Assert(s.Bag.columns==5 && s.Bag.rows==5,"25-slot equipped sack");Assert(s.mana==0,"initial mana zero");
        var ship=CoastTime.CreateShipStorage();Assert(ship.Valid(),"ship no item overlap");Assert(ship.items.Count==39,"thirty-seven hold items including fourteen clay");
        Assert(ship.items.Count(i=>i.kind==ItemKind.Wood)==7 && ItemDefinition.All[ItemKind.Wood].width==1 && ItemDefinition.All[ItemKind.Wood].height==2,"five unstacked 1x2 wood resources in hold");
        Assert(ship.items.Count(i=>i.kind==ItemKind.Clay)==14,"fourteen clay in the hold");
        var potPattern=new GridInventory(5,5,"pot pattern");foreach(var p in new[]{new Vector2Int(0,0),new Vector2Int(2,0),new Vector2Int(0,1),new Vector2Int(2,1),new Vector2Int(0,2),new Vector2Int(1,2),new Vector2Int(2,2)})potPattern.Place(new Item(ItemKind.Clay),p.x,p.y);Assert(potPattern.Craft(new RectInt(0,0,3,3),out var pot) && pot.kind==ItemKind.StoragePot && pot.contents.columns==5 && pot.contents.rows==5,"seven-clay U creates a pot with 25 storage cells");
        Assert(ship.items.Count(i=>i.kind==ItemKind.Brain)==3,"three unstacked brain mushrooms");
        Assert(ship.items.Count(i=>i.kind==ItemKind.IronIngot)==5 && ItemDefinition.All[ItemKind.IronIngot].width==2 && ItemDefinition.All[ItemKind.IronIngot].height==1,"five horizontal 2x1 iron ingots");
        Assert(ItemDefinition.All[ItemKind.RawMeat].width==2 && ItemDefinition.All[ItemKind.RawMeat].height==1,"dragon raw meat occupies one row and two columns");
        var tableCraft=new GridInventory(5,5,"table materials");for(int i=0;i<3;i++)tableCraft.Place(new Item(ItemKind.Wood),i,0);
        Assert(tableCraft.Craft(new RectInt(0,0,3,2),out var table) && table.kind==ItemKind.CraftingTable && tableCraft.items.Count==1,"three complete wood resources produce one crafting table");
        Assert(table.Definition.width==3 && table.Definition.height==3,"crafting table has a 3x3 inventory footprint");
        var narrowCraft=new GridInventory(5,5,"narrow selection");for(int i=0;i<3;i++)narrowCraft.Place(new Item(ItemKind.Wood),i,0);Assert(narrowCraft.Craft(new RectInt(0,0,3,1),out var narrowTable) && narrowTable.kind==ItemKind.CraftingTable,"crafting a 3x3 table only requires outlining the three resource anchors");
        var skater=new Survivor();var boots=new Item(ItemKind.Boots);skater.equipment[(int)GearSlot.Feet]=boots;var rollers=skater.inventory.items.First(i=>i.kind==ItemKind.RollerSkates);Assert(skater.EquipOrDrop(rollers,skater.inventory,GearSlot.Feet,out _) && skater.Skates==rollers && skater.equipment[(int)GearSlot.Feet]==boots,"roller attachments preserve the boots occupying the footwear slot");rollers.Wear(999);Assert(rollers.durability==100 && !rollers.Definition.Durable && rollers.Definition.width==2 && rollers.Definition.height==2,"rollers have a 2x2 footprint and no durability loss");Assert(!skater.skateSlot.Add(new Item(ItemKind.Brain)),"attachment slot refuses ordinary items");
        var forge=new GridInventory(5,5,"forge pattern"){craftLimit=4};foreach(var p in new[]{new Vector2Int(0,0),new Vector2Int(2,0),new Vector2Int(1,1),new Vector2Int(0,2),new Vector2Int(2,2)})forge.Place(new Item(ItemKind.IronIngot),p.x,p.y);
        Assert(forge.Craft(new RectInt(0,0,4,3),out var anvil) && anvil.kind==ItemKind.Anvil && forge.items.Count==1 && forge.Valid(),"five ingots in the user's 4x3 diagram produce an anvil atomically");
        Assert(anvil.Definition.width==2 && anvil.Definition.height==2,"anvil inventory footprint is 2x2");
        var wrongForge=new GridInventory(5,5,"wrong pattern"){craftLimit=4};for(int i=0;i<5;i++)wrongForge.Place(new Item(ItemKind.IronIngot),i%2*2,i/2);
        Assert(!wrongForge.Craft(new RectInt(0,0,4,3),out _) && wrongForge.items.Count==5,"incorrect iron configuration consumes nothing");
        Assert(BuildingSystem.Footprint(ItemKind.Campfire,false)==Vector2Int.one && BuildingSystem.Footprint(ItemKind.CraftingTable,true)==new Vector2Int(1,2) && BuildingSystem.Footprint(ItemKind.CraftingTable,false)==new Vector2Int(2,1),"buildings use exact cells and table orientations");
        var overflow=new Survivor();overflow.inventory=new GridInventory(2,1,"full");var incoming=new Item(ItemKind.YoYo);overflow.inventory.Place(incoming,0,0);overflow.inventory.Place(new Item(ItemKind.Coin),1,0);var displaced=new Item(ItemKind.Sword);overflow.equipment[0]=displaced;
        Assert(overflow.EquipOrDrop(incoming,overflow.inventory,GearSlot.Hand,out var drop) && drop==displaced && overflow.equipment[0]==incoming && overflow.inventory.Valid(),"equipping succeeds and explicitly returns displaced gear for dropping when inventory is full");

        Assert(ship.items.Count(i=>i.kind==ItemKind.BeetleMeat)==3,"three separate meat pieces");
        var brain=ship.items.First(i=>i.kind==ItemKind.Brain);Assert(s.Eat(brain,ship),"eat brain");Assert(s.mana==100,"brain gives 100 mana");
        s.mana=21;s.Tick(4);Assert(s.mana==21,"mana does not regenerate");
        s.Eat(ship.items.First(i=>i.kind==ItemKind.Brain),ship);Assert(s.foods.Count==0,"brain never creates satiety effect");
        s.Eat(ship.items.First(i=>i.kind==ItemKind.Soup),ship);Assert(s.HungerMax==100,"brain does not count toward two-food bonus");
        s.Eat(ship.items.First(i=>i.kind==ItemKind.BeetleMeat),ship);Assert(s.HungerMax==120,"two actual dishes give twenty hunger max");
        s.foods.Add(new FoodEffect{kind=ItemKind.PinkFlower,remaining=12,duration=12});s.health=65;s.Tick(2);Assert(s.health>65,"three foods health regeneration");
        s.foods.Add(new FoodEffect {kind=ItemKind.BlueFlower,remaining=12,duration=12});Assert(s.HungerMax==150,"four foods additional +30");
        s.foods.Add(new FoodEffect {kind=ItemKind.WhiteFlower,remaining=12,duration=12});s.immunity=30;s.Tick(2);Assert(s.immunity>30,"five foods immunity regeneration");
        s.Tick(400);Assert(s.foods.Count==0 && s.HungerMax==100 && s.hunger<=100,"food expiration removes bonuses and clamps max");
        var grid=new GridInventory(10,5,"test");var a=new Item(ItemKind.BlueFlower);var b=new Item(ItemKind.PinkFlower);
        grid.Place(a,0,0);grid.Place(b,1,0);Assert(grid.Craft(new RectInt(0,0,2,1),out _),"compact selection consumes complete flower resources");grid.items.Clear();grid.Place(new Item(ItemKind.BlueFlower),0,0);grid.Place(new Item(ItemKind.PinkFlower),1,0);
        Assert(grid.Craft(new RectInt(0,0,2,2),out var rope),"craft from two complete different flowers");
        Assert(grid.items.Count==1 && rope.Definition.width==2 && rope.Definition.height==1 && grid.Valid(),"2x1 rope consumes ingredients");
        Assert(!grid.Craft(new RectInt(0,0,4,1),out _),"cannot craft selection beyond 3x3");
        var full=new GridInventory(3,2,"full");full.Place(new Item(ItemKind.BlueFlower),0,0);full.Place(new Item(ItemKind.WhiteFlower),1,0);full.Place(new Item(ItemKind.Brain),2,0);full.Place(new Item(ItemKind.Brain),2,1);
        Assert(!full.Craft(new RectInt(0,0,3,2),out _),"craft rejects extra non-flower ingredients");Assert(full.items.Count==4 && full.Valid(),"failed craft keeps all resources");
        var sword=new Item(ItemKind.Sword);Assert(s.inventory.Add(sword),"place 1x3 sword");Assert(s.Equip(sword,s.inventory,GearSlot.Hand),"equip weapon");
        var dagger=new Item(ItemKind.Dagger);s.inventory.Add(dagger);Assert(s.Equip(dagger,s.inventory,GearSlot.Hand) && s.inventory.items.Contains(sword),"equip swap returns old weapon");
        Assert(!s.Equip(sword,s.inventory,GearSlot.Head),"weapon cannot go in helmet slot");
        Assert(!s.inventory.Transfer(s.equipment[(int)GearSlot.Back],s.Bag,0,0),"bag cannot contain itself");
        foreach(var kind in new[]{ItemKind.Sword,ItemKind.Dagger,ItemKind.Scythe,ItemKind.Spear})Assert(ItemDefinition.All[kind].height>ItemDefinition.All[kind].width,"vertical weapon "+kind);
        Assert(Survivor.Fits(new Item(ItemKind.Orb),GearSlot.Hand),"orb is equipable in hand");
        var books=new GridInventory(3,2,"book test"){allowOpenBooks=true};var book=new Item(ItemKind.Tome);books.Add(book);
        Assert(books.ToggleTome(book) && book.opened && book.Definition.width==3 && book.Definition.height==2 && books.Valid(),"open book uses 3 columns 2 rows");
        Assert(books.ToggleTome(book) && !book.opened && book.Definition.height==2,"close book restores 2x2");
        books.Place(new Item(ItemKind.Brain),2,0);
        Assert(!books.ToggleTome(book) && !book.opened && books.Valid(),"failed book expansion is atomic");
        var sigil=SpatialSigil.Resample(SpatialSigil.Template,150).Select(p=>p*300+new Vector2(140,190)).ToList();
        Assert(SpatialSigil.Matches(sigil),"reference sigil recognized");sigil.Reverse();Assert(SpatialSigil.Matches(sigil),"reverse stroke recognized");
        Assert(!SpatialSigil.Matches(SpatialSigil.Resample(new[]{Vector2.zero,Vector2.one*400},80)),"straight stroke rejected");
        var rough=new[]{new Vector2(.20f,.9f),new Vector2(.98f,.48f),new Vector2(.08f,.17f),new Vector2(1,.02f),new Vector2(.19f,.56f),new Vector2(.9f,1)};
        Assert(SpatialSigil.Matches(SpatialSigil.Resample(rough,110).Select(p=>new Vector2(p.x*180+50,p.y*240+180)).ToList()),"rough hand-drawn sigil accepted");
        Assert(!SpatialSigil.Matches(SpatialSigil.Resample(new[]{Vector2.zero,Vector2.one,Vector2.up,Vector2.right},100).Select(p=>p*240).ToList()),"unrelated X gesture rejected");
        Assert(!CoastTime.IsNight(179.99f) && CoastTime.IsNight(180) && !CoastTime.IsNight(360),"180-second day and night");
        Assert(CoastTime.Fog(0)>.8f && CoastTime.Fog(10)==0,"fog ends at 10 seconds");
        Assert(CoastTime.Rain(0)==1 && CoastTime.Rain(20)==0,"storm ends at 20 seconds");
        Assert(CoastGame.Facing(Vector2.right)==6 && CoastGame.Facing(Vector2.left)==2 && CoastGame.Facing(Vector2.up)==4 && CoastGame.Facing(Vector2.down)==0,"directional sprite orientation");
        var art=new ArtBank();new CoastArtV43(art);
        var prismDefinition=ItemDefinition.All[ItemKind.LastPrism];
        Assert(prismDefinition.width==2 && prismDefinition.height==2 && prismDefinition.Weapon && !prismDefinition.Melee,"prism is a separate 2x2 channelled ranged weapon");
        Assert(new Survivor().inventory.items.Any(i=>i.kind==ItemKind.LastPrism),"player begins with a prism available in inventory");
        Assert(art.icons[30].texture.name=="LastPrismV24c" && ArtBank.PrismIcon && ArtBank.PrismIcon.filterMode==FilterMode.Point,"detailed opal prism uses a point-sampled animated icon");
        var dragonArt=new DragonArt(art);
        Assert(dragonArt.walk.Cast<Sprite>().All(f=>f!=null) && dragonArt.attack.Cast<Sprite>().All(f=>f!=null),"complete rooster-dragon walk and attack frames");
        Assert(dragonArt.flight.Length==16 && dragonArt.flight.All(f=>f!=null && f.texture.name=="RoosterFlightV24c"),"sixteen isolated complete wingbeat frames");
        Assert(dragonArt.defeat.Length==16 && dragonArt.defeat.All(f=>f!=null && f.texture.name=="RoosterDefeatV30"),"sixteen authored defeated poses with grounded wings, tongue and rising talons");
        Assert(art.icons[34].texture.name=="AnvilV31","anvil uses a substantial compact iron-only silhouette without wood underneath");
        Assert(BuildingSystem.Footprint(ItemKind.Anvil,true)==Vector2Int.one && BuildingSystem.Footprint(ItemKind.Anvil,false)==Vector2Int.one,"anvil occupies exactly one world cell in either placement orientation");
        Assert(art.tableUpright.texture.name=="CraftingTableUprightV31" && art.tableUpright.bounds.size.y>art.tableUpright.bounds.size.x,"upright table uses separately drawn perspective with downward legs");
        Assert(art.campfireBase.texture.name=="CampfireBaseV31","campfire has a readable separate crossed-log and stone base");
        Assert(dragonArt.column.Length==24 && dragonArt.column.All(f=>f!=null),"twenty-four complete wide fire-column keyframes");
        Assert(dragonArt.column.All(f=>f.texture.name=="DragonFireColumnV32"),"sky fire has the pixel-styled atlas without stone bases");
        Assert(art.explosion.All(f=>f.texture.name=="PyromanticExplosionV32"),"all shared explosions use the new 24-frame atlas");
        Assert(FrogMage.FireReach==5 && FrogMage.FireArc==120,"frog and dungeon mage share the five-unit 120-degree fire cone");
        Assert(RoosterDragon.MaxHealth==1500 && RoosterDragon.FlightInterval==20,"boss has 1500 HP and twenty-second flight intervals");
        Assert(PrismWeapon.FusedDrain>PrismWeapon.ThinDrain*3 && PrismWeapon.FullDps>300,"focused prism uses fast mana drain and high fixed DPS");
        for(int d=0;d<8;d++)for(int f=0;f<8;f++)Assert(art.hero[d,f].rect.height>120 && art.hero[d,f].rect.width>65,"complete eight-phase hero frame "+d+" "+f);
        for(int i=0;i<20;i++)Assert(art.icons[i].rect.width>100 && art.icons[i].rect.height>30,"complete icon "+i);
        Assert(art.icons[4].rect.width/art.icons[4].rect.height>=4.5f,"long narrow sword fills vertical inventory slot");
        Assert(s.equipment.Length==8 && Survivor.Fits(new Item(ItemKind.Boots),GearSlot.Feet) && !Survivor.Fits(new Item(ItemKind.Ring),GearSlot.Feet),"separate compatible footwear slot");
        foreach(var kind in new[]{ItemKind.BlueFlower,ItemKind.WhiteFlower,ItemKind.PinkFlower})Assert(ItemDefinition.All[kind].width==1 && ItemDefinition.All[kind].height==2,"flower uses 1x2 cells "+kind);
        float[] oldDuration={.56f,.27f,.68f,.31f},oldCooldown={.76f,.37f,.86f,.42f};int weaponIndex=0;
        foreach(var kind in new[]{ItemKind.Sword,ItemKind.Dagger,ItemKind.Scythe,ItemKind.Spear}){var d=ItemDefinition.All[kind];Assert(d.attackSeconds<oldDuration[weaponIndex]*.75f && d.cooldown<oldCooldown[weaponIndex]*.75f,"faster attack and recovery "+kind);weaponIndex++;}
        Assert(art.ship.rect.width>1450 && art.mannequin.rect.height>590,"large wreck and complete mannequin");
        Assert(art.flowers.Length==11 && art.door!=null && art.galaxy!=null && art.planet!=null,"six original plants and five new wildflower/clover sprites loaded");
        Assert(art.woodlouse.Length==32 && art.woodlouse.All(f=>f!=null && f.rect.width>140 && f.rect.height>90),"complete thirty-two woodlouse walking and curl poses");
        Assert(art.woodlouse[0].bounds.size.x>2.3f && art.woodlouse[0].bounds.size.x<2.65f,"woodlouse length approximately one and a half players");
        Assert(art.flowerMeshes.All(f=>f.vertexCount==18 && f.triangles.Length==48),"subdivided flower meshes bend with anchored stems");
        Assert(art.icons[18].texture.name=="OpenTomeV6","prettier dedicated open book sprite");
        var animal=new WoodlouseVitals();Assert(animal.health==200 && !animal.Curled,"woodlouse starts with 200 HP and no armor");
        Assert(animal.Hurt(24)==24 && animal.health==176 && animal.Curled && animal.curlRemaining==5,"first hit full damage starts five-second curl");
        animal.Tick(2);Assert(Mathf.Abs(animal.Hurt(24)-4.8f)<.001f && animal.curlRemaining==3,"curled armor absorbs exactly 80 percent without resetting timer");
        animal.Tick(3);Assert(!animal.Curled && animal.Hurt(24)==24 && animal.Curled,"uncurled woodlouse takes full damage and begins another defensive curl");
        Assert(art.galaxy.rect.width>1100 && art.ruins.All(r=>r!=null && r.rect.height>200),"detailed galaxy and complete ruined columns and arches");
        Assert(Resources.Load<Texture2D>("Art/PocketFloorV4").width>1000 && Resources.Load<Texture2D>("Art/PlanetRingV4")!=null,"unique floor and textured planetary ring present");
        for(int d=0;d<8;d++)for(int f=0;f<8;f++)Assert(Mathf.Abs(art.hero[d,f].pivot.y-3)<.01f,"fixed foot baseline "+d+" "+f);
        foreach(var kind in new[]{ItemKind.Pyromancy,ItemKind.AbyssTome})
        {
            var testBook=new Item(kind);var bookGrid=new GridInventory(4,3,"new books");bookGrid.allowOpenBooks=true;bookGrid.Add(testBook);
            Assert(bookGrid.ToggleTome(testBook) && testBook.Definition.width==(kind==ItemKind.Pyromancy?2:3) && testBook.Definition.height==2,"new book opens to requested footprint "+kind);
            Assert(bookGrid.ToggleTome(testBook) && !testBook.opened && testBook.Definition.width==(kind==ItemKind.Pyromancy?1:2),"new book closes and releases occupied cells "+kind);
        }
        var fireCraft=new GridInventory(4,3,"campfire recipe");fireCraft.Place(new Item(ItemKind.Wood),0,0);fireCraft.Place(new Item(ItemKind.Wood),1,0);
        Assert(fireCraft.Craft(new RectInt(0,0,2,1),out _) && fireCraft.items.Count==1,"two logs can be selected through their first occupied row");fireCraft.items.Clear();fireCraft.Place(new Item(ItemKind.Wood),0,0);fireCraft.Place(new Item(ItemKind.Wood),1,0);
        Assert(fireCraft.Craft(new RectInt(0,0,2,2),out var campfire) && campfire.kind==ItemKind.Campfire && fireCraft.items.Count==1,"two complete logs craft one unstacked campfire atomically");
        var star=SpatialSigil.Resample(StarSigil.Template,160).Select(p=>p*260+Vector2.one*200).ToList();Assert(StarSigil.Matches(star),"five-point continuous star recognized");star.Reverse();Assert(StarSigil.Matches(star),"reverse star recognized");Assert(!StarSigil.Matches(sigil),"spatial sigil cannot accidentally summon tentacles");
        Assert(art.flame.Length==16 && art.explosion.Length==24 && art.tentacle!=null && art.frog.GetLength(0)==4 && art.frog.GetLength(1)==8,"complete fire explosion tentacle and frog animation assets");
        Assert(ItemDefinition.All[ItemKind.Lamp].width==2 && ItemDefinition.All[ItemKind.Lamp].height==2 && ItemDefinition.All[ItemKind.YoYo].reach==5,"lamp footprint and five-unit yo-yo range");
        var swapGrid=new GridInventory(5,5,"swap");var large=new Item(ItemKind.Lamp);var little=new Item(ItemKind.Brain);swapGrid.Place(large,0,0);swapGrid.Place(little,3,3);
        Assert(swapGrid.Swap(large,swapGrid,little) && large.x==3 && large.y==3 && little.x==0 && swapGrid.Valid(),"occupied inventory cells exchange full footprints atomically");
        var otherGrid=new GridInventory(3,3,"other");var outgoing=new Item(ItemKind.Rope);otherGrid.Place(outgoing,0,0);
        Assert(swapGrid.Swap(large,otherGrid,outgoing) && otherGrid.items.Contains(large) && swapGrid.items.Contains(outgoing) && swapGrid.Valid() && otherGrid.Valid(),"cross-container swap preserves both objects and coordinates");
        var corner=new Item(ItemKind.Brain);swapGrid.Place(corner,4,4);int before=swapGrid.items.Count;
        Assert(!otherGrid.Swap(large,swapGrid,corner) && swapGrid.items.Count==before && otherGrid.items.Contains(large) && corner.x==4,"failed swap does not remove or move either item");
        var bagA=new Item(ItemKind.Sack);var bagB=new Item(ItemKind.Sack);bagA.contents.Add(bagB);Assert(!GridInventory.CanEnter(bagA,bagB.contents),"nested sack cannot be inserted inside its own descendant");
        var auto=new Survivor();auto.inventory.items.Clear();var ringA=new Item(ItemKind.Ring);var ringB=new Item(ItemKind.Ring);auto.inventory.Add(ringA);auto.inventory.Add(ringB);
        Assert(auto.AutoEquipEmpty(ringA,auto.inventory) && auto.AutoEquipEmpty(ringB,auto.inventory) && auto.equipment[5]==ringA && auto.equipment[6]==ringB,"auto equip uses each empty compatible ring slot");
        var w1=new Item(ItemKind.Sword);var w2=new Item(ItemKind.Dagger);auto.inventory.Add(w1);auto.inventory.Add(w2);auto.AutoEquipEmpty(w1,auto.inventory);
        Assert(!auto.AutoEquipEmpty(w2,auto.inventory) && auto.equipment[0]==w1 && auto.inventory.items.Contains(w2),"right-click auto equip preserves occupied hand");
        auto.foods.Clear();for(int i=0;i<5;i++)auto.foods.Add(new FoodEffect{kind=(ItemKind)(i+1),remaining=20,duration=20});auto.mana=0;var brainAtFull=new Item(ItemKind.Brain);auto.inventory.Add(brainAtFull);
        Assert(auto.Eat(brainAtFull,auto.inventory) && auto.mana==100 && auto.foods.Count==5,"brain restores mana even when all satiety cells are full");
        Assert(ItemDefinition.All[ItemKind.Orb].width==2 && ItemDefinition.All[ItemKind.Orb].height==2,"orb uses two by two inventory cells");
        Assert(CoastGame.FireflyVisibility(180,0)==0 && CoastGame.FireflyVisibility(184,0)>0 && CoastGame.FireflyVisibility(184,20)==0 && CoastGame.FireflyVisibility(210,31)>.99f,"fireflies enter gradually instead of appearing at night boundary");
        Assert(art.frog[0,0].bounds.size.y<1.5f && art.frog[0,0].bounds.size.y>1.2f,"frog is half previous size with complete frame");
        Assert(art.icons[20].texture.name=="BattleYoyoV35" && art.icons[21].texture.name=="ItemsV9a" && art.icons[26].texture.name=="ItemsV9a" && art.icons[40].texture.name=="RollerSkatesV35","spiked yo-yo and roller assets preserve existing lamp and books");
        Assert(Resources.Load<AudioClip>("Audio/BeachCelloV9")!=null && Resources.Load<AudioClip>("Audio/PocketCosmosV9")!=null && Resources.Load<AudioClip>("Audio/GullV9-2")!=null,"original cello cosmic and gull audio imported");
        var buyer=new Survivor();Assert(buyer.Coins==10 && buyer.inventory.items.Single(i=>i.kind==ItemKind.Coin).count==10,"initial ten silver coins in one stack");
        buyer.inventory.items.RemoveAll(item=>item.kind!=ItemKind.Coin);
        var merchant=new GridInventory(10,5,"seller");var pie=new Item(ItemKind.Pie);merchant.Add(pie);Assert(buyer.Purchase(merchant,pie,buyer.inventory,8,3) && buyer.Coins==8 && !merchant.items.Contains(pie),"pie purchase consumes two coins and transfers original object");
        var meat=new Item(ItemKind.BeetleMeat);merchant.Add(meat);Assert(!buyer.Purchase(merchant,meat,buyer.inventory,8,3) && buyer.Coins==8 && merchant.items.Contains(meat),"occupied purchase leaves food and balance unchanged");
        Assert(buyer.Purchase(merchant,meat,buyer.inventory,9,4) && buyer.Coins==7,"meat costs one coin");Assert(!buyer.Purchase(merchant,pie,merchant,0,0),"seller cannot buy food back");
        Assert(!buyer.SpendCoins(8) && buyer.Coins==7 && buyer.SpendCoins(4) && buyer.Coins==3,"hire fee is atomic with insufficient funds");
        var stack=new GridInventory(2,1,"coins");stack.Add(new Item(ItemKind.Coin){count=7});stack.Add(new Item(ItemKind.Coin){count=8});Assert(stack.items.Count==2 && stack.items[0].count==10 && stack.items[1].count==5,"coins merge and overflow at ten");
        var largeStack=new GridInventory(3,1,"coins");Assert(largeStack.Add(new Item(ItemKind.Coin){count=25}) && largeStack.items.Select(i=>i.count).SequenceEqual(new[]{10,10,5}),"large coin pickup splits into stacks capped at ten");
        var blockedCoins=new Item(ItemKind.Coin){count=6};Assert(!largeStack.Add(blockedCoins) && blockedCoins.count==6 && largeStack.items.Sum(i=>i.count)==25,"coin pickup failure preserves both balances atomically");
        buyer.mana=45;Assert(buyer.SpendMana(30) && buyer.mana==15 && !buyer.SpendMana(20) && buyer.mana==15,"spell mana costs refuse unpaid casts atomically");
        var lamp=new Item(ItemKind.Lamp){fuel=30};var fuel=new Item(ItemKind.Wood);buyer.inventory.Add(fuel);Assert(buyer.Refuel(fuel,buyer.inventory,lamp) && lamp.fuel==80 && !buyer.inventory.items.Contains(fuel),"wood refuels lamp and consumes one log");
        lamp.Wear(101);Assert(lamp.durability==0 && !lamp.Usable,"equipment breaks at zero durability");Assert(WorldGrid.Snap(new Vector2(-1.1f,2.9f))==new Vector2(-1.5f,2.5f),"world grid uses one-unit cell centres including negative coordinates");
        foreach(var kind in new[]{ItemKind.Tome,ItemKind.Pyromancy,ItemKind.AbyssTome})
        {var bookWithoutWear=new Item(kind);bookWithoutWear.Wear(200);Assert(!bookWithoutWear.Definition.Durable && bookWithoutWear.Usable && bookWithoutWear.durability==100,"closed book has no durability or hidden breakage "+kind);bookWithoutWear.opened=true;bookWithoutWear.Wear(200);Assert(!bookWithoutWear.Definition.Durable && bookWithoutWear.Usable && bookWithoutWear.durability==100,"open book has no durability or hidden breakage "+kind);}
        var expansion=new ExpansionArt(art);Assert(expansion.people.Cast<Sprite>().All(f=>f!=null) && expansion.mage.Cast<Sprite>().All(f=>f!=null) && expansion.lampHero.Cast<Sprite>().All(f=>f!=null),"complete tavern companion and two-hand hero sprite atlases");
        Assert(expansion.keeper.Length==24 && expansion.keeper.All(f=>f!=null && f.texture.name=="KeeperWipeV14"),"twenty-four complete plate-wiping keyframes");
        Assert(ItemDefinition.All[ItemKind.AbyssTome].width==ItemDefinition.All[ItemKind.Tome].width && ItemDefinition.All[ItemKind.OpenAbyssTome].width==ItemDefinition.All[ItemKind.OpenTome].width,"abyss and spatial books have identical footprints");
        Assert(ItemDefinition.All[ItemKind.Pyromancy].width==1 && ItemDefinition.All[ItemKind.OpenPyromancy].width==2,"fire book retains requested original inventory footprint");
        var chunk=new Vector2Int(-4,15);var sites=MeadowLayout.Generate(chunk,74391);var same=MeadowLayout.Generate(chunk,74391);var different=MeadowLayout.Generate(chunk,9527);
        Assert(sites.Count>150 && sites.Select(f=>f.id).SequenceEqual(same.Select(f=>f.id)) && sites.Select(f=>f.position).SequenceEqual(same.Select(f=>f.position)),"procedural meadow is deterministic independent of generation order");
        Assert(!sites.Select(f=>f.position).SequenceEqual(different.Select(f=>f.position)),"new world seed changes procedural flower layout");
        Assert(sites.All(f=>MeadowLayout.IsExtension(f.position)) && sites.Select(f=>f.id).Distinct().Count()==sites.Count,"generated flowers stay in extension with unique global identities");
        var neighbours=MeadowLayout.Generate(chunk+Vector2Int.right,74391);Assert(!sites.Select(f=>f.id).Intersect(neighbours.Select(f=>f.id)).Any(),"adjacent chunks cannot duplicate flowers");
        Assert(!MeadowLayout.IsExtension(new Vector2(10,15)) && MeadowLayout.IsExtension(new Vector2(10,80)) && MeadowLayout.IsExtension(new Vector2(-100,90)),"original meadow is preserved and extension continues north east and west");
        Assert(MeadowLayout.Walkable(new Vector2(100,850)) && !MeadowLayout.Walkable(new Vector2(450,950)) && !MeadowLayout.Walkable(new Vector2(513,950)) && !MeadowLayout.Walkable(new Vector2(0,1025)) && !MeadowLayout.Walkable(new Vector2(0,-8)),"huge meadow bounds retain ocean collision");
        Assert(MeadowLayout.MapBounds.width*1024>650*60*25,"world bounds enclose an enormous irregular island");
        Assert(MeadowLayout.ChunkAt(new Vector2(-.1f,31.9f))==new Vector2Int(-1,0),"negative-world chunk coordinates use floor division");
        var explored=new ExploredRegion(MapRegion.Coast,MeadowLayout.MapBounds,2,74391);
        Assert(explored.texture.GetPixels32().Count(p=>p.a>0)*4>350*60*25,"actual irregular land area exceeds original meadow by over 350 times");
        Assert(explored.VisitedCells==0 && !explored.IsVisited(new Vector2(200,600)),"new map contains no revealed terrain");
        explored.Reveal(new Vector2(0,0),11);int visited=explored.VisitedCells;
        Assert(explored.IsVisited(Vector2.zero) && !explored.IsVisited(new Vector2(200,600)) && visited>50,"map reveals only circular surroundings of actual visit");
        explored.Reveal(new Vector2(200,600),11);Assert(explored.IsVisited(Vector2.zero) && explored.IsVisited(new Vector2(200,600)) && !explored.IsVisited(new Vector2(180,400)) && explored.VisitedCells>visited,"separated visits preserve explored areas without uncovering space between them");
        explored.Dispose();
        Debug.Log("MODEL_CHECKS_PASSED "+count);
    }
}
