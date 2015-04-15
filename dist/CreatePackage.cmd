@echo off
echo.
echo Deleting old packages
del Owin.ThemableErrorPage.*.nupkg
echo.
echo Updating NuGet
nuget.exe update -self
echo Building solution
C:\Windows\Microsoft.Net\Framework64\v4.0.30319\MSBuild.exe ..\src\Owin.ThemableErrorPage.sln /p:Configuration=Release
echo.
echo Creating Package
nuget pack nuget\Owin.ThemableErrorPage\Owin.ThemableErrorPage.nuspec -Symbols