@Echo Off

:: Clean build
Call cleanbuild.bat

:: Create output folder
MkDir "..\bin\Contents\$(BaseDir)\Tools\swcbuild"

:: Copy the binary to output folder
Copy "bin\swcbuild.exe" "..\bin\Contents\$(BaseDir)\Tools\swcbuild"