# .NET9.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET9.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET9.0 upgrade.
3. Upgrade samples\Plugin.Maui.Feature.Sample\Plugin.Maui.OCR.Sample.csproj
4. Upgrade src\Plugin.Maui.OCR\Plugin.Maui.OCR.csproj
5. Run unit tests to validate upgrade in the projects listed below:

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name | Description |
|:-----------------------------------------------|:---------------------------:|

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name | Current Version | New Version | Description |
|:------------------------------------|:-------------------:|:-----------:|:----------------------------------------------|
| Emgu.CV.runtime.maui.mini |4.9.0.5494 |4.12.0.5764 | Incompatible with target .NET9.0; update required |
| SixLabors.ImageSharp |3.1.7 |3.1.11 | Security vulnerability; upgrade to patched version |
| Xamarin.AndroidX.Fragment.Ktx |1.7.0.2 | | No supported version for .NET9.0; investigate removal or replacement |

### Project upgrade details

#### samples\Plugin.Maui.Feature.Sample\Plugin.Maui.OCR.Sample.csproj modifications

Project properties changes:
 - Target frameworks should be changed from `net8.0-android;net8.0-ios;net8.0-maccatalyst;net8.0-windows10.0.19041.0` to `net8.0-android;net8.0-ios;net8.0-maccatalyst;net8.0-windows10.0.19041.0;net9.0-windows` (adding .NET9 for Windows target where recommended)

NuGet packages changes:
 - Emgu.CV.runtime.maui.mini should be updated from `4.9.0.5494` to `4.12.0.5764` (*incompatible with .NET9.0*)
 - SixLabors.ImageSharp should be updated from `3.1.7` to `3.1.11` (*security vulnerability*)
 - Xamarin.AndroidX.Fragment.Ktx has no supported version for .NET9.0; assess necessity or seek alternative (leave as-is initially, may cause build warnings/errors)

Other changes:
 - Re-test OCR functionality across all platforms after framework update.

#### src\Plugin.Maui.OCR\Plugin.Maui.OCR.csproj modifications

Project properties changes:
 - (If needed later) consider adding `net9.0` specific TFM variants when platform SDK support is available. Currently multi-targeting remains unchanged until required (.NET9 specific mobile TFMs not yet recommended in analysis).

NuGet packages changes:
 - (No direct issues reported for this project during analysis.)

Other changes:
 - Validate compatibility of MAUI dependencies under .NET9 SDK once global upgrade applied.
