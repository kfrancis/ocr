using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Plugin.Maui.OCR;

namespace Plugin.Maui.OCR.Tests;

public class BuilderExtensionsTests
{
    [Test]
    public async Task UseOcr_Registers_IOcrService_AsSingleton()
    {
        // Arrange
        var builder = MauiApp.CreateBuilder();

        // Act
        builder.UseOcr();
        var app = builder.Build();
        var service1 = app.Services.GetService<IOcrService>();
        var service2 = app.Services.GetService<IOcrService>();

        // Assert
        await Assert.That(service1).IsNotNull();
        await Assert.That(service1).IsEqualTo(service2); // singleton
        await Assert.That(service1).IsTypeOf<OcrImplementation>();
    }

    [Test]
    public async Task UseOcr_DoesNotRegisterTwice_WhenCalledMultipleTimes()
    {
        // Arrange
        var builder = MauiApp.CreateBuilder();

        // Act
        builder.UseOcr();
        builder.UseOcr(); // call again
        var app = builder.Build();
        var services = app.Services.GetServices<IOcrService>();

        // Assert
        var list = services.ToList();
        await Assert.That(list).HasCount(1); // Only one registration
    }
}
