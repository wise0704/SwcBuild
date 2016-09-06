# SwcBuild

ActionScript® Component Compiler Tool for FlashDevelop® Projects

## Details

* Supports ActionScript Compiler 2.
* Supports AS Documentation.
* All compiler options in the project file (`*.as3proj`) are used _(except for movie options such as `default-framerate`, `default-size`, etc.)_.
* No extra steps needed to setup the project. Just click `Project > New Project > AS3 Library Project`. _(Not `AS3 Library SWC`!)_
* You can build the project the same way you would build a `.swf` file. Just click `Project > Build Project` or press `F8` (default shortcut).
* Detailed output is redirected to the Output Panel.
* Creates `obj\$(ProjectID)Config.xml` almost exactly the same way as `FDBuild`. You can use this file for debugging purposes.
* Supports AIR and AIR Mobile platforms. Just go to `Project Properties > Output` and choose from the Platform dropbox. No extra work is needed _(no more manual `+configname=air`)_.
* Supports conditional output path: in release mode, `"-debug"` in the path is removed[*](#notes).
* Include sources easily by just adding them to the project classpath list.
* Easily include library paths with `Right Click > Add To Library` _(no more manual `-libraryPaths+=...`)_.

## Installation

Simply open `swcbuild.fdz`.
Alternatively, if you install using AppMan in FlashDevelop, it will be installed automatically.

## Additional Information

* This installation file makes changes in `$(BaseDir)\Data`, `$(BaseDir)\Projects` and `$(BaseDir)\Tools`.
  * $(BaseDir)\Data\SWCBuild
  * $(BaseDir)\Projects\132 ActionScript 3 - AS3 Library Project
  * $(BaseDir)\Tools\swcbuild
* Uninstalling the extension is easy - delete the above folders.
* Under `$(BaseDir)\Data\SWCBuild`, there are three image files called `Project (1).png`, `Project (2).png` and `Project (3).png`.
 Those images can be used as the project template image by replacing `Project.png` under `$(BaseDir)\Projects\132 ActionScrip 3 - AS3 Library Project`.
* Changing output path requires an extra step: in the Project Properties panel, change Compilation Target to `Application` first, then specify the desired output path, then change back to `Custom Build`.
* Disable/enable asdoc options in the `Project Properties > Build Tab > Pre-Build Command Line`.
* Use `swcbuild -help` to view details on the command-line arguments.
  * For example, `-asdoc` is always `false` when `-debug` is set to `true`.
* To use the `swcbuild.exe` tool with an existing project, go to Project Properties panel:
  * Set Compilation Target to `Custom Build`.
  * Set Pre-Build Command Line to `"$(BaseDir)\Tools\swcbuild\swcbuild.exe" "$(ProjectPath)" "-compiler=$(CompilerPath)"`.
    * _These are only the required arguments. Use `swcbuild -help` to see additional information._

## Notes

*Regex for removing `"-debug"` in path in release mode: (This is a bit different from what `FDBuild` uses.)
```C#
path = Regex.Replace(path, @"(\S)[-_.][Dd]ebug([.\\/])", "$1$2");
```