@echo off

set _vc_msbuild=C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\msbuild.exe
set _dotnet=dotnet
set _7z=C:\Program Files\7-Zip\7z.exe

set _root_dir=%~dp0..
set _ae_dir=%_root_dir%\ae
set _install_dir=%_ae_dir%\install
set _run_dir=%_ae_dir%\run
set _zip_file=%_root_dir%\AELoader.zip

del /q "%_zip_file%"
rmdir /q /s "%_ae_dir%"

xcopy /y /q README.md "%_ae_dir%"\      || exit /b 1
xcopy /y /q *.reg     "%_install_dir%"\ || exit /b 1

"%_vc_msbuild%" AELoader\AELoader.vcxproj /p:Configuration=Release /p:Platform=x64   || exit /b 1
"%_vc_msbuild%" AELoader\AELoader.vcxproj /p:Configuration=Release /p:Platform=Win32 || exit /b 1
xcopy /y /q AELoader\bin\x64\Release\AELoader.dll   "%_install_dir%"\x64\ || exit /b 1
xcopy /y /q AELoader\bin\Win32\Release\AELoader.dll "%_install_dir%"\x86\ || exit /b 1

"%_dotnet%" build -c Release AELauncher || exit /b 1
xcopy /y /q AELauncher\bin\Release\ww898.AELauncher.*.nupkg "%_run_dir%"\ || exit /b 1

"%_dotnet%" publish -f net20         -r win-x64 -c Release --self-contained true Runner || exit /b 1
"%_dotnet%" publish -f net40         -r win-x64 -c Release --self-contained true Runner || exit /b 1
"%_dotnet%" publish -f netcoreapp2.0 -r win-x64 -c Release --self-contained true Runner || exit /b 1
xcopy /y /q Runner\bin\Release\net20\win-x64\publish\*         "%_run_dir%"\net20\         || exit /b 1
xcopy /y /q Runner\bin\Release\net40\win-x64\publish\*         "%_run_dir%"\net40\         || exit /b 1
xcopy /y /q Runner\bin\Release\netcoreapp2.0\win-x64\publish\* "%_run_dir%"\netcoreapp2.0\ || exit /b 1

xcopy /y /q AELoader.ini "%_run_dir%\script\" || exit /b 1
xcopy /y /q run.ps1      "%_run_dir%\script\" || exit /b 1

"%_7z%" a "%_zip_file%" -mx=9 -r "%_ae_dir%"\* || exit /b 1

echo Ok
