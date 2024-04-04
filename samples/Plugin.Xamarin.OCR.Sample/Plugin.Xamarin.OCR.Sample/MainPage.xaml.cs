using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Shared.OCR;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Plugin.Xamarin.OCR.Sample
{
    public partial class MainPage : ContentPage
    {
        private readonly IOcrService _ocr;

        public MainPage()
        {
            InitializeComponent();

            _ocr = OcrPlugin.Default;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _ocr.InitAsync();
        }

        private async void OpenFromFileBtn_Clicked(object sender, EventArgs e)
        {
            var photo = await MediaPicker.PickPhotoAsync();

            if (photo != null)
            {
                var result = await ProcessPhoto(photo);

                ResultLbl.Text = result.AllText;

                ClearBtn.IsEnabled = true;
            }
        }

        private async void OpenFromCameraBtn_Clicked(object sender, EventArgs e)
        {
            if (MediaPicker.IsCaptureSupported)
            {
                var photo = await MediaPicker.CapturePhotoAsync();

                if (photo != null)
                {
                    var result = await ProcessPhoto(photo);

                    ResultLbl.Text = result.AllText;

                    ClearBtn.IsEnabled = true;
                }
            }
            else
            {
                await DisplayAlert(title: "Sorry", message: "Image capture is not supported on this device.", cancel: "OK");
            }
        }

        private void ClearBtn_Clicked(object sender, EventArgs e)
        {
            ResultLbl.Text = string.Empty;
            ClearBtn.IsEnabled = false;
        }

        /// <summary>
        /// Takes a photo and processes it using the OCR service.
        /// </summary>
        /// <param name="photo">The photo to process.</param>
        /// <returns>The OCR result.</returns>
        private async Task<OcrResult> ProcessPhoto(FileResult photo)
        {
            // Open a stream to the photo
            using var sourceStream = await photo.OpenReadAsync();

            // Create a byte array to hold the image data
            var imageData = new byte[sourceStream.Length];

            // Read the stream into the byte array
            await sourceStream.ReadAsync(imageData, 0, imageData.Length);

            // Process the image data using the OCR service
            return await _ocr.RecognizeTextAsync(imageData);
        }
    }
}
