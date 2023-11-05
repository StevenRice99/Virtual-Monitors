# Overview

This repository aims to display monitors in an XR headset with Unity. While in the editor, if you do not have a headset yourself, you can go into "Edit > Project Settings... > XR Plug-In Management > XR Interaction Toolkit" and enable "Use XR Device Simulator in scenes".

**This only works on Windows systems.**

To use an Oculus device:
1. Install Oculus Rift: https://www.oculus.com/rift/setup
2. Inside yourself, go to "Link" and connect to your computer.
3. Run the application either in the editor or standalone and it will appear in your Oculus device.

# Work to be done

- It seems at some points in the Meta Quest Pro the monitors look blurry. This could potentially be a Quest Pro only issue (will need to test other headsets) or it may require some looking into the shaders that run the virtual monitors and making some changes to them.
- Add the ability to tilt outer monitors towards the center as most real-world multi-monitor setups have.
- The ability to adjust the overall height, distance, scale, (and tilt once added) the monitors at runtime via the XR controllers, since right now this can only be done in the editor.
- Passthrough so the keyboard and mouse can be seen.
  - The best idea I can think of for this would be you know the position of the hands already in Unity (they are disabled right now since not using them, but under "XR Origin > Camera Offset > Left Controller / Right Controller"), and the user can easily place those close to the keyboard and mouse when they type. They can act as guides for where the passthrough effect should be done.
  - OpenXR should be able to handle this: https://youtu.be/9u3QQi6Gnx0
- Add in background/environment options.
- As of now, this has only been tested with a Meta Quest Pro. Although I have added the settings for other Quest models as well as the Vive, they have not yet been tested and some changes may need to be made.
- This may not be easily doable, but add the ability to create/remove  monitors and position them in Unity.
  - With the current tools, this isn't possible as the libraries to this stuff are read only, and there is not really any C# (or even native Windows C++) libraries to do this.

# Virtual Monitor Drivers

The Unity application will automatically pick up all the monitors attached to your system, but one aim of this is the ability to have more monitors than your system physically has. Windows created some C++ APIs to do this in a tricky way. Luckily, work has already been done to create a driver that does this and some batch files have been made to simplify the process of running them: https://youtu.be/ybHKFZjSkVY

## Virtual Monitor Driver Instructions

The following batch files are inside the "Drivers" folder.

1. Install the driver to create virtual monitors. Right click "Install.bat" and click "Run as administrator".

2. Activate virtual monitors. This can be done up to four times to create four virtual monitors. Right click "Add.bat" and click "Run as administrator". If you want to remove a monitor: Right click "Remove.bat" and click "Run as administrator".

3. Arrange your monitors and virtual monitors. The easiest way is to right click your desktop and click "Display settings". Make sure you set the resolutions you want, arrange them, and apply changes. The monitors will appear in Unity as you arrange them here.

4. Once done, you can remove all virtual monitors and the drivers.
Right click "Uninstall.bat" and click "Run as administrator". **It is recommended you don't keep them around as you can easily "lose" your mouse on the virtual monitors.**