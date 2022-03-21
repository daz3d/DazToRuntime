# DazToRuntime Project

This project houses the official opensource projects for both the Unity and Unreal exporters. These show up in the `File/Send To` menu inside Daz Studio.

## Requirements

1. DAZStudio4.5+ SDK
2. QT 4.8.1
3. CMake
4. Working C++ environment

## Example of how to build using cmake

For building on windows here is an example of how you might configure/setup cmake to build.

```
cmake.exe -G "Visual Studio 14 2015 Win64" .. -DDAZ_SDK_DIR_DEFAULT:STRING="C:\Daz 3D\Applications\64-bit\DAZ 3D\DAZStudio4" -DDAZ_SDK_DIR:STRING="C:\Daz 3D\Applications\64-bit\DAZ 3D\DAZStudio4.5+ SDK" -DQT_INSTALL_PREFIX:STRING="C:\Qt\4.8.1"
cmake.exe --build . --target dzunitybridge --config RelWithDebInfo -- -nologo /maxcpucount:12 /verbosity:minimal
```

After it's built you can copy the `dzunitybridge.dll` or `dzunrealbridge.dll` into your `DAZStudio4/plugins` folder which might be located at: `C:\Daz 3D\Applications\64-bit\DAZ 3D\DAZStudio4\plugins`.
