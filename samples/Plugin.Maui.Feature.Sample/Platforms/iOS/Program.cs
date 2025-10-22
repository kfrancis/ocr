using ObjCRuntime;
using UIKit;

namespace OcrSample;

public class Program
{
    // iOS entry point required to satisfy CS5001 on net9.0-ios single project.
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
