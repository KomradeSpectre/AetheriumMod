### Table of Contents

1. [Prerequisites](#prerequisites)  
2. [Unity Project](#unity-project)  
    2.1. [Project Setup](#project-setup)  
    2.2. [Creating your first Prefab](#creating-your-first-prefab)  
    2.3. [Icon Creation](#icon-creation)  
    2.4. [Creating an Asset Bundle](#creating-an-asset-bundle)  
3. [Visual Studio Project](#visual-studio-project)  
    3.1. [Preparing the Base Class](#preparing-the-base-class)  
    3.2. [Abstraction and You (Creating Item Base and Equipment Base Classes)](#creating-item-base-and-equipment-base-classes)  

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

## Creating Item Base and Equipment Base classes
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
    - A `Hooks` method - Necessary to keep track of what events/methods we subscribe to that we use to give our item its functionality.
    - Well add more fields later as we need them, but for now we have our required abstract properties/fields/methods here.
    
7. Let's start adding these fields/properties in to the abstract class. We'll use the same access and keyword before our type so things inheriting the Item Base must implement them to use the interface. We'll do so for `Name`, `Name Language Token`, `Pickup Description`, `Full Item Description`, and `Lore Entry` firstly.
8. Fill out your item base to look like the following:
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
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }
    }
}
```
9. Let's add in the rest of the properties on our needs list. Like so:
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
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }
        
        public abstract ItemTier Tier { get; }
        
        public abstract string ItemModelPath { get; }
        public abstract string ItemIconPath { get; }
    }
}
```
10. Now we'll need to create our abstract methods, these are slightly different to define than the properties above. For `Initialization`, we'll do something special. We'll add a parameter to pass in a `ConfigFile`. *Why you ask?* It is so we can use the BepinEx configuration that our main plugin class inherits to provide easy config options to our items, we do this by forwarding it to a Config method on our item classes, more on that later. We'll also need to add a using for BepinEx.Configuration. So to define this method, we do:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Items
{
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }
        
        public abstract ItemTier Tier { get; }
        
        public abstract string ItemModelPath { get; }
        public abstract string ItemIconPath { get; }
        
        public abstract void Init(ConfigFile config);
    }
}
```
11. Adding in our final needs on the checklist we made should be simple at this point. It's similar in definition to the above, but with no parameters and a different return type. We'll add it like so:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Items
{
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }
        
        public abstract ItemTier Tier { get; }
        
        public abstract string ItemModelPath { get; }
        public abstract string ItemIconPath { get; }
        
        public abstract void Init(ConfigFile config);
        
        public abstract ItemDisplayRuleDict CreateItemDisplayRules();
        
        public abstract void Hooks();
    }
}
```
12. If you've been following along, we're actually almost to the point we can use our ItemBase now. Amazing, right? For the next step, we can create a method to set up the language tokens inside of our ItemBase. This will give this functionality with no requirement to implement it on the inheritors, it simply relies on being initialized and the assumption we've implemented our strings (the first 5 properties of our itembase).

13. To create the method we will use to set up our language tokens, we will be creating a method with the `protected` access modifier and a `void` return type. `Protected` means only itself and subtypes of itself can use the method.  To do so we do the following:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Items
{
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }
        
        public abstract ItemTier Tier { get; }
        
        public abstract string ItemModelPath { get; }
        public abstract string ItemIconPath { get; }
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();
        
        public abstract void Hooks();        
    }
}
```
14. Now we need to fill this method out so we register our language tokens on each item that inherits this abstract class. To do this, we'll need to use LanguageAPI to add our language tokens. For this step we'll use the overload of their addition method that requires `key` and `value`. Our `key` is our name identifier for the token (or the ID for it) like in the case of our item base, it'd be like `"ITEM_OMNIPOTENT_EGG_NAME"`. The `value` is the actual values of our fields, like `"Omnipotent Egg"`. To fill this out almost automatically, we'll be using the properties we defined earlier.
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Items
{
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }
        
        public abstract ItemTier Tier { get; }
        
        public abstract string ItemModelPath { get; }
        public abstract string ItemIconPath { get; }
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);            
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();
        
        public abstract void Hooks();        
    }
}
```
15. Now back in our main class, we'll need to add the LanguageAPI submodule dependency. Like so:
```csharp
using BepInEx;
using R2API;

namespace MyModCSProjDirectoryName
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepinPlugin(ModGuid, ModName, ModVer)]
    [R2APISubmoduleDependency(nameof(ResourcesAPI), nameof(LanguageAPI))]
    
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
16. Next, we'll make another method in our ItemBase to create our item definition out of our properties and language tokens, `CreateItem()` similar to the `CreateLang()` method we did above.
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Items
{
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }
        
        public abstract ItemTier Tier { get; }
        
        public abstract string ItemModelPath { get; }
        public abstract string ItemIconPath { get; }
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);            
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();
        
        protected void CreateItem()
        {
            
        }
        
        public abstract void Hooks();        
    }
}
```
17. What we'll want to populate this method with is a way to create an item definition. To do this, we will be creating a new `ItemDef`. To do so, let us think about all the things an `ItemDef` shares, and what they do so we can plan out how to lay it out. An `ItemDef` contains the following major things:
    - `name`- The identifier of the item. For example `ITEM_OMNIPOTENT_EGG`.

    - `nameToken` - The string corresponding to the language token for the name of the item. We defined this above. For example `ITEM_OMNIPOTENT_EGG_NAME`.
    - `pickupToken` - The string corresponding to the language token for the pickup description of the item. For example, `ITEM_OMNIPOTENT_EGG_PICKUP`.
    - `descriptionToken` - The string corresponding to the language token for the detailed description of the item. For example, `ITEM_OMNIPOTENT_EGG_DESCRIPTION`.
    - `loreToken` - The string corresponding to the language token for the lorebook entry of the item. For example, `ITEM_OMNIPOTENT_EGG_LORE`.
    - `pickupModelPath` - The string path that leads to the model for the item in our asset bundle. Since we have defined a provider (`@MyModName`) in our main class while setting up the asset bundle, we can use it in our string. For example, `"@MyModName:Assets/Models/Prefab/Item/OmnipotentEgg/OmnipotentEgg.prefab"`.
    - `pickupIconPath` - The string path that leads to the icon for the item in our asset bundle. For example, `"@MyModName:Assets/Textures/Icons/Item/OmnipotentEggIcon.png"`.
    - `hidden` - A boolean that determines whether or not the item in question will be displayed upon the others in the lore screen, and item selections. **That said, not every item requires this.**
    - `tags` - An enum array that represents the category of your item, and gives your item unique features from the base game. An example is `ItemTag.AIBlacklist` which will prevent the AI from obtaining or using your item. Another is `ItemTag.Utility` which will make the item appear in `Utility` chests. **That said, not every item requires this.**
    - `canRemove` - A boolean that determines whether or not we can drop the item, or remove it from our inventory with things like the Bazaar cauldrons. **That said, not every item requires this.**
    - `tier` - An enum that determines what Tier the item will appear. Not only does this contain `ItemTier.Tier1`, `ItemTier.Tier2`, and `ItemTier.Tier3` but also contains unique tiers like `ItemTier.Boss`, `ItemTier.Lunar`, and `ItemTier.None`
    
18. In the above, we identified all the needed major components of the `ItemDef` we'll be writing, but we also identified a few *optional* parts. For these, we don't want to force inheritors of the `ItemBase` to implement them, but we want to provide the option to do so *and* still have a value we can use if they don't. For this, we'll utilize the `virtual` keyword to do so on a few properties and give it a default value for those who don't want to implement it.
19. Let's implement these properties now. The way to do this is similar in method to how we did our `abstract` properties, but with one key difference, we'll give one them a default value as well. Like so:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Items
{
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }
        
        public abstract ItemTier Tier { get; }
        public virtual ItemTag[] ItemTags { get; }
        
        public abstract string ItemModelPath { get; }
        public abstract string ItemIconPath { get; }
        
        public virtual bool CanRemove { get; } = true;
        public virtual bool Hidden { get; } = false;        
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);            
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();
        
        protected void CreateItem()
        {
            
        }

        public abstract void Hooks();        
    }
}
```
20. Now we create our ItemDef. It is made up of all the properties we've defined thus far in a manner of speaking.
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Items
{
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }
        
        public abstract ItemTier Tier { get; }
        public virtual ItemTag[] ItemTags { get; }
        
        public abstract string ItemModelPath { get; }
        public abstract string ItemIconPath { get; }
        
        public virtual bool CanRemove { get; } = true;
        public virtual bool Hidden { get; } = false;        
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);            
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();
        
        protected void CreateItem()
        {
            ItemDef itemDef = new RoR2.ItemDef()
            {
                name = "ITEM_" + ItemLangTokenName,
                nameToken = "ITEM_" + ItemLangTokenName + "_NAME",
                pickupToken = "ITEM_" + ItemLangTokenName + "_PICKUP",
                descriptionToken = "ITEM_" + ItemLangTokenName + "_DESCRIPTION",
                loreToken = "ITEM_" + ItemLangTokenName + "_LORE",
                pickupModelPath = ItemModelPath,
                pickupIconPath = ItemIconPath,
                hidden = Hidden,
                tags = ItemTags,
                canRemove = CanRemove,                
                tier = Tier
            };            
        }
        
        public abstract void Hooks();        
    }
}
```
21. To register the item, we'll need to use `ItemAPI`. `ItemAPI.Add()` requires one argument, a new `CustomItem`. `CustomItem` requires the `ItemDef` we just created, and an `ItemDisplayRuleDict` which we have created a method for earlier and will return an Index of our newly registered item. First things first, let's create a variable to store our `ItemDisplayRuleDict` and we'll do it by calling that method. Like so:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Items
{
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }
        
        public abstract ItemTier Tier { get; }
        public virtual ItemTag[] ItemTags { get; }
        
        public abstract string ItemModelPath { get; }
        public abstract string ItemIconPath { get; }
        
        public virtual bool CanRemove { get; } = true;
        public virtual bool Hidden { get; } = false;        
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);            
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();
        
        protected void CreateItem()
        {
            ItemDef itemDef = new RoR2.ItemDef()
            {
                name = "ITEM_" + ItemLangTokenName,
                nameToken = "ITEM_" + ItemLangTokenName + "_NAME",
                pickupToken = "ITEM_" + ItemLangTokenName + "_PICKUP",
                descriptionToken = "ITEM_" + ItemLangTokenName + "_DESCRIPTION",
                loreToken = "ITEM_" + ItemLangTokenName + "_LORE",
                pickupModelPath = ItemModelPath,
                pickupIconPath = ItemIconPath,
                hidden = Hidden,
                tags = ItemTags,
                canRemove = CanRemove,                
                tier = Tier
            };       
            var itemDisplayRuleDict = CreateItemDisplayRules();
        }
        
        public abstract void Hooks();        
    }
}
```
22. Now that we have the `ItemDisplayRuleDict`, we can register the item with R2API's `ItemAPI`. Firstly, let's add a field `Index` to our `ItemBase` that will store the index of our item when we register it. This index can be used later in a multitude of ways, one of which allows us to easily create an Inventory Count method to track how many of our item we have. To create the field, we just do:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Items
{
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }
        
        public abstract ItemTier Tier { get; }
        public virtual ItemTag[] ItemTags { get; }
        
        public abstract string ItemModelPath { get; }
        public abstract string ItemIconPath { get; }
        
        public virtual bool CanRemove { get; } = true;
        public virtual bool Hidden { get; } = false;
        
        public ItemIndex Index;
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);            
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();
        
        protected void CreateItem()
        {
            ItemDef itemDef = new RoR2.ItemDef()
            {
                name = "ITEM_" + ItemLangTokenName,
                nameToken = "ITEM_" + ItemLangTokenName + "_NAME",
                pickupToken = "ITEM_" + ItemLangTokenName + "_PICKUP",
                descriptionToken = "ITEM_" + ItemLangTokenName + "_DESCRIPTION",
                loreToken = "ITEM_" + ItemLangTokenName + "_LORE",
                pickupModelPath = ItemModelPath,
                pickupIconPath = ItemIconPath,
                hidden = Hidden,
                tags = ItemTags,
                canRemove = CanRemove,                
                tier = Tier
            };       
            var itemDisplayRuleDict = CreateItemDisplayRules();
        }
        
        public abstract void Hooks();        
    }
}
```
23. Now the moment you've been waiting for, where we register our item. We just need to feed in our `itemDef` and our `itemDisplayRuleDict` to `ItemAPI.Add()` and set our `Index` too.
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Items
{
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }
        
        public abstract ItemTier Tier { get; }
        public virtual ItemTag[] ItemTags { get; }
        
        public abstract string ItemModelPath { get; }
        public abstract string ItemIconPath { get; }
        
        public virtual bool CanRemove { get; } = true;
        public virtual bool Hidden { get; } = false;
        
        public ItemIndex Index;
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);            
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();
        
        protected void CreateItem()
        {
            ItemDef itemDef = new RoR2.ItemDef()
            {
                name = "ITEM_" + ItemLangTokenName,
                nameToken = "ITEM_" + ItemLangTokenName + "_NAME",
                pickupToken = "ITEM_" + ItemLangTokenName + "_PICKUP",
                descriptionToken = "ITEM_" + ItemLangTokenName + "_DESCRIPTION",
                loreToken = "ITEM_" + ItemLangTokenName + "_LORE",
                pickupModelPath = ItemModelPath,
                pickupIconPath = ItemIconPath,
                hidden = Hidden,
                tags = ItemTags,
                canRemove = CanRemove,                
                tier = Tier
            };       
            var itemDisplayRuleDict = CreateItemDisplayRules();
            Index = ItemAPI.Add(new CustomItem(itemDef, itemDisplayRuleDict));
        }
        
        public abstract void Hooks();        
    }
}
```
24. Once again, we'll need to return to our main class and add another `R2APISubmoduleDependency`. This time, it's `ItemAPI`. We'll add it with our usual process:
```csharp
using BepInEx;
using R2API;

