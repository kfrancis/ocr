using System.Globalization;
using Plugin.OCR.Maui.Tests.Mocks;

namespace Plugin.OCR.Maui.Tests
{
    public abstract class BaseTest : IDisposable
    {
        readonly CultureInfo _defaultCulture, _defaultUiCulture;

        bool _isDisposed;

        protected enum TestDuration
        {
            Short = 2000,
            Medium = 5000,
            Long = 10000
        }

        protected BaseTest()
        {
            _defaultCulture = Thread.CurrentThread.CurrentCulture;
            _defaultUiCulture = Thread.CurrentThread.CurrentUICulture;

            DispatcherProvider.SetCurrent(new MockDispatcherProvider());
            //DeviceDisplay.SetCurrent(null);
        }

        ~BaseTest() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_isDisposed)
            {
                return;
            }

            Thread.CurrentThread.CurrentCulture = _defaultCulture;
            Thread.CurrentThread.CurrentUICulture = _defaultUiCulture;

            //DeviceDisplay.SetCurrent(null);
            DispatcherProvider.SetCurrent(null);

            _isDisposed = true;
        }

        protected static Task<Stream> GetStreamFromImageSource(StreamImageSource imageSource, CancellationToken token)
            => imageSource.Stream(token);

        protected static bool StreamEquals(Stream a, Stream b)
        {
            if (a == b)
            {
                return true;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            for (var i = 0; i < a.Length; i++)
            {
                if (a.ReadByte() != b.ReadByte())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
