using System;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Plugin.Xamarin.OCR.Sample
{
    public partial class MainPage : ContentPage
    {
        private OcrResult _ocrResult;
        private int _actualWidth;
        private int _actualHeight;

        private readonly IOcrService _ocr;

        public MainPage() : this(DependencyService.Get<IOcrService>())
        {
        }

        public MainPage(IOcrService? ocr)
        {
            InitializeComponent();

            _ocr = ocr ?? OcrPlugin.Default;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _ocr.InitAsync();
        }

        private void ClearBtn_Clicked(object sender, EventArgs e)
        {
            ResultLbl.Text = string.Empty;
            ClearBtn.IsEnabled = false;
            SelectedImage.Source = null;
            Overlay.Children.Clear();
            Overlay.IsVisible = false;
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

        private void DrawBoundingBoxes(OcrResult result, double scaleX, double scaleY)
        {
            Overlay.Children.Clear(); // Clear previous bounding boxes
            Overlay.IsVisible = true;

            foreach (var element in result.Elements)
            {
                var boxView = new BoxView
                {
                    Color = Color.Red,
                    Opacity = 0.5
                };

                // Calculate the scaled position and size.
                // element.X, element.Y, element.Width, and element.Height should be in pixels relative to the original image size.
                var x = element.X * scaleX;
                var y = element.Y * scaleY;
                var width = element.Width * scaleX;
                var height = element.Height * scaleY;

                // Set the layout bounds for the BoxView. Position (x,y) and size (width, height).
                AbsoluteLayout.SetLayoutBounds(boxView, new Rectangle(x, y, width, height));
                AbsoluteLayout.SetLayoutFlags(boxView, AbsoluteLayoutFlags.None); // Using absolute positioning, no flags needed.

                Overlay.Children.Add(boxView);
            }
        }

        private SKBitmap LoadBitmap(byte[] imageData)
        {
            return SKBitmap.Decode(imageData);
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
            await sourceStream.ReadAsync(imageData, 0, imageData.Length);

            // Use SkiaSharp to load the image and obtain its dimensions
            var bitmap = LoadBitmap(imageData);
            _actualWidth = bitmap.Width;
            _actualHeight = bitmap.Height;

            // Assuming ProcessImage returns the actual dimensions of the image
            var ocrResult = await _ocr.RecognizeTextAsync(imageData, TryHardSwitch.IsToggled);

            // Display the image
            var imageSource = ImageSource.FromStream(() => new MemoryStream(imageData));
            SelectedImage.Source = imageSource;

            // Wait for the Image to be rendered to calculate its displayed size
            SelectedImage.SizeChanged += OnSelectedImageSizeChanged;

            _ocrResult = ocrResult;

            return ocrResult;
        }

        private double ConvertToPixels(double xamarinFormsUnits)
        {
            // This conversion will depend on the device's screen density
            var screenDensity = DeviceDisplay.MainDisplayInfo.Density; // Xamarin.Essentials provides this
            return xamarinFormsUnits * screenDensity;
        }

        private void OnSelectedImageSizeChanged(object sender, EventArgs e)
        {
            // Unhook the SizeChanged event handler so this doesn't run again if not needed
            SelectedImage.SizeChanged -= OnSelectedImageSizeChanged;

            // Get the scale factors based on the actual image and its displayed size
            var (scaleX, scaleY) = GetScaleFactors(_actualWidth, _actualHeight, SelectedImage.Width, SelectedImage.Height);

            // Draw the bounding boxes using the calculated scale factors
            DrawBoundingBoxes(_ocrResult, scaleX, scaleY);
        }

        private (double scaleX, double scaleY) GetScaleFactors(int actualWidth, int actualHeight, double displayedWidth, double displayedHeight)
        {
            // Convert displayed dimensions to pixels
            var displayedWidthInPixels = ConvertToPixels(displayedWidth);
            var displayedHeightInPixels = ConvertToPixels(displayedHeight);

            // Take into account potential padding or aspect ratio constraints here

            // Calculate scale factors
            var scaleX = displayedWidthInPixels / actualWidth;
            var scaleY = displayedHeightInPixels / actualHeight;

            return (scaleX, scaleY);
        }
    }
}