namespace MyModCSProjDirectoryName
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepinPlugin(ModGuid, ModName, ModVer)]
    [R2APISubmoduleDependency(nameof(ResourcesAPI), nameof(LanguageAPI), nameof(ItemAPI))]
    
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
25. Before we wrap this section up, let's add a few helper method to our `ItemBase` class that will allow us to easily get the count of items that inherit our `ItemBase`. We'll call these methods `GetCount`. A key thing to note about inventory is that it's not just stored on the `CharacterBody` component, but also the `CharacterMaster` component. So we'll define a `GetCount` method for both.
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Items
{
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }
        
        public abstract ItemTier Tier { get; }
        public virtual ItemTag[] ItemTags { get; }
        
        public abstract string ItemModelPath { get; }
        public abstract string ItemIconPath { get; }
        
        public virtual bool CanRemove { get; } = true;
        public virtual bool Hidden { get; } = false;
        
        public ItemIndex Index;
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);            
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();
        
        protected void CreateItem()
        {
            ItemDef itemDef = new RoR2.ItemDef()
            {
                name = "ITEM_" + ItemLangTokenName,
                nameToken = "ITEM_" + ItemLangTokenName + "_NAME",
                pickupToken = "ITEM_" + ItemLangTokenName + "_PICKUP",
                descriptionToken = "ITEM_" + ItemLangTokenName + "_DESCRIPTION",
                loreToken = "ITEM_" + ItemLangTokenName + "_LORE",
                pickupModelPath = ItemModelPath,
                pickupIconPath = ItemIconPath,
                hidden = Hidden,
                tags = ItemTags,
                canRemove = CanRemove,                
                tier = Tier
            };       
            var itemDisplayRuleDict = CreateItemDisplayRules();
            Index = ItemAPI.Add(new CustomItem(itemDef, itemDisplayRuleDict));
        }
        
        public abstract void Hooks();
        
        public int GetCount(CharacterBody body)
        {
        }
        
        public int GetCount(CharacterMaster master)
        {
        }        
    }
}
```
26. The process of populating these two methods is going to be extremely similar between the two of them. First, we'll do a condition with an early return to null check the parameter and passing that the parameter's inventory. If our parameter or its inventory is null, we will early return a value of `0`, as in `0` items. If our parameter or its inventory is not null, we'll get the count of our items in that inventory using `parameter.inventory.GetItemCount` which takes our field we defined earlier, `Index`. Let's go ahead and populate those methods like so:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Items
{
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }
        
        public abstract ItemTier Tier { get; }
        public virtual ItemTag[] ItemTags { get; }
        
        public abstract string ItemModelPath { get; }
        public abstract string ItemIconPath { get; }
        
        public virtual bool CanRemove { get; } = true;
        public virtual bool Hidden { get; } = false;
        
        public ItemIndex Index;
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);            
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();
        
        protected void CreateItem()
        {
            ItemDef itemDef = new RoR2.ItemDef()
            {
                name = "ITEM_" + ItemLangTokenName,
                nameToken = "ITEM_" + ItemLangTokenName + "_NAME",
                pickupToken = "ITEM_" + ItemLangTokenName + "_PICKUP",
                descriptionToken = "ITEM_" + ItemLangTokenName + "_DESCRIPTION",
                loreToken = "ITEM_" + ItemLangTokenName + "_LORE",
                pickupModelPath = ItemModelPath,
                pickupIconPath = ItemIconPath,
                hidden = Hidden,
                tags = ItemTags,
                canRemove = CanRemove,                
                tier = Tier
            };       
            var itemDisplayRuleDict = CreateItemDisplayRules();
            Index = ItemAPI.Add(new CustomItem(itemDef, itemDisplayRuleDict));
        }
        
        public abstract void Hooks();
        
        public int GetCount(CharacterBody body)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(IndexOfItem);
        }

        public int GetCount(CharacterMaster master)
        {
            if (!master || !master.inventory) { return 0; }

            return master.inventory.GetItemCount(IndexOfItem);
        }
    }
}
```
With that, we've defined an `ItemBase` that we can use for our items. We'll improve it later when we get around to adding configuration options to our items.

