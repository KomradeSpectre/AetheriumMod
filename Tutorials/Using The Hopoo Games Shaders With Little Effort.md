# Using The Hopoo Games Shaders With Little Effort


## Prerequisites
- Knowledge of asset bundle exporting
- Existing Visual Studio and Unity projects.

## Why would I want to do this?
Utilizing this trick allows you to set the values of Hopoo Games shaders from your Unity Project, reducing the amount of code required to utilize the shaders. It will also perform the swap automatically for any shader you set up in the following steps.

### How to perform the trick

 Download the following package: https://github.com/KomradeSpectre/AetheriumMod/raw/rewrite-master/Tutorials/Resources/stubbed-shaders.zip
 
 Then follow the steps outlined in the following images:
 
 ----
 
1. Extract the archive, and locate this folder.
![](https://i.imgur.com/8OZQqvm.png)

----
2. Drag and drop the shader folder into your Unity Project's Asset folder
![](https://i.imgur.com/GPh2tF2.png)

----
3. Create a new material, edit it until it's to your liking.
![](https://i.imgur.com/uDn0cGk.png)

----
4. Change the shader on the material to whatever shader you want to swap at runtime.
![](https://i.imgur.com/ycnirpw.png)

----
5. Ensure the material contains the properties you'd like to have set on the material. E.g. Check the normal map property, and the emission map property.
![](https://i.imgur.com/EV0g07W.png)

----
6. Make sure to set the asset bundle of the material, and then export your bundle.
![](https://i.imgur.com/2HjpZWY.png)

----
7. Create a shader lookup dictionary. It's a dictionary of type <string, string> where the key is the name of the stubbed shader in lowercase, and the value is the path to the shader you want to swap it to. Only create key/value pairs of shaders you want to perform this swap on.
![](https://i.imgur.com/32clja3.png)

----
8. In your main plugin's awake, create the following code blob. We'll be using the asset bundle you should have already set up in your code by this point. We load in all the materials from it and save them to a variable. Then we use a foreach loop to check every material in our variable. In my example, if a shader doesn't start with "Fake RoR" I know it's not one I'm trying to perform the swap on, so we skip this iteration of the foreach loop. In yours, it will likely be "Stubbed".
![](https://i.imgur.com/qxYrp0J.png)

----
9. Any material that did pass the previous check potentially contain the shader we want to perform this swap on. So we'll feed the name of that material's shader into our Dictionary we created. If the key exists in the dictionary, we'll get the value of it which will be the path to the in game shader we want to use. We then are feeding this path into the Resources.Load<Shader>(path);
![](https://i.imgur.com/hTkbS0i.png)

----
10. If we got a shader out of the last step, it means our swap had worked and we'll now set that shader to the material that we're currently on in this foreach loop. That's it. That's how this trick is done. To update the material, you just change the values around in Unity, that's it.
![](https://i.imgur.com/eGsfNXK.png)