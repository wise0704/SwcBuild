# SwcBuild

ActionScript® Component Compiler Tool for FlashDevelop® Projects

## Details

 * Supports ActionScript Compiler 2.
 * Supports AS Documentation.
 * All compiler options in the project file (`*.as3proj`) are used _(except for movie options such as `default-framerate`, `default-size`, etc.)_.
 * No extra steps needed to setup the project. Just click `Project > New Project > AS3 Library Project`.
 * You can build the project the same way you would build a `.swf` file. Just click `Project > Build Project` or press `F8`.
 * Detailed output is redirected to the Output Panel.
 * Creates `obj\$(ProjectID)Config.xml` almost exactly the same way as `FDBuild`. You can use this file for debugging purposes.
 * Supports AIR and AIR Mobile platforms. Just go to `Project Properties > Output` and choose from the Platform dropbox. No extra work is needed (no more manual `+configname=air`).
 * Supports conditional output path: in release mode, `"-debug"` in the path is removed[*](#notes). Also, `"{Build}"` in the path gets substituted with the current build configuration (`"Debug"` or `"Release"`).
 * Include sources easily by just adding them to the project classpath list.
 * Easily include library paths with `Right Click > Add To Library`.

## Installation

Open `swcbuild.fdz` (double-click, press Enter, right-click > open, `File > Open...` in FlashDevelop, or whatever you want).

## Additional Information

 * This installation file makes changes in `$(BaseDir)\Data`, `$(BaseDir)\Projects` and `$(BaseDir)\Tools`.
  * $(BaseDir)\Data\SWCBuild
  * $(BaseDir)\Projects\132 ActionScript 3 - AS3 Library Project
  * $(BaseDir)\Tools\swcbuild
 * Uninstalling the extension is easy - delete the above folders.
 * Under `$(BaseDir)\Data\SWCBuild`, there are three image files called `Project (1).png`, `Project (2).png` and `Project (3).png`.
 Those images can be used as the project template image by replacing `Project.png` under `$(BaseDir)\Projects\132 ActionScrip 3 - AS3 Library Project`.
 * Changing output path requires an extra step: in the Project Properties panel, change Compilation Target to `"Application"` first, then specify the desired output path, then change back to `"Custom Build"`.
 * Disable/enable asdoc options in the `Project Properties > Build Tab > Pre-Build Command Line`.
 * Use `"swcbuild -help"` to view details on the command-line arguments.
  * For example, `-asdoc` is always `false` when `-debug` is set to `true`.

## Notes

 * Regex for removing `"-debug"` in path in release mode: (This is a bit different from what `FDBuild` uses.)
```C#
path = Regex.Replace(path, @"(\S)[-_.][Dd]ebug([.\\/])", "$1$2");
```