--------------------
## Creating the Equipment Base
-----
1. With the creation of the `ItemBase` class, the creation of the `EquipmentBase` class will be a much quicker task. First off, right click your CSPROJ and create a new folder `Equipment`. Then, right click that folder `Add -> New Class` and name it `EquipmentBase`. `EquipmentBase` will share all of our language token properties and path properties, so let's just implement those. This time we'll call them `EquipmentName`, `EquipmentLangTokenName`, and etc. Like so:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Equipment
{
    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }
        
        public abstract string EquipmentModelPath { get; }
        public abstract string EquipmentIconPath { get; }
    }
}
```
2. We will use the same `Initialization` method that we do in our `ItemBase`, as well as the `CreateLang` method, `CreateItemDisplayRules` method, and the `Hooks` method. Let's go ahead and add those in.
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Equipment
{
    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }
        
        public abstract string EquipmentModelPath { get; }
        public abstract string EquipmentIconPath { get; }
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
        }        
        
        public abstract ItemDisplayRuleDict CreateItemDisplayRules();        
        
        public abstract void Hooks();        
    }
}
```
3. Populate the `CreateLang` method similar to how you did in `ItemBase` using our properties. Like so:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Equipment
{
    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }
        
        public abstract string EquipmentModelPath { get; }
        public abstract string EquipmentIconPath { get; }
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_NAME", EquipmentName);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP", EquipmentPickupDesc);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION", EquipmentFullDescription);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_LORE", EquipmentLore);
        }       
        
        public abstract ItemDisplayRuleDict CreateItemDisplayRules();        
        
        public abstract void Hooks();        
    }
}
```
4. We'll need a `CreateEquipment` method similar to how we have a `CreateItem` method in our `ItemBase`. Let's add that in:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Equipment
{
    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }
        
        public abstract string EquipmentModelPath { get; }
        public abstract string EquipmentIconPath { get; }
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_NAME", EquipmentName);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP", EquipmentPickupDesc);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION", EquipmentFullDescription);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_LORE", EquipmentLore);
        }       
        
        public abstract ItemDisplayRuleDict CreateItemDisplayRules();        
        
        protected void CreateEquipment()
        {
        }        
        
        public abstract void Hooks();        
    }
}
```
5. Just like before, we need a definition for our equipment. This isn't `ItemDef` this time though it's `EquipmentDef` and they have a slightly different setup. To that end, let's brainstorm what major components an `EquipmentDef` has so we can plan out our class. An EquipmentDef has:
    - `name` - Just like before, this is the identifier. `EQUIPMENT_BOLT_LAUNCHER` for instance.

    - `nameToken` - Just like before, this corresponds to our Language Token. `EQUIPMENT_BOLT_LAUNCHER_NAME`.
    - `pickupToken` - Just like before, this corresponds to our language token. `EQUIPMENT_BOLT_LAUNCHER_PICKUP`.
    - `descriptionToken` - Just like before, this corresponds to our language token. `EQUIPMENT_BOLT_LAUNCHER_DESCRIPTION`.
    - `loreToken` - Just like before, this corresponds to our language token. `EQUIPMENT_BOLT_LAUNCHER_LORE`.
    - `pickupModelPath` - Just like before, this corresponds to the path to our equipment model.
    - `pickupIconPath` - Just like before, this corresponds to the path to our equipment icon.
    - `appearsInSinglePlayer` - A boolean that determines whether or not you want this equipment to appear in Singleplayer. **This doesn't have to be changed from the default value we'll assign it, it's optional.**
    - `appearsInMultiplayer` - A boolean that determines whether or not you want this equipment to appear in Multiplayer. **This doesn't have to be changed from the default value we'll assign it, it's optional.**
    - `canDrop` - A boolean that determines whether or not we can remove this equipment from our inventory. **This doesn't have to be changed from the default value we'll assign it, it's optional.**
    - `cooldown` - A float that determines how long we want our equipment's cooldown duration to be after use. **This doesn't have to be changed from the default value we'll assign it, it's optional.**
    - `enigmaCompatible` - A boolean that determines whether or not our equipment can be randomly selected by the Artifact of Enigma. **This doesn't have to be changed from the default value we'll assign it, it's optional.**
    - `isBoss` - A boolean that determines if this equipment drops from bosses. **This doesn't have to be changed from the default value we'll assign it, it's optional.**
    - `isLunar` - A boolean that determines if this is a normal equipment that can be found in equipment barrels, or can be found in lunar pods. **This doesn't have to be changed from the default value we'll assign it, it's optional.**
    
