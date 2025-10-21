namespace Plugin.Maui.OCR.Tests;

public class OcrPluginTests
{
    [Test]
    public async Task Default_ReturnsImplementation()
    {
        var service = OcrPlugin.Default;
        await Assert.That(service).IsNotNull();
    }

    [Test]
    public async Task SetDefault_OverridesImplementationAsync()
    {
        var custom = new TestOcrService();
        OcrPlugin.SetDefault(custom);
        await Assert.That(custom).IsEquivalentTo(OcrPlugin.Default);
    }

    [Test]
    public async Task CustomService_Events_Work()
    {
        var custom = new TestOcrService();
        OcrPlugin.SetDefault(custom);
        OcrCompletedEventArgs? args = null;
        custom.RecognitionCompleted += (_, e) => args = e;
        await custom.StartRecognizeTextAsync([], new OcrOptions.Builder().Build());
        await Assert.That(args).IsNotNull();
        await Assert.That(args!.IsSuccessful).IsTrue();
        await Assert.That(args.Result!.AllText).IsEqualTo("test");
    }

    private sealed class TestOcrService : IOcrService
    {
        public event EventHandler<OcrCompletedEventArgs> RecognitionCompleted = delegate { };
        public IReadOnlyCollection<string> SupportedLanguages => new[] { "en" };
        public Task InitAsync(CancellationToken ct = default) => Task.CompletedTask;

        public Task<OcrResult> RecognizeTextAsync(byte[] imageData, bool tryHard = false,
            CancellationToken ct = default) => Task.FromResult(new OcrResult { Success = true, AllText = "test" });

        public Task<OcrResult>
            RecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default) =>
            Task.FromResult(new OcrResult { Success = true, AllText = "test" });

        public Task StartRecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default)
        {
            RecognitionCompleted(this, new OcrCompletedEventArgs(new OcrResult { Success = true, AllText = "test" }));
            return Task.CompletedTask;
        }
    }
}
