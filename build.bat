@Echo Off

:: Clean build
Call cleanbuild.bat

:: Create a temporary folder
MkDir bin\Temp
Set zip=bin\Temp\zip.vbs

:: Create a temporary zipper file
Echo Set objArgs=WScript.Arguments>%zip%
Echo inputFolder=objArgs(0)>>%zip%
Echo zipFile=objArgs(1)>>%zip%
Echo CreateObject("Scripting.FileSystemObject").CreateTextFile(ZipFile, True).Write "PK"^&Chr(5)^&Chr(6)^&String(18, vbNullChar)>>%zip%
Echo Set objShell=CreateObject("Shell.Application")>>%zip%
Echo Set source=objShell.NameSpace(InputFolder).Items>>%zip%
Echo objShell.NameSpace(ZipFile).CopyHere(source)>>%zip%
Echo wScript.Sleep 500>>%zip%

:: Zip the content files and delete temporary folder
%zip% "%cd%\bin\Contents" "%cd%\bin\swcbuild.zip"
RmDir /S /Q "bin\Temp"

:: Rename output file
Rename "bin\swcbuild.zip" "swcbuild.fdz"