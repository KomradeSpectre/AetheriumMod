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

3. Fill the Project Name field with your mod name, Template Type is "3D".
4. Hit Create.
5. On the bottom left of the window, you will see the file explorer for Unity. Delete the Scenes folder.
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

## Creating your first Prefab
1. In the above structure, meshes are your raw models that you made in your 3D program of choice. Prefabs are the GameObject/Model we'll be making out of them. Go ahead and make a model or find one and place it in the Meshes folder.

2. Drag and drop that model to the scene hierarchy at the top left of the Unity window.
3. You'll notice it is in the scene, but it might be untextured (if you did not bake textures into the model). If this is the case, proceed. If it is not the case, click the model in the scene hierarchy at the top left and drag it to your Models -> Prefabs -> Item -> First Item  folder and skip to Step 16.
4. Click into Materials -> Item -> First Item.
5. Right Click anywhere in this folder and Create -> Material. 
6. A new material will be created and ask you for a name. The general practice for naming materials is to name them after what you're applying it to. So, if you're making a material for a handle of a mesh then ItemNameHandle.
7. Click the material, and at the top right you should see all kinds of properties. For now, just modify the color, smoothness, and emission to your liking.
8. Once done, to apply it you simply drag and drop it either on the part of the model, or the part in the scene hierarchy you want to apply it to.
9. Once done setting up materials and applying them, drag the Prefab (in the scene hierarchy it has a box next to it) to your Models -> Prefabs -> Item -> First Item folder.
10. When asked if you want to make an Original Prefab or a Variant Prefab, choose Original. To get the base for an icon, proceed with the following section, otherwise skip over it.

## Icon Creation
1. Assuming the model is still in the scene from the previous steps, continue. Otherwise drag the prefab into the scene hierarchy, and continue.

2. In the scene itself, turn off the Skybox.
3. In the scene itself, on the top right of it go to Gizmos -> Uncheck Show Grid.
4. Right click the compass, and pick a facing you'd like to have your icon be. Alternatively, just rotate the prefab via the values at the top right till you get the alignment you like.
5. Using a screen capture tool (like Lightshot, Greenshot, Printscr, etc), capture the area encompassing the model. Copy it to clipboard.
6. Open your image editor of choice, though for this tutorial I'll cover how to utilize the Hopoo Outline Template. That said, open Photoshop.
7. Create a new image, hit OK.
8. Paste in your image that you captured previously.
9. Select the Magic Wand tool, lower the tolerance down to around 0.
10. Select the gray areas that were from the background in Unity.
11. Delete the selection.
12. Invert the selection, you should now have a selection around your item icon.
13. At the top, Image -> Crop. The image should now only contain the area directly containing the icon.
14. Try to get the image size as close as you can to 460x460. It doesn't have to be exact here, just one of the dimensions should be close to this.
15. Increase Canvas Size to 512x512. There should be ample space around your icon right now.
16. Filters -> Blur -> Blur More.
17. Download the Hopoo Outline Templates: https://cdn.discordapp.com/attachments/567827235013132291/769053836077432872/RoR2_Public_Templates.rar
18. After extracting it, open the Item Icon Template.psd.
19. Expand the Layer Groups on the bottom right and delete any of the icons they had in there previously. Hide any other layers/sublayers aside from the one you want to place your item in.
20. In your icon image, select all and copy.
21. In the Item Icon Template, click the appropriate layer group for the Tier of your item.
22. CRTL+V to paste it in.
23. If everything looks alright, select all and CRTL+SHIFT+C.
24. Create a new image, hit OK. Paste the image in.
25. Image -> Resize Image, and set dimensions to 128x128.
26. Quick Export as PNG again, or save as PNG.
27. Drag the resultant file to your Unity Instance's Textures -> Icons -> Item folder.
28. Click it, and on the top right you'll see Alpha is Transparency. Click it.
29. Increase Anisotropy Level to max, or just leave it alone.
30. At the bottom set compression to none.
31. Hit Apply.
32. At the top of that same area, where it says Texture Type, click it and set it to Sprite (2D and UI).
33. Hit Apply, and you're done with the icon. Congrats!
---------

Preparing the Base Class
--------
1. Name your base class anything like "Main.cs" or "MyModName.cs" to keep track of it. 

2. Fill it out with the following:
```csharp
using BepInEx;

namespace MyUserName
{
    [BepInDependency("com.bepis.r2api")]
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
namespace IfMyModNameRemainsThisItMeansIDidn'tRead
{
```

4. Change the relevant fields in the BepinPlugin attribute to suit your mod. For example:
```csharp
[BepinPlugin("com.MyUserName.IfMyModNameRemainsThisItMeansIDidn'tRead", "IfMyModNameRemainsThisItMeansIDidn'tRead", "0.0.1")]
```  

5. Change the class name to match the class name file's name. For example, if you named your class Main.cs, then:
```csharp
public class Main : BaseUnityPlugin
{
```

6. Create container lists for initializing your items and equipment easily (Don't worry too much about the types listed here yet, we'll cover them later). For example: 
```csharp
    public class Main : BaseUnityPlugin
    {
        public List<ItemBase> Items = new List<ItemBase>();
        public List<EquipmentBase> Equipments = new List<EquipmentBase>();

        public void Awake()
        {
        }
    }
```

-----------