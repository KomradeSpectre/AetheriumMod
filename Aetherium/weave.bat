REM original version https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/C%23-Programming/Networking/UNet/
REM open this in vs it'll be so much nicer

REM call with ./weave.bat $(TargetDir)
set Target=Aetherium
set Output=%1
set Libs=Weaver\libs
set Log=%Output%OUTPUT.log

REM copy unpatched dll to weaver folder in case its needed
robocopy    %Output%   .\Weaver     %Target%.dll     %Target%.pdb    /log:%Log%
ren .\Weaver\%Target%.dll   %Target%.dll.prepatch
ren .\Weaver\%Target%.pdb   %Target%.pdb.prepatch

REM le epic networking patch
.\Weaver\Unity.UNetWeaver.exe   %Libs%\UnityEngine.CoreModule.dll   %Libs%\com.unity.multiplayer-hlapi.Runtime.dll  %Output%    %Output%%Target%.dll   %Libs%

REM move prepatch back to output
robocopy    .\Weaver    %Output%    %Target%.dll.prepatch    %Target%.pdb.prepatch   /log:%Log%
del Weaver\%Target%.dll.prepatch
del Weaver\%Target%.pdb.prepatch