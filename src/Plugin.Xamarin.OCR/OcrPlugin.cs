using System;

namespace Plugin.Xamarin.OCR
{
    public static class OcrPlugin
    {
        private static IOcrService? s_defaultImplementation;

        /// <summary>
        /// Provides the default implementation for static usage of this API.
        /// </summary>
        public static IOcrService Default
        {
            get
            {
#if NETSTANDARD2_0 || UAP10_0_19041 || MACOS
                throw NotImplementedInReferenceAssembly();
#else
                return s_defaultImplementation ??= new OcrImplementation();
#endif
            }
        }

        /// <summary>
        /// Sets the default implementation.
        /// </summary>
        /// <param name="implementation"></param>
        internal static void SetDefault(IOcrService? implementation) =>
            s_defaultImplementation = implementation;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException("This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}
