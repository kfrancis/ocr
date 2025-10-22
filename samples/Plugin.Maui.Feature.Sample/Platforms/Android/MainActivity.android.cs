#if ANDROID
using Android.App;
using Android.Content.PM;
using Android.OS;

namespace OcrSample;

// Android-specific MainActivity. The assembly attributes must be inside an Android build.
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
#endif
