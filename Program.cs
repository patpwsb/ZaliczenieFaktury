using System.Reflection;
using Avalonia;

namespace FakturoNet;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            StartupLog.Write("Program.Main start");
            ConfigureRenderLoopFallback();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception exception)
        {
            StartupLog.Write("Program.Main exception");
            StartupLog.Write(exception.ToString());
            throw;
        }
    }

    private static void ConfigureRenderLoopFallback()
    {
        try
        {
            var avaloniaBaseAssembly = typeof(AvaloniaLocator).Assembly;
            var locatorType = typeof(AvaloniaLocator);
            var currentMutableProperty = locatorType.GetProperty("CurrentMutable", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var currentMutable = currentMutableProperty?.GetValue(null);

            if (currentMutable is null)
            {
                StartupLog.Write("Render fallback skipped: AvaloniaLocator.CurrentMutable unavailable");
                return;
            }

            var renderTimerType = avaloniaBaseAssembly.GetType("Avalonia.Rendering.UiThreadRenderTimer");
            var renderTimerInterfaceType = avaloniaBaseAssembly.GetType("Avalonia.Rendering.IRenderTimer");
            var renderLoopType = avaloniaBaseAssembly.GetType("Avalonia.Rendering.RenderLoop");
            var renderLoopInterfaceType = avaloniaBaseAssembly.GetType("Avalonia.Rendering.IRenderLoop");

            if (renderTimerType is null || renderTimerInterfaceType is null || renderLoopType is null || renderLoopInterfaceType is null)
            {
                StartupLog.Write("Render fallback skipped: Avalonia render types unavailable");
                return;
            }

            var renderTimer = Activator.CreateInstance(renderTimerType, 60);

            if (renderTimer is null)
            {
                StartupLog.Write("Render fallback skipped: failed to create UiThreadRenderTimer");
                return;
            }

            BindAvaloniaService(locatorType, currentMutable, renderTimerInterfaceType, renderTimerType, renderTimer);

            var renderLoop = Activator.CreateInstance(renderLoopType, renderTimer);

            if (renderLoop is null)
            {
                StartupLog.Write("Render fallback skipped: failed to create RenderLoop");
                return;
            }

            BindAvaloniaService(locatorType, currentMutable, renderLoopInterfaceType, renderLoopType, renderLoop);
            StartupLog.Write("Configured reflected UI-thread render loop fallback");
        }
        catch (Exception exception)
        {
            StartupLog.Write("ConfigureRenderLoopFallback exception");
            StartupLog.Write(exception.ToString());
        }
    }

    private static void BindAvaloniaService(
        Type locatorType,
        object currentMutable,
        Type serviceType,
        Type implementationType,
        object implementation)
    {
        var bindMethod = locatorType.GetMethod("Bind", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("AvaloniaLocator.Bind not found.");

        var helper = bindMethod.MakeGenericMethod(serviceType).Invoke(currentMutable, null)
            ?? throw new InvalidOperationException($"Failed to create binding helper for {serviceType.FullName}.");

        var toConstantMethod = helper.GetType().GetMethod("ToConstant", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("RegistrationHelper.ToConstant not found.");

        _ = toConstantMethod.MakeGenericMethod(implementationType).Invoke(helper, [implementation]);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new MacOSPlatformOptions
            {
                ShowInDock = true
            })
            .With(new AvaloniaNativePlatformOptions
            {
                AppSandboxEnabled = false,
                RenderingMode =
                [
                    AvaloniaNativeRenderingMode.Software
                ]
            })
            .WithInterFont()
            .LogToTrace();
}
