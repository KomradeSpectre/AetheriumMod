### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Unity Project](#unity-project)
    2.1. [Project Setup](#project-setup)
    2.2. [Creating your first Prefab](#creating-your-first-prefab)
    2.3. [Icon Creation](#icon-creation)
    2.4. [Creating an Asset Bundle](#creating-an-asset-bundle)
3. [Visual Studio Project](#visual-studio-project)
    3.1. [Preparing the Base Class](#preparing-the-base-class)
    3.2. [Abstraction and You (Creating Item Base and Equipment Base Classes)](#abstraction-and-you-(creating-item-base-and-equipment-base-classes))

----
# Prerequisites
----------
- Read through this tutorial to set up your first project, and get a feel for the environment. Link: https://github.com/risk-of-thunder/R2Wiki/wiki/%5BIn-depth%5D-First-mod

- Download and install Unity. Current version is: https://download.unity3d.com/download_unity/e6e9ca02b32a/Windows64EditorInstaller/UnitySetup64-2018.4.16f1.exe

----------
# Unity Project
--------
## Project Setup

1. After installing Unity, start it up.  

2. Create new project.

3. Fill the `Project Name` field with your mod name, Template Type is `3D`.
4. Hit `Create`.
5. On the bottom left of the window, you will see the file explorer for Unity. Delete the `Scenes` folder.
6. Clear the scene hierarchy at the top left.
7. We'll create a folder structure to keep our assets organized, create the following folder structure:
```
Assets
    Models
        Prefabs
            Item
                First Item
            Equipment
        Meshes
    Shaders
    Textures
        Icons
            Item
            Equipment
            Buff
        Materials
            Item
                First Item
            Equipment
```
-----

## Creating your first Prefab
1. In the above structure, meshes are your raw models that you made in your 3D program of choice. Prefabs are the GameObject/Model we'll be making out of them. Go ahead and make a model or find one and place it in the Meshes folder.

2. Drag and drop that model to the scene hierarchy at the top left of the Unity window.
3. You'll notice it is in the scene, but it might be untextured (if you did not bake textures into the model). If this is the case, proceed. If it is not the case, click the model in the scene hierarchy at the top left and drag it to your `Models -> Prefabs -> Item -> First Item` folder and skip to `Step 9`.
4. Click into `Materials -> Item -> First Item`.
5. Right Click anywhere in this folder and `Create -> Material`. 
6. A new material will be created and ask you for a name. The general practice for naming materials is to name them after what you're applying it to. So, if you're making a material for a handle of a mesh then `ItemNameHandle`.
7. Click the material, and at the top right you should see all kinds of properties. For now, just modify the `Color`, `Smoothness`, and `Emission` to your liking.
8. Once done, to apply it you simply drag and drop it either on the part of the model, or the part in the scene hierarchy you want to apply it to.
9. Once done setting up materials and applying them, drag the Prefab (in the scene hierarchy it has a box next to it) to your `Models -> Prefabs -> Item -> First Item` folder.
10. When asked if you want to make an `Original Prefab` or a `Variant Prefab`, choose `Original`. To get the base for an icon, proceed with the following section, otherwise skip over it.
---------

## Icon Creation
1. Assuming the model is still in the scene from the previous steps, continue. Otherwise drag the prefab into the scene hierarchy, and continue.

2. In the scene itself, turn off the `Skybox`.
3. In the scene itself, on the top right of it go to `Gizmos -> Uncheck Show Grid`.
4. Right click the compass, and pick a facing you'd like to have your icon be. Alternatively, just rotate the prefab via the values at the top right till you get the alignment you like, but make sure to set the values back after.
5. Using a screen capture tool (like Lightshot, Greenshot, Printscr, etc), capture the area encompassing the model. Copy it to clipboard.
6. Open your image editor of choice, though for this tutorial I'll cover how to utilize the `Hopoo Outline Template`. That said, open `Photoshop`.
7. Create a new image, hit `OK`.
8. Paste in your image that you captured previously.
9. Select the `Magic Wand` tool, lower the tolerance down to around `0`.
10. Select the gray areas that were from the background in Unity.
11. Delete the selection.
12. Invert the selection, you should now have a selection around your item icon.
13. At the top, `Image -> Crop`. The image should now only contain the area directly containing the icon.
14. Try to get the image size as close as you can to `460x460`. It doesn't have to be exact here, just one of the dimensions should be close to this.
15. Increase `Canvas Size` to `512x512`. There should be ample space around your icon right now.
16. `Filters -> Blur -> Blur More`
17. Download the Hopoo Outline Templates: https://cdn.discordapp.com/attachments/567827235013132291/769053836077432872/RoR2_Public_Templates.rar
18. After extracting it, open the `Item Icon Template.psd`
19. Expand the Layer Groups on the bottom right and delete any of the icons they had in there previously. Hide any other layers/sublayers aside from the one you want to place your item in.
20. In your icon image, `Select All` and `Copy`.
21. In the Item Icon Template, click the appropriate layer group for the Tier of your item.
22. CRTL+V to paste it in.
23. If everything looks alright, with the layer selected go to `File -> Export -> Export As`.
24. Change the `Export Dimensions` to `128x128` and save.
27. Drag the resultant file to your Unity Instance's `Textures -> Icons -> Item` folder.
28. Click it, and on the top right you'll see `Alpha is Transparency`. Click it.
29. Increase Anisotropy Level to max, or just leave it alone.
30. At the bottom set compression to `None`.
31. Hit `Apply`.
32. At the top of that same area, where it says `Texture Type`, click it and set it to `Sprite (2D and UI)`.
33. Hit `Apply`, and you're done with the icon. Congrats!
---------

## Creating an Asset Bundle  

For asset bundles, think of them as if they're a central zip/pak/etc that stores all your assets in a project (usually excluding sound assets in ROR2). To get started, do the following: 

1. On the top of the unity editor, click `Window -> Package Manager`, find `Asset Bundle Browser`, and click `install`.

2. Click any of the materials/prefabs/icons we made thus far and on the bottom right where it previews it, you'll see AssetBundle with a dropdown menu next to it. Click the dropdown menu.

3. In that menu, hit `New..` and name your asset bundle. For example, `mycoolmod_assets`.

4. In that same dropdown menu, if it is not already assigned, assign it to your new assetbundle by clicking the name.

5. Mixed results here, but you can either click through all your assets in the project and assign them to the asset bundle and skip over the next step, or continue.

6. On the top of the unity editor, Click `Window -> AssetBundle Browser`. It will pull up a window that should show your assetbundle name in the first tab. Click that, and then one of the assets on the list to the right. If a ton show up that you're sure you haven't assigned but want to, click one and do CRTL+A. Generally you'll know this if the list contains folders. Finally, on the bottom right of the unity editor, assign it to the assetbundle and continue to the next step.

7. With this same window up, click the tab at the top of it that says `Build`.

8. Assuming you went through the tutorial to set up a Visual Studio project, in the assetbundle browser window under `Output Path` click browse and navigate to your visual studio project folder. You can find these by default in `YourUserFolder/repos/`. You want to select the directory that contains your CSProj file.

9. Don't click any of the checkboxes in the Build tab, just hit `Build`.
 
10. Congrats, you've just built your first asset bundle.

-----
# Visual Studio Project
---


## Preparing the Base Class
1. Name your base class anything like `Main.cs` or `MyModName.cs` to keep track of it. 

2. Fill it out with the following:
```csharp
using BepInEx;
using R2API;

namespace MyModCsProjDirectoryName
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin("com.MyUsername.MyModName", "My Mod's Title and if we see this exact name on Thunderstore we will deprecate your mod", "1.0.0")]
    public class MyModName : BaseUnityPlugin
    {
        public void Awake()
        {
        }
    }
}
```

3. Change the namespace to the name of the folder you store the csproj in, for example:
```csharp
namespace IfMyNamespaceRemainsThisItMeansIDidn'tRead
{
```

4. Change the relevant fields in the BepinPlugin attribute to suit your mod. For this tutorial we're going to extract these out into fields we'll define in our main class. For example:
```csharp
[BepinPlugin(ModGuid, ModName, ModVer)]
```  

5. Change the class name to match the class name file's name. For example, if you named your class Main.cs, then:
```csharp
public class Main : BaseUnityPlugin
{
```

6. We'll create the fields for our plugin attribute, the thing I mentioned in step 4. You should generally update the ModVer field any time you plan to make an update to your mod. 
```csharp
public class Main : BaseUnityPlugin
{
    public const string ModGuid = "com.MyUserName.MyModName"; //Our Package Name
    public const string ModName = "MyModName";
    public const string ModVer = "0.0.1";

	public void Awake()
	{
	}
}
```

7. *How do I know what number to change on my mod version?* Simple really, the format for this is called the Major.Minor.Patch Versioning Standard and they generally follow this guideline:
```
MAJOR version when you either complete your development milestones, or make extreme changes to your mod.
MINOR version when you are adding minor features (like another item for instance)
PATCH version increment when you're patching bugs, doing small tweaks, polishing assets, etc.
```

8. Assuming you followed the Asset Bundle creation portion of this tutorial, the asset bundle should be showing up in your Solution Explorer to the right as well as its Manifest file. Click the asset bundle file.

9. On the bottom right of Visual Studio's window, you'll see the properties for the Asset Bundle file. Set `Build Action` to `Embedded Resource`.

10. Set `Copy to Output Directory` to `Do Not Copy`

11. In your Main.cs, add the following into your `Awake()` method
```csharp
public class Main : BaseUnityPlugin
{
    public const string ModGuid = "com.MyUserName.MyModName"; //Our Package Name
    public const string ModName = "MyModName";
    public const string ModVer = "0.0.1";

	public void Awake()
	{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MyCoolModsNamespaceHere.mycoolmod_assets"))
		{
			var bundle = AssetBundle.LoadFromStream(stream);
			var provider = new AssetBundleResourcesProvider("@MyModNamePleaseReplace", bundle);
			ResourcesAPI.AddProvider(provider);
		}
	}
}
```

12. We'll need to add a submodule dependency for ResourcesAPI so it loads in upon startup. We can do so via adding the `R2APISubmoduleDependency` attribute and setting its parameters. To do so, we do the following:
```csharp
using BepInEx;
using R2API;

namespace MyModCSProjDirectoryName
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepinPlugin(ModGuid, ModName, ModVer)]
    [R2APISubmoduleDependency(nameof(ResourcesAPI))]
    
    public class Main : BaseUnityPlugin
    {
        public const string ModGuid = "com.MyUserName.MyModName"; //Our Package Name
        public const string ModName = "MyModName";
        public const string ModVer = "0.0.1";
    
    	public void Awake()
    	{
    		using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MyCoolModsNamespaceHere.mycoolmod_assets"))
    		{
    			var bundle = AssetBundle.LoadFromStream(stream);
    			var provider = new AssetBundleResourcesProvider("@MyModNamePleaseReplace", bundle);
    			ResourcesAPI.AddProvider(provider);
    		}
    	}
    }
}
```

13. As we utilize more of R2API, we'll need to add more onto the `R2APISubmoduleDependency` attribute. Doing so just requires you to alter it like so with whatever API you need to load in. In this example we'll load in ItemAPI alongside the ResourcesAPI:
```csharp
[R2APISubmoduleDependency(nameof(ResourcesAPI), nameof(ItemAPI))]
```
14. Easy right? Let's move on to the harder bits now.

-----------

## Abstraction and You (Creating Item Base and Equipment Base classes) 
If you're not too experienced with formal programming you're probably wondering, what *is* abstraction? In the context of this tutorial, we'll be creating abstract "ItemBase" and "EquipmentBase" classes. Imagine them to be a skeleton that all our items and all our equipment share. One that we add the muscle and skin to in our individual items and equipment. Let's get started.

1. In the solution explorer, right click on the CSProj file (the green rectangle with a C# in it) and `Add -> New Folder`, you'll be doing this twice. Name one `Items` and one `Equipment`.

2. Right click the `Items` folder, and `Add -> Class`.
3. Name the class `ItemBase`.
4. At the top of the class, with the other using, we need to add the `R2API` and `RoR2` name space usings. It should look similar to the following:
```csharp
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;
```
5. Add a `public` access modifier to the front of your class definition, and an `abstract` after it. For example:
```csharp
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Items
{
    public abstract class ItemBase
    {
    }
}
```
6. The `abstract` keyword lets us create a class in which all things that inherit from it must implement its fields/methods/etc that are abstract. Best part of this is that when we make an item inherit from it, we can use a quick action to quickly generate in those fields for us to fill out! First things first, we should think of what all our items need and what things they will all have in common. To this end we'll need:
    - A `Name` field - This is the name you give to your item. Like `"Omnipotent Egg"` for example.

    - A `Name Language Token` field - This is what the code will identify this item by. Like `"OMNIPOTENT_EGG"` for example.
    - A `Pickup Description` field - This is the short description of our item.
    - A `Full Item Description` field - This is the long description of our item. It should be detailed.
    - A `Lore Entry` field - This is our lore snippet for the item, they're optional but users like to read them sometimes.
    - An `Item Tier` field - This is what tier our item will appear in. `ItemTier.Lunar` for example.
    - An `Item Model Path` field  - This is the path to our item model in our asset bundle. Like `"@MyModName:Assets/Models/Prefabs/Item/OmnipotentEgg/OmnipotentEgg.prefab"`
    - An `Item Icon Path` field - This is the path to our item icon in our asset bundle. Like `"@MyModName:Assets/Textures/Icons/Item/OmnipotentEgg.png"`
    - An `Initialization` method - Necessary in ordering your code execution flow in your items/item base (or what executes in what order). Also in the context of this tutorial, it will allow you to easily pass through the config file provided to you automatically by BepinEx to allow easy config entries. More on this later.
    - An `Item Display Rule Setup` method - Necessary for an easy time setting up the actual ingame display for your item models. The `ItemDisplayRuleDict` we return from this method will allow us to attach our item display rules to our item definition.