@Echo Off

If Exist "..\bin\Contents\$(BaseDir)\Data" RmDir /S /Q "..\bin\Contents\$(BaseDir)\Data"
If Exist "..\bin\Contents\$(BaseDir)\Projects" RmDir /S /Q "..\bin\Contents\$(BaseDir)\Projects"