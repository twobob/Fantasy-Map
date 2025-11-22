# Godot Edition of Fantasy Map Generator

An attempt to port Azgaar's _Fantasy Map Generator_ to C#.

# Development Environment

* Windows 10 x64
* godot-3.2.2-stable mono edition
* Visual Studio 2017 with C#

# Build & Run

* Open the `godot/Fantasy Map.sln` project in Visual Studio and restore NuGet packages (Json.Net, SkiaSharp, etc.).
  Visual Studio may warn about a missing `GodotSharp.dll`; open the project in Godot to prepare the engine library.
* Run the project from Godot after the dependencies are restored.
