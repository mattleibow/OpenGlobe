namespace OpenGlobe.Sample
{
    using Android.App;
    using Android.Widget;
    using Android.OS;

    using System;
    using System.Linq;

    using Android.Views;

    [Activity(Label = "SolarSystem", MainLauncher = true, Icon = "@drawable/icon")]
    public class SolarSystemActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            SetContentView(Resource.Layout.Main);

            var seekBar = this.FindViewById<SeekBar>(Resource.Id.seekBar);
            this.surface = this.FindViewById<GlobeSurfaceView>(Resource.Id.surface);

            seekBar.ProgressChanged += this.SeekBarOnProgressChanged;
            this.files = this.Assets.List(string.Empty).Where(x => x.EndsWith(".JPG", StringComparison.OrdinalIgnoreCase)).ToArray();
            seekBar.Max = this.files.Length - 1;
        }

        private string[] files;

        private GlobeSurfaceView surface;

        private void SeekBarOnProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            this.surface.UpdateTexture(this.files[e.Progress]);
        }
    }
}
