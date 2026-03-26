using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Themes.Fluent;
using FakturoNet.Services;
using QuestPDF.Infrastructure;

namespace FakturoNet;

public sealed class App : Application
{
    public override void Initialize()
    {
        StartupLog.Write("App.Initialize");
        QuestPDF.Settings.License = LicenseType.Community;
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        StartupLog.Write("App.OnFrameworkInitializationCompleted");
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                StartupLog.Write("Creating services");
                var store = new JsonDataStore();
                store.EnsureSeedDataAsync().GetAwaiter().GetResult();

                var contractorService = new ContractorService(store);
                var invoiceService = new InvoiceService(store, contractorService, SellerSettingsLoader.Load());
                var pdfRenderer = new PdfInvoiceRenderer();

                StartupLog.Write("Creating main window");
                var mainWindow = new MainWindow(contractorService, invoiceService, pdfRenderer);
                desktop.MainWindow = mainWindow;
                StartupLog.Write("Main window assigned");
            }
            catch (Exception exception)
            {
                StartupLog.Write("App startup exception");
                StartupLog.Write(exception.ToString());
                var errorWindow = new Window
                {
                    Title = "FakturoNet - Blad startu",
                    Width = 920,
                    Height = 540,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Content = new ScrollViewer
                    {
                        Content = new StackPanel
                        {
                            Margin = new Thickness(24),
                            Spacing = 12,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = "Aplikacja nie uruchomila glownego okna.",
                                    FontSize = 24,
                                    FontWeight = Avalonia.Media.FontWeight.Bold
                                },
                                new TextBlock
                                {
                                    Text = "Szczegoly bledu:",
                                    FontWeight = Avalonia.Media.FontWeight.SemiBold
                                },
                                new SelectableTextBlock
                                {
                                    Text = $"{exception}\n\nLog: {StartupLog.GetLogPath()}",
                                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                                }
                            }
                        }
                    }
                };

                desktop.MainWindow = errorWindow;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
