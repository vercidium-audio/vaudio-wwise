## Vercidium Audio + Wwise Example

This repository requires the Vercidium Audio SDK v1.1.1 and Wwise SDK to run:
- Download the Vercidium Audio SDK from [vercidium.com/audio](https://vercidium.com/audio)
- Download the Wwise SDK from [audiokinetic.com/en/download](https://www.audiokinetic.com/en/download)

> Please note that neither SDK is free for commercial use. See [audiokinetic.com/pricing](https://www.audiokinetic.com/pricing)

## Wwise Setup

Once Wwise is installed, you must download the C# bindings for Wwise, which are located in the Unity package:
- Run the Wwise Launcher program
- Select `Unity` on the left sidebar
- Click `Download` in the top-right
- Click `Offline integration files`

![Wwise Launcher screen with the 'Download > Offline integration files' option selected](docs/offline_integration_files.png)

Ensure `Microsoft > Windows` is selected on the right, then scroll down and click `Download`

![Download screen for the Unity offline integration files, with the Microsoft > Windows checkbox selected](docs/wwise_unity_download.png)

Then extract the `Unity.Windows.tar.xz` file (twice), then open the `Wwise/API/Runtime/Plugins/Windows/x86_64/Debug` folder and copy the `AkUnitySoundEngine.dll` file to the `vaudio-fmod/lib` folder.

![Folder containing the AkUnitySoundEngine.dll file](docs/akunitysoundenginedll.png)

## Vercidium Audio Setup

You can either copy the entire vaudio folder to the `lib` folder:

![screenshot of the vaudio-wwise/lib folder. It contains a folder called vaudio](docs/lib_folder.png)

Or edit `vaudio-wwise.csproj` to point to the folder where the Vercidium Audio SDK lives:

```xml
<PropertyGroup>
    <!-- Replace this with the path to your vaudio SDK -->
    <VAudioDir>lib\vaudio</VAudioDir>
</PropertyGroup>
```

## File Overview

- `wwise/AkSoundEngine.cs` contains the C# bindings for Wwise
- `WwiseSystem.cs` and `WwiseSound.cs` are helper files for working with Wwise 
- `resource/audio` contains soundbank and config files created by the Wwise Authoring program
- `Scene.cs` creates a Vercidium Audio context and initialises Wwise

Scene.cs is where you can adjust ray counts, add primitives, change materials and more. See the [Vercidium Audio docs](https://docs.vercidium.com/raytraced-audio/v110/Getting+Started) for more.

## Controls

Open the project in Visual Studio 2022 or 2026, and press F5 to run the project.

A debug window will appear that displays:
- the raytracing scene (primitives and rays)
- an echogram at the top
- raytracing stats in the bottom left.

Controls:

- Use WASD and the mouse to move the camera
- Press escape to release the mouse
- Press shift/control to increase camera speed

![Screenshot of the Vercidium Audio debug window, which shows primitives, rays, echograms and raytracing stats](docs/debug_window.png)