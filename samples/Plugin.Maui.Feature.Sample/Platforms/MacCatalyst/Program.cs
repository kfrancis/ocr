using ObjCRuntime;
using UIKit;

namespace OcrSample;

public static class Program
{
    // Entry point for MacCatalyst (and iOS if shared). Required to avoid CS5001.
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
