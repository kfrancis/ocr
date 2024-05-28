<!-- 
Everything in here is of course optional. If you want to add/remove something, absolutely do so as you see fit.
This example README has some dummy APIs you'll need to replace and only acts as a placeholder for some inspiration that you can fill in with your own functionalities.
-->
![](nuget.png)
# Plugin.Xamarin.OCR | Plugin.Maui.OCR

`Plugin.Xamarin.OCR` and `Plugin.Maui.OCR` provide the ability to do simple text from image OCR using nothing but platform APIs.

## Should you use this yet?

**YES**. Let me know how it works for you.

## What Works Matrix

| Platform | iOS | Android | Windows | macOS |
|----------|-----|---------|---------|-------|
| Xamarin  | Yes |   Yes   |   WIP   |  WIP  |
| MAUI     | Yes |   Yes   |   Yes   |  Yes  |

[![Build for CI](https://github.com/kfrancis/ocr/actions/workflows/ci.yml/badge.svg)](https://github.com/kfrancis/ocr/actions/workflows/ci.yml)

## Why

Why am I making this? I'm doing this because I want to make it easier for developers to do OCR in their apps. I want to make it so that you can just use this plugin and not have to worry about the platform specifics.

Too many times I've tried to do OCR and had to wrestle with external dependencies like Tesseract (with its dependencies Leptonica, etc) and these types
of native dependencies can be a real pain to work with. 

### Xamarin??

Well, I still have to maintain a Xamarin app that uses Tesseract and I'm tired of all the problems that come with it. I want to make it easier for myself and others to do OCR in their apps.

## Install Plugin

[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.OCR.svg?label=MAUI)](https://www.nuget.org/packages/Plugin.Maui.OCR/)
[![NuGet](https://img.shields.io/nuget/v/Plugin.Xamarin.OCR.svg?label=Xamarin)](https://www.nuget.org/packages/Plugin.Xamarin.OCR/)

Available on NuGet for [MAUI](http://www.nuget.org/packages/Plugin.Maui.OCR) and [Xamarin](http://www.nuget.org/packages/Plugin.Xamarin.OCR).

Install with the dotnet CLI: `dotnet add package Plugin.Maui.OCR` or `dotnet add package Plugin.Xamarin.OCR`, or through the NuGet Package Manager in Visual Studio.

### Supported Platforms

| Platform | Minimum Version Supported |
|----------|---------------------------|
| iOS      | 13+                       |
| macOS    | 10.15+                    |
| Android  | 5.0 (API 21)              |
| Windows  | 11 and 10 version 1809+   |

## Pattern Matching

One of the more common things that I personally do with OCR is to recognize a pattern in the text. For example, I might want to recognize a date or a phone number or an email address. This is where the `OcrPatternConfig` class comes in.

Let's say you want to recognize an Ontario Health Card Number in the text of your image. Numbers of those types have some specific qualities that make it easy to match.

1. An Ontario HCN is 10 digits long.
1. The number must be [Luhn valid](https://en.wikipedia.org/wiki/Luhn_algorithm) (meaning it has a check digit and it's correct).

To do this, you can create a `OcrPatternConfig` object like so:

```csharp
bool IsValidLuhn(string number)
{
    // Convert the string to an array of digits
    int[] digits = number.Select(d => int.Parse(d.ToString())).ToArray();
    int checkDigit = 0;

    // Luhn algorithm implementation
    for (int i = digits.Length - 2; i >= 0; i--)
    {
        int currentDigit = digits[i];
        if ((digits.Length - 2 - i) % 2 == 0) // check if it's an even index from the right
        {
            currentDigit *= 2;
            if (currentDigit > 9)
            {
                currentDigit -= 9;
            }
        }
        checkDigit += currentDigit;
    }

    return (10 - (checkDigit % 10)) % 10 == digits.Last();
}

var ohipPattern = new OcrPatternConfig(@"\d{10}", IsLuhnValid);

var options = new OcrOptions(tryHard: true, patternConfig: ohipPattern);

var result = await OcrPlugin.Default.RecognizeTextAsync(imageData, options);

var patientHcn = result.MatchedValues.FirstOrDefault(); // This will be the HCN (and only the HCN) if it's found
```

## MAUI Setup and Usage

For MAUI, to initialize make sure you use the MauiAppBuilder extension `AddOcr()` like so:

```csharp
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			}).
			AddOcr();  // <-- add this line

		return builder.Build();
	}
}
```

And then you can just inject `IOcrService` into your classes and use it like so:

```csharp
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
    await sourceStream.ReadAsync(imageData);

    // Process the image data using the OCR service
    return await _ocr.RecognizeTextAsync(imageData);
}
```

## Xamarin Setup and Usage

For Xamarin, if you have some kind of DI framework in place then you can just register the `OcrPlugin` with it.

```csharp
public App()
{
    InitializeComponent();

    DependencyService.RegisterSingleton(OcrPlugin.Default);

    MainPage = new MainPage();
}
```

If you don't have a DI framework in place, you can use the `OcrPlugin.Default` property to access the `IOcrService` instance.

```csharp
private readonly IOcrService _ocr;

public MainPage(IOcrService? ocr)
{
    InitializeComponent();

    _ocr = ocr ?? OcrPlugin.Default;
}
```

## Details

The `IOcrService` interface exposes the following methods:

```csharp
public interface IOcrService
{
    event EventHandler<OcrCompletedEventArgs> RecognitionCompleted;
    IReadOnlyCollection<string> SupportedLanguages { get; }
    Task InitAsync(CancellationToken ct = default);
    Task<OcrResult> RecognizeTextAsync(byte[] imageData, bool tryHard = false, CancellationToken ct = default);
    Task<OcrResult> RecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default);
    Task StartRecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default);
}

public class OcrResult
{
    public bool Success { get; set; }

    public string AllText { get; set; }

    public IList<OcrElement> Elements { get; set; } = new List<OcrElement>();
    public IList<string> Lines { get; set; } = new List<string>();

    public class OcrElement
    {
        public string Text { get; set; }
        public float Confidence { get; set; }

        // Useful for bounding boxes
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
```

### Permissions

Before you can start using Feature, you will need to request the proper permissions on each platform.

#### iOS

If you're handling camera, you'll need the usual permissions for that.

#### Android

If you're handling camera, you'll need the usual permissions for that. The only extra part you'll want in the AndroidManifest.xml is the following:

```xml
<application ..>
  <meta-data android:name="com.google.mlkit.vision.DEPENDENCIES" android:value="ocr" />
</application>
```

This will cause the model necessary to be installed when the application is installed.

### Dependency Injection

You will first need to register the `OcrPlugin` with the `MauiAppBuilder` following the same pattern that the .NET MAUI Essentials libraries follow.

```csharp
builder.Services.AddSingleton(OcrPlugin.Default);
```

You can then enable your classes to depend on `IOcrService` as per the following example.

```csharp
public class OcrViewModel
{
    readonly IOcrService _ocr;

    public OcrViewModel(IOcrService? ocr)
    {
        _ocr = ocr ?? OcrPlugin.Default;
    }

    public void DoSomeOcr()
    {
        byte[] imageData = GetImageData();

        var result = await _ocr.RecognizeTextAsync(imageData);
    }
}
```

### Straight usage

Alternatively if you want to skip using the dependency injection approach you can use the `Feature.Default` property.

```csharp
public class OcrViewModel
{
    public void DoSomeOcr()
    {
        byte[] imageData = GetImageData();

        var result = await OcrPlugin.Default.RecognizeTextAsync(imageData);
    }
}
```

### Feature

Once you have the `OCR` instance, you can interact with it in the following ways:

#### Events

##### `RecognitionCompleted`

This event is fired when the OCR service has completed recognizing text from an image. The event args contain the `OcrResult` object. Only fires if the `StartRecognizeTextAsync` method is called.

#### Properties

##### `SupportedLanguages`

A list of supported languages for the OCR service. This is populated after calling `InitAsync`. Allows you to know what language codes are able to be used in OcrOptions.

#### Methods

##### `InitAsync(CancellationToken ct = default)`

Initialize the feature. If supported on the platform (like iOS), SupportedLanguages will be populated with the available languages.

##### `RecognizeTextAsync(byte[] imageData, bool tryHard = false, CancellationToken ct = default)`

Recognize text from an image. Specify "tryHard" if you want to tell the platform API to do a better job (fast vs accurate, and use language correction (ios/mac)) though it seems very accurate normally.

##### `RecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default)`

Recognize text from an image. OcrOptions contains options for the OCR service, including the language to use and whether to try hard.

##### `Task StartRecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default)`

Start recognizing text from an image. This is a task that will fire the `RecognitionCompleted` event when it completes with the result.

# Acknowledgements

This project could not have came to be without these projects and people, thank you! <3
