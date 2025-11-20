using Android.App;
using Android.Content.PM;
using Android.OS;

namespace EncryptItVC.MobileClient;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // Request audio permissions
        RequestPermissions(new string[] {
            Android.Manifest.Permission.RecordAudio,
            Android.Manifest.Permission.ModifyAudioSettings,
            Android.Manifest.Permission.Internet,
            Android.Manifest.Permission.AccessNetworkState
        }, 1);
    }
}
