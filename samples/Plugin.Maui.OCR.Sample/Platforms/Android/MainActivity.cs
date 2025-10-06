using Android;
using Android.App;
using Android.Content.PM;

// Needed for Picking photo/video
[assembly: UsesPermission(Manifest.Permission.ReadExternalStorage, MaxSdkVersion = 32)]
[assembly: UsesPermission(Manifest.Permission.ReadMediaAudio)]
[assembly: UsesPermission(Manifest.Permission.ReadMediaImages)]
[assembly: UsesPermission(Manifest.Permission.ReadMediaVideo)]

// Needed for Taking photo/video
[assembly: UsesPermission(Manifest.Permission.Camera)]
[assembly: UsesPermission(Manifest.Permission.WriteExternalStorage, MaxSdkVersion = 32)]

// Add these properties if you would like to filter out devices that do not have cameras, or set them false to make them optional
[assembly: UsesFeature("android.hardware.camera", Required = true)]
[assembly: UsesFeature("android.hardware.camera.autofocus", Required = true)]

namespace Plugin.Maui.OCR.Sample;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
