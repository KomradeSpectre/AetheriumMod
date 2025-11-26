This app will patch a Script Assembly by generating all the necessary stuff so that your networking stuff :
Commands Attribute, RPC stuff etc... will work.

Args when launching this weaver must be, in this order:
string unityEngine => Path to UnityEngine.CoreModule.dll
string unetDLL => Path to UnityEngine.Networking.dll
string outputDirectory => Folder Path where your assembly is
string assembly => Path to the assembly.dll that need to be patched
string extraAssemblyPath => Path to a folder where all the assemblies needed for compiling your assembly are

Typical usage :

Unity.UNetWeaver.exe "C:\Program Files\Unity\Editor\Data\Managed/UnityEngine/UnityEngine.CoreModule.dll" "C:/Program Files/Unity/Editor/Data/UnityExtensions/Unity/Networking/UnityEngine.Networking.dll" "Output" "NonPatched\Assembly-CSharp.dll" "C:\libsFolder"
