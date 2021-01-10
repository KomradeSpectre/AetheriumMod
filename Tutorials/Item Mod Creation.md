### Table of Contents

1. [Prerequisites](#prerequisites)  
2. [Unity Project](#unity-project)  
    2.1. [Project Setup](#project-setup)  
    2.2. [Creating your first Prefab](#creating-your-first-prefab)  
    2.3. [Icon Creation](#icon-creation)  
    2.4. [Creating an Asset Bundle](#creating-an-asset-bundle)  
3. [Visual Studio Project](#visual-studio-project)  
    3.1. [Preparing the Base Class](#preparing-the-base-class)  
    3.2. [Abstraction and You (Creating the Item Base Class)](#creating-the-item-base-class)  
	3.3. [Creating the Equipment Base Class](#creating-the-equipment-base-class)  
    3.4. [Implementing Our First Item](#implementing-our-first-item)

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

## Creating the Item Base Class
If you're not too experienced with formal programming you're probably wondering, what *is* abstraction? In the context of this tutorial, we'll be creating abstract "ItemBase" and "EquipmentBase" classes. Imagine them to be a skeleton that all our items and all our equipment share. One that we add the muscle and skin to in our individual items and equipment. Let's get started.

1. In the solution explorer, right click on the CSProj file (the green rectangle with a C# in it) and `Add -> New Folder`. Name it `Items`.

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
## Creating the Equipment Base Class
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
			
			public static EquipmentIndex Index;
			
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
			
			public static EquipmentIndex Index;
			
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
			
			public static EquipmentIndex Index;
			
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
			
			public static EquipmentIndex Index;
			
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
			
			public static EquipmentIndex Index;
			
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
			
			public static EquipmentIndex Index;
			
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

------
## Implementing our First Item
------
Now that we've defined both of our abstract classes that will serve as the template for our items/equipment, we can begin to use them. In the solution explorer, right click our `Items` folder, `Add -> Class`, and name it `OmnipotentEgg` for example. Now let's get started.

1. In our newly generated class, add a `public` access modifier before the class name. It should look like:
	```csharp
	using System;
	using System.Collections.Generic;
	using System.Text;

	namespace MyModsNameSpace.Items
	{
		public class OmnipotentEgg
		{
		}
	}
	```

2. Now, let's inherit from our ItemBase. In this example, we'll do this by adding a colon after the class name, followed by the abstract class name.
	```csharp
	using System;
	using System.Collections.Generic;
	using System.Text;

	namespace MyModsNameSpace.Items
	{
		public class OmnipotentEgg : ItemBase
		{
		}
	}
	```
3. At this point you'll notice the console outputting a ton of errors about us not implementing the abstract class' fields/properties. `Right click the class name OmnipotentEgg -> Quick Actions -> Implement Abstract Class`.

4. Woah! Our class just filled up with a bunch of properties and fields! It also created usings at the top of our class! This is the result of our work in the last two sections. We can easily generate these just by inheriting the `ItemBase` and `EquipmentBase`. First order of business, let's put these in order since it likely jumbled these around when we implemented it in the last step. It should look like this:
	```csharp
	using BepInEx.Configuration;
	using R2API;
	using RoR2;
	using System;
	using System.Collections.Generic;
	using System.Text;

	namespace MyModsNameSpace.Items
	{
		public class OmnipotentEgg : ItemBase
		{
			public override string ItemName => throw new NotImplementedException();
			public override string ItemLangTokenName => throw new NotImplementedException();
			public override string ItemPickupDesc => throw new NotImplementedException();
			public override string ItemFullDescription => throw new NotImplementedException();
			public override string ItemLore => throw new NotImplementedException();
			
			public override ItemTier Tier => throw new NotImplementedException();        

			public override string ItemModelPath => throw new NotImplementedException();
			public override string ItemIconPath => throw new NotImplementedException();
			
			public override void Init(ConfigFile config)
			{
				throw new NotImplementedException();
			}        

			public override ItemDisplayRuleDict CreateItemDisplayRules()
			{
				throw new NotImplementedException();
			}

			public override void Hooks()
			{
				throw new NotImplementedException();
			}
		}
	}
	```
5. Now let us think of what we want this item to do. For this tutorial, we'll make the egg fire out a projectile when we're damaged, but we need to think more in depth about that. 
    - How much damage should our projectile deal?  
    - Does it scale with our damage?  
    - Does it have a set amount of damage it does per stack of the item?  

6. Once we've got these considerations and more all planned out, let's create some config entries for these. These will by default provide our own balance considerations for an item, but allow a user to configure it to their liking as well. After  all of our properties, we'll define the fields that will store the values from our config entries. We'll do one for the damage of the projectile, and one for additional damage per stack. Both of these will be of type `Float`. Like so:
	```csharp
	using BepInEx.Configuration;
	using R2API;
	using RoR2;
	using System;
	using System.Collections.Generic;
	using System.Text;

	namespace MyModsNameSpace.Items
	{
		public class OmnipotentEgg : ItemBase
		{
			public override string ItemName => throw new NotImplementedException();
			public override string ItemLangTokenName => throw new NotImplementedException();
			public override string ItemPickupDesc => throw new NotImplementedException();
			public override string ItemFullDescription => throw new NotImplementedException();
			public override string ItemLore => throw new NotImplementedException();
			
			public override ItemTier Tier => throw new NotImplementedException();        

			public override string ItemModelPath => throw new NotImplementedException();
			public override string ItemIconPath => throw new NotImplementedException();
			
			public float DamageOfMainProjectile;
			public float AdditionalDamageOfMainProjectilePerStack;
			
			public override void Init(ConfigFile config)
			{
				throw new NotImplementedException();
			}        

			public override ItemDisplayRuleDict CreateItemDisplayRules()
			{
				throw new NotImplementedException();
			}

			public override void Hooks()
			{
				throw new NotImplementedException();
			}
		}
	}
	```
7. With these in place, we'll want a method to not only keep these organized, but also allow us to control when we bind the values from our config file to the fields we just made. For this, create a method `CreateConfig` and have one parameter `ConfigFile config`, then in `Init` call that method with the parameter `config`. The `Init` method itself will be getting a `ConfigFile` input into it later when we begin to modify the main class to initialize our items. Back on topic, our code should now look like:
	```csharp
	using BepInEx.Configuration;
	using R2API;
	using RoR2;
	using System;
	using System.Collections.Generic;
	using System.Text;

	namespace MyModsNameSpace.Items
	{
		public class OmnipotentEgg : ItemBase
		{
			public override string ItemName => throw new NotImplementedException();
			public override string ItemLangTokenName => throw new NotImplementedException();
			public override string ItemPickupDesc => throw new NotImplementedException();
			public override string ItemFullDescription => throw new NotImplementedException();
			public override string ItemLore => throw new NotImplementedException();
			
			public override ItemTier Tier => throw new NotImplementedException();        

			public override string ItemModelPath => throw new NotImplementedException();
			public override string ItemIconPath => throw new NotImplementedException();
			
			public float DamageOfMainProjectile;
			public float AdditionalDamageOfMainProjectilePerStack;        
			
			public override void Init(ConfigFile config)
			{
				CreateConfig(config);
			}        

			public void CreateConfig(ConfigFile config)
			{
			
			}

			public override ItemDisplayRuleDict CreateItemDisplayRules()
			{
				throw new NotImplementedException();
			}

			public override void Hooks()
			{
				throw new NotImplementedException();
			}
		}
	}
	```
8. Next we'll be creating a `Config` binding using `config.Bind<Type>` and then grab its field `Value` in one go. `config.Bind<Type>` acts as both the writer and reader of a config file, and will update itself when new values are input into its generated config field, or if the default has changed. The overload we're going to use will consist of the following parts:
    - `section` - The category that this config option will appear under. I generally use either `"Item: " + ItemName` or `"Equipment:"  + EquipmentName` which will place all of our config options for this in the same category and allow jumping to that category when using mod managers like `R2ModMan`.

    - `key` - This is the name of this individual config option. It can be a short description too, like `"Damage of the Main Projectile"`.
    - `defaultValue` - This should be self-explanatory, but this is the value you want this option to have by default. For a config option of type `float` this could be `100.58f`.
    - `description` - This is the long description of what this config entry does. Generally, you ask a question in detail here. For example, `"What should the base damage of the main projectile be?"`.
    
9. Having all this in mind, let's create the config bindings for our item, and assign them to our fields we just created.
	```csharp
	using BepInEx.Configuration;
	using R2API;
	using RoR2;
	using System;
	using System.Collections.Generic;
	using System.Text;

	namespace MyModsNameSpace.Items
	{
		public class OmnipotentEgg : ItemBase
		{
			public override string ItemName => throw new NotImplementedException();
			public override string ItemLangTokenName => throw new NotImplementedException();
			public override string ItemPickupDesc => throw new NotImplementedException();
			public override string ItemFullDescription => throw new NotImplementedException();
			public override string ItemLore => throw new NotImplementedException();
			
			public override ItemTier Tier => throw new NotImplementedException();        

			public override string ItemModelPath => throw new NotImplementedException();
			public override string ItemIconPath => throw new NotImplementedException();
			
			public float DamageOfMainProjectile;
			public float AdditionalDamageOfMainProjectilePerStack;        
			
			public override void Init(ConfigFile config)
			{
				CreateConfig(config);
			}        

			public void CreateConfig(ConfigFile config)
			{
				DamageOfMainProjectile = config.Bind<float>("Item: " + ItemName, "Damage of the Main Projectile", 150f, "How much base damage should the projectile deal?").Value;
				AdditionalDamageOfMainProjectilePerStack = config.Bind<float>("Item: " + ItemName, "Additional Damage of Projectile per Stack", 100f, "How much more damage should the projectile deal per additional stack?").Value;
			}

			public override ItemDisplayRuleDict CreateItemDisplayRules()
			{
				throw new NotImplementedException();
			}

			public override void Hooks()
			{
				throw new NotImplementedException();
			}
		}
	}
	```
10. The main reason we're doing the config entries first is so that we can use them in our description. By doing so, we can automatically update the description string should we or the user change the values around. Now let's fill out the language string fields. We'll do these one at a time, and focus specifically on the fields in question due to a little bit of text shenaniganery we're about to do. First things first, let's set the name of our Item. Do this by replacing the `throw new NotImplementedException();` with a string containing the name of the item, in this example `"Omnipotent Egg"`.
	```csharp
			public override string ItemName => "Omnipotent Egg";
	```
11. Next up, fill out the `ItemLangTokenName` similar to how you did the one above.
	```csharp
			public override string ItemLangTokenName => "OMNIPOTENT_EGG";
	```
12. Next, `ItemPickupDesc` will be a short description of what the item does.
	```csharp
			public override string ItemPickupDesc => "Shoot a projectile out when you are damaged";
	```
13. `ItemFullDescription` which will be where we make our first use of the `TextMeshPro` rich text style tags. Before that however, visit either link below to familiarize yourself with these:
    - https://github.com/risk-of-thunder/R2Wiki/wiki/Style-Reference-Sheet
    - http://digitalnativestudios.com/textmeshpro/docs/rich-text/

14. With that out of the way, let's proceed to fill out the full description. This is where we will go in detail with what the item does. Ours will list the condition in which our item activates, how much damage it does, and how much additional damage per stack it does. For this, we will make use of two of the style tags in the game. `<style=cIsDamage></style>` and `<style=cStack></style>`. Stack information generally will be put into parenthesis after main info. Secondly, we'll be using the shorthand for String.Format before our description string `$` which will allow us to use variables in the string via {field/variable/etc} (like the config fields we did earlier). Let's fill out that description now like so:
	```csharp
			public override string ItemFullDescription => $"When you are damaged, shoot out a projectile for <style=cIsDamage>{DamageOfMainProjectile}</style> <style=cStack>(+{AdditionalDamageOfMainProjectilePerStack}).";
	```
15. Finally, lore is completely optional for an item, but people tend to like the extra polish that goes into creations. We'll just fill it out like so:
	```csharp
			public override string ItemLore => "Since the dawn of man, one egg has always stood above the rest. This egg.";
	```
16. Now we decide what tier we want this item to appear in. The effect is pretty good (identical to razorwire even), so green. That is to say, `ItemTier.Tier2`.
	```csharp
			public override ItemTier Tier => ItemTier.Tier2;
	```
17. Fill out the model and icon paths as we discussed before by using the provider and a string path to the asset (including the filetype).
	```csharp
			public override string ItemModelPath => "@MyModName:Assets/Models/Prefabs/Item/OmnipotentEgg/OmnipotentEgg.prefab";
			public override string ItemIconPath => "@MyModName:Assets/Textures/Icons/Item/OmnipotentEgg.png";
	```
18. Now in our `Init` method, simply add a `CreateLang();` under `CreateConfig(config);`. This will initialize our Language Token registry after we bind the config values to the fields, resulting in a description that changes if the user changes their values.

19. At this point your code should look similar to the following:
	```csharp
	using BepInEx.Configuration;
	using R2API;
	using RoR2;
	using System;
	using System.Collections.Generic;
	using System.Text;

	namespace MyModsNameSpace.Items
	{
		public class OmnipotentEgg : ItemBase
		{
			public override string ItemName => "Omnipotent Egg";
			
			public override string ItemLangTokenName => "OMNIPOTENT_EGG";
			
			public override string ItemPickupDesc => "Shoot a projectile out when you are damaged";
			
			public override string ItemFullDescription => $"When you are damaged, shoot out a projectile for <style=cIsDamage>{DamageOfMainProjectile}</style> <style=cStack>(+{AdditionalDamageOfMainProjectilePerStack}).";
			
			public override string ItemLore => "Since the dawn of man, one egg has always stood above the rest. This egg.";
			
			public override ItemTier Tier => ItemTier.Tier2;        

			public override string ItemModelPath => "@MyModName:Assets/Models/Prefabs/Item/OmnipotentEgg/OmnipotentEgg.prefab";
			public override string ItemIconPath => "@MyModName:Assets/Textures/Icons/Item/OmnipotentEgg.png";
			
			public float DamageOfMainProjectile;
			public float AdditionalDamageOfMainProjectilePerStack;        
			
			public override void Init(ConfigFile config)
			{
				CreateConfig(config);
				CreateLang();
			}        

			public void CreateConfig(ConfigFile config)
			{
				DamageOfMainProjectile = config.Bind<float>("Item: " + ItemName, "Damage of the Main Projectile", 150f, "How much base damage should the projectile deal?").Value;
				AdditionalDamageOfMainProjectilePerStack = config.Bind<float>("Item: " + ItemName, "Additional Damage of Projectile per Stack", 100f, "How much more damage should the projectile deal per additional stack?").Value;
			}

			public override ItemDisplayRuleDict CreateItemDisplayRules()
			{
				throw new NotImplementedException();
			}

			public override void Hooks()
			{
				throw new NotImplementedException();
			}
		}
	}
	```
20. We'll need to create a projectile for our item to fire, so let's create a method `CreateProjectile` with no return type. It will serve to keep our initilizations nice and tidy. Place this method underneath `CreateConfig`.
	```csharp
	using BepInEx.Configuration;
	using R2API;
	using RoR2;
	using System;
	using System.Collections.Generic;
	using System.Text;

	namespace MyModsNameSpace.Items
	{
		public class OmnipotentEgg : ItemBase
		{
			public override string ItemName => "Omnipotent Egg";
			
			public override string ItemLangTokenName => "OMNIPOTENT_EGG";
			
			public override string ItemPickupDesc => "Shoot a projectile out when you are damaged";
			
			public override string ItemFullDescription => $"When you are damaged, shoot out a projectile for <style=cIsDamage>{DamageOfMainProjectile}</style> <style=cStack>(+{AdditionalDamageOfMainProjectilePerStack}).";
			
			public override string ItemLore => "Since the dawn of man, one egg has always stood above the rest. This egg.";
			
			public override ItemTier Tier => ItemTier.Tier2;        

			public override string ItemModelPath => "@MyModName:Assets/Models/Prefabs/Item/OmnipotentEgg/OmnipotentEgg.prefab";
			public override string ItemIconPath => "@MyModName:Assets/Textures/Icons/Item/OmnipotentEgg.png";
			
			public float DamageOfMainProjectile;
			public float AdditionalDamageOfMainProjectilePerStack;        
			
			public override void Init(ConfigFile config)
			{
				CreateConfig(config);
				CreateLang();
			}        

			public void CreateConfig(ConfigFile config)
			{
				DamageOfMainProjectile = config.Bind<float>("Item: " + ItemName, "Damage of the Main Projectile", 150f, "How much base damage should the projectile deal?").Value;
				AdditionalDamageOfMainProjectilePerStack = config.Bind<float>("Item: " + ItemName, "Additional Damage of Projectile per Stack", 100f, "How much more damage should the projectile deal per additional stack?").Value;
			}
			
			public void CreateProjectile()
			{
				
			}

			public override ItemDisplayRuleDict CreateItemDisplayRules()
			{
				throw new NotImplementedException();
			}

			public override void Hooks()
			{
				throw new NotImplementedException();
			}
		}
	}
	```
21. When creating a projectile, there are a few paths you can go:
     - You can create your own projectile from scratch, which takes considerable effort and experience. 
 
     - You can use a base game projectile, such as Commando's `FMJ`.  
     - You can clone a base game projectile, and edit the properties of it safely. **This is what we're going to do.**
22. Below our fields and properties, put a new `public` `static` field of type  `GameObject`. Name it `EggProjectile`. Like so:
    ```csharp
    using BepInEx.Configuration;
    using R2API;
    using RoR2;
    using System;
    using System.Collections.Generic;
    using System.Text;
    
    namespace MyModsNameSpace.Items
    {
        public class OmnipotentEgg : ItemBase
        {
            public override string ItemName => "Omnipotent Egg";
            
            public override string ItemLangTokenName => "OMNIPOTENT_EGG";
            
            public override string ItemPickupDesc => "Shoot a projectile out when you are damaged";
            
            public override string ItemFullDescription => $"When you are damaged, shoot out a projectile for <style=cIsDamage>{DamageOfMainProjectile}</style> <style=cStack>(+{AdditionalDamageOfMainProjectilePerStack}).";
            
            public override string ItemLore => "Since the dawn of man, one egg has always stood above the rest. This egg.";
            
            public override ItemTier Tier => ItemTier.Tier2;        
    
            public override string ItemModelPath => "@MyModName:Assets/Models/Prefabs/Item/OmnipotentEgg/OmnipotentEgg.prefab";
            public override string ItemIconPath => "@MyModName:Assets/Textures/Icons/Item/OmnipotentEgg.png";
            
            public float DamageOfMainProjectile;
            public float AdditionalDamageOfMainProjectilePerStack;
            
            public static GameObject EggProjectile;
            
            public override void Init(ConfigFile config)
            {
                CreateConfig(config);
                CreateLang();
            }        
    
            public void CreateConfig(ConfigFile config)
            {
            	DamageOfMainProjectile = config.Bind<float>("Item: " + ItemName, "Damage of the Main Projectile", 150f, "How much base damage should the projectile deal?").Value;
            	AdditionalDamageOfMainProjectilePerStack = config.Bind<float>("Item: " + ItemName, "Additional Damage of Projectile per Stack", 100f, "How much more damage should the projectile deal per additional stack?").Value;
            }
            
            public void CreateProjectile()
            {
                
            }
    
            public override ItemDisplayRuleDict CreateItemDisplayRules()
            {
                throw new NotImplementedException();
            }
    
            public override void Hooks()
            {
                throw new NotImplementedException();
            }
        }
    }
    ```
23. In `CreateProjectile` we'll use PrefabAPI's `InstantiateClone` clone a base game projectile without modifying the original via referencing it with an extensionless path. It requires two things:

    - A `GameObject` - In our example, this will be the projectile's `Prefab` we want to clone.
    - A `name` - What we want to call our new projectile code wise.
    
    That `FMJ` referenced earlier will be the one we'll use for the `GameObject`, and to use it we'll need to call `Resources.Load<Type>()` with the path leading there to it. For the name, we'll simply use `"EggProjectile"`.
    
24. Done correctly, your `CreateProjectile` method should look like this now:
	```csharp
			public void CreateProjectile()
			{
				EggProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/FMJ");
			}
	```
25. Let's change the damage type of our projectile now so that it slows whatever enemy it hits. We'll do this by calling `GameObject.GetComponent<Type>` on our `EggProjectile` and the type will be `Projectile.ProjectileDamage`. Save this to a variable `damage`.
26. Next, assign the `damageType` field on `damage` to a new damage type. In our case, we'll do `DamageType.SlowOnHit`. Your `CreateProjectile` should look similar to this now:
    ```csharp
			public void CreateProjectile()
			{
				EggProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/FMJ");
				
				var damage = EggProjectile.GetComponent<Projectile.ProjectileDamage>();
				damage = DamageType.SlowOnHit;
			}    
    ```
    
27. Now that we've got the properties of our projectile set up, all we have to do now is register it and then add it to the projectile catalog. The first one is done with `PrefabAPI`'s `RegisterNetworkPrefab` which accepts a `GameObject`. The second one will be us adding to the list of projectiles in the Projectile Catalog. Let's do so now:
    ```csharp
			public void CreateProjectile()
			{
				EggProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/FMJ");
				
				var damage = EggProjectile.GetComponent<Projectile.ProjectileDamage>();
				damage = DamageType.SlowOnHit;
				
                if (EggProjectile) PrefabAPI.RegisterNetworkPrefab(EggProjectile);

                RoR2.ProjectileCatalog.getAdditionalEntries += list =>
                {
                    list.Add(EggProjectile);
                };
			}    
    ```
28. Back in the main class, add `PrefabAPI` into your `R2APISubmoduleDependency`attribute as we've done multiple times now when we use one of `R2API`'s submodules.
    ```csharp
    		[R2APISubmoduleDependency(nameof(ResourcesAPI), nameof(LanguageAPI), nameof(ItemAPI), nameof(PrefabAPI))]
    ```
29. Now for the hellish bit, `ItemDisplayRules`. You can skip this over completely and just return an empty `ItemDisplayRuleDict` in the method `CreateItemDisplayRules()`, or you can sit through this to have your items appear on characters. First things first, give your `CreateItemDisplayRules` method a body. Like so:
    ```csharp
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            
        }
    ```
30. Create another `GameObject` field under your properties at the start of the class with the `public` access modifier and `static` keyword. Name it something like `ItemBodyModelPrefab`. This field will be used to set up your `ItemDisplay`, as well as the `RendererInfos` for it. As an added bonus in our public class, this allows mod authors to easily add in your `ItemDisplay` to their models by referencing it.
31. We'll assign the field in our `CreateItemDisplayRules` method by using `Resource.Load<Type>` to load in our `Prefab`. For the path, we can use the one we set in our properties already, `ItemModelPath`. At this point your code should look similar to the following:
    ```csharp
    using BepInEx.Configuration;
    using R2API;
    using RoR2;
    using System;
    using System.Collections.Generic;
    using System.Text;
    
    namespace MyModsNameSpace.Items
    {
        public class OmnipotentEgg : ItemBase
        {
            public override string ItemName => "Omnipotent Egg";
            
            public override string ItemLangTokenName => "OMNIPOTENT_EGG";
            
            public override string ItemPickupDesc => "Shoot a projectile out when you are damaged";
            
            public override string ItemFullDescription => $"When you are damaged, shoot out a projectile for <style=cIsDamage>{DamageOfMainProjectile}</style> <style=cStack>(+{AdditionalDamageOfMainProjectilePerStack}).";
            
            public override string ItemLore => "Since the dawn of man, one egg has always stood above the rest. This egg.";
            
            public override ItemTier Tier => ItemTier.Tier2;        
    
            public override string ItemModelPath => "@MyModName:Assets/Models/Prefabs/Item/OmnipotentEgg/OmnipotentEgg.prefab";
            public override string ItemIconPath => "@MyModName:Assets/Textures/Icons/Item/OmnipotentEgg.png";
            
            public float DamageOfMainProjectile;
            public float AdditionalDamageOfMainProjectilePerStack;
            
            public static GameObject EggProjectile;
            public static GameObject ItemBodyModelPrefab;
            
            public override void Init(ConfigFile config)
            {
                CreateConfig(config);
                CreateLang();
            }        
    
            public void CreateConfig(ConfigFile config)
            {
            	DamageOfMainProjectile = config.Bind<float>("Item: " + ItemName, "Damage of the Main Projectile", 150f, "How much base damage should the projectile deal?").Value;
            	AdditionalDamageOfMainProjectilePerStack = config.Bind<float>("Item: " + ItemName, "Additional Damage of Projectile per Stack", 100f, "How much more damage should the projectile deal per additional stack?").Value;
            }
            
			public void CreateProjectile()
			{
				EggProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/FMJ");
				
				var damage = EggProjectile.GetComponent<Projectile.ProjectileDamage>();
				damage = DamageType.SlowOnHit;
				
                if (EggProjectile) PrefabAPI.RegisterNetworkPrefab(EggProjectile);

                RoR2.ProjectileCatalog.getAdditionalEntries += list =>
                {
                    list.Add(EggProjectile);
                };
			}    
    
            public override ItemDisplayRuleDict CreateItemDisplayRules()
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(ItemModelPath);
            }
    
            public override void Hooks()
            {
                throw new NotImplementedException();
            }
            
        }
    }
    ```
32. We'll need to add a component `ItemDisplay` to our ItemBodyModelPrefab so we can set up `RenderInfos` for it to work properly with the overlay system in the game. Since that field is a `GameObject` field, we can call `AddComponent<Type>` on it to add our `ItemDisplay` component. We'll also save the result in a variable.
    ```csharp
            public override ItemDisplayRuleDict CreateItemDisplayRules()
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(ItemModelPath);
                var itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            }
    ```
33. Our next move will require us to generate `RendererInfos` for the meshes on our `ItemBodyModelPrefab`. We'll be doing this per mesh but don't worry, it's an easy task I'll provide a helper method for free of charge. First things first, right click the CSProj file in your solution explorer (the green rectangle with a C# in it) and then `Add -> Folder`. Name it `Utils`.
34. Right click the folder, `Add -> Class`. Name it `ItemHelpers`.
35. Add the `public` access modifier to the front of the class.
36. Add usings for `RoR2` and `UnityEngine` at the top of the class.
37. Finally, add in the following snippet to the class:
    ```csharp
        public static CharacterModel.RendererInfo[] ItemDisplaySetup(GameObject obj)
        {
            MeshRenderer[] meshes = obj.GetComponentsInChildren<MeshRenderer>();
            CharacterModel.RendererInfo[] renderInfos = new CharacterModel.RendererInfo[meshes.Length];

            for (int i = 0; i < meshes.Length; i++)
            {
                renderInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = meshes[i].material,
                    renderer = meshes[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false //We allow the mesh to be affected by overlays like OnFire or PredatoryInstinctsCritOverlay.
                };
            }

            return renderInfos;
        }
    ```
    The quick rundown of what this helper does is that when we call it, we pass in a `GameObject obj`. From it, we grab all of the `MeshRenderer` inside of it and put them into a `MeshRenderer` array. We then create a `RendererInfo` array we'll iterate through of a length equal to how many `MeshRenderer` we have in the `MeshRenderer` array. We then iterate through the `MeshRenderer` array and create a new `RendererInfo` for each iteration based on the mesh and its material we currently have the index `i` of. After we're done, we're returning the `RendererInfo` array from the method, and it's set up from that point on.

38. Back in your new item's class, add a static using for `MyModsNameSpace.Utils.ItemHelpers`. To do so, just add a `static` keyword after the `using`.
39. Now in our `CreateItemDisplayRules` method, we'll use our newly created helper method by referencing the property on our `itemDisplay` variable and assigning it with our util method call. Like so:
    ```csharp
            public override ItemDisplayRuleDict CreateItemDisplayRules()
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(ItemModelPath);
                var itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
                itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);
            }
    ```

40. Now we will create the `ItemDisplayRuleDict` that will store our `ItemDisplayRule` arrays. We'll call it `rules`. It will need a new `ItemDisplayRule` array, like so:
    ```csharp
            public override ItemDisplayRuleDict CreateItemDisplayRules()
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(ItemModelPath);
                var itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
                itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

                ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
                {
                
                });
            }
    ```

41. As usual, we need to think about what an `ItemDisplayRule`'s major components are that we need. This however varies between the two rule types of `ItemDisplayRule`, which are `ItemDisplayRuleType.ParentedPrefab` and `ItemDisplayRuleType.LimbMask`. We will be using `ItemDisplayRuleType.ParentedPrefab` and it has the following components:
    - `ruleType` - As established above, this is either `ItemDisplayRuleType.LimbMask` or `ItemDisplayRuleType.ParentedPrefab`.
    
    - `followerPrefab` - The model we seek to place onto the characters. This doesn't have to be your pickup model as we are doing in this tutorial, it can be any prefab. It however needs to have an `ItemDisplay` component and `RendererInfos` set up for it.
    - `childName` - This is a `string` referencing the `ChildLocator` transform pair we wish to attach our `followerPrefab` to.
    - `localPos` - This is the positional offset of our item from the origin of the child we have parented our `followerPrefab` to.
    - `localAngles` - This is the euler angle offset of our item from the base rotation of the child we have parented our `followerPrefab` to. **(0-359 clamped, higher or lower values wrap around)**
    - `localScale` - This is the size offset of our item from the base scale of the child we have parented our `followerPrefab` to.

42. Once the above considerations have been taken into account, decide what the `childName` you're parenting your `followerPrefab` to. These points can be found in any character's `CharacterModel`, under the `ChildLocator` component. For the purpose of the tutorial, we'll just use `Chest` as it is a commonly shared `childName.` Create a zeroed out `ItemDisplayRule` inside of our empty `ItemDisplayRule` array. Like so:
    ```csharp
            public override ItemDisplayRuleDict CreateItemDisplayRules()
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(ItemModelPath);
                var itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
                itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

                ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
                {
                    new RoR2.ItemDisplayRule
                   {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "Chest",
                        localPos = new Vector3(0, 0, 0),
                        localAngles = new Vector3(0, 0, 0),
                        localScale = new Vector3(1, 1, 1)
                    }
                });
            }
    ```
    This is a default rule given that we provide no string leading to a CharacterModel, thus it means this will apply to all characters that have a `childName` `Chest` and don't have a `Chest` rule directly written for them. Zeroing it out like this will place it directly at the origin of the attach point with the rotation of the attach point.

43. Adjustments from this point are up to you to do, but there are tools to make this process loads easier. When using the one I will link one below, you can search for your Prefab's name which will show `PrefabName(Clone)`. Clicking on it will place a question mark above the model, and if it is your model you can open the `Transform` component on it. From there, you can adjust the `localPos`, `localAngles (sometimes localEuler)`, and `localScale` fields until you have it looking as you want it. Once you do, just simply copy the values of those fields to the matching fields in your `ItemDisplayRule` code to apply them properly. That said, the link to the tool is below:

    https://thunderstore.io/package/Twiner/RuntimeInspector/ - RuntimeInspector by Twiner
    
44. The only difference for our specific `CharacterModel` string rules is that we're adding them to our existing `ItemDisplayRuleDict rules` the same way we would add to a normal C# `Dictionary`. That is to say, `key, value`. The internals are functionally the same as what we've done in step 42. Like so:
    ```csharp
            public override ItemDisplayRuleDict CreateItemDisplayRules()
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(ItemModelPath);
                var itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
                itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

                ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
                {
                    new RoR2.ItemDisplayRule
                   {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "Chest",
                        localPos = new Vector3(0, 0, 0),
                        localAngles = new Vector3(0, 0, 0),
                        localScale = new Vector3(1, 1, 1)
                    }
                });
                rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
                {
                    new RoR2.ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "Chest",
                        localPos = new Vector3(0, 0, 0),
                        localAngles = new Vector3(0, 0, 0),
                        localScale = new Vector3(1, 1, 1)
                    }
                });            
            }
    ```
    Make sense? It's as simple as that for the specific rules. On a closing note here, you can add more than one `ItemDisplayRule` to the `ItemDisplayRule` array on each of these by simply adding a comma after the rule and making another `ItemDisplayRule` like the two above.
    
45. For the last part of the `CreateItemDisplayRules` method, we simply need to `return rules;`
