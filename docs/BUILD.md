# Building the Project

This project is a Windows Forms application targeting .NET Framework 4.8.1.

## Prerequisites
- Windows 10 or 11
- Visual Studio 2022 (with .NET desktop development workload)
- MSBuild & NuGet

## Build Instructions
1. Open `src/youtube-downloader/youtube dowload.sln` in Visual Studio.
2. Set configuration to `Release`.
3. Click Build -> Build Solution.

Alternatively, via command line:
```powershell
msbuild "src/youtube-downloader/youtube dowload.sln" /p:Configuration=Release
```

## Running
After building, the executable will be at `src/youtube-downloader/bin/Release/youtube dowload.exe`.
Make sure to place `yt-dlp.exe` and `ffmpeg.exe` in the same directory before running to ensure full functionality.