6. You'll notice that in the above we have a ton of optional things, and we've already added properties for our required parts of the `EquipmentDef`. Just like in the previous section, we'll create `virtual` properties for these and default values so our inheritors don't have to implement them if they are not needed on an equipment. Like so:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Equipment
{
    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }
        
        public abstract string EquipmentModelPath { get; }
        public abstract string EquipmentIconPath { get; }
        
        public virtual bool AppearsInSinglePlayer { get; } = true;
        public virtual bool AppearsInMultiPlayer { get; } = true;
        public virtual bool CanDrop { get; } = true;
        public virtual float Cooldown { get; } = 60f;
        public virtual bool EnigmaCompatible { get; } = true;
        public virtual bool IsBoss { get; } = false;
        public virtual bool IsLunar { get; } = false;
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_NAME", EquipmentName);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP", EquipmentPickupDesc);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION", EquipmentFullDescription);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_LORE", EquipmentLore);
        }       
        
        public abstract ItemDisplayRuleDict CreateItemDisplayRules();        
        
        protected void CreateEquipment()
        {
        }        
        
        public abstract void Hooks();        
    }
}
```
7. Now let's create our `EquipmentDef` with our properties just like we did in `ItemBase`, the `itemDisplayRules`, and then register it all in one go. Like so:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Equipment
{
    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }
        
        public abstract string EquipmentModelPath { get; }
        public abstract string EquipmentIconPath { get; }
        
        public virtual bool AppearsInSinglePlayer { get; } = true;
        public virtual bool AppearsInMultiPlayer { get; } = true;
        public virtual bool CanDrop { get; } = true;
        public virtual float Cooldown { get; } = 60f;
        public virtual bool EnigmaCompatible { get; } = true;
        public virtual bool IsBoss { get; } = false;
        public virtual bool IsLunar { get; } = false;
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_NAME", EquipmentName);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP", EquipmentPickupDesc);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION", EquipmentFullDescription);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_LORE", EquipmentLore);
        }       
        
        public abstract ItemDisplayRuleDict CreateItemDisplayRules();        
        
        protected void CreateEquipment()
        {
            EquipmentDef equipmentDef = new RoR2.EquipmentDef()
            {
                name = "EQUIPMENT_" + EquipmentLangTokenName,
                nameToken = "EQUIPMENT_" + EquipmentLangTokenName + "_NAME",
                pickupToken = "EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP",
                descriptionToken = "EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION",
                loreToken = "EQUIPMENT_" + EquipmentLangTokenName + "_LORE",
                pickupModelPath = EquipmentModelPath,
                pickupIconPath = EquipmentIconPath,
                appearsInSinglePlayer = AppearsInSinglePlayer,
                appearsInMultiPlayer = AppearsInMultiPlayer,
                canDrop = CanDrop,
                cooldown = Cooldown,
                enigmaCompatible = EnigmaCompatible,
                isBoss = IsBoss,
                isLunar = IsLunar
            };        
            var itemDisplayRules = CreateItemDisplayRules();
            ItemAPI.Add(new CustomEquipment(equipmentDef, itemDisplayRules));            
        }        
        
        public abstract void Hooks();        
    }
}
```
8. Don't forget to create an `Index` field of type `EquipmentIndex`, and then assign it when registering the equipment like we did before. Like so:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Equipment
{
    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }
        
        public abstract string EquipmentModelPath { get; }
        public abstract string EquipmentIconPath { get; }
        
        public virtual bool AppearsInSinglePlayer { get; } = true;
        public virtual bool AppearsInMultiPlayer { get; } = true;
        public virtual bool CanDrop { get; } = true;
        public virtual float Cooldown { get; } = 60f;
        public virtual bool EnigmaCompatible { get; } = true;
        public virtual bool IsBoss { get; } = false;
        public virtual bool IsLunar { get; } = false;
        
        public EquipmentIndex Index;
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_NAME", EquipmentName);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP", EquipmentPickupDesc);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION", EquipmentFullDescription);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_LORE", EquipmentLore);
        }       
        
        public abstract ItemDisplayRuleDict CreateItemDisplayRules();        
        
        protected void CreateEquipment()
        {
            EquipmentDef equipmentDef = new RoR2.EquipmentDef()
            {
                name = "EQUIPMENT_" + EquipmentLangTokenName,
                nameToken = "EQUIPMENT_" + EquipmentLangTokenName + "_NAME",
                pickupToken = "EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP",
                descriptionToken = "EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION",
                loreToken = "EQUIPMENT_" + EquipmentLangTokenName + "_LORE",
                pickupModelPath = EquipmentModelPath,
                pickupIconPath = EquipmentIconPath,
                appearsInSinglePlayer = AppearsInSinglePlayer,
                appearsInMultiPlayer = AppearsInMultiPlayer,
                canDrop = CanDrop,
                cooldown = Cooldown,
                enigmaCompatible = EnigmaCompatible,
                isBoss = IsBoss,
                isLunar = IsLunar
            };        
            var itemDisplayRules = CreateItemDisplayRules();
            Index = ItemAPI.Add(new CustomEquipment(equipmentDef, itemDisplayRules));            
        }        
        
        public abstract void Hooks();        
    }
}
```
9. This next part is important. We're going to subscribe to an event for the first time. Think of the process as if we had a conveyor belt that leads to two (or more) paths. On one path, we allow the items on the belt to pass as they are. On the others, we modify the items to do specific things if they match certain conditions. At the end (or sometimes in the beginning) we return these modified items back into the belt same as the others. Following me? The point of this in context to this section of the tutorial is that we're going to subscribe to an event and we're going to call a method if the data in that event matches our equipment's `Index`. We'll be using the event `On.RoR2.EquipmentSlot.PerformEquipmentAction`. Like so:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Equipment
{
    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }
        
        public abstract string EquipmentModelPath { get; }
        public abstract string EquipmentIconPath { get; }
        
        public virtual bool AppearsInSinglePlayer { get; } = true;
        public virtual bool AppearsInMultiPlayer { get; } = true;
        public virtual bool CanDrop { get; } = true;
        public virtual float Cooldown { get; } = 60f;
        public virtual bool EnigmaCompatible { get; } = true;
        public virtual bool IsBoss { get; } = false;
        public virtual bool IsLunar { get; } = false;
        
        public EquipmentIndex Index;
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_NAME", EquipmentName);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP", EquipmentPickupDesc);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION", EquipmentFullDescription);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_LORE", EquipmentLore);
        }       
        
        public abstract ItemDisplayRuleDict CreateItemDisplayRules();        
        
        protected void CreateEquipment()
        {
            EquipmentDef equipmentDef = new RoR2.EquipmentDef()
            {
                name = "EQUIPMENT_" + EquipmentLangTokenName,
                nameToken = "EQUIPMENT_" + EquipmentLangTokenName + "_NAME",
                pickupToken = "EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP",
                descriptionToken = "EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION",
                loreToken = "EQUIPMENT_" + EquipmentLangTokenName + "_LORE",
                pickupModelPath = EquipmentModelPath,
                pickupIconPath = EquipmentIconPath,
                appearsInSinglePlayer = AppearsInSinglePlayer,
                appearsInMultiPlayer = AppearsInMultiPlayer,
                canDrop = CanDrop,
                cooldown = Cooldown,
                enigmaCompatible = EnigmaCompatible,
                isBoss = IsBoss,
                isLunar = IsLunar
            };        
            var itemDisplayRules = CreateItemDisplayRules();
            Index = ItemAPI.Add(new CustomEquipment(equipmentDef, itemDisplayRules));
            On.RoR2.EquipmentSlot.PerformEquipmentAction +=
        }        
        
        public abstract void Hooks();        
    }
}
```
10. Next to the `+=` we're going to input the name we'd like to call our method/delegate. In this case, we'll just use the name `PerformEquipmentAction`. Fill that out with that name, then `Right Click the Name -> Quick Actions -> Generate Method EquipmentBase.PerformEquipmentAction`. Make sure you select `Generate Method EquipmentBase.PerformEquipmentAction` and not `Generate Abstract Method EquipmentBase.PerformEquipmentAction`. Your code should look like this now:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Equipment
{
    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }
        
        public abstract string EquipmentModelPath { get; }
        public abstract string EquipmentIconPath { get; }
        
        public virtual bool AppearsInSinglePlayer { get; } = true;
        public virtual bool AppearsInMultiPlayer { get; } = true;
        public virtual bool CanDrop { get; } = true;
        public virtual float Cooldown { get; } = 60f;
        public virtual bool EnigmaCompatible { get; } = true;
        public virtual bool IsBoss { get; } = false;
        public virtual bool IsLunar { get; } = false;
        
        public EquipmentIndex Index;
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_NAME", EquipmentName);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP", EquipmentPickupDesc);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION", EquipmentFullDescription);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_LORE", EquipmentLore);
        }       
        
        public abstract ItemDisplayRuleDict CreateItemDisplayRules();        
        
        protected void CreateEquipment()
        {
            EquipmentDef equipmentDef = new RoR2.EquipmentDef()
            {
                name = "EQUIPMENT_" + EquipmentLangTokenName,
                nameToken = "EQUIPMENT_" + EquipmentLangTokenName + "_NAME",
                pickupToken = "EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP",
                descriptionToken = "EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION",
                loreToken = "EQUIPMENT_" + EquipmentLangTokenName + "_LORE",
                pickupModelPath = EquipmentModelPath,
                pickupIconPath = EquipmentIconPath,
                appearsInSinglePlayer = AppearsInSinglePlayer,
                appearsInMultiPlayer = AppearsInMultiPlayer,
                canDrop = CanDrop,
                cooldown = Cooldown,
                enigmaCompatible = EnigmaCompatible,
                isBoss = IsBoss,
                isLunar = IsLunar
            };        
            var itemDisplayRules = CreateItemDisplayRules();
            Index = ItemAPI.Add(new CustomEquipment(equipmentDef, itemDisplayRules));
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
        }        
        
        private bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentIndex equipmentIndex)
        {
        }        
        
        public abstract void Hooks();        
    }
}
```
11. The general rule with these kinds of methods is that if we're not planning to disrupt the original input of the method, we return the original input. What we want to do in this method is check if the input data of the method `equipmentIndex` is our equipment's `Index`. If it is, we will call a method we'll create in a second to allow us an easier time creating equipment use actions. If it isn't, we'll return the original input by passing it back into the parameter `orig`. Let's set up the logic for that:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Equipment
{
    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }
        
        public abstract string EquipmentModelPath { get; }
        public abstract string EquipmentIconPath { get; }
        
        public virtual bool AppearsInSinglePlayer { get; } = true;
        public virtual bool AppearsInMultiPlayer { get; } = true;
        public virtual bool CanDrop { get; } = true;
        public virtual float Cooldown { get; } = 60f;
        public virtual bool EnigmaCompatible { get; } = true;
        public virtual bool IsBoss { get; } = false;
        public virtual bool IsLunar { get; } = false;
        
        public EquipmentIndex Index;
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_NAME", EquipmentName);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP", EquipmentPickupDesc);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION", EquipmentFullDescription);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_LORE", EquipmentLore);
        }       
        
        public abstract ItemDisplayRuleDict CreateItemDisplayRules();        
        
        protected void CreateEquipment()
        {
            EquipmentDef equipmentDef = new RoR2.EquipmentDef()
            {
                name = "EQUIPMENT_" + EquipmentLangTokenName,
                nameToken = "EQUIPMENT_" + EquipmentLangTokenName + "_NAME",
                pickupToken = "EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP",
                descriptionToken = "EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION",
                loreToken = "EQUIPMENT_" + EquipmentLangTokenName + "_LORE",
                pickupModelPath = EquipmentModelPath,
                pickupIconPath = EquipmentIconPath,
                appearsInSinglePlayer = AppearsInSinglePlayer,
                appearsInMultiPlayer = AppearsInMultiPlayer,
                canDrop = CanDrop,
                cooldown = Cooldown,
                enigmaCompatible = EnigmaCompatible,
                isBoss = IsBoss,
                isLunar = IsLunar
            };        
            var itemDisplayRules = CreateItemDisplayRules();
            Index = ItemAPI.Add(new CustomEquipment(equipmentDef, itemDisplayRules));
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
        }        
        
        private bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentIndex equipmentIndex)
        {
            if(equipmentIndex == Index)
            {
            }
            else
            {
                return orig(self, equipmentIndex);
            }
        }        
        
        public abstract void Hooks();        
    }
}
```
12. In our condition `equipmentIndex == Index` we're going to be returning the result of a method that will check if we've activated our equipment and whatever behaviors we've defined on it. We'll be doing something similar to a proxy method of type `bool` here, by having our method have an `EquipmentSlot` parameter. We'll call it `ActivateEquipment`. It's easier done than said, so let's start by creating the method:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Equipment
{
    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }
        
        public abstract string EquipmentModelPath { get; }
        public abstract string EquipmentIconPath { get; }
        
        public virtual bool AppearsInSinglePlayer { get; } = true;
        public virtual bool AppearsInMultiPlayer { get; } = true;
        public virtual bool CanDrop { get; } = true;
        public virtual float Cooldown { get; } = 60f;
        public virtual bool EnigmaCompatible { get; } = true;
        public virtual bool IsBoss { get; } = false;
        public virtual bool IsLunar { get; } = false;
        
        public EquipmentIndex Index;
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_NAME", EquipmentName);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP", EquipmentPickupDesc);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION", EquipmentFullDescription);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_LORE", EquipmentLore);
        }       
        
        public abstract ItemDisplayRuleDict CreateItemDisplayRules();        
        
        protected void CreateEquipment()
        {
            EquipmentDef equipmentDef = new RoR2.EquipmentDef()
            {
                name = "EQUIPMENT_" + EquipmentLangTokenName,
                nameToken = "EQUIPMENT_" + EquipmentLangTokenName + "_NAME",
                pickupToken = "EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP",
                descriptionToken = "EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION",
                loreToken = "EQUIPMENT_" + EquipmentLangTokenName + "_LORE",
                pickupModelPath = EquipmentModelPath,
                pickupIconPath = EquipmentIconPath,
                appearsInSinglePlayer = AppearsInSinglePlayer,
                appearsInMultiPlayer = AppearsInMultiPlayer,
                canDrop = CanDrop,
                cooldown = Cooldown,
                enigmaCompatible = EnigmaCompatible,
                isBoss = IsBoss,
                isLunar = IsLunar
            };        
            var itemDisplayRules = CreateItemDisplayRules();
            Index = ItemAPI.Add(new CustomEquipment(equipmentDef, itemDisplayRules));
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
        }        
        
        private bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentIndex equipmentIndex)
        {
            if(equipmentIndex == Index)
            {
            }
            else
            {
                return orig(self, equipmentIndex);
            }
        }        
        
        protected abstract bool ActivateEquipment(EquipmentSlot slot);
        
        public abstract void Hooks();        
    }
}
```
13. All that is left to do now is return our method inside of our condition we defined just a few moments back, and pass in the `self` parameter to it which is the `EquipmentSlot` we need. Like so:
```csharp
using BepInEx.Configuration;
using ROR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModsNameSpace.Equipment
{
    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }
        
        public abstract string EquipmentModelPath { get; }
        public abstract string EquipmentIconPath { get; }
        
        public virtual bool AppearsInSinglePlayer { get; } = true;
        public virtual bool AppearsInMultiPlayer { get; } = true;
        public virtual bool CanDrop { get; } = true;
        public virtual float Cooldown { get; } = 60f;
        public virtual bool EnigmaCompatible { get; } = true;
        public virtual bool IsBoss { get; } = false;
        public virtual bool IsLunar { get; } = false;
        
        public EquipmentIndex Index;
        
        public abstract void Init(ConfigFile config);
        
        protected void CreateLang()
        {
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_NAME", EquipmentName);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP", EquipmentPickupDesc);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION", EquipmentFullDescription);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_LORE", EquipmentLore);
        }       
        
        public abstract ItemDisplayRuleDict CreateItemDisplayRules();        
        
        protected void CreateEquipment()
        {
            EquipmentDef equipmentDef = new RoR2.EquipmentDef()
            {
                name = "EQUIPMENT_" + EquipmentLangTokenName,
                nameToken = "EQUIPMENT_" + EquipmentLangTokenName + "_NAME",
                pickupToken = "EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP",
                descriptionToken = "EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION",
                loreToken = "EQUIPMENT_" + EquipmentLangTokenName + "_LORE",
                pickupModelPath = EquipmentModelPath,
                pickupIconPath = EquipmentIconPath,
                appearsInSinglePlayer = AppearsInSinglePlayer,
                appearsInMultiPlayer = AppearsInMultiPlayer,
                canDrop = CanDrop,
                cooldown = Cooldown,
                enigmaCompatible = EnigmaCompatible,
                isBoss = IsBoss,
                isLunar = IsLunar
            };        
            var itemDisplayRules = CreateItemDisplayRules();
            Index = ItemAPI.Add(new CustomEquipment(equipmentDef, itemDisplayRules));
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
        }        
        
        private bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentIndex equipmentIndex)
        {
            if(equipmentIndex == Index)
            {
                return ActivateEquipment(self);
            }
            else
            {
                return orig(self, equipmentIndex);
            }
        }        
        
        protected abstract bool ActivateEquipment(EquipmentSlot slot);
        
        public abstract void Hooks();        
    }
}
```
With that, we now have our `ItemBase` and `EquipmentBase` classes all set up! It's time to get down to creating our first item and equipment!