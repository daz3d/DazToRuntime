# Daz To Unreal - Daz Studio Plugin
This is the Daz Studio side plugin for the Daz To Unreal tool.
## Building the Plugin for Windows
To buid this plugin you need the Daz Studio SDK https://www.daz3d.com/daz-studio-4-5-sdk
Some settings need changed in the DazToUnreal solution in Visual Studio.
1) In the properties for the DazToUnreal project check that the following values match your Daz Studio installation<br>
	a) General->Ouput Directory should be the plugins folder for Daz Studio<br>
	b) Debugging->Command should be the path to the Daz Studio executable.<br>
	c) C/C++->Additional Include Directories the path F:\Daz3D\Content\DAZStudio4.5+ SDK\include should be changed to match your install of the Daz Studio SDK<br>
	d) Linker->General the path F:\Daz3D\Content\DAZStudio4.5+ SDK\lib\x64 should be changed to match your install of the Daz Studio SDK<br>
2) In the properties for some of the header files (daztounrealdialog.h, morphselectiondialog.h, subdivisiondialog.h), the following need to be changed<br>
	a) Under Custom Build Tool->General F:\Daz3D\Content\DAZStudio4.5+ SDK\bin needs to be changed to match your install of the Daz Studio SDK