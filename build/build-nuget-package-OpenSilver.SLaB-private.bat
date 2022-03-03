@echo off

IF NOT EXIST "nuspec/OpenSilver.SLaB.nuspec" (
echo Wrong working directory. Please navigate to the folder that contains the BAT file before executing it.
PAUSE
EXIT
)



rem Define the escape character for colored text
for /F %%a in ('"prompt $E$S & echo on & for %%b in (1) do rem"') do set "ESC=%%a"

rem Define the "%PackageVersion%" variable:
set /p PackageVersion="%ESC%[92mOpenSilver.SLaB version:%ESC%[0m 1.0.0-private-"

rem Get the current date and time:
for /F "tokens=2" %%i in ('date /t') do set currentdate=%%i
set currenttime=%time%

rem Create a Version.txt file with the date:
md temp
@echo OpenSilver.SLaB 1.0.0-private-%PackageVersion% (%currentdate% %currenttime%)> temp/Version.txt

echo. 
echo %ESC%[95mRestoring NuGet packages%ESC%[0m
echo. 
nuget restore ../Source/OpenSilver.SLaB.sln -v quiet

echo. 
echo %ESC%[95mBuilding %ESC%[0mRelease %ESC%[95mconfiguration%ESC%[0m
echo. 
msbuild slnf/OpenSilver.SLaB.slnf -p:Configuration=Release -clp:ErrorsOnly -restore
echo. 
echo %ESC%[95mPacking %ESC%[0mOpenSilver.SLaB %ESC%[95mNuGet package%ESC%[0m
echo. 
nuget.exe pack nuspec\OpenSilver.SLaB.nuspec -OutputDirectory "output/OpenSilver.SLaB" -Properties "PackageId=OpenSilver.SLaB;PackageVersion=1.0.0-private-%PackageVersion%;Configuration=Release;Target=OpenSilver.SLaB;RepositoryUrl=https://github.com/OpenSilver/OpenSilver"

explorer "output\OpenSilver.SLaB"

pause
