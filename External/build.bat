@Echo Off

:: Clean build
Call cleanbuild.bat

:: Create output folders
MkDir "..\bin\Contents\$(BaseDir)\Data\SWCBuild"
MkDir "..\bin\Contents\$(BaseDir)\Projects"

:: Copy the raw files into root binary output folder
XCopy /E /Y "Raw\Data\*" "..\bin\Contents\$(BaseDir)\Data\SWCBuild"
XCopy /E /Y "Raw\Projects\*" "..\bin\Contents\$(BaseDir)\Projects"