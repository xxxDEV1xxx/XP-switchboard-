# XP-switchboard-
XP switchboard .NET 2.-0


Compile with        
C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /target:exe /out:xpbox9.exe /r:C:\Windows\Microsoft.NET\Framework\v2.0.50727\System.dll /r:C:\Windows\Microsoft.NET\Framework\v2.0.50727\System.Drawing.dll /r:C:\Windows\Microsoft.NET\Framework\v2.0.50727\System.Windows.Forms.dll xpbox9.cs

download and install NET 2.0 on XP 
https://download.lenovo.com/ibmdl/pub/pc/pccbbs/thinkvantage_en/dotnetfx.exe

no color edit in program, have to edit individually before compile as desired.
https://www.youtube.com/watch?v=0zgq5aSUs00

in XP, create a batch file with the following (adjust for actual file to be run, notice spacing of " )
@echo off
start C:\"Program Files\Starcraft\Starcraft.exe"

then place in a directory that doesnt have any spaces in it or its sub directories, then place the .bat location in the switchboard. 
