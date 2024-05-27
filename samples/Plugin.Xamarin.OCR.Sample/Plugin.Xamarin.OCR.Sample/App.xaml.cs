using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Plugin.Xamarin.OCR.Sample
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            DependencyService.RegisterSingleton(OcrPlugin.Default);

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
